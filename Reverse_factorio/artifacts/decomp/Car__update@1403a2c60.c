// Car::update @ 1403a2c60 (function Car::update)

void Car__update(longlong param_1)

{
  float fVar1;
  float fVar2;
  short sVar3;
  longlong *plVar4;
  code *pcVar5;
  ulonglong uVar6;
  char cVar7;
  short sVar8;
  undefined4 *puVar9;
  uint uVar10;
  longlong lVar11;
  undefined4 uVar12;
  ulonglong uVar13;
  longlong lVar14;
  float fVar15;
  undefined8 uVar16;
  undefined4 local_res8;
  double local_28;
  double local_20;
  
  if (*(longlong *)(param_1 + 0x1f8) != *(longlong *)(*(longlong *)(param_1 + -0x30) + 0x1b0)) {
    if (((*(longlong *)(param_1 + 0x30) == 0) ||
        (cVar7 = EquipmentGrid__hasBeltImmunity(), cVar7 == '\0')) &&
       (*(char *)(*(longlong *)(param_1 + -0x60) + 0xa2a) == '\0')) {
      (**(code **)(**(longlong **)(param_1 + 0x300) + 0x58))();
    }
    if (((*(double *)(param_1 + 0x1c8) != 0.0) || (*(int *)(param_1 + 0x1bc) != 0)) ||
       (*(int *)(param_1 + 0x1b8) != 1)) {
      lVar14 = param_1 + -0xa8;
      (**(code **)(**(longlong **)(param_1 + 0x300) + 0xd0))(*(longlong **)(param_1 + 0x300),0);
      if (((int)((ulonglong)*(undefined8 *)(param_1 + 0x1b8) >> 0x20) == 0) &&
         ((*(char *)(*(longlong *)(param_1 + -0x60) + 0xa28) == '\0' ||
          ((int)*(undefined8 *)(param_1 + 0x1b8) == 1)))) {
        uVar16 = 0;
      }
      else {
        uVar16 = 1;
      }
      plVar4 = *(longlong **)(param_1 + 0xd8);
      pcVar5 = *(code **)(*plVar4 + 0x68);
      puVar9 = (undefined4 *)
               (**(code **)(**(longlong **)(param_1 + 0x300) + 0xa8))
                         (*(longlong **)(param_1 + 0x300),&local_res8);
      (*pcVar5)(plVar4,*(undefined8 *)(param_1 + -0x30),lVar14,uVar16,*puVar9);
      uVar16 = Car__calculateEnergyWithEngineContribution(lVar14);
      Car__applyUpdate(lVar14,uVar16);
    }
    if (*(longlong *)(param_1 + 0x170) != 0) {
      lVar14 = *(longlong *)(param_1 + -0x60);
      lVar11 = *(longlong *)(lVar14 + 0xa70);
      local_28 = (double)(int)*(short *)(param_1 + -0x40) * DAT_1424020c8 *
                 *(double *)(param_1 + 0x1c8);
      local_20 = (double)(int)*(short *)(param_1 + -0x3e) * DAT_1424020c8 *
                 *(double *)(param_1 + 0x1c8);
      if ((ulonglong)*(ushort *)(param_1 + 0x1e8) <
          (ulonglong)(*(longlong *)(lVar14 + 0xa78) - lVar11 >> 4)) {
        local_res8 = CONCAT22(CONCAT11(local_res8._3_1_,*(undefined1 *)(param_1 + -0x17)),
                              *(undefined2 *)
                               (*(longlong *)
                                 (lVar11 + (ulonglong)*(ushort *)(param_1 + 0x1e8) * 0x10) + 0x1c8))
        ;
        uVar12 = local_res8;
      }
      else {
        uVar12 = 0;
      }
      Shooter__update(*(longlong *)(param_1 + 0x170),0,uVar12,*(undefined8 *)(param_1 + -0x38),
                      *(undefined1 *)(param_1 + -0x18),&local_28,0,param_1 + -0x58);
    }
    if ((*(longlong *)(param_1 + 0x210) == 0) && (*(int *)(param_1 + 0x268) == 0x7fffffff)) {
      lVar14 = *(longlong *)(param_1 + -0x60);
      uVar13 = (ulonglong)*(uint *)(lVar14 + 0xa34) + *(longlong *)(param_1 + 0x1f0);
      uVar6 = *(ulonglong *)(*(longlong *)(param_1 + -0x30) + 0x1b0);
      if (((uVar6 != uVar13) && (uVar13 <= uVar6)) &&
         (fVar1 = *(float *)(param_1 + 0x200), fVar1 != 0.0)) {
        fVar2 = *(float *)(lVar14 + 0xa30);
        uVar10 = -(int)(longlong)(fVar1 * DAT_14240325c);
        fVar15 = (float)uVar10 * DAT_142401d10;
        if ((uVar10 != 0x80000000) && (0x7fffffff < uVar10)) {
          fVar15 = fVar15 - DAT_1424022c0;
        }
        if (fVar15 <= fVar2) {
          if ((float)((uint)fVar15 ^ DAT_1424051a0) <= fVar2) {
            *(undefined4 *)(param_1 + 0x200) = 0;
          }
          else {
            *(float *)(param_1 + 0x200) = fVar1 - fVar2;
          }
        }
        else {
          *(float *)(param_1 + 0x200) = fVar2 + fVar1;
        }
      }
    }
    else {
      if ((((*(char *)(param_1 + 0x178) != '\0') ||
           (lVar14 = *(longlong *)(param_1 + 0x90), lVar14 == 0)) &&
          (lVar14 = *(longlong *)(param_1 + 0x80), *(char *)(param_1 + 0x178) != '\0')) ||
         (lVar11 = *(longlong *)(param_1 + 0x88), lVar11 == 0)) {
        lVar11 = *(longlong *)(param_1 + 0x78);
      }
      Car__shoot(param_1 + -0xa8,lVar11,lVar14);
    }
    Vehicle__update(param_1);
    if (*(char *)(param_1 + 0x179) != '\0') {
      if (*(char *)(*(longlong *)(param_1 + -0x60) + 0xa29) != '\0') {
        (**(code **)(*(longlong *)(param_1 + 0xe0) + 0x188))();
      }
      *(undefined1 *)(param_1 + 0x179) = 0;
    }
    lVar14 = *(longlong *)(param_1 + 0x30);
    if (lVar14 != 0) {
      sVar8 = EquipmentGrid__calculateInventorySizeBonus(lVar14);
      sVar3 = *(short *)(lVar14 + 0x5a);
      *(short *)(lVar14 + 0x5a) = sVar8;
      if (sVar8 != sVar3) {
        Car__updateTrunkInventorySize(param_1 + -0xa8);
      }
    }
  }
  return;
}

