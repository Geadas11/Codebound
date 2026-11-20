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

## Simulação de máquinas (Core)
- `GameState` agrega `WorldState`, `BuildGrid`, `Factory`, inventário e posição do jogador — é a unidade que se guarda.
- `Factory.Tick(dt)` a 20 Hz fixos (`GameBootstrap`): redes elétricas → ações de máquinas → empurrar saídas.
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
