<?php
// Codebound PHP runtime host — ONE process per player program, started by Unity (ProgramManager.StartPhp) with
//   php -n -d open_basedir=<missing dir> -d disable_functions=<list> -d memory_limit=<RAM>M host.php
// Protocol (one JSON object per line), same as the Python and JavaScript runtimes:
//   Unity -> host: {"t":"start",...} once, then {"t":"reply",...} answers to API calls
//   host -> Unity: {"t":"ready"} {"t":"call"} {"t":"log"} {"t":"stats"} {"t":"exit"}
// Defence in depth: separate process in a Windows Job Object (memory, no child processes, killed with the game),
// no php.ini and no extensions (-n), every file/directory access refused by open_basedir, process/network/env
// functions removed (disable_functions), and a CPU watchdog in Unity (3 s without calling the world API).

const CB_SOURCE = 'computer.php';
$CB_ID = null;
$CB_HANDLERS = [];
$CB_QUEUE = [];
$CB_DISPATCHING = false;
$CB_EXITED = false;
$CB_LOG = ['count' => 0, 'since' => microtime(true), 'partial' => ''];
$CB_STATS = ['at' => microtime(true), 'cpu' => 0.0];
$CB_RAM = 32;

function cb_send(array $m): void
{
    fwrite(STDOUT, json_encode($m, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES | JSON_PARTIAL_OUTPUT_ON_ERROR) . "\n");
    fflush(STDOUT);
}

function cb_read(): array
{
    $line = fgets(STDIN);
    if ($line === false) exit(0);   // Unity went away: never outlive the game
    $m = json_decode($line, true);
    return is_array($m) ? $m : [];
}

function cb_log_line(string $msg): void
{
    global $CB_LOG, $CB_ID;
    $now = microtime(true);
    if ($now - $CB_LOG['since'] >= 1.0) { $CB_LOG['count'] = 0; $CB_LOG['since'] = $now; }
    $CB_LOG['count']++;
    if ($CB_LOG['count'] <= 50) cb_send(['t' => 'log', 'id' => $CB_ID, 'msg' => substr($msg, 0, 2000)]);
    elseif ($CB_LOG['count'] === 51) cb_send(['t' => 'log', 'id' => $CB_ID, 'msg' => '[runtime] log rate limit reached (50 lines/s), dropping output']);
}

// echo / print / printf output goes to the computer's log, one line per newline
function cb_output(string $chunk, int $phase): string
{
    global $CB_LOG;
    $text = $CB_LOG['partial'] . $chunk;
    $lines = explode("\n", $text);
    $CB_LOG['partial'] = array_pop($lines);
    if ($phase & PHP_OUTPUT_HANDLER_FINAL && $CB_LOG['partial'] !== '') { $lines[] = $CB_LOG['partial']; $CB_LOG['partial'] = ''; }
    foreach ($lines as $l) cb_log_line(rtrim($l, "\r"));
    return '';
}

function cb_stats(): void
{
    global $CB_STATS, $CB_ID, $CB_RAM;
    $now = microtime(true);
    if ($now - $CB_STATS['at'] < 1.0) return;
    $ru = getrusage();
    $cpu = ($ru['ru_utime.tv_sec'] ?? 0) + ($ru['ru_utime.tv_usec'] ?? 0) / 1e6;
    $pct = (int)min(100, round(100 * ($cpu - $CB_STATS['cpu']) / max(1e-6, $now - $CB_STATS['at'])));
    $CB_STATS = ['at' => $now, 'cpu' => $cpu];
    cb_send(['t' => 'stats', 'id' => $CB_ID, 'cpu' => $pct, 'heapMb' => round(memory_get_usage(true) / 1048576, 1), 'ramMb' => $CB_RAM]);
}

