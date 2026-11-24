# Codebound — Arquitetura Técnica

## Stack
- Unity **6000.6.10f1** (Unity 6), template Universal 2D (URP 2D Renderer), Input System (novo, `activeInputHandler: 1`).
- C# (scripts do jogo). Projeto Unity em `Codebound/`; documentação na raiz.

## Assemblies (`Codebound/Assets/Codebound/Scripts/`)
| Assembly | Pasta | Depende de | Papel |
|---|---|---|---|
| `Codebound.Core` | `Core/` | **nada** (`noEngineReferences`) | Regras puras: itens, inventário, grelha de construção, definições de estruturas, geração do mundo, estado do mundo. Testável sem cena. |
| `Codebound.Game` | `Game/` | Core, Unity.InputSystem | MonoBehaviours: bootstrap/tick/save, jogador, câmara, chunks, construção, painéis de máquinas, editor de código, `ProgramManager` (ponte para o runtime). |
| `Codebound.Tests.EditMode` | `Tests/EditMode/` | Core, NUnit | Testes unitários. |

Regra: lógica de jogo vai para Core sempre que não precisar de Unity. Game só traduz input/visual ↔ Core.

Futuras assemblies (planeadas): `Codebound.Scripting` (API + host de execução), `Codebound.Net` (multiplayer), `Codebound.Save`.

## Arranque
`GameBootstrap` usa `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` e constrói o jogo em código na cena carregada (câmara existente + Global Light 2D do template). Evita editar YAML de cenas à mão e mantém tudo versionável em C#.

## Mundo
- `WorldGenerator(seed)`: ruído de valor determinístico próprio (hash inteiro) → bioma, terreno, jazidas. Mesma seed ⇒ mesmo mundo (testado).
- `WorldState`: deltas sobre o mundo gerado (jazidas esgotadas/consumidas). É o que será guardado no save — o mundo base nunca é guardado.
- `ChunkRenderer`: 2 `Tilemap` (chão, recursos) + chunks 32×32 carregados/descarregados à volta da câmara com `SetTilesBlock`.
- Sprites pixel art gerados proceduralmente em código (`PixelArtFactory`) até existir arte final (Etapa 8).

## Construção
- `BuildGrid` (Core): ocupação célula→estrutura, footprints multi-célula com rotação 0/90/180/270 (w/h trocam em 90/270), verificação de obstáculos via predicado de terreno.
- `BuildController` (Game): pré-visualização verde/vermelha, custos, colocação, remoção com reembolso.

## Simulação (Core) — tick fixo, ordem explícita (desde a Etapa 1 da simulação industrial, 2026-11-24)
- `GameState` agrega `WorldState`, `BuildGrid`, `Factory`, inventário e posição do jogador — é a unidade que se guarda.
- **Um passo = `GameState.Step()`**, 20 por segundo (`Simulation.TickSeconds` = 0,05 s), chamado por `GameBootstrap` só no anfitrião / jogo a solo. `GameState.Tick` conta os passos e vai no save (`tick N`; saves antigos começam em 0).
- `Simulation` corre **sistemas com nome por esta ordem** (testado em `DeterminismTests.TickOrder_IsExplicit`):
  1. `clock & weather` — o relógio avança 0,05 s **por tick** (antes avançava por frame: solar, meteorologia e relâmpagos dependiam do frame rate); luz do dia e fator solar para a fábrica;
  2. `power` — prepare: reconstrói as redes se algo mudou e zera a satisfação; update: balanço de cada rede (solar → carvão → baterias);
  3. `machine actions` — update: progresso com energia/combustível; **finish**: conclusão pela ordem em que começaram (callbacks e eventos dos programas);
  4. `outputs`, 5. `belts`, 6. `inserters`, 7. `greenhouses`, 8. `launch pads`, 9. `storage hubs`;
  10. `creatures`, 11. `ecology`, 12. `lightning` (dano ao jogador acumulado em `PendingPlayerDamage`, aplicado pelos vitais por frame).
