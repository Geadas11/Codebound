using System;
using System.Collections.Generic;
using System.Text;
using Codebound.Core.Hud;
using UnityEngine;
using UnityEngine.UIElements;

namespace Codebound.Game
{
    /// <summary>
    /// Turns the parsed HTML subset + CSS cascade into UI Toolkit elements. Block elements become
    /// VisualElements (flex column by default); runs of text/inline elements become one rich-text Label,
    /// because UI Toolkit has no inline flow.
    /// </summary>
    public static class HudRenderer
    {
        public sealed class Context
        {
            public List<CssRule> Rules;
            public Func<string, string> Lookup;
            public HashSet<string> Missing;
            public List<string> Problems;
        }

        static readonly Dictionary<string, string> UserAgent = new Dictionary<string, string>
        {
            { "h1", "font-size: 22px; font-weight: bold; margin: 2px 0" },
            { "h2", "font-size: 18px; font-weight: bold; margin: 2px 0" },
            { "h3", "font-size: 15px; font-weight: bold; margin: 2px 0" },
            { "p", "margin: 2px 0" },
            { "ul", "padding-left: 10px" }, { "ol", "padding-left: 10px" },
            { "hr", "height: 1px; background-color: #3a4a55; margin: 4px 0" },
            { "progress", "height: 8px; background-color: #1c2630; color: #5ef0a0; border-radius: 3px; margin: 3px 0" },
        };

        public static VisualElement Render(HtmlNode root, Context ctx)
        {
            var host = new VisualElement();
            BuildChildren(root, host, ctx);
            return host;
        }

        static bool IsInline(HtmlNode n) => n.IsText || HtmlParser.InlineTags.Contains(n.Tag) || n.Tag == "br";

        static void BuildChildren(HtmlNode parent, VisualElement into, Context ctx)
        {
            var run = new List<HtmlNode>();
            int ordinal = 0;
            void FlushRun()
            {
                if (run.Count == 0) return;
                var sb = new StringBuilder();
                foreach (var n in run) AppendInline(n, sb, ctx);
                string text = CollapseWhitespace(sb.ToString());
                run.Clear();
                if (text.Trim().Length == 0) return;
                into.Add(new Label(text) { enableRichText = true, style = { whiteSpace = WhiteSpace.Normal, marginLeft = 0, marginRight = 0, paddingLeft = 0, paddingRight = 0 } });
            }
            foreach (var child in parent.Children)
            {
                if (IsInline(child)) { run.Add(child); continue; }
                FlushRun();
                into.Add(BuildBlock(child, ctx, parent.Tag == "ol" ? ++ordinal : 0, parent.Tag == "ul"));
            }
            FlushRun();
        }

        static VisualElement BuildBlock(HtmlNode n, Context ctx, int ordinal, bool bullet)
        {
            var style = Compute(n, ctx);
            if (n.Tag == "progress") return BuildProgress(n, style, ctx);
            var ve = new VisualElement { name = n.Id };
            foreach (var c in n.Classes) ve.AddToClassList(c);
            Apply(ve, style, ctx.Problems);
            if (n.Tag == "li")
            {
                ve.style.flexDirection = FlexDirection.Row;
                ve.Add(new Label(ordinal > 0 ? ordinal + ". " : bullet ? "• " : "") { style = { marginLeft = 0, paddingLeft = 0 } });
                var body = new VisualElement { style = { flexGrow = 1, flexShrink = 1 } };
                BuildChildren(n, body, ctx);
                ve.Add(body);
                return ve;
            }
            BuildChildren(n, ve, ctx);
            return ve;
        }

        static VisualElement BuildProgress(HtmlNode n, Dictionary<string, string> style, Context ctx)
        {
            float Num(string attr, float def)
            {
                if (!n.Attrs.TryGetValue(attr, out var raw)) return def;
                string v = HudTemplate.Render(raw, ctx.Lookup, ctx.Missing);
                return float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f) ? f : def;
            }
            float max = Mathf.Max(1e-6f, Num("max", 1f)), value = Mathf.Clamp(Num("value", 0f), 0, max);
            var outer = new VisualElement { name = n.Id };
            Apply(outer, style, ctx.Problems);
            var fill = new VisualElement { style = { width = Length.Percent(100f * value / max), height = Length.Percent(100) } };
            fill.style.backgroundColor = style.TryGetValue("color", out var c) && CssValue.TryColor(c, out var rgba) ? ToColor(rgba) : new Color(0.37f, 0.94f, 0.63f);
            if (style.TryGetValue("border-radius", out var br) && CssValue.TryLength(br, out var brl)) SetRadius(fill, brl.Value);
            outer.style.color = StyleKeyword.Null;
            outer.Add(fill);
            return outer;
        }

        static Dictionary<string, string> Compute(HtmlNode n, Context ctx)
        {
            var problems = new List<string>();
            var st = new Dictionary<string, string>();
            if (UserAgent.TryGetValue(n.Tag, out var ua)) foreach (var d in CssParser.ParseDeclarations(ua, 0, problems)) st[d.Key] = d.Value;
            foreach (var kv in CssParser.Compute(n, ctx.Rules, ctx.Problems)) st[kv.Key] = HudTemplate.Render(kv.Value, ctx.Lookup, ctx.Missing);
            return st;
        }

