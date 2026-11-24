// Character::calculateMovementSpeed @ 1403d3eb0 (function Character::calculateMovementSpeed)

double Character__calculateMovementSpeed(longlong param_1,longlong *param_2,char param_3)

{
  int iVar1;
  int iVar2;
  double dVar3;
  double dVar4;
  char cVar5;
  longlong lVar6;
  int *piVar7;
  undefined8 uVar8;
  ushort *puVar9;
  longlong *plVar10;
  float fVar11;
  double dVar12;
  double dVar13;
  undefined1 local_res8 [8];
  
  dVar4 = DAT_1424027a8;
  dVar3 = (*(double *)
            (*(longlong *)
              (*(longlong *)(*(longlong *)(param_1 + 0x78) + 1000) +
              (ulonglong)*(byte *)(param_1 + 0x90) * 8) + 0x3d8) + DAT_1424027a8) *
          *(double *)(*(longlong *)(param_1 + 0x48) + 0x768) *
          (*(double *)(param_1 + 0x200) + DAT_1424027a8);
  lVar6 = (**(code **)(*param_2 + 0x4b8))(param_2);
  dVar13 = 0.0;
  if (lVar6 != 0) {
    if (param_3 == '\0') {
      if (*(char *)(lVar6 + 0x5c) == '\0') {
        dVar12 = (double)EquipmentGrid__getRawCalculatedMovementBonus(lVar6);
      }
      else {
        dVar12 = 0.0;
      }
    }
    else {
      dVar12 = (double)EquipmentGrid__calculateMovementBonusAndExtractEnergy(lVar6,param_1);
    }
    dVar3 = dVar3 * (dVar12 + dVar4);
  }
  if (*(float *)(*(longlong *)(param_1 + 0x518) + 0x24) != 0.0) {
    dVar13 = (double)*(float *)(*(longlong *)(param_1 + 0x518) + 0x20);
  }
  dVar3 = dVar3 * (dVar4 - dVar13);
  if (*(char *)(param_1 + 0x178) == '\0') {
    lVar6 = *param_2;
    piVar7 = (int *)(**(code **)(lVar6 + 0x1b0))(param_2,local_res8);
    iVar1 = *piVar7;
    iVar2 = piVar7[1];
    uVar8 = (**(code **)(*param_2 + 0x1c8))(param_2);
    puVar9 = (ushort *)(**(code **)(lVar6 + 0x498))(param_2,uVar8,CONCAT44(iVar2 >> 8,iVar1 >> 8));
    if ((puVar9 != (ushort *)0x0) &&
       ((dVar13 = *(double *)(*(longlong *)(DAT_1429c51f0 + (ulonglong)*puVar9 * 8) + 0x1b08),
        dVar4 <= dVar13 ||
        ((dVar13 < dVar4 &&
         ((param_3 == '\0' || (cVar5 = Character__startFlying(param_1,param_2), cVar5 == '\0')))))))
       ) {
      dVar3 = dVar3 * dVar13;
    }
  }
  plVar10 = (longlong *)(**(code **)(*param_2 + 8))(param_2,local_res8);
  fVar11 = (float)StickerInterface__getMovementModifierAfterNTicks
                            (param_1 + 0x120,
                             *plVar10 - *(longlong *)(*(longlong *)(param_1 + 0x78) + 0x1b0));
  dVar3 = dVar3 * (double)fVar11;
  fVar11 = (float)StickerInterface__getMovementMaxAfterNTicks(param_1 + 0x120);
  if (fVar11 < 0.0) {
    return dVar3;
  }
  dVar13 = (double)fVar11;
  if (dVar3 <= (double)fVar11) {
    dVar13 = dVar3;
  }
  return dVar13;
}

