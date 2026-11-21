using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class EnergyTests
    {
        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds; t += 0.05f) g.Factory.Tick(0.05f); }

        /// <summary>A solar panel, a battery and a running computer side by side on free land.</summary>
        static (GameState g, PlacedBuilding solar, PlacedBuilding battery, PlacedBuilding pc) Setup()
        {
            var g = new GameState(42);
            var s = g.World.Generator.FindSpawn();
            for (int r = 1; r < 100; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    if (!g.CanPlace(BuildingDatabase.Get("solar"), c, Rotation.R0) || !g.CanPlace(BuildingDatabase.Get("battery"), new Cell(c.X + 2, c.Y), Rotation.R0)
                        || !g.CanPlace(BuildingDatabase.Get("computer"), new Cell(c.X + 3, c.Y), Rotation.R0)) continue;
                    var solar = g.Grid.Place(BuildingDatabase.Get("solar"), c, Rotation.R0);
                    var bat = g.Grid.Place(BuildingDatabase.Get("battery"), new Cell(c.X + 2, c.Y), Rotation.R0);
                    var pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(c.X + 3, c.Y), Rotation.R0);
                    ((ComputerState)pc.State).Running = true;   // draws 20 kW
                    return (g, solar, bat, pc);
                }
            Assert.Fail("no room"); return default;
        }

        [Test]
        public void Solar_FollowsSunlight_ChargesBattery_BatteryCarriesTheNight()
        {
            var (g, solar, battery, pc) = Setup();
            var bs = (BatteryState)battery.State;

            g.Factory.SolarFactor = 1f;                       // full sun: 40 kW for a 20 kW load
            Run(g, 10);
            Assert.AreEqual(1f, pc.State.PowerSatisfaction, 1e-3);
            Assert.AreEqual(Factory.SolarKW, ((SolarState)solar.State).OutputKW, 1e-3);
            Assert.AreEqual(200f, bs.StoredKJ, 2f, "spare 20 kW × 10 s stored");

            g.Factory.SolarFactor = 0f;                       // night
            Assert.IsTrue(g.Factory.HasPower(pc), "battery counts as a power source");
            Run(g, 5);
            Assert.AreEqual(1f, pc.State.PowerSatisfaction, 1e-3, "battery covers the load");
            Assert.AreEqual(100f, bs.StoredKJ, 2f, "20 kW × 5 s drained");
            Run(g, 10);
            Assert.AreEqual(0f, bs.StoredKJ, 1e-3);
            Assert.IsFalse(g.Factory.HasPower(pc), "empty battery, no sun, no coal");
        }

        [Test]
        public void WeatherDimsSolar_AndTotalsReport()
        {
            var c = new WorldClock(1) { Seconds = WorldClock.DaySeconds * 0.5 };   // noon
            Assert.AreEqual(1f, c.SolarFactor(Weather.Clear), 1e-3);
            Assert.Less(c.SolarFactor(Weather.Storm), c.SolarFactor(Weather.Rain));
            Assert.Less(c.SolarFactor(Weather.Rain), c.SolarFactor(Weather.Cloudy));
            c.Seconds = 0;                                                          // midnight
            Assert.AreEqual(0f, c.SolarFactor(Weather.Clear), 1e-3);

            var (g, _, battery, _) = Setup();
            g.Factory.SolarFactor = 0.5f;                     // 20 kW: exactly the load, nothing spare
            Run(g, 2);
            var (cap, used, kj) = g.Factory.Totals();
            Assert.AreEqual(20f, cap, 0.5f); Assert.AreEqual(20f, used, 0.5f); Assert.AreEqual(0f, kj, 0.5f);
            Assert.AreEqual(0f, ((BatteryState)battery.State).StoredKJ, 0.5f);
        }

        [Test]
        public void Energy_IsResearchLocked_AndBatteryChargeIsSaved()
        {
            var g = new GameState(1);
            Assert.IsFalse(g.Research.IsUnlocked(BuildingDatabase.Get("solar")));
            g.Research.Complete(Research.Get("energy"));
            Assert.IsTrue(g.Research.IsUnlocked(BuildingDatabase.Get("battery")));
            var (g2, _, battery, _) = Setup();
            ((BatteryState)battery.State).StoredKJ = 1234.5f;
            var sw = new System.IO.StringWriter(); SaveSystem.Write(g2, sw);
            var loaded = SaveSystem.Read(new System.IO.StringReader(sw.ToString()));
            Assert.AreEqual(1234.5f, ((BatteryState)loaded.Grid.At(battery.Origin).State).StoredKJ);
        }
    }
}
