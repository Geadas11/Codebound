// LightningAttractor::save @ 1404b3740 (function LightningAttractor::save)

void LightningAttractor__save(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  plVar1 = *(longlong **)(param_2 + 0x50);
  if (*(longlong *)(param_1 + 0xa8) != 0) {
    local_res8[0] = 1;
    (**(code **)(*plVar1 + 8))();
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    ElectricEnergySource__save(*(undefined8 *)(param_1 + 0xa8),param_2);
    return;
  }
  local_res8[0] = 0;
  (**(code **)(*plVar1 + 8))(plVar1,local_res8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

