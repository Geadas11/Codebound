# REA_FINDINGS — Resultados obtidos com a metodologia REA

## 1. O que o REA forneceu nesta sessão

| Operação REA | Resultado | Evidence ID |
|---|---|---|
| `inspect_pe_resources` (factorio.exe) | inventário completo de recursos (11, coverage `complete`); SHA-256 do exe `8cfdb711…5611f`; 2 grupos de ícones resolvidos com `size_matches` | `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6` |
| `binary_session` | diagnóstico dos providers: Ghidra `not_configured`, Hopper `unsupported_host` (win32), IDA `not_configured`, Kuna `target_format_unsupported` para PE | — (diagnóstico) |
| `open_binary` (provider ghidra) | recusado: `provider_unavailable`, `GHIDRA_INSTALL_DIR is not set`, mesmo depois de instalar o Ghidra 12.1.4 e configurar o `~/.claude.json`, porque o processo do servidor MCP `rea` não foi reiniciado | — (erro) |

## 2. Metodologia REA seguida
O fluxo da skill foi aplicado: rotear pelo tipo de alvo → diagnosticar o provider em falta → reparar a configuração com aprovação → continuar com o trabalho possível sem ele. Na prática:
1. **Rotear o alvo**: PE nativo x64 → `open_binary` + análise nativa; recursos PE → `inspect_pe_resources` (target-free) ✔.
2. **Diagnosticar o bloqueio** (`binary_session`): faltava um engine nativo. Instalei o Ghidra 12.1.4 (versão exigida pelo REA 6.7.0, SHA-256 verificado contra a release oficial), configurei `GHIDRA_INSTALL_DIR`/`JAVA_HOME` no registo MCP e confirmei com `rea doctor` (todas as verificações do Ghidra `ok`) ✔.
3. **Reconexão**: pendente do lado do utilizador (`/mcp` → Reconnect `rea`). Sem isso, as ferramentas REA nativas (`analyze_function`, `procedure_pseudo_code`, `xrefs`…) continuam indisponíveis nesta sessão ✘.
4. **Continuar com evidência suportada** (princípio da skill): parsers próprios, só de leitura, para PE e PDB (`scripts/`), extração de chamadas diretas a partir dos intervalos de função do PDB (`calls.py`, `callers.py`) e Ghidra headless direto (projeto em `ghidra/`). **Estes resultados não são Evidence REA** e estão rotulados como observações de ferramentas próprias. A distinção é exigida pela skill.

## 3. Ledger de achados (pergunta → resposta → evidência → confiança)

| Pergunta | Resposta | Evidência | Confiança |
|---|---|---|---|
| Versão/identidade | 2.1.24 build 87808, PE32+ MSVC 14.44, assinado Wube | VersionInfo, `pe_info.json`, Authenticode, REA ev_a990… | observado |
| PDB corresponde ao exe? | sim, GUID/age iguais | RSDS vs stream PDB info | observado |
| Arquitetura de módulos | 68 diretórios / 7 128 fontes | `factorio_src_files.txt` | observado |
| Ordem do tick | ver EXECUTION_FLOW | `calls_tick.txt`, `calls_subsystems.txt` | observado (chamadas diretas), parcial para chamadas virtuais |
| Paralelismo da simulação | 7 managers em 4 lotes de `runUpdate`, merge ordenado | descompilação Ghidra (GHIDRA_FINDINGS §1–2) | observado |
| RNG | taus88 | desmontagem manual de `RandomGenerator::getInt` | observado |
| Determinismo | ponto fixo, RNG, GC fixo, Lua sandbox | layouts, strings, chamadas | derivado |
| Formato do save | ZIP + fatias zlib 1 MiB + metadata | cópia de save [R/2.0.60] | observado (noutra versão) |
| Protocolo multiplayer | lockstep com TickClosures, 19 mensagens | enums PDB | derivado |

## 4. Unknowns a registar no REA após a reconexão
As perguntas abertas estão em UNKNOWN_COMPONENTS.md. Quando o `rea` tiver o Ghidra, cada uma pode ser registada com `record_unknown` e resolvida com `analyze_function` / `procedure_pseudo_code` / `xrefs`.

## 5. Limites do método usado em vez do REA nativo
- `calls.py` lê `call rel32` dentro de corpos de função exatos. Pode ter falsos positivos por bytes E8 dentro de outras instruções, mas são filtrados por exigir que o alvo seja um início de função conhecido. Não vê chamadas virtuais nem ramos.
- O parser de PDB não interpreta `S_INLINESITE` (funções inlined), símbolos locais nem linhas de código (C13 lines) [N].
- A primeira análise Ghidra headless (com tipos do PDB) encravou e foi parada. A segunda, só com nomes, completou-se em 35 min (GHIDRA_FINDINGS).

## 6. Evidence REA nativa (2026-11-24) — `artifacts/rea_evidence_bundle.json`
O REA (`open_binary`, provider Ghidra 12.1.4) foi configurado assim: `REA_GHIDRA_STARTUP_TIMEOUT_MS=14400000`, analisadores `PDB Universal` e `Decompiler Parameter ID` desligados, heap de 20 GB (`MAXMEM_DEFAULT` em `analyzeHeadless.bat`, porque o REA não passa `GHIDRA_HEADLESS_MAXMEM` ao Ghidra) e 8 threads de GC.

A análise completou-se e foram registadas 7 Evidence `confidence: observed`, `authority: shipped-artifact`. **O pseudocódigo coincide com o do projeto `ghidra2`** e mostra as constantes já resolvidas:

| Função | Endereço | Evidence | Confirma |
|---|---|---|---|
| `Map::updateEntities` | 0x140c6a3a0 | `ev_0972c0d0…` | 4 lotes de `runUpdate` (GHIDRA_FINDINGS §1) |
| runner multithread (`_Do_call`) | 0x1417cab60 | `ev_ca739857…` | prepare → N tarefas → sync → finish (§2) |
| `ElectricNetworkInputValues::computeUsage` | 0x14033f120 | `ev_488beafd…` | cascata por prioridade (§3) |
| `FluidBuffer::getFlowRate` | 0x140616590 | `ev_395bfaa8…` | 2⁻²⁴, **0,8**, mínimo 0x199999 (§4) |
| `NetworkMessageHeader::save` | 0x140d0dc30 | `ev_115c021e…` | byte de flags, bit aleatório, messageID (§6) |
| `Train::updateSpeed` | 0x14104a850 | `ev_cc612849…` | **v²·0,5/d**, **1000**, **0,001**, 0,0001 (§5) |
| `luaH_next` | 0x141d13e10 | `ev_85083707…` | iteração pela lista de inserção (§15) |

Sem o analisador PDB, o REA mostra as funções como `FUN_<endereço>`; os nomes vêm da correspondência de endereços com `pdb_proc_ranges.tsv`. O REA indica o PDB por GUID `f58d8e17-bce1-4fa0-9d3e-b184b8bfabae`, que é o mesmo GUID do RSDS noutra ordem de bytes.
