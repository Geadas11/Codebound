# TASKS

Legenda: ✅ feito e verificado · 🟡 feito, verificação parcial · ⬜ pendente

## Etapa 1 — Fundação ✅
- ✅ Projeto Unity 6000.6.10f1, template Universal 2D, Input System
- ✅ Assemblies `Codebound.Core` (sem Unity), `Codebound.Game`, `Codebound.Tests.EditMode`
- ✅ Inventário (stacks, operações atómicas) — testado
- ✅ Grelha de construção (multi-célula, rotação, obstáculos, regra por tipo de edifício) — testado
- ✅ Mundo procedural por seed (5 biomas, 9 terrenos, 10 recursos), chunks infinitos — testado + Play Mode
- ✅ Jogador top-down, câmara, mineração manual — Play Mode
- 🟡 Modo construção: lógica testada; pré-visualização com o rato não verificada por mim
- ✅ Interação E: painel de máquina com transferência de itens (take/put) — 🟡 cliques não testados por mim
- ✅ Guardar/carregar (F5/F9): formato versionado, escrita atómica, deteção de corrupção — testado + Play Mode
- 🟡 HUD IMGUI de desenvolvimento — temporário (Etapa 4)

## Etapa 2 — Primeiras máquinas ✅ (base)
- ✅ Fornalha: minério→placa (ferro, cobre, pedra→tijolo), combustível carvão — testado
- ✅ Gerador a carvão + postes elétricos + redes + satisfação de energia — testado + Play Mode
- ✅ Perfuradora elétrica sobre jazidas; só trabalha comandada por programa — testado + Play Mode
- ✅ Computador básico (CPU calls/s, RAM, armazenamento, energia)
- ✅ Saída para o edifício na frente (rotação)
- ✅ Correia (básica, passiva) e transportador programável (`start/stop/setFilter`) — testado + Play Mode
- ✅ Braço/inserter (tecla 0): tira 1 item de trás (baú, saída de fornalha/perfuradora, correia) e põe à frente; `start/stop/setFilter` por programa — testado + Play Mode
- ⬜ Correia inteligente (filtro sem programa), splitters, braços de longo alcance
- ⬜ Gráficos de estado das máquinas (animação a trabalhar, sem energia, etc.)

## Etapa 3 — Programação funcional (em progresso)
- ✅ 1.ª linguagem real: **JavaScript** (Node.js, MIT) em processo separado
- ✅ Isolamento: processo + permission model + worker com heap limitado + vm sem require/eval + watchdog CPU — 8 testes de integração
- ✅ API `codebound`: `log/print`, `sleep`, `computer.on/time/name`, `energy.available/stats`, `machines.list/get/find`, `drill.mine`, `furnace.smelt`, `.status()`, `.contents()`
- ✅ Execução contínua (chamadas bloqueantes em tempo de jogo) + por eventos (`tick`, `item_mined`, `item_smelted`, `power_lost`, `power_restored`)
- ✅ Limite de CPU (chamadas/s do hardware) e RAM (heap do worker), interrupção segura (Stop/terminate)
- ✅ Editor de código no jogo (IMGUI): números de linha, Run/Stop, exemplos, consola, erros com linha, dicas contextuais, monitor CPU/heap/calls, máquinas em alcance, referência da API — Play Mode (captura)
- ✅ Debugger: breakpoints (clicar no nº de linha), Continue (F8), Step (F10), variáveis em escopo; números de linha preservados — testado (runtime) + Play Mode
- ⬜ Debugger: pilha de chamadas, step over/out, watch expressions
- ⬜ Explorador de ficheiros (vários ficheiros por computador), terminal
- ⬜ Histórico de execução, diagramas visuais
- ⬜ Níveis de assistência (Guiado/Normal/Avançado) — hoje só existem dicas "guiadas" fixas
- ✅ Build Windows com Node incluído (`Codebound > Build Windows`), self-test do executável (`-selftest`) — PASS
- ⬜ Builds Linux/macOS (precisam do binário Node de cada plataforma: `CODEBOUND_NODE`)
- ✅ `LICENSE` do Node incluído no build; CPython incluído com o seu `LICENSE.txt`; ver `THIRD_PARTY_NOTICES.md`
- ⬜ Restrição de rede no runtime (Node ≥ 25 `--allow-net` ou QuickJS/Wasm)
- ✅ 2.ª linguagem: **TypeScript** (compilador TS 5.9.3, Apache-2.0, no host), erros mapeados para linhas TS via source map, debugger incluído — testado + Play Mode
- ✅ 3.ª linguagem: **Python** (CPython 3.15.2 embeddable, PSF): processo por programa, Job Object (RAM, sem processos filhos), audit hook PEP 578, imports permitidos, watchdog, debugger com `sys.settrace` — 12 testes de integração (10 tentativas de fuga bloqueadas) + Play Mode + build
- ✅ 4.ª linguagem **SQL** (SQLite): ciclos (`-- @every N`/`-- @once`), tabelas do jogo só de leitura, controlo por `commands`/`hud`, tabelas do jogador guardadas com o computador, authorizer + limite de 2 s por ciclo, debugger por instrução — 8/8 runtime + self-test do executável PASS
- ⬜ Restantes linguagens: PHP, C, C++, C#
- ⬜ Python em Linux/macOS (precisa de uma distribuição standalone por plataforma; limites via RLIMIT_AS não testados)

