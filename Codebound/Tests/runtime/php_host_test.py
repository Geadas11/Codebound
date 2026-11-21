# Integration test for the PHP runtime host. Plays the role of Unity, starting PHP with exactly the
# sandbox flags of ProgramManager.StartPhp (the disabled-function list is read from that file).
# Run: Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/php_host_test.py
import json, os, re, subprocess, threading

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
RUNTIME = os.path.join(ROOT, "Assets", "StreamingAssets", "Runtime", "php")
PHP = os.path.join(ROOT, "Tools", "php", "php.exe")
PM = open(os.path.join(ROOT, "Assets", "Codebound", "Scripts", "Game", "ProgramManager.cs"), encoding="utf-8").read()
DISABLED = "".join(re.findall(r'"([^"]*)"', PM.split("public const string PhpDisabled =")[1].split(";")[0]))


def run(code, ram=32, timeout=20, files=None, debug=False, bps=(), modes=(), watches=()):
    args = [PHP, "-n", f"-d", f"memory_limit={ram}M", "-d", "open_basedir=" + os.path.join(RUNTIME, "no-files-here"),
            "-d", "disable_functions=" + DISABLED, "-d", "display_errors=stderr", "-d", "log_errors=0", "-d", "html_errors=0",
            "-d", "max_execution_time=0", "-d", "allow_url_fopen=0", "-d", "allow_url_include=0", "-d", "opcache.enable_cli=0",
            "-f", os.path.join(RUNTIME, "host.php")]
    p = subprocess.Popen(args, stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=RUNTIME)
    logs, result, subs, ore, ticks = [], {}, set(), [2], [0]

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
            send({"t": "start", "id": 1, "code": code, "ram": ram, "files": files or {}, "debug": debug, "bps": list(bps), "watches": list(watches)})
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
            elif key == "m7.status": r["value"] = {"status": "idle"}
            elif key == "debug.pause":
                result.setdefault("pauses", []).append(m["args"])
                n = len(result["pauses"])
                r["value"] = {"mode": modes[n - 1] if n <= len(modes) else "continue"}
            elif key == "computer.subscribe": subs.add(m["args"][0]); r["value"] = True
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
    result["stderr"] = p.stderr.read().decode("utf-8", "replace")
    return logs, result


def check(name, cond, info=""):
    print(("ok  " if cond else "FAIL") + "  " + name + ("" if cond else "  -> " + str(info)))
    if not cond:
        raise SystemExit(1)


logs, r = run('<?php\n$x = 6 * 7;\necho "answer $x\\n";\nprint_r([1, 2]);\n')
check("runs PHP, echo goes to the log", r.get("reason") == "done" and "answer 42" in logs, (logs, r))
check("real PHP version reported", r.get("version", "").startswith("8."), r)
check("every message carries the numeric program id", "bad_id" not in r, r.get("bad_id"))

logs, r = run('while (($o = $drill->mine()) !== null) echo "got $o\\n";\necho $drill->status()["status"], "\\n";\n$hud->set("ore", 2);\n')
check("machine API: preset variable, method calls, arrays", logs[:2] == ["got iron_ore", "got iron_ore"] and "idle" in logs and 'hud:["ore", 2]' in logs, logs)
check("warnings from the game are logged", any("depleted" in l for l in logs), logs)

logs, r = run('$drill->explode();\n')
check("unknown machine function is an error with its line", r.get("reason") == "error" and "no function" in r.get("msg", "") and r.get("line") == 1, r)

logs, r = run('echo "a";\n$y = ;\n')
check("parse error reported with line", r.get("reason") == "error" and r["msg"].startswith("ParseError") and r.get("line") == 2, r)

logs, r = run('try { $energy->stats(); } catch (RuntimeException $e) { echo "caught ", $e->getMessage(), "\\n"; }\n')
check("API errors are catchable exceptions", "caught no power network" in logs, logs)

logs, r = run('sleep(2.5);\necho "woke\\n";\n')
check("sleep() waits in game time", r.get("slept") == 2.5 and "woke" in logs, (r, logs))

logs, r = run('$computer->on("tick", function ($e) { echo "tick ", $e["n"], "\\n"; if ($e["n"] >= 3) exit(0); });\n')
check("events: handlers run after the main body", logs[:3] == ["tick 1", "tick 2", "tick 3"] and r.get("reason") == "done", (logs, r))

for name, code in [
    ("files", 'var_dump(@file_get_contents("C:/Windows/win.ini"));'),
    ("SplFileObject", 'try { new SplFileObject("C:/Windows/win.ini"); echo "OPEN\\n"; } catch (Throwable $e) { echo "blocked\\n"; }'),
    ("directory listing", 'try { foreach (new DirectoryIterator("C:/") as $f) { echo "LISTED\\n"; break; } } catch (Throwable $e) { echo "blocked\\n"; }'),
    ("include", '$r = @include("C:/Windows/win.ini"); var_dump($r);'),
]:
    logs, r = run(code)
    check("sandbox: " + name, not any(s in " ".join(logs) for s in ("OPEN", "LISTED", "[fonts]", "for 16-bit")), (logs, r))

for fn in ("exec", "shell_exec", "proc_open", "fsockopen", "getenv", "ini_set", "file_put_contents", "putenv", "stream_socket_client"):
    logs, r = run(f'{fn}("x");\n')
    check("sandbox: " + fn + "() does not exist", r.get("reason") == "error" and "undefined function" in r.get("msg", ""), r)

logs, r = run('$a = [];\nwhile (true) $a[] = str_repeat("x", 1 << 20);\n', ram=16)
check("memory limit = computer RAM", r.get("reason") == "error" and "Out of memory" in r.get("msg", ""), r)

logs, r = run('echo double(21), " ", Box::make()->v, "\n";\n', files={"lib.php": "<?php\nfunction double($x) { return $x * 2; }\nclass Box { public $v = 7; static function make() { return new Box(); } }\n"})
check("several files: functions and classes from another file", logs == ["42 7"] and r.get("reason") == "done", (logs, r))
logs, r = run('broken();\n', files={"lib.php": "<?php\nfunction broken() {\n    throw new Exception('boom');\n}\n"})
check("error inside another file names the file and line", r.get("file") == "lib" and r.get("line") == 3 and "boom" in r.get("msg", ""), r)
logs, r = run('echo 1;\n', files={"bad.php": "<?php\n$x = ;\n"})
check("syntax error in another file", r.get("file") == "bad" and r.get("line") == 2, r)

code = 'function add($a, $b) {\n    $s = $a + $b;\n    return $s;\n}\n$items = [["count" => 3]];\n$t = add(1, 2);\n$t = add($t, 4);\necho $t, "\n";\n'
logs, r = run(code, debug=True, bps=[6], modes=["step", "out", "over", "continue"], watches=["$items[0]['count']", "t", "nope"])
lines = [p[0] for p in r.get("pauses", [])]
check("debugger: breakpoint, step into, out, over (PHP pauses after the statement on that line)", r.get("reason") == "done" and logs == ["7"] and lines[:1] == [6] and 2 in lines and lines[-1] in (7, 8), (lines, r))
inside = [p for p in r.get("pauses", []) if p[0] == 2]
check("debugger: call stack inside add()", inside and inside[0][2][0].startswith("add : line 2") and inside[0][2][-1].startswith("main : line 7"), r.get("pauses"))
check("debugger: watches on globals", r["pauses"][0][3][0] == ["$items[0]['count']", 3] and r["pauses"][0][3][2][1] == "(not defined here)", r.get("pauses"))

print("ALL PHP RUNTIME TESTS PASSED")
