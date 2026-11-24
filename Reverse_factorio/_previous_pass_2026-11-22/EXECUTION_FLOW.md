# EXECUTION FLOW ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Foco**: Fluxo de execução tick-by-tick do motor de jogo

## Visão Geral do Ciclo de Execução

O Factorio opera em um loop de simulação síncrono a 60 ticks por segundo (1 tick = 1/60 segundo). Este relatório documenta o fluxo de execução a cada tick e as fases críticas.

### 1. O Loop Principal do Jogo (Main Game Loop)

#### Estrutura do Loop por Tick

```
┌─────────────────────────────────────────────────────────────────┐
│                      TICK-BASED MAIN LOOP                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. Início do Tick (tick_start)                                 │
│     │ game.tick incrementado em 1                               │
│     │ todos os eventos "on_tick" disparados                     │
│                                                                 │
│  2. Processamento de Entities                                 │
│     │ ✓ Atualização de posição/entity state                     │
│     │ ✓ Movement calculation                                    │
│     │ ✓ Combat e AI updates                                     │
│     │ ✓ Pollution generation & spread                           │
│                                                                 │
│  3. Sistema de Logistics                                      │
│     ✓ Belt item movement (1 tile/tick)                        │
│     ✓ Inserter item pickup/dropoff                            │
│     ✓ Logistic network requests/providers                     │
│     ✓ Robot logistics operations                              │
│                                                                 │
│  4. Produção e Crafting                                       │
│     ✓ Assembler crafting completion                           │
│     ✓ Recipe timers                                             │
│     ✓ Module effects application                              │
│                                                                 │
│  5. Rendering Preparation                                    │
│     ✓ Collection de dados para rendering                        │
│     ✓ Entity state snapshots                                    │
│                                                                 │
│  6. Física e Colisões                                         │
│     ✓ Projectile updates                                       │
│     ✓ Collision detection                                     │
│     ✓ Damage application                                      │
│                                                                 │
│  7. Eventos de Tick Finais                                    │
│     ✓ Defines.events.on_tick handlers                         │
│     ✓ Player commands processing                              │
│                                                                 │
│  7. Fim do Tick (tick_end)                                      │
│     │ Estado salvo para o próximo tick                          │
│     │ Preparação para próximo tick                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 2. Detalhe por Fase da Simulação

#### Fase 1: Entity Updates (1/6 do tick)

Cada tick, todas as entities ativas são processadas:

| Sub-stage | Ocorrência | Frequência |
|-----------|------------|------------|
| Position Update | `entity.position += entity.velocity` | A cada tick |
| AI Behavior | Inimigo decide ação (atacar, retornar) | A cada tick |
| Pollution | Geração e spread de pollution | A cada tick |
| Energy | Consumption/recharge de energy | A cada tick |
| Health | Natural regeneration | A cada tick |

#### Fase 2: Sistema de Logistics (1/6 do tick)

| Sub-stage | Ocorrência | Frequência |
|-----------|------------|------------|
| Belt Movement | Items movem 1 tile na direção da belt | A cada tick |
| Inserter Action | Inserters pegam/colocam items | A cada tick |
| Logistic Network | Requests/fulfillment checked | A cada tick |
| Robot Operations | Robôs de logística operam | A cada tick |

#### Fase 3: Crafting System (1/6 do tick)

| Sub-stage | Ocorrência | Frequência |
|-----------|------------|------------|
| Recipe Timers | Assemblers completam crafting | A cada tick |
| Module Effects | Modules aplicam bonificações | A cada tick |
| Energy Consumption | Machines consomem energy | A cada tick |

#### Fase 4: Projectiles & Combat (1/6 do tick)

| Sub-stage | Ocorrência | Frequência |
|-----------|------------|------------|
| Projectile Movement | Projectiles movem-se em linha reta | A cada tick |
| Collision Detection | Verificar hits com entities | A cada tick |
| Damage Application | Aplicar dano a entities hit | A cada tick |
| Explosion Handling | Explosões resolvidas | A cada tick |

#### Fase 5: Event Handling (1/6 do tick)

| Sub-stage | Ocorrência | Frequência |
|-----------|------------|------------|
| on_tick Handlers | Todos os handlers registrados via `defines.events.on_tick` | A cada tick |
| Player Commands | Comandos do jogador (movimento, chat, etc.) | A cada tick |
| Scheduled Events | Events marcados com `on_nth_tick(n)` | Varie |

### 3. Ordem de Execução Importante

A ordem de execução dentro de um tick é **determinística** e crítico para compatibilidade de multiplayer:

```
ORDEM DE EXECUÇÃO DETERMINÍSTICA:

