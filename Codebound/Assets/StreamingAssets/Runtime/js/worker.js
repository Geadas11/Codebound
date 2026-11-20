// One player program. Calls into the game block (Atomics.wait) until Unity replies, so plain
// synchronous code like `while (true) { drill.mine(); }` works and every call costs game time.
'use strict';
const { parentPort, workerData } = require('worker_threads');
const vm = require('vm');

const flag = new Int32Array(workerData.sab, 0, 2);
const bytes = new Uint8Array(workerData.sab, 8);
const decoder = new TextDecoder();

// The only host functions visible to player code. They take/return strings, so no host objects leak.
function hostCall(obj, fn, argsJson) {
  Atomics.store(flag, 0, 0);
  parentPort.postMessage({ t: 'call', obj: String(obj), fn: String(fn), args: JSON.parse(String(argsJson)) });
  Atomics.wait(flag, 0, 0);
  return decoder.decode(bytes.slice(0, Atomics.load(flag, 1)));
}
function hostLog(s) { parentPort.postMessage({ t: 'log', msg: String(s) }); }

const context = vm.createContext({ __cb: hostCall, __log: hostLog, __dbgcfg: JSON.stringify({ on: workerData.debug, bps: workerData.bps }) }, {
  name: 'codebound-program',
  codeGeneration: { strings: false, wasm: false },
});

