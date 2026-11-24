# UNKNOWN_COMPONENTS — O que permanece desconhecido

Cada entrada indica porque está aberta e como a fechar **sem alterar a instalação**. Atualizado a 2026-11-24 com a descompilação Ghidra (**GHIDRA_FINDINGS.md**).

**Balanço:** 🟢 15 resolvidas · 🔴 2 abertas (de 17); as 2 abertas exigem correr o jogo.

| # | Componente / pergunta | Estado | Resposta / método restante |
|---|---|---|---|
| 1 | Posição de `Map::runUpdate(managers)` em `Map::updateEntities` e ordem entre managers | 🟢 | 4 lotes: belts → (superfícies, entidades) → torres → (comboios, forças, jogadores) → circuitos de controlo → {elétrica, calor, fluxo de fluidos, circuitos}. GHIDRA_FINDINGS §1 |
| 2 | O que fazem `prepare/finish`; como o paralelo é determinístico | 🟢 | work-stealing por contador atómico + ações adiadas por thread, aplicadas por merge ordenado pelo índice do grupo. §2 |
| 3 | Equações de movimento | 🟢 | comboio, robô, personagem (sub-passos e fórmula da velocidade) e carro (sin/cos polinomial próprio, energia de impacto). GHIDRA_FINDINGS §5 |
| 4 | Constantes `static const` | 🟢 | 29 751 constantes com valor, extraídas do PDB → `artifacts/pdb/pdb_constants.tsv`. §13 |
| 5 | Fórmula da distribuição elétrica e do fluxo de fluidos | 🟢 | cascata gulosa por prioridade em `double`; fluxo = 0,8·min(cap)·min(fOrig, 1−fDest), mínimo 0,1. §3–4 |
| 6 | Formato binário de `Entity::save` e do cabeçalho de `level.dat` | 🟢 | cabeçalho completo (os 20 bytes atribuídos); `save` das 139 classes → `artifacts/entity_save_formats.md` (ordem e tipo dos blocos; tamanhos byte a byte nos `.c`). GHIDRA_FINDINGS §16–17 |
| 7 | Formato dos pacotes UDP | 🟢 | byte de tipo e flags, messageID, fragmento, confirmações; sem cifra nem MAC. §6 |
| 8 | Polinómio e semântica do CRC | 🟢 | CRC-32 padrão; heurístico = contagens globais + amostra rotativa de jogadores e redes. §7 |
| 9 | Uso do zstd | 🟢 | blueprints, UDP de scripts e comandos remotos; não no save. §8 |
| 10 | Modificações ao núcleo do Lua 5.2 | 🟢 | matemática própria (`custom_*`), sem corrotinas/io/os/package, tabelas com ordem de inserção, GC com lista dupla, `gcblocked`, verificador de bytecode e exceções C++. Comparação instrução a instrução da `lvm.c` não feita. §15, §18 |
| 11 | Funções retiradas ou alteradas nas libs Lua | 🟢 | lista completa das libs e funções; `debug` parcial = `getinfo` + `traceback`; extras `table.pairs_concat` e `string.pack`. §9 |
| 12 | Interpolação visual entre ticks | 🟢 | não existe: o render desenha o último estado (evidência por ausência em `GameRenderer::prepare`, `DrawEngine::prepare` e símbolos). §12 |
| 13 | `RenderLayer::Enum` nome↔valor | 🟢 | 97 camadas no PDB → `artifacts/render_layers.txt`. §14 |
| 14 | Formato 2.1.24 vs 2.0.60 | 🔴 | requer criar um save numa cópia isolada da 2.1.24 (com autorização) |
| 15 | Integração Steam na build GOG | 🟢 | só existe a interface base com métodos vazios (`isSteamApiInitialized` → false). §10 |
| 16 | Papel do OpenCL | 🟢 | tipo declarado, nenhuma função compilada: código morto. §10 |
| 17 | Comportamento em runtime da 2.1.24 | 🔴 | **não executada**; ver RECONSTRUCTION_ROADMAP §5 (protocolo isolado) |

Itens da passagem anterior **resolvidos** nesta sessão: lista de exports (#2 antigo), cliente HTTP/TLS (curl + Schannel), algoritmo do CRC (crc32fast), representação de posições (FixedPoint 24.8), belts (TransportLine com gaps), backend gráfico por omissão (Direct3D 11 [R/2.0.60]), ausência de Box2D/PhysX/Bullet, validade Authenticode, versão do Lua (5.2 modificado), ChromaAppInfo.xml (Razer Chroma), pastas DLC (só base+core), RNG (taus88).
