# INDUSTRY_PLAN — Auditoria e plano da simulação industrial (2026-11-24)

Documento de trabalho da missão "Evoluir o Codebound". Regista o estado real encontrado no código, os riscos e o plano por etapas. Atualizar no fim de cada etapa.

## 0. Confirmações (Etapa 0)
- Pasta: `C:\Users\carlos\Desktop\Jogo_CODEFACT`; projeto Unity em `Codebound/` (Unity **6000.6.10f1**, confirmado em `ProjectSettings/ProjectVersion.txt`).
- Git: branch local `main`, que segue **`origin/desktop`**. Estava 1 commit atrás (remoção de ficheiros vazios em `Reverse_factorio/`); avançado por fast-forward. Sem alterações locais.
- Ponto de recuperação: tag **`pre-industry-2026-11-24`** (local) em `3dbc381`.
- Branch `claude/intelligent-bohr-nfkoxs`: outra versão do jogo (sandbox Wasmtime, `Assets/_Codebound/`), sem histórico comum. **Não é integrada**: não há base comum para comparar e a `desktop` já é a versão de referência.
- Testes de base (antes de qualquer alteração): **135/135 EditMode PASS** (`unity test Codebound --mode EditMode`, Editor fechado, 2026-11-24 15:35).

## 1. Contradições na documentação
| Documento | Problema | Ação |
|---|---|---|
| `TECHNICAL_ARCHITECTURE.md` | Descreve só JS e Python; diz que a restrição de rede do JS está pendente (feita na Etapa 3); não fala de PHP, C#, C/C++, SQL, cifra, órbita, música; descreve o tick antigo | Atualizado na Etapa 1 (secção da simulação) e nas seguintes |
| `README.md` / `NEXT_SESSION.md` | Partes antigas (contagens de testes e listas de etapas antigas) | `NEXT_SESSION.md` reescrito no fim de cada etapa |
| Missão (texto do utilizador) | Diz "dez linguagens" e lista oito | Confirmado no código: **JavaScript, TypeScript, Python, SQL, PHP, C#, C, C++** = 8 linguagens de programação; com **HTML e CSS** do HUD programável são as "10 linguagens" do Prompt Mestre |

## 2. Estado real da simulação (lido no código)
**Tick**: `GameBootstrap.Update` acumula `Time.deltaTime` e corre passos fixos de 0,05 s (20 Hz), até 0,5 s de atraso. Em cada passo, no anfitrião: `Factory.Tick` → `Creatures.Tick` → `Ecology.Tick`. Os convidados não simulam (recebem instantâneos 2×/s).

`Factory.Tick(dt)` (`Core/Factory.cs`) faz 8 passagens, **cada uma sobre todos os edifícios**:
1. `TickPower`: reconstrói as redes se algo mudou (procura espacial desde a Etapa 8), zera a satisfação de todos os consumidores e resolve cada rede: solar → carvão → baterias; as baterias só carregam com sol que sobra.
2. `TickActions`: ações temporizadas pedidas pelos programas (minerar 1 s, fundir 3,2 s, fabricar, investigar); a conclusão corre um callback.
3. `PushOutputs`: perfuradoras, fornalhas, montadoras e estufas empurram 1 item por tick para a frente.
4. `TickConveyors`: 1 item por célula, progresso 0..1 a 1,875 células/s; separador e divisor como variantes.
5. `TickInserters`: braço completa o movimento (0,8 s, 1,2 s no longo) e só então tenta transferir 1 item.
6. `TickGreenhouses`, 7. `TickLaunchPads`, 8. `LinkStorageHubs`.

