// SolarPanel::save @ 14056ee60 (function SolarPanel::save)

void SolarPanel__save(longlong param_1,undefined8 param_2)

{
  EntityWithOwner__save();
  ElectricEnergySource__save(*(undefined8 *)(param_1 + 0xa8),param_2);
  return;
}

