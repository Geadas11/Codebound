# FUNCTION_INVENTORY — Inventário de funções

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE]

> Aviso honesto: **não existe ainda um inventário de funções nativas** com endereços. Isto exige ler o `factorio.pdb` ou correr `list_procedures` do REA sobre os 33 MB de `.text`. Este documento lista o que é verificável hoje e o método para completar.

## 1. Escala do código nativo
[CONFIRMADO] `.text` = 33 056 980 bytes; `.pdata` = 1 239 060 bytes.
[INFERÊNCIA] ≈ 103 255 entradas RUNTIME_FUNCTION (1 239 060 / 12) → ordem de 100 mil funções com unwind (funções folha sem unwind não contam).

## 2. Classes/fontes C++ identificadas por strings (RTTI/asserts) [CONFIRMADO]
| Nome | Origem | Subsistema |
|---|---|---|
| `UDPSocket`, `UDPSocketInterface` | `@@` RTTI | Rede |
| `UDPSocket.cpp` | caminho de ficheiro-fonte | Rede |
| `ChromaController` | RTTI | Periféricos |
| `OpenGL`, `OpenGL::createInternalText...` | RTTI / mensagem | Render |
| `D3D11Device`, `D3D11DeviceContext`, `D3D11Debug` | strings | Render |
| `latencyInputHandler` | string | Rede/Input |
| `SteamContextInterface` | string | Plataforma |

## 3. Funções importadas (API Windows) [CONFIRMADO]
Ver DEPENDENCIES.md §2 — 20 DLLs, ~560 imports. As mais significativas para reconstrução:
- Tempo: `timeBeginPeriod`, `GetTickCount64`.
- Threads: `SubmitThreadpoolWork`, `TryAcquireSRWLockExclusive`, `InitOnceBeginInitialize`.
- Input: `RegisterRawInputDevices`, `GetRawInputData`, `ImmGetCompositionStringW`.
- Gráficos (WGL): `ChoosePixelFormat`, `SetPixelFormat`, `SwapBuffers`.
- Imagem: `GdipSaveImageToStream` ([INFERÊNCIA] screenshots/thumbnails).
- Cripto: `CryptGenRandom`, `CryptCreateHash`, `WinVerifyTrust`.

## 4. Funções Lua confirmadas (data/script)
| Função | Ficheiro | Estado |
|---|---|---|
| `handler.add_lib(lib)` | `core\lualib\event_handler.lua` (usado em `script\freeplay\control.lua`) | [CONFIRMADO] uso |
| `require("freeplay")`, `require("silo-script")`, `require("space-finish-script")` | freeplay control | [CONFIRMADO] |
| `remote.add_interface("freeplay", …)` | `freeplay.lua` | [CONFIRMADO] |
| `data:extend` | `core\lualib\dataloader.lua` | [INFERÊNCIA] definida aí |
| Módulos utilitários | `util`, `math2d`, `math3d`, `collision-mask-util`, `sound-util`, `meld`, `mod-gui`, `story`, `crash-site`, `kill-score`, `production-score` | [CONFIRMADO] ficheiros existem; funções internas **não** inventariadas |

## 5. Exports do executável
[CONFIRMADO] Export directory com 3 500 bytes. [HIPÓTESE] símbolos de seleção de GPU. Por listar.

## 6. Método para completar (só leitura)
1. Ler `factorio.pdb` com `llvm-pdbutil dump -publics` ou DIA SDK → lista nome+RVA.
2. Importar no REA (`set_addresses_names`) e agrupar por prefixo de classe.
3. Gerar tabela por subsistema: Simulation/Entity/Rendering/Network/Lua/GUI.
4. `xrefs` às strings de §2 para ligar funções a subsistemas.

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- [CONFIRMADO] Diretório de exports: RVA 0x27b3570, 3500 bytes, 140 nomes (138 da API C do Lua + `NvOptimusEnablement` + `AmdPowerXpressRequestHighPerformance`). Lista completa em `EVIDENCE_SCAN.txt`.
- Funções internas continuam por inventariar (requer PDB/REA).
