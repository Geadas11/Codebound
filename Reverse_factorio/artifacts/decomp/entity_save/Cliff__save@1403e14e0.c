// Cliff::save @ 1403e14e0 (function Cliff::save)

void Cliff__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  Entity__save();
  local_res8[0] = *(undefined1 *)(param_1 + 0x80);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x81,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

