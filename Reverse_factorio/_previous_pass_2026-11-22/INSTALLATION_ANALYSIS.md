# INSTALLATION ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Distribuição**: GOG (Good Old Games)  
**Instalador**: Hydra  
**Arquitetura**: x86_64  
**Sistema Operacional**: Windows 11 Pro 10.0.26300

## Caminho de Instalação

```
C:\GOG Games\Factorio\
```

## Componentes da Instalação

### 1. Executável Principal
- **Caminho**: `C:\GOG Games\Factorio\bin\x64\factorio.exe`
- **Formato**: PE32+ (Portable Executable 64-bit)
- **Arquitetura**: x86_64
- **SHA256**: `8cfdb7111ebb31d68441113ebdb0165c90cb3723f6cd6902d71339fe2275611f`
- **Tamanho**: ~49.4 MB
- **Rich Header**: Preservado (identificador de compilador Microsoft Visual C++)

### 2. Símbolos e PDB
- **Arquivo**: `C:\GOG Games\Factorio\bin\x64\factorio.pdb`
- **Propósito**: Debug symbols para o compilador MSVC
- **Observação**: O PDB contém símbolos de funções e tipos de dados para debugging

### 3. Dados do Jogo Base (`data\base\`)
Estrutura organizada por categorias:

| Diretório | Conteúdo |
|-----------|----------|
| `prototypes\` | ~80 arquivos Lua definindo entidades, itens, recipes, tecnologias |
| `script\` | Scripts de controle (freeplay, core) |
| `core\` | Engine core modules |
| `locale\` | Localizações/idiomas |
| `sound\` | Definições de som |
| `graphics\` | Recursos gráficos |
| `tilesets\` | Sets de tiles |

### 4. Scripts Lua

#### `data\base\script\freeplay\control.lua`
```lua
local handler = require("event_handler")
handler.add_lib(require("freeplay"))

if script.active_mods["space-age"] then
  handler.add_lib(require("space-finish-script"))
else
  handler.add_lib(require("silo-script"))
end
```
**Função**: Ponto de entrada para o modo freeplay, inicializa mods e gerencia eventos.

#### `data\base\script\freeplay\freeplay.lua`
**Função**: Lógica completa do modo freeplay:
- Configuração de itens iniciais (8 iron plates, 1 wood, pistol, etc.)
- Criação de site de acidência (crash site)
- Interface remota (`freeplay`) para acesso a configurações
- Gestão de eventos do jogador (on_player_created, on_respawn, cutscenes)
- Configuração de vitória e mensagens

### 5. Protótipos (`data\base\prototypes\`)
Mais de 70 arquivos Lua definindo:
- `entity.lua` - Definições de entidades (miners, assemblers, belts, etc.)
- `item.lua` - Definições de itens
- `recipe.lua` - Recipes de crafting
- `technology.lua` - Tecnologias research
- `tile.lua` - Tipos de tiles do mapa
- `fluid.lua` - Fluids (water, oil, etc.)
- `item-groups.lua` - Grupos de itens
- `item-tints.lua` - Tintas para itens

### 6. Configurações do Usuário
- `config-path.cfg` - Caminho de configuração
- `goggame-1238653230.info` - Metadata GOG
- `goggame-1238653230.hashdb` - Hash database GOG

## Distinção GOG vs Hydra vs Factorio

### Componentes Identificados como Factorio Puro
- `bin\x64\factorio.exe` - Motor do jogo
- `data\base\*` - Dados do jogo base
- `data\base\info.json` - Versão 2.1.24

### Possível Influência Hydra (não investigada detalhamente)
- `Launch\ Factorio.lnk` - Atalho de lançamento
- `gog.ico` - Ícone GOG
- `goggame-1238653230.hashdb` - Banco de dados de hash GOG
- `goggame-1238653230.info` - Metadados GOG

**Nota**: O Hydra launcher não foi investigado. Este relatório foca apenas nos arquivos do Factorio na pasta `C:\GOG Games\Factorio\`.

### Verificação de Versão

Confirmado: **Factorio 2.1.24**
- Fonte: `data\base\info.json` → `"version": "2.1.24"`
- Base Mod by Wube Software
- Compatível com formato de save games 2.1.x

## Estrutura de Arquivos Importantes

```
C:\GOG Games\Factorio\
├── bin/
│   └── x64/
│       ├── factorio.exe     (49.4 MB, PE32+)
│       └── factorio.pdb     (símbolos debug)
├── data/
│   └── base/
│       ├── info.json        (versão 2.1.24)
│       ├── core/
│       ├── prototypes/      (~70 arquivos Lua)
│       ├── script/          (Lua scripts)
│       ├── locale/
│       ├── sound/
│       ├── graphics/
│       └── ...
├── config-path.cfg
├── Launch\
│   └── Factorio.lnk
├── doc-html/
├── goggame-1238653230.hashdb
├── goggame-1238653230.info
├── unins000.dat   (desinstalador)
├── unins000.exe   (desinstalador)
├── unins000.ini
├── gog.ico
└── support.ico
```

## Observações Técnicas

1. **Formato de Arquivo**: Todos os arquivos de definição são Lua legível (text-based)
2. **Motor**: O executável utiliza dependências do Microsoft Visual C++ Runtime (constatado pelo Rich header)
3. **Não modificado**: A instalação original foi preservada - nenhum arquivo foi modificado, deletado ou substituído
4. **Compatibilidade**: Estrutura de diretórios idêntica à versão Steam do Factorio 2.1.24
5. **Limitations**: Análise dinâmica limitada no Windows (ferramentas REA como pwntools suportam Linux)

## Próximos Passos

1. `FILE_STRUCTURE.md` - Inventário completo e categorizado de todos os arquivos
2. `LUA_ANALYSIS.md` - Análise detalhada dos scripts Lua e protótipos
3. `SOFTWARE_ARCHITECTURE.md` - Arquitetura de software completa
4. Continuação da análise dos subsistemas...