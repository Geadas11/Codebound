// TrainStop::save @ 1405a6fc0 (function TrainStop::save)

void TrainStop__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  longlong lVar2;
  longlong *plVar3;
  longlong local_res8;
  
  EntityWithOwner__save();
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"station-id",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"station-id",10);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  plVar3 = (longlong *)(param_1 + 0xb0);
  local_res8 = *(longlong *)(param_1 + 0xc0);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar2 = *(longlong *)(param_1 + 0xc0);
  if (0xf < *(ulonglong *)(param_1 + 200)) {
    plVar3 = (longlong *)*plVar3;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar3,lVar2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar2;
  local_res8 = param_2 + 8;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"station-id",10);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  plVar3 = (longlong *)(param_2 + 0x10);
  puVar1 = (undefined8 *)(param_2 + 0x50);
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0xe0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0xe8) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0xe8) != 0) {
    TrainStopControlBehavior__save(*(longlong *)(param_1 + 0xe8),param_2);
  }
  MapSerialiser__operator<<<std::optional<Color>_>(param_2,param_1 + 0xf0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x108,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x114,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x104));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x110,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  if (*(char *)(param_2 + 9) != '\0') {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar1 + 8))
                ((longlong *)*puVar1,&DAT_1421e010c,1,param_4,"complete",param_2);
      *plVar3 = *plVar3 + 1;
      (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"complete",8);
      *plVar3 = *plVar3 + 8;
      (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x10c,4);
    *plVar3 = *plVar3 + 4;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_14230f710,2);
      *plVar3 = *plVar3 + 2;
      (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"complete",8);
      *plVar3 = *plVar3 + 8;
      (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
  }
  return;
}

