using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    public enum Biome : byte { Forest, Desert, Volcanic, Ruins, Radioactive }

    public enum TileType : byte { Grass, Sand, Rock, Water, Ash, Lava, RuinFloor, ToxicSoil, ToxicWater }

    public enum ResourceType : byte { None, Tree, Stone, Iron, Copper, Coal, Silicon, Titanium, Uranium, Oil, AlienCrystal }

    public static class WorldRules
    {
        public const int ChunkSize = 32;

        public static bool IsLiquid(TileType t) => t == TileType.Water || t == TileType.Lava || t == TileType.ToxicWater;
        public static bool IsWalkable(TileType t) => !IsLiquid(t);

        /// <summary>Drills take solid ores only: trees are cut by hand, oil will need pumps.</summary>
        public static bool DrillCanMine(Deposit d) => !d.IsNone && d.Type != ResourceType.Tree && d.Type != ResourceType.Oil;

        public static string ItemFor(ResourceType r)
        {
            switch (r)
            {
                case ResourceType.Tree: return ItemDatabase.Wood;
                case ResourceType.Stone: return ItemDatabase.Stone;
                case ResourceType.Iron: return ItemDatabase.IronOre;
                case ResourceType.Copper: return ItemDatabase.CopperOre;
                case ResourceType.Coal: return ItemDatabase.Coal;
                case ResourceType.Silicon: return ItemDatabase.Silicon;
                case ResourceType.Titanium: return ItemDatabase.TitaniumOre;
                case ResourceType.Uranium: return ItemDatabase.UraniumOre;
                case ResourceType.Oil: return ItemDatabase.Oil;
                case ResourceType.AlienCrystal: return ItemDatabase.AlienCrystal;
                default: return null;
            }
        }

        /// <summary>Seconds of manual mining per unit. Harder materials take longer.</summary>
        public static float ManualMineTime(ResourceType r)
        {
            switch (r)
            {
                case ResourceType.Tree: case ResourceType.Stone: return 0.5f;
                case ResourceType.Titanium: case ResourceType.Uranium: case ResourceType.AlienCrystal: return 1.5f;
                case ResourceType.Oil: return 2f; // manual oil collection is slow on purpose: pumps come later
                default: return 0.8f;
            }
        }
    }

    public readonly struct Deposit
    {
        public readonly ResourceType Type;
        public readonly int Amount;
        public Deposit(ResourceType t, int a) { Type = t; Amount = a; }
        public bool IsNone => Type == ResourceType.None || Amount <= 0;
    }

    /// <summary>Deterministic procedural world: same seed ⇒ same world. Pure function of (seed, cell).</summary>
    public sealed class WorldGenerator
    {
        public readonly int Seed;
        public WorldGenerator(int seed) { Seed = seed; }

        // ---------- noise ----------
        static uint Hash(int x, int y, int s)
        {
            unchecked
            {
                uint h = (uint)s * 0x9E3779B1u;
                h ^= (uint)x * 0x85EBCA6Bu; h = (h << 13) | (h >> 19);
                h ^= (uint)y * 0xC2B2AE35u; h = (h << 17) | (h >> 15);
                h *= 0x27D4EB2Fu; h ^= h >> 15; h *= 0x165667B1u; h ^= h >> 13;
                return h;
            }
        }

        public static double Hash01(int x, int y, int s) => (Hash(x, y, s) & 0xFFFFFF) / (double)0x1000000;

        static double Smooth(double t) => t * t * (3 - 2 * t);

        static double ValueNoise(double x, double y, int s)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            double fx = Smooth(x - x0), fy = Smooth(y - y0);
            double a = Hash01(x0, y0, s), b = Hash01(x0 + 1, y0, s);
            double c = Hash01(x0, y0 + 1, s), d = Hash01(x0 + 1, y0 + 1, s);
            return (a + (b - a) * fx) * (1 - fy) + (c + (d - c) * fx) * fy;
        }

        /// <summary>Fractal noise in [0,1).</summary>
        static double Fbm(double x, double y, int s, int octaves = 4)
        {
            double sum = 0, amp = 1, norm = 0, f = 1;
            for (int i = 0; i < octaves; i++)
            {
                sum += ValueNoise(x * f, y * f, s + i * 1013) * amp;
                norm += amp; amp *= 0.5; f *= 2;
            }
            return sum / norm;
        }

        // ---------- world queries ----------
        public Biome BiomeAt(Cell c)
        {
            double temp = Fbm(c.X / 260.0, c.Y / 260.0, Seed + 11);
            double moist = Fbm(c.X / 220.0, c.Y / 220.0, Seed + 23);
            double weird = Fbm(c.X / 420.0, c.Y / 420.0, Seed + 37, 3);
            if (weird > 0.66) return temp < 0.5 ? Biome.Ruins : Biome.Radioactive;
            if (temp > 0.62) return moist < 0.48 ? Biome.Volcanic : Biome.Desert;
            if (temp > 0.54 && moist < 0.45) return Biome.Desert;
            return Biome.Forest;
        }

        public TileType TileAt(Cell c)
        {
            double liquid = Fbm(c.X / 36.0, c.Y / 36.0, Seed + 51, 3);
            double detail = Fbm(c.X / 9.0, c.Y / 9.0, Seed + 67, 2);
            switch (BiomeAt(c))
            {
                case Biome.Forest: return liquid < 0.28 ? TileType.Water : TileType.Grass;
                case Biome.Desert: return detail > 0.68 ? TileType.Rock : TileType.Sand;
                case Biome.Volcanic: return liquid < 0.26 ? TileType.Lava : (detail > 0.6 ? TileType.Rock : TileType.Ash);
                case Biome.Ruins: return detail > 0.5 ? TileType.RuinFloor : TileType.Grass;
                default: return liquid < 0.25 ? TileType.ToxicWater : TileType.ToxicSoil;
            }
        }

        static readonly ResourceType[] ForestRes = { ResourceType.Stone, ResourceType.Iron, ResourceType.Copper, ResourceType.Coal };
        static readonly ResourceType[] DesertRes = { ResourceType.Stone, ResourceType.Silicon, ResourceType.Copper, ResourceType.Oil, ResourceType.Titanium };
        static readonly ResourceType[] VolcanicRes = { ResourceType.Coal, ResourceType.Iron, ResourceType.Titanium, ResourceType.Stone };
        static readonly ResourceType[] RuinsRes = { ResourceType.Iron, ResourceType.Copper, ResourceType.Silicon, ResourceType.AlienCrystal };
        static readonly ResourceType[] RadioRes = { ResourceType.Uranium, ResourceType.AlienCrystal, ResourceType.Stone };

        public static ResourceType[] ResourcesOf(Biome b)
        {
            switch (b)
            {
                case Biome.Forest: return ForestRes;
                case Biome.Desert: return DesertRes;
                case Biome.Volcanic: return VolcanicRes;
                case Biome.Ruins: return RuinsRes;
                default: return RadioRes;
            }
        }

        static double Threshold(ResourceType r)
        {
            switch (r)
            {
                case ResourceType.AlienCrystal: case ResourceType.Uranium: case ResourceType.Titanium: return 0.78;
                case ResourceType.Oil: return 0.80;
                default: return 0.72;
            }
        }

        /// <summary>Generated (untouched) deposit at a cell.</summary>
        public Deposit DepositAt(Cell c)
        {
            if (WorldRules.IsLiquid(TileAt(c))) return default;
            var biome = BiomeAt(c);

            foreach (var r in ResourcesOf(biome))
            {
                double patch = Fbm(c.X / 14.0, c.Y / 14.0, Seed + 1000 + (int)r * 101, 3);
                double th = Threshold(r);
                if (patch > th)
                {
                    int amount = 20 + (int)((patch - th) / (1 - th) * 180) + (int)(Hash01(c.X, c.Y, Seed + (int)r) * 20);
                    return new Deposit(r, amount);
                }
            }

            if (biome == Biome.Forest || biome == Biome.Ruins)
            {
                double woods = Fbm(c.X / 18.0, c.Y / 18.0, Seed + 777, 2);
                if (woods > 0.5 && Hash01(c.X, c.Y, Seed + 778) < (biome == Biome.Forest ? 0.22 : 0.06))
                    return new Deposit(ResourceType.Tree, 6 + (int)(Hash01(c.X, c.Y, Seed + 779) * 6));
            }
            return default;
        }

        /// <summary>Nearest walkable cell to the origin, searched in growing squares.</summary>
        public Cell FindSpawn()
        {
            for (int r = 0; r < 512; r++)
                for (int x = -r; x <= r; x++)
                    for (int y = -r; y <= r; y++)
                    {
                        if (Math.Abs(x) != r && Math.Abs(y) != r) continue;
                        var c = new Cell(x, y);
                        if (BiomeAt(c) == Biome.Forest && WorldRules.IsWalkable(TileAt(c)) && DepositAt(c).IsNone) return c;
                    }
            return new Cell(0, 0);
        }
    }

    /// <summary>Player-made changes on top of the generated world (what gets saved).</summary>
    public sealed class WorldState
    {
        public readonly WorldGenerator Generator;
        readonly Dictionary<Cell, int> mined = new Dictionary<Cell, int>();
        public event Action<Cell> DepositChanged;

        public WorldState(WorldGenerator gen) { Generator = gen; }

        public Deposit DepositAt(Cell c)
        {
            var d = Generator.DepositAt(c);
            if (d.IsNone) return default;
            mined.TryGetValue(c, out int m);
            int left = d.Amount - m;
            return left > 0 ? new Deposit(d.Type, left) : default;
        }

        /// <summary>Takes up to <paramref name="amount"/> units. Returns units taken.</summary>
        public int Extract(Cell c, int amount)
        {
            var d = DepositAt(c);
            if (d.IsNone || amount <= 0) return 0;
            int take = Math.Min(amount, d.Amount);
            mined.TryGetValue(c, out int m);
            mined[c] = m + take;
            DepositChanged?.Invoke(c);
            return take;
        }

        public IReadOnlyDictionary<Cell, int> MinedDeltas => mined;

        /// <summary>Restores a saved delta (load only).</summary>
        public void SetMined(Cell c, int amount) { if (amount > 0) mined[c] = amount; else mined.Remove(c); }
    }
}
