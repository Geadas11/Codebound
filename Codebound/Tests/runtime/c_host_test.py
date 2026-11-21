# Integration test for C and C++: clang (wasi-sdk) -> WebAssembly -> the Node host (host.js + wasm_worker.js),
# with the same compiler flags as ProgramManager.CompileC and the same Node permission flags as the game.
# Run: Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/c_host_test.py
import base64, json, os, shutil, subprocess, tempfile, threading

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SDK = os.path.join(ROOT, "Tools", "wasi-sdk")
RUNTIME_JS = os.path.join(ROOT, "Assets", "StreamingAssets", "Runtime", "js")
INCLUDE = os.path.join(ROOT, "Assets", "StreamingAssets", "Runtime", "c")


def compile_(code, lang, ram=32, files=None):
    d = tempfile.mkdtemp(prefix="cbcc")
    src = os.path.join(d, "computer.c" if lang == "c" else "computer.cpp")
    open(src, "w", encoding="utf-8").write(code)
    sources = [os.path.basename(src)]
    for name, text in (files or {}).items():
        open(os.path.join(d, name), "w", encoding="utf-8").write(text)
        if name.endswith(".c" if lang == "c" else ".cpp"): sources.append(name)
    std = ["-std=c17"] if lang == "c" else ["-std=c++17", "-fno-exceptions", "-fno-rtti"]
    args = [os.path.join(SDK, "bin", "clang.exe" if lang == "c" else "clang++.exe"), "--target=wasm32-wasip1",
            "--sysroot=" + os.path.join(SDK, "share", "wasi-sysroot"), "-O1", *std, "-isystem", INCLUDE,
            "-Wl,--import-memory", "-Wl,--export-memory", "-Wl,--initial-memory=4194304", f"-Wl,--max-memory={ram * 1048576}",
            "-Wl,-z,stack-size=524288", "-gdwarf-4", "-gline-tables-only", "-o", "computer.wasm", *sources]
    r = subprocess.run(args, capture_output=True, text=True, cwd=d)
    wasm = open(os.path.join(d, "computer.wasm"), "rb").read() if r.returncode == 0 else None
    shutil.rmtree(d, ignore_errors=True)
    return wasm, r.stderr, sources + [n for n in (files or {}) if n not in sources]


def run(code, lang="c", ram=32, timeout=30, files=None):
    wasm, err, sources = compile_(code, lang, ram, files)
    if wasm is None:
        return [], {"reason": "compile", "msg": err}
    node = shutil.which("node")
    p = subprocess.Popen([node, "--permission", "--allow-fs-read=" + RUNTIME_JS, "--allow-worker", "--disallow-code-generation-from-strings",
                          "--no-warnings", "--max-old-space-size=256", os.path.join(RUNTIME_JS, "host.js")],
                         stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=RUNTIME_JS)
    logs, result, ore = [], {}, [2]

    def send(m):
        p.stdin.write((json.dumps(m) + "\n").encode()); p.stdin.flush()

    killer = threading.Timer(timeout, p.kill); killer.start()
    for raw in p.stdout:
        m = json.loads(raw)
        t = m["t"]
        if t == "ready":
            send({"t": "start", "id": 1, "lang": lang, "wasm": base64.b64encode(wasm).decode(), "ram": ram, "sources": sources})
        elif t == "log":
            logs.append(m["msg"])
        elif t == "call":
            key = m["obj"] + "." + m["fn"]; r = {"t": "reply", "id": 1}
            if key == "machines.find": r["value"] = {"id": "m7", "type": "drill", "name": "Electric Drill"} if m["args"][0] == "drill" else None
            elif key == "m7.mine":
                r["value"] = "iron_ore" if ore[0] > 0 else None
                if ore[0] <= 0: r["warn"] = "drill.mine(): depleted"
                ore[0] -= 1
            elif key == "m7.status": r["value"] = {"status": "idle", "progress": 0.5}
            elif key == "computer.sleep": result["slept"] = m["args"][0]; r["value"] = None
            elif key == "computer.subscribe": r["value"] = True
            elif key == "computer.wait_event": r["value"] = None; r["events"] = [{"name": "tick", "data": {"n": 1}}]
            elif key == "energy.stats": r["error"] = "no power network"
            elif key == "hud.set": logs.append("hud:" + json.dumps(m["args"])); r["value"] = True
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


logs, r = run('#include <codebound.h>\n#include <math.h>\nint main(void) {\n  printf("answer %d %.3f\\n", 6 * 7, sqrt(2.0));\n  return 0;\n}\n')
check("C: printf goes to the log, libm works", r.get("reason") == "done" and logs == ["answer 42 1.414"], (logs, r))

