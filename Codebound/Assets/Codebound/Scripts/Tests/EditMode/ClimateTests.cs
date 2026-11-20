using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class ClimateTests
    {
        [Test]
        public void Clock_DayNightSeasons()
        {
            var c = new WorldClock(1);
            Assert.AreEqual(1, c.Day);
            Assert.AreEqual("07:12", c.ClockText);
            c.Seconds = WorldClock.DaySeconds * 0.5;              // noon
            Assert.AreEqual(1f, c.Daylight, 1e-3); Assert.IsFalse(c.IsNight);
            c.Seconds = WorldClock.DaySeconds * 1.0;              // midnight, day 2
            Assert.AreEqual(2, c.Day); Assert.AreEqual(0f, c.Daylight, 1e-3); Assert.IsTrue(c.IsNight);
            Assert.AreEqual(Season.Spring, c.Season);
            c.Seconds = WorldClock.DaySeconds * WorldClock.DaysPerSeason * 3 + 1;
            Assert.AreEqual(Season.Winter, c.Season);
            c.Seconds = WorldClock.DaySeconds * WorldClock.DaysPerSeason * 4 + 1;
            Assert.AreEqual(Season.Spring, c.Season, "seasons cycle");
        }

        [Test]
        public void Weather_IsDeterministic_Varied_AndBiomeAware()
        {
            var a = new WorldClock(77); var b = new WorldClock(77);
            var seen = new HashSet<Weather>();
            int desertRain = 0, forestRain = 0;
            for (int i = 0; i < 2000; i++)
            {
                a.Seconds = b.Seconds = i * WorldClock.WeatherSegmentSeconds;
                Assert.AreEqual(a.WeatherAt(Biome.Forest), b.WeatherAt(Biome.Forest));
                seen.Add(a.WeatherAt(Biome.Forest));
                if (a.WeatherAt(Biome.Desert) == Weather.Rain) desertRain++;
                if (a.WeatherAt(Biome.Forest) == Weather.Rain) forestRain++;
            }
            CollectionAssert.AreEquivalent(new[] { Weather.Clear, Weather.Cloudy, Weather.Rain, Weather.Storm }, seen);
            Assert.Less(desertRain * 3, forestRain, "it rarely rains in the desert");
            Assert.AreEqual("Sandstorm", WorldClock.WeatherName(Weather.Storm, Biome.Desert));
        }

        [Test]
        public void Temperature_DesertNightsAreCold_WinterIsColder()
        {
            var c = new WorldClock(3);
            c.Seconds = WorldClock.DaySeconds * 0.5;   // noon, spring
            float noon = c.TemperatureOffset(Biome.Desert, Weather.Clear);
            c.Seconds = WorldClock.DaySeconds * 1.0;   // midnight
            float midnight = c.TemperatureOffset(Biome.Desert, Weather.Clear);
            Assert.AreEqual(32f, noon - midnight, 0.1f);
            c.Seconds = WorldClock.DaySeconds * (WorldClock.DaysPerSeason * 3 + 0.5); // winter noon
            Assert.Less(c.TemperatureOffset(Biome.Forest, Weather.Clear), 0f);
        }

        [Test]
        public void Storms_ChangeSurvival_RainGivesWater()
        {
            var g = new WorldGenerator(42);
            var clock = new WorldClock(42);
            // find a desert cell and a time with a storm there
            Cell desert = default; bool found = false;
            for (int x = -3000; x < 3000 && !found; x += 31) for (int y = -3000; y < 3000 && !found; y += 31)
                    if (g.BiomeAt(new Cell(x, y)) == Biome.Desert && g.TileAt(new Cell(x, y)) == TileType.Sand) { desert = new Cell(x, y); found = true; }
            Assert.IsTrue(found);
            int i = 0;
            while (clock.WeatherAt(Biome.Desert) != Weather.Storm && i++ < 5000) clock.Seconds += WorldClock.WeatherSegmentSeconds;
            var env = Vitals.At(g, desert, clock);
            Assert.AreEqual(Weather.Storm, env.Weather);
            Assert.IsFalse(env.Breathable, "sandstorm: breathe from the suit");

            var v = new Vitals { Thirst = 50 };
            for (int t = 0; t < 100; t++) v.Tick(0.1f, new Surroundings { AmbientC = 20, Breathable = true, Rain = true });
            Assert.Greater(v.Thirst, 50f, "rain more than covers thirst");
        }

        [Test]
        public void Clock_IsSaved()
        {
            var g = new GameState(5);
            g.Clock.Seconds = 12345.5;
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            Assert.AreEqual(12345.5, SaveSystem.Read(new StringReader(sw.ToString())).Clock.Seconds);
        }
    }
}
