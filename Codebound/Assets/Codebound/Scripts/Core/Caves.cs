using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// The underground. Every world has one cave layer below it, reached through cave mouths on the surface
    /// (one per 64×64 region at most, on dry empty land). Underground the same cell is a ladder back up.
    /// Caves are dark (12 °C, no weather), tunnels between rock walls, with rich ore, crystals and uranium.
    /// Each layer is its own saved world, like a planet: buildings stay where they were built.
    /// </summary>
    public static class Caves
    {
        public const int Region = 64;
        public const float CaveC = 12f;

        static int Div(int a) => (int)Math.Floor(a / (double)Region);

        /// <summary>Where the portal of a region would be (same on the surface and underground), or null.</summary>
        public static Cell? Candidate(int seed, int rx, int ry)
        {
            if (WorldGenerator.Hash01(rx, ry, seed + 6060) > 0.35) return null;
            return new Cell(rx * Region + 10 + (int)(WorldGenerator.Hash01(rx, ry, seed + 6061) * 44),
                            ry * Region + 10 + (int)(WorldGenerator.Hash01(rx, ry, seed + 6062) * 44));
        }

        /// <summary>True for a cave mouth (surface) or a ladder up (underground) at c.</summary>
        public static bool IsPortal(WorldGenerator g, Cell c)
        {
            var cand = Candidate(g.Seed, Div(c.X), Div(c.Y));
            if (cand == null || !cand.Value.Equals(c)) return false;
            if (g.Kind == PlanetKind.Underground) return true;
            var t = g.TileAt(c);
            return WorldRules.IsWalkable(t) && t != TileType.Lava && g.DepositAt(c).IsNone;
        }

        public static IEnumerable<Cell> Near(WorldGenerator g, Cell c, int radius)
        {
            for (int rx = Div(c.X - radius); rx <= Div(c.X + radius); rx++)
                for (int ry = Div(c.Y - radius); ry <= Div(c.Y + radius); ry++)
                    if (Candidate(g.Seed, rx, ry) is Cell p && IsPortal(g, p)) yield return p;
        }

        /// <summary>Distance (in cells) within which an underground cell is kept open around a portal.</summary>
        public static bool NearPortal(int seed, Cell c, int r)
        {
            var cand = Candidate(seed, Div(c.X), Div(c.Y));
            return cand != null && Math.Abs(cand.Value.X - c.X) <= r && Math.Abs(cand.Value.Y - c.Y) <= r;
        }

        public static string UndergroundId(string planetId) => planetId + "-under";
        public static bool IsUnderground(GameState g) => g.PlanetKind == PlanetKind.Underground;
        public static string SurfaceId(string undergroundId) => undergroundId.EndsWith("-under") ? undergroundId.Substring(0, undergroundId.Length - 6) : undergroundId;

        /// <summary>Goes through the portal at c: from the surface down, or from the caves up. `saved` is the other layer
        /// if it was visited before (otherwise it is generated). The player's things come along.</summary>
        public static GameState Pass(GameState from, Cell c, GameState saved)
        {
            bool down = !IsUnderground(from);
            var surfaceKind = down ? from.PlanetKind : (StarSystem.Get(from.HomeSeed, SurfaceId(from.PlanetId))?.Kind ?? PlanetKind.Home);
            var t = saved ?? new GameState(from.Seed, down ? PlanetKind.Underground : surfaceKind)
            {
                PlanetId = down ? UndergroundId(from.PlanetId) : SurfaceId(from.PlanetId),
                HomeSeed = from.HomeSeed,
            };
            Voyage.Carry(from, t);
            t.PlayerX = c.X + 0.5; t.PlayerY = c.Y - 0.5;   // step off the ladder
            return t;
        }
    }
}
