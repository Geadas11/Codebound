# MODDING_API — Referência da API de mods (fases data e control)

**Legenda:** [CONFIRMADO] = visto nos ficheiros de `C:\GOG Games\Factorio\data` · [INFERÊNCIA] · [CONHECIMENTO PÚBLICO] = documentação oficial lua-api.factorio.com, não reverificada aqui.

## 1. Estágios de carregamento
| Estágio | Ficheiros | Estado |
|---|---|---|
| Settings | `settings.lua`, `settings-updates.lua`, `settings-final-fixes.lua` | [CONHECIMENTO PÚBLICO] |
| Data | `data.lua` → `data-updates.lua` → `data-final-fixes.lua` | [CONFIRMADO] `data\core\data.lua` existe; ordem pelo conhecimento público |
| Control (runtime) | `control.lua` | [CONFIRMADO] `script\freeplay\control.lua`; string `control.lua` 5× no exe |

[CONFIRMADO] `data\core\lualib\dataloader.lua` existe → [INFERÊNCIA] implementa `data:extend{...}`, que adiciona protótipos à tabela `data.raw`.

## 2. Fase data — protótipos
[CONFIRMADO] Ficheiros em `data\base\prototypes\entity\` (50 ficheiros, ex.: `entities.lua`, `enemies.lua`, `turrets.lua`, `trains.lua`, `transport-belts.lua`, `mining-drill.lua`, `resources.lua`, `flying-robots.lua`) e `prototypes\item.lua`.
Contagens heurísticas de `type = "..."`: `recipe` 196, `technology` 193, `fluid` 52, `tile` 41, `item-group` 11, `item-subgroup` 113, `virtual-signal` 154, `custom-input` 14, `shortcut` 20, `ammo-category` 14, `autoplace-control` 12.
Exemplo do padrão (forma, não cópia): `data:extend({ { type = "...", name = "...", ... } })`.

Utilitários confirmados em `data\core\lualib`: `util.lua`, `math2d.lua`, `math3d.lua`, `collision-mask-util.lua`, `sound-util.lua`, `circuit-connector-*`, `resource-autoplace.lua`, `autoplace_utils.lua`, `meld.lua`, `prototype-hierarchy.lua`, `prototype-info.lua`, `data-duplicate-checker.lua`.

## 3. Fase control — runtime
### 3.1 Objetos globais
| Global | Uso | Estado |
|---|---|---|
| `script` | eventos, `script.active_mods` | [CONFIRMADO] em `control.lua` do freeplay |
| `storage` | estado persistente do mod (substitui `global` da 1.x) | [CONFIRMADO] em `freeplay.lua` |
| `remote` | `remote.add_interface` / `remote.call` | [CONFIRMADO] interface "freeplay" |
| `game` | jogadores, surfaces, `game.print` | [CONHECIMENTO PÚBLICO] |
| `prototypes` | protótipos em runtime (em 2.x **não** é `game.prototypes`) | [CONHECIMENTO PÚBLICO]; corrige relatórios anteriores |
| `defines` | constantes (`defines.events.*`) | [CONHECIMENTO PÚBLICO] |
| `settings`, `rendering`, `helpers` | definições, desenho, utilitários | [CONHECIMENTO PÚBLICO] |

### 3.2 Biblioteca `event_handler` [CONFIRMADO]
`script\freeplay\control.lua`:
```lua
local handler = require("event_handler")
handler.add_lib(require("freeplay"))
if script.active_mods["space-age"] then handler.add_lib(require("space-finish-script")) else handler.add_lib(require("silo-script")) end
```
[INFERÊNCIA] `event_handler` agrega várias "libs" (tabelas com `events`, `on_init`, `on_load`, `on_configuration_changed`) e regista um único handler por evento que despacha para todas — contorna o limite de um handler por evento por mod.

### 3.3 Ciclo de vida
| Hook | Quando | Estado |
|---|---|---|
| `on_init` | mod adicionado a jogo novo/existente | [CONFIRMADO] usado por freeplay |
| `on_configuration_changed` | versões de mods/jogo mudaram | [CONFIRMADO] usado por freeplay |
| `on_load` | carregar save (sem escrever `storage`) | [CONHECIMENTO PÚBLICO] |
| `on_nth_tick(n)` / `on_tick` | periódico | [CONHECIMENTO PÚBLICO] |

### 3.4 Interfaces remotas [CONFIRMADO]
`freeplay.lua` regista `remote.add_interface("freeplay", {...})` com funções para alterar itens iniciais, itens de respawn, crash site, cutscene e `chart_distance`. Valores por omissão confirmados: iron-plate 8, wood 1, pistol 1, firearm-magazine 10, burner-mining-drill 1, stone-furnace 1; respawn: pistol + 10 magazines; mapa 50×50 salta a intro.

## 4. Regras de determinismo
[INFERÊNCIA a partir de NETWORKING_ANALYSIS] O control stage corre em todos os pares em lockstep: um mod que dependa de estado local (relógio, ficheiros, `pairs` sobre chaves não determinísticas) causa desync.

## 5. Mods presentes na instalação
[CONFIRMADO] `data\base` (2.1.24) e `data\core`. Pastas de DLC (`space-age`, `quality`, `elevated-rails`) — [NÃO VERIFICADO] nesta sessão; o freeplay testa `script.active_mods["space-age"]`, logo o código suporta-as.

## 6. Limitações
Esta referência cobre o que se vê nos ficheiros. A lista completa de classes (`LuaEntity`, `LuaSurface`, …) e eventos está no ficheiro oficial `runtime-api.json` (não incluído na instalação GOG; não verificado).
