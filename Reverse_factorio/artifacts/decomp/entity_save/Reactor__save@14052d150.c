// Reactor::save @ 14052d150 (function Reactor::save)

void Reactor__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  HeatBuffer__save(*(undefined8 *)(param_1 + 0xd8),param_2);
  (**(code **)(**(longlong **)(param_1 + 0xe0) + 0x50))(*(longlong **)(param_1 + 0xe0),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x130,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  WakeUpList__save(param_1 + 0x138,param_2);
  NeighbourConnectable__save(param_1 + 0x158,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x180) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x180) != 0) {
    ReactorControlBehavior__save(*(longlong *)(param_1 + 0x180),param_2);
  }
  EntityHeaterData__save(param_1 + 0xe8,param_2);
  return;
}

