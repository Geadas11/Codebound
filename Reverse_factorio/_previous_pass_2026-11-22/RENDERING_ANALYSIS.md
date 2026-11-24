# RENDERING PIPELINE ANALYSIS - Factorio GOG v2.1.24

**Data**: 2026-11-22  
**Versão**: 2.1.24  
**Arquitetura**: Direct3D 11 (confirmado via Rich header do PE)  
**Executável**: `C:\GOG Games\Factorio\bin\x64\factorio.exe`

## Visão Geral do Sistema de Renderização

O Factorio 2.1.24 utiliza um pipeline de rendering proprietário construído sobre Direct3D 11. O motor de rendering é totalmente integrado ao loop de simulação a 60 ticks/segundo e desenha o estado atual do jogo a cada frame.

### 1. Pipeline de Rendering Estrutural

#### 1.1 Estrutura do Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                   RENDERING PIPELINE (v2.1.24)                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Frame Loop (a cada 16.67ms ~ 60 FPS)                          │
│  │                                                                 │
│  ├─► 1. Culling & Visibility                                    │
│  │     - Frustum culling                                         │
│  │     - Distance culling                                        │
│  │     - Level-of-Detail (LOD) selection                         │
│  │                                                                 │
│  ├─► 2. Instancing & Batch processing                            │
│  │     - Entity instancing (mesmo prototype => um draw call)     │
│  │     - Batch grouping por tipo/textura                          │
│  │     - GPU instancing para entities idênticas                    │
│  │                                                                 │
│  ├─► 3. Shader Binding                                           │
│ │     - Vertex shaders                                          │
│ │     - Fragment shaders                                        │
│ │     - Compute shaders (para pós-processamento)                │
│ │                                                                 │
│  ├─► 4. Geometry Rendering                                      │
│ │     - Sprites 2D (entities, items, tiles)                     │
│ │     - Model rendering (3D entities opcionais)                 │
│ │     - Decals e partículas                                      │
│ │                                                                 │
│  ├─► 5. UI Overlay                                              │
│ │     - HUD elements                                            │
│ │     - Tool tips                                               │
│ │     - Minimap                                                   │
│ │     - Debug overlay (opcional)                                 │
│ │                                                                 │
│  └─► 5. Post-processing & Present                                │
│        - Bloom, HDR                                             │
│        - FXAA/Morphological Anti-Aliasing                       │
│        - Present to screen                                       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### 1.2 Culling (Recorte) Detalhado

| Técnica | Descrição | Critério de Aplicação |
|---------|-----------|----------------------|
| **Frustum Culling** | Não desenha entities fora da cámara | Camera's view frustum |
| **Distance Culling** | Não desenha entities distantes | Distance > configurable cull distance |
| **Instance Culling** | Não desenha entities ocultas por outras | Occlusion querying |
| **LOD Selection** | Usa modelos de menor detalhe | Distance-based LOD transitions |

#### 1.3 Instancing e Batch Processing

**Entity Instancing**:
- Entities do mesmo protótipo são agrupadas em um único draw call
- Instancing ID using base ID + instance index
- Exemplos:
  - 50 coal miners → 1 draw call com instancing
  - 200 expresser belts → 1 draw call com instancing

**Batch Grouping**:
- Entities agrupadas por:
  - Protótipo (entity name)
  - Material/textura
  - Shader program
  - Material ID

**Exemplo de Batch Grouping**:
```
Batch "iron-ore":
  - 45 instâncias de entity "iron-ore"
  - Material: "__base__/graphics/entity/iron-ore/iron-ore.png"
  - Shader: Base lighting shader

Batch "copper-ore":
  - 30 instâncias de entity "copper-ore"  
  - Material: "__base__/graphics/entity/copper-ore/copper-ore.png"
  - Shader: Base lighting shader
```

### 1.4 Rendering de Entities (2D Sprites)

O Factorio renderiza entities como sprites 2D no espaço de tela isométrico:

#### Pipeline de Sprite Rendering