- Cada sistema tem **prepare → update → finish**. Só o finish aplica resultados adiados por ordem fixa, o que permitiria correr o update de um sistema em várias threads sem mudar resultados. **Não está ativado**: medido, 2000 edifícios custam ~0,5 ms do tick de 50 ms (8000 ≈ 2–4 ms). `Simulation.Profile = true` mede cada sistema (`LastMs`, `AverageMs`).
- **Ordem das entidades**: `BuildGrid.Buildings` está sempre por id (= ordem de construção; antes era a ordem de um `Dictionary`, que muda depois de remoções). A `Factory` guarda listas por tipo (consumidores, nós elétricos, produtores, correias, braços, estufas, plataformas, hubs), também por id: cada sistema percorre só as suas entidades.
- **Sleep/wake**: uma correia bloqueada ou um braço sem nada para fazer dorme e regista-se nas células de que depende. Acorda quando um inventário do edifício nessas células muda, quando o filtro ou o liga/desliga muda (`MachineState.Touched`), quando um edifício aparece/desaparece lá, ou quando o seu próprio conteúdo muda. Quem dorme continua na lista, por isso acordar a meio do tick corre na ordem habitual: **resultado idêntico a tentar em cada tick** (`SleepWakeTests` compara os dois modos com `Factory.SleepEnabled`). Hubs e armadilhas não são "observáveis" (dependem de energia/cargas): quem espera por eles tenta em cada tick. Ganho medido com 8000 edifícios: correias 0,84 → 0,14 ms, braços 0,24 → 0,13 ms, tick −22 %.
- **Determinismo**: mesmas entradas por tick ⇒ mesmo estado. `Simulation.StateHash` (FNV-1a do save canónico) compara mundos; testado entre execuções e entre "guardar e carregar a meio" e "não guardar". As chamadas dos programas (processos externos) são entradas aplicadas no tick em que chegam; o multiplayer não é lockstep: o anfitrião é a fonte de verdade.
- `Factory.Tick(dt)` continua a existir (testes), com os mesmos passos.

## Correias e braços (`Core/Logistics.cs`, Etapa 2 da simulação industrial)
- **Correia = 2 pistas** (0 = esquerda, 1 = direita, no sentido da marcha), cada uma com até 4 itens (`BeltItem { Id, Pos }`, 0 = traseira, 1 = frente) a pelo menos `Gap` = 0,25 de distância, à mesma velocidade de antes (1,875 células/s). Débito: até 15 itens/s por correia (antes ~1,9).
- **Dois passos por tick**, para o resultado não depender de que correia foi construída primeiro: (1) cada correia avança os seus itens (o da frente pode ficar um pouco além da borda); (2) cada correia passa os itens da frente para o que está à frente. O item entra na correia seguinte já avançado do que sobrou do passo, por isso uma linha de correias anda como uma só.
- **Entrada na correia seguinte**: mesma direção → mesma pista, atrás; de frente → bloqueado; perpendicular com uma correia a alimentar a de destino por trás → **alimentação lateral** para a pista do lado de onde vem, a meio; perpendicular sem essa correia → **curva**, mesma pista. Máquinas, braços e jogador põem itens a meio, pista direita primeiro. Separador e divisor mantêm as regras (escolhem a saída pelo item da frente).
- **Compatibilidade**: `ConveyorState.Contents` é um inventário-espelho das pistas (papel `item`): `contents()` dos programas, SQL, painel, braços e reembolsos continuam iguais. Tirar de lá tira o item mais à frente; pôr lá põe-no atrás. `Item`/`Progress` = item mais à frente. Save: `bx lanes id@pos,…|id@pos,…`; saves antigos (`bi item` + `bx prog`) carregam como 1 item na posição antiga.
- **Braço com mão**: `InserterState.Hand` (inventário de 1, papel `hand`: guardado, mostrado e devolvido ao desmontar) e `Arm` (0 = a apanhar, 1 = a largar; `bx arm`). Meia volta = `SwingSeconds/2` (×1,5 no longo): o débito continua 1 item / 0,8 s. Só apanha o que o destino aceita (`MachineState.AcceptsItem`); se o destino estiver cheio, espera com o item na mão.
- **Sleep/wake**: uma correia só dorme se nenhum item se puder mexer (`Packed`) e a frente estiver bloqueada; regista-se na saída e na célula atrás dela (aparecer lá uma correia transforma uma curva em alimentação lateral). Uma correia cujos itens avançam acorda quem espera por ela (o espaço atrás abriu sem mudar o conteúdo). Verificado: 4000 edifícios, 1200 ticks, dormir ligado e desligado dão o mesmo estado (tick 5,9 → 5,0 ms).
- Energia: nós = geradores e postes (ligam-se a ≤ 9 células); consumidores ligam-se a um nó (poste 4,5 / gerador 3 células + metade do tamanho). Satisfação = capacidade/procura. Gerador a carvão 150 kW, 1 carvão = 4000 kJ.
- Ações temporizadas pedidas por programas: `StartMine` (1 s, 90 kW), `StartSmelt` (3,2 s, queima carvão). Devolvem código de erro imediato (`no_power`, `no_fuel`, `no_input`, `output_full`, `depleted`, `busy`) ou terminam com callback.
- Saída: perfuradora/fornalha empurram 1 item/tick para o edifício na **frente** (R0 = cima, rotação horária).
- Braços: `InserterState` (0,8 s por item) movem de `Extractable` (trás) para `TryInsert` (frente).
- Transportadores: `ConveyorState` (1 item, progresso 0..1, 1,875 células/s); ao chegar a 1 tenta `TryInsert` no edifício da frente; `ArrivedThisTick` impede que um item atravesse várias células num tick.
- Save: `SaveSystem` formato de texto versionado (`CODEBOUND 1`), só deltas sobre a seed; escrita atómica (tmp + `File.Replace`, mantém `.bak`); leitura valida e indica a linha com erro.

