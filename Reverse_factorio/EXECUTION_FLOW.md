# EXECUTION_FLOW — Fluxo de execução (documento canónico do ciclo de tick)

## Método
Cada sequência abaixo é a **ordem das chamadas diretas (`call rel32`) dentro do corpo da função**. Os corpos usam os intervalos exatos do PDB (`S_GPROC32`) e foram lidos dos bytes do exe com `scripts/calls.py`. Os chamadores foram obtidos com `scripts/callers.py`. Saídas completas em `artifacts/calls_tick.txt`, `artifacts/calls_subsystems.txt` e `artifacts/callers_tick.txt` [C].
**Limites do método:** (1) a ordem por endereço é a ordem de execução em código linear, mas ramos e ciclos não estão representados; (2) chamadas **virtuais** e por `std::function` não aparecem. Quando um passo vem dessas chamadas indiretas, está marcado [I].

## 1. Arranque e threads [C]

```
WinMainCRTStartup (entry RVA 0x1E3E848)  → main (src/Main/Main) → MainLoop::run
MainLoop::run
  GlobalContext::GlobalContext / init         ← configuração, paths, gráficos, áudio, mods
  AppManager::displayUpdateFinishedChangelog, deferNotice…
  MainLoop::processMessage / processEvents
  _beginthreadex  ─────────────► thread de UPDATE:  MainLoop::gameUpdateLoop → MainLoop::gameUpdateStep (ciclo)
  MainLoop::prePrepare / prepare
  MainLoop::gameUpdateLoop      (também chamado diretamente: arranque síncrono [I])
  MainLoop::mainLoopStep        ← ciclo da thread PRINCIPAL (janela + render)
  PerformanceLogger::saveToFile, cancelOrWaitForLoadingMap/SavingToFinish, Scenario::saveAs (gravação na saída)
```
- `MainLoop::UpdateThreadListener::onThreadAvailable/onThreadFinished/onThreadRaisedException` [C]: a thread de update é gerida explicitamente.
- `Map::createUpdateRunner → createMultiThreadedUpdateRunner → ThreadPool::ThreadPool` [C]: um pool de workers para os managers paralelos (§4).

## 2. Thread principal: frame [C]
```
MainLoop::mainLoopStep
  _Mtx_lock / _Cnd_wait / _Mtx_unlock      ← sincroniza com a thread de update (mutex + condition variable)
  MainLoop::prePrepare
  MainLoop::prepare                         ← recolha do que desenhar (GameRenderer::prepare…)
  WorkerThread::run                         ← trabalho paralelo de preparação
  MainLoop::prepareBitmaps
  MainLoop::render                          ← submissão ao backend gráfico
  GlobalContext::swapRenderBuffers          ← double buffering dos dados de render
  FrameTimeStatistics::nextFrame, PerformanceLogger::endFrame
  ReloadableSpritesManager::tryReloadSprite
```
[I, forte] O render e a simulação correm em threads diferentes. A troca acontece num ponto sincronizado (mutex/condvar) e os dados de render estão em **buffer duplo** (`swapRenderBuffers`). A simulação nunca espera pela GPU além desse ponto.

## 3. Thread de update: um tick [C]

