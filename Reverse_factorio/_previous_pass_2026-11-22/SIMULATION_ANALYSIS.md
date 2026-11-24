# SIMULATION ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Foco**: Motor de simulação tick-based do Factorio

## Visão Geral do Motor de Simulação

O Factorio 2.1.24 apresenta um motor de simulação determinístico a 60 ticks por segundo (1 tick = 1/60 segund0). Este relatório documenta a arquitetura interna, fluxo de execução e mecanismos críticos da simulação do jogo.

### 1. Arquitetura de Simulação

#### 1.1 Loop de Tick Principal

```
┌─────────────────────────────────────────────────────────────────┐
│                   SIMULATION TICK LOOP (60 Hz)                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Tick Cycle (repetido a cada ~16.67ms):                         │
│                                                                 │
│  ┌─────────────────────┐  ┌─────────────────────┐             │
│  │ 1. Tick Start       │  │  5. Event Processing │             │
│  │    game.tick++     │  │    (on_tick handlers)│             │
│  │                     │  │                     │             │
│  │  ┌─────────────────┐  │  ┌─────────────────┐             │
│  │  │ 2. Entity Update  │  │  │ 6. Rendering Data │             │
│  │  │    (positions,   │  │    (collection)     │             │
│  │  │    state changes) │  │                     │             │
│  │  └─────────────────┘  │  └─────────────────┘             │
│  │                       │                                   │
│  │  ┌─────────────────┐  │  ┌─────────────────┐             │
│  │  │ 3. Logistics     │  │  │ 7. Tick End      │             │
│  │  │    Processing    │  │    (state save)   │             │
│  │  └─────────────────┘  │  └─────────────────┘             │
│  │                       │                                   │
│  │  ┌─────────────────┐  │  ┌─────────────────┐             │
│  │  │ 4. Production    │  │  └─────────────────┘             │
│  │  │    (crafting)    │                                     │
│  │  └─────────────────┘                                     │
│  └─────────────────────┘  └─────────────────────────────────┘
│                                                                 │
│  Deterministic order:                                   │
│  1. Entity Movement & AI                                │
│  2. Belt Item Transport                                 │
│  3. Inserter Operations                                 │
│  4. Assembler Crafting                                  │
│  5. Projectile & Combat                                 │
│  6. Event Handlers (on_tick)                            │
│  7. Player Input Processing                             │
│  8. Rendering Data Collection                           │
│  └─────────────────────────────────────────────────────────┘
│                                                                 │
│  Critical: Ordem fixa mantém sincronização multiplayer     │
│          Qualquer alteração quebra a determinism          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### 1.2 Controle de Precisão de Tempo

| Parâmetro | Valor | Descrição |
|-----------|-------|-----------|
| `game.tick` | uint32 | Contador global, incrementado 1 a cada tick |
| `ticks_per_round` | 60 | Ticks por "round" (1 segundo real) |
| `target_tick_duration` | ~16.67ms | Tempo alvo por tick |
| `max_chunk_distance` | 2 | Chunks processados por tick |

#### 1.3 Determinismo de Simulação

```
Fatores de Determinismo:

│  ✓ Same world seed → Same map generation
│  ✓ Fixed tick order → Deterministic execution
│  ✓ No random in tick loop → Pure determinism
│  ✓ Floating point types → Consistent behavior (f32)
│  ✓ Fixed math operations → Predictable results
│
│  ⚠ Desync Causes:
│  │  - Different mods between clients
│  │  - Different game settings
│  │  - Floating point drift (mitigated by types)
│  │  - Lua state desynchronization
│  │  - Mods modifying tick behavior
│
│  ✓ Prevention:
│  │  - Verify same mods
│  │  - Regular save/load consistency
│  │  - Server-side input validation
│  │  - State reconciliation
```

### 2. Processamento por Fase da Simulação

#### 2.1 Fase 1: Entity Updates (Entity Movement & AI)

| Sub-stage | Descrição | Frequência |
|-----------|-----------|------------|
| Position Update | `entity.position += entity.velocity` | A cada tick |
| AI Behavior | Inimigo decide ação (atacar, retornar) | A cada tick |
| Pollution Generation | Geração e spread de pollution | A cada tick |
| Energy Consumption | Consumo/recharge de energy | A cada tick |
| Health Regeneration | Regeneração natural | A cada tick |

**Entity State Update Formula**:
```
position_new = position_old + velocity * tick_duration
energy_new = energy_old - consumption * tick_duration
health_new = min(health_old + regen_rate * tick_duration, max_health)
pollution_new = pollution_old + generation_rate * tick_duration
```

#### 2.2 Fase 2: Sistema de Logistics (Belt & Inserter)

| Sub-stage | Descrição | Frequência |
|-----------|-----------|------------|
| Belt Movement | Items movem 1 tile na direção da belt | A cada tick |
| Inserter Action | Inserters pegam/colocam items | A cada tick |
| Logistic Network | Requests/fulfillment checked | A cada tick |
| Robot Operations | Robôs de logística operam | A cada tick |

**Belt Item Movement**:
```
item_position_new = item_position_old + belt_direction * belt_speed * tick_duration
```
Onde `belt_speed` = 1 tile/tick para belts normais.

#### 2.3 Fase 3: Produção e Crafting (Assembler Crafting)

| Sub-stage | Descrição | Frequência |
|-----------|-----------|------------|
| Recipe Timers | Assemblers completam crafting | A cada tick |
| Module Effects | Modules aplicam bonificações | A cada tick |
| Energy Consumption | Machines consomem energy | A cada tick |
| Result Inventory | Items inseridos nos inventários | A cada tick |

**Crafter State Update**:
```
crafting_progress += crafting_speed * tick_duration
if crafting_progress >= crafting_time:
    -- Produzir resultados
    -- Consumir ingredientes
    -- Resetar progresso
