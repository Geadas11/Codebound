# DATA_STRUCTURES — Estruturas e organização dos dados

Todos os layouts abaixo foram extraídos do TPI do PDB (`artifacts/pdb/pdb_layouts.txt`, 19 244 tipos com membros) [C].
`+N` = offset em bytes. Os tamanhos são os da build x64 2.1.24. Os nomes de membros são os originais do código. As interpretações de uso são [I].

## 1. Tipos numéricos e geométricos

| Tipo | Tamanho | Layout | Significado |
|---|---:|---|---|
| `FixedPointNumberTemplate<int,8,0>` | 4 | `int32` com 8 bits fracionários | **1/256 de tile**; base de todas as posições e distâncias de simulação |
| `MapPosition` | 8 | `x,y : FixedPoint<int,8>` | posição contínua no mapa (±2^23 tiles) |
| `TilePosition` / `ChunkPosition` | 8 | `int32 x,y` | coordenadas inteiras de tile / chunk |
| `BoundingBox` | 20 | `MapPosition leftTop, rightBottom; VectorOrientation orientation` | caixa de colisão, rotacionável |
| `RealOrientation` | 4 | `uint32 value` (+ `ValueBits`) | orientação como fração de volta inteira, determinística (sin/cos próprios: `RealOrientation::sincos`) |
| `MapTick` | 8 | `uint64 value` | número do tick (60 por segundo de jogo [conhecimento público]) |
| `FixedPointNumberTemplate<__int64,24,1>` | 8 | int64 com 24 bits fracionários, com sinal | volume e fluxo de **fluidos** (`FluidBuffer::volume/flow`) |
| `FixedPointNumberTemplate<__int64,8,0>` | 8 | int64 com 8 bits fracionários | energia em buffers/estatísticas; **o balanço da rede elétrica é calculado em `double`** [C/G, GHIDRA_FINDINGS §3] |
| `FixedPointNumberTemplate<__int64,32,0>` | 8 | int64, 32 bits fracionários | emissões de poluição por chunk |
| `RandomGenerator` | 12 | `uint32 seed1, seed2, seed3` | **taus88** (gerador combinado de Tausworthe, L'Ecuyer 1996) [C]. A prova está na desmontagem manual de `RandomGenerator::getInt` (RVA via `pdb_proc_ranges.tsv`, 119 B): máscaras `and -2/-8/-16`, shifts 12/4/17, formas combinadas dos shifts 13/19, 2/25 e 3/11, e `return s1^s2^s3`. Detalhe em ALGORITHMS §1 |

**Conclusão [I, forte]:** a simulação evita floats nas grandezas que precisam de reprodutibilidade (posição, fluido, energia, tick). Há `float`/`double` em estado de simulação (ex.: `EntityWithHealth::health float`, `Car::speed double`, `Train::speed double`). O determinismo depende então de as operações IEEE serem reprodutíveis com a mesma build (`-Brepro`, sem `-fp:fast` nas amostras lidas) [I].

## 2. Mundo

```
Map (1 896 B)
 ├─ updateTick, entityTick, ticksPlayed : MapTick
 ├─ runUpdate : std::function<void(initializer_list<MultithreadedUpdatable*>)>   ← executor single/multi-thread
 ├─ electricNetworkManager, fluidSegmentManager, fluidFlowManager, heatBufferManager,
 │  transportLineManager, controlBehaviorManager, circuitNetworkManager,
 │  turretTargetManager, healingManager, trainManager                 (unique_ptr)
 ├─ players, connectedPlayers : vector<Player*>
 ├─ nextUnitNumber (u64), nextTargetableItemNumber, nextLogisticNetworkID, nextSpacePlatformIndex …
 ├─ nextUpdateEntityIterator, activeEntitiesCount, entityCount, toBeDeleted
 └─ (surfaces, forces… depois de +552, não listados aqui)

Surface (17 544 B)  : Targetable, InfiniteRectangle<Chunk*>
 ├─ index, planet*, platform*, name
 ├─ compiledMapGenSettings, mapGenerator, mapGenerationManager
 ├─ activeChunks : IntrusiveList<Chunk, ActiveChunksListTag>
 ├─ activeEntitiesList
 ├─ pollutedChunks : array<IntrusiveList<Chunk>,64>   (pollutedChunksModulo → poluição em 64 "fatias" de ticks [I])
 ├─ commanders, pathFinders
 ├─ dayTime, wind (≈9 KB), surfaceProperties, spawners, activeTriggers, segmentedUnits, freezingLogic

Chunk (5 568 B)   32×32 tiles
 ├─ tiles : ChunkTiles (+40, 3 072 B → 3 B/tile [I]) ; advancedTiles (+3112, 2 048 B)
 ├─ chunkIndex, generatedStatus (Nothing0/CustomTiles10/BasicTiles20/CorrectedTiles30/Tiles40/Entities50)
 ├─ decoratives, trivialSmokes
 ├─ activeWhenSomeoneAround, activeWhenPollutionExists : ActiveEntities (listas intrusivas de UpdatableEntity)
 ├─ activeWhenEnemyIsAround : vector<…>
 ├─ pollution, tree/other emissions (FixedPoint<int64,32>), emissionsPerSecond
 ├─ playerBuildingCount, unitSpawnerCount, treesOnChunkCount, resourcesOnChunk…
 └─ militaryTargets, activeNeighbourForces
   constantes: MAGIC_CHUNK_BEGIN, MAGIC_TILES_END, MAGIC_ENTITIES_END (marcadores no save)
```

## 3. Entidades

Hierarquia completa (142 classes) em `artifacts/entity_hierarchy.txt` [C]. Núcleo:

```
Targetable(24) ─┐
PostTransferNotifiable(8)┐
                Entity (128 B)
                 +32 EntityConnector connector
                 +72 const EntityPrototype* prototype
                 +80 MapPosition position
                 +88 BoundingBox boundingBox
                +108 CollisionMaskIndex collisionMask   (u16 → tabela global de máscaras)
                +110 EntityUsageBitMask usageBitMask
                +112 Surface* surface
                +120 Map& map
   └ EntityWithHealth (144) : float health, bitmask, upgradeTargetID, damageToBeTaken
       └ EntityWithOwner (168) : força, último utilizador…
           ├ Inserter (696), CraftingMachine (760) → AssemblingMachine (768) → RocketSilo (1 264)
           ├ TransportBeltConnectable (296) → TransportBelt (312), UndergroundBelt (392), Splitter (760), Loader (584)
           ├ MiningDrill (1 448), Boiler (816), Generator (544), Reactor (392), Lab (608)…
           ├ Vehicle (328) → Car (944), RollingStock (768) → Locomotive/CargoWagon/FluidWagon/ArtilleryWagon, SpiderVehicle
           ├ FlyingRobot (248) → RobotWithLogisticInterface (880) → LogisticRobot (1 016), ConstructionRobot (1 152)
           └ Character (1 912), Unit (672), Turret (512)…
```
Componentes partilhados por herança múltipla:
- `UpdatableEntity` (40 B): `Freezable`, hook de lista intrusiva, `Chunk* chunk`, `flags` (ACTIVE / DEACTIVATED_BY_{CIRCUIT_NETWORK,SCRIPT,FREEZING,TO_DECONSTRUCT,RECIPE}), `UpdatableActiveMode mode` [C]. **Só as entidades que herdam isto têm `update()` por tick** [I].
- `Wakeable<array<WakeableNode,3>>` + `Notifiable`: inscrição em `WakeUpList`s de vizinhos (sleep/wake).
- `EnergySourceHolderTemplate<EnergySource>`: ponteiro polimórfico para Burner/Electric/Fluid/Heat/Void energy source.

### `Inserter` (696 B) [C]
`heldStack:ItemStack(+328)`, `pickupTargetRail`, `pickupTarget:NotifiableTargeter<Entity>`, `beltItemPickupTarget`, `flags`, `dropTarget:DropTarget(+448)`, `direction`, `stackSizeOverride`, `liveStackSize`, `maxBeltStackSize`, `spoilPriority`, `tickOfLastPickup`, `cached:ValuesFromPrototype`, `headOrientation:RealOrientation(+576)`, `handHeight:float`, `handDistance:double`, `energySource`, `controlBehavior`, `filter[5]`, `dropoffVector/pickupVector:Vector`, `dropoffHandTarget/pickupHandTarget`.
→ a mão é modelada em **coordenadas polares** (orientação + distância + altura), não por posição cartesiana [I].

### `ItemStack` (24 B) [C]
`uint32 count; ID<ItemPrototype,u16> itemID; ID<QualityPrototype,u8> qualityID; u8 stackSize; Item* item; PostTransferNotifiable* owner`
→ itens simples ocupam 8 bytes úteis. Itens com dados (blueprints, armaduras, itens que se estragam) usam `Item* item` alocado [I].

## 4. Belts [C]

```
TransportLine (320 B)              uma faixa de uma sequência de belts
 +48  TransportLineGroup* group
 +56  MapTick lastUpdateTick
 +64  FixedPoint distanceToFirst      distância da frente da linha ao 1.º item
 +68  FixedPoint distanceAfterLast
 +72  FixedPoint speed
 +76  uint32 firstGapIndex, +80 firstSquashedIndex
 +84  FixedPoint length
 +88  CompactDeque<ItemOnTransportLine> items
 +112 TransportLine* directConnection / +120 directInput, +232 SmallVector<TransportLine*,3> inputLines (laterais)
 +144 SplitterLogic* leading/trailingConnectorSplitter
 +168 WakeUpList inputWakeUpList, +200 outputWakeUpList
 static distanceBetweenItems (FixedPoint), distanceFromCenter (float)

ItemOnTransportLine (32 B) = { FixedPoint distanceToNext; uint32 uniqueID; ItemStack stack }
TransportLineGroup (104 B) = { updateContext, manager, transportLines, updateOrder, dirty, activeTransportLines, nextUpdateTransportLineIterator }
SplitterLogic (64 B) = { tickOfLastSplitting, inputLocked, outputLocked, takeNextItemFrom, nextItemGoesTo, filter, leftInput, rightInput, leftOutput, rightOutput }
```
**Representação relativa**: cada item guarda a distância **ao seguinte**, e não a sua posição absoluta. Mover uma fila compacta exige só mudar `distanceToFirst`. `firstGapIndex`/`firstSquashedIndex` marcam onde começa a parte comprimida [I, forte, ver ALGORITHMS §2].

## 5. Fluidos, energia e calor [C]
```
FluidBuffer (40) = { Fluid fluid; FixedPoint<i64,24,1> volume; ID filter; FixedPoint<u16,15> volumeReservationFraction; flags; FixedPoint<i64,24,1> flow }
FluidBox (264)   = { Entity& owner; FluidBuffer buffer; SmallVector<FluidBuffer*,4> outputBuffers, inputBuffers; prototype; flags;
                     FluidSegment* fluidSegment; indexInSegment; flowUpdateIndex; SmallVector<FluidBoxConnection,4> connections }
FluidSegment (200) = { id; double flowPosition; manager; FluidBuffer buffer; lastSeenFluid; boundingBox; maxExtent;
                       vector<FluidBox*> fluidBoxes; set<u32> filteringFluidBoxes; … }
ElectricNetwork (360) = { subNetworkList; type; inputLastTick; usageLastTick; equipmentGrid; statistics; connectedConsumers/Producers }
ElectricSubNetwork (464) = { parentNetwork; connectedEntities…; array<EnergySourceMapping,9> sourcesByUsagePriority;
                             accumulatorGroups; neighbours; allNodesInAnyOrder }
ElectricEnergySource (64) = { EnergySource; ElectricSubNetwork* network; EnergyStorage buffer; additionalNetworks;
                              ElectricUsagePriority priorityIndex; u16 typeIndex; u32 index }
ElectricUsagePriority = PrimaryInput0, SecondaryInput1, PrimaryOutput2, SecondaryOutput3, Tertiary4, Solar5,
                        AccumulatorsManagedByTheAccumulatorGroup6, Lamp7, Dynamic8
```
- Fluidos: um **segmento** agrega muitas `FluidBox` (canos) com **um único buffer partilhado**. O modelo é por segmentos e não cano a cano [I, forte].
- Eletricidade: as fontes estão agrupadas **por prioridade** (9 categorias) dentro de cada sub-rede, e a resolução é por categoria, não por entidade [I].

## 6. Logística [C]
`LogisticNetwork` (856 B): `cells` (roboports), áreas de logística/construção, `cellsWithRequests`, `cellsWithFreeRobots`, listas de robôs ocupados por tipo, listas de pontos por papel (`activeProviderPoints`, `passiveProviderPoints`, `emptyProviderPoints`, `requesterPoints`, `storagePoints`), `activeRequestsByItem`, `supply`, `orderPerTickLimit`, `logistic/constructionRobotsEstimatedToFinishOnChunk`.
`LogisticCell` (256 B): `neighbors`, `edges`, `network`, `owner`, `charger`, contadores de robôs estacionados, `toStation`, `robotSupply`.

## 7. Rede e ações [C]
`InputAction` 128 B (`MapTick updateTick; InputActionType type(u16); u16 playerIndex; char buffer[112]`: união das payloads); `TickClosure` 56 B; `Heartbeat` 40 B. Ver NETWORKING_ANALYSIS.md.

## 8. Protótipos e IDs [C]
- `ID<XPrototype, uint16>` para itens, entidades, fluidos, receitas, tiles; `ID<QualityPrototype, uint8>`.
- `IDWithQuality<ID<…>>` = ID + qualidade, usado como chave de estatísticas, filtros e pedidos.
- `CollisionMaskIndex` (u16) aponta para `static vector<CollisionMask> indexToCollisionMask`: as máscaras são **internadas**, e cada entidade guarda só o índice [C].
- `CollisionMask` (40 B) = `CollisionLayers layers` (32 B = bitset de 256 camadas) + `flags` [C]. `data/base/prototypes/collision-layers.lua` define 24 camadas [C].

## 9. Referências entre objetos [C]
- `Targetable` (24 B) é base de `Entity`, `ElectricNetwork`, `LogisticNetwork`, `Surface`, `Train`…
- `Targeter<T,…>` / `NotifiableTargeter<T>` são ponteiros que o alvo anula ao ser destruído (lista de quem o referencia) [I]. Garantem segurança sem *shared_ptr* e serializam-se via `TargetSerialiser`.
