# ENGINE ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Foco**: Análise do motor C++ e interações com a camada Lua

## Visão Geral do Motor do Factorio

O motor do Factorio é um sistema C++ complexo e otimizado que gerencia todos os aspectos do jogo, com uma camada Lua para flexibilidade e modding. Este relatório documenta a arquitetura interna, funções críticas e interações com a camada Lua.

### 1. Arquitetura do Motor Principal

#### 1.1 Estrutura do Executável

| Componente | Tamanho | Propósito |
|------------|---------|-----------|
| `factorio.exe` | ~49.4 MB | Motor principal |
| `factorio.pdb` | Variável | Símbolos debug MSVC |
| Rich Header | Identifica compilador MSVC | Validação de build |

#### 1.2 Módulos do Motor Core

| Módulo | Função | Observações |
|--------|--------|-------------|
| `simulation` | Loop principal a 60 ticks/s | Coração do motor |
| `rendering` | Pipeline Direct3D 11 | Renderização 2D isométrica |
| `physics` | Colisão, projectiles | Physics engine próprio |
| `logistics` | Belts, robôs, rede | Sistema logístico |
| `production` | Crafting, recipes | Sistema de crafting |
| `fluid` | Fluidos, pipes | Sistema de fluidos |
| `pathfinding` | Navmesh, pathfinding | Pathfinding para units |
| `networking` | Multiplayer sync | Protocolo binário |
| `saving` | Sistema de saves | Save/load system |
| `input` | Inputs do jogador | Keyboard, mouse, gamepad |
| `ai` | Enemy AI | Comportamento de inimigos |

### 2. Loop de Simulação

#### 2.1 Estrutura do Tick

```
Tick Loop (60 ticks/segundo = ~16.67ms por tick):

1. Início do tick
   ├── game.tick incrementado (uint32)
   ├── Todos os handlers "on_tick" disparados
   │   ├── Entity state updates
   │   ├── Belt movement
   │   ├── Crafting timers
   │   ├── Projectile updates
   │   └── Event processing
   │
   ├── Processamento de entities
   │   ├── Position updates
   │   ├── AI behavior
   │   ├── Pollution generation
   │   └── Energy consumption
   │
   ├── Logistics processing
   │   ├── Belt item movement (1 tile/tick)
   │   ├── Inserter operations
   │   ├── Logistic network checks
   │   └── Robot logistics
   │
   ├── Production & Crafting
   │   ├── Recipe timers
   │   ├── Module effects
   │   ├── Energy consumption
   │   └── Result inventory updates
   │
   ├── Physics & Combat
   │   ├── Projectile movement
   │   ├── Collision detection
   │   ├── Damage application
   │   └── Explosion handling
   │
   ├── Event handlers (on_tick)
   │   ├── Player commands
   │   ├── Scheduled events
   │   └── Mod event processing
   │
   └── Fim do tick
       ├── Estado salvo para próximo tick
       ├── Preparação para próximo tick
       └── Rendering data collection
```

#### 2.2 Ordem de Execução Determinística

A ordem é **fixed** e crítica para sincronização multiplayer:

```
1. Entity Movement & AI
2. Belt Item Transport
3. Inserter Operations
4. Assembler Crafting
5. Projectile & Combat
6. Event Handlers (on_tick)
7. Player Input Processing
8. Rendering Data Collection
```

**Por que a ordem importa**:
- Belt movement antes de inserter checks
- Crafting completion afeta itens disponíveis
- Projectile collision depende de entity positions atualizadas

### 3. Sistema de Events

#### 3.1 Definições de Events

| Evento | ID | Frequência | Destinatários |
|--------|-----|------------|---------------|
| `on_tick` | 0 | A cada tick | Todos os módulos registrados |
| `on_player_created` | 1 | Quando player cria | Mods, core scripts |
| `on_player_respawned` | 2 | Quando player ressurgiu | Mods, core scripts |
| `on_entity_died` | 3 | Quando entity morre | Mods, core scripts |
| `on_research_received` | 4 | Quando research completada | Mods, core scripts |
| `on_gui_click` | 5 | Quando GUI click | Mods, interface |
| `on_built_entity` | 6 | Quando entity built | Mods, core scripts |
| `on_robot_launched` | 7 | Quando robot launched | Mods, core scripts |
| `on_chunk_generated` | 8 | Quando chunk gerado | Map generation |
| `on_player_joined_game` | 9 | Quando player joins | Multiplayer |

