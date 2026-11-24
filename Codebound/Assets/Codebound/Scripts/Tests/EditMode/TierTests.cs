using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 10: machine versions — faster, hungrier, and the same to programs.</summary>
    public class TierTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 111)
        {
            var g = new GameState(seed);
            var s = g.World.Generator.FindSpawn();
            for (int r = 3; r < 200; r++)
                for (int x = -r; x <= r; x++)
                    foreach (int y in new[] { -r, r })
                    {
                        bool ok = true;
                        for (int dx = 0; dx < w && ok; dx++) for (int dy = 0; dy < h && ok; dy++) ok = g.CanPlace(BuildingDatabase.Get("wall"), new Cell(s.X + x + dx, s.Y + y + dy), Rotation.R0);
                        if (ok) return (g, s.X + x, s.Y + y);
                    }
            throw new AssertionException("no room");
        }

        static PlacedBuilding P(GameState g, string id, int x, int y) =>
            g.Grid.Place(BuildingDatabase.Get(id), new Cell(x, y), Rotation.R0) ?? throw new AssertionException($"cannot place {id}");

        /// <summary>Crafts gears in a loop (as a program would) and counts them.</summary>
        static int Gears(string model, float seconds)
        {
            var (g, x, y) = World(6, 3);
            P(g, "generator", x, y).State.TryInsert(ItemDatabase.Coal, 50);   // 150 kW: enough for any version
            var asm = P(g, model, x + 3, y);
            var st = (AssemblerState)asm.State;
            st.Input.Add(ItemDatabase.IronPlate, 100);
            int made = 0;
            for (float t = 0; t < seconds; t += 0.05f)
            {
                if (!g.Factory.HasAction(asm)) g.Factory.StartCraft(asm, ItemDatabase.IronGear, r => { if (r != null) made++; });
                g.Factory.Tick(0.05f);
                st.Output.TryRemove(ItemDatabase.IronGear, st.Output.Count(ItemDatabase.IronGear));
            }
            return made;
        }

        [Test]
        public void LaterAssemblers_AreFaster()
        {
            int a1 = Gears("assembler", 20f), a2 = Gears("assembler2", 20f), a3 = Gears("assembler3", 20f);
            TestContext.WriteLine($"gears in 20 s: {a1} / {a2} / {a3}");
            Assert.AreEqual(a1 * 1.5, a2, 2.5, "Mk2 is 1.5x (each piece takes whole ticks: 13.3 -> 14)");
            Assert.AreEqual(a1 * 2.25, a3, 2.5, "Mk3 is 2.25x");
        }

        [Test]
        public void FasterMachines_DrawMorePower()
        {
            var (g, x, y) = World(8, 3);
            P(g, "generator", x, y).State.TryInsert(ItemDatabase.Coal, 50);
            var a1 = P(g, "assembler", x + 2, y); var a3 = P(g, "assembler3", x + 4, y);
            ((AssemblerState)a1.State).Input.Add(ItemDatabase.IronPlate, 10); ((AssemblerState)a3.State).Input.Add(ItemDatabase.IronPlate, 10);
            g.Factory.StartCraft(a1, ItemDatabase.IronGear, null);
            g.Factory.Tick(0.05f);
            float one = g.Factory.NetworkStats(a1).demandKW;
            g.Factory.StartCraft(a3, ItemDatabase.IronGear, null);
            g.Factory.Tick(0.05f);
            Assert.AreEqual(Factory.AssemblerKW, one, 0.01f);
            Assert.AreEqual(Factory.AssemblerKW * (1 + 2.5f), g.Factory.NetworkStats(a1).demandKW, 0.01f);
        }

        [Test]
        public void TheElectricFurnace_SmeltsTwiceAsFast_OnPower_WithoutCoal()
        {
            var (g, x, y) = World(6, 3);
            var gen = P(g, "generator", x, y); gen.State.TryInsert(ItemDatabase.Coal, 20);
            var fb = P(g, "electric_furnace", x + 2, y);
            var f = (FurnaceState)fb.State;
            Assert.IsTrue(f.Electric);
            Assert.IsFalse(f.TryInsert(ItemDatabase.Coal, 1), "no coal needed");
            f.TryInsert(ItemDatabase.IronOre, 10);
            Assert.IsNull(g.Factory.StartSmelt(fb, null));
            int ticks = 0;
            while (f.Output.Count(ItemDatabase.IronPlate) == 0 && ticks < 200) { g.Factory.Tick(0.05f); ticks++; }
            Assert.AreEqual(Factory.SmeltSeconds / 2f, ticks * 0.05f, 0.11f, "half the time of a stone furnace");
            g.Grid.Remove(gen);
            Assert.AreEqual("no_power", g.Factory.StartSmelt(fb, null));
        }

        [Test]
        public void TheMk2Drill_MinesTwiceAsFast()
        {
            int Mine(string model)
            {
                var g = new GameState(111);
                var s = g.World.Generator.FindSpawn();
                for (int r = 2; r < 150; r++)
                    for (int dx = -r; dx <= r; dx++)
                        foreach (int dy in new[] { -r, r })
                        {
                            var c = new Cell(s.X + dx, s.Y + dy);
                            if (!g.CanPlace(BuildingDatabase.Get(model), c, Rotation.R0) || !g.CanPlace(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y), Rotation.R0)
                                || !g.CanPlace(BuildingDatabase.Get("generator"), new Cell(c.X + 4, c.Y), Rotation.R0)) continue;
                            var d = g.Grid.Place(BuildingDatabase.Get(model), c, Rotation.R0);
                            g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y), Rotation.R0).State.TryInsert(ItemDatabase.Coal, 50);
                            g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(c.X + 4, c.Y), Rotation.R0).State.TryInsert(ItemDatabase.Coal, 50);   // the Mk2 draws 180 kW
                            int n = 0;
                            for (float t = 0; t < 10f; t += 0.05f)
                            {
                                if (!g.Factory.HasAction(d)) g.Factory.StartMine(d, it => { if (it != null) n++; });
                                g.Factory.Tick(0.05f);
                                var o = ((DrillState)d.State).Output; if (!o[0].IsEmpty) o.TryRemove(o[0].ItemId, o[0].Count);
                            }
                            return n;
                        }
                throw new AssertionException("no ore field");
            }
            int m1 = Mine("drill"), m2 = Mine("drill2");
            Assert.AreEqual(m1 * 2, m2, 2, $"mk1 {m1}, mk2 {m2}");
        }

        [Test]
        public void ProgramsSee_TheFirstVersionsType()
        {
            Assert.AreEqual("drill", BuildingDatabase.Get("drill2").ProgramType);
            Assert.AreEqual("assembler", BuildingDatabase.Get("assembler3").ProgramType);
            Assert.AreEqual("furnace", BuildingDatabase.Get("electric_furnace").ProgramType);
            Assert.AreEqual("drill", BuildingDatabase.Get("drill").ProgramType);
            Assert.IsNotNull(Research.UnlockerOf("assembler3"));
        }
    }
}