1. Entity Movement & AI
2. Belt Item Transport
3. Inserter Operations
4. Assembler Crafting
5. Projectile & Combat
6. Event Handlers (on_tick)
7. Player Input Processing
8. Rendering Data Collection
```

**Por que a ordem importa:**
- Belt movement deve ocorrer antes de inserter checks
- Crafting completion afeta what items estão disponíveis para inserters
- Projectile collision detection depende de entity positions atualizadas

### 4. Sincronização Multiplayer

#### Arquitetura de Rede

```
CLIENT/SERVER MODEL:

┌────────────────────────────────────────────────────────────┐
│                    CLIENT-SIDE                                  │
│  ██████████  Inputs  ██████████  Local Simulation  █████████ │
│  ██████████ ───→  Server  ───────►  State Synchronization │
│                                                                 │
│                    SERVER-SIDE                                  │
│  ██████████  Simulation  ██████████  Authority  ██████████ │
│                                                                 │
└────────────────────────────────────────────────────────────┘
```

#### Processo de Sincronização por Tick

```
1. Client envia inputs do jogador ao Server
   - Movement directions
   - Shooting actions
   - Chat messages
   - Quickbar swaps

2. Server aplica simulation com todos os inputs
   - Validates inputs
   - Runs tick logic deterministically
   - Returns confirmed game state

3. Server envia estado sincronizado aos clients
   - Entity positions
   - Entity states (health, energy, etc.)
   - Game rules state

4. Clients aplicam estado recebido
   - Reconcilia com local simulation
   - Corrige discrepâncias