// Codebound API, defined inside the sandbox realm.
const PRELUDE = `
'use strict';
(function (global) {
  const cb = __cb, rawLog = __log;
  const dbg = JSON.parse(__dbgcfg);
  delete global.__cb; delete global.__log; delete global.__dbgcfg;
  let bps = new Set(dbg.bps), stepping = false;
  const handlers = Object.create(null);
  let dispatching = false; const queued = [];

  function fmt(x) { if (typeof x === 'string') return x; try { return JSON.stringify(x); } catch (e) { return String(x); } }
  function log(...a) { rawLog(a.map(fmt).join(' ')); }

  function dispatch(events) {
    for (const e of events) queued.push(e);
    if (dispatching) return;
    dispatching = true;
    try {
      while (queued.length) {
        const e = queued.shift();
        const list = handlers[e.name] || [];
        for (const fn of list) fn(e.data);
      }
    } finally { dispatching = false; }
  }

  function call(obj, fn, args) {
    const r = JSON.parse(cb(obj, fn, JSON.stringify(args || [])));
    if (r.warn) rawLog('[warn] ' + r.warn);
    if (r.bps) bps = new Set(r.bps);
    if (r.events) dispatch(r.events);
    if (r.error) throw new Error(r.error);
    return r.value;
  }

  // ---- debugger: values are copied into plain data (depth/size limited) so the game can show them
  function snap(v, depth) {
    if (v === null || v === undefined) return v === null ? null : 'undefined';
    const ty = typeof v;
    if (ty === 'number') return Number.isFinite(v) ? v : String(v);
    if (ty === 'string') return v.length > 200 ? v.slice(0, 200) + '…' : v;
    if (ty === 'boolean') return v;
    if (ty === 'function') return 'ƒ ' + (v.name || 'anonymous') + '()';
    if (ty !== 'object') return String(v);
    if (depth >= 2) return Array.isArray(v) ? 'Array(' + v.length + ')' : '{…}';
    if (Array.isArray(v)) { const a = v.slice(0, 20).map(x => snap(x, depth + 1)); if (v.length > 20) a.push('… ' + (v.length - 20) + ' more'); return a; }
    const o = {}; let n = 0;
    for (const k of Object.keys(v)) { if (n++ >= 20) { o['…'] = 'more'; break; } try { o[k] = snap(v[k], depth + 1); } catch (e) { o[k] = '<error>'; } }
    return o;
  }
  global.__dbg = function (line, getVars) {
    if (!dbg.on || (!stepping && !bps.has(line))) return;
    const vars = {}; const raw = getVars();
    for (const k of Object.keys(raw)) vars[k] = snap(raw[k], 0);
    const r = call('debug', 'pause', [line, vars]);
    stepping = !!(r && r.mode === 'step');
  };

  function machine(info) {
    const m = { id: info.id, type: info.type, name: info.name };
    const id = info.id;
    m.status = () => call(id, 'status');
    m.contents = () => call(id, 'contents');
    if (info.type === 'drill') m.mine = () => call(id, 'mine');
    if (info.type === 'furnace') m.smelt = () => call(id, 'smelt');
    if (info.type === 'greenhouse') {
      m.plant = (plot) => call(id, 'plant', plot === undefined ? [] : [Number(plot)]);
      m.water = (plot) => call(id, 'water', plot === undefined ? [] : [Number(plot)]);
      m.harvest = (plot) => call(id, 'harvest', plot === undefined ? [] : [Number(plot)]);
      m.plots = () => call(id, 'plots');
    }
    if (info.type === 'assembler') m.craft = (item) => call(id, 'craft', [String(item)]);
    if (info.type === 'turret') { m.targets = () => call(id, 'targets'); m.fire = (cid) => call(id, 'fire', [Number(cid)]); }
    if (info.type === 'lab') { m.research = (tech) => call(id, 'research', [String(tech)]); m.techs = () => call(id, 'techs'); }
    if (info.type === 'conveyor' || info.type === 'inserter') {
      m.start = () => call(id, 'start');
      m.stop = () => call(id, 'stop');
      m.setFilter = (item) => call(id, 'set_filter', [item == null ? null : String(item)]);
    }
    return Object.freeze(m);
  }

  const machines = Object.freeze({
    list: () => call('machines', 'list').map(machine),
    get: (id) => { const i = call('machines', 'get', [id]); return i ? machine(i) : null; },
    find: (type) => { const i = call('machines', 'find', [type]); return i ? machine(i) : null; },
  });

  const info = call('computer', 'info');
  const computer = Object.freeze({
    name: info.name,
    cps: info.cps,
    on(name, fn) {
      if (typeof fn !== 'function') throw new TypeError('computer.on(name, fn): fn must be a function');
      if (!handlers[name]) { handlers[name] = []; call('computer', 'subscribe', [String(name)]); }
      handlers[name].push(fn);
    },
    time: () => call('computer', 'time'),
    hasHandlers: () => Object.keys(handlers).length > 0,
    waitEvent: () => call('computer', 'wait_event'),
  });

  const energy = Object.freeze({
    available: () => call('energy', 'available'),
    stats: () => call('energy', 'stats'),
  });

  global.log = log;
  global.print = log;
  global.sleep = (seconds) => call('computer', 'sleep', [Number(seconds) || 0]);
  global.computer = computer;
  global.machines = machines;
  global.energy = energy;
  global.hud = Object.freeze({
    set: (key, value) => call('hud', 'set', [String(key), value === undefined ? null : value]),
    notify: (text) => call('hud', 'notify', [String(text)]),
    alarm: (text) => call('hud', 'alarm', [String(text === undefined ? 'ALARM' : text)]),
  });
  for (const t of ['drill', 'furnace', 'generator', 'chest', 'conveyor', 'inserter', 'greenhouse', 'assembler', 'lab', 'turret']) {
    const i = call('machines', 'find', [t]);
    global[t] = i ? machine(i) : null;
  }
})(globalThis);
`;

function lineOf(err) {
  const m = String(err && err.stack || '').match(/computer\.js:(\d+)/);
  if (!m) return null;
  const gen = Number(m[1]);
  return workerData.lineMap ? (workerData.lineMap[gen] || null) : gen; // TypeScript: map back to the player's line
}

try {
  vm.runInContext(PRELUDE, context, { filename: 'codebound-api.js' });
  const script = new vm.Script(workerData.code, { filename: 'computer.js' });
  script.runInContext(context);
  // Event mode: if handlers were registered, keep serving events after the main body ends.
  vm.runInContext('while (computer.hasHandlers()) computer.waitEvent();', context, { filename: 'codebound-events.js' });
  parentPort.postMessage({ t: 'done' });
} catch (e) {
  const name = e && e.name ? e.name : 'Error';
  const msg = e && e.message !== undefined ? e.message : String(e);
  parentPort.postMessage({ t: 'error', msg: name + ': ' + msg, line: lineOf(e) });
}
