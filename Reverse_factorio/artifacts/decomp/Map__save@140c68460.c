// Map::save @ 140c68460 (function Map::save)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void Map__save(longlong param_1,longlong param_2,undefined8 param_3)

{
  char cVar1;
  undefined1 auVar2 [16];
  undefined4 uVar3;
  undefined8 uVar4;
  ulonglong uVar5;
  undefined8 *puVar6;
  longlong *plVar7;
  longlong lVar8;
  char *pcVar9;
  undefined8 *puVar10;
  longlong *plVar11;
  longlong *plVar12;
  undefined8 *puVar13;
  longlong lVar14;
  char *pcVar15;
  longlong lVar16;
  uint uVar17;
  undefined8 *puVar18;
  undefined8 *puVar19;
  undefined8 *puVar20;
  undefined8 local_res8;
  char *local_a8;
  longlong local_a0;
  longlong local_98;
  longlong local_90;
  char *local_88;
  longlong local_80;
  undefined8 local_78;
  uint uStack_70;
  undefined4 uStack_6c;
  undefined8 local_68;
  undefined8 local_60;
  undefined8 local_58;
  
  local_78 = CONCAT44(_UNK_14217c57c,_DAT_14217c578);
  uStack_70 = (undefined4)DAT_14217c580;
  uStack_6c = DAT_14217c580._4_4_;
  local_68 = *(undefined8 *)(param_1 + 0x1a8);
  local_60 = *(undefined8 *)(param_1 + 0x1b0);
  local_58 = *(undefined8 *)(param_1 + 0x1b8);
  local_98 = param_2;
  local_90 = param_1;
  MapHeader__save(&local_78);
  MapGenSettings__save(*(undefined8 *)(param_1 + 0x2a0),param_2);
  MapSettings__save(*(undefined8 *)(param_1 + 0x238),param_2);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"random-generators");
  RandomGenerator__save(param_1 + 0x434,param_2);
  RandomGenerator__save(param_1 + 0x440,param_2);
  RandomGenerator__save(param_1 + 0x44c,param_2);
  RandomGenerator__save(param_1 + 0x458,param_2);
  RandomGenerator__save(param_1 + 0x464,param_2);
  RandomGenerator__save(param_1 + 0x470,param_2);
  Serialiser__TagWriter___TagWriter(&local_a8);
  lVar8 = *(longlong *)(param_1 + 0x1c0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar8 + 0x60,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar8 + 100,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  local_res8 = (undefined8 *)param_2;
  MplVector<PrototypeList<CustomInputPrototype>,PrototypeList<EquipmentGridPrototype>,PrototypeList<ItemPrototype>,PrototypeList<CollisionLayerPrototype>,PrototypeList<AirbornePollutantPrototype>,PrototypeList<TilePrototype>,PrototypeList<DecorativePrototype>,PrototypeList<TechnologyPrototype>,PrototypeList<EntityPrototype>,PrototypeList<ParticlePrototype>,PrototypeList<RecipeCategory>,PrototypeList<ItemSubGroup>,PrototypeList<ItemGroup>,PrototypeList<FluidPrototype>,PrototypeList<VirtualSignalPrototype>,PrototypeList<AmmoCategory>,PrototypeList<FuelCategory>,PrototypeList<ResourceCategory>,PrototypeList<EquipmentPrototype>,PrototypeList<NamedNoiseFunction>,PrototypeList<NamedNoiseExpression>,PrototypeList<AutoplaceControl>,PrototypeList<DamageType>,PrototypeList<RecipePrototype>,PrototypeList<AchievementPrototype>,PrototypeList<ModuleCategory>,PrototypeList<EquipmentCategory>,PrototypeList<ModSettingPrototype>,PrototypeList<TrivialSmokePrototype>,PrototypeList<AsteroidChunkPrototype>,PrototypeList<QualityPrototype>,PrototypeList<SurfacePropertyPrototype>,PrototypeList<SpaceLocationPrototype>,PrototypeList<SpaceConnectionPrototype>,PrototypeList<ProcessionLayerInheritanceGroup>,PrototypeList<ProcessionPrototype>,PrototypeList<ActiveTriggerPrototype>,PrototypeList<ShortcutPrototype>,PrototypeList<BurnerUsagePrototype>,PrototypeList<SurfacePrototype>,PrototypeList<ModData>,PrototypeList<CustomEventPrototype>_>__forEach<_Map::save_::_2_::<lambda_2>_>
            (&local_res8);
  PrototypeMigrationListDefinition__save(*(undefined8 *)(DAT_142d34fa0 + 0x120),param_2);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"active-entities-count");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x88,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"fully-active-segmented-units-count");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x94,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"minimally-active-segmented-units-count");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x98,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"asleep-segmented-units-count");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x9c,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-unit-number");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x208,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-targetable-item-number");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x210,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-script-path-find-id");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x218,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-unique-item-id");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x21c,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-logistic-network-id");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x224,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-space-platform-index");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x228,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  Serialiser__TagWriter__TagWriter(&local_a8,param_2,"next-equipment-grid-id");
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x22c,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Serialiser__TagWriter___TagWriter(&local_a8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x230,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  MapModSettings__save(*(undefined8 *)(param_1 + 0x198),param_2);
  Planet__save(param_1 + 0x2a8,param_2);
  TrainManager__save(*(undefined8 *)(param_1 + 0x1a0),param_2);
  puVar13 = *(undefined8 **)(param_1 + 0x318);
  local_res8 = puVar13;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar20 = (undefined8 *)0x0;
  puVar19 = puVar20;
  if (puVar13 != (undefined8 *)0x0) {
    do {
      uVar5 = *(longlong *)(param_1 + 0x310) + (longlong)puVar19;
      auVar2._8_8_ = 0;
      auVar2._0_8_ = uVar5;
      lVar8 = SUB168(ZEXT816(0x4e5e0a72f0539783) * auVar2,8);
      puVar18 = (undefined8 *)
                (*(longlong *)(param_1 + 0x308) +
                (uVar5 + ((uVar5 - lVar8 >> 1) + lVar8 >> 7) * -0xc4) * 0x10);
      local_res8 = puVar20;
      for (puVar10 = (undefined8 *)*puVar18; puVar10 != puVar18; puVar10 = (undefined8 *)*puVar10) {
        local_res8 = (undefined8 *)((longlong)local_res8 + 1);
      }
      Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                (param_2,&local_res8);
      for (puVar10 = (undefined8 *)*puVar18; puVar10 != puVar18; puVar10 = (undefined8 *)*puVar10) {
        puVar6 = puVar10 + -1;
        if (puVar10 == (undefined8 *)0x0) {
          puVar6 = puVar20;
        }
        uVar4 = (**(code **)*puVar6)();
        RawPointerToTargetable__save(param_2,uVar4);
      }
      puVar19 = (undefined8 *)((longlong)puVar19 + 1);
    } while (puVar19 < puVar13);
  }
  ForceManager__save(param_1 + 0x380,param_2);
  lVar8 = *(longlong *)(param_1 + 0x178);
  Serialiser__TagWriter__TagWriter
            (&local_a8,param_2,"ControlBehaviorManager","a=%zu",
             *(longlong *)(lVar8 + 0x18) - *(longlong *)(lVar8 + 0x10) >> 3);
  local_res8 = (undefined8 *)(*(longlong *)(lVar8 + 0x18) - *(longlong *)(lVar8 + 0x10) >> 3);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar13 = *(undefined8 **)(lVar8 + 0x18);
  for (puVar19 = *(undefined8 **)(lVar8 + 0x10); puVar19 != puVar13; puVar19 = puVar19 + 1) {
    lVar8 = (*(code *)**(undefined8 **)*puVar19)();
    RawPointerToTargetable__save(param_2,*(undefined8 *)(lVar8 + 8));
  }
  Serialiser__TagWriter___TagWriter(&local_a8);
  CircuitNetworkManager__save(*(undefined8 *)(param_1 + 0x180),param_2);
  ItemSpoilQueue__save(param_1 + 0x6b8,param_2);
  puVar13 = *(undefined8 **)(param_1 + 0x488);
  local_res8 = (undefined8 *)CONCAT44(local_res8._4_4_,*(undefined4 *)(puVar13 + 1));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  plVar7 = *(longlong **)*puVar13;
  cVar1 = *(char *)((longlong)plVar7 + 0x19);
  while (cVar1 == '\0') {
    lVar8 = plVar7[8];
    TargeterBase__save(lVar8,param_2);
    MapSerialiser__operator<<<SizeLimitedString<500>_>(param_2,lVar8 + 0x20);
    plVar11 = (longlong *)plVar7[2];
    if (*(char *)((longlong)plVar11 + 0x19) == '\0') {
      cVar1 = *(char *)(*plVar11 + 0x19);
      plVar7 = plVar11;
      plVar11 = (longlong *)*plVar11;
      while (cVar1 == '\0') {
        cVar1 = *(char *)(*plVar11 + 0x19);
        plVar7 = plVar11;
        plVar11 = (longlong *)*plVar11;
      }
    }
    else {
      cVar1 = *(char *)(plVar7[1] + 0x19);
      plVar12 = (longlong *)plVar7[1];
      plVar11 = plVar7;
      while ((plVar7 = plVar12, cVar1 == '\0' && (plVar11 == (longlong *)plVar7[2]))) {
        cVar1 = *(char *)(plVar7[1] + 0x19);
        plVar12 = (longlong *)plVar7[1];
        plVar11 = plVar7;
      }
    }
    cVar1 = *(char *)((longlong)plVar7 + 0x19);
  }
  ExtraScriptData__save(*(undefined8 *)(param_1 + 0x490),param_2);
  ScriptRendering__save(*(undefined8 *)(param_1 + 0x498),param_2);
  ElectricNetworkManager__save(*(undefined8 *)(param_1 + 0x150),param_2);
  FluidSegmentManager__save(*(undefined8 *)(param_1 + 0x158),param_2);
  local_res8 = (undefined8 *)
               CONCAT44(local_res8._4_4_,
                        (int)(*(longlong *)(*(longlong *)(param_1 + 0x160) + 0x28) -
                              *(longlong *)(*(longlong *)(param_1 + 0x160) + 0x20) >> 3));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  lVar8 = *(longlong *)(param_1 + 0x168);
  local_a8 = "heat-buffer-manager";
  local_a0 = param_2;
  if (*(char *)(param_2 + 0xb) != '\0') {
    Serialiser__calculateWrites(param_2);
    if (*(longlong *)(param_2 + 0x30) != 0) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 0x20,&DAT_1421da9f0,1);
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (param_2 + 0x20,"heat-buffer-manager",0x13);
  }
  MapSerialiser__operator<<<HeatBufferGroup>(param_2,lVar8 + 8);
  local_res8 = (undefined8 *)(*(longlong *)(lVar8 + 0x68) - *(longlong *)(lVar8 + 0x60) >> 3);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar13 = *(undefined8 **)(lVar8 + 0x68);
  for (puVar19 = *(undefined8 **)(lVar8 + 0x60); puVar19 != puVar13; puVar19 = puVar19 + 1) {
    MapSerialiser__operator<<<HeatBufferGroup>(param_2,*puVar19);
  }
  Serialiser__SavingStatsWriter___SavingStatsWriter(&local_a8);
  ExtraScriptData__saveInventories(*(undefined8 *)(param_1 + 0x490),param_2);
  local_a8 = "force-manager-linked-inventories";
  local_a0 = param_2;
  if (*(char *)(param_2 + 0xb) != '\0') {
    Serialiser__calculateWrites(param_2);
    if (*(longlong *)(param_2 + 0x30) != 0) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 0x20,&DAT_1421da9f0,1);
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (param_2 + 0x20,"force-manager-linked-inventories",0x20);
  }
  plVar7 = *(longlong **)(param_1 + 0x388);
  for (plVar11 = *(longlong **)(param_1 + 0x380); plVar11 != plVar7; plVar11 = plVar11 + 1) {
    LinkedInventories__save(*(undefined8 *)(*plVar11 + 0x6d8),param_2);
  }
  Serialiser__SavingStatsWriter___SavingStatsWriter(&local_a8);
  local_res8 = (undefined8 *)CONCAT62(local_res8._2_6_,(short)(DAT_1429c51f8 - DAT_1429c51f0 >> 3));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  puVar13 = puVar20;
  if (DAT_1429c51f8 - DAT_1429c51f0 >> 3 != 0) {
    do {
      lVar8 = *(longlong *)(DAT_1429c51f0 + (longlong)puVar13 * 8);
      if (lVar8 == 0) {
        local_res8 = (undefined8 *)((ulonglong)local_res8._2_6_ << 0x10);
      }
      else {
        local_res8 = (undefined8 *)CONCAT62(local_res8._2_6_,*(undefined2 *)(lVar8 + 0x1f8));
      }
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&local_res8,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      puVar13 = (undefined8 *)((longlong)puVar13 + 1);
    } while (puVar13 < (undefined8 *)(DAT_1429c51f8 - DAT_1429c51f0 >> 3));
  }
  uVar3 = TechnologyPrototype__getComputedTechTreeCrc();
  local_res8._0_4_ = uVar3;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  local_res8._0_4_ = 0;
  puVar13 = puVar20;
  for (plVar7 = *(longlong **)(param_1 + 0x2c0); plVar7 != *(longlong **)(param_1 + 0x2c8);
      plVar7 = plVar7 + 1) {
    if (*plVar7 != 0) {
      local_res8._0_4_ = (int)puVar13 + 1;
      puVar13 = (undefined8 *)(ulonglong)(uint)local_res8;
    }
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  plVar7 = *(longlong **)(param_1 + 0x2c8);
  for (plVar11 = *(longlong **)(param_1 + 0x2c0); plVar11 != plVar7; plVar11 = plVar11 + 1) {
    if (*plVar11 != 0) {
      Surface__preSave(*plVar11,param_2);
    }
  }
  uVar17 = 0;
  uStack_70 = 0;
  local_78 = param_1;
  InfiniteRectangle<Chunk_*>__const_iterator__const_iterator
            (&local_68,**(longlong **)(param_1 + 0x2c0) + 0x18);
  ChunksInMapIterator__findNextSurface(&local_78);
  puVar13 = puVar20;
  if ((ulonglong)uStack_70 <
      (ulonglong)(*(longlong *)(local_78 + 0x2c8) - *(longlong *)(local_78 + 0x2c0) >> 3)) {
    do {
      uVar17 = (int)puVar13 + 1;
      puVar13 = (undefined8 *)(ulonglong)uVar17;
      InfiniteRectangle<Chunk_*>__const_iterator__operator__(&local_68);
      ChunksInMapIterator__findNextSurface(&local_78);
    } while ((ulonglong)uStack_70 <
             (ulonglong)(*(longlong *)(local_78 + 0x2c8) - *(longlong *)(local_78 + 0x2c0) >> 3));
  }
  local_res8 = (undefined8 *)((ulonglong)local_res8._4_4_ << 0x20);
  plVar7 = *(longlong **)(param_1 + 0x2c8);
  for (plVar11 = *(longlong **)(param_1 + 0x2c0); plVar11 != plVar7; plVar11 = plVar11 + 1) {
    if (*plVar11 != 0) {
      Surface__save(*plVar11,param_2,&local_res8,uVar17,param_3);
    }
  }
  TransportLineSerialiser__save(param_2 + 0xc0,param_1,param_2);
  local_a8 = "surface-delete-requests";
  local_a0 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"surface-delete-requests",0x17);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x17;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_res8 = (undefined8 *)(*(longlong *)(param_1 + 0x2e0) - *(longlong *)(param_1 + 0x2d8) >> 2);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar8 = *(longlong *)(param_1 + 0x2e0);
  for (lVar14 = *(longlong *)(param_1 + 0x2d8); lVar14 != lVar8; lVar14 = lVar14 + 4) {
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned_int>(param_2,lVar14);
  }
  pcVar9 = (char *)(param_2 + 8);
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"surface-delete-requests",0x17);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x17;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "platform-delete-requests";
  local_a0 = param_2;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"platform-delete-requests",0x18);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x18;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  MapSerialiser__operator<<<std::vector<Targeter<CargoBay,19,16>,std::allocator<Targeter<CargoBay,19,16>_>_>_>
            (param_2,param_1 + 0x2f0);
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"platform-delete-requests",0x18);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x18;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  pcVar9 = (char *)(param_2 + 8);
  local_a8 = "console-command-used";
  local_a0 = param_2;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"console-command-used",0x14);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x266,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"console-command-used",0x14);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "editor-used";
  local_a0 = param_2;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"editor-used",0xb);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xb;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  pcVar15 = (char *)(param_2 + 8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x267,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"editor-used",0xb);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xb;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "allow-tip-activation";
  local_a0 = param_2;
  if (*pcVar15 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"allow-tip-activation",0x14);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x268,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*pcVar15 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"allow-tip-activation",0x14);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  pcVar9 = (char *)(param_2 + 8);
  local_a8 = "tick-warned-about-console-command-disabling-achievements";
  local_a0 = param_2;
  if (*pcVar15 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),
               "tick-warned-about-console-command-disabling-achievements",0x38);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x38;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x270,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),
               "tick-warned-about-console-command-disabling-achievements",0x38);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x38;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "tick-warned-about-editor-disabling-achievements";
  local_a0 = param_2;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-warned-about-editor-disabling-achievements",0x2f
              );
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x2f;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  pcVar15 = (char *)(param_2 + 8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x278,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-warned-about-editor-disabling-achievements",0x2f
              );
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x2f;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "tick-Last-warned-about-cheat-disabling-achievements";
  local_a0 = param_2;
  if (*pcVar15 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-Last-warned-about-cheat-disabling-achievements",
               0x33);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x33;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x280,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*pcVar15 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-Last-warned-about-cheat-disabling-achievements",
               0x33);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x33;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x288,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  local_a8 = "is-multiplayer";
  local_a0 = param_2;
  if (*pcVar15 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"is-multiplayer",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x290,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"is-multiplayer",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "remove-all-players";
  local_a0 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"remove-all-players",0x12);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x12;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  pcVar9 = (char *)(param_2 + 10);
  if (*pcVar9 == '\0') {
    pcVar9 = (char *)(param_1 + 0x292);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),pcVar9,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"remove-all-players",0x12);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x12;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  pcVar9 = (char *)(param_2 + 8);
  if (*(char *)(param_2 + 9) != '\0') {
    local_a8 = "chunks-with-tiles";
    local_a0 = param_2;
    if (*pcVar9 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"chunks-with-tiles",0x11);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x24c,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*pcVar9 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"chunks-with-tiles",0x11);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_a8 = "chunks-with-entities";
    local_a0 = param_2;
    if (*pcVar9 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"chunks-with-entities",0x14);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x250,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"chunks-with-entities",0x14);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  local_res8 = (undefined8 *)
               CONCAT44(local_res8._4_4_,
                        (int)(*(longlong *)(param_1 + 0x1d0) - *(longlong *)(param_1 + 0x1c8) >> 3))
  ;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  local_a8 = "players";
  local_a0 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"players",7);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 7;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  puVar13 = *(undefined8 **)(param_1 + 0x1d0);
  for (puVar19 = *(undefined8 **)(param_1 + 0x1c8); puVar19 != puVar13; puVar19 = puVar19 + 1) {
    Player__save(*puVar19,param_2);
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"players",7);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 7;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_a8 = "fake-players";
  local_a0 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"fake-players",0xc);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xc;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  lVar8 = *(longlong *)(param_1 + 0x1f8);
  local_res8 = (undefined8 *)
               ((*(longlong *)(lVar8 + 0x68) - *(longlong *)(lVar8 + 0x60) >> 5) *
               -0x5555555555555555);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar13 = *(undefined8 **)(lVar8 + 0x68);
  for (puVar19 = *(undefined8 **)(lVar8 + 0x60); puVar19 != puVar13; puVar19 = puVar19 + 0xc) {
    local_res8 = (undefined8 *)puVar19[2];
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar8 = puVar19[2];
    puVar10 = puVar19;
    if (0xf < (ulonglong)puVar19[3]) {
      puVar10 = (undefined8 *)*puVar19;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),puVar10,lVar8)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar8;
    local_res8 = (undefined8 *)puVar19[10];
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar8 = puVar19[10];
    plVar7 = puVar19 + 8;
    if (0xf < (ulonglong)puVar19[0xb]) {
      plVar7 = (longlong *)*plVar7;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar7,lVar8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar8;
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"fake-players",0xc);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xc;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  puVar13 = *(undefined8 **)(param_1 + 0x4a0);
  local_res8 = (undefined8 *)puVar13[1];
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  plVar7 = *(longlong **)*puVar13;
  cVar1 = *(char *)((longlong)plVar7 + 0x19);
  while (cVar1 == '\0') {
    lVar14 = MapSerialiser__operator<<<Mod>(param_2,plVar7[4]);
    local_res8 = (undefined8 *)plVar7[7];
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(lVar14,&local_res8);
    lVar8 = plVar7[7];
    plVar11 = plVar7 + 5;
    if (0xf < (ulonglong)plVar7[8]) {
      plVar11 = (longlong *)*plVar11;
    }
    (**(code **)(**(longlong **)(lVar14 + 0x50) + 8))(*(longlong **)(lVar14 + 0x50),plVar11,lVar8);
    *(longlong *)(lVar14 + 0x10) = *(longlong *)(lVar14 + 0x10) + lVar8;
    plVar11 = (longlong *)plVar7[2];
    if (*(char *)((longlong)plVar11 + 0x19) == '\0') {
      cVar1 = *(char *)(*plVar11 + 0x19);
      plVar7 = plVar11;
      plVar11 = (longlong *)*plVar11;
      while (cVar1 == '\0') {
        cVar1 = *(char *)(*plVar11 + 0x19);
        plVar7 = plVar11;
        plVar11 = (longlong *)*plVar11;
      }
    }
    else {
      cVar1 = *(char *)(plVar7[1] + 0x19);
      plVar12 = (longlong *)plVar7[1];
      plVar11 = plVar7;
      while ((plVar7 = plVar12, cVar1 == '\0' && (plVar11 == (longlong *)plVar7[2]))) {
        cVar1 = *(char *)(plVar7[1] + 0x19);
        plVar12 = (longlong *)plVar7[1];
        plVar11 = plVar7;
      }
    }
    cVar1 = *(char *)((longlong)plVar7 + 0x19);
  }
  lVar8 = *(longlong *)(param_1 + 0x240);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar8 + 0x61,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar8 + 0x68,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  AchievementStats__save(*(undefined8 *)(param_1 + 0x508),param_2);
  local_a8 = "blueprint-library";
  local_a0 = param_2;
  if (*(char *)(param_2 + 0xb) != '\0') {
    Serialiser__calculateWrites(param_2);
    if (*(longlong *)(param_2 + 0x30) != 0) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 0x20,&DAT_1421da9f0,1);
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (param_2 + 0x20,"blueprint-library",0x11);
  }
  local_88 = "blueprint-library";
  local_80 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"blueprint-library",0x11);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  BlueprintLibrary__save(param_1 + 0x510,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"blueprint-library",0x11);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  Serialiser__SavingStatsWriter___SavingStatsWriter(&local_a8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x293,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x294,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x295,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x296,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_88 = "permission-groups";
  local_80 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"permission-groups",0x11);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  lVar8 = *(longlong *)(param_1 + 0x680);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar8 + 0x10,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  local_res8 = (undefined8 *)
               CONCAT44(local_res8._4_4_,
                        (int)(*(longlong *)(lVar8 + 0x20) - *(longlong *)(lVar8 + 0x18) >> 3));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  plVar7 = *(longlong **)(lVar8 + 0x20);
  for (plVar11 = *(longlong **)(lVar8 + 0x18); plVar11 != plVar7; plVar11 = plVar11 + 1) {
    lVar8 = *plVar11;
    Targetable__save(lVar8,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),lVar8 + 0x20,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    local_res8 = *(undefined8 **)(lVar8 + 0x38);
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar14 = *(longlong *)(lVar8 + 0x38);
    plVar12 = (longlong *)(lVar8 + 0x28);
    if (0xf < *(ulonglong *)(lVar8 + 0x40)) {
      plVar12 = (longlong *)*plVar12;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),plVar12,lVar14);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar14;
    Permissions__ByteIndexedBitset__save(lVar8 + 0x48,param_2);
    local_res8 = (undefined8 *)(*(longlong *)(lVar8 + 0x80) - *(longlong *)(lVar8 + 0x78) >> 5);
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar14 = *(longlong *)(lVar8 + 0x80);
    for (lVar8 = *(longlong *)(lVar8 + 0x78); lVar8 != lVar14; lVar8 = lVar8 + 0x20) {
      TargeterBase__save(lVar8,param_2);
    }
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"permission-groups",0x11);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  ScenarioHistory__save(*(undefined8 *)(param_1 + 0x4a8),param_2);
  MapSavedSpecialItems__save(*(undefined8 *)(param_1 + 0x6a8),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x708,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x709,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x70a,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x70b,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x70c,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x70d,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x710,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  TrashLogisticScheduler__save(param_1 + 0x720,param_2);
  FluidBoxConnectionSerialiser__save(*(undefined8 *)(param_2 + 0x90),param_2);
  lVar8 = *(longlong *)(param_1 + 0x6b0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  plVar7 = (longlong *)(lVar8 + 8);
  if (plVar7 == (longlong *)(lVar8 + 0x1808)) {
    local_res8 = (undefined8 *)0x0;
    uVar3 = Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                      (param_2,&local_res8);
  }
  else {
    do {
      puVar20 = (undefined8 *)((longlong)puVar20 + (plVar7[1] - *plVar7 >> 4) * -0x5555555555555555)
      ;
      plVar7 = plVar7 + 3;
    } while (plVar7 != (longlong *)(lVar8 + 0x1808));
    local_res8 = puVar20;
    uVar3 = Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                      (param_2,&local_res8);
    plVar7 = (longlong *)(lVar8 + 8);
    do {
      lVar14 = plVar7[1];
      for (lVar16 = *plVar7; lVar16 != lVar14; lVar16 = lVar16 + 0x30) {
        TargeterBase__save(lVar16,param_2);
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),lVar16 + 0x20,8);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
        local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(undefined1 *)(lVar16 + 0x28));
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),&local_res8,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        uVar3 = (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),lVar16 + 0x2c,4);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
      }
      plVar7 = plVar7 + 3;
    } while (plVar7 != (longlong *)(lVar8 + 0x1808));
  }
  lVar8 = *(longlong *)(param_1 + 0x738);
  uVar3 = AttachedDataManager__saveList<IntrusiveListImplementation<EntityAttachedData,void,0,0>::List>
                    (uVar3,param_2,lVar8);
  AttachedDataManager__saveList<IntrusiveListImplementation<EntityAttachedData,void,0,0>::List>
            (uVar3,param_2,lVar8 + 0x10);
  HealingManager__save(*(undefined8 *)(param_1 + 400),param_2);
  MapSerialiser__saveSaveHelpers(param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"postSaveHook",0xc);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xc;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  plVar7 = *(longlong **)(param_2 + 0x128);
  for (plVar11 = *(longlong **)(param_2 + 0x120); plVar11 != plVar7; plVar11 = plVar11 + 1) {
    *(ushort *)(*plVar11 + 0x6e) = *(ushort *)(*plVar11 + 0x6e) & 0xefff;
  }
  if (*(longlong *)(param_2 + 0x120) != *(longlong *)(param_2 + 0x128)) {
    *(longlong *)(param_2 + 0x128) = *(longlong *)(param_2 + 0x120);
  }
  if (*(longlong *)(param_1 + 0x528) != 0) {
    BlueprintShelf__saveToStorage();
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"postSaveHook",0xc);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xc;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

