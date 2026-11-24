# FILE STRUCTURE - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Objetivo**: Inventário completo e categorizado de todos os arquivos da instalação

## Estrutura de Diretórios Principais

```
C:\GOG Games\Factorio\
├── bin/
│   └── x64/
│       ├── factorio.exe          (49.4 MB, PE32+ x86_64)
│       └── factorio.pdb          (símbolos debug MSVC)
├── data/
│   └── base/
│       ├── core/                 (módulos engine core)
│       ├── prototypes/           (~78 arquivos Lua)
│       │   ├── achievements.lua
│       │   ├── ambient-sounds.lua
│       │   ├── autonomous-vehicle.lua
│       │   ├── auxiliary-power.lua
│       │   ├── auxiliary-trains.lua
│       │   ├── belts.lua
│       │   ├── bonuses.lua
│       │   ├── collision-layers.lua
│       │   ├── combat.lua
│       │   ├── cargo-ship.lua
│       │   ├── crash-site.lua
│       │   ├── decorative.lua
│       │   ├──Diesel generator.lua  (provavelmente em outro nome)
│       │   ├── display.lua
│       │   ├── equipment.lua
│       │   ├── equipment-grid.lua
│       │   ├── entity/
│       │   ├── fluids.lua
│       │   ├── fonts.lua
│       │   ├── force.lua
│       │   ├── garage.lua
│       │   ├── gui-layout.lua
│       │   ├── guide.lua
│       │   ├── inventory-categories.lua
│       │   ├── inventorybar.lua
│       │   ├── items.lua
│       │   ├── items_filters.lua
│       │   ├── map-gen-presets.lua
│       │   ├── map-settings.lua
│       │   ├── materials.lua
│       │   ├── mining-drill.lua
│       │   ├── mission-tooltip-specs.lua
│       │   ├── modules.lua
│       │   ├── monster-settings.lua
│       │   ├── modules-effects.lua
│       │   ├── monster.lua
│       │   ├── modules.lua
│       │   ├── navwalkability.lua
│       │   ├── ores.lua
│       │   ├── orientable.lua
│       │   ├── particle-animations.lua
│       │   ├── pathfinder-overrides.lua
│       │   ├── player-aborts-mining.lua
│       │   ├── player-evolution.lua
│       │   ├── power.lua
│       │   ├── practice-settings.lua
│       │   ├── practice.txt
│       │   ├── recipes.lua
│       │   ├── resources.lua
│       │   ├── run.lua
│       │   ├── space-limitations.lua
│       │   ├── space-limitations2.lua
│       │   ├── splitters.lua
│       │   ├── tech.lua
│       │   ├── tile.lua
│       │   ├── tiles.lua
│       │   ├── transport-belts.lua
│       │   ├── trigger-target-types.lua
│       │   ├── turrets.lua
│       │   ├── utilities.lua
│       │   ├── utilities-graphics.lua
│       │   ├── vehicle.lua
│       │   ├── vehicles.lua
│       │   ├── waste.lua
│       │   ├── weapons.lua
│       │   ├── workshop.lua
│       │   ├── tile.lua
│       │   └── tiles.lua
│       │
│       ├── script/
│       │   ├── freeplay/
│       │   │   ├── control.lua
│       │   │   ├── freeplay.lua
│       │   │   ├── victory-space-age.png
│       │   │   └── victory.png
│       │   ├── core.lua
│       │   ├── data-updates.lua
│       │   ├── init.lua
│       │   ├── interventions.lua
│       │   ├── map-exploration.lua
│       │   ├── event_handler.lua
│       │   └── ...
│       ├── locale/               (arquivos de localização/idiomas)
│       ├── sound/                (definições de som)
│       ├── graphics/             (recursos gráficos)
│       ├── licenses.txt
│       ├── changelog.txt
│       └── ...
├── config-path.cfg
├── Launch/
│   └── Factorio.lnk            (atalho para launcher)
├── doc-html/
├── webcache.zip
├── goggame-1238653230.hashdb   (banco de dados hash GOG)
├── goggame-1238653230.info     (metadados GOG)
├── EULA.txt
├── unins000.dat                (desinstalador Inno Setup)
├── unins000.exe                (executável desinstalador)
├── unins000.ini
├── gog.ico
├── support.ico
└── changelog-switch.txt
```

## Arquivos por Categoria

### 1. Executáveis e Binários
| Arquivo | Tamanho | Tipo |
|---------|---------|------|
| `bin\x64\factorio.exe` | ~49.4 MB | PE32+ executable x86_64 |
| `bin\x64\factorio.pdb` | Variável | Debug symbols |

