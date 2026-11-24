# BINARY_ANALYSIS — Executável e componentes compilados

Alvo: `C:\GOG Games\Factorio\bin\x64\factorio.exe`, SHA-256 `8cfdb7111ebb31d68441113ebdb0165c90cb3723f6cd6902d71339fe2275611f`.
Métodos: REA `inspect_pe_resources` (Evidence `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`), parser PE próprio (`scripts/pe_dump.py`, só leitura), parser MSF/PDB próprio (`scripts/pdb_dump.py`), extração de strings, Ghidra 12.1.4 headless (projeto `ghidra/`). Ver REA_FINDINGS.md para os limites de cada método.

## 1. Cabeçalho PE [C] (`artifacts/pe_info.json`)

| Campo | Valor | Leitura |
|---|---|---|
| Formato | PE32+ (magic 0x20B), machine 0x8664 | x86-64 |
| Linker | 14.44 | MSVC 2022 |
| Image base | 0x140000000 | padrão x64 |
| Entry point RVA | 0x1E3E848 | CRT `mainCRTStartup`/`WinMainCRTStartup` [I]; a resolução de nome fica para o Ghidra |
| Subsystem | 2 (GUI) | aplicação de janela; os modos "consola" (servidor) usam a mesma imagem [I] |
| DllCharacteristics | 0x8160 | HIGH_ENTROPY_VA + DYNAMIC_BASE (ASLR) + NX_COMPAT + TERMINAL_SERVER_AWARE. **Sem a flag GUARD_CF**, apesar de o PDB registar `-guard:cf` nas opções de compilação [C]: o Control Flow Guard não está ativo na imagem final |
| TLS | 1 900 bytes de dados, callbacks em VA 0x141F8B240 | variáveis `thread_local` / CRT [I] |
| `.pdata` | 103 255 RUNTIME_FUNCTION | ≈103 mil funções não-folha com unwind info [C]. O PDB lista 249 292 nomes, que incluem funções folha, inlines com símbolo e thunks |
| Security dir | 12 240 bytes | Authenticode válido (Wube Software Ltd) [C] |

### Secções [C]

| Secção | VSize | Função |
|---|---:|---|
| `.text` | 33 056 980 | código (≈31,5 MiB) |
| `.rdata` | 8 585 152 | constantes, vtables, strings, RTTI, import/export tables |
| `.data` | 7 940 160 (5 718 528 em disco) | globais inicializados + BSS |
| `.pdata` | 1 239 060 | tabela de exceções x64 |
| `_RDATA` | 1 296 | dados só de leitura do CRT |
| `.rsrc` | 40 184 | recursos (ver §4) |
| `.reloc` | 767 176 | relocalizações (ASLR) |

**Não há packer nem ofuscação** [C]: secções standard do MSVC, entropia típica, imports normais e PDB completo distribuído.

## 2. Imports — 20 DLLs, 0 delay-load [C]

| DLL | Nº | Uso evidenciado pelos nomes |
|---|---:|---|
| KERNEL32 | 236 | memória, threads, thread pool (`SubmitThreadpoolWork`), SList, SRW locks, ficheiros |
| USER32 | 145 | janelas, input raw (`GetRawInputData`), clipboard, monitores |
| WS2_32 | 30 | sockets (UDP multiplayer + TCP do libcurl), `getaddrinfo` |
| GDI32 | 27 | pixel formats (`ChoosePixelFormat`, `SwapBuffers` → contexto OpenGL WGL) |
| CRYPT32 | 18 | cadeias de certificados (`CertGetCertificateChain`): TLS via Schannel no libcurl [I] |
| WLDAP32 | 18 (por ordinal) | dependência transitiva do libcurl (suporte LDAP compilado) [I] |
| gdiplus | 15 | codificação de imagens (screenshots e pré-visualização dos saves em JPEG/PNG) [I] |
| ADVAPI32 | 14 | `CryptGenRandom`, `CryptCreateHash`, registo |
| ole32 / OLEAUT32 | 14 / 4 | COM: drag & drop (`RegisterDragDrop`), WMI/MediaFoundation [I] |
| SHELL32 | 13 | `SHGetFolderPathW` (APPDATA), `ShellExecuteW`, tray (`Shell_NotifyIconW`) |
| IMM32 | 11 | IME (input de texto CJK) |
| SETUPAPI | 8 | enumeração de dispositivos HID (joysticks/hidapi do SDL) |
| VERSION | 3 | `GetFileVersionInfoA` |
| imagehlp / WINTRUST | 3 / 1 | `ImageEnumerateCertificates`, `WinVerifyTrust`: verificação da própria assinatura (updater) [I] |
| WINMM | 2 | `timeBeginPeriod/timeEndPeriod`: resolução do timer do ciclo de 60 UPS [I] |
| DNSAPI | 2 | `DnsQuery_UTF8` (registos SRV/DNS) [I] |
| IPHLPAPI | 1 | `GetIpAddrTable` (broadcast LAN) [I] |
| SHLWAPI | 1 | ordinal 12 |

**Carregados dinamicamente** (strings, não imports) [C]: `d3d11`/`dxgi` (o log [R/2.0.60] mostra `[Direct3D11]` + `d3dcompiler_47.dll`), `opengl32.dll`, `vulkan-1.dll`, `DiscordHook64.dll` (só deteção de crash do overlay). O Vulkan aparece **apenas dentro do SDL3** (`libraries/SDL3/src/gpu/vulkan`, `SDL_Vulkan_LoadLibrary`). O renderizador próprio do Factorio não tem backend Vulkan: `src/Graphics/` só tem `DX11/`, `OpenGL/` e `Metal/` [C].