function cb_dispatch(array $events): void
{
    global $CB_QUEUE, $CB_DISPATCHING, $CB_HANDLERS;
    array_push($CB_QUEUE, ...$events);
    if ($CB_DISPATCHING) return;
    $CB_DISPATCHING = true;
    try {
        while ($CB_QUEUE) {
            $e = array_shift($CB_QUEUE);
            foreach ($CB_HANDLERS[$e['name'] ?? ''] ?? [] as $fn) $fn($e['data'] ?? null);
        }
    } finally { $CB_DISPATCHING = false; }
}

/** Calls the world API and waits for Unity's answer. */
function cb_call(string $obj, string $fn, array $args = [])
{
    global $CB_ID;
    if (ob_get_level() > 0) ob_flush();
    cb_stats();
    cb_send(['t' => 'call', 'id' => $CB_ID, 'obj' => $obj, 'fn' => $fn, 'args' => array_values($args)]);
    $m = cb_read();
    if (!empty($m['warn'])) cb_log_line('[warn] ' . $m['warn']);
    if (!empty($m['events'])) cb_dispatch($m['events']);
    if (!empty($m['error'])) throw new RuntimeException($m['error']);
    return $m['value'] ?? null;
}

final class Machine
{
    const OPS = ['drill' => ['mine'], 'furnace' => ['smelt'], 'conveyor' => ['start', 'stop', 'set_filter'],
        'inserter' => ['start', 'stop', 'set_filter'], 'greenhouse' => ['plant', 'water', 'harvest', 'plots'],
        'assembler' => ['craft'], 'lab' => ['research', 'techs'], 'turret' => ['targets', 'fire'],
        'launchpad' => ['launch', 'throttle', 'flight', 'planets', 'travel', 'land', 'asteroids', 'mine_asteroid', 'dock', 'answer', 'transmit'], 'server' => ['get', 'set', 'keys', 'delete'], 'storage_hub' => ['totals', 'find']];
    public readonly string $id;
    public readonly string $type;
    public readonly string $name;
    public function __construct(array $info) { $this->id = $info['id']; $this->type = $info['type']; $this->name = $info['name']; }
    public function status() { return cb_call($this->id, 'status'); }
    public function contents() { return cb_call($this->id, 'contents'); }
    public function __call(string $op, array $args)
    {
        if (!in_array($op, self::OPS[$this->type] ?? [], true)) throw new BadMethodCallException("'{$this->type}' has no function '$op'");
        return cb_call($this->id, $op, $args);
    }
    public function __toString(): string { return "<{$this->type} {$this->id}>"; }
}

final class Machines
{
    /** @return Machine[] */
    public function list(): array { return array_map(fn($i) => new Machine($i), cb_call('machines', 'list') ?? []); }
    public function get(string $id): ?Machine { $i = cb_call('machines', 'get', [$id]); return $i ? new Machine($i) : null; }
    public function find(string $type): ?Machine { $i = cb_call('machines', 'find', [$type]); return $i ? new Machine($i) : null; }
}

final class Computer
{
    public function __construct(public readonly string $name, public readonly int $cps) {}
    public function on(string $name, callable $fn): void
    {
        global $CB_HANDLERS;
        if (!isset($CB_HANDLERS[$name])) { $CB_HANDLERS[$name] = []; cb_call('computer', 'subscribe', [$name]); }
        $CB_HANDLERS[$name][] = $fn;
    }
    public function time() { return cb_call('computer', 'time'); }
    public function send(string $channel, $data = null) { return cb_call('computer', 'send', [$channel, $data]); }
}

final class Energy { public function available() { return cb_call('energy', 'available'); } public function stats() { return cb_call('energy', 'stats'); } }
final class World { public function pollution() { return cb_call('world', 'pollution'); } public function weather() { return cb_call('world', 'weather'); } }
final class Hud
{
    public function set(string $key, $value) { return cb_call('hud', 'set', [$key, is_scalar($value) || $value === null ? $value : json_encode($value)]); }
    public function notify(string $text) { return cb_call('hud', 'notify', [$text]); }
    public function alarm(string $text = 'ALARM') { return cb_call('hud', 'alarm', [$text]); }
}

