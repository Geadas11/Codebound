# LUA_ANALYSIS — Implementação Lua

## 1. O interpretador embutido

| Facto | Evidência |
|---|---|
| **Lua 5.2**, compilado a partir das fontes e ligado estaticamente (`lua.lib`, 28 módulos) | [C] PDB: `libraries/Lua/lapi.c … lvm.c` (55 ficheiros); string `Lua 5.2`; API `lua_callk`/`lua_pcallk`/`lua_getctx` (só existem na 5.2) |
| **Variante modificada** | [C] exports não-standard `lua_isnumberorstringconvertabletonumber`, `lua_isstringornumberconvertabletostring`; ficheiros extra `LuaCPPUtilities.hpp`, `override_printf.h` |
| **Bibliotecas padrão compiladas**: base, string, table, math, bit32, debug | [C] presentes `lbaselib.c lstrlib.c ltablib.c lmathlib.c lbitlib.c ldblib.c` |
| **Bibliotecas padrão ausentes**: `io`, `os`, `package` (loadlib), `coroutine` | [C] `liolib.c`, `loslib.c`, `loadlib.c`, `lcorolib.c` **não** aparecem entre os 10 712 ficheiros-fonte do PDB. [I] Sem acesso a ficheiros/processos, carregamento de DLLs ou corrotinas: sandbox por omissão de código e não por restrições em runtime |
| `require` próprio | [C] `LuaGameScript::allowRequire`, `LuaState::currentPackage` + `currentPathInPackage` → [I] `require` resolve módulos dentro do *Package* (zip/diretório) do mod, e não no sistema de ficheiros |
| `math.random` controlado | [C] `LuaGameScript::allowMathRandom`; classe `LuaRandomGenerator` (`reSeed`, `save`) → [I] o `math.random` é substituído por um gerador determinístico guardado no save; pode ser proibido em certas fases (ex.: `on_load`) |
| `serpent` disponível como global | [C] `core/lualib/dataloader.lua` usa `serpent.block(...)` sem `require`; `Util/LuaLibSerpent` no PDB → injetado pelo C++ |
| `log`, `localised_print`, `table_size` | [C] `runtime-api.json` → `global_functions` |

## 2. As duas fases Lua

### 2.1 Data stage (carregamento de protótipos)
- **Bootstrap Lua**: `core/lualib/dataloader.lua` cria `data = {raw = {}}` e `data:extend(t)`, que verifica `type`/`name`, duplicados (`data-duplicate-checker.lua`) e insere em `data.raw[type][name]` [C].
- **Ordem por mod** [R/2.0.60, log]: todos os `data.lua` (core → base → …), depois todos os `data-updates.lua`, depois `data-final-fixes.lua` (por inferência do padrão) [I].
- **Conversão para C++**: `LuaDataLoader::loadData/exportData/exportCompactData` → `PrototypeLoader::loadPrototypes` → `loadPrototypeForType` (com `CrashHook` que reporta o protótipo em falha) [C]. Cada `type` Lua corresponde a uma classe `XPrototype` C++.
- **Checksums** [R/2.0.60]: `Checksum for core: 4171658704`, `Checksum of base: 1534080171`, `Prototype list checksum: 790204923`. O mesmo CRC do mod aparece no cabeçalho do save (ver SAVE_SYSTEM §3) e serve para validar a compatibilidade em multiplayer [C, cruzado].
- `data.is_demo` é reescrito pelo C++ [C, comentário em `dataloader.lua`].

### 2.2 Control stage (runtime)
- **Um estado Lua por mod/cenário** [C]: `LuaContext::createScript`, `LuaGameScript { const Mod* mod; Scenario* scenario; Map& map; LuaState luaState; … }` (640 B).
- `LuaGameScript` contém (offsets do PDB) [C]: `isLoaded`, `hasControlLua`, `setupFinished`, `runningOnLoad`, `allowMathRandom`, `allowRequire`, `objectsData`, `luaState`, `commands` (LuaCommandProcessor), `remote` (LuaRemote), `settings`, `RCON`, `rendering`, `prototypes`, `helpers`, `eventRegistrations`. Correspondem exatamente aos objetos globais documentados: `commands game helpers prototypes rcon remote rendering script settings` [C, `runtime-api.json`].
- **Persistência**: tabela `storage` (`LuaGameScript::STORAGE_TABLE` [C]) serializada para `script.dat` no save (`LuaGameScript::saveScriptState`, `LuaContext::save`) [C]. A tabela `global` da série 1.x foi renomeada `storage` na 2.x [C, `control.lua` do freeplay].
- **Despacho de eventos**: `LuaGameScript::runEventHandler`, `runNthTickHandler`; `LuaBootstrap::luaOnEvent/luaOnInit/luaOnLoad/luaOnNthTick/luaOnConfigurationChanged/luaRaiseEvent` [C].
- **Checksums de script**: `LuaContext::scriptChecksums` [C] → [I] entra na verificação de consistência multiplayer.

