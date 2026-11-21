// Integration test for the JavaScript runtime host. Plays the role of Unity.
// Run: node Codebound/Tests/runtime/host.test.js
'use strict';
const { spawn } = require('child_process');
const path = require('path');
const assert = require('assert');
const readline = require('readline');

const RUNTIME = path.resolve(__dirname, '../../Assets/StreamingAssets/Runtime/js');
const args = ['--permission', `--allow-fs-read=${RUNTIME}`, '--allow-worker', '--disallow-code-generation-from-strings',
  '--no-warnings', path.join(RUNTIME, 'host.js')];
const host = spawn(process.execPath, args, { stdio: ['pipe', 'pipe', 'inherit'] });
const listeners = [];
readline.createInterface({ input: host.stdout }).on('line', l => { const m = JSON.parse(l); listeners.slice().forEach(f => f(m)); });
const send = m => host.stdin.write(JSON.stringify(m) + '\n');

// Fake world: one drill with 2 ore units, events on demand.
function run(id, code, opts = {}) {
  return new Promise((resolve) => {
    const logs = []; const pauses = []; let ore = 2; const subs = new Set(); let ticks = 0; let stats = null;
    const t0 = Date.now();
    const on = m => {
      if (m.id !== id) return;
      if (m.t === 'log') logs.push(m.msg);
      if (m.t === 'stats') stats = m;
      if (m.t === 'call') {
        const r = { t: 'reply', id };
        const key = m.obj + '.' + m.fn;
        if (key === 'computer.info') r.value = { name: 'PC-1', cps: 20 };
        else if (key === 'machines.find') r.value = m.args[0] === 'drill' ? { id: 'm7', type: 'drill', name: 'Electric Drill' } : null;
        else if (key === 'm7.mine') { r.value = ore > 0 ? 'iron_ore' : null; if (ore-- <= 0) r.warn = 'drill.mine(): depleted'; }
        else if (key === 'energy.available') r.value = true;
        else if (key === 'computer.subscribe') { subs.add(m.args[0]); r.value = true; }
        else if (key === 'computer.sleep') r.value = null;
        else if (key === 'hud.set' || key === 'hud.notify') { logs.push(key + ':' + JSON.stringify(m.args)); r.value = true; }
        else if (key === 'debug.pause') {
          pauses.push({ line: m.args[0], vars: m.args[1], stack: m.args[2], watches: m.args[3] });
          r.value = { mode: (opts.debugModes || [])[pauses.length - 1] || 'continue' };
        }
        else if (key === 'computer.wait_event') {
          if (++ticks > 3) { send({ t: 'stop', id }); return; }
          r.value = null; r.events = [{ name: 'tick', data: { n: ticks } }];
        }
        else r.error = 'unknown API ' + key;
        setTimeout(() => send(r), 5);
      }
      if (m.t === 'exit') {
        listeners.splice(listeners.indexOf(on), 1);
        resolve({ ...m, logs, pauses, ms: Date.now() - t0, stats, subs });
      }
    };
    listeners.push(on);
    send({ t: 'start', id, code, ram: opts.ram || 32, name: 'PC-1', lang: opts.lang, debug: opts.debug, bps: opts.bps, files: opts.files, watches: opts.watches });
  });
}

