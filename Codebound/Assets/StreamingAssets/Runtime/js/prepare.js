// Turns player source into runnable JavaScript, in the host process (not in the size-limited workers):
//  - TypeScript → JavaScript (TypeScript compiler, Apache-2.0, vendor/typescript) + line map for errors
//  - optional debugger instrumentation: `__dbg(line, getVars)` inserted before each statement, inline,
//    so line numbers in the player's file stay exactly the same.
'use strict';
const vm = require('vm');
let ts = null;
function tsLib() { return ts || (ts = require('./vendor/typescript/typescript.js')); }

const STATEMENT_KINDS = () => {
  const K = tsLib().SyntaxKind;
  return new Set([K.VariableStatement, K.ExpressionStatement, K.IfStatement, K.ForStatement, K.ForOfStatement,
    K.ForInStatement, K.WhileStatement, K.DoStatement, K.ReturnStatement, K.ThrowStatement, K.TryStatement,
    K.SwitchStatement, K.BreakStatement, K.ContinueStatement]);
};

function bindingNames(name, out) {
  const t = tsLib();
  if (!name) return;
  if (t.isIdentifier(name)) out.push(name.text);
  else if (t.isObjectBindingPattern(name) || t.isArrayBindingPattern(name))
    for (const el of name.elements) if (!t.isOmittedExpression(el)) bindingNames(el.name, out);
}

/** Names declared directly in a scope-introducing node (not inside nested functions). */
function declaredIn(node) {
  const t = tsLib(); const out = [];
  const stmts = t.isSourceFile(node) || t.isBlock(node) || t.isCaseClause(node) || t.isDefaultClause(node) ? node.statements : null;
  if (stmts) for (const s of stmts) {
    if (t.isVariableStatement(s)) for (const d of s.declarationList.declarations) bindingNames(d.name, out);
    else if ((t.isFunctionDeclaration(s) || t.isClassDeclaration(s)) && s.name) out.push(s.name.text);
  }
  if (t.isFunctionLike(node) && node.parameters) for (const p of node.parameters) bindingNames(p.name, out);
  if ((t.isForStatement(node) || t.isForOfStatement(node) || t.isForInStatement(node)) && node.initializer && t.isVariableDeclarationList(node.initializer))
    for (const d of node.initializer.declarations) bindingNames(d.name, out);
  if (t.isCatchClause(node) && node.variableDeclaration) bindingNames(node.variableDeclaration.name, out);
  if (t.isCaseBlock(node)) for (const c of node.clauses) out.push(...declaredIn(c));
  return out;
}

