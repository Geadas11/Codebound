// FireFlame::save @ 140450860 (function FireFlame::save)

void FireFlame__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  longlong lVar2;
  longlong *plVar3;
  undefined8 *puVar4;
  char local_res8 [8];
  
  puVar4 = (undefined8 *)(param_2 + 0x50);
  plVar3 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar4 + 8))
              ((longlong *)*puVar4,&DAT_1421e010c,1,param_4,"fire-flame",param_2);
    *plVar3 = *plVar3 + 1;
    (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,"fire-flame",10);
    *plVar3 = *plVar3 + 10;
    (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,&DAT_1421e0108,1);
    *plVar3 = *plVar3 + 1;
  }
  Entity__save(param_1,param_2);
  UpdatableEntity__save(param_1 + 0x80,param_2);
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0xb0,1);
  *plVar3 = *plVar3 + 1;
  TargeterBase__save(param_1 + 0xb8,param_2);
  MapPosition__save(param_1 + 0xd8,param_2);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0xe0);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0xec);
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x100,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x108,2);
  *plVar3 = *plVar3 + 2;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x10a,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x110,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x118,4);
  *plVar3 = *plVar3 + 4;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x11c,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x11d,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x120,4);
  *plVar3 = *plVar3 + 4;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x128,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x130,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x138,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x140,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x148,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x150,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x158,4);
  *plVar3 = *plVar3 + 4;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x15c,4);
  *plVar3 = *plVar3 + 4;
  TargeterBase__save(param_1 + 0x160,param_2);
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x180,8);
  *plVar3 = *plVar3 + 8;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x188,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x189,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x18a,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,param_1 + 0x18c,4);
  *plVar3 = *plVar3 + 4;
  local_res8[0] = (char)(*(longlong *)(param_1 + 0x198) - *(longlong *)(param_1 + 400) >> 3) * -0x55
  ;
  (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,local_res8,1);
  *plVar3 = *plVar3 + 1;
  lVar1 = *(longlong *)(param_1 + 0x198);
  for (lVar2 = *(longlong *)(param_1 + 400); lVar2 != lVar1; lVar2 = lVar2 + 0x18) {
    FireFlame__FlameParticle__save(lVar2,param_2);
  }
  puVar4 = (undefined8 *)(param_2 + 0x50);
  plVar3 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,&DAT_14230f710,2);
    *plVar3 = *plVar3 + 2;
    (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,"fire-flame",10);
    *plVar3 = *plVar3 + 10;
    (**(code **)(*(longlong *)*puVar4 + 8))((longlong *)*puVar4,&DAT_1421e0108,1);
    *plVar3 = *plVar3 + 1;
  }
  return;
}

