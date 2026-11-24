using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    public sealed class HeaterState : MachineState { public HeaterState() { Status = "idle"; } }
    public sealed class LightningRodState : MachineState { public int Strikes; public LightningRodState() { Status = "ready"; } }

    /// <summary>Walkable trap: a crawler stepping on it takes 30 damage and is stunned 3 s. 3 charges; copper wire recharges.</summary>
    public sealed class ShockTrapState : MachineState
    {
        public const int MaxCharges = 3;
        public const float Damage = 30f, StunSeconds = 3f;
        public int Charges = MaxCharges;
        public ShockTrapState() { Status = $"{MaxCharges} charges"; }
        public override bool TryInsert(string itemId, int count)
        {
            if (itemId != ItemDatabase.CopperWire || Charges >= MaxCharges) return false;
            Charges = MaxCharges; Status = $"{Charges} charges";
            return true;
        }
        public override bool AcceptsItem(string itemId) => itemId == ItemDatabase.CopperWire;
        public override void SaveExtra(IDictionary<string, string> d) => d["charges"] = Charges.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public override void LoadExtra(string k, string v) { if (k == "charges") Charges = int.Parse(v, System.Globalization.CultureInfo.InvariantCulture); }
    }

    /// <summary>Gear works while you carry it: armor plating halves creature damage, the thermal liner keeps the suit
    /// halfway to 22 °C whatever the weather.</summary>
    public static class Equipment
    {
        public static float DamageTaken(Inventory inv) => inv.Count(ItemDatabase.ArmorPlating) > 0 ? 0.5f : 1f;

        public static Surroundings Apply(Inventory inv, Surroundings env)
        {
            if (inv.Count(ItemDatabase.ThermalSuit) > 0) env.AmbientC = env.AmbientC + (22f - env.AmbientC) * 0.5f;
            return env;
        }
    }

    /// <summary>
    /// Shelter and heating. A powered heater keeps the air within 5 cells at 21 °C or more. Walls on all four sides
    /// (within 4 cells) make a shelter: storms — rain, sand, ash, fallout — stay outside.
    /// </summary>
    public static class Shelter
    {
        public const float HeaterRadius = 5f, HeaterC = 21f, HeaterKW = 25f;
        public const int WallReach = 4;

        public static bool Heated(GameState g, Cell c)
        {
            foreach (var b in g.Grid.Buildings)
                if (b.State is HeaterState h && h.PowerSatisfaction > 0 && Math.Abs(b.CenterX - (c.X + 0.5)) <= HeaterRadius && Math.Abs(b.CenterY - (c.Y + 0.5)) <= HeaterRadius
                    && Math.Sqrt(Math.Pow(b.CenterX - (c.X + 0.5), 2) + Math.Pow(b.CenterY - (c.Y + 0.5), 2)) <= HeaterRadius) return true;
            return false;
        }

        public static bool Enclosed(GameState g, Cell c)
        {
            foreach (var r in new[] { Rotation.R0, Rotation.R90, Rotation.R180, Rotation.R270 })
            {
                bool wall = false;
                for (int n = 1; n <= WallReach && !wall; n++) wall = g.Grid.At(c.Move(r, n))?.Def.Kind == BuildingKind.Structure;
                if (!wall) return false;
            }
            return true;
        }

        public static Surroundings Apply(GameState g, Cell c, Surroundings env)
        {
            if (Enclosed(g, c) && env.Weather == Weather.Storm || Enclosed(g, c) && env.Rain)
            {
                var biome = g.World.Generator.BiomeAt(c);
                env.Rain = false;
                env.Breathable = biome != Biome.Volcanic && biome != Biome.Radioactive;
                env.Radiation = biome == Biome.Radioactive ? 0.15f : 0f;
                env.Description += " (sheltered)";
            }
            if (Heated(g, c)) { env.AmbientC = Math.Max(env.AmbientC, HeaterC); env.Description += " (heated)"; }
            return env;
        }
    }

    /// <summary>Lightning in thunderstorms (stormy forest/ruins). Strikes land near the player every ~10-25 s.
    /// A lightning rod within 8 cells takes the strike safely and its energy charges the batteries on its network.</summary>
    public sealed class Lightning
    {
        public const float RodRadius = 8f, BuildingDamage = 60f, PlayerDamage = 20f, RodChargeKJ = 1500f, Range = 24f;
        readonly GameState game;
        readonly Random rng;
        float timer = 12f;

        public event Action<double, double, bool> Struck;   // x, y, hit a rod

        public Lightning(GameState g, int seed) { game = g; rng = new Random(seed); }

        public static bool IsThunderstorm(Weather w, Biome b) => w == Weather.Storm && b != Biome.Desert && b != Biome.Volcanic && b != Biome.Radioactive;

        /// <summary>Returns damage dealt to the player this tick.</summary>
        public float Tick(float dt, double px, double py)
        {
            var cell = new Cell((int)Math.Floor(px), (int)Math.Floor(py));
            var biome = game.World.Generator.BiomeAt(cell);
            if (!IsThunderstorm(game.Clock.WeatherAt(biome), biome)) return 0f;
            if ((timer -= dt) > 0) return 0f;
            timer = 10f + (float)rng.NextDouble() * 15f;
            double x = px + (rng.NextDouble() * 2 - 1) * Range, y = py + (rng.NextDouble() * 2 - 1) * Range;
            return Strike(x, y, px, py);
        }

        public float Strike(double x, double y, double px, double py)
        {
            PlacedBuilding rod = null; double best = RodRadius;
            foreach (var b in game.Grid.Buildings)
                if (b.State is LightningRodState)
                {
                    double d = Math.Sqrt(Math.Pow(b.CenterX - x, 2) + Math.Pow(b.CenterY - y, 2));
                    if (d <= best) { best = d; rod = b; }
                }
            if (rod != null)
            {
                ((LightningRodState)rod.State).Strikes++;
                game.Factory.StoreEnergy(rod, RodChargeKJ);
                Struck?.Invoke(rod.CenterX, rod.CenterY, true);
                return 0f;
            }
            var hit = game.Grid.At(new Cell((int)Math.Floor(x), (int)Math.Floor(y)));
            if (hit != null) game.Creatures.DamageBuilding(hit, BuildingDamage);
            Struck?.Invoke(x, y, false);
            return Math.Sqrt(Math.Pow(px - x, 2) + Math.Pow(py - y, 2)) < 1.5 ? PlayerDamage : 0f;
        }
    }
}
