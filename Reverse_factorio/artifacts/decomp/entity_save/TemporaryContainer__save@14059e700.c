// TemporaryContainer::save @ 14059e700 (function TemporaryContainer::save)

void TemporaryContainer__save(longlong param_1,longlong param_2)

{
  ContainerEntity__save();
  UpdatableEntity__save(param_1 + 0x158,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x180,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x188,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  return;
}

