// One C/C++ player program, compiled to WebAssembly by Unity (clang, wasm32-wasip1) and run here.
// WebAssembly is its own sandbox: the program can only call the functions imported below. WASI is
// implemented by hand with NO file system, network or environment: stdout/stderr go to the computer's
// log, clocks and randomness work, poll_oneoff (sleep) waits in game time, everything else is ENOSYS.
// Memory is created here with maximum = the computer's RAM, so the limit does not depend on the program.
'use strict';
const { parentPort, workerData } = require('worker_threads');
const { getRandomValues } = require('crypto');

const flag = new Int32Array(workerData.sab, 0, 2);
const shared = new Uint8Array(workerData.sab, 8);
const dec = new TextDecoder(), enc = new TextEncoder();
const ERRNO = { SUCCESS: 0, BADF: 8, INVAL: 28, NOSYS: 52, SPIPE: 70, NOTCAPABLE: 76 };

function log(s) { parentPort.postMessage({ t: 'log', msg: String(s) }); }
function hostCall(obj, fn, args) {
  Atomics.store(flag, 0, 0);
  parentPort.postMessage({ t: 'call', obj: String(obj), fn: String(fn), args });
  Atomics.wait(flag, 0, 0);
  return JSON.parse(dec.decode(shared.slice(0, Atomics.load(flag, 1))));
}

let lastError = '';
const events = [];
function call(obj, fn, args) {
  const r = hostCall(obj, fn, args);
  if (r.warn) log('[warn] ' + r.warn);
  if (r.events) events.push(...r.events);
  lastError = r.error || '';
  return r;
}

let line = '';
function output(text) {
  line += text;
  let i;
  while ((i = line.indexOf('\n')) >= 0) { log(line.slice(0, i)); line = line.slice(i + 1); }
  if (line.length > 4000) { log(line); line = ''; }
}

class ProcExit { constructor(code) { this.code = code; } }

// ---- source lines for runtime errors: the module is compiled with DWARF line tables (clang -gdwarf-4 -gline-tables-only).
// V8 reports a trap as "wasm-function[N]:0xOFFSET" (byte offset in the module); DWARF addresses are offsets from the
// start of the code section's contents.
function dwarfLines(bytes) {
  let pos = 8, codeStart = -1, line = null;
  const uleb = () => { let r = 0, s = 0, b; do { b = bytes[pos++]; r += (b & 0x7f) * 2 ** s; s += 7; } while (b & 0x80); return r; };
  while (pos < bytes.length) {
    const id = bytes[pos++], size = uleb(), start = pos;
    if (id === 10) codeStart = start;
    if (id === 0) { const n = uleb(); if (Buffer.from(bytes.subarray(pos, pos + n)).toString() === '.debug_line') line = bytes.subarray(pos + n, start + size); }
    pos = start + size;
  }
  if (codeStart < 0 || !line) return null;
  const sequences = [];
  const dv = new DataView(line.buffer, line.byteOffset, line.byteLength);
  let p = 0;
  const u8 = () => line[p++], u16 = () => { const v = dv.getUint16(p, true); p += 2; return v; }, u32 = () => { const v = dv.getUint32(p, true); p += 4; return v; };
  const ul = () => { let r = 0, s = 0, b; do { b = line[p++]; r += (b & 0x7f) * 2 ** s; s += 7; } while (b & 0x80); return r; };
  const sl = () => { let r = 0, s = 0, b; do { b = line[p++]; r |= (b & 0x7f) << s; s += 7; } while (b & 0x80); return s < 32 && (b & 0x40) ? r | (-1 << s) : r; };
  const str = () => { const s0 = p; while (line[p]) p++; return Buffer.from(line.subarray(s0, p++)).toString(); };
  while (p + 4 <= line.length) {
    const unitLen = u32(); if (unitLen === 0xffffffff) break;   // 64-bit DWARF: not produced for wasm32
    const unitEnd = p + unitLen, version = u16();
    if (version !== 4) { p = unitEnd; continue; }
    const headerLen = u32(), progStart = p + headerLen;
    const minInst = u8(); u8(); const defaultStmt = u8(); const lineBase = (u8() << 24) >> 24, lineRange = u8(), opcodeBase = u8();
    const stdLens = []; for (let i = 1; i < opcodeBase; i++) stdLens.push(u8());
    while (line[p]) str(); p++;                        // include directories
    const files = [null]; while (line[p]) { files.push(str()); ul(); ul(); ul(); } p++;
    p = progStart;
    let addr = 0, file = 1, ln = 1, rows = [];
    const emit = () => rows.push([addr, files[file] || '', ln]);
    while (p < unitEnd) {
      const op = u8();
      if (op >= opcodeBase) { const adj = op - opcodeBase; addr += Math.floor(adj / lineRange) * minInst; ln += lineBase + (adj % lineRange); emit(); }
      else if (op === 0) {
        const len = ul(), end = p + len, ext = u8();
        if (ext === 1) { emit(); sequences.push(rows); rows = []; addr = 0; file = 1; ln = 1; }
        else if (ext === 2) addr = u32();
        p = end;
      }
      else if (op === 1) emit();
      else if (op === 2) addr += ul() * minInst;
      else if (op === 3) ln += sl();
      else if (op === 4) file = ul();
      else if (op === 8) addr += Math.floor((255 - opcodeBase) / lineRange) * minInst;
      else if (op === 9) addr += u16();
      else if (op === 5 || op === 12) ul();
      else if (op === 6 || op === 7 || op === 10 || op === 11) { /* flags */ }
      else for (let i = 0; i < stdLens[op - 1]; i++) ul();
    }
    p = unitEnd;
  }
  return { codeStart, sequences };
}

