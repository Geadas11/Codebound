// EntityWithOwner::save @ 140448760 (function EntityWithOwner::save)

void EntityWithOwner__save(longlong param_1,longlong param_2)

{
  undefined8 *puVar1;
  longlong *plVar2;
  longlong *plVar3;
  ushort uVar4;
  ushort local_res8 [4];
  
  EntityWithHealth__save();
  puVar1 = (undefined8 *)(param_2 + 0x50);
  plVar2 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x90,1);
    *plVar2 = *plVar2 + 1;
  }
  MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x91);
  EntityWithOwner__saveUnitNumber(param_1,param_2);
  if (*(char *)(param_2 + 0x149) == '\0') {
    plVar3 = (longlong *)*puVar1;
    uVar4 = *(short *)(param_1 + 0x92) + 1;
    if (uVar4 < 0xff) {
      local_res8[0] = CONCAT11(local_res8[0]._1_1_,(char)uVar4);
      (**(code **)(*plVar3 + 8))();
      *plVar2 = *plVar2 + 1;
    }
    else {
      local_res8[0] = CONCAT11(local_res8[0]._1_1_,0xff);
      (**(code **)(*plVar3 + 8))(plVar3,local_res8,1);
      *plVar2 = *plVar2 + 1;
      local_res8[0] = uVar4;
      (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,local_res8,2);
      *plVar2 = *plVar2 + 2;
    }
  }
  local_res8[0] = CONCAT11(local_res8[0]._1_1_,*(longlong *)(param_1 + 0xa0) != 0);
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,local_res8,1);
  *plVar2 = *plVar2 + 1;
  if (*(longlong *)(param_1 + 0xa0) != 0) {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_142058688,1);
    *plVar2 = *plVar2 + 1;
  }
  return;
}

