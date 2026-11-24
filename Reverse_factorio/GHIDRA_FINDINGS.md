# GHIDRA_FINDINGS — Resultados da descompilação (2026-11-24)

**Como foi obtido.** Segundo projeto Ghidra 12.1.4 headless (`ghidra2/factorio_fast`), criado a partir do `factorio.exe` original (só leitura):
- o analisador PDB do Ghidra foi **desligado**, porque o primeiro projeto (`ghidra/`) ficou horas preso a alinhar estruturas (`DefaultCompositeMember.removeUnnecessaryPadding`);
- os 266 908 nomes de função vieram do nosso `artifacts/pdb/pdb_proc_ranges.tsv` (`scripts/ghidra/ApplyPdbNames.java`);
- a análise automática completa levou 2 127 s;
- 57 funções foram descompiladas por `scripts/ghidra/DecompileList.java` para **`artifacts/decomp/<nome>@<endereço>.c`**.

**Limites.** O pseudocódigo não tem os tipos do PDB: os campos aparecem como `param_1 + 0x150`. Os nomes dos campos foram resolvidos à mão com `scripts/layout.py` sobre `pdb_layouts.txt`, e as constantes `DAT_…` foram lidas diretamente do `.rdata` do exe. Etiqueta **[C/G]** = confirmado por descompilação Ghidra.

## 1. Ordem real de `Map::updateEntities` (`Map__updateEntities@140c6a3a0.c`) [C/G]
O ramo normal (entidades não pausadas):
1. `AlarmQueue::update`
2. **`runUpdate({transportLineManager})`**: belts (paralelo)
3. `FluidSegmentManager::update` + `assert(turretTargetManager->turretsWantingTarget.empty())`
4. `Surface::update` para cada superfície (pathfinding, IA, poluição, `EntityOrderList`…; ver EXECUTION_FLOW)
5. `Map::performPlannedEntityUpdates`
6. **`runUpdate({turretTargetManager})`**, só se houver torres à espera de alvo
7. `TrainManager::update`, depois `ForceData::update` (cada força), depois `Player::update` (cada jogador)
8. **`runUpdate({controlBehaviorManager})`**, só se não estiver vazio
9. **`runUpdate({electricNetworkManager, heatBufferManager, fluidFlowManager, circuitNetworkManager})`**: os quatro em paralelo, num só lote
10. `Planet::update`, depois `ExtraScriptData::update`, `ItemSpoilQueue::updateBuckets`, `AutoTrasher::execute`, `updateLocalFlyingTexts`, `ScriptRendering::destroyObject` (expirados), `Map::applyStagedChanges`, `EvolutionFactors::update`

Cada `runUpdate` é cronometrado com `steady_clock` e somado a um contador de estatísticas por tipo (offsets 0x40–0x68 do bloco de *timings*) [C/G].

O ramo "entidades pausadas" (modo editor) só corre `MapGenerationManager::update`, `updateBuildEffectSmokes`, `TrainManager::applyStagedChanges`, `RailChainSignal::updateChainSignals`, `ControllerManager::sendEvents`, `ExtraScriptData::update` e `applyStagedChanges` [I, forte: é o bloco depois da verificação de pausa]. A lista "por endereço" de EXECUTION_FLOW misturava os dois ramos.

## 2. Execução paralela determinística [C/G]
- **Runner** (`std::_Func_impl…createMultiThreadedUpdateRunner…_Do_call@1417cab60.c`), por ordem:
  1. para cada manager, `vtable[0]` = `prepare(entityTick, nThreads)`, sequencial;
  2. a flag `Map+0x760` passa a 1 ("dentro de update paralelo");
  3. `ThreadPool::run` N tarefas, uma por índice de thread. Cada tarefa chama `vtable[1]` = `update(threadIndex)` em **todos** os managers do lote;
  4. `ThreadPool::synchronize`, e a flag volta a 0;
  5. `vtable[2]` = `finish()` sequencial, pela ordem do lote.

  `ThreadPool` com o nome "MultiThreadedUpdateRunner" e stack de 0x20000 [C/G].
