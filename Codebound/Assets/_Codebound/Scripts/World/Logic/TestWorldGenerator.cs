using System;
using Codebound.Core;
using Codebound.Items;

namespace Codebound.World
{
    /// <summary>
    /// Deterministic generator for the Etapa 1 test world: a crash site in the middle,
    /// salvage piles around it, nearby ore clusters, trees, rocks and lakes.
    /// Same seed => identical map.
    /// </summary>
    public static class TestWorldGenerator
    {
        public const int CrashSiteRadius = 6;

        public static WorldMap Generate(long seed, int width = 128, int height = 128)
        {
            var map = new WorldMap(width, height, seed);
            var center = new GridPos(width / 2, height / 2);
            map.SpawnPoint = center;

            // 1. Terrain from two noise fields (elevation-ish and moisture-ish).
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                double h = ValueNoise.Fractal(seed, x / 22.0, y / 22.0);
                double m = ValueNoise.Fractal(seed + 7919, x / 14.0, y / 14.0);
                TerrainType t;
                if (h < 0.30) t = TerrainType.Water;
                else if (h < 0.35) t = TerrainType.Sand;
                else if (h > 0.70) t = TerrainType.Rock;
                else if (m > 0.62) t = TerrainType.Dirt;
                else t = TerrainType.Grass;
                map.SetTerrain(new GridPos(x, y), t);
            }

            // 2. Clear the crash site so the player never spawns trapped.
            for (int dy = -CrashSiteRadius; dy <= CrashSiteRadius; dy++)
            for (int dx = -CrashSiteRadius; dx <= CrashSiteRadius; dx++)
            {
                var c = new GridPos(center.X + dx, center.Y + dy);
                if (!map.InBounds(c)) continue;
                int d2 = dx * dx + dy * dy;
                if (d2 <= CrashSiteRadius * CrashSiteRadius)
                    map.SetTerrain(c, d2 <= 9 ? TerrainType.CrashSite : TerrainType.Dirt);
            }

            var rng = new SeededRandom(seed ^ 0x5DEECE66DL);

            // 3. Salvage piles around the crash (first building material).
            for (int i = 0; i < 10; i++)
            {
                double a = i / 10.0 * Math.PI * 2 + rng.NextDouble() * 0.4;
                int r = 3 + rng.Range(0, 3);
                var c = new GridPos(center.X + (int)Math.Round(Math.Cos(a) * r), center.Y + (int)Math.Round(Math.Sin(a) * r));
                if (map.InBounds(c) && c != center) map.AddNode(new ResourceNode(c, ItemCatalog.Scrap, 6 + rng.Range(0, 5)));
            }

            // 4. Guaranteed starter ore clusters near the crash, then random ones further out.
            PlaceCluster(map, rng, Around(center, rng, 10, 16), ItemCatalog.IronOre, 7);
            PlaceCluster(map, rng, Around(center, rng, 10, 16), ItemCatalog.CopperOre, 6);
            PlaceCluster(map, rng, Around(center, rng, 10, 18), ItemCatalog.Stone, 7);
            PlaceCluster(map, rng, Around(center, rng, 12, 20), ItemCatalog.Coal, 6);
            string[] extra = { ItemCatalog.IronOre, ItemCatalog.CopperOre, ItemCatalog.Coal, ItemCatalog.Stone, ItemCatalog.Silicon };
            for (int i = 0; i < 14; i++)
                PlaceCluster(map, rng, Around(center, rng, 20, Math.Min(width, height) / 2 - 4), extra[i % extra.Length], 5 + rng.Range(0, 5));

            // 5. Trees on grass, kept off the crash site.
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var c = new GridPos(x, y);
                if (map.GetTerrain(c) != TerrainType.Grass || map.TryGetNode(c, out _)) continue;
                if (c.ManhattanDistance(center) <= CrashSiteRadius + 2) continue;
                if (SeededRandom.Hash01(seed + 31337, x, y) < 0.045) map.AddNode(new ResourceNode(c, ItemCatalog.Wood, 4));
            }

            return map;
        }

        private static GridPos Around(GridPos center, SeededRandom rng, int minR, int maxR)
        {
            double a = rng.NextDouble() * Math.PI * 2;
            double r = minR + rng.NextDouble() * Math.Max(1, maxR - minR);
            return new GridPos(center.X + (int)Math.Round(Math.Cos(a) * r), center.Y + (int)Math.Round(Math.Sin(a) * r));
        }

        /// <summary>Random-walk cluster of resource nodes on walkable, non-crash terrain.</summary>
        private static void PlaceCluster(WorldMap map, SeededRandom rng, GridPos start, string itemId, int size)
        {
            var c = start;
            int placed = 0;
            for (int attempt = 0; attempt < size * 6 && placed < size; attempt++)
            {
                var t = map.GetTerrain(c);
                if (map.InBounds(c) && t != TerrainType.Water && t != TerrainType.CrashSite && !map.TryGetNode(c, out _))
                {
                    if (t == TerrainType.Rock) map.SetTerrain(c, TerrainType.Dirt);
                    map.AddNode(new ResourceNode(c, itemId, 20 + rng.Range(0, 20)));
                    placed++;
                }
                c += ((Direction)rng.Range(0, 4)).ToOffset();
            }
        }
    }
}