// the built-in sleep() is disabled on Codebound computers; this one waits in game time without burning CPU
function sleep($seconds = 0) { cb_call('computer', 'sleep', [(float)$seconds]); return 0; }
function usleep($microseconds) { cb_call('computer', 'sleep', [$microseconds / 1e6]); }

function cb_finish(string $reason, ?string $msg = null, ?int $line = null, ?string $file = null): void
{
    global $CB_EXITED, $CB_ID;
    if ($CB_EXITED) return;
    $CB_EXITED = true;
    while (ob_get_level() > 0) ob_end_flush();
    cb_send(['t' => 'exit', 'id' => $CB_ID, 'reason' => $reason, 'msg' => $msg, 'line' => $line, 'file' => $file]);
}

// ---------------------------------------------------------------- several files
// The main file runs at the top level (one eval); the computer's other files are evaluated first, inside
// cb_run_module, so errors can tell the two apart by the eval site. Functions/classes are mapped to their file.
$CB_FILES = [];
$CB_DEFINED_IN = [];      // lower-case function or class name => file
$CB_LOADING = null;
$CB_SITE_MAIN = 0;
$CB_SITE_MOD = 0;

function cb_run_module(string $src): void { $GLOBALS['CB_SITE_MOD'] = __LINE__; eval($src); }

/** [file|null, line] for a location in the player's code (null file = main file). */
function cb_where(string $file, int $line, array $trace = []): ?array
{
    global $CB_SITE_MAIN, $CB_SITE_MOD, $CB_LOADING, $CB_DEFINED_IN;
    if (str_contains($file, "($CB_SITE_MAIN) : eval()'d code")) return [null, $line];
    if (!str_contains($file, "($CB_SITE_MOD) : eval()'d code")) return null;
    if ($CB_LOADING !== null) return [$CB_LOADING, $line];
    foreach ($trace as $f) {
        $k = strtolower(($f['class'] ?? null) ?: ($f['function'] ?? ''));
        if (isset($CB_DEFINED_IN[$k])) return [$CB_DEFINED_IN[$k], $line];
    }
    return [count($GLOBALS['CB_FILES']) === 1 ? array_key_first($GLOBALS['CB_FILES']) : 'a library file', $line];
}

function cb_at(Throwable $e): array
{
    if ($w = cb_where($e->getFile(), $e->getLine(), $e->getTrace())) return $w;
    foreach ($e->getTrace() as $i => $f) if ($w = cb_where($f['file'] ?? '', $f['line'] ?? 0, array_slice($e->getTrace(), $i + 1))) return $w;
    return [null, null];
}

// ---------------------------------------------------------------- debugger (declare(ticks=1) in the main file)
$CB_DBG = ['bps' => [], 'mode' => 'continue', 'depth' => 0, 'watches' => [], 'pausing' => false];

function cb_snap($v, int $d = 0)
{
    if ($v === null || is_bool($v) || is_int($v)) return $v;
    if (is_float($v)) return is_finite($v) ? $v : (string)$v;
    if (is_string($v)) return strlen($v) > 200 ? substr($v, 0, 200) . '…' : $v;
    if ($v instanceof Machine || $v instanceof Closure) return $v instanceof Closure ? 'ƒ closure' : (string)$v;
    if ($d >= 2) return is_object($v) ? get_class($v) . '(…)' : 'array(…)';
    $o = []; $i = 0;
    foreach (is_object($v) ? get_object_vars($v) : $v as $k => $x) { if ($i++ >= 20) { $o['…'] = 'more'; break; } $o[$k] = cb_snap($x, $d + 1); }
    return is_object($v) && !$o ? get_class($v) : $o;
}

function cb_player_globals(): array
{
    static $api = ['computer', 'machines', 'energy', 'world', 'hud', 'drill', 'furnace', 'generator', 'chest', 'conveyor', 'inserter', 'greenhouse',
                   'assembler', 'lab', 'turret', 'launchpad', 'pad', 'code', 'p', 'e', 'GLOBALS', 'argv', 'argc'];
    $out = [];
    foreach ($GLOBALS as $k => $v) if (!str_starts_with($k, 'CB_') && !str_starts_with($k, '_') && !in_array($k, $api, true)) $out[$k] = $v;
    return $out;
}

