// DisplayPanel::save @ 14040b300 (function DisplayPanel::save)

void DisplayPanel__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong lVar2;
  longlong lVar3;
  longlong *plVar4;
  longlong lVar5;
  longlong local_res8;
  
  EntityWithOwner__save();
  plVar4 = (longlong *)(param_1 + 0xa8);
  local_res8 = *(longlong *)(param_1 + 0xb8);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0xb8);
  if (0xf < *(ulonglong *)(param_1 + 0xc0)) {
    plVar4 = (longlong *)*plVar4;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar4,lVar1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar1;
  SignalIDBase__save(param_1 + 200,param_2);
  if (*(short *)(param_1 + 200) != 0) {
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0xcc);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd1,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd2,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0xe0) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  lVar1 = *(longlong *)(param_1 + 0xe0);
  if (lVar1 != 0) {
    CircuitConnector__save(lVar1 + 0x20,param_2);
    local_res8 = (*(longlong *)(lVar1 + 0x180) - *(longlong *)(lVar1 + 0x178) >> 3) *
                 -0x71c71c71c71c71c7;
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar2 = *(longlong *)(lVar1 + 0x180);
    for (lVar5 = *(longlong *)(lVar1 + 0x178); lVar5 != lVar2; lVar5 = lVar5 + 0x48) {
      CircuitCondition__save(lVar5,param_2);
      SignalIDBase__save(lVar5 + 0x1c,param_2);
      if (*(short *)(lVar5 + 0x1c) != 0) {
        MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,lVar5 + 0x20);
      }
      local_res8 = *(longlong *)(lVar5 + 0x38);
      Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                (param_2,&local_res8);
      plVar4 = (longlong *)(lVar5 + 0x28);
      lVar3 = *(longlong *)(lVar5 + 0x38);
      if (0xf < *(ulonglong *)(lVar5 + 0x40)) {
        plVar4 = (longlong *)*plVar4;
      }
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),plVar4,lVar3);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar3;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar1 + 400,1)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