```

#### 2.4 Fase 4: Física e Combate (Projectile & Combat)

| Sub-stage | Descrição | Frequência |
|-----------|-----------|------------|
| Projectile Movement | Projectiles movem-se em linha reta | A cada tick |
| Collision Detection | Verificar hits com entities | A cada tick |
| Damage Application | Aplicar dano a entities hit | A cada tick |
| Explosion Handling | Explosões resolvidas | A cada tick |

**Projectile Trajectory**:
```
position_new = position_old + direction * speed * tick_duration
distance_traveled += speed * tick_duration
if distance_traveled >= max_range:
    -- Remove projectile
```

#### 2.5 Fase 5: Event Handlers (on_tick)

| Evento | ID | Handler Count | Descrição |
|--------|-----|---------------|-----------|
| `on_tick` | 0 | ~50+ handlers | A cada tick |
| `on_nth_tick(60)` | 60 | ~10 handlers | A cada 1 minuto |
| `on_nth_tick(3600)` | 3600 | ~5 handlers | A cada 1 hora |
| `on_player_tick` | player-specific | Varie | Por player |

#### 2.6 Fase 6: Processamento de Player Input

| Input Type | Descrição | Frequência |
|------------|-----------|------------|
| Movement | WASD, gamepad sticks | A cada tick |
| Shooting | Mouse click, fire button | A cada tick |
| GUI | Inventory toggle, quickbar swap | A cada tick |
| Chat | Text entry, send | Quando player envia |

#### 2.7 Fase 7: Coleta de Dados para Rendering

| Dado Coletado | Fonte | Propósito |
|---------------|-------|-----------|
| Entity positions | Simulation state | Rendering transforms |
| Entity states | Health, energy, etc. | Rendering parameters |
| Tile states | Tile types, animations | World rendering |
| Resource counts | Ore patches, logistics | UI display |
| Player stats | Inventory, research | HUD display |

### 3. Mecanismos de Sincronização Multiplayer

#### 3.1 Arquitetura de Server-Client

```
Multiplayer Architecture:

┌────────────────────────────────────────────────────────────┐
│                    SERVER-SIDE                                  │
│  ██████████  Simulation  ██████████  Authority  ██████████ │
│  ██████████  Input Validation  ██████████  State Sync  █████████ │
│                                                                 │
│                    CLIENT-SIDE                                  │
│  ██████████  Local Simulation  ██████████  State Reconciliation │
│  ██████████  Inputs  ───────►  Server  ───────►  State Update │
│                                                                 │
└────────────────────────────────────────────────────────────┘
```

#### 3.2 Sincronização de Inputs

```
Input Sync Process:

1. Client envia inputs do jogador ao Server a cada tick
   ├── Movement directions (WASD, gamepad sticks)
   ├── Shooting intent (mouse click, fire button)
   ├── GUI actions (inventory toggle, quickbar swap)
   └── Chat messages (quando enviados)

2. Server valida e aplica simulation
   ├── Verify inputs are valid
   ├── Run tick logic deterministically
   ├── Apply confirmed game state
   └── Return state confirmation

3. Server envia estado sincronizado aos clients
   ├── Entity positions and states
   ├── Research progress
   ├── Chat messages
   └── Map state (chunk data)

4. Clients aplicam estado recebido
   ├── Reconcilia com local simulation
   ├── Corrige discrepâncias
   └── Update HUD e rendering
```

#### 3.3 Prevenção de Desync

```
Desync Prevention Measures:

