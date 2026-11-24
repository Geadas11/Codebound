# LOGISTICS ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Foco**: Sistema de logística - transportadores, inserters, rede logistic

## Visão Geral do Sistema de Logistics

O sistema de logísticas do Factorio 2.1.24 é um dos aspectos mais profundos e estratégicos do jogo, gerenciando o movimento de itens através de transportadores (belts), inserters e robôs de logística. Este relatório documenta a arquitetura interna, mecanismos e fluxo de dados.

### 1. Arquitetura do Sistema de Logistics

#### 1.1 Três Sistemas de Transportes

O Factorio implementa três sistemas de logística distintos que podem ser usados individualmente ou em combinação:

| Sistema | Tipo | Mecanism | Velocidade | Capacidade |
|---------|------|----------|------------|------------|
| **Transportadores (Belts)** | Item movement | Físico, 1 tile/tick | 1 tile/tick (~60 items/s) | Ilimitada (até memory) |
| **Inserters** | Item pickup/dropoff | Local, braço mecânico | Variedade (speed stats) | Limited by reach |
| **Robôs de Logística** | Drone/flight | Aérea, energy-dependent | Variedade (speed stats) | Limited by roboport network |

#### 1.2 Arquitetura de Integração

```
┌─────────────────────────────────────────────────────────────┐
│                   LOGISTICS SYSTEM OVERVIEW                  │
├─────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │   BELTS         │  │  INSERTERS      │  │  ROBOTICS       │ │
│  │   (Belts)       │  │   (Inserters)   │  │   (Robots)      │ │
│  └───────▲─────────┘  └───────▲─────────┘  └───────▲─────────┘ │
│          │                    │                    │           │
│  ┌───────▼─────────┐  ┌───────▼─────────┐  ┌───────▼─────────┐ │
│  │ Item Transport  │  │ Item Pickup/Dropoff │ Robot Flight │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
│                                                                 │
│  Todos os sistemas operam no tick 60Hz do jogo              │
│  Podem ser usados individualmente ou em combinação          │
│                                                                 │
└─────────────────────────────────────────────────────────────┘
```

### 2. Sistema de Transportadores (Belt System)

#### 2.1 Funcionamento do Belt

| Propriedade | Valor | Descrição |
|-------------|-------|-----------|
| `belt_speed` | 1 tile/tick | Velocidade padrão |
| `belt_capacity` | ~8 items/tick | Capacidade por lane |
| `item_stack_size` | Variedade | Definido por item prototype |
| `direction` | 8-way | NESW + intercardinais |
| `collision_box` | Small | Para colisão detection |

**Belt Movement Algorithm**:
```
1. Item position no início do tick:
   │  item.x, item.y (em tiles mundo)
   
2. Direction calculation:
   │  dx, dy = belt_direction_vector
   
3. Position update:
   │  item.x_new = item.x_old + dx * belt_speed * tick_duration
   │  item.y_new = item.y_old + dy * belt_speed * tick_duration
   
4. Belt interaction:
   │  - Items movem-se na direção da belt
   │  - Items podem ser inseridas/removidas por inserters
   │  - Items desaparecem se caírem do mapa
   │  - Splitters redirecionam flow
```

#### 2.2 Tipos de Belts

| Type | Speed | Capacity | Sprite | Unlock Tech |
|------|-------|----------|--------|-------------|
| `transport-belt` | 1 tile/tick | ~8 items/tick | Base game | Starting tech |
| `fast-transport-belt` | 2 tiles/tick | ~16 items/tick | Faster | Logistic research |
| `express-transport-belt` | 3 tiles/tick | ~24 items/tick | Fastest | Advanced logistics |

**Belt Graphics**:
```
Sprite Sheet Structure:
├── transport-belt.png           (base belt)
├── fast-transport-belt.png      (fast variant)
└── express-transport-belt.png   (express variant)

Frame animation:
├── idle frames (stationary)
├── running frames (items moving)
└── direction frames (N, NE, E, SE, S, SW, W, NW)
```

#### 2.3 Splitters

| Propriedade | Valor | Descrição |
|-------------|-------|-----------|
| `splitter_speed` | 1 item/tick | Throughput |
| `splitter_direction` | 4-way | Input/output directions |
| `item_interaction` | Can input/output | Define interação |

**Splitter Logic**:
```
1. Input side recebe items da belt
2. Output sides distribuem baseado no ratio
3. Priority handling para overflow
4. Item tracking para consistency
```

### 3. Sistema de Inserters

#### 3.1 Tipos de Inserters

