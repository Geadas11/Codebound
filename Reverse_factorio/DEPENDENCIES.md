# DEPENDENCIES — Bibliotecas e dependências

Fonte principal: caminhos `factorio-build-JmH45D/libraries/<nome>/` nos ficheiros-fonte do PDB e nomes `.lib` dos módulos DBI (`artifacts/pdb/pdb_source_files.txt`, `pdb_modules.tsv`) [C]. Todas as bibliotecas de terceiros estão **ligadas estaticamente**: não há DLLs de terceiros na instalação [C].

## 1. Bibliotecas de terceiros embutidas

| Biblioteca | Ficheiros-fonte | `.lib` (módulos) | Função no Factorio | Evidência adicional |
|---|---:|---|---|---|
| **SDL3** | 544 | `sdl.lib` (219) | janela, input, joystick/haptic/sensor, áudio (WASAPI/DirectSound), threads, IO | `libraries/SDL3/src/*`; o antigo src "joystick/video/haptic" é SDL |
| **SDL_Mixer** | 11 | `sdl-mixer.lib` (8) | mistura de áudio | log [R/2.0.60] `[Audio] Driver:wasapi` |
| **Lua 5.2 (modificado)** | 55 | `lua.lib` (28) | scripting de dados e de runtime | exports `lua_*`; `LuaCPPUtilities.hpp`, `override_printf.h` são adições [C] |
| **curl** (libcurl) | 238 | `curl.lib` (105) | HTTP(S): auth, portal de mods, updater, upload de logs | `src/Http/CurlManager`, `CurlHandle` [C]; strings `CURLOPT_*` |
| **zstd** | 40 | `zstd.lib` (18) | compressão | `Util/ZstdUtil` [C]; usado em blueprints, UDP de scripts e comandos remotos, não no save [C/G] |
| **zlib** | 21 | `zlib.lib` (10) | deflate dos saves (`level.datN` = streams zlib) | observado no save [R/2.0.60] |
| **minizip** | 10 | `minizip.lib` (5) | ZIP dos saves/mods/cenários | `Package/ZipPackage*` [C] |
| **open-vcdiff** | 18 | `open-vcdiff.lib` (7) | patches binários delta (VCDIFF) do **updater** [I] | `Updater/UpdaterDownloader` [C] |
| **re2** | 34 | `re2.lib` (17) | expressões regulares | `Util/RegexUtil` [C] |
| **opus / opusfile / ogg / vorbis** | 115/6/4/39 | sim | descodificação de áudio `.ogg` | 1 943 ficheiros `.ogg` |
| **glm** | 99 | header-only | matemática vetorial (gráficos) | `src/API/glm.h` [C] |
| **glad** | 3 | `glad.lib` | loader de funções OpenGL | |
| **Agui** | 232 | `Agui.lib` (21) | **toolkit de GUI** (widgets, layouts, estilos) usado por `src/Gui` | biblioteca de terceiros, muito adaptada [I] |
| **CommonUtil** | 25 | `common-util.lib` (9) | utilitários partilhados com outras ferramentas Wube [I] | |
| **fontstash / sdf** | 3 / 1 | `fontstash.lib` | rasterização de fontes / signed distance fields | |
| **stb_image** | 5 | `stb.lib` | descodificação de imagens (PNG) | |
| **StackWalker** | 2 | `stack-walker.lib` | stack traces de crash | `Util/CrashHandler`, `StackWalkerToStream` [C] |
| **crc32fast** | 2 | — | CRC32 rápido | `Util/Crc` [C]; usado nos checksums de mods e de desync [I] |
| **cxxopts** | 1 | — | parsing da linha de comandos | `Main/Options` [C] |
| **imgui** | 1 | — | ferramentas internas de debug | `Debug/ImguiSDL` [C] |
| **jc_voronoi** | 1 | — | diagramas de Voronoi | `Noise/VoronoiNoise` [I] |
| **tracy** | 4 | — | profiler (instrumentação compilada mas provavelmente inativa) [H] | |
| **trio** | 9 | `trio.lib` | printf portátil | |
| **CL** | 3 | — | headers OpenCL (`src/API/OpenCL`) [C]; nenhuma função compilada, código morto [C/G] |
| **LogitechLEDLib, Razer Chroma, SteelSeries** | — | `LogitechLEDLib.lib` | iluminação RGB de periféricos | `src/LED/*`, `ChromaAppInfo.xml` [C] |

## 2. Runtime e SDK Microsoft
- UCRT estático (`libucrt.lib`, 309 módulos), `LIBCMT`, `libcpmt` (STL), `libvcruntime`, ATL (`atls.lib`, `comsuppw.lib`) [C]. **Não depende de `vcruntime140.dll`/`msvcp140.dll`** [C]: não aparecem nos imports.
- `wbemuuid.lib` (WMI: informação de hardware para o log `System info`/`Memory info` [R/2.0.60]) [I].

## 3. Dependências de sistema (dinâmicas)
Ver BINARY_ANALYSIS §2: 20 DLLs do Windows importadas e D3D11/DXGI/d3dcompiler_47/OpenGL/Vulkan carregados em runtime.

## 4. Dependências externas (rede/serviços) [C] strings
| Serviço | URL | Uso |
|---|---|---|
| Autenticação | `https://auth.factorio.com` | login, tokens de servidor |
| Portal de mods | `https://mods.factorio.com` | download/atualização de mods (`Package/`, `ModPortal/`) |
| Updater | `https://updater.factorio.com` | patches (`open-vcdiff`) |
| Ping/NAT | `pingpong{1..4}.factorio.com:34197` | descoberta do IP público, NAT punch-through |
| Matching server | `Net/MatchingServer` | lista pública de jogos |
| Steam/Discord | `src/SteamIntegration` (12 ficheiros) | **não** foram encontradas strings `steam_api` nesta build GOG [C]; só existe a interface base com métodos vazios, ou seja, um stub [C/G] |

## 5. Dependências que NÃO existem (contra afirmações anteriores)
Procurei nos ficheiros-fonte do PDB, nos namespaces de procedimentos e nas strings **Boost, Poco, OpenSSL, Box2D, PhysX, Bullet, Qt, FMOD e Wwise**: zero ocorrências reais [C].
Houve apenas falsos positivos, verificados um a um:
- `curl/lib/vtls/openssl.h`: header do curl incluído, mas não é o backend em uso;
- `fmod.asm`/`fmodf.c` do CRT: a função matemática `fmod`, não a biblioteca de áudio;
- a string `bullet_points`.

A física e as colisões são implementação própria (`src/Collision`, `Util/BoundingBox`). **O TLS do libcurl usa Schannel** [C]: strings `schannel: AcquireCredentialsHandle failed`, `schannel: TLS 1.3 is not yet supported` e os imports de CRYPT32.
