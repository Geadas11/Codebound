# LOGISTICS_ANALYSIS — Transportadores, inserters e logística

## 1. Transportadores (belts) [C estruturas; I algoritmo]

### Estrutura
- Cada belt (`TransportBelt`, `UndergroundBelt`, `Splitter`, `Loader`, `LinkedBelt`, todos `TransportBeltConnectable`) tem **duas faixas**. Cada faixa pertence a uma **`TransportLine`**, que pode abranger **muitas belts seguidas**: segmentos colineares são fundidos (`TransportLine::tryToMergeInputLineInside`, `unmergeTransportLine`, `TransportLineMergeRequest`, `TransportLineManager::remergeTransportLineSequence`, `forceMergeOfAllTransportLines`) [C].
- `TransportLineGroup`: conjunto de linhas ligadas que atualizam juntas, com `updateOrder`. Os grupos são ordenados (`UpdateOrderCompare`) [C].
- `TransportLineManager` é um `MultithreadedUpdatable` (`prepare/update/finish`) [C]: **os grupos de belts atualizam em paralelo** [C/G, GHIDRA_FINDINGS §2].

### Representação dos itens [C]
`ItemOnTransportLine { FixedPoint distanceToNext; uint32 uniqueID; ItemStack stack }`. A linha guarda `distanceToFirst`, `distanceAfterLast`, `length`, `speed`, `firstGapIndex`, `firstSquashedIndex` e a constante `distanceBetweenItems`.

### Algoritmo de movimento [I, a partir dos campos e métodos]
```
cada tick, para cada TransportLine ativa:
  avançar = speed
  # a parte da frente (até ao primeiro "gap") está comprimida: move-se em bloco
  se a saída está livre: distanceToFirst -= avançar  (todo o bloco anda com 1 subtração)
  senão: consumir gaps a partir de firstGapIndex (advanceGapIndex), comprimindo itens
         até distanceToNext == distanceBetweenItems  → firstSquashedIndex avança
  itens que ultrapassam o fim: transferTo(próxima linha / splitter / underground)
  se nada se moveu e nada pode mover-se: deactivate() + acordar via WakeUpList quando mudar
```
Custo ≈ O(número de gaps), não O(número de itens) [I]. É a otimização "belt compression" que explica belts cheias quase gratuitas.

- **Inserções laterais**: `inputLines` (até 3), `hasSideInsertionPosition`, `activateSideInputs`, `tryToInsert`, `tryToInsertAtBack` [C].
- **Leitura por circuito**: `BeltReaderStateReference`, `isThisLineDoingBeltRead`, `propagateLineDoingRead`, `sumSignalCounts` [C].
- **Sleep**: `inputWakeUpList`/`outputWakeUpList` acordam inserters/entidades que esperam pela linha [C].

### Splitters [C]
`SplitterLogic { tickOfLastSplitting, inputLocked, outputLocked, takeNextItemFrom, nextItemGoesTo, filter, leftInput, rightInput, leftOutput, rightOutput }` + `SplitterPriority`, `SplitterDirection`, `transitionBufferSize`.
→ alternância esquerda/direita com prioridades e filtro, decidida item a item [I]. O splitter liga as linhas em vez de as possuir (`leadingConnectorSplitter`/`trailingConnectorSplitter` na `TransportLine`).

## 2. Inserters [C estruturas; I comportamento]
- Estado: `heldStack`, `pickupTarget` (entidade ou belt via `BeltItemPickupTarget`), `dropTarget` (`DropTarget`), `headOrientation` (RealOrientation), `handDistance`, `handHeight`, `dropoffVector`/`pickupVector`, `liveStackSize`, `stackSizeOverride`, `filter[5]` + `FilterMode`, `spoilPriority`, `tickOfLastPickup` [C].
- Ciclo [I]: `findPickupTarget` → `moveHandTo` (rotação + extensão a velocidades do protótipo; versões especializadas por tipo de energia `moveHandTo<Burner/Electric/Fluid/Heat/Void>`) → `pickupEntity` / `selectBeltItem` / `transferToHand` → `moveHandTo(drop)` → `transfer` / `dropHeldItem`.
- **Sono**: `shouldSleep`, `tryToGetToSleep`, `goToSleep`, `registerSleeperFromLoad`, `Wakeable<array<WakeableNode,3>>` + `waitingTime` [C]. Um inserter sem nada para pegar ou sem onde largar sai da lista ativa e regista-se nas `WakeUpList` das entidades/linhas de origem e destino [I].
- Leitura de belts: `selectBeltItem`, `updateBeltPickupTarget`, `maxBeltStackSize` (empilhamento em belts, 2.0) [C].
- `Inserter` está numa lista intrusiva própria (`IntrusiveListImplementation<Inserter,InserterListTag>`) [C].