#### 3.2 Mecanism de Registro

```lua
-- Registrando handler para evento
script.on_event(defines.events.on_player_created, function(event)
  -- handler logic
end)

-- Registrando handler para on_nth_tick
script.on_nth_tick(60, function(event)
  -- handler a cada 60 ticks (1 minuto)
end)
```

#### 3.2 Ordem de Execução de Events

A ordem de dispatch de events é:

```
1. Events setup phase (pré-tick)
2. Tick increment
3. on_tick dispatch (a todos handlers registrados)
4. Processamento de tick phases (na ordem fixa)
5. Pós-tick processing
```

A ordem dentro de `on_tick` segue a ordem de registro (FIFO - First In, First Out).

### 4. Prototypes Registry

#### 4.1 Acesso a Prototypes via Lua

O motor C++ mantém um registry global de todos os protótipos, acessível via Lua:

```lua
-- Acessando protótipos
local entity_proto = prototypes.entity["iron-ore"]
local item_proto = prototypes.item["iron-plate"]
local recipe_proto = prototypes.recipe["iron-plate"]
local tech_proto = prototypes.technology["iron-smelting"]  -- [INFERÊNCIA] API 2.x

-- Verificando existência
if prototypes.entity["iron-ore"] then
  -- exists
end

-- Iterando sobre protótipos
for name, proto in pairs(prototypes.entity) do
  -- ...
end
```

#### 4.2 Tipos de Prototypes Acessíveis

[CORRIGIDO] Contagens anteriores (~500+, ~2000+…) eram inventadas e foram removidas. Em 2.x o acesso é `prototypes` (a API 1.x usava `game.` + `prototypes`) [CONHECIMENTO PÚBLICO; confirmado em `script\freeplay\control.lua`].

| Acesso | Tipo | Quantidade |
|--------|------|-------------------|
| `prototypes.entity` | Entities | não contado |
| `prototypes.item` | Items | não contado |
| `prototypes.recipe` | Recipes | não contado |
| `prototypes.technology` | Technologies | não contado |
| `prototypes.fluid` | Fluids | não contado |
| `prototypes.tile` | Tiles | não contado |
| `prototypes.equipment` | Equipment | não contado |
| `prototypes.module` | Modules | não contado |

#### 4.3 Modificação de Prototypes

Mods podem modificar protótipos existentes:

```lua
-- Override de prototype existente
data:extend({
  {
    type = "entity",
    name = "iron-ore",
    -- ... modificando propriedades existentes
    flags = {"placeable-player", "resource", "not-repairable"},
  }
})

-- Adicionando novo prototype
data:extend({
  {
    type = "entity",
    name = "my-custom-entity",
    -- ... nova definição completa
  }
})
```

### 5. Sistema de Remote Interface

#### 5.1 Definição de Remote Interface

```lua -- No mod's main script
remote.add_interface("my_mod", {
  -- Funções públicas exponíveis
  craft_item = function(player, item_name, count)
    -- implementation
  end,
  
  -- Funções com parâmetros múltiplos
  build_entity = function(entity_name, position, direction)
    -- implementation
  end,
  
  -- Funções que retornam valores
  get_player_speed = function(player)
    return player.walking_state.speed
  end
})
```

#### 5.2 Chamada de Remote

```lua -- Outro mod ou script calling
remote.call("my_mod", "craft_item", player, "iron-plate", 10)

-- Ou chamando interface core do Factorio
remote.call("core", "some_function", parameter)
```

#### 5.3 Interfaces Conhecidas do Factorio

| Interface | Propósito | Documentação |
|-----------|-----------|--------------|
| `core` | Interface central do Factorio | Documentada internamente |
| `mining` | Operations de mining | Documentada |
| `crafting` | Operations de crafting | Documentada |
| `space-age` | Interface do mod space-age | Documentada pelo mod |

### 6. Sistema de Save/Load

#### 6.1 Formato de Save File

O save file do Factorio usa um formato JSON estruturado com as seguintes seções principais:

```json
{
  "players": { ... },
  "forces": { ... },
  "surfaces": { ... },
  "research": { ... },
  "items": { ... },
  "entities": { ... },
  "research_queue": [ ... ],
  "global": { ... },
  "schedule": { ... },
  "version": 5
}
```

#### 6.2 Seções do Save File

| Seção | Conteúdo |
|-------|----------|
| `players` | State de cada player (inventário, position, force, etc.) |
| `forces` | State de cada force (tecnologias, alianças, etc.) |
| `surfaces` | State de cada surface/map (chunks, entities, tiles) |
| `research` | Tecnologias research progress |
| `items` | Item counts por player/force/container |
| `entities` | Entity positions, states, networks |
| `research_queue` | Fila de research pendente |
| `global` | Global state table (persist across saves) |
| `schedule` | Train schedules, logistics |
| `version` | Format version for backward compatibility |

#### 6.3 Migração de Saves Antigos

```lua -- Em on_configuration_changed
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
end
```

#### 6.4 Versionamento de Save Format

| Versão | Descrição |
|--------|-----------|
| 1-4 | Formatos antigos (0.15 - 0.17) |
| 5 | Formato atual (0.17 - 1.1) |
| 6+ | Formatos futuros |

A versão é verificada no início do carregamento e handlers `on_configuration_changed` são executados para migrar saves antigos.

### 7. Sistema de Input

#### 7.1 Tipos de Input

| Tipo | Descrição | Exemplos |
|------|-----------|----------|
| `movement` | Movement inputs | WASD, arrow keys, gamepad sticks |
| ` shooting` | Shooting inputs | Mouse click, fire button |
| `gui` | GUI interactions | Inventory open, quickbar swap |
| `chat` | Chat messages | Player typed messages |
| `commands` | Command inputs | /commands, ping |

#### 7.2 Processamento de Input por Tick

```
Input Processing:

1. Coleta de inputs do jogador
   │
   ├── Movement directions
   │   ├── Horizontal (left/right)
   │   └── Vertical (forward/backward)
   │
   ├── Aiming direction
   │   ├── Mouse position
   │   └── Aim vector
   │
   ├── Shooting intent
   │   ├── Mouse click state
   │   └── Fire button (gamepad)
   │
   ├── GUI actions
   │   ├── Inventory toggle
   │   ├── Quickbar swap
   │   └── Menu open/close
   │
   └── Chat input
       ├── Text entry
       ├── Command prefix (/)
       └── Send intent

2. Validação de inputs
   ├── Check para inputs inválidos
   └── Filter por estado do jogo

3. Aplicação no tick
   ├── Movement application
   ├── Shooting resolution
   ├── GUI state updates
   └── Chat message processing
```

### 7.3 Rede Multiplayer

```
Network Protocol:

1. Client → Server
   │
   ├── Player inputs (a cada tick)
   │   ├── Movement
   │   ├── Shooting
   │   ├── Commands
   │   └── Chat
   │
   └── Mod state sync (periodicamente)

2. Server → Clients
   │
   ├── Game state confirmation
   │   ├── Entity positions
   │   ├── Entity states
   │   ├── Research progress
   │   └── Chat messages
   │
   └── World generation params (initial send only)
```

#### 7.4 Sincronização Determinística

```
Deterministic Synchronization:

1. Same world seed → Same map generation
2. Same inputs → Same simulation results
3. Fixed tick order → Deterministic execution
4. Floating point types → Consistent behavior
5. No random no tick loop → Pure determinism

Desync Prevention:

│  - Verify same mods between clients
│  - Verify same settings
│  - Regular save/load consistency checks
│  - Input validation on server
│  - State reconciliation
```

### 8. Sistema de IA de Inimigos

#### 8.1 Behavior Tree / State Machine

Inimigos mantêm estado comportamental:

