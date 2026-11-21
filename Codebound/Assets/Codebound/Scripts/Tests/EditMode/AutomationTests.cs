using System;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class AutomationTests
    {
        static void Run(GameState g, float s) { for (float t = 0; t < s; t += 0.05f) g.Factory.Tick(0.05f); }

        static (GameState g, Cell c) Area()
        {
            var g = new GameState(9);
            var s = g.World.Generator.FindSpawn();
            var wall = BuildingDatabase.Get("wall");
            for (int r = 3; r < 120; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    bool ok = true;
                    for (int dx = -4; dx <= 4 && ok; dx++) for (int dy = -4; dy <= 4 && ok; dy++) ok = g.CanPlace(wall, new Cell(c.X + dx, c.Y + dy), Rotation.R0);
                    if (ok) return (g, c);
                }
            Assert.Fail("no room"); return default;
        }

        static void Power(GameState g, Cell at)
        {
            var gen = g.Grid.Place(BuildingDatabase.Get("generator"), at, Rotation.R0);
            ((GeneratorState)gen.State).Fuel.Add(ItemDatabase.Coal, 20);
        }

        [Test]
        public void StorageHub_SpreadsIntoNearbyChests_AndFeedsInserters_OnlyWithPower()
        {
            var (g, c) = Area();
            var hub = g.Grid.Place(BuildingDatabase.Get("storage_hub"), c, Rotation.R0);
            var hs = (StorageHubState)hub.State;
            var a = (StorageState)g.Grid.Place(BuildingDatabase.Get("chest"), new Cell(c.X + 3, c.Y), Rotation.R0).State;
            var b = (StorageState)g.Grid.Place(BuildingDatabase.Get("chest"), new Cell(c.X - 2, c.Y), Rotation.R0).State;
            b.Items.Add(ItemDatabase.Coal, 1);
            Run(g, 0.1f);
            Assert.IsFalse(hs.TryInsert(ItemDatabase.Coal, 1), "no power, no automation");

            Power(g, new Cell(c.X, c.Y + 2));
            Run(g, 0.5f);
            Assert.AreEqual(2, hs.Chests.Count);
            Assert.IsTrue(hs.TryInsert(ItemDatabase.Coal, 5));
            Assert.AreEqual(6, b.Items.Count(ItemDatabase.Coal), "same item goes to the chest that already has it");
            Assert.IsTrue(hs.TryInsert(ItemDatabase.Stone, 3));
            Assert.AreEqual(3, a.Items.Count(ItemDatabase.Stone) + b.Items.Count(ItemDatabase.Stone));
            Assert.AreEqual(6, hs.Totals()[ItemDatabase.Coal]);
            Assert.IsNotNull(hs.Extractable, "inserters can take from the hub");
        }

        [Test]
        public void Server_StoresKeysWithLimits_NeedsPower_AndIsSaved()
        {
            var (g, c) = Area();
            var srv = g.Grid.Place(BuildingDatabase.Get("server"), c, Rotation.R0);
            var st = (ServerState)srv.State;
            Run(g, 0.1f);
            Assert.AreEqual("no power", st.Set("a", "1"));
            Power(g, new Cell(c.X + 1, c.Y));
            Run(g, 0.2f);
            Assert.IsNull(st.Set("ore", "42"));
            Assert.IsNotNull(st.Set("", "x"));
            Assert.IsNotNull(st.Set("big", new string('x', ServerState.MaxValueLength + 1)));
            Assert.IsNull(st.Set("ünïcode key", "valor: ção"));
            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            var l = (ServerState)SaveSystem.Read(new System.IO.StringReader(sw.ToString())).Grid.At(c).State;
            Assert.AreEqual("42", l.Data["ore"]);
            Assert.AreEqual("valor: ção", l.Data["ünïcode key"]);
            Assert.IsNull(st.Set("ore", null));
            Assert.IsFalse(st.Data.ContainsKey("ore"));
        }

        [Test]
        public void Walls_BetweenTwoPoints_AreCounted()
        {
            var (g, c) = Area();
            g.Grid.Place(BuildingDatabase.Get("wall"), new Cell(c.X + 1, c.Y), Rotation.R0);
            g.Grid.Place(BuildingDatabase.Get("wall"), new Cell(c.X + 2, c.Y), Rotation.R0);
            Assert.AreEqual(2, g.Grid.StructuresBetween(c.X + 0.5, c.Y + 0.5, c.X + 3.5, c.Y + 0.5));
            Assert.AreEqual(0, g.Grid.StructuresBetween(c.X + 0.5, c.Y + 1.5, c.X + 3.5, c.Y + 1.5));
        }

        [Test]
        public void ColourFilter_MakesRedAndGreenDistinguishable()
        {
            CollectionAssert.AreEqual(new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 }, ColorVision.Correction(0));
            float Dist((float r, float g, float b) x, (float r, float g, float b) y) => (float)Math.Sqrt(Math.Pow(x.r - y.r, 2) + Math.Pow(x.g - y.g, 2) + Math.Pow(x.b - y.b, 2));
            // the pair each deficiency confuses: red/green for 1-2, blue/green-ish teal for 3
            var pairs = new[] { default, ((0.8f, 0.2f, 0.2f), (0.2f, 0.8f, 0.2f)), ((0.8f, 0.2f, 0.2f), (0.2f, 0.8f, 0.2f)), ((0.2f, 0.4f, 0.9f), (0.2f, 0.7f, 0.5f)) };
            for (int mode = 1; mode <= 3; mode++)
            {
                var m = ColorVision.Correction(mode);
                var (x, y) = pairs[mode];
                float before = Dist(ColorVision.SimulateDeficiency(mode, x.Item1, x.Item2, x.Item3), ColorVision.SimulateDeficiency(mode, y.Item1, y.Item2, y.Item3));
                var cx = ColorVision.Apply(m, x.Item1, x.Item2, x.Item3); var cy = ColorVision.Apply(m, y.Item1, y.Item2, y.Item3);
                float after = Dist(ColorVision.SimulateDeficiency(mode, cx.r, cx.g, cx.b), ColorVision.SimulateDeficiency(mode, cy.r, cy.g, cy.b));
                Assert.Greater(after, before * 1.3f, ColorVision.Modes[mode]);
            }
        }
    }
}