## Programação real — implementado (JavaScript)
```
Unity (ProgramManager, thread principal)
  ⇅ JSON por linha em stdin/stdout
node host.js  (processo separado, Node permission model)
  └─ Worker por programa (heap limitado = RAM do computador, terminate() a qualquer momento)
       └─ vm context: só a API Codebound (prelude), sem require/process/eval
```
- Ficheiros: `Assets/StreamingAssets/Runtime/js/{host.js,worker.js}` (copiados para builds).
- Node: usa `StreamingAssets/Runtime/node/node(.exe)` se existir (builds), senão `node` do PATH (Editor). `BuildScript` copia o binário para o build (Windows feito; Linux/macOS precisam do binário certo via `CODEBOUND_NODE`). O `LICENSE` do Node tem de acompanhar o binário em qualquer distribuição (`CODEBOUND_NODE_LICENSE`).
- Validação de builds: `Codebound.exe -selftest -logFile x.log` → `[SELFTEST] PASS` e código de saída 0.
- Chamadas bloqueantes: o worker faz `postMessage` + `Atomics.wait` num `SharedArrayBuffer`; o Unity responde quando a ação termina no tempo de jogo. Logo `while (true) drill.mine();` funciona e cada chamada custa tempo de jogo.
- CPU: o computador executa no máximo `CallsPerSecond` chamadas (Basic = 20/s); sem energia as chamadas ficam em espera. Watchdog: 3 s sem chamar a API/`sleep` ⇒ programa terminado (loop infinito). Estatística CPU%/heap enviada 1×/s.
- RAM: `resourceLimits.maxOldGenerationSizeMb` = RAM do computador (Basic = 32 MB) ⇒ erro "Out of memory".
- Eventos: `computer.on(nome, fn)`; o Unity anexa eventos às respostas; quando o corpo principal termina com handlers registados, o worker fica em `wait_event`. Contínuo e por eventos coexistem. Eventos: `tick`, `item_mined`, `item_smelted`, `power_lost`, `power_restored`.
- Validação no jogo para **todas** as chamadas: alcance (8 células), energia, regras da `Factory`, máquina existente.
- Camadas de isolamento (testadas em `Tests/runtime/host.test.js`): processo separado; `--permission` (sem escrita em disco, leitura só da pasta do runtime, sem processos filhos, sem addons); `--disallow-code-generation-from-strings` (bloqueia fugas via `Function` constructor) + `codeGeneration:{strings:false}` no contexto; worker com limites; só funções-ponte que recebem/devolvem strings.
- **Lacuna conhecida:** o permission model do Node 24 não restringe rede. O código do jogador não tem acesso a `require`/`process`, por isso não consegue abrir sockets, mas a defesa em profundidade para rede fica pendente (Node ≥ 25 `--allow-net`, ou mover para QuickJS/Wasm). Relevante antes do multiplayer (correr código de outros jogadores).
- Preparação do código (`prepare.js`, no processo host): verificação de sintaxe; TypeScript → JS (`vendor/typescript`, Apache-2.0) com mapa de linhas (source map); instrumentação de debug.
- Debugger: `__dbg(linha, () => ({vars visíveis}))` inserido em linha antes de cada instrução (nomes visíveis = declarações dos escopos envolventes; cada leitura em `try` por causa da TDZ). Em pausa, o worker faz a chamada bloqueante `debug.pause` com um snapshot (profundidade 2, 20 entradas) e o Unity responde `continue`/`step`. Só é instrumentado quando o jogador carrega em **Debug**.
## Programação real — Python
```
Unity (ProgramManager) ⇄ JSON por linha ⇄ python -I -S -X utf8 host.py   (1 processo por programa)
```
- CPython 3.15.2 embeddable (`Tools/python` no Editor, `StreamingAssets/Runtime/python` nos builds); `-I -S` = sem variáveis de ambiente, sem site-packages; `python315._pth` fixa o `sys.path`.
- Limites no arranque (antes do código do jogador): Job Object (memória = RAM + 16 MB acima da base, 1 processo ativo, kill-on-close); POSIX: `RLIMIT_AS` (não testado).
- Audit hook PEP 578 instalado antes de `exec` do código do jogador; o estado do hook vive em argumentos por defeito imutáveis. Bloqueia ficheiros, processos, rede, ctypes, frames/trace, alteração de funções e imports fora da lista.
- Chamadas bloqueantes: escreve `call` no stdout e lê a `reply` do stdin (thread principal). Watchdog numa thread: 3 s sem chamar a API ⇒ `exit` + `os._exit`.
- Debugger: `sys.settrace` ativado antes do hook; em pausa chama `debug.pause` com `f_locals` (snapshot).
- Stop: o Unity mata o processo. Mensagens de processos antigos são ignoradas (cada linha leva a origem).
- Próximas linguagens: mesmo protocolo JSON por processo.