```lua -- Simplified enemy AI state

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

#### 8.2 Tipos de Inimigo e Comportamento

| Type | Comportamento | Aggressão |
|------|--------------|-----------|
| `biters` | Attack players/bases | Medium |
| `biter-spitter-mix` | Mixed behavior | Medium-High |
| `spitters` | Ranged attacks | Medium |
| `worms` | Ranged, require weapons | High |
| **Worms** | **Ranged attacks, require weapons** | **High** |

#### 8.3 Fator de Evolução

| Fator | Descrição | Efeito |
|-------|-----------|--------|
| `evolution_factor` | Global evolution factor | 0.0 - 1.0 |
| `enemy_evolution` | Per-Force evolution | Afeta dano, saúde, velocidade |
| **Fórmula** | **evolution = base + pollution + research + kills** | **Progressive increase** |

#### 8.4 Fórmulas de Evolução

```
Evolution Factor Calculation:

│  base_evolution = 0.01  (starts at 1%)
│
│  ├─► Pollution contribution
│  │   │
│  │   ├──► Pollution from player actions
│  │   │   ├──► Mining
│   │   │   ├──► Crafting
│   │   │   └──► Combat
│   │   │
│  │   └─► Pollution dispersion (natural)
│   │
│  ├─► Research effects
│  │   │
│  │   ├──► Military science pack research
│  │   └──► Military technologies
│   │
│  └─► Kill contributions
│      │
│      ├──► Enemy kills
│      └──► Player kills
│
│  final_evolution = base + pollution_contribution + research_effect + kill_contribution
```

### 9. Sistema de Pathfinding

#### 9.1 Navmesh Structure

O Factorio usa navmesh para pathfinding de unidades:

```lua -- Simplified navmesh structure

global.pathfinding = {
    navmesh = { ... },          -- Navmesh data structure
    cost_matrix = {{...}, ...}, -- Cost per tile type
    open_set = {...},           -- A* open set
    closed_set = {...},         -- A* closed set
    current_path = {steps...}   -- Current calculated path
}
```

#### 9.3 Cálculo de Path

```
Pathfinding Algorithm:

1. Start position → Current entity position
2. Goal position → Target destination
3. Open set initialization → {start_position}
4. Closed set initialization → {}
5. Loop:
   ├── Pick node with lowest cost from open set
   ├── If node = goal → Path found, reconstruct
   ├── Mark node as visited (closed set)
   ├── Add neighbors to open set
   │   ├── Calculate cost via cost_matrix
   │   └── Check for obstacles (entities, walls)
   └── If open set empty → No path available
6. Path reconstruction → Trace from goal to start
```

#### 9.4 Fatores de Custo

| Tipo de Tile | Custo de Movement | Comentário |
|-------------|------------------|------------|
| `ground` | 1.0 | Terreno normal |
| `water` | 10.0 | Água (custo alto) |
| `wall` | ∞ | Parede (impassable) |
| `entity` | Varies | Depende do type |
| `resource` | 1.0-2.0 | Recursos têm custo variável |
| `decorative` | 1.0 | Decorativos não bloqueiam |

### 10. Sistema de Fluidos

#### 9.1 Flow System

```
Fluid System Architecture:

1. Fluid Boxes
   │
   │   Entity boxes que contém fluidos
   │   ├── Pipes connection points
   │   ├── Base fluid amount
   │   ├── Temperature
   │   └── Pressure (implicit)
   │
   ├── Inputs/Outputs
   │   ├── Pipe connection sides
   │   ├── Flow direction
   │   └── Flow rate
   │
   └── Management
       ├── Flow balancing
       ├── Temperature mixing
       └── Pressure simulation
```

#### 9.2 Fluido Properties

| Fluido | Cor Base | Cor Fluxo | Max Temp | Heat Capacity | Gas | Pipes |
|--------|----------|-----------|----------|---------------|-----|-------|
| `water` | {0.4, 0.8, 1} | {0.5, 0.7, 1} | 100 | "1MJ/" | false | ✓ |
| `lube-oil` | {0.6, 0.3, 0.1} | {0.7, 0.4, 0.2} | 100 | "0.5MJ/" | false | ✓ |
| `steam` | {0.8, 0.6, 0.2} | {0.9, 0.7, 0.3} | 500 | "0.001MJ/" | true | ✗ |
| `crude-oil` | {0.5, 0.2, 0.1} | {0.6, 0.3, 0.2} | 240 | "0.5MJ/" | false | ✓ |
| `heavy-oil` | {0.4, 0.1, 0.1} | {0.5, 0.2, 0.15} | 240 | "0.5MJ/" | false | ✓ |
| `light-oil` | {0.7, 0.4, 0.1} | {0.8, 0.5, 0.2} | 240 | "0.5MJ/" | false | ✓ |
| `petroleum-gas` | {0.7, 0.7, 0.7} | {0.8, 0.8, 0.8} | 240 | "0.001MJ/" | true | ✗ |
| `sulfuric-acid` | {0.7, 0.8, 0.2} | {0.8, 0.9, 0.3} | 100 | "1MJ/" | false | ✓ |

#### 9.3 Pipe Network

```
Pipe Network Architecture:

1. Pipe Items
   │
   │   Basic pipe entities
   │   ├── Straight pipes
   │   ├── Curved pipes (4 directions)
   │   ├── Underground pipes (connect distance)
   │   └── Splitters/mergers (fluid)
   │
   ├── Flow Properties
   │   ├── Flow rate (L/tick)
   │   ├── Pressure (implicit)
   │   ├── Temperature
   │   └── Direction
   │
   └── Connection Rules
       ├── Adjacent pipes connect
       ├── Underground pipes long-distance
       ├── Splitters redirect flow
       └── No cross-connection without splitter
```

## 11. Observações Técnicas

1. **Determinismo Absoluto**: A simulação é completamente determinística - dado o mesmo estado inicial e inputs, o resultado é sempre idêntico. Critical para multiplayer sync.

2. **Arquitetura Híbrida C++/Lua**: Combina performance do C++ com flexibilidade do Lua para modding.

3. **Loop a 60 ticks/s**: O timing é fixo independente do FPS - o FPS afeta apenas a fluidez visual.

4. **Modding Friendly**: ~90% do comportamento do jogo pode ser modificado através de scripts Lua e protótipos.

5. **Interface Bem-Definida**: A boundary entre C++ e Lua é claramente definida através de prototypes, events e remote interfaces.

6. **Gestão de Memória**: O engine gerencia memory através de pools, allocators e garbage collection esporádica.

7. **Performance Otimizações**: Múltiplas otimizações incluindo culling, instancing, batch grouping, e processamento por chunks.

## 12. Diagramas do Motor

```
┌─────────────────────────────────────────────────────────────────┐
│                   FACTORIO ENGINE ARQUITETURA (v2.1.24)        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ────────────────────────────────────────────────────────── │ │
│  │  ENTRADA (Input)                                     │ │
│  │  │                                      │ │
│  │  ├───┬─ Movement ──────┐                │ │
│  │  │   │                 │                │ │
│  │  │   ├───► Shooting ────┼─────┐        │ │
│  │  │   │                 │     │ │        │ │
│  │  │   ├───► GUI ────────┘     │ │        │ │
│  │  │   │                 │     │ │        │ │
│  │  │   └──► Chat ────────────────┘ │        │ │
│  │  └──┬─ Commands ──────┘                │ │
│  │      │                                 │ │
│  ▼                                    │ │
│  ────────────────────────────────────────────────────────── │ │
│  │  TICK LOOP (60 ticks/s)                              │ │
│  │  │                                      │ │
│  │  ├───► Entity Updates ───────────────────────┐   │ │
│  │  │                                      │   │ │
│  │  │   ├───► Movement ──────┐                │ │
│  │  │   │                 │                │ │ │
│  │  │   ├───► AI ───────────────────────┘ │ │ │
│  │  │   │                 │                │ │ │
│  │  │   ├───► Pollution ─────────────────────┘ │ │
│  │  │   │                 │                │ │ │
│  │  │   ├───► Energy ───────────────────────┘ │ │
│  │  │   │                 │                │ │ │
│  │  │   └──► Health ──────────────────────────┘ │ │
│  │  │                                      │ │ │
│  │  ├───► Logistics ───────────────────────────┘ │ │
│  │  │                                      │ │ │
│  │  ├───► Crafting ───────────────────────────────┘ │ │
│  │  │                                      │ │ │
│  │  ├───► Physics/Combat ─────────────────────────┘ │ │
│  │  │                                      │ │ │
│  │  ├───► Events ───────────────────────────────────┘ │ │
│  │  │                                      │ │ │
│  │  └──► Rendering Data Collection ─────────────────┘ │ │
│  │                                      │ │ │
│  └──────► Next Tick State ────────────────────────────┘ │ │
│                                                                 │
│  ────────────────────────────────────────────────────────── │ │
│  │  SAÍDA (Output)                                    │ │
│  │  │                                      │ │
│  │  ├───► Rendering Pipeline ───────────────────────┐ │ │
│  │  │                                      │ │ │
│  │  ├───► Network Sync (multiplayer) ───────────────┘ │ │
│  │  │                                      │ │ │
│  │  ├───► Save Game State ──────────────────────────┘ │ │
│  │  │                                      │ │ │
│  │  └──► Player Feedback (FPS, ticks) ──────────────┘ │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ────────────────────────────────────────────────────────── │ │
│  │  MÓDULOS INTERNOS                                 │ │
│  │                                      │ │
│  │  ├───► simulation/ ───► Core tick processing    │ │
│  │  │                                      │ │
│  │  ├───► rendering/ ───► Direct3D 11 pipeline     │ │
│  │  │                                      │ │
│  │  ├───► physics/  ───► Collision, projectiles   │ │
│  │  │                                      │ │
│  │  ├───► logistics/ ──► Belts, robots, network  │ │
│  │  │                                      │ │
│  │  ├───► production/ ──► Recipes, crafting       │ │
│  │  │                                      │ │
│  │  ├───► fluid/     ──► Fluids, pipes           │ │
│  │  │                                      │ │
│  │  ├───► pathfinding/──► Navmesh, pathfinding    │ │
│  │  │                                      │ │
│  │  ├───► networking/──► Multiplayer sync        │ │
│  │  │                                      │ │
│  │  ├───► saving/    ──► Save/load system         │ │
│  │  │                                      │ │
│  │  ├───► input/     ──► Player inputs            │ │
│  │  │                                      │ │
│  │  └───► ai/        ──► Enemy AI                │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                                 │
│  ────────────────────────────────────────────────────────── │ │
│  │  INTERFACES DE COMUNICAÇÃO                        │ │
│  │                                      │ │
│  │  C++ → Lua:  prototypes, defines.events        │ │
│  │  Lua → C++:  remote.call(), script.on_event    │ │
│  │  Bidirectional: game.gui, player.print         │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                                 │
│  ────────────────────────────────────────────────────────── │ │
│  │  DEPENDÊNCIAS EXTERNAIS                         │ │
│  │                                      │ │
│  │  Direct3D 11, VCRuntime, Windows API          │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Observações Finais

