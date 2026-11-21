// Codebound JavaScript runtime host.
// Runs OUTSIDE the Unity process. Unity talks to it with one JSON object per line on stdin/stdout.
// Each player program runs in its own worker thread (own heap limit, can be terminated at any time)
// inside a vm context that only sees the Codebound API. The process itself is started with the Node
// permission model (no fs writes, fs reads only in this folder, no child processes, no addons) and
// with code generation from strings disabled (blocks Function-constructor escapes).
'use strict';
const { Worker } = require('worker_threads');
const path = require('path');
const readline = require('readline');
const { prepare } = require('./prepare.js');

const WATCHDOG_MS = 3000;     // max time a program may run without calling the world API
const STATS_EVERY_MS = 1000;
const BUF_BYTES = 1 << 20;    // max size of one API reply

const programs = new Map();   // id -> { worker, sab, flag, bytes, busySince, busyMs, logs, ... }

function send(obj) { process.stdout.write(JSON.stringify(obj) + '\n'); }

function start(msg) {
  stop(msg.id, null);
  if (typeof msg.wasm === 'string') return startWasm(msg);   // C / C++: already compiled to WebAssembly by Unity
  let prepared;
  try { prepared = prepare(String(msg.code || ''), String(msg.lang || 'javascript'), !!msg.debug); }
  catch (e) { prepared = { error: 'Internal compiler error: ' + e.message, line: null }; }
  if (prepared.error) { send({ t: 'exit', id: msg.id, reason: 'error', msg: prepared.error, line: prepared.line }); return; }
  const sab = new SharedArrayBuffer(8 + BUF_BYTES);
  const ramMb = Math.max(8, Math.min(1024, msg.ram | 0 || 32));
  const worker = new Worker(path.join(__dirname, 'worker.js'), {
    workerData: { sab, code: prepared.code, lineMap: prepared.lineMap, debug: !!msg.debug, bps: Array.isArray(msg.bps) ? msg.bps : [], name: String(msg.name || 'computer'), id: msg.id },
    resourceLimits: { maxOldGenerationSizeMb: ramMb, maxYoungGenerationSizeMb: Math.max(2, ramMb >> 3), stackSizeMb: 2 },
    stdout: true, stderr: true, // player code has no console; keep worker stdio away from the protocol
  });
  attach(msg, worker, sab, ramMb);
}

function startWasm(msg) {
  const sab = new SharedArrayBuffer(8 + BUF_BYTES);
  const ramMb = Math.max(8, Math.min(1024, msg.ram | 0 || 32));
  const worker = new Worker(path.join(__dirname, 'wasm_worker.js'), {
    workerData: { sab, wasm: Buffer.from(msg.wasm, 'base64'), ramMb },   // wasm memory is created with maximum = ramMb
    resourceLimits: { maxOldGenerationSizeMb: 32, maxYoungGenerationSizeMb: 4, stackSizeMb: 2 },
    stdout: true, stderr: true,
  });
  attach(msg, worker, sab, ramMb);
}

function attach(msg, worker, sab, ramMb) {
  const p = {
    id: msg.id, worker, sab, flag: new Int32Array(sab, 0, 2), bytes: new Uint8Array(sab, 8),
    ramMb, busySince: Date.now(), busyMs: 0, windowStart: Date.now(), logCount: 0, exited: false,
  };
  programs.set(msg.id, p);

  worker.on('message', m => {
    if (m.t === 'call') {
      accountIdle(p);
      send({ t: 'call', id: p.id, obj: m.obj, fn: m.fn, args: m.args });
    } else if (m.t === 'log') {
      if (++p.logCount <= 50) send({ t: 'log', id: p.id, msg: String(m.msg).slice(0, 2000) });
      else if (p.logCount === 51) send({ t: 'log', id: p.id, msg: '[runtime] log rate limit reached (50 lines/s), dropping output' });
    } else if (m.t === 'error') {
      finish(p, 'error', m.msg, m.line);
    } else if (m.t === 'done') {
      finish(p, 'done');
    }
  });
  worker.on('error', e => {
    const oom = e && (e.code === 'ERR_WORKER_OUT_OF_MEMORY' || /heap/i.test(e.message));
    finish(p, 'error', oom ? `Out of memory: program exceeded ${p.ramMb} MB RAM` : String(e && e.message || e));
  });
  worker.on('exit', () => finish(p, 'done'));
}

function accountIdle(p) {
  // program just blocked on the API: it was busy from busySince until now
  if (p.busySince) { p.busyMs += Date.now() - Math.max(p.busySince, p.windowStart); p.busySince = 0; }
}

function finish(p, reason, msg, line) {
  if (p.exited) return;
  p.exited = true;
  programs.delete(p.id);
  p.worker.terminate().catch(() => {});
  send({ t: 'exit', id: p.id, reason, msg: msg || null, line: line || null });
}

function stop(id, reason) {
  const p = programs.get(id);
  if (!p) return;
  if (reason) finish(p, reason);
  else { p.exited = true; programs.delete(id); p.worker.terminate().catch(() => {}); }
}

function reply(msg) {
  const p = programs.get(msg.id);
  if (!p) return;
  const enc = Buffer.from(JSON.stringify({ value: msg.value === undefined ? null : msg.value, error: msg.error || null, warn: msg.warn || null, events: msg.events || null, bps: msg.bps || null }), 'utf8');
  if (enc.length > BUF_BYTES) { finish(p, 'error', 'API reply too large'); return; }
  p.bytes.set(enc, 0);
  Atomics.store(p.flag, 1, enc.length);
  Atomics.store(p.flag, 0, 1);
  Atomics.notify(p.flag, 0);
  p.busySince = Date.now();
}

// watchdog + CPU/RAM stats
setInterval(() => {
  const now = Date.now();
  for (const p of programs.values()) {
    if (p.busySince && now - p.busySince > WATCHDOG_MS) {
      finish(p, 'error', `CPU watchdog: ran ${WATCHDOG_MS / 1000}s without calling the world API or sleep() — infinite loop?`);
    }
  }
}, 250);

setInterval(async () => {
  const now = Date.now();
  for (const p of programs.values()) {
    let busy = p.busyMs + (p.busySince ? now - Math.max(p.busySince, p.windowStart) : 0);
    const cpu = Math.min(100, Math.round(100 * busy / Math.max(1, now - p.windowStart)));
    p.busyMs = 0; p.windowStart = now; p.logCount = 0; // busySince stays: the watchdog needs it
    let heapMb = null;
    try { const h = await p.worker.getHeapStatistics(); heapMb = Math.round(h.used_heap_size / 1048576 * 10) / 10; } catch (_) { }
    if (!p.exited) send({ t: 'stats', id: p.id, cpu, heapMb, ramMb: p.ramMb });
  }
}, STATS_EVERY_MS);

const rl = readline.createInterface({ input: process.stdin });
rl.on('line', line => {
  let msg;
  try { msg = JSON.parse(line); } catch (_) { return; }
  switch (msg.t) {
    case 'start': start(msg); break;
    case 'stop': stop(msg.id, 'stopped'); break;
    case 'reply': reply(msg); break;
    case 'ping': send({ t: 'pong', version: process.version }); break;
  }
});
rl.on('close', () => process.exit(0)); // Unity went away: never outlive the game
send({ t: 'ready', version: process.version });
