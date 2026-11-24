# PERFORMANCE_ANALYSIS — Desempenho

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE] · [CONHECIMENTO PÚBLICO]

Nenhuma medição em runtime foi feita (o jogo não foi executado). Tudo abaixo é estático.

## 1. Evidência estática
| Facto | Estado | Implicação |
|---|---|---|
| `timeBeginPeriod`/`timeEndPeriod` | [CONFIRMADO] | Timer de 1 ms para manter o tick fixo [INFERÊNCIA] |
| `SubmitThreadpoolWork`, `CloseThreadpoolWork`, `InitializeSListHead`, `InterlockedPushEntrySList`, SRW locks | [CONFIRMADO] | Trabalho paralelo via thread pool do Windows e estruturas lock-free [INFERÊNCIA] |
| `GetTickCount64` | [CONFIRMADO] | Medição de tempo |
| 20 DLLs importadas, 33 MB de `.text` | [CONFIRMADO] | Ligação estática, LTO provável [HIPÓTESE] |
| Configuração `VanillaFinalRelease` | [CONFIRMADO] (caminho PDB) | Build otimizada sem asserts de debug [INFERÊNCIA] |
| D3D11/OpenGL/Vulkan dinâmicos | [CONFIRMADO strings] | Escolha de backend por máquina |

## 2. Princípios de desempenho (fontes públicas, não verificados no binário)
- [CONHECIMENTO PÚBLICO] 60 UPS fixos; a métrica é UPS, não FPS.
- [CONHECIMENTO PÚBLICO] Entidades inativas "dormem" e saem da lista de update.
- [CONHECIMENTO PÚBLICO] Correias otimizadas em segmentos; fluídos 2.0 em segmentos.
- [CONHECIMENTO PÚBLICO] Paralelismo em partes da atualização (ex.: redes elétricas, render preparado em threads).

## 3. Custos do lado Lua [INFERÊNCIA]
- `on_tick` em mods é o maior risco: corre em todos os clientes (lockstep).
- `on_nth_tick` e filtros de eventos reduzem trabalho.
- `storage` grande → `script.dat` grande → saves e joins mais lentos.
- O `event_handler` adiciona um nível de despacho por evento — custo baixo.

## 4. Dados
[CONFIRMADO] 7 770 ficheiros em `data\base`, 1 272 em `data\core`. [INFERÊNCIA] o arranque é dominado por carregamento de sprites e execução da fase data; ZSTD pode servir a cache.

## 5. Recomendações para mods
1. Evitar `on_tick`; usar `on_nth_tick` ou eventos filtrados.
2. Guardar referências em `storage` em vez de `find_entities_filtered` repetido.
3. Distribuir trabalho por ticks (round-robin).
4. Manter `storage` pequeno e sem duplicação.

## 6. Próximos passos
Medição real: correr o jogo com `--benchmark` sobre uma **cópia** de save numa pasta temporária (requer autorização do utilizador).
