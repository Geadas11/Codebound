# RENDERING_ANALYSIS — Sistema gráfico e rendering

## 1. Backends [C]
| Backend | Evidência | Plataforma |
|---|---|---|
| **Direct3D 11** | `src/Graphics/DX11/` (22 ficheiros); string `direct3d11`; log [R/2.0.60] `[Direct3D11] … d3dcompiler_47.dll`, `D3D Feature Level: 11.1, DXGI 1.5+, SwapChain: 3,flip-discard` | Windows (omissão) |
| **OpenGL** | `src/Graphics/OpenGL/` (33); `glad`; opções `force-opengl`, `disabled-opengl-extensions`; imports `ChoosePixelFormat`/`SwapBuffers` | Windows (alternativa) / Linux |
| **Metal** | `src/Graphics/Metal/` (26); 110 ficheiros `.metal` | macOS |
| Headless | `HeadlessGraphicsInterface`, `HeadlessWindow` [C] | servidor dedicado / benchmark |
| ~~Vulkan~~ | só dentro do SDL3 (`SDL_gpu_vulkan`) [C]; **não** existe backend Vulkan próprio do Factorio |

`GpuApi`, `GraphicsInterface`, `RenderInterface`, `PipelineStateObject`, `PipelineBindings`, `BlendState`, `DepthStencilState`, `Framebuffer` [C] formam a camada de abstração sobre os backends [I].

## 2. Arquitetura do pipeline [C nomes, I fluxo]

```
MainLoop::prepare ──► GameRenderer::prepare ──► DrawQueue (recolha) ──► MainLoop::render ──► backend
   (fase "prepare": percorre o estado de jogo, gera DrawOrders)      (fase "render": submete batches à GPU)
```
- **Recolha**: renderizadores especializados, todos com `prepare()` [C]: `EntityRenderer`, `BuildingRenderer`, `CursorRenderer`, `ChartRenderer` (mapa), `BlueprintChartRenderer`, `CloudsRenderer`, `CoveredChunkRenderer`, `GuiRenderer`, `ProcessionRenderer`, `RoboportInfoRenderer`, `PlayerDamageRenderer`, `DebugInfoRenderer`, `OutputConsoleRenderer`, `LightningDrawEngine`…
- **DrawQueue** (4 784 B) [C]: `array<SpriteDrawOrderVector,255> sprites` (**um vetor por render layer**, `RenderLayer::Enum` com `sortLayer[256]` a indicar quais são ordenadas), `shadows[6]`, `treeShadows`, `gradientLights`, `detailLights`, `decayingCorpses`, `fogMasks`, `waterReflections`, `turretRanges`, `pollution`, `territories`, `asteroidDrawStates`, `lightningDrawStates`, `fusionEffectDrawStates`, `drawingLatencyState`…
- Constantes de ordenação [C]: `DRAW_SHADOWS_BEFORE_LAYER`, `DRAW_ELEVATED_SHADOWS_BEFORE_LAYER`, `DRAW_ASTEROIDS_AND_THRUSTERS_BEFORE_LAYER`, `DRAW_FOG_BEFORE_LAYER`, `ABOVE_CHART_MAP_SPRITES`. As sombras e o nevoeiro são intercalados entre layers.
- **DrawOrders tipados** [C]: `ImageDrawOrder`, `LineDrawOrder`, `CircleDrawOrder`, `PolygonDrawOrder`, `RectangleListDrawOrder`, `RenderToTextureDrawOrder`, `BlurEffectDrawOrder`, `GradientOverlayEffectDrawOrder`, `EntityReflectionDrawOrder`, `PieSliceDrawOrder`…
- **Batching**: `DrawCommandBatch`, `AsteroidDrawCommandBatch` [C].
- **Separação de threads**: `AssertRenderThread` [C] → existe uma thread de render dedicada (verificada por asserção) [I]. O log mostra `Max threads (load/render): 32/22` [R/2.0.60].
- A **camada de latência** também é desenhada (`DrawQueue::drawingLatencyState`) [C]: o cliente mostra o estado previsto.