        // ---------------------------------------------------------------- inline → rich text
        static void AppendInline(HtmlNode n, StringBuilder sb, Context ctx)
        {
            if (n.IsText) { sb.Append(EscapeRich(HudTemplate.Render(n.Text, ctx.Lookup, ctx.Missing))); return; }
            if (n.Tag == "br") { sb.Append('\n'); return; }
            var st = Compute(n, ctx);
            var close = new Stack<string>();
            void Open(string tag, string arg = null) { sb.Append('<').Append(tag); if (arg != null) sb.Append('=').Append(arg); sb.Append('>'); close.Push("</" + tag + ">"); }
            bool bold = n.Tag == "b" || n.Tag == "strong" || (st.TryGetValue("font-weight", out var fw) && (fw == "bold" || fw.StartsWith("6") || fw.StartsWith("7") || fw.StartsWith("8") || fw.StartsWith("9")));
            bool italic = n.Tag == "i" || n.Tag == "em" || (st.TryGetValue("font-style", out var fs) && fs == "italic");
            if (bold) Open("b");
            if (italic) Open("i");
            if (n.Tag == "u") Open("u");
            if (st.TryGetValue("color", out var col) && CssValue.TryColor(col, out var rgba)) Open("color", "#" + ColorUtility.ToHtmlStringRGBA(ToColor(rgba)));
            else if (n.Tag == "code") Open("color", "#9cdcfe");
            if (st.TryGetValue("font-size", out var fz) && CssValue.TryLength(fz, out var len)) Open("size", Mathf.RoundToInt(len.Unit == CssValue.Unit.Em ? len.Value * 13 : len.Value).ToString());
            else if (n.Tag == "small") Open("size", "11");
            if (st.TryGetValue("background-color", out var bg) && CssValue.TryColor(bg, out var bgc)) Open("mark", "#" + ColorUtility.ToHtmlStringRGBA(ToColor(bgc)));
            foreach (var c in n.Children) AppendInline(c, sb, ctx);
            while (close.Count > 0) sb.Append(close.Pop());
        }

        /// <summary>Player text must never be read as rich-text tags.</summary>
        static string EscapeRich(string s) => s.IndexOf('<') < 0 ? s : s.Replace("<", "<​");

        static string CollapseWhitespace(string s)
        {
            var sb = new StringBuilder(s.Length);
            bool space = false;
            foreach (char c in s)
            {
                if (c == '\n' && sb.Length > 0 && sb[sb.Length - 1] == '\n') continue;
                if (c == ' ' || c == '\t' || c == '\r') { space = true; continue; }
                if (c == '\n') { space = false; sb.Append('\n'); continue; }
                if (space && sb.Length > 0 && sb[sb.Length - 1] != '\n') sb.Append(' ');
                space = false; sb.Append(c);
            }
            return sb.ToString();
        }

        // ---------------------------------------------------------------- CSS → UI Toolkit style
        public static Color ToColor(CssValue.Rgba c) => new Color(c.R, c.G, c.B, c.A);

        static StyleLength Len(CssValue.Length l) =>
            l.Unit == CssValue.Unit.Auto ? new StyleLength(StyleKeyword.Auto)
            : l.Unit == CssValue.Unit.Percent ? new StyleLength(Length.Percent(l.Value))
            : new StyleLength(l.Unit == CssValue.Unit.Em ? l.Value * 13 : l.Value);

        static void SetRadius(VisualElement ve, float r)
        {
            ve.style.borderTopLeftRadius = r; ve.style.borderTopRightRadius = r; ve.style.borderBottomLeftRadius = r; ve.style.borderBottomRightRadius = r;
        }

