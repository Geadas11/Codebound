// ElectricNetworkInputValues::computeUsage @ 14033f120 (function ElectricNetworkInputValues::computeUsage)

double * ElectricNetworkInputValues__computeUsage(longlong param_1,double *param_2)

{
  double dVar1;
  double dVar2;
  double dVar3;
  double dVar4;
  double dVar5;
  short sVar6;
  double dVar7;
  double dVar8;
  double dVar9;
  double dVar10;
  double dVar11;
  double dVar12;
  double dVar13;
  double dVar14;
  double dVar15;
  double dVar16;
  double dVar17;
  double dVar18;
  double dVar19;
  double dVar20;
  double dVar21;
  double dVar22;
  double dVar23;
  
  dVar19 = *(double *)(param_1 + 0x38);
  dVar1 = *(double *)(param_1 + 0x28);
  dVar2 = *(double *)(param_1 + 0x10);
  dVar16 = *(double *)(param_1 + 0x18);
  dVar3 = *(double *)(param_1 + 0x30);
  dVar18 = *(double *)(param_1 + 0x40);
  dVar21 = *(double *)(param_1 + 0x20);
  sVar6 = _dclass(dVar1);
  if (sVar6 != 2) {
    sVar6 = _dclass(dVar3);
    if (sVar6 != 2) {
      dVar14 = dVar1;
      if (dVar3 <= dVar1) {
        dVar14 = dVar3;
      }
      dVar20 = dVar1 - dVar14;
      dVar5 = dVar3 - dVar14;
      sVar6 = _dclass(dVar2);
      if (sVar6 == 2) goto LAB_14033f743;
      sVar6 = _dclass(dVar5);
      if (sVar6 != 2) {
        dVar15 = dVar2;
        if (dVar5 <= dVar2) {
          dVar15 = dVar5;
        }
        dVar2 = dVar2 - dVar15;
        dVar5 = dVar5 - dVar15;
        sVar6 = _dclass(dVar16);
        if (sVar6 == 2) goto LAB_14033f743;
        sVar6 = _dclass(dVar5);
        if (sVar6 != 2) {
          dVar7 = dVar16;
          if (dVar5 <= dVar16) {
            dVar7 = dVar5;
          }
          dVar16 = dVar16 - dVar7;
          dVar5 = dVar5 - dVar7;
          if (NAN(dVar21)) goto LAB_14033f743;
          sVar6 = _dclass(dVar5);
          if (sVar6 != 2) {
            dVar17 = dVar21;
            if (dVar5 <= dVar21) {
              dVar17 = dVar5;
            }
            dVar21 = dVar21 - dVar17;
            dVar5 = dVar5 - dVar17;
            sVar6 = _dclass(dVar20);
            if (sVar6 == 2) goto LAB_14033f743;
            sVar6 = _dclass(dVar19);
            if (sVar6 != 2) {
              dVar8 = dVar20;
              if (dVar19 <= dVar20) {
                dVar8 = dVar19;
              }
              dVar20 = dVar20 - dVar8;
              dVar19 = dVar19 - dVar8;
              sVar6 = _dclass(dVar2);
              if (sVar6 == 2) goto LAB_14033f743;
              sVar6 = _dclass(dVar19);
              if (sVar6 != 2) {
                dVar9 = dVar2;
                if (dVar19 <= dVar2) {
                  dVar9 = dVar19;
                }
                dVar2 = dVar2 - dVar9;
                dVar19 = dVar19 - dVar9;
                sVar6 = _dclass(dVar16);
                if (sVar6 == 2) goto LAB_14033f743;
                sVar6 = _dclass(dVar19);
                if (sVar6 != 2) {
                  dVar10 = dVar16;
                  if (dVar19 <= dVar16) {
                    dVar10 = dVar19;
                  }
                  dVar16 = dVar16 - dVar10;
                  dVar19 = dVar19 - dVar10;
                  sVar6 = _dclass(dVar21);
                  if (sVar6 == 2) goto LAB_14033f743;
                  sVar6 = _dclass(dVar19);
                  if (sVar6 != 2) {
                    dVar11 = dVar21;
                    if (dVar19 <= dVar21) {
                      dVar11 = dVar19;
                    }
                    dVar21 = dVar21 - dVar11;
                    dVar19 = dVar19 - dVar11;
                    sVar6 = _dclass(dVar20);
                    if (sVar6 == 2) goto LAB_14033f743;
                    sVar6 = _dclass(dVar18);
                    if (sVar6 != 2) {
                      dVar12 = dVar20;
                      if (dVar18 <= dVar20) {
                        dVar12 = dVar18;
                      }
                      dVar20 = dVar20 - dVar12;
                      dVar18 = dVar18 - dVar12;
                      sVar6 = _dclass(dVar2);
                      if (sVar6 == 2) goto LAB_14033f743;
                      sVar6 = _dclass(dVar18);
                      if (sVar6 != 2) {
                        dVar13 = dVar2;
                        if (dVar18 <= dVar2) {
                          dVar13 = dVar18;
                        }
                        dVar2 = dVar2 - dVar13;
                        dVar18 = dVar18 - dVar13;
                        sVar6 = _dclass(dVar16);
                        if (sVar6 == 2) goto LAB_14033f743;
                        sVar6 = _dclass(dVar18);
                        dVar23 = DAT_1424027a8;
                        if (sVar6 != 2) {
                          dVar4 = *(double *)(param_1 + 0x10);
                          dVar22 = dVar16;
                          if (dVar18 <= dVar16) {
                            dVar22 = dVar18;
                          }
                          dVar16 = dVar16 - dVar22;
                          dVar18 = dVar18 - dVar22;
                          dVar22 = dVar14 + 0.0 + dVar15 + dVar7 + dVar17 + dVar8 + dVar9 + dVar10 +
                                   dVar11 + dVar12 + dVar13 + dVar22;
                          *param_2 = dVar22;
                          if (dVar2 < dVar4) {
                            dVar14 = dVar23;
                            if (0.0 < dVar2) {
                              dVar14 = (dVar4 - dVar2) / dVar4;
                            }
                          }
                          else {
                            dVar14 = 0.0;
                          }
                          dVar2 = *(double *)(param_1 + 0x18);
                          param_2[6] = dVar14;
                          if (dVar16 < dVar2) {
                            dVar15 = dVar23;
                            if (0.0 < dVar16) {
                              dVar15 = (dVar2 - dVar16) / dVar2;
                            }
                          }
                          else {
                            dVar15 = 0.0;
                          }
                          dVar2 = *(double *)(param_1 + 0x20);
                          param_2[7] = dVar15;
                          if (dVar21 < dVar2) {
                            dVar16 = dVar23;
                            if (0.0 < dVar21) {
                              dVar16 = (dVar2 - dVar21) / dVar2;
                            }
                          }
                          else {
                            dVar16 = 0.0;
                          }
                          param_2[8] = dVar16;
                          if (dVar20 < dVar1) {
                            dVar16 = dVar23;
                            if (0.0 < dVar20) {
                              dVar16 = (dVar1 - dVar20) / dVar1;
                            }
                          }
                          else {
                            dVar16 = 0.0;
                          }
                          param_2[9] = dVar16;
                          if (dVar5 < dVar3) {
                            dVar21 = dVar23;
                            if (0.0 < dVar5) {
                              dVar21 = (dVar3 - dVar5) / dVar3;
                            }
                          }
                          else {
                            dVar21 = 0.0;
                          }
                          dVar5 = *(double *)(param_1 + 0x38);
                          param_2[10] = dVar21;
                          if (dVar19 < dVar5) {
                            dVar20 = dVar23;
                            if (0.0 < dVar19) {
                              dVar20 = (dVar5 - dVar19) / dVar5;
                            }
                          }
                          else {
                            dVar20 = 0.0;
                          }
                          dVar7 = *(double *)(param_1 + 0x40);
                          param_2[0xb] = dVar20;
                          if (dVar18 < dVar7) {
                            dVar17 = dVar23;
                            if (0.0 < dVar18) {
                              dVar17 = (dVar7 - dVar18) / dVar7;
                            }
                          }
                          else {
                            dVar17 = 0.0;
                          }
                          dVar18 = *(double *)(param_1 + 0x10);
                          param_2[0xc] = dVar17;
                          dVar3 = dVar3 + dVar5;
                          dVar1 = dVar1 + dVar18 + *(double *)(param_1 + 0x18);
                          dVar18 = dVar23;
                          if ((dVar19 != 0.0) || (dVar2 + dVar1 <= 0.0)) {
                            if (dVar19 < dVar5) {
                              if (0.0 < dVar19) {
                                dVar18 = (dVar5 - dVar19) / dVar5;
                              }
                            }
                            else {
                              dVar18 = 0.0;
                            }
                          }
                          param_2[5] = dVar18;
                          dVar19 = dVar22;
                          if (dVar22 <= dVar1) {
                            dVar19 = dVar1;
                          }
                          param_2[1] = dVar19;
                          if (0.0 < dVar22) {
                            dVar19 = dVar23;
                            if ((dVar22 < dVar1) &&
                               (((dVar16 != dVar23 || (dVar14 != dVar23)) || (dVar15 != dVar23)))) {
                              dVar19 = dVar22 / dVar1;
                            }
                          }
                          else {
                            dVar19 = 0.0;
                          }
                          param_2[2] = dVar19;
                          dVar19 = dVar22;
                          if (dVar22 <= dVar3) {
                            dVar19 = dVar3;
                          }
                          param_2[3] = dVar19;
                          if (0.0 < dVar22) {
                            if ((dVar22 < dVar3) && ((dVar21 != dVar23 || (dVar20 != dVar23)))) {
                              dVar23 = dVar22 / dVar3;
                            }
                          }
                          else {
                            dVar23 = (double)(0.0 < dVar2 + dVar1);
                          }
                          param_2[4] = dVar23;
                          return param_2;
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Energy\\ElectricNetworkValues.cpp"
                        ,0x36,"!std::isnan(demand.leftOver)");
  }
LAB_14033f743:
                    /* WARNING: Subroutine does not return */
  ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Energy\\ElectricNetworkValues.cpp"
                      ,0x35,"!std::isnan(output.leftOver)");
}

