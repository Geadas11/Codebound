using System;
using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class SpaceTests
    {
        static Ascent Fly(Func<Ascent, float> program, float fuel)
        {
            var a = new Ascent { Fuel = fuel };
            while (a.Phase == "ascent") { a.Throttle = program(a); a.Step(0.05f); }
            return a;
        }

        [Test]
        public void Ascent_NeedsAProgramThatManagesHeat()
        {
            var floored = Fly(a => 1f, 200);
            Assert.AreEqual("aborted", floored.Phase);
            StringAssert.Contains("heat", floored.Reason);

            var smart = Fly(a => (70 - a.Heat) / 30f, 200);   // throttle back as the hull heats up
            Assert.AreEqual("orbit", smart.Phase);
            Assert.GreaterOrEqual(smart.Altitude, Ascent.TargetAltitude);
            Assert.GreaterOrEqual(smart.Velocity, Ascent.TargetVelocity);
            Assert.Greater(smart.Fuel, 0);

            var starved = Fly(a => (70 - a.Heat) / 30f, 80);
            Assert.AreEqual("aborted", starved.Phase);
            StringAssert.Contains("fuel", starved.Reason);
        }

        static (GameState g, PlacedBuilding pad) WithPad()
        {
            var g = new GameState(11);
            var s = g.World.Generator.FindSpawn();
            var def = BuildingDatabase.Get("launchpad");
            for (int r = 3; r < 80; r++)
                for (int x = -r; x <= r; x++)
                    if (g.CanPlace(def, new Cell(s.X + x, s.Y + r), Rotation.R0))
                        return (g, g.Grid.Place(def, new Cell(s.X + x, s.Y + r), Rotation.R0));
            Assert.Fail("no room"); return default;
        }

        [Test]
        public void LaunchPad_RequiresTheRebuiltShipAndFuel_FlightIsTickedAndSaved()
        {
            var (g, pad) = WithPad();
            var lp = (LaunchPadState)pad.State;
            Assert.IsFalse(g.Research.IsUnlocked(BuildingDatabase.Get("launchpad")));
            StringAssert.Contains("not rebuilt", lp.Launch(g.Research));
            foreach (var t in Research.Tree.Where(t => t.MainLine)) g.Research.Complete(t);
            StringAssert.Contains("50 fuel", lp.Launch(g.Research));
            Assert.IsFalse(lp.TryInsert(ItemDatabase.Stone, 1), "only fuel goes in");
            Assert.IsTrue(lp.TryInsert(ItemDatabase.Coal, 50));
            Assert.AreEqual(200, lp.FuelUnits, 1e-3);
            Assert.IsNull(lp.Launch(g.Research));
            Assert.AreEqual(0, lp.FuelBay.Count(ItemDatabase.Coal), "fuel pumped into the tanks");

            string ended = null; g.Factory.FlightEnded += (b, r) => ended = r;
            lp.Flight.Throttle = 0.5f;
            for (int i = 0; i < 20 * 30; i++) g.Factory.Tick(0.05f);
            Assert.AreEqual("ascent", lp.Phase);
            Assert.Greater(lp.Flight.Altitude, 1f);

            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            var lpl = (LaunchPadState)SaveSystem.Read(new System.IO.StringReader(sw.ToString())).Grid.At(pad.Origin).State;
            Assert.AreEqual("ascent", lpl.Phase);
            Assert.AreEqual(lp.Flight.Altitude, lpl.Flight.Altitude, 1e-4);

            for (int i = 0; i < 20 * 600 && ended == null; i++) g.Factory.Tick(0.05f);
            Assert.AreEqual("orbit", ended, "half throttle gets there with 200 units");
            Assert.AreEqual("orbit", lp.Phase);
        }

        [Test]
        public void Travel_CarriesThePlayer_LeavesTheBaseBehind_AndPlanetsDiffer()
        {
            var sys = StarSystem.Of(11);
            Assert.AreEqual(4, sys.Count);
            Assert.AreEqual(sys.Select(p => p.Name), StarSystem.Of(11).Select(p => p.Name), "deterministic");
            var volcanic = sys.First(p => p.Kind == PlanetKind.Volcanic);

            var (g, pad) = WithPad();
            var lp = (LaunchPadState)pad.State;
            StringAssert.Contains("orbit", Voyage.Check(g, pad, volcanic));
            lp.Phase = "orbit"; lp.Flight = new Ascent { Phase = "orbit", Fuel = 10 };
            StringAssert.Contains("fuel", Voyage.Check(g, pad, volcanic));
            lp.Flight.Fuel = 120;
            Assert.IsNull(Voyage.Check(g, pad, volcanic));

            g.Inventory.Add(ItemDatabase.IronPlate, 33);
            g.Research.Complete(Research.Get("logistics"));
            var t = Voyage.Arrive(g, pad, volcanic, null);
            Assert.IsNull(g.Grid.At(pad.Origin), "the ship left the home world");
            Assert.AreEqual(volcanic.Id, t.PlanetId);
            Assert.AreEqual(PlanetKind.Volcanic, t.PlanetKind);
            Assert.AreEqual(33, t.Inventory.Count(ItemDatabase.IronPlate));
            Assert.IsTrue(t.Research.IsDone("logistics"));
            var landed = t.Grid.Buildings.Single(b => b.State is LaunchPadState);
            Assert.AreEqual(120 - Voyage.Cost(g, volcanic), ((LaunchPadState)landed.State).Reserve, 1e-3);
            Assert.AreEqual("ready", ((LaunchPadState)landed.State).Phase);

            var sw = new System.IO.StringWriter(); SaveSystem.Write(t, sw);
            var back = SaveSystem.Read(new System.IO.StringReader(sw.ToString()));
            Assert.AreEqual(volcanic.Id, back.PlanetId); Assert.AreEqual(PlanetKind.Volcanic, back.PlanetKind); Assert.AreEqual(11, back.HomeSeed);

            int hot(WorldGenerator w) { int n = 0; for (int x = -2000; x < 2000; x += 40) for (int y = -2000; y < 2000; y += 40) if (w.BiomeAt(new Cell(x, y)) == Biome.Volcanic) n++; return n; }
            Assert.Greater(hot(t.World.Generator), hot(g.World.Generator) * 2, "a volcanic world has far more volcanic land");
        }
    }
}