### Problemas encontrados
| # | Problema | Impacto | Etapa |
|---|---|---|---|
| P1 | O **relógio do mundo e os relâmpagos avançam por frame** (`Time.deltaTime` em `GameBootstrap.TickSurvival`), não por tick | Solar, meteorologia, dia/noite e relâmpagos (que destroem edifícios) dependem do frame rate → simulação **não determinística** | 1 |
| P2 | **Ordem de atualização acidental**: cada passagem percorre `BuildGrid.byId` (ordem de inserção do `Dictionary`, com reutilização de posições depois de remoções) | Resultado depende do histórico de construções; não documentado | 1 |
| P3 | Não há **contador de ticks** no estado nem no save | Impossível agendar trabalho em ticks futuros, comparar execuções ou ligar o determinismo ao save | 1 |
| P4 | Sem forma de **comparar estados** entre execuções | Divergências passam despercebidas | 1 |
| P5 | **8 varrimentos completos** por tick + verificação de tipo em cada edifício | Custo linear em todos os edifícios, mesmo inativos | 1 |
| P6 | **Alocações por tick**: listas novas por rede, `string` de estado formatada por gerador/bateria por tick, arrays por correia | Pressão no GC com fábricas grandes | 1 |
| P7 | Correias com **1 item por célula**, sem pistas, progresso discreto | Débito baixo, sem fusões/curvas reais | 2 |
| P8 | Braços transferem **instantaneamente** no fim do movimento; sem mão, sem posição | Sem física de braço | 2 |
| P9 | Eletricidade com 3 fontes fixas, sem categorias de procura/prioridade nem estatísticas | | 4 |
| P10 | Sem fluidos, circuitos, comboios ou robôs | | 3, 5, 6, 7 |
| P11 | `InstanceId` é renumerado ao carregar (o save não guarda ids) | Programas que guardem ids `m12` entre sessões apontam para outra máquina (os programas reiniciam ao carregar, por isso o impacto atual é baixo) | 1 (registar), 8 |
| P12 | As ações temporizadas em curso não são guardadas | Ao carregar perde-se até 3,2 s de uma fundição em curso; os programas recomeçam | 8 |

## 3. O que a investigação do Factorio sugere (e o que se adapta)
Fontes: `Reverse_factorio/EXECUTION_FLOW.md`, `GHIDRA_FINDINGS.md` §1–4, `LOGISTICS_ANALYSIS.md`, `ALGORITHMS.md`, `SIMULATION_ANALYSIS.md` (graus [C] confirmado, [I] inferido).
- **Ordem fixa e explícita** do tick, com entidades **agrupadas por tipo** (`EntityOrderList`) [C]. → Adotado: sistemas com nome, por ordem, e listas por tipo ordenadas por id.
- **prepare (sequencial) → update (paralelo) → finish (sequencial, aplica ações adiadas por ordem de grupo)** [C/G]. → Adotada a estrutura prepare/update/finish; **paralelismo não ativado**: medido, o tick inteiro de 2000 edifícios custa ~1 ms (2 % do orçamento de 50 ms). Threads trariam risco sem ganho medido. A estrutura permite ativá-lo por sistema quando um benchmark o justificar.
- **Sleep/wake** (`WakeUpList`, `goToSleep`) [C para os nomes, I para o mecanismo]. → Etapa 1 com medição; ver §5.
- **Belts como sequências com distâncias relativas** [I forte]. → Etapa 2.
- **Eletricidade em cascata de prioridades em `double`** [C/G]. → Etapa 4.
- Multiplayer do Factorio = lockstep UDP. **Não adotado**: o Codebound usa anfitrião autoritativo com instantâneos cifrados, mais simples e compatível com programas em processos externos (não determinísticos por natureza).

Nada de código, dados ou assets do Factorio é copiado; só ideias de algoritmos, reescritas em C#.

## 4. Princípio de determinismo do Codebound
A simulação é determinística **dadas as mesmas entradas por tick**. Os programas dos jogadores correm em processos externos com tempos imprevisíveis; as suas chamadas são **entradas** aplicadas no tick em que chegam (como as ações do jogador). Por isso o objetivo não é lockstep, e sim:
1. mesma sequência de entradas ⇒ mesmo estado (testável);
2. guardar e carregar a meio ⇒ mesma continuação que sem guardar (testável);
3. o anfitrião é a única fonte de verdade em multiplayer (já é assim).

