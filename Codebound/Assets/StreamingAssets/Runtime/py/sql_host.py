# Codebound SQL runtime — one process per program, reusing host.py's protocol, OS limits and audit hook.
# The player's SQL script runs once per cycle (default every 1 s; "-- @every N" or "-- @once").
# Each cycle sees read-only TEMP tables with live game data and controls the world by writing rows:
#   machines(id, type, name, status, power, busy, distance)   contents(machine, role, item, count)
#   energy(capacity_kw, demand_kw, satisfaction, available)    events(name, machine, item, time)
#   game(time, cycle, pollution)
#   commands(machine, action, arg)  -> executed by the game after the cycle (start, stop, set_filter, mine, smelt)
#   hud(key, value)                 -> published like hud.set(key, value)
# Tables the player creates live in the main database and are saved with the computer (storage limit applies).
import os, re, sqlite3, sqlite3.dump, sys, time   # sqlite3.dump is lazily imported by iterdump(): load it before the audit hook
sys.dont_write_bytecode = True                                    # never write __pycache__ next to the runtime
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))   # -I removes the script dir from sys.path
import host as H

CYCLE_BUDGET_S = 2.0
GAME_TABLES = ("machines", "contents", "energy", "events", "game", "creatures")
SAFE_PRAGMAS = {"table_info", "table_list", "index_list", "index_info", "foreign_keys", "user_version"}
DIRECTIVE = re.compile(r"^\s*--\s*@(every|once)\s*([0-9.]*)", re.I | re.M)


class Sql:
    host_mode = False      # True while the host itself refreshes the game tables
    deadline = 0.0


def authorizer(action, arg1, arg2, dbname, source):
    if Sql.host_mode:
        return sqlite3.SQLITE_OK
    if action in (sqlite3.SQLITE_ATTACH, sqlite3.SQLITE_DETACH):
        return sqlite3.SQLITE_DENY
    if action == sqlite3.SQLITE_PRAGMA and (arg1 or "").lower() not in SAFE_PRAGMAS:
        return sqlite3.SQLITE_DENY
    if action == sqlite3.SQLITE_FUNCTION and (arg2 or "").lower() in ("load_extension", "readfile", "writefile", "fts3_tokenizer"):
        return sqlite3.SQLITE_DENY
    if action in (sqlite3.SQLITE_INSERT, sqlite3.SQLITE_UPDATE, sqlite3.SQLITE_DELETE) and (arg1 or "").lower() in GAME_TABLES:
        return sqlite3.SQLITE_DENY
    if action in (sqlite3.SQLITE_DROP_TEMP_TABLE, sqlite3.SQLITE_DROP_TABLE, sqlite3.SQLITE_ALTER_TABLE) and (arg1 or "").lower() in GAME_TABLES + ("commands", "hud"):
        return sqlite3.SQLITE_DENY
    return sqlite3.SQLITE_OK


def progress():
    if Sql.host_mode:
        return 0
    return 1 if time.monotonic() > Sql.deadline else 0   # non-zero aborts the running statement


def split_statements(script):
    """Returns [(line, statement)] using SQLite's own completeness check (strings/comments with ';' are fine)."""
    out, buf, start_line, line = [], "", None, 1
    for ch in script:
        if start_line is None and not ch.isspace():
            start_line = line
        buf += ch
        if ch == "\n":
            line += 1
        if ch == ";" and sqlite3.complete_statement(buf):
            _add(out, start_line, buf)
            buf, start_line = "", None
    if buf.strip():
        _add(out, start_line or line, buf)
    return out


def _add(out, start_line, buf):
    """Keeps statements that contain code; reports the line of the first code line, not of leading comments."""
    lines = buf.strip().splitlines()
    skip = 0
    while skip < len(lines) and (not lines[skip].strip() or lines[skip].strip().startswith("--")):
        skip += 1
    if skip < len(lines):
        out.append((start_line + skip, "\n".join(lines[skip:]).strip()))


