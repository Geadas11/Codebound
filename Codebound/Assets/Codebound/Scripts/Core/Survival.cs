using System;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>What the environment does to the player at a cell.</summary>
    public struct Surroundings
    {
        public float AmbientC;      // air temperature, °C
        public bool Breathable;     // can the suit refill oxygen from the air here
        public float Radiation;     // health damage per second from radiation
        public string Description;
        public Weather Weather;
        public bool Rain;           // outdoors in the rain: the suit collects a little water
    }

    /// <summary>
    /// Player survival stats. Designed to matter without being punishing: slow decay, regeneration when
    /// cared for, and death only returns you to the crash site (inventory kept).
    /// </summary>
    public sealed class Vitals
    {
        public const float Max = 100f, NormalTemp = 37f;
        public const float HungerPerSecond = 100f / 2400f;   // full → empty in 40 min
        public const float ThirstPerSecond = 100f / 1500f;   // 25 min
        public const float OxygenDrain = 0.5f, OxygenRefill = 8f; // suit tank: 200 s in unbreathable air
        public const float StarveDamage = 0.3f, SuffocateDamage = 5f, HeatColdDamage = 0.8f, Regen = 0.15f;
        public const float HotLimit = 39.5f, ColdLimit = 34f;   // a winter night without shelter is uncomfortable, not deadly
        public const float RainWater = 0.15f;

        public float Health = Max, Hunger = Max, Thirst = Max, Oxygen = Max, BodyTemp = NormalTemp;
        public bool Dead => Health <= 0;
        /// <summary>The most serious current threat, for the HUD ("" when fine).</summary>
        public string Warning = "";

        public void Tick(float dt, Surroundings env)
        {
            if (Dead) return;
            Hunger = Math.Max(0, Hunger - HungerPerSecond * dt);
            Thirst = Math.Max(0, Math.Min(Max, Thirst - ThirstPerSecond * dt + (env.Rain ? RainWater * dt : 0)));
            Oxygen = env.Breathable ? Math.Min(Max, Oxygen + OxygenRefill * dt) : Math.Max(0, Oxygen - OxygenDrain * dt);

            // the suit insulates: body temperature drifts slowly toward a damped version of the air temperature
            float target = NormalTemp + (env.AmbientC - 22f) * 0.08f;   // desert noon (~54 °C) ≈ the hot limit; lava is still dangerous
            BodyTemp += (target - BodyTemp) * Math.Min(1f, 0.02f * dt);

            float damage = 0; Warning = "";
            if (Oxygen <= 0) { damage += SuffocateDamage; Warning = "NO OXYGEN"; }
            if (BodyTemp > HotLimit) { damage += (BodyTemp - HotLimit) * HeatColdDamage; if (Warning == "") Warning = "OVERHEATING"; }
            if (BodyTemp < ColdLimit) { damage += (ColdLimit - BodyTemp) * HeatColdDamage; if (Warning == "") Warning = "FREEZING"; }
            if (Thirst <= 0) { damage += StarveDamage; if (Warning == "") Warning = "DEHYDRATED"; }
            if (Hunger <= 0) { damage += StarveDamage; if (Warning == "") Warning = "STARVING"; }
            if (env.Radiation > 0) { damage += env.Radiation; if (Warning == "") Warning = "RADIATION"; }
            if (Warning == "" && !env.Breathable && Oxygen < 40) Warning = "OXYGEN LOW";

            bool healthy = Hunger > 40 && Thirst > 40 && Oxygen > 0 && BodyTemp <= HotLimit && BodyTemp >= ColdLimit && env.Radiation <= 0;
            Health = Math.Max(0, Math.Min(Max, Health - damage * dt + (healthy ? Regen * dt : 0)));
        }

        /// <summary>Eats one unit of food. Returns false if the item is not food.</summary>
        public bool Eat(string itemId)
        {
            if (!Foods.TryGetValue(itemId, out var f)) return false;
            Hunger = Math.Min(Max, Hunger + f.hunger);
            Thirst = Math.Min(Max, Thirst + f.thirst);
            Health = Math.Min(Max, Health + f.health);
            return true;
        }

        /// <summary>One sip of water. Toxic water quenches thirst but hurts.</summary>
        public void Drink(bool toxic)
        {
            Thirst = Math.Min(Max, Thirst + 6f);
            if (toxic) Health = Math.Max(0, Health - 3f);
        }

        public void Respawn()
        {
            Health = 60; Hunger = Math.Max(Hunger, 50); Thirst = Math.Max(Thirst, 50); Oxygen = Max; BodyTemp = NormalTemp; Warning = "";
        }

        static readonly System.Collections.Generic.Dictionary<string, (float hunger, float thirst, float health)> Foods =
            new System.Collections.Generic.Dictionary<string, (float, float, float)>
            {
                { ItemDatabase.AlienFruit, (18f, 4f, 2f) },
            };

        public static bool IsFood(string itemId) => Foods.ContainsKey(itemId);

        // ---------------------------------------------------------------- environment
        public static float BiomeTemperature(Biome b)
        {
            switch (b)
            {
                case Biome.Desert: return 38f;
                case Biome.Volcanic: return 46f;
                case Biome.Ruins: return 15f;
                case Biome.Radioactive: return 24f;
                default: return 19f;
            }
        }

        public static Surroundings At(WorldGenerator g, Cell c, WorldClock clock = null)
        {
            var biome = g.BiomeAt(c);
            var weather = clock?.WeatherAt(biome) ?? Weather.Clear;
            var env = new Surroundings
            {
                AmbientC = BiomeTemperature(biome) + (clock?.TemperatureOffset(biome, weather) ?? 0f),
                Breathable = biome != Biome.Volcanic && biome != Biome.Radioactive,   // ash and fallout: suit air only
                Radiation = biome == Biome.Radioactive ? 0.15f : 0f,
                Description = biome.ToString(),
                Weather = weather,
                Rain = weather == Weather.Rain || (weather == Weather.Storm && biome != Biome.Desert && biome != Biome.Volcanic && biome != Biome.Radioactive),
            };
            if (weather == Weather.Storm)
            {
                if (biome == Biome.Desert || biome == Biome.Volcanic) env.Breathable = false;   // sand / ash fills the air
                if (biome == Biome.Radioactive) env.Radiation *= 3f;                           // fallout
            }
            // standing next to lava is very hot
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++)
                    if (g.TileAt(new Cell(c.X + dx, c.Y + dy)) == TileType.Lava) { env.AmbientC = Math.Max(env.AmbientC, 80f); env.Description += " (lava!)"; return env; }
            return env;
        }

        // ---------------------------------------------------------------- save
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public string SaveLine() => string.Join(" ", "vitals", Health.ToString("R", Inv), Hunger.ToString("R", Inv), Thirst.ToString("R", Inv),
            Oxygen.ToString("R", Inv), BodyTemp.ToString("R", Inv));

        public void Load(string[] p)
        {
            if (p.Length < 6) throw new FormatException("vitals record needs 5 values");
            Health = float.Parse(p[1], Inv); Hunger = float.Parse(p[2], Inv); Thirst = float.Parse(p[3], Inv);
            Oxygen = float.Parse(p[4], Inv); BodyTemp = float.Parse(p[5], Inv);
        }
    }
}
