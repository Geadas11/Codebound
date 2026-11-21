using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codebound.Core
{
    /// <summary>
    /// Help for players who are learning, at three levels (Options → Coding help):
    /// Guided — examples, an always-visible "what next?" tip and an explanation for every error;
    /// Normal — examples and error explanations; Advanced — none of it (the API reference stays one tab away).
    /// </summary>
    public static class Assist
    {
        public static readonly string[] Levels = { "guided", "normal", "advanced" };

        /// <summary>A plain-language explanation of an error or warning, for the program's language; null if none fits.</summary>
        public static string Hint(string lang, string msg)
        {
            if (string.IsNullOrEmpty(msg)) return null;
            // the game world: same for every language
            if (msg.Contains("no_power") || msg.Contains("no power")) return "The machine has no electricity. Is a Coal Generator with coal (or a chain of Power Poles) close enough?";
            if (msg.Contains("no_fuel")) return "Furnaces burn coal. Put coal in its fuel slot (E on the furnace) or route a coal drill into it.";
            if (msg.Contains("no_input")) return "The furnace input is empty. What should feed it — you, or a drill facing it?";
            if (msg.Contains("output_full")) return "The output is full. Empty it, or place a chest/furnace in front of the machine (the side it faces).";
            if (msg.Contains("depleted")) return "No ore left under this drill. Time to explore for a new deposit.";
            if (msg.Contains("busy")) return "That machine is already doing something. Each machine runs one action at a time.";
            if (msg.Contains("CPU watchdog")) return "Your loop never calls the world API or sleep(). Every loop that waits should call sleep() or a machine action.";
            if (msg.Contains("Out of memory")) return "The program keeps growing data (lists, arrays, objects) without limit. What keeps adding items?";
            if (msg.Contains("Sandbox:")) return "Codebound computers only allow the game API and the language's basics (no files, network, reflection or threads).";
            if (msg.Contains("only system headers")) return "Include headers with <...> (like <stdio.h>), or your own files with \"name.h\" after creating them in the Files tab.";
            switch (lang)
            {
                case "python":
                    if (msg.Contains("NameError")) return "A name is unknown. Check spelling and case, and that it is defined before this line. Is the machine within 8 cells?";
                    if (msg.Contains("IndentationError") || msg.Contains("unexpected indent")) return "Python uses indentation for blocks: the lines inside an if/for/def must be indented the same amount.";
                    if (msg.Contains("SyntaxError")) return "This line isn't valid Python: look for a missing colon after if/for/def, or an unclosed bracket or quote.";
                    if (msg.Contains("NoneType")) return "Something is None — e.g. no drill in range, so `drill` is None. Check `if drill:` before using it.";
                    if (msg.Contains("TypeError")) return "A value has the wrong type for this operation, e.g. adding a number to a string. Convert it with int() or str().";
                    if (msg.Contains("ImportError") || msg.Contains("not available")) return "Only a few safe modules can be imported (math, random, json, collections...) plus your own files from the Files tab.";
                    break;
                case "php":
                    if (msg.Contains("undefined function")) return "That function does not exist here. Check its name; for machines use $drill->mine() style methods.";
                    if (msg.Contains("Undefined variable")) return "PHP variables start with $ and must be set before use. Inside a function, globals need `global $name;`.";
                    if (msg.Contains("ParseError")) return "This line isn't valid PHP: look for a missing semicolon, $ or closing bracket just before it.";
                    if (msg.Contains("on null")) return "Something is null — e.g. no drill in range, so $drill is null. Check `if ($drill)` before using it.";
                    break;
                case "csharp":
                    if (msg.Contains("CS0103")) return "A name doesn't exist in this context. Check spelling and case: C# is case-sensitive (Drill, not drill).";
                    if (msg.Contains("CS1002")) return "A statement must end with a semicolon (;).";
                    if (msg.Contains("CS0029") || msg.Contains("CS0266")) return "Types don't match. JsonNode results need a cast, e.g. (string)s[\"status\"] or (double)f[\"heat\"].";
                    if (msg.Contains("CS8803")) return "Top-level statements must come before class declarations in the main file — or move classes to another file.";
                    if (msg.Contains("NullReferenceException")) return "Something is null — e.g. no drill in range, so Drill is null. Check `if (Drill != null)` first.";
                    if (msg.Contains("CS")) return "The compiler rejected this line. Read the message: it names what C# expected.";
                    break;
                case "c":
                case "cpp":
                    if (msg.Contains("undeclared identifier") || msg.Contains("was not declared")) return "This name isn't declared. C needs a declaration before use — include the right header or declare the variable.";
                    if (msg.Contains("expected ';'")) return "A statement must end with a semicolon (;).";
                    if (msg.Contains("implicit declaration")) return "Calling a function C doesn't know yet: include its header (e.g. <stdio.h> for printf, <codebound.h> for the game API).";
                    if (msg.Contains("RuntimeError") || msg.Contains("out of bounds")) return "The program read or wrote memory it doesn't own — check array indexes and pointers.";
                    if (msg.Contains("error:")) return "The compiler rejected this line. The first error is usually the real one: fix it and compile again.";
                    break;
                case "sql":
                    if (msg.Contains("no such table")) return "That table doesn't exist. Game tables: machines, contents, energy, events, game, creatures; act by inserting into commands or hud.";
                    if (msg.Contains("no such column")) return "That column doesn't exist. Run SELECT * FROM <table> LIMIT 1 to see the columns.";
                    if (msg.Contains("syntax error")) return "This statement isn't valid SQL near that word: check commas, quotes and keywords.";
                    break;
                default:   // javascript, typescript
                    if (msg.Contains("is not defined")) return "A name is unknown. Check spelling and case: JavaScript is case-sensitive. Is the machine within 8 cells?";
                    if (msg.Contains("is not a function")) return "You called something that isn't a function. Check the API tab for the exact names.";
                    if (msg.Contains("SyntaxError")) return "This line isn't valid code: look for a missing bracket, quote or parenthesis just before it.";
                    if (msg.Contains("Cannot read properties of null") || msg.Contains("Cannot read properties of undefined")) return "Something is null — e.g. no drill in range, so `drill` is null. Check before using it.";
                    if (msg.Contains("no file")) return "require() loads this computer's own files: create it in the Files tab first.";
                    break;
            }
            if (msg.Contains("not found or failed to start")) return "This language's runtime is missing from this installation.";
            return null;
        }

        /// <summary>Guided mode: what to do next, from what the computer sees.</summary>
        public static string Tip(bool powered, int machinesInRange, bool everRan, bool running, string lastError)
        {
            if (!powered) return "Next: power this computer — build a Coal Generator (with coal) or Solar Panel next to it, or connect it with Power Poles.";
            if (machinesInRange == 0) return "Next: build a machine within 8 cells of this computer (an Electric Drill on ore is a good start). Programs only reach nearby machines.";
            if (running) return "Tip: watch the Console. Click a line number to set a breakpoint, then use Debug to pause there and look at your variables.";
            if (!everRan) return "Next: pick an example from the buttons above, read it, and press Run. Then change one thing and run it again.";
            if (!string.IsNullOrEmpty(lastError)) return "Next: read the error and the hint below, fix that line (it is highlighted), and press Run again.";
            return "Tip: try another example, split your code into files (Files tab), or test one line in the Terminal tab.";
        }
    }

    /// <summary>
    /// The computer's terminal: shell commands over its files and programs. Any other input is run as a one-off snippet
    /// in the computer's language (the saved program is not changed). The Game layer performs the returned action.
    /// </summary>
    public static class Terminal
    {
        public enum Action { None, Run, Debug, Stop, Snippet, Clear }

        public const string Help =
            "help · ls · cat <file> · new <file> · rm <file> · run · debug · stop · status · lang <language> · watch <path> · unwatch <path> · clear\n" +
            "anything else runs once as code in this computer's language, e.g.  drill.mine()   (JavaScript)";

        public static (Action action, string output, string snippet) Execute(ComputerState cs, string input, bool running)
        {
            string line = (input ?? "").Trim();
            if (line.Length == 0) return (Action.None, null, null);
            int sp = line.IndexOf(' ');
            string cmd = (sp < 0 ? line : line.Substring(0, sp)).ToLowerInvariant(), arg = sp < 0 ? "" : line.Substring(sp + 1).Trim();
            switch (cmd)
            {
                case "help": return (Action.None, Help, null);
                case "ls":
                {
                    var sb = new StringBuilder($"main ({cs.Language}) {Encoding.UTF8.GetByteCount(cs.Code ?? "")} B");
                    foreach (var kv in cs.Files) sb.Append($"\n{kv.Key} {Encoding.UTF8.GetByteCount(kv.Value)} B");
                    sb.Append($"\n{cs.TotalCodeBytes / 1024f:0.0}/{cs.MaxCodeBytes / 1024} KB used");
                    return (Action.None, sb.ToString(), null);
                }
                case "cat":
                    if (arg == "main" || arg == "") return (Action.None, cs.Code, null);
                    return cs.Files.TryGetValue(arg, out var src) ? (Action.None, src, null) : (Action.None, $"no file '{arg}' (ls lists them)", null);
                case "new":
                    if (!ComputerState.ValidFileName(arg)) return (Action.None, "file names: letters, digits and _, with an extension, e.g. utils.js", null);
                    if (cs.Files.ContainsKey(arg)) return (Action.None, $"'{arg}' already exists", null);
                    if (cs.Files.Count >= ComputerState.MaxFiles) return (Action.None, $"at most {ComputerState.MaxFiles} extra files", null);
                    cs.Files[arg] = "";
                    return (Action.None, $"created {arg}", null);
                case "rm":
                    return cs.Files.Remove(arg) ? (Action.None, $"removed {arg}", null) : (Action.None, $"no file '{arg}'", null);
                case "run": return running ? (Action.None, "already running (stop first)", null) : (Action.Run, null, null);
                case "debug": return running ? (Action.None, "already running (stop first)", null) : (Action.Debug, null, null);
                case "stop": return running ? (Action.Stop, null, null) : (Action.None, "nothing is running", null);
                case "status":
                {
                    var last = cs.History.LastOrDefault();
                    return (Action.None, $"{(running ? "running" : "stopped")} · {cs.Language} · {cs.Files.Count + 1} file(s)" +
                                         (last != null ? $" · last run {last.When}: {last.Result}{(last.Message != null ? " — " + last.Message : "")}" : ""), null);
                }
                case "lang":
                    if (Array.IndexOf(ComputerState.Languages, arg) < 0) return (Action.None, "languages: " + string.Join(", ", ComputerState.Languages), null);
                    if (running) return (Action.None, "stop the program first", null);
                    cs.Language = arg;
                    return (Action.None, "language: " + arg, null);
                case "watch":
                    if (arg.Length == 0) return (Action.None, cs.Watches.Count == 0 ? "no watches" : string.Join("\n", cs.Watches), null);
                    if (!cs.Watches.Contains(arg)) cs.Watches.Add(arg);
                    return (Action.None, "watching " + arg, null);
                case "unwatch": return (Action.None, cs.Watches.Remove(arg) ? "removed " + arg : "not watched: " + arg, null);
                case "clear": return (Action.Clear, null, null);
            }
            if (running) return (Action.None, "a program is running — stop it before running a snippet", null);
            string snippet = Snippet(cs.Language, line);
            return snippet == null ? (Action.None, $"one-line snippets are not available for {cs.Language} (it is compiled): use Run", null)
                                   : (Action.Snippet, null, snippet);
        }

        /// <summary>Wraps one line so its value is shown, when it looks like an expression.</summary>
        public static string Snippet(string lang, string line)
        {
            bool statement = line.EndsWith(";") || line.EndsWith("}") || line.Contains(" = ") || line.StartsWith("for ") || line.StartsWith("if ")
                             || line.StartsWith("while ") || line.StartsWith("let ") || line.StartsWith("const ") || line.StartsWith("var ") || line.StartsWith("import ");
            switch (lang)
            {
                case "javascript": case "typescript": return statement ? line : $"log({line});";
                case "python": return statement || line.StartsWith("print(") ? line : $"print({line})";
                case "php": return statement ? line : $"echo json_encode({line}), \"\\n\";";
                case "csharp": return statement ? line : $"Log({line});";
                case "sql": return "-- @once\n" + line;
                default: return null;   // C and C++ are compiled: no one-liners
            }
        }
    }
}
