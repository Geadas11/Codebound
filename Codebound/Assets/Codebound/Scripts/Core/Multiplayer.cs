using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Codebound.Core
{
    /// <summary>
    /// LAN co-op (Etapa 6), host-authoritative. The host simulates everything (machines, programs, creatures, pollution)
    /// and sends snapshots; guests send actions, which the host validates here: reach, materials, research, role.
    /// Guests keep their own inventory on the host. Sockets live in the Game layer (NetSession); this is pure logic.
    /// </summary>
    public static class Multiplayer
    {
        public const int Port = 47820, ProtocolVersion = 1;
        public const double ActionReach = 6;   // cells; a little over the local reach to absorb latency
        public const int MaxGuestLine = 256 * 1024, MaxHostLine = 32 * 1024 * 1024, MaxProgramChars = 64 * 1024;
        public const string Builder = "builder", Visitor = "visitor";

        static readonly Regex NameRx = new Regex("^[A-Za-z0-9_-]{1,16}$");
        public static bool ValidName(string n) => n != null && NameRx.IsMatch(n);

        // ---------------------------------------------------------------- snapshots (host → guest)
        /// <summary>The save text minus secrets: password-protected computers go out without their hash, code or database,
        /// and other guests' inventories are left out.</summary>
        public static string Snapshot(GameState g)
        {
            var w = new StringWriter(); SaveSystem.Write(g, w);
            var lines = w.ToString().Split('\n');
            var o = new System.Text.StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i].TrimEnd('\r');
                if (l.Length == 0 || l.StartsWith("ginv ")) continue;
                if (l.StartsWith("b "))
                {
                    int end = i + 1;
                    while (end < lines.Length && (lines[end].StartsWith("bx ") || lines[end].StartsWith("bi ") || lines[end].StartsWith("bh "))) end++;
                    bool locked = false;
                    for (int k = i + 1; k < end; k++) if (lines[k].StartsWith("bx pwd ")) locked = true;
                    o.Append(l).Append('\n');
                    for (int k = i + 1; k < end; k++)
                    {
                        string bl = lines[k].TrimEnd('\r');
                        if (locked && (bl.StartsWith("bx code ") || bl.StartsWith("bx sqldb "))) continue;
                        o.Append(locked && bl.StartsWith("bx pwd ") ? "bx pwd locked" : bl).Append('\n');
                    }
                    i = end - 1;
                    continue;
                }
                o.Append(l).Append('\n');
            }
            return o.ToString();
        }

        static string Key(PlacedBuilding b) => $"{b.Def.Id} {b.Origin.X} {b.Origin.Y} {(int)b.Rotation}";

        /// <summary>Brings a guest's world in line with the host's snapshot, in place (views follow the grid events).
        /// Personal things — inventory, vitals, HUD, position — are left alone.</summary>
        public static void Apply(GameState t, GameState s, PlacedBuilding editing = null)
        {
            foreach (var kv in new List<KeyValuePair<Cell, int>>(t.World.MinedDeltas)) t.World.SetMined(kv.Key, 0);
            foreach (var kv in s.World.MinedDeltas) t.World.SetMined(kv.Key, kv.Value);

            var want = new Dictionary<string, PlacedBuilding>();
            foreach (var b in s.Grid.Buildings) want[Key(b)] = b;
            var have = new Dictionary<string, PlacedBuilding>();
            foreach (var b in new List<PlacedBuilding>(t.Grid.Buildings))
                if (want.ContainsKey(Key(b))) have[Key(b)] = b; else t.Grid.Remove(b);
            foreach (var kv in want)
            {
                var sb = kv.Value;
                if (!have.TryGetValue(kv.Key, out var tb)) tb = t.Grid.Place(sb.Def, sb.Origin, sb.Rotation);
                if (tb == null) continue;   // cannot happen with the same seed and deltas; skip rather than break the session
                if (tb == editing && tb.State is ComputerState oc && sb.State is ComputerState nc) { nc.Code = oc.Code; nc.Language = oc.Language; }   // keep what the guest is typing
                tb.State = sb.State; tb.Health = sb.Health;
            }

            t.Clock.Seconds = s.Clock.Seconds;
            if (s.Research.SaveLine() is string rl) t.Research.Load(rl.Split(' '));
            foreach (var line in s.Exploration.SaveLines()) t.Exploration.Load(line.Split(' '));
            t.Ecology.CopyFrom(s.Ecology);
        }

        // ---------------------------------------------------------------- guest actions (validated on the host)
        static double Num(Dictionary<string, object> a, string k) => a.TryGetValue(k, out var v) && v is double d && !double.IsNaN(d) && Math.Abs(d) < 1e9 ? d : throw new FormatException("bad " + k);
        static string Str(Dictionary<string, object> a, string k) => a.TryGetValue(k, out var v) && v is string s ? s : throw new FormatException("bad " + k);
        static Cell CellOf(Dictionary<string, object> a) => new Cell((int)Num(a, "x"), (int)Num(a, "y"));

        /// <summary>Applies one guest action on the host. Returns null on success, otherwise a reason for the guest.</summary>
        public static string Act(GameState g, string role, Inventory inv, double px, double py, Dictionary<string, object> a)
        {
            try
            {
                string kind = Str(a, "a");
                if (role != Builder && kind != "attack") return "you are a visitor (ask the host to make you a builder)";
                Cell c = kind == "attack" || kind == "consume" ? default : CellOf(a);
                if (kind != "attack" && kind != "consume" && Math.Sqrt(Math.Pow(c.X + 0.5 - px, 2) + Math.Pow(c.Y + 0.5 - py, 2)) > ActionReach) return "too far";
                switch (kind)
                {
                    case "build":
                    {
                        var def = BuildingDatabase.TryGet(Str(a, "def"));
                        if (def == null) return "unknown building";
                        if (!g.Research.IsUnlocked(def)) return "needs research";
                        if (!g.CanPlace(def, c, (Rotation)((int)Num(a, "rot") & 3))) return "blocked";
                        return g.TryBuild(def, c, (Rotation)((int)Num(a, "rot") & 3), null, inv) == null ? "not enough materials" : null;
                    }
                    case "remove":
                    {
                        var b = g.Grid.At(c);
                        if (b == null) return "nothing there";
                        return g.TryDeconstruct(b, inv) ? null : "inventory full";
                    }
                    case "mine":
                    {
                        var dep = g.World.DepositAt(c);
                        if (dep.IsNone || g.Grid.At(c) != null) return "nothing to mine";
                        string item = WorldRules.ItemFor(dep.Type);
                        if (inv.Add(item, 1) > 0) return "inventory full";
                        g.World.Extract(c, 1);
                        return null;
                    }
                    case "take":
                    case "put":
                    {
                        var st = g.Grid.At(c)?.State;
                        string item = Str(a, "item");
                        int count = Math.Max(1, Math.Min(1000, (int)Num(a, "count")));
                        if (st == null || !ItemDatabase.Exists(item)) return "nothing there";
                        if (kind == "put")
                        {
                            int moved = 0;
                            while (moved < count && inv.Count(item) > 0 && st.TryInsert(item, 1)) { inv.TryRemove(item, 1); moved++; }
                            return moved > 0 ? null : "does not fit";
                        }
                        foreach (var (_, mi) in st.Inventories)
                        {
                            int n = Math.Min(count, mi.Count(item));
                            if (n == 0) continue;
                            int left = inv.Add(item, n);
                            mi.TryRemove(item, n - left);
                            return n - left > 0 ? null : "inventory full";
                        }
                        return "nothing to take";
                    }
                    case "repair": return g.Repair(g.Grid.At(c), inv);
                    case "explore":
                    {
                        var f = g.Exploration.FacilityAt(c);
                        if (f == null) return "nothing there";
                        if (c.Equals(f.Terminal)) { g.Exploration.Read(f); return null; }
                        return g.Exploration.Loot(f, inv);
                    }
                    case "consume": return inv.TryRemove(Str(a, "item"), 1) ? null : "you have none";
                    case "attack":
                    {
                        var foe = g.Creatures.ById((int)Num(a, "id"));
                        if (foe == null) return "gone";
                        if (Math.Sqrt(Math.Pow(foe.X - px, 2) + Math.Pow(foe.Y - py, 2)) > ActionReach) return "too far";
                        g.Creatures.Hurt(foe, (float)Math.Min(12, Num(a, "damage")));
                        return null;
                    }
                    default: return "unknown action";
                }
            }
            catch (FormatException e) { return "malformed action: " + e.Message; }
            catch (InvalidCastException) { return "malformed action"; }
        }

        // ---------------------------------------------------------------- creatures in snapshots
        public static List<object> CreaturesOf(GameState g)
        {
            var l = new List<object>();
            foreach (var c in g.Creatures.All)
                l.Add(new List<object> { (double)c.Id, (double)(int)c.Type, Math.Round(c.X, 2), Math.Round(c.Y, 2), (double)c.Health, (double)c.MaxHealth });
            return l;
        }

        public static List<Creature> ParseCreatures(List<object> l)
        {
            var r = new List<Creature>();
            if (l == null) return r;
            foreach (var o in l)
                if (o is List<object> e && e.Count >= 6)
                    r.Add(new Creature { Id = (int)(double)e[0], Type = (CreatureType)(int)(double)e[1], X = (double)e[2], Y = (double)e[3], Health = (float)(double)e[4], MaxHealth = (float)(double)e[5] });
            return r;
        }

        public static List<object> InventoryOf(Inventory inv)
        {
            var l = new List<object>();
            for (int i = 0; i < inv.SlotCount; i++) if (!inv[i].IsEmpty) l.Add(new List<object> { inv[i].ItemId, (double)inv[i].Count });
            return l;
        }

        public static void LoadInventory(Inventory inv, List<object> l)
        {
            inv.Clear();
            if (l == null) return;
            foreach (var o in l)
                if (o is List<object> e && e.Count == 2 && e[0] is string id && ItemDatabase.Exists(id) && e[1] is double n) inv.Add(id, (int)n);
        }
    }
}
