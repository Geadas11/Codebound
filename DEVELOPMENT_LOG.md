# DEVELOPMENT LOG

## 2026-11-20 — Sessão 1
**Ambiente:** Windows 11, Unity 6000.6.10f1 (licença Personal ativa), Unity CLI 1.0.0, dotnet runtime, Python, Node.
**Feito:**
- Projeto criado com `unity projects create` (template `com.unity.template.universal-2d`, sem Unity Cloud). Pacote `com.unity.pipeline` incluído para automação via CLI.
- Documentos de continuidade criados.
- Core (sem dependências Unity): `Cell`/`Rotation`, `ItemDatabase`, `Inventory`, `BuildingDatabase`/`BuildGrid`, `WorldGenerator` (ruído de valor próprio, determinístico), `WorldState`.
- Game: `GameBootstrap` (constrói o jogo por código ao carregar a cena), `PlayerController`, `CameraFollow`, `ChunkRenderer`, `BuildingViews`, `BuildController`, `MiningController`, `DebugHud`, `PixelArtFactory` (sprites procedurais temporários).
- 10 testes EditMode — **10/10 passam** (`unity test`, batch mode). Assembly `Codebound.Game` compila sem erros.

**Decisões:**
- Sem física Unity para o jogador: colisão por consulta à grelha (mais simples e determinística, útil para multiplayer).
- Árvores bloqueiam movimento; jazidas de minério são caminháveis; não se constrói sobre jazidas (até existirem perfuradoras que as usem).
- Sprites gerados em código até à passagem de arte (Etapa 8).
- HUD IMGUI só para desenvolvimento.

**Verificação em Play Mode** (depois de o utilizador aceitar os termos do Editor): frames a avançar, 0 erros/avisos na consola, 16 chunks carregados, 120 FPS, spawn em Floresta. 5 estruturas colocadas via `script eval` → 5 sprites, computador rodado 90° correto. Inventário com carvão/madeira recolhidos pelo input real. Screenshots em `Codebound/Screenshots/`.
- Ajuste: caixa do inventário passa a ter altura proporcional ao conteúdo (alteração trivial, não recompilada ainda).

## 2026-11-20 — Sessão 1 (parte 2): máquinas, save/load e JavaScript real
**Feito:**
- Core: `GameState` (agregado), `Factory` (redes elétricas, ações temporizadas, saída para a frente), `Machines` (baú, fornalha, perfuradora, gerador, computador), novos edifícios (gerador a carvão, poste), `SaveSystem` (texto versionado), `Json` mínimo. Kit inicial: 6 silício (salvado da nave).
- Runtime JavaScript fora do Unity: `host.js` + `worker.js` (Node 24, permission model, worker por programa, vm sem require/eval, chamadas bloqueantes via Atomics, watchdog, limites de RAM, eventos).
- Unity: `ProgramManager` (processo, protocolo, API validada, CPU calls/s, eventos), `MachinePanel` (E: transferência de itens; computador: editor de código com consola, erros por linha, dicas, monitor), F5/F9.
- Testes: 16/16 EditMode, 8/8 runtime. Play Mode: programa JS real minerou pedra para o baú, eventos `item_mined` recebidos, save→load restaurou edifícios/inventários/código e reiniciou o programa automaticamente; sem processos Node órfãos depois de sair do Play Mode.

**Decisões:**
- 1.ª linguagem = JavaScript (Node já instalado, MIT, isolamento testável hoje). Python segue com o mesmo protocolo.
- Regra "todas as máquinas precisam de programa" aplicada às máquinas de produção (perfuradora, fornalha); infraestrutura (gerador, postes, baús, paredes) é passiva. Registado em GAME_DESIGN.
- Ações de máquinas custam tempo de jogo e as chamadas da API têm limite por segundo (CPU do computador) — estilo The Farmer Was Replaced.

**Bugs encontrados e corrigidos:** watchdog nunca disparava (as estatísticas de CPU reiniciavam o relógio) — apanhado pelo teste do runtime; respostas atrasadas de um programa parado podiam chegar a um programa novo no mesmo computador — corrigido.

**Limitações conhecidas:** Node não é incluído nos builds ainda; sem restrição de rede ao nível do processo (ver TECHNICAL_ARCHITECTURE); sem debugger; UI IMGUI provisória.

## 2026-11-20 — Sessão 1 (parte 3): debugger, TypeScript, transportadores
**Feito:**
- `prepare.js` no host: instrumentação do código (AST do compilador TypeScript) com `__dbg(linha, getVars)` inserido **na mesma linha** antes de cada instrução → breakpoints/step/variáveis sem mexer nos números de linha. Instruções embutidas (`if (x) foo();`) são envolvidas em `{}`; instruções com label ficam de fora.
- Pausa = chamada bloqueante `debug.pause` no mesmo protocolo da API (não conta para CPU nem energia). Breakpoints alterados com o programa a correr seguem na próxima resposta.
- TypeScript: `transpileModule` no host (o worker não carrega os 9 MB do compilador); erros de execução mapeados para a linha TS via source map; erros de sintaxe com linha.
- Compilador TypeScript 5.9.3 copiado da instalação global local (`npm root -g`), com `LICENSE.txt` e `ThirdPartyNoticeText.txt` em `Runtime/js/vendor/typescript/`.
- Correia (`belt`) e transportador programável (`conveyor`: `start/stop/setFilter`); 1 item por célula, 1,875 células/s; caminháveis; itens desenhados a deslizar.
- Testes: 17/17 EditMode, 11/11 runtime. Play Mode: sessão de debug TypeScript (pausa na linha 3 com `i=0`, depois `i=1`, breakpoint removido em execução, resultado `total 30`); cadeia perfuradora → correia → transportador (ligado por JS) → correia → baú entregou itens.

