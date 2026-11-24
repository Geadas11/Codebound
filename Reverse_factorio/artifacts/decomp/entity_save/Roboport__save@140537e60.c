// Roboport::save @ 140537e60 (function Roboport::save)

void Roboport__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_1 + 0xd0) + 0x50))(*(longlong **)(param_1 + 0xd0),param_2);
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xd8,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    CyclicFramePosition__save(param_1 + 0xdc,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xe0,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xf0,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(*(longlong *)(param_1 + 0xf8) + 0xd8))((longlong *)(param_1 + 0xf8),param_2);
    (**(code **)(*(longlong *)(param_1 + 0x120) + 0xd8))((longlong *)(param_1 + 0x120),param_2);
    MapSerialiser__operator<<<SizeLimitedString<500>_>(param_2,param_1 + 0x148);
    LogisticCell__save(param_1 + 0x168,param_2);
    WakeUpList__save(param_1 + 0x268,param_2);
    LogisticPoint__save(*(undefined8 *)(param_1 + 0x290),param_2);
  }
  Roboport__RobotRequesterPoint__save(param_1 + 0x298,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x288) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x288) != 0) {
    RoboportControlBehavior__save(*(longlong *)(param_1 + 0x288),param_2);
  }
  if (*(char *)(param_2 + 9) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xe4,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    MapSerialiser__operator<<<ForceChartedArea>(param_2,param_1 + 0x398);
  }
  return;
}

