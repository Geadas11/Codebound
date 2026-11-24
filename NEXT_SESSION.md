# NEXT SESSION — como retomar

## Estado (sessão 3: + segurança informática, reparação, energia renovável, localização)
- Etapas 1 e 2 (base) feitas; Etapa 3 com **JavaScript real a correr isolado** e a comandar máquinas. Ver TASKS.md.
- Testes: 58/58 EditMode; localização em 10 línguas (tabelas em `StreamingAssets/Localization`, geradas por `Codebound/Tools/localization/build_tables.py` — editar o script, não os JSON); energia solar/baterias; 53/53 antes; segurança informática e reparação feitas; áudio sintetizado feito; menu de opções/acessibilidade feito (Esc); criaturas/torres feitas; antes 42/42 EditMode; investigação feita (tecla T, laboratório programável); antes 39/39 EditMode; estufa, montadora e Computador Avançado feitos. Antes: 36/36 EditMode + 8/8 runtime SQL; dia/noite/clima feito; linguagens JS, TS, Python e SQL; sobrevivência feita. Antes: 25/25 EditMode + 12/12 runtime JS + 13/13 runtime Python. HUD programável HTML/CSS feito. Linguagens: JavaScript, TypeScript, Python. Build Windows com Node + Python incluídos, self-test PASS nas 3. Verificado em Play Mode: programa JS minerou 5 pedras para um baú, eventos dispararam, save/load restaurou tudo e reiniciou o programa.
- Requisito de desenvolvimento: **Node.js no PATH** (testado com v24.13.0).

## Git / GitHub
- Repositório local na raiz `Jogo_CODEFACT/` (branch `main`) → remoto `origin` = `https://github.com/Geadas11/Codebound`, branch **`desktop`** (`git push` envia para lá: `push.default=upstream` está configurado neste repositório).
- O mesmo repositório tem outro branch, `claude/intelligent-bohr-nfkoxs`, com **outra versão** do Codebound (sandbox em Wasmtime, estrutura `Assets/_Codebound/...`), sem histórico comum. Não misturar.
- Graphify (MCP) indexa o branch principal do repositório; para indexar este código, o branch principal no GitHub tem de ser `desktop`.
- `Codebound/Tools/python/` não está no git (ver README para o obter).

## Simulação industrial (missão de 2026-11-24)
- Plano e auditoria: `INDUSTRY_PLAN.md`. Etapas 1 (tick ordenado, determinismo, sleep/wake) e 2 (correias com 2 pistas, braços com mão — `Core/Logistics.cs`) feitas. Etapa 3 (fluidos — `Core/Fluids.cs`) feita. Etapa 4 (energia — `Core/Power.cs`) feita. **Próximo: Etapa 5 — sinais** (cabos, redes de sinais, combinadores, ligação a máquinas, computadores e HUD). Pendente da Etapa 4: reconstrução incremental das redes elétricas (depois baixar o limite de `Building_InABigBase_DoesNotStall` de 25 ms).
- Antes de mexer na simulação: `DeterminismTests` e `SleepWakeTests` têm de continuar verdes (comparam hashes de estado).
- Recuperação: tag local `pre-industry-2026-11-24`.

## Mapa do código
- Core: `Codebound/Assets/Codebound/Scripts/Core/` — `GameState.cs` (agregado + `SaveSystem`), `Factory.cs` (energia/ações/logística), `Machines.cs` (estados), `Buildings.cs`, `World.cs`, `Inventory.cs`, `Items.cs`, `Json.cs`.
- Game: `.../Scripts/Game/` — `GameBootstrap.cs` (arranque, tick 20 Hz, F5/F9), `ProgramManager.cs` (ponte runtime + API), `MachinePanel.cs` (painel E + editor de código), `BuildController.cs`, `MiningController.cs`, `ChunkRenderer.cs`, `DebugHud.cs`.
- HUD: `Core/Hud/{Html,Css,HudLayout}.cs`, `Game/{HudLayer,HudRenderer,HudEditor}.cs`.
- Game também: `ConveyorItemsView.cs` (itens nas correias), `PixelArtFactory.cs`, `SelfTest.cs` (`-selftest`).
- Editor: `Scripts/Editor/BuildScript.cs` (build + cópia do Node). Build via Editor aberto: `unity command script eval ... -- --code 'return Codebound.EditorTools.BuildScript.Build(UnityEditor.BuildTarget.StandaloneWindows64, Codebound.EditorTools.BuildScript.WindowsOutput, "node.exe");' --timeout 1400000`.
- Python: `Runtime/py/host.py` (1 processo por programa), `Game/RuntimeProcess.cs`, CPython em `Codebound/Tools/python/`. Teste: `Codebound/Tools/python/python.exe -I Codebound/Tests/runtime/python_host_test.py`.
- Runtime: `Codebound/Assets/StreamingAssets/Runtime/js/host.js` (processo), `worker.js` (programa + API), `prepare.js` (TS→JS, instrumentação do debugger), `vendor/typescript/` (Apache-2.0). Teste: `node Codebound/Tests/runtime/host.test.js`.

