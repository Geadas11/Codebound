// Beacon::save @ 14038dc00 (function Beacon::save)

void Beacon__save(longlong param_1,longlong param_2)

{
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_1 + 0xd0) + 0x50))(*(longlong **)(param_1 + 0xd0),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xf0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xf8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(*(longlong *)(param_1 + 0x100) + 0xd8))((longlong *)(param_1 + 0x100),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*(char *)(param_2 + 9) != '\0') {
    Effect__save(param_1 + 0x130,param_2);
  }
  return;
}

