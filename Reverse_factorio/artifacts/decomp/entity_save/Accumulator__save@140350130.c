// Accumulator::save @ 140350130 (function Accumulator::save)

void Accumulator__save(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  undefined1 local_res8 [8];
  undefined1 local_res10 [8];
  
  EntityWithOwner__save();
  local_res8[0] = *(longlong *)(param_1 + 0xc0) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  plVar1 = *(longlong **)(param_2 + 0x50);
  if (*(longlong *)(param_1 + 0xc0) == 0) {
    (**(code **)(*plVar1 + 8))(plVar1,param_1 + 200,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xca,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  }
  else {
    (**(code **)(*plVar1 + 8))(plVar1,*(longlong *)(param_1 + 0xc0) + 0xc,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  ElectricEnergySource__save(*(undefined8 *)(param_1 + 0xd8),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res10[0] = *(longlong *)(param_1 + 0xe8) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res10,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0xe8) != 0) {
    AccumulatorControlBehavior__save(*(longlong *)(param_1 + 0xe8),param_2);
  }
  return;
}

