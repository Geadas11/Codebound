# FILE_STRUCTURE — Inventário de ficheiros

Inventário recursivo completo (12 450 ficheiros, 2,39 GB) em **`artifacts/file_inventory.csv`** (caminho, bytes, extensão) e resumo por diretório em **`artifacts/dir_summary.tsv`** [C]. Gerado por varrimento só de leitura (`os.walk`).

## 1. Por diretório

| Diretório | Ficheiros | MB | Conteúdo |
|---|---:|---:|---|
| `bin/x64` | 3 | 510,9 | `factorio.exe` (47 MB), `factorio.pdb` (440 MB), `ChromaAppInfo.xml` |
| `data/base/graphics` | 4 673 | 1 137,2 | sprites PNG das entidades, terreno, ícones e 362 ficheiros `.lua` de definição gráfica |
| `data/base/sound` | 1 716 | 154,2 | áudio `.ogg` |
| `data/base/campaigns` | 570 | 19,6 | campanhas `tight-spot`, `transport-belt-madness`, `tutorial` |
| `data/base/scenarios` | 346 | 27,6 | `freeplay`, `pvp`, `rocket-rush`, `sandbox`, `supply`, `team-production`, `wave-defense` |
| `data/base/tutorials` | 251 | 1,8 | tutoriais interativos |
| `data/base/prototypes` | 105 | 3,4 | definições Lua de protótipos (itens, receitas, entidades, tecnologias) |
| `data/base/script` | 25 | 1,2 | `control.lua` dos cenários |
| `data/base/locale` | 50 | 4,2 | traduções `.cfg` |
| `data/base/menu-simulations` | 20 | 5,3 | simulações do menu principal (saves `.zip` + script) |
| `data/base/migrations` | 9 | 0,0 | migrações JSON/Lua entre versões (1.1.0 → 2.0.0) |
| `data/core/graphics` | 861 | 17,1 | GUI, cursores, **shaders** (`.psh/.vsh` HLSL, `.frag/.vert` GLSL, `.metal`, `.cso`) |
| `data/core/fonts` | 41 | 133,4 | fontes `.ttf` |
| `data/core/lualib` | 25 | 0,2 | biblioteca Lua partilhada (ver LUA_ANALYSIS.md) |
| `data/core/prototypes` | 15 | 0,5 | protótipos essenciais (estilos, utilidades, camadas de colisão, noise) |
| `data/core/locale` | 100 | 12,0 | traduções do core |
| `doc-html/*` | 3 384 | ~343 | documentação oficial da API (HTML + `runtime-api.json` + `prototype-api.json` + índice Pagefind) |
| raiz GOG | 11 | ~10 | instalador, ícones e metadados GOG |

## 2. Por extensão (instalação inteira)

| Ext | N | MB | | Ext | N | MB |
|---|---:|---:|---|---|---:|---:|
| .png | 4 879 | 1 129,8 | | .lua | 567 | 5,0 |
| .ogg | 1 943 | 151,6 | | .metal | 110 | 0,4 |
| .html | 1 617 | 269,3 | | .psh/.vsh | 72/38 | 0,4 |
| .pf_fragment | 1 617 | 2,4 | | .frag/.vert | 72/38 | 0,2 |
| .cfg | 1 182 | 18,5 | | .json | 79 | 3,9 |
| .zip | 42 | 57,1 | | .ttf | 35 | 127,2 |
| .cso | 3 | 0,1 | | .pdb | 1 | 440,1 |

[C] contagem direta.

## 3. Ficheiros "secundários" relevantes
- `data/changelog.txt` (1,8 MB): histórico de alterações; usado para cruzar nomes de funcionalidades [C].
- `data/licenses.txt`: licenças de terceiros; corrobora DEPENDENCIES.md [C].
- `data/*.example.json`: esquemas de `map-gen-settings`, `map-settings`, `server-settings` e `server-whitelist`. Documentam parâmetros do servidor dedicado [C].
- `data/base/require_test.lua`: teste do mecanismo `require` [C].
- `data/base/menu-simulations/*.zip`: saves completos usados no menu. São reproduzidos pelo motor real em runtime [I] (`MainMenuSimulationSelector` no PDB [C]).
- `.cso` (3): shaders HLSL já compilados (Direct3D) [C]. Os restantes shaders são compilados em runtime: o log [R/2.0.60] mostra `d3dcompiler_47.dll`.