**Correções:** perfuradoras extraíam petróleo como sólido → `WorldRules.DrillCanMine` exclui árvores e petróleo (petróleo vai precisar de bombas).
**Nota de ferramentas:** nesta máquina a shell interpreta barras invertidas em heredocs → usar Write/Edit para código com `\`.
**Aviso por investigar:** "Deleting invalid font reference" apareceu uma vez na consola (provavelmente a fonte dinâmica do editor IMGUI).

## 2026-11-20 — Sessão 1 (parte 4): braços, build Windows, self-test
**Feito:**
- Braço/inserter (`InserterState`): a cada 0,8 s tira 1 item do inventário "extraível" do edifício de trás (`MachineState.Extractable`: itens do baú, saída da fornalha/perfuradora, item da correia) e mete-o no da frente se este o aceitar; `start/stop/setFilter` pela API (partilhado com o transportador programável). Tecla 0.
- `SelfTest` (`Codebound.exe -selftest`): monta gerador + computador, corre um programa JavaScript e um TypeScript no runtime incluído, escreve `[SELFTEST] PASS/FAIL` no log e sai com código 0/1.
- `Codebound.EditorTools.BuildScript` (menu **Codebound > Build Windows (with Node runtime)**): build do player + cópia do `node.exe` para `Codebound_Data/StreamingAssets/Runtime/node/`.
- Build Windows: 93 s, OK. Executável com `-selftest`: **PASS**, usou o Node incluído no build (não o do sistema), saiu com 0, sem processos Node órfãos.
- Testes: 18/18 EditMode, 11/11 runtime.

**Bug encontrado e corrigido:** em Play Mode sem recarregar o domínio, a cache estática de sprites devolvia texturas destruídas da sessão anterior (chão preto); também a subscrição `sceneLoaded` ficava pendurada. Agora há `ResetStatics` em `SubsystemRegistration` e a cache verifica se o sprite ainda existe. Verificado com duas sessões de Play Mode seguidas.
**Rebuild** depois da correção: build OK (17 s incremental), `-selftest` PASS outra vez.
**Licenças:** a instalação local do Node não tem o ficheiro `LICENSE`; o build avisa. Obrigatório antes de qualquer distribuição.

## 2026-11-20 — Sessão 1 (parte 5): Python (3.ª linguagem real)
**Downloads autorizados pelo utilizador:** CPython 3.15.2 embeddable (SHA-256 verificado contra python.org) → `Codebound/Tools/python/`; `LICENSE` do Node v24.13.0 → `Codebound/Tools/node/LICENSE`. Registo em `THIRD_PARTY_NOTICES.md`.

**Feito:**
- `Runtime/py/host.py`: um processo Python por programa, mesmo protocolo JSON do runtime JS. Defesas: Job Object do Windows (limite de memória = RAM do computador, `ActiveProcessLimit=1` → sem processos filhos), audit hook PEP 578 (bloqueia `open`, `os.*`, `subprocess`, `socket`, `ctypes`, `sys._getframe`, `sys.settrace`, alteração de funções, imports fora da lista), builtins sem `open/eval/exec/compile/input`, lista de módulos permitidos, watchdog de CPU (3 s). Estado do hook ligado como argumentos imutáveis por defeito (o código do jogador alcança globais por introspeção, mas não os consegue alterar).
- API Python igual à de JS em snake_case; `print()` = `log()`; `@computer.on("tick")` como decorador. O exemplo do Prompt Mestre (`while True: if energy.available(): drill.mine()`) corre tal e qual.
- Debugger Python nativo com `sys.settrace` (breakpoints, step, variáveis locais; nomes da API escondidos).
- Unity: `RuntimeProcess` (processo + leitura em threads + mensagens marcadas com a origem, para ignorar mensagens atrasadas de processos antigos); `ProgramManager` com o host JS partilhado + um processo por programa Python; Stop = matar o processo; memória do Python medida de fora (o hook bloqueia o ctypes do próprio host).
- Editor: botão "Language >" (JavaScript → TypeScript → Python), exemplos em Python, referência da API.
- Build: copia `Tools/python` e a licença do Node. Self-test do executável: **PASS** em JavaScript, TypeScript e Python com os runtimes incluídos.
- Testes: 18/18 EditMode, 11/11 runtime JS, 12/12 runtime Python. Play Mode: programa Python ligou o transportador e mineirou ferro até ao baú; Stop matou o processo (1 → 0).

**Bugs apanhados pelos testes:** o host chamava `sys.settrace` depois de instalar o hook (bloqueado pelo próprio hook); `print(True)` mostrava `true`.
**Limitação honesta:** dentro do processo o Python não é uma sandbox completa — a segurança vem da combinação processo + Job Object + audit hook. Antes de correr código de outros jogadores (multiplayer) acrescentar isolamento do SO (AppContainer/token restrito no Windows, seccomp/namespaces no Linux).

## 2026-11-20 — Sessão 1 (parte 6): HUD programável HTML/CSS
**Feito:**
- Core `Hud/`: `HtmlParser` (subconjunto tolerante, erros com linha, nunca lança), `CssParser` (seletores tag/.class/#id, descendente, listas, especificidade + ordem + inline), `CssValue` (cores #rgb/#rrggbb(aa)/rgb()/nomes, px/%/em, caixas 1–4 valores), `HudLayout` (caixas + valores publicados + notificações + save), `HudTemplate` (`{{chave}}`).
- Game: `HudLayer` (UI Toolkit; PanelSettings criadas em código, fonte embutida; painéis com barra de título arrastável, canto de redimensionar, E/_/x; botão "+"; toasts; deteção de rato sobre o HUD para não minerar/construir por baixo), `HudRenderer` (blocos → VisualElements flex; texto/inline → Label com rich text; `progress` → barra; estilos CSS → estilos UI Toolkit), `HudEditor` (IMGUI).
- `hud.set` / `hud.notify` na API JS/TS/Python e no `ProgramManager` (chaves validadas, nomes de sistema protegidos, limites de tamanho).
- Novo jogo começa com uma caixa "Status" de exemplo (HTML/CSS editáveis).
- `EventSystem` + `InputSystemUIInputModule` criados no arranque.
- Testes: 25/25 EditMode (7 novos de HUD), 12/12 runtime JS, 13/13 runtime Python. Play Mode: duas caixas com dados ao vivo e valores de um programa Python (`hud.set`), notificação, erros intencionais detetados com linha; "+" cria caixa e abre o editor, "_" minimiza, arrastar move (eventos de ponteiro do UI Toolkit). Build + self-test PASS, log do executável sem erros.

**Descoberta:** não dá para injetar rato no jogo a partir do Editor via CLI (o Input System separa estados do Editor e do jogo); os testes de UI usam eventos de ponteiro do UI Toolkit.

## 2026-11-20 — Sessão 1 (parte 7, interrompida): runtime SQL
- `Runtime/py/sql_host.py`: SQL (SQLite do CPython incluído) num processo por programa, reutilizando o protocolo, os limites do SO e o audit hook de `host.py`. Corre em ciclos (`-- @every N`, `-- @once`); tabelas TEMP só de leitura com o estado do jogo (`machines`, `contents`, `energy`, `events`, `game`); o mundo é controlado com `commands` e `hud`; tabelas do jogador persistem (dump enviado ao Unity com `{"t":"state"}`), limitado ao armazenamento do computador. Authorizer bloqueia ATTACH/VACUUM INTO, escrita/DROP nas tabelas do jogo, PRAGMAs e extensões; progress handler corta ciclos > 2 s; SELECT só é registado quando o resultado muda; debugger por linha de instrução.
- Testes: `Tests/runtime/sql_host_test.py` 7/7 (inclui 6 ataques bloqueados e consulta recursiva infinita parada em 2,1 s).
- Bugs corrigidos: `iterdump()` importava `sqlite3.dump` depois do audit hook (bloqueado) → pré-importado; restauro do save falhava porque as tabelas `commands/hud` eram criadas antes do dump.
- **Por fazer:** integração no Unity (ver NEXT_SESSION passo 1). O SQL ainda NÃO aparece no jogo.

## Sessão 2: SQL no jogo + sobrevivência
- **SQL integrado no Unity:** linguagem "sql" no editor (exemplos: regra da perfuradora, relatório, registo de produção, alarme de energia; referência das tabelas), `ProgramManager` (`sql.snapshot` com máquinas/conteúdos/energia/eventos, `sql.commands` com avisos legíveis, mensagem `state` → `ComputerState.SqlDb` guardado no save). Self-test do executável passa nas 4 linguagens (JS, TS, Python, SQL).
- Correção: erros SQL apontavam a linha do comentário antes da instrução; agora apontam a primeira linha de código (teste novo, 8/8).
- **Sobrevivência (Core `Survival.cs`):** `Vitals` (vida, fome 40 min, sede 25 min, oxigénio do fato 200 s em ar irrespirável, temperatura corporal amortecida pelo fato), `Vitals.At` (ambiente por bioma + lava), comer/beber, `Respawn`. Fruta alienígena (30 % ao cortar árvores, F para comer), beber água com LMB, aviso principal no estado e no HUD, caixa "Status" inicial com barra de vida.
- Testes: 31/31 EditMode (6 novos de sobrevivência), 8/8 SQL. Play Mode: no bioma vulcânico o O2 desceu 0,5/s e apareceu "OXYGEN LOW" no estado e na caixa HUD.
- **Não feito:** o build em `Builds/Windows` é anterior à sobrevivência e à correção das linhas SQL (o Editor estava em Play Mode a ser usado) — refazer e correr `-selftest`.

## Sessão 2 (cont.): dia/noite, estações, clima
- Core `Climate.cs`: `WorldClock` (dia de 10 min, estações de 5 dias, `Daylight`, `ClockText`), tempo regional = função pura de (seed, período de 2 min, bioma) — igual para todos os jogadores, não precisa de save; `TemperatureOffset` (amplitude dia/noite por bioma — deserto ±16 °C —, estação, tempo). Relógio guardado no save.
- Sobrevivência usa o clima: tempestades de areia/cinza tornam o ar irrespirável, a radioativa triplica a radiação, a chuva repõe água; limite de frio passou para 34 °C (noite de inverno desconfortável, não mortal).
- `ClimateView`: controla a Global Light 2D (azul à noite, quente ao entardecer, mais escuro com nuvens/tempestade), camada de cor por tempo e partículas de chuva/poeira; `ClimateView.ReduceEffects` (PlayerPrefs) para acessibilidade.
- HUD: `{{game.day}}`, `{{game.clock}}`, `{{game.season}}`, `{{game.weather}}`, `{{game.daylight}}`, `{{player.ambient}}`; estado mostra dia/hora/estação/tempo/temperatura do ar.
- Caixas HUD novas abrem abaixo do bloco de estado (Y=205).
- Testes: 36/36 EditMode. Play Mode: entardecer com chuva (luz 0,26, ~390 partículas, ar 12 °C). Build + self-test PASS.
- Nota de processo: um build via `script eval` ficou pendurado com o auto-tick do Editor desligado; com auto-tick ligado durante o build funcionou (18 s).

## Sessão 2 (cont.): agricultura e fabricação
- Core: `GreenhouseState` (4 canteiros, solo, tanque, luz), `Factory.Greenhouse(op)` (plant/water/harvest/plots, operações instantâneas), crescimento em `TickGreenhouses` (luz = max(dia, lâmpadas com energia), sem água não cresce), tanque alimentado por lago próximo (calculado ao colocar). `AssemblerState` + `StartCraft` (ação temporizada com energia): engrenagem (2 placas de ferro), fio (1 placa de cobre → 2), circuito (ferro + 3 fios + silício). `ComputerState.ForModel("computer2")`: Computador Avançado. Novos itens: engrenagem, circuito. `Factory.Daylight` vindo do relógio.
- APIs: JS/TS (`greenhouse.plant/water/harvest/plots`, `assembler.craft`), Python (idem), SQL (`commands`: plant/water/harvest/craft). Resultados normais (nada maduro, sem sementes) não geram avisos — vêm no valor devolvido.
- UI: barra de construção com botões clicáveis em várias linhas; `GameSession.PointerBlocked` (HUD UI Toolkit ou barra IMGUI) impede minerar/construir por baixo.
- Testes: 39/39 EditMode (3 novos), runtimes JS/Python/SQL todos verdes. Play Mode: programa Python plantou 4 canteiros, regou, publicou o crescimento no HUD; consumo 50 kW (computador + lâmpadas). Build + self-test PASS.

## Sessão 2 (cont.): investigação e progressão
- Core `Research.cs`: `Tech` (área, custo, pré-requisitos, desbloqueios, linha principal), árvore com Logistics → Manufacturing → Advanced Computing, Agriculture, e a linha principal Ship: Hull Frame → Reactor / Navigation / Engines → Launch Sequence (todos com materiais obteníveis). `Research` (feitas, estado, progresso da nave, save tolerante a ids desconhecidos). `LabState` + `Factory.StartResearch` (ação temporizada, 40 kW, consome o custo no fim).
- Bloqueio de estruturas: `GameState.TryBuild` recusa estruturas não investigadas; a barra mostra-as a cinzento com a investigação necessária; o modo construção explica o que falta.
- APIs: `lab.research(id)`/`lab.techs()` (JS/TS/Python), comando SQL `research`. Painel T (árvore por área, ids para usar no código). Notificações ao concluir; ao concluir o lançamento, aviso honesto de que a fase espacial ainda não existe.
- HUD: `{{research.done}}`, `{{ship.progress}}`.
- Testes: 42/42 EditMode. Play Mode: programa JS listou "logistics, agriculture", investigou Logistics em ~20 s e desbloqueou a correia. Build + self-test PASS.
- Nota: saves antigos não têm investigação — estruturas já construídas continuam, mas construir novas correias/estufas/montadoras exige investigar.

## Sessão 2 (cont.): criaturas e combate leve
- Core `Creatures.cs`: `CreatureSystem` (ruído → atenção com decaimento → ondas de 1–6; crawlers e drones; movimento direto com deslizamento e "roer" obstáculos; dano a estruturas e jogador; drones comprometem computadores; despawn a > 120 células do jogador; `Enabled` para modo pacífico), torres (`TurretState`, alcance 10, recarga 0,6 s, 25 de dano, precisam de energia). `PlacedBuilding.Health`/`MaxHealth`, `ComputerState.Compromised` — ambos guardados. Investigação "Defense" desbloqueia a torre.
- Unity: `CreatureViews` (sprites em pixel art, barra de vida, dissolução ao morrer, linha de disparo — desligada com reduzir efeitos), tonalidade vermelha em estruturas danificadas, ataque do jogador no `MiningController` (prioridade sobre minerar), notificações (onda, destruição, intrusão), `ProgramManager`: `turret.targets/fire`, evento `creature_detected`, criaturas no snapshot SQL, computadores comprometidos param e mostram erro; Run restaura.
- Testes: 48/48 EditMode (6 novos), runtimes JS/Python/SQL verdes. Play Mode: programa Python com `@computer.on('creature_detected')` + ciclo `turret.targets()/fire()` abateu 3 crawlers com 6 tiros, torre intacta. Build + self-test PASS.
- Erros meus apanhados pelos testes: jogador a 1e6 nos testes (criaturas eram removidas por estarem longe); patch de criaturas para SQL caiu no sítio errado (`energy.stats`) — corrigido.

## Sessão 2 (cont.): opções e acessibilidade
- `Settings` (PlayerPrefs): pacífico, reduzir efeitos, alto contraste, paleta daltónica, escala de UI e de texto; `UiGui` (escala do IMGUI por `GUI.matrix`, rato convertido para coordenadas escaladas); `Controls` (12 ações remapeáveis; setas continuam a mover).
- `OptionsMenu` (Esc): pausa (`Time.timeScale = 0`), navegação ↑/↓ ←/→ Enter Esc e rato, remapear tecla a tecla, repor teclas, guardar/carregar/sair. O Esc que fecha outro painel no mesmo frame não abre o menu (estado do frame anterior).
- Aplicado em: jogador (movimento), construção, painel de máquinas, investigação, HUD fixo (ajuda mostra as teclas atuais), HUD UI Toolkit (escala, texto, contraste), clima (reduzir efeitos), criaturas (pacífico).
- Play Mode: menu aberto a 1,5×, jogo em pausa, tecla de construção remapeada para G (a ajuda mudou), modo pacífico removeu a criatura ao retomar; preferências repostas no fim do teste. Build + self-test PASS.
- Honesto: volumes/legendas aparecem como "no audio yet"; a paleta daltónica cobre as cores de estado, não é um filtro de ecrã.

## Sessão 2 (cont.): áudio
- Core `Synth.cs`: 31 receitas sintetizadas (osciladores, ruído, envolventes, filtro passa-baixo), loops com cross-fade na junção, normalização a 0,8; semente estável. Testes: todos audíveis, sem saturação, sem cliques na junção nem no fim, determinísticos.
- `AudioDirector`: 8 vozes para as máquinas audíveis mais próximas (16 células) com som por estado; ambiente com cross-fade por bioma e camada de chuva/tempestade; música adaptativa (calma / indústria proporcional às máquinas a trabalhar / perigo com criaturas próximas ou avisos de sobrevivência), camadas sincronizadas; one-shots ligados a eventos (construir, remover, criaturas, ondas, intrusões, investigação, minerar, comer, beber, dano); `hud.alarm`; o menu de pausa silencia o mundo; legendas com direção (opção).
- Opções: volumes Master/Música/Ambiente/Máquinas/Efeitos, legendas de sons.
- Equilíbrio: isolamento do fato aumentado (0,12 → 0,08): ao meio-dia no deserto (~54 °C) o corpo fica no limite em vez de sofrer dano contínuo; lava continua perigosa.
- Play Mode: perfuradora + gerador + computador com vozes ativas, camada "industry" a 0,09, ambiente do deserto; com uma criatura perto a camada de perigo subiu; legenda do alarme programável visível (fundo opaco após correção do contraste). Build + self-test PASS.
- Não verificado: setas de direção nas legendas (o teste via Editor usa outro relógio), e não se ouviu o som (sem colunas na verificação automática).

## Sessão 2 (cont.): git e Graphify
- Repositório git criado (`.gitignore` sem Library/Builds/Tools/python/Screenshots; `.gitattributes`), commit inicial `37ef698`, enviado para `Geadas11/Codebound` no branch `desktop` (a pedido do utilizador).
- Graphify ligado: indexa o branch principal do repositório, que contém outra versão do jogo; para indexar este código o branch principal tem de passar a `desktop`.
- Correção: o runtime Python escrevia `__pycache__` em StreamingAssets (ia para os builds) → `-B` e `sys.dont_write_bytecode`.

## Sessão 3 (2026-11-21): segurança informática e reparação
- Graphify confirmado a indexar o branch `desktop` (7204 nós); notar que o grafo não resolve chamadas feitas através de propriedades (`session.Factory.X()`).
- Core: `ComputerState` com palavra-passe (PBKDF2-SHA256, 20 000 iterações, sal aleatório, comparação em tempo constante; só o hash é guardado), `CreatureSystem.HackTime` (firewall ×3 com a investigação Cybersecurity, palavra-passe ×2), evento `IntrusionStarted`; `GameState.Repair` (1 pedra = 20 HP, limitado ao máximo).
- Unity: cadeado no editor de código (pede a palavra-passe uma vez por sessão), secção Segurança no editor (estado da firewall, definir/mudar/remover palavra-passe, tempo de invasão), evento `intrusion` para programas, notificação e alarme sonoro com legenda; reparação com LMB no `MiningController`.
- Testes: 53/53 EditMode. Play Mode: drone a atacar PC com firewall + palavra-passe → programa JS recebeu `INTRUSION m2 drone 1 in 18 s`, computador não comprometido; editor trancado mostrado. Build + self-test PASS.

## Sessão 3 (cont.): energia renovável
- Core: `SolarState`, `BatteryState` (capacidade em kJ); `Factory.SolarFactor` (definido pelo `GameBootstrap` a partir de `WorldClock.SolarFactor(clima)`), redes com painéis e baterias; fornecimento solar → carvão → bateria; baterias carregam só com sobra solar; `Factory.Totals()` e `NetworkStoredKJ`.
- Investigação Energy desbloqueia painel e bateria; sprites; HUD `power.storedKJ`; API `energy.stats().storedKJ`.
- Testes: 56/56 EditMode (3 novos: dia/noite com bateria, clima reduz o solar + totais, bloqueio por investigação + carga guardada).

## Sessão 3 (cont.): localização
- Core `Loc`: tabela chave → texto, inglês escrito no próprio código como recurso; `T(chave, inglês, args)` com `string.Format` invariante; tradução com marcadores errados volta ao inglês em vez de rebentar; `Building/Item/Tech`.
- Tabelas geradas por `Codebound/Tools/localization/build_tables.py` (uma linha por chave com as 10 línguas) para `StreamingAssets/Localization/<código>.json`.
- `Settings.Language` (PlayerPrefs `cb.lang`, omissão = idioma do SO), `ApplyLanguage` no arranque; `Settings.UiFont` cria fonte do SO (Yu Gothic / Microsoft YaHei / Malgun Gothic) para ja/zh/ko, aplicada ao IMGUI em `UiGui.Begin` e ao HUD UI Toolkit.
- Traduzidos: menu de opções (com seletor de idioma), nomes das teclas, inventário/ajuda/barra de construção, painel de investigação, notificações, nomes de estruturas/itens/tecnologias/áreas.
- Testes: 58/58 EditMode (todas as línguas têm as chaves do inglês, sem vazios e com os mesmos marcadores; recurso ao inglês). Play Mode: painel de investigação em japonês e menu em pt-PT corretos (capturas). Build + self-test PASS; as 10 tabelas vão no build.
- Por traduzir: descrições das tecnologias, painel de máquinas/editor, legendas de som, mensagens do `BuildController`/`MiningController`.

## Sessão 3 (cont.): exploração
- Core `Exploration.cs`: `Facility` (terminal + caixa a 2 células), uma por região de 48×48 com 55% de hipótese se o ponto cair nas Ruínas em chão seco e vazio; `Read` entrega os registos por ordem (o primeiro terminal lido é sempre o registo 1; a partir do 8.º repete o último); `Loot` uma vez por caixa; `Lore` com 8 registos (expedição automática Meridian, drones custódios, WARDEN — liga as criaturas e drones hostis à história, sem civilizações NPC).
- Unity `ExplorationController`: sprites de terminal/caixa (aberta quando saqueada) num raio de 48 células, E lê/abre, J diário com scanner. Tecla `journal` remapeável.
- Testes: 61/61. Play Mode: posto a 202 células do início, registo 1 lido, caixa deu placas/fio/circuitos/fruta, diário em pt-PT com scanner "49 células E".

## Sessão 3 (cont.): ecologia
- Core `Ecology.cs`: campo de poluição por regiões de 16×16 em passos de 1 s (independente do frame rate); emissões, dispersão 5%/s, decaimento 0,4%/s + 0,004/s por árvore (contagem em cache por 60 s); purificador de ar (`ScrubberState`, consumidor passivo de 30 kW) remove 2/s da sua região; `GrowthFactor` nas estufas; `Noise()` das criaturas + total/500.
- Unity: neblina castanha proporcional à poluição local, ar irrespirável no smog; chaves HUD; API `world.pollution()` → {here, total, smog, smogLevel}; SQL `game.pollution`.
- Equilíbrio: uma região com pico dissipa-se em ~30 s para as vizinhas; smog exige vários geradores a plena carga juntos.
- Testes: 64/64 EditMode, runtimes JS/Python/SQL a passar. Play Mode: smog → "unbreathable", oxigénio do fato a descer, neblina visível.

## Sessão 3 (cont.): Etapa 6 — multijogador LAN
- Core `Multiplayer.cs`: `Snapshot` (save filtrado: sem hash/código/SQL de computadores trancados, sem inventários de outros convidados), `Apply` (sincroniza deltas minados, edifícios por chave def+posição+rotação — os iguais ficam, mudam só o estado; relógio, investigação, exploração, poluição; mantém o código que o convidado está a escrever), `Act` (validação das ações), conversões de criaturas/inventários; `GameState.Guests` + `ginv` no save; `TryBuild/TryDeconstruct/Repair` com inventário pagador.
- Unity `NetSession.cs`: `Peer` (thread de leitura, limite de linha 256 KB do convidado / 32 MB do anfitrião, timeout de envio), `Net` estático (sobrevive ao recarregar da cena do convidado), anfitrião (aceita, valida `hello` com versão e nome, processa ≤64 mensagens/frame por convidado, instantâneos 2×/s), convidado (aplica só o instantâneo mais recente, envia posição 10×/s), fantasmas com nome, painel N.
- Interceções no convidado: construção, remoção, mineração, reparação, ataque, painel de máquinas (tirar/pôr, Run/Stop/Save → anfitrião, palavra-passe enviada ao anfitrião), exploração, comer; sem simulação local, sem F5/F9, programas nunca arrancam localmente.
- Bug encontrado no teste real: o nome do convidado era reposto das preferências ao recarregar a cena → corrigido.
- Testes: 67/67. Verificação real descrita no TASKS.

## Sessão 3 (cont.): Etapa 7 — espaço
- Core `Space.cs`: `StarSystem` (derivado da seed), `Ascent` (constantes afinadas com um protótipo Python: a fundo → sobreaquece; controlo proporcional ao calor → órbita com ~25% de sobra; erro encontrado no protótipo: com a viragem a acabar em 120 km a altitude nunca chegava lá — agora acaba em 130 km), `LaunchPadState` (save do voo), `Voyage.Check/Cost/Arrive`. `WorldGenerator` com `PlanetKind` (viés de temperatura/humidade/estranheza; spawn aceita deserto/ruínas noutros mundos). Save: registo `planet <id> <kind> <homeSeed>` antes de `seed`.
- Unity: operações da plataforma em `ProgramManager` (+ worker.js, host.py, comandos SQL), `GameBootstrap.OnTravel` (guarda o mundo que fica, carrega/gera o destino), `SpaceView` (nave, chama, leitura), chaves HUD.
- Testes: 70/70 + runtimes. Play Mode: subida pilotada por JS até à órbita, viagem para o mundo vulcânico; ficheiro de planeta criado pelo teste apagado depois.

## Sessão 3 (cont.): Etapa 8 — polimento (início)
- `TitleMenu` (ecrã inicial com teclado/rato, traduzido), `GameBootstrap.NewGame(seed)`, `LoadFrom(path)`, auto-save de 5 min em ficheiro próprio com escrita atómica.
- Play Mode: ecrã em pt-PT, mundo pausado; Novo jogo com seed 123 gerou esse mundo e retomou o tempo. 70/70 testes, build + self-test PASS.

## Sessão 3 (cont.): PHP e C#
- PHP 8.5.13 (download autorizado, SHA-256 verificado). `Runtime/php/host.php` com o mesmo protocolo do Python; testado primeiro que `open_basedir` numa pasta inexistente bloqueia ficheiros/diretórios/`SplFileObject`/`include` sem impedir o próprio anfitrião, e que funções desativadas podem ser redefinidas (o `sleep()` do jogo). Sem threads no PHP: o vigia de CPU passou para o `ProgramManager` (`BusySince`). `RuntimeProcess.Confine`: Job Object por P/Invoke. Build leva só `php.exe` + `php8.dll` + runtime VC (14 MB).
- C#: esta máquina não tem SDK .NET, mas o Unity Editor inclui um (8.0.318, com Roslyn) — usado offline. `Tools/cshost` (Host.cs: protocolo, API, compilação em memória com PDB para linhas de erro; Sandbox.cs: lista branca por símbolo). Runtime .NET 8.0.21 copiado para `Tools/dotnet`.
- Bug encontrado pelo self-test do build: o anfitrião C# devolvia o id do programa como texto e o Unity só aceita números → as chamadas eram ignoradas e o vigia matava o programa. Corrigido (o id volta tal como chega) e os testes de runtime de PHP e C# passaram a verificá-lo.
- Correção ao que foi dito ao utilizador: o wasi-sdk para Windows tem 619 MB (não ~150 MB) → C/C++ à espera de nova confirmação.
- Self-test do build: JS, TS, Python, SQL, PHP e C# PASS.

## Sessão 3 (cont.): C e C++
- wasi-sdk 34 (619 MB, autorizado; SHA-256 conferido) + textos de licença dos repositórios oficiais (o tarball não os traz). Build leva um SDK reduzido de ~194 MB; descoberto que o driver do clang só encontra os cabeçalhos da libc++ por alvo se existir a pasta genérica `include/c++/v1` (vazia).
- `Runtime/c/codebound.h` (API C + `cb::Machine` em C++), `Runtime/js/wasm_worker.js` (WASI escrito à mão: stdout/stderr, relógios, aleatório, `poll_oneoff` = sleep do jogo, resto ENOSYS; ponteiros verificados; memória importada com máximo = RAM), `host.js` com `startWasm`/`attach`.
- Problemas encontrados nos testes: memória importada com páginas iniciais diferentes das do linker; `printf` com buffer total porque o fd dizia ter direitos de seek (a libc só usa buffer por linha em "terminais"); o otimizador apagava `malloc` não usados no teste de memória.
- Unity: `Core/CSource.cs` (filtro, 15 testes), `RuntimeProcess.RunConfined` (Job Object com vários processos — o clang chama o wasm-ld — e tempo limite), compilação numa thread e envio quando termina; mensagens do compilador sem caminhos locais.
- 85/85 EditMode; runtimes JS, Python, SQL, PHP, C#, C/C++ a passar; self-test do build com 8 linguagens PASS. Build: 525 MB.

## Sessão 3 (cont.): Etapas 2, 4 e 5 completas
- Etapa 2: `sorter`/`splitter` (estado `ConveyorState.Sorts/Turn`, saídas calculadas com `Rotation.Move`), `long_inserter` (alcance 2, 1,5× mais lento), `Factory.Activity` + LED/pulsação em `BuildingViews`. Bug apanhado pelo teste: o separador recusava à entrada itens que não eram do filtro.
- Etapa 4: `<button>`/`<img>` no `HudRenderer` (+ `ProgramManager.Broadcast`), `computer.send` + `ServerState` + `StorageHubState` (Core/Networking.cs), `BuildGrid.StructuresBetween` para oclusão no `AudioDirector`, `ColorVision` + `ColorFilter` (Volume/ChannelMixer do URP; asmdef passou a referir `Unity.RenderPipelines.Core.Runtime`).
- Etapa 5: Core/Shelter.cs (aquecedor, abrigo, relâmpagos, para-raios → `Factory.StoreEnergy`, armadilha, equipamento), Core/Plants.cs (espécies, genética), hidroponia, rações, Core/Caves.cs (camada subterrânea reaproveitando a troca de mundos da viagem espacial: `Voyage.Carry`, `GameBootstrap.SwitchWorld`), `CaveController`, lanterna `Light2D`, Core/Skills.cs, tipos de ruína, 7 ramos de investigação, dono dos edifícios + papel admin.
- Barra de construção passou a mostrar só o desbloqueado (com 31 edifícios ocupava meio ecrã) + "+N na árvore de investigação".
- Traduções: `Tools/localization/tables_more.py` (descrições, registos, textos novos) → 230 chaves.
- Ferramentas: a saída do `unity command script eval` termina numa linha vazia e tem caracteres de controlo — ler para ficheiro e filtrar com `tr -d '\r\033' | grep -v '^$'` (explica as respostas "vazias" desta sessão).
- Testes: 107/107 EditMode; runtimes JS/TS, Python, SQL, PHP, C#, C/C++ a passar; self-test do build com 8 linguagens PASS. Play Mode: LEDs, clique num botão do HUD recebido por um programa JS, descida a uma caverna (escuro, lanterna, escada, 12 °C). Ficheiro de planeta criado pelo teste apagado.

## Sessão 3 (cont.): Etapa 3 completa
- Debugger: modos continue/step/over/out pela profundidade da pilha; pausa envia `[linha, variáveis, pilha, watches]`, resposta `{mode, watches}`. JS: `__dbgIn/__dbgOut` (try/finally) inseridos pelo `prepare.js`; Python: `sys.settrace`; PHP: `declare(ticks=1)` (pára depois da instrução, só globais visíveis); C#: instrumentação Roslyn (`Tools/cshost/Debugger.cs`, `Codebound.Dbg` proibido aos jogadores; se o código instrumentado não compilar corre sem debugger). Watches só por caminho — o sandbox proíbe gerar código.
- Vários ficheiros: `ComputerState.Files` (nomes validados, sem `main`, ≤8), enviados no `start`. JS compila cada módulo no realm do programa; TS passou a CommonJS. C/C++: `CSource.Check` aceita `#include "x.h"` só dos ficheiros do computador; armadilhas WASM mapeadas para ficheiro+linha com a `.debug_line` (DWARF v4, `-gline-tables-only`).
- `Core/Assist.cs`: dicas por linguagem, próximo passo do modo Guiado, terminal (`Terminal.Execute` devolve a ação; o `MachinePanel` executa-a). `ProgramManager.End` regista o histórico em todas as saídas; `CallMap` alimenta o diagrama.
- `MachinePanel`: separadores de ficheiros, barra em duas linhas, painel lateral com Monitor/Debug/Files/Terminal/History/Diagram/API. Dois erros de IMGUI evitados (rótulo a aparecer no mesmo evento do clique; separador Debug forçado em cada frame) e diagrama refeito em layout radial depois das capturas.
- Nota de teste: `GameBootstrap.NewGame` por `script eval` sem fechar o ecrã inicial deixa `Time.timeScale = 0` (o jogo real fecha-o antes) — repor com `Time.timeScale = 1`.
- Testes: 114/114 EditMode (7 novos em `ProgrammingToolsTests`); runtimes JS/TS, Python, PHP, C#, C/C++ a passar; build + self-test PASS; Play Mode: pausa na linha 4 com pilha `count ← main`, `require("utils")`, snippet do terminal (`42`), histórico.