## HUD programável (HTML/CSS → UI Toolkit)
- Core `Codebound.Core.Hud`: parser HTML (subconjunto), parser CSS + cascata, valores CSS, `HudLayout` (guardado como linhas `hud` no save, em base64), `HudTemplate`.
- `HudLayer`: `UIDocument` com `PanelSettings` criadas em runtime (tema vazio, fonte `LegacyRuntime.ttf`), uma `View` por caixa; refresca 4×/s e só reconstrói os elementos quando algum `{{valor}}` mudou (assinatura do texto interpolado).
- `HudRenderer`: blocos → `VisualElement` (flex column); sequências de texto/inline → um `Label` com rich text (UI Toolkit não tem fluxo inline); texto do jogador é escapado para não ser lido como tags.
- Dados: chaves de sistema (`player.*`, `game.*`, `inventory.*`, `power.*`, `machines.count`, `programs.running`) + `HudLayout.Values` publicados por programas via `hud.set` (validados no Core).
- Input: `EventSystem` + `InputSystemUIInputModule`; `GameSession.PointerOverHud` bloqueia cliques no mundo por baixo do HUD.

## Play Mode sem recarregar o domínio
O projeto usa Enter Play Mode sem domain reload: qualquer estado estático tem de ser reposto num método `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` (ver `GameBootstrap.ResetStatics`, `PixelArtFactory.ResetStatics`).

## Testes
- EditMode (NUnit) para Core: `unity test Codebound --mode EditMode` (Editor fechado) ou `unity command test run --project-path …` (Editor aberto).
- Runtime JS: `node Codebound/Tests/runtime/host.test.js`.
- Verificação em Play Mode via Unity CLI (`unity open` + `unity command`).

## Fluidos (`Core/Fluids.cs`, Etapa 3 da simulação industrial)
- **Fluidos**: água, vapor, petróleo bruto (`Fluids.All`). Unidades abstratas.
- **Segmentos**: tubos e depósitos ligados (4 vizinhos) formam um segmento com **um fluido, uma quantidade e uma capacidade** (soma: tubo 100, depósito 2×2 2500). O fluxo dentro do segmento é instantâneo e não custa nada por tubo (ideia dos segmentos de fluido do Factorio 2.0, reescrita). Os segmentos reconstroem-se quando se põe/tira um tubo ou depósito; cada tubo guarda a sua parte (capacidade proporcional), por isso dividir um segmento reparte o conteúdo e tirar um tubo leva a parte dele.
- **Nunca há misturas**: `GameState.CanPlace` recusa um tubo/depósito que juntaria segmentos com fluidos diferentes (`Factory.FluidConflict`).
- **Máquinas** (`fluidMachines`, por id, sistema `fluids` depois de `inserters`): bomba de água (na margem, água do mosaico atrás, 60/s, sem energia); bomba (do segmento de trás para o da frente, 120/s, 20 kW, `start()/stop()`); bomba de petróleo (num campo de petróleo, 10/s, 60 kW, `start()/stop()`, 1 unidade do depósito = 10 de petróleo); caldeira (carvão + água → vapor, até 300 kW = 10 vapor/s); máquina a vapor (vapor de trás ou da frente → até 150 kW, 30 kJ por unidade); enchedora (20 de petróleo → 1 bidão `crude_oil`, 2 s, 30 kW, `start()/stop()`). As máquinas têm caixas próprias (`FluidBox`, 20 unidades) e trocam com o segmento do tubo encostado à sua entrada/saída; ligar máquina a máquina diretamente não é suportado (põe-se um tubo).
- **Energia**: a máquina a vapor é um nó da rede; ordem de fornecimento **sol → vapor já feito → carvão → baterias**. O vapor só se gasta conforme a procura (a caldeira para quando o vapor enche).
- **Estufas** encostadas a um tubo de água enchem o depósito a 5/s (além do lago próximo).
- **Saves**: o primeiro tubo (menor id) de cada segmento guarda a quantidade total e os outros 0, por isso carregar soma exatamente o mesmo número (o teste de guardar/carregar a meio compara hashes). Caixas das máquinas: `bx water/steam/crude`. Saves antigos não têm fluidos: nada a migrar.
- **Programas**: `contents()` ganha `fluids` (`{"water": 45.5}`; um tubo dá o segmento inteiro); SQL: linhas `role = 'fluid'` na tabela `contents` (unidades inteiras). `pump`, `pumpjack` e `filler` aceitam `start/stop` em JS/TS, Python, PHP, C# (`Tools/cshost` recompilado), C/C++ (genérico) e SQL (`commands`).
- **Status**: `MachineState.Status` passou de campo a propriedade virtual: o tubo calcula o seu texto quando é lido (sem texto por tubo por tick).

