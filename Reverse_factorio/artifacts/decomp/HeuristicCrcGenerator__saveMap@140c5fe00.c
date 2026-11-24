// HeuristicCrcGenerator::saveMap @ 140c5fe00 (function HeuristicCrcGenerator::saveMap)

void HeuristicCrcGenerator__saveMap(longlong param_1,longlong param_2)

{
  short *psVar1;
  int iVar2;
  char cVar3;
  char *pcVar4;
  longlong lVar5;
  longlong lVar6;
  uint uVar7;
  longlong *plVar8;
  longlong *plVar9;
  ushort uVar10;
  undefined8 *puVar11;
  short *psVar12;
  longlong *plVar13;
  longlong *plVar14;
  longlong *plVar15;
  undefined8 *puVar16;
  longlong lVar17;
  longlong *plVar18;
  longlong *plVar19;
  undefined8 *puVar20;
  undefined4 uVar21;
  uint uVar22;
  ushort uVar23;
  undefined8 *puVar24;
  ulonglong uVar25;
  undefined8 *puVar26;
  longlong *plVar27;
  undefined8 local_res8;
  longlong *local_res10;
  longlong *local_res18;
  undefined8 local_res20;
  longlong *in_stack_ffffffffffffff78;
  ulonglong local_78;
  double local_70;
  longlong local_68;
  char *local_60;
  longlong local_58;
  
  *(undefined1 *)(param_2 + 0x148) = 1;
  local_res20 = (longlong *)((ulonglong)local_res20 & 0xffffffff00000000);
  puVar16 = (undefined8 *)(*(longlong *)(param_1 + 0x150) + 0x10);
  puVar26 = (undefined8 *)0x0;
  local_res8 = param_1;
  for (puVar20 = (undefined8 *)*puVar16; puVar20 != puVar16; puVar20 = (undefined8 *)*puVar20) {
    pcVar4 = (char *)(puVar20 + 4);
    if (puVar20 == (undefined8 *)0x0) {
      pcVar4 = &DAT_00000038;
    }
    if (*pcVar4 == '\0') {
      uVar7 = (int)puVar26 + 1;
      puVar26 = (undefined8 *)(ulonglong)uVar7;
      uVar25 = (ulonglong)local_res20 >> 0x20;
      local_res20 = (longlong *)CONCAT44((int)uVar25,uVar7);
      puVar24 = puVar20 + 2;
      if (puVar20 == (undefined8 *)0x0) {
        puVar24 = (undefined8 *)&DAT_00000028;
      }
      puVar11 = (undefined8 *)*puVar24;
      if (puVar11 != puVar24) {
        do {
          puVar26 = puVar11 + -3;
          if (puVar11 == (undefined8 *)0x0) {
            puVar26 = (undefined8 *)0x0;
          }
          ElectricNetworkSerialiser__registerElectricNetwork(param_2 + 0x98,puVar26,param_2);
          puVar11 = (undefined8 *)*puVar11;
        } while (puVar11 != puVar24);
        puVar26 = (undefined8 *)((ulonglong)local_res20 & 0xffffffff);
      }
    }
  }
  if (*(longlong *)(local_res8 + 0x1e0) != *(longlong *)(local_res8 + 0x1e8)) {
    uVar7 = (uint)(*(longlong *)(local_res8 + 0x1e8) - *(longlong *)(local_res8 + 0x1e0) >> 3);
    iVar2 = (int)(*(ulonglong *)(local_res8 + 0x1a8) % ((ulonglong)(uVar7 + 9) / 10));
    uVar22 = iVar2 * 10;
    if (uVar22 + 10 < uVar7) {
      uVar7 = uVar22 + 10;
    }
    if (uVar22 < uVar7) {
      lVar17 = (ulonglong)uVar22 << 3;
      uVar25 = (ulonglong)(uVar7 + iVar2 * -10);
      do {
        lVar6 = *(longlong *)(lVar17 + *(longlong *)(local_res8 + 0x1e0));
        in_stack_ffffffffffffff78 = (longlong *)(lVar6 + 0x4e0);
        if (0xf < *(ulonglong *)(lVar6 + 0x4f8)) {
          in_stack_ffffffffffffff78 = (longlong *)*in_stack_ffffffffffffff78;
        }
        Serialiser__TagWriter__TagWriter
                  (&local_70,param_2,"player","name=%s",in_stack_ffffffffffffff78);
        Player__save(lVar6,param_2);
        cVar3 = (**(code **)(**(longlong **)(lVar6 + 0x878) + 0x7e8))();
        if ((cVar3 == '\x03') &&
           (lVar5 = (**(code **)(**(longlong **)(lVar6 + 0x888) + 0x8d0))(), lVar5 != 0)) {
          lVar6 = *(longlong *)(lVar5 + 0x188);
        }
        else {
          lVar6 = (**(code **)(**(longlong **)(lVar6 + 0x878) + 0x8c8))();
          if (lVar6 == 0) {
            lVar6 = 0;
          }
          else {
            lVar6 = *(longlong *)(lVar6 + 0x188);
          }
        }
        if (lVar6 != 0) {
          Character__save(lVar6,param_2);
        }
        lVar6 = local_68;
        if (*(char *)(local_68 + 8) != '\0') {
          (**(code **)(**(longlong **)(local_68 + 0x50) + 8))
                    (*(longlong **)(local_68 + 0x50),&DAT_14230f710,2);
          *(longlong *)(lVar6 + 0x10) = *(longlong *)(lVar6 + 0x10) + 2;
          lVar5 = -1;
          do {
            lVar5 = lVar5 + 1;
          } while (*(char *)((longlong)local_70 + lVar5) != '\0');
          (**(code **)(**(longlong **)(lVar6 + 0x50) + 8))
                    (*(longlong **)(lVar6 + 0x50),local_70,lVar5);
          *(longlong *)(lVar6 + 0x10) = *(longlong *)(lVar6 + 0x10) + lVar5;
          (**(code **)(**(longlong **)(lVar6 + 0x50) + 8))
                    (*(longlong **)(lVar6 + 0x50),&DAT_1421e0108,1);
          *(longlong *)(lVar6 + 0x10) = *(longlong *)(lVar6 + 0x10) + 1;
        }
        lVar17 = lVar17 + 8;
        uVar25 = uVar25 - 1;
      } while (uVar25 != 0);
    }
  }
  local_78 = *(ulonglong *)(local_res8 + 0x410);
  cVar3 = '\0';
  uVar25 = (ulonglong)local_res10 >> 8;
  local_res10 = (longlong *)((ulonglong)local_res10 & 0xffffffffffffff00);
  if (local_78 != 0) {
    cVar3 = (&DAT_1421c2460)[((-local_78 & local_78) * 0x3f79d71b4cb0a89 >> 0x3a) * 4];
    local_res10 = (longlong *)CONCAT71((int7)uVar25,cVar3);
  }
  do {
    if (cVar3 != '\0') goto LAB_140c600af;
    while( true ) {
      uVar21 = (undefined4)((ulonglong)in_stack_ffffffffffffff78 >> 0x20);
      local_res18 = (longlong *)(param_2 + 0x50);
      if (local_78 == 0) {
        if ((int)local_res20 != 0) {
          uVar7 = (int)(*(ulonglong *)(local_res8 + 0x1a8) % ((ulonglong)((int)local_res20 + 4) / 5)
                       ) * 5;
          uVar22 = 0;
          puVar26 = (undefined8 *)(*(longlong *)(local_res8 + 0x150) + 0x10);
          for (puVar20 = (undefined8 *)*puVar26; puVar20 != puVar26;
              puVar20 = (undefined8 *)*puVar20) {
            pcVar4 = (char *)(puVar20 + 4);
            if (puVar20 == (undefined8 *)0x0) {
              pcVar4 = &DAT_00000038;
            }
            if (((*pcVar4 == '\0') && (uVar22 = uVar22 + 1, uVar7 <= uVar22)) &&
               (uVar22 < uVar7 + 5)) {
              plVar18 = puVar20 + 0x1f;
              if (puVar20 == (undefined8 *)0x0) {
                plVar18 = (longlong *)&DAT_00000110;
              }
              FlowStatistics<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,double,ElectricityTag>__Precision__saveLastValue
                        (*plVar18,param_2);
              local_60 = "energy-input-running-counts";
              local_58 = param_2;
              if (*(char *)(param_2 + 8) != '\0') {
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),"energy-input-running-counts",0x1b);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x1b;
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
              }
              lVar17 = *plVar18;
              local_res10 = (longlong *)
                            (*(longlong *)(lVar17 + 0x450) - *(longlong *)(lVar17 + 0x448) >> 4);
              Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                        (param_2,&local_res10);
              psVar1 = *(short **)(lVar17 + 0x450);
              for (psVar12 = *(short **)(lVar17 + 0x448); psVar12 != psVar1; psVar12 = psVar12 + 8)
              {
                if (*psVar12 == 0) {
                    /* WARNING: Subroutine does not return */
                  ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/SerialiserHelper.hpp"
                                      ,0xeb,"bool(item.first)");
                }
                ID<EntityPrototype,unsigned_short>__save(psVar12,param_2);
                if (*psVar12 != 0) {
                  MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>
                            (param_2,psVar12 + 1);
                }
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),psVar12 + 4,8);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
              }
              if (*(char *)(param_2 + 8) != '\0') {
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),"energy-input-running-counts",0x1b);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x1b;
                (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                          (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
                *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
              }
            }
            uVar21 = (undefined4)((ulonglong)in_stack_ffffffffffffff78 >> 0x20);
          }
        }
        local_60 = "next-train-id";
        pcVar4 = (char *)(param_2 + 8);
        puVar20 = (undefined8 *)(param_2 + 0x50);
        local_58 = param_2;
        if (*pcVar4 != '\0') {
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e010c,1);
          plVar18 = (longlong *)(param_2 + 0x10);
          *plVar18 = *plVar18 + 1;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,"next-train-id",0xd);
          *plVar18 = *plVar18 + 0xd;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        plVar18 = (longlong *)(param_2 + 0x10);
        local_res10 = (longlong *)
                      CONCAT44(local_res10._4_4_,
                               *(undefined4 *)(*(longlong *)(local_res8 + 0x1a0) + 0xb4));
        (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&local_res10,4);
        *plVar18 = *plVar18 + 4;
        if (*pcVar4 != '\0') {
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_14230f710,2);
          *plVar18 = *plVar18 + 2;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,"next-train-id",0xd);
          *plVar18 = *plVar18 + 0xd;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        local_60 = "next-unit-number";
        local_58 = param_2;
        if (*pcVar4 != '\0') {
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e010c,1);
          *plVar18 = *plVar18 + 1;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,"next-unit-number",0x10);
          *plVar18 = *plVar18 + 0x10;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        lVar17 = local_res8;
        pcVar4 = (char *)(param_2 + 8);
        local_res10 = *(longlong **)(local_res8 + 0x208);
        (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&local_res10,8);
        *plVar18 = *plVar18 + 8;
        if (*pcVar4 != '\0') {
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_14230f710,2);
          *plVar18 = *plVar18 + 2;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,"next-unit-number",0x10);
          *plVar18 = *plVar18 + 0x10;
          (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        plVar14 = (longlong *)(param_2 + 0x50);
        plVar18 = (longlong *)(param_2 + 0x10);
        local_60 = "next-targetable-item-number";
        local_58 = param_2;
        if (*pcVar4 != '\0') {
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e010c,1);
          *plVar18 = *plVar18 + 1;
          (**(code **)(*(longlong *)*plVar14 + 8))
                    ((longlong *)*plVar14,"next-targetable-item-number",0x1b);
          *plVar18 = *plVar18 + 0x1b;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        local_res10 = *(longlong **)(lVar17 + 0x210);
        (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&local_res10,8);
        *plVar18 = *plVar18 + 8;
        if (*pcVar4 != '\0') {
          local_res10 = plVar14;
          local_res18 = plVar18;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_14230f710,2);
          *plVar18 = *plVar18 + 2;
          (**(code **)(*(longlong *)*plVar14 + 8))
                    ((longlong *)*plVar14,"next-targetable-item-number",0x1b);
          *plVar18 = *plVar18 + 0x1b;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        local_res18 = (longlong *)(param_2 + 0x10);
        local_res10 = (longlong *)(param_2 + 0x50);
        plVar9 = (longlong *)(param_2 + 8);
        local_60 = "active-entities-count";
        local_58 = param_2;
        if (*pcVar4 != '\0') {
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e010c,1);
          *plVar18 = *plVar18 + 1;
          (**(code **)(*(longlong *)*plVar14 + 8))
                    ((longlong *)*plVar14,"active-entities-count",0x15);
          *plVar18 = *plVar18 + 0x15;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        plVar8 = local_res18;
        plVar13 = local_res10;
        (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,local_res8 + 0x88,4);
        *plVar18 = *plVar18 + 4;
        if (*(char *)plVar9 != '\0') {
          local_res10 = plVar9;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_14230f710,2);
          *plVar8 = *plVar8 + 2;
          (**(code **)(*(longlong *)*plVar13 + 8))
                    ((longlong *)*plVar13,"active-entities-count",0x15);
          *plVar8 = *plVar8 + 0x15;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar8 = *plVar8 + 1;
        }
        local_res10 = (longlong *)(param_2 + 8);
        local_60 = "entity-count";
        plVar14 = plVar8;
        plVar18 = plVar13;
        local_58 = param_2;
        if (*(char *)plVar9 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e010c,1);
          *plVar8 = *plVar8 + 1;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,"entity-count",0xc);
          *plVar8 = *plVar8 + 0xc;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar8 = *plVar8 + 1;
          plVar18 = (longlong *)(param_2 + 0x50);
          plVar14 = (longlong *)(param_2 + 0x10);
        }
        plVar27 = local_res10;
        (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,local_res8 + 0x8c,4);
        *plVar8 = *plVar8 + 4;
        if (*(char *)plVar9 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_14230f710,2);
          *plVar8 = *plVar8 + 2;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,"entity-count",0xc);
          *plVar8 = *plVar8 + 0xc;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar8 = *plVar8 + 1;
          plVar13 = plVar18;
          plVar8 = plVar14;
        }
        local_60 = "segmented-unit-count";
        plVar18 = plVar27;
        local_58 = param_2;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e010c,1);
          *plVar8 = *plVar8 + 1;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,"segmented-unit-count",0x14)
          ;
          *plVar8 = *plVar8 + 0x14;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar8 = *plVar8 + 1;
          plVar18 = (longlong *)(param_2 + 8);
        }
        (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,local_res8 + 0x90,4);
        *plVar8 = *plVar8 + 4;
        plVar14 = plVar27;
        local_res10 = plVar13;
        local_res18 = plVar8;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_14230f710,2);
          *plVar8 = *plVar8 + 2;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,"segmented-unit-count",0x14)
          ;
          *plVar8 = *plVar8 + 0x14;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar8 = *plVar8 + 1;
          local_res10 = (longlong *)(param_2 + 0x50);
          local_res18 = (longlong *)(param_2 + 0x10);
          plVar14 = plVar18;
        }
        local_60 = "fully-active-segmented-units-count";
        plVar18 = plVar13;
        plVar9 = plVar8;
        local_58 = param_2;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e010c,1);
          *plVar8 = *plVar8 + 1;
          (**(code **)(*(longlong *)*plVar13 + 8))
                    ((longlong *)*plVar13,"fully-active-segmented-units-count",0x22);
          *plVar8 = *plVar8 + 0x22;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar8 = *plVar8 + 1;
          plVar18 = local_res10;
          plVar9 = local_res18;
        }
        (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,local_res8 + 0x94,4);
        *plVar8 = *plVar8 + 4;
        local_res10 = plVar14;
        if ((char)*plVar14 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"fully-active-segmented-units-count",0x22);
          *plVar9 = *plVar9 + 0x22;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          local_res10 = (longlong *)(param_2 + 8);
        }
        local_60 = "minimally-active-segmented-units-count";
        plVar13 = plVar18;
        plVar8 = plVar9;
        plVar27 = plVar14;
        local_58 = param_2;
        if ((char)*plVar14 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"minimally-active-segmented-units-count",0x26);
          *plVar9 = *plVar9 + 0x26;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar13 = (longlong *)(param_2 + 0x50);
          plVar8 = (longlong *)(param_2 + 0x10);
          plVar27 = local_res10;
        }
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,local_res8 + 0x98,4);
        *plVar9 = *plVar9 + 4;
        if ((char)*plVar14 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"minimally-active-segmented-units-count",0x26);
          *plVar9 = *plVar9 + 0x26;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar18 = plVar13;
          plVar9 = plVar8;
        }
        local_60 = "asleep-segmented-units-count";
        plVar14 = plVar27;
        local_58 = param_2;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"asleep-segmented-units-count",0x1c);
          *plVar9 = *plVar9 + 0x1c;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar14 = (longlong *)(param_2 + 8);
        }
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,local_res8 + 0x9c,4);
        *plVar9 = *plVar9 + 4;
        plVar13 = plVar18;
        plVar8 = plVar9;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"asleep-segmented-units-count",0x1c);
          *plVar9 = *plVar9 + 0x1c;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar13 = (longlong *)(param_2 + 0x50);
          plVar8 = (longlong *)(param_2 + 0x10);
          plVar27 = plVar14;
        }
        uVar7 = *(uint *)(local_res8 + 0x1a8);
        Serialiser__TagWriter__TagWriter
                  (&local_70,param_2,"polluted-chunks","index=%u",
                   CONCAT44(uVar21,uVar7) & 0xffffffff0000003f);
        local_res10 = (longlong *)
                      CONCAT44(local_res10._4_4_,
                               *(undefined4 *)
                                (**(longlong **)(local_res8 + 0x2c0) +
                                ((ulonglong)(uVar7 & 0x3f) + 0xe) * 0x18));
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&local_res10,4);
        *plVar9 = *plVar9 + 4;
        if (*(char *)(local_68 + 8) != '\0') {
          (**(code **)(**(longlong **)(local_68 + 0x50) + 8))
                    (*(longlong **)(local_68 + 0x50),&DAT_14230f710,2);
          *(longlong *)(local_68 + 0x10) = *(longlong *)(local_68 + 0x10) + 2;
          lVar17 = -1;
          do {
            lVar17 = lVar17 + 1;
          } while (*(char *)((longlong)local_70 + lVar17) != '\0');
          (**(code **)(**(longlong **)(local_68 + 0x50) + 8))
                    (*(longlong **)(local_68 + 0x50),local_70,lVar17);
          *(longlong *)(local_68 + 0x10) = *(longlong *)(local_68 + 0x10) + lVar17;
          (**(code **)(**(longlong **)(local_68 + 0x50) + 8))
                    (*(longlong **)(local_68 + 0x50),&DAT_1421e0108,1);
          *(longlong *)(local_68 + 0x10) = *(longlong *)(local_68 + 0x10) + 1;
          plVar18 = plVar13;
          plVar9 = plVar8;
        }
        local_60 = "chunks-total";
        plVar14 = plVar27;
        plVar13 = plVar18;
        plVar8 = plVar9;
        local_58 = param_2;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"chunks-total",0xc);
          *plVar9 = *plVar9 + 0xc;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar14 = (longlong *)(param_2 + 8);
          plVar13 = (longlong *)(param_2 + 0x50);
          plVar8 = (longlong *)(param_2 + 0x10);
        }
        local_res18 = plVar8;
        local_res10 = plVar13;
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,local_res8 + 0x248,4);
        *plVar9 = *plVar9 + 4;
        plVar13 = plVar18;
        plVar8 = plVar9;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"chunks-total",0xc);
          *plVar9 = *plVar9 + 0xc;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar13 = local_res10;
          plVar8 = local_res18;
          plVar27 = plVar14;
        }
        local_60 = "chunks-with-tiles";
        local_58 = param_2;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"chunks-with-tiles",0x11);
          *plVar9 = *plVar9 + 0x11;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar18 = plVar13;
          plVar9 = plVar8;
        }
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,local_res8 + 0x24c,4);
        *plVar9 = *plVar9 + 4;
        local_res10 = plVar27;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"chunks-with-tiles",0x11);
          *plVar9 = *plVar9 + 0x11;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          local_res10 = (longlong *)(param_2 + 8);
        }
        local_60 = "chunks-with-entities";
        plVar14 = plVar18;
        plVar8 = plVar27;
        plVar13 = plVar9;
        local_58 = param_2;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"chunks-with-entities",0x14)
          ;
          *plVar9 = *plVar9 + 0x14;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar14 = (longlong *)(param_2 + 0x50);
          plVar13 = (longlong *)(param_2 + 0x10);
          plVar8 = local_res10;
        }
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,local_res8 + 0x250,4);
        *plVar9 = *plVar9 + 4;
        if ((char)*plVar27 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"chunks-with-entities",0x14)
          ;
          *plVar9 = *plVar9 + 0x14;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar18 = plVar14;
          plVar9 = plVar13;
        }
        local_60 = "active-transport-line-groups";
        plVar14 = plVar8;
        local_58 = param_2;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"active-transport-line-groups",0x1c);
          *plVar9 = *plVar9 + 0x1c;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar14 = (longlong *)(param_2 + 8);
        }
        lVar17 = local_res8;
        local_res10 = (longlong *)
                      CONCAT44(local_res10._4_4_,
                               (int)(*(longlong *)(*(longlong *)(local_res8 + 0x170) + 0x30) -
                                     *(longlong *)(*(longlong *)(local_res8 + 0x170) + 0x28) >> 3));
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&local_res10);
        *plVar9 = *plVar9 + 4;
        local_res10 = plVar18;
        local_res18 = plVar9;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"active-transport-line-groups",0x1c);
          *plVar9 = *plVar9 + 0x1c;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108);
          *plVar9 = *plVar9 + 1;
          local_res10 = (longlong *)(param_2 + 0x50);
          local_res18 = (longlong *)(param_2 + 0x10);
        }
        local_60 = "active-transport-lines";
        lVar6 = lVar17;
        local_58 = param_2;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"active-transport-lines",0x16);
          *plVar9 = *plVar9 + 0x16;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108);
          *plVar9 = *plVar9 + 1;
          plVar14 = (longlong *)(param_2 + 8);
          plVar18 = local_res10;
          plVar9 = local_res18;
          lVar6 = local_res8;
        }
        lVar5 = local_res8;
        lVar17 = *(longlong *)(lVar17 + 0x170);
        plVar13 = *(longlong **)(lVar17 + 0x30);
        plVar8 = *(longlong **)(lVar17 + 0x28);
        lVar17 = 0;
        uVar21 = 0;
        if (plVar8 != plVar13) {
          do {
            lVar17 = lVar17 + *(longlong *)(*plVar8 + 0x40);
            uVar21 = (undefined4)lVar17;
            plVar8 = plVar8 + 1;
          } while (plVar8 != plVar13);
          plVar18 = (longlong *)(param_2 + 0x50);
          plVar9 = (longlong *)(param_2 + 0x10);
          plVar14 = (longlong *)(param_2 + 8);
          lVar6 = local_res8;
        }
        local_res10 = (longlong *)CONCAT44(local_res10._4_4_,uVar21);
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&local_res10,4);
        *plVar9 = *plVar9 + 4;
        if (*(char *)(param_2 + 8) != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"active-transport-lines",0x16);
          *plVar9 = *plVar9 + 0x16;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
        }
        local_60 = "transport-line-requests";
        plVar13 = plVar18;
        local_res18 = plVar9;
        local_res20 = plVar14;
        local_58 = param_2;
        if ((char)*plVar14 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"transport-line-requests",0x17);
          *plVar9 = *plVar9 + 0x17;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar13 = (longlong *)(param_2 + 0x50);
          local_res18 = (longlong *)(param_2 + 0x10);
          local_res20 = (longlong *)(param_2 + 8);
        }
        local_res10._0_4_ = *(undefined4 *)(*(longlong *)(lVar5 + 0x170) + 0x58);
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&local_res10,4);
        *plVar9 = *plVar9 + 4;
        local_res10 = (longlong *)
                      CONCAT44(local_res10._4_4_,
                               *(undefined4 *)(*(longlong *)(lVar6 + 0x170) + 0x60));
        (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&local_res10,4);
        *plVar9 = *plVar9 + 4;
        plVar8 = plVar14;
        if ((char)*plVar14 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"transport-line-requests",0x17);
          *plVar9 = *plVar9 + 0x17;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar18 = plVar13;
          plVar9 = local_res18;
          plVar8 = local_res20;
        }
        local_60 = "random-generator";
        local_58 = param_2;
        if ((char)*plVar14 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"random-generator",0x10);
          *plVar9 = *plVar9 + 0x10;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
        }
        lVar17 = local_res8;
        RandomGenerator__save(local_res8 + 0x434,param_2);
        RandomGenerator__save(lVar17 + 0x440,param_2);
        RandomGenerator__save(lVar17 + 0x44c,param_2);
        RandomGenerator__save(lVar17 + 0x458,param_2);
        RandomGenerator__save(lVar17 + 0x464,param_2);
        RandomGenerator__save(lVar17 + 0x470,param_2);
        plVar14 = plVar8;
        plVar13 = plVar18;
        plVar27 = plVar9;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_14230f710,2);
          *plVar9 = *plVar9 + 2;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,"random-generator",0x10);
          *plVar9 = *plVar9 + 0x10;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar14 = (longlong *)(param_2 + 8);
          plVar13 = (longlong *)(param_2 + 0x50);
          plVar27 = (longlong *)(param_2 + 0x10);
        }
        local_60 = "enemy-evolution-factors";
        plVar15 = plVar27;
        plVar19 = plVar13;
        local_58 = param_2;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e010c,1);
          *plVar9 = *plVar9 + 1;
          (**(code **)(*(longlong *)*plVar18 + 8))
                    ((longlong *)*plVar18,"enemy-evolution-factors",0x17);
          *plVar9 = *plVar9 + 0x17;
          (**(code **)(*(longlong *)*plVar18 + 8))((longlong *)*plVar18,&DAT_1421e0108,1);
          *plVar9 = *plVar9 + 1;
          plVar15 = (longlong *)(param_2 + 0x10);
          plVar19 = (longlong *)(param_2 + 0x50);
          plVar8 = plVar14;
        }
        lVar17 = local_res8;
        MapSerialiser__operator<<<EvolutionFactorData>
                  (param_2,*(longlong *)(*(longlong *)(local_res8 + 1000) + 0x10) + 0x50);
        plVar18 = plVar27;
        plVar14 = plVar13;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_14230f710,2);
          *plVar27 = *plVar27 + 2;
          (**(code **)(*(longlong *)*plVar13 + 8))
                    ((longlong *)*plVar13,"enemy-evolution-factors",0x17);
          *plVar27 = *plVar27 + 0x17;
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e0108,1);
          *plVar27 = *plVar27 + 1;
          plVar18 = plVar15;
          plVar14 = plVar19;
        }
        local_60 = "script-rendering";
        local_58 = param_2;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar13 + 8))((longlong *)*plVar13,&DAT_1421e010c,1);
          *plVar18 = *plVar27 + 1;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,"script-rendering",0x10);
          *plVar18 = *plVar18 + 0x10;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        local_res8 = CONCAT44(local_res8._4_4_,*(undefined4 *)(*(longlong *)(lVar17 + 0x498) + 0x28)
                             );
        (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&local_res8,4);
        *plVar18 = *plVar18 + 4;
        if ((char)*plVar8 != '\0') {
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_14230f710,2);
          *plVar18 = *plVar18 + 2;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,"script-rendering",0x10);
          *plVar18 = *plVar18 + 0x10;
          (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e0108,1);
          *plVar18 = *plVar18 + 1;
        }
        plVar9 = *(longlong **)(lVar17 + 0x2c8);
        local_res10 = *(longlong **)(lVar17 + 0x2c0);
        local_res18 = plVar9;
        if (local_res10 != plVar9) {
          do {
            lVar17 = *local_res10;
            if (lVar17 != 0) {
              plVar13 = *(longlong **)(lVar17 + 0x108);
              for (plVar8 = *(longlong **)(lVar17 + 0x100); plVar8 != plVar13; plVar8 = plVar8 + 1)
              {
                lVar17 = *plVar8;
                if (lVar17 != 0) {
                  local_res8._4_4_ = (undefined4)((ulonglong)local_res8 >> 0x20);
                  local_res8._0_4_ = *(undefined4 *)(lVar17 + 0x18);
                  (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&local_res8,4);
                  *plVar18 = *plVar18 + 4;
                  local_res8 = CONCAT44(local_res8._4_4_,
                                        (int)(*(longlong *)(lVar17 + 0x30) -
                                              *(longlong *)(lVar17 + 0x28) >> 3));
                  (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&local_res8,4);
                  *plVar18 = *plVar18 + 4;
                  lVar6 = *(longlong *)(lVar17 + 0xa8);
                  for (lVar5 = *(longlong *)(lVar17 + 0xa0); lVar5 != lVar6; lVar5 = lVar5 + 0x10) {
                    (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,lVar5 + 8,4);
                    *plVar18 = *plVar18 + 4;
                  }
                  lVar6 = *(longlong *)(lVar17 + 0x100);
                  for (lVar5 = *(longlong *)(lVar17 + 0xf8); lVar5 != lVar6; lVar5 = lVar5 + 0x10) {
                    (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,lVar5 + 8,4);
                    *plVar18 = *plVar18 + 4;
                  }
                  lVar6 = *(longlong *)(lVar17 + 0x158);
                  for (lVar17 = *(longlong *)(lVar17 + 0x150); lVar17 != lVar6;
                      lVar17 = lVar17 + 0x10) {
                    (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,lVar17 + 8,4);
                    *plVar18 = *plVar18 + 4;
                  }
                }
                plVar9 = local_res18;
              }
            }
            local_res10 = local_res10 + 1;
          } while (local_res10 != plVar9);
        }
        return;
      }
