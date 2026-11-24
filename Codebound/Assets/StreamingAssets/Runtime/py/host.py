# Codebound Python runtime host — ONE process per player program, started by Unity with
#   python -I -S -X utf8 host.py
# Protocol (one JSON object per line), same as the JavaScript runtime:
#   Unity -> host: {"t":"start",...} once, then {"t":"reply",...} answers to API calls
#   host -> Unity: {"t":"ready"} {"t":"call"} {"t":"log"} {"t":"stats"} {"t":"exit"}
# Defence in depth (Python cannot isolate code inside one interpreter):
#   1. separate OS process per program (Unity kills it on Stop)
#   2. Windows Job Object: memory limit = computer RAM, no child processes   (POSIX: RLIMIT_AS, best effort)
#   3. PEP 578 audit hook: blocks file, network, process, ctypes, frame/trace access from player code
#   4. restricted builtins (no open/eval/exec/compile/input) and an import allow-list
#   5. CPU watchdog thread: 3 s without calling the world API => program terminated
import sys, os, json, threading, time, types, builtins, re

PY_FILE = "computer.py"
WATCHDOG_S = 3.0
ALLOWED_MODULES = {
    "math", "random", "json", "collections", "itertools", "functools", "re", "string", "heapq", "bisect",
    "statistics", "dataclasses", "typing", "enum", "operator", "fractions", "decimal", "copy", "textwrap",
}

_out = sys.stdout.buffer
_in = sys.stdin.buffer
_out_lock = threading.Lock()


def send(obj):
    data = (json.dumps(obj, ensure_ascii=False) + "\n").encode("utf-8")
    with _out_lock:
        _out.write(data)
        _out.flush()


def read_msg():
    line = _in.readline()
    if not line:
        os._exit(0)                   # Unity went away: never outlive the game
    return json.loads(line.decode("utf-8"))


# ---------------------------------------------------------------- OS limits
_baseline = 0
_mem_query = None


def apply_limits(ram_mb):
    global _baseline, _mem_query
    if os.name == "nt":
        import ctypes
        from ctypes import wintypes
        k32 = ctypes.WinDLL("kernel32", use_last_error=True)

        class PMC(ctypes.Structure):
            _fields_ = [("cb", wintypes.DWORD), ("PageFaultCount", wintypes.DWORD)] + \
                       [(n, ctypes.c_size_t) for n in ("PeakWorkingSetSize", "WorkingSetSize", "QuotaPeakPagedPoolUsage",
                                                       "QuotaPagedPoolUsage", "QuotaPeakNonPagedPoolUsage", "QuotaNonPagedPoolUsage",
                                                       "PagefileUsage", "PeakPagefileUsage", "PrivateUsage")]

        class BASIC(ctypes.Structure):
            _fields_ = [("PerProcessUserTimeLimit", ctypes.c_int64), ("PerJobUserTimeLimit", ctypes.c_int64),
                        ("LimitFlags", wintypes.DWORD), ("MinimumWorkingSetSize", ctypes.c_size_t),
                        ("MaximumWorkingSetSize", ctypes.c_size_t), ("ActiveProcessLimit", wintypes.DWORD),
                        ("Affinity", ctypes.c_size_t), ("PriorityClass", wintypes.DWORD), ("SchedulingClass", wintypes.DWORD)]

        class IOC(ctypes.Structure):
            _fields_ = [(n, ctypes.c_uint64) for n in ("R", "W", "O", "RT", "WT", "OT")]

        class EXT(ctypes.Structure):
            _fields_ = [("Basic", BASIC), ("Io", IOC), ("ProcessMemoryLimit", ctypes.c_size_t),
                        ("JobMemoryLimit", ctypes.c_size_t), ("PeakProcessMemoryUsed", ctypes.c_size_t),
                        ("PeakJobMemoryUsed", ctypes.c_size_t)]

        k32.GetCurrentProcess.restype = wintypes.HANDLE
        k32.CreateJobObjectW.restype = wintypes.HANDLE
        k32.K32GetProcessMemoryInfo.argtypes = [wintypes.HANDLE, ctypes.POINTER(PMC), wintypes.DWORD]
        k32.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
        k32.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        me = k32.GetCurrentProcess()

        def private_bytes():
            pmc = PMC(); pmc.cb = ctypes.sizeof(PMC)
            k32.K32GetProcessMemoryInfo(me, ctypes.byref(pmc), pmc.cb)
            return pmc.PrivateUsage
        _mem_query = private_bytes
        _baseline = private_bytes()

        job = k32.CreateJobObjectW(None, None)
        info = EXT()
        info.Basic.LimitFlags = 0x100 | 0x8 | 0x2000   # PROCESS_MEMORY | ACTIVE_PROCESS | KILL_ON_JOB_CLOSE
        info.Basic.ActiveProcessLimit = 1               # no child processes
        info.ProcessMemoryLimit = _baseline + (ram_mb + 16) * 1024 * 1024
        ok = k32.SetInformationJobObject(job, 9, ctypes.byref(info), ctypes.sizeof(info)) and \
            k32.AssignProcessToJobObject(job, me)
        return bool(ok)
    try:
        import resource
        soft, hard = resource.getrlimit(resource.RLIMIT_AS)
        usage = resource.getrusage(resource.RUSAGE_SELF).ru_maxrss * 1024
        limit = usage + (ram_mb + 64) * 1024 * 1024
        resource.setrlimit(resource.RLIMIT_AS, (limit, hard))
        _baseline = usage
        return True
    except Exception:
        return False