## Comandos úteis
```bash
node Codebound/Tests/runtime/host.test.js
# Editor aberto:
unity command test run --project-path <abs>/Codebound --caller plugin --skill unity-cli
unity recompile --project-path <abs>/Codebound
# Editor fechado:
unity test Codebound --mode EditMode --output test-results.xml
```
Play Mode por CLI: `editor set_auto_tick` → `editor.playmode start` → `status --until-ready` → `script eval` → `capture game_view -- --source camera --output-path Screenshots/x.png`. Cenário de teste pronto: ver o script de montagem usado na sessão 1 (perfuradora + baú + gerador + poste + computador) — reescrever a partir de `FactoryTests.WorldWithOre`.

## Próximos passos (por ordem)
1. ~~Energia~~ e ~~Localização~~ feitas (falta traduzir descrições das tecnologias e o painel de máquinas; textos novos: `Loc.T("chave", "inglês")` + linha no `build_tables.py`).
3. ~~Exploração~~ (postos nas Ruínas, diário J) e ~~Ecologia~~ (poluição, purificador) feitas; falta subsolo.
3b. ~~Multijogador LAN~~ (Etapa 6 base) feito — `Core/Multiplayer.cs`, `Game/NetSession.cs`; testar com `Builds/Windows/Codebound.exe -batchmode -nographics -host` + Editor `Net.Join("127.0.0.1","nome")`.
3c. ~~Etapa 7 base~~ (plataforma, subida programada, viagem entre mundos) feita — `Core/Space.cs`, `Game/SpaceView.cs`.
4. Equipamento do jogador (armas/ferramentas melhores), minas/barreiras.
5. ~~PHP~~ e ~~C#~~ feitos (testes: `Tests/runtime/php_host_test.py`, `csharp_host_test.py`; depois de mudar `Tools/cshost`, recompilar com o `dotnet.exe` do Unity). C e C++ feitos (`Tests/runtime/c_host_test.py`). Todas as 10 linguagens funcionam. Estado das ferramentas nesta máquina: PHP 8.0.30 em `C:\wamp64` (não incluído no projeto), .NET SDK instalado (Roslyn), nenhum compilador C/C++. Plano proposto: C e C++ compilados para WebAssembly (clang + wasi-sdk, ~150 MB, exige download autorizado) e executados no Node já incluído (isolamento do próprio WASM); PHP com o PHP oficial para Windows (download autorizado) num processo com Job Object e `disable_functions`/`open_basedir`; C# com Roslyn num processo separado e lista branca de APIs verificada na árvore sintática + Job Object.
6. ~~Etapas 2–8~~ completas (Etapa 8: desempenho, tutorial, arte e animações, música composta). Só faltam as builds macOS/Linux (downloads). Antes: ~~Etapas 2, 3, 4, 5, 6 e 7~~ completas (ver TASKS): multijogador cifrado pela internet, órbita/asteroides/retransmissores/final. Por testar: UPnP num router real. Teste de rede: Editor anfitrião + `dotnet Tests/nettest/bin/Release/net8.0/nettest.dll <pcX> <pcY>` (ou `travel`). Etapa 3: só faltam builds Linux/macOS (módulos do Unity + Node/Python por plataforma — downloads, pedir autorização).
7. Etapa 8: arte final, tutorial, desempenho.

## Avisos
- HUD/painéis são IMGUI de desenvolvimento — o HUD programável HTML/CSS é Etapa 4.
- Sprites procedurais (`PixelArtFactory`) — substituir na Etapa 8 mantendo 16 px/célula.
- Não editar `.unity` à mão: o jogo é construído por `GameBootstrap`.

- **Barras invertidas:** a shell desta máquina interpreta `\` em heredocs/python inline. Para código com barras invertidas, usar as ferramentas Write/Edit.
- Play Mode sem domain reload: estado estático novo precisa de reset em `SubsystemRegistration`.
- Testar UI sem rato real: enviar `PointerDownEvent`/`PointerMoveEvent` (com `EventType.MouseDrag`)/`PointerUpEvent` para `panel.visualTree` via `script eval` (ver DEVELOPMENT_LOG parte 6).
- Build via `script eval`: ligar `editor set_auto_tick` antes (com ele desligado o build ficou pendurado). Com o Editor fechado: `unity build --target StandaloneWindows64 --execute-method Codebound.EditorTools.BuildScript.BuildWindows Codebound`.
- Play Mode por script: depois de `GameBootstrap.NewGame(seed)` fazer `Time.timeScale = 1` e `TitleMenu.IsOpen = false` (por reflexão; o ecrã inicial ficou "aberto").
