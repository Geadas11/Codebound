// ElectricSubNetwork::useProductionAndDemand @ 140346730 (function ElectricSubNetwork::useProductionAndDemand)

void ElectricSubNetwork__useProductionAndDemand(longlong param_1,longlong param_2,longlong param_3)

{
  char cVar1;
  uint uVar2;
  undefined8 uVar3;
  longlong lVar4;
  longlong lVar5;
  double dVar6;
  float *pfVar7;
  longlong *plVar8;
  longlong *plVar9;
  longlong *plVar10;
  longlong *plVar11;
  longlong lVar12;
  longlong *plVar13;
  longlong lVar14;
  double dVar15;
  double dVar16;
  double dVar17;
  double dVar18;
  
  dVar18 = DAT_142402278;
  dVar17 = *(double *)(param_2 + 0x48);
  if (dVar17 != 0.0) {
    plVar13 = (longlong *)**(longlong **)(param_1 + 0x140);
    cVar1 = *(char *)((longlong)plVar13 + 0x19);
    while (cVar1 == '\0') {
      plVar11 = *(longlong **)plVar13[5];
      uVar3 = *(undefined8 *)
               (*(longlong *)(*(longlong *)(param_1 + 0x28) + 0x2c0) +
               (ulonglong)*(uint *)(plVar13 + 4) * 8);
      cVar1 = *(char *)((longlong)plVar11 + 0x19);
      while (cVar1 == '\0') {
        dVar15 = (double)SolarPanelPrototype__effectivePowerOutput
                                   (*(undefined8 *)
                                     (DAT_1429c6210 + (ulonglong)*(ushort *)(plVar11 + 4) * 8),
                                    *(undefined1 *)((longlong)plVar11 + 0x22),uVar3);
        lVar14 = plVar11[5];
        pfVar7 = (float *)FlatMap<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,float,std::less<void>_>__private_subscript
                                    (param_3 + 0x18,plVar11 + 4);
        *pfVar7 = (float)((double)lVar14 * dVar18 * dVar15 * dVar17) + *pfVar7;
        plVar9 = (longlong *)plVar11[2];
        if (*(char *)((longlong)plVar9 + 0x19) == '\0') {
          cVar1 = *(char *)(*plVar9 + 0x19);
          plVar11 = plVar9;
          plVar9 = (longlong *)*plVar9;
          while (cVar1 == '\0') {
            cVar1 = *(char *)(*plVar9 + 0x19);
            plVar11 = plVar9;
            plVar9 = (longlong *)*plVar9;
          }
        }
        else {
          cVar1 = *(char *)(plVar11[1] + 0x19);
          plVar8 = (longlong *)plVar11[1];
          plVar9 = plVar11;
          while ((plVar11 = plVar8, cVar1 == '\0' && (plVar9 == (longlong *)plVar11[2]))) {
            cVar1 = *(char *)(plVar11[1] + 0x19);
            plVar8 = (longlong *)plVar11[1];
            plVar9 = plVar11;
          }
        }
        cVar1 = *(char *)((longlong)plVar11 + 0x19);
      }
      plVar11 = (longlong *)plVar13[2];
      if (*(char *)((longlong)plVar11 + 0x19) == '\0') {
        cVar1 = *(char *)(*plVar11 + 0x19);
        plVar13 = plVar11;
        plVar11 = (longlong *)*plVar11;
        while (cVar1 == '\0') {
          cVar1 = *(char *)(*plVar11 + 0x19);
          plVar13 = plVar11;
          plVar11 = (longlong *)*plVar11;
        }
      }
      else {
        cVar1 = *(char *)(plVar13[1] + 0x19);
        plVar9 = (longlong *)plVar13[1];
        plVar11 = plVar13;
        while ((plVar13 = plVar9, cVar1 == '\0' && (plVar11 == (longlong *)plVar13[2]))) {
          cVar1 = *(char *)(plVar13[1] + 0x19);
          plVar9 = (longlong *)plVar13[1];
          plVar11 = plVar13;
        }
      }
      cVar1 = *(char *)((longlong)plVar13 + 0x19);
    }
  }
  ElectricSubNetwork__useProduction
            (param_1,*(undefined8 *)(param_2 + 0x30),param_1 + 0x98,param_3 + 0x18);
  ElectricSubNetwork__useProduction
            (param_1,*(undefined8 *)(param_2 + 0x38),param_1 + 0xb0,param_3 + 0x18);
  ElectricSubNetwork__useProduction
            (param_1,*(undefined8 *)(param_2 + 0x40),param_1 + 200,param_3 + 0x18);
  ElectricSubNetwork__useDemand(param_1,*(undefined8 *)(param_2 + 0x50),param_1 + 0x68,param_3);
  ElectricSubNetwork__useDemand(param_1,*(undefined8 *)(param_2 + 0x58),param_1 + 0x80,param_3);
  ElectricSubNetwork__useLampsDemand(param_1,*(undefined8 *)(param_2 + 0x28),param_3);
  ElectricSubNetwork__useDemand(param_1,*(undefined8 *)(param_2 + 0x60),param_1 + 200,param_3);
  plVar13 = *(longlong **)(param_1 + 0x150);
  plVar11 = *(longlong **)(param_1 + 0x158);
  do {
    if (plVar13 == plVar11) {
      lVar14 = *(longlong *)(param_1 + 0x128);
      lVar4 = *(longlong *)(param_1 + 0x130);
      if (lVar14 != lVar4) {
        plVar13 = (longlong *)(lVar14 + 0x48);
        do {
          lVar5 = plVar13[-3];
          for (lVar12 = plVar13[-4]; lVar12 != lVar5; lVar12 = lVar12 + 0x40) {
            (**(code **)(**(longlong **)(lVar12 + 8) + 0x798))
                      (*(longlong **)(lVar12 + 8),param_2,param_3,1);
          }
          plVar9 = (longlong *)*plVar13;
          for (plVar11 = (longlong *)plVar13[-1]; plVar11 != plVar9; plVar11 = plVar11 + 1) {
            lVar5 = *plVar11;
            if (*(longlong *)(*(longlong *)(lVar5 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38))
            {
              uVar2 = **(uint **)(lVar5 + 0x30);
              if (uVar2 != 0) {
                plVar8 = *(longlong **)(*(uint **)(lVar5 + 0x30) + 2);
                plVar10 = plVar8 + (ulonglong)uVar2 * 2;
                while( true ) {
                  if (plVar8 == plVar10) {
                    /* WARNING: Subroutine does not return */
                    Logging__logAndAbortOrThrow
                              ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Energy\\ElectricEnergySource.cpp"
                               ,0x234,9,"Internal electric network error");
                  }
                  if (*plVar8 == param_1) break;
                  if (*(longlong *)(*plVar8 + 0x38) == *(longlong *)(param_1 + 0x38))
                  goto LAB_140346b93;
                  plVar8 = plVar8 + 2;
                }
                (**(code **)(**(longlong **)(lVar5 + 8) + 0x798))
                          (*(longlong **)(lVar5 + 8),param_2,param_3,0);
              }
            }
LAB_140346b93:
          }
          lVar14 = lVar14 + 0x58;
          plVar13 = plVar13 + 0xb;
        } while (lVar14 != lVar4);
      }
      return;
    }
    lVar14 = *plVar13;
    plVar9 = *(longlong **)(lVar14 + 0x28);
    while( true ) {
      if (plVar9 == *(longlong **)(lVar14 + 0x30)) {
                    /* WARNING: Subroutine does not return */
        Logging__logAndAbortOrThrow
                  ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\AccumulatorGroup.cpp"
                   ,0x132,9,"Internal accumulator group error");
      }
      if (*plVar9 == param_1) break;
      if (*(longlong *)(*plVar9 + 0x38) == *(longlong *)(param_1 + 0x38)) goto LAB_140346abf;
      plVar9 = plVar9 + 1;
    }
    dVar17 = *(double *)(lVar14 + 0x10);
    dVar18 = *(double *)(lVar14 + 0x50);
    dVar15 = dVar18;
    if (dVar17 <= dVar18) {
      dVar15 = dVar17;
    }
    dVar16 = *(double *)(lVar14 + 0x40) - dVar17;
    dVar15 = dVar15 * *(double *)(param_2 + 0x40);
    dVar6 = *(double *)(lVar14 + 0x48);
    if (dVar16 <= *(double *)(lVar14 + 0x48)) {
      dVar6 = dVar16;
    }
    if (0.0 < dVar15) {
      uVar2 = *(uint *)(lVar14 + 0x58);
      *(double *)(lVar14 + 0x10) = dVar17 - dVar15;
      *(double *)(lVar14 + 0x50) = dVar18 - dVar15;
      pfVar7 = (float *)FlatMap<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,float,std::less<void>_>__private_subscript
                                  (param_3 + 0x18,lVar14 + 8);
      *pfVar7 = (float)((double)uVar2 * dVar15) + *pfVar7;
      dVar17 = *(double *)(lVar14 + 0x10);
    }
    dVar6 = dVar6 * *(double *)(param_2 + 0x60);
    if (0.0 < dVar6) {
      uVar2 = *(uint *)(lVar14 + 0x58);
      dVar18 = dVar17 + dVar6;
      if (*(double *)(lVar14 + 0x40) <= dVar17 + dVar6) {
        dVar18 = *(double *)(lVar14 + 0x40);
      }
      *(double *)(lVar14 + 0x48) = *(double *)(lVar14 + 0x48) - dVar6;
      *(double *)(lVar14 + 0x10) = dVar18;
      pfVar7 = (float *)FlatMap<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,float,std::less<void>_>__private_subscript
                                  (param_3,lVar14 + 8);
      *pfVar7 = (float)((double)uVar2 * dVar6) + *pfVar7;
    }
    uVar2 = *(uint *)(lVar14 + 0x58);
    dVar17 = *(double *)(lVar14 + 0x10);
    pfVar7 = (float *)FlatMap<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,float,std::less<void>_>__private_subscript
                                (param_3 + 0x30,lVar14 + 8);
    *pfVar7 = (float)((double)uVar2 * dVar17) + *pfVar7;
LAB_140346abf:
    plVar13 = plVar13 + 1;
  } while( true );
}