## 3. Sprites, atlases e texturas
- **Atlas dinâmico** [C]: `Atlas`, `AtlasBuilder` (`cropSpritesParallel`), `AtlasSystem`, `AtlasCache`, `CropCache` (`crop-cache.dat` em `%APPDATA%`).
- Log [R/2.0.60]: atlases de 16 384 px agrupados por **classe de uso**: `[none]`, `[low-object]`, `[corpse-decay]`, `[decal]`, `[terrain, mipmap…]`, `[terrain-effect-map]`, `[icon, not-compressed…]`, `[alpha-mask]`, `[shadow, …alpha-mask]`, `[smoke, mipmap…]`, `[not-compressed]`.
- **Compressão em GPU** [C]: shaders `compress-bc4`, `compress-rygDXT`, `compress-ycocg-dxt` (únicos com `.cso` pré-compilado). Os sprites são comprimidos para BC/DXT na GPU durante o carregamento [I]. As opções do log incluem `DXT: high-quality` e `Video memory usage: all`.
- `ReloadableSpritesManager`, `ParallelSpriteLoader`, `MinimalModeSpriteLoadingContext` [C]: carregamento paralelo e modo mínimo (servidor/benchmark).
- Formatos: `sprite565.psh`, `sprite-r8.psh`, `sprite_ycocg.psh`, `shadow-a8.psh` [C]. Várias codificações de cor (RGB565, R8, YCoCg) reduzem memória de vídeo [I].

## 4. Shaders [C] (`data/core/graphics/shaders/`, 3 variantes por shader: `.psh/.vsh` HLSL, `.frag/.vert` GLSL, `-frag/-vert.metal`)
Principais: `game` (composição), `sprite*` (≈20 variantes: masked, light, tree growth/leaves, sdf, ycocg), `tiles-*` (blit, color, decal masked by water), `map`, `clouds`, `fog-gleba`, `fog-vulcanus`, `planet`, `space-starfield`, `asteroid*`, `thruster-jet-flame`, `fusion-effect`, `lightning`, `hologram`, `ghost-shimmer`, `territory-overlay`, `blur`, `blend-luts`, `final-postprocessing(-custom)`, `font`, `font-sdf`, `mipmap-gen*`.
- `ColorLookupEngine`, `DaytimeColorLookup`, `color_luts/` [C]: **color grading por LUT** dependente da hora do dia [I].
- Iluminação: `sprite_light`, `simple-with-light`, `lightOcclusion` (log: `Light occlusion: YES`) [C/R].

## 5. Relação com a simulação
- O render **não escreve** no estado de jogo [I, forte]. Os renderizadores só leem `Entity::draw(...)` e as funções `draw*` (≈ todas as classes de entidade têm `draw`, `drawSelectionInfo`…).
- **Não há interpolação visual entre ticks** [C/G, por ausência]: ver GHIDRA_FINDINGS §12.
- Animações de entidades são derivadas de contadores no estado (`CyclicFramePosition`, `animationState`) [C]. A simulação guarda a fase da animação para que todos os peers a vejam igual [I].

## 6. GUI [C]
`src/Gui/` (1 213 ficheiros) sobre a biblioteca **Agui**, estilos definidos em Lua (`core/prototypes/style.lua` → `LuaStyle`, 127 funções ligadas). As fontes usam `fontstash` + SDF (`font-sdf.psh`).

## 7. Limitações
- `RenderLayer::Enum`: 97 nomes↔valores extraídos do PDB → `artifacts/render_layers.txt` [C] (GHIDRA_FINDINGS §14).
- O agendamento entre a thread de update e a de render (double-buffer do estado? cópia? lock?) [N]. É o principal ponto a confirmar com descompilação de `MainLoop::mainLoopStep`/`prepare`.
