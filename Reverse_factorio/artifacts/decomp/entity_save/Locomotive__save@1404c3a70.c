// Locomotive::save @ 1404c3a70 (function Locomotive::save)

void Locomotive__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong *plVar2;
  undefined8 local_res8;
  
  RollingStock__save();
  (**(code **)(**(longlong **)(param_1 + 0x300) + 0x50))(*(longlong **)(param_1 + 0x300),param_2);
  plVar2 = (longlong *)(param_1 + 0x308);
  local_res8 = *(undefined8 *)(param_1 + 0x318);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x318);
  if (0xf < *(ulonglong *)(param_1 + 800)) {
    plVar2 = (longlong *)*plVar2;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar2,lVar1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar1;
  WakeUpList__save(param_1 + 0x328,param_2);
  return;
}

