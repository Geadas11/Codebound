// HighlightBoxEntity::save @ 1404835a0 (function HighlightBoxEntity::save)

void HighlightBoxEntity__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  BoundingBox__save(param_1 + 0xa8,param_2);
  TargeterBase__save(param_1 + 0xc0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8[0] = *(undefined1 *)(param_1 + 0xe1);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe4,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xf0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  return;
}

