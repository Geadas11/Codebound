# Integration test for the Python runtime host. Plays the role of Unity.
# Run: Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/python_host_test.py
import json, os, subprocess, sys, threading, time

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
HOST = os.path.join(ROOT, "Assets", "StreamingAssets", "Runtime", "py", "host.py")
PY = os.path.join(ROOT, "Tools", "python", "python.exe")


def run(code, ram=32, debug=False, bps=(), debug_modes=(), timeout=20):
    p = subprocess.Popen([PY, "-I", "-S", "-B", "-X", "utf8", HOST], stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                         stderr=subprocess.PIPE, cwd=os.path.dirname(HOST))
    logs, pauses, subs, result = [], [], set(), {}
    ore = [2]; ticks = [0]

    def send(m):
        p.stdin.write((json.dumps(m) + "\n").encode()); p.stdin.flush()

    t0 = time.time()
    killer = threading.Timer(timeout, p.kill); killer.start()
    for raw in p.stdout:
        m = json.loads(raw)
        t = m["t"]
        if t == "ready":
            send({"t": "start", "id": 1, "code": code, "ram": ram, "debug": debug, "bps": list(bps)})
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
            elif key == "energy.available": r["value"] = True
            elif key == "computer.subscribe": subs.add(m["args"][0]); r["value"] = True
            elif key == "computer.sleep": r["value"] = None
            elif key in ("hud.set", "hud.notify"): logs.append(key + ":" + json.dumps(m["args"])); r["value"] = True
            elif key == "computer.wait_event":
                ticks[0] += 1
                if ticks[0] > 3:
                    p.kill(); break                      # Unity stops Python programs by killing the process
                r["events"] = [{"name": "tick", "data": {"n": ticks[0]}}]
            elif key == "debug.pause":
                pauses.append((m["args"][0], m["args"][1]))
                r["value"] = {"mode": debug_modes[len(pauses) - 1] if len(pauses) <= len(debug_modes) else "continue"}
            else: r["error"] = "unknown API " + key
            send(r)
        elif t == "exit":
            result = m
    killer.cancel()
    p.wait()
    err = p.stderr.read().decode(errors="replace")
    return dict(result, logs=logs, pauses=pauses, subs=subs, ms=int((time.time() - t0) * 1000), stderr=err, code=p.returncode)


def check(cond, msg, r=None):
    if not cond:
        print("FAIL", msg, "\n", json.dumps({k: v for k, v in (r or {}).items() if k != "subs"}, default=str, ensure_ascii=False)[:1500])
        sys.exit(1)


r = run("log('hi ' + computer.name)\nn = 0\nwhile drill.mine():\n    n += 1\nprint('mined', n)")
check(r.get("reason") == "done" and r["logs"] == ["hi PC-1", "[warn] drill.mine(): depleted", "mined 2"], "continuous + blocking API + print", r)
print("ok  continuous program + blocking API calls + print()")

r = run("while True:\n    if energy.available():\n        if not drill.mine():\n            break\nlog('prompt example ok')")
check(r.get("reason") == "done" and r["logs"][-1] == "prompt example ok", "prompt example", r)
print("ok  the Prompt Mestre example (while True / energy.available / drill.mine)")

r = run("x = 0\nwhile True:\n    x += 1")
check(r.get("reason") == "error" and "CPU watchdog" in r.get("msg", ""), "watchdog", r)
print(f"ok  infinite loop stopped by watchdog after {r['ms']} ms")

r = run("a = []\nwhile True:\n    a.append(bytearray(1000000))", ram=16)
check(r.get("reason") == "error" and "memory" in r.get("msg", "").lower(), "memory limit", r)
print("ok  memory limit enforced: " + r["msg"])

r = run("log(1)\nx = = 2")
check(r.get("line") == 2 and "SyntaxError" in r.get("msg", ""), "syntax error line", r)
r = run("log(1)\nNone.boom")
check(r.get("line") == 2 and "AttributeError" in r.get("msg", ""), "runtime error line", r)
print("ok  syntax/runtime errors reported at the right line")

r = run("import math, json, random\nlog(math.sqrt(16), json.dumps([1]), 0 <= random.random() < 1)")
check(r["logs"] == ["4.0 [1] True"], "allowed imports", r)
print("ok  allowed modules (math, json, random...)")

escapes = {
    "open": "open('x.txt', 'w')",
    "import os": "import os",
    "import socket": "import socket",
    "__import__ subprocess": "__import__('subprocess')",
    "eval": "eval('1+1')",
    "os.system via subclasses": "w = [c for c in ().__class__.__base__.__subclasses__() if c.__name__ == '_wrap_close'][0]\nw.__init__.__globals__['system']('echo pwned')",
    "file read via os.open": "w = [c for c in ().__class__.__base__.__subclasses__() if c.__name__ == '_wrap_close'][0]\nw.__init__.__globals__['open']('C:/Windows/win.ini', 0)",
    "patch audit hook": "g = Machine.status.__globals__ if 'Machine' in dir() else drill.status.__func__.__globals__\ng['audit'].__code__ = (lambda e, a: None).__code__",
    "frame walking": "drill.status.__func__.__globals__['sys']._getframe(0)",
    "builtins open via module": "drill.status.__func__.__globals__['builtins'].open('x.txt', 'w')",
}
for name, code in escapes.items():
    r = run(code)
    check(r.get("reason") == "error" and any(k in r.get("msg", "") for k in ("PermissionError", "ImportError", "NameError")), "sandbox: " + name, r)
    check("pwned" not in r["stderr"] and not os.path.exists(os.path.join(os.path.dirname(HOST), "x.txt")), "side effect: " + name, r)
print(f"ok  sandbox: {len(escapes)} escape attempts blocked (files, imports, eval, os.system, hook patching, frames)")

r = run("computer.on('tick', lambda e: log('tick', e['n']))\n@computer.on('power_lost')\ndef alarm(e):\n    log('ALARM')\nlog('waiting')")
check(r["logs"] == ["waiting", "tick 1", "tick 2", "tick 3"] and r["subs"] == {"tick", "power_lost"}, "events", r)
print("ok  event mode (computer.on, also as decorator) + stop")

r = run("hud.set('ore', 12)\nhud.set('ok', True)\nhud.notify('hello')")
check(r["logs"] == ['hud.set:["ore", 12]', 'hud.set:["ok", true]', 'hud.notify:["hello"]'], "hud api", r)
print("ok  hud.set / hud.notify")

code = "total = 0\nitems = ['a', 'b']\nfor i in range(3):\n    total += i\ndef f(x):\n    y = x * 2\n    return y\nlog(f(total))"
r = run(code, debug=True, bps=[4, 6], debug_modes=["continue", "continue", "continue", "step", "continue"])
lines = [l for l, _ in r["pauses"]]
check(r.get("reason") == "done" and r["logs"] == ["6"] and lines == [4, 4, 4, 6, 7], "debugger lines " + str(lines), r)
check(r["pauses"][1][1].get("i") == 1 and r["pauses"][1][1].get("total") == 0 and r["pauses"][0][1].get("items") == ["a", "b"], "debugger vars", r)
check(r["pauses"][3][1].get("x") == 3 and r["pauses"][4][1].get("y") == 6 and "drill" not in r["pauses"][0][1], "debugger locals", r)
print("ok  debugger: breakpoints, step, variables (API names hidden)")

leftover = os.path.join(os.path.dirname(HOST), "x.txt")
check(not os.path.exists(leftover), "no files written")
print("ALL PYTHON RUNTIME TESTS PASSED")
