# DATA STRUCTURES ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Foco**: Estruturas de dados internas do engine

## Visão Geral

Este relatório documenta as estruturas de dados internas utilizadas pelo engine do Factorio 2.1.24. Estas estruturas são a base para todo o estado do jogo e são manipuladas tanto pelo código C++ compilado quanto pelos scripts Lua.

## 1. Estruturas de Entidade (Entity States)

### 1.1 Entity Base Structure

Cada entity no jogo mantém um estado structurado da seguinte forma:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `name` | string | Nome do protótipo (ex: "iron-ore", "assembly-robot") |
| `position` | {x, y} | Posição no mapa (tiles coordinates) |
| `direction` | defines.direction | Direção (0-8, cardinal/intercardinal) |
| `speed` | float | Velocidade atual |
| `status` | string | Status (idle, working, running, etc.) |
| `energy` | {current, max} | Energy state |
| `health` | {current, max} | Health points |
| `inventory` | table<slot_id, stack> | Inventários da entity |
| `target` | entity* | Target entity (para ataques) |
| `last_user` | player* | Último jogador que interagiu |

#### Exemplos de Entity Types

| Type | Campos Adicionais |
|------|------------------|
| `mining-drill` | `energy_consumption`, `mining_speed`, `resource` |
| `assembling-machine` | `input_count`, `output_count`, `module_inventory_size` |
| `transport-belt` | `speed`, `direction`, `collision_box` |
| `inserter` | `speed`, `stack_size`, `insert_vector` |
| `player` | `character`, `requests`, `quick_bar`, `grid` |

### 1.2 Inventário Systems

#### Inventory Structure

```lua
{
    [1] = {name = "iron-plate", count = 50},  -- main inventory
    [2] = {name = "copper-cable", count = 10},  -- belt inventory
    [3] = nil,  -- empty slot
    … 
}
```

#### Inventory Slots Principais

| Inventário | Slots | Propósito |
|------------|-------|-----------|
| `main` | 45 | Inventário principal do player |
| `main_quickbar` | 16 | Quickbar do player |
| `off_hand` | 1 | Off-hand equipment |
| `grid` | 0 | Equipment grid (armor, weapons) |
| `logistic_requests` | 8 | Request slots (logistic network) |
| `logistic_trash` | 8 | Trash slots (logistic network) |
| `fuel` | 1 | Fuel slot (vehicle/assembler) |
| `character_quickbar` | 46 | Quickbar do character |
| `arrow` | 1 | Arrow slot (weapon) |
| `selection` | 1 | Selection tool slot |

#### Logistic Network Structure

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `requests` | table | Items requestados (quantidade + name) |
| `providers` | table | Itens providers (chests with buffer) |
| `providers_with_requests` | table | Chests com both provider e request |
| `request_threshold` | table | Quantity thresholds por item |
| `withdrawal_distance` | number | Distância máxima de retirada |

#### Logistic Cell

Cada cell da rede logistics mantém:
- Provider/requestor status
- Item filters
- Connection neighbors
- Network ownership

### 1.3 Prototypes Data Structure

#### LuaEntityPrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "entity" | "entity" |
| `name` | string | Nome único | "iron-ore" |
| `localised_name` | {name, params} | Name with params | {"entity-name.iron-ore"} |
| `icon` | string | Caminho do ícone | "__base__/graphics/entity/iron-ore/iron-ore.png" |
| `flags` | table | Flags de entity | {"placeable-player", "not-repairable"} |
| `corpse` | string | Nome do corpse | "iron-ore-corpse" |
| `mining` | {speed, productivity, times} | Mining parameters | {speed=0.5, productivity=1, times=3} |
| `max_health` | float | Health maximum | 10 |
| `selection_box` | {{x1,y1},{x2,y2}} | Box para selection | {{-0.5,-0.5},{0.5,0.5}} |
| `collision_box` | {{x1,y1},{x2,y2}} | Box para collision | {{-0.5,-0.5},{0.5,0.5}} |
| `energy_source` | {type, buffer, consumption} | Source de energy | {type="electric", buffer=50, consumption="5kW"} |
| `energy_usage` | string | Usage por tick | "5kW" |
| `mining_speed` | float | Speed de mining | 1 |
| `pollution` | float | Pollution generation | 0.0001 |
| `base_pollution_multiplier` | float | Multiplier for base pollution | 1 |
| `resistances` | table<resistance-type, percent> | Damage resistances | {["physical"] = 0.2} |
| `immunities` | table | Immunities | {"poison"} |
| `working_visualisations` | table | Visual when working | {light, animation} |
| `animation` | {idle, working, destroyed} | Animation sets | See entity-prototype |
| `scale` | float | Size multiplier | 1 |
| `aspect_ratio` | float | Aspect ratio | 1 |
| `auto_pickup_distance` | float | Distance for auto-pickup | 0.5 |
| `pickup_distance` | float | Distance for pickups | 0.5 |