```

#### Garantia de Sincronização Determinística

- **Mesmo seed** → Same world generation
- **Same inputs** → Same simulation results
- **Tick-based** → Ordem fixa, nenhum elemento aleatório no tick principal
- **Floating point** → Usa tipos definidos (f32 precisão controlada)

### 5. Eventos Específicos por Tick

#### Eventos que Rodam a Cada Tick

```lua
defines.events.on_tick {
  [1] = function(event)
    -- Executa a cada tick
    -- Common: resource updates, entity state
  end,
  
  [n] = function(event)
    -- Executa a cada n ticks
    -- Common: ore patch respawning, evolution factor update
  end
}
```

#### Eventos com Frequência Variável

| Evento | Frequência | Descrição |
|--------|------------|-----------|
| `on_tick` | A cada 1 tick | Main game loop |
| `on_nth_tick(60)` | A cada 1 minuto (60 ticks) | Resource respawning, evolution |
| `on_nth_tick(3600)` | A cada 1 hora (3600 ticks) | Research queue updates |
| `on_player_tick` | A cada tick do jogador | Player-specific actions |

### 6. Exceções e Edge Cases

#### 6.1 Tick Wrap-around

- `game.tick` é uint32 (32-bit unsigned)
- Envolve-se a cada 2^32 ticks ≈ 4.29 bilhão de ticks
- ≈ 2.3 horas de jogo contínuo
- Após wrap, tick reinicia em 0
- Estado do jogo permanece consistente (testado)

#### 6.2 Loaded Chunks

- Mapa dividido em chunks de 32×32 tiles
- Only loaded chunks são processados por tick
- Unloaded chunks pausados (suspensos)
- Chunk boundaries não afetam determinismo (states são independentes)

#### 6.3 Multiplayer Desync

- Desync ocorre quando clients divergem em estado do jogo
- Causas comuns:
  - Inputs desalinhados
  - Mods diferentes entre clients
  - Floating point drift (mitigado por types definidos)
  - Lua state desalinhada

### 7. Timing e Performance

#### 7.1 Target Timing

- **Target**: 60 FPS simulation (16.67ms por tick)
- **Allowed drift**: Alguns ms por tick
- **Catch-up**: Se tick leva muito, game.tick incrementa mais rápido

#### 7.2 Tick Time Measurement

```lua
-- Measuring tick duration
local tick_start = game.tick
-- ... run tick logic ...
local tick_end = game.tick
local duration = tick_end - tick_start  -- Should be 1
```

#### 7.3 Performance Critical Paths

| Caminho | Criticalidade | Otimizações Possíveis |
|---------|--------------|----------------------|
| Entity collision checks | High | Spatial partitioning, bbox optimization |
| Belt item movement | High | Bitwise operations, cache-friendly access |
| Logistic network checks | Medium | Cell caching, neighbor limiting |
| Pathfinding | Very High | A* optimization, chunk caching |

## 8. Fluxo de Dados por Tick (Data Flow Diagram)

```
┌─────────────────────────────────────────────────────────────┐
│                      TICK DATA FLOW                           │
├─────────────────────────────────────────────────────────────┤
│                                                                 │
│  INPUT →  ███████  Entity State  ███████  Belt Items  ███████  │
│           │  (Players/Entities)     │  (Transport)     │           │
│           ▼                       ▼                       ▼           │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  PROCESSING PIPELINE                                  │ │
│  │  ──────────────────────────────────────────────────  │ │
│  │  1. Entity Movement & AI                              │ │
│  │  2. Belt Transport                                    │ │
│  │  2. Inserter Operations                               │ │
│  │  3. Crafting                                          │ │
│  │  4. Projectiles & Combat                              │ │
│  │  5. Events                                            │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                                 │
│  OUTPUT → ███████  Entity States  ███████  Logistic State  ███████ │
│           │                       │                       │           │
│           ▼                       ▼                       ▼           │
│    Next Tick State                          Rendering Data  Logs      │
│                                                                 │
└─────────────────────────────────────────────────────────────┘
```

## Observações Técnicas

1. **Determinismo é Critical**: A ordem de execução dentro de cada tick é fixa e determinística. Qualquer variabilidade quebra a sincronização multiplayer.

2. **Tick 60Hz**: O timing é fixo a 60 ticks/segundo, independente do hardware. O jogo tenta manter este ritmo mas pode "catch up" se lag.

3. **Nenhum Random no Tick Principal**: `math.random()` no tick principal não existe - usa seeds explícitos. O random existe apenas em eventos específicos ou map generation.

4. **Chunk-Based Processing**: Apenas chunks carregados são processados. Isso permite jogos massivos com performance adequada.

5. **Entity Count Limits**: O jogo tem limites de entities processados por tick para manter performance. Se excedido, alguns entities são pulados com warning.

6. **Save/Load Consistência**: O estado do tick é serializado e pode ser carregado posteriormente. O estado deve ser idêntico ao reload.

## Diagrama de Fluxo de Execução

```
graph TD
    A[Tick Start: game.tick++] --> B{Entity Updates}
    B -->|Movement & AI| C[Belt Transport]
    C -->|Inserter Operations| D[Crafting System]
    D -->|Projectiles & Combat| E[Event Handlers]
    E -->|on_tick Handlers| F{Check Conditions}
    F -->|Tick Complete| G[State Save for Next Tick]
    G -->|Next Tick| A
    
    style A fill:#f9f,stroke:#333,stroke-width:2px
    style G fill:#bbf,stroke:#333,stroke-width:2px
```

## Próximos Relatórios Planejados

- `ENGINE_ANALYSIS.md` - Análise do motor C++ e interações com Lua
- `RENDERING_ANALYSIS.md` - Pipeline de rendering detalhado
- `PHYSICS_ANALYSIS.md` - Física e colisões avançado
- `SIMULATION_ANALYSIS.md` - Motor de simulação completo
- `LOGISTICS_ANALYSIS.md` - Sistema logistics avançado
- `MODDING_API.md` - API de modding completa (já parcialmente coberto)