## Sessão 4: Etapas 5 (tradução) e 6 completas
- Tradução: `L()`/`Loc.T` em todo o `MachinePanel` (máquinas + editor), `Assist` (40 dicas e 6 passos do modo Guiado com chaves numeradas `hint.N`/`tip.N`, respostas do terminal), legendas do `AudioDirector`, estados das máquinas só no ecrã (`st.*`). Tabelas novas em `Tools/localization/tables_etapa5.py` (lidas pelo `build_tables.py`); verificado que os `{n}` e as tags são iguais em todas as línguas.
- Multijogador: `Core/Multiplayer.cs` (`CleanChat`, `CheckMove` com orçamento, `LoadVitals`, `WorldPasswordOk`, `Snapshot(g, unlocked)`, `Apply` mantém ficheiros/watches do computador em edição), `GuestBody` + linha `gbody` no save, `Core/Upnp.cs` (texto SSDP/SOAP) + `Game/RouterPort.cs` (sockets; só aceita a descrição servida pelo próprio aparelho que respondeu). `NetSession.cs` reescrito: mensagens `chat`, `vit`, `view`, `unlock`/`unlocked`, `pc`, `setpos`; `program` com ops run/debug/stop/resume/bp/snippet/setpwd/clear. Protocolo passou a versão 2.
- `MachinePanel` com caminho de convidado: estado do programa vem de `Net.RemotePc`; os exemplos ficam escondidos durante a execução (a barra não cabia em pt-PT).
- Testes: 121/121 EditMode; build + self-test PASS; cliente de protocolo em Python contra o Editor anfitrião (13/13) e anfitrião simulado em Python para o Editor convidado (capturas `etapa6_guest.png`, `etapa6_chat.png`).
- Ferramentas: o build não aceita `unity command --runtime` (sem suporte Pipeline no player), por isso os testes de rede usam o Editor + scripts Python. Heredoc voltou a estragar `\n` num patch — corrigido com um .py em ficheiro.

