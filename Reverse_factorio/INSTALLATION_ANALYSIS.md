# INSTALLATION_ANALYSIS — Instalação e versão

> Investigação só de leitura. Legenda de evidência usada em todos os documentos:
> **[C]** confirmado por evidência estática direta (ficheiro, PE, PDB, string) · **[R]** observado em runtime · **[I]** inferência técnica fundamentada · **[H]** hipótese não confirmada · **[N]** não investigado / não investigável.

## 1. Identificação

| Item | Valor | Evidência |
|---|---|---|
| Produto | Factorio (Wube Software) | [C] `VersionInfo` do exe: `CompanyName=Wube Software`, `ProductName=Factorio` |
| Versão | **2.1.24** (FileVersion 2.1.24.87808) | [C] VersionInfo; [C] `data/base/info.json` → `"version": "2.1.24"` |
| Build | 87808 | [C] FileVersion (o campo raw `22272` = 87808 & 0xFFFF) |
| Distribuição | GOG (gameId 1238653230, buildId 60166908948733930) | [C] `goggame-1238653230.info` |
| Executável | `bin/x64/factorio.exe`, 49 423 312 bytes, SHA-256 `8cfdb7111ebb31d68441113ebdb0165c90cb3723f6cd6902d71339fe2275611f` | [C] REA `inspect_pe_resources` Evidence `ev_a9903e7c…569db6`; [C] `artifacts/pe_info.json` |
| Símbolos | `bin/x64/factorio.pdb`, 461 451 264 bytes, MSF 7.00, GUID `178e8df5e1bca04f9d3eb184b8bfabae`, age 1 | [C] `artifacts/pdb/pdb_info.json`; GUID igual ao registo CodeView RSDS do exe → **PDB corresponde exatamente ao exe** |
| Caminho de build | `C:\Users\build\AppData\Local\Temp\factorio-build-JmH45D\bin\VanillaFinalReleasex64vsbt\factorio.pdb` | [C] diretório Debug do PE |
| Configuração de build | `VanillaFinalRelease` x64 com toolchain `vsbt` (VS Build Tools) | [C] caminho acima; [I] "Vanilla" = sem DLC/modo especial |
| Compilador | MSVC 14.44 (VS 2022 BuildTools 14.44.35207), linker 14.44 | [C] cabeçalho DBI (toolchain 14.44), opt header linker 14.44, caminhos de include |
| Assinatura | Authenticode **válida**, `CN=Wube Software Ltd, L=London, C=GB`, TSA GlobalSign R45 (2025-10) | [C] `Get-AuthenticodeSignature` |
| Plataforma | PE32+ x86-64, subsystem 2 (Windows GUI) | [C] `pe_info.json` |

## 2. Layout da instalação

```
C:\GOG Games\Factorio
├── bin\x64\            factorio.exe, factorio.pdb, ChromaAppInfo.xml (Razer Chroma)
├── data\               core\ (motor base), base\ (mod "base"), changelog, cacert.pem, exemplos JSON
├── doc-html\           documentação oficial offline da API Lua (runtime-api.json, prototype-api.json)
├── config-path.cfg     use-system-read-write-data-directories=true
├── goggame-*.*/gog.ico/unins000.*   ficheiros do instalador GOG (fora do âmbito do motor)
└── webcache.zip        cache do launcher GOG (fora do âmbito)
```

12 450 ficheiros, 2 385 755 417 bytes. [C] `artifacts/file_inventory.csv`, `artifacts/dir_summary.tsv`.

### Pontos relevantes
- **Só `core` e `base` em `data\`** [C]. As pastas `space-age`, `quality` e `elevated-rails` não existem nesta instalação. O código do DLC Space Age está **compilado no exe** [C] (diretório de fontes `src/Space/` no PDB, 131 ficheiros; `DoSpaceAgeFixUp`, `SpacePlatform`) e o `control.lua` do freeplay consulta `script.active_mods["space-age"]` [C]. O DLC é, portanto, conteúdo de dados ativado por mod sobre um motor único [I].
- `config-path.cfg` → `config-path=__PATH__system-write-data__/config` e `use-system-read-write-data-directories=true` [C]: os dados graváveis vão para `%APPDATA%\Factorio`.
- `doc-html\runtime-api.json` (2,1 MB) e `prototype-api.json` (1,9 MB) [C]: descrição máquina-legível de toda a API de modding desta versão. É a fonte oficial da interface pública (ver MODDING_API.md).
- `data\cacert.pem` [C]: bundle de CAs usado pelo libcurl embutido (HTTPS para auth/mods/updater) [I].

## 3. Diretório de escrita do utilizador (`%APPDATA%\Factorio`) — dependência externa

Existe e foi apenas **listado e lido** (sem alterações) [C]:
`config\`, `mods\` (só `mod-list.json`), `saves\` (4 saves), `temp\`, `crop-cache.dat`, `factorio-current.log`, `factorio-previous.log`, `player-data.json`.

> ⚠️ **Estes dados NÃO pertencem a esta instalação.** O log diz `Factorio 2.0.60 (build 83512, win64, steam, space-age)`, com o binário em `C:\Users\carlos\Downloads\Factorio (2020)\Factorio\bin\x64\factorio.exe` [C].
> É uma instalação Steam anterior, com o DLC, que partilha o mesmo diretório de escrita. Foi usada **apenas** como evidência secundária de comportamento em runtime da mesma família de motor, e está sempre identificada como **[R/2.0.60]**.

## 4. Mods instalados
- Nesta instalação: nenhum mod de terceiros. Só os mods internos `core` e `base` [C].
- `%APPDATA%\Factorio\mods\mod-list.json` pertence à instalação externa [C].

## 5. Limitações
- O jogo não foi executado nesta instalação, para a preservar e não alterar `%APPDATA%`, que é partilhado [N]. Todo o comportamento em runtime vem do log e dos saves existentes da instalação 2.0.60.
- O conteúdo do `goggame-*.hashdb` (integridade GOG) não foi interpretado [N]; está fora do âmbito do motor.
