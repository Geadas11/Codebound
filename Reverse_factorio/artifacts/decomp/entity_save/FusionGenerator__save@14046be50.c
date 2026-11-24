// FusionGenerator::save @ 14046be50 (function FusionGenerator::save)

void FusionGenerator__save(longlong param_1,longlong param_2)

{
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  ElectricEnergySource__save(*(undefined8 *)(param_1 + 0xd8),param_2);
  FluidBox__save(param_1 + 0xe0,param_2);
  FluidBox__save(param_1 + 0x1e8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2f0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2f8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  CyclicFramePosition__save(param_1 + 0x300,param_2);
  return;
}

