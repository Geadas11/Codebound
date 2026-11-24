// RobotWithLogisticInterface::save @ 14053e3d0 (function RobotWithLogisticInterface::save)

void RobotWithLogisticInterface__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  FlyingRobot__save();
  (**(code **)(*(longlong *)(param_1 + 0x160) + 0xd8))((longlong *)(param_1 + 0x160),param_2);
  local_res8[0] = *(undefined1 *)(param_1 + 0x18a);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8[0] = *(undefined1 *)(param_1 + 0x18b);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x1b8,param_2);
  TargeterBase__save(param_1 + 0x1e0,param_2);
  TargeterBase__save(param_1 + 0x208,param_2);
  MapPosition__save(param_1 + 0x228,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x230,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*(char *)(param_2 + 9) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x1a0,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1a8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  MapPosition__save(param_1 + 0x1b0,param_2);
  return;
}

