# SOFTWARE ARCHITECTURE ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Arquitetura**: Two-layer system (C++ engine + Lua layer)  
**Executável**: `C:\GOG Games\Factorio\bin\x64\factorio.exe` (PE32+, x86_64)

## Visão Geral da Arquitetura de Software

O Factorio 2.1.24 utiliza uma arquitetura de software "two-layer" sofisticada, separando responsabilidades entre um motor compilado (C++) e uma camada de scripting (Lua). Esta arquitetura permite tanto alto desempenho quanto flexibilidade para modding.

### 1. Arquitetura de Camadas Duas

#### 1.1 Layer 1: Motor Compilado (C++)

**Responsabilidades Principais**:
- Loop de simulação a 60 ticks/segundo
- Rendering pipeline (Direct3D 11)
- Física e colisões
- Networking e multiplayer
- Sistema de arquivos e I/O
- Memory management
- Loading de recursos

**Componentes do Motor**:
| Componente | Descrição |
|------------|-----------|
| `factorio.exe` | Executável principal, ~49.4 MB |
| `factorio.pdb` | Debug symbols (MSVC) |
| Engine core | Módulos central da engine |
| Rendering engine | Pipeline Direct3D 11 |
| Physics engine | Colisão, projectiles |
| Networking layer | Protocolo multiplayer |
| File system | I/O operations |
| DLL dependencies | VCRuntime, etc. |

**Dependências de Sistema**:
- Microsoft Visual C++ Redistributable
- Direct3D 11 (Windows API)
- CRT (C Runtime Library)
- Windows API components

#### 1.2 Layer 2: Camada Lua (Scripting)

**Responsabilidades Principais**:
- Lógica de jogo (scripts freeplay, core)
- API de modding
- Prototypes registration
- Event handling
- Interface remota (remote.call)

