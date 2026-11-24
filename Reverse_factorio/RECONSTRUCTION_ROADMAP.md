# RECONSTRUCTION_ROADMAP — Orientações para reconstrução independente

> Objetivo: um **motor original** com uma arquitetura inspirada na que foi observada (um "factory game engine"). Não se trata de recriar o Factorio: nome, assets, conteúdo e aparência são da Wube e não fazem parte deste plano. O que se reutiliza são **ideias de arquitetura**, documentadas aqui com evidência.

## 1. Decisões que a evidência recomenda (por ordem de importância)
1. **Determinismo desde o dia 1**: posições em ponto fixo 24.8 (`int32`), fluidos em `int64` de ponto fixo (o original usa `double` no balanço elétrico, aceitável por ser IEEE estrito com SSE2; ponto fixo é mais seguro num motor novo), RNG próprio guardado no estado (taus88 é suficiente e cabe em 12 bytes), iteração com ordem estável, sem `HashMap` não ordenado no estado. Ver DATA_STRUCTURES §1, ALGORITHMS §1.
2. **Tick fixo + duas threads**: update numa thread, render noutra, com troca sincronizada e buffer duplo dos dados de render. Ver EXECUTION_FLOW §2–3.
3. **Input como única forma de mudar o estado**: todas as ações do jogador são `InputAction` com tick-alvo. O mesmo caminho serve single-player, multiplayer lockstep, replays e testes, como o `Scenario::updateStep` partilhado mostra.
4. **Trabalho proporcional à atividade**: listas de entidades ativas, sleep/wake por listas de interessados, ativação por chunk.
5. **Agregar estruturas em grafo**: belts colineares numa linha com itens em distâncias relativas e gaps; canos num segmento com um buffer; eletricidade somada por categorias de prioridade.
6. **Paralelizar só os sistemas em grafo** com `prepare` (sequencial) → `update` (paralelo, sem escrita partilhada) → `finish` (sequencial, ordenado).
7. **Protótipos imutáveis vindos de dados** (`XPrototype` + IDs de 16 bits) e instâncias que só guardam ponteiro ou ID.
8. **Scripting sandboxed**: Lua (ou Luau) sem IO/OS, `require` só na carga, RNG determinístico, GC com passo fixo por tick, objetos expostos como proxies fracos.
9. **CRC do estado por serialização** para detetar desync. Reutiliza o serializador do save.

## 2. Mapa de módulos sugerido (mínimo viável)
| Fase | Módulo | Inspiração (evidência) |
|---|---|---|
| 1 | `core/fixed`, `core/rng`, `core/serialize` | FixedPointNumberTemplate, RandomGenerator, Serialiser |
| 1 | `world/map, surface, chunk(32×32)`, `entity` base + `UpdatableEntity` | DATA_STRUCTURES §2–3 |
| 2 | `belt/transport_line` (gaps), `inserter` (polar), `crafting_machine` (sleep reason) | LOGISTICS §1–2, SIMULATION §5 |
| 2 | `energy/electric` (prioridades), `fluid/segment` | ALGORITHMS §5–6 |
| 3 | `save` (ZIP + fatias comprimidas em paralelo) | SAVE_SYSTEM |
| 3 | `script` (Lua sandbox, eventos, proxies) | LUA_ANALYSIS, MODDING_API |
| 4 | `net` (lockstep UDP, heartbeats, CRC) | NETWORKING_ANALYSIS |
| 4 | `pathfinding` hierárquico (componentes por chunk + A*) | ALGORITHMS §3 |
| 5 | render com DrawQueue por layer, atlases | RENDERING_ANALYSIS |

## 3. Escolha de tecnologia
- **C++ ou Rust**: o mais próximo da arquitetura observada (controlo de memória, listas intrusivas, pools, ponto fixo sem custo).
- **Unity/Godot**: viáveis para protótipo **se a simulação ficar fora do motor**: uma biblioteca de simulação pura (C#/C++ com ponto fixo) e o motor só como camada de render/input. Física, `float` e `Update()` do motor não são determinísticos entre máquinas e não devem tocar no estado.
- Compatibilidade binária com saves/mods do Factorio: **não recomendada nem coberta**. Os formatos estão só parcialmente decodificados (UNKNOWN_COMPONENTS #6, #7), e o objetivo é um motor independente.

## 4. Testes que a arquitetura observada sugere
- **Teste de replay**: correr N ticks a partir do mesmo estado + o mesmo log de inputs em duas máquinas ou com 1 vs N threads e comparar CRCs (equivalente a `MainTasks::runReplay`, `checkHeavyModeCrc`).
- **Teste de consistência incremental** (como `ConsistencyScraper::step`): invariantes verificados alguns objetos por tick.
- **Benchmark headless** (como `MainTasks::benchmark`): ticks/s num mapa fixo, sem render.

## 5. Protocolo seguro para análise dinâmica futura do Factorio (não executado)
Para fechar as lacunas de runtime sem tocar na instalação nem no `%APPDATA%`:
1. Copiar `C:\GOG Games\Factorio` para um diretório temporário. A cópia é a única coisa executada.
2. Na cópia, `config-path.cfg` → `use-system-read-write-data-directories=false`, para que todos os dados fiquem dentro da cópia.
3. Correr em modo headless: `factorio.exe --create teste.zip`, `--benchmark teste.zip --benchmark-ticks 600`, `--start-server teste.zip` (sem rede pública), e capturar o tráfego localmente.
4. Usar o REA (`capture_process_scenario`, `inspect_pcap_capture`) e comparar com o estático.
**Requer autorização explícita** antes de executar.
