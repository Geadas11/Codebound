// Boiler::save @ 140396b70 (function Boiler::save)

void Boiler__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_1 + 0x2e8) + 0x50))(*(longlong **)(param_1 + 0x2e8),param_2);
  WakeUpList__save(param_1 + 0x2f0,param_2);
  FluidBox__save(param_1 + 0xd8,param_2);
  FluidBox__save(param_1 + 0x1e0,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x328) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  lVar1 = *(longlong *)(param_1 + 0x328);
  if (lVar1 != 0) {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"boiler-control-behavior",
                 param_2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"boiler-control-behavior",0x17);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x17;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    GenericOnOffControlBehavior__save(lVar1,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),lVar1 + 0x218,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"boiler-control-behavior",0x17);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x17;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  return;
}