## 3. Exports — 140 nomes [C]
- `NvOptimusEnablement`, `AmdPowerXpressRequestHighPerformance`: pedem a GPU dedicada em portáteis híbridos.
- **138 funções da API C do Lua 5.2** (`lua_*`, `luaL_*`), incluindo as extensões não-standard `lua_isnumberorstringconvertabletonumber` e `lua_isstringornumberconvertabletostring` [C]. Isto prova que o Lua embutido é uma **variante modificada** [C].
  O motivo da exportação não está provado [H]: possivelmente o depurador/profiler externo (`src/DebugAdapter`, 24 ficheiros) ou ferramentas internas.

## 4. Recursos (REA `inspect_pe_resources`, coverage `complete`, 11 recursos) [C]
- `PNG` id 103 (6 813 B): logótipo/splash.
- `RT_ICON` 1–6 e `RT_GROUP_ICON` 101/102: dois grupos de ícones 16/32/48 px, 32 bpp. REA resolveu os candidatos com `size_matches: true`.
- `RT_VERSION` (700 B) e `RT_MANIFEST` id 1 (1 832 B).
- **Não há dados de jogo embutidos** em recursos. Todos os assets estão em `data/`.

## 5. PDB — a principal fonte de evidência [C]

| Métrica | Valor |
|---|---|
| Streams MSF | 1 095 (bloco 4 KiB, 112 659 blocos) |
| Módulos DBI | 1 079 |
| Ficheiros-fonte únicos | 10 712 (7 128 do Factorio, 470 SDL3, resto bibliotecas/CRT/SDK) |
| Procedimentos globais (S_PROCREF) | 249 292 |
| Símbolos públicos | 205 808 |
| Globais de dados | 14 408 |
| Registos de tipo (TPI) | 2 068 277 |
| Tipos nomeados definidos | 110 798 |
| Ligações tipo→fonte:linha (IPI) | 99 485 |

O PDB é **completo, não "stripped"**: inclui TPI (tipos com membros e offsets), IPI e símbolos privados de módulo. É o que torna possível reconstruir a arquitetura (SOFTWARE_ARCHITECTURE.md), as estruturas (DATA_STRUCTURES.md) e o inventário de funções (FUNCTION_INVENTORY.md) sem acesso ao código-fonte.

### Opções de compilação registadas (LF_BUILDINFO) [C]
`-Os -GL -Z7 -Zp8 -GF -Gy -Gw -Qspectre -guard:cf -Brepro -experimental:deterministic -Zc:char8_t -Zc:preprocessor -Zc:threadSafeInit- -Zc:sizedDealloc- -ZH:SHA_256 -TP`
- `-GL` (LTCG): explica módulos agregados e funções com inline entre unidades de compilação.
- `-Os`: otimização para tamanho em pelo menos parte das unidades [C]. As restantes podem usar outras flags; só foram lidas 3 amostras de BUILDINFO [N].
- `-Brepro`, `-experimental:deterministic`: **builds reprodutíveis/determinísticas** [C].
- `-Zc:threadSafeInit-`: estáticos locais sem guarda thread-safe. O motor controla explicitamente a inicialização [I].
- Sistema de build: **FASTBuild** (`fastbuild` aparece como raiz de fontes) [C].

## 6. Composição do código por origem [C] (ficheiros-fonte compilados, PDB)

```
Factorio (src/)          7 128  ████████████████████████████████████████
SDL3                       544  ███
Windows SDK headers        461  ███
UCRT / VC runtime        ~700  ████
curl                       238  █
Agui (GUI própria)         232  █
opus/vorbis/ogg            164  █
glm                         99
Lua                         55
zstd 40 · re2 34 · zlib 21 · open-vcdiff 18 · minizip 10 · SDL_Mixer 11 · outros < 10
```

## 7. Strings [C] (`artifacts/exe_strings.txt`, 180 207 strings ≥5 chars)
- Endpoints: `https://auth.factorio.com`, `https://mods.factorio.com`, `https://updater.factorio.com`, `pingpong1..4.factorio.com:34197` (descoberta do endereço público / NAT punch, ver NETWORKING_ANALYSIS.md).
- `force-opengl`, `disabled-opengl-extensions`, `direct3d11`, `opengl`, `opengles2`, `vulkan`: seleção de API gráfica.
- `Lua 5.2`; `local pairs = function(t) return next, t end -- avoid using __pairs in Lua 5.2+` (código Lua embutido, provavelmente do serializador `serpent`, `Util/LuaLibSerpent` [C]).
- Mensagens de diagnóstico com nomes de ficheiros `.cpp` (ex.: `ModManager.cpp:444` no log [R/2.0.60]). Os logs usam o nome real do ficheiro-fonte.

## 8. Análise dinâmica
- **Não foi executada** contra esta instalação [N]. Motivos: obrigação de preservar a instalação e o `%APPDATA%` partilhado; o jogo escreve config/log/crop-cache ao arrancar.
- Evidência de runtime disponível: o log e os saves da instalação 2.0.60 (identificados como [R/2.0.60]).
- Ver RECONSTRUCTION_ROADMAP.md §5 para um protocolo de análise dinâmica isolada (cópia num diretório próprio, `--config`/`--mod-directory`/`--no-log-rotation`, servidor headless).