```
MainLoop::gameUpdateStep
 ├─ (ramo de save/load: MapSerialiser → Map::applyStagedChanges → Map::save → LuaContext::save;
 │                     MapDeserialiser → Scenario::loadFactory)
 ├─ MainLoop::checkHeavyModeCrc
 ├─ Game::updateInputSource                     ← ações locais (teclado/rato/replay)
 ├─ MultiplayerManagerBase::updateMultiplayerInput  ← TickClosures recebidas da rede
 ├─ Scenario::updateStep  ──────────────────────────────────────────────┐
 ├─ MainLoop::checkHeavyModeCrc                                         │
 ├─ LatencyInputHandler::update                 ← previsão local (latency state)
 └─ Map::updateStopLevelFromScenarioMessageDialog                        │
                                                                         ▼
Scenario::updateStep                     (chamado também por MainTasks::benchmark/runReplay/runTest/untilTick
 ├─ Scenario::update ───────────────┐      e SimulationWidget: o MESMO passo serve jogo, benchmark,
 ├─ LuaGameScript::postUpdate       │      replay e simulações do menu [C, callers])
 ├─ Map::postUpdate                 │
 ├─ Scenario::disableReplayInternal │
 └─ LuaContext::applyLuaObjectStatistics
                                    ▼
Scenario::update
 ├─ getAutosaveState, handleFinished
 ├─ Map::applyStagedChanges
 ├─ Scenario::updateCrcCheck        ← CRC: crc32 + MapSerialiser/Map::save + HeuristicCrcGenerator::get
 │                                    → InputAction CheckCRC enviada (tryToSendInputAction)
 ├─ GameEvents::sendOnTick          ← evento Lua on_tick  (ANTES da simulação das entidades)
 ├─ GameEvents::sendOnNthTicks      ← on_nth_tick
 ├─ luaC_forcestep                  ← um passo de GC do Lua por tick (GC incremental controlado)
 ├─ LuaObject list clear_and_delete
 ├─ Map::applyStagedChanges
 ├─ Game::update ───────────────────┐
 ├─ Scenario::checkReloadScript     │
 └─ Scenario::checkReloadMods       │
                                    ▼
Game::update
 ├─ Map::dispatchInitialGeneratedChunkEvents
 ├─ Map::updateEntities ────────────┐   ← NÚCLEO DA SIMULAÇÃO
 ├─ MapSavedSpecialItems::update    │
 ├─ ConsistencyScraper::step        │   ← verificação incremental de consistência
 ├─ Chart::updateGameState          │   ← mapa (charting)
 └─ Player::postUpdate              │
                                    ▼
Map::updateEntities   (ordem por endereço; a ORDEM REAL, descompilada, está em GHIDRA_FINDINGS §1:
                       os itens 1–8 abaixo pertencem ao ramo "entidades pausadas")
  1  MapGenerationManager::update                   geração de chunks pedida
  2  Surface::updateBuildEffectSmokes
  3  TrainManager::applyStagedChanges
  4  RailChainSignal::updateChainSignals
  5  ControllerManager::sendEvents
  6  Player::generateFarAwayChunks / requestToChartNeighbourChunks
  7  ExtraScriptData::update
  8  Map::applyStagedChanges, updateLocalFlyingTexts
  9  AlarmQueue::update                             alarmes agendados por tick
 10  FluidSegmentManager::update                    reorganização dos segmentos de fluido
 11  Surface::update   (por superfície) ──────┐
 12  Map::performPlannedEntityUpdates          │     updateInNTicks: entidades agendadas para um tick futuro
 13  TrainManager::update                      │
 14  ForceData::update                         │     pesquisa, estatísticas por força
 15  Player::update                            │
 16  Planet::update                            │     Space Age
 17  FlowStatistics::Precision::onNextTick     │
 18  HealingManager (buckets)                  │     regeneração de vida distribuída por baldes de ticks
 19  ExtraScriptData::update
 20  ItemSpoilQueue::updateBuckets             │     apodrecimento (Space Age)
 21  AutoTrasher::execute
 22  EvolutionFactors::update                  │     evolução dos inimigos
 23  ScriptRendering::destroyObject (objetos expirados), Map::applyStagedChanges
     + [C/G] Map::runUpdate em 4 lotes: {TransportLine} antes de FluidSegment/Surface; {TurretTarget} após
           performPlannedEntityUpdates; {ControlBehavior} após Player::update; {Electric, Heat, FluidFlow,
           Circuit} antes de Planet::update (GHIDRA_FINDINGS §1)
                                              ▼
Surface::update
  LightningAttractorCache / LightningGenerator  (relâmpagos)
  PathFinder::update                         ← pathfinding com orçamento por tick
  Commander::update                          ← IA de grupos/unidades
  MapGenerationManager::update, updateBuildEffectSmokes
  Chunk::updatePollution                     ← difusão de poluição (chunks poluídos)
  EntityOrderList<LogisticRobot, ConstructionRobot, Inserter, Roboport, Loader, AssemblingMachine,
     AgriculturalTower, OldAgriculturalTower, Furnace, MiningDrill, FluidWagon, ArtilleryWagon,
     InfinityCargoWagon, CargoWagon, Locomotive, Character, Boiler, Generator, BurnerGenerator, Reactor,
     Lab, LandMine, ArtilleryFlare, ArtilleryProjectile, ArtilleryTurret, Beam, Car, SpiderVehicle,
     TemporaryContainer, CharacterCorpse, CombatRobot, CaptureRobot, Corpse, ElectricEnergyInterface,
     EnemySpawner, Explosion, FlameThrowerExplosion, FluidStream, FluidTurret, FlyingTextEntity,
     FusionGenerator, FusionReactor, Gate, HeatInterface, HighlightBoxEntity, InfinityContainer,
     InfinityPipe, ItemRequestProxy, OffshorePump, ParticleSource, PowerSwitch, Projectile, Pump, Valve,
     Radar, ProgrammableSpeaker, RocketSilo, RocketSiloRocket, CargoPod, SmokeWithTrigger, SpeechBubble,
     Sticker, Turret, AsteroidCollector, Asteroid, Thruster, SpiderUnit, Unit>::update
                                             ← entidades ativas, AGRUPADAS POR TIPO nesta ordem fixa [C: nome
                                               do template; I: agrupamento por localidade de cache + determinismo]
  Chunk::shouldBeActive / Chunk::updateEntities → Chunk::updateActiveEntities
                                             ← entidades "ativas condicionalmente" (perto de jogadores/inimigos)
  EvolutionFactorData::increaseEvolutionFactorByPollution
  SegmentedUnitController::update, ParticleSurface::update
  GameEvents::sendOnNextDayStarted, Wind::update
  PathFinder::pollScriptPathfindClients      ← resultados de pathfinding pedidos por scripts
  FreezingLogic::update                      ← congelamento (Aquilo, Space Age)
```

