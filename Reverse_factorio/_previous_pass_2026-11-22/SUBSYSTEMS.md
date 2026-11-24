# SUBSYSTEMS ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Foco**: Subsystems do motor de jogo identificados através de análise de arquivos e protótipos

## Visão Geral dos Subsystems

O Factorio 2.1.24 organiza seu motor em subsistemas distintos, cada um com responsabilidades específicas. A arquitetura segue um padrão de "separation of concerns" com comunicação através de eventos e prototypes.

### 1. Sistema de Simulação (Game Simulation)

#### Responsabilidades
- Tick-based simulation (60 ticks/second)
- Entity state management
- AI e comportamento de inimigos
- Pathfinding navmesh
- Resource node depletion

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| `defines.ticks_per_round` | Core defines | 60 ticks por round |
| `entity causing pollution` | Prototypes | Entities with `pollution` property |
| `pathfinder` | `pathfinder-overrides.lua` | Overrides de pathfinding |
| `evolution factor` | `player-evolution.lua` | Fator de evolução dos inimigos |

#### Funcionamento Chave
- Cada tick, o motor processa todas as entidades ativas
- Inimigos atacam jogadores e bases através de AI scripted
- Recursos (ore patches) são esgotados gradualmente
- O pathfinding usa navmesh para unidades caminharem

#### Ponto de Extensão
- `defines.events.on_tick` - Hook para custom tick processing
- `game.forces.enemy` - Acesso ao force de inimigos
- `surface.find_path()` - API de pathfinding

### 2. Sistema de Logistics (Logistics System)

#### Responsabilidades
- Transportador (belts) movimentação de items
- Inserters pegando/colocando items
- Logistics network (inserters de robôs, request/buffer chests)
- Cargo wagons e logistic wagons

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| `transport-belts.lua` | prototypes/entity | Definições de belts |
| `inserter prototypes` | prototypes/entity | Definições de inserters |
| `logistic-cell` | Engine core | Cell da rede logistics |
| `robot-tooling` | Prototypes | Tooling for robôs |

#### Funcionamento Chave

**Belt System**:
- Items movem-se a 1 tile/tick nas belts
- Items podem ser redirecionados com splitters
- Items desaparecem se caírem do mapa

**Inserter System**:
- Inserters pegam items da belt e colocam em inventários
- Inserters de robôs usam logistic network
- Speed/effectivity stats definem performance

**Logistics Network**:
- Robôs de logística transportam entre chests/requesters
- Network coverage definido por robôs e roboports
- Request/buffer chests definem necesidades/overflows

#### Ponto de Extensão
- `defines.events.on_robot_launched` - Robot launch events
- `game.create_robot()` - Criar robôs manualmente
- `surface.request_entities()` - Solicitar entity spawn

### 3. Sistema de Produção e Crafting (Production/Crafting)

#### Responsabilidades
- Recipes de crafting
- Assembler machines
- Fluid handling
- Electricity consumption

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| `recipes.lua` | prototypes/recipe | ~400 recipes definidas |
| `assembler-pictures.lua` | prototypes/entity | Graphics do assembler |
| `fluid.lua` | prototypes/fluid | Fluids handling |
| `electric-energy` | Prototypes | Energy consumption |

#### Funcionamento Chave

**Crafter System**:
- Assemblers consomem items + energy → produzem output items
- Speed modifier afeta velocidade de crafting
- Module slots afeta bonificações
- Energy source define tipo de energia (electric, steam, solar)

**Recipe System**:
- Cada recipe tem inputs, outputs, time, energy cost
- Tech research desbloqueia recipes
- Tech prerequisites definem tree de research

**Fluid System**:
- Pipes transport fluids
- Pumpjacks extraem oil
- Fluid tanks storage
- Fluid boxes em entities

#### Ponto de Extensão
- `defines.events.on_recipe_learned` - Quando tech é researched
- `player.remove_recipe()` - Remover recipe do player
- `game.reset_recipes()` - Reset all recipes