#### Exemplos Práticos

**iron-ore entity prototype:**
```lua
type = "resource"
name = "iron-ore"
mining = {speed = 0.5, productivity = 1, times = 3}
collision_box = {{-0.1, -0.1}, {0.1, 0.1}}
selection_box = {{-0.5, -0.5}, {0.5, 0.5}}
pollution = 0.0001
respawn_time = 120
```
- É um resource (tipo especial)
- Tem speed de mining productiviy
- Collision box pequeno (apenas o visual)
- Selection box maior (para player selection)
- Pollution generation
- Respawn time definido

**assembly-robot prototype:**
```lua
type = "robot"
name = "logistic-robot"
corpse = "logistic-robot-corpse"
energy_source = {type = "electric", buffer = 300, consumption = "80kW"}
energy_usage = "80kW"
speed = 0.25
pollution = 0.001
-- module slots
module_inventory_size = 1
-- cargo inventory
cargo_inventory_size = 1
```

### 1.4 Recipe Structure

#### LuaRecipePrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "recipe" | "recipe" |
| `name` | string | Nome único | "iron-plate" |
| `localised_name` | {name, params} | Name localised | {"recipe-name.iron-plate"} |
| `ingredients` | table<name, amount> | Ingredientes necessários | {["iron-ore"] = 5} |
| `results` | table<name, amount> | Resultados produzidos | {["iron-plate"] = 1} |
| `energy_required` | float | Tempo em segundos | 3.5 |
| `enabled` | boolean | Desbloqueado por défaut? | true |
| `lock_when_out_of_recipe_materials` | boolean | Travar quando sem materiais | true |
| `category` | string | Categoria do assembler | "crafting" |
| `main_product` | string | Product principal (para outputs múltiplos) | "iron-plate" |
| `subgroup` | string | Subgroup do item | "intermediate-products" |
| `order` | string | Ordem de sort | "a[iron-plate]" |
| `always_show_made_in` | boolean | Mostrar always em info? | false |

#### Exemplo: iron-plate recipe

```lua
type = "recipe"
name = "iron-plate"
ingredients = {["iron-ore"] = 5}
results = {["iron-plate"] = 1}
energy_required = 3.5
category = "smelting"
main_product = "iron-plate"
order = "a[iron-plate]"
```

#### Subsystem Interactions

- **Tech Research**: Technologies unlock recipes (`tech.prerequisites`, `tech.effects[].recipe`)
- **Assembler Compatibility**: Recipes têm `category` que determina quais machines podem craftar
- **Module Bonuses**: Modules podem modificar `energy_required`, `ingredients`, `results`
- **Productivity Modules**: Reduzem `energy_required` mas aumentam `ingredient` consumption

### 1.5 Technology Structure

#### LuaTechnologyPrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "technology" | "technology" |
| `name` | string | Nome único | "iron-smelting" |
| `localised_name` | {name, params} | Name localised | {"tech-name.iron-smelting"} |
| `icon` | string | Caminho do ícone | "__base__/graphics/technology/electric-energypolin research" |
| `unit` | {count, ingredients, time} | Custos de research | {count=50, ingredients=[["science-pack-1"]], time=30} |
| `prerequisites` | table<table> | Tech necessárias antes | {"automation"} |
| `effects` | table<effect-type, params> | Efeitos da tech | {type="unlock-recipe", recipe="iron-plate"} |
| `ancient_connections` | boolean | Tecnologia antiga? | false |
| `down_sides` | table | Efeitos colaterais | { "productivity" penalty } |
| `hidden` | boolean | Escondida na gui? | false |
| `red_cost` | color | Custo vermelho na GUI | {r=0.4, g=0.4, b=0.4} |
| `green_cost` | color | Custo verde na GUI | {r=0.8, g=0.8, b=0.8} |
| `order` | string | Ordem de sort na tech-tree | "a-a" |

#### Exemplo: iron-smelting technology

```lua
type = "technology"
name = "iron-smelting"
prerequisites = {"automation"}
unit = {count = 50, ingredients = [["science-pack-1"]], time = 30}
effects = {{type = "unlock-recipe", recipe = "iron-plate"}}
order = "a-a"
```

### 1.6 Item Prototype Structure

#### LuaItemPrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "item" | "item" |
| `name` | string | Nome único | "iron-plate" |
| `localised_name` | {name, params} | Name localised | {"item-name.iron-plate"} |
| `icon` | string | Caminho do ícone | "__base__/graphics/item/iron-plate.png" |
| `flags` | table | Flags do item | {"goes-to-main-inventory"} |
| `stack_size` | integer | Tamanho do stack | 50 |
| `subgroup` | string | Subgroup | "intermediate-products" |
| `order` | string | Ordem de sort | "a[iron-plate]" |
| `placeable_by` | string | Entity que pode placear | "assembling-machine-1" |
| `fuel_category` | string | Category de fuel | "coal" |
| `fuel_value` | string | Valor de fuel | "8MJ" |
| `fuel_acceleration_multiplier` | float | Multiplicador de acceleration | 1 |
| `fuel_acceleration_offset` | float | Offset de acceleration | 0 |
| `fuel_top_speed_multiplier` | float | Multiplicador de top_speed | 1 |
| `fuel_top_speed_offset` | float | Offset de top_speed | 0 |
| `height_modifier` | float | Modifier para height | 0 |
| `height_offset` | float | Offset de height | 0 |
| `weight` | string | Weight da entity | "auto" |
| `pictures` | {icon, grounded_icon, hand_base} | Graphics do item | ... |

#### Exemplo: iron-plate item prototype

```lua
type = "item"
name = "iron-plate"
flags = {"goes-to-main-inventory"}
stack_size = 50
subgroup = "intermediate-products"
order = "a[iron-plate]"
placeable_by = "assembling-machine-1"
```

### 1.7 Fluid Prototype Structure

#### LuaFluidPrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "fluid" | "fluid" |
| `name` | string | Nome único | "water" |
| `localised_name` | {name, params} | Name localised | {"fluid-name.water"} |
| `icon` | string | Caminho do ícone | "__base__/graphics/fluid/water.png" |
| `icon_size` | integer | Size do ícone | 64 |
| `base_color` | {r,g,b} | Cor base RGB | {r=0.4, g=0.8, b=1} |
| `flow_color` | {r,g,b} | Cor do fluxo RGB | {r=0.5, g=0.7, b=1} |
| `max_temperature` | float | Temperatura máxima | 100 |
| `heat_capacity` | string | Capacity de heat | "1MJ/" |
| `base_resource density` | float | Densidade por tile | 1 |
| `gas` | boolean | É gás? | false |
| `handled_by_pipes` | boolean | Manuseado por pipes? | true |
| `handled_by_underground_pipes` | boolean | Manuseado por underground pipes? | true |
| `max_entit` | float | Max amount em entity | 10000 |
| `base_color` | {r,g,b} | Cor para pipes | {r=0.4, g=0.8, b=1} |

#### Exemplo: water fluid prototype

```lua
type = "fluid"
name = "water"
base_color = {r=0.4, g=0.8, b=1}
flow_color = {r=0.5, g=0.7, b=1}
max_temperature = 100
heat_capacity = "1MJ/"
gas = false
handled_by_pipes = true
handled_by_underground_pipes = true
max_temperature = 100
```

### 1.8 Tile Prototype Structure

#### LuaTilePrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "tile" | "tile" |
| `name` | string | Nome único | "grass" |
| `localised_name` | {name, params} | Name localised | {"tile-name.grass"} |
| `icon` | string | Caminho do ícone | "__base__/graphics/tile/grass.png" |
| `walkable` | boolean | Pode ser atravessado? | true |
| `attle` | boolean | É attackable? | false |
| `renders` | {ground, water} | What this tile renders | {ground=true, water=false} |
| `order` | string | Ordem de sort | "a[grass]" |
| `autoplace` | string | Autoplace control | "resource" |
| `category` | string | Category para pathfinding | "water" |
| `always_autoplace` | boolean | Always autoplace? | false |

#### Exemplo: grass tile prototype

```lua
type = "tile"
name = "grass"
walkable = true
category = "ground"
order = "a[grass]"
```

### 1.9 Equipment Prototype Structure

#### LuaEquipmentPrototype Structure

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "equipment" | "equipment" |
| `name` | string | Nome único | "electric-speed-module-3" |
| `localised_name` | {name, params} | Name localised | {"equipment-name.electric-speed-module-3"} |
| `icon` | string | Caminho do ícone | "__base__/graphics/equipment/electric-speed-module-3.png" |
| `flags` | table | Flags | {"empty-toobar", "goes-to-main-inventory"} |
| `energy_source` | {type, buffer, consumption} | Source de energia | {type="electric", buffer=50, consumption="80kW"} |
| `energy_consumption` | string | Consumo por tick | "80kW" |
| `module_slots` | integer | Slots para modules | 0 (equipment itself) |
| `energy_priority` | string | Priority de energy | "secondary" |
| `stats` | {speed, productivity, distance} | Estatísticas do module | {speed=0.3, productivity=-0.2, distance=0} |