### Observações sobre a ordem
- **`on_tick` corre antes das entidades** [C: `sendOnTick` precede `Game::update` em `Scenario::update`].
- **O GC do Lua avança um passo fixo por tick** (`luaC_forcestep`) [C]. Isto mantém a GC determinística e com custo distribuído [I].
- **O CRC de desync é calculado no início do tick**, antes de qualquer alteração [C: `updateCrcCheck` antes de `Game::update`].
- `Map::applyStagedChanges` é chamado várias vezes [C]. As mudanças pedidas durante uma fase são **adiadas para pontos fixos** [I], o que evita invalidar iteradores.
- Belts **não** estão na `EntityOrderList`: `TransportBelt`, `UndergroundBelt`, `Splitter` e `TransportLine` não fazem parte da lista [C]. Movem-se pelo `TransportLineManager` (paralelo). Os `Loader` estão na lista porque têm lógica própria de transferência [I].
- As entidades sem `update` (paredes, canos simples, belts, postes) não ocupam tempo de update [C: ausência na lista + ausência de base `UpdatableEntity`].

## 4. Managers paralelos [C estrutura, I execução]
- `Map::runUpdate : std::function<void(std::initializer_list<MultithreadedUpdatable*>)>` [C].
- Implementações: `createSingleThreadedUpdateRunner` (lambda) e `createMultiThreadedUpdateRunner` (lambda + `ThreadPool`) [C].
- Interface `MultithreadedUpdatable { prepare(); update(); finish(); }`. **Nenhum `XManager::update` tem chamadores diretos** [C, `callers_tick.txt`]: são invocados virtualmente pelo runner.
- O padrão é: `prepare()` sequencial → `update()` em paralelo nos workers → `finish()` sequencial, que aplica os resultados por ordem determinística [C/G]: work-stealing por contador atómico, ações adiadas por thread e merge por índice de grupo em `MultithreadingOutput::dispatchActions` (GHIDRA_FINDINGS §2).
- Dentro de cada manager:
  - `TransportLineManager::update → TransportLine::update` [C]
  - `ElectricNetworkManager::update → ElectricNetwork::update → ElectricSubNetwork::getProductionAndDemand → ElectricNetworkInputValues::computeUsage → ElectricSubNetwork::useProductionAndDemand → statisticsBuffer.apply`, e no fim `mergeAccumulatorGroups` [C]
  - `FluidFlowManager::update → FluidBox::update` [C]

## 5. Uma entidade: `Inserter::update` [C]
`detachInternal` (sai da lista quando inativo) → `Burner::canInsert` (abastecer o próprio burner) → `sqrt` + `Vector::getOrientation` (alvo em coordenadas polares) → `moveHandTo` → `dropHeldItem` → `Burner::update` → `DropTarget::tryFindTarget` → `moveHandTo` → `goToSleep` → `findPickupTarget` → `getPickupTarget` → `moveHandTo` → `tryToGetToSleep` → `moveHandTo` → `dropHeldItem`.

## 6. Uma linha de belt: `TransportLine::update` [C]
`SplitterLogic::resolveSplitting` (×2: splitter à frente e atrás) → `activateDirectInputs` / `hasSideInsertionPosition` / `activateSideInputs` → **`TransportLine::move`** → nova ativação de entradas → `deactivate` (se parada).

## 7. Carregamento de dados (arranque) [R/2.0.60 log + C]
`ModManager` (feature flags) → `Loading mod core/base… (data.lua)` → `(data-updates.lua)` → checksums por mod → `Prototype list checksum` → `Loading sounds` → atlases (`AtlasBuilder`) → menu (`MainMenuSimulationSelector`, que usa o mesmo `Scenario::updateStep`).