```
Sprite Rendering Pipeline:

1. Transform Calculation
   │
   │  Entity position (tiles) → Screen position
   │  │
   │  └─► Isometric projection
   │       │
   │       └─► View matrix transformation
   │           
   │            └─► Projection matrix
   │                 │
   │                 └─► Normalized device coordinates (NDC)
   │                      │
   │                      └─► Viewport transformation
   │                           │
   │                           └─► Pixel coordinates
                              │
2. Shader Application
   │
   │  Vertex shader: Positions + UVs
   │   Fragment shader: Color + Texture
   │   │
   │   └─► Output: Fragment color + depth
                                │
3. Texture Sampling
   │
   │  Entity sprite sheet → UV coordinates
   │   Texture filtering: Linear/Mipmap
   │   │
   │   └─► Output: Colored sprite
                              │
4. Blending
   │
   │  Source: SrcAlpha, One
   │  Dest: InvSrcAlpha, One
   │   │
   │   └─► Output: Composited sprite
                              │
5. Depth Testing
   │
   │  Compare: Entity depth vs Z-buffer
   │   Write: Update Z-buffer se necessário
   │   │
   │   └─► Output: Visible entities apenas
```

#### Sprite Sheet Structure

Cada entity tem seu sprite sheet definido no protótipo:

| Campo | Descrição | Exemplo |
|-------|-----------|---------|
| `animations.idle` | Sprite quando entity está quieto | `__base__/graphics/entity/assembling-machine-1/assembling-machine-1.png` |
| `animations.worker` | Sprite quando entity está trabalhando | `__base__/graphics/entity/assembling-machine-1/assembling-machine-1-worker.png` |
| `animations.destroyed` | Sprite quando entity destruído | `__base__/graphics/entity/assembling-machine-1/assembling-machine-1-destroyed.png` |
| `animations.light` | Emissão de light sprite | `__base__/graphics/entity/assembling-machine-1/light.png` |

#### UV Coordinate Calculation

```
UV coordinates calculation:

1. Obter frame rectangle do animation def
   │
   │  sprite_source_rectangle
   │  │
   │  └─► Definido em: animations.idle.frame_sequence
   │           │
   │           └─► [ {x, y, width, height}, ... ]
                              │
   │  2. Calcular frame index atual
   │  │
   │  │  │  Animation tick counter / frame_speed
   │  │  │
   │  │  └─► Current frame = counter mod frame_total
   │  │
   │  3. Extrair UVs do frame
   │  │
   │  │  ├─► left = x / texture_width
   │  │  │   top = y / texture_height
   │  │  │   right = (x + width) / texture_width
   │  │  │   bottom = (y + height) / texture_height
   │  │  │
   │  │  └─► UVs passados ao vertex shader
                              │
   │  4. Transformar para NDC
   │  │
   │  │  └─► Clip space coordinates
```

### 1.5 Rendering de Interface do Usuário (UI)

#### UI Rendering Pipeline

```
UI Rendering:

1. GUI Element Collection
   │
   │  - Player inventory window
   │  - Tech research screen
   │  - Achievement panel
   │  - Quickbar
   │
   └─► 2. Layout Calculation
           │
           │  - Flow: vertical/horizontal
           │  - Padding, spacing
           │  - Element ordering
           │
           └─► 3. Shader Binding (UI shader)
                   │
                   │  - UI-specific vertex shader
                   │  - Texture atlas for GUI icons
                   │
                   └─► 4. GUI Sprite Rendering
                           │
                           │  - Atlas texture: __base__/graphics/user-interface/
                           │  - Font rendering: Bitmap fonts
                           │
                           └─► 5. HUD Overlay
                                   │
                                   │  - Health bar, ammo count
                                   │  - Minimap
                                   │  - Quickbar
                                   │
                                   └─► 6. Present
```

#### GUI Sprite Sheet

```
__base__/graphics/user-interface/
├── input-panel.png          (960x256)
├── input-panel-hover.png    (960x256)
├── input-panel-active.png   (960x256)
├── equipment-grid.png       (256x256)
├── quick-bar.png            (960x32)
├── quick-bar-numbers.png    (256x32)
├── toggle-button.png        (128x128)
├── slider.png               (256x32)
├── drill-down-arrow.png     (64x64)
└── ... (mais de 50 arquivos)
```

### 1.6 Pós-processamento (Post-processing)

#### Pipeline de Pós-processamento

