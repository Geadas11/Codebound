# Interface do Codebound: auditoria, arquitetura e redesign (missão de 2026-11-24)

## 1. Auditoria (antes)
- **Tecnologias.** Havia três tecnologias de interface:
  - IMGUI (`OnGUI`) em todos os menus e painéis: título, opções (Esc), máquinas e editor de código, investigação, rede (N), conta, tutorial, diário, espaço e final.
  - UI Toolkit só no HUD programável HTML/CSS (`HudLayer` + `HudRenderer`, com layout de `Core/Hud`).
  - uGUI: não era usado.
- **Aspeto.** Cada painel criava a sua textura 1×1 e as suas cores (verde #5ef0a0 sobre azul-escuro). Os botões eram os do Unity por defeito. A escala era fixa: em 1440p e 4K ficava tudo pequeno.
- **Ecrã inicial.** Uma coluna de 5 botões sobre o mundo em pausa.
- **Saves.** Só havia um slot: `quicksave.cbsave` e `autosave.cbsave`. Não existia o conceito de "mundos", nem perfis locais.
- **Opções.** Uma única lista. Faltavam gráficos, volume da interface e restauro de predefinições, e não havia deteção de conflitos de teclas.
- **Multijogador.** Só no painel N dentro do jogo. A porta e o limite de jogadores eram constantes, e não havia histórico nem favoritos.

## 2. Arquitetura escolhida e porquê
**Escolha: manter o IMGUI para menus e painéis, sob um *design system* próprio.** O HUD HTML/CSS continua em UI Toolkit, sem alterações de funcionamento.

Razões:
- Mais de 15 componentes, incluindo o editor de código com debugger, já são IMGUI. Passar para UI Toolkit ou uGUI obrigava a reescrever o editor de código, que a missão pede para não simplificar.
- Assim não se mistura uma terceira tecnologia.
- O IMGUI tem um ponto central, `GUI.skin`. Ao instalar um skin industrial em `UiGui.Begin()`, todos os painéis existentes passam a ter o mesmo aspeto sem serem reescritos.
- Não há assets importados: as texturas são geradas em código, como os sprites (`PixelArtFactory`).

**Peças:**

| Ficheiro | O que faz |
|---|---|
| `Game/Ui/UiTheme.cs` | Tokens de cor e texturas geradas (placas com cantos chanfrados, aresta de aço, destaque âmbar; estados normal / hover / pressionado / desativado). Fontes do sistema: Bahnschrift nos títulos, Segoe UI no texto, fonte CJK em japonês, chinês e coreano. Estilos nomeados e o `GUISkin`. Reconstrói-se quando muda o contraste, a paleta, o tamanho do texto ou o idioma. |
| `Game/Ui/Ui.cs` | Componentes: botão, campo, toggle, slider, seletor ‹ ›, secção, chave-valor, badge, spinner, toasts e navegação por teclado ou comando. `UiScreen` + `UiNav`: pilha de ecrãs com transição (fade + deslize), Voltar (Esc / B), diálogos de confirmação, estado "ocupado" que bloqueia a interação (com cancelamento opcional) e `Run()`, que mostra o carregamento antes de o executar. |
| `Game/Ui/UiArt.cs` | Fundo animado (duas silhuetas de fábricas em paralaxe, chaminés com fumo, janelas a piscar, correia com caixas). Avatares de perfil. Pré-visualização de mapas com dados reais. |
| `Game/Ui/MenuScreens.cs` | Perfis, principal, singleplayer, mapas e saves, novo mundo, créditos. `FrontEnd` guarda o estado partilhado. |
| `Game/Ui/OptionsScreen.cs` | Opções em 8 categorias. É partilhado pelo ecrã inicial e pela pausa. |
| `Game/Ui/MultiplayerScreen.cs` | Criar partida, entrar numa partida e `ServerHistory`. |
| `Game/TitleMenu.cs` | Anfitrião dos ecrãs iniciais. |
| `Game/OptionsMenu.cs` | Menu de pausa (Esc). |
| `Core/Worlds.cs` | Sem dependências do Unity: `WorldStore` (listar, ler o cabeçalho, metadados, mudar nome, duplicar, apagar para `saves/deleted`, pesquisar e ordenar) e `ProfileStore`. |

A lógica de jogo fica fora da apresentação: os ecrãs chamam `GameBootstrap`, `Net`, `Online`, `Settings` e `WorldStore`.

**Escala.** `UiGui.S` = escala do jogador × fator automático, com 1080p = 1×, 1440p ≈ 1,33× e 4K = 2×, entre 0,7 e 4. O HUD HTML usa a mesma escala.

## 3. Design system
- **Cores.** Carvão #0C0E11 / #15181D, grafite #1E232A / #29313A, aço #353E49 / #55606E, texto #E8E3D7, secundário #9CA5AF, destaque âmbar #F2A23A / #FFC066 e perigo #D9573F.
  - Alto contraste: preto, branco e amarelo.
  - Paleta para daltónicos: azul e laranja nos estados.
- **Botões.** Placa grafite. No hover ficam mais claros e ganham uma barra âmbar à esquerda; pressionados ficam âmbar escuro; desativados ficam lisos e com texto apagado. Há variantes primária (âmbar), perigo, fantasma, separador e linha de lista.
- **Painéis.** Cantos chanfrados, aresta de 1 px e realce no topo. As secções têm título âmbar em maiúsculas e um filete por baixo. O cabeçalho dos ecrãs tem "‹ Voltar", título, subtítulo e um filete com um segmento âmbar.
- **Tipografia.** Títulos em Bahnschrift negrito (64 / 30 / 18), texto em Segoe UI 14–15. Tudo passa por `UiGui.Font()`, por isso respeita o tamanho de texto escolhido.
- **Movimento.** Transições de 0,22 s, toasts a deslizar e fundo animado. "Reduzir movimento" (ou "Reduzir flashes") desliga tudo isto.

## 4. O que cada ecrã faz (tudo ligado a sistemas reais)

### Entrada / perfis
- Criar, escolher, editar o nome e o avatar (12) e apagar perfis.
- O perfil escolhido é lembrado entre sessões.
- O nome do perfil é o nome no multijogador e segue a mesma regra de validação.
- Não há palavra-passe: um perfil não é uma conta. A conta online, opcional, é o painel que já existia, aberto a partir daqui.
- Apagar um perfil não apaga mundos.

### Menu principal
- Entradas: Continuar, Singleplayer, Multiplayer, Mapas e saves, Opções, Créditos e Sair.
- "Continuar" carrega o mundo mais recente que se consegue ler. Fica desativado, com explicação, se não houver nenhum.
- Mostra também:
  - o cartão do perfil, com o estado da conta online;
  - um badge com os pedidos de amizade e convites reais do serviço;
  - a versão do jogo e do formato de save.
- **Mods:** não há infraestrutura de mods no projeto, por isso a entrada não existe.

### Mapas e saves
- **Lista.** Cada mundo mostra:
  - pré-visualização;
  - nome, seed e bioma inicial;
  - tempo de jogo (ticks simulados ÷ 20);
  - última sessão e data de criação;
  - formato do save e versão do jogo;
  - estado: pronto, danificado ou de versão mais recente.
- **Pesquisa** por nome ou seed, **4 ordenações** e o filtro "só deste perfil".
- **Ações:** jogar (duplo clique ou Enter), mudar nome, duplicar (copia também os planetas visitados), apagar com confirmação e abrir a pasta.
- **Modelo.**
  - Um mundo é um ficheiro `saves/<id>.cbsave`. Ao lado ficam `<id>.meta.json` e `<id>.png`, e os outros planetas visitados ficam em `saves/planets/<id>/`.
  - Os saves antigos (quicksave e autosave) aparecem como mundos, sem migração.
  - Os saves na nuvem continuam a ver todos os slots.
  - Mudar o nome só altera o nome mostrado: o id fica, por isso as ligações à nuvem continuam válidas.
  - **Apagar move os ficheiros para `saves/deleted/<id>-<hora>/`. Nada é destruído.**
- **Pré-visualização.**
  - O terreno e as jazidas vêm do gerador com a seed do save.
  - Os edifícios vêm das linhas `b` do próprio save (ou do estado vivo, ao gravar).
  - É feita em segundo plano e fica guardada como PNG.

### Novo mundo
- Campos: nome, seed (aleatória ou escrita, validada) e criaturas hostis.
- O modo pacífico é uma definição global, e o ecrã di-lo.
- O gerador só tem uma definição: a seed. O ecrã explica isto em vez de inventar opções de biomas ou recursos que o motor não suporta.
- A pré-visualização ao vivo mostra a zona inicial real, o bioma de partida e as jazidas a menos de 40 células.

### Multijogador
- **Criar partida:**
  - mundo existente (com pré-visualização) ou novo;
  - nome da sessão (usado na listagem online);
  - limite de jogadores (1–8 além do anfitrião);
  - papel de quem entra (visitante, construtor ou administrador);
  - palavra-passe;
  - acesso por LAN ou Internet (UPnP);
  - listagem online para amigos, todos ou só por convite (só com conta);
  - porta.
  - "Iniciar servidor" carrega o mundo e começa a alojar (`Net.PendingHost`). O endereço e o estado aparecem depois no painel N.
- **Entrar numa partida:**
  - endereço `host[:porta]` (ou `[IPv6]:porta`) e palavra-passe;
  - favoritos e servidores recentes;
  - jogos listados no serviço online, com pesquisa (só com conta; sem conta o ecrã di-lo);
  - estado da ligação num overlay que se pode cancelar com Esc;
  - erros claros.
  - Não foi inventada descoberta na LAN nem matchmaking.
- **Dentro da partida (painel N):**
  - lista de jogadores e respetivos papéis, com mudança de papel;
  - expulsar e banir;
  - listagem online e convites;
  - "Sair da partida" para convidados e "Encerrar o servidor" para o anfitrião.
- A arquitetura host-authoritative mantém-se.

### Opções
| Categoria | O que tem |
|---|---|
| **Gráficos** | Resolução e modo de ecrã (só na build), VSync, limite de FPS, qualidade (níveis do projeto) e escala da interface. |
| **Áudio** | Geral, música, efeitos, máquinas, ambiente e interface (novo canal com sons de clique e confirmação, que ignora a pausa), e legendas. |
| **Controlos** | Teclas agrupadas em movimento, construção, interface e jogo. Remapear, conflitos marcados a vermelho, restaurar, e atalhos fixos de programação (debugger e editor) só para consulta. |
| **Jogo** | Criaturas, intervalo da gravação automática (desligada ou 1–30 min) e ajuda à programação. |
| **Interface** | Escala, tamanho de texto, FPS na caixa de estado, notificações pop-up e reduzir movimento. |
| **Idioma** | As 10 línguas, aplicadas de imediato. |
| **Acessibilidade** | Texto, alto contraste, paleta para daltónicos, filtro de ecrã, flashes, movimento e legendas. |
| **Rede** | Porta, jogadores, papel de quem entra, estado da ligação, endereços LAN e nome. |

- Tudo é guardado de imediato. Cada categoria tem "Repor predefinições".
- Os `Set` de `Settings` ignoram valores iguais, para não escrever PlayerPrefs a cada frame.

### Dentro do jogo
- **Menu de pausa:** retomar, guardar, carregar outro mundo (grava este primeiro), opções, multijogador, conta, sair para o menu principal (grava; avisa o anfitrião de que fecha o servidor) e sair do jogo.
- Em multijogador o anfitrião já não pausa o mundo dos convidados.
- **Painéis:** estado, barra de construção, máquinas e editor de código, investigação, editor de HUD, tutorial, diário, conta, espaço e final passam a usar o painel e as cores do tema.
- **Barra de construção:** mais legível (128×54, tecla em âmbar, custo em cinzento, seleção pressionada).
- **HUD HTML:** a moldura usa as mesmas cores. O CSS inicial dos mundos novos usa títulos âmbar. O HTML/CSS do jogador continua a funcionar igual.

## 5. Verificação
- **EditMode: 227/227**, incluindo 4 testes novos:
  - lista de mundos (cabeçalho, ficheiros danificados ou mais recentes, "Continuar", pesquisa por seed);
  - mudar nome, duplicar com planetas e apagar sem perder nada;
  - endereço:porta com IPv6;
  - perfis (validação, persistência, ficheiro estragado guardado como `.bad`).
- **Play Mode com capturas** em 1920×1080, 2560×1440, 3840×2160 e 1280×720. Foram percorridos estes fluxos:
  - entrada e perfil;
  - menu principal;
  - criar mundo, jogar e gravar (save, metadados e PNG escritos);
  - mapas e saves com pré-visualização;
  - novo mundo com pré-visualização ao vivo;
  - opções;
  - multijogador (criar, entrar e erro "recusada"; histórico gravado);
  - pausa;
  - sair para o menu principal (gravou o mundo);
  - "Iniciar servidor" a partir do menu, com **um convidado real (a build anterior em `-batchmode -join`) admitido com o papel escolhido**;
  - painel N com o jogador;
  - editor de código com o tema novo;
  - textos em português.
- **Problemas corrigidos depois das capturas:**
  - corte vertical na vinheta;
  - silhueta da fábrica demasiado alta;
  - fundo cinzento nos botões sem textura (o IMGUI usa uma placa por defeito: agora há uma textura transparente explícita);
  - pré-visualização a sobrepor-se à legenda;
  - seletores e botões do painel N cortados;
  - mensagens de erro do Windows com caracteres nulos.

## 6. O que não foi feito (honestamente)
- **Latência:** o protocolo não a mede, e o ecrã de rede di-lo.
- **Descoberta na LAN e matchmaking próprio:** não existem. A listagem online depende do serviço, que ainda não está em produção (ver `ONLINE_PLAN.md`).
- **Mods:** sem infraestrutura, por isso sem entrada no menu.
- **Gerador:** não tem opções além da seed.
- **Comando:**
  - funcionam a cruz direcional ou o analógico, A, B e Start no menu principal, na pausa e nas listas;
  - os campos de texto e os painéis do jogo continuam a usar teclado e rato.
- **Barra de construção:** os custos longos são cortados (falta uma dica ao passar o rato).
- **Build e self-test:** não foram repetidos nesta missão.

## 7. Protótipo do menu principal (2026-11-25)
- Só o ecrã principal (`MainScreen.Draw`) mudou; os outros ecrãs, as ações dos botões e os sistemas do jogo ficaram iguais. Se a arte faltar, o menu volta ao desenho anterior (`DrawPlain`).
- Arte feita para o Codebound, sem assets externos:
  - `StreamingAssets/Art/menu_bg.jpg` (3840×2160): fábrica renderizada no Blender, gerada por `Tools/art/menu_scene.py` (EEVEE, cerca de 12 s). O terminal mostra um programa real do Codebound.
  - `StreamingAssets/Art/menu_logo.png`: letras industriais próprias, em aço extrudido, geradas por `Tools/art/menu_logo.py`.
- `Game/Ui/MainMenuArt.cs` trata do resto em jogo:
  - desvio lento da câmara, luz das fornalhas a tremeluzir, cursor do terminal a piscar, poeira na luz e vinheta (tudo parado com "reduzir movimento");
  - coluna escura com régua metálica;
  - ícones vetoriais (SDF) gerados em código;
  - placas de aço com estados normal, hover (barra e ícone âmbar, deslize, seta), pressionado e desativado.
- Cartões com dados reais: perfil e estado online; última sessão, com o mapa do save.
- Verificado em Play Mode a 1920×1080, 2560×1440 e 3840×2160 (`Codebound/Screenshots/menu_final_*.png`). Todas as ações foram testadas: Continuar carregou o mundo e Sair pede confirmação. 227/227 testes passam.

## 8. Correção da direção artística do menu principal (2026-11-25)
- O que estava mal no protótipo: sete placas iguais empilhadas (aspeto de tabela), dois cartões retangulares, uma barra de rodapé de lado a lado, uma coluna retangular, e uma fábrica feita de cubos parada numa imagem.
- **Cenário vivo** (`Tools/art/menu_scene.py`, v2): um loop perfeito de 6 s (144 fotogramas), renderizado no Blender em `StreamingAssets/Art/menu_loop.mp4` (2560×1440, H.264) e tocado pelo `VideoPlayer`. O still 4K, `menu_bg.jpg`, aparece enquanto o vídeo não arranca, se ele falhar ou com "reduzir movimento". A cena tem:
  - montadoras octogonais com janelas iluminadas, anel e ventoinha a rodar, placas A-02/A-03 e LEDs;
  - fornalhas com grelha incandescente e fogo a tremeluzir;
  - três braços robóticos articulados a pegar e pousar peças;
  - correias em três planos (chão, elevada e inclinada), com minério, caixas, chapas e bobinas a circular;
  - ponte rolante em vaivém com farol rotativo;
  - plataforma com escadas, tubagens curvas e válvula;
  - tanques com escadas;
  - feixes de luz fria a entrar pelas janelas e névoa volumétrica;
  - o terminal do Codebound a correr código, com o cursor a piscar.
  O projeto usa o renderer 2D do URP, que não ilumina cenas 3D, por isso a luz e o movimento vêm pré-renderizados do Blender. O Unity faz a composição e a animação da interface.
- **Interface sem caixas** (`MainMenuArt.cs`, `MainScreen.Draw`):
  - consola escura com corte diagonal, aresta de aço, segmentos âmbar e marcas de régua;
  - navegação num trilho de circuito, com os itens escalonados na diagonal, um nó em losango que acende no item selecionado e um sinal a percorrer o trilho;
  - separadores de grupo em sinalética de perigo;
  - uma lâmina com recuo inclinado e ponta em seta que desliza até ao item selecionado;
  - o primeiro item (Continuar) destaca-se, com o nome do mundo em âmbar.
  - Perfil e última sessão em placas assimétricas: corte grande em cima à esquerda, corte pequeno em baixo à direita, luz âmbar no corte e uma marca no topo. Ações do perfil como ligações com sublinhado. Rodapé numa tira com o fim em diagonal.
- As ações dos botões ficaram iguais. Teclado e comando continuam a funcionar.

## 9. Menu principal com fábrica 3D e navegação pelo terminal (2026-11-25)

**Arquitetura**
- **Renderer:** a cena da fábrica é 3D real no Unity. O projeto desenha com o renderer 2D do URP, por isso `Editor/MenuStageSetup.cs` acrescenta ao `UniversalRP` um renderer Universal (Forward+), em `Settings/MenuRenderer3D.asset` (índice 1). Também ativa as sombras das luzes de foco e as sombras suaves; o renderer 2D ignora ambas.
- **Modelos:** `Tools/art/menu_stage.py` (Blender 5.2) constrói a fábrica e exporta `Resources/MenuStage/menu_factory.fbx`:
  - a geometria estática é agrupada por material e recebe UVs projetadas por caixa;
  - as peças móveis ficam separadas;
  - os marcadores `Mark_*` indicam as poses de câmara, os cantos do vidro do terminal, as pontas das correias, as bases dos braços e o curso da ponte rolante;
  - `Proto_*` são os protótipos dos braços e dos itens.
- **`Game/Ui/MenuStage.cs`** monta, ilumina e anima a cena na camada 31, com câmara e volume próprios:
  - os materiais URP/Lit são afinados por nome, com uma textura de sujidade gerada;
  - as luzes são afinadas por nome, com sombras suaves nas mais próximas e um foco no emblema;
  - a névoa é exponencial e os feixes são cones aditivos (`Resources/MenuStage/LightShaft.shader`);
  - o pós-processamento tem bloom, ACES, ajustes de cor e vinheta.
  - A animação é feita em C#:
    - itens a circular nas quatro correias, cuja superfície também desliza;
    - três braços montados a partir dos protótipos, que pegam junto à correia mais próxima, rodam cerca de 110° e pousam;
    - ventoinhas e anéis das montadoras a rodar;
    - a ponte rolante em vaivém, com o farol a rodar;
    - o fogo das fornalhas a tremeluzir e os LEDs a piscar;
    - o programa no ecrã do terminal, com o cursor a piscar.
- **Terminal físico:** uma estação em plinto elevado, com consola, teclado e botões, um monitor 16:9 com moldura, pala e barra de luz, armários com LEDs e cabos.
- **Navegação cinematográfica:**
  - *Ida:* `Activate()` no `MainScreen` faz `MenuStage.GoTerminal()`. A câmara segue uma curva em arco com easing cúbico (1,6 s) e o campo de visão estreita até o vidro encher o ecrã, com margem para se ver a moldura. A consola desliza para fora e o vidro "liga-se" (linha que se abre), e só então o ecrã real da opção é empurrado na pilha (`Nav.Push`).
  - *No terminal:* o `TitleMenu` desenha esse ecrã dentro do retângulo projetado do vidro (`MenuStage.ScreenRect()`, com `GUI.BeginClip`). O fundo passa a ser o fósforo do tubo (`MenuBackdrop.Terminal`), com linhas de varrimento, cantos escuros e reflexo. Rato e teclado funcionam normalmente.
  - *Volta:* "Voltar" (ou Esc) tira o ecrã da pilha. O `MainScreen.OnShow` faz `GoOverview()`: o vidro desliga-se, a câmara recua e o menu volta a deslizar para dentro.
  - *Sair:* não move a câmara; pede confirmação.
- **Interface da vista geral:**
  - laje metálica com recortes, parafusos, grelhas e um segmento aceso na aresta;
  - emblema engrenagem-C e logótipo com "BOUND" em laranja;
  - botões em barra chanfrada com ponta em seta, poço de ícone, brilho e deslize no hover ou seleção, estado pressionado e desativado; "Continuar" em laranja aceso;
  - perfil e última sessão (com o mapa real) em placas assimétricas.
- **HUD do jogo:** fica escondido enquanto o ecrã inicial está aberto (`HudLayer`, `DebugHud`).
- **Recurso de segurança:** sem o modelo, o menu volta ao fundo pré-renderizado (`menu_bg.jpg`); sem a arte, volta ao desenho temático.

**Verificação**
- Play Mode a 1920×1080 e 2560×1440, com capturas em `Codebound/Screenshots/menu_final3_*.png` e `term_*.png`.
- Percurso de cada opção (ativada pelo código do menu e com `Nav.Back`):
  - Singleplayer, Multiplayer, Mapas / Saves, Opções e Créditos: abrem no terminal com a câmara lá (`atTerminal=True`) e regressam à vista geral (`atOverview=True`).
  - Sair: abre a confirmação sem viajar.
  - Continuar: viaja, carrega o mundo "Vale do Ferro" com o HUD normal, e "Sair para o menu principal" reconstrói o palco.
- Os testes EditMode passam todos.

## 10. Auditoria de UX do menu principal (skill ux-audit, 2026-11-25)

**Camadas:** L1 (`MenuScreens.cs`, `MainMenuArt.cs`, `MenuStage.cs`, `TitleMenu.cs`), L2 (capturas a 1080p e 1440p, vista geral e terminal) e L3 (o percurso acima).

**Padrões em uso**
- *Navegação:* pilha de ecrãs com Voltar, com o ecrã atual integrado no mundo (o terminal como "centro").
- *Seleção:* destaque que segue o rato ou o teclado, com uma ação principal destacada (Continuar).
- *Painéis:* resumo da última sessão com dados reais; confirmação antes de sair.

**Pontos fortes a preservar**
- O HUD do jogo está ausente do menu.
- A ação principal é óbvia.
- Todas as ações estão ligadas a sistemas reais.
- "Sem mundos" desativa Continuar com explicação.
- O estado offline do serviço é explicado no perfil e nos jogos online.
- Os estados hover, seleção, pressionado e desativado estão desenhados.
- A interação fica bloqueada durante a viagem da câmara.
- A escala funciona de 720p a 4K.

**Lacunas, por gravidade**
- **Média:** a viagem bloqueia a interação durante 1,6 s; não há atalho para quem repete o percurso. Hoje só "Reduzir movimento" o encurta, porque com ele ativo a câmara salta logo para o destino. *Remédio:* permitir saltar a viagem com Enter ou Esc.
- **Média:** a navegação por teclado ou comando dentro dos ecrãs do terminal depende de cada ecrã (por exemplo, setas na lista de mundos). Não há um foco visual uniforme.
- **Média:** espaço vazio entre a placa do logótipo e os botões em ecrãs altos (L2, `menu_final3_1440.png`).
- **Média:** à primeira execução, sem perfil, o ecrã de perfis abre por cima da cena, sem a viagem até ao terminal.

**Ficou por verificar**
- Rato e teclado físicos: o percurso foi conduzido por código, pelos mesmos métodos que os cliques chamam.
- O estado pressionado em captura.
- 4K com o palco 3D (o layout é o mesmo, testado a 4K na versão anterior).

## 11. Vida industrial da fábrica do menu (2026-11-25)

Apenas visual. A simulação do jogo, o HUD e os ecrãs de jogo não foram tocados. Código em `Game/Ui/MenuFactory.cs`, ligado a partir do `MenuStage` (`Animated()` cria a fábrica e `Update()` chama `Tick`).

- **Blender** (`Tools/art/menu_stage.py`): layout coerente (correias Front/Mid/High/Incline, braços A/B/C, AsmHero/AsmB/AsmFar, fornalhas A–C). Exporta como objetos separados as peças que se animam:
  - `Roller_*`, `PistonRod_*`, `Status_*`;
  - cabos e tubos `Sway_*`;
  - máquinas inteiras `Grp_*`, para vibrarem;
  - marcadores `Mark_Mouth_*`.
- **Correias:**
  - itens em pool (sem `Instantiate` depois do arranque), com espaçamento natural;
  - acumulam quando a frente pára;
  - passam da rampa para a correia alta;
  - entram e saem por portais ou máquinas;
  - rolos a rodar e textura da tela à velocidade real;
  - travam com firmeza e arrancam suavemente.
- **Braços:**
  - IK analítica de 2 ossos;
  - máquina de estados: espera e segue a peça que chega → trava a correia → aproxima → desce → fecha a pinça na largura da peça → levanta → transporta em arco à volta da base → pousa → abre → sobe → regressa;
  - ArmA: minério da Front para a rampa. ArmB: bobinas da Mid para a rampa. ArmC: chapas da Mid para a boca da fornalha B (chama e faíscas).
- **Máquinas:**
  - A AsmHero trabalha quando recebe caixas.
  - AsmB, AsmFar e as fornalhas A/C têm ciclos próprios e aleatórios.
  - Pistões com dois golpes por ciclo e impacto no fim do curso.
  - Lâmpada: verde em espera, âmbar a trabalhar, vermelho a piscar num aviso raro.
  - Fornalhas com chama que respira e clarão quando alimentadas; vapor no fim do ciclo.
- **Vibração:** local e por eventos (prensa, peça a cair, correia a andar); cabos a balouçar.
- **Partículas** (`Resources/MenuStage/SoftParticle.shader`): faíscas, vapor, brasas e pó, em tempo não escalado.
- **Terminal:** a última linha mostra contadores reais da linha (`menu.factoryStatus`).
- **Câmara do menu:** 0,6 m mais alta e rodada 7° para a linha, para os braços se lerem entre o painel e o terminal.
- **"Reduzir movimento"** pára tudo.
- **Medições:** `Tick` custa ≈0,05 ms por frame; 227 testes passam; viagem ao terminal e regresso verificadas.
- **Gravação:** `Codebound/Recordings/menu_factory_life.mp4` (ignorado no git).
