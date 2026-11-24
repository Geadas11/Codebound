// CombatRobot::save @ 1403e49e0 (function CombatRobot::save)

void CombatRobot__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  FlyingRobot__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x108,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  Vector2<double,Vector>__save(param_1 + 0x110,param_2);
  TargeterBase__save(param_1 + 0x120,param_2);
  Shooter__save(param_1 + 0x140,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"owner",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"owner",5);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 5;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  TargeterBase__save(param_1 + 0x1b0,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"owner",5);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 5;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

