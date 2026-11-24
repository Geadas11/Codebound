// MapSerialiser::MapSerialiser @ 140c8a810 (function MapSerialiser::MapSerialiser)

undefined8 *
MapSerialiser__MapSerialiser(undefined8 *param_1,undefined8 *param_2,longlong param_3,int param_4)

{
  undefined8 *puVar1;
  longlong lVar2;
  undefined8 local_40;
  undefined8 local_38;
  
  local_40 = *param_2;
  local_38 = param_2[1];
  param_2[1] = 0;
  Serialiser__Serialiser(param_1,&local_40);
  *param_1 = MapSerialiser::vftable;
  param_1[0xc] = 0;
  *(undefined1 *)(param_1 + 0xd) = 0;
  puVar1 = (undefined8 *)operator_new(0x18);
  *puVar1 = 0;
  puVar1[1] = 0;
  puVar1[2] = 0;
  param_1[0xf] = puVar1;
  puVar1 = (undefined8 *)operator_new(0x18);
  *puVar1 = 0;
  puVar1[1] = 0;
  puVar1[2] = 0;
  param_1[0x10] = puVar1;
  param_1[0x11] = 0;
  puVar1 = (undefined8 *)operator_new(0x38);
  *(undefined4 *)((longlong)puVar1 + 0x31) = 0;
  *(undefined2 *)((longlong)puVar1 + 0x35) = 0;
  *(undefined1 *)((longlong)puVar1 + 0x37) = 0;
  *puVar1 = 0;
  puVar1[1] = 0;
  puVar1[2] = 0;
  puVar1[3] = 0;
  puVar1[4] = 0;
  puVar1[5] = 0;
  *(undefined1 *)(puVar1 + 6) = 0;
  param_1[0x12] = puVar1;
  param_1[0x13] = 0;
  param_1[0x14] = 0;
  param_1[0x15] = 0;
  lVar2 = operator_new(0x30);
  *(longlong *)lVar2 = lVar2;
  *(longlong *)(lVar2 + 8) = lVar2;
  *(longlong *)(lVar2 + 0x10) = lVar2;
  *(undefined2 *)(lVar2 + 0x18) = 0x101;
  param_1[0x14] = lVar2;
  param_1[0x16] = 0;
  param_1[0x17] = 0;
  lVar2 = operator_new(0x30);
  *(longlong *)lVar2 = lVar2;
  *(longlong *)(lVar2 + 8) = lVar2;
  *(longlong *)(lVar2 + 0x10) = lVar2;
  *(undefined2 *)(lVar2 + 0x18) = 0x101;
  param_1[0x16] = lVar2;
  param_1[0x18] = 0;
  param_1[0x19] = 0;
  param_1[0x1a] = 0;
  param_1[0x1b] = 0;
  param_1[0x1c] = 0;
  param_1[0x1d] = 0;
  *(undefined1 *)(param_1 + 0x1e) = 1;
  param_1[0x1f] = 0;
  param_1[0x20] = 0;
  param_1[0x21] = 0;
  param_1[0x22] = 0;
  param_1[0x23] = 0;
  param_1[0x24] = 0;
  param_1[0x25] = 0;
  param_1[0x26] = 0;
  param_1[0x27] = 0;
  param_1[0x28] = 0;
  *(undefined4 *)(param_1 + 0x29) = 0;
  *(undefined2 *)((longlong)param_1 + 0x14c) = 0;
  *(undefined1 *)((longlong)param_1 + 0x14e) = 0;
  if (param_4 == 0) {
    MapVersion__save(&DAT_14217c578,param_1);
  }
  if (param_3 == 0) {
    puVar1 = (undefined8 *)operator_new(0x38);
    *puVar1 = 0;
    puVar1[1] = 0;
    puVar1[2] = 0;
    puVar1[3] = 0;
    puVar1[4] = 0;
    puVar1[5] = 0;
    *(undefined2 *)(puVar1 + 6) = 0;
    param_1[0xe] = puVar1;
  }
  else {
    param_1[0xe] = param_3;
  }
  *(bool *)((longlong)param_1 + 0x14f) = param_3 == 0;
  puVar1 = (undefined8 *)param_2[1];
  param_2[1] = 0;
  if (puVar1 != (undefined8 *)0x0) {
    (**(code **)*puVar1)(puVar1,1);
  }
  return param_1;
}

