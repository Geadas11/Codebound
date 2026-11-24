// Lab::save @ 1404a5950 (function Lab::save)

void Lab__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong *plVar2;
  longlong lVar3;
  longlong local_res8;
  undefined2 local_res10 [4];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(*(longlong *)(param_1 + 0x118) + 0xd8))((longlong *)(param_1 + 0x118),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x140) + 0xd8))((longlong *)(param_1 + 0x140),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x170) + 0xd8))((longlong *)(param_1 + 0x170),param_2);
  (**(code **)(**(longlong **)(param_1 + 0x198) + 0x50))(*(longlong **)(param_1 + 0x198),param_2);
  MapSerialiser__operator<<<EffectReceiver>(param_2,param_1 + 0x1d0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1a0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  WakeUpList__save(param_1 + 0x218,param_2);
  WakeUpList__save(param_1 + 0x238,param_2);
  CyclicFramePosition__save(param_1 + 0x1a8,param_2);
  if (*(longlong *)(param_1 + 0xd8) == 0) {
    local_res10[0] = 0;
    plVar2 = (longlong *)local_res10;
  }
  else {
    plVar2 = &local_res8;
    local_res8 = CONCAT62(local_res8._2_6_,*(undefined2 *)(*(longlong *)(param_1 + 0xd8) + 0x1c8));
  }
  SerialiserHelper<MapSerialiser>__save<ID<TechnologyPrototype,unsigned_short>_>(param_2,plVar2);
  plVar2 = (longlong *)(param_1 + 0x1b0);
  local_res8 = *(longlong *)(param_1 + 0x1c0);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x1c0);
  if (0xf < *(ulonglong *)(param_1 + 0x1c8)) {
    plVar2 = (longlong *)*plVar2;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar2,lVar1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1ac,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0xfc));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = *(longlong *)(param_1 + 0x108) - *(longlong *)(param_1 + 0x100) >> 4;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x108);
  for (lVar3 = *(longlong *)(param_1 + 0x100); lVar3 != lVar1; lVar3 = lVar3 + 0x10) {
    ID<ItemPrototype,unsigned_short>__save(lVar3,param_2);
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,lVar3 + 2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),lVar3 + 8,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  }
  plVar2 = (longlong *)(param_2 + 0x10);
  local_res8._1_7_ = (undefined7)((ulonglong)local_res8 >> 8);
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 600) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *plVar2 = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 600) != 0) {
    LabControlBehavior__save(*(longlong *)(param_1 + 600),param_2);
  }
  if ((*(char *)(param_2 + 9) != '\0') && ((*(byte *)(param_1 + 0x6e) & 0x20) == 0)) {
    local_res8 = *(longlong *)(param_1 + 0xf0);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,8)
    ;
    *plVar2 = *plVar2 + 8;
    local_res8 = CONCAT44(local_res8._4_4_,*(undefined4 *)(param_1 + 0xf8));
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4)
    ;
    *plVar2 = *plVar2 + 4;
  }
  return;
}