| Type | Speed | Reach | Energy Consumption | Module Slots |
|------|-------|-------|-------------------|--------------|
| `inserter` | Normal | 5.5 tiles | 15kW/tick | 0 |
| `long-handed-inserter` | Normal | 7.5 tiles | 15kW/tick | 0 |
| `filtered-inserter` | Normal | 5.5 tiles | 15kW/tick | 0 |
| `sticky-inserter` | Normal | 5.5 tiles | 15kW/tick | 0 |
| `fast-inserter` | Fast | 5.5 tiles | 25kW/tick | 0 |
| `long-handed-fast-inserter` | Fast | 7.5 tiles | 25kW/tick | 0 |
| `mega-inserter` | Mega | 9.5 tiles | 50kW/tick | 0 |

#### 3.2 Mecanism de Pickup/Dropoff

```
Inserter Pickup Algorithm:

1. Check reach:
   │   ├─── distance(entity, target_item) <= inserter_reach
   │   └─── └─ if yes, proceed; else skip
   
2. Check inventory space:
   │   ├─── has_empty_slot(target_inventory, item_type)
   │   └─── └─ if yes, proceed; else skip
   
3. Calculate pickup position:
   │   ├─── entity.position + inserter.offset
   │   └─── └─ offset defined por prototype
   
4. Execute pickup:
   │   ├─── Remove item stack da belt/entity
   │   ├─── Add item stack para inserter's buffer
   │   └─── Consumir energy (15kW per operation)
```

**Inserter Dropoff Algorithm**:
```
1. Check target reach:
   │   ├─── distance(entity, target_position) <= inserter_reach
   │   └─── └─ if yes, proceed; else skip
   
2. Check inventory space em target:
   │   ├─── target_inventory.has_empty_slot()
   │   └─── └─ if yes, proceed; else skip (item drops na belt)
   
3. Calculate drop position:
   │   ├─── target_position + inserter.offset
   │   └─── └─ offset defined por prototype
   
4. Execute dropoff:
   │   ├─── Remove item stack da buffer do inserter
   │   ├─── Add item stack para target_inventory
   │   └─── Consumir energy (15kW per operation)
```

#### 3.3 Inserter Energy Consumption

```
Energy per operation formula:
│  energy = base_energy * (1 + module_effects)
│
│  Where:
│  │  base_energy = 15kW (inserter normal)
│  │  module_effects = sum(module bonuses)
│  │
│  Example: fast-inserter
│  │  base_energy = 25kW
│  │  modules can modify speed/energy consumption
```

### 4. Robôs de Logística (Logistic Robots)

#### 4.1 Tipos de Robôs

| Type | Speed | Carrying Capacity | Energy Consumption | Roboport Requirement |
|------|-------|------------------|-------------------|---------------------|
| `logistic-drone` | Slow | 1× item stack | 1kW/tick | Small roboport |
| `fast-logistic-drone` | Fast | 2× item stack | 5kW/tick | Medium roboport |
| `flying-robot-frame` | - | Variable | - | Nenhum (equipment) |
| `fast-flying-robot-frame` | Fast | Variable | Fast | Medium roboport (equipment) |
| `mega-flying-robot-frame` | Mega | Variable | Mega | Large roboport (equipment) |

#### 4.2 Roboport Network

```
Roboport Network Architecture:

│  Small Roboport
│  │  └─ Range: ~30 tiles
│  │  └─ Power: 300 kW
│  │  └─ Robot limit: 20 robots
│  │
│  Medium Roboport
│  │  └─ Range: ~50 tiles
│  │  └─ Power: 500 kW
│  │  └─ Robot limit: 50 robots
│  │
│  Large Roboport
│  │  └─ Range: ~80 tiles
│  │  └─ Power: 800 kW
│  │  └─ Robot limit: 100 robots
│  │
│  Network Coverage:
│  │  ├─ Overlapping ranges combinam
│  │  ├─ Robot count limits se aplicam individually
│  │  ├─ Energy coverage se estende por toda a rede
│  │  └─ Logistics requests se estendem por toda a rede
```

#### 4.3 Robot Flight Logic

```
Drone Flight Algorithm:

1. Takeoff:
   │   ├─── Check roboport coverage
   │   ├─── Check energy > minimum flight cost
   │   └─── └─ if yes, takeoff; else wait
   
2. Navigation:
   │   ├─── Fly directly para destination
   │   ├─── Avoid entities/players/other robots
   │   ├─── Follow waypoint path se necessário
   │   └─── Maintain altitude (y-level)
   
3. Pickup/Dropoff:
   │   ├─── Hover sobre target entity/chest
   │   ├─── Transfer item stack
   │   ├─── Consumir energy por operação
   │   └─── Return para roboport para recharging
   
4. Return/Recharge:
   │   ├─── Fly de volta ao roboport mais próximo
   │   ├─── Land no landing pad
   │   ├─── Recharge energy
   │   └─── Release para next mission
```

