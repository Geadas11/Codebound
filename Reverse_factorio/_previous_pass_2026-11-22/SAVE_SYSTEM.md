# SAVE_SYSTEM — Sistema de gravação

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE] · [CONHECIMENTO PÚBLICO]

## 1. Evidência no binário — [CONFIRMADO]
Strings encontradas em `factorio.exe`:
| String | Ocorrências |
|---|---|
| `level.dat` | 1 |
| `level-init.dat` | 2 |
| `script.dat` | 2 |
| `control.lua` | 5 |
| `.zip` | 16 |
| `ZSTD` / `zstd` | 2 / 1 |
| `mod-settings.dat` | 1 |
| `achievements.dat` | 1 |
| `player-data.json` | 1 |

## 2. Estrutura do save
[INFERÊNCIA] Um save é um arquivo `.zip` com, pelo menos:
- `level.dat` (ou partes numeradas — [CONHECIMENTO PÚBLICO] em 1.1+/2.x é dividido em `level.dat0`, `level.dat1`, …; a regex `level\.dat[0-9]?` encontrou apenas `level.dat`, o que sugere que os sufixos são gerados por formatação `%u`) — estado do mapa/simulação;
- `level-init.dat` — estado inicial do cenário;
- `script.dat` — estado serializado do Lua (`storage` de cada mod);
- `control.lua` + ficheiros do cenário — o script do cenário viaja dentro do save.
[HIPÓTESE] Compressão interna com zstd ou deflate. Não verificado — não foi aberto nenhum save do utilizador (preservar dados).
[HIPÓTESE] Magic "FACO" (referido em relatórios anteriores) — **não confirmado**; trate como hipótese.

## 3. `storage` (Lua)
[CONFIRMADO] `freeplay.lua` grava estado em `storage` (ex.: `storage.chart_distance`, por omissão 200) e usa `on_init` / `on_configuration_changed`.
[INFERÊNCIA] O conteúdo de `storage` é serializado para `script.dat`; só tipos serializáveis (tabelas, números, strings, booleanos, referências a objetos do jogo) sobrevivem.
[CONHECIMENTO PÚBLICO] Funções e metatabelas não registadas não são guardadas; `on_load` não pode escrever em `storage`.

## 4. Migrações
[CONFIRMADO] `on_configuration_changed` é usado por `freeplay.lua` → o motor notifica scripts quando versões de mods mudam ao carregar um save.
[INFERÊNCIA] Existe também migração de protótipos (renomear entidades) via pasta `migrations` dos mods — ver ADVANCED_MODDING.md.

## 5. Determinismo
[INFERÊNCIA] Save + lista de ações → mesmo estado em todas as máquinas; é por isso que `desync on loading` existe (ver NETWORKING_ANALYSIS.md). O save é também o "snapshot" enviado ao cliente quando entra num servidor.

## 6. Ficheiros fora do save
[CONFIRMADO strings] `mod-list.json` (mods ativos), `mod-settings.dat` (definições de mods), `player-data.json`, `achievements.dat`, `config.ini` (não pesquisado).
Localização: [CONHECIMENTO PÚBLICO] `%APPDATA%\Factorio` ou pasta de instalação se `config-path.cfg` indicar `use-system-read-write-data-directories=false`. **Não inspecionado** nesta investigação.

## 7. O que falta
- Abrir um save de teste (cópia, nunca o original) e documentar os membros do zip e os primeiros bytes de `level.dat*`.
- Confirmar o algoritmo de compressão e o cabeçalho de versão.

## Evidência adicional (scan 2026-11-23)
Fonte: `EVIDENCE_SCAN.txt` (scan só de leitura, Python/mmap; REA indisponível — `provider_unavailable`). Recursos PE: Evidence ID `ev_a9903e7c31d7f73ee72e27699d064657da6163bd680018c8b711ce595e569db6`.
- [CONFIRMADO] Strings `zip` e `level.dat` adjacentes @0x21db550 (junto a `Invalid save file location`).
- [CONFIRMADO] Magic `FACO`: 0 ocorrências no exe; as 10 no PDB são só fragmentos de nomes mangled (`??_C@...FACO...`). Continua **não confirmado** como magic de save.