│  ✓ Verify same mods between all clients
│  ✓ Verify same game settings
│  ✓ Regular save/load consistency checks
│  ✓ Input validation on server
│  ✓ State reconciliation on clients
│  ✓ Same world seed guarantees
│  ✓ Deterministic tick execution
│
│ ⚠ Desync Indicators:
│  │  - Entity positions diverge
│  │  │  - Resource counts differ
│  │  - Research progress differs
│  │  - Entity health/states differ
│  │  - Chat messages out of sync
│
│  ✓ Recovery:
│  │  - Reload save game
│  │  - Re-sync via /reconnect
│  │  - Restart multiplayer session
```

### 4. Sistema de Pollution (Poluição)

#### 4.1 Geração de Poluição

| Source | Contribution | Formula |
|--------|-------------|---------|
| Player mining | Pollution per resource mined | Fixed amount per resource |
| Player crafting | Pollution per item crafted | Fixed amount per recipe |
| Enemy kills | Pollution per enemy killed | Fixed amount per kill |
| Entity burning | Pollution per tick burning | Fixed rate per entity |
| Player movement | Pollution per step | Very small amount |

**Pollution Cloud**:
```
Pollution dispersion follows:
- Radial spread from source
- Decay over distance and time
- Dilution with game time
- Interaction with terrain
```

#### 4.2 Fator de Evolução (Evolution Factor)

```
Evolution Factor Calculation:

│  base_evolution = 0.01  (starts at 1%)
│
│  ├─► Pollution contribution
│  │   │
│  │   ├──► Pollution from player actions
│   │   │   ├──► Mining
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
│
│  Range: 0.0 (0%) a 1.0 (100%)
│  Increases over time naturally
│  Can be accelerated by player actions
```

### 4.3 Limpeza de Pollution

| Mecanism | Descrição | Frequência |
|-----------|-----------|------------|
| Natural decay | Pollution dilutes over time | A cada tick |
| Rain effect | Rain reduz pollution | Durante chuva |
| Player cleanup | Player can clean pollution | Manual action |
| Distance dilution | Pollution dilutes com distância | Contínuo |

### 4.4 Impacto no Gameplay

| Nível de Evolução | Comportamento de Inimigos |
|------------------|--------------------------|
| 0.0 - 0.3 | Inimigos fracos, ataques esporádicos |
| 0.3 - 0.6 | Inimigosmoderados, ataques mais frequentes |
| 0.6 - 1.0 | Inimigos fortes, ataques constantes |
| 1.0+ | Inimigos max evolution, ataques intensos |

### 5. Sistema de Pesquisa e Tech Tree

#### 5.1 Pesquisa Structure

```
Research State:

│  current_research = "technology_name"
│  researched Technologies = {"tech1", "tech2", ...}
│  research_queue = {"tech3", "tech4", ...}
│  science_packs_producing = {"science-pack-1", ...}
│  science_packs_required = {{name, count}, ...}
│
│  Tech Tree Structure:
│  │
│  │  ├─── Prerequisites (techs que desbloqueiam outras)
│  │  ├─── Effects (what each tech unlocks)
│  │  ├─── Costs (science packs, time)
│  │  └─── Order (tree structure)
│
│  Tech Progression Example:
│  │
│  │  automation → logistics → military → ...
│  │  │          │
│  │  │          └── military-2 → military-3
│  │
│  └──► Military techs unlock weapons, armors, enemy AI upgrades
```

#### 5.2 Custo de Pesquisa

| Tipo | Custo | Formula |
|------|-------|---------|
| `science_packs` | Count por pack type | Configurado por tech |
| `time` | Duration em segundos | Configurado por tech |
| `research_queue_size` | Max pending researches | Configurado por configurações |

**Research Completion**:
```
when science_packs_producing matches science_packs_required:
    │
    ├─── Technology is "researched"
    │   ├─── Add to researched Technologies list
    │   ├─── Remove from research_queue
    │   ├─── Apply technology effects
    │   │   ├─── Unlock recipes
    │   │   ├─── Unlock entities
    │   │   ├─── Modify game parameters
    │   │   └─── etc.
    │   └──► Trigger on_research_picked events
```

### 5.3 Árvore de Tecnologias (Tech Tree)

```
Tech Tree Overview:

├── Automation (Tier 1)
│   ├── Logistics (Tier 1)
│   │   ├── Warehousing
│   │   ├── Basic buffs
│   │   └── Basic efficiency
│   ├── Military (Tier 1)
│   │   ├── Small arms
│   │   ├── Armor
│   │   └── Weapon upgrades
│   └── Economy (Tier 1)
│       ├── Oil processing
│       ├── Energy production
│       └── Basic modules
│
├── Military (Tier 2+)
│   ├── Advanced weapons
│   ├── Better armor
│   ├── Module upgrades
│   └── Enemy AI upgrades
│
├── Production (Tier 2+)
│   ├── Module tiers (M1-M3)
│   ├── Efficiency modules
│   ├── Productivity modules
│   └── Speed modules
│
└── Late game (Tier 3+)
    ├── Space science
    ├── Quantum tech
    ├── Prod-3 modules
    └── Krastorio/other mods