(async () => {
  let r;
  r = await run(1, `log("hi " + computer.name);\nlet n = 0;\nwhile (drill.mine()) n++;\nlog("mined", n);`);
  assert.strictEqual(r.reason, 'done', JSON.stringify(r));
  assert.deepStrictEqual(r.logs, ['hi PC-1', '[warn] drill.mine(): depleted', 'mined 2']);
  console.log('ok  continuous program + blocking API calls');

  r = await run(2, `let x = 1;\nwhile (true) { x++; }`);
  assert.strictEqual(r.reason, 'error');
  assert.match(r.msg, /CPU watchdog/);
  console.log(`ok  infinite loop stopped by watchdog after ${r.ms} ms`);

  r = await run(3, `const a = [];\nwhile (true) a.push(new Array(1e5).fill(1.5));`, { ram: 16 });
  assert.strictEqual(r.reason, 'error');
  assert.match(r.msg, /memory|heap/i);
  console.log('ok  memory limit enforced: ' + r.msg);

  r = await run(4, `log(1);\nlet = ;`);
  assert.strictEqual(r.reason, 'error');
  assert.strictEqual(r.line, 2, JSON.stringify(r));
  assert.match(r.msg, /SyntaxError/);
  console.log('ok  syntax error reported at line ' + r.line);

  r = await run(5, `log("a");\nnull.boom;`);
  assert.strictEqual(r.line, 2); assert.match(r.msg, /TypeError/);
  console.log('ok  runtime error reported at line ' + r.line);

  r = await run(6, `log.constructor("return process")().exit(1);`);
  assert.strictEqual(r.reason, 'error'); assert.match(r.msg, /EvalError/);
  r = await run(61, `try { require("fs"); } catch (e) { log(e.message); }\nlog(typeof process, typeof __cb, typeof eval("1"));`);
  // require() only loads the computer's own files: Node modules do not exist in the sandbox
  assert.strictEqual(r.reason, 'error'); assert.match(r.msg, /EvalError/); assert.deepStrictEqual(r.logs, ["no file 'fs' on this computer"]);
  console.log('ok  sandbox: no Function-constructor escape, no require, no eval');

  r = await run(7, `computer.on("tick", e => log("tick", e.n));\nlog("waiting");`);
  assert.strictEqual(r.reason, 'stopped');
  assert.deepStrictEqual(r.logs, ['waiting', 'tick 1', 'tick 2', 'tick 3']);
  assert.ok(r.subs.has('tick'));
  console.log('ok  event mode (computer.on) + stop');

  r = await run(8, `drill.explode();`);
  assert.match(r.msg, /TypeError/);
  console.log('ok  unknown machine function is a clear TypeError');

  r = await run(15, `hud.set("ore", 12);\nhud.set("state", "OK");\nhud.notify("hello");`);
  assert.deepStrictEqual(r.logs, ['hud.set:["ore",12]', 'hud.set:["state","OK"]', 'hud.notify:["hello"]'], JSON.stringify(r));
  console.log('ok  hud.set / hud.notify');

  // ---- debugger
  const dbgCode = [
    'let total = 0;',                       // 1
    'const items = ["a", "b"];',            // 2
    'for (let i = 0; i < 3; i++)',          // 3
    '  total += i;',                        // 4  (embedded statement: wrapped in braces)
    'if (total > 1) log("big", total);',    // 5
    'else log("small");',                   // 6
    'function f(x) { const y = x * 2; return y; }', // 7
    'log(f(total));',                       // 8
  ].join('\n');
  r = await run(9, dbgCode, { debug: true, bps: [4, 7] , debugModes: ['continue', 'continue', 'continue', 'step', 'continue'] });
  assert.strictEqual(r.reason, 'done', JSON.stringify(r));
  assert.deepStrictEqual(r.logs, ['big 3', '6'], 'instrumented program behaves the same');
  assert.deepStrictEqual(r.pauses.map(p => p.line), [4, 4, 4, 7, 7], JSON.stringify(r.pauses.map(p => p.line)));
  assert.strictEqual(r.pauses[1].vars.i, 1); assert.strictEqual(r.pauses[1].vars.total, 0);
  assert.deepStrictEqual(r.pauses[0].vars.items, ['a', 'b']);
  assert.strictEqual(r.pauses[3].vars.x, 3, 'function params visible');
  assert.strictEqual(r.pauses[4].vars.y, 6, 'step reached next statement with y assigned');
  console.log('ok  debugger: breakpoints, step, variables (incl. loop vars and params)');

  r = await run(10, 'log(1);\nlet o = null;\no.x = 2;', { debug: true });
  assert.strictEqual(r.line, 3, 'line numbers preserved under instrumentation');
  console.log('ok  debugger keeps original line numbers in errors');

  // ---- TypeScript
  r = await run(11, 'interface P { x: number }\nconst p: P = { x: 41 };\nfunction inc(v: number): number { return v + 1; }\nlog("ts", inc(p.x));', { lang: 'typescript' });
  assert.strictEqual(r.reason, 'done', JSON.stringify(r)); assert.deepStrictEqual(r.logs, ['ts 42']);
  r = await run(12, 'type T = { a: string };\n\nconst t: T = { a: "x" };\nlet n: number = 1;\n(null as any).boom;', { lang: 'typescript' });
  assert.strictEqual(r.line, 5, 'runtime error mapped back to TypeScript line: ' + JSON.stringify(r));
  r = await run(13, 'const a: number = 1;\nconst b = ;', { lang: 'typescript' });
  assert.strictEqual(r.line, 2); assert.match(r.msg, /SyntaxError/);
  r = await run(14, 'type V = number;\nlet s: V = 0;\nfor (let i: V = 0; i < 2; i++)\n  s += i;\nlog(s);', { lang: 'typescript', debug: true, bps: [4] });
  assert.deepStrictEqual(r.pauses.map(p => p.line), [4, 4]); assert.deepStrictEqual(r.logs, ['1']);
  console.log('ok  TypeScript: runs, errors mapped to TS lines, debugger works');

  // ---- Etapa 3: call stack, step over/out, watches
  const dbgSrc = 'function add(a, b) {\n  const s = a + b;\n  return s;\n}\nconst items = [{ count: 3 }];\nlet t = add(1, 2);\nt = add(t, 4);\nlog(t);\n';
  // breakpoint on the call, step into add(), step out, step over the second call
  r = await run(90, dbgSrc, { debug: true, bps: [6], debugModes: ['step', 'out', 'over', 'continue'], watches: ['items[0].count', 'a', 'nope.x'] });
  assert.strictEqual(r.reason, 'done');
  assert.deepStrictEqual(r.pauses.map(p => p.line), [6, 2, 7, 8]);
  assert.deepStrictEqual(r.pauses[1].stack, ['add : line 2', 'main : line 6'], 'call stack inside add()');
  assert.deepStrictEqual(r.pauses[0].watches[0], ['items[0].count', 3]);
  assert.deepStrictEqual(r.pauses[1].watches[1], ['a', 1], 'watch sees the parameter inside add()');
  assert.strictEqual(r.pauses[0].watches[2][1], '(not defined here)');
  assert.deepStrictEqual(r.logs, ['7']);
  console.log('ok  debugger: call stack, step out, step over, watch paths');

  // ---- several files: require() other files on the computer, errors name the file
  r = await run(91, 'const u = require("./utils");\nlog(u.double(21));\n', { files: { 'utils.js': 'function double(x) { return x * 2; }\nmodule.exports = { double };\n' } });
  assert.deepStrictEqual(r.logs, ['42']);
  r = await run(92, 'const u = require("utils");\nu.broken();\n', { files: { 'utils.js': 'module.exports = {\n  broken() { null.x; }\n};\n' } });
  assert.strictEqual(r.file, 'utils'); assert.strictEqual(r.line, 2);
  r = await run(93, 'import { sq } from "./mathx";\nconst n: number = sq(6);\nlog(n);\n', { lang: 'typescript', files: { 'mathx.ts': 'export function sq(x: number): number {\n  return x * x;\n}\n' } });
  assert.deepStrictEqual(r.logs, ['36']);
  console.log('ok  several files: require() in JavaScript, import in TypeScript, error file + line');

  // ---- no network in the sandbox: none of the web/Node network APIs exist
  r = await run(94, 'log([typeof fetch, typeof WebSocket, typeof XMLHttpRequest, typeof process, typeof setTimeout, typeof WebAssembly === "object" ? typeof WebAssembly.instantiate : "none"].join(","));\n');
  assert.deepStrictEqual(r.logs, ['undefined,undefined,undefined,undefined,undefined,function']);
  r = await run(95, 'WebAssembly.compile(new Uint8Array([0,97,115,109,1,0,0,0])).then(() => log("compiled"), e => log("blocked", e.name));\nsleep(0.1);\n');
  assert.ok(!r.logs.includes('compiled'), 'no WebAssembly code generation for JS programs');
  console.log('ok  sandbox: no network APIs (fetch, WebSocket, XHR, process), no WebAssembly compilation');

  host.stdin.end();
  console.log('ALL RUNTIME TESTS PASSED');
})().catch(e => { console.error('FAIL', e); process.exit(1); });
