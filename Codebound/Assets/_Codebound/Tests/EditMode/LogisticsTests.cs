using System.IO;
using Codebound.Building;
using Codebound.Core;
using Codebound.Items;
using Codebound.Machines;
using Codebound.Save;
using Codebound.World;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class LogisticsTests
    {
        private GameState _s;
        private long _tick;

        [SetUp]
        public void SetUp()
        {
            _s = GameState.CreateNew(11);
            _tick = 0;
            // Clear a flat test area far from the crash site.
            for (int y = 2; y < 30; y++)
            for (int x = 2; x < 30; x++)
            {
                var c = new GridPos(x, y);
                _s.World.SetTerrain(c, TerrainType.Grass);
                _s.World.RemoveNode(c);
            }
        }

        private PlacedStructure Place(string id, int x, int y, Direction f = Direction.North)
        {
            Assert.AreEqual(PlacementStatus.Ok, _s.Grid.TryPlace(StructureCatalog.Find(id), new GridPos(x, y), f, null, out var p), id);
            return p;
        }

        private void Run(int ticks) { for (int i = 0; i < ticks; i++) _s.Machines.Tick(++_tick); }

        private Computer Computer(int x, int y)
        {
            Place(StructureCatalog.PowerPole, x - 1, y);
            // Black start: a battery powers the computer so its program can start the generator.
            Place(StructureCatalog.EmergencyBattery, x - 2, y + 2);
            var gen = Place(StructureCatalog.CoalGenerator, x - 4, y);
            var c = _s.Machines.Get<Computer>(Place(StructureCatalog.Computer, x, y));
            c.LoadProgram(ProgrammingLanguage.Python, "...");
            gen.Inventory.Add(ItemCatalog.Coal, 50);
            var g = _s.Machines.Get<CoalGenerator>(gen);
            c.MarkRunning(); // what the Etapa 3 runtime reports once the program started
            Assert.AreEqual(LinkResult.Ok, c.Link(g));
            g.Enabled = true;
            return c;
        }

        [Test]
        public void Belt_MovesItemsAlongAndIntoCrate()
        {
            for (int x = 5; x < 9; x++) Place(StructureCatalog.Conveyor, x, 5, Direction.East);
            var crate = Place(StructureCatalog.Crate, 9, 5);
            Assert.IsTrue(_s.Machines.Conveyors.TryInsert(new GridPos(5, 5), ItemCatalog.IronOre));
            Assert.IsFalse(_s.Machines.Conveyors.TryInsert(new GridPos(5, 5), ItemCatalog.IronOre), "one item per cell");
            Run(ConveyorSystem.TicksPerCell * 4 + 4);
            Assert.AreEqual(1, crate.Inventory.CountOf(ItemCatalog.IronOre));
            foreach (var b in _s.Machines.Conveyors.Belts) Assert.IsTrue(b.IsEmpty);
        }

        [Test]
        public void Belt_BacksUpWhenBlockedAndLosesNothing()
        {
            for (int x = 5; x < 8; x++) Place(StructureCatalog.Conveyor, x, 5, Direction.East);
            // Nothing at the end (8,5): items queue up.
            int inserted = 0;
            for (int i = 0; i < 40; i++)
            {
                if (_s.Machines.Conveyors.TryInsert(new GridPos(5, 5), ItemCatalog.Coal)) inserted++;
                Run(3);
            }
            Assert.AreEqual(3, inserted, "three belts hold three items");
            int onBelts = 0;
            foreach (var b in _s.Machines.Conveyors.Belts) if (!b.IsEmpty) onBelts++;
            Assert.AreEqual(3, onBelts);
        }

        [Test]
        public void Belt_RemovalGivesCarriedItemToPlayer()
        {
            Place(StructureCatalog.Conveyor, 5, 5, Direction.East);
            _s.Machines.Conveyors.TryInsert(new GridPos(5, 5), ItemCatalog.CopperOre);
            _s.Grid.TryRemove(new GridPos(5, 5), _s.PlayerInventory, out _);
            Assert.AreEqual(1, _s.PlayerInventory.CountOf(ItemCatalog.CopperOre));
        }

        [Test]
        public void Chain_DrillToBeltToFurnaceToCrate()
        {
            // ore under drill at (10..11, 10..11), drill faces east, belt (12,10)->(13,10) east into furnace (14..15,10..11)
            _s.World.AddNode(new ResourceNode(new GridPos(10, 10), ItemCatalog.IronOre, 3));
            var c = Computer(12, 14);
            var drill = _s.Machines.Get<Drill>(Place(StructureCatalog.Drill, 10, 10, Direction.East));
            Place(StructureCatalog.Conveyor, 12, 10, Direction.East);
            Place(StructureCatalog.Conveyor, 13, 10, Direction.East);
            var furnace = _s.Machines.Get<Furnace>(Place(StructureCatalog.Furnace, 14, 10, Direction.East));
            var crate = Place(StructureCatalog.Crate, 16, 10);
            Place(StructureCatalog.PowerPole, 13, 12);
            Assert.AreEqual(LinkResult.Ok, c.Link(drill));
            Assert.AreEqual(LinkResult.Ok, c.Link(furnace));
            Assert.AreEqual(new GridPos(16, 10), furnace.OutputCell);

            // Simulated program: keep both machines busy (what `while True: drill.mine(); furnace.smelt()` will do).
            for (int t = 0; t < 2000 && crate.Inventory.CountOf(ItemCatalog.IronPlate) < 3; t++)
            {
                drill.RequestMine();
                furnace.RequestSmelt();
                Run(1);
            }
            Assert.AreEqual(3, crate.Inventory.CountOf(ItemCatalog.IronPlate));
            Assert.AreEqual(3, drill.TotalMined);
            Assert.AreEqual(3, furnace.TotalProduced);
            Assert.AreEqual(0, furnace.Structure.Inventory.CountOf(ItemCatalog.IronOre));
        }

        [Test]
        public void BlackStart_GeneratorAloneCannotStartAComputer()
        {
            Place(StructureCatalog.PowerPole, 11, 14);
            var gen = Place(StructureCatalog.CoalGenerator, 8, 14);
            gen.Inventory.Add(ItemCatalog.Coal, 10);
            var c = _s.Machines.Get<Computer>(Place(StructureCatalog.Computer, 12, 14));
            c.LoadProgram(ProgrammingLanguage.Python, "generator.enable(True)");
            c.MarkRunning();
            var g = _s.Machines.Get<CoalGenerator>(gen);
            c.Link(g);
            g.Enabled = true;
            Run(40);
            Assert.IsFalse(c.IsRunningProgram, "no battery: the computer never gets power");
            Assert.AreEqual(0, g.LastOutputWatts);

            Place(StructureCatalog.EmergencyBattery, 10, 16);
            Run(40);
            Assert.IsTrue(c.IsRunningProgram);
            Assert.Greater(g.LastOutputWatts, 0);
        }

        [Test]
        public void Furnace_RejectsNonSmeltableFromLogistics()
        {
            var furnace = Place(StructureCatalog.Furnace, 10, 10, Direction.East);
            Assert.IsFalse(_s.Machines.Deliver(new GridPos(10, 10), ItemCatalog.Wood));
            Assert.IsTrue(_s.Machines.Deliver(new GridPos(11, 11), ItemCatalog.CopperOre));
            Assert.AreEqual(1, furnace.Inventory.CountOf(ItemCatalog.CopperOre));
        }

        [Test]
        public void Furnace_StoneNeedsTwoForOneBrick()
        {
            var r = SmeltingRecipe.ForInput(ItemCatalog.Stone);
            Assert.AreEqual(2, r.InputCount);
            Assert.AreEqual(ItemCatalog.StoneBrick, r.Output);
        }

        [Test]
        public void Save_PreservesBeltContentsAndFurnaceState()
        {
            Place(StructureCatalog.Conveyor, 5, 5, Direction.East);
            _s.Machines.Conveyors.Restore(new GridPos(5, 5), ItemCatalog.IronOre, 0.5);
            var f = _s.Machines.Get<Furnace>(Place(StructureCatalog.Furnace, 10, 10));
            f.RestoreState(true, SmeltingRecipe.ForInput(ItemCatalog.CopperOre), 0.25, 7);

            using var ms = new MemoryStream();
            SaveSystem.Save(_s, ms);
            ms.Position = 0;
            var l = SaveSystem.Load(ms);

            var belt = l.Machines.Conveyors.GetBelt(new GridPos(5, 5));
            Assert.AreEqual(ItemCatalog.IronOre, belt.Item);
            Assert.AreEqual(0.5, belt.Progress, 1e-9);
            var lf = l.Machines.Get<Furnace>(l.Grid.GetAt(new GridPos(10, 10)));
            Assert.AreEqual(ItemCatalog.CopperPlate, lf.Current.Output);
            Assert.AreEqual(0.25, lf.Progress, 1e-9);
            Assert.AreEqual(7, lf.TotalProduced);
        }
    }
}
