using System;
using System.Globalization;

namespace Codebound.Core
{
    public enum Season : byte { Spring, Summer, Autumn, Winter }
    public enum Weather : byte { Clear, Cloudy, Rain, Storm }

    /// <summary>
    /// World time, day/night, seasons and regional weather. Weather is a pure function of (seed, time
    /// segment, biome), so every player in a world sees the same sky (Etapa 6) and it needs no saving.
    /// </summary>
    public sealed class WorldClock
    {
        public const float DaySeconds = 600f;          // one in-game day = 10 real minutes
        public const int DaysPerSeason = 5;
        public const float WeatherSegmentSeconds = 120f;
        public const double StartSeconds = DaySeconds * 0.30; // the crash happens at 07:12

        readonly int seed;
        public double Seconds = StartSeconds;          // since the world began

        public WorldClock(int seed) { this.seed = seed; }

        public int Day => (int)(Seconds / DaySeconds) + 1;
        /// <summary>0 = midnight, 0.5 = noon.</summary>
        public float TimeOfDay => (float)(Seconds / DaySeconds % 1.0);
        public Season Season => (Season)((Day - 1) / DaysPerSeason % 4);
        /// <summary>-1 at midnight, +1 at noon.</summary>
        public float Sun => -(float)Math.Cos(2 * Math.PI * TimeOfDay);
        /// <summary>0 (night) .. 1 (full day), with dawn and dusk transitions.</summary>
        public float Daylight => Math.Max(0f, Math.Min(1f, 0.5f + Sun * 0.9f));
        public bool IsNight => Daylight < 0.25f;

        public string ClockText
        {
            get { int minutes = (int)(TimeOfDay * 24 * 60); return $"{minutes / 60:00}:{minutes % 60:00}"; }
        }

        public Weather WeatherAt(Biome b)
        {
            if (b == Biome.Cavern) return Weather.Clear;   // no sky underground
            long segment = (long)(Seconds / WeatherSegmentSeconds);
            double r = WorldGenerator.Hash01((int)segment, (int)b * 7919, seed + 4242);
            double storm = Season == Season.Winter ? 0.16 : 0.08;
            double rain = b == Biome.Desert || b == Biome.Volcanic ? 0.04 : Season == Season.Autumn || Season == Season.Spring ? 0.30 : 0.18;
            if (r < storm) return Weather.Storm;
            if (r < storm + rain) return Weather.Rain;
            if (r < storm + rain + 0.25) return Weather.Cloudy;
            return Weather.Clear;
        }

        /// <summary>Player-facing name: storms depend on the biome.</summary>
        public static string WeatherName(Weather w, Biome b)
        {
            if (w != Weather.Storm) return w.ToString();
            switch (b)
            {
                case Biome.Desert: return "Sandstorm";
                case Biome.Volcanic: return "Ash storm";
                case Biome.Radioactive: return "Fallout storm";
                default: return "Thunderstorm";
            }
        }

        /// <summary>Air temperature change from time of day, season and weather (°C).</summary>
        public float TemperatureOffset(Biome b, Weather w)
        {
            if (b == Biome.Cavern) return 0f;
            float swing = b == Biome.Desert ? 16f : b == Biome.Volcanic ? 4f : 7f;   // deserts: hot days, cold nights
            float season = Season == Season.Summer ? 5f : Season == Season.Winter ? -12f : Season == Season.Autumn ? -3f : 0f;
            float weather = w == Weather.Rain ? -4f : w == Weather.Storm ? (b == Biome.Desert ? 3f : -7f) : w == Weather.Cloudy ? -1f : 0f;
            return swing * Sun + season + weather;
        }

        /// <summary>Sunlight on solar panels 0..1: daylight dimmed by clouds, rain and storms.</summary>
        public float SolarFactor(Weather w) => Daylight * (w == Weather.Storm ? 0.15f : w == Weather.Rain ? 0.4f : w == Weather.Cloudy ? 0.7f : 1f);

        public string SaveLine() => "clock " + Seconds.ToString("R", CultureInfo.InvariantCulture);
        public void Load(string[] p) => Seconds = double.Parse(p[1], CultureInfo.InvariantCulture);
    }
}
