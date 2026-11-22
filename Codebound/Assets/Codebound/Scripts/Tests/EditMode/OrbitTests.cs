using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 7 completion: landing back, asteroids, Meridian relays, the ending, and what travels with the ship.</summary>
    public class OrbitTests
    {
        static (GameState g, PlacedBuilding pad, LaunchPadState lp) InOrbit(float fuel = 200)
        {
            var g = new GameState(11);
            var s = g.World.Generator.FindSpawn();
            var def = BuildingDatabase.Get("launchpad");
            for (int r = 3; r < 80; r++)
                for (int x = -r; x <= r; x++)
                    if (g.CanPlace(def, new Cell(s.X + x, s.Y + r), Rotation.R0))
                    {
                        var pad = g.Grid.Place(def, new Cell(s.X + x, s.Y + r), Rotation.R0);
                        var lp = (LaunchPadState)pad.State;
                        lp.Phase = "orbit"; lp.Flight = new Ascent { Phase = "orbit", Fuel = fuel };
                        return (g, pad, lp);
                    }
            Assert.Fail("no room"); return default;
        }

        [Test]
        public void Land_ComesBackDown_KeepsTheRestOfTheFuel()
        {
            var (g, pad, lp) = InOrbit(30);
            Assert.IsNull(lp.Land());
            Assert.AreEqual("ready", lp.Phase); Assert.IsNull(lp.Flight);
            Assert.AreEqual(20, lp.Reserve, 1e-3);
            StringAssert.Contains("not in orbit", lp.Land());
            (g, pad, lp) = InOrbit(5);
            StringAssert.Contains("fuel", lp.Land());
        }

        [Test]
        public void Asteroids_AreDailyAndMinedOnce_IntoTheCargo()
        {
            var (g, pad, lp) = InOrbit(200);
            var list = lp.AsteroidList(g);
            Assert.AreEqual(5, list.Count);
            CollectionAssert.AreEqual(list.Select(o => ((Dictionary<string, object>)o)["ore"]), lp.AsteroidList(g).Select(o => ((Dictionary<string, object>)o)["ore"]), "same day, same rocks");
            var first = (Dictionary<string, object>)list[0];
            float fuelBefore = lp.Flight.Fuel;
            Assert.IsNull(lp.MineAsteroid(g, 1));
            Assert.AreEqual((double)first["amount"], lp.Cargo.Count((string)first["ore"]));
            Assert.AreEqual(fuelBefore - (double)first["fuel"], lp.Flight.Fuel, 1e-3);
            StringAssert.Contains("already mined", lp.MineAsteroid(g, 1));
            Assert.IsTrue((bool)((Dictionary<string, object>)lp.AsteroidList(g)[0])["mined"]);
            StringAssert.Contains("no such", lp.MineAsteroid(g, 99));
            g.Clock.Seconds += 24 * 3600;   // the next day brings new rocks
            Assert.IsNull(lp.MineAsteroid(g, 1));

            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var back = (LaunchPadState)SaveSystem.Read(new StringReader(sw.ToString())).Grid.At(pad.Origin).State;
            Assert.AreEqual(lp.Cargo.Count((string)first["ore"]), back.Cargo.Count((string)first["ore"]), "cargo is saved");
            Assert.IsTrue(back.Mined.Contains(1));
        }

        [Test]
        public void Relays_NeedARightAnswer_AndGiveTheFragmentOnce()
        {
            var (g, pad, lp) = InOrbit(200);
            StringAssert.Contains("dock", lp.Answer(g, 1.0));
            Assert.IsNull(lp.Dock(g, out var info));
            Assert.AreEqual("sum", info["check"]);
            Assert.AreEqual(195, lp.Flight.Fuel, 1e-3, "docking costs fuel once");
            Assert.IsNull(lp.Dock(g, out _)); Assert.AreEqual(195, lp.Flight.Fuel, 1e-3);
            StringAssert.Contains("rejected", lp.Answer(g, -1.0));
            double sum = ((List<object>)info["data"]).Sum(x => (double)x);
            Assert.IsNull(lp.Answer(g, sum));
            Assert.IsTrue(g.Story.Fragments.Contains("home"));
            Assert.Greater(lp.Cargo.Count(ItemDatabase.Circuit), 0, "salvage");
            int circuits = lp.Cargo.Count(ItemDatabase.Circuit);
            Assert.IsNull(lp.Answer(g, sum)); Assert.AreEqual(circuits, lp.Cargo.Count(ItemDatabase.Circuit), "salvage only the first time");
        }

        [Test]
        public void EveryRelayCheck_IsSolvableByAProgram()
        {
            foreach (var id in Relays.Order)
            {
                var (kind, data, answer) = Relays.Challenge(11, id);
                object mine;
                switch (kind)
                {
                    case "sum": mine = ((List<object>)data).Sum(x => (double)x); break;
                    case "count": mine = (double)((List<object>)data).Count(x => ((string)x).StartsWith("ERR")); break;
                    case "sort": mine = ((List<object>)data).Select(x => (double)x).OrderBy(x => x).Select(x => (object)x).ToList(); break;
                    default:
                    {
                        var d = (Dictionary<string, object>)data; int sh = (int)(double)d["shift"];
                        mine = new string(((string)d["text"]).Select(c => c >= 'A' && c <= 'Z' ? (char)('A' + (c - 'A' - sh + 26) % 26) : c).ToArray());
                        break;
                    }
                }
                Assert.IsTrue(Relays.Check(answer, mine), id + " " + kind);
                Assert.IsNotNull(Relays.Name(id)); Assert.IsNotNull(Relays.Log(id));
            }
            Assert.IsTrue(Relays.Check("ABC", " abc "), "text ignores case and spaces around");
            Assert.IsFalse(Relays.Check(new List<object> { 1.0, 2.0 }, new List<object> { 2.0, 1.0 }), "order matters");
        }

        [Test]
        public void Transmit_EndsTheStory_OnlyFromHomeWithAllFragments()
        {
            var (g, pad, lp) = InOrbit();
            int ended = 0; g.Story.EndingReached += () => ended++;
            StringAssert.Contains("fragments", lp.Transmit(g));
            g.Story.Fragments.UnionWith(Relays.Order);
            Assert.IsNull(lp.Transmit(g));
            Assert.IsTrue(g.Story.Ended); Assert.AreEqual(1, ended);
            lp.Transmit(g); Assert.AreEqual(1, ended, "only once");

            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var back = SaveSystem.Read(new StringReader(sw.ToString()));
            Assert.IsTrue(back.Story.Ended); Assert.AreEqual(4, back.Story.Fragments.Count);
        }

        [Test]
        public void Travel_CarriesStoryCargoAndGuests()
        {
            var (g, pad, lp) = InOrbit(200);
            g.Story.Fragments.Add("home");
            lp.Cargo.Add(ItemDatabase.TitaniumOre, 12);
            g.GuestInventory("ana").Add(ItemDatabase.Coal, 7);
            var body = new GuestBody(); body.Vitals.Health = 42; g.GuestBodies["ana"] = body;
            var volcanic = StarSystem.Get(11, "volcanic");
            var t = Voyage.Arrive(g, pad, volcanic, null);
            Assert.IsTrue(t.Story.Fragments.Contains("home"));
            var landed = (LaunchPadState)t.Grid.Buildings.Single(b => b.State is LaunchPadState).State;
            Assert.AreEqual(12, landed.Cargo.Count(ItemDatabase.TitaniumOre));
            Assert.AreEqual(7, t.GuestInventory("ana").Count(ItemDatabase.Coal));
            Assert.AreEqual(42f, t.GuestBodies["ana"].Vitals.Health);
            Assert.AreEqual(t.PlayerX, t.GuestBodies["ana"].X, "guests land with the ship");
            landed.Phase = "orbit"; landed.Flight = new Ascent { Phase = "orbit", Fuel = 50 };
            StringAssert.Contains("home world", landed.Transmit(t), "the antenna is at home");
        }
    }
}
