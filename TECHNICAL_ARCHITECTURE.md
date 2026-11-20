# Codebound — Arquitetura Técnica

_Última revisão: 2026-11-20 (Sessão 1, continuação)_

## 1. Visão geral

```
┌──────────────────────── Processo Unity (jogo) ────────────────────────┐
│  Camada de apresentação  (Scripts/Game, asmdef Codebound.Game)        │
│   GameBootstrap · WorldView · StructureView · PlayerController        │
│   CameraController · BuildModeController · InteractionController     │
│   DevHud (temporário) · GameInput · PixelArt                          │
│                         │ usa                                         │
│  Lógica de simulação (Scripts/<Módulo>/Logic, sem UnityEngine)       │
│   Core · Items · Inventory · World · Building · Player · …            │
└──────────────────────────────┬────────────────────────────────────────┘
                               │ IPC local (JSON) — Etapa 3
┌──────────────────────────────▼────────────────────────────────────────┐
│  Runtime de execução de código (processo separado, /Runtime)          │
│   host supervisor → sandbox por programa (limites CPU/RAM/tempo)      │
│   10 linguagens reais + bindings da API Codebound                     │
└───────────────────────────────────────────────────────────────────────┘
```

**Princípio central:** toda a regra de jogo vive em C# puro (`noEngineReferences: true`). O Unity só lê input, desenha e toca som. Benefícios:
- Testável fora do Unity com `dotnet test` (as sessões de desenvolvimento não têm o editor).
- Determinística (tick fixo, RNG próprio) → base para o multiplayer com host autoritário (Etapa 6).
- O código do jogador nunca corre no processo do Unity.

## 2. Estrutura de pastas

```
/                               documentos de continuidade
/Codebound/                     projeto Unity (abrir esta pasta no Unity Hub)
  Assets/_Codebound/
    Scripts/<Módulo>/Logic/     lógica pura + Codebound.<Módulo>.asmdef (noEngineReferences)
    Scripts/Game/               apresentação Unity (Codebound.Game.asmdef)
    Editor/                     ferramentas de editor (Codebound.Editor.asmdef)
    Tests/EditMode/             testes NUnit (correm no Unity E no dotnet)
    Scenes/ Data/ Art/ Audio/ …
  Packages/manifest.json
  ProjectSettings/ProjectVersion.txt   (Unity 6000.0 LTS)
/Runtime/                       serviço de execução de código (Etapa 3)
/Tools/LogicTests/              dotnet test da lógica + testes EditMode
/Tools/UnityCompileCheck/       compila Scripts/** contra DLLs de referência do Unity
/Tools/WorldPreview/            gera imagem de um mundo a partir de uma seed
/.claude/skills/                skills de sessão (continuidade e convenções)
```

## 3. Módulos (estado atual)

| Módulo | asmdef | Depende de | Conteúdo |
|---|---|---|---|
| Core | Codebound.Core | — | `GridPos`, `Vec2`, `Direction`, `EventBus`, `SimulationClock` (20 ticks/s), `SeededRandom` (SplitMix64), `ValueNoise`, `Loc`, `CbLog`, interfaces `IWalkabilityQuery`/`ITerrainBuildQuery` |
| Items | Codebound.Items | Core | `ItemDefinition`, `ItemStack`, `ItemRegistry`, `ItemCatalog` (ids estáveis `item.*`) |
| Inventory | Codebound.Inventory | Core, Items | `ItemInventory` (slots, stacking, operações atómicas, transferências) |
| World | Codebound.World | Core, Items | `TerrainType`, `ResourceNode`, `WorldMap` (limitado, Etapa 5 → chunks), `TestWorldGenerator` |
| Building | Codebound.Building | Core, Items, Inventory | `StructureDefinition`, `StructureCatalog`, `Footprint`, `BuildGrid` (validar/colocar/remover/reembolsar), `PlacedStructure` |
| Player | Codebound.Player | Core, Items, Inventory, World, Building | `PlayerMotor` (colisão AABB vs grelha, sub-passos anti-túnel), `InteractionSystem` (recolher, abrir armazenamento, inspecionar) |
| Energy | Codebound.Energy | Core | `PowerNode`, `Battery`, `PowerPole` (raio de alimentação 2, ligação 7), `PowerGrid` (redes por union-find, equilíbrio produção/consumo/baterias, *brown-out* proporcional) |
| Machines | Codebound.Machines | Core, Items, Inventory, World, Building, Energy | `Machine` (regra: só funciona controlada por um computador com programa a correr), `Computer` (hardware, programa, estado, ligações com alcance e portas), `Drill`, `CoalGenerator`, `MachineSystem` (sincronizado com o `BuildGrid`, corre no tick) |
| Save | Codebound.Save | todos os de lógica | `GameState` (liga todos os objetos da simulação), `SaveSystem` (formato binário versionado) |
| Game | Codebound.Game | todos + Unity.InputSystem | apresentação Unity (ver §1) |
| Editor | Codebound.Editor | Game | menu `Codebound/Create Test Scene` |

Módulos previstos (ainda não criados): Networks, Programming, Execution, HUD, UI, AI, Combat, Survival, Multiplayer, Research, Audio, Localization.

Regras: sem dependências cíclicas; comunicação entre módulos por interfaces do módulo inferior ou eventos no `EventBus`; dados referenciados por ids estáveis (`item.iron_ore`, `structure.wall`).

## 4. Decisões técnicas

