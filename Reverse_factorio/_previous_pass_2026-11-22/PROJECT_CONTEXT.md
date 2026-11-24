# PROJECT_CONTEXT — Contexto completo para retomar o trabalho

> Documento de passagem de sessão. Uma sessão nova do Claude Code deve ler **este ficheiro primeiro** e depois seguir a secção 9 ("Próximos passos exatos").
> Data da última atualização: 2026-11-23.

---

## 1. Objetivo do projeto

Engenharia reversa **só de leitura** do Factorio (versão GOG instalada via Hydra), usando a skill/MCP **REA** (`reverse-engineer-anything`), para produzir relatórios técnicos em português sobre arquitetura, motor, simulação, rede, saves, Lua/modding, desempenho e um plano de reconstrução de um motor *inspirado* (não uma cópia).

| Item | Caminho |
|---|---|
| Jogo (só leitura) | `C:\GOG Games\Factorio` |
| Executável alvo | `C:\GOG Games\Factorio\bin\x64\factorio.exe` (49 423 312 bytes) |
| PDB | `C:\GOG Games\Factorio\bin\x64\factorio.pdb` (461 451 264 bytes) |
| Outro ficheiro em bin\x64 | `ChromaAppInfo.xml` (339 bytes, ainda não lido) |
| Dados | `C:\GOG Games\Factorio\data\base` (7 770 ficheiros, versão 2.1.24), `data\core` (1 272 ficheiros) |
| Saída (única pasta onde se escreve) | `C:\GOG Games\FACTORIO_REVERSE_ENGINEERING` |
| Script auxiliar de parse PE/strings | `C:\Users\carlos\AppData\Local\Temp\claude\56cb13f2bce7b839\c841b7ce-3b55-4bbd-898d-7c5f0fa0fe0d\scratchpad\evidence.py` (pode ter sido limpo; recriar se faltar) |
| Transcrição da sessão anterior | `C:\Users\carlos\.claude\projects\C--GOG-Games-FACTORIO-REVERSE-ENGINEERING\c841b7ce-3b55-4bbd-898d-7c5f0fa0fe0d.jsonl` |

---

## 2. Regras obrigatórias (nunca violar)

1. **Responder sempre em português.**
2. **Só leitura sobre o jogo.** Nunca modificar, apagar, substituir, renomear ou corromper exe, PDB, mods, configs ou saves.
3. **Escrever apenas** em `C:\GOG Games\FACTORIO_REVERSE_ENGINEERING` (ou no scratchpad para ficheiros temporários).
4. **Não investigar** credenciais, tokens ou dados privados do Hydra. **Ignorar o GOG launcher.**
5. **Etiquetar cada afirmação:** `[CONFIRMADO]`, `[INFERÊNCIA]`, `[HIPÓTESE]`, `[CONHECIMENTO PÚBLICO]`.
6. **Nunca inventar** código, funções ou algoritmos; citar sempre a evidência (ficheiro, string, import, Evidence ID do REA).
7. **Não declarar a análise completa** enquanto houver lacunas em UNKNOWN_COMPONENTS.md.
8. **Não usar WebFetch** (rejeitado duas vezes pelo utilizador).
9. **Não sobrescrever** `RELATORIOS_PARA_AUDITORIA.txt` (se existir).
10. **Modo ponytail:** respostas concisas; terminar com uma linha sobre o que não foi verificado e os riscos.
11. **Não recriar o Factorio** (nome, assets, aparência). O roadmap é para um motor original inspirado na arquitetura.
12. Se for preciso correr o jogo (ex.: `--benchmark`, `--dump-data`), **pedir autorização** e usar só cópias em pastas temporárias.

---

## 3. Stack / ferramentas

- **REA (MCP `rea`)** — as ferramentas são *deferred*: depois de cada compactação recarregar os schemas com
  `ToolSearch` → `select:mcp__rea__binary_session,mcp__rea__open_binary,mcp__rea__binary_overview,mcp__rea__search_strings,mcp__rea__search_procedures,mcp__rea__xrefs,mcp__rea__close_binary,mcp__rea__inspect_pe_resources`
- **Limitações REA já observadas:**
  - `inspect_binary_layout` só aceita ELF → para PE usar `evidence.py`.
  - `inspect_signature` só aceita Mach-O → usar `Get-AuthenticodeSignature` no PowerShell.
  - Risco de lentidão/timeout com exe de 49.4 MB e PDB de 461.5 MB (≈440 MiB); se acontecer, registar a limitação e usar `evidence.py`.
