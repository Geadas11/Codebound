// LogisticContainer::save @ 1404c7b30 (function LogisticContainer::save)

void LogisticContainer__save(longlong param_1,longlong param_2,char param_3)

{
  undefined8 *puVar1;
  longlong *plVar2;
  int iVar3;
  longlong lVar4;
  undefined1 local_res18 [8];
  
  ContainerEntity__save();
  puVar1 = (undefined8 *)(param_2 + 0x50);
  plVar2 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x180,8);
    *plVar2 = *plVar2 + 8;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x188,1);
    *plVar2 = *plVar2 + 1;
    (**(code **)(*(longlong *)(param_1 + 0x158) + 0xd8))((longlong *)(param_1 + 0x158),param_2);
  }
  if (param_3 != '\0') {
    local_res18[0] = *(longlong *)(param_1 + 400) != 0;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,local_res18,1);
    *plVar2 = *plVar2 + 1;
    if (*(longlong *)(param_1 + 400) != 0) {
      LogisticPoint__save(*(longlong *)(param_1 + 400),param_2);
    }
  }
  local_res18[0] = *(longlong *)(param_1 + 0x198) != 0;
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,local_res18,1);
  *plVar2 = *plVar2 + 1;
  if (*(longlong *)(param_1 + 0x198) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x198),param_2);
  }
  if (*(longlong *)(param_1 + 0x128) != 0) {
    lVar4 = __RTtypeid();
    iVar3 = __std_type_info_compare(lVar4 + 8,0x142a7aab0);
    if (iVar3 == 0) {
      MapSerialiser__saveOptional<std::unique_ptr<ContainerControlBehavior,std::default_delete<ContainerControlBehavior>_>_>
                (param_2,param_1 + 0x128);
      return;
    }
  }
  local_res18[0] = 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res18,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

