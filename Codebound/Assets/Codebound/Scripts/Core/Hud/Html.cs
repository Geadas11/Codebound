using System;
using System.Collections.Generic;
using System.Text;

namespace Codebound.Core.Hud
{
    /// <summary>A parsed HTML element or text run.</summary>
    public sealed class HtmlNode
    {
        public string Tag;                    // null for text nodes
        public string Text;                   // text nodes only (entities decoded)
        public readonly Dictionary<string, string> Attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public readonly List<HtmlNode> Children = new List<HtmlNode>();
        public HtmlNode Parent;
        public int Line;

        public bool IsText => Tag == null;
        public string Id => Attrs.TryGetValue("id", out var v) ? v : null;
        public IEnumerable<string> Classes =>
            Attrs.TryGetValue("class", out var v) ? v.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();

        public override string ToString() => IsText ? "\"" + Text + "\"" : "<" + Tag + ">";
    }

    /// <summary>
    /// Forgiving parser for the HTML subset the HUD supports. Never throws: problems become friendly
    /// messages with line numbers, because players are learning HTML here.
    /// </summary>
    public static class HtmlParser
    {
        public static readonly HashSet<string> BlockTags = new HashSet<string>
            { "div", "p", "h1", "h2", "h3", "ul", "ol", "li", "section", "header", "footer", "hr", "progress", "br", "button", "img" };
        public static readonly HashSet<string> InlineTags = new HashSet<string>
            { "span", "b", "strong", "i", "em", "small", "code", "u" };
        static readonly HashSet<string> VoidTags = new HashSet<string> { "br", "hr", "img", "input", "meta" };

        public static bool IsSupported(string tag) => BlockTags.Contains(tag) || InlineTags.Contains(tag);

        public static HtmlNode Parse(string html, List<string> problems)
        {
            var root = new HtmlNode { Tag = "root" };
            var cur = root;
            int i = 0, line = 1, n = html?.Length ?? 0;
            var text = new StringBuilder();

            void FlushText()
            {
                if (text.Length == 0) return;
                string t = Decode(text.ToString());
                text.Clear();
                if (t.Trim().Length == 0 && !InlineTags.Contains(cur.Tag)) return; // drop formatting whitespace between blocks
                cur.Children.Add(new HtmlNode { Text = t, Parent = cur, Line = line });
            }

            while (i < n)
            {
                char c = html[i];
                if (c != '<') { if (c == '\n') line++; text.Append(c); i++; continue; }

                if (string.CompareOrdinal(html, i, "<!--", 0, 4) == 0)
                {
                    int end = html.IndexOf("-->", i + 4, StringComparison.Ordinal);
                    int stop = end < 0 ? n : end + 3;
                    for (int k = i; k < stop; k++) if (html[k] == '\n') line++;
                    if (end < 0) problems.Add($"line {line}: comment '<!--' is never closed with '-->'");
                    i = stop; continue;
                }

                int close = html.IndexOf('>', i + 1);
                if (close < 0) { problems.Add($"line {line}: '<' without a matching '>' (write &lt; for a literal <)"); text.Append(c); i++; continue; }
                FlushText();
                string inner = html.Substring(i + 1, close - i - 1);
                int tagLine = line;
                foreach (char ch in inner) if (ch == '\n') line++;
                i = close + 1;

                if (inner.StartsWith("/"))
                {
                    string name = inner.Substring(1).Trim().ToLowerInvariant();
                    var match = cur;
                    while (match != null && match.Tag != name) match = match.Parent;
                    if (match == null || match == root) { problems.Add($"line {tagLine}: closing tag </{name}> has no matching <{name}>"); continue; }
                    for (var x = cur; x != match; x = x.Parent) problems.Add($"line {tagLine}: <{x.Tag}> (line {x.Line}) was not closed before </{name}>");
                    cur = match.Parent;
                    continue;
                }

                bool selfClose = inner.EndsWith("/");
                if (selfClose) inner = inner.Substring(0, inner.Length - 1);
                var node = ParseTag(inner, tagLine, problems);
                if (node == null) continue;
                node.Parent = cur;
                cur.Children.Add(node);
                if (!IsSupported(node.Tag)) problems.Add($"line {tagLine}: <{node.Tag}> is not supported in the HUD yet — shown as <div>");
                if (!selfClose && !VoidTags.Contains(node.Tag)) cur = node;
            }
            FlushText();
            for (var x = cur; x != root && x != null; x = x.Parent) problems.Add($"line {x.Line}: <{x.Tag}> is never closed");
            return root;
        }

        static HtmlNode ParseTag(string inner, int line, List<string> problems)
        {
            int j = 0, n = inner.Length;
            while (j < n && !char.IsWhiteSpace(inner[j])) j++;
            string tag = inner.Substring(0, j).ToLowerInvariant();
            if (tag.Length == 0 || !char.IsLetter(tag[0])) { problems.Add($"line {line}: '<{inner}>' is not a valid tag"); return null; }
            var node = new HtmlNode { Tag = tag, Line = line };
            while (j < n)
            {
                while (j < n && char.IsWhiteSpace(inner[j])) j++;
                int s = j;
                while (j < n && inner[j] != '=' && !char.IsWhiteSpace(inner[j])) j++;
                if (s == j) { j++; continue; }
                string name = inner.Substring(s, j - s);
                string value = "";
                while (j < n && char.IsWhiteSpace(inner[j])) j++;
                if (j < n && inner[j] == '=')
                {
                    j++;
                    while (j < n && char.IsWhiteSpace(inner[j])) j++;
                    if (j < n && (inner[j] == '"' || inner[j] == '\''))
                    {
                        char q = inner[j++]; int e = inner.IndexOf(q, j);
                        if (e < 0) { problems.Add($"line {line}: attribute {name} is missing its closing {q}"); e = n; }
                        value = inner.Substring(j, e - j); j = Math.Min(n, e + 1);
                    }
                    else { int e = j; while (e < n && !char.IsWhiteSpace(inner[e])) e++; value = inner.Substring(j, e - j); j = e; }
                }
                node.Attrs[name] = Decode(value);
            }
            return node;
        }

        public static string Decode(string s)
        {
            if (s.IndexOf('&') < 0) return s;
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '&') { sb.Append(s[i]); continue; }
                int semi = s.IndexOf(';', i);
                if (semi < 0 || semi - i > 8) { sb.Append('&'); continue; }
                string ent = s.Substring(i + 1, semi - i - 1);
                string rep = ent == "amp" ? "&" : ent == "lt" ? "<" : ent == "gt" ? ">" : ent == "quot" ? "\"" : ent == "apos" ? "'" : ent == "nbsp" ? " " : null;
                if (rep == null && ent.StartsWith("#") && int.TryParse(ent.Substring(1), out int code) && code > 0 && code < 0x10000) rep = ((char)code).ToString();
                if (rep == null) { sb.Append('&'); continue; }
                sb.Append(rep); i = semi;
            }
            return sb.ToString();
        }
    }
}