### 4. Sistema de Renderização (Rendering)

#### Responsabilidades
- Direct3D 11 rendering
- Sprites e tiles
- Partículas
- Interface do usuário (UI)
- Animações de entidades

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| `graphics/` | data\base\graphics/ | Definições de recursos gráficos |
| `entity-animations.lua` | prototypes/ | Animações de entities |
| `rendering pipeline` | Engine core | Pipeline renderização |

#### Funcionamento Chave

**Render Pipeline**:
- 60 FPS render loop
- Entity sprites desenhados baseados no estado da simulação
- Interface UI desenhada overlay
- Particles effects para explosions, smoke, etc.

**Sprites e Animações**:
- Entities têm sprite sheets definidos em protótipos
- Animações baseadas no estado (working, idle, running, etc.)
- Rotacionamento baseado no direction of travel

**Interface do Usuário**:
- HUD elements definidos em protótipos
- Tool_tips baseados em entity prototypes
- Minimap e visão de área

#### Ponto de Extensão
- `defines.events.on_render_altered` - Hook para rendering customizado
- `player.create_equipment_grid()` - Grid equipment UI
- `gui-element` creation API

### 5. Sistema de Física e Colisões (Physics/Collisions)

#### Responsabilidades
- Hitboxes e collision masks
- Projectile physics
- Explosion radius
- Damage calculation

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| `collision-layers.lua` | prototypes/ | Definições de layers |
| `entity-util.lua` | prototypes/ | Utilities for entities |
| `damage-type.lua` | prototypes/ | Types of damage |
| `projectiles.lua` | prototypes/ | Projectile definitions |

#### Funcionamento Chave

**Collision System**:
- Cada entity tem collision box definidapor `collision-layer`
- Projectiles e bullets usam collision masks para determinar hits
- Entity-to-entity collisions definem comportamento de empurrão

**Damage System**:
- `damage-type` defines: physical, explosive, fire, acid, nuclear
- Each type tem modifiers vs entity types
- Entity resistances definidas em protótipos

**Projectile Physics**:
- Linhas retas projéteis (tanks bullets, laser)
- Explosivos têm area of effect (radius)
- Flying robo projectiles usam pathfinding

#### Ponto de Extensão
- `defines.events.on_entity_died` - Quando entity dies
- `entity.deal_damage()` - Causar dano
- `create_explosion()` - Criar explosão

### 6. Sistema de Multiplayer e Networking (Multiplayer/Networking)

#### Responsabilidades
- Sincronização de jogo entre players
- Protocolo de transmissão
- Sistema de saves e loads
- Conexões player-host

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| Network protocol | Engine core | Protocolo binário customizado |
| `save-system` | Engine core | Saves e loads |
| `multiplayer-settings.lua` | Configurações | Configurações multiplayer |

#### Funcionamento Chave

**Jogo Sincronizado**:
- Todos os clients recebem o mesmo tick sequence
- Inputs dos players são transmitidos e aplicados simetricamente
- O servidor (ou host) é a autoridade final

**Sistema de Saves**:
- `save-game` contains entire game state
- Incremental saves a cada few minutos
- Manual saves via F5/save menu
- Save format includes: entity positions, player state, map state, research, etc.

**Network Protocol**:
- Pacotes binários enviados a cada tick
- Player inputs (movement, shooting, etc.)
- Entity state changes
- Chat messages

#### Ponto de Extensão
- `defines.events.on_player_joined_game` - Player joins
- `defines.events.on_player_left_game` - Player leaves
- `game.save()` - Manual save
- `game.load()` - Load save

### 7 Sistema de Modding API (Modding API)

#### Responsabilidades
- Registro de prototypes
- Event handlers
- Remote interfaces
- Compatibilidade entre mods

#### Evidências Observadas

