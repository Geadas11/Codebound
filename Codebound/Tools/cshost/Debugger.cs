using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codebound
{
    /// <summary>
    /// The C# debugger. Before compiling in debug mode, every statement of the main file gets
    /// `global::Codebound.Dbg.At(line, new object[] { "name", (object)name, ... })` inserted in front of it (same line,
    /// so line numbers do not move) with the locals that are certainly assigned there, and every method body enters a
    /// frame for the call stack. Players cannot call Dbg themselves (the sandbox refuses it); the check runs on the
    /// original source, before instrumentation.
    /// </summary>
    public static class Dbg
    {
        internal static bool On;
        internal static HashSet<int> Bps = new HashSet<int>();
        internal static string Mode = "continue";
        internal static int PauseDepth;
        internal static List<string> Watches = new List<string>();
        static bool pausing;
        static readonly List<string> names = new List<string> { "main" };
        static readonly List<int> lines = new List<int> { 0 };

        public static Frame Enter(string name, int line) { names.Add(name); lines.Add(line); return new Frame(); }

        public sealed class Frame : IDisposable
        {
            public void Dispose() { if (names.Count > 1) { names.RemoveAt(names.Count - 1); lines.RemoveAt(lines.Count - 1); } }
        }

        public static void At(int line, object[] vars)
        {
            lines[lines.Count - 1] = line;
            if (!On || pausing) return;
            int depth = names.Count;
            if (!(Bps.Contains(line) || Mode == "step" || (Mode == "over" && depth <= PauseDepth) || (Mode == "out" && depth < PauseDepth))) return;
            pausing = true;
            try
            {
                var dict = new Dictionary<string, object>();
                var v = new JsonObject();
                for (int i = 0; i + 1 < vars.Length; i += 2) { dict[(string)vars[i]] = vars[i + 1]; v[(string)vars[i]] = Snap(vars[i + 1], 0); }
                var frames = new JsonArray();
                for (int i = names.Count - 1; i >= 0; i--) frames.Add($"{names[i]} : line {lines[i]}");
                var w = new JsonArray();
                foreach (var e in Watches) w.Add(new JsonArray(e, Watch(e, dict)));
                var r = Proto.Call("debug", "pause", line, v, frames, w);
                Mode = r?["mode"]?.ToString() ?? "continue";
                PauseDepth = depth;
                if (r?["watches"] is JsonArray wa) Watches = wa.Select(x => x?.ToString() ?? "").ToList();
            }
            finally { pausing = false; }
        }

        internal static JsonNode Snap(object o, int d)
        {
            switch (o)
            {
                case null: return null;
                case bool b: return b;
                case string s: return s.Length > 200 ? s.Substring(0, 200) + "…" : s;
                case JsonNode n: return n.DeepClone();
                case Machine m: return m.ToString();
                case Delegate _: return "ƒ function";
                case IConvertible c when o.GetType().IsPrimitive || o is decimal: return JsonValue.Create(Convert.ToDouble(c, System.Globalization.CultureInfo.InvariantCulture));
                case Enum e: return e.ToString();
            }
            if (d >= 2) return o.GetType().Name + "(…)";
            if (o is IDictionary dict)
            {
                var obj = new JsonObject(); int i = 0;
                foreach (DictionaryEntry kv in dict) { if (i++ >= 20) { obj["…"] = "more"; break; } obj[kv.Key?.ToString() ?? "null"] = Snap(kv.Value, d + 1); }
                return obj;
            }
            if (o is IEnumerable seq)
            {
                var arr = new JsonArray(); int i = 0;
                foreach (var x in seq) { if (i++ >= 20) { arr.Add("…"); break; } arr.Add(Snap(x, d + 1)); }
                return arr;
            }
            var fields = new JsonObject(); int k = 0;   // the player's own objects: public fields and properties
            foreach (var f in o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) { if (k++ >= 20) break; fields[f.Name] = Snap(f.GetValue(o), d + 1); }
            foreach (var p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (k++ >= 20 || p.GetIndexParameters().Length > 0) continue;
                try { fields[p.Name] = Snap(p.GetValue(o), d + 1); } catch (Exception) { fields[p.Name] = "(error)"; }
            }
            return fields.Count > 0 ? fields : o.GetType().Name;
        }

        static readonly Regex PathRx = new Regex(@"^([A-Za-z_]\w*)((?:\.[A-Za-z_]\w*|\[\d+\]|\[""[^""]*""\])*)$");
        static readonly Regex PartRx = new Regex(@"\.[A-Za-z_]\w*|\[\d+\]|\[""[^""]*""\]");

        /// <summary>Watch: a name and a path (items[0].Count, Drill.Name) — no code is evaluated.</summary>
        internal static JsonNode Watch(string expr, Dictionary<string, object> vars)
        {
            var m = PathRx.Match((expr ?? "").Trim());
            if (!m.Success) return "(use a name or a path, e.g. items[0].Count)";
            object v;
            if (vars.TryGetValue(m.Groups[1].Value, out var local)) v = local;
            else if (typeof(Game).GetProperty(m.Groups[1].Value) is PropertyInfo gp) v = gp.GetValue(null);
            else return "(not defined here)";
            foreach (Match p in PartRx.Matches(m.Groups[2].Value))
            {
                if (v == null) return $"(cannot read {p.Value} of null)";
                string s = p.Value;
                try
                {
                    if (s[0] == '.')
                    {
                        var t = v.GetType(); string name = s.Substring(1);
                        if (t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance) is PropertyInfo pi && pi.GetIndexParameters().Length == 0) v = pi.GetValue(v);
                        else if (t.GetField(name, BindingFlags.Public | BindingFlags.Instance) is FieldInfo fi) v = fi.GetValue(v);
                        else return $"(no {name})";
                    }
                    else if (s[1] == '"')
                    {
                        string key = s.Substring(2, s.Length - 4);
                        v = v is JsonObject jo ? jo[key] : v is IDictionary dd ? dd[key] : throw new InvalidOperationException("not a dictionary");
                    }
                    else
                    {
                        int i = int.Parse(s.Substring(1, s.Length - 2));
                        v = v is JsonArray ja ? ja[i] : v is IList list ? list[i] : v is IEnumerable en ? en.Cast<object>().ElementAt(i) : throw new InvalidOperationException("not a list");
                    }
                }
                catch (Exception e) { return $"({e.GetType().Name})"; }
            }
            return Snap(v, 0);
        }

        // ------------------------------------------------------------------ instrumentation
        /// <summary>The instrumented main file, or null if instrumentation is not possible (then it runs normally).</summary>
        internal static string Instrument(CSharpCompilation comp, SyntaxTree tree)
        {
            var model = comp.GetSemanticModel(tree);
            var root = tree.GetRoot();
            var text = tree.GetText();
            var edits = new List<(int pos, int pri, string text)>();
            int LineOf(int pos) => text.Lines.GetLineFromPosition(pos).LineNumber + 1;

            foreach (var st in root.DescendantNodes().OfType<StatementSyntax>())
            {
                if (st is BlockSyntax) continue;
                var parent = st.Parent;
                bool list = parent is BlockSyntax || parent is GlobalStatementSyntax || parent is SwitchSectionSyntax;
                bool embedded = parent is IfStatementSyntax || parent is ElseClauseSyntax || parent is ForStatementSyntax || parent is ForEachStatementSyntax
                                || parent is WhileStatementSyntax || parent is DoStatementSyntax || parent is UsingStatementSyntax || parent is LockStatementSyntax;
                if (!list && !embedded) continue;
                string probe = $"global::Codebound.Dbg.At({LineOf(st.SpanStart)}, new object[] {{ {Visible(model, st)} }});";
                if (list) edits.Add((st.SpanStart, 0, probe));
                else { edits.Add((st.SpanStart, 0, "{" + probe)); edits.Add((st.Span.End, 0, "}")); }
            }
            foreach (var node in root.DescendantNodes())
            {
                BlockSyntax body = null; string name = null;
                if (node is MethodDeclarationSyntax md) { body = md.Body; name = md.Identifier.Text; }
                else if (node is ConstructorDeclarationSyntax cd) { body = cd.Body; name = cd.Identifier.Text; }
                else if (node is LocalFunctionStatementSyntax lf) { body = lf.Body; name = lf.Identifier.Text; }
                if (body == null) continue;
                edits.Add((body.OpenBraceToken.Span.End, 1, $"using var __dbgFrame = global::Codebound.Dbg.Enter(\"{name}\", {LineOf(node.SpanStart)});"));
            }
            // apply from the end; at the same position the higher pri is applied last so it ends up first
            var sb = new StringBuilder(text.ToString());
            foreach (var e in edits.OrderByDescending(e => e.pos).ThenBy(e => e.pri)) sb.Insert(e.pos, e.text);
            return sb.ToString();
        }

        /// <summary>Locals and parameters visible at the statement that are certainly assigned (declared earlier with a value).</summary>
        static string Visible(SemanticModel model, StatementSyntax st)
        {
            var parts = new List<string>();
            var seen = new HashSet<string>();
            foreach (var s in model.LookupSymbols(st.SpanStart))
            {
                ITypeSymbol type;
                if (s is ILocalSymbol l)
                {
                    if (l.IsRef || l.IsFixed) continue;
                    var decl = l.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                    if (decl == null || decl.SpanStart >= st.SpanStart) continue;
                    if (!(decl is ForEachStatementSyntax) && decl.Span.Contains(st.SpanStart)) continue;   // e.g. inside its own initializer's lambda
                    bool assigned = decl is VariableDeclaratorSyntax vd ? vd.Initializer != null
                                  : decl is ForEachStatementSyntax || decl is CatchDeclarationSyntax;
                    if (!assigned) continue;
                    type = l.Type;
                }
                else if (s is IParameterSymbol p) { if (p.RefKind == RefKind.Out || p.RefKind == RefKind.Ref) continue; type = p.Type; }
                else continue;
                if (type == null || type.IsRefLikeType || type is IPointerTypeSymbol || !seen.Add(s.Name) || s.Name.StartsWith("__")) continue;
                parts.Add($"\"{s.Name}\", (object){s.Name}");
            }
            return string.Join(", ", parts);
        }
    }
}
