// AssemblingMachine::save @ 1403760a0 (function AssemblingMachine::save)

void AssemblingMachine__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  CraftingMachine__save();
  local_res8[0] = *(longlong *)(param_1 + 0x2f8) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x2f8) != 0) {
    AssemblingMachineControlBehavior__save(*(longlong *)(param_1 + 0x2f8),param_2);
  }
  return;
}

