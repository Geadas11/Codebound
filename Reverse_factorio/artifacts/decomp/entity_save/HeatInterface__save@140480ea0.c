// HeatInterface::save @ 140480ea0 (function HeatInterface::save)

void HeatInterface__save(longlong param_1,undefined8 param_2)

{
  undefined8 *puVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  char *local_18;
  undefined8 local_10;
  
  local_10 = 0xb;
  local_18 = "temperature";
  puVar1 = (undefined8 *)PropertyTree__operator__(param_2,&local_18);
  PropertyTree__clear(puVar1);
  *puVar1 = *(undefined8 *)(param_1 + 0xe0);
  *(byte *)((longlong)puVar1 + 0x29) = *(byte *)((longlong)puVar1 + 0x29) & 0xc2 | 2;
  uVar2 = InfinityFilter__modeToString(*(undefined1 *)(param_1 + 0xe8));
  local_10 = 4;
  local_18 = "mode";
  uVar3 = PropertyTree__operator__(param_2,&local_18);
  PropertyTree__clear(uVar3);
  PropertyTree__setInternal(uVar3,uVar2);
  return;
}

