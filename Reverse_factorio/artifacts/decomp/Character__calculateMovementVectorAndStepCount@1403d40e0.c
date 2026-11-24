// Character::calculateMovementVectorAndStepCount @ 1403d40e0 (function Character::calculateMovementVectorAndStepCount)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

double * Character__calculateMovementVectorAndStepCount
                   (longlong param_1,double *param_2,longlong *param_3,undefined1 param_4)

{
  int iVar1;
  longlong lVar2;
  int *piVar3;
  ulonglong uVar4;
  char cVar5;
  ulonglong uVar6;
  int iVar7;
  double dVar8;
  double dVar9;
  double dVar10;
  undefined1 auVar11 [16];
  undefined1 local_res8 [8];
  
  lVar2 = (**(code **)(*param_3 + 0x338))(param_3);
  dVar9 = *(double *)(lVar2 + 0x20);
  dVar10 = *(double *)(lVar2 + 0x28);
  dVar8 = (double)Character__calculateMovementSpeed(param_1,param_3,param_4);
  dVar9 = dVar10 * dVar10 + dVar9 * dVar9;
  if (dVar9 < 0.0) {
    dVar9 = sqrt(dVar9);
  }
  else {
    auVar11._8_8_ = 0;
    auVar11._0_8_ = dVar9;
    auVar11 = sqrtpd(ZEXT816(0),auVar11);
    dVar9 = auVar11._0_8_;
  }
  iVar1 = *(int *)(param_1 + 100) - *(int *)(param_1 + 0x5c);
  iVar7 = *(int *)(param_1 + 0x60) - *(int *)(param_1 + 0x58);
  if (iVar7 == iVar1) {
    cVar5 = '\0';
  }
  else {
    cVar5 = -1;
    if (iVar1 <= iVar7) {
      cVar5 = '\x01';
    }
  }
  if (cVar5 < '\0') {
    iVar1 = iVar7;
  }
  if (iVar1 == 0x10) {
    cVar5 = '\0';
  }
  else {
    cVar5 = -1;
    if (iVar1 < 0x11) {
      cVar5 = '\x01';
    }
  }
  if ('\0' < cVar5) {
    iVar1 = 0x10;
  }
  dVar10 = (dVar8 * dVar9) / ((double)iVar1 * DAT_142402278);
  dVar10 = ceil((double)CONCAT44((uint)((ulonglong)dVar10 >> 0x20) & _UNK_142405184,
                                 SUB84(dVar10,0) & _DAT_142405180));
  if (!NAN(dVar10) && !NAN(dVar10)) {
    if (DAT_142403038 <= dVar10) {
      uVar6 = 0xffffffff;
      goto LAB_1403d41f7;
    }
    if (0.0 < dVar10) {
      uVar6 = (ulonglong)dVar10;
      goto LAB_1403d41f7;
    }
  }
  uVar6 = 0;
LAB_1403d41f7:
  lVar2 = (**(code **)(*param_3 + 0x338))(param_3);
  piVar3 = (int *)Vector__getOrientation(lVar2 + 0x20,local_res8);
  uVar4 = (ulonglong)(*piVar3 + 0x10000000U >> 0x1c & 0xe);
  dVar9 = (dVar8 * dVar9) / (double)(uVar6 & 0xffffffff);
  dVar10 = (double)(&DAT_142007e78)[uVar4 * 2];
  *param_2 = (double)(&PTR_142007e70)[uVar4 * 2] * dVar9;
  param_2[1] = dVar10 * dVar9;
  *(int *)(param_2 + 2) = (int)uVar6;
  return param_2;
}