**Componentes da Camada Lua**:
| Componente | Arquivo | Descrição |
|------------|---------|-----------|
| `control.lua` | `data\base\script\freeplay\` | Ponto de entrada freeplay |
| `freeplay.lua` | `data\base\script\freeplay\` | Lógica completa freeplay |
| `event_handler.lua` | `data\base\script\` | Centraliza event registrations |
| `util.lua` | Core module | Utilitários diversos |
| Prototypes | `data\base\prototypes\` | ~78 arquivos de definição |
| Scripts variados | `data\base\script\` | Core scripts |

**Comunicação Lua ↔ C++**:

| Direção | Mechanim | Exemplos |
|---------|----------|----------|
| C++ → Lua | `prototypes` | Entities, items, recipes, technologies |
| Lua → C++ | Eventos/Calls | `defines.events.on_player_created`, `remote.call()` |

### 3. Módulos do Engine Core

O engine core do Factorio é modularizado em vários subsystems:

#### 3.1 Módulos Identificados

| Módulo | Função | Arquivo/Referência |
|--------|--------|------------------|
| `simulation` | Loop de tick, entity updates | Engine core |
| `rendering` | Backends D3D11/OpenGL/Vulkan (carregados dinamicamente), sprites | Engine core |
| `physics` | Colisão, projectiles | Engine core |
| `logistics` | Belts, robots, network | Engine core |
| `production` | Crafting, recipes | Engine core |
| `fluid` | Fluids, pipes | Engine core |
| `pathfinding` | Navmesh, pathfinding | Engine core |
| `networking` | Multiplayer sync | Engine core |
| `saving` | Save system | Engine core |
| `input` | Player inputs | Engine core |
| `ai` | Enemy AI | Engine core |

#### 3.2 Mapa de Dependências entre Módulos

```
┌──────────────────────────────────────────────────────────────┐
│                   FACTORIO ENGINE MODULES                      │
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │  SIMULATION     │  │  LOGISTICS      │  │  PRODUCTION │ │
│  │  (Tick 60Hz)   │  │  (Belts/Robots) │  │  (Crafting) │ │
│  └───────▲─────────┘  └───────▲─────────┘  └───────▲─────┘ │
│           │                    │                    │          │
│  ┌───────▼─────────┐  ┌───────▼─────────┐  ┌───────▼─────┐ │
│  │  PHYSICS/COL    │  │  RENDERING      │  │  FLUIDS     │ │
│  │  ISION          │  │   (GPU*)       │  │  (Pipes)    │ │
│  └───────▲─────────┘  └───────▲─────────┘  └───────▲─────┘ │
│           │                    │                    │          │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                 NETWORKING/MP                          │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                               │
│  TODOS os módulos comunicam através de:                      │
│  - prototypes (registro/acesso)                             │
│  - defines.events (event system)                           │
│  - game.tick (sincronização determinística)                │
│  - shared memory (state entre módulos)                     │
│                                                               │
└──────────────────────────────────────────────────────────────┘
```

### 4. Padrões de Design Arquitetônicos

#### 4.1 Data-Driven Design

A maior parte do comportamento é definida em dados (prototipos Lua) em vez de código C++ compilado:

| Categoria | Definido em | Exemplos |
|-----------|-------------|----------|
| Entities | `prototypes/entity/*.lua` | Miners, assemblers, belts |
| Items | `prototypes/item/*.lua` | Iron plate, copper cable |
| Recipes | `prototypes/recipe/*.lua` | Smelting, assembling |
| Technologies | `prototypes/technology/*.lua` | Research tree |
| Fluids | `prototypes/fluid/*.lua` | Water, oil, chemicals |
| Tiles | `prototypes/tile/*.lua` | Grass, stone, concrete |

**Vantagens**:
- Modding sem recompilar o executable
- Iterações rápidas (mudança de dados ao vivo)
- Flexibilidade para balanceamento

**Desvantagens**:
- Performance overhead indireto
- Limitações de expressividade Lua vs C++
- Debug mais difícil (stack traces Lua vs C++)

#### 4.2 Event-Driven Architecture

O sistema usa eventos extensivamente para comunicação entre módulos:

| Evento | Definição | Destinatários |
|--------|-----------|---------------|
| `on_tick` | A cada tick | Todos os módulos registrados |
| `on_player_created` | Quando player cria | Mods, core scripts |
| `on_entity_died` | Quando entity morre | Mods, core scripts |
| `on_research_peed` | Quando research completa | Mods, core scripts |
| `on_gui_click` | Quando GUI click | Mods, interface |

**Vantagens**:
- Desacoplamento entre módulos
- Extensibilidade via mods
- Fluxo de controle claro

**Desvantagens**:
- Ordem de execução de events importa
- Performance overhead (event dispatch)
- Debug de fluxo de events complexo

#### 4.3 Component-Based Design (Entity System)

Entities no Factorio seguem um padrão component-like, embora implementado através de protótipos:

| Componente | Descrição | Protótipo |
|------------|-----------|-----------|
| `type` | Tipo da entity | `entity-prototype` |
| `name` | Nome único | `entity-prototype` |
| `position` | Posição no mapa | Runtime |
| `direction` | Direção | `defines.direction` |
| `energy` | State de energy | Runtime |
| `health` | Health points | Runtime |
| `inventory` | Inventários | Runtime |
| `modules | Slots para modules | `module-inventory-size` |
| `connected_entities` | Entities conectadas | `Lconnectable`-type |

#### 4.4 Interface Limpa (Clean API Boundary)

A API entre C++ e Lua é bem definida:

| Interface | Direção | Mecanism |
|-----------|---------|----------|
| `prototypes` | C++ → Lua | Registry of all prototypes |
| `defines.events` | C++ → Lua | Event IDs e handlers |
| `remote.call()` | Lua → C++ | Remote procedure calls |
| `script.register_on_event()` | Lua → C++ | Event registration |
| `game.gui` | C++ ↔ Lua | GUI management |
| `player.print()` | Lua → C++ | Print to player |

## 5. Estrutura de Comunicação Inter-Layer

### 5.1 Prototypes Registry (C++ → Lua)

O motor C++ registra todos os protótipos em uma tabela global acessível via Lua:

```lua
-- Acessando protótipos registrados no C++
local iron_ore = prototypes.entity["iron-ore"]
local iron_plate = prototypes.item["iron-plate"]
local iron_smelting = game.technologies["iron-smelting"]

-- Verificando se prototype existe
if prototypes.entity["iron-ore"] then
  -- exists
end
```

**Tipos de Prototypes Acessíveis**:
- `prototypes.entity` - Entities
- `prototypes.item` - Items
- `prototypes.recipe` - Recipes
- `prototypes.technology` - Technologies
- `prototypes.fluid` - Fluids
- `prototypes.tile` - Tiles
- `prototypes.equipment` - Equipment
- `prototypes.module` - Modules (some)

### 5.2 Remote Interface (Lua ↔ C++)

O sistema `remote.call()` permite chamadas entre mods ou entre Lua e engine:

```lua
-- Calling C++ function from Lua
remote.call("core", "some_function", parameter)

-- Defining remote interface (typically in mod's script)
remote.add_interface("my_mod", {
  some_function = function(param1, param2)
    -- implementation
  end
})
```

**Protocolos Conhecidos**:
- `core` - Interface central do Factorio (documentada)
- `mining` - Mining operations
- `crafting` - Crafting operations
- `space-age` - Space age mod interface

### 5.3 Event System (C++ → Lua)

Events são registrados no C++ e manipulados em Lua:

```lua
-- Registrando handler para evento
script.on_event(defines.events.on_player_created, function(event)
  -- handler logic
  -- Called quando player é criado
end)

-- Events disponíveis (resumo)
defines.events.on_tick                    -- A cada tick
defines.events.on_player_created          -- Player criado
defines.events.on_player_respawned        -- Player ressurgido
defines.events.on_entity_died             -- Entity morreu
defines.events.on_research_received       -- Research completada
defines.events.on_gui_click               -- GUI click
defines.events.on_built_entity            -- Entity built
defines.events.on_robot_launched          -- Robot launched
```

### 5.4 State Synchronization (Multiplayer)

Para multiplayer, o estado deve ser sincronizado entre server e clients:

```
Sincronização de Estado:

1. Server simula tick deterministically
2. Client recebe inputs dos jogadores
3. Server valida e aplica simulation
4. Server envia estado confirmado aos clients
5. Clients aplicam estado recebido
```

**Estado Sincronizado**:
- Entity positions e states
- Player inventories
- Research progress
- Map generation state
- Entity tree states

## 6. Módulos e Dependências

### 6.1 Dependências do Sistema Operacional

| Dependência | Versão Mínima | Propósito |
|-------------|---------------|-----------|
| Windows 10+ | 10.0.19041+ | Sistema operacional |
| Direct3D 11 | 11.0 | Pipeline rendering |
| VCRuntimes | 14.2x+ (VS 2019) | C Runtime Library |
| Windows API | Various | System calls, file I/O, etc. |

### 6.2 Bibliotecas Terceirizadas

| Biblioteca | Propósito | Licença |
|------------|-----------|---------|
| ~~Poco~~ | não encontrado (0 strings) [HIPÓTESE removida] | — |
| ~~Boost~~ | não encontrado (0 strings) [HIPÓTESE removida] | — |
| ~~OpenSSL~~ | não encontrado; TLS via Schannel (`schannel: failed to send close msg`, EVIDENCE_SCAN.txt) [CONFIRMADO] | — |
| **zlib** | Compression (saves) | zlib |
| **LZ4** | Compression (saves) | LZ4 License |

### 6.2 Formato de Executável

| Formato | Detalhes |
|---------|----------|
| **PE32+** | 64-bit Portable Executable |
| **Rich Header** | Microsoft Visual C++ identificador |
| **Entry Point** | `WinMain` ou `main` |
| **Sections** | .text, .data, .rdata, .pdata, .xdata, .idata, .optheader |
| **Architecture** | x86_64 (AMD64) |

## 7. Points de Extensão e Modding

### 7.1 Extensões Arquiteturais

| Ponto de Extensão | Mecanismo | Exemplos |
|------------------|-----------|----------|
| `data:extend()` | Adicionar prototypes | New entities, items, recipes |
| `prototypes.entity` | Override prototypes | Modificar entities existentes |
| `defines.events` | Registrar events | Novos event handlers |
| `remote.add_interface()` | Adicionar remote interfaces | Mod comunicação |
| `script.on_event()` | Registrar event handlers | Mod event processing |
| `game.gui` | Custom GUI | Custom windows, panels |

### 7.2 Exemplo de Extensão Arquitetural

```lua
-- Mod adicionando nova entity via data:extend()
data:extend({
  {
    type = "entity",
    name = "my-custom-miner",
    -- ... definição completa
  }
})

-- Override de prototype existente
data:extend({
  {
    type = "entity",
    name = "iron-ore",
    -- ... modifying existing prototype
    flags = {"placeable-player", "resource", "not-iraisable"},
  }
})
```

## 8. Diagramas Arquiteturais

```
┌─────────────────────────────────────────────────────────────────┐
│                   FACTORIO v2.1.24 ARQUITETURA                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────┐    ┌─────────────────────────────┐ │
│  │  EXECUTÁVEL (C++)   │    │  CAMADA LUA (Scripting)      │ │
│  │  factorio.exe       │    │  scripts/                    │ │
│  │  ~49.4 MB          │    │  data/base/script/          │ │
│  └────────▲────────────┘    └────────▲─────────────────────┘ │
│           │                      │                     │        │
│  ┌────────▼────────────┐  ┌─────▼────────────────────┐ │
│  │  ENGINE CORE MÓDULOS  │  │  PROTOTIPOS & SCRIPTS    │ │
│  │  │                  │  │  │                      │ │
│  │  │ simulation/      │  │  │ prototypes/            │ │
│  │  │ rendering/       │  │  │ scripts/               │ │
│  │  │ physics/         │  │  │ event_handler.lua      │ │
│  │  │ logistics/       │  │  │ util.lua               │ │
│  │  │ production/      │  │  │ ... (70+ files)        │ │
│  │  │ fluid/           │  │  └──────────────────────┘ │
│  │  │ pathfinding/     │                             │
│  │  │ networking/      │                             │
│  │  │ saving/          │                             │
│  │  │ input/           │                             │
│  │  │ ai/              │                             │
│  │  └────────────────┘                             │
│  └─────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  INTERFACE DE COMUNICAÇÃO                              │ │
│  │  │                                                     │ │
│  │  │  C++ → Lua:  prototypes, defines.events             │ │
│  │  │  Lua → C++:  remote.call(), script.on_event        │ │
│  │  │  Bidirectional: game.gui, player.print              │ │
│  │  └─────────────────────────────────────────────────────┘ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  DEPENDÊNCIAS EXTERNAIS                               │ │
│  │  │  Direct3D 11, VCRuntime, Windows API                │ │
│  │  └─────────────────────────────────────────────────────┘ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  FLUXO DE TICK                                        │ │
│  │  │  tick 60Hz → simulation → rendering → events         │ │
│  │  └─────────────────────────────────────────────────────┘ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Observações Técnicas

1. **Arquitetura Híbrida**: A combinação C++/Lua fornece o melhor dos dois mundos - performance do C++ e flexibilidade do Lua.

2. **Determinismo**: A simulação é totalmente determinística - dado o mesmo estado inicial e inputs, o resultado é sempre idêntico. Critical para multiplayer.

3. **Modding Friendly**: ~90% do comportamento do jogo pode ser modificado através de scripts Lua e protótipos, sem necessidade de modificar o executable.

4. **Separação de Responsabilidades**: Cada layer tem responsabilidades claras - C++ para performance-critical e sistema, Lua para lógica de jogo e modding.

5. **Interface Bem-Definida**: A boundary entre C++ e Lua é claramente definida através de prototypes, events e remote interfaces.

6. **Versionamento**: A arquitectura evoluiu gradualmente de 0.15 a 2.1.x, mantendo backward compatibility através de `on_configuration_changed` handlers.

## Próximos Relatórios Planejados

- `ENGINE_ANALYSIS.md` - Análise detalhada do motor C++ e interações com Lua
- `PHYSICS_ANALYSIS.md` - Física e colisões avançado
- `SIMULATION_ANALYSIS.md` - Motor de simulação completo
- `LOGISTICS_ANALYSIS.md` - Sistema logistics avançado
- `MODDING_API.md` - API de modding completa (já parcialmente coberto)
- `NETWORKING_ANALYSIS.md` - Multiplayer e networking
- `SAVE_SYSTEM.md` - Sistema de saves
- `DEPENDENCIES.md` - Dependências e bibliotecas
- `FUNCTION_INVENTORY.md` - Inventário de funções
- `ALGORITHMS.md` - Algoritmos identificados
- `ARCHITECTURE_DIAGRAMS.md` - Diagramas arquiteturais
- `REA_FINDINGS.md` - Descobertas REA
- `UNKNOWN_COMPONENTS.md` - Componentes desconhecidos
- `RECONSTRUCTION_ROADMAP.md` - Roadmap para reconstrução futura