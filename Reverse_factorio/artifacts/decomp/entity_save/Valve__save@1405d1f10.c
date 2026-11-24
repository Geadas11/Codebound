// Valve::save @ 1405d1f10 (function Valve::save)

void Valve__save(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  CyclicFramePosition__save(param_1 + 0xd4,param_2);
  FluidBox__save(param_1 + 0xd8,param_2);
  plVar1 = *(longlong **)(param_2 + 0x50);
  if (*(char *)(param_1 + 0x1e4) != '\0') {
    local_res8[0] = 1;
    (**(code **)(*plVar1 + 8))();
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x1e0,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    return;
  }
  local_res8[0] = 0;
  (**(code **)(*plVar1 + 8))(plVar1,local_res8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

