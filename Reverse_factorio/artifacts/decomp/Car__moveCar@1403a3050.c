// Car::moveCar @ 1403a3050 (function Car::moveCar)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void Car__moveCar(longlong param_1,undefined8 param_2,double param_3)

{
  undefined8 *puVar1;
  longlong *plVar2;
  double dVar3;
  char cVar4;
  undefined8 *puVar5;
  longlong lVar6;
  float fVar7;
  uint uVar8;
  double dVar9;
  double dVar10;
  uint uVar12;
  double extraout_XMM0_Qa;
  double extraout_XMM0_Qa_00;
  uint uVar13;
  undefined1 auVar11 [16];
  uint uVar14;
  undefined8 extraout_XMM0_Qb;
  uint uVar15;
  uint uVar16;
  uint uVar17;
  uint uVar18;
  uint uVar19;
  undefined1 in_XMM1 [16];
  uint uVar20;
  int iVar21;
  double dVar22;
  int iVar26;
  undefined1 auVar23 [16];
  undefined1 auVar24 [16];
  undefined1 auVar25 [16];
  int iVar27;
  double dVar28;
  float fVar30;
  undefined1 auVar29 [16];
  double dVar31;
  undefined1 auVar32 [16];
  float fVar33;
  double dVar34;
  float fVar35;
  double dVar36;
  undefined1 auVar37 [16];
  double dVar38;
  double dVar39;
  int local_res8;
  int iStackX_c;
  undefined8 local_d8;
  undefined8 uStack_d0;
  undefined4 local_c8;
  longlong local_b8 [2];
  longlong local_a8;
  
  dVar3 = in_XMM1._0_8_;
  dVar22 = dVar3 - *(double *)(param_1 + 0x270);
  dVar9 = (double)Car__getWeight();
  dVar28 = DAT_1424025f0;
  dVar10 = *(double *)(param_1 + 0x270);
  if ((*(int *)(param_1 + 0x260) == 1) && (param_3 == 0.0)) {
    dVar31 = 0.0;
  }
  else if (((*(char *)(*(longlong *)(param_1 + 0x48) + 0xa28) == '\0') ||
           (cVar4 = (**(code **)(**(longlong **)(param_1 + 0x180) + 0x80))(), cVar4 != '\0')) ||
          ((*(int *)(param_1 + 0x264) != 0 && (*(int *)(param_1 + 0x264) != 2)))) {
    dVar31 = 0.0;
  }
  else {
    dVar31 = param_3 * param_3 * dVar28;
  }
  dVar9 = (dVar22 * dVar22 * dVar28 + dVar10 * dVar10 + dVar31) * dVar9;
  if (0.0 < dVar9) {
    auVar37 = *(undefined1 (*) [16])(param_1 + 0x58);
    dVar38 = (double)(int)(short)*(undefined4 *)(param_1 + 0x68) * DAT_1424020c8;
    dVar39 = (double)(int)(short)((uint)*(undefined4 *)(param_1 + 0x68) >> 0x10) * DAT_1424020c8;
    fVar7 = (float)(uint)(*(int *)(param_1 + 0xe0) - (int)(longlong)(param_3 * DAT_142403508)) *
            DAT_142401d10;
    dVar10 = (double)fVar7;
    dVar22 = dVar10 - DAT_1424024c0;
    dVar22 = dVar22 - (double)(int)dVar22;
    dVar10 = dVar10 - (double)(int)fVar7;
    auVar25._0_8_ =
         (double)CONCAT44((uint)((ulonglong)dVar22 >> 0x20) & _UNK_142405184,
                          SUB84(dVar22,0) & _DAT_142405180);
    auVar25._8_4_ = SUB84(dVar10,0) & _UNK_142405188;
    auVar25._12_4_ = (uint)((ulonglong)dVar10 >> 0x20) & _UNK_14240518c;
    dVar34 = _DAT_142404bf0 - auVar25._0_8_;
    dVar36 = _UNK_142404bf8 - auVar25._8_8_;
    auVar23._8_4_ = SUB84(dVar36,0);
    auVar23._0_8_ = dVar34;
    auVar23._12_4_ = (int)((ulonglong)dVar36 >> 0x20);
    dVar10 = dVar34 * dVar34;
    dVar22 = auVar23._8_8_ * dVar36;
    auVar29._8_4_ = SUB84(dVar22,0);
    auVar29._0_8_ = dVar10;
    auVar29._12_4_ = (int)((ulonglong)dVar22 >> 0x20);
    dVar28 = dVar10 * dVar10;
    dVar31 = auVar29._8_8_ * dVar22;
    dVar36 = ((_UNK_142404fa8 - _UNK_142404f98 * dVar22) * dVar31 +
              (_UNK_142404ef8 - _UNK_142404f78 * dVar22) + dVar31 * dVar31 * _UNK_142404f68) *
             dVar36;
    fVar33 = (float)(((_DAT_142404fa0 - _DAT_142404f90 * dVar10) * dVar28 +
                      (_DAT_142404ef0 - _DAT_142404f70 * dVar10) + dVar28 * dVar28 * _DAT_142404f60)
                    * dVar34) * _DAT_1424050d0;
    fVar35 = (float)(double)CONCAT44((uint)((ulonglong)dVar36 >> 0x20) ^ DAT_142405190._4_4_,
                                     SUB84(dVar36,0) ^ (uint)DAT_142405190) * _UNK_1424050d4;
    fVar7 = (float)(~DAT_1424051a0 & (uint)fVar33);
    fVar30 = (float)(~_UNK_1424051a4 & (uint)fVar35);
    auVar32._0_4_ = (uint)fVar7;
    auVar32._4_4_ = (uint)fVar30;
    auVar24._4_4_ = auVar32._0_4_;
    auVar24._0_4_ = auVar32._4_4_;
    auVar24._8_4_ = auVar32._4_4_;
    auVar24._12_4_ = auVar32._0_4_;
    uVar15 = (auVar32._0_4_ - (fVar7 < (float)(int)auVar32._0_4_)) +
             (uint)((float)(int)auVar32._0_4_ <= fVar7);
    uVar17 = (auVar32._4_4_ - (fVar30 < (float)(int)auVar32._4_4_)) +
             (uint)((float)(int)auVar32._4_4_ <= fVar30);
    auVar32._12_4_ = uVar17;
    auVar32._8_4_ = uVar15;
    auVar25 = pmaddwd(auVar24,auVar24);
    auVar11 = pmaddwd(auVar32,auVar32);
    uVar16 = (_DAT_142404e70 - auVar25._0_4_) - auVar11._0_4_;
    uVar18 = (_UNK_142404e74 - auVar25._4_4_) - auVar11._4_4_;
    uVar19 = (_UNK_142404e78 - auVar25._8_4_) - auVar11._8_4_;
    uVar20 = (_UNK_142404e7c - auVar25._12_4_) - auVar11._12_4_;
    uVar8 = (int)uVar16 >> 0x1f;
    uVar12 = (int)uVar18 >> 0x1f;
    uVar13 = (int)uVar19 >> 0x1f;
    uVar14 = (int)uVar20 >> 0x1f;
    iVar21 = (uVar8 ^ uVar16) - uVar8;
    iVar26 = (uVar13 ^ uVar19) - uVar13;
    iVar27 = (uVar14 ^ uVar20) - uVar14;
    uVar16 = -(uint)(iVar26 < iVar21 && fVar30 < fVar7);
    uVar18 = -(uint)(iVar27 < (int)((uVar12 ^ uVar18) - uVar12) && fVar7 < fVar30);
    uVar19 = -(uint)(iVar21 < iVar26 &&
                    fVar7 < (float)(~_UNK_1424051a8 & (uint)(_UNK_1424050d8 * 0.0)));
    uVar20 = -(uint)(iVar21 < iVar27 &&
                    fVar7 < (float)(~_UNK_1424051ac & (uint)(_UNK_1424050dc * 0.0)));
    uVar8 = (uint)(fVar33 < 0.0);
    uVar12 = (uint)(fVar35 < 0.0);
    uVar13 = (uint)(_UNK_1424050d8 * 0.0 < 0.0);
    uVar14 = (uint)(_UNK_1424050dc * 0.0 < 0.0);
    auVar11._0_4_ = ((uVar15 & uVar16 | ~uVar16 & auVar32._0_4_) ^ -uVar8) + uVar8;
    auVar11._4_4_ = ((uVar17 & uVar18 | ~uVar18 & auVar32._4_4_) ^ -uVar12) + uVar12;
    auVar11._8_4_ = ((auVar32._0_4_ & uVar19 | ~uVar19 & uVar15) ^ -uVar13) + uVar13;
    auVar11._12_4_ = ((auVar32._0_4_ & uVar20 | ~uVar20 & uVar17) ^ -uVar14) + uVar14;
    auVar11 = pshuflw(auVar11,auVar11,0xd8);
    FixedPointNumberTemplate<int,8,0>__FixedPointNumberTemplate<int,8,0>(&local_res8);
    FixedPointNumberTemplate<int,8,0>__FixedPointNumberTemplate<int,8,0>(&iStackX_c);
    local_d8 = (double)CONCAT44(auVar37._4_4_ + iStackX_c,auVar37._0_4_ + local_res8);
    uStack_d0 = (double)CONCAT44(auVar37._12_4_ + iStackX_c,auVar37._8_4_ + local_res8);
    auVar37._8_4_ = in_XMM1._8_4_;
    auVar37._0_8_ = dVar9;
    auVar37._12_4_ = in_XMM1._12_4_;
    local_c8 = auVar11._0_4_;
    cVar4 = (**(code **)(**(longlong **)(param_1 + 0x3a8) + 0x30))
                      (*(longlong **)(param_1 + 0x3a8),&local_d8);
    if (cVar4 == '\0') {
      puVar5 = (undefined8 *)
               (**(code **)(**(longlong **)(param_1 + 0x3a8) + 0x48))
                         (*(longlong **)(param_1 + 0x3a8),local_b8,&local_d8);
      puVar1 = (undefined8 *)puVar5[1];
      dVar10 = extraout_XMM0_Qa;
      dVar22 = dVar9;
      for (puVar5 = (undefined8 *)*puVar5; puVar5 != puVar1; puVar5 = puVar5 + 1) {
        plVar2 = (longlong *)*puVar5;
        cVar4 = (**(code **)(**(longlong **)(param_1 + 0x3a8) + 0x70))
                          (*(longlong **)(param_1 + 0x3a8),plVar2,
                           DAT_142f45838 + (ulonglong)*(ushort *)(param_1 + 0x6c) * 0x28);
        dVar10 = extraout_XMM0_Qa_00;
        if (cVar4 == '\0') {
          if ((*(char *)(*(longlong *)(param_1 + 0x48) + 0xa2d) != '\0') &&
             (lVar6 = (**(code **)(*plVar2 + 0xb58))(plVar2), lVar6 != 0)) {
            if (DAT_142402380 <
                (double)CONCAT44((uint)((ulonglong)*(undefined8 *)(param_1 + 0x270) >> 0x20) &
                                 _UNK_142405184,
                                 (uint)*(undefined8 *)(param_1 + 0x270) & _DAT_142405180)) {
              Car__tryToPlayImpactSound(param_1,plVar2);
            }
            if (local_b8[0] != 0) {
              std__allocator<double>__deallocate();
            }
            dVar22 = 0.0;
            goto LAB_1403a34a3;
          }
          dVar10 = (double)Car__handleCrashDamageAndTriggers(param_1);
          auVar37._8_4_ = (int)extraout_XMM0_Qb;
          auVar37._0_8_ = dVar10;
          auVar37._12_4_ = (int)((ulonglong)extraout_XMM0_Qb >> 0x20);
          if (dVar10 <= 0.0) {
            dVar22 = 0.0;
            break;
          }
        }
        dVar22 = auVar37._0_8_;
      }
      if (local_b8[0] != 0) {
        std__allocator<double>__deallocate(dVar10,local_b8[0],local_a8 - local_b8[0] >> 3);
      }
    }
    else {
      dVar22 = 0.0;
    }
LAB_1403a34a3:
    dVar10 = dVar22 / dVar9;
    if (dVar10 < 0.0) {
      dVar10 = sqrt(dVar10);
    }
    else {
      dVar10 = SQRT(dVar10);
    }
    *(double *)(param_1 + 0x270) = dVar10 * dVar3;
    if (dVar9 - dVar22 == 0.0) {
      local_d8 = dVar38 * dVar3;
      uStack_d0 = dVar39 * dVar3;
      Car__handleProgression(param_1,&local_d8);
    }
  }
  return;
}

