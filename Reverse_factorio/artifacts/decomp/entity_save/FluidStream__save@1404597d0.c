// FluidStream::save @ 1404597d0 (function FluidStream::save)

void FluidStream__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  longlong *plVar2;
  undefined8 *puVar3;
  longlong lVar4;
  int local_res8 [2];
  
  puVar3 = (undefined8 *)(param_2 + 0x50);
  plVar2 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))
              ((longlong *)*puVar3,&DAT_1421e010c,1,param_4,"fluid-stream",param_2);
    *plVar2 = *plVar2 + 1;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"fluid-stream",0xc);
    *plVar2 = *plVar2 + 0xc;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  Entity__save(param_1,param_2);
  UpdatableEntity__save(param_1 + 0x80,param_2);
  TargeterBase__save(param_1 + 0xa8,param_2);
  MapPosition__save(param_1 + 0x108,param_2);
  BoundingBox__save(param_1 + 0x110,param_2);
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x124,1);
  *plVar2 = *plVar2 + 1;
  TargeterBase__save(param_1 + 0x128,param_2);
  MapPosition__save(param_1 + 0x188,param_2);
  BoundingBox__save(param_1 + 400,param_2);
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x1a4,1);
  *plVar2 = *plVar2 + 1;
  MapPosition__save(param_1 + 0x1a8,param_2);
  MapPosition__save(param_1 + 0x1b0,param_2);
  MapPosition__save(param_1 + 0x1b8,param_2);
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x1c0,1);
  *plVar2 = *plVar2 + 1;
  TargeterBase__save(param_1 + 0x1c8,param_2);
  MapPosition__save(param_1 + 0x1e8,param_2);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0x1f0);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0x1fc);
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x210,8);
  *plVar2 = *plVar2 + 8;
  CyclicFramePosition__save(param_1 + 0x218,param_2);
  CyclicFramePosition__save(param_1 + 0x21c,param_2);
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x220,2);
  *plVar2 = *plVar2 + 2;
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x222,2);
  *plVar2 = *plVar2 + 2;
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x224,1);
  *plVar2 = *plVar2 + 1;
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x225,1);
  *plVar2 = *plVar2 + 1;
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x228,8);
  *plVar2 = *plVar2 + 8;
  local_res8[0] =
       (int)(*(longlong *)(param_1 + 0x238) - *(longlong *)(param_1 + 0x230) >> 3) * -0x33333333;
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,local_res8,4);
  *plVar2 = *plVar2 + 4;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e010c,1);
    *plVar2 = *plVar2 + 1;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"particles",9);
    *plVar2 = *plVar2 + 9;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  lVar1 = *(longlong *)(param_1 + 0x238);
  for (lVar4 = *(longlong *)(param_1 + 0x230); lVar4 != lVar1; lVar4 = lVar4 + 0x28) {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e010c,1);
      *plVar2 = *plVar2 + 1;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"fluid-particle",0xe);
      *plVar2 = *plVar2 + 0xe;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
      *plVar2 = *plVar2 + 1;
    }
    MapPosition__save(lVar4,param_2);
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,lVar4 + 8,4);
    *plVar2 = *plVar2 + 4;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,lVar4 + 0xc,4);
    *plVar2 = *plVar2 + 4;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,lVar4 + 0x10,4);
    *plVar2 = *plVar2 + 4;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,lVar4 + 0x16,2);
    *plVar2 = *plVar2 + 2;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,lVar4 + 0x18,8);
    *plVar2 = *plVar2 + 8;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,lVar4 + 0x20,1);
    *plVar2 = *plVar2 + 1;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
      *plVar2 = *plVar2 + 2;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"fluid-particle",0xe);
      *plVar2 = *plVar2 + 0xe;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
      *plVar2 = *plVar2 + 1;
    }
  }
  puVar3 = (undefined8 *)(param_2 + 0x50);
  plVar2 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
    *plVar2 = *plVar2 + 2;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"particles",9);
    *plVar2 = *plVar2 + 9;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
    *plVar2 = *plVar2 + 2;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"fluid-stream",0xc);
    *plVar2 = *plVar2 + 0xc;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  return;
}