## Sessão 4 (cont.): Etapa 6 cifrada e Etapa 7 completa
- `Core/SecureChannel.cs`: DH grupo 14 (RFC 3526) com `BigInteger` + PBKDF2-SHA256 (20 000 iterações) da palavra-passe do mundo + HMAC das duas chaves públicas → 4 chaves (cifra/MAC por sentido). AES-256-CBC com IV aleatório, MAC sobre seq‖IV‖texto, comparação em tempo constante. A verificação explícita da palavra-passe foi retirada (a cifra já a verifica). Linhas de convidado até 1 MB (base64).
- O cliente de teste passou de Python para C# (`Tests/nettest`, compilado com o .NET do Unity e com o próprio `SecureChannel.cs`) porque o Python incluído não tem AES.
- Bug encontrado no teste real: o convidado mostrava "ligação perdida" em vez de "palavra-passe errada" — o anfitrião fecha logo a seguir ao kick; agora o convidado lê as mensagens pendentes antes de desistir.
- Etapa 7: `Core/Orbit.cs` (`Story`, `Relays` com desafios determinísticos e `Check` por valor, `Asteroids`), `LaunchPadState` com porão, `Land`, `MineAsteroid`, `Dock`, `Answer`, `Transmit`; `Voyage.Carry` leva história, convidados e corpos; `Arrive` leva o porão. `EndingView` (GUI.depth à frente da leitura da órbita). `SpaceView` mostra a órbita.
- Descoberto ao testar: a variável `pad` nunca existiu nos programas (o jogo e o TASKS diziam `pad.launch()`); adicionada em JS/Python/PHP/C#.
- Teste de rede deixou `planet_1_home.cbsave` na pasta de saves (mundo de teste com seed 1); a remoção automática foi bloqueada — pode ser apagado à mão.
- Testes: 129/129 EditMode; runtimes JS/TS, Python, PHP, C#, C/C++ a passar; build + self-test PASS.