- **Distribuição do trabalho**: não é estática. `TransportLineManager::update` tira o próximo grupo de um **contador atómico** (`+0x80`, posto a 0 no `prepare`). É *work-stealing*: o grupo que cada thread processa varia de corrida para corrida.
- **Como continua determinístico**: cada thread escreve só no seu `MultithreadingOutput`, ou seja, uma lista de ações adiadas (`std::function`, 64 B). No fim de cada grupo marca a ação com o **índice do grupo** (`MultithreadingOutput::finish(group+0x38)`). O `finish()` chama `MultithreadingOutput::dispatchActions`, que faz um **merge k-way com min-heap pelo índice do grupo**. As ações são aplicadas pela mesma ordem que teriam numa execução com 1 thread, qualquer que seja a thread que fez cada grupo.
- `TransportLineManager::prepare`: aplica as re-fusões pendentes de sequências de belts (`remergeTransportLineSequence`, `TransportLineGroup::extract`) **antes** do paralelo, e redimensiona as saídas para N threads.
- `ElectricNetworkManager::prepare` guarda o tick e limpa uma flag atómica. `finish` só verifica conquistas (`checkAchievements`).

## 3. Eletricidade (`ElectricNetworkInputValues__computeUsage@14033f120.c`, `ElectricSubNetwork__useProductionAndDemand@140346730.c`) [C/G]
O balanço da rede é calculado em **`double`**, não em ponto fixo. Isto corrige DATA_STRUCTURES §1: o `FixedPoint<int64,8>` existe, mas este cálculo não o usa.

Entradas: `solarOutput`, `primaryOutput`, `secondaryOutput`, `tertiaryOutput`, `primaryDemand`, `secondaryDemand`, `tertiaryDemand`.

**Algoritmo (cascata gulosa):**
1. `primaryDemand` é servida por esta ordem: solar → primary → secondary → tertiary.
2. `secondaryDemand` é servida pela mesma ordem, com o que sobrou.
3. `tertiaryDemand` (carga dos acumuladores) é servida por solar → primary → secondary. **Nunca por tertiary**: os acumuladores não se carregam uns aos outros.

Em cada passo, `usado = min(oferta, procura)`. No fim calculam-se razões ∈ [0,1]:
- fração usada de cada fonte: `(cap − sobra)/cap`;
- satisfação de cada procura.

Há uma regra especial para a razão "tertiary" (descarga dos acumuladores) quando a procura é 0. Asserts: `!isnan(output.leftOver)` e `!isnan(demand.leftOver)`.

**Aplicação:** cada entidade recebe `razão × o_seu_valor`.
- Solar: `potência × 1/256 × razão_solar × fator`.
- Acumuladores: `descarga = min(energia, maxSaída) × razão_tertiaryOutput` e `carga = min(cap − energia, maxEntrada) × razão_tertiaryDemand`.

## 4. Fluidos (`FluidBuffer__getFlowRate@140616590.c`, `FluidBuffer__takeFluid@140616730.c`) [C/G]
Quantidades em ponto fixo com **24 bits fracionários**: a constante 2⁻²⁴ = 5,96e-8 está em `.rdata`.

`getFlowRate(origem, destino)`:
1. `fOrig = quant(qOrig/capOrig)` e `fDest = quant(qDest/capDest)`, quantizados a 2⁻²⁴.
2. `d = min(fOrig, 1 − fDest)`.
3. `fluxo = d × (0,8 × min(capOrig, capDest))`, com **mínimo 0x199999 ≈ 0,1** unidades.
4. O fluxo é limitado ao espaço livre no destino e ao disponível na origem, menos uma reserva `minimumAmount × 512 × 2⁻²⁴ × capOrig` (campo u16 em `+0x1a`).

`takeFluid` só transfere se os tipos de fluido forem compatíveis (ID 0 = vazio). `FluidSegmentManager::update` faz avançar uma fase visual `+= 0,2 × fluxo/cap` (módulo 1) e emite alertas a cada 60 ticks.

## 5. Movimento [C/G parcial]
- **Comboio** (`Train__updateSpeed@14104a850.c`; campos `Train`: `speed +0xb8`, `weight`, `brakingForce`, `frictionForce`, `airResistance`, `maxForward/BackwardSpeed`):
  - **Sem travar:**
    1. `v ← v − sinal(v)·min(|v|, frictionForce/weight)`;
    2. `v ← v × max(0,001, 1 − airResistance/(weight/1000))`;
    3. `Train::accelerate`;
    4. clamp a `[−maxBackward, maxForward]`.
  - **A travar com distância d ao destino:** `desac = min(|v|, v²·0,5/d)`, ou seja, a travagem uniforme v²/2d. Sem d, `desac = brakingForce/weight`. `|v| < 0,0001` passa a 0.
