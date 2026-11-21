# Integration test for the C# runtime host (Roslyn, separate process). Plays the role of Unity.
# Run: Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/csharp_host_test.py
import json, os, subprocess, threading

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
DOTNET = os.path.join(ROOT, "Tools", "dotnet", "dotnet.exe")
HOST = os.path.join(ROOT, "Tools", "cshost", "out", "cshost.dll")


def run(code, ram=32, timeout=40):
    p = subprocess.Popen([DOTNET, HOST], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    logs, result, ore, ticks = [], {}, [2], [0]

    def send(m):
        p.stdin.write((json.dumps(m) + "\n").encode()); p.stdin.flush()

    killer = threading.Timer(timeout, p.kill); killer.start()
    for raw in p.stdout:
        m = json.loads(raw)
        t = m["t"]
        if t != "ready" and m.get("id") != 1:   # Unity finds the program by its numeric id
            result.setdefault("bad_id", m)
        if t == "ready":
            result["version"] = m["version"]
            send({"t": "start", "id": 1, "code": code, "ram": ram})
        elif t == "log":
            logs.append(m["msg"])
        elif t == "call":
            key = m["obj"] + "." + m["fn"]; r = {"t": "reply", "id": 1}
            if key == "computer.info": r["value"] = {"name": "PC-1", "cps": 20}
            elif key == "machines.find": r["value"] = {"id": "m7", "type": "drill", "name": "Electric Drill"} if m["args"][0] == "drill" else None
            elif key == "m7.mine":
                r["value"] = "iron_ore" if ore[0] > 0 else None
                if ore[0] <= 0: r["warn"] = "drill.mine(): depleted"
                ore[0] -= 1
            elif key == "m7.status": r["value"] = {"status": "idle", "progress": 0.5}
            elif key == "computer.subscribe": r["value"] = True
            elif key == "computer.sleep": result["slept"] = m["args"][0]; r["value"] = None
            elif key == "hud.set": logs.append("hud:" + json.dumps(m["args"])); r["value"] = True
            elif key == "computer.wait_event":
                ticks[0] += 1
                r["value"] = None; r["events"] = [{"name": "tick", "data": {"n": ticks[0]}}]
            elif key == "energy.stats": r["error"] = "no power network"
            else: r["value"] = None
            send(r)
        elif t == "exit":
            result.update(m); break
    killer.cancel()
    p.kill()
    return logs, result


def check(name, cond, info=""):
    print(("ok  " if cond else "FAIL") + "  " + name + ("" if cond else "  -> " + str(info)))
    if not cond:
        raise SystemExit(1)


logs, r = run('int x = 6 * 7;\nLog("answer", x);\nvar l = new List<int> { 3, 1, 2 };\nLog(string.Join(",", l.OrderBy(v => v)));\n')
check("runs C# top-level statements with LINQ", r.get("reason") == "done" and logs == ["answer 42", "1,2,3"], (logs, r))
check("real .NET version reported", ".NET 8" in r.get("version", ""), r)
check("every message carries the numeric program id", "bad_id" not in r, r.get("bad_id"))

logs, r = run('JsonNode o;\nwhile ((o = Drill.Mine()) != null) Log("got", o);\nvar s = Drill.Status();\nLog((string)s["status"], (double)s["progress"]);\nHud.Set("ore", 2);\n')
check("machine API: preset Drill, JsonNode results", logs[:2] == ["got iron_ore", "got iron_ore"] and "idle 0.5" in logs and 'hud:["ore", 2]' in logs, logs)
check("warnings from the game are logged", any("depleted" in l for l in logs), logs)

logs, r = run('Log("a");\nint y = ;\n')
check("compile error with line", r.get("reason") == "error" and r["msg"].startswith("CS") and r.get("line") == 2, r)

logs, r = run('var a = new int[2];\nLog("x");\na[5] = 1;\n')
check("runtime exception with line", r.get("reason") == "error" and "IndexOutOfRange" in r["msg"] and r.get("line") == 3, r)

logs, r = run('try { Energy.Stats(); } catch (InvalidOperationException e) { Log("caught", e.Message); }\n')
check("API errors are catchable exceptions", "caught no power network" in logs, logs)

logs, r = run('Sleep(2.5);\nLog("woke");\n')
check("Sleep() waits in game time", r.get("slept") == 2.5 and "woke" in logs, (r, logs))

logs, r = run('var c = new Counter();\nComputer.On("tick", e => { c.N++; Log("tick", (int)e["n"]); if (c.N == 3) throw new Exception("stop"); });\nclass Counter { public int N; }\n')
check("events and player classes", logs[:3] == ["tick 1", "tick 2", "tick 3"] and r.get("reason") == "error" and "stop" in r.get("msg", ""), (logs, r))

for name, code in [
    ("System.IO", 'Log(System.IO.File.ReadAllText("C:/Windows/win.ini"));'),
    ("Environment", 'Log(Environment.UserName);'),
    ("Console", 'Console.WriteLine("x");'),
    ("GetType()", 'Log(1.GetType());'),
    ("typeof", 'var t = typeof(string);'),
    ("reflection via exception", 'try { throw new Exception(); } catch (Exception e) { Log(e.TargetSite); }'),
    ("Activator", 'Activator.CreateInstance<List<int>>();'),
    ("AppDomain", 'Log(AppDomain.CurrentDomain.FriendlyName);'),
    ("threads", 'new System.Threading.Thread(() => {}).Start();'),
    ("tasks", 'System.Threading.Tasks.Task.Run(() => {});'),
    ("dynamic", 'dynamic d = 1; Log(d);'),
    ("unsafe", 'unsafe { int x = 1; int* p = &x; }'),
    ("extern / DllImport", 'class N { [System.Runtime.InteropServices.DllImport("kernel32")] static extern int GetTickCount(); }'),
    ("Process", 'System.Diagnostics.Process.Start("cmd");'),
    ("expression trees", 'System.Linq.Expressions.Expression<Func<int>> e = () => 1;'),
    ("delegate reflection", 'Action a = () => {}; Log(a.Method);'),
    ("__makeref", 'int i = 1; var r = __makeref(i);'),
    ("Type via var", 'var x = new object(); var t = x.GetType(); Log(t.Assembly);'),
]:
    logs, r = run(code + "\n")
    check("sandbox: " + name, r.get("reason") == "error" and not any("[fonts]" in l for l in logs), (logs, r))

logs, r = run('var all = new List<byte[]>();\nwhile (true) { all.Add(new byte[1 << 20]); Sleep(0); }\n', ram=16)
check("memory limit = computer RAM", r.get("reason") == "error" and "Out of memory" in r.get("msg", ""), r)

print("ALL C# RUNTIME TESTS PASSED")