## Sessão 4 (cont.): Etapa 8
- Desempenho medido antes de mexer (`PerformanceTests` com 2000 edifícios + decomposição temporária por passo do tick): `LinkStorageHubs` 4,3 ms/tick (todos os hubs × todos os edifícios) e `RebuildNetworks` 21,8 ms (todos os postes × todos) — passaram a usar a grelha/baldes espaciais, mantendo a mesma ordem de resultados. Em Play Mode, `BuildingViews.LateUpdate` gastava 31,6 ms/frame: `Settings.ReduceEffects`/`ColorblindPalette` liam `PlayerPrefs` (registo do Windows) por edifício — cache em `Settings`.
- `Core/Tutorial.cs` + `Game/TutorialView.cs`; linha `tutorial` no save (sem ela = save antigo → concluído); viaja entre mundos.
- `PixelArtFactory` refeito: `Tile` (Blot periódico, detalhes), `Edge` (franja do tipo de chão mais forte, máscara de 4 lados, espuma), `Resource(r, variante)`, `Tree(variante)`, `Building(def, frame)` + `Frames(def)`, `Shadow`, `Player(frame)`, `Creature(t, frame)`. `ChunkRenderer` com camada de bordas (lê o pedaço + 1 célula de margem uma vez) e variantes de recursos. `BuildingViews` guarda os SpriteRenderers, troca de frame só quando muda.
- Verificação de frames: texturas não legíveis → cópia pela GPU (Blit + ReadPixels) para comparar; as criaturas eram simétricas (o espelho dava o mesmo frame) — corrigido.
- `Core/Music.cs`: partitura em notas + instrumentos sintetizados, caudas das notas dão a volta ao loop; `Synth.Render` delega as camadas `mus_*`; `AudioDirector` compõe com `Task.Run` e arranca as 4 camadas juntas; dia/noite por `Clock.Daylight`.
- Nota de teste: depois de `NewGame` por script, além de `Time.timeScale = 1`, é preciso `TitleMenu.IsOpen = false` (o jogo real fecha o ecrã inicial antes).
- Testes: 135/135 EditMode; build + self-test PASS.

