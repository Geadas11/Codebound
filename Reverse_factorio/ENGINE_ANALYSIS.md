# ENGINE_ANALYSIS — Análise do motor de jogo

> Para não repetir conteúdo: a ordem exata do tick está em **EXECUTION_FLOW.md** (canónico), os algoritmos em **ALGORITHMS.md** e os layouts em **DATA_STRUCTURES.md**. Este documento resume as propriedades do motor e como se relacionam.

## 1. Identidade do motor
- Motor **próprio** em C++ (MSVC 14.44), um único executável com todo o motor e bibliotecas estáticas [C]. Não usa motor comercial [C, pesquisa nas 10 712 fontes do PDB]. As únicas correspondências a "unity/unreal/godot" são `BeltImmunity*` e `Unity1.cpp…UnityN.cpp`. Estes últimos são ficheiros de **unity build** do FASTBuild (vários `.cpp` concatenados para compilar mais depressa), não o motor Unity.
- Lua 5.2 modificado só como camada de **dados** e de **scripts de jogo** [C].
- O mesmo binário serve cliente, servidor headless, benchmark, replay, testes e simulações do menu: todos chamam `Scenario::updateStep` [C, callers: `MainLoop::gameUpdateStep`, `MainTasks::benchmark/runReplay/runTest/untilTick`, `SimulationWidget`].

## 2. Princípios de desenho (com evidência)

| Princípio | Evidência [C] | Consequência [I] |
|---|---|---|
| **Simulação determinística** | ponto fixo em posições/fluidos/energia; RNG taus88 no estado; GC Lua `luaC_forcestep` 1×/tick; Lua sem `io/os`; build `-Brepro` | lockstep multiplayer e replays exatos |
| **Tick fixo** | `MapTick` u64; `Map::updateTick/entityTick/ticksPlayed`; `GameSpeed` | a simulação avança em passos discretos; o render é independente |
| **Separação estado/apresentação** | threads de update e de render; `swapRenderBuffers`; `LatencyState` | o render nunca altera o jogo; o cliente prevê o input sem o aplicar |
| **Trabalho proporcional à atividade, não ao tamanho** | sleep/wake, ativação por chunk, entidades sem `UpdatableEntity` | fábricas grandes e paradas custam pouco |
| **Agregação** | belts fundidas em `TransportLine`, canos em `FluidSegment`, eletricidade por categoria | O(estruturas), não O(entidades) |
| **Paralelismo onde é seguro** | 7 managers `MultithreadedUpdatable`; `ThreadPool`; saves/zip/atlas/sons em paralelo | o código das entidades é sequencial; os sistemas em grafo são paralelos com `finish` ordenado |
| **Mudanças adiadas** | `Map::applyStagedChanges` (várias vezes por tick), `toBeDeleted`, `removeEntitiesStagedForDeletion` | iteradores estáveis e ordem determinística |
| **Dados orientados a protótipos** | `XPrototype` imutáveis carregados de Lua; IDs u8/u16 | mods mudam conteúdo sem código |

## 3. Componentes centrais e relação
```
MainLoop ─(thread update)─► Scenario ─► Game ─► Map ─┬─► Surface* ─► Chunk* ─► Entity*
   │                          │                       ├─► managers paralelos (belts, elétrica, fluidos, calor, circuitos, torretas)
   │                          ├─► LuaContext/LuaGameScript (eventos)        ├─► TrainManager, ForceData, Players, Planets
   │                          └─► CRC / replay / autosave                    └─► MapGenerationManager, PathFinder(s), Commander(s)
   └─(thread principal)─► AppManager (estados de UI) ─► GameRenderer/GuiRenderer ─► backend DX11/GL/Metal
```

## 4. Inicialização
Ver EXECUTION_FLOW §1 e §7. Pontos-chave [C/R]: `GlobalContext::init` (configuração, paths, gráficos, áudio), `ModManager` (feature flags: `expansion-shaders`, `freezing`, `quality`, `rail-bridges`, `segmented-units`, `space-travel`, `spoiling` [R/2.0.60]), data stage Lua, checksums, sons, atlases e menu.

## 5. Gestão de entidades
- **Criação**: `Surface::addEntity` → `Entity::setup` (registo no chunk, ligação a redes, wake-up lists); `Map::registerEntityByUnitNumber` (u64 `nextUnitNumber`) [C].
- **Atualização**: só `UpdatableEntity` ativas; agrupadas por tipo numa ordem fixa (`EntityOrderList<…>` em EXECUTION_FLOW §3) [C].
- **Destruição**: `Entity::destroy` → staged → `removeEntitiesStagedForDeletion`; os `Targeter` são anulados [C nomes, I mecanismo].
- **Identidade**: `unit_number` u64 + `Targetable` para referências fracas [C].

## 6. Multiplataforma
`src/Core/{Windows,Linux}`, `Platform/`, `MacOS/`, `NintendoSwitch/` (13), backends gráficos e de áudio por plataforma, SDL3 para janela e input [C].

## 7. Ferramentas internas embutidas [C]
`Benchmark/PerformanceLogger`, `Main/{Benchmark, DumpData, DumpIconSprites, DumpPrototypeLocale, GenerateMapPreview, MapToScenario, ScenarioToMap, RunReplay, RunTest, UntilTick, UploadLogFile}`, `ConsistencyScraper`, `Debug/{DebugTools, TileCorruptionDetector, ImguiSDL}`, `DebugAdapter/`, `LuaProfiler`, `SlowDetector`, `StackOverflowWatcher`, `CrashHandler`.
