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
