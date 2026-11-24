// FlyingRobot::moveToAndUseEnergy @ 140464220 (function FlyingRobot::moveToAndUseEnergy)

ulonglong FlyingRobot__moveToAndUseEnergy(longlong param_1,undefined8 param_2,double param_3)

{
  double dVar1;
  ulonglong in_RAX;
  longlong lVar2;
  double dVar3;
  double dVar4;
  double local_38;
  byte local_30;
  
  lVar2 = *(longlong *)(param_1 + 0xd0);
  if (*(double *)(lVar2 + 0x728) != 0.0) {
    dVar4 = *(double *)(param_1 + 0xe0);
    if (0.0 < dVar4) {
      dVar1 = *(double *)(lVar2 + 0x728) * *(double *)(*(longlong *)(param_1 + 0x70) + 0x2ff0);
      if (dVar4 < dVar1 * param_3) {
        dVar3 = dVar4 / dVar1;
        if (param_3 <= dVar4 / dVar1) {
          dVar3 = param_3;
        }
        param_3 = (param_3 - dVar3) * (double)*(float *)(lVar2 + 0x740) + dVar3;
      }
      FlyingRobot__moveTo(param_1,&local_38,param_2,SUB84(param_3,0));
      dVar4 = *(double *)(param_1 + 0xe0) - local_38 * dVar1;
      *(double *)(param_1 + 0xe0) = dVar4;
      if (dVar4 < 0.0) {
        *(undefined8 *)(param_1 + 0xe0) = 0;
      }
      return (ulonglong)local_30;
    }
    if (*(float *)(lVar2 + 0x740) <= 0.0) {
      return in_RAX & 0xffffffffffffff00;
    }
  }
  lVar2 = FlyingRobot__moveTo(param_1,&local_38,param_2,SUB84(param_3,0));
  return (ulonglong)*(byte *)(lVar2 + 8);
}