# ---------------------------------------------------------------- audit hook
BLOCKED_PREFIXES = ("open", "os.", "subprocess.", "socket.", "ctypes.", "shutil.", "winreg.", "_winapi.", "msvcrt.",
                    "sys._getframe", "sys._current_frames", "sys.settrace", "sys.setprofile", "sys.monitoring",
                    "code.__new__", "function.__new__", "marshal.", "pickle.", "urllib.", "http.", "ftplib.",
                    "smtplib.", "webbrowser.", "glob.", "tempfile.", "sqlite3.", "mmap.", "signal.",
                    "_thread.start_new_thread", "builtins.input", "cpython._PySys_ClearAuditHooks", "socket")


# Everything the hook needs is bound as immutable default arguments (frozenset/tuple/types): player code can reach
# module globals through introspection, but cannot change these, and changing a function's __code__/__defaults__
# is itself an audited event that the hook refuses.
def audit(event, args, _allowed=frozenset(ALLOWED_MODULES), _blocked=BLOCKED_PREFIXES,
          _fn=types.FunctionType, _PE=PermissionError, _IE=ImportError):
    if event == "import":
        name = args[0]
        if type(name) is str and name.partition(".")[0] not in _allowed and name.partition(".")[0] != "encodings":
            raise _IE("module '" + name + "' is not available on Codebound computers")
        return
    if event == "object.__setattr__" and type(args[0]) is _fn:
        raise _PE("changing functions is not allowed on Codebound computers")
    if event.startswith(_blocked):
        raise _PE("'" + event + "' is not allowed on Codebound computers")


# ---------------------------------------------------------------- world API
class _State:
    busy_since = time.monotonic()
    handlers = {}
    queued = []
    dispatching = False
    debug = False
    bps = set()
    mode = "continue"          # debugger: continue | step | over | out
    pause_depth = 0
    watches = []
    log_count = 0


def _fmt(x):
    return x if isinstance(x, str) else str(x)   # same as Python's print()


def _log(*args, **_kw):
    _State.log_count += 1
    if _State.log_count <= 50:
        send({"t": "log", "id": PROGRAM_ID, "msg": " ".join(_fmt(a) for a in args)[:2000]})
    elif _State.log_count == 51:
        send({"t": "log", "id": PROGRAM_ID, "msg": "[runtime] log rate limit reached (50 lines/s), dropping output"})


def _dispatch(events):
    _State.queued.extend(events)
    if _State.dispatching:
        return
    _State.dispatching = True
    try:
        while _State.queued:
            e = _State.queued.pop(0)
            for fn in list(_State.handlers.get(e.get("name"), [])):
                fn(e.get("data"))
    finally:
        _State.dispatching = False


def _call(obj, fn, args=None):
    send({"t": "call", "id": PROGRAM_ID, "obj": obj, "fn": fn, "args": list(args or [])})
    _State.busy_since = None
    msg = read_msg()
    _State.busy_since = time.monotonic()
    if msg.get("bps") is not None:
        _State.bps = set(msg["bps"])
    if msg.get("warn"):
        send({"t": "log", "id": PROGRAM_ID, "msg": "[warn] " + msg["warn"]})
    if msg.get("events"):
        _dispatch(msg["events"])
    if msg.get("error"):
        raise RuntimeError(msg["error"])
    return msg.get("value")


