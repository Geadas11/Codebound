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

## Etapa 2 — Primeiras máquinas ✅
- ✅ Fornalha: minério→placa (ferro, cobre, pedra→tijolo), combustível carvão — testado
- ✅ Gerador a carvão + postes elétricos + redes + satisfação de energia — testado + Play Mode
- ✅ Perfuradora elétrica sobre jazidas; só trabalha comandada por programa — testado + Play Mode
- ✅ Computador básico (CPU calls/s, RAM, armazenamento, energia)
- ✅ Saída para o edifício na frente (rotação)
- ✅ Correia (básica, passiva) e transportador programável (`start/stop/setFilter`) — testado + Play Mode
- ✅ Braço/inserter (tecla 0): tira 1 item de trás (baú, saída de fornalha/perfuradora, correia) e põe à frente; `start/stop/setFilter` por programa — testado + Play Mode
- ✅ Correia separadora (o item escolhido no painel E segue em frente, os outros viram à direita — sem programa), divisor (alterna frente/direita, usa o outro lado se um estiver cheio), braço longo (2 células); investigação Robotics — 3 testes
- ✅ Estado das máquinas no mapa: LED verde (a trabalhar, com pulsação), amarelo a piscar (bloqueada/cheia/sem combustível), vermelho a piscar (sem energia); `Factory.Activity()`; respeita "reduzir efeitos" e a paleta daltónica — 1 teste + Play Mode

## Etapa 3 — Programação funcional (completa, exceto builds Linux/macOS)
- ✅ 1.ª linguagem real: **JavaScript** (Node.js, MIT) em processo separado
- ✅ Isolamento: processo + permission model + worker com heap limitado + vm sem require/eval + watchdog CPU — 8 testes de integração
- ✅ API `codebound`: `log/print`, `sleep`, `computer.on/time/name`, `energy.available/stats`, `machines.list/get/find`, `drill.mine`, `furnace.smelt`, `.status()`, `.contents()`
- ✅ Execução contínua (chamadas bloqueantes em tempo de jogo) + por eventos (`tick`, `item_mined`, `item_smelted`, `power_lost`, `power_restored`)
- ✅ Limite de CPU (chamadas/s do hardware) e RAM (heap do worker), interrupção segura (Stop/terminate)
- ✅ Editor de código no jogo (IMGUI): números de linha, Run/Stop, exemplos, consola, erros com linha, dicas contextuais, monitor CPU/heap/calls, máquinas em alcance, referência da API — Play Mode (captura)
- ✅ Debugger: breakpoints (clicar no nº de linha), Continue (F8), Step (F10), variáveis em escopo; números de linha preservados — testado (runtime) + Play Mode
- ✅ Debugger completo em JS/TS, Python, PHP e C#: pilha de chamadas, Continue (F8), Step into (F11), Step over (F10), Step out (Shift+F11), watches por caminho (`items[0].count` — sem eval); o separador Debug abre sozinho ao parar — testes de runtime nas 4 + Play Mode (captura `Screenshots/etapa3_debug.png`). C/C++: sem passo a passo (WebAssembly), mas erros em tempo de execução dão ficheiro + linha (DWARF)
- ✅ Vários ficheiros por computador (até 8 + main; separadores no editor e separador Files): `require("x")` em JS, `import` em TS/Python, PHP carrega-os antes do main, C# junta as classes, C/C++ compila todos os .c/.cpp e permite `#include "x.h"` só dos ficheiros do computador; erros indicam o ficheiro; guardados no save
- ✅ Terminal (separador Terminal): `help ls cat new rm run debug stop status lang watch unwatch clear`; outra linha corre como snippet na linguagem do computador sem alterar o programa (não em C/C++) — testes EditMode + Play Mode
- ✅ Histórico de execução (últimas 20: hora, linguagem, resultado, duração, chamadas; guardado) e diagrama (computador + máquinas ao alcance na direção real, linhas mais fortes com mais chamadas) — Play Mode (capturas `etapa3_history.png`, `etapa3_diagram.png`)
- ✅ Níveis de assistência (Opções → Ajuda na programação): Guiado (próximo passo + dicas + exemplos), Normal (dicas + exemplos), Avançado (só o erro); dicas por linguagem (`Core/Assist.cs`)
- ✅ Build Windows com Node incluído (`Codebound > Build Windows`), self-test do executável (`-selftest`) — PASS
- ⬜ Builds Linux/macOS (precisam do binário Node de cada plataforma: `CODEBOUND_NODE`)
- ✅ `LICENSE` do Node incluído no build; CPython incluído com o seu `LICENSE.txt`; ver `THIRD_PARTY_NOTICES.md`
- ✅ Restrição de rede no runtime JS: o realm do programa não tem `fetch`/`WebSocket`/`XMLHttpRequest`/`process`, `require` só carrega ficheiros do computador e a compilação de WebAssembly pelo programa está bloqueada — testado
- ✅ 2.ª linguagem: **TypeScript** (compilador TS 5.9.3, Apache-2.0, no host), erros mapeados para linhas TS via source map, debugger incluído — testado + Play Mode
- ✅ 3.ª linguagem: **Python** (CPython 3.15.2 embeddable, PSF): processo por programa, Job Object (RAM, sem processos filhos), audit hook PEP 578, imports permitidos, watchdog, debugger com `sys.settrace` — 12 testes de integração (10 tentativas de fuga bloqueadas) + Play Mode + build
- ✅ 4.ª linguagem **SQL** (SQLite): ciclos (`-- @every N`/`-- @once`), tabelas do jogo só de leitura, controlo por `commands`/`hud`, tabelas do jogador guardadas com o computador, authorizer + limite de 2 s por ciclo, debugger por instrução — 8/8 runtime + self-test do executável PASS
- ✅ Restantes linguagens: PHP, C, C++, C# (ver "Linguagens")
- ⬜ Python em Linux/macOS (precisa de uma distribuição standalone por plataforma; limites via RLIMIT_AS não testados)