function cb_watch(string $expr, array $vars)
{
    if (!preg_match('/^\$?([A-Za-z_]\w*)((?:->[A-Za-z_]\w*|\[\d+\]|\[\'[^\']*\'\]|\["[^"]*"\])*)$/', trim($expr), $m)) return '(use a variable and a path, e.g. $items[0][\'count\'])';
    if (!array_key_exists($m[1], $vars) && !array_key_exists($m[1], $GLOBALS)) return '(not defined here)';
    $v = $vars[$m[1]] ?? $GLOBALS[$m[1]];
    preg_match_all('/->[A-Za-z_]\w*|\[\d+\]|\[\'[^\']*\'\]|\["[^"]*"\]/', $m[2], $parts);
    foreach ($parts[0] as $p) {
        $key = $p[0] === '-' ? substr($p, 2) : (ctype_digit(substr($p, 1, -1)) ? (int)substr($p, 1, -1) : substr($p, 2, -2));
        if (is_array($v) && array_key_exists($key, $v)) $v = $v[$key];
        elseif (is_object($v) && isset($v->$key)) $v = $v->$key;
        else return "(no $p)";
    }
    return cb_snap($v);
}

function cb_tick(): void
{
    global $CB_DBG, $CB_SITE_MAIN;
    if ($CB_DBG['pausing']) return;
    $bt = debug_backtrace(DEBUG_BACKTRACE_IGNORE_ARGS);
    $marker = "($CB_SITE_MAIN) : eval()'d code";
    if (!str_contains($bt[0]['file'] ?? '', $marker)) return;
    $line = $bt[0]['line'];
    $frames = []; $at = $line;
    for ($i = 1; $i < count($bt); $i++) {
        $fn = $bt[$i]['function'] ?? '';
        if ($fn === '' || $fn === 'eval' || str_starts_with($fn, 'cb_')) break;
        $frames[] = (isset($bt[$i]['class']) ? $bt[$i]['class'] . '::' : '') . $fn . ' : line ' . $at;
        if (!str_contains($bt[$i]['file'] ?? '', $marker)) { $at = null; break; }
        $at = $bt[$i]['line'];
    }
    if ($at !== null) $frames[] = 'main : line ' . $at;
    $depth = count($frames); $mode = $CB_DBG['mode'];
    if (!(isset($CB_DBG['bps'][$line]) || $mode === 'step' || ($mode === 'over' && $depth <= $CB_DBG['depth']) || ($mode === 'out' && $depth < $CB_DBG['depth']))) return;
    $CB_DBG['pausing'] = true;
    try {
        $g = cb_player_globals();
        $vars = []; foreach ($g as $k => $v) $vars[$k] = cb_snap($v);
        $w = []; foreach ($CB_DBG['watches'] as $e) $w[] = [$e, cb_watch($e, $g)];
        $r = cb_call('debug', 'pause', [$line, (object)$vars, $frames, $w]);
        $CB_DBG['mode'] = is_array($r) && isset($r['mode']) ? (string)$r['mode'] : 'continue';
        $CB_DBG['depth'] = $depth;
        if (is_array($r) && isset($r['watches']) && is_array($r['watches'])) $CB_DBG['watches'] = array_map('strval', $r['watches']);
    } finally { $CB_DBG['pausing'] = false; }
}