## 5. Plano por etapas
| Etapa | Conteúdo | Depende de | Ficheiros principais |
|---|---|---|---|
| **1 Fundação** | `Simulation` com sistemas por ordem (prepare/update/finish), contador de ticks no save, relógio e relâmpagos no tick fixo (P1), listas por tipo ordenadas (P2, P5), hash de estado e testes de determinismo (P4), menos alocações (P6), sleep/wake onde medir ganho | — | `Core/Simulation.cs` (novo), `Factory.cs`, `GameState.cs`, `Game/GameBootstrap.cs`, testes |
| **2 Correias e braços** | Correias com 2 pistas e vários itens por segmento (distâncias relativas), curvas, fusões, braços com mão/posição/tempos, mantendo `start/stop/setFilter` e os saves antigos (migração do item único) | 1 (listas, ordem) | `Factory.cs` → `Logistics/*`, `Machines.cs`, `ConveyorItemsView`, API |
| **3 Fluidos** | Tipos, tubos, segmentos com buffer, fluxo, bombas, máquinas com fluidos | 1 | novo `Core/Fluids/*`, pixel art, API |
| **4 Energia** | Categorias de produção/procura, prioridades, acumuladores, estatísticas por rede | 1 (3 para geradores a vapor, opcional) | `Factory.cs` → `Power/*` |
| **5 Sinais** | Cabos, redes de sinais tipados, combinadores, ligação a máquinas, computadores e HUD | 1, 4 | novo `Core/Circuits/*`, API nas 8 linguagens |
| **6 Logística avançada** | Redes logísticas, robôs, pedidos, integração com o Hub | 1, 5 | `Networking.cs` → `Logistics/*` |
| **7 Comboios** | Carris, blocos, sinais, pathfinding, estações, horários | 1, 5 | novo `Core/Rail/*` |
| **8 Integração** | Multiplayer, migrações, desempenho, interface, documentação | todas | |

Ordem mantida: a auditoria confirmou que a Etapa 1 é pré-requisito de todas (ordem, ticks, testes de determinismo) e que as correias (2) são a base física das restantes.

## 6. Registo por etapa
### Etapa 1 — concluída (2026-11-24)
- **Feito**: P1 (relógio/relâmpagos por tick), P2 (ordem por id), P3 (tick no save), P4 (hash + testes), P5 (listas por tipo + sleep/wake), P6 (texto de estado só quando muda; listas reutilizadas). P11 e P12 ficam registados para a Etapa 8.
- **Ficheiros**: `Core/Simulation.cs` (novo), `Core/GameState.cs`, `Core/Factory.cs`, `Core/Buildings.cs`, `Core/Machines.cs`, `Core/Networking.cs`, `Game/GameBootstrap.cs`; testes `DeterminismTests.cs`, `SleepWakeTests.cs` (novos), `PerformanceTests.cs`.
- **Testes**: 142/142 EditMode; build + self-test PASS; Play Mode (171 ticks = 8,55 s).
- **Limitações**: paralelismo não ativado (sem ganho medido); hubs/armadilhas não adormecem quem espera por eles; alocações por tick não medidas (API não suportada no Mono); medições no Editor ruidosas.
- **Retomar**: Etapa 2. As correias atuais (1 item/célula) passam a segmentos com 2 pistas e itens a distâncias relativas; manter a API (`start/stop/setFilter`) e migrar o `bx`/`bi item` dos saves antigos.

