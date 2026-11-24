// Lamp::save @ 1404aae90 (function Lamp::save)

void Lamp__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  MapSerialiser__operator<<<Color>(param_2,param_1 + 0xa8);
  local_res8[0] = *(undefined1 *)(param_1 + 0xc4);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xc5,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_1 + 200) + 0x50))(*(longlong **)(param_1 + 200),param_2);
  local_res8[0] = *(longlong *)(param_1 + 0xb8) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0xb8) != 0) {
    LampControlBehavior__save(*(longlong *)(param_1 + 0xb8),param_2);
  }
  return;
}

