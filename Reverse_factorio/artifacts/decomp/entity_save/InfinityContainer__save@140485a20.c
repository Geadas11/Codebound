// InfinityContainer::save @ 140485a20 (function InfinityContainer::save)

void InfinityContainer__save(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  undefined1 *puVar2;
  undefined1 local_res8 [8];
  undefined1 local_res10 [8];
  
  LogisticContainer__save(param_1,param_2,0);
  UpdatableEntity__save(param_1 + 0x1a0,param_2);
  InfinityInventorySettings__save(param_1 + 0x1c8,param_2);
  puVar2 = local_res8;
  plVar1 = *(longlong **)(param_2 + 0x50);
  if (*(char *)(param_1 + 0x1f9) == '\0') {
    local_res8[0] = 0;
  }
  else {
    local_res8[0] = 1;
    (**(code **)(*plVar1 + 8))(plVar1,puVar2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    puVar2 = local_res10;
    local_res10[0] = *(undefined1 *)(param_1 + 0x1f8);
    plVar1 = *(longlong **)(param_2 + 0x50);
  }
  (**(code **)(*plVar1 + 8))(plVar1,puVar2,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<std::optional<bool>_>(param_2,param_1 + 0x1fa);
  MapSerialiser__operator<<<std::optional<bool>_>(param_2,param_1 + 0x1fc);
  local_res8[0] = *(longlong *)(param_1 + 0x200) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x200) != 0) {
    LogisticSections__save(*(longlong *)(param_1 + 0x200),param_2);
  }
  local_res8[0] = *(longlong *)(param_1 + 0x208) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x208) != 0) {
    LogisticSections__save(*(longlong *)(param_1 + 0x208),param_2);
  }
  local_res8[0] = *(longlong *)(param_1 + 400) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 400) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 400),param_2);
  }
  return;
}

