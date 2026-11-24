// Car::save @ 1403a5ca0 (function Car::save)

void Car__save(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  undefined1 local_res8 [8];
  
  Vehicle__save();
  (**(code **)(**(longlong **)(param_1 + 0x180) + 0x50))(*(longlong **)(param_1 + 0x180),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x188) + 0xd8))((longlong *)(param_1 + 0x188),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x1c0) + 0xd8))((longlong *)(param_1 + 0x1c0),param_2);
  WakeUpList__save(param_1 + 0x1f8,param_2);
  MapSerialiser__operator<<<RidingState>(param_2,param_1 + 0x260);
  plVar1 = *(longlong **)(param_2 + 0x50);
  if (*(char *)(param_1 + 0x26c) == '\0') {
    local_res8[0] = 0;
    (**(code **)(*plVar1 + 8))(plVar1,local_res8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  else {
    local_res8[0] = 1;
    (**(code **)(*plVar1 + 8))();
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x268,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x270,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x278,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x280,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x288,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  local_res8[0] = *(longlong *)(param_1 + 0x218) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x218) != 0) {
    Shooter__save(*(longlong *)(param_1 + 0x218),param_2);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x220,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x290,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x221,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(*(longlong *)(param_1 + 0x228) + 0xd8))((longlong *)(param_1 + 0x228),param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x250) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x250) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x250),param_2);
  }
  local_res8[0] = *(longlong *)(param_1 + 600) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 600) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 600),param_2);
  }
  CyclicFramePosition__save(param_1 + 0x330,param_2);
  CyclicFramePosition__save(param_1 + 0x334,param_2);
  MapSerialiser__operator<<<Color>(param_2,param_1 + 0x338);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x298,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2a8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  TargeterBase__save(param_1 + 0x2b0,param_2);
  MapPosition__save(param_1 + 0x310,param_2);
  BoundingBox__save(param_1 + 0x318,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x32c,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

