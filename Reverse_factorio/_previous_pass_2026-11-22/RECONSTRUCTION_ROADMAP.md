# RECONSTRUCTION_ROADMAP — Plano de reconstrução

**Objetivo realista:** um motor *inspirado* na arquitetura do Factorio (simulação determinística em grelha + mods Lua), escrito de raiz. Não copiar código, assets, nome ou aparência do Factorio — isso seria uma recriação de um jogo existente e viola os direitos da Wube.

## Fase A — Fechar lacunas de evidência (só leitura)
1. Ler `factorio.pdb` → inventário de funções/classes (FUNCTION_INVENTORY.md).
2. Listar exports e validar a assinatura.
3. Correr REA (`search_strings`, `xrefs`) para rede, save, CRC.
4. Documentar `data\core\lualib\*.lua` (APIs de utilitários) e `collision-mask-defaults.lua`.
5. Atualizar os 13 relatórios antigos com etiquetas CONFIRMADO/INFERÊNCIA/HIPÓTESE.

## Fase B — Núcleo determinístico
| Componente | Requisito-chave | Fonte de evidência |
|---|---|---|
| Tick fixo (60 UPS — [CONHECIMENTO PÚBLICO]) | `timeBeginPeriod` confirmado | DEPENDENCIES |
| Posições em ponto fixo | evitar divergência float | PHYSICS (HIPÓTESE) |
| Grelha de chunks | geração e update por região | ALGORITHMS #6 |
| Colisão por máscaras | bitset por camada | PHYSICS §2 |
| CRC do estado | deteção de desync | NETWORKING §2 |

## Fase C — Dados e scripting
- Lua 5.2 embutido, 3 estágios (settings/data/control).
- `data.raw` → conversão para structs C++ imutáveis.
- Eventos + `storage` serializável; `remote` interfaces; `event_handler` estilo multiplexador.

## Fase D — Sistemas de jogo (design próprio)
Correias, braços, máquinas, redes elétricas, fluídos, comboios, robôs, inimigos. Cada um com design original; usar os relatórios só como referência de *arquitetura*.

## Fase E — Rede
UDP lockstep cliente-servidor, heartbeats por tick, latency hiding, transferência de save no join, CRC periódico.

## Fase F — Render
Backend abstrato com D3D11/OpenGL/Vulkan (o Factorio carrega os três dinamicamente — CONFIRMADO por strings). Sprite batching 2D.

## Fase G — Save
Zip com estado nativo + estado Lua separados; migrações por versão.

## Estimativa
Equipa pequena: anos (o Factorio tem ~100k funções com unwind — INFERÊNCIA por `.pdata`). Um protótipo jogável mínimo (correias + mineração + fornos + Lua) é viável em semanas.

## Ferramentas sugeridas
Unity é possível para um protótipo, mas o determinismo exige evitar física/float do motor; C++ ou Rust com motor próprio aproxima-se mais da arquitetura observada.