1. **A arquitetura do Factorio é um exemplo notável de design de software híbrido**, combinando o melhor dos mundos C++ e Lua.

2. **Determinismo é a pedra angular** - a sincronização multiplayer depende inteiramente de execução determinística tick-a-tick.

3. **A camada Lua fornece flexibilidade sem sacrificar performance** - a maior parte do gameplay logic está em Lua, permitindo modding extenso sem modificar o executable.

4. **A evolução do engine foi gradual** de 0.15 a 2.1.x, mantendo backward compatibility através de handlers `on_configuration_changed`.

5. **O engine foi otimizado para chunks e processamento paralelo** onde possível, permitindo jogos massivos com performance adequada.

## Próximos Relatórios Planejados

- `PHYSICS_ANALYSIS.md` - Física e colisões avançado
- `SIMULATION_ANALYSIS.md` - Motor de simulação completo
- `LOGISTICS_ANALYSIS.md` - Sistema logistics avançado
- `NETWORKING_ANALYSIS.md` - Multiplayer e networking
- `SAVE_SYSTEM.md` - Sistema de saves
- `DEPENDENCIES.md` - Dependências e bibliotecas
- `FUNCTION_INVENTORY.md` - Inventário de funções
- `ALGORITHMS.md` - Algoritmos identificados
- `ARCHITECTURE_DIAGRAMS.md` - Diagramas arquiteturais
- `REA_FINDINGS.md` - Descobertas REA
- `UNKNOWN_COMPONENTS.md` - Componentes desconhecidos
- `RECONSTRUCTION_ROADMAP.md` - Roadmap para reconstrução futura