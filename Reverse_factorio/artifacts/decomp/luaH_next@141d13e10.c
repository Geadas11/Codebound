// luaH_next @ 141d13e10 (function luaH_next)

undefined8 luaH_next(undefined8 param_1,longlong param_2,double *param_3)

{
  int iVar1;
  double *pdVar2;
  undefined4 uVar3;
  longlong lVar4;
  int *piVar5;
  int iVar6;
  
  if (*(int *)(param_3 + 1) == 0) {
    iVar6 = -1;
  }
  else if ((((*(int *)(param_3 + 1) == 3) && (iVar6 = (int)*param_3, (double)iVar6 == *param_3)) &&
           (0 < iVar6)) && (iVar6 <= *(int *)(param_2 + 0x40))) {
    iVar6 = iVar6 + -1;
  }
  else {
    lVar4 = mainposition(param_2,param_3);
    while (((*(int *)(lVar4 + 0x18) != *(int *)(param_3 + 1) ||
            (iVar6 = luaV_equalobj_(0,lVar4 + 0x10,param_3), iVar6 == 0)) &&
           ((*(int *)(lVar4 + 0x18) != 0xb ||
            ((((ulonglong)param_3[1] & 0x40) == 0 || (*(double *)(lVar4 + 0x10) != *param_3))))))) {
      lVar4 = *(longlong *)(lVar4 + 0x20);
      if (lVar4 == 0) {
                    /* WARNING: Subroutine does not return */
        luaG_runerror(param_1,"invalid key to \'next\'");
      }
    }
    iVar6 = (int)((lVar4 - *(longlong *)(param_2 + 0x28)) / 0x38) + *(int *)(param_2 + 0x40);
  }
  iVar1 = iVar6 + 1;
  if (iVar1 < *(int *)(param_2 + 0x40)) {
    lVar4 = (longlong)iVar1;
    piVar5 = (int *)(*(longlong *)(param_2 + 0x20) + 8 + lVar4 * 0x10);
    do {
      if (*piVar5 != 0) {
        *(undefined4 *)(param_3 + 1) = 3;
        *param_3 = (double)(iVar1 + 1);
        lVar4 = *(longlong *)(param_2 + 0x20);
        param_3[2] = *(double *)(lVar4 + (longlong)iVar1 * 0x10);
        uVar3 = *(undefined4 *)(lVar4 + 8 + (longlong)iVar1 * 0x10);
        goto LAB_141d13f6f;
      }
      lVar4 = lVar4 + 1;
      piVar5 = piVar5 + 4;
      iVar1 = iVar1 + 1;
    } while (lVar4 < *(int *)(param_2 + 0x40));
  }
  if (iVar6 < *(int *)(param_2 + 0x40)) {
    pdVar2 = *(double **)(param_2 + 0x48);
  }
  else {
    pdVar2 = *(double **)
              ((longlong)(iVar6 - *(int *)(param_2 + 0x40)) * 0x38 + 0x28 +
              *(longlong *)(param_2 + 0x28));
  }
  while( true ) {
    if (pdVar2 == (double *)0x0) {
      return 0;
    }
    if (*(int *)(pdVar2 + 1) != 0) break;
    pdVar2 = (double *)pdVar2[5];
  }
  *param_3 = pdVar2[2];
  *(undefined4 *)(param_3 + 1) = *(undefined4 *)(pdVar2 + 3);
  param_3[2] = *pdVar2;
  uVar3 = *(undefined4 *)(pdVar2 + 1);
LAB_141d13f6f:
  *(undefined4 *)(param_3 + 3) = uVar3;
  return 1;
}