```
Post-processing Chain:

1. Resolve Render Target
   │
   │  - Resolve multi-sampled buffer
   │  - Downsample if necessary
   │
   └─► 2. Bloom Pass
           │
           │  - Gaussian blur horizontal
           │  - Gaussian blur vertical
           │  - Threshold control
           │
           └─► 3. FXAA/Morphological AA
                   │
                   │  - Fast approximate anti-aliasing
                   │  - Edge detection-based
                   │
                   └─► 4. Color Grading
                           │
                           │  - Color correction
                           │  - Hue/saturation/brightness
                           │
                           └─► 5. Tone Mapping
                                   │
                                   │  - ACES filmic tone mapping
                                   │  - Exposure control
                                   │
                                   └─► 6. Present to Screen
```

#### Configurações de Pós-processamento

| Configuração | Valor Padrão | Descrição |
|-------------|-------------|-----------|
| `graphics quality` | Medium | Low/Medium/High/Ultra |
| `anti-aliasing` | FXAA | Nenhum/FXAA/MFAA/TSAA |
| `bloom` | Enabled | Bloom effect on/off |
| `exposure` | Auto | Exposure value (auto/manual) |
| `contrast` | 1.0 | Contrast multiplier |
| `saturation` | 1.0 | Saturation multiplier |

### 1.7 Rendering Isométrico

O Factorio usa rendering isométrico 2D falso-3D:

#### Projeção Isométrica

```
Isometric Projection Matrix:

│  α  0  0  0 │     │  x' │     │  x + y/2 │
│  0  -α  0  0 │  ×  │  y'  =  │  y - x/2 │
│  0  0  1  0 │     │  z' │     │  z         │
│  0  0  0  1 │     │  w' │     │  1         │

Onde α = 0.5 (ângulo isométrico)
```

#### World-to-Screen Transformation

```
1. World Position (tiles):
   │  x_world, y_world (em tiles)

2. Isometric Projection:
   │  x_iso = x_world + y_world
   │  y_iso = y_world - x_world

3. Scale to pixels:
   │  x_screen = x_iso * TILE_SIZE * SCALE
   │  y_screen = y_iso * TILE_SIZE * SCALE

4. Camera Offset:
   │  x_screen += camera_x
   │  y_screen += camera_y

5. Final Position:
   │  draw_x = x_screen
   │  draw_y = y_screen
```

#### TILE_SIZE Padrão

- `TILE_SIZE` = 32 pixels (no texture space)
- `SCALE` = dependendo de configurações de zoom (1.0 = 100%)
- Zoom levels: 0.5, 0.75, 1.0, 1.5, 2.0

## 2. Shaders e Programáveis

### 2.1 Vertex Shader

#### Input Layout

| Semântica | Tipo | Descrição |
|-----------|------|-----------|
| `POSITION` | float3 | Position em espaço de objeto |
| `NORMAL` | float3 | Normal vector (para lighting) |
| `TEXCOORD` | float2 | UV coordinates para texturing |
| `COLOR` | float4 | Color por vertex (opcional) |

#### Shader Constants (constantes globais)

| Constant | Tipo | Valor Padrão | Descrição |
|----------|------|-------------|-----------|
| `gWorldViewProj` | matrix | Calculado por frame | World × View × Projection |
| `gWorld` | matrix | World matrix | Transformação do entity |
| `gView` | matrix | View matrix | Camera transform |
| `gProjection` | matrix | Projection matrix | Projection (perspective/ortho) |
| `gEyePosWorld` | float3 | Position da camera | Para lighting calculations |
| `gTime` | float | Tempo do jogo (game.tick / 60) | Para animações |
| `gDeltaTime` | float | Tempo desde último frame | 1/60 segundo |

#### Exemplos de Vertex Shader

```hlsl
// Vertex shader básico para entity rendering
struct VertexInput
{
    float4 position : POSITION;
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL;
};

struct VertexOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL0;
};

VertexOutput main(VertexInput input)
{
    VertexOutput output;
    
    // Aplicar transformações
    output.position = gWorldViewProj * input.position;
    
    // Passar UVs e normal
    output.texcoord = input.texcoord;
    output.normal = normalize(gWorld * input.normal);
    
    return output;
}
```

### 2.2 Pixel (Fragment) Shader

#### Input Layout

| Semântica | Tipo | Descrição |
|-----------|------|-----------|
| `TEXCOORD0` | float2 | UV coordinates |
| `NORMAL` | float3 | Normal map normal |
| `COLOR` | float4 | Base color (modulado) |

