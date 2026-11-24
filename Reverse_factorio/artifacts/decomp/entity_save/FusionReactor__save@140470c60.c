// FusionReactor::save @ 140470c60 (function FusionReactor::save)

void FusionReactor__save(longlong param_1,longlong param_2)

{
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  ElectricEnergySource__save(*(undefined8 *)(param_1 + 0xd8),param_2);
  Burner__save(*(undefined8 *)(param_1 + 0xe0),param_2);
  WakeUpList__save(param_1 + 0xe8,param_2);
  NeighbourConnectable__save(param_1 + 0x108,param_2);
  FluidBox__save(param_1 + 0x130,param_2);
  FluidBox__save(param_1 + 0x238,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x340,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  return;
}

