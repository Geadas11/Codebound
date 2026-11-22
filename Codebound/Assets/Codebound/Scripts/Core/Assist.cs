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
            if (msg.Contains("no_power") || msg.Contains("no power")) return Loc.T("hint.1", "The machine has no electricity. Is a Coal Generator with coal (or a chain of Power Poles) close enough?");
            if (msg.Contains("no_fuel")) return Loc.T("hint.2", "Furnaces burn coal. Put coal in its fuel slot (E on the furnace) or route a coal drill into it.");
            if (msg.Contains("no_input")) return Loc.T("hint.3", "The furnace input is empty. What should feed it — you, or a drill facing it?");
            if (msg.Contains("output_full")) return Loc.T("hint.4", "The output is full. Empty it, or place a chest/furnace in front of the machine (the side it faces).");
            if (msg.Contains("depleted")) return Loc.T("hint.5", "No ore left under this drill. Time to explore for a new deposit.");
            if (msg.Contains("busy")) return Loc.T("hint.6", "That machine is already doing something. Each machine runs one action at a time.");
            if (msg.Contains("CPU watchdog")) return Loc.T("hint.7", "Your loop never calls the world API or sleep(). Every loop that waits should call sleep() or a machine action.");
            if (msg.Contains("Out of memory")) return Loc.T("hint.8", "The program keeps growing data (lists, arrays, objects) without limit. What keeps adding items?");
            if (msg.Contains("Sandbox:")) return Loc.T("hint.9", "Codebound computers only allow the game API and the language's basics (no files, network, reflection or threads).");
            if (msg.Contains("only system headers")) return Loc.T("hint.10", "Include headers with <...> (like <stdio.h>), or your own files with \"name.h\" after creating them in the Files tab.");
            switch (lang)
            {
                case "python":
                    if (msg.Contains("NameError")) return Loc.T("hint.11", "A name is unknown. Check spelling and case, and that it is defined before this line. Is the machine within 8 cells?");
                    if (msg.Contains("IndentationError") || msg.Contains("unexpected indent")) return Loc.T("hint.12", "Python uses indentation for blocks: the lines inside an if/for/def must be indented the same amount.");
                    if (msg.Contains("SyntaxError")) return Loc.T("hint.13", "This line isn't valid Python: look for a missing colon after if/for/def, or an unclosed bracket or quote.");
                    if (msg.Contains("NoneType")) return Loc.T("hint.14", "Something is None — e.g. no drill in range, so `drill` is None. Check `if drill:` before using it.");
                    if (msg.Contains("TypeError")) return Loc.T("hint.15", "A value has the wrong type for this operation, e.g. adding a number to a string. Convert it with int() or str().");
                    if (msg.Contains("ImportError") || msg.Contains("not available")) return Loc.T("hint.16", "Only a few safe modules can be imported (math, random, json, collections...) plus your own files from the Files tab.");
                    break;
                case "php":
                    if (msg.Contains("undefined function")) return Loc.T("hint.17", "That function does not exist here. Check its name; for machines use $drill->mine() style methods.");
                    if (msg.Contains("Undefined variable")) return Loc.T("hint.18", "PHP variables start with $ and must be set before use. Inside a function, globals need `global $name;`.");
                    if (msg.Contains("ParseError")) return Loc.T("hint.19", "This line isn't valid PHP: look for a missing semicolon, $ or closing bracket just before it.");
                    if (msg.Contains("on null")) return Loc.T("hint.20", "Something is null — e.g. no drill in range, so $drill is null. Check `if ($drill)` before using it.");
                    break;
                case "csharp":
                    if (msg.Contains("CS0103")) return Loc.T("hint.21", "A name doesn't exist in this context. Check spelling and case: C# is case-sensitive (Drill, not drill).");
                    if (msg.Contains("CS1002")) return Loc.T("hint.22", "A statement must end with a semicolon (;).");
                    if (msg.Contains("CS0029") || msg.Contains("CS0266")) return Loc.T("hint.23", "Types don't match. JsonNode results need a cast, e.g. (string)s[\"status\"] or (double)f[\"heat\"].");
                    if (msg.Contains("CS8803")) return Loc.T("hint.24", "Top-level statements must come before class declarations in the main file — or move classes to another file.");
                    if (msg.Contains("NullReferenceException")) return Loc.T("hint.25", "Something is null — e.g. no drill in range, so Drill is null. Check `if (Drill != null)` first.");
                    if (msg.Contains("CS")) return Loc.T("hint.26", "The compiler rejected this line. Read the message: it names what C# expected.");
                    break;
                case "c":
                case "cpp":
                    if (msg.Contains("undeclared identifier") || msg.Contains("was not declared")) return Loc.T("hint.27", "This name isn't declared. C needs a declaration before use — include the right header or declare the variable.");
                    if (msg.Contains("expected ';'")) return Loc.T("hint.28", "A statement must end with a semicolon (;).");
                    if (msg.Contains("implicit declaration")) return Loc.T("hint.29", "Calling a function C doesn't know yet: include its header (e.g. <stdio.h> for printf, <codebound.h> for the game API).");
                    if (msg.Contains("RuntimeError") || msg.Contains("out of bounds")) return Loc.T("hint.30", "The program read or wrote memory it doesn't own — check array indexes and pointers.");
                    if (msg.Contains("error:")) return Loc.T("hint.31", "The compiler rejected this line. The first error is usually the real one: fix it and compile again.");
                    break;
                case "sql":
                    if (msg.Contains("no such table")) return Loc.T("hint.32", "That table doesn't exist. Game tables: machines, contents, energy, events, game, creatures; act by inserting into commands or hud.");
                    if (msg.Contains("no such column")) return Loc.T("hint.33", "That column doesn't exist. Run SELECT * FROM <table> LIMIT 1 to see the columns.");
                    if (msg.Contains("syntax error")) return Loc.T("hint.34", "This statement isn't valid SQL near that word: check commas, quotes and keywords.");
                    break;
                default:   // javascript, typescript
                    if (msg.Contains("is not defined")) return Loc.T("hint.35", "A name is unknown. Check spelling and case: JavaScript is case-sensitive. Is the machine within 8 cells?");
                    if (msg.Contains("is not a function")) return Loc.T("hint.36", "You called something that isn't a function. Check the API tab for the exact names.");
                    if (msg.Contains("SyntaxError")) return Loc.T("hint.37", "This line isn't valid code: look for a missing bracket, quote or parenthesis just before it.");
                    if (msg.Contains("Cannot read properties of null") || msg.Contains("Cannot read properties of undefined")) return Loc.T("hint.38", "Something is null — e.g. no drill in range, so `drill` is null. Check before using it.");
                    if (msg.Contains("no file")) return Loc.T("hint.39", "require() loads this computer's own files: create it in the Files tab first.");
                    break;
            }
            if (msg.Contains("not found or failed to start")) return Loc.T("hint.40", "This language's runtime is missing from this installation.");
            return null;
        }

        /// <summary>Guided mode: what to do next, from what the computer sees.</summary>
        public static string Tip(bool powered, int machinesInRange, bool everRan, bool running, string lastError)
        {
            if (!powered) return Loc.T("tip.1", "Next: power this computer — build a Coal Generator (with coal) or Solar Panel next to it, or connect it with Power Poles.");
            if (machinesInRange == 0) return Loc.T("tip.2", "Next: build a machine within 8 cells of this computer (an Electric Drill on ore is a good start). Programs only reach nearby machines.");
            if (running) return Loc.T("tip.3", "Tip: watch the Console. Click a line number to set a breakpoint, then use Debug to pause there and look at your variables.");
            if (!everRan) return Loc.T("tip.4", "Next: pick an example from the buttons above, read it, and press Run. Then change one thing and run it again.");
            if (!string.IsNullOrEmpty(lastError)) return Loc.T("tip.5", "Next: read the error and the hint below, fix that line (it is highlighted), and press Run again.");
            return Loc.T("tip.6", "Tip: try another example, split your code into files (Files tab), or test one line in the Terminal tab.");
        }
    }

    /// <summary>
    /// The computer's terminal: shell commands over its files and programs. Any other input is run as a one-off snippet
    /// in the computer's language (the saved program is not changed). The Game layer performs the returned action.
    /// </summary>
    public static class Terminal
    {
        public enum Action { None, Run, Debug, Stop, Snippet, Clear }

        public static string Help =>
            "help · ls · cat <file> · new <file> · rm <file> · run · debug · stop · status · lang <language> · watch <path> · unwatch <path> · clear\n" +
            Loc.T("term.help", "anything else runs once as code in this computer's language, e.g.  drill.mine()   (JavaScript)");

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
                    sb.Append("\n" + Loc.T("term.used", "{0}/{1} KB used", (cs.TotalCodeBytes / 1024f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture), cs.MaxCodeBytes / 1024));
                    return (Action.None, sb.ToString(), null);
                }
                case "cat":
                    if (arg == "main" || arg == "") return (Action.None, cs.Code, null);
                    return cs.Files.TryGetValue(arg, out var src) ? (Action.None, src, null) : (Action.None, Loc.T("term.noFileLs", "no file '{0}' (ls lists them)", arg), null);
                case "new":
                    if (!ComputerState.ValidFileName(arg)) return (Action.None, Loc.T("term.badName", "file names: letters, digits and _, with an extension, e.g. utils.js"), null);
                    if (cs.Files.ContainsKey(arg)) return (Action.None, Loc.T("term.exists", "'{0}' already exists", arg), null);
                    if (cs.Files.Count >= ComputerState.MaxFiles) return (Action.None, Loc.T("term.maxFiles", "at most {0} extra files", ComputerState.MaxFiles), null);
                    cs.Files[arg] = "";
                    return (Action.None, Loc.T("term.created", "created {0}", arg), null);
                case "rm":
                    return cs.Files.Remove(arg) ? (Action.None, Loc.T("term.removed", "removed {0}", arg), null) : (Action.None, Loc.T("term.noFile", "no file '{0}'", arg), null);
                case "run": return running ? (Action.None, Loc.T("term.alreadyRunning", "already running (stop first)"), null) : (Action.Run, null, null);
                case "debug": return running ? (Action.None, Loc.T("term.alreadyRunning", "already running (stop first)"), null) : (Action.Debug, null, null);
                case "stop": return running ? (Action.Stop, null, null) : (Action.None, Loc.T("term.notRunning", "nothing is running"), null);
                case "status":
                {
                    var last = cs.History.LastOrDefault();
                    return (Action.None, Loc.T(running ? "term.statusRunning" : "term.statusStopped", running ? "running · {0} · {1} file(s)" : "stopped · {0} · {1} file(s)", cs.Language, cs.Files.Count + 1) +
                                         (last != null ? " · " + Loc.T("term.lastRun", "last run {0}: {1}", last.When, Loc.T("ed.result." + last.Result, last.Result)) + (last.Message != null ? " — " + last.Message : "") : ""), null);
                }
                case "lang":
                    if (Array.IndexOf(ComputerState.Languages, arg) < 0) return (Action.None, Loc.T("term.languages", "languages: {0}", string.Join(", ", ComputerState.Languages)), null);
                    if (running) return (Action.None, Loc.T("term.stopFirst", "stop the program first"), null);
                    cs.Language = arg;
                    return (Action.None, Loc.T("term.language", "language: {0}", arg), null);
                case "watch":
                    if (arg.Length == 0) return (Action.None, cs.Watches.Count == 0 ? Loc.T("term.noWatches", "no watches") : string.Join("\n", cs.Watches), null);
                    if (!cs.Watches.Contains(arg)) cs.Watches.Add(arg);
                    return (Action.None, Loc.T("term.watching", "watching {0}", arg), null);
                case "unwatch": return (Action.None, cs.Watches.Remove(arg) ? Loc.T("term.removed", "removed {0}", arg) : Loc.T("term.notWatched", "not watched: {0}", arg), null);
                case "clear": return (Action.Clear, null, null);
            }
            if (running) return (Action.None, Loc.T("term.runningSnippet", "a program is running — stop it before running a snippet"), null);
            string snippet = Snippet(cs.Language, line);
            return snippet == null ? (Action.None, Loc.T("term.noSnippet", "one-line snippets are not available for {0} (it is compiled): use Run", cs.Language), null)
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
