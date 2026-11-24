// ElectricSubNetwork::useDemand @ 140341ce0 (function ElectricSubNetwork::useDemand)

void ElectricSubNetwork__useDemand
               (longlong param_1,double param_2,undefined8 *param_3,undefined8 param_4)

{
  uint uVar1;
  longlong *plVar2;
  longlong lVar3;
  longlong *plVar4;
  double dVar5;
  double dVar6;
  float *pfVar7;
  longlong *plVar8;
  longlong lVar9;
  longlong *plVar10;
  longlong lVar11;
  longlong *plVar12;
  longlong *plVar13;
  double dVar14;
  double dVar15;
  double dVar16;
  double dVar17;
  
  dVar6 = DAT_142403280;
  dVar5 = DAT_1424027a8;
  if (DAT_142401ed0 < param_2) {
    plVar2 = (longlong *)param_3[1];
    for (plVar13 = (longlong *)*param_3; plVar13 != plVar2; plVar13 = plVar13 + 0xb) {
      lVar9 = *plVar13;
      dVar17 = *(double *)(lVar9 + 0x50);
      if (dVar17 != dVar6) {
        dVar17 = dVar17 * (double)*(float *)(*(longlong *)
                                              (DAT_1429c5290 +
                                              (ulonglong)*(byte *)((longlong)plVar13 + 10) * 8) +
                                            0x370);
      }
      if (*(char *)((longlong)plVar13 + 0xc) == '\0') {
        if (dVar17 == dVar6) {
          lVar9 = plVar13[5];
          lVar3 = plVar13[6];
          if (param_2 == dVar5) {
            for (; lVar9 != lVar3; lVar9 = lVar9 + 0x40) {
              *(undefined8 *)(lVar9 + 0x20) = *(undefined8 *)(lVar9 + 0x28);
            }
            plVar4 = (longlong *)plVar13[9];
            for (plVar12 = (longlong *)plVar13[8]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
              lVar9 = *plVar12;
              if (*(longlong *)(*(longlong *)(lVar9 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)
                 ) {
                uVar1 = **(uint **)(lVar9 + 0x30);
                if (uVar1 != 0) {
                  plVar8 = *(longlong **)(*(uint **)(lVar9 + 0x30) + 2);
                  plVar10 = plVar8 + (ulonglong)uVar1 * 2;
                  while( true ) {
                    if (plVar8 == plVar10) goto LAB_140342269;
                    if (*plVar8 == param_1) break;
                    if (*(longlong *)(*plVar8 + 0x38) == *(longlong *)(param_1 + 0x38))
                    goto LAB_140341ff5;
                    plVar8 = plVar8 + 2;
                  }
                  *(undefined8 *)(lVar9 + 0x20) = *(undefined8 *)(lVar9 + 0x28);
                }
              }
LAB_140341ff5:
            }
          }
          else {
            for (; lVar9 != lVar3; lVar9 = lVar9 + 0x40) {
              dVar17 = *(double *)(lVar9 + 0x28);
              dVar14 = (dVar17 - *(double *)(lVar9 + 0x20)) * param_2 + *(double *)(lVar9 + 0x20);
              if (dVar14 <= dVar17) {
                dVar17 = dVar14;
              }
              *(double *)(lVar9 + 0x20) = dVar17;
            }
            plVar4 = (longlong *)plVar13[9];
            for (plVar12 = (longlong *)plVar13[8]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
              lVar9 = *plVar12;
              if (*(longlong *)(*(longlong *)(lVar9 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)
                 ) {
                uVar1 = **(uint **)(lVar9 + 0x30);
                if (uVar1 != 0) {
                  plVar8 = *(longlong **)(*(uint **)(lVar9 + 0x30) + 2);
                  plVar10 = plVar8 + (ulonglong)uVar1 * 2;
                  while( true ) {
                    if (plVar8 == plVar10) goto LAB_140342269;
                    if (*plVar8 == param_1) break;
                    if (*(longlong *)(*plVar8 + 0x38) == *(longlong *)(param_1 + 0x38))
                    goto LAB_1403420c0;
                    plVar8 = plVar8 + 2;
                  }
                  dVar17 = *(double *)(lVar9 + 0x28);
                  dVar14 = (dVar17 - *(double *)(lVar9 + 0x20)) * param_2 +
                           *(double *)(lVar9 + 0x20);
                  if (dVar14 <= dVar17) {
                    dVar17 = dVar14;
                  }
                  *(double *)(lVar9 + 0x20) = dVar17;
                }
              }
LAB_1403420c0:
            }
          }
        }
        else if (dVar17 != 0.0) {
          lVar9 = plVar13[5];
          lVar3 = plVar13[6];
          if (param_2 == dVar5) {
            for (; lVar9 != lVar3; lVar9 = lVar9 + 0x40) {
              dVar16 = dVar17 + *(double *)(lVar9 + 0x20);
              dVar14 = *(double *)(lVar9 + 0x28);
              if (dVar16 <= *(double *)(lVar9 + 0x28)) {
                dVar14 = dVar16;
              }
              *(double *)(lVar9 + 0x20) = dVar14;
            }
            plVar4 = (longlong *)plVar13[9];
            for (plVar12 = (longlong *)plVar13[8]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
              lVar9 = *plVar12;
              if (*(longlong *)(*(longlong *)(lVar9 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)
                 ) {
                uVar1 = **(uint **)(lVar9 + 0x30);
                if (uVar1 != 0) {
                  plVar8 = *(longlong **)(*(uint **)(lVar9 + 0x30) + 2);
                  plVar10 = plVar8 + (ulonglong)uVar1 * 2;
                  while( true ) {
                    if (plVar8 == plVar10) goto LAB_140342269;
                    if (*plVar8 == param_1) break;
                    if (*(longlong *)(*plVar8 + 0x38) == *(longlong *)(param_1 + 0x38))
                    goto LAB_140342196;
                    plVar8 = plVar8 + 2;
                  }
                  dVar16 = dVar17 + *(double *)(lVar9 + 0x20);
                  dVar14 = *(double *)(lVar9 + 0x28);
                  if (dVar16 <= *(double *)(lVar9 + 0x28)) {
                    dVar14 = dVar16;
                  }
                  *(double *)(lVar9 + 0x20) = dVar14;
                }
              }
LAB_140342196:
            }
          }
          else {
            for (; lVar9 != lVar3; lVar9 = lVar9 + 0x40) {
              dVar14 = *(double *)(lVar9 + 0x28);
              dVar15 = dVar14 - *(double *)(lVar9 + 0x20);
              dVar16 = dVar17;
              if (dVar15 <= dVar17) {
                dVar16 = dVar15;
              }
              dVar16 = dVar16 * param_2 + *(double *)(lVar9 + 0x20);
              if (dVar16 <= dVar14) {
                dVar14 = dVar16;
              }
              *(double *)(lVar9 + 0x20) = dVar14;
            }
            plVar4 = (longlong *)plVar13[9];
            for (plVar12 = (longlong *)plVar13[8]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
              lVar9 = *plVar12;
              if (*(longlong *)(*(longlong *)(lVar9 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)
                 ) {
                uVar1 = **(uint **)(lVar9 + 0x30);
                if (uVar1 != 0) {
                  plVar8 = *(longlong **)(*(uint **)(lVar9 + 0x30) + 2);
                  plVar10 = plVar8 + (ulonglong)uVar1 * 2;
                  while( true ) {
                    if (plVar8 == plVar10) goto LAB_140342269;
                    if (*plVar8 == param_1) break;
                    if (*(longlong *)(*plVar8 + 0x38) == *(longlong *)(param_1 + 0x38))
                    goto LAB_14034225b;
                    plVar8 = plVar8 + 2;
                  }
                  dVar14 = *(double *)(lVar9 + 0x28);
                  dVar15 = dVar14 - *(double *)(lVar9 + 0x20);
                  dVar16 = dVar17;
                  if (dVar15 <= dVar17) {
                    dVar16 = dVar15;
                  }
                  dVar16 = dVar16 * param_2 + *(double *)(lVar9 + 0x20);
                  if (dVar16 <= dVar14) {
                    dVar14 = dVar16;
                  }
                  *(double *)(lVar9 + 0x20) = dVar14;
                }
              }
LAB_14034225b:
            }
          }
        }
      }
      else {
        lVar3 = plVar13[6];
        for (lVar11 = plVar13[5]; lVar11 != lVar3; lVar11 = lVar11 + 0x40) {
          dVar14 = (double)AmmoTurret__getElectricInputFlowLimit
                                     (*(undefined8 *)(lVar11 + 8),lVar9,
                                      *(undefined1 *)((longlong)plVar13 + 10));
          dVar17 = *(double *)(lVar11 + 0x28);
          dVar16 = dVar17 - *(double *)(lVar11 + 0x20);
          if (dVar16 <= dVar14) {
            dVar14 = dVar16;
          }
          dVar14 = dVar14 * param_2 + *(double *)(lVar11 + 0x20);
          if (dVar14 <= dVar17) {
            dVar17 = dVar14;
          }
          *(double *)(lVar11 + 0x20) = dVar17;
        }
        plVar4 = (longlong *)plVar13[9];
        for (plVar12 = (longlong *)plVar13[8]; plVar12 != plVar4; plVar12 = plVar12 + 1) {
          lVar3 = *plVar12;
          if (*(longlong *)(*(longlong *)(lVar3 + 0x18) + 0x38) != *(longlong *)(param_1 + 0x38)) {
            uVar1 = **(uint **)(lVar3 + 0x30);
            if (uVar1 != 0) {
              plVar8 = *(longlong **)(*(uint **)(lVar3 + 0x30) + 2);
              plVar10 = plVar8 + (ulonglong)uVar1 * 2;
              do {
                if (plVar8 == plVar10) {
LAB_140342269:
                    /* WARNING: Subroutine does not return */
                  Logging__logAndAbortOrThrow
                            ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Energy\\ElectricEnergySource.cpp"
                             ,0x234,9,"Internal electric network error");
                }
                if (*plVar8 == param_1) {
                  dVar14 = (double)AmmoTurret__getElectricInputFlowLimit
                                             (*(undefined8 *)(lVar3 + 8),lVar9,
                                              *(undefined1 *)((longlong)plVar13 + 10));
                  dVar17 = *(double *)(lVar3 + 0x28);
                  dVar16 = dVar17 - *(double *)(lVar3 + 0x20);
                  if (dVar16 <= dVar14) {
                    dVar14 = dVar16;
                  }
                  dVar14 = dVar14 * param_2 + *(double *)(lVar3 + 0x20);
                  if (dVar14 <= dVar17) {
                    dVar17 = dVar14;
                  }
                  *(double *)(lVar3 + 0x20) = dVar17;
                  break;
                }
                if (*(longlong *)(*plVar8 + 0x38) == *(longlong *)(param_1 + 0x38)) break;
                plVar8 = plVar8 + 2;
              } while( true );
            }
          }
        }
      }
      if ((short)plVar13[1] != 0) {
        dVar17 = (double)plVar13[4];
        pfVar7 = (float *)FlatMap<IDWithQuality<ID<EntityPrototype,unsigned_short>_>,float,std::less<void>_>__private_subscript
                                    (param_4,plVar13 + 1);
        *pfVar7 = (float)(param_2 * dVar17) + *pfVar7;
      }
      plVar13[4] = -0x4010000000000000;
    }
  }
  return;
}

