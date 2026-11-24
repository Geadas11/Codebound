// ElectricPole::save @ 140410b60 (function ElectricPole::save)

void ElectricPole__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  MapSerialiser__operator<<<CopperWireConnector>(param_2,param_1 + 0xb0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xaa,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8[0] = *(longlong *)(param_1 + 0x138) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x138) != 0) {
    CircuitConnector__save(*(longlong *)(param_1 + 0x138),param_2);
  }
  return;
}