## Energia (`Core/Power.cs`, Etapa 4 da simulação industrial)
- **Redes atualizadas em cada edição**, não reconstruídas: pôr um nó (poste, gerador, painel, bateria, máquina a vapor) liga-o às redes ao alcance (junta-as se forem várias) e dá-lhe os consumidores que ninguém alimentava; tirar um nó reagrupa só a rede dele (pode partir-se) e reatribui só os consumidores dela; consumidores entram/saem da rede do **nó mais antigo que os alcança**. Nós e consumidores ficam em quadrados de `PoleReach` (9) células, por isso cada operação só olha para os 3×3 à volta.
- **Referência**: `Factory.RebuildPowerNetworks()` constrói tudo do zero. `PowerTests` confirma, depois de 400 edições, que as redes mantidas são iguais às reconstruídas e que o mundo fica igual (hash) ao de reconstruir em cada tick. Todas as listas de uma rede estão por id (antes a ordem das baterias dependia da pesquisa em profundidade; mudou a ordem de descarga, o total é o mesmo).
- **Custo medido** (2000 edifícios): edição longe da rede 0,4 ms/tick (antes 15–17 ms, porque cada edição reconstruía tudo: 4,7 ms só a reconstrução + 2 ms dos hubs); tirar/pôr um poste no meio de uma rede grande ~3 ms (pior 4 ms). Os hubs só voltam a procurar baús quando se constrói ou tira um baú ou hub (antes: em qualquer construção e a cada 20 ticks).
- **Prioridades**: computadores, servidores, torretas e aquecedores são **críticos** (programas, defesa, sobrevivência) e recebem energia antes dos outros. Fontes por ordem: sol → vapor já feito → carvão → baterias; as baterias só carregam com sol que sobra.
- **Estatísticas por rede** (`PowerStats`, último tick): fornecido e capacidade por fonte, procura e satisfação por prioridade, carga das baterias, energia guardada e capacidade, nº de nós e consumidores; id da rede = id do nó mais antigo. Painel da máquina: duas linhas "Rede #…". Programas: `energy.network()` (JS/TS, Python, PHP, C# `Energy.Network()`, C/C++ `cb_call("energy","network",…)`), sem mudar `energy.stats()` (o exemplo em C usa um buffer de 512); SQL: colunas novas no fim da tabela `energy` (`solar_kw, steam_kw, coal_kw, battery_kw, critical_satisfaction, normal_satisfaction, stored_kj`).

## Sinais (`Core/Signals.cs`, Etapa 5 da simulação industrial)
- **Cabos de sinal** que se tocam formam uma rede de sinais (uma cor, ligação por vizinhança como os tubos; o jogador passa por cima). Sinais: ids de itens (`iron_plate`), fluidos (`fluid_water`, `fluid_steam`, `fluid_crude_oil`) e virtuais `A`–`Z`, `0`–`9`; valores inteiros de 32 bits (somas dão a volta, divisão por 0 = 0, sem exceções).
- **Fontes** (edifícios encostados a um cabo): baús (conteúdo), tubos/depósitos (nível do segmento, contado uma vez por rede), combinadores, computadores (`signals.send`). Cada tick, sistema `signals` (depois de `outputs`, antes de `belts`): cada fonte escreve em `Next`, troca-se para `Current`. Os combinadores leem o `Current` do tick anterior da rede **atrás** e escrevem na rede **à frente**: 1 tick por combinador, ciclos bem definidos (um `A+1->A` em anel conta ticks).
- **Uma só sintaxe de texto** para tudo, igual no painel, programas, SQL, multijogador e save: condição `iron_plate<100` (`< > = != <= >=`, número ou sinal à direita); constante `A=5,B=2`; aritmético `iron_plate/10->A`, `EACH*2->EACH` (`+ - * / %`); decisor `iron_plate<100->A` (envia 1) ou `A>5->B:input` (envia o valor de B à entrada). `SignalSpec` valida (nomes existentes, tamanho máximo 200, 16 pares) e escreve a forma canónica sem espaços.
- **Condições**: braços, correias/transportadores, bombas, bombas de petróleo, enchedoras e lâmpadas só trabalham enquanto a condição se verifica — **se** estiverem encostados a um cabo (sem rede, a condição é ignorada). Estado "disabled"; a lâmpada desligada não gasta energia. Máquinas com condição numa rede não dormem (os sinais mudam sem eventos). `MachineState.Condition/SignalAllows/SetCondition`; o save escreve `bx cond …` e `bx sigoff 1`.
- **Ligações** recalculadas só quando se põe/tira um cabo; pôr/tirar outros edifícios liga só esse edifício. Sem cabos, o sistema não faz nada (só as lâmpadas).
- **Saves**: o primeiro cabo de cada rede guarda os valores da rede (`bx sig A=5,…`), as saídas dos computadores vão em `bx sigout`; carregar continua exatamente (testado com o contador em anel).
- **Programas**: `signals.read()` / `signals.send({A: 5})` (JS/TS, Python, PHP, C# `Signals.Read/Send`, C/C++ `cb_call("signals",…)`); máquinas com sinais: `configure(text)` e `signals()`; SQL: tabela `signals(name, value)`, `INSERT INTO commands VALUES ('signals','send','A=5')` e ação `configure` em qualquer máquina. Multijogador: ação `cfg` dos convidados validada pelo mesmo `Factory.Configure`.
- **Limitações**: uma só cor de fio (não há vermelho/verde separados); combinadores sem consumo de energia; sem `ANY/EVERYTHING` no decisor; o HUD mostra sinais só através de programas (`hud.set`).

## Robôs logísticos (`Core/Robots.cs`, Etapa 6 da simulação industrial)
- **Robôporto** (2×2, 30 kW): cobre um quadrado de ±12 células; robôportos cujos quadrados se tocam formam uma rede logística. Os robôs parados são **itens** no robôporto (`logistic_robot`, feitos na montadora: 2 circuitos, 2 engrenagens, 4 fios); só os que voam são objetos (`Factory.Robots`, por ordem de descolagem).
- **Baús**: `StorageState.Role` = nenhum / fornecedor / pedidos. O baú de pedidos guarda pedidos em texto (`iron_plate=50,copper_plate=20`, a mesma sintaxe dos sinais, só itens, 0–1600). Um hub de armazenamento **com energia** dentro da rede empresta os seus baús normais (procurados depois dos fornecedores). Os hubs deixam de usar baús de pedidos.
- **Sistema `robots`** (último sistema da fábrica): reconstrói as redes só quando se põe/tira um robôporto, baú ou hub; move cada robô em linha reta a 3 células/s (×energia do seu robôporto, mínimo 25 %: nunca param no ar); ao chegar: apanha (o que houver, até 4), entrega (o que couber), volta ao robôporto mais próximo que cubra a posição e pousa (a carga que sobrou vai para o porão do robôporto). Depois, **distribuição**: para cada baú de pedidos (por id) e item (por nome), falta = pedido − tem − a caminho; escolhe o fornecedor mais próximo com itens **não reservados** e o robôporto com energia e robôs mais próximo desse fornecedor; até 8 descolagens por rede por tick. Reservas e "a caminho" são recalculadas a partir dos robôs em voo em cada tick (não há estado escondido).
- **Robustez**: se o baú de origem ou destino desaparece, o robô volta; sem robôporto nenhum, fica a pairar até haver um (nada se perde). Teste: em 800 ticks com dois pedidos a disputar 12 itens, o total de itens no mundo (inventários + carga em voo) nunca muda.
- **Sinais**: um robôporto encostado a um cabo envia os itens disponíveis na rede e `R` = robôs prontos.
- **Saves**: linhas `lrobot tarefa x y origem destino casa item quantidade` depois dos edifícios; pedidos em `bx req`. Testado: guardar a meio de voos e continuar dá o mesmo hash.
- **Programas**: `roboport.network()` (robôportos, robôs prontos/em voo, fornecedores, pedidos, baús do hub, itens) e `requester_chest.configure("iron_plate=50")` nas 8 linguagens; o painel mostra a rede e tem o campo de pedidos; convidados usam a ação `cfg`.
- **Limitações**: robôs sem bateria própria (a velocidade depende da energia do robôporto); sem robôs de construção; sem baús de armazenamento dedicados nem "lixo"; robôs não evitam obstáculos (voam).

## Comboios (`Core/Trains.cs`, Etapa 7 da simulação industrial)
- **Carris** são edifícios 1×1 (atravessáveis); ligam-se aos 4 vizinhos, por isso curvas e cruzamentos resultam da forma da via. **Sinais de via** são carris que dividem a via em **blocos** (componentes ligadas de carris sem sinais) e são **de sentido único** (a rotação com que se constroem): só se passam na direção da seta e só se o bloco a seguir estiver livre — sem outro comboio lá dentro nem outro comboio parado no sinal de entrada. Não há reservas guardadas: tudo se deduz das posições.
- **Depósito de comboios** (2×2): guarda a locomotiva e até 4 vagões (itens feitos na montadora), o carvão e a carga (4 vagões × 8 espaços) — por isso a carga é guardada, mostrada e devolvida com o depósito. Horário em texto: `iron_mine:full,base:empty,depot:30,x:iron_ore>=100` (cheio, vazio, segundos, ou uma condição sobre a carga com a sintaxe dos sinais). O comboio nasce no carril à frente do depósito e repete o horário.
- **Paragens** com nome (`a-z0-9_`): o carril vizinho é a plataforma. Com um comboio parado, a paragem é a "baía de carga": braços que põem lá enchem os vagões (o carvão vai primeiro para a locomotiva, até 20), braços que tiram esvaziam-nos. As paragens não são "observáveis": braços à espera junto a elas não dormem (um comboio chega sem evento).
- **Movimento** (sistema `trains`, depois de `robots`): 4 células/s (−0,4 por vagão, mínimo 1), 150 kW de carvão. Caminho: pesquisa em largura sobre (célula, de onde veio) — **sem inversões de marcha** — com ordem de vizinhos fixa; recalculado sempre que a cabeça muda de célula (cache só para o mesmo estado), por isso carregar um save dá exatamente o mesmo caminho. Os vagões seguem o rasto da cabeça. Um comboio nunca entra numa célula ocupada por outro, mesmo sem sinais.
- **Teste de segurança**: dois comboios num anel com 4 sinais, 2400 ticks: nunca partilham uma célula e ambos cumprem o horário. A primeira versão (sinais de dois sentidos, 2 blocos) bloqueava frente a frente — por isso os sinais passaram a ser de sentido único.
- **Sinais de circuito**: uma paragem encostada a um cabo envia a carga do comboio parado e `T = 1`.
- **Saves**: rasto, de onde veio, índice do horário, fase, progresso, espera e combustível em `bx` do depósito; nomes das paragens em `bx name`. Testado: guardar a meio e continuar dá o mesmo hash.
- **Programas**: `depot.configure(horário)`, `depot.train()` (estado, paragem, à espera, vagões, carvão, carga, posição), `stop.configure(nome)` nas 8 linguagens; painel com campo de texto e linha do comboio; convidados via `cfg`.
- **Limitações**: um comboio por depósito, só locomotiva à frente (sem marcha-atrás); sem sinais de cadeia (pode haver bloqueios com cruzamentos mal sinalizados); carris desenhados como cruz (sem peças de curva); comboios não atropelam nem bloqueiam o jogador; nos convidados os comboios saltam 2×/s (instantâneos).

## Integração (Etapa 8 da simulação industrial)
- **Ids de máquinas guardados** (P11): a linha `b` do save leva o id no fim (`b furnace 10 4 0 12`); carregar mantém-no, por isso programas que guardam `m12` continuam a apontar para a mesma máquina. Saves antigos (sem id) numeram por ordem, como antes. `BuildGrid.Place(..., id)` aceita um id mais novo do que todos os existentes (senão usa o seguinte). Os convidados recebem os mesmos ids do anfitrião.
- **Ações em curso guardadas** (P12): `act <id> mine|smelt|research <tech>|craft <produto> <segundos que faltam>`, pela ordem em que começaram. Os materiais só se gastam ao terminar, por isso restaurar = recriar a ação com o tempo que falta (sem verificações: já tinha começado). Os programas recomeçam ao carregar, como antes (sem callback).
- **Ecologia**: o acumulador dos passos de 1 s e o seu relógio vão no save (`ecoclock`); a contagem de árvores em cache é descartada quando uma árvore muda. Sem isto, um mundo carregado poluía em ticks diferentes do que foi guardado (bug antigo, apanhado pelo teste das ações em curso).
- **Multijogador**: os convidados recebem os robôs em voo (linhas `lrobot`), as lâmpadas acesas (`bx lit`) e os comboios (estado do depósito); configurações vindas dos convidados passam por `Factory.Configure` (ação `cfg`). Limites conhecidos: nos convidados as estatísticas de energia e os valores das redes de sinais no painel não são atualizados (só o anfitrião simula); comboios e robôs avançam aos saltos (instantâneos 2×/s).
- **Desempenho**: base mista com todos os sistemas (vapor, correias, braços, sinais, lâmpadas, robôs, carris; 2280 edifícios) = 0,8 ms/tick (limite do teste 10 ms; o orçamento do tick é 50 ms). Equivalência dormir/não dormir e guardar/não guardar testada nessa base.

## Transporte subterrâneo (Etapa 9)
- **Tapete subterrâneo** (`underground_belt`, uma `ConveyorState` com `Exit`): ao construir, fica **saída** se o tapete subterrâneo mais próximo atrás (mesma direção, até 5 células) for uma entrada; o par é procurado ao longo da linha. A entrada passa os itens da frente para a mesma pista da saída (sem tempo no túnel); a saída só aceita o que vem do túnel. Entrada sem saída fica bloqueada e não dorme (uma saída pode aparecer). Save: `bx ug out`.
- **Tubo subterrâneo** (`pipe_to_ground`, `PipeState.Underground`): liga-se só ao tubo atrás de si e ao par — o primeiro tubo subterrâneo à frente (até 10 células) virado para trás. Os tubos ao lado não se ligam. A colocação recusa ligar dois fluidos pelo túnel.

## Máquinas em várias versões (Etapa 10)
- `BuildingDef` ganhou `Speed`, `Energy`, `ModuleSlots` e `BaseType`. Perfuradora Mk2 (velocidade 2, energia 2), Montadora Mk2 (1,5 / 1,5), Mk3 (2,25 / 2,5), Fornalha elétrica (2; 90 kW da rede em vez de carvão, não aceita carvão).
- As ações avançam a `velocidade × energia recebida`; a procura de energia é a da primeira versão × `Energy`. `Factory.SpeedOf` é o ponto único para a velocidade (a Etapa 11 junta-lhe módulos e beacons).
- Programas: `type` = tipo da primeira versão (`drill`, `assembler`, `furnace`), por isso `mine/craft/smelt` e `machines.find("drill")` funcionam em todas as linguagens sem mudar os runtimes; o novo campo `model` diz a versão.

## Módulos e beacons (`Core/Modules.cs`, Etapa 11)
- Módulos de velocidade (+20 % velocidade, +50 % energia), eficiência (−30 % energia) e produtividade (−5 % velocidade, +40 % energia, +4 % produto). Ranhuras: perfuradora 2, Mk2 3, laboratório 2, montadora Mk2 2, Mk3 4, fornalha elétrica 2; primeira montadora e fornalha de pedra 0.
- `MachineState.Modules` (inventário criado ao construir se `ModuleSlots > 0`), `AllInventories` (inventários + módulos: saves, reembolso, painel, `contents()`, SQL), `PutByHand` (o jogador/convidado põe módulos nas ranhuras, um por ranhura; braços e correias não).
- `Factory.Effects`: módulos próprios + metade dos módulos de velocidade/eficiência de cada **beacon com energia** a ≤3 células (só afeta máquinas com ranhuras). Velocidade = versão × max(0,2; 1+soma); energia = base × versão × max(0,2; 1+soma).
- Produtividade: `Produce` junta `ProductivityBonus` a cada produto; cada 1 inteiro dá um produto grátis (fica guardado se a saída estiver cheia). Guardado em `bx prodbonus`.