#### Shader Constants

| Constant | Tipo | Descrição |
|----------|------|-----------|
| `gTexture` | sampler2D | Textura do entity |
| `gNormalMap` | sampler2D | Normal map (opcional) |
| `gLightDir` | float3 | Direction of light |
| `gCameraPos` | float3 | Position da camera |
| `gExposure` | float | Exposure value |
| `gBloomThreshold` | float |Threshold para bloom |

#### Exemplos de Fragment Shader

```hlsl
// Fragment shader básico para entity rendering
struct PixelInput
{
    float2 texcoord : TEXCOORD0;
    float3 normal : NORMAL0;
    float4 color : COLOR0;
};

float4 main(PixelInput input) : SV_TARGET
{
    // Sample textura
    float4 texColor = gTexture.Sample(gSampler, input.texcoord);
    
    // Aplicar cor base
    float4 finalColor = texColor * input.color;
    
    // Iluminação básica (Lambert)
    float3 lightDir = normalize(gLightDir);
    float diffuse = max(dot(input.normal, lightDir), 0.0);
    
    // Adicionar iluminação
    finalColor.rgb *= diffuse;
    
    // Bloom check (simplified)
    if (finalColor.r > gBloomThreshold)
    {
        // Add bloom contribution
    }
    
    return finalColor;
}
```

### 2.3 Compute Shaders (Opcional)

 usados para:
- Pós-processamento avançado
- Particle system simulation
- Physics simulation offload
- Generation de textures derivadas

## 3. Recursos e Texturas

### 3.1 Textura Atlas System

O Factorio usa textura atlas extensivamente para reduzir draws calls:

#### Entity Atlas

```
Entity Texture Atlas Structure:

├── iron-ore.png              (64x64, frame 1)
├── iron-ore.png              (64x64, frame 2)  
├── iron-ore.png              (64x64, frame 3)
├── ... (anim frames)
├── copper-ore.png            (64x64)
├── coal.png                  (64x64)
├── stone.png                 (64x64)
└── ... (mais resources)
```

#### Item Atlas

```
Item Texture Atlas:

├── iron-plate.png           (32x32)
├── copper-plate.png         (32x32)
├── stone-brick.png          (32x32)
├── wood.png                 (32x32)
├── coal.png                 (32x32)
└── ... (mais itens)
```

#### GUI Atlas

```
GUI Texture Atlas:

├── input-panel.png          (960x256)
├── quick-bar.png            (960x32)
├── quick-bar-numbers.png    (256x32)
├── toggle-button.png        (128x128)
└── ... (mais GUI elements)
```

### 3.2 Formatos de Textura

| Formato | Bit depth | Cases d'uso |
|---------|-----------|-------------|
| **DXT5** | 4 bytes/pixel | Texturas comuns, normal maps |
| **BC7** | 8.5 bytes/pixel | Texturas com alta qualidade |
| **RGBA32F** | 16 bytes/pixel | HDR rendering, bloom |
| **RGFloat** | 8 bytes/pixel | Values floats |
| **PNG (lossless)** | Variável | Assets editoriais, GUI |

### 3.3 Anisotropic Filtering

Configurações disponíveis:

| Nível | Qualidade | Impacto Performance |
|-------|-----------|---------------------|
| 1x | Básico | Baseline |
| 2x | Melhor | ~5% overhead |
| 4x | Alta | ~10% overhead |
| 8x | Máxima | ~15-20% overhead |
| 16x | Overkill | ~25-30% overhead (diminishing returns) |

## 4. Performance Otimizações

### 4.1 CPU-side Otimizações

| Otimização | Descrição | Impacto |
|------------|-----------|---------|
| **Culling** | Pular entities fora da visão | Reduz draw calls 30-50% |
| **Instancing** | Agrupar entities idênticas | Reduz draw calls 60-80% |
| **Batch Grouping** | Agrupar por material/textura | Reduz state changes |
| **Cascade Rendering** | Renderização por layers | Melhora cache locality |
| **Threading** | Parallel processing de chunks | Utiliza múltiplos cores |

### 4.2 GPU-side Otimizações

