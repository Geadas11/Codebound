// RocketSilo::save @ 140544700 (function RocketSilo::save)

void RocketSilo__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  CraftingMachine__save();
  local_res8[0] = *(longlong *)(param_1 + 0x2f8) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x2f8) != 0) {
    AssemblingMachineControlBehavior__save(*(longlong *)(param_1 + 0x2f8),param_2);
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    local_res8[0] = *(undefined1 *)(param_1 + 0x300);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x308,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x310,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x318,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x328,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x330,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    CyclicFramePosition__save(param_1 + 0x334,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x338,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    TargeterBase__save(param_1 + 0x340,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x360,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x364,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  Inventory__save(param_1 + 0x368,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x390,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  MapSerialiser__operator<<<CargoStation>(param_2,param_1 + 0x3c0);
  (**(code **)(*(longlong *)(param_1 + 0x398) + 0xd8))((longlong *)(param_1 + 0x398),param_2);
  if (*(char *)(param_2 + 0x149) == '\0') {
    local_res8[0] = *(longlong *)(param_1 + 0x468) != 0;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    if (*(longlong *)(param_1 + 0x468) != 0) {
      LogisticPoint__save(*(longlong *)(param_1 + 0x468),param_2);
    }
  }
  local_res8[0] = *(longlong *)(param_1 + 0x498) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x498) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x498),param_2);
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    local_res8[0] = *(longlong *)(param_1 + 0x4a0) != 0;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    if (*(longlong *)(param_1 + 0x4a0) != 0) {
      LogisticPoint__save(*(longlong *)(param_1 + 0x4a0),param_2);
    }
  }
  TargeterBase__save(param_1 + 0x478,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x4a8) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x4a8) != 0) {
    RocketSiloControlBehavior__save(*(longlong *)(param_1 + 0x4a8),param_2);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x4b0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x4b8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