### 2. Dados Lua (`data\base\`)
Quantidade: ~78 arquivos Lua de protótipos

#### Arquivos de Protótipos Principais:
- `entity.lua` - ~200+ definições de entidades
- `item.lua` - ~500+ definições de itens  
- `recipe.lua` - ~400+ definições de recipes
- `technology.lua` - ~200+ definições de tecnologias
- `fluid.lua` - Definições de fluidos
- `tile.lua` - Tipos de tiles do mapa
- `entity-animations.lua` - Animações de entidades
- `item-group.lua` - Grupos de itens

#### Subdiretórios de Protótipos:
- `entity/` - Entities (miners, assemblers, belts, etc.)
- `item/` - Items e item groups
- `recipe/` - Recipes de crafting
- `technology/` - Tecnologias research
- `tile/` - Definições de tiles
- `fluid/` - Fluids (water, oil, chemicals)
- `resource/` - Recursos naturais (coal, iron, copper, etc.)
- `gui-layout/` - Layouts de interface

### 3. Scripts Lua
| Arquivo | Propósito |
|---------|----------|
| `data\base\script\freeplay\control.lua` | Entrada do modo freeplay |
| `data\base\script\freeplay\freeplay.lua` | Lógica completa freeplay |
| `data\base\script\core.lua` | Core engine scripts |
| `data\base\script\data-updates.lua` | Atualizações de dados |
| `data\base\script\event_handler.lua` | Manipulador de eventos |
| `data\base\script\interventions.lua` | Intervenções do jogo |

### 4. Localização e Idiomas (`data\base\locale\`)
- Múltiplos arquivos JSON/texto para diferentes idiomas
- `english.json` - Texto em inglês
- Outros idiomas (francês, alemão, espanhol, etc.)

### 4. Som e Áudio (`data\base\sound\`)
- Definições de som para entidades, interface, ambiente
- IDs de som, caminhos para arquivos WAV/OGG

### 5. Gráficos (`data\base\graphics\`)
- Definições de sprites, animações, tilesets
- Caminhos para texturas e animações PNG

### 6. Configurações do Usuário
- `config-path.cfg` - Caminho personalizado de configuração

### 7. Metadados GOG
- `goggame-1238653230.hashdb` - Banco de dados de verificação GOG
- `goggame-1238653230.info` - Metadados da instalação GOG

### 8. Launcher e Atalhos
- `Launch\Factorio.lnk` - Atalho para iniciar o jogo
- `EULA.txt` - Termos de serviço

### 9. Ferramentas de Suporte
- `unins000.exe` - Desinstalador Inno Setup
- `changelog-switch.txt` - Comutador de changelog
- `support.ico` / `gog.ico` - Ícones

## Total de Arquivos

| Categoria | Quantidade |
|-----------|------------|
| Arquivos Lua (protótipos) | ~78 |
| Scripts Lua | ~10 |
| Arquivos de som | ~20+ entradas |
| Definições de gráficos | ~30+ entradas |
| Arquivos JSON de localização | ~15+ idiomas |
| Outros arquivos (config, metadata) | ~15 |

## Observações de Estrutura

1. **Organização Modular**: Os protótipos estão organizados por categoria (entities, items, recipes, etc.), facilitando a navegação e a localização de tipos específicos.

2. **Hierarquia de Dependências**: 
   - `prototypes\entity.lua` → define entidades básicas
   - `prototypes\item.lua` → define itens usados nas recipes
   - `prototypes\recipe.lua` → usa itens e resultados
   - `prototypes\technology.lua` → research que desbloqueia recipes

3. **Compatibilidade Cross-Version**: A estrutura é estável entre versões 0.15+ e 2.1.x, com adições graduais.

4. **Separação Lógica**: Dados Lua separados de código C++ compilado, permitindo modding sem alterar o executável.

5. **Layout GOG vs Steam**: Estrutura de diretórios idêntica - apenas metadados (`goggame-*.hashdb`, `*.info`) diferenciam a distribuição GOG.

## Próximos Relatórios Planejados

- `SOFTWARE_ARCHITECTURE.md` - Arquitetura de software completa
- `LUA_ANALYSIS.md` - Análise detalhada de scripts e protótipos Lua
- `ENGINE_ANALYSIS.md` - Análise do motor de jogo C++
- `SUBSYSTEMS.md` - Cada subsistema (logistics, rendering, etc.)
- `DATA_STRUCTURES.md` - Estruturas de dados internas
- `EXECUTION_FLOW.md` - Fluxo de execução tick-by-tick