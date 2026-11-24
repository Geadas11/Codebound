// ElectricSubNetwork::useProduction @ 140341800 (function ElectricSubNetwork::useProduction)

void ElectricSubNetwork__useProduction
               (longlong param_1,double param_2,longlong *param_3,undefined8 param_4)

{
  double dVar1;
  uint uVar2;
  longlong *plVar3;
  longlong *plVar4;
  longlong lVar5;
  longlong lVar6;
  longlong *plVar7;
  float *pfVar8;
  longlong *plVar9;
  longlong *plVar10;
  longlong *plVar11;
  longlong *plVar12;
  bool bVar13;
  bool bVar14;
  double dVar15;
  double dVar16;
  double dVar17;
  double dVar18;
  
  dVar15 = DAT_142403280;
  dVar1 = DAT_1424027a8;
  plVar10 = (longlong *)*param_3;
  plVar3 = (longlong *)param_3[1];
  if (DAT_142401ed0 < param_2) {
    if (plVar10 != plVar3) {
      plVar11 = plVar10 + 9;
      do {
        dVar18 = *(double *)(*plVar10 + 0x58);
        bVar13 = NAN(dVar18) || NAN(dVar15);
        bVar14 = dVar18 == dVar15;
        if (!bVar14) {
          dVar18 = dVar18 * (double)*(float *)(*(longlong *)
                                                (DAT_1429c5290 +
                                                (ulonglong)*(byte *)((longlong)plVar11 + -0x3e) * 8)
                                              + 0x370);
          bVar13 = NAN(dVar18) || NAN(dVar15);
          bVar14 = dVar18 == dVar15;
        }
        if ((bVar13) || (!bVar14)) {
          if (dVar18 != 0.0) {
            lVar6 = plVar11[-4];
            lVar5 = plVar11[-3];
            if (param_2 == dVar1) {
              for (; lVar6 != lVar5; lVar6 = lVar6 + 0x40) {
                dVar17 = *(double *)(lVar6 + 0x20) - dVar18;
                if (dVar17 <= 0.0) {
                  dVar17 = 0.0;
                }
                *(double *)(lVar6 + 0x20) = dVar17;
              }
              plVar4 = (longlong *)*plVar11;
              for (plVar12 = (longlong *)plVar11[-1]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
                lVar6 = *plVar12;
                if (*(longlong *)(*(longlong *)(lVar6 + 0x18) + 0x38) !=
                    *(longlong *)(param_1 + 0x38)) {
                  uVar2 = **(uint **)(lVar6 + 0x30);
                  if (uVar2 != 0) {
                    plVar7 = *(longlong **)(*(uint **)(lVar6 + 0x30) + 2);
                    plVar9 = plVar7 + (ulonglong)uVar2 * 2;
                    while( true ) {
                      if (plVar7 == plVar9) goto LAB_140341cc0;
                      if (*plVar7 == param_1) break;
                      if (*(longlong *)(*plVar7 + 0x38) == *(longlong *)(param_1 + 0x38))
                      goto LAB_140341b42;
                      plVar7 = plVar7 + 2;
                    }
                    dVar17 = *(double *)(lVar6 + 0x20) - dVar18;
                    if (dVar17 <= 0.0) {
                      dVar17 = 0.0;
                    }
                    *(double *)(lVar6 + 0x20) = dVar17;
                  }
                }
LAB_140341b42:
              }
            }
            else {
              for (; lVar6 != lVar5; lVar6 = lVar6 + 0x40) {
                dVar17 = *(double *)(lVar6 + 0x20);
                dVar16 = dVar17;
                if (dVar18 <= dVar17) {
                  dVar16 = dVar18;
                }
                *(double *)(lVar6 + 0x20) = dVar17 - dVar16 * param_2;
              }
              plVar4 = (longlong *)*plVar11;
              for (plVar12 = (longlong *)plVar11[-1]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
                lVar6 = *plVar12;
                if (*(longlong *)(*(longlong *)(lVar6 + 0x18) + 0x38) !=
                    *(longlong *)(param_1 + 0x38)) {
                  uVar2 = **(uint **)(lVar6 + 0x30);
                  if (uVar2 != 0) {
                    plVar7 = *(longlong **)(*(uint **)(lVar6 + 0x30) + 2);
                    plVar9 = plVar7 + (ulonglong)uVar2 * 2;
                    while( true ) {
                      if (plVar7 == plVar9) goto LAB_140341cc0;
                      if (*plVar7 == param_1) break;
                      if (*(longlong *)(*plVar7 + 0x38) == *(longlong *)(param_1 + 0x38))
                      goto LAB_140341bf8;
                      plVar7 = plVar7 + 2;
                    }
                    dVar17 = *(double *)(lVar6 + 0x20);
                    dVar16 = dVar17;
                    if (dVar18 <= dVar17) {
                      dVar16 = dVar18;
                    }
                    *(double *)(lVar6 + 0x20) = dVar17 - dVar16 * param_2;
                  }
                }
LAB_140341bf8:
              }
            }
          }
        }
        else {
          lVar6 = plVar11[-4];
          lVar5 = plVar11[-3];
          if (param_2 == dVar1) {
            for (; lVar6 != lVar5; lVar6 = lVar6 + 0x40) {
              *(undefined8 *)(lVar6 + 0x20) = 0;
            }
            plVar4 = (longlong *)*plVar11;
            for (plVar12 = (longlong *)plVar11[-1]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
              lVar6 = *plVar12;
              if (*(longlong *)(*(longlong *)(lVar6 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)
                 ) {
                uVar2 = **(uint **)(lVar6 + 0x30);
                if (uVar2 != 0) {
                  plVar7 = *(longlong **)(*(uint **)(lVar6 + 0x30) + 2);
                  plVar9 = plVar7 + (ulonglong)uVar2 * 2;
                  do {
                    if (plVar7 == plVar9) {
LAB_140341cc0:
                    /* WARNING: Subroutine does not return */
                      Logging__logAndAbortOrThrow
                                ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Energy\\ElectricEnergySource.cpp"
                                 ,0x234,9,"Internal electric network error");
                    }
                    if (*plVar7 == param_1) {
                      *(undefined8 *)(lVar6 + 0x20) = 0;
                      break;
                    }
                    if (*(longlong *)(*plVar7 + 0x38) == *(longlong *)(param_1 + 0x38)) break;
                    plVar7 = plVar7 + 2;
                  } while( true );
                }
              }
            }
          }
          else {
            for (; lVar6 != lVar5; lVar6 = lVar6 + 0x40) {
              *(double *)(lVar6 + 0x20) =
                   *(double *)(lVar6 + 0x20) - *(double *)(lVar6 + 0x20) * param_2;
            }
            plVar4 = (longlong *)*plVar11;
            for (plVar12 = (longlong *)plVar11[-1]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
              lVar6 = *plVar12;
              if (*(longlong *)(*(longlong *)(lVar6 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)
                 ) {
                uVar2 = **(uint **)(lVar6 + 0x30);
                if (uVar2 != 0) {
                  plVar7 = *(longlong **)(*(uint **)(lVar6 + 0x30) + 2);
                  plVar9 = plVar7 + (ulonglong)uVar2 * 2;
                  while( true ) {
                    if (plVar7 == plVar9) goto LAB_140341cc0;
                    if (*plVar7 == param_1) break;
                    if (*(longlong *)(*plVar7 + 0x38) == *(longlong *)(param_1 + 0x38))
                    goto LAB_140341a74;
                    plVar7 = plVar7 + 2;
                  }
                  *(double *)(lVar6 + 0x20) =
                       *(double *)(lVar6 + 0x20) - *(double *)(lVar6 + 0x20) * param_2;
                }
              }
LAB_140341a74:
            }
          }
        }
        lVar6 = plVar11[-3];
        lVar5 = plVar11[-4];
        if ((lVar5 != lVar6) && (dVar18 = (double)plVar11[-7], 0.0 < dVar18)) {
          do {
            dVar17 = *(double *)(lVar5 + 0x20) - dVar18;
            if (dVar17 <= 0.0) {
              dVar17 = 0.0;
            }
            *(double *)(lVar5 + 0x20) = dVar17;
            lVar5 = lVar5 + 0x40;
          } while (lVar5 != lVar6);
        }
        if ((short)plVar11[-8] != 0) {
          dVar18 = (double)plVar11[-6];
          pfVar8 = (float *)FlatMap<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,float,std::less<void>_>__private_subscript
                                      (param_4,plVar11 + -8);
          *pfVar8 = (float)(param_2 * dVar18) + *pfVar8;
        }
        plVar11[-6] = -0x4010000000000000;
        plVar10 = plVar10 + 0xb;
        plVar11 = plVar11 + 0xb;
      } while (plVar10 != plVar3);
    }
  }
  else if (plVar10 != plVar3) {
    do {
      lVar6 = plVar10[6];
      lVar5 = plVar10[5];
      if ((lVar5 != lVar6) && (dVar1 = (double)plVar10[2], 0.0 < dVar1)) {
        do {
          dVar15 = *(double *)(lVar5 + 0x20) - dVar1;
          if (dVar15 <= 0.0) {
            dVar15 = 0.0;
          }
          *(double *)(lVar5 + 0x20) = dVar15;
          lVar5 = lVar5 + 0x40;
        } while (lVar5 != lVar6);
      }
      plVar10 = plVar10 + 0xb;
    } while (plVar10 != plVar3);
  }
  return;
}

