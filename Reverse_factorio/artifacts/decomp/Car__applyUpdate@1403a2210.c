// Car::applyUpdate @ 1403a2210 (function Car::applyUpdate)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void Car__applyUpdate(longlong param_1,double param_2)

{
  longlong lVar1;
  uint uVar2;
  double dVar3;
  double dVar4;
  float fVar5;
  float fVar6;
  float fVar7;
  
  dVar3 = (double)Car__getWeight();
  dVar3 = param_2 / (dVar3 * _DAT_142402e28);
  dVar3 = (double)CONCAT44((uint)((ulonglong)dVar3 >> 0x20) & _UNK_142405184,
                           SUB84(dVar3,0) & _DAT_142405180);
  if (dVar3 < 0.0) {
    dVar3 = sqrt(dVar3);
  }
  else {
    dVar3 = SQRT(dVar3);
  }
  dVar4 = (double)Car__getDesiredOrientationChange(param_1);
  if (dVar4 == 0.0) {
    uVar2 = -(int)(longlong)
                  ((double)(*(int *)(param_1 + 0xe0) + 0x10000000U >> 0x1c & 0xe) * DAT_1424023a0 *
                  DAT_142403508) - *(int *)(param_1 + 0xe0);
    fVar5 = (float)uVar2 * DAT_142401d10;
    if ((uVar2 != 0x80000000) && (0x7fffffff < uVar2)) {
      fVar5 = fVar5 - DAT_1424022c0;
    }
    if ((fVar5 != 0.0) &&
       ((double)(float)((uint)fVar5 & DAT_142405170) <
        *(double *)(*(longlong *)(param_1 + 0x48) + 0xa20))) {
      dVar4 = (double)fVar5;
    }
  }
  if ((DAT_1424022c8 < dVar3) ||
     ((*(char *)(*(longlong *)(param_1 + 0x48) + 0xa28) != '\0' &&
      (DAT_142402020 <
       (double)CONCAT44((uint)((ulonglong)dVar4 >> 0x20) & _UNK_142405184,
                        SUB84(dVar4,0) & _DAT_142405180))))) {
    Car__moveCar(param_1);
    lVar1 = *(longlong *)(param_1 + 0x48);
    if (*(int *)(lVar1 + 0x928) != 0) {
      lVar1 = *(longlong *)(lVar1 + 0x910);
      if (param_2 < 0.0) {
        fVar5 = (float)dVar3 * *(float *)(lVar1 + 0x1c);
        if (*(float *)(lVar1 + 0x18) <= fVar5) {
          fVar5 = *(float *)(lVar1 + 0x18);
        }
        fVar5 = *(float *)(param_1 + 0x330) - fVar5;
        if (fVar5 < 0.0) {
          fVar7 = (float)*(ushort *)(lVar1 + 0x24);
          fVar6 = fVar5 / fVar7;
          if (NAN(fVar6)) {
            lVar1 = 0;
          }
          else if (fVar6 < DAT_142403260) {
            lVar1 = -0x8000000000000000;
            if (DAT_142403574 < fVar6) {
              lVar1 = (longlong)fVar6;
            }
          }
          else {
            lVar1 = 0x7fffffffffffffff;
          }
          fVar5 = fVar5 - (float)(lVar1 + -1) * fVar7;
          if (fVar5 == fVar7) {
            fVar5 = 0.0;
          }
        }
        *(float *)(param_1 + 0x330) = fVar5;
      }
      else {
        CyclicFramePosition__cyclicMove();
      }
    }
    if (*(int *)(*(longlong *)(param_1 + 0x48) + 0x978) != 0) {
      CyclicFramePosition__cyclicMove(param_1 + 0x334,(float)dVar3);
    }
  }
  else {
    *(undefined8 *)(param_1 + 0x270) = 0;
  }
  return;
}