## Sessão 5 (2026-11-24): simulação industrial — Etapas 0 e 1
- Auditoria (`INDUSTRY_PLAN.md`): o relógio e os relâmpagos avançavam por frame (P1); ordem de atualização acidental (`Dictionary` com posições reutilizadas, P2); sem contador de ticks (P3); 8 varrimentos de todos os edifícios por tick (P5); texto de estado criado em cada tick (P6). `TECHNICAL_ARCHITECTURE.md` estava desatualizado (só JS/Python).
- `Core/Simulation.cs` (`SimSystem` com prepare/update/finish, `Simulation.Run`, `StateHash`), `GameState.Step/Tick/PendingPlayerDamage`, `Factory.Systems()`, listas por tipo, `BuildGrid.Buildings` por id, sleep/wake por célula (`sleepersAt`, `MachineState.Asleep/Touched`; filtros e liga/desliga de correias e braços passaram a propriedades que avisam).
- Bug apanhado pela medição: uma correia adormecida a quem um braço tirava o item e que depois recebia outro ficava a dormir. Corrigido (mudar o próprio conteúdo acorda) e com teste.
- Medições (Editor, mesma corrida, dois mundos iguais alternados): 8000 edifícios, sleep desligado vs ligado: tick 5,4 → 4,2 ms; correias 0,84 → 0,14; braços 0,24 → 0,13. 2000 edifícios: ~0,5 ms/tick (antes da etapa ~1,0). As medições no Editor variam até 2× entre corridas (máquina ocupada): por isso comparo sempre dentro da mesma corrida.
- `GC.GetAllocatedBytesForCurrentThread` devolve 0 neste Mono: alocações não medidas; só se removeram as fontes óbvias (texto de geradores, baterias, hubs, laboratórios; listas e arrays por tick).
- Testes: 142/142 EditMode; build + self-test PASS; Play Mode: tick e relógio corretos. Runtimes das linguagens não corridos (não mudaram).