| # | Decisão | Motivo |
|---|---|---|
| D1 | Unity 6000.0 LTS, C# 9 (`LangVersion 9` nas ferramentas) | Versão LTS atual; C# 9 é o que o compilador do Unity aceita (sem namespaces de ficheiro, sem `record struct`) |
| D2 | Lógica pura separada da apresentação (`Logic/` vs `Game/`) | Testes sem editor, determinismo, multiplayer |
| D3 | Simulação a tick fixo (20/s) | Independência do frame rate; base de CPU/energia dos computadores |
| D4 | Arte procedural (`PixelArt`) como placeholder | Projeto corre sem assets importados; substituível na Etapa 8 |
| D5 | Renderer Built-in por agora; migrar para URP 2D (luzes 2D) quando houver sessão com editor | Criar URP assets à mão sem editor é frágil |
| D6 | `GameInput` suporta Input System e Input Manager antigo; teclas físicas num dicionário de bindings | Funciona com qualquer configuração; remapeamento futuro só altera dados |
| D7 | HUD de desenvolvimento em IMGUI, assinalado como temporário | O HUD final é HTML/CSS programável (Etapa 4) |
| D8 | `GameBootstrap` cria-se automaticamente ao entrar em Play em qualquer cena | Projeto jogável sem cena serializada à mão |
| D9 | Colisão do jogador na lógica (não Physics2D) | Determinística, testável, igual no host multiplayer |
| D10 | Chão (`structure.floor`) ocupa a célula; camadas de construção (chão + objeto) ficam para a Etapa 2/4 | Simplicidade na Etapa 1 |
| D12 | "Todas as máquinas precisam de programa": uma máquina só atua ligada a um computador alimentado cujo programa está `Running`. O computador é o anfitrião dos programas e não precisa de outro computador | Respeita a regra do design sem dependência circular |
| D13 | Arranque energético: a **bateria de emergência da nave** (estrutura do mundo, não construível nem desmontável, 120 kJ) alimenta o primeiro computador, que programa o gerador | Resolve o "ovo e galinha" (o gerador precisa de programa, o computador precisa de energia) de forma narrativa |
| D14 | Unidades: watts e joules; 1 tick = 1/20 s. Gerador a carvão 300 W, 9 kJ por carvão, só gasta combustível pela carga pedida; perfuradora 90 W a trabalhar, 5 W em espera, 40 ticks por ciclo à potência total | Números simples e afináveis |
| D15 | Gravação: o terreno não é guardado (regenera-se da seed); formato binário com magic `CBSV` e versão; escrita para ficheiro temporário e depois rename | Ficheiros pequenos, migráveis, sem corrupção a meio da escrita |
| D16 | Ao carregar, programas que estavam `Running` voltam como `Loaded`: o runtime tem de os reiniciar | O estado interno do programa (memória Wasm) não é persistido nesta versão |
| D11 | Verificação sem Unity: `UnityEngine.Modules` 2021.3.33 (NuGet) só como referência de compilação local, nunca no repositório nem no jogo | Apanhar erros de API Unity sem o editor. Limitação: API 2021.3, sem Input System nem UnityEditor |

## 5. Execução de código do jogador (Etapa 3 — primeiro código em `Runtime/host`, **ainda não verificado**)

Requisitos: linguagens reais, fora do processo Unity, limites de CPU/RAM/tempo, permissões, interrupção segura, eventos + ciclos contínuos, hardware in-game a limitar a execução.

**Proposta: host de sandbox baseado em WebAssembly (Wasmtime, Apache-2.0) num processo separado.**
- `Codebound.Runtime.Host`: aplicação .NET 8 autónoma por plataforma (Win/Linux/macOS), lançada pelo jogo; fala com o Unity por stdio/socket local em JSON.
- Cada programa corre numa instância Wasm própria:
  - **CPU:** *fuel* do Wasmtime = orçamento de instruções por tick → mapeia diretamente para o CPU do computador in-game (programas ineficientes gastam mais).
  - **RAM:** limite de memória linear da instância = RAM in-game.
  - **Tempo:** epoch interruption para matar loops infinitos.
  - **Isolamento:** sem acesso a ficheiros/rede do sistema; só um sistema de ficheiros virtual do computador in-game e a API Codebound importada.
- Linguagens (a validar uma a uma, licenças incluídas):
  - JavaScript → QuickJS-ng compilado para WASI (MIT)
  - TypeScript → transpilação com o compilador TypeScript (Apache-2.0) e execução em QuickJS
  - Python → CPython WASI (PSF)
  - PHP → build WASI do PHP (PHP License)
  - SQL → SQLite WASI (domínio público)
  - C / C++ → compilação para Wasm (clang/wasi-sdk, Apache-2.0 com exceção LLVM); tamanho a avaliar
  - C# → por decidir (Roslyn + runtime .NET em Wasm, ou outra via); é a mais difícil de isolar
  - HTML / CSS → não executam: são interpretados pelo renderizador do HUD (subset seguro), e o JS do HUD corre na sandbox JS
- A API Codebound é definida uma vez (`/Runtime/api`, esquema JSON) e gerada/ligada por linguagem. Cada chamada é uma mensagem validada pelo Unity contra permissões e hardware do computador.

Primeiro passo da Etapa 3: provar o host Wasmtime com uma linguagem (JS/QuickJS ou Python), com testes que mostram que um loop infinito é interrompido e que o limite de memória é respeitado.

## 6. Verificação

| O quê | Como | Comando |
|---|---|---|
| Lógica + testes | NUnit no .NET 8 | `cd Tools/LogicTests && dotnet test` |
| Código Unity compila | Referências UnityEngine 2021.3 | ver README, "Verificação sem Unity" |
| Mundo gerado | Imagem PPM/PNG | `cd Tools/WorldPreview && dotnet run -- <seed> out.ppm` |
| Jogo no Unity | Manual, no editor | **ainda não feito** |