## Etapa 4 — Automação ✅
- ✅ Transportadores e braços (ver Etapa 2)
- ✅ **HUD programável HTML/CSS** (UI Toolkit): botão "+" cria caixas; painéis movíveis (título), redimensionáveis (canto), minimizáveis, editáveis (E) e fecháveis; layout guardado no save — testado (Core) + Play Mode
- ✅ Subconjunto HTML (div p h1-h3 span b strong i em u small code ul ol li br hr progress) e CSS (tag/.class/#id, descendente, listas, especificidade, inline style; 30 propriedades) com erros por linha
- ✅ `{{chave}}` com dados ao vivo (jogador, energia, inventário, tempo, fps…) e `hud.set(chave, valor)` / `hud.notify(texto)` a partir de programas JS/TS/Python
- ✅ Editor de caixas HUD (HTML/CSS, Apply/Revert, problemas, valores disponíveis)
- 🟡 Cliques reais do rato no HUD: botões e arrasto verificados com eventos de ponteiro do UI Toolkit; EventSystem + InputSystemUIInputModule adicionado; clique físico não verificado por mim
- ✅ Menu de opções (Esc, pausa o jogo, navegável por teclado): modo pacífico, escala da interface (0,75–2×, IMGUI + HUD), tamanho de texto (caixas HUD, editor de código), alto contraste, paleta para daltonismo (azul/laranja em vez de verde/vermelho nos estados), reduzir flashes e partículas, remapear 12 ações, guardar/carregar/sair — Play Mode
- ✅ Áudio sintetizado em código (31 sons, sem ficheiros nem licenças de terceiros): máquinas com som por estado (a trabalhar / quebra de energia mais grave / bloqueada com bip de aviso / sem energia em silêncio), ambiente por bioma e clima, música adaptativa em 3 camadas (calma, indústria, perigo), efeitos (minerar, construir, comer, beber, disparo, dissolução, dano), alarmes programáveis `hud.alarm()` (JS/TS/Python; SQL `INSERT INTO hud VALUES('alarm', …)`), volumes separados e legendas de sons com direção — testes do sintetizador + Play Mode
- ✅ Áudio espacial: estéreo + distância + oclusão (cada parede entre a fonte e o jogador baixa o volume e abafa o som, filtro passa-baixo nas máquinas) — HRTF verdadeiro precisaria de um plugin de espacialização, não incluído
- ✅ Filtro de ecrã para daltonismo (deuteranopia, protanopia, tritanopia) no menu Esc, por Channel Mixer do URP; matrizes testadas contra simulação (a primeira tentativa "daltonize" em RGB piorava a distinção — corrigida) — 1 teste
- ✅ Idiomas (ver Localização)
- ✅ `<button data-event="x">` nas caixas → evento `hud_button` {name, panel} em todos os programas; `<img src="item:...">` / `src="building:..."` com os ícones do jogo (sem ficheiros/URLs) — Play Mode: clique chegou ao programa JS
- ✅ Fabricação: montadora programável (`craft`: engrenagem, fio de cobre, circuito; 60 kW) e **Computador Avançado** (60 chamadas/s, 64 MB, 256 KB) feito de circuitos — testes
- ✅ Barra de construção clicável em várias linhas (13 estruturas; teclas 1–9,0 para as 10 primeiras); cliques na barra não minam/constroem por baixo
- ✅ Redes lógicas: `computer.send(canal, dados)` → evento `message` nos outros computadores (JS/TS/Python/PHP/C#/C); Servidor de dados (chave-valor partilhado, 256 chaves, guardado; `get/set/keys/delete`); Hub de armazenamento (baús a ≤6 células funcionam como um: entra para o baú que já tem o item, braços tiram dos baús; `totals/find`); investigação Networking — 2 testes

## Etapa 5 — Mundo completo ✅
- ✅ Sobrevivência: vida, fome, sede, oxigénio (tanque do fato), temperatura corporal; ambiente por bioma (vulcânico/radioativo sem ar respirável, radiação, calor perto de lava); comer fruta alienígena (F; as árvores dão às vezes), beber água (LMB sobre água; tóxica faz dano); morte → regressa ao local do acidente sem perder o inventário; guardado no save; no HUD (`{{player.health}}`…) — 6 testes + Play Mode
- ✅ Dia/noite (dia = 10 min), estações (5 dias cada), tempo regional determinístico (limpo, nublado, chuva, tempestade: de areia, de cinza, radioativa, trovoada) — afeta temperatura, ar respirável, radiação e sede (a chuva dá água); luz global 2D, camada de cor e partículas; opção de reduzir efeitos (`cb.reduceEffects`) — 5 testes + Play Mode
- ✅ Aquecedor (21 °C num raio de 5 células, 25 kW), abrigo (paredes nas 4 direções a ≤4 células: a tempestade fica lá fora), relâmpagos nas trovoadas (dano a edifícios e ao jogador ao lado; clarão e trovão com legenda), para-raios (apanha a ≤8 células e carrega baterias); `world.weather()` em todas as linguagens e colunas `weather/temperature/day/daylight` na tabela `game` (SQL); investigação Climate Control — 2 testes
- ✅ Agricultura: estufa programável (4 canteiros; `plant/water/harvest/plots`; tanque enche-se com um lago a ≤ 4 células; cresce com luz do dia ou lâmpadas elétricas 30 kW; 1 fruta → 3) — testes + Play Mode (programa Python a cultivar)
- ✅ 3 espécies (fruta, glowcap — cresce no escuro e cura —, erva-fibra) com sementes das árvores por bioma e das ruínas; `plant(plot, species)`; genética: cada colheita muta o vigor ±15% e a estufa guarda a melhor estirpe (vigor acelera e aumenta a colheita; `plots()` mostra `vigor` e `strains`); Baía hidropónica (só lâmpadas, metade da água, 1,5× mais rápida); rações na montadora (2 fruta + 1 glowcap); F come o melhor alimento — 4 testes. (Aeroponia: não separada da hidroponia.)
- ✅ Ecologia: poluição por regiões de 16×16 (gerador a carvão ∝ carga, fornalha, perfuradora, montadora), espalha-se às regiões vizinhas, desaparece aos poucos e mais depressa com árvores; purificador de ar (30 kW, investigação Ecology após Agriculture); smog ≥ 120 tira o ar respirável e mostra neblina; estufas crescem até 60% mais devagar; poluição aumenta a atenção das criaturas; `{{pollution.here}}`/`{{pollution.total}}`, `world.pollution()` (JS/TS/Python), coluna `game.pollution` (SQL); guardada no save — 3 testes + Play Mode
- ✅ Exploração: postos abandonados da expedição Meridian nas Ruínas (determinísticos por região de 48×48): terminal com registos em ordem de história (8 registos) e caixa de mantimentos (uma vez); tecla J = diário + scanner com distância/direção do posto por ler mais próximo (240 células); terreno dos postos não é construível; guardado no save — 3 testes + Play Mode
- ✅ Subsolo: entradas de caverna à superfície (determinísticas, não construíveis), E desce para uma camada subterrânea por mundo (túneis entre paredes de rocha, escuro, 12 °C, sem tempo, minério em dobro, cristais/titânio/urânio; lanterna do fato), escada para subir, cada camada guardada à parte; não se lança a nave do subsolo; o scanner (J) mostra a entrada mais próxima — 3 testes + Play Mode
- ✅ Tipos de ruínas: posto (terminal + caixa), destroços (só caixa, sucata), retransmissor (só terminal) — 1 teste
- ✅ Registos da história traduzidos nas 10 línguas
- ✅ Investigação: árvore híbrida (ramos Indústria/Ciência/Computação + linha principal de 5 passos para reconstruir a nave), laboratório programável (`lab.research(id)`, `lab.techs()`; SQL `research`), estruturas bloqueadas até investigadas, painel T, notificações — 3 testes + Play Mode
- ✅ Ramos novos: Robotics, Networking, Programming (+50% chamadas/s), Climate Control, Hydroponics, Materials, Exploration (scanner 2,5×) — além de Energy, Ecology, Cybersecurity, Defense
- ✅ Competências por uso: mineração (+6%/nível de velocidade), combate (+6% dano), engenharia (+10% por reparação); nível = √(xp/20) até 10, notificação ao subir, mostradas no inventário, guardadas e levadas entre mundos — 1 teste
- ✅ Criaturas e combate leve: "atenção" da base cresce com o ruído industrial (perfuradoras, geradores, fornalhas, montadoras, laboratórios) → ondas; base silenciosa fica em paz. Crawlers atacam máquinas ruidosas e o jogador próximo; drones hostis comprometem computadores (programa para; Run restaura do backup). Torre programável (`turret.targets()/fire(id)`, investigação Defense), evento `creature_detected`, tabela SQL `creatures` + comando `fire`; ataque do jogador (LMB, 12 de dano); estruturas com vida (parede 400), dano visível, destruição; sem violência gráfica (dissolvem-se) — 6 testes + Play Mode
- ✅ Segurança informática: investigação Cybersecurity (firewall: drones demoram 3×), palavra-passe por computador (PBKDF2 com sal; dobra o tempo de invasão e tranca o editor), evento `intrusion` {computer, drone, seconds} para programas a ≤ 12 células, alarme e notificação — 3 testes + Play Mode
- ✅ Reparação: manter LMB sobre estrutura danificada (+20 HP por pedra)
- ✅ Energia renovável: painel solar 2×2 (40 kW × luz do dia × clima), bateria 1×1 (carrega só com sobra solar, descarrega quando falta), investigação Energy; ordem de fornecimento solar → carvão → bateria; `{{power.storedKJ}}`, `energy.stats().storedKJ`; carga guardada no save — 3 testes
- ✅ Localização: 10 línguas (en, pt-PT, pt-BR, es, fr, de, it, ja, zh-Hans, ko), 115 textos (menu, teclas, HUD fixo, investigação, notificações, estruturas, itens, tecnologias), seletor no menu Esc, idioma do SO por omissão, fonte CJK do sistema; sintaxe e API de programação nunca traduzidas — 2 testes + Play Mode (ja, pt-PT)
- ✅ Localização das descrições das tecnologias, registos e de todos os textos novos (230 chaves × 10 línguas)
- ✅ Localização completa do painel de máquinas, editor de código (separadores, depurador, terminal, histórico, diagrama), dicas por linguagem, modo Guiado, estados das máquinas (só no ecrã: os programas continuam a ver os códigos em inglês) e legendas de som — 468 chaves × 10 línguas (`Tools/localization/tables_etapa5.py`), placeholders verificados em todas. Ficam em inglês: nomes dos exemplos, referência da API e código
- ✅ Equipamento (funciona enquanto está no inventário, feito na montadora): multiferramenta laser (dano 25 e mineração 2×), placas de blindagem (metade do dano das criaturas), forro térmico (o fato fica a meio caminho dos 22 °C); armadilha de choque (pisável, 30 de dano + 3 s atordoado, 3 cargas, recarrega com fio de cobre — sem violência gráfica); muro reforçado (1000 HP); investigação Materials — 2 testes
- ✅ Permissões multijogador: dono de cada edifício (guardado); construtores só removem o que construíram; novo papel administrador; painel N alterna visitante → construtor → administrador — 1 teste

## Etapa 6 — Multijogador ✅
- ✅ LAN cooperativo com anfitrião autoritário (TCP, porta 47820, JSON por linha): painel N (nome, IP, abrir/entrar/desligar), `-host` / `-join <ip>` / `-name <n>` na linha de comandos
- ✅ O anfitrião simula tudo (máquinas, programas, criaturas, poluição) e envia instantâneos 2×/s (save sem segredos: computadores com palavra-passe vão sem hash, código nem base de dados SQL); o convidado aplica-os no lugar (edifícios iguais não são recriados) e não simula
- ✅ Ações do convidado validadas no anfitrião (`Multiplayer.Act`): construir, remover, minerar (limite 4/s), tirar/pôr itens, reparar, atacar, explorar, comer; alcance de 6 células, materiais, investigação, papel
- ✅ Inventário próprio por convidado, guardado com o mundo (`ginv`)
- ✅ Permissões: construtor / visitante (só anda e luta), expulsar; projetos partilhados: o convidado edita e corre/para programas no computador do anfitrião (sandbox do anfitrião; palavra-passe verificada no anfitrião)
- ✅ Outros jogadores visíveis com nome
- ✅ 3 testes + verificação real: build como anfitrião (batchmode) + Editor como convidado → mundo recebido, 2 pedras mineradas, muro construído e devolvido no instantâneo
- ✅ Convidados com o editor completo: Run/Debug/Stop, Continue/Into/Over/Out, breakpoints, watches, ficheiros, terminal e snippets correm no anfitrião; consola, depurador (linha, pilha, variáveis, watches), monitor e diagrama chegam 4×/s só para o computador aberto
- ✅ Computadores trancados: o convidado manda a palavra-passe uma vez (máx. 1 tentativa/s), o anfitrião verifica e passa a enviar o código e ficheiros só a esse convidado; o hash nunca sai do anfitrião
- ✅ Chat (Enter): texto limpo (sem tags nem caracteres de controlo, 200 caracteres), 2 mensagens/s por convidado, nomes com a cor do jogador
- ✅ Posição do convidado validada: orçamento de movimento (velocidade × 1,2 + 6 células de folga) e sem entrar em células sólidas; movimento impossível → o anfitrião devolve-o à última posição válida; voltar ao ponto de partida (respawn) é permitido
- ✅ Vitais e posição de cada convidado guardados no mundo (`gbody`); ao voltar, o convidado aparece onde e como estava
- ✅ Tráfego cifrado: troca de chaves Diffie-Hellman (2048 bits) por ligação, misturada com a palavra-passe do mundo (PBKDF2) — a palavra-passe nunca viaja e um impostor sem ela não consegue ler nem escrever; mensagens AES-256-CBC + HMAC-SHA256 com número de sequência (sem leitura, alteração, repetição nem troca de ordem). Só a troca de chaves vai em claro. Protocolo versão 3 — 3 testes + cliente de protocolo em C# (`Tests/nettest`, 14 verificações) + build como anfitrião e Editor como convidado (palavra-passe errada recusada com a mensagem certa)
- ✅ Internet: palavra-passe do mundo opcional, máx. 8 convidados, ligações sem "hello" fechadas em 10 s, endereço por IP ou nome; botão "Abrir à internet" pede ao router por UPnP o reencaminhamento da porta 47820 (e mostra o IP externo), retirado ao parar de hospedar ou sair do jogo; se falhar, explica como reencaminhar à mão
- ✅ 7 testes + verificação real: Editor como anfitrião + cliente de protocolo (13 verificações: palavra-passe do mundo, teletransporte corrigido, chat limpo, computador trancado/destrancado, programa e snippet do convidado com consola remota, vitais ao voltar) e Editor como convidado de um anfitrião simulado (depurador remoto e chat no ecrã, mensagens enviadas corretas)
- ⬜ Não testado: UPnP num router real (só o texto dos pedidos/respostas) — fica para o jogador experimentar

## Etapa 7 — Espaço ✅
- ✅ Plataforma de lançamento 3×3 (desbloqueada por Ship: Launch Sequence), combustível carvão = 4 / petróleo = 10 unidades, até 250
- ✅ Subida programada: `pad.launch()`, `pad.throttle(x)`, `pad.flight()` (JS/TS/Python; SQL `launch`/`throttle`/`travel`); física com viragem gravitacional automática, arrasto e aquecimento ∝ densidade·v³: a fundo sempre → escudo sobreaquece; demasiado tímido → sem combustível; falhar é seguro (cápsula volta de paraquedas)
- ✅ Sistema solar determinístico (4 mundos: casa, vulcânico, deserto, ruínas) com viés de biomas; `pad.planets()`, `pad.travel(id)` custa 20 + 30/UA; cada mundo guardado em `planet_<seed>_<id>.cbsave`; inventário, investigação, vitais, HUD e relógio viajam; nave aterra com o combustível restante
- ✅ Leitura do voo no ecrã, nave a subir com chama, chaves HUD `ship.*` e `planet.name`
- ✅ 3 testes + Play Mode: programa JS levou a nave à órbita (29 de sobra com 200), listou planetas e viajou para o mundo vulcânico
- ✅ Variável `pad` (JS/TS, Python, PHP `$pad`, C# `Pad`), como o jogo sempre anunciou — antes só existia `machines.find("launchpad")`
- ✅ Aterrar: `pad.land()` volta da órbita ao mesmo pad (10 de combustível); o porão (`cargo`) descarrega-se no painel do pad
- ✅ Asteroides: `pad.asteroids()` (5 por dia e por mundo, determinísticos) e `pad.mineAsteroid(id)` (combustível 3 + distância, guarda sempre 10 para aterrar; cada um uma vez por dia) → ferro, cobre, titânio, urânio, cristal, silício no porão
- ✅ Estações orbitais: um retransmissor Meridian acima de cada mundo; `pad.dock()` (5 de combustível) devolve o registo e um desafio de programação com dados (somar telemetria, contar linhas ERR, ordenar leituras, decifrar César); `pad.answer(valor)` certo → fragmento da chave da antena + despojos no porão
- ✅ Final: com os 4 fragmentos, `pad.transmit()` na órbita do mundo inicial desbloqueia a antena e chama os colonos → ecrã final traduzido com estatísticas; o jogo continua. Progresso da história (`story`) guardado e levado entre mundos
- ✅ Multijogador: a viagem do anfitrião (nave ou cavernas) leva os convidados — recarregam no novo mundo junto do anfitrião, com inventário e vitais; o porão viaja com a nave; os convidados não podem lançar viagens
- ✅ SQL: comandos `land`, `mine_asteroid`, `dock`, `answer`, `transmit` (sem leitura dos dados do desafio — usar outra linguagem para os retransmissores)
- ✅ 7 testes + Play Mode (programa JS: acoplar, resolver, minerar, transmitir → ecrã final) + cliente de rede: o convidado segue o anfitrião para o mundo vulcânico e aterra ao lado

## Etapa 8 — Polimento ✅ (exceto builds macOS/Linux)
- ✅ Ecrã inicial (Continuar = save mais recente entre quicksave e autosave, Novo jogo com seed opcional, Idioma, Sair), mundo em pausa por trás; não aparece com -selftest/-host/-join/-seed/batchmode
- ✅ Auto-save a cada 5 min em `autosave.cbsave` (separado do quicksave; nunca nos convidados)
- ✅ Desempenho: base de 2000 edifícios — tick da simulação 5,06 → ~1,0 ms (hubs de armazenamento e redes elétricas com procura espacial em vez de todos-contra-todos), tick depois de mexer na rede 28 → ~4 ms; no ecrã 24,5 → 96,6 FPS (as opções liam o registo do Windows por edifício e por frame — agora em cache). 2 testes de desempenho com orçamento
- ✅ Tutorial guiado (8 passos: madeira, pedra, perfuradora, eletricidade, computador, primeiro programa, mineração automática, investigação), verificado pelo estado do mundo, guardado, pode ser saltado; saves antigos começam com ele concluído; escondido nos convidados — 2 testes
- ✅ Arte final em código: terreno com manchas e detalhes (ervas, flores, ondulações, fendas, ondas), 8 variantes, bordas suaves entre tipos de chão e espuma na água; 3 variantes de árvores (folhosa, pinheiro, arbusto) e de cada minério; sombras dos edifícios; detalhe nos edifícios lisos (hub, servidor, depurador, aquecedor, para-raios, plataforma de lançamento, muro reforçado…)
- ✅ Animações: jogador a andar (4 frames), máquinas a trabalhar (perfuradora roda, ventoinha do gerador, engrenagem da montadora, chevrons das correias, chama da fornalha, bolhas do laboratório, LEDs do servidor, grelha do depurador, resistência do aquecedor), criaturas (patas, rotores); respeitam "reduzir efeitos" — frames distintos verificados
- ✅ Música composta (`Core/Music.cs`): peça de 16 compassos em lá menor a 96 BPM (secções A/B, ~40 s, loop sem costura), 4 camadas sincronizadas — dia (melodia, arpejo, pad, eco), noite (sinos, entra ao anoitecer), indústria (baixo, bombo, pratos), perigo (drone, tímpanos, cordas em trémulo); composta numa thread em segundo plano — 2 testes; WAVs em `Codebound/Screenshots/mus_*.wav`
- ✅ HUD traduzido: caixa de estado (tempo, estações, biomas, terrenos, recursos, avisos dos vitais) e a caixa "Estado" inicial do HUD programável
- ⬜ Builds macOS/Linux (precisam de módulos do Unity e do Node/Python de cada plataforma — downloads)

## Simulação industrial (missão 2026-11-24) — ver `INDUSTRY_PLAN.md`
- ✅ Etapa 0 — Auditoria: branch/estado confirmados, tag `pre-industry-2026-11-24`, 135/135 de base, problemas P1–P12 registados
- ✅ Etapa 1 — Fundação: `GameState.Step` + `Simulation` com 12 sistemas por ordem (prepare/update/finish, perfil por sistema); relógio e relâmpagos por tick (antes por frame); contador de ticks no save; edifícios por ordem de construção; listas por tipo; sleep/wake de correias e braços com resultado idêntico; menos texto criado por tick; `MachinesNear` espacial com desempate por id; hash de estado — 7 testes novos (142/142), build + self-test PASS, Play Mode: 171 ticks = 8,55 s de relógio
- ✅ Etapa 2 — Correias com 2 pistas (4 itens por pista, espaçamento, curvas, alimentação lateral, passo em dois tempos independente da ordem de construção; débito 1,9 → até 15 itens/s), braços com mão e posição (mesmo débito, só apanham o que o destino aceita, esperam com o item), migração de saves antigos, vista por pista e mão visível — 8 testes novos (150/150), build + self-test PASS, Play Mode verificado
- ✅ Etapa 3 — Fluidos: segmentos de tubos/depósitos (sem misturas), bomba, bomba de água, bomba de petróleo, caldeira, máquina a vapor (rede elétrica: sol → vapor → carvão → baterias), enchedora de bidões, estufas por tubo; 3 tecnologias, arte, 27 textos × 10 línguas, `contents().fluids` e `start/stop` nas 8 linguagens — 8 testes novos (158/158), runtimes JS/Python/PHP/SQL/C#/C PASS, build + self-test PASS, Play Mode com programa JS a controlar a bomba
- ✅ Etapa 4 — Energia: redes atualizadas em cada edição (igual à reconstrução, testado), prioridade para máquinas críticas, estatísticas por rede e por fonte (painel, `energy.network()` em 8 linguagens, colunas SQL), hubs sem varrimentos periódicos; edição na rede 15 → 0,4 ms/tick; limite do teste 25 → 5 ms — 6 testes novos (164/164), runtimes PASS, build + self-test PASS, Play Mode com JS e SQL
- ✅ Etapa 5 — Sinais: cabos, redes de sinais (itens, fluidos, A–Z/0–9), combinadores constante/aritmético/decisor (1 tick por combinador, ciclos bem definidos), condições em braços/correias/bombas/enchedoras/lâmpadas, lâmpada, uma sintaxe de texto para painel/programas/SQL/multijogador/save; `signals.read/send` e `configure` nas 8 linguagens — 7 testes novos (171/171), runtimes PASS, build + self-test PASS, Play Mode com JS e SQL
- ✅ Etapa 6 — Robôs logísticos: robôporto, redes logísticas, baús fornecedor/pedidos, hub como armazém, robôs a voar com carga (reservas, regresso, nada se perde), sinais (itens + R), saves dos robôs em voo, `network()`/`configure()` nas 8 linguagens — 7 testes novos (178/178), runtimes PASS, build + self-test PASS, Play Mode com Python
- ✅ Etapa 7 — Comboios: carris, sinais de sentido único com blocos, paragens como baías de carga, depósitos com locomotiva/vagões/carvão/carga e horários em texto, caminho sem inversões, seguro entre comboios, sinais de circuito, saves exatos, `configure`/`train()` nas 8 linguagens — 6 testes novos (184/184), runtimes PASS, build + self-test PASS, Play Mode com JS
- ✅ Etapa 8 — Integração: ids de máquinas guardados (P11), ações em curso guardadas (P12), bug de determinismo da ecologia corrigido, convidados veem robôs/lâmpadas/comboios com os ids do anfitrião, base mista com todos os sistemas (2280 edifícios, 0,8 ms/tick), equivalências dormir/guardar na base mista — 5 testes novos (189/189), build + self-test PASS, Play Mode (guardar/carregar pelo jogo mantém m-ids e termina a fundição)

## Linguagens
- ✅ PHP 8.5: um processo por programa, sem php.ini nem extensões, `open_basedir` numa pasta inexistente (nenhum ficheiro acessível), funções de processos/rede/ficheiros/ambiente/configuração removidas, `memory_limit` = RAM + Job Object (memória, sem processos filhos), vigia de CPU no Unity; API `$machines`, `$drill`, `$energy`, `$hud`, `$world`, `$computer->on()`, `sleep()` em tempo de jogo, `echo` → log — 24 testes de runtime
- ✅ C# 12: Roslyn num processo .NET 8 separado; o programa (instruções de topo, classes, LINQ, `JsonNode`) só pode usar uma lista branca de namespaces verificada símbolo a símbolo (sem ficheiros, reflexão, `GetType`/`typeof`, ambiente, consola, threads, `dynamic`, `unsafe`, `extern`, árvores de expressão); Job Object + limite de heap do GC + vigia de CPU; API `Drill.Mine()`, `Machines.Find()`, `Computer.On()`, `Sleep()`, `Log()` — 29 testes de runtime
- ✅ Self-test do build com 6 linguagens (JS, TS, Python, SQL, PHP, C#)
- ✅ C17 e C++17: o Unity verifica o código (`CSource`: só `#include <cabeçalhos do sistema>`, sem `__has_include`, `#embed`, `asm`/`.incbin` — o compilador corre no PC e podia ler ficheiros), compila com clang (wasi-sdk) para WebAssembly num Job Object com 20 s de limite, e o programa corre num worker do Node já incluído, com WASI próprio (sem sistema de ficheiros nem rede; `printf` → log; `sleep` em tempo de jogo) e memória criada pelo anfitrião com máximo = RAM; API em `<codebound.h>` (`cb_find`, `cb_op`, `cb_call`, `cb_num`, eventos) e `cb::Machine` em C++ — 12 testes de runtime + 15 testes do filtro
- ✅ Self-test do build com 8 linguagens (JS, TS, Python, SQL, PHP, C, C++, C#); HTML/CSS no HUD → as 10 linguagens do Prompt Mestre funcionam
- ✅ Depurador passo a passo para PHP e C#; linhas (e ficheiro) de erro em tempo de execução para C/C++

## Mais Factorio (pedido de 2026-11-24)
- ✅ Etapa 9 — Tapetes subterrâneos (até 5 células) e tubos subterrâneos (até 10), saída só aceita do túnel, tubos ao lado não ligam, saves — 5 testes (194/194), build + self-test PASS
- ✅ Etapa 10 — Perfuradora Mk2, montadoras Mk2/Mk3, fornalha elétrica; mais rápidas e com mais consumo; programas veem o tipo base — 5 testes (199/199), build + self-test PASS
- ✅ Etapa 11 — Módulos de velocidade/eficiência/produtividade, ranhuras por máquina, beacons a 50 % com energia, bónus de produtividade guardado — 5 testes (204/204), build + self-test PASS
- ✅ Etapa 12 — Refinaria (petróleo → gás), fábrica química (plástico, enxofre, ácido sulfúrico, células de bateria), circuitos avançados (Mk3 e módulos passam a usá-los) — 5 testes (209/209), runtimes PASS, build + self-test PASS
- ✅ Etapa 13 — Robôs de construção: planos com Shift+clique, demolição com Shift+clique direito, recolha de materiais em vários baús, reservas partilhadas, saves exatos — 5 testes (214/214), build + self-test PASS, Play Mode
