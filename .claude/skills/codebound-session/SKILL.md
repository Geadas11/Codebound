---
name: codebound-session
description: Start and end a Codebound work session with full continuity. Use at the START of every Codebound session (before writing any code) and again near the END (when time/tokens run low, before the last commit, or when the user says to wrap up). Reads and updates NEXT_SESSION.md, TASKS.md, DEVELOPMENT_LOG.md, README.md, GAME_DESIGN.md and TECHNICAL_ARCHITECTURE.md so a new session can resume without rediscovering anything.
---

# Codebound — session continuity

Codebound is developed across many sessions (max ~5h each). Each session must
start from the recorded state and end by leaving an accurate record. Never
claim something was implemented, compiled or tested unless it really was.

## Continuity files (repo root)

| File | Purpose |
|---|---|
| `README.md` | Overview, install, how to open/run the Unity project |
| `GAME_DESIGN.md` | Game specification (the master prompt, structured) |
| `TECHNICAL_ARCHITECTURE.md` | Modules, assemblies, sandbox design, decisions |
| `DEVELOPMENT_LOG.md` | Chronological log, newest entry at the top |
| `TASKS.md` | Done / In progress / Pending, grouped by Etapa 1–8 |
| `NEXT_SESSION.md` | Concrete instructions to resume — the first file to read |

If any file is missing, create it from the templates below.

## A. Session start (do this first, keep it short)

1. `git status`, `git log --oneline -15`, current branch.
2. Read `NEXT_SESSION.md` fully, then `TASKS.md`. Skim the top entry of
   `DEVELOPMENT_LOG.md`. Read `TECHNICAL_ARCHITECTURE.md` only for the modules
   you will touch.
3. Check the toolchain the session has (Unity editor? `dotnet`? node, python,
   php, gcc/g++, sqlite3?) and note what is missing — it decides what can be
   verified this session.
4. Verify the "known state" claims in `NEXT_SESSION.md` that matter for today's
   work (e.g. run the existing test suites). If reality differs, trust reality
   and fix the docs.
5. Pick 1–3 small, completable goals from the top of `TASKS.md`
   (current Etapa first). Mark them **In progress**. Do not re-plan the whole
   game; do not rewrite working systems.
6. Start implementing immediately. No long analysis in chat.

## B. During the session

- Work in small, complete increments: implement → test → commit.
- Commit and push after each working increment (on the session's branch).
- Fix foundation-breaking bugs before moving on.
- Record notable technical decisions as you make them (one line each) so they
  can go into the log/architecture at the end.
- Watch the budget: once roughly 75% of time/tokens is used, stop starting new
  features and move to section C.

## C. Session end (always leave enough budget for this)

1. Run every test/verification available; record the real results.
2. Update `TASKS.md`: move items between Done / In progress / Pending. A task
   is **Done** only if its logic works and was verified as far as tools allow;
   otherwise mark it `(não verificado no Unity)` or keep it In progress.
3. Prepend a `DEVELOPMENT_LOG.md` entry (template below).
4. Update `TECHNICAL_ARCHITECTURE.md` if structure or decisions changed.
5. Rewrite `NEXT_SESSION.md` completely (template below) — it must be enough
   on its own to resume.
6. Update `README.md` if setup/run steps changed.
7. Commit (`docs: session handoff YYYY-MM-DD`) and push.
8. Report to the user in a few lines: what was done, what was verified and
   how, what is blocked, what is next.

## Templates

### DEVELOPMENT_LOG.md entry
```markdown
## YYYY-MM-DD — Sessão N — <tema curto>
**Feito:** …
**Verificado:** <como: testes dotnet / node / manual no Unity / não verificado>
**Decisões:** …
**Problemas / bloqueios:** …
**Commits:** <hashes curtos>
```

### NEXT_SESSION.md
```markdown
# Próxima sessão
_Atualizado: YYYY-MM-DD (Sessão N)_

## Estado atual
- Etapa em curso: …
- O que funciona e foi verificado: …
- O que existe mas não foi verificado: …

## Fazer a seguir (por ordem)
1. … (ficheiros: `…`, critério de concluído: …)
2. …

## Bloqueios e limitações conhecidas
- …

## Como verificar rapidamente o estado
- comandos exatos de teste/build
```

### TASKS.md
```markdown
# Tarefas
## Em progresso
## Pendentes
### Etapa 1 — Fundação
- [ ] …
## Concluídas
- [x] … (verificado: …)
```

## Rules carried from the master prompt

- Never replace the 10 real languages (HTML, CSS, JavaScript, SQL, Python, PHP,
  TypeScript, C, C++, C#) with pseudocode or a fictional language.
- Never run player code inside the Unity process; never pretend a sandbox
  exists before it is implemented.
- Never present a UI without working logic as a finished feature.
- If blocked (B case): explain the blocker concretely, save state, record it in
  `NEXT_SESSION.md`, and propose the closest alternative — do not silently
  change the game concept.
- No mods support; no active NPC civilisations or human NPCs; no livestock.
