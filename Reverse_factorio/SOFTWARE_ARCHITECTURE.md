# SOFTWARE_ARCHITECTURE — Arquitetura geral do software

Reconstrução baseada principalmente na **árvore de fontes recuperada do PDB**: 7 128 ficheiros em `factorio-build-JmH45D/src/`, listados em `artifacts/pdb/factorio_src_files.txt` e `factorio_src_dirs.tsv` [C].
Os nomes de diretórios, ficheiros, classes e métodos são factos [C]. As responsabilidades atribuídas a cada módulo derivam desses nomes e dos layouts de membros, e são [I] salvo indicação em contrário.

## 1. Visão em camadas

```
┌──────────────────────────────────────────────────────────────────────────────┐
│  Aplicação / Shell   Main/ · AppManager · MainLoop · Options(cxxopts)       │
│                      Benchmark · DumpData · RunReplay · Updater · ModPortal │
├──────────────────────────────────────────────────────────────────────────────┤
│  Apresentação (cliente, NÃO determinística)                                  │
│   Graphics/ (DX11·OpenGL·Metal) · Gui/ + Agui · Sound/ · Input/ · Chart/    │
│   GameView · Controller/ (Character, God, Editor, Spectator, Remote…)        │
│   Latency* (previsão local de input) · Undo/ · Blueprint/ (lado UI)          │
├──────────────────────────────────────────────────────────────────────────────┤
│  Sincronização   Net/ (Server/ClientSynchronizer, Heartbeat, TickClosure)    │
│                  InputAction (357 valores)   · Scenario::updateCrcCheck         │
├──────────────────────────────────────────────────────────────────────────────┤
│  SIMULAÇÃO DETERMINÍSTICA  ("game state")                                    │
│   Map ─┬─ Surface ── Chunk ── Entity (142 classes) ── ActiveEntities        │
│        ├─ ForceManager/Force (tecnologias, receitas, bónus)                 │
│        ├─ Managers multithread: TransportLine · ElectricNetwork · FluidFlow │
│        │    HeatBuffer · CircuitNetwork · ControlBehavior · TurretTarget    │
│        ├─ Logistics/ (robôs) · Train/ + Rail/ · AI/ (pathfinding, unidades) │
│        ├─ MapGeneration (Noise/ compilador de expressões) · Pollution       │
│        ├─ Space/ (plataformas, Space Age) · Technology/ · Trigger/ · Damage │
│        └─ Script/ ── LuaGameScript (um estado Lua por mod) ── 251 classes   │
├──────────────────────────────────────────────────────────────────────────────┤
│  Dados/Protótipos   Data/ (ModManager, PrototypeLoader, LuaDataLoader)       │
│                     *Prototype / *PrototypeList / ID<…> (tabelas imutáveis)  │
├──────────────────────────────────────────────────────────────────────────────┤
│  Infraestrutura   Util/ (FixedPoint, Serialiser, Targeter, ThreadPool,       │
│   MemoryPool, Crc, RandomGenerator, PropertyTree) · Package/ (ZIP/dir IO)    │
│   Filesystem/ · Http/ (curl) · Thread/ · Platform/ · Log                     │
├──────────────────────────────────────────────────────────────────────────────┤
│  Bibliotecas: SDL3 · Lua 5.2* · curl · zlib · zstd · minizip · re2 · Agui …  │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Princípio central [I, forte]:** separação estrita entre o **estado de jogo determinístico** e a **apresentação**.
- Os renderizadores (`GameRenderer`, `EntityRenderer`…) têm fases `prepare`/`render` próprias e só **leem** o estado [C: nomes].
- A simulação só muda por `InputAction` agendadas num `TickClosure` [C: layouts].
- O lado do cliente tem um estado paralelo de **latência** (`LatencyState`, `LatencySurface`, `LatencyInputHandler`, `LatencyUndoRedoStack`) [C], que mostra resultados previstos antes de o tick ser confirmado pelo servidor.

## 2. Módulos (diretórios de `src/`) e dimensão [C]

| Diretório | Ficheiros | Responsabilidade |
|---|---:|---|
| `Gui/` | 1 213 | todas as janelas, controlos e estilos (sobre Agui); editor de mapas, explorador de protótipos, mods GUI |
| `Entity/` | 822 | as 142 classes de entidade e os respetivos protótipos (ver DATA_STRUCTURES.md) |
| `Graphics/` | 570 | motor de desenho: atlases, sprites, draw orders, renderizadores, backends DX11/OpenGL/Metal |
| `Script/` | 542 | ponte Lua: `LuaGameScript`, 251 classes `Lua*`, filtros de eventos, `LuaRendering` |
| `Util/` | 480 | infraestrutura genérica |
| raiz `src/` | 339 | `Game`, `Player`, `MainLoop`, `Latency*`, `CraftingQueue`, `Build*`, `CommandProcessor`… |
| `Item/` | 225 | itens, pilhas, inventários (vários tipos), blueprints, ferramentas |
| `Map/` | 155 | `Map`, geração de mapa, tiles, decoratives, serialização, versão |
| `Logistics/` | 148 | redes logísticas, robôs, construção, células de roboport |
| `CircuitNetwork/` | 146 | redes de circuito, combinadores, `ControlBehavior` por tipo de entidade |
| `Trigger/` | 146 | sistema de triggers (efeitos de projéteis, explosões, criação de entidades) |
| `Input/` | 139 | teclado, rato, gamepad, ações configuráveis |
| `Achievement/` | 134 | conquistas |
| `Net/` | 133 | multiplayer |
| `Space/` | 131 | Space Age: plataformas, planetas, cargo pods, "processions" (animações de transição) |
| `Sound/` | 107 | sistema de som (SDL_Mixer) |
| `Technology/` | 93 | árvore tecnológica, triggers de pesquisa |
| `Data/` | 82 | mods, configurações de mods, carregamento de protótipos |
| `Surface/` | 82 | superfícies, chunks, entidades ativas, poluição, relâmpagos, território |
| `AI/` | 81 | pathfinding e comportamentos das unidades |
| `ID/` | 81 | identificadores tipados de protótipos e filtros |
| `Noise/` | 81 | compilador/avaliador de noise expressions (geração de terreno) |
| `Equipment/` | 80 | grelhas de equipamento |
| `Blueprint/` | 72 | blueprints (string import/export, livro, biblioteca) |
| `Energy/` | 62 | fontes de energia (burner, elétrica, fluida, calor, void), redes elétricas |
| `Rail/` + `Train/` | 59 + 43 | carris, sinais, blocos, comboios, pathfinding de comboios |
| `Package/` | 50 | leitura/escrita de ZIP e diretórios, com versões multithread |
| `Fluid/` | 48 | fluidos (segmentos) |
| `Undo/` | 45 | undo/redo |
| `Wires/` | 43 | fios de cobre e de circuito |
| outros (< 40) | — | `Chart/` (mapa), `Force/`, `Schedule/`, `Belt/`, `Collision/`, `Recipe/`, `Heat/`, `Mining/`, `Http/`, `ModPortal/`, `DebugAdapter/`, `SteamIntegration/`, `NintendoSwitch/`, `MacOS/`, `Benchmark/`, `Updater/`… |

A presença de `NintendoSwitch/` (13), `MacOS/`, `Graphics/Metal/` e `core/linux` mostra **um único código multiplataforma** compilado com seleção por plataforma [C].

## 3. Padrões arquiteturais identificados

| Padrão | Evidência [C] | Interpretação |
|---|---|---|
| **Protótipo / instância** | `XPrototype` + `X` para quase tudo (`InserterPrototype`/`Inserter`, `FluidPrototype`/`Fluid`…); `XPrototypeList` | os dados imutáveis vêm de Lua (data stage); as instâncias guardam um ponteiro `const XPrototype*` (ex.: `Entity::prototype` @+72) |
| **IDs tipados compactos** | `ID<ItemPrototype,unsigned short>`, `ID<QualityPrototype,unsigned char>`, `IDWithQuality<…>` | protótipos referenciados por índices de 8/16 bits: estado compacto e serialização barata |
| **Herança + casting virtual** | `Entity::asInserter`, `asTransportBelt`… (≈130 métodos `asX`) | substitui `dynamic_cast` por despacho virtual barato [I] |
| **Listas intrusivas** | `IntrusiveListImplementation<…>::Hook` em `UpdatableEntity`, `TransportLine`, `Chunk`, `ElectricNetwork` | sem alocações nas listas de atualização |
| **Sleep/wake-up** | `Wakeable<…>`, `WakeUpList`, `Inserter::goToSleep`, `tryToGetToSleep`, `TransportLine::inputWakeUpList/outputWakeUpList` | entidades sem trabalho saem da lista ativa e são acordadas por eventos: a chave da escalabilidade |
| **Atividade por chunk** | `Chunk::activeWhenSomeoneAround`, `activeWhenPollutionExists`, `ActiveEntitiesWhenEnemyIsAround`, `Chunk::shouldBeActive` | entidades militares/naturais só atualizam perto de jogadores ou inimigos |
| **Targeter/Targetable** | `Targetable` (base de `Entity`, `ElectricNetwork`…), `Targeter<Entity,…>`, `NotifiableTargeter` | referências fracas seguras entre objetos, anuladas quando o alvo morre; serializáveis |
| **Managers multithread** | 7 subclasses de `MultithreadedUpdatable { prepare(); update(); finish(); }` | os sistemas em rede atualizam em paralelo (ver ENGINE_ANALYSIS §3) |
| **Serialização simétrica** | `save(MapSerialiser&)`/`load(MapDeserialiser&)` em quase todas as classes; `XSerialiser`/`XDeserialiser` dedicados | um único formato binário para saves, sincronização multiplayer e CRC |
| **Ponto fixo** | `FixedPointNumberTemplate<int,8,0>` (posições), `<__int64,24,1>` (fluidos), `<__int64,8,0>` (energia; o balanço da rede é em `double` [C/G]) | aritmética determinística entre plataformas e compiladores |
| **Máquinas de estado da aplicação** | `AppManager::pushState<InGame>`, `<InMainMenu>`, `<InModsDialog>`… | pilha de estados de UI |
| **Adapters/decorators** | `GameStateAdapter`, `LatencyStateAdapter`, `WiresInterfaceDecoratorBase`, `GameAdapterDecoratorBase` | a mesma lógica de UI funciona sobre o estado real ou o previsto |

## 4. Fronteira motor ↔ Lua (resumo; detalhe em LUA_ANALYSIS.md e MODDING_API.md)
- **Data stage** (arranque): `Data/LuaDataLoader` + `PrototypeLoader` executam `data.lua`/`data-updates.lua`/`data-final-fixes.lua` de cada mod e convertem a tabela `data.raw` em `*Prototype` C++ [C: nomes; log R/2.0.60 "Loading mod base (data.lua)"].
- **Control stage** (jogo): cada mod tem um `LuaGameScript` com o seu `LuaState`. Os eventos C++ (`LuaEventType`, 200+ valores) são despachados para handlers registados. Os objetos C++ são expostos como userdata `Lua*` com referência fraca (`Targeter<Entity>` dentro de `LuaEntity`) [C].
- **Toda a simulação "pesada" é C++** [C]. Belts, inserters, fluidos, eletricidade, robôs, comboios e pathfinding não têm implementação Lua em `data/`. O Lua de `data/` só define protótipos e a lógica de cenário (freeplay, pvp…).

## 5. Diagramas
Ver ARCHITECTURE_DIAGRAMS.md (Mermaid): camadas, dependências entre módulos, ciclo de tick, pipeline de dados, fluxo multiplayer e serialização.
