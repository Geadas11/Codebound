// LinkedBelt::save @ 1404b79e0 (function LinkedBelt::save)

void LinkedBelt__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  TransportBeltConnectable__save();
  local_res8[0] = *(undefined1 *)(param_1 + 0x128);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x130,param_2);
  return;
}

