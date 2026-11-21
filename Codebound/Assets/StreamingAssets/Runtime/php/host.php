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
        'launchpad' => ['launch', 'throttle', 'flight', 'planets', 'travel']];
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
}

final class Energy { public function available() { return cb_call('energy', 'available'); } public function stats() { return cb_call('energy', 'stats'); } }
final class World { public function pollution() { return cb_call('world', 'pollution'); } }
final class Hud
{
    public function set(string $key, $value) { return cb_call('hud', 'set', [$key, is_scalar($value) || $value === null ? $value : json_encode($value)]); }
    public function notify(string $text) { return cb_call('hud', 'notify', [$text]); }
    public function alarm(string $text = 'ALARM') { return cb_call('hud', 'alarm', [$text]); }
}

// the built-in sleep() is disabled on Codebound computers; this one waits in game time without burning CPU
function sleep($seconds = 0) { cb_call('computer', 'sleep', [(float)$seconds]); return 0; }
function usleep($microseconds) { cb_call('computer', 'sleep', [$microseconds / 1e6]); }

function cb_finish(string $reason, ?string $msg = null, ?int $line = null): void
{
    global $CB_EXITED, $CB_ID;
    if ($CB_EXITED) return;
    $CB_EXITED = true;
    while (ob_get_level() > 0) ob_end_flush();
    cb_send(['t' => 'exit', 'id' => $CB_ID, 'reason' => $reason, 'msg' => $msg, 'line' => $line]);
}

function cb_line(Throwable $e): ?int
{
    if (str_contains($e->getFile(), "eval()'d code")) return $e->getLine();
    foreach ($e->getTrace() as $f) if (str_contains($f['file'] ?? '', "eval()'d code")) return $f['line'] ?? null;
    return null;
}

// ---------------------------------------------------------------- main
cb_send(['t' => 'ready', 'version' => PHP_VERSION]);
$start = cb_read();
$CB_ID = $start['id'] ?? null;
$CB_RAM = max(8, min(1024, (int)($start['ram'] ?? 32)));
$code = (string)($start['code'] ?? '');
if (str_starts_with(ltrim($code), '<?php')) { $p = strpos($code, '<?php'); $code = substr_replace($code, '     ', $p, 5); }   // keep line numbers

register_shutdown_function(function () use ($CB_RAM) {
    $e = error_get_last();
    if ($e && ($e['type'] & (E_ERROR | E_CORE_ERROR | E_COMPILE_ERROR | E_USER_ERROR))) {
        $oom = str_contains($e['message'], 'Allowed memory size');
        cb_finish('error', $oom ? "Out of memory: program exceeded {$CB_RAM} MB RAM" : $e['message'], str_contains($e['file'], "eval()'d code") ? $e['line'] : null);
    } else cb_finish('done');
});
set_error_handler(function (int $no, string $msg, string $file, int $line) {   // notices and warnings are logged, not fatal
    cb_log_line('[warn] ' . (str_contains($file, "eval()'d code") ? "line $line: " : '') . $msg);
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
unset($t, $i, $info, $start);

ob_start('cb_output', 4096);
try {
    eval($code);
    while ($CB_HANDLERS) cb_call('computer', 'wait_event');   // event mode: keep serving events after the main body
    cb_finish('done');
} catch (ParseError $e) {
    cb_finish('error', 'ParseError: ' . $e->getMessage(), $e->getLine());
} catch (Throwable $e) {
    cb_finish('error', get_class($e) . ': ' . $e->getMessage(), cb_line($e));
}
exit(0);
