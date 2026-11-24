# SUBSYSTEMS — Subsistemas

> Índice dos subsistemas pedidos, com onde cada um vive (diretório de `src/` no PDB), o ponto de entrada no tick e o documento com o detalhe. Para evitar duplicação, o detalhe está noutros documentos.

| Subsistema | `src/` [C] | Entrada no tick / responsável [C] | Detalhe | Grau |
|---|---|---|---|---|
| Inicialização | `Main/`, raiz (`MainLoop`, `GlobalContext`, `AppManager`) | `MainLoop::run` | EXECUTION_FLOW §1 | 🟢 |
| Ciclo principal | raiz | `mainLoopStep` / `gameUpdateStep` | EXECUTION_FLOW §2–3 | 🟢 |
| Simulação (núcleo) | `Map/`, `Surface/` | `Map::updateEntities`, `Surface::update` | SIMULATION_ANALYSIS | 🟢 |
| Entidades | `Entity/` (822 ficheiros, 142 classes) | `EntityOrderList<…>::update`, `Chunk::updateEntities` | DATA_STRUCTURES §3 | 🟢 estrutura / 🟡 lógica por tipo |
| Geração do mundo | `Map/` (MapGeneration*), `Noise/` | `MapGenerationManager::update` | ALGORITHMS §11 | 🟡 |
| Rendering | `Graphics/` | `MainLoop::prepare/render` | RENDERING_ANALYSIS | 🟡 |
| Física e colisões | `Collision/`, `Util/BoundingBox` | consultas de `Surface` | PHYSICS_ANALYSIS | 🟡 |
| Pathfinding | `AI/` (PathFinder, PathFindAlgorithm), `Train/` | `PathFinder::update`, `TrainPathFinder::findPath` | ALGORITHMS §3–4 | 🟢 estrutura |
| IA | `AI/` (Commander, UnitGroup, Behaviors) | `Commander::update` | ALGORITHMS §9 | 🟡 |
| Belts | `Belt/` | `TransportLineManager` (paralelo) | LOGISTICS §1, ALGORITHMS §2 | 🟢 |
| Inserters | `Entity/Inserter` | `Inserter::update` | LOGISTICS §2 | 🟢 |
| Logística (robôs) | `Logistics/` | `LogisticNetwork::update` | LOGISTICS §4, ALGORITHMS §7 | 🟡 |
| Comboios | `Train/`, `Rail/`, `Schedule/` | `TrainManager::update` | LOGISTICS §5 | 🟡 |
| Crafting e produção | `Recipe/`, `Entity/CraftingMachine`, raiz `CraftingQueue` | `EntityOrderList` (AssemblingMachine, Furnace…) | SIMULATION §5 | 🟡 |
| Eletricidade | `Energy/` | `ElectricNetworkManager` (paralelo) | ALGORITHMS §5 | 🟢 |
| Calor | `Heat/` | `HeatBufferManager` (paralelo) | DATA_STRUCTURES §5 | 🟠 |
| Fluidos | `Fluid/` | `FluidSegmentManager` + `FluidFlowManager` | ALGORITHMS §6 | 🟢 |
| Circuitos | `CircuitNetwork/`, `Wires/` | `CircuitNetworkManager`, `ControlBehaviorManager` (paralelos) | ver abaixo | 🟠 |
| Inventários | `Item/` (Inventory* em 9 variantes) | transferências (`TransferSpecification`) | DATA_STRUCTURES §3 | 🟡 |
| Jogadores e controladores | raiz `Player`, `Controller/` (9 tipos: `ControllerType`) | `Player::update`, `ControllerManager::sendEvents` | — | 🟡 |
| Multiplayer | `Net/` | `MultiplayerManagerBase::updateMultiplayerInput` | NETWORKING_ANALYSIS | 🟢 estrutura |
| Saves | `Map/MapSerialiser`, `Package/` | `Scenario::saveMap`, `ParallelScenarioSaver` | SAVE_SYSTEM | 🟢 contentor / 🟠 payload |
| Eventos | `Script/`, raiz `GameEvents` | `GameEvents::sendOnTick/run<…>` | MODDING_API §3 | 🟢 |
| Lua / modding | `Script/`, `Data/` | `LuaGameScript::runEventHandler` | LUA_ANALYSIS, MODDING_API | 🟢 |
| Pesquisa / forças | `Technology/`, `Force/` | `ForceData::update` | — | 🟠 |
| Poluição / evolução | `Surface/`, `Map/Pollution` | `Chunk::updatePollution`, `EvolutionFactors::update` | ALGORITHMS §8 | 🟡 |
| Space Age | `Space/` (131 ficheiros) | `Planet::update`, `SpacePlatformManager`, `ItemSpoilQueue`, `FreezingLogic` | — | 🟠 |
| Undo/redo | `Undo/` | ações `Undo`/`Redo` (InputActionType 41/42) | — | 🟠 |
| Blueprints | `Blueprint/`, `Util/BlueprintImportExportEngine` | ações `ImportBlueprintString`, `SetupBlueprint`… | — | 🟠 |
| Som | `Sound/`, SDL_Mixer | thread principal | DEPENDENCIES | 🟠 |
| Input | `Input/`, SDL3 | `Game::updateInputSource` | — | 🟠 |
| HTTP / portal de mods / updater | `Http/` (curl), `ModPortal/`, `Updater/` | fora do tick | DEPENDENCIES §4 | 🟡 |

Legenda: 🟢 estrutura e fluxo confirmados por evidência · 🟡 estrutura confirmada, lógica inferida · 🟠 só inventário (nomes/layouts).

## Circuitos (resumo, não documentado noutro sítio) [C nomes; I lógica]
- `CircuitNetwork` + `CircuitConnector` (por entidade) + `WireConnector` (fios vermelho/verde; `WireType`). Os sinais são `SignalID` (item/fluido/virtual) com contagem; `OptimizedSignalID`, `MergedSignalIterator`, `CombinedSignalIterator`.
- Cada tipo de entidade tem um `XControlBehavior` (40+ classes: `InserterControlBehavior`, `TransportBeltControlBehavior`, `TrainStopControlBehavior`…) que lê ou escreve sinais. `ControlBehaviorManager` atualiza-os em paralelo e `CircuitNetworkManager` agrega as redes [I].
- Combinadores: `ArithmeticCombinatorParameters`, `DeciderCombinatorParameters`, `SelectorCombinatorParameters`; `Util/MathExpressions` (expressões em sinais) [C].
- [I] Atraso de 1 tick por combinador (comportamento público conhecido). É coerente com uma fase de leitura (prepare) e outra de escrita (finish), mas não foi verificado.