class Machine:
    __slots__ = ("id", "type", "name")

    def __init__(self, info):
        object.__setattr__(self, "id", info["id"]); object.__setattr__(self, "type", info["type"])
        object.__setattr__(self, "name", info["name"])

    def __setattr__(self, k, v):
        raise AttributeError("machines are read-only")

    def __repr__(self):
        return f"<{self.type} {self.id}>"

    def __getattr__(self, name):
        allowed = {"drill": {"mine"}, "furnace": {"smelt"}, "conveyor": {"start", "stop", "set_filter"},
                   "inserter": {"start", "stop", "set_filter"}, "greenhouse": {"plant", "water", "harvest", "plots"},
                   "assembler": {"craft"}, "lab": {"research", "techs"}, "turret": {"targets", "fire"},
                   "launchpad": {"launch", "throttle", "flight", "planets", "travel", "land", "asteroids", "mine_asteroid", "dock", "answer", "transmit"},
                   "server": {"get", "set", "keys", "delete"}, "storage_hub": {"totals", "find"},
                   "pump": {"start", "stop"}, "pumpjack": {"start", "stop"}, "filler": {"start", "stop"}, "roboport": {"network"}}.get(self.type, set())
        if self.type in ("inserter", "long_inserter", "conveyor", "belt", "pump", "pumpjack", "filler", "lamp",
                         "constant_combinator", "arithmetic_combinator", "decider_combinator", "requester_chest"):
            allowed = allowed | {"configure", "signals"}
        if name not in allowed:
            raise AttributeError(f"'{self.type}' has no function '{name}'")
        return lambda *a: _call(self.id, name, list(a))

    def status(self):
        return _call(self.id, "status")

    def contents(self):
        return _call(self.id, "contents")


class _Machines:
    def list(self):
        return [Machine(i) for i in _call("machines", "list")]

    def get(self, mid):
        i = _call("machines", "get", [mid]); return Machine(i) if i else None

    def find(self, kind):
        i = _call("machines", "find", [kind]); return Machine(i) if i else None


class _Computer:
    def __init__(self, info):
        self.name = info["name"]; self.cps = info["cps"]

    def on(self, name, fn=None):
        """computer.on("tick", handler)  or as a decorator: @computer.on("tick")"""
        def register(f):
            if not callable(f):
                raise TypeError("computer.on(name, fn): fn must be a function")
            if name not in _State.handlers:
                _State.handlers[name] = []
                _call("computer", "subscribe", [str(name)])
            _State.handlers[name].append(f)
            return f
        return register(fn) if fn is not None else register

    def time(self):
        return _call("computer", "time")

    def send(self, channel, data=None):
        return _call("computer", "send", [str(channel), data])


class _Signals:
    def read(self):
        return _call("signals", "read")

    def send(self, values=None):
        return _call("signals", "send", [values or {}])


class _Energy:
    def available(self):
        return _call("energy", "available")

    def stats(self):
        return _call("energy", "stats")

    def network(self):
        return _call("energy", "network")


class _World:
    def pollution(self):
        return _call("world", "pollution")

    def weather(self):
        return _call("world", "weather")


class _Hud:
    def set(self, key, value):
        return _call("hud", "set", [str(key), value if isinstance(value, (str, int, float, bool)) or value is None else str(value)])

    def notify(self, text):
        return _call("hud", "notify", [str(text)])

    def alarm(self, text="ALARM"):
        return _call("hud", "alarm", [str(text)])


def _sleep(seconds=0):
    _call("computer", "sleep", [float(seconds)])


# ---------------------------------------------------------------- debugger (sys.settrace)
def _snap(v, depth=0):
    if v is None or isinstance(v, (bool, int)):
        return v
    if isinstance(v, float):
        return v if v == v and abs(v) != float("inf") else str(v)
    if isinstance(v, str):
        return v if len(v) <= 200 else v[:200] + "…"
    if isinstance(v, (types.FunctionType, types.BuiltinFunctionType, types.MethodType)):
        return f"ƒ {getattr(v, '__name__', 'function')}()"
    if isinstance(v, types.ModuleType):
        return f"module {v.__name__}"
    if isinstance(v, Machine):
        return repr(v)
    if depth >= 2:
        return f"{type(v).__name__}(…)"
    if isinstance(v, (list, tuple, set, frozenset)):
        items = list(v)
        out = [_snap(x, depth + 1) for x in items[:20]]
        if len(items) > 20:
            out.append(f"… {len(items) - 20} more")
        return out
    if isinstance(v, dict):
        out = {}
        for i, (k, x) in enumerate(v.items()):
            if i >= 20:
                out["…"] = "more"; break
            out[str(k)] = _snap(x, depth + 1)
        return out
    r = repr(v)
    return r if len(r) <= 200 else r[:200] + "…"


