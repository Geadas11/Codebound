// Train::updateMaxSpeed @ 141049850 (function Train::updateMaxSpeed)

void Train__updateMaxSpeed(longlong param_1)

{
  char cVar1;
  longlong lVar2;
  longlong lVar3;
  longlong *plVar4;
  ulonglong uVar5;
  longlong *plVar6;
  bool bVar7;
  double dVar8;
  double dVar9;
  double dVar10;
  double dVar11;
  undefined4 uVar12;
  undefined4 uVar13;
  double dVar14;
  
  lVar3 = *(longlong *)(param_1 + 0x30);
  uVar5 = 0;
  uVar12 = 0;
  uVar13 = 0;
  dVar10 = 0.0;
  dVar11 = DAT_142402b30;
  if (lVar3 != 0) {
    if ((*(longlong *)(lVar3 + 0x220) != 0) && (*(longlong *)(lVar3 + 0x248) != 0)) {
                    /* WARNING: Subroutine does not return */
      Logging__logAndAbortOrThrow
                ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\RollingStock.cpp"
                 ,0x40c,9,
                 "Internal error in RollingStock::getNeighborDirection, neighbor not specified. Maybe related to https://forums.factorio.com/101113 ?"
                );
    }
    bVar7 = *(longlong *)(lVar3 + 0x220) == 0;
    do {
      lVar2 = *(longlong *)(lVar3 + 0x48);
      if ((*(char *)(lVar2 + 0x918) == '\0') || (*(char *)(lVar2 + 0xb03) == '\0')) {
        dVar9 = *(double *)(lVar2 + 0x910);
        if (*(char *)(lVar2 + 0x918) != '\0') goto LAB_141049913;
      }
      else {
        dVar9 = *(double *)
                 (*(longlong *)(DAT_1429c5290 + (ulonglong)*(byte *)(lVar3 + 0x91) * 8) + 0x3b0) *
                *(double *)(lVar2 + 0x910);
LAB_141049913:
        dVar10 = dVar10 + dVar9;
        uVar5 = uVar5 + 1;
      }
      if (bVar7) {
        cVar1 = *(char *)(lVar3 + 0x260);
        lVar3 = *(longlong *)(lVar3 + 0x248);
      }
      else {
        cVar1 = *(char *)(lVar3 + 0x238);
        lVar3 = *(longlong *)(lVar3 + 0x220);
      }
      bVar7 = cVar1 == '\0';
    } while (lVar3 != 0);
    if (uVar5 != 0) {
      if ((longlong)uVar5 < 0) {
        dVar11 = dVar10 / (double)uVar5;
      }
      else {
        dVar11 = dVar10 / (double)(longlong)uVar5;
      }
    }
  }
  plVar6 = *(longlong **)(param_1 + 0x70);
  plVar4 = *(longlong **)(param_1 + 0x68);
  dVar9 = DAT_1424027a8;
  dVar10 = DAT_1424027a8;
  dVar14 = DAT_142402b30;
  if (plVar4 != plVar6) {
    dVar10 = 0.0;
    do {
      dVar8 = dVar9;
      if (*(char *)(*(longlong *)(*plVar4 + 0x300) + 0x10) == '\0') {
        dVar8 = (double)Burner__getCurrentFuelTopSpeedModifier();
      }
      plVar4 = plVar4 + 1;
      dVar10 = dVar10 + dVar8;
    } while (plVar4 != plVar6);
    dVar10 = dVar10 / (double)(ulonglong)((longlong)plVar6 - *(longlong *)(param_1 + 0x68) >> 3);
  }
  dVar10 = dVar10 * dVar11;
  if (dVar10 <= (double)CONCAT44(uVar13,uVar12)) {
    dVar10 = (double)CONCAT44(uVar13,uVar12);
  }
  if (dVar14 <= dVar10) {
    dVar10 = dVar14;
  }
  *(double *)(param_1 + 0x198) = dVar10;
  plVar6 = *(longlong **)(param_1 + 0x88);
  plVar4 = *(longlong **)(param_1 + 0x80);
  if (plVar4 != plVar6) {
    dVar10 = (double)CONCAT44(uVar13,uVar12);
    do {
      dVar8 = dVar9;
      if (*(char *)(*(longlong *)(*plVar4 + 0x300) + 0x10) == '\0') {
        dVar8 = (double)Burner__getCurrentFuelTopSpeedModifier();
      }
      plVar4 = plVar4 + 1;
      dVar10 = dVar10 + dVar8;
    } while (plVar4 != plVar6);
    dVar9 = dVar10 / (double)(ulonglong)
                             (*(longlong *)(param_1 + 0x70) - *(longlong *)(param_1 + 0x68) >> 3);
  }
  dVar9 = dVar9 * dVar11;
  if (dVar9 <= (double)CONCAT44(uVar13,uVar12)) {
    dVar9 = (double)CONCAT44(uVar13,uVar12);
  }
  if (dVar14 <= dVar9) {
    dVar9 = dVar14;
  }
  *(double *)(param_1 + 0x1a0) = dVar9;
  return;
}

