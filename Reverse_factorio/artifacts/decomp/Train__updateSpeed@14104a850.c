// Train::updateSpeed @ 14104a850 (function Train::updateSpeed)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void Train__updateSpeed(longlong param_1)

{
  char cVar1;
  longlong lVar2;
  longlong *plVar3;
  bool bVar4;
  double dVar5;
  undefined8 uVar6;
  double dVar7;
  double dVar8;
  uint uVar9;
  undefined4 uVar10;
  uint uVar11;
  undefined4 uVar12;
  undefined8 local_res8;
  
  uVar11 = _UNK_142405184;
  uVar9 = _DAT_142405180;
  dVar8 = *(double *)(param_1 + 0xb8);
  dVar7 = (double)CONCAT44((uint)((ulonglong)dVar8 >> 0x20) & _UNK_142405184,
                           SUB84(dVar8,0) & _DAT_142405180);
  dVar5 = *(double *)(param_1 + 0x188) / *(double *)(param_1 + 0x178);
  if (dVar5 <= dVar7) {
    dVar7 = dVar5;
  }
  if (*(int *)(param_1 + 0xa4) == 2) {
    uVar10 = 0;
    uVar12 = 0;
    lVar2 = (**(code **)(**(longlong **)(param_1 + 0x98) + 0x20))();
    if (lVar2 != 0) {
      uVar6 = AutomatedTrainController__getRemainingDistance(lVar2);
      uVar10 = (undefined4)uVar6;
      uVar12 = (undefined4)((ulonglong)uVar6 >> 0x20);
    }
    dVar8 = *(double *)(param_1 + 0xb8);
    dVar7 = (double)CONCAT44((uint)((ulonglong)dVar8 >> 0x20) & uVar11,SUB84(dVar8,0) & uVar9);
    if (0.0 < (double)CONCAT44(uVar12,uVar10)) {
      if ((double)CONCAT44(uVar12,uVar10) <= dVar7 && dVar7 != (double)CONCAT44(uVar12,uVar10)) {
        dVar7 = dVar7 - (double)CONCAT44(uVar12,uVar10);
      }
      dVar5 = (dVar8 * dVar8 * DAT_1424025f0) / (double)CONCAT44(uVar12,uVar10);
    }
    else {
      dVar5 = *(double *)(param_1 + 0x180) / *(double *)(param_1 + 0x178);
    }
    if (dVar5 <= dVar7) {
      dVar7 = dVar5;
    }
    if (dVar8 < 0.0) {
      dVar7 = (double)CONCAT44((uint)((ulonglong)dVar7 >> 0x20) ^ DAT_142405190._4_4_,
                               SUB84(dVar7,0) ^ (uint)DAT_142405190);
    }
    dVar8 = dVar8 - dVar7;
    *(double *)(param_1 + 0xb8) = dVar8;
    if (DAT_142402130 <=
        (double)CONCAT44((uint)((ulonglong)dVar8 >> 0x20) & uVar11,SUB84(dVar8,0) & uVar9)) {
      return;
    }
    *(undefined8 *)(param_1 + 0xb8) = 0;
    return;
  }
  if (dVar8 < 0.0) {
    dVar7 = (double)CONCAT44((uint)((ulonglong)dVar7 >> 0x20) ^ DAT_142405190._4_4_,
                             SUB84(dVar7,0) ^ (uint)DAT_142405190);
  }
  dVar5 = DAT_1424027a8 -
          *(double *)(param_1 + 400) / (*(double *)(param_1 + 0x178) / DAT_142402dd8);
  if (dVar5 <= DAT_142402210) {
    dVar5 = DAT_142402210;
  }
  dVar5 = dVar5 * (dVar8 - dVar7);
  *(double *)(param_1 + 0xb8) = dVar5;
  lVar2 = 0x1a0;
  if (0.0 <= dVar5) {
    lVar2 = 0x198;
  }
  local_res8 = *(undefined8 *)(lVar2 + param_1);
  uVar9 = (uint)local_res8;
  uVar11 = (uint)((ulonglong)local_res8 >> 0x20);
  if ((dVar5 == 0.0) && (plVar3 = *(longlong **)(param_1 + 0x30), plVar3 != (longlong *)0x0)) {
    if ((plVar3[0x44] != 0) && (plVar3[0x49] != 0)) {
                    /* WARNING: Subroutine does not return */
      Logging__logAndAbortOrThrow
                ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\RollingStock.cpp"
                 ,0x40c,9,
                 "Internal error in RollingStock::getNeighborDirection, neighbor not specified. Maybe related to https://forums.factorio.com/101113 ?"
                );
    }
    bVar4 = plVar3[0x44] == 0;
    do {
      cVar1 = (**(code **)(*plVar3 + 0x1380))(plVar3);
      if (cVar1 == '\0') goto LAB_14104a9c1;
      if (plVar3 == (longlong *)0x0) break;
      if (bVar4) {
        cVar1 = (char)plVar3[0x4c];
        plVar3 = plVar3 + 0x49;
      }
      else {
        cVar1 = (char)plVar3[0x47];
        plVar3 = plVar3 + 0x44;
      }
      bVar4 = cVar1 == '\0';
      plVar3 = (longlong *)*plVar3;
    } while (plVar3 != (longlong *)0x0);
  }
  if (*(int *)(param_1 + 0xa4) == 1) {
    uVar10 = 1;
  }
  else {
    if (*(int *)(param_1 + 0xa4) != 3) goto LAB_14104a9c1;
    uVar10 = 0;
  }
  Train__accelerate(param_1,uVar10,&local_res8,dVar8);
  uVar9 = (uint)local_res8;
  uVar11 = (uint)((ulonglong)local_res8 >> 0x20);
LAB_14104a9c1:
  if (0.0 <= (double)CONCAT44(uVar11,uVar9)) {
    dVar7 = (double)CONCAT44(uVar11 ^ DAT_142405190._4_4_,uVar9 ^ (uint)DAT_142405190);
    dVar8 = *(double *)(param_1 + 0xb8);
    if (*(double *)(param_1 + 0xb8) <= dVar7) {
      dVar8 = dVar7;
    }
    if ((double)CONCAT44(uVar11,uVar9) <= dVar8) {
      dVar8 = (double)CONCAT44(uVar11,uVar9);
    }
    *(double *)(param_1 + 0xb8) = dVar8;
    return;
  }
                    /* WARNING: Subroutine does not return */
  ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Train\\Train.cpp"
                      ,0x22c,"maxSpeed >= 0.0");
}

