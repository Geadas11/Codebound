# ALGORITHMS — Algoritmos e mecanismos identificados

Evidência: ordem de chamadas diretas lida dos bytes (`scripts/calls.py` → `artifacts/calls_algorithms.txt`, `calls_subsystems.txt`), layouts do PDB e desmontagem manual pontual [C]. A descrição da lógica, quando vai além do que as chamadas e campos mostram, é [I].

## 1. Gerador aleatório: taus88 [C]
`RandomGenerator { u32 seed1, seed2, seed3 }`. `RandomGenerator::getInt` (119 bytes) desmontado à mão:
```
s1: b = ((s1 >> 13) ^ (s1 & 0x7FFC0)) >> 6        ≡ ((s1<<13 ^ s1) >> 19)
    s1 = ((s1 & ~1u) << 12) ^ b                     (and -2 ; shl 12)
s2: b ≡ ((s2<<2 ^ s2) >> 25)   via máscara 0x3F800000, shr 2, shr 23
    s2 = ((s2 & ~7u) << 4) ^ b                      (and -8 ; shl 4)
s3: b ≡ ((s3<<3 ^ s3) >> 11)   via máscara 0x1FFFFF00, shr 3, shr 8
    s3 = ((s3 & ~15u) << 17) ^ b                    (and -16 ; shl 17)
return s1 ^ s2 ^ s3
```
São exatamente as constantes do **taus88** (L'Ecuyer, 1996). `uniformDouble/Float/Integer`, `normal`, `testChance` e `randomElement` derivam de `getInt` [C nomes]. O estado tem 12 bytes, é guardado no save (`save/load`) e há geradores separados por contexto: `LuaRandomGenerator` (exposto aos mods) e `FastRandomGenerator` [C].

## 2. Belts: fila relativa com "gaps" [C chamadas + layout; I semântica]
`TransportLine::move`: `unsquash` → `tryToInsertAtBack` (entradas) → `pop_front` (item sai pela frente) → **`advanceGapIndex`**, **`advanceFirstSquashedIndex`** → `tryToInsert` → `TransportLineConnector::registerInteraction` → `remove`.
- Os itens são uma `CompactDeque<ItemOnTransportLine{distanceToNext}>`. As posições absolutas não são guardadas.
- A frente comprimida (itens à distância mínima `distanceBetweenItems`) anda em bloco quando a saída está livre. Quando bloqueia, o movimento é absorvido pelos gaps a partir de `firstGapIndex`. O custo por tick é proporcional ao número de gaps/eventos e não ao de itens.
- Linhas colineares são fundidas numa única `TransportLine` (`tryToMergeInputLineInside`, `TransportLineMergeRequest`). Um corredor reto de N belts custa como uma só linha.
- Linhas sem movimento possível fazem `deactivate()` e só voltam a ser ativadas por `WakeUpList` ou por `activateDirectInputs/SideInputs` de vizinhos.

## 3. Pathfinding de unidades: A* hierárquico em dois níveis [C]
`PathFinder::update`: `cleanupSuspendedJobs` → `calculateOverloadFactor` → `PathCache::decay` → ciclo `{ exceededWorkLimit? ; getClient ; PathFindAlgorithm::update ; updatePathCache ; PathFindClient::finished }` → `releasePath` → `scheduleDependants` → `scheduleWaitingClients`.
`PathFindAlgorithm::update`: `startAbstractSearch` → **`abstractSearch`** → `startBaseSearch` → `insertBackwardBaseNodesAtRadius` → **`baseSearch`** → `constructPath`.
- **Nível abstrato**: `abstractSearch` faz `pop` numa `IntrusivePairingHeap<AbstractNode, AbstractNodeHeuristicCompare>` e `expandAbstractNode`. Os nós abstratos são **componentes conexos de tiles dentro de cada chunk** (`ChunkComponent`, `findNeighborChunkComponent`, `getTileComponent`, `onChunkTileChanged`) [C].
- **Nível base**: `baseSearch` faz `pop` em `IntrusivePairingHeap<BaseNode, BaseNodeHeuristicCompare>` e `expandBaseNode`. A heurística é a distância **abstrata** (`requestAbstractDistancesToNeighborNodes`) [C] → A* com heurística quase exata [I].
- Pesquisa **para trás** a partir do destino ("backward base nodes at radius") [C].
- Custos: `computeNodeCost`, `computeColliderPenalty`, `computeExtendedTileCollisionPenalty`, `computeContiguous/DiscontiguousBaseExpansionPenalty` [C]. Atravessar entidades destrutíveis é possível com penalização [I].
- **Orçamento por tick** (`exceededWorkLimit`, `calculateOverloadFactor`), **cache de caminhos** (`PathCache` com `decay`) e **suspensão/retoma** de pesquisas (`SearchData::save/load`, `cleanupSuspendedJobs`) [C]. As pesquisas longas espalham-se por vários ticks e são guardadas no save.
- Nós alocados em `MemoryPool<BaseNode,100>` [C].

## 4. Pathfinding de comboios: Dijkstra/A* sobre o grafo de carris [C]
`TrainPathFinder::findPath` (6 834 bytes): `MinHeap<TrainPathFinderNode,double>::heapUp/heapDown` (heap binário com custo `double`), `SimpleMemoryPool<TrainPathFinderNode>`, `Rail::getRailIfOne`, `sqrt` (distância euclidiana → heurística A* [I]), `RailSignalBase::getOutgoingBlock` (penalidades por blocos/sinais [I]), `constructPath` e `TrainPathFinderNoPathReporter`.
Há vários `constructPath`/`clear` → tentativas sucessivas com objetivos diferentes (estação, qualquer paragem com o nome, interrupções) [I].

## 5. Eletricidade: equilíbrio por prioridades [C chamadas; I fórmula]
`ElectricNetwork::update`: por sub-rede `getProductionAndDemand` → `ElectricNetworkInputValues::computeUsage` → `useProductionAndDemand` → `statisticsBuffer.apply`.
- `getProductionAndDemand`: `getAvailableProduction` ×2, `getDemand` ×2, `getLampsDemand`, `updateTertiaryInputValues`, `SolarPanelPrototype::effectivePowerOutput`.
- `useProductionAndDemand`: `useProduction` ×3, `useDemand` ×3, `useLampsDemand`.
- As 9 categorias de `ElectricUsagePriority` (Primary/Secondary input/output, Tertiary, Solar, acumuladores, Lamp, Dynamic) agrupam as fontes em `sourcesByUsagePriority[9]`.
- [I] Algoritmo: (1) somar a produção disponível por categoria (solar → primária → secundária → terciária/acumuladores) e a procura por categoria; (2) calcular a fração satisfeita (`getInputSatisfactionFraction`); (3) distribuir **proporcionalmente** dentro de cada categoria, por ordem de prioridade. O custo é O(categorias), não O(entidades): as entidades leem a fração e o seu buffer depois. Painéis solares são contados em bloco (`SolarCountsType`).

## 6. Fluidos: segmentos + fluxo entre buffers [C chamadas; I modelo]
- `FluidSegmentManager::update` (reorganização: `tryToMergeSegments`, `buildNewSegment`, `markVisitedRecursive`) e `FluidFlowManager::update` → `FluidBox::update` → `FluidBuffer::getFlowRate` → `takeFluid` [C].
- Um `FluidSegment` reúne muitos canos num **buffer único** (`FluidSegment::buffer`). O fluxo só é calculado nas fronteiras: `FluidBox` de máquinas, bombas, filtros (`filteringFluidBoxes`), com `outputBuffers`/`inputBuffers` por caixa.
- Volumes em ponto fixo `int64` com 24 bits fracionários (`FluidBuffer::volume/flow`) e `volumeReservationFraction` (u16, 15 bits) [C].
- `FluidSegment::maxExtent`, `isPipelineOverextended` [C] → os segmentos têm comprimento máximo.

## 7. Logística (robôs) [C chamadas; I lógica]
`LogisticNetwork::update`: `requestFirstAvailableItem` → `dispatchPendingSubset` → `fillRequesters` → `solveProviders`.
Seleção de robô: `findBestRobot<…>`, `findBestBusyRobot` (um robô ocupado que termina perto, ver `…EstimatedToFinishOnChunk`), `findCellClosestTo`, `findStoragePointNextToRobot` [C]. O trabalho por tick é limitado (`orderPerTickLimit`, `lastProcessedActiveRequestIndex`) [C].

## 8. Poluição [C chamadas; I modelo]
`Chunk::updatePollution`: absorção passiva (`Pollution::getChunkPassivePollutionReductionPerSecond`), criação/geração de chunks vizinhos (`Chunk::Chunk`, `MapGenerationManager::request`) → **difusão para vizinhos**, `activateChunkForPollution`, dano a árvores com `RandomGenerator::uniformDouble` + `damageTrees`. Os chunks poluídos estão em `Surface::pollutedChunks[64]` (`pollutedChunksModulo`) → cada chunk é processado 1 vez a cada 64 ticks (processamento em fatias) [I].

## 9. IA de inimigos [C]
`Commander::update`: `UnitGroup::update` → `pruneUnitGroups` → `ExpansionPlanner::shouldExpandNow` → `tryBuildNewBase`. Comportamentos (`AttackAreaBehavior`, `BuildBaseBehavior`, `FleeBehavior`, `GotoBehavior`, `WanderBehavior`…) compõem comandos (`Command`, `Commandable`) [C].

## 10. CRC de sincronização [C]
`Scenario::updateCrcCheck` e `HeuristicCrcGenerator::get` → `crc32_16bytes` (crc32fast, slicing-by-16 [I: nome]) sobre o stream de `MapSerialiser` (`HeuristicCrcGenerator::saveMap`). O polinómio standard IEEE é [I]: não foi verificada a tabela.

## 11. Geração de mapas: compilador de expressões de ruído [C]
`NoiseExpressionParser` → `ParsedNoiseExpression` → `NoiseExpressionCompilationContext` → `NoiseProgramBuilder` → `NoiseProgram`/`CompiledNoiseExpression` com `NoiseRegisterIndex` e `NoiseCache`. Operações: `BasisNoise`, `MultioctaveNoise`, `VariablePersistenceMultioctaveNoise`, `SpotNoise` (+`SpotNoiseCache`), `VoronoiNoise` (jc_voronoi), `DistanceFromNearestPoint`, `Ridge`, `Terrace`, `Clamp`, `If`, `PowInt`, `RandomPenalty`, `GridOperation`, `Multisampling`.
[I] As expressões Lua (strings) são compiladas para um **programa de registos** avaliado em lotes por chunk (`MapGenerationTask`: `BasicTiles` → `TileCorrection` → `Entity` → `Variations`, coerente com `ChunkGeneratedStatus` 10/20/30/40/50).

## 12. Escalonamento temporal (técnicas transversais) [C]
| Técnica | Evidência |
|---|---|
| Sleep/wake por eventos | `WakeUpList`, `Wakeable`, `goToSleep`, `tryToGetToSleep` |
| Atualização agendada para tick futuro | `Map::updateInNTicks`, `performPlannedEntityUpdates`, `AlarmQueue` |
| Processamento em baldes de ticks | `HealingManager::getBucket`, `ItemSpoilQueue::updateBuckets`, `pollutedChunks[64]`, `SCHEDULED_REPATH_TICK_BINS` |
| Fila circular | `Util/CircularUpdateQueue` |
| Ativação por proximidade | `Chunk::shouldBeActive`, `activeWhenSomeoneAround`, `ActiveEntitiesWhenEnemyIsAround` |
| Orçamentos por tick | `PathFinder::exceededWorkLimit`, `LogisticNetwork::orderPerTickLimit` |
| Paralelismo determinístico | `MultithreadedUpdatable` prepare/update/finish + `MultithreadingOutput::finish` |