## Etapa 4 — Automação (em progresso)
- ✅ Transportadores e braços (ver Etapa 2)
- ✅ **HUD programável HTML/CSS** (UI Toolkit): botão "+" cria caixas; painéis movíveis (título), redimensionáveis (canto), minimizáveis, editáveis (E) e fecháveis; layout guardado no save — testado (Core) + Play Mode
- ✅ Subconjunto HTML (div p h1-h3 span b strong i em u small code ul ol li br hr progress) e CSS (tag/.class/#id, descendente, listas, especificidade, inline style; 30 propriedades) com erros por linha
- ✅ `{{chave}}` com dados ao vivo (jogador, energia, inventário, tempo, fps…) e `hud.set(chave, valor)` / `hud.notify(texto)` a partir de programas JS/TS/Python
- ✅ Editor de caixas HUD (HTML/CSS, Apply/Revert, problemas, valores disponíveis)
- 🟡 Cliques reais do rato no HUD: botões e arrasto verificados com eventos de ponteiro do UI Toolkit; EventSystem + InputSystemUIInputModule adicionado; clique físico não verificado por mim
- ✅ Menu de opções (Esc, pausa o jogo, navegável por teclado): modo pacífico, escala da interface (0,75–2×, IMGUI + HUD), tamanho de texto (caixas HUD, editor de código), alto contraste, paleta para daltonismo (azul/laranja em vez de verde/vermelho nos estados), reduzir flashes e partículas, remapear 12 ações, guardar/carregar/sair — Play Mode
- ✅ Áudio sintetizado em código (31 sons, sem ficheiros nem licenças de terceiros): máquinas com som por estado (a trabalhar / quebra de energia mais grave / bloqueada com bip de aviso / sem energia em silêncio), ambiente por bioma e clima, música adaptativa em 3 camadas (calma, indústria, perigo), efeitos (minerar, construir, comer, beber, disparo, dissolução, dano), alarmes programáveis `hud.alarm()` (JS/TS/Python; SQL `INSERT INTO hud VALUES('alarm', …)`), volumes separados e legendas de sons com direção — testes do sintetizador + Play Mode
- ⬜ Áudio espacial com HRTF/oclusão, música composta (Etapa 8), daltonismo completo (filtro de ecrã), idiomas
- ⬜ Interatividade nas caixas (botões HTML que chamam programas), imagens
- ✅ Fabricação: montadora programável (`craft`: engrenagem, fio de cobre, circuito; 60 kW) e **Computador Avançado** (60 chamadas/s, 64 MB, 256 KB) feito de circuitos — testes
- ✅ Barra de construção clicável em várias linhas (13 estruturas; teclas 1–9,0 para as 10 primeiras); cliques na barra não minam/constroem por baixo
- ⬜ Redes lógicas/servidores, armazenamento automático

## Etapa 5 — Mundo completo (em progresso)
- ✅ Sobrevivência: vida, fome, sede, oxigénio (tanque do fato), temperatura corporal; ambiente por bioma (vulcânico/radioativo sem ar respirável, radiação, calor perto de lava); comer fruta alienígena (F; as árvores dão às vezes), beber água (LMB sobre água; tóxica faz dano); morte → regressa ao local do acidente sem perder o inventário; guardado no save; no HUD (`{{player.health}}`…) — 6 testes + Play Mode
- ✅ Dia/noite (dia = 10 min), estações (5 dias cada), tempo regional determinístico (limpo, nublado, chuva, tempestade: de areia, de cinza, radioativa, trovoada) — afeta temperatura, ar respirável, radiação e sede (a chuva dá água); luz global 2D, camada de cor e partículas; opção de reduzir efeitos (`cb.reduceEffects`) — 5 testes + Play Mode
- ⬜ Abrigos/aquecimento, relâmpagos, tempo visível na tabela `game` do SQL e na API JS/Python
- ✅ Agricultura: estufa programável (4 canteiros; `plant/water/harvest/plots`; tanque enche-se com um lago a ≤ 4 células; cresce com luz do dia ou lâmpadas elétricas 30 kW; 1 fruta → 3) — testes + Play Mode (programa Python a cultivar)
- ⬜ Hidroponia/aeroponia, plantas alienígenas variadas, melhoramento genético, processamento alimentar, ecologia/poluição
- ✅ Investigação: árvore híbrida (ramos Indústria/Ciência/Computação + linha principal de 5 passos para reconstruir a nave), laboratório programável (`lab.research(id)`, `lab.techs()`; SQL `research`), estruturas bloqueadas até investigadas, painel T, notificações — 3 testes + Play Mode
- ⬜ Mais ramos (Energia, Robótica, Materiais, Defesa, Segurança, Exploração, Programação), competências individuais por uso
- ⬜ Lançamento da nave e fase espacial (Etapa 7) — hoje o último passo só avisa que chega numa atualização futura
- ✅ Criaturas e combate leve: "atenção" da base cresce com o ruído industrial (perfuradoras, geradores, fornalhas, montadoras, laboratórios) → ondas; base silenciosa fica em paz. Crawlers atacam máquinas ruidosas e o jogador próximo; drones hostis comprometem computadores (programa para; Run restaura do backup). Torre programável (`turret.targets()/fire(id)`, investigação Defense), evento `creature_detected`, tabela SQL `creatures` + comando `fire`; ataque do jogador (LMB, 12 de dano); estruturas com vida (parede 400), dano visível, destruição; sem violência gráfica (dissolvem-se) — 6 testes + Play Mode
- ⬜ Firewalls/palavras-passe/deteção de intrusões, armas e equipamento do jogador, minas/barreiras, reparação de estruturas, modo pacífico no menu

## Etapas 6–8
Ver Prompt Mestre §20. Não iniciadas.