### Etapa 2 — concluída (2026-11-24)
- **Feito**: P7 (correias com 2 pistas, 4 itens/pista, distâncias relativas, curvas, alimentação lateral, dois passos por tick independentes da ordem de construção) e P8 (braço com mão, posição e tempos; só apanha o que o destino aceita). API dos programas, saves (com migração do item único), multijogador (instantâneos = save) e vistas mantidos.
- **Ficheiros**: `Core/Logistics.cs` (novo), `Core/Machines.cs`, `Core/Factory.cs`, `Core/Networking.cs`, `Core/Research.cs`, `Core/Shelter.cs`, `Core/Space.cs`, `Game/ConveyorItemsView.cs`; testes `BeltLaneTests.cs` (novo), `FactoryTests.cs`, `SleepWakeTests.cs`, `PerformanceTests.cs`.
- **Testes**: 150/150 EditMode; sleep/wake idêntico com 4000 edifícios × 1200 ticks; build + self-test PASS; Play Mode verificado.
- **Limitações**: fusões de duas correias de frente para a mesma célula usam a regra de curva/lateral (não há "prioridade" configurável); filtros por pista não existem; o limite de desempenho de edição elétrica subiu para 25 ms (reconstrução completa das redes, Etapa 4).
- **Retomar**: Etapa 3 — fluidos.

### Etapa 3 — concluída (2026-11-24)
- **Feito**: fluidos (água, vapor, petróleo) em segmentos de tubos/depósitos; bombas programáveis; bomba de água; bomba de petróleo (consome os depósitos de petróleo que já existiam no mundo); caldeira e máquina a vapor ligadas à rede elétrica (sol → vapor → carvão → baterias); enchedora de bidões (liga o petróleo ao combustível da nave); estufas alimentadas por tubo. API em 8 linguagens, investigação, arte, 10 línguas, saves determinísticos.
- **Ficheiros**: `Core/Fluids.cs` (novo), `Core/Buildings.cs`, `Core/Factory.cs`, `Core/Machines.cs`, `Core/GameState.cs`, `Core/Research.cs`, `Game/ProgramManager.cs`, `Game/MachinePanel.cs`, `Game/PixelArtFactory.cs`, runtimes JS/Python/PHP, `Tools/cshost/Host.cs`, `Tools/localization/tables_fluids.py` (novo) e tabelas geradas; testes `FluidTests.cs` (novo), `DeterminismTests.cs`.
- **Testes**: 158/158 EditMode; runtimes das 8 linguagens PASS; build + self-test PASS; Play Mode verificado.
- **Limitações**: fluxo instantâneo dentro de um segmento (sem pressão nem velocidade por tubo); máquinas ligam-se a tubos, não diretamente umas às outras; sem tubos subterrâneos; tubos atravessáveis pelo jogador (não bloqueiam caminhos); a máquina a vapor não guarda energia (só o vapor que tem).
- **Retomar**: Etapa 4 — energia com prioridades, estatísticas por rede e reconstrução incremental das redes (baixar o limite de 25 ms).

### Etapa 4 — concluída (2026-11-24)
- **Feito**: P9 (prioridades de procura, categorias de produção, estatísticas por rede) e a reconstrução incremental pendente desde a Etapa 2; hubs deixam de procurar baús sem motivo.
- **Ficheiros**: `Core/Power.cs` (novo), `Core/Factory.cs`, `Game/ProgramManager.cs`, `Game/MachinePanel.cs`, runtimes JS/Python/PHP/SQL, `Tools/cshost/Host.cs`, `Tools/localization/tables_power.py` (novo) e tabelas; testes `PowerTests.cs` (novo), `PerformanceTests.cs`.
- **Testes**: 164/164 EditMode; runtimes das 8 linguagens PASS; build + self-test PASS; Play Mode verificado.
- **Limitações**: prioridades fixas por tipo de máquina (os jogadores ainda não as mudam); estatísticas só do último tick (sem histórico/gráficos); os acumuladores continuam a ser as baterias existentes; tirar um nó no meio de uma rede muito grande ainda percorre essa rede (~3 ms com 2000 edifícios).
- **Retomar**: Etapa 5 — sinais.