## 4. Artefactos produzidos por esta investigação (`FACTORIO_REVERSE_ENGINEERING/`)

| Caminho | Conteúdo |
|---|---|
| `artifacts/pe_info.json` | cabeçalhos PE, secções, 20 DLLs importadas, 140 exports, diretório Debug, TLS |
| `artifacts/pdb/pdb_info.json` | cabeçalhos PDB/DBI, contagens, opções de compilação |
| `artifacts/pdb/pdb_modules.tsv` | 1 079 módulos (`.obj`/`.lib`) |
| `artifacts/pdb/pdb_source_files.txt` | 10 712 ficheiros-fonte referenciados |
| `artifacts/pdb/factorio_src_files.txt` / `factorio_src_dirs.tsv` | 7 128 ficheiros-fonte do próprio Factorio |
| `artifacts/pdb/pdb_procs.txt` | 249 292 nomes de procedimentos |
| `artifacts/pdb/pdb_publics.tsv` | 205 808 símbolos públicos (decorados) com secção:offset |
| `artifacts/pdb/pdb_gdata.txt` | 14 408 variáveis globais |
| `artifacts/pdb/pdb_udts.tsv` | 110 798 tipos (class/struct/union/enum) com tamanho |
| `artifacts/pdb/pdb_layouts.txt` | layouts de membros de 19 244 tipos não-std (offset, tipo, nome, bases, métodos, valores de enum) |
| `artifacts/pdb/pdb_udt_src.tsv` | 99 485 ligações tipo → ficheiro:linha |
| `artifacts/entity_hierarchy.txt` | árvore de herança de `Entity` (142 classes) |
| `artifacts/exe_strings.txt` | 180 207 strings ASCII com offset |
| `artifacts/data_lua_stats.json` | estatísticas dos ficheiros Lua de `data/` |
| `artifacts/pdb/pdb_proc_ranges.tsv` | nome, secção, offset e tamanho de cada função |
| `artifacts/calls_*.txt`, `artifacts/callers_tick.txt` | chamadas diretas em ordem e chamadores das funções-chave |
| `artifacts/function_inventory_by_dir.tsv` / `_by_class.tsv` | funções por diretório-fonte e por classe |
| `scripts/pdb_dump.py`, `scripts/pe_dump.py`, `scripts/calls.py`, `scripts/callers.py` | parsers/extratores só de leitura (stdlib), reprodutíveis |
| `ghidra/` | 1.º projeto Ghidra (com tipos PDB; análise encravada no alinhamento de estruturas) |
| `ghidra2/` | 2.º projeto Ghidra (só nomes de funções; análise completa em 2 127 s) |
| `artifacts/decomp/*.c` | 74 funções descompiladas (nome@endereço) |
| `artifacts/pdb/pdb_constants.tsv` | 29 751 constantes com nome e valor (`scripts/constants.py`) |
| `artifacts/render_layers.txt` | enum `RenderLayer` (97 camadas) |
| `artifacts/decomp/entity_save/*.c`, `artifacts/entity_save_formats.md` | `save()` das 139 classes de entidade e a tabela resumo (`scripts/entity_save_summary.py`) |
| `artifacts/rea_evidence_bundle.json` | 7 Evidence REA nativas (pseudocódigo Ghidra via REA) |
| `artifacts/lua_functions.tsv` | 528 funções do Lua por módulo (`scripts/lua_funcs.py`) |
| `scripts/ghidra/*.java`, `scripts/layout.py` | scripts Ghidra (aplicar nomes, descompilar lista) e leitor de layouts |
| `_previous_pass_2026-11-22/` | relatórios da passagem anterior, arquivados por conterem erros |
