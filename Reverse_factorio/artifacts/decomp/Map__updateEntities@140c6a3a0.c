// Map::updateEntities @ 140c6a3a0 (function Map::updateEntities)

void Map__updateEntities(longlong param_1)

{
  longlong *plVar1;
  undefined1 uVar2;
  int iVar3;
  char cVar4;
  longlong *plVar5;
  longlong *plVar6;
  longlong *plVar7;
  undefined8 uVar8;
  longlong lVar9;
  undefined8 *puVar10;
  undefined8 *puVar11;
  longlong lVar12;
  byte bVar13;
  ulonglong uVar14;
  undefined8 *puVar15;
  undefined8 *puVar16;
  longlong lVar17;
  ulonglong uVar18;
  undefined1 *puVar19;
  undefined4 extraout_XMM0_Da;
  undefined4 uVar20;
  longlong *local_res8;
  longlong **local_78;
  undefined1 *puStack_70;
  undefined8 local_68;
  longlong *local_60;
  undefined8 local_58;
  longlong *local_50;
  undefined8 local_48;
  undefined8 local_40;
  undefined8 local_38;
  undefined1 local_30 [8];
  
  if ((*(char *)(*(longlong *)(param_1 + 0x1c0) + 0x60) == '\0') ||
     (*(int *)(*(longlong *)(param_1 + 0x1c0) + 100) != 0)) {
    AlarmQueue__update(*(undefined8 *)(param_1 + 0x6b0),*(undefined8 *)(param_1 + 0x1b0));
    local_68 = (longlong *)CONCAT44(local_68._4_4_,0x30);
    plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res8);
    lVar9 = *plVar5;
    local_res8 = *(longlong **)(param_1 + 0x170);
    local_78 = &local_res8;
    puStack_70 = &stack0x00000010;
    plVar5 = *(longlong **)(param_1 + 0x148);
    local_60 = (longlong *)lVar9;
    if (plVar5 == (longlong *)0x0) {
                    /* WARNING: Subroutine does not return */
      std___Xbad_function_call();
    }
    (**(code **)(*plVar5 + 0x10))(plVar5,&local_78);
    plVar6 = (longlong *)std__chrono__steady_clock__now(&local_res8);
    LOCK();
    plVar5 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x50);
    *plVar5 = *plVar5 + (*plVar6 - lVar9);
    UNLOCK();
    FluidSegmentManager__update(*(undefined8 *)(param_1 + 0x158));
    lVar9 = *(longlong *)(param_1 + 0x188);
    if (*(longlong *)(lVar9 + 0x10) != *(longlong *)(lVar9 + 0x18)) {
                    /* WARNING: Subroutine does not return */
      ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Map\\Map.cpp"
                          ,0x53a,"this->turretTargetManager->turretsWantingTarget.empty()");
    }
    puVar19 = (undefined1 *)(lVar9 + 8);
    uVar2 = *puVar19;
    local_60 = (longlong *)CONCAT71(local_60._1_7_,uVar2);
    *puVar19 = 1;
    uVar18 = *(longlong *)(param_1 + 0x2c8) - *(longlong *)(param_1 + 0x2c0) >> 3;
    uVar14 = 0;
    local_68 = (longlong *)puVar19;
    if (uVar18 != 0) {
      do {
        if (*(longlong *)(*(longlong *)(param_1 + 0x2c0) + uVar14 * 8) != 0) {
          Surface__update();
        }
        uVar14 = uVar14 + 1;
      } while (uVar14 < uVar18);
    }
    Map__performPlannedEntityUpdates(param_1);
    *puVar19 = uVar2;
    if (*(longlong *)(*(longlong *)(param_1 + 0x188) + 0x10) !=
        *(longlong *)(*(longlong *)(param_1 + 0x188) + 0x18)) {
      local_68 = (longlong *)CONCAT44(local_68._4_4_,0x48);
      plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      lVar9 = *plVar5;
      local_res8 = *(longlong **)(param_1 + 0x188);
      local_78 = &local_res8;
      puStack_70 = &stack0x00000010;
      plVar5 = *(longlong **)(param_1 + 0x148);
      local_60 = (longlong *)lVar9;
      if (plVar5 == (longlong *)0x0) {
                    /* WARNING: Subroutine does not return */
        std___Xbad_function_call();
      }
      (**(code **)(*plVar5 + 0x10))(plVar5,&local_78);
      plVar6 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      LOCK();
      plVar5 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x68);
      *plVar5 = *plVar5 + (*plVar6 - lVar9);
      UNLOCK();
    }
    TrainManager__update(*(undefined8 *)(param_1 + 0x1a0),1);
    uVar8 = *(undefined8 *)(param_1 + 0x1b0);
    puVar15 = *(undefined8 **)(param_1 + 0x388);
    for (puVar11 = *(undefined8 **)(param_1 + 0x380); puVar11 != puVar15; puVar11 = puVar11 + 1) {
      ForceData__update(*puVar11,uVar8);
    }
    puVar15 = *(undefined8 **)(param_1 + 0x1d0);
    for (puVar11 = *(undefined8 **)(param_1 + 0x1c8); puVar11 != puVar15; puVar11 = puVar11 + 1) {
      Player__update(*puVar11);
    }
    if (*(longlong *)(*(longlong *)(param_1 + 0x178) + 0x10) !=
        *(longlong *)(*(longlong *)(param_1 + 0x178) + 0x18)) {
      local_68._4_4_ = (undefined4)((ulonglong)local_68 >> 0x20);
      local_68 = (longlong *)CONCAT44(local_68._4_4_,0x20);
      plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      lVar9 = *plVar5;
      local_res8 = *(longlong **)(param_1 + 0x178);
      local_78 = &local_res8;
      puStack_70 = &stack0x00000010;
      plVar5 = *(longlong **)(param_1 + 0x148);
      local_60 = (longlong *)lVar9;
      if (plVar5 == (longlong *)0x0) {
                    /* WARNING: Subroutine does not return */
        std___Xbad_function_call();
      }
      (**(code **)(*plVar5 + 0x10))(plVar5,&local_78);
      plVar6 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      LOCK();
      plVar5 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x40);
      *plVar5 = *plVar5 + (*plVar6 - lVar9);
      UNLOCK();
    }
    local_68._0_4_ = 0x38;
    plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res8);
    lVar9 = *plVar5;
    local_50 = *(longlong **)(param_1 + 0x150);
    local_48 = *(undefined8 *)(param_1 + 0x168);
    local_40 = *(undefined8 *)(param_1 + 0x160);
    local_38 = *(undefined8 *)(param_1 + 0x180);
    local_78 = &local_50;
    puStack_70 = local_30;
    plVar5 = *(longlong **)(param_1 + 0x148);
    local_60 = (longlong *)lVar9;
    if (plVar5 != (longlong *)0x0) {
      (**(code **)(*plVar5 + 0x10))(plVar5,&local_78);
      plVar6 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      LOCK();
      plVar5 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x58);
      *plVar5 = *plVar5 + (*plVar6 - lVar9);
      UNLOCK();
      local_68 = (longlong *)CONCAT44(local_68._4_4_,0x28);
      plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      plVar5 = (longlong *)*plVar5;
      plVar6 = *(longlong **)(param_1 + 0x2b0);
      local_60 = plVar5;
      for (plVar7 = *(longlong **)(param_1 + 0x2a8); plVar7 != plVar6; plVar7 = plVar7 + 1) {
        if (*plVar7 != 0) {
          Planet__update();
        }
      }
      plVar7 = (longlong *)std__chrono__steady_clock__now(&local_res8);
      LOCK();
      plVar6 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x48);
      *plVar6 = *plVar6 + (*plVar7 - (longlong)plVar5);
      UNLOCK();
      plVar5 = *(longlong **)(param_1 + 0x2c8);
      plVar6 = *(longlong **)(param_1 + 0x2c0);
      uVar20 = extraout_XMM0_Da;
      local_res8 = plVar5;
      if (plVar6 != plVar5) {
        do {
          if (*plVar6 != 0) {
            uVar8 = *(undefined8 *)(param_1 + 0x1b0);
            lVar9 = *(longlong *)(*plVar6 + 0x750);
            if (lVar9 != 0) {
              lVar12 = lVar9 + 0x400;
              for (lVar17 = lVar9; lVar17 != lVar12; lVar17 = lVar17 + 0x80) {
                uVar20 = FlowStatistics<ID<EntityPrototype,unsigned_short>,double,void>__Precision__onNextTick
                                   (lVar17,uVar8,lVar12,lVar9 + 0x418);
              }
              uVar20 = _FlowStatistics<ID<EntityPrototype,unsigned_short>,double,void>__onNextTick____2___<lambda_1>__operator__
                                 (uVar20,lVar9 + 0x430,lVar12);
              uVar20 = _FlowStatistics<ID<EntityPrototype,unsigned_short>,double,void>__onNextTick____2___<lambda_1>__operator__
                                 (uVar20,lVar9 + 0x448,lVar9 + 0x418);
              plVar5 = local_res8;
            }
          }
          plVar6 = plVar6 + 1;
        } while (plVar6 != plVar5);
      }
      plVar5 = *(longlong **)(param_1 + 400);
      if ((int)plVar5[2] != 0) {
        uVar14 = (ulonglong)*(uint *)((longlong)plVar5 + 0xc);
        lVar9 = *plVar5;
        local_58 = *(undefined8 *)(lVar9 + 0x10 + uVar14 * 0x18);
        *(undefined8 *)(lVar9 + 0x10 + uVar14 * 0x18) = 0;
        plVar6 = *(longlong **)(lVar9 + 8 + uVar14 * 0x18);
        *(undefined8 *)(lVar9 + 8 + uVar14 * 0x18) = 0;
        plVar7 = *(longlong **)(lVar9 + uVar14 * 0x18);
        *(undefined8 *)(lVar9 + uVar14 * 0x18) = 0;
        local_68 = plVar7;
        local_60 = plVar6;
        std__vector<Targeter<AutoTrasher,35,0>,std::allocator<Targeter<AutoTrasher,35,0>_>_>___vector<Targeter<AutoTrasher,35,0>,std::allocator<Targeter<AutoTrasher,35,0>_>_>
                  (*plVar5 + (ulonglong)*(uint *)((longlong)plVar5 + 0xc) * 0x18);
        *(int *)((longlong)plVar5 + 0xc) = *(int *)((longlong)plVar5 + 0xc) + 1;
        if (*(int *)((longlong)plVar5 + 0xc) == (int)plVar5[1]) {
          *(undefined4 *)((longlong)plVar5 + 0xc) = 0;
        }
        plVar1 = plVar5 + 2;
        *(int *)plVar1 = (int)*plVar1 + -1;
        if ((int)*plVar1 == 0) {
          *(undefined4 *)((longlong)plVar5 + 0xc) = 0;
        }
        if (plVar7 != plVar6) {
          plVar7 = plVar7 + 1;
          do {
            if (((longlong *)*plVar7 != (longlong *)0x0) &&
               (cVar4 = (**(code **)(*(longlong *)*plVar7 + 0x11d0))(), cVar4 != '\0')) {
              (**(code **)(*(longlong *)*plVar7 + 0x11d8))();
              if (*plVar7 != 0) {
                uVar8 = HealingManager__getBucket
                                  (plVar5,*(char *)(*(longlong *)(*plVar7 + 0x48) + 0x6ec) + -1);
                std__vector<Targeter<EntityWithHealth,41,0>,std::allocator<Targeter<EntityWithHealth,41,0>_>_>__emplace_back<Targeter<EntityWithHealth,41,0>_>
                          (uVar8,plVar7 + -1);
              }
            }
            plVar1 = plVar7 + 3;
            plVar7 = plVar7 + 4;
          } while (plVar1 != plVar6);
        }
        std__vector<Targeter<AutoTrasher,35,0>,std::allocator<Targeter<AutoTrasher,35,0>_>_>___vector<Targeter<AutoTrasher,35,0>,std::allocator<Targeter<AutoTrasher,35,0>_>_>
                  (&local_68);
      }
      ExtraScriptData__update(*(undefined8 *)(param_1 + 0x490));
      *(longlong *)(param_1 + 0x1b0) = *(longlong *)(param_1 + 0x1b0) + 1;
      iVar3 = *(int *)(*(longlong *)(param_1 + 0x1c0) + 100);
      if (iVar3 != 0) {
        *(int *)(*(longlong *)(param_1 + 0x1c0) + 100) = iVar3 + -1;
      }
      ItemSpoilQueue__updateBuckets(param_1 + 0x6b8);
      if (*(longlong *)(param_1 + 0x700) != 0) {
        local_78 = (longlong **)0x0;
        puStack_70 = (undefined1 *)0x0;
        lVar9 = operator_new(0x48);
        *(longlong *)lVar9 = lVar9;
        *(longlong *)(lVar9 + 8) = lVar9;
        *(longlong *)(lVar9 + 0x10) = lVar9;
        *(undefined2 *)(lVar9 + 0x18) = 0x101;
        local_78 = *(longlong ***)(param_1 + 0x6f8);
        *(longlong *)(param_1 + 0x6f8) = lVar9;
        puVar19 = *(undefined1 **)(param_1 + 0x700);
        *(undefined1 **)(param_1 + 0x700) = puStack_70;
        plVar5 = *local_78;
        cVar4 = *(char *)((longlong)plVar5 + 0x19);
        puStack_70 = puVar19;
        while (cVar4 == '\0') {
          if ((longlong *)plVar5[5] != (longlong *)0x0) {
            (**(code **)(*(longlong *)plVar5[5] + 0x1240))();
          }
          plVar6 = (longlong *)plVar5[2];
          if (*(char *)((longlong)plVar6 + 0x19) == '\0') {
            cVar4 = *(char *)(*plVar6 + 0x19);
            plVar5 = plVar6;
            plVar6 = (longlong *)*plVar6;
            while (cVar4 == '\0') {
              cVar4 = *(char *)(*plVar6 + 0x19);
              plVar5 = plVar6;
              plVar6 = (longlong *)*plVar6;
            }
          }
          else {
            cVar4 = *(char *)(plVar5[1] + 0x19);
            plVar7 = (longlong *)plVar5[1];
            plVar6 = plVar5;
            while ((plVar5 = plVar7, cVar4 == '\0' && (plVar6 == (longlong *)plVar5[2]))) {
              cVar4 = *(char *)(plVar5[1] + 0x19);
              plVar7 = (longlong *)plVar5[1];
              plVar6 = plVar5;
            }
          }
          cVar4 = *(char *)((longlong)plVar5 + 0x19);
        }
        cVar4 = *(char *)((longlong)local_78[1] + 0x19);
        plVar5 = local_78[1];
        while (cVar4 == '\0') {
          std___Tree_val<std::_Tree_simple_types<ItemSpoilQueue::MachineToCheck>_>___Erase_tree<std::allocator<std::_Tree_node<ItemSpoilQueue::MachineToCheck,void_*>_>_>
                    (&local_78,&local_78,plVar5[2]);
          plVar6 = (longlong *)*plVar5;
          TargeterBase___TargeterBase(plVar5 + 4);
          operator_delete__(plVar5,0x48);
          plVar5 = plVar6;
          cVar4 = *(char *)((longlong)plVar6 + 0x19);
        }
        operator_delete__(local_78,0x48);
      }
      local_58 = *(undefined8 *)(param_1 + 0x730);
      *(undefined8 *)(param_1 + 0x730) = 0;
      lVar9 = *(longlong *)(param_1 + 0x728);
      *(undefined8 *)(param_1 + 0x728) = 0;
      lVar12 = *(longlong *)(param_1 + 0x720);
      *(undefined8 *)(param_1 + 0x720) = 0;
      local_60 = (longlong *)lVar9;
      local_68 = (longlong *)lVar12;
      for (; lVar12 != lVar9; lVar12 = lVar12 + 0x20) {
        if (*(longlong *)(lVar12 + 8) != 0) {
          AutoTrasher__execute();
        }
      }
      std__vector<Targeter<AutoTrasher,35,0>,std::allocator<Targeter<AutoTrasher,35,0>_>_>___vector<Targeter<AutoTrasher,35,0>,std::allocator<Targeter<AutoTrasher,35,0>_>_>
                (&local_68);
      uVar14 = *(ulonglong *)(param_1 + 0x410);
      bVar13 = 0;
      if (uVar14 == 0) goto LAB_140c6ac80;
      do {
        bVar13 = (&DAT_1421c2460)[((-uVar14 & uVar14) * 0x3f79d71b4cb0a89 >> 0x3a) * 4];
LAB_140c6ac80:
        if (bVar13 != 0) goto LAB_140c6ac8e;
        while( true ) {
          if (uVar14 == 0) {
            Map__updateLocalFlyingTexts(param_1);
            lVar9 = *(longlong *)(param_1 + 0x498);
            plVar5 = (longlong *)**(longlong **)(lVar9 + 0x30);
            cVar4 = *(char *)((longlong)plVar5 + 0x19);
            while (cVar4 == '\0') {
              ScriptRendering__destroyObject(lVar9,plVar5[4],1);
              plVar6 = (longlong *)plVar5[2];
              if (*(char *)((longlong)plVar6 + 0x19) == '\0') {
                cVar4 = *(char *)(*plVar6 + 0x19);
                plVar5 = plVar6;
                plVar6 = (longlong *)*plVar6;
                while (cVar4 == '\0') {
                  cVar4 = *(char *)(*plVar6 + 0x19);
                  plVar5 = plVar6;
                  plVar6 = (longlong *)*plVar6;
                }
              }
              else {
                cVar4 = *(char *)(plVar5[1] + 0x19);
                plVar7 = (longlong *)plVar5[1];
                plVar6 = plVar5;
                while ((plVar5 = plVar7, cVar4 == '\0' && (plVar6 == (longlong *)plVar5[2]))) {
                  cVar4 = *(char *)(plVar5[1] + 0x19);
                  plVar7 = (longlong *)plVar5[1];
                  plVar6 = plVar5;
                }
              }
              cVar4 = *(char *)((longlong)plVar5 + 0x19);
            }
            std___Tree<std::_Tmap_traits<unsigned_short,BlueprintRecordID,std::less<unsigned_short>,std::allocator<std::pair<unsigned_short_const_,BlueprintRecordID>_>,0>_>__clear
                      (lVar9 + 0x30);
            Map__applyStagedChanges(param_1);
            return;
          }
LAB_140c6ac8e:
          plVar5 = (longlong *)
                   **(longlong **)
                     (*(longlong *)
                       (*(longlong *)(param_1 + 1000) + (ulonglong)(byte)(bVar13 + 1) * 8) + 0x50);
          cVar4 = *(char *)((longlong)plVar5 + 0x19);
          while (cVar4 == '\0') {
            EvolutionFactors__update(plVar5 + 5,param_1);
            plVar6 = (longlong *)plVar5[2];
            if (*(char *)((longlong)plVar6 + 0x19) == '\0') {
              cVar4 = *(char *)(*plVar6 + 0x19);
              plVar5 = plVar6;
              plVar6 = (longlong *)*plVar6;
              while (cVar4 == '\0') {
                cVar4 = *(char *)(*plVar6 + 0x19);
                plVar5 = plVar6;
                plVar6 = (longlong *)*plVar6;
              }
            }
            else {
              cVar4 = *(char *)(plVar5[1] + 0x19);
              plVar7 = (longlong *)plVar5[1];
              plVar6 = plVar5;
              while ((plVar5 = plVar7, cVar4 == '\0' && (plVar6 == (longlong *)plVar5[2]))) {
                cVar4 = *(char *)(plVar5[1] + 0x19);
                plVar7 = (longlong *)plVar5[1];
                plVar6 = plVar5;
              }
            }
            cVar4 = *(char *)((longlong)plVar5 + 0x19);
          }
          uVar14 = -2L << (bVar13 & 0x3f) & uVar14;
          if (uVar14 != 0) break;
          bVar13 = 0;
        }
      } while( true );
    }
                    /* WARNING: Subroutine does not return */
    std___Xbad_function_call();
  }
  uVar18 = *(longlong *)(param_1 + 0x2c8) - *(longlong *)(param_1 + 0x2c0) >> 3;
  uVar14 = 0;
  if (uVar18 != 0) {
    do {
      lVar9 = *(longlong *)(*(longlong *)(param_1 + 0x2c0) + uVar14 * 8);
      if (lVar9 != 0) {
        MapGenerationManager__update(*(undefined8 *)(lVar9 + 0x128),lVar9);
        Surface__updateBuildEffectSmokes(lVar9,*(undefined8 *)(param_1 + 0x1a8));
      }
      uVar14 = uVar14 + 1;
    } while (uVar14 < uVar18);
  }
  lVar9 = *(longlong *)(param_1 + 0x1a0);
  local_78 = (longlong **)CONCAT44(local_78._4_4_,0xa0);
  plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res8);
  lVar12 = *plVar5;
  puStack_70 = (undefined1 *)lVar12;
  TrainManager__applyStagedChanges(lVar9);
  RailChainSignal__updateChainSignals(lVar9 + 0xd0);
  plVar6 = (longlong *)std__chrono__steady_clock__now(&local_res8);
  LOCK();
  plVar5 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0xc0);
  *plVar5 = *plVar5 + (*plVar6 - lVar12);
  UNLOCK();
  plVar5 = *(longlong **)(param_1 + 0x1d0);
  for (plVar6 = *(longlong **)(param_1 + 0x1c8); plVar6 != plVar5; plVar6 = plVar6 + 1) {
    lVar9 = *plVar6;
    ControllerManager__sendEvents(lVar9 + 0x860);
    if (*(char *)(lVar9 + 0x478) != '\0') {
      *(longlong *)(lVar9 + 0x498) = *(longlong *)(lVar9 + 0x498) + 1;
      (**(code **)(**(longlong **)(lVar9 + 0x868) + 0x48))();
      (**(code **)(**(longlong **)(lVar9 + 0x868) + 0x2c8))();
      Player__generateFarAwayChunks(lVar9);
      Player__requestToChartNeighbourChunks(lVar9,0);
    }
  }
  ExtraScriptData__update(*(undefined8 *)(param_1 + 0x490));
  puVar15 = *(undefined8 **)(param_1 + 0x728);
  puVar11 = *(undefined8 **)(param_1 + 0x720);
  if (puVar11 != puVar15) {
    do {
      if (puVar11[1] == 0) break;
      puVar11 = puVar11 + 4;
    } while (puVar11 != puVar15);
    puVar16 = puVar11;
    if (puVar11 != puVar15) {
      while (puVar10 = puVar16 + 4, puVar10 != puVar15) {
        plVar5 = puVar16 + 5;
        puVar16 = puVar10;
        if (*plVar5 != 0) {
          TargeterBase__operator_(puVar11);
          puVar11 = puVar11 + 4;
          puVar16 = puVar10;
        }
      }
      if (puVar11 != puVar15) {
        puVar16 = *(undefined8 **)(param_1 + 0x728);
        for (; puVar15 != puVar16; puVar15 = puVar15 + 4) {
          TargeterBase__operator_(puVar11,puVar15);
          puVar11 = puVar11 + 4;
        }
        puVar15 = *(undefined8 **)(param_1 + 0x728);
        for (puVar16 = puVar11; puVar16 != puVar15; puVar16 = puVar16 + 4) {
          (**(code **)*puVar16)(puVar16,0);
        }
        *(undefined8 **)(param_1 + 0x728) = puVar11;
      }
    }
  }
  Map__applyStagedChanges(param_1);
  Map__updateLocalFlyingTexts(param_1);
  return;
}

