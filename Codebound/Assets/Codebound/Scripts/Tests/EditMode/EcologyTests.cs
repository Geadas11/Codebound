using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class EcologyTests
    {
        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds; t += 0.05f) { g.Factory.Tick(0.05f); g.Ecology.Tick(0.05f); } }

        /// <summary>A coal generator under load (a running computer) on free land.</summary>
        static (GameState g, PlacedBuilding gen, PlacedBuilding pc) Setup()
        {
            var g = new GameState(42);
            var s = g.World.Generator.FindSpawn();
            for (int r = 1; r < 100; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    if (!Ecology.RegionOf(c).Equals(Ecology.RegionOf(new Cell(c.X, c.Y + 2)))
                        || !g.CanPlace(BuildingDatabase.Get("generator"), c, Rotation.R0)
                        || !g.CanPlace(BuildingDatabase.Get("computer"), new Cell(c.X + 3, c.Y), Rotation.R0)
                        || !g.CanPlace(BuildingDatabase.Get("scrubber"), new Cell(c.X, c.Y + 2), Rotation.R0)) continue;
                    var gen = g.Grid.Place(BuildingDatabase.Get("generator"), c, Rotation.R0);
                    ((GeneratorState)gen.State).Fuel.Add(ItemDatabase.Coal, 50);
                    var pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(c.X + 3, c.Y), Rotation.R0);
                    ((ComputerState)pc.State).Running = true;
                    return (g, gen, pc);
                }
            Assert.Fail("no room"); return default;
        }

        [Test]
        public void Generator_Pollutes_Spreads_AndLandRecoversWhenItStops()
        {
            var (g, gen, pc) = Setup();
            Run(g, 120);
            float here = g.Ecology.At(gen.Origin);
            Assert.Greater(here, 0.5f, "burning coal pollutes (20 kW load ≈ 0.2 units/s)");
            Assert.Greater(g.Ecology.PollutedRegions, 1, "pollution drifts to neighbouring regions");
            Assert.Greater(g.Ecology.Total, here);

            ((ComputerState)pc.State).Running = false;           // no load → the generator idles, no emission
            float peak = g.Ecology.Total;
            Run(g, 300);
            Assert.Less(g.Ecology.Total, peak * 0.5f, "the land recovers once emissions stop");
        }

        [Test]
        public void Scrubber_CleansWithPower_SmogBlocksAir_SlowsGreenhouses_AndIsSaved()
        {
            var (g, gen, pc) = Setup();
            Run(g, 60);
            float before = g.Ecology.At(gen.Origin);
            var sc = g.Grid.Place(BuildingDatabase.Get("scrubber"), new Cell(gen.Origin.X, gen.Origin.Y + 2), Rotation.R0);
            Assert.AreEqual(Ecology.RegionOf(gen.Origin), Ecology.RegionOf(sc.Origin), "same region in this setup");
            Run(g, 60);
            Assert.Less(g.Ecology.At(gen.Origin), before, "scrubber (2/s) beats one generator (1/s)");
            Assert.Greater(((ScrubberState)sc.State).Cleaned, 10f);

            var c = new Cell(1000, 1000);
            var sw0 = new System.IO.StringWriter(); SaveSystem.Write(g, sw0);
            g.Ecology.Load(new[] { "eco", "62", "62", "200" });   // region of (1000,1000)
            Assert.IsTrue(g.Ecology.IsSmog(c));
            Assert.AreEqual(0.4f, g.Ecology.GrowthFactor(c), 1e-3);
            Assert.AreEqual(1f, g.Ecology.GrowthFactor(new Cell(-5000, -5000)), 1e-3);

            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            var l = SaveSystem.Read(new System.IO.StringReader(sw.ToString()));
            Assert.AreEqual(200f, l.Ecology.At(c), 1e-3);
            Assert.AreEqual(g.Ecology.At(gen.Origin), l.Ecology.At(gen.Origin), 1e-3);
        }

        [Test]
        public void Scrubber_IsResearchLocked_BehindAgriculture()
        {
            var g = new GameState(1);
            Assert.IsFalse(g.Research.IsUnlocked(BuildingDatabase.Get("scrubber")));
            Assert.AreEqual("ecology", Research.UnlockerOf("scrubber").Id);
            CollectionAssert.Contains(Research.Get("ecology").Prereqs, "agriculture");
        }
    }
}
