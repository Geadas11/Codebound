// HeatPipe::save @ 1404820c0 (function HeatPipe::save)

void HeatPipe__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  HeatBuffer__save(*(undefined8 *)(param_1 + 0xb0),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  EntityHeaterData__save(param_1 + 0xc0,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x100) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x100) != 0) {
    HeatPipeControlBehavior__save(*(longlong *)(param_1 + 0x100),param_2);
  }
  return;
}

