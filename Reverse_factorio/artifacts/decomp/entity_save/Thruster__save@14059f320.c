// Thruster::save @ 14059f320 (function Thruster::save)

void Thruster__save(longlong param_1,longlong param_2)

{
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2e0,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  FluidBox__save(param_1 + 0xd0,param_2);
  FluidBox__save(param_1 + 0x1d8,param_2);
  return;
}