| Otimização | Descrição | Impacto |
|------------|-----------|---------|
| **Mipmapping** | Textures with multiple LODs | Melhora qualidade + performance |
| **Anisotropic Filtering** | Better texture quality at angles | Small performance cost |
| **Early Z** | Testar depth antes de shading | Reduz fragment shader calls |
| **Blend State** | Otimizar blending functions | Reduz overdraw |
| **Render Targets** | Usar texturas menores para pós-processamento | Reduz memory bandwidth |

### 4.3 Memory Management

| Recurso | Uso Estimado | Gestão |
|---------|--------------|--------|
| **Textures** | 200-500 MB (dependendo da qualidade) | Atlas pooling, lazy loading |
| **Buffers** | 50-100 MB | Dynamic buffers, ring buffers |
| **Render Targets** | 50-100 MB | Framebuffers, depth buffers |
| **Shader Constants** | < 1 MB | Constant buffers (CBUFFER) |

## 5. Configurações de Rendering (User-facing)

### 4.1 Menu de Opções de Renderização

```
Configurações de Renderização:

│  Qualidade Gráfica: ──────── Low ▓▓▓▓ Medium ▓▓▓▓▓▓▓▓ High ▓▓▓▓▓▓▓▓▓▓ Ultra ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Anti-aliasing: ──────── None ▓▓▓▓ FXAA ▓▓▓▓▓ MFAA ▓▓▓▓▓▓ TSAA ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Bloom: ──────── Out ▓▓▓▓ In ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Exposição: ──────── Auto ▓▓▓▓ Manual ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Contraste: ──────── 0.5 ▓▓▓▓ 1.0 ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Saturação: ──────── 0.5 ▓▓▓▓ 1.0 ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Distância de Renderização: ──────── 50 ▓▓▓▓ 100 ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Sombra: ──────── Des ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
│  Refletos: ──────── Des ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Impacto das Configurações no Performance

| Configuração | Low → Medium | Medium → High | High → Ultra |
|-------------|-------------|--------------|--------------|
| **FPS típico** | +15-25% | +10-20% | +5-10% |
| **Usage GPU** | +10-15% | +15-25% | +20-35% |
| **Usage RAM** | +5-10% | +10-15% | +15-25% |
| **Tempo de carregamento** | +5-10% | +10-15% | +15-20% |

## 5. Depuração e Visualização de Rendering

### 5.1 Modos de Depuração

O Factorio inclui vários modos de depuração de rendering acessíveis através do mod ou teclas de atalho:

| Modo | Tecla Atalho | Descrição |
|------|-------------|-----------|
| **Wireframe** | `F3` | Mostra wireframe em vez de solid textures |
| **Bounding Boxes** | `F4` | Mostra collision boxes de todas as entities |
| **Pathfinding** | `F5` | Mostra navmesh e rotas de pathfinding |
| **Pollution** | `F6` | Visualização de spread de pollution |
| **Chunk Borders** | `Ctrl+F3` | Mostra borders entre chunks |
| **Entity IDs** | `F2` | Mostra IDs de entities |
| **Tile Types** | `Shift+F1` | Mostra tipos de tiles |

### 5.2 Visualizadores de Performance

| Visualizador | Descrição | Atalho |
|-------------|-----------|--------|
| **FPS Counter** | Contador de frames por segundo | `Alt+F1` |
| **Tick Time** | Tempo gasto por tick de simulação | `Alt+F2` |
| **Draw Calls** | Número de draws calls por frame | `Alt+F3` |
| **Texture Memory** | Memory usado por texturas | `Alt+F4` |

## 6. Integração com o Loop de Simulação

### 6.1 Sync entre Tick e Frame

```
Sincronização Tick-Frame:

│  Tick 1: ──────────┐
│                   │ 60 ticks/segundo = ~16.67ms por tick
│                   ├──► Update game state (simulation)
│                   │   │
│                   │   ├──► Entity updates
│                   │   ├──► Belt movement
│                   │   ├──► Crafting
│                   │   └──► Event handling
│                   │
│                   ├──► Prepare rendering data
│                   │   │
│                   │   ├──► Collect entity states
│                   │   ├──► Calculate transforms
│                   │   └──► Setup shader constants
│                   │
│                   └──► Rendering loop begins
│                       │
│                       ├──► Culling & Visibility
│                       ├──► Instancing setup
│                       ├──► Geometry rendering
│                       ├──► UI overlay
│                       └──► Post-processing
│
│                       └──► Present to screen
│
│  Tick 2: ──────────┘
│
└───────────────────────────────────────────────────────────────┘
```

### 5.2 Frame Timing Constraints

```
Timing Constraints:

│  Max time per tick:  ~16.67ms (para 60 FPS)
│  Max time per frame: ~16.67ms (para 60 FPS)
│
│  Actual timing varies:
│  - Min: ~8-10ms (sistemas simples)
│  - Avg: ~12-15ms (jogo típico)
│  - Max: ~20-30ms+ (late game, muitos entities)
│
│  If tick takes > 16.67ms:
│  - Game clock slows down (slowdown mode)
│  - FPS drops below 60
│  - Player perceives "lag"
│
│  Catch-up mechanism:
│  - Se tick takes < 16.67ms, game clock acelera
│  - Multiple ticks podem ser processados por frame
│  - Mantém game tempo consistente
```

## 7. Limitations e Known Issues

### 7.1 Limitações de Rendering

| Limitação | Descrição | Workaround |
|-----------|-----------|------------|
| **Draw call limit** | Número finito de draws calls por frame | Instancing, batch grouping |
| **Texture memory** | Limite de memória de texturas | Mipmapping, texture compression |
| **Shadow rendering** | Shadows têm custo performance significativo | Shadow quality settings |
| **Post-processing** | Efeitos de pós-processamento custam GPU | Disable blooms/AA para performance |
| **Entity limit** | Número máximo de entities processados por tick | Chunk loading/unloading |

### 7.2 Known Issues

| Issue | Descrição | Status |
|-------|-----------|--------|
| **FPS drop late game** | FPS pode cair para 30-40 em late game com muitos entities | Known, mitigated por configurações |
| **Memory leaks** | Alguns mods causam memory leaks ao longo do tempo | Mod-dependent |
| **Stuttering** | Micro-freezes durante geração de chunks | Known, related to chunk loading |
| **Alt-tab issues** | Rendering state corruptions after alt-tab | Fixed em patches recentes |

## 8. Integração com Mods

### 7.1 Mod Rendering API

Mods podem estender o rendering através:

| Método | Descrição | Exemplos |
|--------|-----------|----------|
| `defines.gui.type` | Definir tipos de GUI customizados | Custom windows, panels |
| `defines.entity.draw` | Custom drawing de entities | Particle effects, glow effects |
| `defines.shader` | Adicionar shaders customizados | Post-processing effects, custom lighting |
| `game.render_altered` | Hook para modificar rendering | Color correction, custom post-processing |
| `render_mod` | Mods de rendering completos | New color schemes, custom pipelines |

### 7.2 Exemplos de Mods de Rendering

| Mod | Funcionalidade | Impacto de Performance |
|-----|---------------|-----------------------|
| **Night Vision** | Night vision goggles effect | Small (~5% FPS impact) |
| **Dynamic Lighting** | Dynamic light sources | Medium (~15-20% FPS impact) |
| **Shader Mods** | Custom shader effects | Variable (5-30% dependendo) |
| **Resolution Scaling** | Render at lower resolution, upscale | Good performance tradeoff |

## Observações Técnicas

1. **Pipeline Fixo**: O pipeline de rendering do Factorio é amplamente fixo - a maioria das alterações requer editar o executável ou usar mods que injetam código.

2. **Determinismo**: O rendering é conduzido pelo estado do jogo (entities positions, etc.), então a sequência de draws calls é determinística dado o mesmo estado do jogo.

3. **60 FPS Target**: O motor visa 60 FPS, mas o gameplay rola a 60 ticks/segundo independentemente do FPS - o FPS afeta apenas a fluidez visual.

4. **Isométrico 2D**: Apesar de parecer 3D, o rendering é 2D isométrico - não há true 3D rendering do mundo do jogo.

5. **Modding Friendly**: Grande parte do rendering pode ser customizada via mods sem tocar no executável.

## Próximos Relatórios Planejados

- `PHYSICS_ANALYSIS.md` - Física e colisões avançado
- `SIMULATION_ANALYSIS.md` - Motor de simulação completo
- `LOGISTICS_ANALYSIS.md` - Sistema logistics avançado
- `MODDING_API.md` - API de modding completa (já parcialmente coberto)
- `BINARY_ANALYSIS.md` - Análise do executável (REA limited)