/** {file, line} of the deepest frame of the error that is in one of the player's files, or null. */
function sourceOf(err) {
  if (!lineInfo) return null;
  for (const m of String(err && err.stack || '').matchAll(/wasm-function\[\d+\]:0x([0-9a-f]+)/g)) {
    const pc = parseInt(m[1], 16) - lineInfo.codeStart;
    for (const seq of lineInfo.sequences) {
      if (!seq.length || pc < seq[0][0] || pc >= seq[seq.length - 1][0]) continue;
      let best = null;
      for (const r of seq) { if (r[0] > pc) break; best = r; }
      if (best && playerFiles.has(best[1]) && best[2] > 0) return { file: best[1], line: best[2] };
    }
  }
  return null;
}
const playerFiles = new Set(workerData.sources || []);
let lineInfo = null;
try { lineInfo = dwarfLines(new Uint8Array(workerData.wasm)); } catch (_) { lineInfo = null; }   // no line numbers, but the program still runs

const INITIAL_PAGES = 64;   // = clang -Wl,--initial-memory=4194304 in ProgramManager.CompileC
const memory = new WebAssembly.Memory({ initial: INITIAL_PAGES, maximum: Math.max(INITIAL_PAGES, Math.floor(workerData.ramMb * 1048576 / 65536)) });
const u8 = () => new Uint8Array(memory.buffer);
const view = () => new DataView(memory.buffer);
function range(p, n) {
  if (!Number.isInteger(p) || !Number.isInteger(n) || p < 0 || n < 0 || p + n > memory.buffer.byteLength) throw new WebAssembly.RuntimeError('bad pointer passed to the game API');
}
function str(p, n) { range(p, n); return dec.decode(u8().subarray(p, p + n)); }
function put(p, cap, s) {          // writes up to cap bytes, returns the full length (so C can tell it was cut)
  const b = enc.encode(s);
  const n = Math.max(0, Math.min(b.length, cap));
  range(p, n);
  u8().set(b.subarray(0, n), p);
  return b.length;
}

const codebound = {
  call(objP, objL, fnP, fnL, argsP, argsL, outP, cap) {
    let args;
    try { args = JSON.parse(str(argsP, argsL)); } catch (_) { lastError = 'args must be a JSON array, e.g. "[]" or "[\\"iron_plate\\"]"'; return -1; }
    if (!Array.isArray(args)) { lastError = 'args must be a JSON array'; return -1; }
    const r = call(str(objP, objL), str(fnP, fnL), args);
    if (r.error) return -1;
    return put(outP, cap, JSON.stringify(r.value === undefined ? null : r.value));
  },
  error(outP, cap) { return put(outP, cap, lastError); },
  next_event(outP, cap) { return events.length ? put(outP, cap, JSON.stringify(events.shift())) : 0; },
};