#### 4.4 Energy Consumption por Operação

```
Energy per operation:

│  drone_energy = base_energy + (distance * energy_per_tile)
│
│  Where:
│  │  base_energy = 1kW (logistic-drone)
│  │  energy_per_tile ≈ 0.01kW por tile
│  │  distance = distancia voo em tiles
│
│  Example: 50-tile trip
│  │  energy = 1kW + (50 * 0.01kW) = 1.5kW
│
│  Recharge rate at roboport:
│  │  Small: 300kW (total), ~15kW por drone simultâneo
│  │  Medium: 500kW
│  │  Large: 800kW
```

### 5. Rede Logistic vs. Network Basic

#### 5.1 Logistic Network (Com Robots/Belts/Inserters)

```
Logistic Network Features:

│  ✓ Request/provider chests definition
│  ✓ Item request thresholds
│  ✓ Provider chest buffer storage
│  ✓ Logistic cell network coverage
│  ✓ Robot logistics operations
│  ✓ Belt integration (item movement)
│  ✓ Inserter requests filters
│  ✓ Network-wide item tracking
│  ✓ Global request prioritization
│
│  Data Structure:
│  │
│  │  global.logistic_networks = {
│  │  │  network_id: {
│  │  │  │  providers: {{item, count}, ...},
│  │  │  │  requesters: {{item, count}, ...},
│  │  │  │  connections: {chunk_positions...},
│  │  │  │  request_thresholds: {{item, count}, ...},
│  │  │  │  withdrawal_distance: number
│  │  │  }
│  │  │
│  │  }
│
│  Network Coverage Expansion:
│  │  │  │  - New roboports expand range
│  │  │  │  - New requester providers expand coverage
│  │  │  │  - Belt coverage extends network logically
│  │  │  │  - Robot flight range determines physical coverage
│  │  │  │
```

#### 5.2 Network Basic (Sem Robots)

```
Basic Network Features (Sem Robots):

│  ✓ Requester chests definition
│  ✓ Provider chests definition
│  ✓ Item requests thresholds
│  ✓ Limited to chest proximity
│  ✓ No robot operations
│  ✓ Belt integration still works
│  ✓ Simpler data structure
│
│  Limitations:
│  │  │  - No long-distance coverage
│  │  │  - Limited a chest vicinity
│  │  │  - Manual logistics management required
│  │  │  - No network-wide item tracking
│  │  │
│  Use Cases:
│  │  │  │  - Early game base design
│  │  │  │  - Compact bases
│  │  │  │  - Specific subsystem design
│  │  │  │  - Modded gameplay constraints
```

### 6. Integração entre Sistemas

#### 6.1 Belt + Inserter Integration

```
Belt-Inserter Interaction:

1. Item na belt → Inserter pickup
   │
   ├─── distance(entity, item) <= inserter_reach
   │   └─ if yes, inserter picks up item
   │      └─ Item removido da belt
   │      └─ Item adicionado ao buffer do inserter
   └─ if no, item continua na belt

2. Item da belt → Inserter dropoff
   │
   ├─── distance(entity, target) <= inserter_reach
   │   └─ if yes, inserter tries dropoff
   │      ├─── target_inventory.has_empty_slot()
   │      │    └─ if yes, item inserido
   │      │    └─ if no, item cai na belt (drop)
   └─ if no, inserter não atinge target

3. Belt belt interaction (com splitters)
   │
   ├─── Items mantêm velocidade 1 tile/tick
   │   ├─── Splitters redirecionam flow
   │   │   ├─── Baseado em ratio de outputs
   │   │   └─── Priority handling
   │   └─── Items podem se acumular (mas não bloqueiam totalmente)
   └─ Items desaparecem se caírem do mapa após X ticks
```

#### 6.2 Belt + Robot Integration

```
Belt-Robot Interaction:

1. Robot pickup da belt
   │
   ├─── Robot hover sobre item na belt
   │   ├─── Check roboport coverage
   │   ├─── Check energy > cost
   │   └─── if yes, robot picks up item
   │      └─ Item removido da belt
   │      └─ Item adicionado ao cargo hold do robot
   
2. Robot dropoff na belt
   │
   ├─── Robot hover sobre target location na belt
   │   ├─── Check roboport coverage
   │   ├─── Check cargo hold tem item desejado
   │   └─── if yes, robot drops itemonto belt
   │      └─ Item adicionado à belt na posição de landing
   └─ if no, robot retorna ao roboport

3. Priority handling:
   │
   ├─── Robots têm prioridade sobre belts para item pickup
   │   └─ Se robot e inserter querem mesmo item, robot vence
   │
   └─ Belt mantém flow independente de robot operations
```

#### 6.3 Robot + Inserter Integration