API_NAMES = set()


_PATH = re.compile(r"^([A-Za-z_]\w*)((?:\.[A-Za-z]\w*|\[\d+\]|\[\"[^\"]*\"\]|\['[^']*'\])*)$")
_PART = re.compile(r"\.[A-Za-z]\w*|\[\d+\]|\[\"[^\"]*\"\]|\['[^']*'\]")


def _watch(expr, frame):
    """Watch expressions are a name and a path (items[0].count): no eval, and no _private attributes."""
    m = _PATH.match(str(expr).strip())
    if not m:
        return "(use a name or a path, e.g. items[0].count)"
    name = m.group(1)
    if name in frame.f_locals:
        v = frame.f_locals[name]
    elif name in frame.f_globals:
        v = frame.f_globals[name]
    else:
        return "(not defined here)"
    try:
        for p in _PART.findall(m.group(2)):
            v = getattr(v, p[1:]) if p[0] == "." else v[int(p[1:-1])] if p[1].isdigit() else v[p[2:-2]]
    except Exception as e:
        return f"({type(e).__name__}: {e})"
    return _snap(v)


def _frames(frame):
    out = []
    while frame is not None:
        if frame.f_code.co_filename == PY_FILE:
            out.append(("main" if frame.f_code.co_name == "<module>" else frame.f_code.co_name) + " : line " + str(frame.f_lineno))
        frame = frame.f_back
    return out


def _tracer(frame, event, arg):
    if frame.f_code.co_filename != PY_FILE:
        return None
    if event == "line":
        stack = _frames(frame)
        depth, m = len(stack), _State.mode
        if frame.f_lineno in _State.bps or m == "step" or (m == "over" and depth <= _State.pause_depth) or (m == "out" and depth < _State.pause_depth):
            vars_ = {k: _snap(v) for k, v in frame.f_locals.items()
                     if not k.startswith("__") and k not in API_NAMES and not isinstance(v, types.ModuleType)}
            watches = [[w, _watch(w, frame)] for w in _State.watches]
            r = _call("debug", "pause", [frame.f_lineno, vars_, stack, watches])
            _State.mode = (r.get("mode") if isinstance(r, dict) else None) or "continue"
            _State.pause_depth = depth
            if isinstance(r, dict) and isinstance(r.get("watches"), list):
                _State.watches = [str(x) for x in r["watches"]]
    return _tracer


# ---------------------------------------------------------------- threads: watchdog + stats
def _watchdog():
    while True:
        time.sleep(0.25)
        since = _State.busy_since
        if since is not None and time.monotonic() - since > WATCHDOG_S:
            send({"t": "exit", "id": PROGRAM_ID, "reason": "error", "line": None,
                  "msg": f"CPU watchdog: ran {WATCHDOG_S:g}s without calling the world API or sleep() — infinite loop?"})
            os._exit(3)


def _stats(ram_mb):
    last_cpu, last_wall = time.process_time(), time.monotonic()
    while True:
        time.sleep(1.0)
        cpu, wall = time.process_time(), time.monotonic()
        pct = min(100, round(100 * (cpu - last_cpu) / max(1e-6, wall - last_wall)))
        last_cpu, last_wall = cpu, wall
        heap = None
        try:
            if _mem_query:
                heap = round(max(0, _mem_query() - _baseline) / 1048576, 1)
        except Exception:
            pass
        _State.log_count = 0
        send({"t": "stats", "id": PROGRAM_ID, "cpu": pct, "heapMb": heap, "ramMb": ram_mb})


# ---------------------------------------------------------------- main
def _error_at(exc):
    """(file, line) of the error in the player's code: file None = main file."""
    names = {PY_FILE} | {n + ".py" for n in PLAYER_FILES}
    if isinstance(exc, SyntaxError) and exc.filename in names:
        return (None if exc.filename == PY_FILE else exc.filename[:-3]), exc.lineno
    at, tb = (None, None), exc.__traceback__
    while tb is not None:
        f = tb.tb_frame.f_code.co_filename
        if f in names:
            at = (None if f == PY_FILE else f[:-3]), tb.tb_lineno
        tb = tb.tb_next
    return at


