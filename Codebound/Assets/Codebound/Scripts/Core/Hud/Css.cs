using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Codebound.Core.Hud
{
    /// <summary>One simple selector part: tag, #id and .classes (e.g. "div.card#main"). "*" matches anything.</summary>
    public sealed class SimpleSelector
    {
        public string Tag; public string Id; public readonly List<string> Classes = new List<string>();

        public bool Matches(HtmlNode n)
        {
            if (n.IsText) return false;
            if (Tag != null && Tag != "*" && Tag != n.Tag) return false;
            if (Id != null && Id != n.Id) return false;
            foreach (var c in Classes) if (!n.Classes.Contains(c)) return false;
            return true;
        }
    }

    public sealed class CssRule
    {
        public List<SimpleSelector> Chain;   // descendant combinator: a b c
        public int Specificity;              // ids*10000 + classes*100 + tags
        public int Order;
        public readonly List<KeyValuePair<string, string>> Declarations = new List<KeyValuePair<string, string>>();
        public string SelectorText;

        public bool Matches(HtmlNode n)
        {
            if (!Chain[Chain.Count - 1].Matches(n)) return false;
            int k = Chain.Count - 2;
            for (var a = n.Parent; a != null && k >= 0; a = a.Parent) if (Chain[k].Matches(a)) k--;
            return k < 0;
        }
    }

    /// <summary>CSS subset: rules with tag/.class/#id/descendant selectors, comments, cascade by specificity and order.</summary>
    public static class CssParser
    {
        public static readonly HashSet<string> Supported = new HashSet<string>
        {
            "color", "background-color", "background", "font-size", "font-weight", "font-style", "text-align",
            "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
            "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
            "border", "border-width", "border-color", "border-radius", "width", "height", "min-width", "min-height",
            "max-width", "max-height", "display", "flex-direction", "justify-content", "align-items", "flex-grow",
            "flex-wrap", "opacity",
        };

        public static List<CssRule> Parse(string css, List<string> problems)
        {
            var rules = new List<CssRule>();
            if (string.IsNullOrEmpty(css)) return rules;
            // strip comments, remembering line numbers by replacing comment chars with spaces/newlines
            var chars = css.ToCharArray();
            for (int i = 0; i + 1 < chars.Length; i++)
            {
                if (chars[i] != '/' || chars[i + 1] != '*') continue;
                int end = css.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end < 0) { problems.Add($"line {LineAt(css, i)}: comment '/*' is never closed"); end = chars.Length - 2; }
                for (int k = i; k < end + 2 && k < chars.Length; k++) if (chars[k] != '\n') chars[k] = ' ';
                i = end + 1;
            }
            string s = new string(chars);
            int pos = 0, order = 0;
            while (pos < s.Length)
            {
                int open = s.IndexOf('{', pos);
                if (open < 0) { if (s.Substring(pos).Trim().Length > 0) problems.Add($"line {LineAt(s, pos)}: text after the last rule is ignored (missing '{{'?)"); break; }
                int close = s.IndexOf('}', open);
                if (close < 0) { problems.Add($"line {LineAt(s, open)}: '{{' is never closed with '}}'"); close = s.Length; }
                string selText = s.Substring(pos, open - pos).Trim();
                string body = s.Substring(open + 1, Math.Max(0, close - open - 1));
                int line = LineAt(s, open);
                var decls = ParseDeclarations(body, line, problems);
                foreach (var one in selText.Split(','))
                {
                    var chain = ParseSelector(one.Trim(), line, problems);
                    if (chain == null) continue;
                    var r = new CssRule { Chain = chain, Order = order++, SelectorText = one.Trim(), Specificity = SpecificityOf(chain) };
                    r.Declarations.AddRange(decls);
                    rules.Add(r);
                }
                pos = close + 1;
            }
            return rules;
        }

        public static List<KeyValuePair<string, string>> ParseDeclarations(string body, int line, List<string> problems)
        {
            var list = new List<KeyValuePair<string, string>>();
            foreach (var part in body.Split(';'))
            {
                if (part.Trim().Length == 0) continue;
                int colon = part.IndexOf(':');
                if (colon < 0) { problems.Add($"line {line}: '{part.Trim()}' is missing ':' (property: value;)"); continue; }
                string prop = part.Substring(0, colon).Trim().ToLowerInvariant();
                string val = part.Substring(colon + 1).Trim();
                if (!Supported.Contains(prop)) { problems.Add($"line {line}: property '{prop}' is not supported in the HUD yet"); continue; }
                list.Add(new KeyValuePair<string, string>(prop, val));
            }
            return list;
        }

        static List<SimpleSelector> ParseSelector(string text, int line, List<string> problems)
        {
            if (text.Length == 0) { problems.Add($"line {line}: empty selector"); return null; }
            var chain = new List<SimpleSelector>();
            foreach (var part in text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.IndexOfAny(new[] { '>', '+', '~', ':', '[' }) >= 0) { problems.Add($"line {line}: selector '{text}' uses features not supported yet (>, +, ~, :, [ ])"); return null; }
                var sel = new SimpleSelector();
                int i = 0;
                while (i < part.Length)
                {
                    char kind = part[i] == '.' || part[i] == '#' ? part[i++] : 't';
                    int s = i;
                    while (i < part.Length && part[i] != '.' && part[i] != '#') i++;
                    string name = part.Substring(s, i - s);
                    if (name.Length == 0) { problems.Add($"line {line}: selector '{text}' has an empty name"); return null; }
                    if (kind == '.') sel.Classes.Add(name); else if (kind == '#') sel.Id = name; else sel.Tag = name.ToLowerInvariant();
                }
                chain.Add(sel);
            }
            return chain;
        }

        static int SpecificityOf(List<SimpleSelector> chain)
        {
            int ids = 0, cls = 0, tags = 0;
            foreach (var c in chain) { if (c.Id != null) ids++; cls += c.Classes.Count; if (c.Tag != null && c.Tag != "*") tags++; }
            return ids * 10000 + cls * 100 + tags;
        }

        static int LineAt(string s, int index)
        {
            int l = 1;
            for (int i = 0; i < index && i < s.Length; i++) if (s[i] == '\n') l++;
            return l;
        }

        /// <summary>Cascade: matching rules by (specificity, order), then the inline style attribute wins.</summary>
        public static Dictionary<string, string> Compute(HtmlNode n, List<CssRule> rules, List<string> problems)
        {
            var result = new Dictionary<string, string>();
            foreach (var r in rules.Where(r => r.Matches(n)).OrderBy(r => r.Specificity).ThenBy(r => r.Order))
                foreach (var d in r.Declarations) result[d.Key] = d.Value;
            if (n.Attrs.TryGetValue("style", out var inline))
                foreach (var d in ParseDeclarations(inline, n.Line, problems)) result[d.Key] = d.Value;
            return result;
        }
    }

    /// <summary>CSS value parsing shared by the renderer and tests.</summary>
    public static class CssValue
    {
        public struct Rgba { public float R, G, B, A; public Rgba(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; } }
        public enum Unit { Px, Percent, Em, Auto }
        public struct Length { public float Value; public Unit Unit; }

        static readonly Dictionary<string, uint> Named = new Dictionary<string, uint>
        {
            { "white", 0xffffff }, { "black", 0x000000 }, { "red", 0xff0000 }, { "green", 0x008000 }, { "lime", 0x00ff00 },
            { "blue", 0x0000ff }, { "yellow", 0xffff00 }, { "orange", 0xffa500 }, { "gray", 0x808080 }, { "grey", 0x808080 },
            { "cyan", 0x00ffff }, { "magenta", 0xff00ff }, { "purple", 0x800080 }, { "pink", 0xffc0cb }, { "silver", 0xc0c0c0 },
            { "navy", 0x000080 }, { "teal", 0x008080 }, { "gold", 0xffd700 },
        };

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static bool TryColor(string v, out Rgba c)
        {
            c = default;
            v = v.Trim().ToLowerInvariant();
            if (v == "transparent") { c = new Rgba(0, 0, 0, 0); return true; }
            if (Named.TryGetValue(v, out uint rgb)) { c = new Rgba((rgb >> 16 & 255) / 255f, (rgb >> 8 & 255) / 255f, (rgb & 255) / 255f, 1); return true; }
            if (v.StartsWith("#"))
            {
                string h = v.Substring(1);
                if (h.Length == 3 || h.Length == 4) h = string.Concat(h.Select(ch => new string(ch, 2)));
                if ((h.Length != 6 && h.Length != 8) || !uint.TryParse(h, NumberStyles.HexNumber, Inv, out uint x)) return false;
                if (h.Length == 6) x = (x << 8) | 0xff;
                c = new Rgba((x >> 24 & 255) / 255f, (x >> 16 & 255) / 255f, (x >> 8 & 255) / 255f, (x & 255) / 255f);
                return true;
            }
            if (v.StartsWith("rgb"))
            {
                int a = v.IndexOf('('), b = v.IndexOf(')');
                if (a < 0 || b < a) return false;
                var p = v.Substring(a + 1, b - a - 1).Split(',');
                if (p.Length < 3) return false;
                float F(string t) => float.Parse(t.Trim(), Inv);
                try { c = new Rgba(F(p[0]) / 255f, F(p[1]) / 255f, F(p[2]) / 255f, p.Length > 3 ? F(p[3]) : 1); return true; }
                catch (FormatException) { return false; }
            }
            return false;
        }

        public static bool TryLength(string v, out Length l)
        {
            l = default;
            v = v.Trim().ToLowerInvariant();
            if (v == "auto") { l.Unit = Unit.Auto; return true; }
            Unit u = Unit.Px; string num = v;
            if (v.EndsWith("px")) num = v.Substring(0, v.Length - 2);
            else if (v.EndsWith("%")) { num = v.Substring(0, v.Length - 1); u = Unit.Percent; }
            else if (v.EndsWith("em")) { num = v.Substring(0, v.Length - 2); u = Unit.Em; }
            if (!float.TryParse(num, NumberStyles.Float, Inv, out float f)) return false;
            l.Value = f; l.Unit = u;
            return true;
        }

        /// <summary>CSS box shorthand: 1-4 values → (top, right, bottom, left).</summary>
        public static bool TryBox(string v, out Length[] box)
        {
            box = null;
            var parts = v.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1 || parts.Length > 4) return false;
            var ls = new Length[parts.Length];
            for (int i = 0; i < parts.Length; i++) if (!TryLength(parts[i], out ls[i])) return false;
            box = parts.Length == 1 ? new[] { ls[0], ls[0], ls[0], ls[0] }
                : parts.Length == 2 ? new[] { ls[0], ls[1], ls[0], ls[1] }
                : parts.Length == 3 ? new[] { ls[0], ls[1], ls[2], ls[1] }
                : ls;
            return true;
        }
    }
}