LAB_140c600af:
      plVar14 = (longlong *)(param_2 + 0x50);
      plVar18 = (longlong *)(param_2 + 0x10);
      local_60 = "force-data";
      local_res18 = plVar14;
      local_58 = param_2;
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e010c,1);
        *plVar18 = *plVar18 + 1;
        (**(code **)(*(longlong *)*local_res18 + 8))((longlong *)*local_res18,"force-data",10);
        *plVar18 = *plVar18 + 10;
        (**(code **)(*(longlong *)*plVar14 + 8))((longlong *)*plVar14,&DAT_1421e0108,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      }
      lVar17 = *(longlong *)(*(longlong *)(local_res8 + 1000) + (ulonglong)(byte)(cVar3 + 1) * 8);
      lVar6 = *(longlong *)
               ((ulonglong)*(uint *)(**(longlong **)(local_res8 + 0x2c0) + 0x28) * 0x20 +
               *(longlong *)(lVar17 + 0x6c0));
      if (lVar6 != 0) {
        FlowStatistics<IDWithQuality<ID<ItemPrototype,unsigned_short>_>,unsigned___int64,void>__Precision__saveLastValue
                  (lVar6,param_2);
      }
      uVar10 = 0;
      local_res18 = (longlong *)((ulonglong)local_res18 & 0xffffffffffff0000);
      uVar23 = (ushort)((longlong)
                        (ulonglong)(uint)(*(int *)(lVar17 + 0x6b0) - *(int *)(lVar17 + 0x6a8)) >> 2)
      ;
      if (uVar23 != 0) {
        do {
          if (*(int *)(*(longlong *)(lVar17 + 0x6a8) + (ulonglong)uVar10 * 4) != 0) {
            plVar18 = *(longlong **)(param_2 + 0x50);
            (**(code **)(*plVar18 + 8))(plVar18,&local_res18,2);
            plVar18 = (longlong *)(param_2 + 0x10);
            *plVar18 = *plVar18 + 2;
            plVar14 = *(longlong **)(param_2 + 0x50);
            (**(code **)(*plVar14 + 8))
                      (plVar14,*(longlong *)(lVar17 + 0x6a8) + ((ulonglong)local_res18 & 0xffff) * 4
                       ,4);
            *plVar18 = *plVar18 + 4;
            uVar10 = (ushort)local_res18;
          }
          uVar10 = uVar10 + 1;
          local_res18 = (longlong *)CONCAT62(local_res18._2_6_,uVar10);
        } while (uVar10 < uVar23);
      }
      puVar20 = (undefined8 *)(param_2 + 0x50);
      plVar18 = (longlong *)(param_2 + 0x10);
      lVar17 = *(longlong *)(lVar17 + 0x78);
      if (*(longlong *)(lVar17 + 0x68) == 0) {
        local_70 = 0.0;
      }
      else {
        plVar14 = *(longlong **)(*(longlong *)(lVar17 + 0x68) + 0x10);
        if (plVar14 == (longlong *)0x0) {
          local_70 = *(double *)(lVar17 + 0x48) / (double)*(ulonglong *)(lVar17 + 0x70);
        }
        else {
          local_70 = (double)(**(code **)(*plVar14 + 0x30))();
        }
      }
      (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&local_70,8);
      *plVar18 = *plVar18 + 8;
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_14230f710,2);
        *plVar18 = *plVar18 + 2;
        (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,"force-data",10);
        *plVar18 = *plVar18 + 10;
        (**(code **)(*(longlong *)*puVar20 + 8))((longlong *)*puVar20,&DAT_1421e0108,1);
        *plVar18 = *plVar18 + 1;
      }
      local_78 = -2L << ((byte)local_res10 & 0x3f) & local_78;
      if (local_78 != 0) break;
      cVar3 = '\0';
      local_res10 = (longlong *)((ulonglong)local_res10._1_7_ << 8);
    }
    cVar3 = (&DAT_1421c2460)[((-local_78 & local_78) * 0x3f79d71b4cb0a89 >> 0x3a) * 4];
    local_res10 = (longlong *)CONCAT71(local_res10._1_7_,cVar3);
  } while( true );
}

