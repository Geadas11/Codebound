# EXECUTIVE SUMMARY - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: Factorio 2.1.24 (GOG/Hydra)  
**Arquitetura**: x86_64  
**Executável Principal**: `C:\GOG Games\Factorio\bin\x64\factorio.exe`  
**SHA256 Executável**: `8cfdb7111ebb31d68441113ebdb0165c90cb3723f6cd6902d71339fe2275611f`  
**Tamanho Executável**: ~49.4 MB

## Visão Geral da Instalação

Esta é uma distribuição **GOG** do Factorio instalada via **Hydra**. O diretório principal é:

```
C:\GOG Games\Factorio\
├── bin\x64\ factorio.exe (PE32+ executable)
├── data\ base\ - dados do jogo
│   ├── prototypes\ - definições Lua
│   ├── script\ - scripts de controle
│   ├── core\ - engine core
│   └── ...
└── config\ - configurações do usuário
```

### Componentes Identificados

| Componente | Caminho | Descrição |
|------------|---------|-----------|
| Executável | `bin\x64\factorio.exe` | Motor principal, PE32+, x86_64 |
| Info do Mod | `data\base\info.json` | Versão 2.1.24, Base Mod |
| Scripts Lua | `data\base\script\freeplay\` | control.lua, freeplay.lua |
| Protótipos | `data\base\prototypes\` | Múltiplos arquivos Lua por categoria |

### Arquitetura do Software

O Factorio utiliza uma arquitetura de duas camadas distintas:

1. **Motor Compilado (C++)**: O executável `factorio.exe` gerencia:
   - Simulação de jogo (tick-based)
   - Renderização (Direct3D 11)
   - Física e colisões
   - Networking e multiplayer
   - Sistema de saves
   - Pathfinding e IA

2. **Camada Lua**: Scripts de jogo e lógica de mods que se comunicam com o motor através de:
   - API bindings definidas
   - Eventos e callbacks
   - Prototype registration functions

A comunicação entre camadas segue um padrão bem definido onde o motor registra protótipos (entidades, itens, recipes) que então são acessíveis via `prototypes` na Lua.

### subsistemas Principais (Identificados)

1. **Simulação** - sistema tick-based, gestão de entidades, AI de inimigos, pathfinding
2. **Rendering** - Direct3D 11, sprites, partículas, interface
3. **Física/Colisões** - bounding boxes, collision masks, trigger volumes
4. **Logistics** - transportadores (belts), inserters, rede de logística, logistics network
5. **Produção/Crafting** - recipes, assembler machines, fluid handling
6. **Multiplayer/Networking** - protocolo client-server, sincronização, saves
7. **Modding API** - entity prototype registration, event handlers, custom commands

### Descobertas Técnicas REA (iniciais)

- ✅ Executável identificado como PE32+ com arquitetura x86_64
- ✅ SHA256 confirmado: `8cfdb7111ebb31d68441113ebdb0165c90cb3723f6cd6902d71339fe2275611f`
- ✅ Estrutura de diretórios compatível com versão Steam (mesmo layout)
- ✅ Arquivos Lua são texto legível, facilmente analisáveis
- ✅ Directory `data\base\prototypes\` contém ~80 arquivos de definição
- ⚠️ Análise dinâmica limitada em Windows (pwntools só suporta Linux)
- ⚠️ Necessário verificar componentes Hydra vs Factorio puros

### Versão Confirmada

- Factorio 2.1.24 (confirmado via `data\base\info.json`)
- Base Mod by Wube Software
- Nenhum mod adicional detectado na instalação base

### Premissas e Limitações

- Análise baseada em arquivos estáticos (não executado)
- Windows limita algumas capacidades REA (pwntools Linux-only)
- Distribuição GOG pode ter pequenas diferenças de versão vs Steam
- Hydra launcher NÃO foi investigado (fora escopo - apenas Factorio files)

### Próximos Passos Documentados

1. `INSTALLATION_ANALYSIS.md` - Análise detalhada da instalação GOG
2. `FILE_STRUCTURE.md` - Inventário completo de todos os arquivos
3. `SOFTWARE_ARCHITECTURE.md` - Arquitetura de software detalhada
4. `ENGINE_ANALYSIS.md` - Análise do motor de jogo
5. `BINARY_ANALYSIS.md` - Análise do executável (REA limited)
6. `LUA_ANALYSIS.md` - Análise da implementação Lua
7. `SUBSYSTEMS.md` - Cada subsistema detalhado
8. `DATA_STRUCTURES.md` - Estruturas de dados internas
9. `EXECUTION_FLOW.md` - Fluxo de execução tick-by-tick
10. `RENDERING_ANALYSIS.md` - Rendering pipeline
11. `PHYSICS_ANALYSIS.md` - Física e colisões
12. `SIMULATION_ANALYSIS.md` - Motor de simulação
13. `LOGISTICS_ANALYSIS.md` - Logistics system
14. `MODDING_API.md` - API e integração Lua
15. `NETWORKING_ANALYSIS.md` - Multiplayer e networking
16. `SAVE_SYSTEM.md` - Sistema de saves
17. `DEPENDENCIES.md` - Dependências e bibliotecas
18. `FUNCTION_INVENTORY.md` - Inventário de funções
19. `ALGORITHMS.md` - Algoritmos identificados
20. `ARCHITECTURE_DIAGRAMS.md` - Diagramas arquiteturais
21. `REA_FINDINGS.md` - Descobertas REA
22. `UNKNOWN_COMPONENTS.md` - Componentes não identificados
23. `RECONSTRUCTION_ROADMAP.md` - Roadmap para reconstrução futura