using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Codebound
{
    /// <summary>Line-based JSON protocol with Unity (same as the Python/PHP runtimes). The streams are private to the host.</summary>
    static class Proto
    {
        static readonly Stream Out = Console.OpenStandardOutput();
        static readonly StreamReader In = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false));
        static readonly object Lock = new object();
        internal static JsonNode Id;   // echoed back exactly as Unity sent it (a number)
        internal static int RamMb = 32;
        static int logCount; static long logSince = Stopwatch.GetTimestamp();
        static long statsAt = Stopwatch.GetTimestamp(); static TimeSpan cpuAt;

        internal static void Send(JsonObject m)
        {
            var b = Encoding.UTF8.GetBytes(m.ToJsonString() + "\n");
            lock (Lock) { Out.Write(b, 0, b.Length); Out.Flush(); }
        }

        internal static JsonObject Read()
        {
            string line = In.ReadLine();
            if (line == null) Environment.Exit(0);   // Unity went away: never outlive the game
            return JsonNode.Parse(line) as JsonObject ?? new JsonObject();
        }

        internal static void Log(string msg)
        {
            if (Stopwatch.GetElapsedTime(logSince).TotalSeconds >= 1) { logCount = 0; logSince = Stopwatch.GetTimestamp(); }
            if (++logCount <= 50) Send(new JsonObject { ["t"] = "log", ["id"] = Id?.DeepClone(), ["msg"] = msg.Length > 2000 ? msg.Substring(0, 2000) : msg });
            else if (logCount == 51) Send(new JsonObject { ["t"] = "log", ["id"] = Id?.DeepClone(), ["msg"] = "[runtime] log rate limit reached (50 lines/s), dropping output" });
        }

        static void Stats()
        {
            if (Stopwatch.GetElapsedTime(statsAt).TotalSeconds < 1) return;
            var cpu = Process.GetCurrentProcess().UserProcessorTime;
            int pct = (int)Math.Min(100, Math.Round(100 * (cpu - cpuAt).TotalSeconds / Stopwatch.GetElapsedTime(statsAt).TotalSeconds));
            statsAt = Stopwatch.GetTimestamp(); cpuAt = cpu;
            double heap = Math.Round(GC.GetTotalMemory(false) / 1048576.0, 1);
            Send(new JsonObject { ["t"] = "stats", ["id"] = Id?.DeepClone(), ["cpu"] = pct, ["heapMb"] = heap, ["ramMb"] = RamMb });
            if (heap > RamMb && GC.GetTotalMemory(true) / 1048576.0 > RamMb) throw new OutOfMemoryException($"program exceeded {RamMb} MB RAM");
        }

        static readonly Queue<JsonObject> events = new Queue<JsonObject>();
        static bool dispatching;
        internal static readonly Dictionary<string, List<Action<JsonNode>>> Handlers = new Dictionary<string, List<Action<JsonNode>>>();

        internal static JsonNode Call(string obj, string fn, params object[] args)
        {
            Stats();
            var a = new JsonArray();
            foreach (var x in args) a.Add(x == null ? null : x is JsonNode n ? n.DeepClone() : JsonValue.Create(x is int i ? (double)i : x is float f ? (double)f : x));
            Send(new JsonObject { ["t"] = "call", ["id"] = Id?.DeepClone(), ["obj"] = obj, ["fn"] = fn, ["args"] = a });
            var m = Read();
            if (m["warn"] is JsonNode w && w.GetValueKind() == JsonValueKind.String) Log("[warn] " + w.GetValue<string>());
            if (m["events"] is JsonArray ev) Dispatch(ev);
            if (m["error"] is JsonNode e && e.GetValueKind() == JsonValueKind.String) throw new InvalidOperationException(e.GetValue<string>());
            return m["value"]?.DeepClone();
        }

        static void Dispatch(JsonArray ev)
        {
            foreach (var e in ev) if (e is JsonObject o) events.Enqueue(o);
            if (dispatching) return;
            dispatching = true;
            try
            {
                while (events.Count > 0)
                {
                    var e = events.Dequeue();
                    string name = e["name"]?.GetValue<string>() ?? "";
                    if (Handlers.TryGetValue(name, out var hs)) foreach (var h in hs.ToList()) h(e["data"]?.DeepClone());
                }
            }
            finally { dispatching = false; }
        }
    }

    // ------------------------------------------------------------------ the game API, visible to player code
    public sealed class Machine
    {
        static readonly Dictionary<string, string[]> Ops = new Dictionary<string, string[]>
        {
            ["drill"] = new[] { "mine" }, ["furnace"] = new[] { "smelt" }, ["conveyor"] = new[] { "start", "stop", "set_filter" },
            ["inserter"] = new[] { "start", "stop", "set_filter" }, ["greenhouse"] = new[] { "plant", "water", "harvest", "plots" },
            ["assembler"] = new[] { "craft" }, ["lab"] = new[] { "research", "techs" }, ["turret"] = new[] { "targets", "fire" },
            ["launchpad"] = new[] { "launch", "throttle", "flight", "planets", "travel" },
            ["server"] = new[] { "get", "set", "keys", "delete" }, ["storage_hub"] = new[] { "totals", "find" },
        };
        public string Id { get; }
        public string Type { get; }
        public string Name { get; }
        internal Machine(JsonNode info) { Id = info["id"]?.ToString(); Type = info["type"]?.ToString(); Name = info["name"]?.ToString(); }
        internal static Machine From(JsonNode n) => n == null ? null : new Machine(n);

        public JsonNode Status() => Proto.Call(Id, "status");
        public JsonNode Contents() => Proto.Call(Id, "contents");
        /// <summary>Any operation of this machine type, e.g. drill.Call("mine").</summary>
        public JsonNode Call(string op, params object[] args)
        {
            if (!Ops.TryGetValue(Type, out var ops) || !ops.Contains(op)) throw new InvalidOperationException($"'{Type}' has no function '{op}'");
            return Proto.Call(Id, op, args);
        }
        public JsonNode Mine() => Call("mine");
        public JsonNode Smelt() => Call("smelt");
        public JsonNode Craft(string item) => Call("craft", item);
        public JsonNode Research(string tech) => Call("research", tech);
        public JsonNode Techs() => Call("techs");
        public JsonNode Start() => Call("start");
        public JsonNode Stop() => Call("stop");
        public JsonNode SetFilter(string item) => Call("set_filter", item);
        public JsonNode Plant(int plot = -1, string species = null) => Call("plant", plot < 0 ? null : (object)plot, species);
        public JsonNode Water(int plot = -1) => plot < 0 ? Call("water") : Call("water", plot);
        public JsonNode Harvest(int plot = -1) => plot < 0 ? Call("harvest") : Call("harvest", plot);
        public JsonNode Plots() => Call("plots");
        public JsonNode Targets() => Call("targets");
        public JsonNode Fire(int creatureId) => Call("fire", creatureId);
        public JsonNode Launch() => Call("launch");
        public JsonNode Throttle(double x) => Call("throttle", x);
        public JsonNode Flight() => Call("flight");
        public JsonNode Planets() => Call("planets");
        public JsonNode Travel(string planet) => Call("travel", planet);
        public override string ToString() => $"<{Type} {Id}>";
    }

    public sealed class MachineApi
    {
        public List<Machine> List() => (Proto.Call("machines", "list") as JsonArray ?? new JsonArray()).Select(Machine.From).ToList();
        public Machine Find(string type) => Machine.From(Proto.Call("machines", "find", type));
        public Machine Get(string id) => Machine.From(Proto.Call("machines", "get", id));
    }

    public sealed class ComputerApi
    {
        public string Name { get; internal set; }
        public int Cps { get; internal set; }
        public void On(string name, Action<JsonNode> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (!Proto.Handlers.ContainsKey(name)) { Proto.Handlers[name] = new List<Action<JsonNode>>(); Proto.Call("computer", "subscribe", name); }
            Proto.Handlers[name].Add(handler);
        }
        public double Time() => Proto.Call("computer", "time")?.GetValue<double>() ?? 0;
        /// <summary>Sends data to every other computer listening to the "message" event (needs Networking).</summary>
        public bool Send(string channel, object data = null) => Proto.Call("computer", "send", channel, data is JsonNode || data is string || data is double || data is int || data is bool || data == null ? data : data.ToString())?.GetValue<bool>() ?? false;
    }

    public sealed class EnergyApi { public bool Available() => Proto.Call("energy", "available")?.GetValue<bool>() ?? false; public JsonNode Stats() => Proto.Call("energy", "stats"); }
    public sealed class WorldApi { public JsonNode Pollution() => Proto.Call("world", "pollution"); public JsonNode Weather() => Proto.Call("world", "weather"); }
    public sealed class HudApi
    {
        public void Set(string key, object value) => Proto.Call("hud", "set", key, value is string || value is bool || value is double || value is int || value is float || value == null ? value : value.ToString());
        public void Notify(string text) => Proto.Call("hud", "notify", text);
        public void Alarm(string text = "ALARM") => Proto.Call("hud", "alarm", text);
    }

    /// <summary>Everything here is in scope in a player's program (global using static Codebound.Game).</summary>
    public static class Game
    {
        public static MachineApi Machines { get; } = new MachineApi();
        public static ComputerApi Computer { get; } = new ComputerApi();
        public static EnergyApi Energy { get; } = new EnergyApi();
        public static WorldApi World { get; } = new WorldApi();
        public static HudApi Hud { get; } = new HudApi();
        public static Machine Drill { get; internal set; }
        public static Machine Furnace { get; internal set; }
        public static Machine Generator { get; internal set; }
        public static Machine Chest { get; internal set; }
        public static Machine Conveyor { get; internal set; }
        public static Machine Inserter { get; internal set; }
        public static Machine Greenhouse { get; internal set; }
        public static Machine Assembler { get; internal set; }
        public static Machine Lab { get; internal set; }
        public static Machine Turret { get; internal set; }
        public static Machine LaunchPad { get; internal set; }

        public static void Log(params object[] parts) => Proto.Log(string.Join(" ", parts.Select(p => p switch { null => "null", bool b => b ? "true" : "false", JsonValue v when v.GetValueKind() == JsonValueKind.String => v.GetValue<string>(), JsonNode n => n.ToJsonString(), _ => Convert.ToString(p, System.Globalization.CultureInfo.InvariantCulture) })));
        /// <summary>Waits in game time without using CPU.</summary>
        public static void Sleep(double seconds) => Proto.Call("computer", "sleep", seconds);
    }

    /// <summary>Console output from player code (it cannot name Console, but library code may write to it) goes to the log.</summary>
    sealed class LogWriter : TextWriter
    {
        readonly StringBuilder line = new StringBuilder();
        public override Encoding Encoding => Encoding.UTF8;
        public override void Write(char c) { if (c == '\n') { Proto.Log(line.ToString().TrimEnd('\r')); line.Clear(); } else line.Append(c); }
    }

    // ------------------------------------------------------------------ main
    static class Host
    {
        const string FileName = "computer.cs";

        static void Exit(string reason, string msg = null, int? line = null, string file = null)
        {
            Proto.Send(new JsonObject { ["t"] = "exit", ["id"] = Proto.Id?.DeepClone(), ["reason"] = reason, ["msg"] = msg, ["line"] = line, ["file"] = file });
            Environment.Exit(0);
        }

        static IEnumerable<MetadataReference> References()
        {
            var keep = new HashSet<string> { "System.Private.CoreLib", "System.Runtime", "System.Collections", "System.Linq", "System.Text.RegularExpressions",
                                             "System.Text.Json", "System.Memory", "System.Runtime.Extensions", "netstandard" };
            foreach (var p in ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator))
                if (keep.Contains(Path.GetFileNameWithoutExtension(p))) yield return MetadataReference.CreateFromFile(p);
            yield return MetadataReference.CreateFromFile(typeof(Game).Assembly.Location);
        }

        static int Main()
        {
            Proto.Send(new JsonObject { ["t"] = "ready", ["version"] = "C# 12 / .NET " + Environment.Version });
            var start = Proto.Read();
            Proto.Id = start["id"]?.DeepClone();
            Proto.RamMb = Math.Max(8, Math.Min(1024, (int)(start["ram"]?.GetValue<double>() ?? 32)));
            string code = start["code"]?.GetValue<string>() ?? "";

            var parse = new CSharpParseOptions(LanguageVersion.CSharp12);
            var tree = CSharpSyntaxTree.ParseText(code, parse, FileName, Encoding.UTF8);
            // the computer's other files: more classes in the same program (only the main file has top-level statements)
            var others = new List<SyntaxTree>();
            if (start["files"] is JsonObject files)
                foreach (var kv in files)
                {
                    string n = kv.Key.EndsWith(".cs") ? kv.Key.Substring(0, kv.Key.Length - 3) : kv.Key;
                    if (System.Text.RegularExpressions.Regex.IsMatch(n, @"^[A-Za-z_]\w*$") && n != "computer")
                        others.Add(CSharpSyntaxTree.ParseText(kv.Value?.GetValue<string>() ?? "", parse, n + ".cs", Encoding.UTF8));
                }
            var usings = CSharpSyntaxTree.ParseText("global using System; global using System.Collections.Generic; global using System.Linq; global using System.Text; " +
                                                    "global using System.Text.Json.Nodes; global using Codebound; global using static Codebound.Game;", parse, "usings.cs", Encoding.UTF8);
            var comp = CSharpCompilation.Create("computer", new[] { tree, usings }.Concat(others), References(),
                new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: false, optimizationLevel: OptimizationLevel.Debug,
                                             nullableContextOptions: NullableContextOptions.Disable));
            var errors = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
            {
                var e = errors[0];
                bool player = e.Location.IsInSource && (e.Location.SourceTree == tree || others.Contains(e.Location.SourceTree));
                int? l = player ? e.Location.GetLineSpan().StartLinePosition.Line + 1 : (int?)null;
                Exit("error", $"{e.Id}: {e.GetMessage(System.Globalization.CultureInfo.InvariantCulture)}" + (errors.Count > 1 ? $" (+{errors.Count - 1} more)" : ""), l,
                     player ? FileOf(e.Location.SourceTree.FilePath) : null);
            }
            foreach (var t in new[] { tree }.Concat(others))
            {
                string violation = Sandbox.Check(comp, t);
                if (violation == null) continue;
                int lineNo = int.TryParse(violation.Substring(5, violation.IndexOf(':') - 5), out var ln) ? ln : 0;
                Exit("error", "Sandbox: " + violation, lineNo > 0 ? lineNo : (int?)null, FileOf(t.FilePath));
            }
            if (start["debug"]?.GetValue<bool>() == true)
            {
                // instrument the main file (checked above in its original form); if that fails to compile, run without the debugger
                var dbgTree = CSharpSyntaxTree.ParseText(Dbg.Instrument(comp, tree), parse, FileName, Encoding.UTF8);
                var dbgComp = comp.ReplaceSyntaxTree(tree, dbgTree);
                if (dbgComp.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error)) Proto.Log("[debug] this program could not be instrumented — running without the debugger");
                else
                {
                    comp = dbgComp; Dbg.On = true;
                    if (start["bps"] is JsonArray bps) foreach (var b in bps) Dbg.Bps.Add((int)b.GetValue<double>());
                    if (start["watches"] is JsonArray ws) Dbg.Watches = ws.Select(x => x?.ToString() ?? "").ToList();
                }
            }

            var pe = new MemoryStream(); var pdb = new MemoryStream();
            EmitResult r = comp.Emit(pe, pdb, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));
            if (!r.Success) Exit("error", "Could not build the program: " + r.Diagnostics.FirstOrDefault(d => d.Severity == DiagnosticSeverity.Error)?.GetMessage());
            pe.Position = 0; pdb.Position = 0;
            var asm = new AssemblyLoadContext("computer").LoadFromStream(pe, pdb);
            comp = null; tree = null; GC.Collect();   // Roslyn's memory is not the program's

            var info = Proto.Call("computer", "info");
            Game.Computer.Name = info?["name"]?.ToString(); Game.Computer.Cps = (int)(info?["cps"]?.GetValue<double>() ?? 20);
            Game.Drill = Machine.From(Proto.Call("machines", "find", "drill")); Game.Furnace = Machine.From(Proto.Call("machines", "find", "furnace"));
            Game.Generator = Machine.From(Proto.Call("machines", "find", "generator")); Game.Chest = Machine.From(Proto.Call("machines", "find", "chest"));
            Game.Conveyor = Machine.From(Proto.Call("machines", "find", "conveyor")); Game.Inserter = Machine.From(Proto.Call("machines", "find", "inserter"));
            Game.Greenhouse = Machine.From(Proto.Call("machines", "find", "greenhouse")); Game.Assembler = Machine.From(Proto.Call("machines", "find", "assembler"));
            Game.Lab = Machine.From(Proto.Call("machines", "find", "lab")); Game.Turret = Machine.From(Proto.Call("machines", "find", "turret"));
            Game.LaunchPad = Machine.From(Proto.Call("machines", "find", "launchpad"));

            Console.SetOut(new LogWriter()); Console.SetError(new LogWriter());
            try
            {
                var entry = asm.EntryPoint;
                entry.Invoke(null, entry.GetParameters().Length == 0 ? null : new object[] { new string[0] });
                while (Proto.Handlers.Count > 0) Proto.Call("computer", "wait_event");   // event mode
                Exit("done");
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null) { Fail(tie.InnerException); }
            catch (Exception e) { Fail(e); }
            return 0;
        }

        static void Fail(Exception e)
        {
            if (e is OutOfMemoryException || e is InsufficientExecutionStackException)
                Exit("error", e is OutOfMemoryException ? $"Out of memory: program exceeded {Proto.RamMb} MB RAM" : "Stack overflow: recursion too deep");
            int? line = null; string file = null;
            foreach (var f in new StackTrace(e, true).GetFrames() ?? new StackFrame[0])
                if (f.GetFileName() is string fn && fn.EndsWith(".cs") && fn != "usings.cs") { line = f.GetFileLineNumber(); file = FileOf(fn); break; }
            Exit("error", $"{e.GetType().Name}: {e.Message}", line, file);
        }

        /// <summary>null for the main file, otherwise the file's name without .cs.</summary>
        static string FileOf(string path)
        {
            string n = System.IO.Path.GetFileName(path ?? "");
            return n == FileName || n.Length == 0 ? null : n.EndsWith(".cs") ? n.Substring(0, n.Length - 3) : n;
        }
    }
}
