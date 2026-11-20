# Integration test for the SQL runtime (sql_host.py). Plays the role of Unity.
# Run: Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/sql_host_test.py
import json, os, subprocess, sys, threading, time

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
HOST = os.path.join(ROOT, "Assets", "StreamingAssets", "Runtime", "py", "sql_host.py")
PY = os.path.join(ROOT, "Tools", "python", "python.exe")

SNAP = {
    "time": 12.5,
    "machines": [{"id": "m7", "type": "drill", "name": "Electric Drill #7", "status": "idle", "power": 1.0, "busy": False, "distance": 3.2},
                 {"id": "m9", "type": "chest", "name": "Storage Chest #9", "status": "idle", "power": 1.0, "busy": False, "distance": 4.0}],
    "contents": [["m7", "output", "iron_ore", 3], ["m9", "items", "coal", 40]],
    "energy": {"capacityKW": 150, "demandKW": 20, "satisfaction": 1, "available": True},
    "events": [{"name": "item_mined", "data": {"machine": "m7", "item": "iron_ore"}}],
}


def run(code, db="", cycles=1, debug=False, bps=(), timeout=20):
    p = subprocess.Popen([PY, "-I", "-S", "-B", "-X", "utf8", HOST], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    out = {"logs": [], "cmds": [], "hud": [], "states": [], "pauses": [], "exit": None, "snaps": 0}
    killer = threading.Timer(timeout, p.kill); killer.start()

    def send(m):
        p.stdin.write((json.dumps(m) + "\n").encode()); p.stdin.flush()
    for raw in p.stdout:
        m = json.loads(raw); t = m["t"]
        if t == "ready": send({"t": "start", "id": 1, "code": code, "ram": 32, "db": db, "storage": 4096, "debug": debug, "bps": list(bps)})
        elif t == "log": out["logs"].append(m["msg"])
        elif t == "state": out["states"].append(m["db"])
        elif t == "exit": out["exit"] = m
        elif t == "call":
            key = m["obj"] + "." + m["fn"]; r = {"t": "reply", "id": 1, "value": None}
            if key == "sql.snapshot":
                out["snaps"] += 1
                if out["snaps"] > cycles: p.kill(); break
                r["value"] = SNAP
            elif key == "sql.commands": out["cmds"].extend(m["args"][0]); r["value"] = ["m99: no such machine"] if any(c[0] == "m99" for c in m["args"][0]) else []
            elif key == "hud.set": out["hud"].append(m["args"])
            elif key == "debug.pause": out["pauses"].append(m["args"]); r["value"] = {"mode": "continue"}
            send(r)
    killer.cancel(); p.wait()
    out["stderr"] = p.stderr.read().decode(errors="replace")
    return out


def check(cond, msg, r=None):
    if not cond:
        print("FAIL", msg, json.dumps(r, default=str)[:1500]); sys.exit(1)


r = run("-- @once\nSELECT id, type FROM machines ORDER BY id;\nSELECT SUM(count) AS ore FROM contents WHERE item = 'iron_ore';")
check(r["exit"] and r["exit"]["reason"] == "done", "once finishes", r)
check(any("m7" in l and "drill" in l for l in r["logs"]) and any("ore" in l and "3" in l for l in r["logs"]), "select output", r)
print("ok  reads live game tables (machines, contents) and prints SELECT results")

r = run("INSERT INTO commands SELECT id, 'mine', NULL FROM machines WHERE type = 'drill';\n"
        "INSERT INTO commands VALUES ('m99', 'start', NULL);\n"
        "INSERT OR REPLACE INTO hud VALUES ('ore', (SELECT count FROM contents WHERE item = 'iron_ore'));\n"
        "SELECT name FROM events;", cycles=2)
check(r["cmds"] == [["m7", "mine", None], ["m99", "start", None]] * 2, "commands each cycle", r)
check(["ore", "3"] in r["hud"] and "[warn] m99: no such machine" in r["logs"], "hud + warnings", r)
check(sum(1 for l in r["logs"] if "item_mined" in l) == 1, "unchanged SELECT result logged only once", r)
print("ok  controls the world via commands/hud tables; repeated cycles; quiet when results don't change")

r = run("-- @once\nCREATE TABLE IF NOT EXISTS runs(n INTEGER);\nINSERT INTO runs VALUES ((SELECT COUNT(*) FROM runs) + 1);")
check(r["states"] and "runs" in r["states"][-1], "state dump sent", r)
r2 = run("-- @once\nINSERT INTO runs VALUES ((SELECT COUNT(*) FROM runs) + 1);\nSELECT COUNT(*) AS total FROM runs;", db=r["states"][-1])
check(any("total" in l and "2" in l for l in r2["logs"]), "persistence", r2)
print("ok  player tables persist across restarts (saved with the computer)")

attacks = {
    "ATTACH file": "ATTACH DATABASE 'pwned.db' AS x;",
    "write game table": "DELETE FROM machines;",
    "drop game table": "DROP TABLE temp.machines;",
    "pragma": "PRAGMA writable_schema = 1;",
    "load_extension": "SELECT load_extension('evil');",
    "vacuum into file": "VACUUM INTO 'pwned.db';",
}
for name, sql in attacks.items():
    r = run("-- @once\n" + sql)
    check(r["exit"] and r["exit"]["reason"] == "error", "blocked: " + name, r)
    check(not os.path.exists("pwned.db") and not os.path.exists(os.path.join(os.path.dirname(HOST), "pwned.db")), "no file: " + name, r)
print(f"ok  sandbox: {len(attacks)} attacks blocked (ATTACH, VACUUM INTO, game-table writes, PRAGMA, extensions)")

t0 = time.time()
r = run("-- @once\nWITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x + 1 FROM n) SELECT COUNT(*) FROM n;")
check(r["exit"] and "CPU watchdog" in r["exit"]["msg"], "runaway query stopped", r)
print(f"ok  runaway recursive query stopped after {time.time() - t0:.1f}s")

r = run("-- @once\nSELECT 1;\nSELEC oops;")
check(r["exit"]["line"] == 3 and "syntax error" in r["exit"]["msg"], "syntax error line", r)
r = run("-- @once\n-- a comment\n\nSELEC oops;")
check(r["exit"]["line"] == 4, "line skips leading comments", r)
print("ok  SQL errors reported at the statement's line")

r = run("-- @once\nSELECT 1;\nSELECT 2;", debug=True, bps=[3])
check([p[0] for p in r["pauses"]] == [3] and r["pauses"][0][1]["statement"] == "SELECT 2;", "debugger", r)
print("ok  debugger pauses before the statement on a breakpoint line")
print("ALL SQL RUNTIME TESTS PASSED")
