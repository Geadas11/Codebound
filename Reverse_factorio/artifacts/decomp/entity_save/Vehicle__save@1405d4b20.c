// Vehicle::save @ 1405d4b20 (function Vehicle::save)

void Vehicle__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  char *pcVar1;
  longlong *plVar2;
  undefined8 *puVar3;
  longlong lVar4;
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  lVar4 = *(longlong *)(param_1 + 0xd8);
  if ((lVar4 != 0) && (*(longlong *)(lVar4 + 0x48) != param_1)) {
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\Vehicle.cpp"
                        ,0xe0,"!this->grid || this->grid->owner.entity == this");
  }
  EquipmentGrid__save((longlong *)(param_1 + 0xd8),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe0,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  PortableRoboportProvider__save(param_1 + 0xf0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x118,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  pcVar1 = (char *)(param_2 + 8);
  if (*pcVar1 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"driver",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"driver",6);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 6;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  VehiclePassenger__save(param_1 + 0x120,param_2);
  if (*pcVar1 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"driver",6);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 6;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  plVar2 = (longlong *)(param_2 + 0x10);
  puVar3 = (undefined8 *)(param_2 + 0x50);
  if (*pcVar1 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"gunner",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"gunner",6);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 6;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  VehiclePassenger__save(param_1 + 0x130,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
    *plVar2 = *plVar2 + 2;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"gunner",6);
    *plVar2 = *plVar2 + 6;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x144,1);
  *plVar2 = *plVar2 + 1;
  if ((*(byte *)(param_1 + 0x6e) & 0x20) == 0) {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x140,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  return;
}

