// CargoLandingPad::save @ 1403b21a0 (function CargoLandingPad::save)

void CargoLandingPad__save(longlong param_1,longlong param_2)

{
  char cVar1;
  longlong *plVar2;
  longlong *plVar3;
  longlong *plVar4;
  undefined8 local_res8;
  
  EntityWithOwner__save();
  local_res8 = *(undefined8 *)(param_1 + 0xa8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<InventoryWithBar>(param_2,param_1 + 0xb8);
  (**(code **)(*(longlong *)(param_1 + 0xe0) + 0xd8))((longlong *)(param_1 + 0xe0),param_2);
  local_res8 = *(undefined8 *)(param_1 + 0x110);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  plVar4 = (longlong *)**(longlong **)(param_1 + 0x108);
  cVar1 = *(char *)((longlong)plVar4 + 0x19);
  while( true ) {
    if (cVar1 != '\0') {
      MapSerialiser__operator<<<CargoStation>(param_2,param_1 + 0x118);
      local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x1c0) != 0);
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&local_res8,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      if (*(longlong *)(param_1 + 0x1c0) != 0) {
        LogisticPoint__save(*(longlong *)(param_1 + 0x1c0),param_2);
      }
      LogisticPoint__save(*(undefined8 *)(param_1 + 0x1c8),param_2);
      local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x1d0) != 0);
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&local_res8,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      if (*(longlong *)(param_1 + 0x1d0) != 0) {
        LogisticPoint__save(*(longlong *)(param_1 + 0x1d0),param_2);
      }
      WakeUpList__save(param_1 + 0x1d8,param_2);
      WakeUpList__save(param_1 + 0x1f8,param_2);
      MapSerialiser__operator<<<ForceChartedArea>(param_2,param_1 + 0x218);
      MapSerialiser__operator<<<std::vector<Targeter<CargoBay,19,16>,std::allocator<Targeter<CargoBay,19,16>_>_>_>
                (param_2,param_1 + 0x228);
      local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x240) != 0);
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&local_res8,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      if (*(longlong *)(param_1 + 0x240) != 0) {
        CargoLandingPadControlBehavior__save(*(longlong *)(param_1 + 0x240),param_2);
      }
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),param_1 + 0x248,8);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),param_1 + 0x250,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      return;
    }
    if ((short)plVar4[4] == 0) break;
    ID<ItemPrototype,unsigned_short>__save(plVar4 + 4,param_2);
    if ((short)plVar4[4] != 0) {
      MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>
                (param_2,(longlong)plVar4 + 0x22);
    }
    (**(code **)(plVar4[5] + 0xd8))(plVar4 + 5,param_2);
    plVar2 = (longlong *)plVar4[2];
    if (*(char *)((longlong)plVar2 + 0x19) == '\0') {
      cVar1 = *(char *)(*plVar2 + 0x19);
      plVar4 = plVar2;
      plVar2 = (longlong *)*plVar2;
      while (cVar1 == '\0') {
        cVar1 = *(char *)(*plVar2 + 0x19);
        plVar4 = plVar2;
        plVar2 = (longlong *)*plVar2;
      }
    }
    else {
      cVar1 = *(char *)(plVar4[1] + 0x19);
      plVar3 = (longlong *)plVar4[1];
      plVar2 = plVar4;
      while ((plVar4 = plVar3, cVar1 == '\0' && (plVar2 == (longlong *)plVar4[2]))) {
        cVar1 = *(char *)(plVar4[1] + 0x19);
        plVar3 = (longlong *)plVar4[1];
        plVar2 = plVar4;
      }
    }
    cVar1 = *(char *)((longlong)plVar4 + 0x19);
  }
                    /* WARNING: Subroutine does not return */
  ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/SerialiserHelper.hpp"
                      ,0xd9,"bool(item.first)");
}

