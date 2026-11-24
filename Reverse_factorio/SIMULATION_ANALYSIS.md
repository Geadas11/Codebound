# SIMULATION_ANALYSIS — Sistemas de simulação

> Ordem do tick: **EXECUTION_FLOW.md** (canónico). Algoritmos: **ALGORITHMS.md**. Aqui: modelo de simulação, determinismo e escalabilidade.

## 1. Modelo de tempo
- Unidade: `MapTick` (u64) [C]. Três contadores em `Map`: `updateTick`, `entityTick` (pode parar: `EntityUpdatePausedState` [C]) e `ticksPlayed` [C].
- Um tick é a função `Scenario::updateStep`, chamada pela thread de update, pelo benchmark, pelo replay e pelos testes [C].
- A velocidade é ajustável (`GameSpeed` [C]). 60 ticks por segundo é conhecimento público, não verificado no binário [N].

## 2. Determinismo: inventário de mecanismos [C]
| Fonte de não-determinismo | Contramedida |
|---|---|
| floats diferentes entre CPUs/compiladores | ponto fixo (`FixedPoint<int,8>`, `<int64,24>`, `<int64,8>`, `<int64,32>`), `RealOrientation` com `sincos` próprio |
| aleatoriedade | `RandomGenerator` taus88 guardado no save; `LuaRandomGenerator`; `allowMathRandom` |
| ordem de iteração | listas intrusivas, `EntityOrderList` com ordem fixa por tipo, `FlatMap`/`std::map` ordenados, `UnitNumberComparator` |
| threads | `prepare/update/finish` + `MultithreadingOutput::finish` (resultados aplicados sequencialmente) |
| GC Lua | `luaC_forcestep` 1 passo fixo por tick |
| scripts de mods | Lua sem `io/os/package/coroutine`; `require` só durante o parsing de `control.lua` (string `Require can't be used outside of control.lua parsing.`); metatables só registáveis nessa fase (`Metatables can't be registered outside of control.lua parsing.`) |
| deteção | CRC por tick (`updateCrcCheck`) + CRC heurístico + "heavy mode" (`checkHeavyModeCrc`) + `ConsistencyScraper` |

Há também mecanismos explícitos de *não determinismo controlado*. `FluidSegment::getNonDeterministicFluidBoxes` e `getNonDeterministicFilteringFluidBoxes` [C] mostram que algumas estruturas internas não têm ordem estável. O motor isola-as do que afeta o estado [I].

## 3. Escalabilidade: o que torna a simulação barata [I, fundamentado em C]
1. **Atividade**: só `UpdatableEntity` ativas entram no update. Inserters, máquinas, drills, belts e robôs adormecem.
2. **Proximidade**: militares, árvores e unidades ficam em listas por chunk, ativadas só perto de jogadores/inimigos (`Chunk::shouldBeActive`).
3. **Agregação**: belts → `TransportLine` fundidas; canos → `FluidSegment`; eletricidade → somas por categoria de prioridade.
4. **Paralelismo**: 7 managers de grafos em threads.
5. **Amortização**: poluição em 64 fatias, regeneração e apodrecimento em baldes de ticks, pathfinding com orçamento e cache, comboios com repath em bins.
6. **Localidade**: entidades atualizadas agrupadas por tipo, alocações em pools (`MemoryPool`, `ObjectPool`, `TypeAwareMemoryPool`).

## 4. Sistemas simulados (índice)
| Sistema | Onde no tick | Documento |
|---|---|---|
| Entidades ativas (máquinas, inserters, robôs, veículos, unidades…) | `Surface::update → EntityOrderList` | LOGISTICS, PHYSICS |
| Belts | manager paralelo | LOGISTICS §1, ALGORITHMS §2 |
| Eletricidade, calor | managers paralelos | ALGORITHMS §5 |
| Fluidos | `FluidSegmentManager` (sequencial) + `FluidFlowManager` (paralelo) | ALGORITHMS §6 |
| Circuitos | `CircuitNetworkManager`, `ControlBehaviorManager` (paralelos) | SUBSYSTEMS |
| Comboios | `TrainManager::update` | LOGISTICS §5 |
| Pesquisa/força | `ForceData::update` | SUBSYSTEMS |
| Poluição, evolução, expansão | `Surface::update`, `EvolutionFactors::update`, `Commander` | ALGORITHMS §8–9 |
| Geração de mundo | `MapGenerationManager::update` | ALGORITHMS §11 |
| Space Age (planetas, plataformas, apodrecimento, congelamento, relâmpagos) | `Planet::update`, `ItemSpoilQueue`, `FreezingLogic`, `LightningGenerator` | SUBSYSTEMS |

## 5. Crafting (máquinas e jogador) [C]
- `CraftingMachine` (760 B): `sourceInventory`, `resultInventory`, `moduleInventory`, `energySource`, `bonusProduction` (produtividade), `effectReceiver` (módulos/beacons), `recipeID`, `productsFinished`, `sleepReason: ProductionResult`, `inputWakeUpList`/`outputWakeUpList`, `fluidBoxManager`.
  → a máquina dorme com um **motivo** (sem ingredientes, saída cheia, sem energia) e acorda pela lista correspondente [I].
- Jogador: `CraftingQueue`, `CraftingOrder`, `ManualCrafter`, `RecipeTree` (resolução de intermédios) [C].
- Qualidade (2.0): `IDWithQuality`, `QualityPrototype`, `QualityCondition` [C].