- **Robô** (`FlyingRobot__moveTo@140464100.c`): vetor até ao alvo em tiles (posições inteiras × 1/256) → `Vector::setDistanceUpTo(velocidade)` → nova posição inteira, **sem colisões** (`changePositionWithoutCollisionOrAABBCheck`). A orientação só é atualizada se a distância² > 0,001.
- **Carro** (`Car__moveCar@1403a3050.c`): calcula a energia de impacto `(0,5·Δv² + v² + 0,5·ω²)·peso` e a rotação com **sin/cos polinomial próprio** (determinístico, sem `sin` do CRT). O deslocamento é convertido para `FixedPoint<int,8>`, e a colisão na nova posição é testada através da superfície. Se houver colisão, o dano depende da energia. `Car::applyUpdate` só trata a animação.
- **Personagem** (`Character__calculateMovementSpeed@1403d3eb0.c`, `…VectorAndStepCount@1403d40e0.c`):
  - `velocidade = runningSpeed(protótipo) × (1 + bónus da força) × (1 + bónus da personagem) × (1 + exoesqueletos) × (1 − abrandamento) × modificador do tile × stickers`, limitada pelo máximo dos stickers;
  - o passo é partido em **sub-passos** de no máximo `min(lado da caixa de colisão, 16)/256` tiles: `n = ceil(v·|dir| / passo)`;
  - cada sub-passo usa uma das **8 direções** (tabela de vetores unitários) e testa colisões (`calculateShortMovement`).

## 6. Rede UDP (`NetworkMessageHeader__save@140d0dc30.c`, `Heartbeat__saveBase@140cfef50.c`) [C/G]
- **Byte 0:**
  - `tipo` (5 bits, `typeMask`);
  - `0x20` = **bit aleatório** (taus88 local à thread, semeado com `time()`). Não é segurança, só ruído;
  - `0x40` = fragmentado;
  - `0x80` = último fragmento.
- Se a mensagem for fiável ou fragmentada: **u16 messageID** (bit 0x8000 = traz confirmações).
- Se for fragmentada: número do fragmento em `saveSpaceOptimized` (u8, ou 0xFF + u16).
- Se houver confirmações: `vector<u32>` de IDs confirmados.
- **Sem MAC nem cifra no cabeçalho.**
- **Heartbeat**: `u32 sequenceNumber` + nº de `TickClosure` (omitido quando é 1) + cada `TickClosure::save`.

## 7. CRC de desync (`HeuristicCrcGenerator__get@140c5fd20.c`, `__saveMap`, `crc32_16bytes`) [C/G]
- **CRC-32 padrão** (refletido 0xEDB88320; tabela[1] = 0x77073096), versão "slicing-by-16".
- `get()` serializa num `CrcWriteStream` através do `MapSerialiser` normal, que é o mesmo código do save.
- O heurístico guarda, entre outros:
  - contagens: `entity-count`, `active-entities-count`, `active-transport-lines`, `active-transport-line-groups`, `chunks-total`, `polluted-chunks`, unidades segmentadas;
  - contadores `next-unit-number`, `next-train-id`, `next-targetable-item-number`;
  - `random-generator`, `enemy-evolution-factors`, `force-data`, `script-rendering`, `energy-input-running-counts`;
  - **uma fração rotativa** dos jogadores (`tick % ⌈n/10⌉`, com `Player::save` + `Character::save`) e das redes elétricas (`⌈n/5⌉`).

  Ou seja, cada tick verifica contagens globais mais uma amostra rotativa, e não o mapa inteiro.

## 8. zstd [C/G + chamadas diretas]
Chamado só por:
- `Blueprint::serialize`, `BlueprintShelf::loadFromStorage` e `TransferBlueprintData::loadBlueprint` (blueprints);
- `LuaHelpersObject::luaRecvUDP` e `InputActionHandler::udpPacketReceived` (UDP de scripts);
- `RemoteCommandProcessor::update` e `InputActionHandler::serverCommand` (comandos remotos).

**Não é usado no save do mapa**, que é zlib.