| Componente | Arquivo/Referência | Descrição |
|------------|-------------------|-----------|
| `prototypes` | Engine core | Registry of all prototypes |
| `remote.call()` | Engine core | Remote procedure calls |
| `script.register_on_event()` | Engine core | Event registration |
| `data.filters` | Engine core | Data validation |

#### Funcionamento Chave

**Prototype Registration**:
- Mods podem adicionar novos entities, items, recipes via `data:extend()`
- Prototype override via `prototypes.entity.xxx = {...}`
- Hidden prototypes podem ser tornados visíveis

**Event System**:
- `script.on_event(event_id, handler)` - Registrar handler
- `defines.events` - Lista completa de event IDs
- Eventos podem ser definidos por mods

**Remote Interface**:
- `remote.define("mod-name", {functions})` - Definir interface remote
- `remote.call("mod-name", "function-name", ...)` - Chamar função remota
- Compatibilidade entre versões definidas em `remote.interfaces`

**Data Updating**:
- `data.updates[1].for_each_entity()` - Atualizar entities em saves
- `data.fixes` - Fixes for old saves

#### Ponto de Extensão
- `script.register_on_nth_tick(n, handler)` - Handler a cada n ticks
- `prototypes.extend()` - Extender prototypes existentes
- `table.deepcopy()` - Deep copy for mod data

## Mapa de Interdependências entre Subsystems

```
┌─────────────────────────────────────────────────────────────────┐
│          FACTORIO ENGINE SUBSYSTEMS (v2.1.24)                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   SIMULATION    │  │   LOGISTICS     │  │   PRODUCTION    │ │
│  │   (Tick 60Hz)   │  │   (Belts/Robots)│  │   (Crafting)    │ │
│  └───────▲─────────┘  └───────▲─────────┘  └───────▲─────────┘ │
│           │                    │                    │           │
│  ┌───────▼─────────┐  ┌───────▼─────────┐  ┌───────▼─────────┐ │
│  │  PHYSICS/COLL   │  │  RENDERING      │  │  FLUIDS         │ │
│  │  ISION          │  │   (GPU*)        │  │   (Pipes)       │ │
│  └───────▲─────────┘  └───────▲─────────┘  └───────▲─────────┘ │
│           │                    │                    │           │
│  ┌───────▼─────────────────────▼───────▼───────▲───────┐ │
│  │                   MULTIPLAYER/NETWORKING                     │ │
│  │                   (Authority Sync)                       │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                 │
│  TODOS os subsystems comunicam através de:                      │
│  - prototypes (registro/acesso)                                 │
│  - defines.events (event system)                               │
│  - remote.call (inter-mod communication)                       │
│  - game.tick (sincronização)                                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Observações Técnicas

1. **Tick-Based Architecture**: Todos os subsystems operam no tick do jogo (60 ticks/segundo). A sincronização é crítica para multiplayer.

2. **Data-Driven Design**: A maior parte do comportamento é definida em protótipos Lua, permitindo modding sem alterar o código C++.

3. **Separation of Concerns**: Cada subsystem tem responsabilidades bem definidas e se comunica através de interfaces claras.

4. **Extensibility Points**: O Factorio provê muitos hooks (`defines.events`) para mods interagirem com o motor base.

5. **Version Compatibility**: A estrutura de subsystems é estável entre versões 0.15 a 2.1.x, com adições graduais de features.

6. **Limitations**: A análise estática limitada - o comportamento real requer execução do jogo para validar interações entre subsystems.

## Próximos Relatórios Planejados

- `DATA_STRUCTURES.md` - Estruturas de dados internas detalhadas
- `EXECUTION_FLOW.md` - Fluxo de execução tick-by-tick
- `RENDERING_ANALYSIS.md` - Análise profunda do rendering pipeline
- `PHYSICS_ANALYSIS.md` - Física e colisões detalhadas
- `SIMULATION_ANALYSIS.md` - Motor de simulação detalhado
- `LOGISTICS_ANALYSIS.md` - Sistema logistics detalhado
- `MODDING_API.md` - API de modding completa