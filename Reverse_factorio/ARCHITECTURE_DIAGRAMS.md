# ARCHITECTURE_DIAGRAMS — Diagramas

Diagramas em Mermaid (renderizam no GitHub, VS Code e Obsidian). Cada nó vem de um nome do PDB [C]; as setas indicam chamadas diretas observadas [C] ou relações inferidas (assinaladas com `..>`/tracejado) [I].

## 1. Camadas e dependências entre módulos
```mermaid
flowchart TB
  subgraph App[Aplicação]
    Main --> MainLoop --> AppManager
  end
  subgraph Pres[Apresentação - thread principal]
    Graphics --> Backends[DX11 / OpenGL / Metal]
    Gui --> Agui
    Sound --> SDL_Mixer
    Input --> SDL3
  end
  subgraph Sync[Sincronização]
    Net --> UDP[UDPSocket]
    Latency[Latency*]
  end
  subgraph Sim[Simulação determinística - thread de update]
    Scenario --> Game --> Map --> Surface --> Chunk --> Entity
    Map --> Managers[7 MultithreadedUpdatable]
    Map --> TrainManager
    Surface --> PathFinder & Commander & MapGenerationManager
    Scenario --> Script[LuaContext / LuaGameScript]
  end
  subgraph Data[Dados]
    ModManager --> PrototypeLoader --> Prototypes[XPrototype]
  end
  subgraph Infra[Infraestrutura]
    Util & Package & Http --> curl
    Package --> zlib & minizip
  end
  MainLoop --> Scenario
  MainLoop --> Graphics
  Net -. TickClosures .-> Scenario
  Graphics -. lê .-> Map
  Entity -. const XPrototype* .-> Prototypes
  Script -. proxies Targeter .-> Entity
```

## 2. Threads e sincronização
```mermaid
sequenceDiagram
  participant M as Thread principal (MainLoop::mainLoopStep)
  participant U as Thread de update (gameUpdateLoop)
  participant P as ThreadPool workers
  loop cada frame
    M->>M: _Mtx_lock / _Cnd_wait
    M->>M: prePrepare, prepare (DrawQueue)
    M->>M: prepareBitmaps, render
    M->>M: swapRenderBuffers
  end
  loop cada tick
    U->>U: updateInputSource, updateMultiplayerInput
    U->>U: Scenario::updateStep → update → updateCrcCheck, on_tick, Lua GC step
    U->>U: Game::update → Map::updateEntities
    U->>P: runUpdate({TransportLine})
    U->>U: FluidSegmentManager, Surface::update, performPlannedEntityUpdates
    U->>P: runUpdate({TurretTarget}), Trains/Forces/Players, runUpdate({ControlBehavior})
    U->>P: runUpdate({Electric, Heat, FluidFlow, Circuit})
    P-->>U: finish() = ações aplicadas por ordem de grupo
    U->>U: LatencyInputHandler::update
  end
```

## 3. Tick (fluxograma)
```mermaid
flowchart TD
  A[gameUpdateStep] --> B[updateInputSource]
  B --> C[updateMultiplayerInput]
  C --> D[Scenario::updateStep]
  D --> E[Scenario::update]
  E --> E1[applyStagedChanges]
  E1 --> E2[updateCrcCheck]
  E2 --> E3[on_tick / on_nth_tick]
  E3 --> E4[luaC_forcestep]
  E4 --> F[Game::update]
  F --> G[Map::updateEntities]
  G --> G1[MapGeneration · trains staged · chain signals · alarms]
  G1 --> G2[FluidSegmentManager::update]
  G2 --> S[Surface::update ×N]
  S --> S1[Lightning · PathFinder · Commander · pollution]
  S1 --> S2[EntityOrderList por tipo]
  S2 --> S3[Chunk::updateEntities]
  S3 --> G3[planned updates · TrainManager · ForceData · Player · Planet · spoil · evolution]
  G3 --> H[chart · consistency · Player::postUpdate]
  H --> I[LuaGameScript::postUpdate · Map::postUpdate]
  I --> J[LatencyInputHandler::update]
```

## 4. Pipeline de dados (mods → protótipos → jogo)
```mermaid
flowchart LR
  L1[data.lua por mod] --> L2[data-updates.lua] --> L3[data-final-fixes.lua]
  L3 --> R[data.raw]
  R --> LDL[LuaDataLoader::exportData]
  LDL --> PL[PrototypeLoader::loadPrototypes]
  PL --> P[XPrototype + ID u8/u16]
  P --> CRC[checksum por mod + Prototype list checksum]
  P --> Game[Map / Entity instâncias]
  C1[control.lua por mod] --> LGS[LuaGameScript + storage]
  LGS <-->|eventos / API| Game
```

## 5. Multiplayer
```mermaid
sequenceDiagram
  participant C as Cliente
  participant S as Servidor
  C->>S: ConnectionRequest
  S->>C: ConnectionRequestReply / AcceptOrDeny
  S->>C: MapReadyForDownload
  C->>S: TransferBlockRequest
  S->>C: TransferBlock ×N
  C->>C: carregar mapa, TryingToCatchUp
  S->>C: ClientShouldStartSendingTickClosures
  loop cada tick
    C->>S: ClientToServerHeartbeat (InputActions p/ tick futuro)
    S->>C: ServerToClientHeartbeat (TickClosures ordenadas)
    C->>C: aplicar TickClosure no tick T; CheckCRC
  end
  Note over C,S: CRC divergente → PlayerDesynced → novo download + relatório
```

## 6. Save
```mermaid
flowchart LR
  Map -->|Map::save| MS[MapSerialiser]
  MS --> T[ThreadedChunkedZipPackageWriteStream]
  T -->|fatias 1 MiB · zlib em threads| Z[level.dat0..N stored]
  T --> MD[level.datmetadata u64]
  LGS[LuaGameScript::saveScriptState] --> SD[script.dat]
  Init[estado inicial] --> LI[level-init.dat]
  Z & MD & SD & LI --> ZIP[save.zip]
```

## 7. Hierarquia de entidades (resumo; completa em `artifacts/entity_hierarchy.txt`)
```mermaid
classDiagram
  Targetable <|-- Entity
  Entity <|-- EntityWithHealth
  EntityWithHealth <|-- EntityWithOwner
  EntityWithOwner <|-- Inserter
  EntityWithOwner <|-- CraftingMachine
  CraftingMachine <|-- AssemblingMachine
  CraftingMachine <|-- Furnace
  AssemblingMachine <|-- RocketSilo
  EntityWithOwner <|-- TransportBeltConnectable
  TransportBeltConnectable <|-- TransportBelt
  TransportBeltConnectable <|-- UndergroundBelt
  TransportBeltConnectable <|-- SplitterBase
  EntityWithOwner <|-- Vehicle
  Vehicle <|-- Car
  Vehicle <|-- RollingStock
  RollingStock <|-- Locomotive
  EntityWithOwner <|-- FlyingRobot
  FlyingRobot <|-- RobotWithLogisticInterface
  RobotWithLogisticInterface <|-- LogisticRobot
  RobotWithLogisticInterface <|-- ConstructionRobot
  UpdatableEntity <|-- Inserter
  UpdatableEntity <|-- CraftingMachine
  class Entity { +prototype +position +boundingBox +surface }
  class UpdatableEntity { +chunk +flags +mode }
```
