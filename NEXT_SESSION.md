# NEXT SESSION — como retomar

## Estado (sessão 2: + clima, agricultura, fabricação, investigação, criaturas, opções, áudio)
- Etapas 1 e 2 (base) feitas; Etapa 3 com **JavaScript real a correr isolado** e a comandar máquinas. Ver TASKS.md.
- Testes: 50/50 EditMode; áudio sintetizado feito; menu de opções/acessibilidade feito (Esc); criaturas/torres feitas; antes 42/42 EditMode; investigação feita (tecla T, laboratório programável); antes 39/39 EditMode; estufa, montadora e Computador Avançado feitos. Antes: 36/36 EditMode + 8/8 runtime SQL; dia/noite/clima feito; linguagens JS, TS, Python e SQL; sobrevivência feita. Antes: 25/25 EditMode + 12/12 runtime JS + 13/13 runtime Python. HUD programável HTML/CSS feito. Linguagens: JavaScript, TypeScript, Python. Build Windows com Node + Python incluídos, self-test PASS nas 3. Verificado em Play Mode: programa JS minerou 5 pedras para um baú, eventos dispararam, save/load restaurou tudo e reiniciou o programa.
- Requisito de desenvolvimento: **Node.js no PATH** (testado com v24.13.0).

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
1. **Segurança informática**: firewall e palavra-passe nos computadores (bloqueiam/atrasam drones), deteção de intrusão como evento, backups explícitos; reparação de estruturas (multiferramenta + materiais).
2. **Energia**: painel solar (depende de `Daylight` e do tempo), baterias, investigação Energy.
3. **Localização**: extrair textos de UI para tabelas (pt-PT, pt-BR, en, es, fr, de, it, ja, zh-Hans, ko); seletor no menu.
4. **Exploração**: instalações abandonadas nas Ruínas com registos/terminais (narrativa), subsolo.
5. Linguagens restantes: PHP, C, C++, C#.

## Avisos
- HUD/painéis são IMGUI de desenvolvimento — o HUD programável HTML/CSS é Etapa 4.
- Sprites procedurais (`PixelArtFactory`) — substituir na Etapa 8 mantendo 16 px/célula.
- Não editar `.unity` à mão: o jogo é construído por `GameBootstrap`.

- **Barras invertidas:** a shell desta máquina interpreta `\` em heredocs/python inline. Para código com barras invertidas, usar as ferramentas Write/Edit.
- Play Mode sem domain reload: estado estático novo precisa de reset em `SubsystemRegistration`.
- Testar UI sem rato real: enviar `PointerDownEvent`/`PointerMoveEvent` (com `EventType.MouseDrag`)/`PointerUpEvent` para `panel.visualTree` via `script eval` (ver DEVELOPMENT_LOG parte 6).
- Build via `script eval`: ligar `editor set_auto_tick` antes (com ele desligado o build ficou pendurado).