logs, r = run('#include <codebound.h>\nint main(void) {\n  char d[32], item[64], st[256];\n  if (!cb_find("drill", d, sizeof d)) return 1;\n'
              '  while (cb_op(d, "mine", "[]", item, sizeof item) > 0) printf("got %s\\n", item);\n'
              '  cb_call(d, "status", "[]", st, sizeof st);\n  printf("%.1f\\n", cb_num(st, "progress"));\n  cb_hud_set("ore", "2");\n'
              '  if (cb_call("energy", "stats", "[]", st, sizeof st) < 0) printf("error: %s\\n", cb_error());\n  return 0;\n}\n')
check("C: machine API, JSON helpers, errors", logs[:2] == ["got iron_ore", "got iron_ore"] and "0.5" in logs and 'hud:["ore", "2"]' in logs
      and "error: no power network" in logs and any("depleted" in l for l in logs), logs)

logs, r = run('#include <codebound.h>\n#include <unistd.h>\nint main(void) { sleep(2); puts("woke"); cb_sleep(0.5); return 0; }\n')
check("C: sleep() waits in game time", "woke" in logs and r.get("slept") == 0.5, (logs, r))

logs, r = run('#include <codebound.h>\nint main(void) { char e[128]; cb_subscribe("tick"); cb_wait_event(e, sizeof e); puts(e); return 3; }\n')
check("C: events; non-zero exit code is an error", '{"name":"tick","data":{"n":1}}' in logs and r.get("reason") == "error" and "code 3" in r.get("msg", ""), (logs, r))

logs, r = run('#include <codebound.h>\n#include <string>\n#include <vector>\n#include <algorithm>\n#include <cstdio>\n'
              'int main() {\n  std::vector<int> v{3, 1, 2};\n  std::sort(v.begin(), v.end());\n  cb::Machine drill("drill");\n  std::string s;\n'
              '  while (!(s = drill.op("mine")).empty()) std::printf("got %s\\n", s.c_str());\n  std::printf("%d%d%d\\n", v[0], v[1], v[2]);\n}\n', lang="cpp")
check("C++: STL + cb:: wrapper", [l for l in logs if not l.startswith("[warn]")] == ["got iron_ore", "got iron_ore", "123"] and r.get("reason") == "done", (logs, r))

logs, r = run('int main(void) { int x = ; }\n')
check("compile errors are reported", r.get("reason") == "compile" and "computer.c:1" in r.get("msg", ""), r)

logs, r = run('#include <stdio.h>\nint main(void) { FILE *f = fopen("C:/Windows/win.ini", "r"); printf("%s\\n", f ? "OPEN" : "no files"); return 0; }\n')
check("sandbox: no file system", logs == ["no files"], (logs, r))

logs, r = run('int main(void) { volatile int *p = (int *)0x7fffffff; return *p; }\n')
check("sandbox: wild pointer is a trap, not a crash", r.get("reason") == "error" and "RuntimeError" in r.get("msg", ""), r)

logs, r = run('#include <stdlib.h>\n#include <stdio.h>\nstatic volatile char *keep[1000];\nint main(void) { for (int i = 0; i < 1000; i++) { keep[i] = malloc(1 << 20); if (!keep[i]) { puts("malloc failed"); return 0; } keep[i][0] = 1; } puts("TOO MUCH"); return 0; }\n', ram=16)
check("memory limit = computer RAM (malloc fails)", logs == ["malloc failed"], (logs, r))

logs, r = run('int main(void) { for (;;) { } }\n', timeout=15)
check("CPU watchdog stops infinite loops", r.get("reason") == "error" and "watchdog" in r.get("msg", ""), r)

logs, r = run('#include <stdio.h>\n#include "util.h"\nint main(void) {\n  printf("%d\\n", twice(21));\n  return 0;\n}\n', files={"util.h": "int twice(int x);\n", "util.c": "#include \"util.h\"\nint twice(int x) { return x * 2; }\n"})
check("several files: a .c file and its header", logs == ["42"] and r.get("reason") == "done", (logs, r))

logs, r = run('#include <stdio.h>\nint boom(int *p) {\n    return *p;\n}\nint main(void) {\n    puts("before");\n    return boom((int *)0x7ffffff0);\n}\n')
check("runtime error (trap) reports the source line", r.get("reason") == "error" and r.get("line") == 3 and r.get("file") is None, r)

logs, r = run('#include "lib.h"\nint main(void) { return crash(); }\n', files={"lib.h": "int crash(void);\n", "lib.c": "#include \"lib.h\"\nint crash(void) {\n    volatile int *p = (int *)0x7ffffff0;\n    return *p;\n}\n"})
check("trap inside another file: file + line", r.get("file") == "lib.c" and r.get("line") == 4, r)

logs, r = run('#include <vector>\n#include <cstdio>\nint main() {\n    std::vector<int> v{1};\n    int *p = (int *)0x7ffffff0;  // in WebAssembly address 0 is valid memory, so nullptr would not trap\n    std::printf("%d\\n", v[0] + *p);\n}\n', lang="cpp")
check("C++ trap reports the line", r.get("reason") == "error" and r.get("line") == 6, r)

print("ALL C/C++ RUNTIME TESTS PASSED")
