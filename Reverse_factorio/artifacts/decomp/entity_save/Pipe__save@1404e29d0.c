// Pipe::save @ 1404e29d0 (function Pipe::save)

void Pipe__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  FluidBox__save(param_1 + 0xb8,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x1c0) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x1c0) != 0) {
    SingleFluidBoxControlBehavior__save(*(longlong *)(param_1 + 0x1c0),param_2);
  }
  return;
}

