# SAVE_SYSTEM — Estrutura dos saves e persistência

Evidência: estrutura de um save **real**, analisada sobre uma **cópia** (`_autosave3.zip` da instalação externa 2.0.60, copiada para o scratchpad; SHA-256 do original e da cópia idênticos `5CDA4C53…13DA18`, o original não foi aberto para escrita) [R/2.0.60], cruzada com tipos e strings do binário 2.1.24 [C].

## 1. Contentor [R/2.0.60]

Um save é um **ZIP** (`Package/ZipPackage*`, minizip [C]) com uma pasta de topo igual ao nome do save:

| Entrada | Método ZIP | Conteúdo |
|---|---|---|
| `level.dat0 … level.datN` | **0 (stored)** | estado do mapa, em **fatias de exatamente 1 048 576 bytes descomprimidos**, cada uma um *stream zlib* independente (cabeçalho `78 01`) |
| `level.datmetadata` | deflate | 8 bytes: `u64` = tamanho total descomprimido (4 810 465 = soma das 5 fatias ✔) |
| `level-init.dat` | deflate | estado **inicial** do mapa (mesmo cabeçalho que `level.dat`) → base dos replays [I] (strings `replay.dat`, `ReplaySaver`, `ReplayInputSource` [C]) |
| `script.dat` | deflate | estado Lua (`storage`) de cada mod/cenário (`Loading script.dat: %llu bytes.` [C]) |
| `control.lua` | deflate | `require('__base__/script/freeplay/control.lua')`: o cenário embutido |
| `info.json`, `description.json` | deflate | metadados |
| `preview.jpg` | deflate | miniatura (GDI+ [I]) |
| `locale/<lang>/*.cfg` | deflate | traduções do cenário |

**Porquê fatias de 1 MiB com zlib manual?** [I] Classes `ThreadedChunkedZipPackageWriteStream`, `ChunkedZipPackageReadStream`, `ThreadedZipPackageWriteStream` [C]. A compressão é feita **em paralelo por fatia**, em threads de trabalho, e o resultado é escrito como entrada *stored*. O minizip só empacota; o zlib comprime cada fatia numa thread.

## 2. Pipeline de gravação [C nomes, I ordem]

```
AppManager::autoSaveGameInSinglePlayer / saveGameOnServer
  └─ Scenario::saveAs → Scenario::saveMap
       └─ ParallelScenarioSaver::save → doSave (thread)            [C]
            ├─ MapSerialiser (herda Serialiser, stream de escrita)
            │    Map::save → Surface → Chunk::save (MAGIC_CHUNK_BEGIN/
            │    MAGIC_TILES_END/MAGIC_ENTITIES_END [C, constantes em Chunk])
            │    → Entity::save (virtual, por classe)
            │    sub-serialisers: TransportLine, ElectricNetwork, HeatBuffer,
            │    WireConnector, CollisionMask, FluidBoxConnection, ThreadedChunkTileSaver
            ├─ LuaGameScript::saveScriptState → script.dat
            └─ ThreadedChunkedZipPackageWriteStream → level.datN (zlib/1 MiB)
```
- `Serialiser { forHuman, completeMode, savingAsScenario, savingStatistics, position, … }` [C]. `forHuman` → existe um modo de dump legível [I]; `savingStatistics` → contabilidade de bytes por "endereço" (`std::map<string,u64> written`, `logSavingStatistics`) [C].
- **Referências entre objetos**: `TargetSerialiser`/`TargetDeserialiser` [C] → ponteiros (`Targeter<…>`) gravados como identificadores e religados no load [I].
- **Carregamento em várias passagens**: `MapDeserialiser` tem `loadHelpers`, `preFinalLoadHelpers`, `finalLoadHelpers` [C] → objetos são criados e religados em fases após a leitura.
- **Migração**: `MapDeserialiser` tem `LegacyCollisionMaskDeserialiser`, `LegacyFluidSystemDeserialiser`, `migratedTiles` [C]; `Map::clearMigrationData`; JSON de migração em `data/base/migrations` [C].

## 3. Cabeçalho de `level.dat` / `level-init.dat` (decodificado) [R/2.0.60 + C]

```
02 00 00 00 3C 00 00 00 | 00            MapVersion: u16 major=2, u16 minor=0, u16 patch=60, u16 dev=0 | branch=0
08 "freeplay"                           nome do cenário (u8 len + bytes)
04 "base"                               mod que contém o cenário
01 | 00×6 01 | 02 00 3C | 38 46 01 00 | 01 | 00 00 A0 00   dificuldade | flags do cenário | versão 2.0.60 | build 83512 [I] | allowedCommands=True | largeBlueprintSize=10 MiB  (GHIDRA_FINDINGS §16)
04                                       nº de mods = 4
  04 "base"            02 00 3C  AB 34 70 5B     nome, versão (u8 maj, u8 min, u8 patch → 2.0.60), CRC
  0E "elevated-rails"  02 00 3C  8C D4 69 C9
  07 "quality"         02 00 3C  27 DE 42 D6
  09 "space-age"       02 00 3C  D5 75 AA 46
…  (dados do cenário: mensagens de vitória, imagens __base__/…)
```
- `MapVersion { uint64 version; BranchVersion branchVersion; }` (16 B) [C]: os 8 bytes iniciais são os 4 × u16 empacotados em `version`.
- **Validação cruzada**: o CRC de `base` no save, `0x5B7034AB = 1534080171`, é igual ao `Checksum of base: 1534080171` do log [R/2.0.60]; `elevated-rails` `0xC969D48C = 3379156108` ✔. O save regista o checksum de cada mod usado, e o motor recusa/avisa no load se divergirem [I].
- Inteiros de comprimento usam 1 byte quando cabem. O PDB tem `SizeOptimizedUInt` e `MapPosition::saveCompact` [C] → [I] codificação "space optimized": `u8`, com `0xFF` a escapar para `u32`.

## 4. Persistência fora dos saves
- `%APPDATA%/Factorio/config/config.ini` (`PropertyTreeIni` [C]), `player-data.json` (local + cloud, `PlayerData.cpp` [R/2.0.60]), `mods/mod-list.json`, `mod-settings.dat`, `crop-cache.dat` (`Graphics/CropCache` [C]: cache de recortes de sprites para acelerar o arranque [I]), `achievements.dat` [H].
- Blueprints: `blueprint-storage.dat` [H]; `BlueprintImportExportEngine` [C] faz o formato de string `0` + base64(zlib(JSON)) [conhecimento público, não verificado aqui].

## 5. Questões em aberto
- Uso do **zstd** [C/G]: só em blueprints (`Blueprint::serialize`, `BlueprintShelf`), UDP de scripts e comandos remotos. O save do mapa é zlib. Ordem de `Map::save` e campos de `Entity::save`: GHIDRA_FINDINGS §11.
- Cabeçalho completo e `save()` das 139 classes de entidade: GHIDRA_FINDINGS §16–17 e `artifacts/entity_save_formats.md` [C/G].
- O save observado é **2.0.60**. O formato da 2.1.24 pode diferir (`MapVersion::atLeast` faz branching por versão [C]).