function instrument(src, isTs) {
  const t = tsLib();
  const K = t.SyntaxKind;
  const kinds = STATEMENT_KINDS();
  const sf = t.createSourceFile(isTs ? 'computer.ts' : 'computer.js', src, t.ScriptTarget.Latest, true, isTs ? t.ScriptKind.TS : t.ScriptKind.JS);
  const inserts = [];
  const lists = new Set([K.SourceFile, K.Block, K.CaseClause, K.DefaultClause, K.ModuleBlock]);
  const embeds = new Set([K.IfStatement, K.ForStatement, K.ForOfStatement, K.ForInStatement, K.WhileStatement, K.DoStatement]);

  function visible(node) {
    const names = new Set();
    for (let a = node.parent; a; a = a.parent) for (const n of declaredIn(a)) if (!n.startsWith('__')) names.add(n);
    return [...names];
  }

  function fnName(node) {
    if (node.name && t.isIdentifier(node.name)) return node.name.text;
    if (t.isConstructorDeclaration(node)) return 'constructor';
    const p = node.parent;
    if (p && t.isVariableDeclaration(p) && t.isIdentifier(p.name)) return p.name.text;
    if (p && t.isPropertyAssignment(p) && t.isIdentifier(p.name)) return p.name.text;
    return '(anonymous)';
  }

  function visit(node) {
    // call stack: every function body with braces enters/leaves a frame (inline, so line numbers do not move)
    if (t.isFunctionLike(node) && node.body && t.isBlock(node.body)) {
      const line = sf.getLineAndCharacterOfPosition(node.getStart(sf)).line + 1;
      inserts.push({ pos: node.body.getStart(sf) + 1, text: `__dbgIn(${JSON.stringify(fnName(node))},${line});try{`, pri: 1 });
      inserts.push({ pos: node.body.end - 1, text: '}finally{__dbgOut()}', pri: -1 });
    }
    if (kinds.has(node.kind) && node.parent && node.parent.kind !== K.LabeledStatement) {
      const isDirective = t.isExpressionStatement(node) && t.isStringLiteral(node.expression) && node.parent.kind === K.SourceFile;
      if (!isDirective && (lists.has(node.parent.kind) || embeds.has(node.parent.kind))) {
        const start = node.getStart(sf);
        const line = sf.getLineAndCharacterOfPosition(start).line + 1;
        const getter = '()=>{const __v={};' + visible(node).map(n => `try{__v[${JSON.stringify(n)}]=${n}}catch(__e){}`).join('') + 'return __v}';
        const probe = `__dbg(${line},${getter});`;
        if (lists.has(node.parent.kind)) inserts.push({ pos: start, text: probe });
        else { inserts.push({ pos: start, text: '{' + probe }); inserts.push({ pos: node.end, text: '}' }); }
      }
    }
    t.forEachChild(node, visit);
  }
  visit(sf);
  // apply from the end so earlier positions stay valid; at the same position, lower pri ends up later in the text
  inserts.sort((a, b) => b.pos - a.pos || (a.pri || 0) - (b.pri || 0));
  let out = src;
  for (const i of inserts) out = out.slice(0, i.pos) + i.text + out.slice(i.pos);
  return out;
}

// --- source maps (TypeScript output line → player's line) ---
const B64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
function decodeLineMap(mappings) {
  const map = []; let srcLine = 0;
  mappings.split(';').forEach((group, genLine) => {
    let first = null;
    for (const seg of group.split(',')) {
      if (!seg) continue;
      const vals = []; let v = 0, shift = 0;
      for (const ch of seg) {
        const d = B64.indexOf(ch);
        v += (d & 31) << shift;
        if (d & 32) shift += 5; else { vals.push(v & 1 ? -(v >> 1) : v >> 1); v = 0; shift = 0; }
      }
      if (vals.length >= 4) { srcLine += vals[2]; if (first === null) first = srcLine; }
    }
    if (first !== null) map[genLine + 1] = first + 1;
  });
  return map;
}

/** Returns { code, lineMap } or { error, line } with the player's line number. */
function prepare(src, lang, debug) {
  const isTs = lang === 'typescript';
  if (lang !== 'javascript' && !isTs) return { error: `Language '${lang}' is not available in the JavaScript runtime`, line: null };
  if (!isTs) {
    try { new vm.Script(src, { filename: 'computer.js' }); } // syntax check on the original source
    catch (e) { const m = String(e.stack).match(/computer\.js:(\d+)/); return { error: 'SyntaxError: ' + e.message, line: m ? Number(m[1]) : null }; }
    return { code: debug ? instrument(src, false) : src, lineMap: null };
  }
  const t = tsLib();
  const input = debug ? instrument(src, true) : src;
  const out = t.transpileModule(input, {
    fileName: 'computer.ts', reportDiagnostics: true,
    compilerOptions: { target: t.ScriptTarget.ES2022, module: t.ModuleKind.CommonJS, sourceMap: true, removeComments: false },
  });
  const diag = (out.diagnostics || []).find(d => d.category === t.DiagnosticCategory.Error);
  if (diag) {
    const line = diag.file && diag.start !== undefined ? diag.file.getLineAndCharacterOfPosition(diag.start).line + 1 : null;
    return { error: 'SyntaxError: ' + t.flattenDiagnosticMessageText(diag.messageText, '\n'), line };
  }
  const code = out.outputText.replace(/\/\/# sourceMappingURL=.*\s*$/, '');
  return { code, lineMap: decodeLineMap(JSON.parse(out.sourceMapText).mappings) };
}

module.exports = { prepare, instrument };