## Sessão 5 (cont.): Etapa 2 — correias com pistas e braços com mão
- `Core/Logistics.cs` (novo): `ConveyorState`/`InserterState` e o tick de correias e braços saíram de `Machines.cs`/`Factory.cs` (`Factory` passou a `partial`). `MachineState.AcceptsItem` novo (fornalha, gerador, montadora, estufa, plataforma, armadilha, baú, hub, laboratório).
- Primeira versão perdia débito (72 itens em 10 s em vez de ~130): a correia de trás via a da frente antes de esta avançar, conforme a ordem de construção. Passou a dois passos por tick (mover, depois passar) e o item leva o resto do passo para a correia seguinte.
- O teste de equivalência sleep/wake divergiu ao tick 17: uma correia bloqueada que recebia um item no segundo passo voltava a dormir com esse item ainda por avançar. Agora só dorme "compactada" (`Packed`).
- Mudança de comportamento intencional: cabem 2 itens lado a lado à entrada de uma correia (o teste "1 item por célula" foi atualizado); um braço pode ficar com 1 item na mão (antes nunca tirava se o destino estivesse cheio).
- `PerformanceTests.Building_InABigBase_DoesNotStall`: limite 15 → 25 ms. Falhava também no commit anterior (15,5 ms nesta máquina); o custo é a reconstrução completa das redes elétricas (~11 ms), a tornar incremental na Etapa 4.
- Testes: 150/150 EditMode; equivalência sleep/wake com 4000 edifícios × 1200 ticks; build + self-test PASS (8 linguagens); Play Mode: braço a 0,8 s/item, itens visíveis na pista direita.

## Sessão 5 (cont.): Etapa 3 — fluidos
- `Core/Fluids.cs` (novo): segmentos (um fluido, quantidade, capacidade), `FluidBox`, estados de tubo/depósito, bomba, bomba de água, bomba de petróleo, caldeira, máquina a vapor, enchedora; `Factory.TickFluids`, `FluidConflict`. 8 edifícios novos, 3 tecnologias (`fluid_handling`, `steam_power`, `oil_processing`), arte em pixel e animações, `tables_fluids.py` (27 chaves × 10 línguas).
- Decisões: segmentos com fluxo instantâneo (simples, barato e determinístico); misturas impedidas na colocação em vez de resolvidas depois; o primeiro tubo do segmento guarda o total no save para o carregamento dar o mesmo número exato; bomba, bomba de petróleo e enchedora são programáveis (start/stop) como as outras máquinas de produção; caldeira, máquina a vapor e bomba de água são passivas (infraestrutura, como o gerador e a correia).
- `MachineState.Status` passou a propriedade virtual (compatível com o código existente) para o tubo não criar texto por tick.
- C#: `Tools/cshost` recompilado com o SDK do Unity (`Editor/Data/DotNetSdk/dotnet.exe build -c Release -o out`); o `Tools/dotnet` só tem o runtime.
- `php -l host.php` acusa "Cannot redeclare sleep()" também na versão anterior: o host redefine `sleep` de propósito e o php corre com essa função desativada (os testes PHP passam).
- Testes: 158/158 EditMode (8 novos em `FluidTests`); `DeterminismTests.TickOrder_IsExplicit` atualizado com o sistema `fluids`; runtimes JS, Python, PHP, SQL, C#, C/C++ PASS; build + self-test PASS; Play Mode: caldeira + máquina a vapor alimentam um computador, programa JS liga a bomba e lê `contents().fluids`.

## Sessão 5 (cont.): Etapa 4 — energia
- Medição primeiro: o tick depois de uma edição custava 15–17 ms porque **qualquer** construção reconstruía todas as redes (4,7 ms com 444 nós) e fazia os hubs procurar baús (2 ms, que também corria a cada 20 ticks).
- `Core/Power.cs` (novo): redes mantidas em cada edição (juntar ao pôr, reagrupar só a rede afetada ao tirar), listas por id, prioridades (críticos primeiro), `PowerStats` por rede. `RebuildPowerNetworks()` fica como referência para os testes. O código antigo de energia saiu de `Factory.cs`.
- `energy.network()` novo em vez de alargar `energy.stats()`: o exemplo de C lê `stats` para um buffer de 512 caracteres e podia deixar de caber.
- Resultado: edição longe 0,4 ms/tick, edição no meio de uma rede grande ~3 ms; limite de `Building_InABigBase_DoesNotStall` 25 → 5 ms.
- Mudanças de comportamento: com falta de energia, os críticos são servidos primeiro (antes todos recebiam a mesma fração); baterias descarregam por ordem de id.
- Testes: 164/164 EditMode (6 novos em `PowerTests`); runtimes JS, Python, PHP, SQL, C#, C/C++ PASS; build + self-test PASS; Play Mode: `energy.network()` em JS e as colunas novas em SQL.

