# PHYSICS_ANALYSIS — Física e colisões

## 1. Conclusão principal
O Factorio **não usa motor de física genérico** (sem Box2D, PhysX, Bullet: verificado em DEPENDENCIES §5) [C]. A "física" são vários modelos específicos, cinemáticos e determinísticos, implementados à mão [I, forte]:

| Domínio | Modelo | Evidência [C] |
|---|---|---|
| Colisão estática | AABB / caixa rodada contra camadas de colisão | `BoundingBox`, `RotatedBoundingBox`, `CollisionMask`, `Surface::collide*` |
| Personagem / unidades | movimento cinemático com resolução de colisão | `Character` (1 912 B), `Unit`, `Steering`, `ObstacleAvoider`, `StuckDetector` |
| Veículos (carro, tanque) | velocidade escalar + orientação; fricção, eficiência | `Car { double speed, effectivityModifier, consumptionModifier, frictionModifier; RealOrientation; RidingState }`, `VehicleUtils` |
| Comboios | dinâmica 1-D ao longo do carril | `Train { weight, brakingForce, frictionForce, airResistance, maxForwardSpeed, maxBackwardSpeed, speed }`, `TrainMover`, `RailPath` |
| Projéteis | trajetória linear com alcance | `Projectile { orientation, speed, initialSpeed, remainingRange, traveledDistance, remainingPiercingDamage }` |
| Jatos (flamethrower) | trajetória balística | `FluidStream` (584 B), `ArtilleryProjectile`, protótipo `stream` |
| Transporte por belts | deslocamento imposto | `MovableByTransportBelt` (base de `Car`, `Character`…) |
| Partículas/fumo | visuais, fora da simulação ou "trivial" | `ParticleSurface`, `TrivialSmoke` (guardado no chunk, mas sem interação) |

## 2. Sistema de colisões

### 2.1 Geometria [C]
- `BoundingBox` (20 B): dois cantos em ponto fixo 1/256 + `VectorOrientation`. Métodos: `collide`, `collideNotRotated`, `aabbContinuousCollide`, `continuousCollide`, `collideWithCircle`, `collideWithLine`, `collideWithSegment`, `segmentCollision`, `firstCollisionPoint`, `getRotatedVertices`, `rotatedCoveredByCircle`.
  → teste **AABB rápido** quando não há rotação; **caixa orientada** (SAT ou equivalente) quando há [I]; deteção **contínua** (*swept*) para objetos rápidos [I, de `continuousCollide`].
- `ConvexPolygon`, `Intersection`, `Line`, `SimpleBoundingBox` (Util) [C].

### 2.2 Camadas [C]
- `CollisionMask` = `CollisionLayers` (bitset de 256 camadas) + `flags`. As máscaras estão internadas (`CollisionMaskIndex` u16 → `indexToCollisionMask`).
- 24 camadas definidas em Lua (`data/base/prototypes/collision-layers.lua`): `ground_tile, water_tile, resource, doodad, floor, rail, transport_belt, item, ghost, object, player, car, train, elevated_rail, elevated_train, empty_space, lava_tile, meltable, rail_support, trigger_target, cliff, is_lower_object, is_object, out_of_map`.
  **As camadas são dados, não código**: o motor só faz `(maskA & maskB) != 0` sobre o bitset [I]. Mods podem acrescentar camadas (`CollisionLayerPrototype`).
- `TileCollisionMask` + `TileCollisionMaskConnector` [C]: tiles também têm máscara (água bloqueia `object`, etc.).
- `PathfindingCollisionMask` [C]: máscara separada para o pathfinder.

### 2.3 Consultas espaciais [C, `Surface`]
`collide`, `collideWithEntity(Predicate)`, `collideWithTile(WithTransitions)`, `checkTileCollisions`, `findNonCollidingPosition`, `findNonCollidingPositionForSpiderLeg`, `findItemNonCollidingPositionOptimized`, `findAllEntities`, `findNearestEnemy`, `itemCollide`.
- Partição espacial = **chunks de 32×32** (`ChunkPosition`, `Surface : InfiniteRectangle<Chunk*>`) [C]. As entidades são indexadas por chunk e as consultas percorrem os chunks intersectados [I]. Não há BVH/quadtree global [N: sem tipos com esses nomes].
- `CollidingEntitiesProcessor` (raiz `src/`) [C].

## 3. Movimento de personagens/unidades [C nomes, I comportamento]
- `Steering`, `SteeringSettings` (separação, força), `ObstacleAvoider`, `StuckDetector`, `StrafeBehavior`, `FollowPathBehavior` (AI/).
- `Character` move-se pelo input `StartWalking`/`StopWalking` (InputActionType 70/1) e `StopMovementInTheNextTick` (32) [C]. A posição é recalculada no tick determinístico. A previsão local é feita em `LatencyState` [I].

## 4. Unidades e determinismo
Todas as posições usam `FixedPoint<int,8>` [C]. As velocidades de veículos/comboios e os ângulos de projéteis são `double`/`float` [C]. O resultado final é quantizado para `MapPosition` [I]. A ordem de avaliação é fixa (listas intrusivas, iteração determinística) [I].

## 5. O que não foi confirmado
- Algoritmo exato de resolução de colisão do personagem (deslizamento ao longo de paredes?) [N].
- Fórmulas da dinâmica do carro e do comboio (aceleração = f(força, peso, atrito, resistência do ar)) [N]. Os campos existem [C]; as equações exigem descompilação de `Car::update`/`TrainMover`.