#### Exemplo: electric-speed-module-3

```lua
type = "equipment"
name = "electric-speed-module-3"
energy_consumption = "80kW"
module_slots = 0
stats = {speed = 0.3, productivity = -0.2, distance = 0}
```

### 1.10 Vehicle Prototype Structure

#### LuaVehiclePrototype Structure (resumo expandido)

| Campo | Tipo | Descrição | Exemplo |
|-------|------|-----------|---------|
| `type` | string | "vehicle" | "vehicle" |
| `name` | string | Nome único | "car" |
| `localised_name` | {name, params} | Name localised | {"vehicle-name.car"} |
| `icon` | string | Caminho do ícone | ... |
| `aspect_ratio` | float | Aspect ratio | 1.5 |
| `strength` | float | Strength do vehicle | 150 |
| `collision_box` | {{x1,y1},{x2,y2}} | Collision box | ... |
| `selection_box` | {{x1,y1},{x2,y2}} | Selection box | ... |
| `energy_source` | {type, buffer, consumption} | Energy source | ... |
| `energy_usage` | string | Energy usage/tick | ... |
| `max_speed` | float | Speed máximo | 38 |
| `max_power` | float | Power máximo | 300 |
| ` braking_power` | string | Braking power | "300kW" |
| `reverse_speed` | float | Reverse speed | 19 |
| `turn_while_reversing_speed_modifier` | float | Modifier reversing turn | 1 |
| `maximum_reversing_distance` | float | Max distance reversing | 40 |
| ` vehicle_stand_height` | float | Height quando parado | 0.4 |
| `pictures` | {standing, moving, turning, wreck} | Graphics sets | ... |
| `request_to_rotate` | boolean | Request to rotate? | false |
| `spawn_to_deconstruct` | boolean | Spawn to deconstruct? | false |

## 2. Estruturas de Rede e Logistics

### 2.1 Logistic Network State

A rede logistics mantém estado estruturado:

```lua
global.logistic_networks = {
    [1] = {  -- network ID
        providers = {},
        requesters = {},
        connections = {},
        network_name = "base-logistics",
        request_thresholds = {},
        withdrawal_distance = 30
    },
    …
}
```

### 2.2 Logistic Cell State

Cada cell da rede mantém:

```lua
global.logistic_cells = {
    [chunk_position] = {
        network_id = 1,
        provider_items = {{name="iron-plate", count=50}},
        requester_items = {{name="copper-cable", count=10}},
        neighbors = {chunk_positions...}
    }
}
```

### 2.3 Request/Provider System

**Request System**:
- Chests com `logistic_request` definem o que a rede deve provider
- `request_threshold` define quantidade mínima antes de request
- Network provider items quando available

**Provider System**:
- Provider chests armazenam items excedentess
- Network balanceia entre provider e requester
- Trash chest coleta itens não desejados

## 3. Estruturas de Tempo e Tick

### 3.1 Tick System

- **60 ticks por segundo** (1 tick = 1/60 segundo)
- `game.tick` - Current tick count (uint32, wraps a cada ~4.29 bilhões de ticks ≈ 2.3 horas)
- `defines.ticks_per_round` = 60

#### Eventos Baseados em Tick

| Evento | Descrição |
|--------|-----------|
| `on_tick` | A cada tick (a cada 1/60s) |
| `on_nth_tick(n)` | A cada n ticks |
| `on_marked_tick` | Ticks marcados explicitamente |

### 3.2 Time Structure

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `game.tick` | uint32 | Tick atual do jogo |
| `game.day` | float | Day time (0-24, 0 = midnight) |
| `game.ticktime` | string | Formatted tick time |
| `surface.is_day()` | boolean | É dia atual? |

## 4. Estruturas de Mundo e Map

### 4.1 Map Generation Settings

```lua
map_gen_settings = {
    width = 256,
    height = 256,
    starting_area = 1,
    difficulty_settings = {
        enemy_evolution = ...
    },
    seed = -723451234,
    ...
}
```

### 4.2 Surface State

Cada surface mantém:

