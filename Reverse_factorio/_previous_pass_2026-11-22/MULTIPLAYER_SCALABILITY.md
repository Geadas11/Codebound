# MULTIPLAYER_SCALABILITY — Escalabilidade do multijogador

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE] · [CONHECIMENTO PÚBLICO]

## 1. Base
Ver NETWORKING_ANALYSIS.md. Modelo: lockstep determinístico sobre UDP, com servidor a ordenar ações [INFERÊNCIA forte a partir de strings de desync/heartbeat/latency].

## 2. O que escala e o que não escala
| Dimensão | Comportamento esperado | Razão |
|---|---|---|
| Tráfego por jogador | baixo | só ações de input viajam, não o estado [INFERÊNCIA] |
| Tamanho do mapa | **não** afeta tráfego em jogo | estado não é transmitido [INFERÊNCIA] |
| Tamanho do mapa | afeta **join** | o cliente descarrega o save inteiro [CONHECIMENTO PÚBLICO] |
| CPU | cada cliente simula **toda** a fábrica | lockstep [INFERÊNCIA] → o cliente mais lento limita |
| Nº de jogadores | heartbeats crescem linearmente; centenas possíveis | [CONHECIMENTO PÚBLICO] eventos com 500+ jogadores |
| Latência | escondida por `latencyInputHandler` | [CONFIRMADO string] |

## 3. Pontos de falha
- **Desync**: qualquer não-determinismo (mod mal escrito, bug) → `desync-report` [CONFIRMADO string]; o cliente é desligado e reentra.
- **Cliente lento**: não consegue acompanhar os ticks → fica para trás e é expulso/reconecta [INFERÊNCIA].
- **Join com mapas grandes**: tempo de download + carregamento; o servidor continua a correr e o cliente tem de recuperar os ticks em falta [CONHECIMENTO PÚBLICO].

## 4. Configuração do servidor
[CONFIRMADO] `server-settings.json` é referenciado no binário. [CONHECIMENTO PÚBLICO] contém nome, visibilidade, `max_players`, `autosave_interval`, `max_upload_in_kilobytes_per_second`, `minimum_latency_in_ticks`. A string `latency_in_ticks` [CONFIRMADO] é consistente com isto.

## 5. Recomendações
1. Mods: estritamente determinísticos; nada de estado local no control stage.
2. Servidor headless em CPU com bom desempenho single-thread.
3. Autosaves menos frequentes em mapas grandes (o save bloqueia a simulação — [CONHECIMENTO PÚBLICO], async só em Linux).
4. Limitar `on_tick` de mods: o custo multiplica-se por todos os clientes.

## 6. Não verificado
Limites reais de jogadores, MTU, compressão de pacotes, cifragem. Requer captura pcap local autorizada.
