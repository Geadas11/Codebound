using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Codebound.Core
{
    /// <summary>The ending's progress: antenna key fragments recovered from the orbital relays. Travels between worlds.</summary>
    public sealed class Story
    {
        public readonly HashSet<string> Fragments = new HashSet<string>();   // planet ids whose relay challenge was solved
        public bool Ended;
        public event Action EndingReached;

        public void End() { if (Ended) return; Ended = true; EndingReached?.Invoke(); }

        public string SaveLine() => Fragments.Count == 0 && !Ended ? null : "story " + (Ended ? "1" : "0") + " " + string.Join(",", new SortedSet<string>(Fragments));

        public void Load(string[] p)
        {
            Ended = p.Length > 1 && p[1] == "1";
            if (p.Length > 2) foreach (var f in p[2].Split(',')) if (f.Length > 0) Fragments.Add(f);
        }

        public void CopyFrom(Story s)
        {
            Fragments.Clear(); Fragments.UnionWith(s.Fragments);
            if (s.Ended) End();
        }
    }

    /// <summary>
    /// The Meridian relays: one abandoned station in orbit above each world. WARDEN split the long-range antenna's unlock
    /// code across them; each relay gives its fragment to whoever solves its maintenance check with a program.
    /// </summary>
    public static class Relays
    {
        public const int Total = 4;
        public const float DockFuel = 5, LandFuel = 10;

        public static readonly string[] Order = { "home", "volcanic", "desert", "ruins" };

        static readonly Dictionary<string, (string name, string log)> Info = new Dictionary<string, (string, string)>
        {
            ["home"] = ("Relay Alpha", "Relay Alpha, Meridian network. WARDEN split the antenna unlock code in four and hid the parts on the relays above each world, so no single station could call the settlers without its approval. This relay checks every visitor: add up the telemetry and send the total."),
            ["volcanic"] = ("Relay Beta", "Relay Beta. Heat damage on every deck. The custodians kept a fault log here; WARDEN trusts only visitors who can read it. Count the lines that report an ERR and send the number."),
            ["desert"] = ("Relay Gamma", "Relay Gamma. Sand got into the sensor buffer and scrambled the calibration readings. Put them back in order, smallest first, and send the whole list."),
            ["ruins"] = ("Relay Delta", "Relay Delta, above the old capital. The last crew left a message here, encoded so WARDEN could not read it. Each letter was moved forward in the alphabet by the shift shown. Decode it and send the text."),
        };

        public static string Name(string planetId) => Info.TryGetValue(planetId, out var i) ? Loc.T("relay." + planetId + ".name", i.name) : null;
        public static string Log(string planetId) => Info.TryGetValue(planetId, out var i) ? Loc.T("relay." + planetId + ".log", i.log) : null;

        static readonly string[] Words = { "THE SETTLERS ARE WAITING", "KEEP YOUR CODE CLEAN", "WARDEN CANNOT HEAR THIS", "THE ANTENNA STILL WORKS", "BUILD THE SHIP AND CALL" };

        /// <summary>The relay's check for this star system: what to do, the data, and the right answer.</summary>
        public static (string kind, object data, object answer) Challenge(int homeSeed, string planetId)
        {
            int k = Array.IndexOf(Order, planetId);
            var r = new Random(unchecked(homeSeed * 7919 + k * 104729));
            switch (k)
            {
                case 0:
                {
                    var l = new List<object>(); double sum = 0;
                    for (int i = 0; i < 12; i++) { int v = r.Next(10, 500); l.Add((double)v); sum += v; }
                    return ("sum", l, sum);
                }
                case 1:
                {
                    var l = new List<object>(); int n = 0;
                    string[] parts = { "pump", "valve", "coolant", "drone", "hatch", "array", "relay", "fan" };
                    for (int i = 0; i < 15; i++)
                    {
                        bool e = r.Next(3) == 0; if (e) n++;
                        l.Add($"{(e ? "ERR" : "OK ")} {parts[r.Next(parts.Length)]} {r.Next(1, 99)}");
                    }
                    return ("count", l, (double)n);
                }
                case 2:
                {
                    var l = new List<object>(); var sorted = new List<double>();
                    for (int i = 0; i < 10; i++) { double v = r.Next(-50, 200); l.Add(v); sorted.Add(v); }
                    sorted.Sort();
                    return ("sort", l, sorted.ConvertAll(x => (object)x));
                }
                case 3:
                {
                    string plain = Words[r.Next(Words.Length)];
                    int shift = r.Next(3, 23);
                    var sb = new StringBuilder();
                    foreach (char c in plain) sb.Append(c >= 'A' && c <= 'Z' ? (char)('A' + (c - 'A' + shift) % 26) : c);
                    return ("caesar", new Dictionary<string, object> { { "text", sb.ToString() }, { "shift", (double)shift } }, plain);
                }
                default: return (null, null, null);
            }
        }

        /// <summary>Is a program's answer right? Numbers, lists of numbers and text are compared by value.</summary>
        public static bool Check(object expected, object given)
        {
            switch (expected)
            {
                case double d: return given is double g && Math.Abs(d - g) < 1e-9;
                case string s: return given is string t && string.Equals(s, t.Trim(), StringComparison.OrdinalIgnoreCase);
                case List<object> l:
                    if (!(given is List<object> gl) || gl.Count != l.Count) return false;
                    for (int i = 0; i < l.Count; i++) if (!Check(l[i], gl[i])) return false;
                    return true;
                default: return false;
            }
        }

        /// <summary>What a relay gives the first time its check is solved (into the ship's cargo).</summary>
        public static (string item, int count)[] Salvage(string planetId)
        {
            switch (planetId)
            {
                case "home": return new[] { (ItemDatabase.Circuit, 10), (ItemDatabase.CopperWire, 20) };
                case "volcanic": return new[] { (ItemDatabase.TitaniumOre, 25) };
                case "desert": return new[] { (ItemDatabase.Silicon, 30), (ItemDatabase.AlienCrystal, 5) };
                default: return new[] { (ItemDatabase.UraniumOre, 15), (ItemDatabase.AlienCrystal, 10) };
            }
        }
    }

    /// <summary>Asteroids passing a world's orbit: a new set every game day, each minable once that day.</summary>
    public static class Asteroids
    {
        static readonly string[] Ores = { ItemDatabase.IronOre, ItemDatabase.CopperOre, ItemDatabase.TitaniumOre, ItemDatabase.TitaniumOre, ItemDatabase.UraniumOre, ItemDatabase.AlienCrystal, ItemDatabase.Silicon };

        public sealed class Rock { public int Id; public string Ore; public int Amount; public float Distance; public float Fuel => 3 + Distance; }

        public static List<Rock> Of(int homeSeed, string planetId, int day)
        {
            int k = Math.Max(0, Array.IndexOf(Relays.Order, planetId));
            var r = new Random(unchecked(homeSeed * 31337 + k * 7907 + day * 104659));
            var l = new List<Rock>();
            for (int i = 0; i < 5; i++)
                l.Add(new Rock { Id = i + 1, Ore = Ores[r.Next(Ores.Length)], Amount = r.Next(10, 41), Distance = (float)Math.Round(1 + r.NextDouble() * 8, 1) });
            return l;
        }

        public static Dictionary<string, object> Info(Rock a, bool mined) => new Dictionary<string, object>
        {
            { "id", (double)a.Id }, { "ore", a.Ore }, { "amount", (double)a.Amount }, { "distance", Math.Round(a.Distance, 1) }, { "fuel", Math.Round(a.Fuel, 1) }, { "mined", mined },
        };
    }
}
