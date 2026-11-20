# Registo de desenvolvimento

Entradas mais recentes no topo.

## 2026-11-20 — Sessão 1 (continuação) — Etapa 2 e gravação

**Feito:**
- Regra de colocação "sobre recurso" para perfuradoras; a interação dá prioridade à estrutura sobre o minério por baixo.
- Módulo Energy: postes, redes, baterias, equilíbrio por tick e falta de energia proporcional.
- Módulo Machines: regra "sem programa válido não funciona", `Computer` (hardware, programa, ligações), `Drill`, `CoalGenerator`, `MachineSystem`.
- Bateria de emergência da nave colocada no início do mundo (resolve o arranque energético, D13).
- Unity: sprites novos (rodados conforme a orientação), painel de inspeção com estado da rede e do hardware, modo de ligação de máquinas, estado das máquinas nas janelas de armazenamento, textos pt-PT/en.
- Módulo Save: `GameState` + `SaveSystem` (binário versionado); F5/F9 no jogo.

**Verificado:** `dotnet test` → **59/59**; `Tools/UnityCompileCheck` compila. Não verificado no Unity.

**Decisões:** D12–D16 (`TECHNICAL_ARCHITECTURE.md`).

**Problemas / bloqueios:** o sandbox (Etapa 3) continua por executar; não houve nova tentativa. Enquanto não houver runtime, nenhuma máquina funciona no jogo, e o painel do computador diz isso explicitamente.

**Commits:** `8cb91c6` lógica da Etapa 2 · `adaccf3` integração Unity · `d349829` gravação · (este) documentação

## 2026-11-20 — Sessão 1 — Fundação (Etapa 1) e início do runtime de código

**Feito:**
- Skills de projeto `codebound-session` (continuidade) e `unity-conventions` (arquitetura e estilo).
- Esqueleto do projeto Unity 6 (`Codebound/`): manifest de pacotes, versão, asmdefs por módulo, `.gitignore`.
- Lógica pura (sem UnityEngine): Core (grelha, direções, EventBus, relógio a 20 ticks/s, RNG determinístico, ruído, localização, logging), Items, Inventory, World (mapa + gerador do mundo de teste com local do despenhamento, sucata, minérios, árvores, lagos e rocha), Building (multi-célula, rotação, validação de obstáculos e custo, colocação, desmontagem com reembolso), Player (movimento com colisão deslizante e sub-passos, interação/recolha).
- Camada Unity (`Scripts/Game`): bootstrap automático, arte pixel procedural, terreno em Tilemap, nós de recurso, estruturas, jogador animado, câmara pixel-perfect com zoom, modo de construção com fantasma verde/amarelo/vermelho, interação com destaque, HUD de desenvolvimento IMGUI (inventário, paleta de construção com custos, janela de armazenamento com transferências, avisos), textos pt-PT/en com troca por F1, menu de editor "Create Test Scene".
- Ferramentas: `Tools/LogicTests`, `Tools/UnityCompileCheck`, `Tools/WorldPreview`.
- Documentos: README, GAME_DESIGN, TECHNICAL_ARCHITECTURE, TASKS, NEXT_SESSION.
- Etapa 3 antecipada (justificação: a primeira máquina programável da Etapa 2 depende da execução de código): host de sandbox `Runtime/host/Codebound.Runtime.Sandbox` (Wasmtime: fuel = CPU, limite de memória, timeout por epochs, sem diretórios do anfitrião) e testes com CPython 3.12 WASI — **escrito, não executado**.

**Verificado:**
- `dotnet test` em `Tools/LogicTests`: **42/42 testes passam** (lógica + testes EditMode partilhados).
- `dotnet build` em `Tools/UnityCompileCheck`: todo o `Scripts/**` compila contra as referências UnityEngine 2021.3 (o ramo do Input System e os scripts de Editor não estão cobertos).
- Imagem do mundo da seed 2026 gerada e inspecionada: espaço central livre, recursos iniciais por perto.
- **Não verificado:** abrir/jogar no Unity 6 (sem editor nesta sessão); host de sandbox (execução dos testes bloqueada, ver abaixo).

**Decisões:** D1–D11 em `TECHNICAL_ARCHITECTURE.md` §4; proposta do sandbox em §5 (WebAssembly + Wasmtime, fuel ↔ CPU in-game, memória ↔ RAM in-game).

**Problemas / bloqueios:**
- Sem editor Unity no ambiente de desenvolvimento → a integração Unity está marcada `[~]`.
- A execução dos testes do sandbox (correr CPython WASI descarregado sob Wasmtime) foi **recusada pelo verificador de segurança do modo automático** desta sessão. O código foi guardado mas não compilado nem executado. Precisa de autorização do utilizador para continuar (ver `NEXT_SESSION.md`).
- `UnityEngine.Modules` no NuGet só existe para 2021.3; a verificação de APIs exclusivas do Unity 6 só será possível no editor.

**Commits:** `984fb53` skills · `0e23a5f` lógica + testes · `dc852ad` camada Unity e ferramentas · (este) documentação e sandbox não verificado
