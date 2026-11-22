using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Codebound.Core.Hud
{
    /// <summary>One player-made HUD box: HTML + CSS, position and size on screen (pixels at UI scale 1).</summary>
    public sealed class HudPanel
    {
        public int Id;
        public string Title = "Panel";
        public string Html = "", Css = "";
        public float X = 20, Y = 205, W = 260, H = 220;   // below the fixed status block
        public bool Collapsed;
    }

    /// <summary>The player's HUD: panels plus values published by programs with hud.set(key, value).</summary>
    public sealed class HudLayout
    {
        public const int MaxValues = 200, MaxKeyLength = 40, MaxValueLength = 200, MaxPanels = 20;
        public readonly List<HudPanel> Panels = new List<HudPanel>();
        public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
        public readonly Queue<string> Notifications = new Queue<string>();
        /// <summary>Programmable alarms (hud.alarm): a sound plus a notification.</summary>
        public readonly Queue<string> Alarms = new Queue<string>();
        int nextId = 1;
        public event Action Changed;

        static readonly Regex KeyRx = new Regex("^[A-Za-z_][A-Za-z0-9_.-]{0,39}$");

        public HudPanel Add(HudPanel p)
        {
            if (Panels.Count >= MaxPanels) return null;
            p.Id = nextId++;
            Panels.Add(p);
            Changed?.Invoke();
            return p;
        }

        public void Remove(HudPanel p) { if (Panels.Remove(p)) Changed?.Invoke(); }
        public void Touch() => Changed?.Invoke();

        /// <summary>Validates and stores a program-published value. Returns an error message or null.</summary>
        public string SetValue(string key, string value)
        {
            if (key == null || !KeyRx.IsMatch(key)) return "hud.set: key must be letters/digits/_ . - (max 40 chars), starting with a letter";
            if (key.StartsWith("player.") || key.StartsWith("inventory.") || key.StartsWith("power.") || key.StartsWith("game."))
                return $"hud.set: '{key}' is a built-in name and can't be overwritten";
            if (!Values.ContainsKey(key) && Values.Count >= MaxValues) return $"hud.set: too many keys (max {MaxValues})";
            value = value ?? "";
            Values[key] = value.Length > MaxValueLength ? value.Substring(0, MaxValueLength) : value;
            return null;
        }

        public void Notify(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (Notifications.Count >= 20) Notifications.Dequeue();
            Notifications.Enqueue(text.Length > 200 ? text.Substring(0, 200) : text);
        }

        public void Alarm(string text)
        {
            text = string.IsNullOrEmpty(text) ? "ALARM" : (text.Length > 120 ? text.Substring(0, 120) : text);
            if (Alarms.Count < 10) Alarms.Enqueue(text);
            Notify("ALARM: " + text);
        }

        // ------------------------------------------------------------ starter content
        /// <summary>The starter box, with its labels in the player's language (it is plain HTML the player can edit).</summary>
        public static string StarterHtml =>
            "<!-- Your HUD box. Edit the HTML and CSS, then press Apply. -->\n" +
            "<div class=\"card\">\n" +
            "  <h3>" + Loc.T("hudtpl.status", "Status") + "</h3>\n" +
            "  <p class=\"warn\">{{player.warning}}</p>\n" +
            "  <p>" + Loc.T("hudtpl.health", "Health") + " {{player.health}}</p>\n" +
            "  <progress class=\"hp\" value=\"{{player.health}}\" max=\"100\"></progress>\n" +
            "  <p>" + Loc.T("hudtpl.vitals", "Food {0}  Water {1}  O2 {2}  {3} C", "{{player.hunger}}", "{{player.thirst}}", "{{player.oxygen}}", "{{player.temp}}") + "</p>\n" +
            "  <p>" + Loc.T("hudtpl.biome", "Biome:") + " <b>{{player.biome}}</b></p>\n" +
            "  <p>" + Loc.T("hudtpl.position", "Position:") + " {{player.x}}, {{player.y}}</p>\n" +
            "  <p>" + Loc.T("hudtpl.power", "Power:") + " <span class=\"kw\">{{power.demandKW}} / {{power.capacityKW}} kW</span></p>\n" +
            "  <progress value=\"{{power.usage}}\" max=\"100\"></progress>\n" +
            "  <p>" + Loc.T("hudtpl.ores", "Iron ore: {0}  Coal: {1}", "{{inventory.iron_ore}}", "{{inventory.coal}}") + "</p>\n" +
            "</div>\n";

        public const string StarterCss =
            "/* Style your box. Supported: colors, sizes, padding, margin, border, flex... */\n" +
            ".card { padding: 4px; }\n" +
            "h3 { color: #5ef0a0; }\n" +
            ".kw { color: #ffd43b; }\n" +
            "progress { height: 8px; }\n" +
            ".hp { color: #ff5d5d; }\n" +
            ".warn { color: orange; font-weight: bold; }\n";

        public static HudPanel Starter() => new HudPanel { Title = Loc.T("hudtpl.status", "Status"), Html = StarterHtml, Css = StarterCss };

        // ------------------------------------------------------------ save
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static string B64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s ?? ""));
        static string Un64(string s) => Encoding.UTF8.GetString(Convert.FromBase64String(s));

        public IEnumerable<string> SaveLines()
        {
            foreach (var p in Panels)
                yield return string.Join(" ", "hud", p.X.ToString("R", Inv), p.Y.ToString("R", Inv), p.W.ToString("R", Inv), p.H.ToString("R", Inv),
                    p.Collapsed ? "1" : "0", B64(p.Title), B64(p.Html), B64(p.Css));
        }

        public void LoadLine(string[] parts)
        {
            if (parts.Length < 9) throw new FormatException("hud record needs 8 fields");
            Add(new HudPanel
            {
                X = float.Parse(parts[1], Inv), Y = float.Parse(parts[2], Inv), W = float.Parse(parts[3], Inv), H = float.Parse(parts[4], Inv),
                Collapsed = parts[5] == "1", Title = Un64(parts[6]), Html = Un64(parts[7]), Css = Un64(parts[8]),
            });
        }
    }

    /// <summary>{{ key }} interpolation. Unknown keys render empty and are reported once per render.</summary>
    public static class HudTemplate
    {
        static readonly Regex Rx = new Regex(@"\{\{\s*([A-Za-z_][A-Za-z0-9_.-]*)\s*\}\}");

        public static string Render(string s, Func<string, string> lookup, ICollection<string> missing)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf("{{", StringComparison.Ordinal) < 0) return s;
            return Rx.Replace(s, m =>
            {
                string v = lookup(m.Groups[1].Value);
                if (v == null) { missing?.Add(m.Groups[1].Value); return ""; }
                return v;
            });
        }
    }
}
