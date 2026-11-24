// Wall::save @ 1405d9a80 (function Wall::save)

void Wall__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  WallConnectable__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8[0] = *(longlong *)(param_1 + 0xd8) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0xd8) != 0) {
    WallControlBehavior__save(*(longlong *)(param_1 + 0xd8),param_2);
  }
  return;
}

