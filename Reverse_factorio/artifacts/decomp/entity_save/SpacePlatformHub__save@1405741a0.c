// SpacePlatformHub::save @ 1405741a0 (function SpacePlatformHub::save)

void SpacePlatformHub__save(longlong param_1,longlong param_2)

{
  undefined8 *puVar1;
  undefined8 *puVar2;
  longlong local_res8;
  
  EntityWithOwner__save();
  local_res8 = *(longlong *)(param_1 + 0xa8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  MapSerialiser__operator<<<InventoryWithBar>(param_2,param_1 + 0xb0);
  (**(code **)(*(longlong *)(param_1 + 0xd8) + 0xd8))((longlong *)(param_1 + 0xd8),param_2);
  MapSerialiser__operator<<<CargoStation>(param_2,param_1 + 0x100);
  LogisticPoint__save(*(undefined8 *)(param_1 + 0x1a8),param_2);
  LogisticPoint__save(*(undefined8 *)(param_1 + 0x1b0),param_2);
  WakeUpList__save(param_1 + 0x1b8,param_2);
  WakeUpList__save(param_1 + 0x1d8,param_2);
  local_res8 = *(longlong *)(param_1 + 0x200) - *(longlong *)(param_1 + 0x1f8) >> 4;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar1 = *(undefined8 **)(param_1 + 0x200);
  for (puVar2 = *(undefined8 **)(param_1 + 0x1f8); puVar2 != puVar1; puVar2 = puVar2 + 2) {
    Character__save(*puVar2,param_2);
    ID<SpaceLocationPrototype,unsigned_short>__save(puVar2 + 1,param_2);
  }
  MapSerialiser__operator<<<std::vector<Targeter<CargoBay,19,16>,std::allocator<Targeter<CargoBay,19,16>_>_>_>
            (param_2,param_1 + 0x210);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x228,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x238) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x238) != 0) {
    SpacePlatformHubControlBehavior__save(*(longlong *)(param_1 + 0x238),param_2);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x240,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

