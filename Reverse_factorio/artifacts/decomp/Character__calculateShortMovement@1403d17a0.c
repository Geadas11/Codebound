// Character::calculateShortMovement @ 1403d17a0 (function Character::calculateShortMovement)

undefined1 *
Character__calculateShortMovement
          (longlong param_1,undefined1 *param_2,undefined8 *param_3,double *param_4,
          undefined8 param_5)

{
  double dVar1;
  double dVar2;
  longlong *plVar3;
  char cVar4;
  int iVar5;
  longlong lVar6;
  int iVar7;
  int iVar8;
  undefined1 auVar9 [16];
  undefined8 local_res20;
  int local_48;
  int local_44;
  int local_40;
  int local_3c;
  undefined4 local_38;
  
  dVar1 = *param_4;
  dVar2 = param_4[1];
  if (dVar1 == 0.0) {
    if (dVar2 == 0.0) {
LAB_1403d17d0:
      *(undefined8 *)(param_2 + 4) = *param_3;
      *param_2 = 0;
      return param_2;
    }
  }
  else if (dVar2 != 0.0) goto LAB_1403d17d0;
  if (dVar1 == 0.0) {
    iVar7 = 8;
    iVar5 = -1;
    if (0.0 < dVar2) {
      iVar5 = 1;
    }
    do {
      iVar8 = (int)((double)iVar7 * (double)iVar5) + (int)((ulonglong)*param_3 >> 0x20);
      plVar3 = *(longlong **)(*(longlong *)(param_1 + 0x398) + 8);
      local_res20._0_4_ = (int)*param_3;
      local_res20 = CONCAT44(iVar8,(int)local_res20);
      if (((plVar3 == (longlong *)0x0) || (lVar6 = (**(code **)(*plVar3 + 0x178))(), lVar6 == 0)) ||
         (lVar6 = *(longlong *)(lVar6 + 0x30), *(char *)(lVar6 + 0x5b8) == '\0')) {
        auVar9 = *(undefined1 (*) [16])(*(longlong *)(param_1 + 0x48) + 0x278);
        local_38 = *(undefined4 *)(*(longlong *)(param_1 + 0x48) + 0x288);
      }
      else {
        auVar9 = *(undefined1 (*) [16])(lVar6 + 0x5a4);
        local_38 = *(undefined4 *)(lVar6 + 0x5b4);
      }
      local_48 = auVar9._0_4_ + (int)local_res20;
      local_44 = auVar9._4_4_ + iVar8;
      local_40 = auVar9._8_4_ + (int)local_res20;
      local_3c = auVar9._12_4_ + iVar8;
      cVar4 = Character__checkCollisionDependingOnLatencyHidingMode
                        (param_1,&local_48,
                         DAT_142f45838 + (ulonglong)*(ushort *)(param_1 + 0x6c) * 0x28,param_5);
      if (cVar4 == '\0') goto LAB_1403d1a95;
      iVar7 = iVar7 + -1;
    } while (0 < iVar7);
  }
  else {
    if (dVar2 != 0.0) {
                    /* WARNING: Subroutine does not return */
      Logging__logAndAbortOrThrow
                ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\Character.cpp"
                 ,0xd10,9,&DAT_1422080b0);
    }
    iVar7 = 8;
    iVar5 = -1;
    if (0.0 < dVar1) {
      iVar5 = 1;
    }
    do {
      iVar8 = (int)((double)iVar7 * (double)iVar5) + (int)*param_3;
      plVar3 = *(longlong **)(*(longlong *)(param_1 + 0x398) + 8);
      local_res20._4_4_ = (int)((ulonglong)*param_3 >> 0x20);
      local_res20 = CONCAT44(local_res20._4_4_,iVar8);
      if (((plVar3 == (longlong *)0x0) || (lVar6 = (**(code **)(*plVar3 + 0x178))(), lVar6 == 0)) ||
         (lVar6 = *(longlong *)(lVar6 + 0x30), *(char *)(lVar6 + 0x5b8) == '\0')) {
        auVar9 = *(undefined1 (*) [16])(*(longlong *)(param_1 + 0x48) + 0x278);
        local_38 = *(undefined4 *)(*(longlong *)(param_1 + 0x48) + 0x288);
      }
      else {
        auVar9 = *(undefined1 (*) [16])(lVar6 + 0x5a4);
        local_38 = *(undefined4 *)(lVar6 + 0x5b4);
      }
      local_48 = auVar9._0_4_ + iVar8;
      local_44 = auVar9._4_4_ + local_res20._4_4_;
      local_40 = auVar9._8_4_ + iVar8;
      local_3c = auVar9._12_4_ + local_res20._4_4_;
      cVar4 = Character__checkCollisionDependingOnLatencyHidingMode
                        (param_1,&local_48,
                         DAT_142f45838 + (ulonglong)*(ushort *)(param_1 + 0x6c) * 0x28,param_5);
      if (cVar4 == '\0') goto LAB_1403d1a95;
      iVar7 = iVar7 + -1;
    } while (0 < iVar7);
  }
  local_res20 = *param_3;
  *param_2 = 0;
  goto LAB_1403d193e;
LAB_1403d1a95:
  *param_2 = 1;
LAB_1403d193e:
  *(undefined8 *)(param_2 + 4) = local_res20;
  return param_2;
}

