# ADVANCED_MODDING — Modding avançado

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [CONHECIMENTO PÚBLICO]

Complementa MODDING_API.md. Os exemplos descrevem padrões; não são cópias de código do jogo.

## 1. Estrutura de um mod
[CONHECIMENTO PÚBLICO] Pasta ou zip `nome_versao` com `info.json` (name, version, title, author, factorio_version, dependencies), opcionalmente `settings.lua`, `data*.lua`, `control.lua`, `locale\`, `migrations\`, `graphics\`.
[CONFIRMADO] `data\base\info.json` segue este formato (name `base`, version 2.1.24, author Wube Software).

## 2. Modificar protótipos existentes
- Em `data-updates.lua` / `data-final-fixes.lua`, alterar `data.raw["tipo"]["nome"]` [CONHECIMENTO PÚBLICO].
- `core\lualib\meld.lua` [CONFIRMADO ficheiro] → [INFERÊNCIA] merge profundo de tabelas, útil para patches parciais de protótipos.
- `util.table.deepcopy` (de `core\lualib\util.lua` [CONFIRMADO ficheiro]) para clonar um protótipo e criar uma variante.
- `data-duplicate-checker.lua` [CONFIRMADO ficheiro] → [INFERÊNCIA] deteta nomes duplicados.

## 3. Compatibilidade com DLC
[CONFIRMADO] padrão em `script\freeplay\control.lua`: `if script.active_mods["space-age"] then ... else ... end`.
Na fase data o equivalente é `mods["space-age"]` [CONHECIMENTO PÚBLICO].

## 4. Composição de scripts com `event_handler`
[CONFIRMADO] `handler.add_lib(require("..."))` permite dividir um mod em módulos, cada um com a sua tabela `events` e hooks de ciclo de vida. Útil em mods grandes para evitar conflitos de `script.on_event`.

## 5. Interfaces entre mods
[CONFIRMADO] `remote.add_interface("freeplay", {...})` em `freeplay.lua`. Outro mod pode chamar `remote.call("freeplay", "<função>", ...)` — por exemplo, alterar os itens iniciais. Verificar a existência com `remote.interfaces["freeplay"]` [CONHECIMENTO PÚBLICO].

## 6. Migrações
- `on_configuration_changed` [CONFIRMADO uso] para migrar `storage`.
- `migrations\*.json` para renomear protótipos e `migrations\*.lua` para scripts únicos [CONHECIMENTO PÚBLICO].

## 7. Geração de mapa
[CONFIRMADO] Protótipos `noise-expression` (89×), `noise-function` (28×), `autoplace-control` (12×) e `resource-autoplace.lua`. Mods podem definir novas expressões e ligá-las a recursos ou tiles.

## 8. Camadas de colisão personalizadas
[CONFIRMADO] `collision-layer` é um tipo de protótipo (24×). [INFERÊNCIA] Um mod pode declarar camadas novas e usar `collision-mask-util.lua` para as aplicar.

## 9. GUI
[CONFIRMADO] `core\lualib\mod-gui.lua` existe. Contagens `type=`: `frame` 64, `flow` 50, `button` 47, `label` 135, `table` 36, `sprite-button` 12 → [INFERÊNCIA] elementos de GUI construídos em Lua no runtime (`player.gui.*.add{type=...}`).

## 10. Regras de ouro
1. Determinismo (lockstep — ver NETWORKING_ANALYSIS.md).
2. Não escrever em `storage` dentro de `on_load`.
3. Usar `prototypes` (2.x), não `game.prototypes`.
4. Testar em multijogador local antes de publicar.