## Sessão 5 (cont.): Etapa 5 — sinais
- `Core/Signals.cs` (novo): `SignalSpec` (sintaxe única, validação, aritmética sem exceções), `SignalCondition`, cabos, combinadores, lâmpada, sistema `signals` e ligações. 5 edifícios novos, tecnologia `circuit_network`, arte (cabo vermelho, ecrãs # + ?, lâmpada acesa/apagada), 20 textos × 10 línguas.
- Decisão: **uma sintaxe de texto** para todas as configurações. Cada linguagem só precisa de `configure(text)` e `signals.read/send`; o painel tem um campo de texto; os convidados mandam o mesmo texto e o anfitrião valida com o mesmo código.
- Decisão: cabos por vizinhança (como os tubos) em vez de fios ponto-a-ponto: cabe na ferramenta de construção que já existe. Uma só cor (limitação registada).
- Bugs apanhados pelos testes: o analisador lia `EACH*-2` com o `-` como operador (agora o operador é o primeiro símbolo que não segue outro); uma edição de Python da Etapa 5 partiu a expressão da lista de funções (`'drill' has no function 'mine'`), apanhado pelo teste de runtime de Python e corrigido antes do commit.
- Testes: 171/171 EditMode (7 novos em `SignalTests`); runtimes JS, Python, PHP, SQL, C#, C/C++ PASS; build + self-test PASS; Play Mode: JS (`signals.send/read`, `configure`, erro de sintaxe) e SQL (tabela `signals`, `send`, `configure`).

## Sessão 5 (cont.): Etapa 6 — robôs logísticos
- `Core/Robots.cs` (novo): `RoboportState`, `LogisticRobot`, redes logísticas, movimento, distribuição com reservas, sinais, linhas de save `lrobot`. `StorageState` ganhou `Role` e pedidos; item `logistic_robot` e receita da montadora; 3 edifícios; tecnologia `logistic_robots`; arte (robôporto, baús com seta, robô a voar); 11 textos × 10 línguas (`mp.role.cargo` já existia — mantida a tradução original "porão").
- Decisões: robôs parados como itens (refundo e braços funcionam sem código novo); reservas recalculadas dos robôs em voo (sem estado que possa divergir no save); pedidos com a mesma sintaxe de texto dos sinais (uma só API em todas as linguagens); o hub de armazenamento entra como armazém da rede, ligando as Etapas 6 e 8 da missão original.
- O teste de save com `Step()` falhou na contagem porque o relógio real muda a luz (energia e velocidade dos robôs); o hash depois de carregar era igual. O teste passou a comparar os dois mundos em vez de um número fixo.
- Testes: 178/178 EditMode (7 novos em `RobotTests`, incluindo conservação de itens em 800 ticks); runtimes das 8 linguagens PASS; build + self-test PASS; Play Mode: programa Python com `configure` e `network()`, 4 robôs entregaram 20 placas.

## Sessão 5 (cont.): Etapa 7 — comboios
- `Core/Trains.cs` (novo): carris, blocos, paragens, depósito (que guarda o comboio e a carga), horários, pesquisa de caminho, movimento, sinais de circuito. 4 edifícios, 2 itens (locomotiva, vagão) com receitas, tecnologia `railways`, arte (via, sinal com seta, paragem, depósito, locomotiva e vagão rodados ao longo da via), 22 textos × 10 línguas (sem substituir chaves existentes).
- Decisão principal: **o comboio pertence a um depósito** (edifício normal). Assim o painel, o reembolso, os saves (`bi`/`bx`), os braços e a API funcionam pelo caminho que já existia, sem uma ferramenta de construção nova para pôr veículos em cima dos carris.
- Problemas apanhados: (1) um braço a dormir junto a uma paragem nunca acordaria quando o comboio chegasse — paragens marcadas como não observáveis; (2) a verificação manual mostrou dois comboios bloqueados frente a frente num anel com sinais de dois sentidos (o teste só verificava que não colidiam) — sinais passaram a ser de sentido único e o teste exige agora que ambos cumpram o horário.
- Testes: 184/184 EditMode (6 novos em `TrainTests`); runtimes das 8 linguagens PASS; build + self-test PASS; Play Mode: anel com sinais, comboio de 3 vagões, programa JS com `configure` e `train()`.

## Sessão 5 (cont.): Etapa 8 — integração
- Auditoria: a barra de construção já mostra os edifícios novos à medida que a investigação os desbloqueia; os instantâneos de multijogador já levavam o estado dos edifícios, mas não os robôs em voo (linhas fora dos edifícios) nem as lâmpadas acesas; P11 (ids renumerados) e P12 (ações perdidas) continuavam abertos.
- Feito: ids no save e nos instantâneos; ações em curso no save (as funções de conclusão saíram para métodos reutilizáveis); robôs e lâmpadas para os convidados; `IntegrationTests` com uma base mista de todos os sistemas.
- Bug antigo encontrado: a ecologia não guardava onde caíam os passos de 1 s (nem invalidava a cache de árvores), por isso guardar e carregar com máquinas a poluir dava outro mundo. Corrigido (`ecoclock`, cache invalidada por `DepositChanged`).
- Testes: 189/189 EditMode; build + self-test PASS; Play Mode: guardar/carregar pelo jogo mantém `m2` (com um id livre antes) e a fundição a meio termina depois de carregar.

## Resumo da missão "Evoluir o Codebound" (Etapas 0–8, 2026-11-24)
- 135 → 189 testes EditMode, todos a passar; runtimes das 8 linguagens a passar; build Windows + self-test PASS em cada etapa; cada etapa verificada em Play Mode.
- Novos ficheiros do núcleo: `Simulation.cs`, `Logistics.cs`, `Fluids.cs`, `Power.cs`, `Signals.cs`, `Robots.cs`, `Trains.cs`. Nada foi removido do jogo; saves antigos continuam a carregar (migrações testadas onde o formato mudou).
- Ver `INDUSTRY_PLAN.md` para o registo por etapa e as limitações conhecidas.

## Sessão 6 (2026-11-24): pedido "o que falta do Factorio" — Etapa 9, transporte subterrâneo
- Plano: 9 subterrâneos, 10 máquinas em várias versões, 11 módulos e beacons, 12 refinaria e química, 13 robôs de construção.
- Tapetes e tubos subterrâneos (ver `TECHNICAL_ARCHITECTURE.md`); tecnologia `underground`; arte; 3 textos × 10 línguas; `build_tables.py` passou a incluir as tabelas das etapas seguintes quando existirem.
- Testes: 194/194 EditMode (5 novos em `UndergroundTests`); build + self-test PASS.

## Sessão 6 (cont.): Etapa 10 — máquinas em várias versões
- 4 edifícios (perfuradora Mk2, montadoras Mk2/Mk3, fornalha elétrica), tecnologia `advanced_machines`, marcas de versão na arte, 5 textos × 10 línguas.
- Os testes de velocidade mostraram dois efeitos reais a ter em conta: uma máquina mais rápida gasta mais energia (a Mk2 com um só gerador de 150 kW trabalha a 83 %), e cada peça leva um número inteiro de ticks (1/1,5 s = 13,3 → 14 ticks).
- Testes: 199/199 EditMode (5 novos em `TierTests`); build + self-test PASS.

## Sessão 6 (cont.): Etapa 11 — módulos e beacons
- `Core/Modules.cs` (novo), 3 itens com receitas, beacon, tecnologia `modules`, 8 textos × 10 línguas; ranhuras nas máquinas da Etapa 10, perfuradora e laboratório.
- Testes: 204/204 EditMode (5 novos em `ModuleTests`: um por ranhura e reembolso, velocidade/energia, eficiência com mínimo de 20 %, produtividade 50 × 16 % = 8 grátis, beacons a 50 % só com energia e sem produtividade, saves); build + self-test PASS.

## Sessão 6 (cont.): Etapa 12 — refinaria e química
- `Core/Chemistry.cs` (novo): refinaria, fábrica química com 4 receitas; 2 fluidos, 4 itens, receita do circuito avançado; custos da Mk3 e dos módulos com circuitos avançados; tecnologia `chemistry`; arte; 13 textos × 10 línguas; `start/stop/configure` nas 8 linguagens (C# recompilado).
- Bug apanhado pelo teste de save: as quantidades e o progresso eram lidos antes da receita (ordem alfabética das chaves) e perdiam-se. Corrigido com valores pendentes.
- Testes: 209/209 EditMode (5 novos em `ChemistryTests`); runtimes das 8 linguagens PASS; build + self-test PASS.

## Sessão 6 (cont.): Etapa 13 — robôs de construção
- `Core/Construction.cs` (novo): planos, marcas de demolição, robôs de construção com recolha em várias paragens, reservas, regresso, saves; controlos com Shift no `BuildController`; planos e robôs desenhados; edifícios marcados a vermelho; tecnologia `construction_robots`; 2 textos × 10 línguas.
- Duas regras acrescentadas antes dos testes para evitar ciclos sem fim: construir à mão por cima de um plano apaga-o, e um plano bloqueado não envia robôs.
- Testes: 214/214 EditMode (5 novos em `ConstructionTests`); build + self-test PASS; Play Mode com as Etapas 9–13 juntas (túnel, Mk3 com módulo e beacon a ×3,15, refinaria e fábrica química, dois baús construídos e um muro demolido por robôs).

## Resumo do pedido "mais Factorio" (Etapas 9–13)
- Tapetes e tubos subterrâneos, máquinas em várias versões, módulos e beacons, refinaria e química, robôs de construção. 189 → 214 testes, todos a passar; runtimes das 8 linguagens a passar; build + self-test PASS em cada etapa.

## Sessão 7: contas, social e serviços online
- Auditoria do multiplayer (identidade só por nome = usurpação possível) e comparação de backends em `ONLINE_PLAN.md`; escolhido um serviço próprio em ASP.NET Core 8 + SQLite por ser o único testável aqui sem contas nem custos.
- Serviço com migrações, limites de pedidos, registo JSON, cópias de segurança online e manutenção (retenção de 30 dias). 28 testes de integração com relógio controlável.
- Cliente sem motor em `Core/Online` e integração em `Game/Online.cs`, `AccountMenu.cs`, `NetSession.cs`, `TitleMenu.cs`, `OptionsMenu.cs`; ganchos de estatísticas (tempo, distância, edifícios, mineração, itens, programas e linguagens, investigação, lançamentos, mundos).
- Bugs apanhados pelos testes: `Refresh()` guardava uma tarefa já terminada quando o transporte respondia de forma síncrona (o `finally` corria antes da atribuição) — a sessão revogada nunca terminava; um 429 durante a renovação terminaria a sessão (agora só 400/401). No Play Mode: as opções de perfil apareciam sem caixa de seleção.
- Nota para testes por `script eval`: não bloquear (`GetResult`) métodos `async` do jogo — retomam no thread principal e o Editor bloqueia; os métodos de `OnlineClient` podem ser bloqueados (usam `ConfigureAwait(false)`).
- Resultado: serviço 34/34, Unity 223/223, build + self-test PASS; Play Mode contra o serviço local.