## 9. Lua [C/G + tabelas lidas do `.rdata`]
- `luaL_openlibs` abre só: `_G` (base), `table`, `string`, `bit32`, `math`, e `debug` em duas variantes: **partial** = só `getinfo`, `traceback`; **full** quando um argumento é ≠ 0, provavelmente em modo de instrumentação/depuração [I]. **Não há** `io`, `os`, `package` nem `coroutine`.
- **base**: `assert collectgarbage error getmetatable ipairs load loadstring next pairs pcall print rawequal rawlen rawget rawset select setmetatable tonumber tostring type xpcall`. Faltam `dofile`, `loadfile` e `require`: o `require` é fornecido à parte pelo motor.
- **table**: inclui a extensão própria **`pairs_concat`**.
- **string**: inclui **`pack`, `packsize`, `unpack`**, um backport do 5.3.
- **math**: o `math_random` da biblioteca usa `rand()` do CRT. O exe tem a string "Attempting to call math.random() outside an event or during loading…". Isto indica que o motor **substitui** `math.random` no control stage [I, forte].

## 10. Steam e OpenCL [C/G]
- Só existe a classe base `SteamContextInterface` (28 métodos). Por exemplo, `isSteamApiInitialized` = `xor al,al; ret`, ou seja, devolve sempre `false`. Não há implementação concreta nem strings `steam_api`: **nesta build GOG, o Steam é um stub vazio.**
- `OpenCLImpl` é declarado em `src/API/OpenCL.hpp`, mas **não há nenhuma função compilada** com esse nome: é código morto.

## 11. Save (`Map__save@140c68460.c`, `Entity__save@14041af50.c`, `Chunk__save@140ff5b00.c`) [C/G parcial]
- `Map::save` grava por esta ordem: `MapHeader`, `MapModSettings`, `MapGenSettings`, `MapSettings`, `PrototypeMigrationList`, `ForceManager`, `random-generators`, `permission-groups`, `players`, `Planet`/`Surface` (com `Chunk::save`: tiles via `ThreadedChunkTileSaver`, decoratives, entidades, poluição, componentes de pathfinding), `TransportLineSerialiser`, `ElectricNetworkManager`, `FluidSegmentManager`, `heat-buffer-manager`, `CircuitNetworkManager`, `TrainManager`, `HealingManager`, `ItemSpoilQueue`, `ScriptRendering`, `BlueprintLibrary`, `AchievementStats` e contadores `next-*`.
  - As etiquetas de texto (`TagWriter`) só são escritas em modo de diagnóstico [I].
- `Entity::save` (base): `ID<EntityPrototype,u16>`, `MapPosition`, bounding box opcional, `CollisionMask` quando difere da do protótipo, flags em `saveSpaceOptimized`, `Targetable::save` e efeitos ativos. As subclasses acrescentam campos através do `save` virtual. **Payload de cada subclasse: não descompilado [N].**

## 12. Render (`GameRenderer__prepare@1406d7380.c`)
`GameRenderer::prepare` e `DrawEngine::prepare` (`DrawEngine__prepare@1406b25d0.c`) só montam parâmetros de render (escuridão, LUT de cor, relâmpagos, jatos dos propulsores). **Não há interpolação por fração de tick**: nenhuma função nem string do PDB/exe liga `interpolat` ao tick; só existe o `zoomInterpolation` da câmara. O render desenha o último estado de simulação copiado [C/G, evidência por ausência].

## 13. Constantes com nome (`artifacts/pdb/pdb_constants.tsv`, `scripts/constants.py`) [C]
O PDB guarda os valores de **29 751 constantes** (registos S_CONSTANT, com o tipo resolvido; `float`/`double` descodificados). Exemplos:
- `TicksPerSecond = 60`, `Inserter::waitingTime = 20`;
- `FluidBoxConnection::presureToSpeedRatio = 0,4`, `flowToEnergyRatio = 0,59`, `fluidFlowPositionModifier = 0,2`;
- `Burner::minTicksOfEnergyToBuffer = 180`, `ElectricEnergySource::restingEnergyConsumption = 1/30`;
- `AutomatedTrainController::ARRIVE_DISTANCE = 2`, `CurvedRailAPrototype::Radius = 13`;
- `Commander::pursueThresholdDistance = 200`, `DayTime::defaultDusk = 0,25`.

