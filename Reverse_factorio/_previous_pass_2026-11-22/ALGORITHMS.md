# ALGORITHMS — Algoritmos identificados

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE] · [CONHECIMENTO PÚBLICO] (Friday Facts/docs da Wube; não verificado no binário)

Nenhum algoritmo abaixo foi lido em código desmontado. Cada entrada indica a evidência que existe.

| # | Algoritmo | Evidência local | Estado |
|---|---|---|---|
| 1 | **Simulação em lockstep determinístico** | strings `desynchronisation: crc test (%s) failed`, `desync on loading` | [INFERÊNCIA forte] |
| 2 | **Verificação CRC do estado** | mesma string "crc test"; algoritmo exato desconhecido (`crc32` não encontrado) | [INFERÊNCIA] existe; tipo [HIPÓTESE] |
| 3 | **Janela de latência / latency hiding** | `latency window`, `latency_in_ticks`, `latencyInputHandler` | [INFERÊNCIA] |
| 4 | **Retransmissão de heartbeats perdidos** | `heartbeat(%u) that we don't have.` | [INFERÊNCIA] |
| 5 | **Colisão por máscaras de camadas** | `collision-mask-util.lua`, 24× `collision-layer` | [INFERÊNCIA] intersecção de bitsets |
| 6 | **Geração de mapa por expressões de ruído** | `noise-expression` 89×, `noise-function` 28×, `autoplace-control` 12×, `resource-autoplace.lua`, `autoplace_utils.lua` | [CONFIRMADO] que o terreno é definido por grafos de expressões declarados em Lua; [INFERÊNCIA] compilados/avaliados pelo motor |
| 7 | **Despacho de eventos agregados** | `event_handler.lua` + `handler.add_lib` | [CONFIRMADO] uso; [INFERÊNCIA] multiplexagem |
| 8 | **Pathfinding de inimigos/unidades** | [CONHECIMENTO PÚBLICO] A* hierárquico em chunks | [HIPÓTESE] local — sem string verificada |
| 9 | **Pathfinding de comboios** | [CONHECIMENTO PÚBLICO] Dijkstra/A* sobre grafo de segmentos de carril | [HIPÓTESE] local |
| 10 | **Correias de transporte** | [CONHECIMENTO PÚBLICO] segmentos fundidos com distâncias entre itens (otimização 0.17) | [HIPÓTESE] local |
| 11 | **Redes elétricas/fluídos** | [CONHECIMENTO PÚBLICO] fluídos 2.0 em segmentos (pipe segments) | [HIPÓTESE] local |
| 12 | **Entidades ativas/adormecidas** | [CONHECIMENTO PÚBLICO] entidades inativas saem da lista de update | [HIPÓTESE] local |
| 13 | **Compressão** | `ZSTD`/`zstd`; saves `.zip` | [CONFIRMADO] referência; uso [INFERÊNCIA] |
| 14 | **Aleatoriedade segura** | import `CryptGenRandom` | [CONFIRMADO] import; [INFERÊNCIA] IDs/seeds não-simulação |
| 15 | **Merge profundo de tabelas** | `core\lualib\meld.lua` | [CONFIRMADO] ficheiro; [INFERÊNCIA] merge recursivo de protótipos |
| 16 | **Pontuação de produção** | `production-score.lua` | [CONFIRMADO] ficheiro; [INFERÊNCIA] valor de itens derivado de receitas |
| 17 | **Geometria 2D/3D** | `math2d.lua`, `math3d.lua` | [CONFIRMADO] ficheiros |

## Pseudo-fluxo do lockstep (INFERÊNCIA, não código real)
```
para cada tick:
    ações = heartbeats confirmados do tick
    se faltar heartbeat → pedir; esperar
    aplicar ações; atualizar simulação
    se tick é ponto de verificação → crc(estado) → comparar
```

## Próximo passo
Ler PDB → encontrar classes (`TransportLine`, `PathFinder`, `CRC`, …) e confirmar/refutar 8–12.
