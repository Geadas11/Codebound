// CargoBay::save @ 1403aaef0 (function CargoBay::save)

void CargoBay__save(longlong param_1,longlong param_2)

{
  undefined8 local_res8;
  
  EntityWithOwner__save();
  local_res8 = *(undefined8 *)(param_1 + 0xa8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  TargeterBase__save(param_1 + 0xb0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<std::vector<std::unique_ptr<CargoHatch,std::default_delete<CargoHatch>_>,std::allocator<std::unique_ptr<CargoHatch,std::default_delete<CargoHatch>_>_>_>_>
            (param_2,param_1 + 0xd8);
  if (*(char *)(param_2 + 9) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0xf0,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

