// Loader::save @ 1404bcef0 (function Loader::save)

void Loader__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  byte bVar2;
  longlong *plVar3;
  longlong *plVar4;
  undefined8 *puVar5;
  undefined8 *puVar6;
  undefined1 local_res8 [8];
  
  TransportBeltConnectable__save();
  UpdatableEntity__save(param_1 + 0x128,param_2);
  local_res8[0] = *(undefined1 *)(param_1 + 0x1cd);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_1 + 0x1d0) + 0x50))(*(longlong **)(param_1 + 0x1d0),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1dc,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  TargeterBase__save(param_1 + 0x1f0,param_2);
  TargeterBase__save(param_1 + 0x218,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),*(longlong *)(param_1 + 0x48) + 0x8f8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  bVar2 = 0;
  if (*(char *)(*(longlong *)(param_1 + 0x48) + 0x8f8) != '\0') {
    do {
      lVar1 = param_1 + (ulonglong)bVar2 * 4;
      ID<ItemPrototype,unsigned_short>__save(lVar1 + 0x1b8,param_2);
      MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,lVar1 + 0x1ba);
      if (*(char *)(lVar1 + 0x1ba) != '\0') {
        local_res8[0] = *(undefined1 *)(lVar1 + 0x1bb);
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),local_res8,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      }
      bVar2 = bVar2 + 1;
    } while (bVar2 < *(byte *)(*(longlong *)(param_1 + 0x48) + 0x8f8));
  }
  puVar5 = (undefined8 *)(param_2 + 0x50);
  plVar4 = (longlong *)(param_2 + 0x10);
  local_res8[0] = *(undefined1 *)(param_1 + 0x1cc);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *plVar4 = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,param_1 + 0x1da,1);
  *plVar4 = *plVar4 + 1;
  local_res8[0] = *(longlong *)(param_1 + 0x240) != 0;
  (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,local_res8,1);
  *plVar4 = *plVar4 + 1;
  if (*(longlong *)(param_1 + 0x240) != 0) {
    LoaderControlBehavior__save(*(longlong *)(param_1 + 0x240),param_2);
  }
  if (*(char *)(param_2 + 9) != '\0') {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar5 + 8))
                ((longlong *)*puVar5,&DAT_1421e010c,1,param_4,"complete-mode",param_2);
      *plVar4 = *plVar4 + 1;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,"complete-mode",0xd);
      *plVar4 = *plVar4 + 0xd;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
    puVar6 = (undefined8 *)(param_2 + 0x50);
    plVar3 = (longlong *)(param_2 + 0x10);
    (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,param_1 + 0x1e0,4);
    *plVar4 = *plVar4 + 4;
    (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,param_1 + 0x1e4,4);
    *plVar4 = *plVar4 + 4;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar3 = *plVar3 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"complete-mode",0xd);
      *plVar3 = *plVar3 + 0xd;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
  }
  return;
}

