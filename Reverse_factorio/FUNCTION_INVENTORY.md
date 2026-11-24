# FUNCTION_INVENTORY — Inventário das funções identificadas

O inventário completo **não é copiado para aqui**: 249 292 nomes ficariam ilegíveis em Markdown. Fica nos artefactos, gerados do PDB [C]:

| Artefacto | Conteúdo |
|---|---|
| `artifacts/pdb/pdb_procs.txt` | todos os procedimentos globais (nome qualificado, ex.: `Inserter::update`) |
| `artifacts/pdb/pdb_proc_ranges.tsv` | **nome, secção, offset, tamanho** de cada função (S_GPROC32/S_LPROC32) → RVA = 0x1000 + offset para a secção 1 |
| `artifacts/pdb/pdb_publics.tsv` | 205 808 símbolos públicos decorados com `secção:offset` |
| `artifacts/function_inventory_by_dir.tsv` | nº de funções por diretório de `src/` |
| `artifacts/function_inventory_by_class.tsv` | nº de funções por classe, com o diretório-fonte |
| `artifacts/calls_*.txt`, `artifacts/callers_tick.txt` | chamadas diretas em ordem / chamadores de funções-chave |

Ferramentas: `scripts/calls.py "Classe::metodo"` (chamadas diretas, em ordem) e `scripts/callers.py` (quem chama). Ambas são só de leitura.

## 1. Distribuição por subsistema [C]
Foram mapeadas 58 971 funções-membro de classes do Factorio para o diretório-fonte da classe, via `LF_UDT_SRC_LINE`. Ficaram 7 541 por mapear: templates e classes sem registo de linha.

| Diretório | Funções | | Diretório | Funções |
|---|---:|---|---|---:|
| Script | 10 969 | | Logistics | 674 |
| Gui | 9 982 | | Noise | 666 |
| Entity | 9 227 | | Achievement | 622 |
| Util | 3 682 | | CustomGui | 622 |
| (raiz src) | 3 579 | | MapEditor | 509 |
| Graphics | 2 110 | | Data | 480 |
| ID | 1 863 | | Blueprint | 479 |
| Controller | 1 250 | | Trigger | 471 |
| Info | 1 247 | | Space | 458 |
| CircuitNetwork | 1 149 | | Sound | 420 |
| Item | 1 043 | | Equipment / Technology / Energy | 387 / 377 / 376 |
| Net | 837 | | Surface | 339 |
| Input | 727 | | DebugAdapter | 331 |
| AI | 691 | | Train / Fluid / Rail | 213 / 184 / 116 |
| Map | 676 | | | |

Leitura [I]: a ponte Lua (Script) e a GUI somam mais funções do que a simulação. O núcleo simulado (Entity, Logistics, Fluid, Energy, Train, Belt…) é compacto, e a superfície de API e de UI é grande.

## 2. Classes com mais funções [C]
`Values` 842 · `LuaEntityPrototype` 631 · `LuaEntity` 596 · `Entity` 502 · `LuaBindableObject` 428 · `InputActionHandler` 360 (um handler por tipo de `InputAction`) · `Controller` 291 · `EntityPrototype` 245 · `Character` 226 · `EditorController` 218 · `LuaGuiElement` 216 · `LuaForce` 209 · `PlayerInputSource` 191 · `LuaPlayer` 173 · `LuaSurface` 169 · `LatencyStateAdapter` 144 · `PrototypeLoader` 140 · `CraftingMachine` 131 · `Inserter` 126 · `Car` 121.

## 3. Funções-chave documentadas (RVA = base 0x140000000 + rva)

| Função | RVA | Tamanho | Papel | Doc |
|---|---|---:|---|---|
| `MainLoop::run` | 0x151ED0 | 4 941 | arranque, criação da thread de update, ciclo principal | EXECUTION_FLOW §1 |
| `MainLoop::mainLoopStep` | 0x153440 | 1 115 | frame: sync, prepare, render, swap | §2 |
| `MainLoop::gameUpdateStep` | 0x155B10 | 2 420 | um passo da thread de update | §3 |
| `Scenario::updateStep` | 0xD8C300 | 172 | tick (partilhado com benchmark/replay/testes) | §3 |
| `Scenario::update` | 0xD8C8C0 | 652 | CRC, on_tick, GC Lua, Game::update | §3 |
| `Scenario::updateCrcCheck` | 0xD8CB50 | 878 | CRC de desync | NETWORKING §5 |
| `Game::update` | 0xD61F0 | 491 | Map::updateEntities + chart + consistência | §3 |
| `Map::updateEntities` | 0xC6A3A0 | 2 693 | núcleo da simulação | §3 |
| `Surface::update` | 0x1012700 | 2 348 | por superfície: IA, poluição, entidades por tipo | §3 |
| `Chunk::updateEntities` | 0xFF84B0 | 1 181 | entidades ativas por chunk | §3 |
| `TransportLine::update` / `move` | 0x202EF0 / 0x203160 | 618 / 1 188 | belts | ALGORITHMS §2 |
| `Inserter::update` | 0x492AC0 | 1 230 | inserter | EXECUTION_FLOW §5 |
| `ElectricNetwork::update` | 0x33AFF0 | 644 | eletricidade | ALGORITHMS §5 |
| `FluidBox::update` | 0x60B950 | 400 | fluxo de fluidos | ALGORITHMS §6 |
| `PathFinder::update` / `PathFindAlgorithm::update` | 0x1F0900 / 0x1E8EE0 | 1 867 / 1 751 | pathfinding | ALGORITHMS §3 |
| `TrainPathFinder::findPath` | 0x1058620 | 6 834 | rotas de comboio | ALGORITHMS §4 |
| `LogisticNetwork::update` | 0xC15DB0 | 1 035 | robôs | ALGORITHMS §7 |
| `RandomGenerator::getInt` | via `pdb_proc_ranges.tsv` | 119 | taus88 | ALGORITHMS §1 |
| `HeuristicCrcGenerator::get` | 0xC5FD20 | 222 | CRC heurístico | NETWORKING §5 |
| `ThreadPool::run` | 0x1127E10 | 369 | pool de workers | EXECUTION_FLOW §4 |

## 4. Exports
140 nomes (2 flags de GPU híbrida + 138 da API C do Lua). Ver BINARY_ANALYSIS §3.