## 3. Mecanismo de binding C++ ↔ Lua [C, PDB]

```
LuaBindableObject                          (base de todos os objetos expostos)
  └─ LuaObjectTemplate<T, N>               (nome do tipo, tipo para gravação, initLuaState)
        └─ T = LuaEntity, LuaPlayer, LuaSurface, LuaRandomGenerator, … (251 classes)
LuaBinder<T>
   getFunctionMap()   : FlatMap<string, FuncWrapper>   → métodos   (luaXxx)
   getReadIndexMap()  : FlatMap<string, FuncWrapper>   → leitura de atributos (luaRead…)
   getWriteIndexMap() : FlatMap<string, FuncWrapper>   → escrita de atributos (luaWrite…)
   getMetaFunctionMap(), registerCommonMetaMethods(), keyNotFound(), writableKeyNotFound()
LuaBindableObject::luaIndex<LuaBinder<T>> / luaNewIndex<…>  → metamétodos __index/__newindex
```
- [I] Cada acesso `entity.foo` faz `__index` → procura binária na `FlatMap` de leitura → chama `LuaEntity::luaReadFoo`, que valida o `Targeter<Entity>` e lê o objeto C++. Não há tabelas Lua com cópias de estado: **os objetos Lua são proxies vivos**.
- **Referências fracas**: `LuaEntity { LuaObjectTemplate; LuaControl; Targeter<Entity,28,129> entityTarget; }` (72 B) [C]. Quando a entidade C++ é destruída, o `Targeter` é anulado e o proxy fica inválido (`valid == false`) [I, coerente com a API pública].
- **Dimensão**: 4 732 funções `Lua*::luaXxx` no PDB [C]. A API pública declara 157 classes, 1 042 métodos e 2 774 atributos [C, `doc-html/runtime-api.json`]. As funções C++ excedem a soma porque cada atributo pode ter funções de leitura e de escrita.
- Classes com mais funções ligadas [C]: `LuaEntityPrototype` 628, `LuaEntity` 575, `LuaBindableObject` 417, `LuaForce` 200, `LuaGuiElement` 190, `LuaSurface` 159, `LuaPlayer` 159, `LuaStyle` 127, `LuaControl` 114, `LuaRenderObject` 104.

## 4. Lua em `data/` (o que *está* em Lua) [C]

| Área | Ficheiros | Linhas | Papel |
|---|---:|---:|---|
| `base/graphics` | 362 | — | definições de sprites/animações (dados) |
| `base/prototypes` | 105 | — | protótipos do mod base |
| `core/lualib` | 25 | — | `util`, `dataloader`, `event_handler`, `mod-gui`, `math2d/3d`, `meld`, `collision-mask-util`, `resource-autoplace`, `production-score`, `silo-script`, `space-finish-script`, `story`, `crash-site`… |
| `core/prototypes` | 15 | — | estilos de GUI, constantes, noise functions/programs, controladores |
| `base/script` + `scenarios` + `campaigns` + `tutorials` | 54 | — | lógica de cenário (control stage) |
| **Total** | **567** | **171 977** | [C] `artifacts/data_lua_stats.json` |

`event_handler.lua` [C] é uma biblioteca de multiplexagem: junta vários "libs" (cada um com `events`, `on_nth_tick`, `on_init`, `add_remote_interface`, `add_commands`) num único registo `script.on_event`, porque cada evento só aceita um handler por mod (`check_handler` avisa em caso de duplicação).

O freeplay (`base/script/freeplay/control.lua`) [C]:
```lua
local handler = require("event_handler")
handler.add_lib(require("freeplay"))
if script.active_mods["space-age"] then handler.add_lib(require("space-finish-script"))
else handler.add_lib(require("silo-script")) end
```

## 5. O que NÃO está em Lua (responsabilidade do motor C++) [C]
Belts, inserters, crafting, fluidos, eletricidade, calor, circuitos, robôs, comboios, pathfinding, IA de inimigos, geração de mapa (o Lua só declara *noise expressions*, compiladas pelo `Noise/NoiseExpressionParser` + `NoiseProgramBuilder` C++), poluição, colisões, render, rede e saves.
Nenhum destes sistemas tem implementação em `data/`. Os ficheiros-fonte correspondentes existem em `src/` (ver SUBSYSTEMS.md).

## 6. Limitações
- **Modificações à VM** [C/G]: tabelas com ordem de inserção (`pairs` determinístico), **matemática própria `custom_*`** (sin/cos/exp/log/pow determinísticos), sem corrotinas/io/os/package (nem compilados), GC com lista dupla e `gcblocked`, verificador de bytecode e erros por exceções C++. Ver GHIDRA_FINDINGS §15 e §18.
- Bibliotecas e funções exatas [C/G]: `base` sem `dofile`/`loadfile`/`require`; `debug` parcial = `getinfo` + `traceback`; extras `table.pairs_concat` e `string.pack/packsize/unpack`. Ver GHIDRA_FINDINGS §9.