## 14. RenderLayer (`artifacts/render_layers.txt`) [C]
O enum completo está no PDB: 97 nomes ↔ valores 0–255. Exemplos: `TRANSPORT_BELT = 104`, `ITEM = 109`, `OBJECT = 124`, `ELEVATED_OBJECT = 142`, `WIRES_ABOVE = 150`, `CLOUDS = 151`, `CURSOR = 226`. Os nomes Lua (`"object"`, …) são a versão minúscula, construída em runtime em `layerNames` [I].

## 15. VM Lua: alterações ao 5.2 (`luaH_next@141d13e10.c` + layouts do PDB) [C/G]
- **Tabelas com ordem de inserção.** Comparado com o Lua 5.2 oficial:
  - `Table` ganhou `firstadded` e `lastadded`, mais `customflags`;
  - cada `Node` tem **`next`/`prev`** de ordem de inserção (Node = 0x38 B, contra os 0x28 B do oficial);
  - `luaH_next` percorre o array e depois **segue esta lista ligada**, em vez de varrer o vetor de hash.

  Resultado: **`pairs`/`next` dão a mesma ordem em todas as máquinas**, independentemente de endereços ou hashes. É a peça que torna os mods determinísticos em multiplayer.
- Os `GCObject` têm um ponteiro `previous` (lista duplamente ligada), o que permite remoções O(1).
- `luaH_getn` é igual ao oficial (pesquisa binária no array e depois `unbound_search`).
- Diff completo da `lvm.c` (5 668 B de `luaV_execute`): não feito [N].

## 16. Save: cabeçalho e subclasses (`ScenarioExecutionContext__save@140d90460.c`, `MapHeader__save@140c629a0.c`) [C/G]
- **Ordem no `level.dat`:**
  1. `MapVersion` (escrito pelo construtor de `MapSerialiser`);
  2. `ScenarioExecutionContext::save` = `ScenarioLocation`, depois `difficulty` (u8), 6 bools/bytes de estado do cenário, `ApplicationVersion` (u8 major/minor/patch + **u32 build**), `allowed-commands`, `active-mods` (`ModID` = nome, versão, CRC), `startup-mod-settings-crc`, `startup-mod-settings` (PropertyTree) e 2× `EndingInfo`;
  3. `Map::save`, que começa por `MapHeader` = `MapVersion` + **`updateTick`, `entityTick`, `ticksPlayed`** (u64).
- Dos ~20 bytes "não identificados" no save 2.0.60, `01 | 00 00 00 00 00 00 01 | 02 00 3C | 38 46 01 00`:
  - `01` = dificuldade;
  - os 7 bytes seguintes = flags do cenário;
  - `02 00 3C` = versão **2.0.60**;
  - `0x14638` = **build 83512** [I].

  Os últimos 5 bytes também estão resolvidos (`ScenarioExecutionContext` +0x168/+0x16c; `ApplicationVersion::save` = `Version` + u32 build, sem plataforma):
  - `01` = **`allowedCommands` = `True`** (enum: 1 True, 2 False, 3 AdminsOnly);
  - `00 00 A0 00` = **`largeBlueprintSize` = 10 485 760** (10 MiB, u32).

  Os 8 bytes de flags são, por ordem: `difficulty`, `finished`, `playerWon`, `nextLevel` (string vazia = 1 byte 0), `canContinue`, `finishedButContinuing`, `savingReplay` e `allowNonAdminDebugOptions` [C/G para a ordem, I para o mapeamento byte a byte].
- **Subclasses** (amostra):
  - `Inserter::save` = `EntityWithOwner` + `UpdatableEntity` + mão (`ItemStack`), alvos de recolha (entidade ou belt) e de entrega, `InserterControlBehavior`, filtros e `max-belt-stack-size`;
  - `CraftingMachine::save` = efeitos (módulos/beacons), `spoil-ticks`, temperaturas dos ingredientes, `DropTarget`, 2× `WakeUpList`, `FluidBoxManager`, estado da animação e `cached-recipe-production-bonus`;
  - `TransportBeltConnectable::save` = 2× `TransportLineConnector`, direção e ligações (por `unit number`).

  **Todas as 139 classes**: ver §17.

