// CargoWagon::save @ 1403bdcd0 (function CargoWagon::save)

void CargoWagon__save(longlong param_1,undefined8 param_2)

{
  RollingStock__save();
  (**(code **)(*(longlong *)(param_1 + 0x300) + 0xd8))((longlong *)(param_1 + 0x300),param_2);
  WakeUpList__save(param_1 + 0x338,param_2);
  WakeUpList__save(param_1 + 0x358,param_2);
  return;
}

