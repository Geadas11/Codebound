// AmmoTurret::save @ 1403636f0 (function AmmoTurret::save)

void AmmoTurret__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  Turret__save();
  (**(code **)(*(longlong *)(param_1 + 0x208) + 0xd8))((longlong *)(param_1 + 0x208),param_2);
  WakeUpList__save(param_1 + 0x230,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x200) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x200) != 0) {
    ElectricEnergySource__save(*(longlong *)(param_1 + 0x200),param_2);
  }
  return;
}

