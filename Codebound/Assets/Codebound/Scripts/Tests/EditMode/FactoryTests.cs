using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class FactoryTests
    {
        /// <summary>Finds an origin where a 2x2 drill covers an iron/copper/coal/stone deposit and there is free land around.</summary>
        static (GameState g, Cell drillOrigin) WorldWithOre()
        {
            var g = new GameState(42);
            for (int x = -150; x < 150; x++)
                for (int y = -150; y < 150; y++)
                {
                    var o = new Cell(x, y);
                    var dep = g.World.DepositAt(o);
                    if (!WorldRules.DrillCanMine(dep) || dep.Amount < 20) continue;
                    if (!g.CanPlace(BuildingDatabase.Get("drill"), o, Rotation.R0)) continue;
                    // need free buildable cells for a chest above and a generator to the left
                    if (g.CanPlace(BuildingDatabase.Get("chest"), new Cell(x, y + 2), Rotation.R0) &&
                        g.CanPlace(BuildingDatabase.Get("generator"), new Cell(x - 3, y), Rotation.R0))
                        return (g, o);
                }
            Assert.Fail("no suitable ore spot");
            return default;
        }

        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds; t += 0.05f) g.Factory.Tick(0.05f); }

        [Test]
        public void Drill_NeedsPower_ThenMinesIntoChestInFront()
        {
            var (g, o) = WorldWithOre();
            var drill = g.Grid.Place(BuildingDatabase.Get("drill"), o, Rotation.R0);
            var chest = g.Grid.Place(BuildingDatabase.Get("chest"), new Cell(o.X, o.Y + 2), Rotation.R0);
            Assert.NotNull(drill); Assert.NotNull(chest);

            Assert.AreEqual("no_power", g.Factory.StartMine(drill, null));

            var gen = g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(o.X - 3, o.Y), Rotation.R0);
            Assert.NotNull(gen);
            Assert.AreEqual("no_power", g.Factory.StartMine(drill, null), "generator without fuel");
            Assert.IsTrue(gen.State.TryInsert(ItemDatabase.Coal, 5));

            string mined = "pending";
            Assert.IsNull(g.Factory.StartMine(drill, r => mined = r));
            Assert.AreEqual("busy", g.Factory.StartMine(drill, null));
            Run(g, Factory.MineSeconds + 0.2f);
            Assert.AreNotEqual("pending", mined);
            Assert.NotNull(mined);
            Run(g, 0.2f);
            Assert.AreEqual(1, ((StorageState)chest.State).Items.Count(mined), "drill output pushed into chest in front");
            Assert.Less(((GeneratorState)gen.State).Fuel.Count(ItemDatabase.Coal), 5, "generator burned coal");
        }

        [Test]
        public void Furnace_SmeltsWithFuel()
        {
            var g = new GameState(1);
            var spot = g.World.Generator.FindSpawn();
            BuildingDef fdef = BuildingDatabase.Get("furnace");
            PlacedBuilding f = null;
            for (int x = 0; x < 50 && f == null; x++) f = g.Grid.Place(fdef, new Cell(spot.X + x, spot.Y), Rotation.R0);
            Assert.NotNull(f);
            var st = (FurnaceState)f.State;
            Assert.AreEqual("no_input", g.Factory.StartSmelt(f, null));
            Assert.IsTrue(st.TryInsert(ItemDatabase.IronOre, 3));
            Assert.AreEqual("no_fuel", g.Factory.StartSmelt(f, null));
            Assert.IsTrue(st.TryInsert(ItemDatabase.Coal, 1));
            Assert.IsFalse(st.TryInsert(ItemDatabase.Wood, 1), "wood is neither fuel nor ore");
            string product = null;
            Assert.IsNull(g.Factory.StartSmelt(f, r => product = r));
            Run(g, Factory.SmeltSeconds + 0.1f);
            Assert.AreEqual(ItemDatabase.IronPlate, product);
            Assert.AreEqual(1, st.Output.Count(ItemDatabase.IronPlate));
            Assert.AreEqual(2, st.Input.Count(ItemDatabase.IronOre));
        }

        [Test]
        public void Deconstruct_RefundsCostAndContents()
        {
            var g = new GameState(3);
            var s = g.World.Generator.FindSpawn();
            g.Inventory.Add(ItemDatabase.Wood, 8);
            PlacedBuilding chest = null;
            for (int x = 1; x < 30 && chest == null; x++) chest = g.TryBuild(BuildingDatabase.Get("chest"), new Cell(s.X + x, s.Y), Rotation.R0);
            Assert.NotNull(chest);
            Assert.AreEqual(0, g.Inventory.Count(ItemDatabase.Wood));
            Assert.IsTrue(chest.State.TryInsert(ItemDatabase.Coal, 30));
            Assert.IsTrue(g.TryDeconstruct(chest));
            Assert.AreEqual(8, g.Inventory.Count(ItemDatabase.Wood));
            Assert.AreEqual(30, g.Inventory.Count(ItemDatabase.Coal));
            Assert.AreEqual(0, g.Grid.Count);
        }

        [Test]
        public void Save_RoundTrip_PreservesEverything()
        {
            var (g, o) = WorldWithOre();
            g.PlayerX = 12.25; g.PlayerY = -3.5;
            g.Inventory.Add(ItemDatabase.Stone, 42);
            var drill = g.Grid.Place(BuildingDatabase.Get("drill"), o, Rotation.R90);
            var comp = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(o.X - 3, o.Y), Rotation.R0);
            Assert.NotNull(comp);
            ((ComputerState)comp.State).Code = "log('olá ção');\nwhile (true) { sleep(1); }";
            ((ComputerState)comp.State).Running = true;
            ((DrillState)drill.State).Output.Add(ItemDatabase.IronOre, 7);
            Cell minedCell = o;
            int before = g.World.DepositAt(minedCell).Amount;
            g.World.Extract(minedCell, 3);

            var sw = new StringWriter();
            SaveSystem.Write(g, sw);
            var g2 = SaveSystem.Read(new StringReader(sw.ToString()));

            Assert.AreEqual(g.Seed, g2.Seed);
            Assert.AreEqual(12.25, g2.PlayerX); Assert.AreEqual(-3.5, g2.PlayerY);
            Assert.AreEqual(42, g2.Inventory.Count(ItemDatabase.Stone));
            Assert.AreEqual(before - 3, g2.World.DepositAt(minedCell).Amount);
            Assert.AreEqual(2, g2.Grid.Count);
            var d2 = g2.Grid.At(o);
            Assert.AreEqual(Rotation.R90, d2.Rotation);
            Assert.AreEqual(7, ((DrillState)d2.State).Output.Count(ItemDatabase.IronOre));
            var c2 = (ComputerState)g2.Grid.At(new Cell(o.X - 3, o.Y)).State;
            Assert.AreEqual(((ComputerState)comp.State).Code, c2.Code);
            Assert.IsTrue(c2.Running);
            // writing again gives identical text (stable format)
            var sw2 = new StringWriter(); SaveSystem.Write(g2, sw2);
            Assert.AreEqual(sw.ToString(), sw2.ToString());
        }

        [Test]
        public void Belts_CarryItemsIntoChest_ProgrammableConveyorObeysStartAndFilter()
        {
            var g = new GameState(11);
            var s = g.World.Generator.FindSpawn();
            // find a free horizontal run of 4 cells: belt, conveyor, belt -> chest (all facing right)
            Cell o = default; bool ok = false;
            for (int y = -40; y < 40 && !ok; y++)
                for (int x = -40; x < 40 && !ok; x++)
                {
                    ok = true;
                    for (int i = 0; i < 4 && ok; i++) ok = g.CanPlace(BuildingDatabase.Get("chest"), new Cell(s.X + x + i, s.Y + y), Rotation.R0);
                    if (ok) o = new Cell(s.X + x, s.Y + y);
                }
            Assert.IsTrue(ok);
            var b1 = g.Grid.Place(BuildingDatabase.Get("belt"), o, Rotation.R90);
            var conv = g.Grid.Place(BuildingDatabase.Get("conveyor"), new Cell(o.X + 1, o.Y), Rotation.R90);
            var b2 = g.Grid.Place(BuildingDatabase.Get("belt"), new Cell(o.X + 2, o.Y), Rotation.R90);
            var chest = g.Grid.Place(BuildingDatabase.Get("chest"), new Cell(o.X + 3, o.Y), Rotation.R0);
            var cs = (ConveyorState)conv.State;

            Assert.IsTrue(b1.State.TryInsert(ItemDatabase.IronOre, 1));
            Assert.IsTrue(b1.State.TryInsert(ItemDatabase.IronOre, 1), "two lanes: a second item fits beside the first");
            Assert.IsFalse(b1.State.TryInsert(ItemDatabase.IronOre, 1), "but not a third in the middle of the cell");
            Run(g, 2f);
            Assert.AreEqual(ItemDatabase.IronOre, cs.Item, "programmable conveyor holds the item while stopped");
            Assert.AreEqual(0, ((StorageState)chest.State).Items.Count(ItemDatabase.IronOre));

            cs.Running = true;
            Run(g, 2f);
            Assert.AreEqual(2, ((StorageState)chest.State).Items.Count(ItemDatabase.IronOre), "both delivered once started");

            cs.Filter = ItemDatabase.Coal;
            Assert.IsTrue(b1.State.TryInsert(ItemDatabase.IronOre, 1));
            Run(g, 2f);
            Assert.IsNull(cs.Item, "filter rejects iron");
            Assert.AreEqual("blocked", b1.State.Status);
        }

        [Test]
        public void Inserter_MovesFromChestIntoFurnace_RespectsFilterAndStop()
        {
            var g = new GameState(21);
            var s = g.World.Generator.FindSpawn();
            Cell o = default; bool ok = false;
            for (int y = -40; y < 40 && !ok; y++)
                for (int x = -40; x < 40 && !ok; x++)
                {
                    var c = new Cell(s.X + x, s.Y + y);
                    ok = g.CanPlace(BuildingDatabase.Get("chest"), c, Rotation.R0)
                        && g.CanPlace(BuildingDatabase.Get("chest"), new Cell(c.X + 1, c.Y), Rotation.R0)
                        && g.CanPlace(BuildingDatabase.Get("furnace"), new Cell(c.X + 2, c.Y), Rotation.R0);
                    if (ok) o = c;
                }
            Assert.IsTrue(ok);
            var chest = g.Grid.Place(BuildingDatabase.Get("chest"), o, Rotation.R0);
            var arm = g.Grid.Place(BuildingDatabase.Get("inserter"), new Cell(o.X + 1, o.Y), Rotation.R90); // faces right
            var furnace = g.Grid.Place(BuildingDatabase.Get("furnace"), new Cell(o.X + 2, o.Y), Rotation.R0);
            var items = ((StorageState)chest.State).Items;
            var f = (FurnaceState)furnace.State;
            var ins = (InserterState)arm.State;
            items.Add(ItemDatabase.IronOre, 3);
            items.Add(ItemDatabase.Coal, 2);
            items.Add(ItemDatabase.Wood, 5);

            Run(g, 5f); // ~6 swings
            Assert.AreEqual(3, f.Input.Count(ItemDatabase.IronOre));
            Assert.AreEqual(2, f.Fuel.Count(ItemDatabase.Coal));
            Assert.AreEqual(5, items.Count(ItemDatabase.Wood), "furnace refuses wood, so it stays in the chest");
            Assert.AreEqual(5, ins.Moved);

            ins.Running = false;
            items.Add(ItemDatabase.CopperOre, 1);
            Run(g, 2f);
            Assert.AreEqual(1, items.Count(ItemDatabase.CopperOre), "stopped inserter does nothing");

            ins.Running = true; ins.Filter = ItemDatabase.Coal;
            Run(g, 2f);
            Assert.AreEqual(1, items.Count(ItemDatabase.CopperOre), "filter only lets coal through");
        }

        [Test]
        public void Load_RejectsCorruptFiles()
        {
            Assert.Throws<InvalidDataException>(() => SaveSystem.Read(new StringReader("hello")));
            Assert.Throws<InvalidDataException>(() => SaveSystem.Read(new StringReader("CODEBOUND 1\nseed 5\n"))); // truncated
            Assert.Throws<InvalidDataException>(() => SaveSystem.Read(new StringReader("CODEBOUND 1\nseed 5\ninv nonsense 3\nend\n")));
        }
    }
}
