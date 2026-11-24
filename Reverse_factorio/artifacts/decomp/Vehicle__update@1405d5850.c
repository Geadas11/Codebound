// Vehicle::update @ 1405d5850 (function Vehicle::update)

void Vehicle__update(longlong param_1)

{
  longlong *plVar1;
  undefined8 uVar2;
  longlong lVar3;
  undefined4 uVar4;
  
  plVar1 = (longlong *)(param_1 + -0xa8);
  if (((*(byte *)(param_1 + -0x3a) & 0x10) == 0) && ((*(byte *)(param_1 + 0x20) & 0x3e) == 0)) {
    VehiclePassenger__setPositionWithoutMapRegistration
              (param_1 + 0x78,param_1 + -0x58,*(undefined8 *)(param_1 + -0x38));
    VehiclePassenger__setPositionWithoutMapRegistration
              (param_1 + 0x88,param_1 + -0x58,*(undefined8 *)(param_1 + -0x38));
    if (((*(ushort *)(param_1 + -0x3a) & 0x20) == 0) &&
       ((*(ushort *)(param_1 + -0x3a) >> 0xe & 1) == 0)) {
      uVar2 = (**(code **)(*plVar1 + 0x710))(plVar1);
      PortableRoboportProvider__update(param_1 + 0x48,uVar2);
    }
    if ((*(byte *)(param_1 + -0x3a) & 0x10) == 0) {
      Vehicle__scanAndChart(plVar1);
      if ((*(byte *)(param_1 + -0x3a) & 0x20) == 0) {
        lVar3 = (**(code **)(*plVar1 + 0xfd8))(plVar1);
        if (lVar3 == 0) {
          uVar4 = (**(code **)(*plVar1 + 0x1268))(plVar1);
          Vehicle__checkApplySpeedChangedTriggers(plVar1,uVar4);
        }
      }
    }
  }
  return;
}