## 3. Loaders, contentores e transferências [C]
- `Loader` (584 B), `LinkedBelt`, `LinkedContainer`, `ProxyContainer`, `InfinityContainer`.
- Transferências genéricas: `TransferSpecification`, `TransferResult`, `TransferAdapter`, `StackTransfers`, `PostTransferNotifiable::postTransferHook`. **Qualquer** movimento de itens notifica o dono do stack (inventário, belt, mão), que acorda dependentes.

## 4. Robôs logísticos e de construção [C estruturas; I comportamento]
- `LogisticNetwork` (856 B): células (`LogisticCell` = área de um roboport), áreas, índices `cellsWithRequests`/`cellsWithFreeRobots` por item, listas de pontos por papel (active provider, passive provider, empty, requester, storage), `activeRequestsByItem`, `supply`, `orderPerTickLimit`, `lastProcessedActiveRequestIndex`.
  → os pedidos são servidos de forma **incremental e limitada por tick** (`orderPerTickLimit`, índice retomado) [I].
- Tipos de trabalho (`Logistics/`): `ItemDeliverJob`, `PickupJob`, `DeliverJob`, `ConstructionJob`, `DeconstructionJob`, `UpgradeJob`, `RepairJob`, `ExplodeJob` (cliffs), `WorkerRobotOrder`, `RobotOrderType` [C].
- Construção: `ConstructionManager`, `DeconstructionOrder`, `MissingConstructionManagerItems`, `MobileNetworkTaskDispatcher` (roboports pessoais), `WorkOrderCancelToken` [C].
- Robôs: `FlyingRobot { FLIGHT_HEIGHT, bobbing, height, double energy, orientation, targetOrientation }`, `RobotWithLogisticInterface`, `RobotMoveIntention`, `RobotChargeState`, `ChargeSpot`, `LogisticCellCharger`, `RobotAscendDescendLogic` [C].
  → voo em **linha reta** com orientação suave; energia consumida por distância; recarga em `ChargeSpot` do roboport mais próximo [I].
- **Estimativa espacial**: `logisticRobotsEstimatedToFinishOnChunk` (`InfiniteRectangle<BusyRobotList>`) [C] → escolhe robôs que vão ficar livres perto do trabalho [I].
- `Triangulation` [C] → [H] triangulação das células para encontrar o caminho de recarga/rota entre roboports.
- Filtros e grupos: `LogisticSection(s)`, `LogisticGroup`, `CompiledLogisticFilter`, `AutoTrasher`, `TrashLogicScheduler` [C].

## 5. Comboios [C estruturas; I comportamento]
- `Train` (432 B): `rollingStocks`, `controller` (`AutomatedTrainController`/`ManualTrainController`), `speed`, `weight`, `brakingForce`, `frictionForce`, `airResistance`, `maxForward/BackwardSpeed`, `schedulable`.
- Carris: `RailSegment`, `RailBlock`, `RailJoint`, `RailSignalBase` → `RailSignal`, `RailChainSignal`, `RailPath`, `RailPlanFinder` (planeador manual) [C].
- Pathfinding: `TrainPathFinder`, `TrainPathFinderGoal`, `TrainPathRequestType`, `TrainPathFinderNoPathReporter`, `RepathData`, `SCHEDULED_REPATH_TICK_BINS` [C] → pesquisa sobre o **grafo de segmentos de carril**, com custos por ocupação/sinal [I]; *repath* periódico em "bins" de ticks.
- Estações: `TrainStopReservation`, `TrainStopLimitLogic`, `WaitStationLogic`, `TrainPriority`, interrupções de horário (`AddTrainInterrupt`…) [C].

## 6. Integração entre sistemas de logística
```mermaid
flowchart LR
  Drill[MiningDrill] -- dropTarget --> Belt[TransportLine]
  Belt -- WakeUpList --> Ins[Inserter]
  Ins -- transfer --> AM[AssemblingMachine]
  AM -- outputWakeUpList --> Ins2[Inserter]
  Ins2 --> Chest[LogisticContainer]
  Chest -- LogisticMember --> LN[LogisticNetwork]
  LN -- ItemDeliverJob --> Robot[LogisticRobot]
  Robot --> Req[Requester chest]
  Req --> Ins3[Inserter] --> Wagon[CargoWagon] --> Train
```
Todas as setas de transferência passam por `ItemStack::transfer` + `postTransferHook`. Isso mantém contadores e acorda entidades adormecidas [I].

## 7. Lacunas
- Valores numéricos (velocidades, `distanceBetweenItems`, `waitingTime`, `orderPerTickLimit`) [N]: são `static const` sem valor no TPI; ler a `.rdata` via Ghidra.
- Ordem exata entre a atualização das belts (thread paralela) e dos inserters (lista de entidades) [N]. Ver EXECUTION_FLOW.md.
