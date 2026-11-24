// ContainerEntity::save @ 1403f41e0 (function ContainerEntity::save)

void ContainerEntity__save(longlong param_1,longlong param_2)

{
  int iVar1;
  longlong lVar2;
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  SpecificInventoryLoader__saveInventory(*(undefined8 *)(param_1 + 0xe0),param_2);
  WakeUpList__save(param_1 + 0xe8,param_2);
  WakeUpList__save(param_1 + 0x108,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x154,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x128) != 0) {
    lVar2 = __RTtypeid();
    iVar1 = __std_type_info_compare(lVar2 + 8,0x142a02420);
    if (iVar1 == 0) {
      MapSerialiser__saveOptional<std::unique_ptr<ContainerControlBehavior,std::default_delete<ContainerControlBehavior>_>_>
                (param_2,param_1 + 0x128);
      return;
    }
  }
  local_res8[0] = 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

