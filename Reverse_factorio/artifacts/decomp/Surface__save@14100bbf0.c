// Surface::save @ 14100bbf0 (function Surface::save)

void Surface__save(longlong param_1,longlong *param_2,uint *param_3,uint param_4,longlong *param_5)

{
  code *pcVar1;
  undefined2 uVar2;
  ulonglong uVar3;
  longlong *plVar4;
  ulonglong uVar5;
  longlong *plVar6;
  longlong *plVar7;
  int iVar8;
  longlong lVar9;
  longlong lVar10;
  longlong lVar11;
  longlong local_res8;
  undefined8 local_res10;
  undefined8 local_b8;
  longlong *plStack_b0;
  int local_a0 [2];
  longlong *local_98;
  int local_90;
  undefined4 uStack_8c;
  longlong *plStack_88;
  longlong *local_80;
  longlong local_78;
  longlong *local_70;
  longlong local_68;
  char *local_60;
  longlong *local_58;
  longlong *local_50;
  longlong local_48;
  
  local_60 = "surface";
  local_res8 = param_1;
  local_58 = param_2;
  if (*(char *)((longlong)param_2 + 0xb) != '\0') {
    Serialiser__calculateWrites(param_2);
    if (param_2[6] != 0) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 4,&DAT_1421da9f0,1);
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (param_2 + 4,"surface",7);
  }
  local_50 = param_2 + 0x28;
  local_68 = *local_50;
  *local_50 = param_1;
  plVar7 = (longlong *)(param_1 + 0x40);
  local_res10 = *(longlong **)(param_1 + 0x50);
  local_48 = local_68;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res10);
  lVar9 = *(longlong *)(param_1 + 0x50);
  if (0xf < *(ulonglong *)(param_1 + 0x58)) {
    plVar7 = (longlong *)*plVar7;
  }
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],plVar7,lVar9);
  param_2[2] = param_2[2] + lVar9;
  LocalisedString__save(param_1 + 0x60,param_2);
  CompiledMapGenSettings__save(*(undefined8 *)(param_1 + 0xd8),param_2);
  plVar7 = (longlong *)param_2[10];
  if (*(char *)(param_1 + 0x820) == '\0') {
    local_res10 = (longlong *)((ulonglong)local_res10._1_7_ << 8);
    (**(code **)(*plVar7 + 8))(plVar7,&local_res10);
    param_2[2] = param_2[2] + 1;
  }
  else {
    local_res10 = (longlong *)CONCAT71(local_res10._1_7_,1);
    (**(code **)(*plVar7 + 8))();
    param_2[2] = param_2[2] + 1;
    MapSerialiser__operator<<<ID<AirbornePollutantPrototype,unsigned_short>_>
              (param_2,param_1 + 0x81e);
  }
  if (*(char *)(param_1 + 0x820) == '\0') {
    if (*(longlong *)(param_1 + 0x38) == 0) {
      if (*(longlong *)(param_1 + 0x30) == 0) {
        uVar2 = *(undefined2 *)(DAT_142d355b8 + 0x1c8);
      }
      else {
        uVar2 = *(undefined2 *)(*(longlong *)(*(longlong *)(param_1 + 0x30) + 0x20) + 0x570);
      }
    }
    else {
      uVar2 = 0;
    }
  }
  else {
    uVar2 = *(undefined2 *)(param_1 + 0x81e);
  }
  local_res10 = (longlong *)CONCAT62(local_res10._2_6_,uVar2);
  MapSerialiser__operator<<<ID<AirbornePollutantPrototype,unsigned_short>_>(param_2,&local_res10);
  local_res10 = (longlong *)
                CONCAT44(local_res10._4_4_,
                         (int)(*(longlong *)(param_1 + 0x108) - *(longlong *)(param_1 + 0x100) >> 3)
                        );
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res10,4);
  param_2[2] = param_2[2] + 4;
  plVar7 = *(longlong **)(param_1 + 0x108);
  for (plVar4 = *(longlong **)(param_1 + 0x100); plVar4 != plVar7; plVar4 = plVar4 + 1) {
    lVar9 = *plVar4;
    local_res10._1_7_ = (uint7)((ulonglong)local_res10 >> 8);
    local_res10 = (longlong *)CONCAT71(local_res10._1_7_,lVar9 != 0);
    (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res10,1);
    param_2[2] = param_2[2] + 1;
    if (lVar9 != 0) {
      PathFinder__save(lVar9,param_2);
    }
  }
  local_res10._4_4_ = (undefined4)((ulonglong)local_res10 >> 0x20);
  local_res10 = (longlong *)
                CONCAT44(local_res10._4_4_,
                         (int)(*(longlong *)(param_1 + 0xf0) - *(longlong *)(param_1 + 0xe8) >> 3));
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res10,4);
  param_2[2] = param_2[2] + 4;
  plVar7 = *(longlong **)(param_1 + 0xf0);
  for (plVar4 = *(longlong **)(param_1 + 0xe8); plVar4 != plVar7; plVar4 = plVar4 + 1) {
    lVar9 = *plVar4;
    local_res10._1_7_ = (uint7)((ulonglong)local_res10 >> 8);
    local_res10 = (longlong *)CONCAT71(local_res10._1_7_,lVar9 != 0);
    (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res10,1);
    param_2[2] = param_2[2] + 1;
    if (lVar9 != 0) {
      Commander__save(lVar9,param_2);
    }
  }
  Surface__saveSegmentedUnits(param_1,param_2);
  MapGenerationManager__save(*(undefined8 *)(param_1 + 0x128),param_2);
  lVar9 = param_2[0x1f];
  if ((lVar9 != 0) && (plVar7 = (longlong *)(lVar9 + 0xa0), (longlong *)*plVar7 != plVar7)) {
    do {
      TileCorruptionDetector__clearDataForChunk(lVar9);
    } while ((longlong *)*plVar7 != plVar7);
  }
  InfiniteRectangle<Chunk_*>__const_iterator__const_iterator(local_a0,param_1 + 0x18);
  if ((longlong *)((longlong)(int)local_80[1] * 0x10 + *local_80) != local_98) {
    plVar7 = local_98;
    iVar8 = local_a0[0];
    do {
      local_res10 = (longlong *)CONCAT44(local_90,iVar8);
      Chunk__save(*plStack_88,param_2,&local_res10);
      if (param_5 != (longlong *)0x0) {
        pcVar1 = *(code **)(*param_5 + 0x10);
        *param_3 = *param_3 + 1;
        (*pcVar1)(param_5,(float)*param_3 / (float)param_4);
      }
      do {
        local_90 = local_90 + 1;
        plStack_88 = plStack_88 + 1;
        if ((longlong *)(*plVar7 + (longlong)(int)plVar7[1] * 8) == plStack_88) {
          plVar4 = plVar7;
          do {
            plVar7 = plVar4 + 2;
            iVar8 = iVar8 + 1;
            if ((longlong *)((longlong)(int)local_80[1] * 0x10 + *local_80) == plVar7)
            goto LAB_14100bfbb;
            plStack_b0 = (longlong *)*plVar7;
            local_90 = -*(int *)((longlong)plVar4 + 0x1c);
            local_b8 = (char *)CONCAT44(local_b8._4_4_,local_90);
            uStack_8c = local_b8._4_4_;
            plVar6 = plVar4 + 3;
            plVar4 = plVar7;
            plStack_88 = plStack_b0;
          } while (((longlong)(int)*plVar6 & 0x1fffffffffffffffU) == 0);
        }
      } while (*plStack_88 == 0);
LAB_14100bfbb:
      param_1 = local_res8;
    } while ((longlong *)((longlong)(int)local_80[1] * 0x10 + *local_80) != plVar7);
  }
  Surface__saveActiveAndPollutedChunksOrders(param_1,param_2);
  Surface__saveActiveTriggers(param_1,param_2);
  local_res10 = (longlong *)CONCAT71(local_res10._1_7_,*(longlong *)(param_1 + 0x750) != 0);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res10,1);
  param_2[2] = param_2[2] + 1;
  if (*(longlong *)(param_1 + 0x750) != 0) {
    FlowStatistics<ID<EntityPrototype,unsigned_short>,double,void>__save
              (*(longlong *)(param_1 + 0x750),param_2);
  }
  if (*(longlong *)(param_1 + 0x7b8) != *(longlong *)(param_1 + 0x7c0)) {
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Particle\\ParticleSurface.cpp"
                        ,0x15,"this->queuedParticles.empty()");
  }
  local_res10 = (longlong *)
                CONCAT44(local_res10._4_4_,
                         (int)(*(longlong *)(param_1 + 0x790) - *(longlong *)(param_1 + 0x788) >> 3)
                        );
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res10,4);
  param_2[2] = param_2[2] + 4;
  local_b8 = *(char **)(param_1 + 0x790);
  local_res10 = *(longlong **)(param_1 + 0x788);
  if (local_res10 != (longlong *)local_b8) {
    do {
      plVar4 = (longlong *)*local_res10;
      plVar7 = plVar4 + 0x30;
      local_70 = plVar7;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],plVar7,4);
      param_2[2] = param_2[2] + 4;
      (**(code **)(*(longlong *)param_2[10] + 8))
                ((longlong *)param_2[10],(longlong)plVar4 + 0x184,4);
      param_2[2] = param_2[2] + 4;
      if (plVar4 != plVar7) {
        do {
          for (plVar6 = plVar4; plVar6 != plVar4 + 0xc; plVar6 = plVar6 + 3) {
            local_78 = (plVar6[1] - *plVar6 >> 3) * 0x6db6db6db6db6db7;
            Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                      (param_2,&local_78);
            lVar9 = plVar6[1];
            for (lVar11 = *plVar6; lVar11 != lVar9; lVar11 = lVar11 + 0x38) {
              Particle__save(lVar11,param_2);
            }
            plVar7 = local_70;
          }
          plVar4 = plVar4 + 0xc;
        } while (plVar4 != plVar7);
      }
      local_res10 = local_res10 + 1;
      param_1 = local_res8;
    } while (local_res10 != (longlong *)local_b8);
  }
  HiddenTilesContainer__save(param_1 + 0x7f8,param_2);
  HiddenTilesContainer__save(param_1 + 0x808,param_2);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x818,1);
  param_2[2] = param_2[2] + 1;
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x819,1);
  param_2[2] = param_2[2] + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x828) != 0);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res8,1);
  param_2[2] = param_2[2] + 1;
  if (*(longlong *)(param_1 + 0x828) != 0) {
    (**(code **)(*(longlong *)param_2[10] + 8))
              ((longlong *)param_2[10],*(longlong *)(param_1 + 0x828),1);
    param_2[2] = param_2[2] + 1;
  }
  MapSerialiser__operator<<<DayTime>(param_2,*(undefined8 *)(param_1 + 0x830));
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x848,8);
  param_2[2] = param_2[2] + 8;
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x850,4);
  param_2[2] = param_2[2] + 4;
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x858,8);
  param_2[2] = param_2[2] + 8;
  Vector2<double,Vector>__save(param_1 + 0x2df0,param_2);
  Vector2<double,Vector>__save(param_1 + 0x2e00,param_2);
  lVar9 = 0;
  iVar8 = 0;
  lVar11 = 0x78;
  do {
    uVar5 = (ulonglong)*(uint *)(param_1 + 0x860) + lVar9;
    uVar3 = uVar5 - 0x78;
    if (uVar5 < 0x78) {
      uVar3 = uVar5;
    }
    Vector2<double,Vector>__save(uVar3 * 0x10 + 0x868 + param_1,param_2);
    lVar9 = lVar9 + 1;
    lVar11 = lVar11 + -1;
  } while (lVar11 != 0);
  SurfaceProperties__save(param_1 + 0x2e10,param_2);
  uVar3 = *(ulonglong *)(param_1 + 0x2fd0);
  if (uVar3 != 0) {
    plVar7 = (longlong *)param_2[0x16];
    plVar4 = (longlong *)plVar7[1];
    plVar6 = plVar7;
    if (*(char *)((longlong)plVar4 + 0x19) == '\0') {
      do {
        plVar7 = plVar4;
        if ((ulonglong)plVar4[4] < uVar3) {
          plVar4 = plVar4 + 2;
          plVar7 = plVar6;
        }
        plVar4 = (longlong *)*plVar4;
        plVar6 = plVar7;
      } while (*(char *)((longlong)plVar4 + 0x19) == '\0');
      plVar6 = (longlong *)param_2[0x16];
    }
    if ((*(char *)((longlong)plVar7 + 0x19) != '\0') || (uVar3 < (ulonglong)plVar7[4])) {
      plVar7 = plVar6;
    }
    if (plVar7 == plVar6) {
      RuntimeError__RuntimeError
                (&local_b8,
                 "ElectricNetworkSerialiser::getElectricNetworkIndex, electric network was not saved."
                );
                    /* WARNING: Subroutine does not return */
      _CxxThrowException(&local_b8,(ThrowInfo *)&DAT_1427b2490);
    }
    iVar8 = (int)plVar7[5];
    if (iVar8 == 0) {
      RuntimeError__RuntimeError(&local_b8,&DAT_1422000f4);
                    /* WARNING: Subroutine does not return */
      _CxxThrowException(&local_b8,(ThrowInfo *)&DAT_1427b2490);
    }
  }
  local_res8 = CONCAT44(local_res8._4_4_,iVar8);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res8,4);
  param_2[2] = param_2[2] + 4;
  MapSerialiser__operator<<<std::set<ChunkPosition,std::less<ChunkPosition>,std::allocator<ChunkPosition>_>_>
            (param_2,param_1 + 0x2e30);
  MapSerialiser__operator<<<Color>(param_2,param_1 + 12000);
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x2fd8) != 0);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res8,1);
  param_2[2] = param_2[2] + 1;
  lVar9 = *(longlong *)(param_1 + 0x2fd8);
  if (lVar9 != 0) {
    (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar9 + 0x18,8);
    param_2[2] = param_2[2] + 8;
    RandomGenerator__save(lVar9 + 0x20,param_2);
  }
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x2fe0) != 0);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res8,1);
  param_2[2] = param_2[2] + 1;
  if (*(longlong *)(param_1 + 0x2fe0) != 0) {
    TerritoryManager__save(*(longlong *)(param_1 + 0x2fe0),param_2);
  }
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x2fe8,1);
  param_2[2] = param_2[2] + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x2ec8) != 0);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res8,1);
  param_2[2] = param_2[2] + 1;
  lVar9 = *(longlong *)(param_1 + 0x2ec8);
  if (lVar9 != 0) {
    (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar9 + 8,8);
    param_2[2] = param_2[2] + 8;
    local_res8 = (*(longlong *)(lVar9 + 0x18) - *(longlong *)(lVar9 + 0x10) >> 3) *
                 0x6db6db6db6db6db7;
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar11 = *(longlong *)(lVar9 + 0x18);
    lVar9 = *(longlong *)(lVar9 + 0x10);
    if (lVar9 != lVar11) {
      lVar10 = lVar9 + 0x28;
      do {
        TargeterBase__save(lVar9,param_2);
        (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar10 + -8,4);
        param_2[2] = param_2[2] + 4;
        (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar10 + -4,4);
        param_2[2] = param_2[2] + 4;
        (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar10,4);
        param_2[2] = param_2[2] + 4;
        (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar10 + 4,4);
        param_2[2] = param_2[2] + 4;
        (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar10 + 8,1);
        param_2[2] = param_2[2] + 1;
        lVar9 = lVar9 + 0x38;
        lVar10 = lVar10 + 0x38;
      } while (lVar9 != lVar11);
    }
  }
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x3020) != 0);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&local_res8,1);
  param_2[2] = param_2[2] + 1;
  lVar9 = *(longlong *)(param_1 + 0x3020);
  if (lVar9 != 0) {
    local_b8 = "surface-effect-source";
    plStack_b0 = param_2;
    if ((char)param_2[1] != '\0') {
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_1421e010c,1);
      param_2[2] = param_2[2] + 1;
      (**(code **)(*(longlong *)param_2[10] + 8))
                ((longlong *)param_2[10],"surface-effect-source",0x15);
      param_2[2] = param_2[2] + 0x15;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_1421e0108,1);
      param_2[2] = param_2[2] + 1;
    }
    Effect__save(lVar9 + 8,param_2);
    if ((char)param_2[1] != '\0') {
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_14230f710,2);
      param_2[2] = param_2[2] + 2;
      (**(code **)(*(longlong *)param_2[10] + 8))
                ((longlong *)param_2[10],"surface-effect-source",0x15);
      param_2[2] = param_2[2] + 0x15;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_1421e0108,1);
      param_2[2] = param_2[2] + 1;
    }
  }
  local_res8 = *(longlong *)(param_1 + 0x2fb0) - *(longlong *)(param_1 + 0x2fa8) >> 4;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar11 = *(longlong *)(param_1 + 0x2fb0);
  for (lVar9 = *(longlong *)(param_1 + 0x2fa8); lVar9 != lVar11; lVar9 = lVar9 + 0x10) {
    (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],lVar9,1);
    param_2[2] = param_2[2] + 1;
    SurfaceCargoLogistics__save(*(undefined8 *)(lVar9 + 8),param_2);
  }
  MapSerialiser__operator<<<std::map<ForceID,std::map<ID<TilePrototype,unsigned_short>,ID<TilePrototype,unsigned_short>,std::less<ID<TilePrototype,unsigned_short>_>,std::allocator<std::pair<ID<TilePrototype,unsigned_short>_const_,ID<TilePrototype,unsigned_short>_>_>_>,std::less<ForceID>,std::allocator<std::pair<ForceID_const_,std::map<ID<TilePrototype,unsigned_short>,ID<TilePrototype,unsigned_short>,std::less<ID<TilePrototype,unsigned_short>_>,std::allocator<std::pair<ID<TilePrototype,unsigned_short>_const_,ID<TilePrototype,unsigned_short>_>_>_>_>_>_>_>
            (param_2,param_1 + 0x3060);
  (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],param_1 + 0x2fc0,8);
  param_2[2] = param_2[2] + 8;
  if (*(char *)((longlong)param_2 + 9) != '\0') {
    local_b8 = "complete";
    plStack_b0 = param_2;
    if ((char)param_2[1] != '\0') {
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_1421e010c,1);
      param_2[2] = param_2[2] + 1;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],"complete",8);
      param_2[2] = param_2[2] + 8;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_1421e0108,1);
      param_2[2] = param_2[2] + 1;
    }
    MapSerialiser__operator<<<std::set<ChunkPosition,std::less<ChunkPosition>,std::allocator<ChunkPosition>_>_>
              (param_2,param_1 + 0x2f98);
    if ((char)param_2[1] != '\0') {
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_14230f710,2);
      param_2[2] = param_2[2] + 2;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],"complete",8);
      param_2[2] = param_2[2] + 8;
      (**(code **)(*(longlong *)param_2[10] + 8))((longlong *)param_2[10],&DAT_1421e0108,1);
      param_2[2] = param_2[2] + 1;
    }
  }
  param_2[0x28] = local_68;
  Serialiser__SavingStatsWriter___SavingStatsWriter(&local_60);
  return;
}

