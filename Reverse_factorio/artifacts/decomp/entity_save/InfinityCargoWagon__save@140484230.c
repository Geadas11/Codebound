// InfinityCargoWagon::save @ 140484230 (function InfinityCargoWagon::save)

void InfinityCargoWagon__save(longlong param_1,longlong param_2)

{
  RollingStock__save();
  (**(code **)(*(longlong *)(param_1 + 0x300) + 0xd8))((longlong *)(param_1 + 0x300),param_2);
  WakeUpList__save(param_1 + 0x338,param_2);
  WakeUpList__save(param_1 + 0x358,param_2);
  InfinityInventorySettings__save(param_1 + 0x3a0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x3d0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

