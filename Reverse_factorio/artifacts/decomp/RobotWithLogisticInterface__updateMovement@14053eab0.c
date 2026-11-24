// RobotWithLogisticInterface::updateMovement @ 14053eab0 (function RobotWithLogisticInterface::updateMovement)

void RobotWithLogisticInterface__updateMovement(longlong *param_1)

{
  longlong *plVar1;
  char cVar2;
  uint uVar3;
  longlong lVar4;
  byte bVar5;
  ulonglong uVar6;
  ulonglong uVar7;
  double dVar8;
  
  plVar1 = param_1 + 0x36;
  uVar7 = *(longlong *)(param_1[0xf] + 0x1b0) - param_1[0x35];
  param_1[0x35] = *(longlong *)(param_1[0xf] + 0x1b0);
  if ((int)*plVar1 != 0x7fffffff) {
    dVar8 = (double)RobotWithLogisticInterface__getSpeed();
    FlyingRobot__moveToAndUseEnergy(param_1,*plVar1,dVar8 * (double)uVar7);
    if ((int)*plVar1 == 0x7fffffff) {
                    /* WARNING: Subroutine does not return */
      ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/Container/Optional.hpp"
                          ,0x3f,"*this");
    }
    if (((int)param_1[10] == (int)*plVar1) &&
       (*(int *)((longlong)param_1 + 0x54) == *(int *)((longlong)param_1 + 0x1b4))) {
      *plVar1 = (longlong)&DAT_7fffffff7fffffff;
    }
    else {
      lVar4 = *(longlong *)(param_1[0xf] + 0x1b0);
      uVar3 = RobotMoveIntention__calculateRemainingTicks(plVar1,param_1);
      param_1[0x1d] = (ulonglong)uVar3 + lVar4;
    }
  }
  if (*(char *)((longlong)param_1 + 0x18b) == '\x01') {
    if (((((longlong *)param_1[0x38] != (longlong *)0x0) &&
         (lVar4 = (**(code **)(*(longlong *)param_1[0x38] + 0x800))(), lVar4 != 0)) &&
        ((*(byte *)(lVar4 + 0xc0) & 2) != 0)) &&
       (cVar2 = RobotWithLogisticInterface__needsToMoveTowardRoboport(param_1), cVar2 != '\0')) {
      Entity__changePositionWithoutCollisionOrAABBCheck
                (param_1,CONCAT44(*(int *)(*(longlong *)(lVar4 + 0x68) + 0x54) -
                                  (int)(*(double *)(*(longlong *)(lVar4 + 0x70) + 0x78) *
                                       DAT_1424034a8),
                                  *(int *)(*(longlong *)(lVar4 + 0x68) + 0x50) -
                                  (int)(*(double *)(*(longlong *)(lVar4 + 0x70) + 0x70) *
                                       DAT_1424034a8)),param_1 + 0x15);
    }
    uVar6 = *(longlong *)(param_1[0xf] + 0x1b0) - param_1[0x1d];
    if (uVar6 != 0x11) {
      bVar5 = 0x11;
      if (uVar6 < 0x11) {
        bVar5 = (byte)uVar6;
      }
      if (bVar5 < 0x11) goto LAB_14053ecd8;
    }
    if (*(char *)((longlong)param_1 + 0x18a) == '\0') {
      PortTargeter__set(param_1 + 0x37,0);
    }
    *(undefined4 *)((longlong)param_1 + 0xdc) = 0x3f800000;
    uVar6 = *(ulonglong *)(param_1[0xf] + 0x1b0);
    *(undefined1 *)((longlong)param_1 + 0xd9) = 0xff;
    *(char *)(param_1 + 0x1b) = ((char)(uVar6 / 0x5a) + '\x01') * 'Z' - (char)uVar6;
    if (param_1[0x3d] == 0) {
      *(undefined1 *)((longlong)param_1 + 0x18b) = 2;
    }
    else {
      *(undefined1 *)((longlong)param_1 + 0x18b) = 3;
      PortTargeter__set(param_1 + 0x37);
      PortTargeter__set(param_1 + 0x3c,0);
    }
  }
LAB_14053ecd8:
  cVar2 = FlyingRobot__useEnergyPerTick(param_1,uVar7);
  if (cVar2 == '\0') {
    (**(code **)(*param_1 + 0x1260))();
  }
  return;
}

