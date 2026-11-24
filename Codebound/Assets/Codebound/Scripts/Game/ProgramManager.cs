using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Codebound.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Codebound.Game
{
    /// <summary>
    /// Runs player programs in the out-of-process JavaScript runtime (StreamingAssets/Runtime/js/host.js)
    /// and executes their world-API calls on the main thread. Every call is validated here: range, power,
    /// CPU rate (calls per second of the computer's hardware) and machine rules in Core.Factory.
    /// </summary>
    public sealed class ProgramManager : MonoBehaviour
    {
        sealed class Program
        {
            public PlacedBuilding Computer;
            public ComputerState St;
            public Dictionary<string, object> Pending;   // call waiting to be executed
            public bool AwaitingAction;                  // machine action in progress, reply on completion
            public float NextCallTime, SleepUntil = -1;
            public bool WaitingEvent;
            public readonly HashSet<string> Subs = new HashSet<string>();
            public readonly List<object> Events = new List<object>();
            public float LastTick;
            public bool WasPowered = true;
            public int Calls, CallsPerSecShown; public float CallWindow;
            public bool Debug;
            public int PausedLine;                       // 0 = not paused
            public Dictionary<string, object> PausedVars;
            public bool BreakpointsDirty;
            public RuntimeProcess Proc;                  // Python: one process per program; null = shared JS host
            public double BaselineMb;                    // Python: process memory when the program started
            public float BusySince = -1;
            public float StartedAt = Time.realtimeSinceStartup;
            public int TotalCalls;
            public readonly Dictionary<string, int> CallsByTarget = new Dictionary<string, int>();
            public List<object> PausedStack, PausedWatches;                 // PHP: real time since the program last got an answer (CPU watchdog)
            public readonly HashSet<int> SeenCreatures = new HashSet<int>();
        }

        public struct DebugInfo { public bool Debugging; public int PausedLine; public Dictionary<string, object> Vars; public List<object> Stack, Watches; }

        public struct Stats { public bool Running; public int Cpu; public double HeapMb; public int RamMb; public int CallsPerSec; public string Waiting; }

        GameSession session;
        RuntimeProcess host;   // shared JavaScript/TypeScript runtime (one worker per program inside it)
        readonly ConcurrentQueue<(RuntimeProcess src, string line)> inbox = new ConcurrentQueue<(RuntimeProcess, string)>();
        readonly ConcurrentQueue<string> errors = new ConcurrentQueue<string>();
        readonly Dictionary<int, Program> programs = new Dictionary<int, Program>();
        readonly Dictionary<int, Stats> stats = new Dictionary<int, Stats>();
        readonly List<PlacedBuilding> compromised = new List<PlacedBuilding>();
        public string RuntimeError { get; private set; }

        public void Init(GameSession s)
        {
            session = s;
            s.Grid.Removed += b => { if (programs.ContainsKey(b.InstanceId)) Stop(b); };
            s.Factory.ActionCompleted += OnActionCompleted;
            s.State.Creatures.IntrusionStarted += (target, drone) =>
            {
                foreach (var p in programs.Values)
                    if (p.Computer.DistanceTo(target) <= 12)
                        Emit(p, "intrusion", new Dictionary<string, object> { { "computer", "m" + target.InstanceId }, { "drone", drone.Id }, { "seconds", session.State.Creatures.HackTime((ComputerState)target.State) } });
            };
            if (!Net.IsGuest)   // guests see the host's programs; they never run them locally
                foreach (var b in s.Grid.Buildings)
                    if (b.State is ComputerState cs && cs.Running) StartProgram(b);
        }

        // ------------------------------------------------------------ host process
        public static string RuntimeDir => Path.Combine(Application.streamingAssetsPath, "Runtime", "js");

        /// <summary>Bundled Node next to the scripts if present (release builds), otherwise "node" from PATH (development).</summary>
        public static string NodeExecutable()
        {
            string bundled = Path.Combine(Application.streamingAssetsPath, "Runtime", "node",
                Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor ? "node.exe" : "bin/node");
            return File.Exists(bundled) ? bundled : "node";
        }

        public static string PythonRuntimeDir => Path.Combine(Application.streamingAssetsPath, "Runtime", "py");

        /// <summary>Bundled CPython (builds), then the project's Tools/python (Editor), then "python" from PATH.</summary>
        public static string PythonExecutable()
        {
            bool win = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
            string exe = win ? "python.exe" : "bin/python3";
            string bundled = Path.Combine(Application.streamingAssetsPath, "Runtime", "python", exe);
            if (File.Exists(bundled)) return bundled;
            string tools = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "python", exe));
            return File.Exists(tools) ? tools : (win ? "python" : "python3");
        }

        bool EnsureHost()
        {
            if (host != null && !host.HasExited) return true;
            try
            {
                // Node permission model: read only the runtime folder, no fs writes, no child processes, no addons.
                host = RuntimeProcess.Start(NodeExecutable(),
                    $"--permission \"--allow-fs-read={RuntimeDir}\" --allow-worker --disallow-code-generation-from-strings --no-warnings --max-old-space-size=256 \"{Path.Combine(RuntimeDir, "host.js")}\"",
                    RuntimeDir, inbox, errors);
                RuntimeError = null;
                return true;
            }
            catch (Exception e)
            {
                RuntimeError = "JavaScript runtime (Node.js) not found or failed to start: " + e.Message;
                host = null;
                return false;
            }
        }

        public static string PhpRuntimeDir => Path.Combine(Application.streamingAssetsPath, "Runtime", "php");
        public const float PhpWatchdogSeconds = 3f;

        /// <summary>Bundled PHP (builds) or the project's Tools/php (Editor).</summary>
        public static string PhpExecutable()
        {
            string bundled = Path.Combine(PhpRuntimeDir, "bin", "php.exe");
            return File.Exists(bundled) ? bundled : Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "php", "php.exe"));
        }

        // everything that reaches outside the program: processes, network, files, environment, configuration, real sleeping
        public const string PhpDisabled = "exec,shell_exec,system,passthru,proc_open,popen,proc_close,proc_get_status,proc_terminate,proc_nice,pcntl_exec,dl," +
            "putenv,getenv,mail,fsockopen,pfsockopen,stream_socket_client,stream_socket_server,stream_socket_accept,stream_socket_pair," +
            "fopen,tmpfile,tempnam,file_get_contents,file_put_contents,file,readfile,unlink,rename,copy,mkdir,rmdir,touch,chmod,chown,chgrp,link,symlink," +
            "glob,opendir,scandir,dir,parse_ini_file,highlight_file,show_source,php_strip_whitespace,ini_set,ini_alter,ini_restore,set_include_path," +
            "phpinfo,php_uname,gethostname,get_current_user,getcwd,chdir,error_log,sleep,usleep,time_nanosleep,time_sleep_until,set_time_limit,ignore_user_abort";

        RuntimeProcess StartPhp(ComputerState st)
        {
            try
            {
                string jail = Path.Combine(PhpRuntimeDir, "no-files-here");   // never created: every file access is outside open_basedir
                string args = $"-n -d memory_limit={st.RamMb}M \"-d open_basedir={jail}\" -d disable_functions={PhpDisabled} -d display_errors=stderr " +
                              "-d log_errors=0 -d html_errors=0 -d max_execution_time=0 -d allow_url_fopen=0 -d allow_url_include=0 -d opcache.enable_cli=0 " +
                              $"-f \"{Path.Combine(PhpRuntimeDir, "host.php")}\"";
                var proc = RuntimeProcess.Start(PhpExecutable(), args, PhpRuntimeDir, inbox, errors);
                if (!proc.Confine((ulong)(st.RamMb + 64) * 1048576UL)) st.AddLog("[runtime] warning: OS process limits could not be applied on this platform");
                return proc;
            }
            catch (Exception e)
            {
                Fail(st, "PHP runtime not found or failed to start: " + e.Message);
                return null;
            }
        }

        public static string CsRuntimeDir => Path.Combine(Application.streamingAssetsPath, "Runtime", "csharp");
        static string ToolsDir => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools"));

        /// <summary>C#: Roslyn compiles in a separate .NET process (bundled runtime in builds, Tools/ in the Editor).</summary>
        RuntimeProcess StartCsharp(ComputerState st)
        {
            try
            {
                string dotnet = Path.Combine(CsRuntimeDir, "dotnet", "dotnet.exe"), host = Path.Combine(CsRuntimeDir, "host", "cshost.dll");
                if (!File.Exists(dotnet)) { dotnet = Path.Combine(ToolsDir, "dotnet", "dotnet.exe"); host = Path.Combine(ToolsDir, "cshost", "out", "cshost.dll"); }
                var env = new Dictionary<string, string>
                {
                    // the compiler needs ~200 MB once; the program's own RAM is enforced by the host after compiling
                    { "DOTNET_GCHeapHardLimit", "0x" + ((long)(st.RamMb + 256) * 1048576).ToString("X") },
                    { "DOTNET_CLI_TELEMETRY_OPTOUT", "1" }, { "DOTNET_gcServer", "0" },
                };
                var proc = RuntimeProcess.Start(dotnet, $"\"{host}\"", Path.GetDirectoryName(host), inbox, errors, env);
                if (!proc.Confine((ulong)(st.RamMb + 512) * 1048576UL)) st.AddLog("[runtime] warning: OS process limits could not be applied on this platform");
                return proc;
            }
            catch (Exception e)
            {
                Fail(st, "C# runtime not found or failed to start: " + e.Message);
                return null;
            }
        }

        // ------------------------------------------------------------ C / C++ (clang -> WebAssembly -> the JS host)
        public static string CRuntimeDir => Path.Combine(Application.streamingAssetsPath, "Runtime", "c");
        public const int CompileTimeoutMs = 20000;
        readonly ConcurrentQueue<(Program p, string wasm, string error, int? line, string file, List<string> sources)> compiled = new ConcurrentQueue<(Program, string, string, int?, string, List<string>)>();

        static string CSdkDir()
        {
            string bundled = Path.Combine(CRuntimeDir, "sdk");
            return File.Exists(Path.Combine(bundled, "bin", "clang.exe")) ? bundled : Path.Combine(ToolsDir, "wasi-sdk");
        }

        /// <summary>Compiles on a worker thread; Update() sends the result to the JS host. The source was checked by CSource first.</summary>
        void CompileC(Program prog, string lang, string code, Dictionary<string, string> files, int ramMb, string tempRoot)
        {
            string sdk = CSdkDir(), include = CRuntimeDir;
            System.Threading.Tasks.Task.Run(() =>
            {
                string dir = Path.Combine(tempRoot, "cc-" + Guid.NewGuid().ToString("N"));
                try
                {
                    Directory.CreateDirectory(dir);
                    string src = lang == "c" ? "computer.c" : "computer.cpp";
                    File.WriteAllText(Path.Combine(dir, src), code, new UTF8Encoding(false));
                    var sources = new List<string> { src };
                    foreach (var kv in files)   // names were validated by ComputerState.ValidFileName: no paths
                    {
                        File.WriteAllText(Path.Combine(dir, kv.Key), kv.Value, new UTF8Encoding(false));
                        if (kv.Key.EndsWith(lang == "c" ? ".c" : ".cpp")) sources.Add(kv.Key);
                    }
                    string std = lang == "c" ? "-std=c17" : "--driver-mode=g++ -std=c++17 -fno-exceptions -fno-rtti";
                    long maxMem = Math.Max(4L, ramMb) * 1048576L;
                    string args = $"{std} --target=wasm32-wasip1 \"--sysroot={Path.Combine(sdk, "share", "wasi-sysroot")}\" -O1 \"-isystem{include}\" " +
                                  $"-Wl,--import-memory -Wl,--export-memory -Wl,--initial-memory=4194304 -Wl,--max-memory={maxMem} -Wl,-z,stack-size=524288 " +
                                  $"-gdwarf-4 -gline-tables-only -fdiagnostics-color=never -o computer.wasm {string.Join(" ", sources)}";
                    var (rc, output) = RuntimeProcess.RunConfined(Path.Combine(sdk, "bin", "clang.exe"), args, dir, CompileTimeoutMs, 1536UL * 1048576UL, 4);
                    if (rc != 0)
                    {
                        string msg = rc == -1 ? $"Compiler stopped: took longer than {CompileTimeoutMs / 1000} s" : CleanDiagnostics(output, dir, sdk, include);
                        var m = System.Text.RegularExpressions.Regex.Match(output, @"([A-Za-z0-9_]+\.(c|cpp|h|hpp)):(\d+):\d+: (fatal )?error");
                        compiled.Enqueue((prog, null, msg, m.Success ? int.Parse(m.Groups[3].Value) : (int?)null, m.Success && m.Groups[1].Value != src ? m.Groups[1].Value : null, null));
                    }
                    else
                    {
                        var all = new List<string> { src }; all.AddRange(files.Keys);
                        compiled.Enqueue((prog, Convert.ToBase64String(File.ReadAllBytes(Path.Combine(dir, "computer.wasm"))), null, null, null, all));
                    }
                }
                catch (Exception e) { compiled.Enqueue((prog, null, "C compiler not available: " + e.Message, null, null, null)); }
                finally { try { Directory.Delete(dir, true); } catch { } }
            });
        }

        /// <summary>First error (and its context lines) without local paths: they would show the player's user name.</summary>
        static string CleanDiagnostics(string output, string dir, string sdk, string include)
        {
            string s = output.Replace(dir + Path.DirectorySeparatorChar, "").Replace(dir, "").Replace(sdk, "<sdk>").Replace(include, "<codebound>")
                             .Replace(sdk.Replace('\\', '/'), "<sdk>").Replace(include.Replace('\\', '/'), "<codebound>");
            var lines = s.Replace("\r", "").Split('\n');
            int start = Array.FindIndex(lines, l => l.Contains("error"));
            if (start < 0) return s.Trim().Length > 0 ? s.Trim() : "compile failed";
            return string.Join("\n", lines, start, Math.Min(4, lines.Length - start)).Trim();
        }

        RuntimeProcess StartPython(ComputerState st)
        {
            try
            {
                // -I isolated (no env vars, no user site), -S no site module, -X utf8. Limits are applied by host.py itself.
                string script = st.Language == "sql" ? "sql_host.py" : "host.py"; // SQL runs in SQLite inside the same sandboxed Python process model
                return RuntimeProcess.Start(PythonExecutable(), $"-I -S -B -X utf8 \"{Path.Combine(PythonRuntimeDir, script)}\"", PythonRuntimeDir, inbox, errors);
            }
            catch (Exception e)
            {
                Fail(st, "Python runtime not found or failed to start: " + e.Message);
                return null;
            }
        }

        void Send(Program p, Dictionary<string, object> msg)
        {
            var target = p.Proc ?? host;
            if (target == null || !target.Send(Json.Write(msg))) RuntimeError = "Runtime connection lost";
        }

        void OnDestroy()
        {
            if (session != null) session.Factory.ActionCompleted -= OnActionCompleted;
            foreach (var p in programs.Values) p.Proc?.Shutdown(0);
            host?.Shutdown(300);
            host = null;
        }

        // ------------------------------------------------------------ public API (UI)
        public bool IsRunning(PlacedBuilding computer) => programs.ContainsKey(computer.InstanceId);
        public int RunningCount => programs.Count;
        public Stats GetStats(PlacedBuilding c) => stats.TryGetValue(c.InstanceId, out var s) ? s : default;

        /// <summary>A program asked the ship (in orbit) to fly to another world; GameBootstrap does the switch.</summary>
        public static event Action<PlacedBuilding, Planet> TravelRequested;

        /// <param name="snippet">terminal one-liner: runs instead of the saved program, which is not changed</param>
        public bool StartProgram(PlacedBuilding computer, bool debug = false, string snippet = null)
        {
            var st = (ComputerState)computer.State;
            if (programs.ContainsKey(computer.InstanceId)) Stop(computer);
            st.LastError = null; st.LastErrorLine = 0;
            if (st.Compromised) { st.Compromised = false; st.AddLog("[security] system restored from backup after a rogue-drone intrusion"); }
            if (System.Array.IndexOf(ComputerState.Languages, st.Language) < 0) return Fail(st, $"Language '{st.Language}' is not available yet.");
            if (st.TotalCodeBytes > st.MaxCodeBytes) return Fail(st, $"Program too large: storage is {st.MaxCodeBytes / 1024} KB.");
            RuntimeProcess proc = null;
            if (st.Language == "python" || st.Language == "sql") { if ((proc = StartPython(st)) == null) return false; }
            else if (st.Language == "php") { if ((proc = StartPhp(st)) == null) return false; }
            else if (st.Language == "csharp") { if ((proc = StartCsharp(st)) == null) return false; }
            else if (st.Language == "c" || st.Language == "cpp")
            {
                var headers = new List<string>(); foreach (var f in st.Files.Keys) if (f.EndsWith(".h") || f.EndsWith(".hpp")) headers.Add(f);
                string bad = CSource.Check(st.Code, headers);
                foreach (var f in st.Files) if (bad == null && (bad = CSource.Check(f.Value, headers)) != null) { st.LastErrorFile = f.Key; break; }
                if (bad != null) return Fail(st, bad);
                if (!EnsureHost()) return Fail(st, RuntimeError);
            }
            else if (!EnsureHost()) return Fail(st, RuntimeError);
            bool watched = st.Language == "php" || st.Language == "csharp";   // single-threaded hosts: the CPU watchdog lives here
            var prog = new Program { Computer = computer, St = st, LastTick = Time.time, Debug = debug, Proc = proc,
                                     BusySince = !watched ? -1 : Time.realtimeSinceStartup + (st.Language == "csharp" ? 15f : 0f) };   // C#: compiling first
            programs[computer.InstanceId] = prog;
            stats[computer.InstanceId] = new Stats { Running = true, RamMb = st.RamMb };
            st.Running = true;
            st.AddLog($"[{(snippet != null ? "snippet" : debug ? "debug" : "run")}] {DateTime.Now:HH:mm:ss} {st.Language}");
            if (st.Language == "c" || st.Language == "cpp")
            {
                if (debug) st.AddLog($"[debug] the step debugger is not available for {st.Language} yet — running normally");
                st.AddLog("[compile] clang → WebAssembly…");
                CompileC(prog, st.Language, st.Code, new Dictionary<string, string>(st.Files), st.RamMb, Application.temporaryCachePath);
                return true;
            }
            Send(prog, new Dictionary<string, object> {
                { "t", "start" }, { "id", computer.InstanceId }, { "code", snippet ?? st.Code }, { "ram", st.RamMb }, { "name", "PC-" + computer.InstanceId },
                { "lang", st.Language }, { "debug", debug }, { "bps", new List<object>(BpList(st)) },
                { "db", st.SqlDb ?? "" }, { "storage", st.MaxCodeBytes }, { "files", FilesOf(st) }, { "watches", new List<object>(st.Watches) } });
            return true;
        }

        static Dictionary<string, object> FilesOf(ComputerState st) { var d = new Dictionary<string, object>(); foreach (var kv in st.Files) d[kv.Key] = kv.Value; return d; }

        static IEnumerable<object> BpList(ComputerState st) { foreach (var l in st.Breakpoints) yield return l; }

        public DebugInfo GetDebug(PlacedBuilding c) =>
            programs.TryGetValue(c.InstanceId, out var p) ? new DebugInfo { Debugging = p.Debug, PausedLine = p.PausedLine, Vars = p.PausedVars, Stack = p.PausedStack, Watches = p.PausedWatches } : default;

        /// <summary>Resume a paused program: "continue" runs to the next breakpoint, "step" stops at the next statement.</summary>
        /// <summary>mode: "continue" (to the next breakpoint), "step" (into), "over" (same function), "out" (back to the caller).</summary>
        public void Resume(PlacedBuilding c, string mode)
        {
            if (!programs.TryGetValue(c.InstanceId, out var p) || p.PausedLine == 0) return;
            p.PausedLine = 0; p.PausedVars = null; p.PausedStack = null; p.PausedWatches = null;
            Reply(p, new Dictionary<string, object> { { "mode", mode }, { "watches", new List<object>(p.St.Watches) } });
        }

        public void ToggleBreakpoint(PlacedBuilding c, int line)
        {
            var st = (ComputerState)c.State;
            if (!st.Breakpoints.Remove(line)) st.Breakpoints.Add(line);
            if (programs.TryGetValue(c.InstanceId, out var p)) p.BreakpointsDirty = true;
        }

        bool Fail(ComputerState st, string msg) { st.LastError = msg; st.Running = false; st.AddLog("[error] " + msg); return false; }

        public void Stop(PlacedBuilding computer)
        {
            var st = (ComputerState)computer.State;
            st.Running = false;
            if (!programs.TryGetValue(computer.InstanceId, out var p)) return;
            End(p, "stopped", null);
            if (p.Proc != null) p.Proc.Shutdown(0); // Python: the process is the sandbox, so stopping = killing it
            else Send(p, new Dictionary<string, object> { { "t", "stop" }, { "id", computer.InstanceId } });
            st.AddLog("[stopped]");
        }

        // ------------------------------------------------------------ main loop
        void Update()
        {
            while (errors.TryDequeue(out var e)) Debug.LogWarning("[runtime] " + e);
            while (inbox.TryDequeue(out var item)) Handle(item.src, item.line);
            while (compiled.TryDequeue(out var c))
            {
                if (!programs.TryGetValue(c.p.Computer.InstanceId, out var cur) || cur != c.p) continue;   // stopped while compiling
                if (c.error != null)
                {
                    c.p.St.LastErrorLine = c.line ?? 0; c.p.St.LastErrorFile = c.file;
                    Fail(c.p.St, c.error);
                    End(c.p, "error", c.p.St.LastError);
                    continue;
                }
                Send(c.p, new Dictionary<string, object> { { "t", "start" }, { "id", c.p.Computer.InstanceId }, { "lang", c.p.St.Language }, { "wasm", c.wasm }, { "sources", new List<object>(c.sources) },
                                                         { "ram", c.p.St.RamMb }, { "name", "PC-" + c.p.Computer.InstanceId } });
            }
            if (host != null && host.HasExited && host.Eof && inbox.IsEmpty)
            {
                RuntimeError = "JavaScript runtime stopped unexpectedly (exit code " + host.ExitCode + ")";
                foreach (var p in new List<Program>(programs.Values))
                    if (p.Proc == null) { Fail(p.St, RuntimeError); End(p, "error", p.St.LastError); }
                host = null;
            }
            if (inbox.IsEmpty)
                foreach (var p in new List<Program>(programs.Values))
                    if (p.Proc != null && p.Proc.HasExited && p.Proc.Eof)
                    {   // the process died without reporting an exit (crash, OS memory kill...)
                        Fail(p.St, $"{p.St.Language} runtime stopped unexpectedly (exit code {p.Proc.ExitCode})");
                        End(p, "error", p.St.LastError);
                    }

            foreach (var p in new List<Program>(programs.Values))   // PHP has no threads, so its CPU watchdog lives here
                if (p.BusySince >= 0 && Time.realtimeSinceStartup - p.BusySince > PhpWatchdogSeconds)
                {
                    p.Proc?.Shutdown(0);
                    Fail(p.St, $"CPU watchdog: ran {PhpWatchdogSeconds:0}s without calling the world API or sleep() — infinite loop?");
                    End(p, "error", p.St.LastError);
                }

            float now = Time.time;
            compromised.Clear();
            foreach (var p in programs.Values)
            {
                if (p.St.Compromised) { compromised.Add(p.Computer); continue; }
                bool powered = session.Factory.HasPower(p.Computer);
                if (p.Subs.Contains("creature_detected"))
                    foreach (var c in session.State.Creatures.All)
                        if (c.Alive && !p.SeenCreatures.Contains(c.Id) && Math.Sqrt((c.X - p.Computer.CenterX) * (c.X - p.Computer.CenterX) + (c.Y - p.Computer.CenterY) * (c.Y - p.Computer.CenterY)) <= 12)
                        {
                            p.SeenCreatures.Add(c.Id);
                            Emit(p, "creature_detected", new Dictionary<string, object> { { "id", c.Id }, { "type", c.Type.ToString().ToLowerInvariant() }, { "health", c.Health } });
                        }
                if (powered != p.WasPowered) { Emit(p, powered ? "power_restored" : "power_lost", null); p.WasPowered = powered; }
                if (p.Subs.Contains("tick") && now - p.LastTick >= 1f) { p.LastTick = now; Emit(p, "tick", new Dictionary<string, object> { { "time", Math.Round(now, 2) } }); }

                if (now - p.CallWindow >= 1f) { p.CallsPerSecShown = p.Calls; p.Calls = 0; p.CallWindow = now; }
                var s = stats[p.Computer.InstanceId]; s.CallsPerSec = p.CallsPerSecShown;

                if (p.SleepUntil >= 0) { s.Waiting = "sleep"; if (now >= p.SleepUntil) { p.SleepUntil = -1; Reply(p, null); } }
                else if (p.WaitingEvent) { s.Waiting = "event"; if (p.Events.Count > 0) { p.WaitingEvent = false; Reply(p, null); } }
                else if (p.AwaitingAction) s.Waiting = "machine";
                else if (p.PausedLine != 0) s.Waiting = "debugger";
                else if (p.Pending != null && (p.Pending["obj"] as string) == "debug")
                {
                    var call = p.Pending; p.Pending = null; Execute(p, call); // debugger never waits for CPU or power
                }
                else if (p.Pending != null)
                {
                    if (!powered) s.Waiting = "no power";
                    else if (now >= p.NextCallTime)
                    {
                        s.Waiting = null;
                        var call = p.Pending; p.Pending = null;
                        p.NextCallTime = now + 1f / Mathf.Max(1, Cps(p));
                        p.Calls++; p.TotalCalls++;
                        if (call.TryGetValue("obj", out var to) && to is string target && target != "computer" && target != "debug")
                            p.CallsByTarget[target] = (p.CallsByTarget.TryGetValue(target, out var cn) ? cn : 0) + 1;
                        Execute(p, call);
                    }
                    else s.Waiting = "cpu";
                }
                else s.Waiting = null;
                stats[p.Computer.InstanceId] = s;
            }
            foreach (var b in compromised)
            {
                var st = (ComputerState)b.State;
                st.LastError = "System compromised by a rogue drone — press Run to restore from backup";
                Stop(b);
            }
        }

        void Handle(RuntimeProcess src, string line)
        {
            Dictionary<string, object> m;
            try { m = Json.Parse(line) as Dictionary<string, object>; } catch (Exception e) { Debug.LogWarning("[runtime] bad message: " + e.Message); return; }
            if (m == null || !m.TryGetValue("t", out var t)) return;
            int id = m.TryGetValue("id", out var idv) && idv is double d ? (int)d : -1;
            programs.TryGetValue(id, out var p);
            if (p != null && (p.Proc ?? host) != src) p = null; // late message from a process this program no longer uses
            switch ((string)t)
            {
                case "call": if (p != null) { p.Pending = m; p.BusySince = -1; } break;
                case "log": if (p != null) p.St.AddLog(m["msg"] as string); break;
                case "state":   // SQL: the player's tables changed
                    if (p != null && m.TryGetValue("db", out var dbv) && dbv is string dump) p.St.SqlDb = dump;
                    break;
                case "stats":
                    if (p != null && stats.TryGetValue(id, out var s))
                    {
                        s.Cpu = m["cpu"] is double c ? (int)c : 0;
                        if (p.Proc != null)
                        {   // Python: measure the process from outside (host.py's own ctypes is blocked by its audit hook)
                            double mb = p.Proc.PrivateMb;
                            if (p.BaselineMb <= 0) p.BaselineMb = mb;
                            s.HeapMb = Math.Round(Math.Max(0, mb - p.BaselineMb), 1);
                        }
                        else s.HeapMb = m.TryGetValue("heapMb", out var h) && h is double hd ? hd : 0;
                        stats[id] = s;
                    }
                    break;
                case "exit":
                    if (p == null) break;
                    string reason = m["reason"] as string;
                    if (reason == "error")
                    {
                        int errLine = m.TryGetValue("line", out var ln) && ln is double l ? (int)l : 0;
                        string errFile = m.TryGetValue("file", out var fv) ? fv as string : null;
                        if (errFile != null && !p.St.Files.ContainsKey(errFile))   // runtimes report names without the extension
                            foreach (var k in p.St.Files.Keys) if (System.IO.Path.GetFileNameWithoutExtension(k) == errFile) { errFile = k; break; }
                        Fail(p.St, (errFile != null ? errFile + " " : "") + (errLine > 0 ? $"line {errLine}: " : "") + (m["msg"] as string));
                        p.St.LastErrorLine = errLine; p.St.LastErrorFile = errFile;
                        End(p, "error", p.St.LastError);
                    }
                    else { if (reason == "done") { p.St.Running = false; p.St.AddLog("[program finished]"); } End(p, reason == "done" ? "done" : "stopped", null); }
                    break;
            }
        }

        /// <summary>Removes a program and adds the run to the computer's history.</summary>
        void End(Program p, string result, string msg)
        {
            if (programs.TryGetValue(p.Computer.InstanceId, out var cur) && cur == p) { programs.Remove(p.Computer.InstanceId); stats.Remove(p.Computer.InstanceId); }
            p.St.Record(new RunRecord { When = DateTime.Now.ToString("HH:mm:ss"), Language = p.St.Language, Result = result, Message = msg,
                                        Seconds = Time.realtimeSinceStartup - p.StartedAt, Calls = p.TotalCalls });
        }

        /// <summary>How often the running program called each machine (and API object) — for the diagram tab.</summary>
        public Dictionary<string, int> CallMap(PlacedBuilding c) => programs.TryGetValue(c.InstanceId, out var p) ? new Dictionary<string, int>(p.CallsByTarget) : null;

        /// <summary>An event for every running program (except `from`) that subscribed to it: HUD buttons, network messages.</summary>
        public void Broadcast(string name, object data, object from = null)
        {
            foreach (var p in programs.Values) if (p != from) Emit(p, name, data);
        }

        /// <summary>Calls per second: the computer's CPU, +50% with the Programming research.</summary>
        float Cps(Program p) => p.St.CallsPerSecond * (session.State.Research.IsDone("programming") ? 1.5f : 1f);

        void Emit(Program p, string name, object data)
        {
            if (!p.Subs.Contains(name)) return;
            if (p.Events.Count >= 64) p.Events.RemoveAt(0); // bounded: slow handlers drop the oldest events
            p.Events.Add(new Dictionary<string, object> { { "name", name }, { "data", data } });
        }

        void OnActionCompleted(PlacedBuilding machine, string item)
        {
            string evt = machine.Def.Kind == BuildingKind.Drill ? "item_mined" : machine.Def.Kind == BuildingKind.Furnace ? "item_smelted" : null;
            if (evt == null) return;
            foreach (var p in programs.Values)
                if (p.Computer.DistanceTo(machine) <= Factory.ComputerRange)
                    Emit(p, evt, new Dictionary<string, object> { { "machine", "m" + machine.InstanceId }, { "item", item } });
        }

        void Reply(Program p, object value, string error = null, string warn = null)
        {
            // a stopped program's late machine callbacks must not reach a newer program on the same computer
            if (!programs.TryGetValue(p.Computer.InstanceId, out var current) || current != p) return;
            var msg = new Dictionary<string, object> { { "t", "reply" }, { "id", p.Computer.InstanceId }, { "value", value } };
            if (error != null) msg["error"] = error;
            if (warn != null) msg["warn"] = warn;
            if (p.BreakpointsDirty) { msg["bps"] = new List<object>(BpList(p.St)); p.BreakpointsDirty = false; }
            if (p.Events.Count > 0) { msg["events"] = new List<object>(p.Events); p.Events.Clear(); }
            if (p.St.Language == "php" || p.St.Language == "csharp") p.BusySince = Time.realtimeSinceStartup;
            Send(p, msg);
        }

        // ------------------------------------------------------------ world API
        static Dictionary<string, object> Info(PlacedBuilding b) =>
            new Dictionary<string, object> { { "id", "m" + b.InstanceId }, { "type", b.Def.ProgramType }, { "model", b.Def.Id }, { "name", $"{b.Def.DisplayName} #{b.InstanceId}" } };

        PlacedBuilding Resolve(Program p, object idArg)
        {
            string s = idArg is double d ? ((int)d).ToString() : (idArg as string ?? "");
            if (s.StartsWith("m")) s = s.Substring(1);
            if (!int.TryParse(s, out int iid)) return null;
            var b = session.Grid.ById(iid);
            return b != null && b != p.Computer && b.State != null && b.DistanceTo(p.Computer) <= Factory.ComputerRange ? b : null;
        }

        void Execute(Program p, Dictionary<string, object> call)
        {
            string obj = call["obj"] as string, fn = call["fn"] as string;
            var args = call.TryGetValue("args", out var a) ? a as List<object> ?? new List<object>() : new List<object>();
            object Arg(int i) => i < args.Count ? args[i] : null;

            switch (obj)
            {
                case "debug":
                    if (fn == "pause" && p.Debug)
                    {
                        p.PausedLine = Arg(0) is double dl ? (int)dl : -1;
                        p.PausedVars = Arg(1) as Dictionary<string, object> ?? new Dictionary<string, object>();
                        p.PausedStack = Arg(2) as List<object>;
                        p.PausedWatches = Arg(3) as List<object>;
                        return; // reply comes from Resume()
                    }
                    break;
                case "computer":
                    switch (fn)
                    {
                        case "info": Reply(p, new Dictionary<string, object> { { "name", "PC-" + p.Computer.InstanceId }, { "cps", Cps(p) } }); return;
                        case "time": Reply(p, Math.Round(Time.time, 3)); return;
                        case "sleep":
                            double sec = Arg(0) is double sd ? sd : 0;
                            p.SleepUntil = Time.time + (float)Math.Max(0, Math.Min(3600, sec)); return;
                        case "subscribe": p.Subs.Add(Arg(0) as string ?? ""); Reply(p, true); return;
                        case "send":
                        {   // logic networks: every other running program subscribed to "message" receives it
                            if (!session.State.Research.IsDone("networking")) { Reply(p, false, warn: "computer.send(): needs the Networking research"); return; }
                            string channel = Arg(0) as string ?? "";
                            object payload = Arg(1);
                            if (channel.Length == 0 || channel.Length > 64) { Reply(p, false, warn: "computer.send(channel, data): channel must be 1-64 characters"); return; }
                            if (Json.Write(payload).Length > 4096) { Reply(p, false, warn: "computer.send(): data larger than 4 KB"); return; }
                            Broadcast("message", new Dictionary<string, object> { { "channel", channel }, { "data", payload }, { "from", "PC-" + p.Computer.InstanceId } }, p);
                            Reply(p, true); return;
                        }
                        case "wait_event": if (p.Events.Count > 0) Reply(p, null); else p.WaitingEvent = true; return;
                    }
                    break;
                case "hud":
                    if (fn == "set")
                    {
                        string key = Arg(0) as string;
                        object raw = Arg(1);
                        string val = raw == null ? "" : raw is string str ? str : raw is double dv ? dv.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) : raw is bool bv ? (bv ? "true" : "false") : Json.Write(raw);
                        string err = session.State.Hud.SetValue(key, val);
                        if (err != null) { Reply(p, null, error: err); return; }
                        Reply(p, true); return;
                    }
                    if (fn == "alarm") { session.State.Hud.Alarm($"PC-{p.Computer.InstanceId}: {Arg(0)}"); Reply(p, true); return; }
                    if (fn == "notify") { session.State.Hud.Notify($"{"PC-" + p.Computer.InstanceId}: {Arg(0)}"); Reply(p, true); return; }
                    break;
                case "signals":
                    if (fn == "read") { Reply(p, ToObjects(session.Factory.SignalsAt(p.Computer))); return; }
                    if (fn == "send")
                    {
                        var sendErr = SetSignalOut((ComputerState)p.Computer.State, Arg(0) as Dictionary<string, object>);
                        if (sendErr != null) { Reply(p, null, error: sendErr); return; }
                        Reply(p, true); return;
                    }
                    break;
                case "energy":
                    if (fn == "available") { Reply(p, session.Factory.HasPower(p.Computer)); return; }
                    if (fn == "stats")
                    {
                        var (cap, dem) = session.Factory.NetworkStats(p.Computer);
                        Reply(p, new Dictionary<string, object> { { "capacityKW", cap }, { "demandKW", dem }, { "satisfaction", p.Computer.State.PowerSatisfaction }, { "storedKJ", Math.Round(session.Factory.NetworkStoredKJ(p.Computer), 1) } });
                        return;
                    }
                    if (fn == "network") { Reply(p, NetworkInfo(session.Factory.NetworkDetails(p.Computer))); return; }
                    break;
                case "world":
                    if (fn == "weather")
                    {
                        var clk = session.State.Clock;
                        var at = p.Computer.Origin;
                        var biome = session.World.Generator.BiomeAt(at);
                        var w = clk.WeatherAt(biome);
                        var env = Shelter.Apply(session.State, at, Vitals.At(session.World.Generator, at, clk));
                        Reply(p, new Dictionary<string, object> { { "weather", w.ToString().ToLowerInvariant() }, { "name", WorldClock.WeatherName(w, biome) },
                            { "temperature", Math.Round(env.AmbientC, 1) }, { "time", clk.ClockText }, { "day", clk.Day }, { "season", clk.Season.ToString().ToLowerInvariant() },
                            { "daylight", Math.Round(clk.Daylight, 3) }, { "biome", biome.ToString().ToLowerInvariant() } });
                        return;
                    }
                    if (fn == "pollution")
                    {
                        var eco = session.State.Ecology;
                        Reply(p, new Dictionary<string, object> { { "here", Math.Round(eco.At(p.Computer.Origin), 1) }, { "total", Math.Round(eco.Total, 1) },
                            { "smog", eco.IsSmog(p.Computer.Origin) }, { "smogLevel", Ecology.SmogLevel } });
                        return;
                    }
                    break;
                case "sql":
                    if (fn == "snapshot") { Reply(p, SqlSnapshot(p)); return; }
                    if (fn == "commands") { Reply(p, SqlCommands(p, Arg(0) as List<object>)); return; }
                    break;
                case "machines":
                    var near = session.Factory.MachinesNear(p.Computer);
                    if (fn == "list") { var l = new List<object>(); foreach (var b in near) l.Add(Info(b)); Reply(p, l); return; }
                    if (fn == "find") { string type = Arg(0) as string; foreach (var b in near) if (b.Def.ProgramType == type || b.Def.Id == type) { Reply(p, Info(b)); return; } Reply(p, null); return; }
                    if (fn == "get") { var b = Resolve(p, Arg(0)); Reply(p, b != null ? Info(b) : null); return; }
                    break;
                default:
                    var m = Resolve(p, obj);
                    if (m == null) { Reply(p, null, error: $"Machine {obj} is gone or out of range ({Factory.ComputerRange} cells)."); return; }
                    switch (fn)
                    {
                        case "configure":   // combinators and machine conditions: one text, e.g. "iron_plate<100"
                        {
                            var cfgErr = Factory.Configure(m, Arg(0) as string ?? "");
                            if (cfgErr != null) { Reply(p, null, error: cfgErr); return; }
                            Reply(p, true); return;
                        }
                        case "signals":
                            Reply(p, ToObjects(session.Factory.SignalsAt(m))); return;
                        case "train":   // train depots: their train's state, stop, cargo and fuel
                            if (!(m.State is TrainDepotState)) break;
                            Reply(p, session.Factory.TrainInfo(m)); return;
                        case "network":   // roboports: robots and items of their logistic network
                            if (!(m.State is RoboportState)) break;
                            Reply(p, session.Factory.LogisticInfo(m)); return;
                        case "status":
                            Reply(p, new Dictionary<string, object> { { "status", m.State.Status }, { "power", m.State.PowerSatisfaction }, { "busy", session.Factory.HasAction(m) } });
                            return;
                        case "contents":
                            var c = new Dictionary<string, object>();
                            foreach (var (role, inv) in m.State.AllInventories)
                            {
                                var items = new Dictionary<string, object>();
                                for (int i = 0; i < inv.SlotCount; i++)
                                    if (!inv[i].IsEmpty) items[inv[i].ItemId] = (items.TryGetValue(inv[i].ItemId, out var n) ? (int)n : 0) + inv[i].Count;
                                c[role] = items;
                            }
                            Dictionary<string, object> fl = null;   // fluids: {"water": 45.5}; a pipe reports its whole segment
                            foreach (var (fluid, amount) in m.State.FluidContents)
                                (fl ?? (fl = new Dictionary<string, object>()))[fluid] = Math.Round((fl.TryGetValue(fluid, out var had) ? (double)had : 0) + amount, 1);
                            if (fl != null) c["fluids"] = fl;
                            Reply(p, c); return;
                        case "start":
                        case "stop":
                        case "set_filter":
                        {
                            // programmable conveyors and inserters share start/stop/filter
                            var cv = m.Def.Kind == BuildingKind.Conveyor ? (ConveyorState)m.State : null;
                            var ins = m.State as InserterState;
                            if (m.State is SwitchedFluidState sw && fn != "set_filter") { sw.Running = fn == "start"; Reply(p, true); return; }   // pump, pumpjack, filler
                            if (cv == null && ins == null) break;
                            string f = fn == "set_filter" ? Arg(0) as string : null;
                            if (f != null && !ItemDatabase.Exists(f)) { Reply(p, null, error: $"Unknown item id '{f}'"); return; }
                            bool? run = fn == "start" ? true : fn == "stop" ? false : (bool?)null;
                            if (cv != null) { if (run.HasValue) cv.Running = run.Value; else cv.Filter = f; cv.Status = cv.Running ? "moving" : "stopped"; }
                            if (ins != null) { if (run.HasValue) ins.Running = run.Value; else ins.Filter = f; }
                            Reply(p, true); return;
                        }
                        case "plant":
                        case "water":
                        case "harvest":
                        case "plots":
                        {
                            if (m.Def.Kind != BuildingKind.Greenhouse) break;
                            var (val, gerr) = session.Factory.Greenhouse(m, fn, Arg(0), Arg(1));
                            // routine outcomes (nothing ripe, no free plot...) are already in the return value; only warn about problems
                            bool problem = gerr == "tank_empty" || gerr == "output_full";
                            Reply(p, val, warn: problem ? $"greenhouse.{fn}(): {gerr}" : null);
                            return;
                        }
                        case "get":
                        case "set":
                        case "keys":
                        case "delete":
                        {
                            if (!(m.State is ServerState sv)) break;
                            if (sv.PowerSatisfaction <= 0) { Reply(p, null, warn: $"server.{fn}(): no power"); return; }
                            string key = Arg(0) as string;
                            if (fn == "keys") { Reply(p, new List<object>(sv.Data.Keys)); return; }
                            if (fn == "get") { Reply(p, key != null && sv.Data.TryGetValue(key, out var val) ? (object)val : null); return; }
                            string serr = sv.Set(key, fn == "delete" ? null : Arg(1) is string sval ? sval : Json.Write(Arg(1)));
                            Reply(p, serr == null, warn: serr != null ? $"server.{fn}(): {serr}" : null); return;
                        }
                        case "totals":
                        case "find":
                        {
                            if (!(m.State is StorageHubState hub)) break;
                            var t = hub.Totals();
                            if (fn == "find") { Reply(p, t.TryGetValue(Arg(0) as string ?? "", out var cnt) ? cnt : 0); return; }
                            var d = new Dictionary<string, object>(); foreach (var kv in t) d[kv.Key] = kv.Value;
                            Reply(p, d); return;
                        }
                        case "launch":
                        case "throttle":
                        case "flight":
                        case "planets":
                        case "travel":
                        case "land":
                        case "asteroids":
                        case "mine_asteroid":
                        case "dock":
                        case "answer":
                        case "transmit":
                        {
                            if (!(m.State is LaunchPadState lp)) break;
                            var gs = session.State;
                            switch (fn)
                            {
                                case "land": { string e = lp.Land(); Reply(p, e == null, warn: e != null ? "pad.land(): " + e : null); return; }
                                case "asteroids": Reply(p, lp.AsteroidList(gs)); return;
                                case "mine_asteroid":
                                {
                                    string e = Arg(0) is double aid ? lp.MineAsteroid(gs, (int)aid) : "needs an asteroid id";
                                    Reply(p, e == null, warn: e != null ? "pad.mineAsteroid(): " + e : null); return;
                                }
                                case "dock":
                                {
                                    string e = lp.Dock(gs, out var info);
                                    if (e != null) { Reply(p, null, warn: "pad.dock(): " + e); return; }
                                    Reply(p, info); return;
                                }
                                case "answer": { string e = lp.Answer(gs, Arg(0)); Reply(p, e == null, warn: e != null ? "pad.answer(): " + e : null); return; }
                                case "transmit": { string e = lp.Transmit(gs); Reply(p, e == null, warn: e != null ? "pad.transmit(): " + e : null); return; }
                                case "launch": { lp.Underground = Caves.IsUnderground(session.State); string le = lp.Launch(session.State.Research); Reply(p, le == null, warn: le != null ? "pad.launch(): " + le : null); return; }
                                case "throttle":
                                    if (lp.Flight == null || lp.Phase != "ascent") { Reply(p, false, warn: "pad.throttle(): not in flight"); return; }
                                    lp.Flight.Throttle = Arg(0) is double tv && !double.IsNaN(tv) ? (float)Math.Max(0, Math.Min(1, tv)) : 0f;
                                    Reply(p, true); return;
                                case "flight":
                                {
                                    var fi = lp.Flight != null ? lp.Flight.Info() : new Dictionary<string, object> { { "phase", lp.Phase } };
                                    fi["pad"] = lp.Phase; fi["fuelUnits"] = Math.Round(lp.FuelUnits, 1); fi["last"] = lp.LastResult;
                                    Reply(p, fi); return;
                                }
                                case "planets":
                                {
                                    var list = new List<object>();
                                    foreach (var pl in StarSystem.Of(session.State.HomeSeed))
                                        list.Add(new Dictionary<string, object> { { "id", pl.Id }, { "name", pl.Name }, { "kind", pl.Kind.ToString().ToLowerInvariant() },
                                            { "distanceAU", pl.DistanceAU }, { "fuel", Math.Round(Voyage.Cost(session.State, pl), 1) }, { "here", pl.Id == session.State.PlanetId } });
                                    Reply(p, list); return;
                                }
                                default:
                                {
                                    var dest = StarSystem.Get(session.State.HomeSeed, Arg(0) as string ?? "");
                                    string te = Net.IsGuest ? "only the host's world can travel" : Voyage.Check(session.State, m, dest);
                                    Reply(p, te == null, warn: te != null ? "pad.travel(): " + te : null);
                                    if (te == null) TravelRequested?.Invoke(m, dest);
                                    return;
                                }
                            }
                        }
                        case "targets":
                        case "fire":
                        {
                            if (m.Def.Kind != BuildingKind.Turret) break;
                            if (fn == "targets")
                            {
                                var list = new List<object>();
                                foreach (var cr in session.State.Creatures.InRange(m))
                                    list.Add(new Dictionary<string, object> { { "id", cr.Id }, { "type", cr.Type.ToString().ToLowerInvariant() }, { "health", Math.Round(cr.Health, 1) },
                                        { "distance", Math.Round(Math.Sqrt((cr.X - m.CenterX) * (cr.X - m.CenterX) + (cr.Y - m.CenterY) * (cr.Y - m.CenterY)), 2) } });
                                Reply(p, list); return;
                            }
                            int targetId = Arg(0) is double td ? (int)td : -1;
                            string ferr = session.State.Creatures.Fire(m, targetId, session.State.Clock.Seconds);
                            Reply(p, ferr == null, warn: ferr != null && ferr != "cooldown" && ferr != "no_such_target" ? $"turret.fire(): {ferr}" : null);
                            return;
                        }
                        case "techs":
                        {
                            if (m.Def.Kind != BuildingKind.Lab) break;
                            var list = new List<object>();
                            foreach (var t in Research.Tree)
                            {
                                var cost = new Dictionary<string, object>();
                                foreach (var ci in t.Cost) cost[ci.ItemId] = ci.Count;
                                list.Add(new Dictionary<string, object> { { "id", t.Id }, { "name", t.Name }, { "area", t.Area },
                                    { "status", session.State.Research.Status(t) }, { "main", t.MainLine }, { "seconds", t.Seconds }, { "cost", cost } });
                            }
                            Reply(p, list); return;
                        }
                        case "mine":
                        case "smelt":
                        case "craft":
                        case "research":
                            if (fn == "craft" && m.Def.Kind != BuildingKind.Assembler) break;
                            if (fn == "research" && m.Def.Kind != BuildingKind.Lab) break;
                            p.AwaitingAction = true;
                            System.Action<string> doneCb = r => { p.AwaitingAction = false; Reply(p, r); };
                            string err = fn == "mine" ? session.Factory.StartMine(m, doneCb)
                                : fn == "smelt" ? session.Factory.StartSmelt(m, doneCb)
                                : fn == "research" ? session.Factory.StartResearch(m, Arg(0) as string, doneCb)
                                : session.Factory.StartCraft(m, Arg(0) as string, doneCb);
                            if (err != null) { p.AwaitingAction = false; Reply(p, null, warn: $"{m.Def.Id}.{fn}(): {err}"); }
                            return;
                    }
                    break;
            }
            Reply(p, null, error: $"Unknown API: {obj}.{fn}()");
        }
    
        // ------------------------------------------------------------ SQL
        static Dictionary<string, object> ToObjects(Dictionary<string, int> d)
        {
            var o = new Dictionary<string, object>();
            foreach (var kv in d) o[kv.Key] = kv.Value;
            return o;
        }

        /// <summary>signals.send({A: 5}): what this computer puts on its signal networks (replaces the previous values; {} clears).</summary>
        static string SetSignalOut(ComputerState cs, Dictionary<string, object> values)
        {
            if (values == null) return "signals.send needs an object like {A: 5}";
            if (values.Count > SignalSpec.MaxPairs) return $"at most {SignalSpec.MaxPairs} signals";
            var next = new Dictionary<string, int>();
            foreach (var kv in values)
            {
                if (!SignalSpec.IsName(kv.Key)) return $"'{kv.Key}' is not a signal (items like iron_plate, fluid_water, or A-Z, 0-9)";
                double v = kv.Value is double dv ? dv : kv.Value is bool bv ? (bv ? 1 : 0) : double.NaN;
                if (double.IsNaN(v) || v < int.MinValue || v > int.MaxValue) return $"'{kv.Key}': the value must be a whole number";
                if (v != 0) next[kv.Key] = (int)Math.Round(v);
            }
            cs.SignalOut.Clear();
            foreach (var kv in next) cs.SignalOut[kv.Key] = kv.Value;
            return null;
        }

        /// <summary>energy.network(): the last tick of the computer's power network, by source and priority (null when it has none).</summary>
        static Dictionary<string, object> NetworkInfo(PowerStats s)
        {
            if (s == null) return null;
            double R(float v) => Math.Round(v, 2);
            return new Dictionary<string, object> {
                { "id", s.Network }, { "nodes", s.Nodes }, { "consumers", s.Consumers },
                { "supplyKW", new Dictionary<string, object> { { "solar", R(s.SolarKW) }, { "steam", R(s.SteamKW) }, { "coal", R(s.CoalKW) }, { "battery", R(s.BatteryKW) } } },
                { "capacityKW", new Dictionary<string, object> { { "solar", R(s.SolarCapacityKW) }, { "steam", R(s.SteamCapacityKW) }, { "coal", R(s.CoalCapacityKW) }, { "battery", R(s.BatteryCapacityKW) }, { "total", R(s.CapacityKW) } } },
                { "demandKW", new Dictionary<string, object> { { "critical", R(s.CriticalDemandKW) }, { "normal", R(s.NormalDemandKW) } } },
                { "satisfaction", new Dictionary<string, object> { { "critical", R(s.CriticalSatisfaction) }, { "normal", R(s.NormalSatisfaction) } } },
                { "chargingKW", R(s.ChargeKW) }, { "storedKJ", R(s.StoredKJ) }, { "storageKJ", R(s.StorageKJ) } };
        }

        /// <summary>One cycle's view of the world for an SQL program: machines in range, their contents, power, events.</summary>
        Dictionary<string, object> SqlSnapshot(Program p)
        {
            var machines = new List<object>();
            var contents = new List<object>();
            foreach (var b in session.Factory.MachinesNear(p.Computer))
            {
                machines.Add(new Dictionary<string, object> {
                    { "id", "m" + b.InstanceId }, { "type", b.Def.ProgramType }, { "name", $"{b.Def.DisplayName} #{b.InstanceId}" },
                    { "status", b.State.Status }, { "power", b.State.PowerSatisfaction }, { "busy", session.Factory.HasAction(b) },
                    { "distance", Math.Round(b.DistanceTo(p.Computer), 2) } });
                foreach (var (role, inv) in b.State.AllInventories)
                    for (int i = 0; i < inv.SlotCount; i++)
                        if (!inv[i].IsEmpty) contents.Add(new List<object> { "m" + b.InstanceId, role, inv[i].ItemId, inv[i].Count });
                foreach (var (fluid, amount) in b.State.FluidContents)   // fluids as role 'fluid', whole units
                    contents.Add(new List<object> { "m" + b.InstanceId, "fluid", fluid, (int)Math.Round(amount) });
            }
            var signals = new List<object>();
            foreach (var kv in session.Factory.SignalsAt(p.Computer)) signals.Add(new List<object> { kv.Key, kv.Value });
            var creatures = new List<object>();
            foreach (var cr in session.State.Creatures.All)
            {
                double dist = Math.Sqrt((cr.X - p.Computer.CenterX) * (cr.X - p.Computer.CenterX) + (cr.Y - p.Computer.CenterY) * (cr.Y - p.Computer.CenterY));
                if (cr.Alive && dist <= 20) creatures.Add(new Dictionary<string, object> { { "id", cr.Id }, { "type", cr.Type.ToString().ToLowerInvariant() }, { "health", Math.Round(cr.Health, 1) }, { "distance", Math.Round(dist, 2) } });
            }
            var (cap, dem) = session.Factory.NetworkStats(p.Computer);
            // events go into the events table, so take them out of the queue before Reply() would attach them
            var events = new List<object>(p.Events);
            p.Events.Clear();
            return new Dictionary<string, object> {
                { "time", Math.Round(Time.time, 2) }, { "pollution", Math.Round(session.State.Ecology.At(p.Computer.Origin), 1) }, { "weather", session.State.Clock.WeatherAt(session.World.Generator.BiomeAt(p.Computer.Origin)).ToString().ToLowerInvariant() }, { "temperature", Math.Round(Vitals.At(session.World.Generator, p.Computer.Origin, session.State.Clock).AmbientC, 1) }, { "day", session.State.Clock.Day }, { "daylight", Math.Round(session.State.Clock.Daylight, 3) }, { "machines", machines }, { "contents", contents }, { "events", events }, { "creatures", creatures }, { "signals", signals },
                { "energy", EnergyRow(p, cap, dem) } };
        }

        Dictionary<string, object> EnergyRow(Program p, float cap, float dem)
        {
            var s = session.Factory.NetworkDetails(p.Computer);
            return new Dictionary<string, object> { { "capacityKW", cap }, { "demandKW", dem },
                { "satisfaction", p.Computer.State.PowerSatisfaction }, { "available", session.Factory.HasPower(p.Computer) },
                { "solarKW", s?.SolarKW ?? 0f }, { "steamKW", s?.SteamKW ?? 0f }, { "coalKW", s?.CoalKW ?? 0f }, { "batteryKW", s?.BatteryKW ?? 0f },
                { "criticalSatisfaction", s?.CriticalSatisfaction ?? 0f }, { "normalSatisfaction", s?.NormalSatisfaction ?? 0f }, { "storedKJ", s?.StoredKJ ?? 0f } };
        }

        /// <summary>Runs rows of the SQL commands table; returns readable warnings for the console.</summary>
        List<object> SqlCommands(Program p, List<object> rows)
        {
            var warnings = new List<object>();
            if (rows == null) return warnings;
            int n = 0;
            foreach (var r in rows)
            {
                if (++n > 50) { warnings.Add("commands: only 50 commands per cycle, the rest were ignored"); break; }
                var row = r as List<object>;
                string id = row != null && row.Count > 0 ? row[0] as string : null;
                string action = row != null && row.Count > 1 ? (row[1] as string ?? "").ToLowerInvariant() : "";
                string arg = row != null && row.Count > 2 ? row[2] as string : null;
                if (id == "signals")   // INSERT INTO commands VALUES ('signals', 'send', 'A=5,B=2')
                {
                    var vals = new Dictionary<string, int>();
                    string serr = action == "send" ? SignalSpec.ParsePairs(arg, vals) : "the signals row only knows 'send'";
                    if (serr == null) { var cs = (ComputerState)p.Computer.State; cs.SignalOut.Clear(); foreach (var kv in vals) if (kv.Value != 0) cs.SignalOut[kv.Key] = kv.Value; }
                    else warnings.Add("signals.send: " + serr);
                    continue;
                }
                var m = Resolve(p, id);
                if (m == null) { warnings.Add($"commands: machine '{id}' is gone or out of range"); continue; }
                string err = null;
                switch (action)
                {
                    case "mine": err = session.Factory.StartMine(m, null); break;
                    case "smelt": err = session.Factory.StartSmelt(m, null); break;
                    case "craft": err = session.Factory.StartCraft(m, arg, null); break;
                    case "research": err = session.Factory.StartResearch(m, arg, null); break;
                    case "launch": err = m.State is LaunchPadState lps ? lps.Launch(session.State.Research) : "not a launch pad"; break;
                    case "throttle":
                        if (m.State is LaunchPadState lpt && lpt.Flight != null && double.TryParse(arg, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double th))
                            lpt.Flight.Throttle = (float)Math.Max(0, Math.Min(1, th));
                        else err = "throttle needs a flying launch pad and a number 0..1";
                        break;
                    case "travel":
                    {
                        var dest = StarSystem.Get(session.State.HomeSeed, arg ?? "");
                        err = Net.IsGuest ? "only the host's world can travel" : Voyage.Check(session.State, m, dest);
                        if (err == null) TravelRequested?.Invoke(m, dest);
                        break;
                    }
                    case "land": err = m.State is LaunchPadState lpl ? lpl.Land() : "not a launch pad"; break;
                    case "mine_asteroid": err = m.State is LaunchPadState lpm ? (int.TryParse(arg, out int ast) ? lpm.MineAsteroid(session.State, ast) : "mine_asteroid needs an asteroid id") : "not a launch pad"; break;
                    case "dock": err = m.State is LaunchPadState lpd ? lpd.Dock(session.State, out _) : "not a launch pad"; break;
                    case "answer":
                    {
                        object val = null;   // a number, a JSON list or text
                        if (double.TryParse(arg, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double av)) val = av;
                        else if (arg != null && arg.TrimStart().StartsWith("[")) { try { val = Json.Parse(arg); } catch (FormatException) { val = arg; } }
                        else val = arg;
                        err = m.State is LaunchPadState lpa ? lpa.Answer(session.State, val) : "not a launch pad";
                        break;
                    }
                    case "transmit": err = m.State is LaunchPadState lptr ? lptr.Transmit(session.State) : "not a launch pad"; break;
                    case "fire":
                        err = int.TryParse(arg, out int fid) ? session.State.Creatures.Fire(m, fid, session.State.Clock.Seconds) : "fire needs a creature id";
                        if (err == "cooldown" || err == "no_such_target") err = null;
                        break;
                    case "plant": case "water": case "harvest":
                        object plot = arg != null && double.TryParse(arg, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double pl) ? (object)pl : null;
                        err = session.Factory.Greenhouse(m, action, plot, plot == null ? arg : null).error; break;   // SQL: a number is a plot, a word is a species
                    case "start": case "stop": case "set_filter":
                        var cv = m.Def.Kind == BuildingKind.Conveyor ? (ConveyorState)m.State : null;
                        var ins = m.State as InserterState;
                        if (m.State is SwitchedFluidState sws && action != "set_filter") { sws.Running = action == "start"; break; }   // pump, pumpjack, filler
                        if (cv == null && ins == null) { err = "only conveyors, inserters, pumps, pumpjacks and fillers accept " + action; break; }
                        if (action == "set_filter" && arg != null && !ItemDatabase.Exists(arg)) { err = $"unknown item id '{arg}'"; break; }
                        bool? run = action == "start" ? true : action == "stop" ? false : (bool?)null;
                        if (cv != null) { if (run.HasValue) cv.Running = run.Value; else cv.Filter = arg; cv.Status = cv.Running ? "moving" : "stopped"; }
                        if (ins != null) { if (run.HasValue) ins.Running = run.Value; else ins.Filter = arg; }
                        break;
                    case "configure": err = Factory.Configure(m, arg ?? ""); break;
                    default: err = $"unknown action '{action}' (mine, smelt, craft, research, fire, plant, water, harvest, start, stop, set_filter, configure)"; break;
                }
                if (err != null && err != "busy" && err != "nothing_ripe" && err != "nothing_to_water") warnings.Add($"{id}.{action}: {err}");
            }
            return warnings;
        }
    }
}
