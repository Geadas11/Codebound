using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class FarmAndCraftTests
    {
        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds; t += 0.05f) g.Factory.Tick(0.05f); }

        static PlacedBuilding PlaceNear(GameState g, string id, System.Func<Cell, bool> extra = null)
        {
            var s = g.World.Generator.FindSpawn();
            for (int r = 1; r < 200; r++)
                for (int x = -r; x <= r; x++)
                    for (int y = -r; y <= r; y++)
                    {
                        var c = new Cell(s.X + x, s.Y + y);
                        if (extra != null && !extra(c)) continue;
                        var b = g.Grid.Place(BuildingDatabase.Get(id), c, Rotation.R0);
                        if (b != null) return b;
                    }
            Assert.Fail("no room for " + id);
            return null;
        }

        static bool NearLake(GameState g, Cell c)
        {
            for (int dx = -3; dx <= 3; dx++) for (int dy = -3; dy <= 3; dy++)
                    if (g.World.Generator.TileAt(new Cell(c.X + dx, c.Y + dy)) == TileType.Water) return true;
            return false;
        }

        [Test]
        public void Greenhouse_PlantWaterGrowHarvest_NeedsLightAndWater()
        {
            var g = new GameState(42);
            var gh = PlaceNear(g, "greenhouse", c => NearLake(g, c));
            var st = (GreenhouseState)gh.State;
            Assert.IsTrue(st.NearWater);
            Assert.AreEqual("no_seeds", g.Factory.Greenhouse(gh, "plant", null).error);
            Assert.IsTrue(st.TryInsert(ItemDatabase.AlienFruit, 2));
            Assert.IsFalse(st.TryInsert(ItemDatabase.Stone, 1));
            Assert.AreEqual(0, g.Factory.Greenhouse(gh, "plant", null).value);
            Assert.AreEqual(1, g.Factory.Greenhouse(gh, "plant", null).value);
            Assert.AreEqual("no_seeds", g.Factory.Greenhouse(gh, "plant", null).error);

            g.Factory.Daylight = 1f;
            Run(g, 30);                                    // dry soil: nothing grows, the tank fills from the lake
            Assert.AreEqual(0f, st.Growth[0]);
            Assert.Greater(st.Tank, 50f);
            Assert.AreEqual(0, g.Factory.Greenhouse(gh, "water", 0d).value);
            Assert.AreEqual(1, g.Factory.Greenhouse(gh, "water", null).value);

            g.Factory.Daylight = 0f;                       // night, no power for grow lamps
            Run(g, 20);
            Assert.AreEqual(0f, st.Growth[0], 1e-4, "no light, no growth");

            g.Factory.Daylight = 1f;
            for (int i = 0; i < 40 && st.Growth[0] < 1; i++) { Run(g, 10); g.Factory.Greenhouse(gh, "water", null); g.Factory.Greenhouse(gh, "water", null); }
            Assert.AreEqual(1f, st.Growth[0], "ripe after enough light and water");
            Assert.AreEqual(GreenhouseState.Yield, g.Factory.Greenhouse(gh, "harvest", null).value);
            Assert.AreEqual(GreenhouseState.Yield, st.Output.Count(ItemDatabase.AlienFruit));
            var plots = (Dictionary<string, object>)g.Factory.Greenhouse(gh, "plots", null).value;
            Assert.AreEqual(GreenhouseState.Plots, ((List<object>)plots["plots"]).Count);

            // save round trip keeps the plots
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var st2 = (GreenhouseState)SaveSystem.Read(new StringReader(sw.ToString())).Grid.At(gh.Origin).State;
            Assert.AreEqual(st.Growth[1], st2.Growth[1], 1e-5); Assert.AreEqual(st.Tank, st2.Tank, 1e-3);
        }

        [Test]
        public void Assembler_CraftsGearsWiresCircuits_WithPower()
        {
            var g = new GameState(42);
            var asm = PlaceNear(g, "assembler");
            var st = (AssemblerState)asm.State;
            Assert.AreEqual("unknown_recipe", g.Factory.StartCraft(asm, "diamond", null));
            Assert.IsTrue(st.TryInsert(ItemDatabase.CopperPlate, 2));
            Assert.IsTrue(st.TryInsert(ItemDatabase.IronPlate, 3));
            Assert.IsTrue(st.TryInsert(ItemDatabase.Silicon, 1));
            Assert.IsFalse(st.TryInsert(ItemDatabase.Wood, 1), "wood is not an ingredient");
            Assert.AreEqual("no_power", g.Factory.StartCraft(asm, ItemDatabase.IronGear, null));

            var gen = PlaceNear(g, "generator", c => System.Math.Abs(c.X - asm.Origin.X) <= 4 && System.Math.Abs(c.Y - asm.Origin.Y) <= 4);
            gen.State.TryInsert(ItemDatabase.Coal, 10);
            string result = "pending";
            Assert.IsNull(g.Factory.StartCraft(asm, ItemDatabase.CopperWire, r => result = r));
            Run(g, 1f);
            Assert.AreEqual(ItemDatabase.CopperWire, result);
            Assert.AreEqual(2, st.Output.Count(ItemDatabase.CopperWire));
            st.Input.Add(st.Output[0].ItemId, st.Output[0].Count); st.Output.TryRemove(ItemDatabase.CopperWire, 2);
            Assert.IsNull(g.Factory.StartCraft(asm, ItemDatabase.CopperWire, null)); Run(g, 1f);
            st.Input.Add(ItemDatabase.CopperWire, 2); st.Output.TryRemove(ItemDatabase.CopperWire, 2);
            Assert.IsNull(g.Factory.StartCraft(asm, ItemDatabase.Circuit, r => result = r));
            Run(g, 3.5f);
            Assert.AreEqual(ItemDatabase.Circuit, result);
            Assert.AreEqual(1, st.Output.Count(ItemDatabase.Circuit));
            Assert.AreEqual(0, st.Input.Count(ItemDatabase.Silicon));
            StringAssert.StartsWith("missing_", g.Factory.StartCraft(asm, ItemDatabase.Circuit, null), "ingredients were consumed");
        }

        [Test]
        public void AdvancedComputer_HasBetterHardware()
        {
            var g = new GameState(1);
            var pc = PlaceNear(g, "computer2");
            var cs = (ComputerState)pc.State;
            Assert.AreEqual(60, cs.CallsPerSecond); Assert.AreEqual(64, cs.RamMb);
            Assert.AreEqual(20, ((ComputerState)PlaceNear(g, "computer").State).CallsPerSecond);
        }
    }
}
