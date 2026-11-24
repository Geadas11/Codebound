# EXECUTIVE_SUMMARY — Resumo da investigação

**Alvo:** Factorio **2.1.24** (build 87808, GOG), `C:\GOG Games\Factorio`, analisado só em leitura. Nenhum ficheiro original foi alterado; os saves foram analisados sobre cópias.
**Fonte principal de evidência:** o `factorio.pdb` (461 MB) distribuído com o jogo. É um PDB completo (tipos, membros, offsets, fontes) e o GUID coincide com o do exe. Complementado por REA (`inspect_pe_resources`), parsers próprios só de leitura, leitura direta das chamadas nos bytes do exe, ficheiros Lua/JSON de `data/` e `doc-html/`, e um save e um log de uma instalação 2.0.60 que partilha o `%APPDATA%`.

## Principais conclusões
1. **Motor próprio em C++**, um único executável (MSVC 14.44) com 29 bibliotecas estáticas (SDL3, Lua 5.2 modificado, curl/Schannel, zlib, zstd, minizip, re2, opus/vorbis, Agui…). Sem motor comercial nem motor de física. [C]
2. **7 128 ficheiros-fonte em 68 módulos**, recuperados do PDB, com 249 292 funções e 110 798 tipos. A maior parte do código é API Lua, GUI e entidades. [C]
3. **Simulação determinística por ticks**, numa thread de update separada do render (mutex + condvar + buffer duplo). Usa ponto fixo 24.8 nas posições, ponto fixo 2⁻²⁴ nos fluidos, `double` no balanço elétrico [C/G], RNG taus88 no estado e um passo de GC Lua por tick. [C]
4. **Ordem do tick reconstruída a partir das chamadas reais**: input → CRC → `on_tick` → GC Lua → `Game::update` → `Map::updateEntities` → por superfície: relâmpagos, pathfinding, IA, poluição e entidades ativas **agrupadas por tipo numa ordem fixa**. [C] (EXECUTION_FLOW)
5. **Escala por atividade e agregação**: entidades adormecem e são acordadas por listas de interessados. Belts colineares são uma só linha com itens em distâncias relativas, canos são um segmento com buffer único, e a eletricidade é resolvida por categorias de prioridade. 7 sistemas em grafo atualizam **em paralelo** (prepare/update/finish). [C/I]
6. **Multiplayer lockstep** sobre UDP: as ações do jogador (357 tipos) agrupadas por tick em `TickClosure`, com o servidor a ordená-las, CRC do estado serializado para detetar desync e camada de previsão local para esconder a latência. [C/I]
7. **Lua tem duas fases**: a *data stage* cria protótipos imutáveis e a *control stage* tem um estado Lua por mod, com eventos (200+) e proxies fracos para objetos C++ (4 732 funções ligadas). O Lua não tem `io`/`os`/`package`/`coroutine`, e `require` só funciona na carga. **Nenhum sistema de simulação está implementado em Lua.** [C]
8. **Save** = ZIP com o mapa em fatias de 1 MiB comprimidas com zlib **em paralelo**, mais `script.dat` (Lua), `level-init.dat` (base de replay) e checksums dos mods. [R/2.0.60 + C]

## Relatório de cobertura

| Grau | Componentes |
|---|---|
| 🟢 **Compreendidos** (estrutura + fluxo com evidência direta) | identificação, instalação e dependências; arquitetura de módulos; ciclo principal e threads; ordem do tick (chamadas diretas); estruturas de dados centrais; belts; inserters; eletricidade (fluxo); fluidos (modelo); RNG; contentor de save; protocolo multiplayer (mensagens/estados); camada Lua e binding; API de modding |
| 🟢 **Compreendidos após descompilação** (GHIDRA_FINDINGS) | managers paralelos e o seu determinismo; fórmulas da eletricidade e dos fluidos; física dos comboios, personagem, robôs e carro; cabeçalho UDP; CRC de desync; libs Lua, `pairs` e matemática determinísticos; formato de save de todas as entidades; constantes; camadas de render; zstd; Steam/OpenCL |
| 🟡 **Parcialmente compreendidos** (estrutura confirmada, lógica inferida) | pathfinding (algoritmo, sem custos exatos); comboios; robôs; crafting; rendering (pipeline, sem sincronização fina); física/colisões (geometria, sem equações); geração de mundo; poluição/IA; circuitos |
| 🟠 **Só inventário** | calor, pesquisa/forças, Space Age, undo/redo, blueprints (formato), som, input |
| 🔴 **Não investigados / não investigáveis aqui** | comportamento em runtime da 2.1.24 (o jogo não foi executado, por preservação); tráfego de rede real; tamanho byte a byte de cada campo de entidade (só nos `.c`); comparação instrução a instrução de `luaV_execute` |

Lista completa e método para fechar cada lacuna: **UNKNOWN_COMPONENTS.md**.

## Limitações
- **Ghidra**: o primeiro projeto (`ghidra/`, com tipos do PDB) ficou horas preso a alinhar estruturas. O segundo (`ghidra2/`) aplicou só os nomes das funções a partir do nosso parser e completou a análise em 35 min. Daí vêm 74 funções descompiladas em `artifacts/decomp/`. O pseudocódigo **não tem tipos**: os campos foram resolvidos à mão com os layouts do PDB.
- **REA**: depois de lhe aumentar o tempo e a memória, a análise completou-se. 7 funções-chave têm Evidence REA nativa (`artifacts/rea_evidence_bundle.json`), idêntica à descompilação própria (REA_FINDINGS §6).
- O runtime vem de **outra instalação** (2.0.60 Steam + Space Age). O formato pode diferir na 2.1.24.

## Mapa dos documentos
INSTALLATION_ANALYSIS · FILE_STRUCTURE · BINARY_ANALYSIS · DEPENDENCIES · SOFTWARE_ARCHITECTURE · ENGINE_ANALYSIS · **EXECUTION_FLOW (canónico do tick)** · SIMULATION_ANALYSIS · SUBSYSTEMS (índice) · DATA_STRUCTURES · ALGORITHMS · LOGISTICS_ANALYSIS · PHYSICS_ANALYSIS · RENDERING_ANALYSIS · LUA_ANALYSIS · MODDING_API · NETWORKING_ANALYSIS · SAVE_SYSTEM · FUNCTION_INVENTORY · ARCHITECTURE_DIAGRAMS · **GHIDRA_FINDINGS** · REA_FINDINGS · UNKNOWN_COMPONENTS · RECONSTRUCTION_ROADMAP · PROGRESS.