- **Python** (evidence.py) para ler cabeçalhos PE, imports, secções e strings sem executar o jogo.
- **PowerShell / Glob / Grep / Read** para contagens nos ficheiros Lua de `data\`.

---

## 4. Factos estabelecidos (com etiqueta)

### 4.1 Binário
- PE32+ x64, 7 secções, `.text` ≈ 33 MB [CONFIRMADO].
- **20 DLLs importadas** [CONFIRMADO]. (A afirmação antiga "Sem DLLs" está ERRADA.)
- d3d11, OpenGL e Vulkan carregados **dinamicamente** (evidência por strings, não imports estáticos) [CONFIRMADO strings].
- Lua 5.2 e ZSTD embutidos [CONFIRMADO strings]; versão exata/modificações do Lua por verificar.
- Caminho PDB: `VanillaFinalReleasex64vsbt\factorio.pdb` [CONFIRMADO].
- Export Directory ≈ 3 500 bytes [CONFIRMADO tamanho]; conteúdo por listar.
- Authenticode ≈ 12 240 bytes [CONFIRMADO tamanho]; validade por verificar.
- Imports relevantes: `timeBeginPeriod`/`timeEndPeriod`, `SubmitThreadpoolWork`, `CloseThreadpoolWork`, SList (`InitializeSListHead`, `InterlockedPushEntrySList`), SRW locks, `GetTickCount64` [CONFIRMADO].
- `.pdata` sugere ~100k funções com unwind [INFERÊNCIA].

### 4.2 Rede
- Strings: porta UDP 34197, `desync-report`, `heartbeat`, `latencyInputHandler`, `server-settings.json`, `latency_in_ticks` [CONFIRMADO strings].
- Modelo lockstep determinístico cliente-servidor [INFERÊNCIA forte].

### 4.3 Save
- Zip com `level.dat`, `level-init.dat`, `script.dat` [CONHECIMENTO PÚBLICO / strings].
- Magic "FACO" **NÃO verificado** — não apresentar como facto.

### 4.4 Lua / dados
- `storage` (2.x), `event_handler.add_lib`, `remote.add_interface("freeplay")`, `prototypes` (não `game.prototypes`), `script.active_mods["space-age"]` em `script\freeplay\control.lua` [CONFIRMADO].
- Lualib: `meld.lua`, `util.lua`, `mod-gui.lua`, `collision-mask-util.lua`, `data-duplicate-checker.lua` [CONFIRMADO ficheiros].
- Em `data\` só existem `base` e `core` (sem pastas DLC instaladas) [CONFIRMADO].
- Contagens de protótipos: `noise-expression` 89, `noise-function` 28, `autoplace-control` 12, `collision-layer` 24 [CONFIRMADO].
- Contagens GUI `type=`: frame 64, flow 50, button 47, label 135, table 36, sprite-button 12 [CONFIRMADO].

---

## 5. Estado dos ficheiros na pasta de saída

Existem `PROGRESS.md`, `.claude\settings.local.json` e **27 relatórios**:

**13 antigos (têm enchimento e afirmações erradas — reescrever):**
BINARY_ANALYSIS, DATA_STRUCTURES, ENGINE_ANALYSIS, EXECUTION_FLOW, EXECUTIVE_SUMMARY, FILE_STRUCTURE, INSTALLATION_ANALYSIS, LOGISTICS_ANALYSIS, LUA_ANALYSIS, RENDERING_ANALYSIS, SIMULATION_ANALYSIS, SOFTWARE_ARCHITECTURE, SUBSYSTEMS.

**14 novos (curtos, baseados em evidência):**
ADVANCED_MODDING, ALGORITHMS, ARCHITECTURE_DIAGRAMS, DEPENDENCIES, FUNCTION_INVENTORY, MODDING_API, MULTIPLAYER_SCALABILITY, NETWORKING_ANALYSIS, PERFORMANCE_ANALYSIS, PHYSICS_ANALYSIS, REA_FINDINGS, RECONSTRUCTION_ROADMAP, SAVE_SYSTEM, UNKNOWN_COMPONENTS.

**Já relidos nesta ronda:**
| Ficheiro | Estado |
|---|---|
| ADVANCED_MODDING | OK |
| MULTIPLAYER_SCALABILITY | OK |
| RECONSTRUCTION_ROADMAP | OK |
| PERFORMANCE_ANALYSIS | **Bug:** linha 13 "Sem DLLs, 33 MB de `.text`" → deve ser "20 DLLs importadas, 33 MB de `.text`" |
| UNKNOWN_COMPONENTS | Lacuna #14 (pastas DLC) já resolvida: só base+core. Restantes pendentes (ver §7) |

**Por reler:** ALGORITHMS, ARCHITECTURE_DIAGRAMS, DEPENDENCIES, FUNCTION_INVENTORY, MODDING_API, NETWORKING_ANALYSIS, PHYSICS_ANALYSIS, REA_FINDINGS, SAVE_SYSTEM, os 13 antigos e PROGRESS.md.

---

## 6. Bugs conhecidos nos relatórios (afirmações a retirar)

Fazer grep por estas expressões em todos os `.md`:
1. **"Sem DLLs"** / "sem dependências" → errado (20 DLLs importadas).
2. **d3d11 como import estático** → errado (carregamento dinâmico).
3. **Poco, Boost, OpenSSL** → sem evidência; remover ou marcar [HIPÓTESE] com "não encontrado".
4. **`game.prototypes`** → API 1.x; em 2.x é `prototypes`.
5. **Magic "FACO" como facto** → só [HIPÓTESE] até se abrir a cópia de um save.

---

## 7. Lacunas ainda abertas (de UNKNOWN_COMPONENTS.md)

| # | Lacuna | Como resolver (só leitura) |
|---|---|---|
| 2 | Lista de exports | evidence.py: ler Export Directory |
| 3 | Cliente HTTP/TLS | `search_strings` curl_/Schannel; xrefs `CertGetCertificateChain` |
| 4 | Algoritmo CRC de desync | `search_strings` "crc"; PDB; xrefs |
| 6 | Magic FACO | abrir **cópia** de um save de teste |
| 7 | Ordem das fases do tick | `search_procedures` Map::update / Surface::update |
| 8 | Representação de posições | `search_procedures` MapPosition / RealOrientation |
| 10 | Correias | `search_procedures` TransportLine |
| 12 | Backend gráfico por omissão | `search_strings` graphics-api |
| 13 | Uso do ZSTD | xrefs a strings ZSTD |
| 16 | Box2D/PhysX/Bullet | `search_strings` (confirmar ausência) |
| 17 | Validade Authenticode | `Get-AuthenticodeSignature` |
| 18 | Versão exata do Lua | `search_strings` "Lua 5." / `$LuaVersion` |
| 19 | ChromaAppInfo.xml | Read do ficheiro |

Lacunas #1, #5, #9, #11, #15, #20 continuam também abertas (PDB completo, formato `level.dat`, pathfinding, threads, contagem exata de itens, pasta `%APPDATA%\Factorio` — esta só listar).

---

## 8. Histórico: tentativas e o que não funcionou

- Prometi 27 relatórios e entreguei 13; os 14 em falta foram escritos depois.
- O utilizador perguntou pela diferença de tamanhos: os antigos têm enchimento; os novos só evidência.
- `inspect_binary_layout` falhou (só ELF) → criado `evidence.py`.
- `inspect_signature` não aplicável a PE → usar PowerShell.
- WebFetch rejeitado duas vezes → não voltar a usar.
- Várias compactações de contexto; os schemas REA perdem-se a cada uma (recarregar com ToolSearch).

---

## 9. Próximos passos exatos

1. **Reler** os ficheiros listados em §5 "Por reler" e fazer grep pelas afirmações de §6. Anotar correções.
2. **Correr o REA** sobre o exe:
   1. Recarregar schemas (ToolSearch, ver §3).
   2. `open_binary` com `{"path":"C:\\GOG Games\\Factorio\\bin\\x64\\factorio.exe"}`.
   3. `binary_overview`.
   4. `search_strings`: `crc`, `desync`, `ZSTD`, `curl`, `Schannel`, `Lua 5.`, `FACO`, `level.dat`, `Box2D`, `graphics-api`.
   5. `search_procedures`: `TransportLine`, `MapPosition`, `Map::update`.
   6. `xrefs` sobre os resultados relevantes.
   7. `inspect_pe_resources` com o caminho do exe.
   8. `close_binary` no fim (guardar `snapshot_path`).
3. Fora do REA (só leitura): `Get-AuthenticodeSignature` no exe; exports via `evidence.py`; Read de `ChromaAppInfo.xml`.
4. **Reescrever os 27 relatórios e o PROGRESS.md**: etiquetas em cada afirmação, Evidence IDs do REA, sem as afirmações de §6, UNKNOWN_COMPONENTS atualizado. **Não tocar** em `RELATORIOS_PARA_AUDITORIA.txt`.
5. Terminar com um resumo curto + linha de riscos/não verificado.

---

## 10. Perguntas originais do utilizador ainda a responder nos relatórios

- Quantos itens/entidades tem o jogo (contagem exata ainda é a lacuna #15; dar só contagens confirmadas).
- É possível reconstruir o Factorio? Em Unity? → resposta em RECONSTRUCTION_ROADMAP: só um motor original inspirado; Unity possível para protótipo, mas determinismo exige evitar física/float do motor; C++/Rust mais próximo da arquitetura observada.