// ---------------------------------------------------------------- main
cb_send(['t' => 'ready', 'version' => PHP_VERSION]);
$start = cb_read();
$CB_ID = $start['id'] ?? null;
$CB_RAM = max(8, min(1024, (int)($start['ram'] ?? 32)));
$code = (string)($start['code'] ?? '');
function cb_strip_tag(string $c): string { if (str_starts_with(ltrim($c), '<?php')) { $p = strpos($c, '<?php'); $c = substr_replace($c, '     ', $p, 5); } return $c; }   // keep line numbers
$code = cb_strip_tag($code);
foreach (($start['files'] ?? []) as $fname => $src) {
    $n = preg_replace('/\.php$/', '', (string)$fname);
    if (preg_match('/^[A-Za-z_]\w*$/', $n)) $CB_FILES[$n] = cb_strip_tag((string)$src);
}
if (!empty($start['debug'])) {
    foreach (($start['bps'] ?? []) as $b) $CB_DBG['bps'][(int)$b] = true;
    $CB_DBG['watches'] = array_map('strval', $start['watches'] ?? []);
    $code = 'declare(ticks=1);' . $code;   // same line: line numbers do not move
    register_tick_function('cb_tick');
}
unset($b, $n, $fname, $src);   // keep the host's loop variables out of the player's globals

register_shutdown_function(function () use ($CB_RAM) {
    $e = error_get_last();
    if ($e && ($e['type'] & (E_ERROR | E_CORE_ERROR | E_COMPILE_ERROR | E_USER_ERROR))) {
        $oom = str_contains($e['message'], 'Allowed memory size');
        $w = cb_where($e['file'], $e['line']) ?? [null, null];
        cb_finish('error', $oom ? "Out of memory: program exceeded {$CB_RAM} MB RAM" : $e['message'], $w[1], $w[0]);
    } else cb_finish('done');
});
set_error_handler(function (int $no, string $msg, string $file, int $line) {   // notices and warnings are logged, not fatal
    $w = cb_where($file, $line);
    cb_log_line('[warn] ' . ($w ? ($w[0] ? "$w[0] " : '') . "line $w[1]: " : '') . $msg);
    return true;
});

$info = cb_call('computer', 'info');
$computer = new Computer((string)($info['name'] ?? 'PC'), (int)($info['cps'] ?? 20));
$machines = new Machines();
$energy = new Energy();
$world = new World();
$hud = new Hud();
foreach (['drill', 'furnace', 'generator', 'chest', 'conveyor', 'inserter', 'greenhouse', 'assembler', 'lab', 'turret', 'launchpad'] as $t) {
    $i = cb_call('machines', 'find', [$t]);
    $$t = $i ? new Machine($i) : null;
}
$pad = $launchpad;
unset($t, $i, $info, $start);

ob_start('cb_output', 4096);
foreach ($CB_FILES as $CB_LOADING => $CB_SRC) {   // the other files first: their functions and classes
    $CB_BEFORE = [get_defined_functions()['user'], get_declared_classes()];
    try { cb_run_module($CB_SRC); }
    catch (ParseError $e) { cb_finish('error', 'ParseError: ' . $e->getMessage(), $e->getLine(), $CB_LOADING); exit(0); }
    catch (Throwable $e) { cb_finish('error', get_class($e) . ': ' . $e->getMessage(), $e->getLine(), $CB_LOADING); exit(0); }
    foreach (array_diff(get_defined_functions()['user'], $CB_BEFORE[0]) as $f) $CB_DEFINED_IN[strtolower($f)] = $CB_LOADING;
    foreach (array_diff(get_declared_classes(), $CB_BEFORE[1]) as $c) $CB_DEFINED_IN[strtolower($c)] = $CB_LOADING;
}
$CB_LOADING = null; unset($CB_SRC, $CB_BEFORE);
try {
    $CB_SITE_MAIN = __LINE__ + 1;
    eval($code);
    while ($CB_HANDLERS) cb_call('computer', 'wait_event');   // event mode: keep serving events after the main body
    cb_finish('done');
} catch (ParseError $e) {
    cb_finish('error', 'ParseError: ' . $e->getMessage(), $e->getLine());
} catch (Throwable $e) {
    [$f, $l] = cb_at($e);
    cb_finish('error', get_class($e) . ': ' . $e->getMessage(), $l, $f);
}
exit(0);
