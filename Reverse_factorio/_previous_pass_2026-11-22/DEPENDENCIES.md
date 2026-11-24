# DEPENDENCIES — Dependências do Factorio (Windows x64, GOG)

**Legenda:** [CONFIRMADO] = lido diretamente no binário/ficheiro nesta investigação · [INFERÊNCIA] = conclusão lógica a partir de evidência · [HIPÓTESE] = plausível mas não verificado.

Alvo: `C:\GOG Games\Factorio\bin\x64\factorio.exe` (SHA256 `8cfdb7111ebb31d68441113ebdb0165c90cb3723f6cd6902d71339fe2275611f`, 49 423 312 bytes).
Método: parser PE próprio em Python, só leitura (`open(..., "rb")` + `mmap ACCESS_READ`). Nenhum ficheiro do jogo foi alterado.

## 1. Conteúdo de `bin\x64`
[CONFIRMADO] Só existem `factorio.exe`, `factorio.pdb` e `ChromaAppInfo.xml`. Não há nenhuma DLL junto do executável.
[INFERÊNCIA] Todas as bibliotecas de terceiros (Lua, compressão, rede, áudio, imagem) estão ligadas estaticamente.

## 2. Tabela de imports (estática) — [CONFIRMADO]
| DLL | Nº funções | Funções relevantes / significado |
|---|---|---|
| KERNEL32.dll | 236 | runtime base, threads, thread pool (`SubmitThreadpoolWork`, `CloseThreadpoolWork`), SRW locks, `InitOnce*` |
| USER32.dll | 145 | janelas, input, Raw Input (`RegisterRawInputDevices`, `GetRawInputData`), clipboard, hooks |
| GDI32.dll | 27 | `ChoosePixelFormat`, `SetPixelFormat`, `SwapBuffers`, `DescribePixelFormat` (WGL/OpenGL), DIBs |
| gdiplus.dll | 15 | codificação/decodificação de imagens (`GdipSaveImageToStream`, `GdipGetImageEncoders`) |
| ole32.dll | 14 | COM (`CoCreateInstance`), drag&drop (`RegisterDragDrop`) |
| OLEAUT32.dll | 4 | por ordinal (#2,#6,#8,#9 = BSTR/VARIANT) |
| SHELL32.dll | 13 | `ShellExecuteW`, `SHGetFolderPathW`, `DragAcceptFiles`, `SHOpenFolderAndSelectItems`, `Shell_NotifyIconW` |
| SHLWAPI.dll | 1 | ordinal #12 |
| IMM32.dll | 11 | IME (entrada CJK) |
| WINMM.dll | 2 | `timeBeginPeriod`/`timeEndPeriod` (resolução do timer) |
| SETUPAPI.dll | 8 | enumeração de dispositivos (`SetupDiGetClassDevsA`, `CM_Get_Parent`) |
| ADVAPI32.dll | 14 | registo + CryptoAPI (`CryptGenRandom`, `CryptCreateHash`, `CryptEncrypt`, `CryptImportKey`) |
| CRYPT32.dll | 18 | certificados (`CertGetCertificateChain`, `PFXImportCertStore`, `CryptQueryObject`) |
| WINTRUST.dll | 1 | `WinVerifyTrust` |
| imagehlp.dll | 3 | `ImageEnumerateCertificates`, `ImageGetCertificateData/Header` |
| VERSION.dll | 3 | `GetFileVersionInfoA`, `VerQueryValueA` |
| WS2_32.dll | 30 | sockets (maioria por ordinal), `getaddrinfo`, `WSAIoctl` |
| IPHLPAPI.DLL | 1 | `GetIpAddrTable` |
| DNSAPI.dll | 2 | `DnsQuery_UTF8`, `DnsFree` |
| WLDAP32.dll | 18 | LDAP (por ordinal) |

[CONFIRMADO] Delay-import directory = vazio (0,0).
[CONFIRMADO] **Não** há imports estáticos de `d3d11.dll`, `dxgi.dll`, `opengl32.dll`, `vulkan-1.dll`, `xaudio2`, `dsound`, `steam_api64.dll` nem `ChromaSDK64.dll`.

## 3. Bibliotecas carregadas dinamicamente
[CONFIRMADO] Strings no binário: `D3D11CreateDeviceAndSwapChain`, `D3D11CreateDevice`, `D3D11Device`(30×), `OpenGL::createInternalText...`, `Vulkan instance.`, `Vulkan Portability library`, `ChromaSDK64.dll`, `SteamOverlayProcessCapture`.
[INFERÊNCIA] D3D11, OpenGL (`opengl32.dll`), Vulkan, Razer Chroma e Steam são carregados em runtime (`LoadLibrary`/`GetProcAddress`), por isso não aparecem na tabela de imports.
[HIPÓTESE] Isto permite ao mesmo executável arrancar em máquinas sem um destes runtimes, mas não foi verificado por análise dinâmica.

## 4. Bibliotecas estaticamente embutidas
| Componente | Evidência | Estado |
|---|---|---|
| Lua 5.2 | strings `Lua 5.2` e `Lua 5.2+` | [CONFIRMADO] presença; [INFERÊNCIA] versão modificada pela Wube (não verificável sem diff) |
| LuaJIT | 0 ocorrências | [CONFIRMADO] ausente |
| Zstandard | `ZSTD`(2×), `zstd`(1×) | [CONFIRMADO] referência; [INFERÊNCIA] usado em compressão (saves/rede?) |
| zlib/deflate | padrão "deflate x.y Copyright" não encontrado | [HIPÓTESE] pode existir sem string de copyright; saves são `.zip` (`.zip` 16×) |
| Cliente HTTP/TLS (tipo libcurl) | imports WLDAP32+DNSAPI+CRYPT32 típicos de libcurl estático com Schannel; string `libcurl/` **não** encontrada | [HIPÓTESE] |
| OpenSSL | 0 ocorrências de `OpenSSL x` | [CONFIRMADO] sem marcador; [INFERÊNCIA] TLS provavelmente via Schannel/CryptoAPI |
| Boost, Poco, SDL, Allegro, FreeType, libpng | 0 ocorrências das strings procuradas | [CONFIRMADO] sem marcador textual. **Correção:** relatórios anteriores que afirmavam Poco/Boost/OpenSSL devem ser lidos como HIPÓTESE não suportada |
| Tracy / crashpad / breakpad | 0 ocorrências | [CONFIRMADO] sem marcador |
| Razer Chroma SDK | `ChromaSDK64.dll`, `ChromaController@@`, `ChromaAppInfo.xml` | [CONFIRMADO] |
| Steam | `Steam`, `SteamDeck`, `SteamVirtualGamepadInfo` | [CONFIRMADO] referências; [INFERÊNCIA] código opcional ativo só quando corre via Steam |

Nota: ausência de string ≠ ausência da biblioteca (strings podem ser removidas). Só a análise do PDB (símbolos) resolve isto — ver UNKNOWN_COMPONENTS.md.

## 5. Toolchain
[CONFIRMADO] Linker 14.44, subsystem 2 (Windows GUI), Rich header presente, timestamp PE `1793108967` = 2026-10-27 13:49:27 UTC.
[CONFIRMADO] Caminho PDB: `C:\Users\build\AppData\Local\Temp\factorio-build-JmH45D\bin\VanillaFinalReleasex64vsbt\factorio.pdb`.
[INFERÊNCIA] MSVC (Visual Studio 2022, toolset 14.44); configuração de build "VanillaFinalRelease x64"; build automatizado numa máquina/utilizador `build`.

## 6. Dependências de dados (mods)
[CONFIRMADO] `data\base\info.json`: `base` 2.1.24, autor Wube Software, sem dependências. `core` é carregado sempre (contém `lualib`).
[CONFIRMADO] Strings de ficheiros de configuração: `mod-list.json`, `mod-settings.dat`, `player-data.json`, `achievements.dat`, `server-settings.json`.

## 7. Fronteira Factorio vs. Hydra
Nenhum import ou string analisada referencia o launcher Hydra. [INFERÊNCIA] O executável é autónomo; o Hydra apenas o lança. Dados do Hydra não foram investigados (restrição do utilizador).

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- [CONFIRMADO] libcurl com backend Schannel (`schannel: failed to send close msg` @0x23bf068); OpenSSL não encontrado.
- [CONFIRMADO] ZSTD usado no cache do atlas de sprites (`ZSTD Compress/Decompress Error`, `Sprite atlas cache saving fa…` @0x222b768).
- [CONFIRMADO] Box2D: 0 ocorrências no exe e no PDB.
- [CONFIRMADO] Assinatura Authenticode da Wube Software Ltd, válida até 2028-08-15.
