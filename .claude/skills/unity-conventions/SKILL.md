---
name: unity-conventions
description: Codebound's fixed Unity/C# architecture, folder layout, naming and coding rules. Use whenever creating or editing any Codebound code, asset folder, assembly definition, scene, test or the player-code sandbox, so every session produces consistent, modular code. Also use when deciding where a new system or file belongs.
---

# Codebound — Unity & code conventions

Unity 2D (URP 2D renderer, pixel-perfect camera), C#, top-down grid game.
Target platforms: Windows, Linux, macOS. Keep the project openable and
compiling at every important step.

## 1. Repository layout

```
/                         continuity docs (README, GAME_DESIGN, TASKS, …)
/Codebound/               the Unity project (open this folder in Unity Hub)
  Assets/_Codebound/
    Scripts/<Module>/Logic/  pure C# simulation code, Codebound.<Module>.asmdef (noEngineReferences: true)
    Scripts/Game/         Unity presentation layer (Codebound.Game.asmdef) — input, views, HUD
    Editor/               editor-only tools (Codebound.Editor.asmdef)
    Tests/EditMode/       NUnit EditMode tests
    Tests/PlayMode/       PlayMode tests
    Art/Sprites|Tiles|Animations|Materials
    Audio/Music|Sfx|Ambience
    Data/                 ScriptableObject definitions (items, machines, recipes)
    Prefabs/<Module>/
    Scenes/               Boot.unity, TestWorld.unity, …
    Localization/
    UI/                   UXML/USS (UI Toolkit)
  Packages/manifest.json
  ProjectSettings/
/Runtime/                 player-code execution service (outside Unity)
  host/                   sandbox supervisor (process isolation, limits)
  langs/<language>/       per-language runner + Codebound API binding
  api/                    API spec shared by all languages (JSON schema)
  tests/
```

## 2. Modules (namespace = `Codebound.<Module>`, asmdef = `Codebound.<Module>`)

`Core` (bootstrap, services, events, time, utilities) · `World` · `Building` ·
`Inventory` · `Items` · `Machines` · `Energy` · `Networks` · `Programming`
(editor, debugger, docs, files) · `Execution` (Unity-side client of the
sandbox) · `HUD` · `UI` · `AI` · `Combat` · `Survival` · `Multiplayer` ·
`Save` · `Research` · `Audio` · `Localization` · `Player`.

Dependency rules:
- Everything may depend on `Core`. `Core` depends on nothing in Codebound.
- No cyclic asmdef references. Cross-module communication goes through
  interfaces in the lower module or events on the `Core` event bus.
- Gameplay logic in plain C# classes under `Logic/` with `noEngineReferences: true`
  (compiled and tested by `Tools/LogicTests` with `dotnet test`). MonoBehaviours in
  `Scripts/Game` are thin adapters (input, rendering, lifecycle).
- Logic code uses `Codebound.Core.Vec2`/`GridPos`, never `UnityEngine.Vector2`.
- Data lives in ScriptableObjects under `Data/`, referenced by stable string IDs
  (e.g. `item.iron_ore`, `machine.drill_basic`) — never by display name.

## 3. C# style

- **C# 9 only** (Unity's compiler): block-scoped `namespace X { }`, no file-scoped
  namespaces, no `record struct`, no `init`/`required`, no global usings.
- One main public type per file; file name = type name.
- `PascalCase` types/methods/properties, `_camelCase` private fields,
  `camelCase` locals/params, `I` prefix for interfaces.
- `[SerializeField] private` instead of public fields.
- No `FindObjectOfType`/`GameObject.Find` in gameplay code; use explicit
  references or the service locator in `Core`.
- No per-frame allocations in hot paths (Update, ticks); avoid LINQ there.
- Simulation runs on a fixed tick (`Core.SimulationClock`, default 20 ticks/s),
  independent of frame rate — machines, energy, conveyors and program
  scheduling use ticks, not `Time.deltaTime`.
- Grid: `Vector2Int` cell coords, 1 cell = 1 world unit, 16 px per unit
  (pixel art). Rotations as `enum Direction { North, East, South, West }`.
- XML doc comments on public APIs. Errors: validate inputs, log with
  `CbLog` (module-tagged), never swallow exceptions silently.
- All player-visible strings go through `Localization` keys — never hardcode
  UI text. Programming-language syntax is never translated.

## 4. Player-code execution (critical)

- Player code NEVER runs in the Unity process. Unity (`Codebound.Execution`)
  talks to the external `/Runtime` host over a local IPC channel
  (JSON messages over stdio or a localhost socket).
- Each program runs in an isolated child process with: CPU-time limit, memory
  limit, wall-clock timeout, no filesystem/network access outside what the
  game grants, kill on infinite loop, structured error reporting.
- The world API (e.g. `drill.mine()`, `energy.available()`,
  `computer.on("resource_detected", …)`) is defined once in `/Runtime/api`
  and bound per language. Calls are messages to Unity, validated against the
  machine's permissions and the computer's hardware budget (CPU/RAM/storage/
  network/energy) — inefficient programs cost real in-game resources.
- Support both continuous loops and event handlers.
- A sandbox feature is only "done" when a test proves the limit is enforced
  (e.g. an infinite loop is killed within the timeout).
- Languages: HTML, CSS, JavaScript, SQL, Python, PHP, TypeScript, C, C++, C#.
  Never substitute pseudocode or a fictional language.

## 5. Dependencies

- Prefer Unity packages already in the registry (URP, Input System, UI
  Toolkit, Netcode for GameObjects, Localization, Test Framework).
- Before adding any third-party package/runtime: check licence (must allow
  commercial distribution), platform support (Win/Linux/macOS) and need;
  record the decision in `TECHNICAL_ARCHITECTURE.md`.
- No mod support.

## 6. Testing & verification

- New gameplay logic gets EditMode tests (`Codebound.<Module>.Tests`).
- EditMode tests in `Tests/EditMode` must only reference Logic assemblies, so the
  same files run in Unity and in `Tools/LogicTests` (`dotnet test`).
- Unity-side code: compile-check with `Tools/UnityCompileCheck` (see README).
- `/Runtime` gets its own automated tests (limits, API calls, errors).
- In the docs, always state HOW something was verified; if the Unity editor
  was not available, say "não verificado no Unity".

## 7. Commits

- Small, focused commits: `<module>: <what>` (e.g. `building: grid placement
  preview with obstacle check`).
- Do not commit `Library/`, `Temp/`, `Obj/`, `Build/`, `Logs/`, `UserSettings/`
  (keep the Unity `.gitignore` in place). Commit `.meta` files.