```
Robot-Inserter Interaction:

1. Robot entrega item para inserter
   │
   ├─── Robot hover perto de inserter
   │   ├─── Check roboport coverage
   │   ├─── Check cargo hold tem item
   │   └─── if yes, inserter pega item do robot
   │      └─ Robot energy consumption modificado
   └─ if no, robot continua sua missão
   
2. Inserter entrega item para robot
   │
   ├─── Inserter pega item da belt/entity
   │   ├─── Inserter buffer tem space
   │   └─── if yes, item adicionado ao buffer
   │
   ├─── Robot pega item do buffer do inserter
   │   ├─── Robot cargo hold tem space
   │   └─── if yes, item movido para robot
   └─ Chain operations seamless
```

### 7. Otimizações de Logistics

#### 7.1 Performance Otimizações

| Otimização | Descrição | Impacto |
|------------|-----------|---------|
| **Chunk processing** | Apenas chunks carregados processam | Linear com chunk count |
| **Entity limits** | Limite de entities por tick | Evita lag extremo |
| **Belt optimization** | Items movem 1 tile/tick fixo | Predictable performance |
| **Robot sparsification** | Apenas robots próximos de roboports processam | Reduces CPU load |
| **Network caching** | Logistic network state cached | Reduz recálculos |

#### 7.2 Design Patterns

```
Efficient Logistics Design:

│  ✓ Use belts para main bus / long-distance transport
│  ✓ Use inserters para local movimentação entre machines
│  ✓ Use robots para reach distante / áreas remotas
│  ✓ Use requester/provider chests para auto-logistics
│  ✓ Use buffer chests para item storage management
│  ✓ Use splitters para direction control
│  ✓ Use roboports com coverage adequada
│  ✓ Mantenha balanced throughput entre todos os sistemas
│
│ ⚠ Design Anti-Patterns:
│  │  │  │  - Belt backup causando item accumulation
│  │  │  │  - Robot range insuficiente causando retornos frequentes
│  │  │  │  - Inserter reach limitado causando bottlenecks
│  │  │  │  - Network overlap causando contador conflicts
│  │  │  │
│  │  ✓ Best practices:
│  │  │  │  - Mantenha belt lanes separadas por tipo de item
│  │  │  │  - Use express belts para long-distance main bus
│  │  │  │  - Deploy roboports a 40-60 tile intervals
│ │  │  │  - Use requester/provider chests para auto-logistics
│ │  │  │  - Mantenha balanced inserter counts por production line
```

### 7.3 Quality of Life Features

```
QoL Features:

│  ✓ Show network info (F3) - Visualizar rede logistics
│  ✓ Request item counts - Contar requests por item
│  ✓ Provider counts - Contar providers por item
│  ✓ Network coverage - Visualizar range de roboports
│  ✓ Belt direction arrows - Ver direction of item flow
│  ✓ Filter debugging - Debug item filter settings
│  ✓ Energy display - Mostrar energy status de robôs
│
│  Debug Commands:
│  │  /lstg - List logistic network groups
│  │  /lrh - List request and provide counts por item
│  │  /lrt - List request table
│  │  /plt - List provider table
```

## Observações Técnicas

1. **Três Sistemas Distintos**: Belts, inserters e robots fornecem diferentes trade-offs de velocidade, distância e complexidade.

2. **Determinismo**: O sistema de logística é totalmente determinístico - dado o mesmo estado inicial e inputs, o resultado é sempre idêntico.

3. **Chunk-Based Processing**: Apenas chunks carregados processam operations de logística por tick.

4. **Energy Dependency**: Robôs requerem energy constante de roboports; belts e inserters requerem energy menor/opcional.

5. **Mod Compatibility**: Múltiplos mods podem estender/logistics system através de prototype overriding e event handlers.

6. **Save/Load Consistência**: O estado da logística é serializado e pode ser carregado posteriormente. O estado deve ser idêntico ao reload.

7. **Performance Scaling**: O system escala bem de bases pequenas (~200 entities) a bases gigantes (~10,000+ entities) com otimizações apropriadas.

## Próximos Relatórios Planejados

- `NETWORKING_ANALYSIS.md` - Multiplayer e networking
- `SAVE_SYSTEM.md` - Sistema de saves
- `DEPENDENCIES.md` - Dependências e bibliotecas
- `FUNCTION_INVENTORY.md` - Inventário de funções
- `ALGORITHMS.md` - Algoritmos identificados
- `ARCHITECTURE_DIAGRAMS.md` - Diagramas arquiteturais
- `REA_FINDINGS.md` - Descobertas REA
- `UNKNOWN_COMPONENTS.md` - Componentes desconhecidos
- `RECONSTRUCTION_ROADMAP.md` - Roadmap para reconstrução futura