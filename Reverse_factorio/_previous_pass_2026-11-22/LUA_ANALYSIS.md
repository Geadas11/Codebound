# LUA ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Arquitetura**: Two-layer (C++ engine + Lua layer)

## Visão Geral da Arquitetura Lua

O Factorio utiliza uma arquitetura de "two-layer" onde:

1. **Motor Compilado (C++)**: `factorio.exe` - gerencia simulação, rendering, física, networking
2. **Camada Lua**: `data\base\script\` - lógica de jogo, API de modding, prototypes

### Comunicação Entre Camadas

A comunicação segue padrões definidos:

| Direção | Mechanim | Exemplos |
|---------|----------|----------|
| C++ → Lua | `prototypes` | Entities, items, recipes, technologies |
| Lua → C++ | Eventos/Calls | `defines.events.on_player_created`, `remote.call()` |

### Protótipos Registrados no Motor (accessíveis via `prototypes`)

| Categoria | Tipo | Arquivo de Definição |
|-----------|------|---------------------|
| Entities | `LuaEntityPrototype` | `prototypes/entity/*.lua` |
| Items | `LuaItemPrototype` | `prototypes/item/*.lua` |
| Recipes | `LuaRecipePrototype` | `prototypes/recipe/*.lua` |
| Technologies | `LuaTechnologyPrototype` | `prototypes/technology/*.lua` |
| Fluids | `LuaFluidPrototype` | `prototypes/fluid/*.lua` |
|Tiles | `LuaTilePrototype` | `prototypes/tile/*.lua` |

## Análise de Scripts Principais

### `data\base\script\freeplay\control.lua`

```lua
local handler = require("event_handler")
handler.add_lib(require("freeplay"))

if script.active_mods["space-age"] then
  handler.add_lib(require("space-finish-script"))
else
  handler.add_lib(require("silo-script"))
end
```

**Função**: Ponto de entrada do modo freeplay. Carrega o handler de eventos e o lib freeplay ou silo-script dependendo se o mod "space-age" está ativo.

**Fluxo**:
1. Require `event_handler` - módulo que centraliza event registrations
2. Require `freeplay` - lógica específica do modo freeplay
3. Verifica se `space-age` mod está ativo
4. Carrega script de fim de jogo apropriado

### `data\base\script\freeplay\freeplay.lua`

**Função**: Lógica completa do modo freeplay - análise detalhada:

#### Funções Principais

1. **`created_items()`** - Itens iniciais do jogador
   ```lua
   {
     ["iron-plate"] = 8,
     ["wood"] = 1,
     ["pistol"] = 1,
     ["firearm-magazine"] = 10,
     ["burner-mining-drill"] = 1,
     ["stone-furnace"] = 1
   }
   ```
   **Análise**: Define o inventário inicial do jogador no modo freeplay. 6 itens tipos diferentes, total de 21 itens.

2. **`respawn_items()`** - Itens ao resspawnar
   ```lua
   {
     ["pistol"] = 1,
     ["firearm-magazine"] = 10
   }
   ```

3. **`ship_items()`** - Itens para o ship/rocket
   ```lua
   {
     ["firearm-magazine"] = 8
   }
   ```

4. **`debris_items()`** - Itens de debris
   ```lua
   {
     ["iron-plate"] = 8
   }
   ```

5. **`ship_parts()`** - Partes do ship (chama crash-site)
   ```lua
   return crash_site.default_ship_parts()
   ```

6. **`chart_starting_area()`** - Charting da área inicial
   ```lua
   local r = storage.chart_distance or 200
   -- chart a 400x400 area around player spawn
   ```

7. **`get_starting_message()`** - Mensagem de início
   - Verifica `storage.custom_intro_message`
   - Se `space-age` mod ativo → `{"msg-intro-space-age"}`
   - Senão → `{"msg-intro"}`

8. **`show_intro_message()`** - Mostra mensagem deIntro
   - Em multiplayer: `player.print()`
   - Em single: `game.show_message_dialog()`

9. **`on_player_created()`** - Event handler para player criado
   - Chama `util.insert_safe(player, storage.created_items)` - injeta itens no inventário
   - `storage.init_ran` - flag para garantir init happens once
   - `chart_starting_area()` - chart a área ao redor do spawn
   - `crash_site.create_crash_site()` - cria site de acidência
   - `show_intro_message(player)` - mostra mensagem

10. **`on_player_respawned()`** - Event handler para player respawned
    - `util.insert_safe(player, storage.respawn_items)`

11. **`on_cutscene_waypoint_reached()`** - Waypoint da cutscene
    - Verifica `storage.crash_site_cutscene_active`
    - Verifica `crash_site.is_crash_site_cutscene()`
    - `player.exit_cutscene()`
    - `show_intro_message(player)`

12. **`skip_crash_site_cutscene()`** - Pular cutscene
    - Apenas para player_index 1
    - Verifica `player.controller_type == defines.controllers.cutscene`

13. **`on_cutscene_cancelled()`** - Cutscene cancelada
    - Restaura `destructible` do character
    - Remove label da GUI
    - Reset zoom

14. **`on_player_display_refresh()`** - Refresh da display
    - Chama `crash_site.on_player_display_refresh(event)`

#### Interface Remota (`freeplay_interface`)

```lua
freeplay_interface =
{
  get_created_items = function() ... end,
  set_created_items = function(map) ... end,
  -- ... mais 13 getters/setters
  get_ship_items = function() ... end,
  set_ship_items = function(map) ... end,
  -- etc.
}
```

**Propósito**: Permite acesso remoto a variáveis de storage via interface `freeplay`. Outros mods ou scripts podem chamar `remote.call("freeplay", "get_created_items")`.

#### Eventos Regulados

```lua
freeplay.events =
{
  [defines.events.on_player_created] = on_player_created,
  [defines.events.on_player_respawned] = on_player_respawned,
  [defines.events.on_cutscene_waypoint_reached] = on_cutscene_waypoint_reached,
  ["crash-site-skip-cutscene"] = skip_crash_site_cutscene,
  [defines.events.on_player_display_resolution_changed] = on_player_display_refresh,
  [defines.events.on_player_display_scale_changed] = on_player_display_refresh,
  [defines.events.on_cutscene_cancelled] = on_cutscene_cancelled
}
```

**Eventos Mapeados**:
- `on_player_created` - Quando um novo jogador entra/conecta
- `on_player_respawned` - Quando player ressurgir após death
- `on_cutscene_waypoint_reached` - Quando a cutscene do crash site chega a um waypoint
- `crash-site-skip-cutscene` - Evento personalizado para pular cutscene
- `on_player_display_resolution_changed` - Quando resolução da display muda
- `on_player_display_scale_changed` - Quando scale da display muda
- `on_cutscene_cancelled` - Quando cutscene é cancelada pelo jogador

#### `on_configuration_changed()`

```lua
freeplay.on_configuration_changed = function()
  -- Initialize storage tables if not exist
  storage.created_items = storage.created_items or created_items()
  storage.respawn_items = storage.respawn_items or respawn_items()
  storage.crashed_ship_items = storage.crashed_ship_items or ship_items()
  storage.crashed_debris_items = storage.crashed_debris_items or debris_items()
  storage.crashed_ship_parts = storage.crashed_ship_parts or ship_parts()
  
  -- Check if init ran (for old saves)
  storage.init_ran = storage.init_ran or #game.players > 0
  
  -- Re-initialize ending info
  init_ending_info()
```
**Função**: Migration de saves antigas e inicialização de storage tables.

#### `on_init()`

```lua
freeplay.on_init = function()
  game.allow_tip_activation = true
  storage.created_items = created_items()
  storage.respawn_items = respawn_items()
  storage.crashed_ship_items = ship_items()
  storage.crashed_debris_items = debris_items()
  storage.crashed_ship_parts = ship_parts()
  
  -- Debug mode check
  function is_debug()
    local surface = game.surfaces.nauvis
    local map_gen_settings = surface.map_gen_settings
    return map_gen_settings.width == 50 and map_gen_settings.height == 50
  end
  
  if is_debug() then
    storage.skip_intro = true
    storage.disable_crashsite = true
  end
  
  init_ending_info()
```
**Função**: Inicialização no primeiro carregamento do jogo. Verifica modo debug (mapa 50x50) e configura skip intro accordingly.

#### `init_ending_info()`

```lua
local function init_ending_info()
  local is_space_age = script.active_mods["space-age"]
  local info =
  {
    image_path = is_space_age and "__base__/script/freeplay/victory-space-age.png" or "__base__/script/freeplay/victory.png",
    title = {"gui-game-finished.victory"},
    message = is_space_age and {"victory-message-space-age"} or {"victory-message"},
    bullet_points =
    {
      {"victory-bullet-point-1"},
      {"victory-bullet-point-2"},
      {"victory-bullet-point-3"},
      {"victory-bullet-point-4"}
    },
    final_message = {"victory-final-message"},
  }
  game.set_win_ending_info(info)
end
```

**Função**: Configura a info de vitória/fim de jogo dependendo se o mod space-age está ativo.

### Utilitários (`util` module)

O script usa `require("util")` - módulo utilitário padrão do Factorio com funções como:
- `util.insert_safe(player, table)` - Insere itens no inventário com segurança (não sobrescreve itens existentes)
- `util.copy(table)` - Cópia profunda de tabela

## Análise de Dependências Lua

### Módulos Externos Require

| Módulo | Caminho | Propósito |
|--------|---------|-----------|
| `event_handler` | `data\base\script\event_handler.lua` | Centraliza registrations de eventos |
| `freeplay` | `data\base\script\freeplay\` | Lógica freeplay (próprio módulo) |
| `space-finish-script` | Apenas se space-age ativo | Script de fim space-age |
| `silo-script` | Padrão se sem space-age | Script de fim padrão |
| `util` | Core Factorio module | Utilitários diversos |

### Padrões de Require Observados

1. **Core modules** (fornecidos pelo Factorio engine):
   - `defines` - Constants e defines
   - `script` - API do script environment
   - `util` - Utilitários
   - `event_handler` - Padrão do base game

2. **Proprios módulos do base game**:
   - `freeplay` - Modo freeplay
   - `crash-site` - Lógica do site de acidência

3. **Mods externos** (condicionais):
   - `space-age` - Se mod space-age estiver ativo

## Estrutura de Dados de Storage

Variáveis de `storage` usadas neste módulo:

| Variável | Tipo | Descrição |
|----------|------|-----------|
| `storage.created_items` | table | Itens criados no início |
| `storage.respawn_items` | table | Itens ao resspawnar |
| `storage.crashed_ship_items` | table | Itens do ship acidentado |
| `storage.crashed_debris_items` | table | Debris do ship acidentado |
| `storage.crashed_ship_parts` | table | Partes do ship acidentado |
| `storage.init_ran` | boolean | Flag: init já aconteceu? |
| `storage.skip_intro` | boolean | Pular intro? |
| `storage.disable_crashsite` | boolean | Desativar crash site? |
| `storage.custom_intro_message` | string | Mensagem customizada |
| `storage.chart_distance` | number | Distância para charting inicial |

## Padrões e Observações

1. **Initialization Pattern**: O módulo usa `storage.init_ran` flag para garantir que init acontece apenas uma vez, importante para compatibilidade com saves antigos.

2. **Conditional Logic**: Múltiplos `if script.active_mods["space-age"]` checks - o módulo se adapta baseado em mods instalados.

3. **Storage Initialization Pattern**: Todas as tabelas de storage são inicializadas com valores padrões se não existirem (padrão de migration de saves).

4. **Event Handling**: Todos os eventos críticos são registrados na tabela `freeplay.events` e manipulados pelas funções correspondentes.

5. **Remote Interface**: A interface `freeplay` expõe 13 funções getter/setter para acesso remoto via `remote.call()`.

6. **Crash Site Integration**: O módulo integra profundamente com o sistema `crash-site`, criando sites, cutscenes e gerenciando state visual.

7. **Debug Mode**: Detecta mapa 50x50 como modo debug e auto-configura skip intro + disable crashsite.

## Próximos Passos na Análise Lua

1. Analisar `event_handler.lua` - como eventos são registrados centralmente
2. Investigar `crash-site.lua` - lógica do site de acidência
3. Comparar com versões anteriores do Factorio (0.15, 0.17, 1.1)
4. Documentar API completa exposta para modders
5. Identificar pontos de extensão para mods

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- [CONFIRMADO] `_VERSION` = `Lua 5.2` @0x23c2b90.
- [CONFIRMADO] 140 exports, dos quais 138 são funções da API C do Lua (`lua_*`/`luaL_*`); os outros 2 são `NvOptimusEnablement` e `AmdPowerXpressRequestHighPerformance`.