const wasi = {
  fd_write(fd, iovs, iovsLen, nwrittenP) {
    if (fd !== 1 && fd !== 2) return ERRNO.BADF;
    range(iovs, iovsLen * 8);
    const v = view();
    let total = 0, text = '';
    for (let i = 0; i < iovsLen; i++) {
      const p = v.getUint32(iovs + i * 8, true), n = v.getUint32(iovs + i * 8 + 4, true);
      text += str(p, n); total += n;
    }
    output(text);
    range(nwrittenP, 4); view().setUint32(nwrittenP, total, true);
    return ERRNO.SUCCESS;
  },
  fd_read(fd, iovs, iovsLen, nreadP) { range(nreadP, 4); view().setUint32(nreadP, 0, true); return fd === 0 ? ERRNO.SUCCESS : ERRNO.BADF; },
  fd_close(fd) { return fd <= 2 ? ERRNO.SUCCESS : ERRNO.BADF; },
  fd_seek() { return ERRNO.SPIPE; },
  fd_fdstat_get(fd, p) {
    if (fd > 2) return ERRNO.BADF;
    range(p, 24);
    const v = view();
    v.setUint8(p, 2); v.setUint16(p + 2, 0, true);                       // character device, no flags
    v.setBigUint64(p + 8, 0x5bn, true); v.setBigUint64(p + 16, 0n, true);   // read/write/sync rights, no seek/tell: libc treats it as a terminal (line-buffered)
    return ERRNO.SUCCESS;
  },
  fd_prestat_get() { return ERRNO.BADF; },                                // no pre-opened directories: no file system at all
  fd_prestat_dir_name() { return ERRNO.BADF; },
  environ_sizes_get(cP, sP) { range(cP, 4); range(sP, 4); view().setUint32(cP, 0, true); view().setUint32(sP, 0, true); return ERRNO.SUCCESS; },
  environ_get() { return ERRNO.SUCCESS; },
  args_sizes_get(cP, sP) { range(cP, 4); range(sP, 4); view().setUint32(cP, 0, true); view().setUint32(sP, 0, true); return ERRNO.SUCCESS; },
  args_get() { return ERRNO.SUCCESS; },
  clock_time_get(id, precision, timeP) {
    range(timeP, 8);
    const ns = id === 0 ? BigInt(Date.now()) * 1000000n : process.hrtime.bigint();
    view().setBigUint64(timeP, ns, true);
    return ERRNO.SUCCESS;
  },
  clock_res_get(id, resP) { range(resP, 8); view().setBigUint64(resP, 1000n, true); return ERRNO.SUCCESS; },
  random_get(p, n) { range(p, n); for (let o = 0; o < n; o += 65536) getRandomValues(u8().subarray(p + o, p + Math.min(n, o + 65536))); return ERRNO.SUCCESS; },
  sched_yield() { return ERRNO.SUCCESS; },
  proc_exit(code) { throw new ProcExit(code); },
  // sleep(), usleep(), nanosleep() end up here: wait for the longest relative clock timeout, in game time
  poll_oneoff(inP, outP, nsubs, neventsP) {
    range(inP, nsubs * 48); range(outP, nsubs * 32); range(neventsP, 4);
    const v = view();
    let longest = 0n;
    for (let i = 0; i < nsubs; i++) {
      const s = inP + i * 48;
      if (v.getUint8(s + 8) !== 0) return ERRNO.NOTCAPABLE;               // only clock subscriptions (no fds)
      const t = v.getBigUint64(s + 24, true);
      if (t > longest) longest = t;
    }
    const r = call('computer', 'sleep', [Number(longest) / 1e9]);
    const w = view();
    for (let i = 0; i < nsubs; i++) {
      const e = outP + i * 32;
      w.setBigUint64(e, v.getBigUint64(inP + i * 48, true), true); w.setUint16(e + 8, 0, true); w.setUint8(e + 10, 0);
    }
    w.setUint32(neventsP, nsubs, true);
    return r.error ? ERRNO.INVAL : ERRNO.SUCCESS;
  },
};

try {
  const mod = new WebAssembly.Module(workerData.wasm);
  const imports = { env: { memory }, codebound: {}, wasi_snapshot_preview1: {} };
  for (const i of WebAssembly.Module.imports(mod)) {
    if (i.module === 'env' && i.name === 'memory') continue;
    if (i.module === 'codebound' && codebound[i.name]) imports.codebound[i.name] = codebound[i.name];
    else if (i.module === 'wasi_snapshot_preview1') imports.wasi_snapshot_preview1[i.name] = wasi[i.name] || (() => ERRNO.NOSYS);
    else throw new Error(`the program needs '${i.module}.${i.name}', which Codebound computers do not have`);
  }
  const inst = new WebAssembly.Instance(mod, imports);
  try { inst.exports._start(); }
  catch (e) { if (!(e instanceof ProcExit)) throw e; if (e.code !== 0) { if (line) log(line); parentPort.postMessage({ t: 'error', msg: `Program exited with code ${e.code}`, line: null }); return; } }
  if (line) log(line);
  parentPort.postMessage({ t: 'done' });
} catch (e) {
  if (line) log(line);
  const oom = e instanceof RangeError && /memory/i.test(e.message);
  const msg = oom ? `Out of memory: program exceeded ${workerData.ramMb} MB RAM` : `${e && e.name || 'Error'}: ${e && e.message || e}`;
  const at = oom ? null : sourceOf(e);
  parentPort.postMessage({ t: 'error', msg, line: at ? at.line : null, file: at && !/^computer\.(c|cpp)$/.test(at.file) ? at.file : null });
}
