# MODDING_API — API de modding e integração Lua

Fontes: `doc-html/runtime-api.json` e `doc-html/prototype-api.json` (documentação oficial máquina-legível incluída na instalação, `application_version 2.1.24`, `api_version 6`) [C], cruzadas com as classes C++ do PDB [C].

## 1. Superfície pública (oficial) [C]

| Estágio | Conteúdo |
|---|---|
| **Runtime** | 157 classes · 1 042 métodos · 2 774 atributos · **229 eventos** · 496 conceitos · 62 grupos de `defines` |
| Objetos globais | `game`, `script`, `prototypes`, `settings`, `remote`, `commands`, `rendering`, `rcon`, `helpers` |
| Funções globais | `log`, `localised_print`, `table_size` |
| **Prototype** | 279 tipos de protótipo · 706 tipos auxiliares |

## 2. Mapeamento API pública → motor [C]

| Lua | Classe C++ (PDB) | Notas |
|---|---|---|
| `game` | `LuaGameScript` (+ `LuaGameScriptData`) | um por estado Lua |
| `script` | `LuaBootstrap` (16 B: ponteiro para `LuaGameScript`) | `on_event`, `on_init`, `on_load`, `on_nth_tick`, `raise_event`, `register_on_object_destroyed`… |
| `prototypes` | `LuaPrototypes` | 68 funções ligadas; na 1.x era `game.*_prototypes` |
| `remote` | `LuaRemote` | interfaces entre mods (`remote.add_interface("freeplay", …)` no freeplay [C]) |
| `commands` | `LuaCommandProcessor` | comandos de consola personalizados |
| `rendering` | `LuaRendering` + `Script/ScriptRendering/` (26 ficheiros) + `LuaRenderObject` | desenho persistente gerido pelo motor |
| `settings` | `LuaSettings` + `Data/ModSettings*` | startup / runtime-global / runtime-per-user |
| `rcon` | `LuaRCON` | |
| `helpers` | `LuaHelpersObject` | utilitários (JSON, encode/decode…) |
| `LuaEntity` | `LuaEntity` → `Targeter<Entity>` | proxy fraco para `Entity` C++ |
| eventos `defines.events.*` | `enum LuaEventType` (`ON_TICK=0`, `ON_GUI_CLICK=1`, …, `ON_ENTITY_DIED=4`…) | **a numeração interna corresponde ao enum C++** [I]; ver lista completa em `artifacts/pdb/pdb_layouts.txt` (`== enum LuaEventType`) |
| filtros de eventos | 66 ficheiros `Script/LuaEventFiltering/` (`LuaEntityDiedEventFilters`…) | filtragem no lado C++ antes de entrar no Lua (desempenho) |
| `LuaCustomTable` | `Script/LuaCustomTable/` (33) | proxies de coleções do motor (`game.players`, `game.surfaces`…) com acesso preguiçoso |

## 3. Como os mods interagem com os sistemas internos [I, fundamentado]

```
 mod (Lua)                                   motor (C++)
 ──────────                                   ──────────
 data stage:  data:extend{...}  ──────────▶  PrototypeLoader → XPrototype (imutável no jogo)
 control:     script.on_event(id, fn) ───▶  LuaEventRegistrations (por mod)
                                             ▲  C++ gera evento (ex. Entity::die → ON_ENTITY_DIED)
              fn(event)  ◀── runEventHandler ┘  filtros C++ aplicados antes
              entity.active = false ──────▶  LuaEntity::luaWriteActive → Entity/UpdatableEntity
                                             (UpdatableEntity::DEACTIVATED_BY_SCRIPT [C])
              surface.create_entity{…} ───▶  Surface::addEntity → setup → listas ativas
              storage.x = …              ──▶  serializado em script.dat (determinístico)
```
- **Os mods não podem substituir lógica C++**: só configuram protótipos, reagem a eventos e chamam a API. Exemplo: um mod não pode mudar o algoritmo das belts, só os parâmetros (`speed`) do protótipo [I, ausência de qualquer ponto de extensão de algoritmo na API pública].
- **Determinismo** [I]: os handlers correm no tick de simulação de todos os peers (mesmo `TickClosure`). Por isso o `math.random` é determinístico e não há `io`/`os` (ver LUA_ANALYSIS §1). Um mod não-determinístico provoca desync, detetado por CRC (NETWORKING_ANALYSIS §5).
- `UpdatableEntity` tem flags explícitas para quem desativou a entidade (`DEACTIVATED_BY_SCRIPT`, `_BY_CIRCUIT_NETWORK`, `_BY_FREEZING`, `_BY_TO_DECONSTRUCT`, `_BY_RECIPE`) [C]: o estado "ativo" visto pelo Lua é uma das várias fontes combinadas.

## 4. Pontos de extensão na interface de mods
- **Protótipos**: 279 tipos (ver `prototype-api.json`); os `type` encontrados nos ficheiros de `data/` estão em `artifacts/data_lua_stats.json` [C].
- **Noise expressions** (geração de mapas): strings de expressão compiladas pelo motor (`Noise/NoiseExpressionParser`, `NoiseProgramBuilder`, `CompiledNoiseExpression`) [C]. Um mod descreve o terreno numa pequena linguagem de expressões; não executa Lua por tile.
- **Mod settings**: `Data/ModSettings*`, tipos `bool/int/double/string/color` [C].
- **Migrações**: `data/base/migrations/*.json|.lua` [C]. JSON para renomear protótipos, Lua para corrigir o estado.
- **Debug adapter**: `src/DebugAdapter/` (24 ficheiros) [C], suporte a depuração de mods via protocolo externo [I].
- **Instrumentação**: `LuaProfiler` [C].

## 5. Limitações
- A correspondência 1:1 entre os valores de `defines.events` em runtime e `enum LuaEventType` é [I]. Confirmar com `defines.events.on_tick == 0` num teste controlado (não executado).
- Restrições específicas por fase (o que é proibido em `on_load` além de `math.random`) não foram enumeradas [N].