```

### 6. Sistema de Save/Load (Simplified)

#### 6.1 Save File Structure

```
Save File Sections:

├─ players: State de cada player
├─ forces: State de cada force
├─ surfaces: State de cada surface/map
├─ research: Technologies progress
├─ items: Item counts
├─ entities: Entity positions/states
├─ research_queue: Pending research
├─ global: Global state table
├─ schedule: Train schedules
└─ version: Format version
```

#### 6.2 Save Format Versioning

| Version | Period | Notes |
|---------|--------|-------|
| 1-4 | Old formats (0.15 - 0.17) | Not compatible with 2.1.x |
| 5 | Current (0.17 - 1.1) | Partial compatibility |
| 6+ | Future formats | Forward compat only |

**Migration via `on_configuration_changed`**:
```lua
freeplay.on_configuration_changed = function()
  -- Initialize storage tables
  storage.created_items = storage.created_items or created_items()
  storage.respawn_items = storage.respawn_items or respawn_items()
  -- ... etc
  
  -- Check if init ran (for old saves)
  storage.init_ran = storage.init_ran or #game.players > 0
  
  -- Re-initialize ending info
  init_ending_info()
end
```

### 7. Otimizações de Performance

#### 7.1 Chunk-Based Processing

```
Chunk Processing:

│  Map divided into chunks 32×32 tiles
│  Only loaded chunks process per tick
│  Unloaded chunks suspended (paused)
│  Chunk boundaries don't affect determinism
│  State is independent per chunk
│
│  Chunk State Update:
│  │
│  │  1. Collect chunk entity states
│  │  2. Process belt movement
│  │  3. Process inserter operations
│  │  4. Process crafting
│  │  5. Process pollution
│  │  6. Collect rendering data
│  │  7. Save chunk state
│  │
│  │  Performance impact:
│  │  │  - Linear with chunk count
│  │  │  - More chunks = more CPU
│  │  │  - Optimal: render distance balance
```

#### 7.2 Entity Count Limits

```
Entity Processing Limits:

│  Target: Process all entities per tick
│  Limit: ~10,000 entities/tick (configurable)
│  Beyond limit: Some entities skipped with warning
│
│  Performance Tiers:
│  │  - Small base: ~200-500 entities
│  │  - Medium base: ~1,000-2,000 entities
│  │  - Large base: ~3,000-5,000 entities
│  │  - Very large: ~5,000+ entities
│  │  - Lag starts: ~10,000+ entities
│
│  Mitigation:
│  │  - Increase render distance? No, opposite
│  │  - Use construction robots vs belts
│  │  - Blueprints for blueprint libraries
│  │  - Performance mods
│  │  - /prove performance test command
```

### 8. Diagrama de Fluxo de Simulação

```
graph TD
    A[Tick Start: game.tick++] --> B{Phase 1: Entity Updates}
    B --> C[Phase 2: Logistics Processing]
    C --> D[Phase 3: Production/Crafting]
    D --> E[Phase 4: Projectile & Combat]
    E --> F[Phase 5: Event Handlers (on_tick)]
    F --> G[Phase 6: Player Input Processing]
    G --> H[Phase 7: Rendering Data Collection]
    H --> I[Tick End: State Save]
    I --> A
    
    style A fill:#f9f,stroke:#333,stroke-width:2px
    style I fill:#bbf,stroke:#333,stroke-width:2px
```

## Observações Técnicas

1. **Determinismo Absoluto**: A simulação é completamente determinística - dado o mesmo estado inicial e inputs, o resultado é sempre idêntico. Critical para multiplayer sync.

2. **Tick 60Hz**: O timing é fixo a 60 ticks/segundo, independente do FPS - o FPS afeta apenas a fluidez visual.

3. **Processamento por Chunks**: Apenas chunks carregados são processados por tick. Isso permite jogos massivos com performance adequada.

4. **Limites de Entities**: O jogo tem limites de entities processados por tick para manter performance. Se excedido, alguns entities são pulados com warning.

5. **Pollution System**: O system de poluição é global e afeta comportamento de inimigos através do evolution factor.

6. **Tech Tree Progressão**: A árvore de tecnologias é pré-definida com prerequisites, custos e efeitos claros.

7. **Save/Load Consistência**: O estado do tick é serializado e pode ser carregado posteriormente. O estado deve ser idêntico ao reload.

8. **Mod Compatibility**: Mods podem modificar behaviour através de `on_tick` handlers, `on_research_picked` events, e prototype overriding.

## Próximos Relatórios Planejados

- `PHYSICS_ANALYSIS.md` - Física e colisões avançado
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