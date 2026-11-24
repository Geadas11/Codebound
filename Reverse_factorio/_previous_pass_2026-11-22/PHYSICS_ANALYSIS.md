# PHYSICS_ANALYSIS — "Física" e colisões

**Legenda:** [CONFIRMADO] · [INFERÊNCIA] · [HIPÓTESE] · [CONHECIMENTO PÚBLICO]

## 1. Conclusão principal
[CONFIRMADO] Pesquisa só-leitura por bytes (mmap) no `factorio.exe` e no `factorio.pdb` (script `physlib_scan.py`, saída em `EVIDENCE_PHYSLIBS.txt`): **0 ocorrências** de `PxFoundation`, `PxPhysics`, `physx`, `PhysX`, `btCollision`, `btRigidBody`, `btDiscreteDynamicsWorld`, `Bullet Physics`, `Havok`, `hkpWorld`, `hkBase`, `b2World`, `b2Body`, `Box2D`, `chipmunk`, `cpSpace`, `dWorldCreate`. Os acertos de `ODE_` são falsos positivos verificados por contexto (substrings de `SDL_KEYCODE_OPTIONS`, `…RELATIVE_MODE_CENTER`, `…_EXCLUSIVE_MODE_ON_MOVE`). Nenhuma DLL de física em `bin\x64`.
[INFERÊNCIA forte] Factorio **não usa um motor de física de corpos rígidos de terceiros**. [INFERÊNCIA] A "física" é cinemática e determinística: caixas de colisão alinhadas aos eixos, movimento por velocidade por tick. O código interno de movimento não foi decompilado (Ghidra indisponível).

## 2. Colisões — evidência Lua [CONFIRMADO]
- `data\core\lualib\collision-mask-defaults.lua` e `collision-mask-util.lua` existem.
- `type = "collision-layer"` aparece 24× nos protótipos de `base`+`core`.
[INFERÊNCIA] Cada entidade tem uma `collision_mask` = conjunto de camadas; duas entidades colidem se as máscaras se intersectam. As camadas são protótipos (`collision-layer`), logo mods podem adicionar camadas.
[CONHECIMENTO PÚBLICO] Cada entidade tem `collision_box` (retângulo) e `selection_box`; colisão = intersecção de retângulos + intersecção de máscaras.

## 3. Movimento
| Sistema | Modelo provável |
|---|---|
| Personagem | [INFERÊNCIA] velocidade fixa por tick, bloqueado por colisão; sem aceleração física |
| Veículos (carro, tanque) | [CONHECIMENTO PÚBLICO] protótipos têm `weight`, `braking_power`, `friction`, `consumption` → modelo simplificado de inércia/atrito |
| Comboios | [CONHECIMENTO PÚBLICO] seguem carris; `max_speed`, `air_resistance`, `weight` → equação 1D ao longo do caminho, não 2D |
| Projéteis | [CONFIRMADO] `projectile` 80×, `stream` 26×, `beam` 12× em `type=` → trajetórias calculadas, não física genérica |
| Itens em correias | ver LOGISTICS_ANALYSIS.md (posições discretas ao longo da correia) |

## 4. Efeitos de impacto [CONFIRMADO contagens]
`create-particle` 811×, `explosion` 190×, `create-entity` 65×, `create-trivial-smoke` 22×, `create-sticker` 10×. São **gatilhos** declarativos (trigger effects) e não simulação física.
[INFERÊNCIA] Partículas são sobretudo visuais; `trivial-smoke` sugere uma via barata que não participa na simulação determinística.

## 5. Determinismo
[INFERÊNCIA] Toda a cinemática tem de ser determinística entre máquinas (lockstep — ver NETWORKING_ANALYSIS.md). [HIPÓTESE] posições em ponto fixo (o [CONHECIMENTO PÚBLICO] fala de MapPosition em 1/256 de tile) para evitar divergência de vírgula flutuante. Não verificado no binário.

## 6. O que falta
- Confirmar no PDB classes como `CollisionMask`, `MapPosition`, `BoundingBox`.
- Ler `collision-mask-defaults.lua` e documentar as camadas por omissão por tipo de entidade.
