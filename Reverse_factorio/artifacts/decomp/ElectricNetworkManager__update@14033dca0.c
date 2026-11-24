// ElectricNetworkManager::update @ 14033dca0 (function ElectricNetworkManager::update)

void ElectricNetworkManager__update
               (longlong param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  char cVar1;
  longlong lVar2;
  short sVar3;
  undefined2 uVar4;
  longlong *plVar5;
  longlong *plVar6;
  longlong *plVar7;
  short sVar8;
  longlong lVar9;
  longlong *plVar10;
  longlong *plVar11;
  longlong lVar12;
  longlong lVar13;
  double dVar14;
  double dVar15;
  double dVar16;
  longlong local_res8;
  longlong local_res18;
  longlong local_res20;
  
  LOCK();
  cVar1 = *(char *)(param_1 + 0x78);
  *(char *)(param_1 + 0x78) = '\x01';
  UNLOCK();
  if (cVar1 == '\0') {
    local_res8 = param_1;
    plVar5 = (longlong *)std__chrono__steady_clock__now(&local_res18,param_2,param_3,param_4,0x70);
    dVar16 = DAT_142403280;
    dVar14 = DAT_142402088;
    local_res20 = *plVar5;
    plVar5 = *(longlong **)(param_1 + 0x60);
    for (plVar7 = *(longlong **)(param_1 + 0x58); plVar7 != plVar5; plVar7 = plVar7 + 1) {
      lVar9 = *plVar7;
      sVar8 = *(short *)(lVar9 + 0x22);
      if (sVar8 != 0) {
        sVar8 = sVar8 + -1;
        *(short *)(lVar9 + 0x22) = sVar8;
      }
      sVar3 = *(short *)(lVar9 + 0x20);
      if (sVar3 != 0) {
        sVar3 = sVar3 + -1;
        *(short *)(lVar9 + 0x20) = sVar3;
      }
      dVar15 = *(double *)(lVar9 + 0x10);
      if ((*(double *)(lVar9 + 0x18) <= dVar15) || (sVar3 != 0)) {
        if ((*(double *)(lVar9 + 0x18) < dVar15) && (sVar8 == 0)) {
          lVar12 = *(longlong *)(DAT_1429c6210 + (ulonglong)*(ushort *)(lVar9 + 8) * 8);
          if (*(char *)(lVar12 + 0x858) == '\0') {
            uVar4 = 1;
          }
          else {
            uVar4 = *(undefined2 *)(lVar12 + 0x808);
          }
          *(undefined2 *)(lVar9 + 0x20) = uVar4;
        }
      }
      else {
        lVar12 = *(longlong *)(DAT_1429c6210 + (ulonglong)*(ushort *)(lVar9 + 8) * 8);
        if (*(char *)(lVar12 + 0x858) == '\0') {
          *(undefined2 *)(lVar9 + 0x22) = 1;
        }
        else {
          *(undefined2 *)(lVar9 + 0x22) = *(undefined2 *)(lVar12 + 0x850);
        }
      }
      if (dVar15 < dVar14) {
        *(undefined4 *)(lVar9 + 0x20) = 0;
      }
      *(double *)(lVar9 + 0x18) = dVar15;
      lVar9 = *plVar7;
      lVar12 = *(longlong *)(DAT_1429c6210 + (ulonglong)*(ushort *)(lVar9 + 8) * 8);
      dVar15 = *(double *)(*(longlong *)(lVar12 + 0x710) + 0x50);
      if (dVar15 != dVar16) {
        dVar15 = dVar15 * (double)*(float *)(*(longlong *)
                                              (DAT_1429c5290 + (ulonglong)*(byte *)(lVar9 + 10) * 8)
                                            + 0x370);
      }
      *(double *)(lVar9 + 0x48) = dVar15;
      dVar15 = *(double *)(*(longlong *)(lVar12 + 0x710) + 0x58);
      if (dVar15 != dVar16) {
        dVar15 = dVar15 * (double)*(float *)(*(longlong *)
                                              (DAT_1429c5290 + (ulonglong)*(byte *)(lVar9 + 10) * 8)
                                            + 0x370);
      }
      *(double *)(lVar9 + 0x50) = dVar15;
    }
    plVar5 = (longlong *)(param_1 + 0x10);
    plVar7 = (longlong *)*plVar5;
    if (plVar7 != plVar5) {
      do {
        plVar11 = plVar7 + 2;
        if (plVar7 == (longlong *)0x0) {
          plVar11 = (longlong *)&DAT_00000028;
        }
        param_1 = local_res8;
        for (plVar10 = (longlong *)*plVar11; local_res8 = param_1, plVar10 != plVar11;
            plVar10 = (longlong *)*plVar10) {
          plVar6 = plVar10 + 0x23;
          if (plVar10 == (longlong *)0x0) {
            plVar6 = (longlong *)&DAT_00000130;
          }
          lVar9 = *plVar6;
          plVar6 = plVar10 + 0x22;
          if (plVar10 == (longlong *)0x0) {
            plVar6 = (longlong *)&DAT_00000128;
          }
          lVar12 = *plVar6;
          local_res18 = lVar9;
          if (lVar12 != lVar9) {
            do {
              lVar2 = *(longlong *)(lVar12 + 0x30);
              for (lVar13 = *(longlong *)(lVar12 + 0x28); lVar13 != lVar2; lVar13 = lVar13 + 0x40) {
                (**(code **)(**(longlong **)(lVar13 + 8) + 0x788))();
                lVar9 = local_res18;
              }
              lVar12 = lVar12 + 0x58;
            } while (lVar12 != lVar9);
          }
          param_1 = local_res8;
        }
        plVar7 = (longlong *)*plVar7;
      } while (plVar7 != plVar5);
      plVar5 = (longlong *)(param_1 + 0x10);
    }
    lVar9 = local_res20;
    for (plVar7 = (longlong *)*plVar5; plVar7 != plVar5; plVar7 = (longlong *)*plVar7) {
      plVar11 = plVar7 + -3;
      if (plVar7 == (longlong *)0x0) {
        plVar11 = (longlong *)0x0;
      }
      ElectricNetwork__update(plVar11,*(undefined8 *)(param_1 + 0x70));
    }
    if ((int)*(ulonglong *)(param_1 + 0x70) + (int)(*(ulonglong *)(param_1 + 0x70) / 0x1e) * -0x1e
        == 0) {
      ElectricNetworkManager__mergeAccumulatorGroups(param_1);
    }
    plVar5 = *(longlong **)(param_1 + 0x40);
    for (plVar7 = *(longlong **)(param_1 + 0x38); plVar7 != plVar5; plVar7 = plVar7 + 4) {
      plVar11 = (longlong *)plVar7[2];
      for (plVar10 = (longlong *)plVar7[1]; plVar10 != plVar11; plVar10 = plVar10 + 1) {
        dVar16 = *(double *)(*plVar10 + 0x20) - *(double *)(*plVar7 + 0x60);
        dVar14 = 0.0;
        if (0.0 <= dVar16) {
          dVar14 = dVar16;
        }
        *(double *)(*plVar10 + 0x20) = dVar14;
      }
    }
    plVar7 = (longlong *)std__chrono__steady_clock__now(&local_res8);
    LOCK();
    plVar5 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x90);
    *plVar5 = *plVar5 + (*plVar7 - lVar9);
    UNLOCK();
  }
  return;
}