## 17. `save()` de todas as classes de entidade (`artifacts/entity_save_formats.md`) [C/G]
Foram descompiladas as 143 funções `X::save` das 139 classes de `entity_hierarchy.txt` (`artifacts/decomp/entity_save/`). A tabela foi gerada por `scripts/entity_save_summary.py`.
- **131 classes**: formato = `save` do antepassado mais próximo com `save`, seguido dos campos próprios (sub-serializadores pela ordem de chamada, etiquetas e nº de escritas brutas).
- **6 classes nunca são gravadas** (o `save` aborta): `AsteroidChunkEntity`, `FlameThrowerExplosion`, `OldAgriculturalTower` ("should not be saved"), `PlatformCargoChest`, `TrackingStation` ("has been removed") e `FlyingTextEntity` ("deprecated").
- `Entity` é a raiz. `Arrow` partilha o corpo com `Entity::save` (função idêntica dobrada pelo linker).
- **Limite:** a tabela dá a **ordem e o tipo** dos blocos gravados, não o tamanho de cada campo bruto (o pseudocódigo não tem tipos). Para um leitor byte a byte é preciso ler o `.c` de cada classe.

## 18. Lua 5.2: comparação com o oficial (`artifacts/lua_functions.tsv`, `scripts/lua_funcs.py`) [C para o lado Factorio; I para o lado oficial]
O lado oficial vem do conhecimento do código do Lua 5.2.4: **não havia cópia local** da fonte oficial, por isso não é um `diff` de ficheiros. Funções que o compilador fez *inline* podem faltar sem terem sido removidas, por isso as ausências são indicativas; as **funções novas** são evidência forte.

**Módulos.** O `lua.lib` tem 28 módulos e 528 funções. **Não foram compilados:** `loslib` (os), `liolib` (io), `loadlib` (package/require) e `lcorolib` (coroutine).

**Funções novas (não existem no 5.2 oficial):**

| Módulo | Novas | Para quê [I] |
|---|---|---|
| `lmathlib` | `custom_exp`, `custom_log`, `custom_log10`, `custom_pow`, `custom_asin`, `custom_atan`, `custom_satan`, `custom_xatan`, `custom_xcos` | **matemática própria e determinística**: `math.sin/cos/tan` usam `custom_xcos`, `math.exp` usa `custom_exp`, e por aí fora (confirmado pelas chamadas diretas). Não depende do CRT nem do CPU |
| `lapi` | `lua_getlfield`, `lua_setlfield`, `lua_rawgetglobal`, `lua_rawsetglobal`, `lua_tablesize`, `checkstack_locked`; `lua_isnumber`/`lua_isstring` renomeadas para `lua_isnumberorstringconvertabletonumber`/`…tostring` | acesso rápido com comprimento conhecido, contagem de tabela e nomes explícitos para evitar enganos de conversão |
| `lstate` | `lua_traceandabort` | as violações de assert abortam com traceback |
| `ltable` | `luaH_getinthash`, `checknilnode` | suporte à tabela com ordem de inserção (§15) |
| `ltablib` | `tpairsconcat` (`table.pairs_concat`) | |
| `lstrlib` | `str_pack`, `str_packsize`, `str_unpack`, `packint`, `unpackint` | backport do `string.pack` do 5.3 |
| `ldblib` | `luaopen_partialdebug`, `luaopen_fulldebug` (em vez de `luaopen_debug`) | `debug` cortado por omissão |
| `lundump` | `luai_verifycode` | **verificador de bytecode** ao carregar chunks binários (segurança) |

**Removido ou ausente:** `lua_resume`, `lua_yieldk`, `luaV_finishOp` e a maquinaria de `resume`/`unroll`. Isto é consistente com **não haver corrotinas**.

**Estruturas (layouts do PDB vs 5.2.4):**
- `CommonHeader` ganhou **`previous`**: todas as listas de GC são duplamente ligadas.
- `global_State` ganhou **`gcblocked`** (bloquear a GC, por exemplo durante a serialização [I]).
- `lua_State` ganhou **`userData1`, `userData2`** (ligação ao contexto C++).
- `Table` ganhou `customflags`, `firstadded` e `lastadded`; `Node` ganhou `next`/`prev` (§15).
- `Proto`, `CallInfo`, `UpVal`, `TString` e `Udata` são iguais ao oficial, tirando o cabeçalho.

**Erros:** `luaD_rawrunprotected` tem 5 blocos `catch`, ou seja, os erros Lua usam **exceções C++** em vez de `longjmp`.

**Não feito:** a comparação instrução a instrução de `luaV_execute` (5 668 B) e do parser [N].
