using Codebound.Building;
using Codebound.Core;
using Codebound.Energy;
using Codebound.Items;
using Codebound.Machines;
using Codebound.World;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class MachineTests
    {
        private const double Dt = 1.0 / 20;
        private ItemRegistry _items;
        private WorldMap _map;
        private BuildGrid _grid;
        private MachineSystem _sys;
        private long _tick;

        [SetUp]
        public void SetUp()
        {
            _items = ItemCatalog.CreateDefault();
            _map = new WorldMap(40, 40, 0);
            _grid = new BuildGrid(_map, _items);
            _sys = new MachineSystem(_map, _grid, Dt);
            _tick = 0;
        }

        private PlacedStructure Place(string id, int x, int y, Direction f = Direction.North)
        {
            var status = _grid.TryPlace(StructureCatalog.Find(id), new GridPos(x, y), f, null, out var s);
            Assert.AreEqual(PlacementStatus.Ok, status, $"placing {id} at {x},{y}");
            return s;
        }

        private void Run(int ticks) { for (int i = 0; i < ticks; i++) _sys.Tick(++_tick); }

        /// <summary>Battery + pole + computer running a program (what the Etapa 3 runtime will report).</summary>
        private Computer PoweredComputer(int x = 10, int y = 10)
        {
            Place(StructureCatalog.EmergencyBattery, x - 3, y);
            Place(StructureCatalog.PowerPole, x - 1, y);
            var c = _sys.Get<Computer>(Place(StructureCatalog.Computer, x, y));
            Assert.IsTrue(c.LoadProgram(ProgrammingLanguage.Python, "while True:\n    drill.mine()"));
            c.MarkRunning();
            return c;
        }

        // ---------- placement ----------

        [Test]
        public void Drill_MustCoverOre_AndMayStandOnIt()
        {
            var def = StructureCatalog.Find(StructureCatalog.Drill);
            Assert.AreEqual(PlacementStatus.NoResourceUnderneath, _grid.Validate(def, new GridPos(5, 5), Direction.North, null).Status);
            _map.AddNode(new ResourceNode(new GridPos(6, 6), ItemCatalog.IronOre, 10));
            Assert.AreEqual(PlacementStatus.Ok, _grid.Validate(def, new GridPos(5, 5), Direction.North, null).Status);
            // Normal structures still cannot be built on ore.
            Assert.AreEqual(PlacementStatus.TerrainBlocked,
                _grid.Validate(StructureCatalog.Find(StructureCatalog.Wall), new GridPos(6, 6), Direction.North, null).Status);
        }

        [Test]
        public void EmergencyBattery_IsNotInBuildPalette()
        {
            foreach (var d in StructureCatalog.Buildable) Assert.AreNotEqual(StructureCatalog.EmergencyBattery, d.Id);
            Assert.IsNotNull(StructureCatalog.Find(StructureCatalog.EmergencyBattery));
        }

        // ---------- power ----------

        [Test]
        public void Power_PolesFormNetworksByRange()
        {
            Place(StructureCatalog.PowerPole, 0, 0);
            Place(StructureCatalog.PowerPole, 7, 0);   // within 7: same network
            Place(StructureCatalog.PowerPole, 30, 30); // isolated
            _sys.Power.Rebuild();
            Assert.AreEqual(2, _sys.Power.Networks.Count);
        }

        [Test]
        public void Power_NodeOutsidePoleRangeIsNotConnected()
        {
            Place(StructureCatalog.PowerPole, 0, 0);
            var near = _sys.Get<Computer>(Place(StructureCatalog.Computer, 2, 2));
            var far = _sys.Get<Computer>(Place(StructureCatalog.Computer, 3, 3));
            Run(1);
            Assert.IsTrue(near.Connected);
            Assert.IsFalse(far.Connected);
            Assert.AreEqual(MachineState.NoPower, far.State);
        }

        [Test]
        public void Power_BatteryCoversDemandThenRunsOut()
        {
            var c = PoweredComputer();
            var battery = _sys.GetBattery(_grid.GetAt(new GridPos(7, 10)));
            double start = battery.StoredJoules;
            Run(20); // 1 s at 30 W
            Assert.AreEqual(start - 30, battery.StoredJoules, 1e-6);
            Assert.AreEqual(1.0, c.Satisfaction, 1e-9);
            battery.GetType(); // keep reference
            // Drain artificially and check brown-out.
            typeof(Battery).GetProperty("StoredJoules").SetValue(battery, 0.5);
            Run(2);
            Assert.Less(c.Satisfaction, 1.0);
            Assert.IsFalse(c.IsRunningProgram, "computer without power cannot run its program");
        }

        [Test]
        public void Power_BrownOutSharesSupplyEvenly()
        {
            var grid = new PowerGrid();
            var a = new TestConsumer(new GridPos(0, 0), 100);
            var b = new TestConsumer(new GridPos(1, 0), 300);
            var g = new TestProducer(new GridPos(0, 1), 200);
            grid.AddPole(new PowerPole(new GridPos(0, 0)));
            grid.AddNode(a); grid.AddNode(b); grid.AddNode(g);
            grid.Tick(Dt);
            Assert.AreEqual(0.5, a.Satisfaction, 1e-9);
            Assert.AreEqual(0.5, b.Satisfaction, 1e-9);
            Assert.AreEqual(200, g.Used, 1e-9);
        }

        // ---------- program rule ----------

        [Test]
        public void Machines_DoNothingWithoutAValidProgram()
        {
            _map.AddNode(new ResourceNode(new GridPos(17, 11), ItemCatalog.IronOre, 10));
            var c = PoweredComputer();
            var drill = _sys.Get<Drill>(Place(StructureCatalog.Drill, 16, 10));
            drill.RequestMine();
            Run(100);
            Assert.AreEqual(MachineState.NoProgram, drill.State, "not linked");
            Assert.AreEqual(0, drill.TotalMined);

            Assert.AreEqual(LinkResult.Ok, c.Link(drill));
            c.MarkError("SyntaxError");
            Run(100);
            Assert.AreEqual(MachineState.NoProgram, drill.State, "program has an error");
            Assert.AreEqual(0, drill.TotalMined);
        }

        // ---------- drill ----------

        [Test]
        public void Drill_MinesOneUnitPerCycleIntoCrateInFront()
        {
            _map.AddNode(new ResourceNode(new GridPos(13, 10), ItemCatalog.IronOre, 3));
            var c = PoweredComputer();
            Place(StructureCatalog.PowerPole, 11, 12);
            var drill = _sys.Get<Drill>(Place(StructureCatalog.Drill, 12, 10, Direction.East));
            var crate = Place(StructureCatalog.Crate, 14, 10);
            Assert.AreEqual(new GridPos(14, 10), drill.OutputCell);
            Assert.AreEqual(LinkResult.Ok, c.Link(drill));

            Assert.IsTrue(drill.RequestMine());
            Assert.IsFalse(drill.RequestMine(), "one cycle at a time");
            Run(Drill.CycleTicks + 1);
            Assert.AreEqual(1, drill.TotalMined);
            Assert.AreEqual(1, crate.Inventory.CountOf(ItemCatalog.IronOre));

            for (int i = 0; i < 5; i++) { drill.RequestMine(); Run(Drill.CycleTicks + 1); }
            Assert.AreEqual(3, crate.Inventory.CountOf(ItemCatalog.IronOre));
            Assert.AreEqual(MachineState.Depleted, drill.State);
            Assert.IsFalse(_map.TryGetNode(new GridPos(13, 10), out _));
        }

        [Test]
        public void Drill_HalfPowerTakesTwiceAsLong()
        {
            var grid = new PowerGrid();
            grid.AddPole(new PowerPole(new GridPos(0, 0)));
            var map = new WorldMap(10, 10, 0);
            map.AddNode(new ResourceNode(new GridPos(1, 1), ItemCatalog.Coal, 10));
            var bg = new BuildGrid(map, _items);
            bg.TryPlace(StructureCatalog.Find(StructureCatalog.Drill), new GridPos(0, 0), Direction.North, null, out var s);
            bg.TryPlace(StructureCatalog.Find(StructureCatalog.Computer), new GridPos(2, 2), Direction.North, null, out var cs);
            var drill = new Drill(s);
            var comp = new Computer(cs, ComputerHardware.Basic);
            comp.LoadProgram(ProgrammingLanguage.JavaScript, "drill.mine()");
            comp.MarkRunning();
            comp.Link(drill);
            grid.AddNode(drill); grid.AddNode(comp);
            // Exactly half of total demand (computer 30 W + working drill 90 W).
            grid.AddNode(new TestProducer(new GridPos(0, 1), 60));
            var ctx = new MachineContext(map, bg, Dt);
            drill.RequestMine();
            int ticks = 0;
            while (drill.TotalMined == 0 && ticks < 1000) { grid.Tick(Dt); comp.Tick(ctx); drill.Tick(ctx); ticks++; }
            Assert.AreEqual(Drill.CycleTicks * 2, ticks, 1);
        }

        [Test]
        public void Drill_BlocksWhenOutputFullAndNothingIsLost()
        {
            _map.AddNode(new ResourceNode(new GridPos(17, 11), ItemCatalog.Coal, 100));
            var c = PoweredComputer();
            Place(StructureCatalog.PowerPole, 15, 12);
            var drill = _sys.Get<Drill>(Place(StructureCatalog.Drill, 16, 10));
            Assert.AreEqual(LinkResult.Ok, c.Link(drill));
            for (int i = 0; i < 60; i++) { drill.RequestMine(); Run(Drill.CycleTicks + 1); }
            Assert.AreEqual(MachineState.OutputBlocked, drill.State);
            Assert.AreEqual(50, drill.Structure.Inventory.CountOf(ItemCatalog.Coal));
            _map.TryGetNode(new GridPos(17, 11), out var node);
            Assert.AreEqual(50, node.Remaining);
        }

        // ---------- computer ----------

        [Test]
        public void Computer_LinksRespectRangePortsAndOwnership()
        {
            var c = PoweredComputer();
            var other = _sys.Get<Computer>(Place(StructureCatalog.Computer, 11, 10));
            var gens = new CoalGenerator[6];
            int[] xs = { 12, 15, 18, 12, 15, 30 };
            int[] ys = { 12, 12, 12, 15, 15, 30 };
            for (int i = 0; i < 6; i++) gens[i] = _sys.Get<CoalGenerator>(Place(StructureCatalog.CoalGenerator, xs[i], ys[i]));

            Assert.AreEqual(LinkResult.OutOfRange, c.Link(gens[5]));
            Assert.AreEqual(LinkResult.NotProgrammable, c.Link(other));
            for (int i = 0; i < 4; i++) Assert.AreEqual(LinkResult.Ok, c.Link(gens[i]));
            Assert.AreEqual(LinkResult.AlreadyLinked, c.Link(gens[0]));
            Assert.AreEqual(LinkResult.NoFreePorts, c.Link(gens[4]));
            Assert.AreEqual(LinkResult.LinkedToOtherComputer, other.Link(gens[0]));

            _grid.TryRemove(gens[0].Structure.Cells[0], null, out _);
            Assert.AreEqual(3, c.Links.Count);
            _grid.TryRemove(new GridPos(10, 10), null, out _);
            Assert.IsNull(gens[1].ControlledBy);
        }

        [Test]
        public void Computer_ProgramMustFitInStorage()
        {
            var c = PoweredComputer();
            Assert.IsFalse(c.LoadProgram(ProgrammingLanguage.C, new string('x', ComputerHardware.Basic.StorageBytes + 1)));
            Assert.AreEqual("storage_full", c.LastError);
            Assert.AreEqual(ProgramStatus.Running, c.ProgramStatus, "previous program untouched");
        }

        // ---------- generator ----------

        [Test]
        public void Generator_PowersNetworkFromCoalAndRechargesBattery()
        {
            var c = PoweredComputer();
            Place(StructureCatalog.PowerPole, 13, 10);
            var gen = _sys.Get<CoalGenerator>(Place(StructureCatalog.CoalGenerator, 14, 10));
            gen.Structure.Inventory.Add(ItemCatalog.Coal, 2);
            c.Link(gen);
            var battery = _sys.GetBattery(_grid.GetAt(new GridPos(7, 10)));
            typeof(Battery).GetProperty("StoredJoules").SetValue(battery, 1000.0);

            Run(20);
            Assert.AreEqual(MachineState.Idle, gen.State, "disabled until the program enables it");
            gen.Enabled = true;
            Run(20); // 1 s: 30 W to computer + up to 200 W charging the battery
            Assert.AreEqual(MachineState.Working, gen.State);
            Assert.AreEqual(1, gen.FuelCount, "one coal loaded into the burner");
            Assert.Greater(battery.StoredJoules, 1000.0 - 30 + 150);
            Assert.AreEqual(1.0, c.Satisfaction, 1e-9);

            Run(20 * 120); // burn everything
            Assert.AreEqual(0, gen.FuelCount);
            Assert.AreEqual(MachineState.Depleted, gen.State);
        }

        // ---------- helpers ----------

        private sealed class TestConsumer : PowerNode
        {
            private readonly double _w;
            public TestConsumer(GridPos c, double w) : base(new[] { c }) { _w = w; }
            public override double DemandWatts => _w;
        }

        private sealed class TestProducer : PowerNode
        {
            private readonly double _w;
            public double Used;
            public TestProducer(GridPos c, double w) : base(new[] { c }) { _w = w; }
            public override double ProductionWatts => _w;
            protected internal override void OnProductionUsed(double watts, double tickSeconds) => Used = watts;
        }
    }
}