def fmt_table(cur, rows):
    cols = [d[0] for d in cur.description]
    shown = rows[:10]
    w = [min(18, max([len(c)] + [len(str(r[i])) for r in shown])) for i, c in enumerate(cols)]
    lines = [" | ".join(c[:18].ljust(w[i]) for i, c in enumerate(cols))]
    lines += [" | ".join(str(v)[:18].ljust(w[i]) for i, v in enumerate(r)) for r in shown]
    if len(rows) > 10:
        lines.append(f"... {len(rows) - 10} more rows")
    return "\n".join(lines) if rows else " | ".join(cols) + "\n(no rows)"


def refresh(db, snap, cycle):
    Sql.host_mode = True
    try:
        db.executescript("""
            DROP TABLE IF EXISTS temp.machines; DROP TABLE IF EXISTS temp.contents; DROP TABLE IF EXISTS temp.energy;
            DROP TABLE IF EXISTS temp.events; DROP TABLE IF EXISTS temp.game; DROP TABLE IF EXISTS temp.creatures;
            CREATE TEMP TABLE machines(id TEXT, type TEXT, name TEXT, status TEXT, power REAL, busy INTEGER, distance REAL);
            CREATE TEMP TABLE contents(machine TEXT, role TEXT, item TEXT, count INTEGER);
            CREATE TEMP TABLE energy(capacity_kw REAL, demand_kw REAL, satisfaction REAL, available INTEGER);
            CREATE TEMP TABLE events(name TEXT, machine TEXT, item TEXT, time REAL);
            CREATE TEMP TABLE game(time REAL, cycle INTEGER, pollution REAL);
            CREATE TEMP TABLE creatures(id INTEGER, type TEXT, health REAL, distance REAL);
        """)
        db.executemany("INSERT INTO temp.machines VALUES (?,?,?,?,?,?,?)",
                       [(m["id"], m["type"], m["name"], m["status"], m["power"], 1 if m["busy"] else 0, m["distance"]) for m in snap["machines"]])
        db.executemany("INSERT INTO temp.contents VALUES (?,?,?,?)", [tuple(c) for c in snap["contents"]])
        e = snap["energy"]
        db.execute("INSERT INTO temp.energy VALUES (?,?,?,?)", (e["capacityKW"], e["demandKW"], e["satisfaction"], 1 if e["available"] else 0))
        db.executemany("INSERT INTO temp.events VALUES (?,?,?,?)",
                       [(ev.get("name"), (ev.get("data") or {}).get("machine"), (ev.get("data") or {}).get("item"), snap["time"]) for ev in snap["events"]])
        db.execute("INSERT INTO temp.game VALUES (?,?,?)", (snap["time"], cycle, snap.get("pollution", 0)))
        db.executemany("INSERT INTO temp.creatures VALUES (?,?,?,?)", [(c["id"], c["type"], c["health"], c["distance"]) for c in snap.get("creatures", [])])
        db.commit()
    finally:
        Sql.host_mode = False


def user_dump(db):
    Sql.host_mode = True
    try:
        return "\n".join(l for l in db.iterdump() if not l.startswith(("BEGIN", "COMMIT")))
    finally:
        Sql.host_mode = False


