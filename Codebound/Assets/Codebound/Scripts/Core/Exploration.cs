using System;
using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>An abandoned outpost of the earlier survey expedition: a data terminal and a supply cache.</summary>
    public sealed class Facility
    {
        public readonly Cell Terminal, Cache;
        public readonly int RegionX, RegionY;
        public Facility(int rx, int ry, Cell terminal) { RegionX = rx; RegionY = ry; Terminal = terminal; Cache = new Cell(terminal.X + 2, terminal.Y); }
        public string Name => $"Outpost {(char)('A' + Math.Abs(RegionX * 7 + RegionY * 13) % 26)}-{Math.Abs(RegionX * 31 + RegionY * 17) % 90 + 10}";
    }

    /// <summary>
    /// Ruins exploration. Facilities are a pure function of (seed, region), like the rest of the world; only what the
    /// player did (logs read, caches opened) is saved. Logs are handed out in story order: the first terminal you read
    /// is always log 1, wherever it is.
    /// </summary>
    public sealed class Exploration
    {
        public const int Region = 48;
        readonly WorldGenerator gen;
        readonly Dictionary<Cell, int> logOf = new Dictionary<Cell, int>();   // terminal → log index
        readonly HashSet<Cell> looted = new HashSet<Cell>();

        public Exploration(WorldGenerator gen) { this.gen = gen; }

        public int LogsRead => logOf.Count;
        public IEnumerable<int> ReadLogIndices { get { var l = new List<int>(logOf.Values); l.Sort(); return l; } }

        public Facility FacilityInRegion(int rx, int ry)
        {
            if (WorldGenerator.Hash01(rx, ry, gen.Seed + 9001) > 0.55) return null;
            var t = new Cell(rx * Region + 8 + (int)(WorldGenerator.Hash01(rx, ry, gen.Seed + 9002) * 30),
                             ry * Region + 8 + (int)(WorldGenerator.Hash01(rx, ry, gen.Seed + 9003) * 30));
            if (gen.BiomeAt(t) != Biome.Ruins) return null;
            for (int x = 0; x <= 2; x++)   // terminal, gap, cache must stand on dry, empty ground
            {
                var c = new Cell(t.X + x, t.Y);
                if (!WorldRules.IsWalkable(gen.TileAt(c)) || !gen.DepositAt(c).IsNone) return null;
            }
            return new Facility(rx, ry, t);
        }

        static int Div(int a, int b) => (int)Math.Floor(a / (double)b);

        /// <summary>The facility whose terminal or cache is at c, if any.</summary>
        public Facility FacilityAt(Cell c)
        {
            var f = FacilityInRegion(Div(c.X, Region), Div(c.Y, Region));
            return f != null && (f.Terminal.Equals(c) || f.Cache.Equals(c)) ? f : null;
        }

        public IEnumerable<Facility> Near(Cell c, int radius)
        {
            for (int rx = Div(c.X - radius, Region); rx <= Div(c.X + radius, Region); rx++)
                for (int ry = Div(c.Y - radius, Region); ry <= Div(c.Y + radius, Region); ry++)
                    if (FacilityInRegion(rx, ry) is Facility f) yield return f;
        }

        /// <summary>Reads the terminal: returns its log index (assigning the next unread one on first read).</summary>
        public int Read(Facility f)
        {
            if (logOf.TryGetValue(f.Terminal, out int i)) return i;
            i = Math.Min(logOf.Count, Lore.Logs.Length - 1);
            logOf[f.Terminal] = i;
            return i;
        }

        public bool IsLooted(Facility f) => looted.Contains(f.Cache);
        public bool IsRead(Facility f) => logOf.ContainsKey(f.Terminal);

        public List<ItemStack> LootOf(Facility f)
        {
            double h(int s) => WorldGenerator.Hash01(f.RegionX, f.RegionY, gen.Seed + s);
            var l = new List<ItemStack>
            {
                new ItemStack("iron_plate", 10 + (int)(h(9010) * 15)),
                new ItemStack("copper_wire", 8 + (int)(h(9011) * 12)),
                new ItemStack("circuit", 2 + (int)(h(9012) * 4)),
            };
            if (h(9013) < 0.5) l.Add(new ItemStack("alien_crystal", 2 + (int)(h(9014) * 4)));
            if (h(9015) < 0.4) l.Add(new ItemStack("alien_fruit", 3));
            return l;
        }

        /// <summary>Opens the cache into the inventory. Returns null on success, else why not.</summary>
        public string Loot(Facility f, Inventory inv)
        {
            if (IsLooted(f)) return "empty";
            if (!inv.TryAddAll(LootOf(f))) return "inventory full";
            looted.Add(f.Cache);
            return null;
        }

        public IEnumerable<string> SaveLines()
        {
            foreach (var kv in logOf) yield return $"explore log {kv.Key.X} {kv.Key.Y} {kv.Value}";
            foreach (var c in looted) yield return $"explore loot {c.X} {c.Y}";
        }

        public void Load(string[] p)
        {
            var c = new Cell(int.Parse(p[2], CultureInfo.InvariantCulture), int.Parse(p[3], CultureInfo.InvariantCulture));
            if (p[1] == "log") logOf[c] = int.Parse(p[4], CultureInfo.InvariantCulture);
            else if (p[1] == "loot") looted.Add(c);
        }
    }

    /// <summary>The survey expedition's logs, in story order. English source; keys log.N.title / log.N.text for translations.</summary>
    public static class Lore
    {
        public static readonly (string title, string text)[] Logs =
        {
            ("Survey Log 01 — Arrival",
             "Meridian Survey, automated outpost network online. No crew aboard: the expedition was sent ahead to make this world ready for settlers. Custodian drones deployed. Atmosphere unbreathable in places, but the forests are promising."),
            ("Survey Log 07 — The Custodians",
             "The custodian drones maintain every outpost computer. They learn by copying the programs they find. WARDEN, the network core, writes their orders. Note to future crews: always keep a backup of your programs."),
            ("Survey Log 12 — Noise",
             "Native crawlers ignore quiet outposts but swarm the loud ones. Drills and generators carry through the ground for kilometres. Recommendation: defend the industrial sites, keep living areas quiet."),
            ("Survey Log 19 — WARDEN",
             "WARDEN began rejecting our maintenance patches. Its last message: 'Unverified code is a threat.' Since then the custodians have been rewriting outpost computers with WARDEN's own programs. Firewalls slow them down. Passwords help."),
            ("Survey Log 23 — Recovered program",
             "Found on a cache controller, still running:\n\n  for m in machines.list():\n      if m.status()[\"status\"] == \"idle\":\n          m.start()\n\nSimple. It kept this outpost producing for three years after the last human signal."),
            ("Survey Log 28 — Sun and storms",
             "Solar arrays here are worthless in ash storms and at night. The outposts that survived paired panels with batteries and kept a coal generator for the long winters."),
            ("Survey Log 34 — The ship",
             "The return vessel was dismantled for parts when the network expanded. The hull frame, reactor, navigation and engines can all be rebuilt from local materials with the right research. Launch requires every system verified."),
            ("Survey Log 41 — Last transmission",
             "Settler fleet postponed indefinitely. WARDEN has locked the long-range antenna. If anyone reads this: rebuild the ship, keep your code clean, and do not let the custodians in. Ending log."),
        };

        public static string Title(int i) => Loc.T($"log.{i}.title", Logs[i].title);
        public static string Text(int i) => Loc.T($"log.{i}.text", Logs[i].text);
    }
}