```lua
global.surface_state = {
    width_in_chunks = 512,
    height_in_chunks = 512,
    spawn_points = {{x,y}, ...},
    pollution_fields = {{{x,y}, pollution_value}, ...},
    tile_transitions = ...,
    entity_clusters = ...,
    ...
}
```

## 5. Estruturas de Jogador

### 5.1 Player State Structure

```lua
global.player_state = {
    [player_index] = {
        character = entity*,
        force = "player",
        surface = "nauvis",
        position = {x, y},
        inventory = {{slot_id = {name, count}, ...}, ...},
        equipment = {{grid = {equipment_id = equipment_proto, ...}, ...}, ...},
        equipment_grid = {grid_size, items = {{slot_id, ...}, ...}},
        quick_bar = {{name, count}, ...},
        selected_gui_type = "some_gui",
        open_gui = "some_gui_type",
        open_inventory = "main",
        reach_distance = 32,
        reach_distance_promoted = 32,
        drop_inventory = "main",
        script_relationships = {},
        truncate_natural_selection_gameplay = false,
        selected = entity*,
        commands = {},
        taunt_cooldown = 0,
        ...
    }
}
```

### 5.2 Force State Structure

```lua
global.force_state = {
    ["player"] = {
        name = "player",
        friendly_fire = true,
        max_ammo_size = 200,
        max_power = "300kW",
        max_fight_distance = 60,
        tags = {},
        virtual_signal_count = 0,
        loot_train_state = "none",
        recipient_filter = {item = ""},
        ...
    }
}
```

## 6. Estruturas de Pesquisa (Research)

### 6.1 Research State Structure

```lua
global.research_state = {
    [force_name] = {
        current_research = "automation",
        researched technologies = {"automation", "logistics", ...},
        research_queue = {"military-2", "military-3", ...},
        science_packs_producing = {"science-pack-1", ...},
        science_packs_required = {{name="science-pack-1", count=50}, ...}
    }
}
```

## 7. Estruturas de Save/Load

### 7.1 Save Game Format

O sistema de save usa JSON estruturado com as seguintes seções principais:

| Seção | Conteúdo |
|-------|----------|
| `players` | State de cada player |
| `forces` | State de cada force |
| `surfaces` | State de cada surface/map |
| `research` | Technologies researched |
| `items` | Item counts por player/force |
| `entities` | Entity positions e states |
| `research_queue` | Fila de research pendente |
| `global` | Global state table |
| `schedule` | Train schedules, etc. |

### 7.2 Save Integrity

- SHA-256 hash do save file para integridade
- Compressão LZ4 + zlib
- Formato versionado (`save-game-format-version`)
- Migração de saves antigos via `on_configuration_changed`

## 8. Estruturas de Comportamento de IA

### 8.1 Enemy AI State

Inimigos mantêm estado:

```lua
global.enemy_ai = {
    [entity_number] = {
        target = entity* | nil,
        state = "idle" | "attacking" | "retreating" | "returning",
        last_attack_tick = uint32,
        aggression = float (0-1),
        last_player_seen = {x, y} | nil,
        last_building_seen = {entity_number, ...}
    }
}
```

### 8.2 Pathfinding Data

Dados de pathfinding mantidos:

```lua
global.pathfinding = {
    navmesh = { ... },
    cost_matrix = {{...}, ...},
    open_set = {...},
    closed_set = {...},
    current_path = {steps...}
}
```

## Observações Técnicas

1. **Tudo é Lua table**: A maior parte das estruturas de dados são tables Lua, permitindo dinamicidade.

2. **Network Serialization**: Save games serializam estas structures para JSON/binary format.

3. **Memory Layout**: O engine C++ tem structs correspondentes que são preenchidas a partir dos protótipos Lua.

4. **Versionamento**: Algumas structures têm versionamento para compatibilidade entre versões do jogo.

5. **Performance**: Estruturas são otimizadas para acesso frequente (entity state tick a cada 1/60s).

6. **Mod Compatibility**: Mods podem estender/modificar estas structures via `data:extend()` e prototype overriding.

## Próximos Relatórios Planejados

- `EXECUTION_FLOW.md` - Fluxo de execução tick-by-tick
- `ENGINE_ANALYSIS.md` - Análise do motor C++ e interações
- `RENDERING_ANALYSIS.md` - Pipeline de rendering detalhado
- `PHYSICS_ANALYSIS.md` - Física e colisões avançado
- `SIMULATION_ANALYSIS.md` - Motor de simulação completo
- `LOGISTICS_ANALYSIS.md` - Sistema logistics avançado
- `MODDING_API.md` - API de modding completa (já parcialmente coberto)