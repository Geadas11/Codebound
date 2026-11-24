# Codebound — Game Design (resumo vivo)

**Tagline:** BUILD. CODE. SURVIVE. EXPLORE.
Sobrevivência + automação industrial + exploração + **programação real**, top-down pixel art em grelha, Unity 2D, Windows/Linux/macOS, 12+/13+, coop até 8 jogadores.

A especificação completa e normativa é o "Prompt Mestre" fornecido pelo utilizador (secções 3–18). Este ficheiro regista as **decisões de design concretas** tomadas durante o desenvolvimento. Quando houver conflito, o Prompt Mestre prevalece.

## Pilares
1. **Código real controla o mundo** — HTML, CSS, JavaScript, SQL, Python, PHP, TypeScript, C, C++, C#. Sem pseudocódigo. API própria `codebound` exposta a cada linguagem.
2. **Fábrica em grelha** — máquinas multi-célula, rotação, pré-visualização, planeamento.
3. **Sobrevivência não punitiva** — vida, fome, sede, oxigénio, temperatura.
4. **Exploração** — mundo procedural infinito com seed partilhável; 5 biomas.
5. **Sem NPCs/civilizações ativas** — narrativa por registos, ruínas, terminais.

## Decisões concretas (até agora)
| Tema | Decisão |
|---|---|
| Escala | 1 célula = 1 unidade Unity = 16×16 px (PPU 16). |
| Mundo | Chunks de 32×32 células, gerados por seed (int), carregados num raio à volta da câmara. |
| Biomas | Floresta, Deserto, Vulcânico, Ruínas tecnológicas, Radioativo — escolhidos por ruído de temperatura/humidade + "estranheza". |
| Terreno | Água e lava são intransponíveis; resto caminhável. |
| Recursos (Etapa 1) | Jazidas finitas por célula: ferro, cobre, carvão, pedra, silício, titânio, urânio, petróleo, cristal alienígena + árvores (madeira). Distribuição depende do bioma. |
| Mineração manual | Manter botão esquerdo sobre jazida ao alcance (≤ 3 células). |
| Construção | Modo construção (B), seleção 1–9, rodar R, colocar clique esquerdo, remover clique direito (reembolso 100%). Custos verificados contra o inventário. |
| Máquinas | Regra do Prompt aplicada às **máquinas de produção** (perfuradora, fornalha): só trabalham quando um programa num computador as comanda. Infraestrutura (gerador, postes, baús, paredes) é passiva. |
| Computador | Comanda máquinas num raio de 8 células. Basic Computer: 20 chamadas/s, 32 MB RAM, 64 KB de código, 20 kW. Sem energia o programa congela. |
| Energia | Gerador a carvão 150 kW (1 carvão = 4000 kJ). Postes ligam-se a 9 células e alimentam num raio de 4,5. Falta de energia abranda as máquinas proporcionalmente. |
| Perfuradora | 2×2, tem de cobrir minério (não árvores). `mine()` = 1 unidade/s a 90 kW. Empurra o produto para o edifício na frente. |
| Fornalha | `smelt()` = 3,2 s por unidade, queima carvão. Ferro→placa, cobre→placa, pedra→tijolo. |
| Primeira linguagem | JavaScript. Chamadas bloqueantes em tempo de jogo; eventos via `computer.on`. Erros com número de linha e dicas que explicam o problema sem dar a solução. |
| Início | 6 silício salvado da nave (permite o 1.º computador). |
| Transportadores | Correias com **duas pistas** (até 4 itens por pista em cada célula); curvas mantêm a pista, uma correia que entra de lado numa linha enche a pista desse lado; braços levam o item na mão e esperam com ele se o destino estiver cheio. Correia (1 placa de ferro): passiva, sempre a mover. Transportador programável (ferro + cobre): parado até um programa chamar `start()`, aceita `setFilter(item)`. Ambos caminháveis. |
| Braço (inserter) | 2 placas de ferro + 1 de cobre. Tira 1 item a cada 0,8 s do edifício de trás e põe no da frente. Passivo por defeito; programas podem `stop()/start()/setFilter()`. Pega no primeiro item que encontra — usar filtros para escolher (ex.: só carvão para o combustível). |
| Estufa | 2×2 (madeira, pedra, silício). 4 canteiros; um programa planta (gasta 1 fruta), rega (do tanque, que enche junto a um lago) e colhe (3 frutas). Cresce em ~3 min com luz total; à noite só com lâmpadas (30 kW). |
| Montadora | 2×2. `craft(item)`: engrenagem (1 s), fio de cobre ×2 (0,5 s), circuito (3 s). 60 kW. |
| Computador Avançado | Feito de circuitos, engrenagens e fio: 60 chamadas/s, 64 MB RAM, 256 KB. |
| Investigação | Laboratório programável consome materiais. Início: parede, baú, fornalha, perfuradora, computador, gerador, poste, laboratório. Logistics → correias/transportador/braço; Agriculture → estufa; Manufacturing → montadora; Advanced Computing → Computador Avançado. Linha principal (5 passos) reconstrói a nave. |
| Ameaças | A indústria faz ruído; muito ruído atrai ondas de crawlers (atacam máquinas ruidosas e o jogador próximo) ou drones hostis (comprometem computadores). Bases silenciosas ficam em paz. Defesa: torres programáveis (`targets`/`fire`), paredes (400 de vida) e a multiferramenta do jogador. Sem sangue: as criaturas dissolvem-se. |
| Áudio | Tudo sintetizado. Cada máquina soa diferente e muda com o estado (trabalhar, falta de energia mais grave, bloqueada com bip a cada 4 s). Música em camadas: calma, indústria (sobe com as máquinas), perigo (criaturas perto ou avisos). `hud.alarm(texto)` toca um alarme programável. |
| Petróleo | Não é extraído por perfuradoras — precisará de bombas (Etapa 4/5). |
| Linguagens | JavaScript, TypeScript e Python disponíveis; botão "Language >" no editor. Python usa a mesma API em snake_case, `print()` vai para a consola, imports limitados a módulos de cálculo (math, random, json, collections…). |
| Debugger | Clicar no número da linha = breakpoint; **Debug** corre com instrumentação; F8 continuar, F10 passo. Variáveis visíveis no painel lateral. |
| HUD | Caixas HTML/CSS criadas com "+"; movíveis, redimensionáveis, minimizáveis; `{{chave}}` mostra dados ao vivo; computadores publicam valores com `hud.set` e alertas com `hud.notify`. Novo jogo traz uma caixa "Status" para o jogador começar a aprender HTML/CSS. |
| Tempo e clima | Dia de 10 min, estações de 5 dias. Deserto: dias quentes, noites frias. Tempestades de areia/cinza obrigam a usar o ar do fato; tempestade radioativa triplica a radiação; chuva repõe um pouco de água. O tempo é regional (depende do bioma) e igual para todos os jogadores. |
| Sobrevivência | Vida, fome (40 min), sede (25 min), oxigénio do fato (200 s fora de ar respirável: vulcânico e radioativo), temperatura corporal (o fato amortece o calor do deserto; lava é perigosa). Regenera quando bem alimentado. Morrer devolve-te ao local do acidente com o inventário intacto. |
| SQL | Programas correm em ciclos sobre tabelas do jogo; agem inserindo em `commands`/`hud`; as tabelas do jogador ficam guardadas no computador. |
| Inventário | 40 slots, stacks por item (definidos no ItemDatabase). |

## Controlos (Etapa 1)
WASD/setas mover · rato apontar · LMB minerar/colocar · RMB remover · B modo construção · R rodar · 1–9 escolher estrutura · Esc/Q sair do modo · E abrir máquina/computador · Tab inventário · roda do rato zoom · F5 guardar · F9 carregar.
