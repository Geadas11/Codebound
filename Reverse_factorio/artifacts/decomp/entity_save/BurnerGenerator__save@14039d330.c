// BurnerGenerator::save @ 14039d330 (function BurnerGenerator::save)

void BurnerGenerator__save(longlong param_1,longlong param_2)

{
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  ElectricEnergySource__save(*(undefined8 *)(param_1 + 0xd0),param_2);
  Burner__save(*(undefined8 *)(param_1 + 0xd8),param_2);
  WakeUpList__save(param_1 + 0xe0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x100,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  CyclicFramePosition__save(param_1 + 0x108,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x10c,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

