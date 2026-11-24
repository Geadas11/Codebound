# Formato de `save()` por classe de entidade (gerado)

Gerado por `scripts/entity_save_summary.py` a partir de `artifacts/decomp/entity_save/*.c` (Ghidra, sem tipos). **Base** = `save` do antepassado que é chamado primeiro (formato = antepassado + campos abaixo). **Sub-objetos** = serializadores chamados, por ordem de 1.ª ocorrência. **Campos etiquetados** = nomes `TagWriter` (só gravados como texto em modo diagnóstico; indicam o campo seguinte). **Escritas diretas** = nº de escritas brutas (u8/u16/u32/u64/double).

| Classe | Mãe | Base | Sub-objetos serializados | Campos etiquetados | Escritas diretas |
|---|---|---|---|---|---:|
| Accumulator | EntityWithOwner | ✔ EntityWithOwner | ElectricEnergySource::save, AccumulatorControlBehavior::save | — | 5 |
| AgriculturalTower | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, <<EffectReceiver>, <<StuckItems>, WakeUpList::save, AgriculturalCraneState::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, ID<EntityPrototype,unsigned_short>::save, TargeterBase::save, <<CraftingMachineAnimationState>, AgriculturalTower__Task::save, GenericOnOffControlBehavior::save, CircuitCondition::save | — | 6 |
| AmmoTurret | Turret | ✔ Turret | WakeUpList::save, ElectricEnergySource::save | — | 1 |
| ArithmeticCombinator | Combinator | ✔ Combinator | ArithmeticCombinatorControlBehavior::save | — | 0 |
| Arrow | Entity | — | ID<EntityPrototype,unsigned_short>::save, MapPosition::save, BoundingBox::save, Targetable::save, Effect::save | entity-data, bounding-box, has-surface, entity-status, effect | 10 |
| ArtilleryFlare | Entity | ✔ Entity | UpdatableEntity::save, CyclicFramePosition::save, Vector2<double,Vector>::save | — | 10 |
| ArtilleryProjectile | Entity | ✔ Entity | UpdatableEntity::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, TargeterBase::save, MapPosition::save, <<TriggerModifierData>, BoundingBox::save | damage-estimate, targets | 12 |
| ArtilleryTurret | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, WakeUpList::save, Shooter::save, TargeterBase::save, MapPosition::save, BoundingBox::save, CircleIterator<ChunkPosition>::save | — | 13 |
| ArtilleryWagon | RollingStock | ✔ RollingStock | WakeUpList::save, Shooter::save, TargeterBase::save, MapPosition::save, BoundingBox::save, CircleIterator<ChunkPosition>::save | — | 11 |
| AssemblingMachine | CraftingMachine | ✔ CraftingMachine | AssemblingMachineControlBehavior::save | — | 1 |
| Asteroid | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, Vector2<double,Vector>::save, BoundingBox::save | — | 0 |
| AsteroidChunkEntity | Entity | nunca gravada (aborta) | — | — | 0 |
| AsteroidCollector | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, <<InventoryWithBar>, WakeUpList::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, AsteroidCollectorArm::save, AsteroidCollectorControlBehavior::save, TargeterBase::save, Vector2<double,Vector>::save | asteroid-collector | 3 |
| Beacon | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, Effect::save | — | 4 |
| Beam | Entity | ✔ Entity | UpdatableEntity::save, TargeterBase::save, MapPosition::save, BoundingBox::save, <<TriggerModifierData>, Vector2<double,Vector>::save | — | 11 |
| Boiler | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, WakeUpList::save, FluidBox::save, GenericOnOffControlBehavior::save | boiler-control-behavior | 8 |
| BurnerGenerator | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ElectricEnergySource::save, Burner::save, WakeUpList::save, CyclicFramePosition::save | — | 2 |
| CaptureRobot | FlyingRobot | ✔ FlyingRobot | TargeterBase::save | — | 2 |
| Car | Vehicle | ✔ Vehicle | WakeUpList::save, <<RidingState>, Shooter::save, LogisticPoint::save, CyclicFramePosition::save, <<Color>, TargeterBase::save, MapPosition::save, BoundingBox::save | — | 14 |
| CargoBay | EntityWithOwner | ✔ EntityWithOwner | TargeterBase::save | — | 3 |
| CargoLandingPad | EntityWithOwner | ✔ EntityWithOwner | <<InventoryWithBar>, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, <<CargoStation>, LogisticPoint::save, WakeUpList::save, <<ForceChartedArea>, CargoLandingPadControlBehavior::save, ID<ItemPrototype,unsigned_short>::save | — | 7 |
| CargoPod | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CargoUnit::save, TargeterBase::save, Vector2<double,Vector>::save, MapPosition::save | — | 12 |
| CargoWagon | RollingStock | ✔ RollingStock | WakeUpList::save | — | 0 |
| Character | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, Status::save, CharacterModifiers::save, ManualMiner::save, TargeterBase::save, MapPosition::save, WakeUpList::save, ItemStack::save, ID<ItemPrototype,unsigned_short>::save, CyclicFramePosition::save, ManualCrafter::save, Shooter::save, EntitySelector::save, <<GuiArrowSpecification>, GameStateManualBuilder::save, LogisticPoint::save, PortableRoboportProvider::save, CharacterMapConnectSpecification::save, <<Color>, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, RawPointerToTargetable::save | character, character-data, light-orientation, custom-color, associated-player | 13 |
| CharacterCorpse | Entity | ✔ Entity | UpdatableEntity::save, <<Color>, LocalisedString::save | — | 5 |
| Cliff | Entity | ✔ Entity | — | — | 2 |
| CombatRobot | FlyingRobot | ✔ FlyingRobot | Vector2<double,Vector>::save, TargeterBase::save, Shooter::save | owner | 6 |
| Combinator | EntityWithOwner | ✔ EntityWithOwner | saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64> | — | 2 |
| ConstantCombinator | EntityWithOwner | ✔ EntityWithOwner | CircuitConnector::save, <<CompiledLogisticFilters>, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64> | constant-combinator-control-behavior | 0 |
| ConstructionRobot | RobotWithLogisticInterface | ✔ RobotWithLogisticInterface | WorkerRobotOrder::save, MapPosition::save, CyclicFramePosition::save | — | 4 |
| ContainerEntity | EntityWithOwner | ✔ EntityWithOwner | SpecificInventoryLoader::saveInventory, WakeUpList::save | — | 2 |
| Corpse | Entity | ✔ Entity | UpdatableEntity::save, <<ByteColor> | — | 9 |
| CraftingMachine | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, <<EffectReceiver>, ID<FluidPrototype,unsigned_short>::save, DropTarget::save, WakeUpList::save, FluidBoxManager::save, <<CraftingMachineAnimationState> | spoil-ticks, ingredient-temperatures, cached-recipe-production-bonus | 1 |
| DeciderCombinator | Combinator | ✔ Combinator | DeciderCombinatorControlBehavior::save | — | 0 |
| DeconstructibleTileProxy | Entity | ✔ Entity | — | — | 3 |
| DisplayPanel | EntityWithOwner | ✔ EntityWithOwner | saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, SignalIDBase::save, CircuitConnector::save, CircuitCondition::save | — | 5 |
| ElectricEnergyInterface | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ElectricEnergySource::save, CyclicFramePosition::save | — | 9 |
| ElectricPole | EntityWithOwner | ✔ EntityWithOwner | <<CopperWireConnector>, CircuitConnector::save | — | 2 |
| ElectricTurret | Turret | ✔ Turret | — | — | 0 |
| EnemySpawner | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, TargeterBase::save | — | 4 |
| Entity | — | — | ID<EntityPrototype,unsigned_short>::save, MapPosition::save, BoundingBox::save, Targetable::save, Effect::save | entity-data, bounding-box, has-surface, entity-status, effect | 10 |
| EntityGhost | Ghost | ✔ Entity | WakeUpList::save, TargeterBase::save, InsertPlan::save | — | 3 |
| EntityWithHealth | Entity | ✔ Entity | ID<EntityPrototype,unsigned_short>::save | — | 2 |
| EntityWithOwner | EntityWithHealth | ✔ EntityWithHealth | EntityWithOwner::saveUnitNumber | — | 0 |
| Explosion | Entity | ✔ Entity | UpdatableEntity::save, CyclicFramePosition::save, Vector2<double,Vector>::save | — | 7 |
| FireFlame | Entity | ✔ Entity | UpdatableEntity::save, TargeterBase::save, MapPosition::save, <<TriggerModifierData>, FireFlame__FlameParticle::save | fire-flame | 0 |
| Fish | EntityWithHealth | ✔ EntityWithHealth | UpdatableEntity::save | — | 4 |
| FlameThrowerExplosion | Explosion | nunca gravada (aborta) | — | — | 0 |
| FluidStream | Entity | ✔ Entity | UpdatableEntity::save, TargeterBase::save, MapPosition::save, BoundingBox::save, <<TriggerModifierData>, CyclicFramePosition::save | fluid-stream, particles, fluid-particle | 0 |
| FluidTurret | Turret | ✔ Turret | <<Fluid>, FluidBox::save | — | 1 |
| FluidWagon | RollingStock | ✔ RollingStock | <<Fluid>, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, TargeterBase::save | — | 0 |
| FlyingRobot | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save | — | 7 |
| FlyingTextEntity | Entity | nunca gravada (aborta) | — | — | 0 |
| Furnace | CraftingMachine | ✔ CraftingMachine | FurnaceControlBehavior::save | — | 1 |
| FusionGenerator | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ElectricEnergySource::save, FluidBox::save, CyclicFramePosition::save | — | 3 |
| FusionReactor | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ElectricEnergySource::save, Burner::save, WakeUpList::save, NeighbourConnectable::save, FluidBox::save | — | 2 |
| Gate | WallConnectable | ✔ WallConnectable | UpdatableEntity::save, TargeterBase::save | controlled-externally | 9 |
| Generator | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ElectricEnergySource::save, CyclicFramePosition::save, FluidBox::save | — | 5 |
| Ghost | Entity | ✔ Entity | — | — | 1 |
| HeatInterface | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, HeatBuffer::save, EntityHeaterData::save | temperature, mode | 2 |
| HeatPipe | EntityWithOwner | ✔ EntityWithOwner | HeatBuffer::save, EntityHeaterData::save, HeatPipeControlBehavior::save | — | 2 |
| HighlightBoxEntity | Entity | ✔ Entity | UpdatableEntity::save, BoundingBox::save, TargeterBase::save | — | 5 |
| InfinityCargoWagon | CargoWagon | ✔ RollingStock | WakeUpList::save, InfinityInventorySettings::save | — | 1 |
| InfinityContainer | LogisticContainer | ✔ LogisticContainer | UpdatableEntity::save, InfinityInventorySettings::save, LogisticSections::save, LogisticPoint::save | — | 3 |
| InfinityPipe | Pipe | ✔ EntityWithOwner | FluidBox::save, SingleFluidBoxControlBehavior::save, ID<FluidPrototype,unsigned_short>::save, UpdatableEntity::save | — | 4 |
| Inserter | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ItemStack::save, TargeterBase::save, DropTarget::save, InserterControlBehavior::save, ID<ItemPrototype,unsigned_short>::save, Vector2<double,Vector>::save | pickup-taget, belt-item-pickup-target, drop-target, filters, max-belt-stack-size | 8 |
| ItemEntity | Entity | ✔ Entity | ItemStack::save | — | 1 |
| ItemRequestProxy | Entity | ✔ Entity | UpdatableEntity::save, LogisticPointFilters::save, TargeterBase::save, InsertPlan::save | — | 4 |
| Lab | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, <<EffectReceiver>, WakeUpList::save, CyclicFramePosition::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, ID<ItemPrototype,unsigned_short>::save, LabControlBehavior::save | — | 7 |
| Lamp | EntityWithOwner | ✔ EntityWithOwner | <<Color>, LampControlBehavior::save | — | 3 |
| LandMine | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save | — | 2 |
| LaneSplitter | SplitterBase | ✔ TransportBeltConnectable | <<SplitterLogic>, <<SplitterStructureAnimation>, TransportLineConnector::save | — | 0 |
| Lightning | Entity | ✔ Entity | UpdatableEntity::save, <<TrackingMapPosition> | lightning | 7 |
| LightningAttractor | EntityWithOwner | ✔ EntityWithOwner | ElectricEnergySource::save | — | 0 |
| LinkedBelt | TransportBeltConnectable | ✔ TransportBeltConnectable | TargeterBase::save | — | 1 |
| LinkedContainer | EntityWithOwner | ✔ EntityWithOwner | — | — | 4 |
| Loader | TransportBeltConnectable | ✔ TransportBeltConnectable | UpdatableEntity::save, TargeterBase::save, ID<ItemPrototype,unsigned_short>::save, LoaderControlBehavior::save | complete-mode | 5 |
| Locomotive | RollingStock | ✔ RollingStock | saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, WakeUpList::save | — | 0 |
| LogisticContainer | ContainerEntity | ✔ ContainerEntity | LogisticPoint::save | — | 1 |
| LogisticRobot | RobotWithLogisticInterface | ✔ RobotWithLogisticInterface | WorkerRobotOrder::save | — | 2 |
| Market | EntityWithOwner | ✔ EntityWithOwner | saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, ID<ItemPrototype,unsigned_short>::save | — | 3 |
| MiningDrill | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, TargeterBase::save, DropTarget::save, <<StuckItems>, WakeUpList::save, <<EffectReceiver>, MiningDrillControlBehavior::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, MiningDrillFilter::save | — | 11 |
| OffshorePump | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, FluidBox::save | — | 3 |
| OldAgriculturalTower | AssemblingMachine | nunca gravada (aborta) | — | — | 0 |
| ParticleSource | Entity | ✔ Entity | UpdatableEntity::save, Vector2<double,Vector>::save | — | 4 |
| Pipe | EntityWithOwner | ✔ EntityWithOwner | FluidBox::save, SingleFluidBoxControlBehavior::save | — | 1 |
| PipeToGround | EntityWithOwner | ✔ EntityWithOwner | FluidBox::save, SingleFluidBoxControlBehavior::save | — | 2 |
| Plant | Tree | ✔ EntityWithHealth | — | — | 5 |
| PlatformCargoChest | EntityWithOwner | nunca gravada (aborta) | — | — | 0 |
| PowerSwitch | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, <<CopperWireConnector> | — | 4 |
| ProgrammableSpeaker | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, ProgrammableSpeakerParameters::save, ProgrammableSpeakerAlertParameters::save, CircuitConnector::save, CircuitCondition::save | programmable-speaker-control-behavior | 6 |
| Projectile | Entity | ✔ Entity | UpdatableEntity::save, TargeterBase::save, MapPosition::save, BoundingBox::save, <<TriggerModifierData>, CyclicFramePosition::save | — | 12 |
| ProxyContainer | EntityWithOwner | ✔ EntityWithOwner | WakeUpList::save, TargeterBase::save, ProxyContainerControlBehavior::save | — | 3 |
| Pump | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, PumpRailConnector::save, WakeUpList::save, FluidBox::save | — | 6 |
| Radar | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, WakeUpList::save, <<ForceChartedArea>, RadarControlBehavior::save | — | 6 |
| Rail | EntityWithOwner | ✔ EntityWithOwner | WakeUpList::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, RawPointerToTargetable::save | direction, front | 8 |
| RailChainSignal | RailSignalBase | ✔ RailSignalBase | — | — | 1 |
| RailRemnants | Corpse | ✔ Corpse | — | — | 1 |
| RailSignalBase | EntityWithOwner | ✔ EntityWithOwner | TargeterBase::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned_int>, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, RailSignalBaseControlBehavior::save | state | 12 |
| RailSupport | EntityWithOwner | ✔ EntityWithOwner | — | — | 1 |
| Reactor | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, HeatBuffer::save, WakeUpList::save, NeighbourConnectable::save, ReactorControlBehavior::save, EntityHeaterData::save | — | 2 |
| ResourceEntity | Entity | ✔ Entity | — | — | 4 |
| Roboport | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, LogisticCell::save, WakeUpList::save, LogisticPoint::save, Roboport__RobotRequesterPoint::save, RoboportControlBehavior::save, <<ForceChartedArea> | — | 6 |
| RobotWithLogisticInterface | FlyingRobot | ✔ FlyingRobot | TargeterBase::save, MapPosition::save | — | 5 |
| RocketSilo | AssemblingMachine | ✔ CraftingMachine | AssemblingMachineControlBehavior::save, CyclicFramePosition::save, TargeterBase::save, Inventory::save, <<CargoStation>, LogisticPoint::save, RocketSiloControlBehavior::save | — | 17 |
| RocketSiloRocket | Entity | ✔ Entity | UpdatableEntity::save, CyclicFramePosition::save, MapPosition::save, TargeterBase::save | — | 7 |
| RocketSiloRocketShadow | Entity | ✔ Entity | TargeterBase::save | — | 0 |
| RollingStock | Vehicle | ✔ Vehicle | RollingStock__Carriage::save, TargeterBase::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64> | connected-to-front, front-connection-side, connected-to-back, back-connection-side, position-between-ties | 0 |
| Segment | EntityWithOwner | ✔ EntityWithOwner | <<EffectTriggerer> | segment | 6 |
| SegmentedUnit | Segment | ✔ EntityWithOwner | <<EffectTriggerer>, Shooter::save | segmented-unit, segment | 13 |
| SelectorCombinator | Combinator | ✔ Combinator | SelectorCombinatorControlBehavior::save | — | 0 |
| SimpleEntity | EntityWithHealth | ✔ EntityWithHealth | — | — | 1 |
| SimpleEntityWithOwner | EntityWithOwner | ✔ EntityWithOwner | <<ByteColor> | — | 4 |
| Smoke | Entity | ✔ Entity | UpdatableEntity::save, CyclicFramePosition::save, Vector2<double,Vector>::save | — | 3 |
| SmokeWithTrigger | Smoke | ✔ Entity | UpdatableEntity::save, CyclicFramePosition::save, Vector2<double,Vector>::save, TargeterBase::save, MapPosition::save | — | 7 |
| SolarPanel | EntityWithOwner | ✔ EntityWithOwner | ElectricEnergySource::save | — | 0 |
| SpacePlatformHub | EntityWithOwner | ✔ EntityWithOwner | <<InventoryWithBar>, <<CargoStation>, LogisticPoint::save, WakeUpList::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, Character::save, ID<SpaceLocationPrototype,unsigned_short>::save, SpacePlatformHubControlBehavior::save | — | 4 |
| SpeechBubble | Entity | ✔ Entity | UpdatableEntity::save, LocalisedString::save, TargeterBase::save | — | 2 |
| SpiderLeg | EntityWithOwner | ✔ EntityWithOwner | MapPosition::save | — | 3 |
| SpiderUnit | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, Commandable::save, <<SpiderEngine>, AISettings::save, Vector2<double,Vector>::save, Shooter::save, EnemySpawnerLink::save, <<ForceChartedArea> | enemy-data | 10 |
| SpiderVehicle | Vehicle | ✔ Vehicle | saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, WakeUpList::save, LogisticPoint::save, Vector2<double,Vector>::save, <<SpiderEngine>, MapPosition::save, TargeterBase::save, <<Color> | trash-slots, request-point, provider-point, autopilot, gun-selection, shooting-target | 0 |
| Splitter | SplitterBase | ✔ TransportBeltConnectable | <<SplitterLogic>, SplitterControlBehavior::save, <<SplitterStructureAnimation>, TransportLineConnector::save, MapPosition::save, EntityWithOwner::saveUnitNumber | left-belt, right-belt, connection | 5 |
| Sticker | Entity | ✔ Entity | UpdatableEntity::save, TargeterBase::save, MapPosition::save, <<TriggerModifierData>, <<EffectTriggerer>, Vector2<double,Vector>::save, CyclicFramePosition::save | — | 5 |
| StorageTank | EntityWithOwner | ✔ EntityWithOwner | PipeToGround::save, FluidBox::save, SingleFluidBoxControlBehavior::save | — | 2 |
| TemporaryContainer | ContainerEntity | ✔ ContainerEntity | UpdatableEntity::save | — | 2 |
| Thruster | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, FluidBox::save | — | 1 |
| TileGhost | Ghost | ✔ Entity | — | — | 0 |
| TrackingStation | EntityWithOwner | nunca gravada (aborta) | — | — | 0 |
| TrainStop | EntityWithOwner | ✔ EntityWithOwner | saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, TrainStopControlBehavior::save | station-id, complete | 10 |
| TransportBelt | TransportBeltConnectable | ✔ TransportBeltConnectable | TransportBeltControlBehavior::save | shape | 7 |
| TransportBeltConnectable | EntityWithOwner | ✔ EntityWithOwner | TransportLineConnector::save, EntityWithOwner::saveUnitNumber | connection | 21 |
| Tree | EntityWithHealth | ✔ EntityWithHealth | — | — | 4 |
| Turret | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, Shooter::save, TargeterBase::save, saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>, ID<EntityPrototype,unsigned_short>::save | — | 12 |
| UndergroundBelt | TransportBeltConnectable | ✔ TransportBeltConnectable | TransportLineConnector::save, EntityWithOwner::saveUnitNumber | underground-connection | 6 |
| Unit | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, EnemySpawnerLink::save, AISettings::save, Shooter::save, Vector2<double,Vector>::save, <<ForceChartedArea>, Commandable::save | enemy-data | 16 |
| Valve | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, CyclicFramePosition::save, FluidBox::save | — | 2 |
| Vehicle | EntityWithOwner | ✔ EntityWithOwner | UpdatableEntity::save, EquipmentGrid::save, PortableRoboportProvider::save, VehiclePassenger::save | driver, gunner | 11 |
| Wall | WallConnectable | ✔ WallConnectable | WallControlBehavior::save | — | 2 |
| WallConnectable | EntityWithOwner | ✔ EntityWithOwner | EntityWithOwner::saveUnitNumber | connection | 8 |
