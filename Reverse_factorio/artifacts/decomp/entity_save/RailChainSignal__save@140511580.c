// RailChainSignal::save @ 140511580 (function RailChainSignal::save)

void RailChainSignal__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  RailSignalBase__save();
  local_res8[0] = *(undefined1 *)(param_1 + 0x140);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

