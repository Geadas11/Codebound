using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 11: modules and beacons.</summary>
    public class ModuleTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 121)
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

        /// <summary>An Assembler Mk3 with plenty of power at (x+4, y); generators at x and x+2.</summary>
        static (GameState g, PlacedBuilding asm, int x, int y) Setup()
        {
            var (g, x, y) = World(12, 4);
            P(g, "generator", x, y).State.TryInsert(ItemDatabase.Coal, 100);
            P(g, "generator", x + 2, y).State.TryInsert(ItemDatabase.Coal, 100);
            var asm = P(g, "assembler3", x + 4, y);
            ((AssemblerState)asm.State).Input.Add(ItemDatabase.IronPlate, 400);
            return (g, asm, x, y);
        }

        static int Craft(GameState g, PlacedBuilding asm, float seconds)
        {
            var st = (AssemblerState)asm.State;
            int before = 0, made = 0;
            for (float t = 0; t < seconds; t += 0.05f)
            {
                if (!g.Factory.HasAction(asm)) g.Factory.StartCraft(asm, ItemDatabase.IronGear, null);
                g.Factory.Tick(0.05f);
                made += st.Output.Count(ItemDatabase.IronGear) - before;
                st.Output.TryRemove(ItemDatabase.IronGear, st.Output.Count(ItemDatabase.IronGear));
            }
            return made;
        }

        [Test]
        public void Modules_GoIntoSlotsByHand_OnePerSlot()
        {
            var (g, asm, x, y) = Setup();
            var st = asm.State;
            Assert.AreEqual(4, st.Modules.SlotCount);
            for (int i = 0; i < 4; i++) Assert.IsTrue(st.PutByHand(ItemDatabase.SpeedModule));
            Assert.IsFalse(st.PutByHand(ItemDatabase.SpeedModule), "four slots, four modules");
            Assert.IsFalse(st.TryInsert(ItemDatabase.SpeedModule, 1), "arms and belts do not insert modules");
            Assert.IsNull(P(g, "assembler", x + 7, y).State.Modules, "the first assembler has no slots");
            g.Inventory.Clear();
            Assert.IsTrue(g.TryDeconstruct(asm));
            Assert.AreEqual(4, g.Inventory.Count(ItemDatabase.SpeedModule), "modules come back with the machine");
        }

        [Test]
        public void SpeedModules_MakeItFaster_AndHungrier_EfficiencyCutsPower()
        {
            var (g, asm, _, _) = Setup();
            int plain = Craft(g, asm, 10f);
            float plainKW = g.Factory.NetworkStats(asm).demandKW;
            for (int i = 0; i < 2; i++) asm.State.PutByHand(ItemDatabase.SpeedModule);
            Assert.AreEqual(1.4f, g.Factory.Effects(asm).speed + 1, 1e-5f);
            int fast = Craft(g, asm, 10f);
            float fastKW = g.Factory.NetworkStats(asm).demandKW;
            TestContext.WriteLine($"10 s: {plain} vs {fast} gears; {plainKW} vs {fastKW} kW");
            Assert.AreEqual(plain * 1.4, fast, 4, "+40 % speed (whole ticks per piece)");
            Assert.AreEqual(plainKW * 2f, fastKW, 0.5f, "+100 % power");
            var (g2, a2, _, _) = Setup();
            for (int i = 0; i < 4; i++) a2.State.PutByHand(ItemDatabase.EfficiencyModule);
            Craft(g2, a2, 1f);
            Assert.AreEqual(Factory.AssemblerKW * 2.5f * 0.2f, g2.Factory.NetworkStats(a2).demandKW, 0.5f, "efficiency: never below 20 %");
        }

        [Test]
        public void ProductivityModules_GiveFreeProducts()
        {
            var (g, asm, _, _) = Setup();
            for (int i = 0; i < 4; i++) asm.State.PutByHand(ItemDatabase.ProductivityModule);   // +16 %
            var st = (AssemblerState)asm.State;
            int crafts = 0, made = 0;
            for (int t = 0; t < 3000 && crafts < 50; t++)
            {
                if (!g.Factory.HasAction(asm)) { if (g.Factory.StartCraft(asm, ItemDatabase.IronGear, r => { if (r != null) crafts++; }) != null) break; }
                g.Factory.Tick(0.05f);
                made += st.Output.Count(ItemDatabase.IronGear);
                st.Output.TryRemove(ItemDatabase.IronGear, st.Output.Count(ItemDatabase.IronGear));
            }
            Assert.AreEqual(50, crafts);
            Assert.That(made, Is.InRange(57, 58), "50 crafts x 16 % = 8 free gears (7 if the float sum lands just under 8)");
            Assert.Less(asm.State.ProductivityBonus, 1f);
        }

        [Test]
        public void Beacons_PassOnHalf_OnlyWhilePowered_AndRefuseProductivity()
        {
            var (g, asm, x, y) = Setup();
            var beacon = P(g, "beacon", x + 6, y);   // next to the assembler
            Assert.IsFalse(beacon.State.PutByHand(ItemDatabase.ProductivityModule), "beacons take no productivity modules");
            Assert.IsTrue(beacon.State.PutByHand(ItemDatabase.SpeedModule)); Assert.IsTrue(beacon.State.PutByHand(ItemDatabase.SpeedModule));
            g.Factory.Tick(0.05f);
            Assert.AreEqual(0.2f, g.Factory.Effects(asm).speed, 1e-5f, "two speed modules at half strength");
            var far = P(g, "assembler2", x + 4, y + 2);   // within 3 cells of the beacon too
            Assert.AreEqual(0.2f, g.Factory.Effects(far).speed, 1e-5f);
            Assert.AreEqual(0f, g.Factory.Effects(P(g, "assembler", x + 9, y + 2)).speed, 1e-5f, "no slots, no beacon effect");
            // no power: no effect
            g.Grid.Remove(g.Grid.At(new Cell(x, y))); g.Grid.Remove(g.Grid.At(new Cell(x + 2, y)));
            g.Factory.Tick(0.05f);
            Assert.AreEqual(0f, g.Factory.Effects(asm).speed, 1e-5f);
        }

        [Test]
        public void ModulesAndBonus_AreSaved()
        {
            var (g, asm, x, y) = Setup();
            asm.State.PutByHand(ItemDatabase.ProductivityModule); asm.State.PutByHand(ItemDatabase.SpeedModule);
            Craft(g, asm, 5f);
            var w = new StringWriter(); SaveSystem.Write(g, w);
            StringAssert.Contains("bi modules productivity_module 1", w.ToString());
            var h = SaveSystem.Read(new StringReader(w.ToString()));
            var ha = h.Grid.ById(asm.InstanceId);
            Assert.AreEqual(1, ha.State.Modules.Count(ItemDatabase.SpeedModule));
            Assert.AreEqual(asm.State.ProductivityBonus, ha.State.ProductivityBonus, 1e-6f);
        }
    }
}