PLAYER_FILES = {}      # other files on the computer: name (without .py) -> source
_player_modules = {}
_module_globals = {}   # API names and safe builtins for player modules (set in main)


def _player_module(name):
    if name not in _player_modules:
        mod = types.ModuleType(name)
        mod.__dict__.update(_module_globals)
        _player_modules[name] = mod   # registered first: circular imports see the partial module, like CPython
        exec(compile(PLAYER_FILES[name], name + ".py", "exec"), mod.__dict__)
    return _player_modules[name]


def _safe_import(name, globals=None, locals=None, fromlist=(), level=0):
    if level == 0 and name in PLAYER_FILES:
        return _player_module(name)
    if level != 0 or name.split(".")[0] not in ALLOWED_MODULES:
        raise ImportError(f"module '{name}' is not available on Codebound computers "
                          f"(allowed: {', '.join(sorted(ALLOWED_MODULES))})")
    return __import__(name, globals, locals, fromlist, level)


def main():
    global PROGRAM_ID
    send({"t": "ready", "version": sys.version.split()[0]})
    start = read_msg()
    PROGRAM_ID = start.get("id")
    ram = max(8, min(1024, int(start.get("ram") or 32)))
    code = str(start.get("code") or "")
    _State.debug = bool(start.get("debug"))
    _State.bps = set(start.get("bps") or [])
    _State.watches = [str(w) for w in (start.get("watches") or [])]
    for fname, src in (start.get("files") or {}).items():
        n = str(fname)[:-3] if str(fname).endswith(".py") else str(fname)
        if n.isidentifier():
            PLAYER_FILES[n] = str(src)

    limited = apply_limits(ram)
    for m in ALLOWED_MODULES:            # import before the hook so their own internal imports/opens succeed
        __import__(m)
    threading.Thread(target=_watchdog, daemon=True).start()
    threading.Thread(target=_stats, args=(ram,), daemon=True).start()

    try:
        compiled = compile(code, PY_FILE, "exec")
    except SyntaxError as e:
        send({"t": "exit", "id": PROGRAM_ID, "reason": "error", "msg": f"SyntaxError: {e.msg}", "line": e.lineno, "file": None}); return

    safe_builtins = {k: v for k, v in vars(builtins).items()
                     if k not in ("open", "eval", "exec", "compile", "input", "breakpoint", "help", "exit", "quit", "__import__", "__loader__", "__spec__")}
    safe_builtins["__import__"] = _safe_import
    safe_builtins["print"] = _log

    info = _call("computer", "info")
    g = {"__name__": "__main__", "__builtins__": safe_builtins,
         "log": _log, "sleep": _sleep, "computer": _Computer(info), "machines": _Machines(), "energy": _Energy(), "signals": _Signals(), "world": _World(), "hud": _Hud()}
    for t in ("drill", "furnace", "generator", "chest", "conveyor", "inserter", "greenhouse", "assembler", "lab", "turret"):
        i = _call("machines", "find", [t])
        g[t] = Machine(i) if i else None
    i = _call("machines", "find", ["launchpad"])
    g["pad"] = Machine(i) if i else None
    API_NAMES.update(g.keys())
    _module_globals.update(g)
    _module_globals.pop("__name__", None)
    if not limited:
        _log("[runtime] warning: OS memory limit could not be applied on this platform")

    if _State.debug:
        sys.settrace(_tracer)            # must happen before the hook, which forbids sys.settrace
    sys.addaudithook(audit)              # from here on, player code runs under the hook
    try:
        _State.busy_since = time.monotonic()
        exec(compiled, g)
        while _State.handlers:           # event mode: keep serving events after the main body ends
            _call("computer", "wait_event")
        send({"t": "exit", "id": PROGRAM_ID, "reason": "done", "msg": None, "line": None})
    except MemoryError:
        send({"t": "exit", "id": PROGRAM_ID, "reason": "error", "msg": f"Out of memory: program exceeded {ram} MB RAM", "line": None})
    except BaseException as e:           # includes SystemExit/KeyboardInterrupt raised by player code
        f, ln = _error_at(e)
        send({"t": "exit", "id": PROGRAM_ID, "reason": "error", "msg": f"{type(e).__name__}: {e}", "line": ln, "file": f})
    finally:
        _out.flush()
        os._exit(0)


PROGRAM_ID = None
if __name__ == "__main__":
    main()