        static void Apply(VisualElement ve, Dictionary<string, string> st, List<string> problems)
        {
            var s = ve.style;
            FontStyle fontStyle = FontStyle.Normal;
            foreach (var kv in st)
            {
                string v = kv.Value.Trim().ToLowerInvariant();
                bool ok = true;
                switch (kv.Key)
                {
                    case "color": ok = CssValue.TryColor(v, out var c1); if (ok) s.color = ToColor(c1); break;
                    case "background":
                    case "background-color": ok = CssValue.TryColor(v, out var c2); if (ok) s.backgroundColor = ToColor(c2); break;
                    case "font-size": ok = CssValue.TryLength(v, out var fl); if (ok) s.fontSize = Len(fl); break;
                    case "font-weight": if (v == "bold" || v.StartsWith("6") || v.StartsWith("7") || v.StartsWith("8") || v.StartsWith("9")) fontStyle = fontStyle == FontStyle.Italic ? FontStyle.BoldAndItalic : FontStyle.Bold; break;
                    case "font-style": if (v == "italic") fontStyle = fontStyle == FontStyle.Bold ? FontStyle.BoldAndItalic : FontStyle.Italic; break;
                    case "text-align":
                        s.unityTextAlign = v == "center" ? TextAnchor.UpperCenter : v == "right" ? TextAnchor.UpperRight : TextAnchor.UpperLeft; break;
                    case "padding": ok = CssValue.TryBox(v, out var pb); if (ok) { s.paddingTop = Len(pb[0]); s.paddingRight = Len(pb[1]); s.paddingBottom = Len(pb[2]); s.paddingLeft = Len(pb[3]); } break;
                    case "margin": ok = CssValue.TryBox(v, out var mb); if (ok) { s.marginTop = Len(mb[0]); s.marginRight = Len(mb[1]); s.marginBottom = Len(mb[2]); s.marginLeft = Len(mb[3]); } break;
                    case "padding-top": ok = CssValue.TryLength(v, out var l1); if (ok) s.paddingTop = Len(l1); break;
                    case "padding-right": ok = CssValue.TryLength(v, out var l2); if (ok) s.paddingRight = Len(l2); break;
                    case "padding-bottom": ok = CssValue.TryLength(v, out var l3); if (ok) s.paddingBottom = Len(l3); break;
                    case "padding-left": ok = CssValue.TryLength(v, out var l4); if (ok) s.paddingLeft = Len(l4); break;
                    case "margin-top": ok = CssValue.TryLength(v, out var m1); if (ok) s.marginTop = Len(m1); break;
                    case "margin-right": ok = CssValue.TryLength(v, out var m2); if (ok) s.marginRight = Len(m2); break;
                    case "margin-bottom": ok = CssValue.TryLength(v, out var m3); if (ok) s.marginBottom = Len(m3); break;
                    case "margin-left": ok = CssValue.TryLength(v, out var m4); if (ok) s.marginLeft = Len(m4); break;
                    case "border":
                        foreach (var part in v.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (CssValue.TryLength(part, out var bw) && bw.Unit == CssValue.Unit.Px) { s.borderTopWidth = bw.Value; s.borderRightWidth = bw.Value; s.borderBottomWidth = bw.Value; s.borderLeftWidth = bw.Value; }
                            else if (CssValue.TryColor(part, out var bc)) { var col = ToColor(bc); s.borderTopColor = col; s.borderRightColor = col; s.borderBottomColor = col; s.borderLeftColor = col; }
                        }
                        break;
                    case "border-width": ok = CssValue.TryLength(v, out var bwl); if (ok) { s.borderTopWidth = bwl.Value; s.borderRightWidth = bwl.Value; s.borderBottomWidth = bwl.Value; s.borderLeftWidth = bwl.Value; } break;
                    case "border-color": ok = CssValue.TryColor(v, out var bcl); if (ok) { var col = ToColor(bcl); s.borderTopColor = col; s.borderRightColor = col; s.borderBottomColor = col; s.borderLeftColor = col; } break;
                    case "border-radius": ok = CssValue.TryLength(v, out var rl); if (ok) SetRadius(ve, rl.Value); break;
                    case "width": ok = CssValue.TryLength(v, out var w); if (ok) s.width = Len(w); break;
                    case "height": ok = CssValue.TryLength(v, out var h); if (ok) s.height = Len(h); break;
                    case "min-width": ok = CssValue.TryLength(v, out var w1); if (ok) s.minWidth = Len(w1); break;
                    case "min-height": ok = CssValue.TryLength(v, out var h1); if (ok) s.minHeight = Len(h1); break;
                    case "max-width": ok = CssValue.TryLength(v, out var w2); if (ok) s.maxWidth = Len(w2); break;
                    case "max-height": ok = CssValue.TryLength(v, out var h2); if (ok) s.maxHeight = Len(h2); break;
                    case "display": if (v == "none") s.display = DisplayStyle.None; else if (v == "flex" || v == "block") s.display = DisplayStyle.Flex; else ok = false; break;
                    case "flex-direction":
                        s.flexDirection = v == "row" ? FlexDirection.Row : v == "row-reverse" ? FlexDirection.RowReverse : v == "column-reverse" ? FlexDirection.ColumnReverse : FlexDirection.Column; break;
                    case "flex-wrap": s.flexWrap = v == "wrap" ? Wrap.Wrap : Wrap.NoWrap; break;
                    case "flex-grow": ok = float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float g); if (ok) s.flexGrow = g; break;
                    case "justify-content":
                        s.justifyContent = v == "center" ? Justify.Center : v == "flex-end" || v == "end" ? Justify.FlexEnd : v == "space-between" ? Justify.SpaceBetween : v == "space-around" ? Justify.SpaceAround : Justify.FlexStart; break;
                    case "align-items":
                        s.alignItems = v == "center" ? Align.Center : v == "flex-end" || v == "end" ? Align.FlexEnd : v == "stretch" ? Align.Stretch : Align.FlexStart; break;
                    case "opacity": ok = float.TryParse(v, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float o); if (ok) s.opacity = Mathf.Clamp01(o); break;
                }
                if (!ok) problems.Add($"value '{kv.Value}' is not valid for '{kv.Key}'");
            }
            if (fontStyle != FontStyle.Normal) s.unityFontStyleAndWeight = fontStyle;
        }
    }
}
