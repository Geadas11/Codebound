# REA_FINDINGS — Resultados das ferramentas REA e da análise PE

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE]

## 1. Ferramentas REA usadas
| Ferramenta | Resultado |
|---|---|
| `open_binary` | [CONFIRMADO] abriu `factorio.exe` como PE32+ x64. Evidence id `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6` |
| `inspect_pe_resources` | [CONFIRMADO] PNG id 103, grupos de ícones 16/32/48, RT_VERSION, RT_MANIFEST |
| `inspect_binary_layout` | falhou — só suporta Linux/ELF neste ambiente |

Limitação: não foram ainda usados `list_procedures`, `procedure_pseudo_code`, `xrefs`, `search_strings` sobre o binário de 49 MB. Ver RECONSTRUCTION_ROADMAP.md.

## 2. Cabeçalho PE (parser próprio, só leitura) — [CONFIRMADO]
- Machine `0x8664` (AMD64), 7 secções, subsystem 2 (GUI), linker 14.44.
- TimeDateStamp `1793108967` → 2026-10-27 13:49:27 UTC.
- Rich header presente.
- Export directory: RVA 41629040, tamanho 3500 bytes → **o exe exporta símbolos**. [HIPÓTESE] inclui `NvOptimusEnablement`/`AmdPowerXpressRequestHighPerformance` (pedido de GPU dedicada); não listado ainda.
- TLS directory: RVA 37771648 (40 bytes) → usa TLS estático.
- Debug directory: RVA 37771088 (84 bytes) → aponta para PDB.
- Certificate table: offset 49411072, 12240 bytes → **assinatura Authenticode presente** (validade não verificada).

## 3. Secções — [CONFIRMADO]
| Nome | RVA | VirtualSize | RawSize |
|---|---|---|---|
| .text | 0x1000 | 33 056 980 | 33 057 280 |
| .rdata | 0x1F88000 | 8 585 152 | 8 585 216 |
| .data | 0x27B8000 | 7 940 160 | 5 718 528 |
| .pdata | 0x2F4B000 | 1 239 060 | 1 239 552 |
| _RDATA | 0x307A000 | 1 296 | 1 536 |
| .rsrc | 0x307B000 | 40 184 | 40 448 |
| .reloc | 0x3085000 | 767 176 | 767 488 |

[INFERÊNCIA] `.pdata` com 1 239 060 bytes / 12 bytes por RUNTIME_FUNCTION ≈ **103 255 entradas de unwind** → ordem de grandeza de ~100 mil funções não-folha. É uma estimativa, não uma contagem de funções.
[INFERÊNCIA] `.data` tem VirtualSize > RawSize (~2.2 MB de BSS).
[INFERÊNCIA] Sem secções empacotadas (UPX etc.); código ~33 MB de `.text` normal.

## 4. PDB
[CONFIRMADO] `bin\x64\factorio.pdb` existe localmente; caminho de build `C:\Users\build\AppData\Local\Temp\factorio-build-JmH45D\bin\VanillaFinalReleasex64vsbt\factorio.pdb`.
[NÃO FEITO] Ainda não foi lido. Se for um PDB completo (não "stripped"), contém nomes de funções, classes e ficheiros-fonte — a maior fonte de evidência disponível. Prioridade nº1 no roadmap.

## 5. Strings de interesse — [CONFIRMADO] (contagens)
| Padrão | Exemplos |
|---|---|
| Lua | `Lua 5.2` |
| Rede | `UDPSocket.cpp`, `UDPSocketInterface@@`, `UDP Socket failed: %s`, `34197` (4×) |
| Sincronização | `desynchronisation: crc test (%s) failed for cr...`, `desync-report`, `desync on loading.` |
| Latência | `latency window`, `latency_in_ticks`, `latencyInputHandler`, `heartbeat(%s) behind the latency window`, `heartbeats_per_second` |
| Save | `level.dat`, `level-init.dat`, `script.dat`, `.zip` |
| Config | `mod-list.json`, `mod-settings.dat`, `player-data.json`, `achievements.dat`, `server-settings.json` |
| Render | `D3D11Device`, `OpenGL for rendering`, `Vulkan: Application reques...` (129×) |
| Periféricos | `ChromaSDK64.dll`, `ChromaController@@`, `SteamDeck` |
| Compressão | `ZSTD`, `zstd` |

Os nomes com `@@` são fragmentos de nomes decorados MSVC (RTTI/typeinfo) → [INFERÊNCIA] RTTI está ativo e expõe nomes de classes C++ (ex.: `UDPSocket`, `ChromaController`, `OpenGL`).

## 6. Dados Lua
[CONFIRMADO] 567 ficheiros `.lua` em `data\base` + `data\core`; `data\base` tem 7 770 ficheiros, `data\core` 1 272.
[CONFIRMADO] Contagem heurística de `type = "..."` (regex, inclui campos que não são protótipos): `recipe` 196, `technology` 193, `fluid` 52, `tile` 41, `item-subgroup` 113, `virtual-signal` 154, `noise-expression` 89, `collision-layer` 24, `custom-input` 14, `shortcut` 20. `item` aparece 836× mas inclui `type="item"` dentro de ingredientes/resultados → **não** é o número de itens.

## 7. Correções a relatórios anteriores
- "d3d11.dll importada estaticamente" → FALSO; carregamento dinâmico.
- "Poco/Boost/OpenSSL" → sem evidência textual; rebaixar a HIPÓTESE.
- Número de secções → 7 (confirmado).
- Rich header/MSVC → confirmado (linker 14.44 + Rich).
- Magic "FACO" do save → não verificado; manter HIPÓTESE.

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- [CONFIRMADO] REA deu `provider_unavailable` nesta sessão; os resultados abaixo vêm do scan Python.
- Exports: 140 (a [HIPÓTESE] sobre NvOptimus/AmdPowerXpress passa a [CONFIRMADO]).
- Recursos: 11; ícone/versão; título Chroma `Factorio`.
- Assinatura: Authenticode da Wube Software Ltd, válida até 2028-08-15.
- Strings: Lua 5.2, curl+Schannel, ZSTD, string de desync CRC; Box2D/FACO não encontrados.