def main():
    H.send({"t": "ready", "version": "sqlite " + sqlite3.sqlite_version})
    start = H.read_msg()
    H.PROGRAM_ID = start.get("id")
    ram = max(8, min(1024, int(start.get("ram") or 32)))
    code = str(start.get("code") or "")
    storage = int(start.get("storage") or 65536)
    bps = set(start.get("bps") or [])
    debug = bool(start.get("debug"))
    H.apply_limits(ram)
    H.threading.Thread(target=H._watchdog, daemon=True).start()
    H.threading.Thread(target=H._stats, args=(ram,), daemon=True).start()

    db = sqlite3.connect(":memory:", isolation_level=None)
    saved = start.get("db") or ""
    if saved:   # restore first: the dump already contains the commands/hud tables
        try:
            Sql.host_mode = True
            db.executescript(saved)
        except sqlite3.Error as e:
            H._log(f"[runtime] saved database could not be restored: {e}")
        finally:
            Sql.host_mode = False
    db.execute("CREATE TABLE IF NOT EXISTS commands(machine TEXT, action TEXT, arg TEXT)")
    db.execute("CREATE TABLE IF NOT EXISTS hud(key TEXT PRIMARY KEY, value TEXT)")
    db.set_authorizer(authorizer)
    db.set_progress_handler(progress, 2000)
    sys.addaudithook(H.audit)

    m = DIRECTIVE.search(code)
    once = bool(m and m.group(1).lower() == "once")
    every = float(m.group(2)) if m and m.group(1).lower() == "every" and m.group(2) else 1.0
    every = max(0.2, min(3600.0, every))
    statements = split_statements(code)
    if not statements:
        H.send({"t": "exit", "id": H.PROGRAM_ID, "reason": "done", "msg": None, "line": None}); return
    for name in ("item_mined", "item_smelted", "power_lost", "power_restored"):
        H._call("computer", "subscribe", [name])

    last_out, last_dump, cycle = {}, user_dump(db), 0
    stepping = False
    try:
        while True:
            cycle += 1
            snap = H._call("sql", "snapshot")
            refresh(db, snap, cycle)
            Sql.deadline = time.monotonic() + CYCLE_BUDGET_S
            H._State.busy_since = time.monotonic()
            for line, stmt in statements:
                if debug and (stepping or line in bps):
                    r = H._call("debug", "pause", [line, {"statement": stmt[:200], "cycle": cycle}])
                    stepping = bool(r and r.get("mode") == "step")
                    Sql.deadline = time.monotonic() + CYCLE_BUDGET_S
                try:
                    cur = db.execute(stmt)
                    if cur.description is not None:
                        out = fmt_table(cur, cur.fetchall())
                        if last_out.get(line) != out:      # only log a SELECT when its result changes
                            last_out[line] = out
                            H._log(f"[line {line}]\n" + out)
                except sqlite3.OperationalError as e:
                    if "interrupted" in str(e):
                        raise TimeoutError(f"CPU watchdog: the cycle ran over {CYCLE_BUDGET_S:g}s (a query that never ends?)") from None
                    raise _SqlError(line, e)
                except sqlite3.Error as e:
                    raise _SqlError(line, e)
            # world effects of this cycle
            Sql.host_mode = True
            cmds = db.execute("SELECT machine, action, arg FROM commands").fetchall()
            huds = db.execute("SELECT key, value FROM hud").fetchall()
            db.execute("DELETE FROM commands"); db.execute("DELETE FROM hud")
            Sql.host_mode = False
            if cmds:
                for w in H._call("sql", "commands", [[list(c) for c in cmds]]) or []:
                    H._log("[warn] " + w)
            for k, v in huds:
                if str(k) == "alarm":   # INSERT INTO hud VALUES ('alarm', 'text') sounds a programmable alarm
                    H._call("hud", "alarm", ["" if v is None else str(v)])
                else:
                    H._call("hud", "set", [str(k), "" if v is None else v])
            dump = user_dump(db)
            if dump != last_dump:
                if len(dump.encode("utf-8")) > storage:
                    raise _SqlError(None, f"database larger than this computer's storage ({storage // 1024} KB)")
                last_dump = dump
                H.send({"t": "state", "id": H.PROGRAM_ID, "db": dump})
            if once:
                break
            H._sleep(every)
        H.send({"t": "exit", "id": H.PROGRAM_ID, "reason": "done", "msg": None, "line": None})
    except _SqlError as e:
        H.send({"t": "exit", "id": H.PROGRAM_ID, "reason": "error", "msg": e.msg, "line": e.line})
    except MemoryError:
        H.send({"t": "exit", "id": H.PROGRAM_ID, "reason": "error", "msg": f"Out of memory: program exceeded {ram} MB RAM", "line": None})
    except BaseException as e:
        H.send({"t": "exit", "id": H.PROGRAM_ID, "reason": "error", "msg": f"{type(e).__name__}: {e}", "line": None})
    finally:
        H._out.flush()
        H.os._exit(0)


class _SqlError(Exception):
    def __init__(self, line, err):
        self.line = line
        text = str(err)
        if "not authorized" in text:
            text += " (game tables are read-only; ATTACH, most PRAGMAs and extensions are disabled)"
        self.msg = ("SQL error: " if isinstance(err, Exception) else "") + text


if __name__ == "__main__":
    main()
