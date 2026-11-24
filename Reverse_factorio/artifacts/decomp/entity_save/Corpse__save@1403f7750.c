// Corpse::save @ 1403f7750 (function Corpse::save)

void Corpse__save(longlong param_1,longlong param_2)

{
  byte bVar1;
  byte local_res8 [8];
  
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xa8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xac,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb0,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb4,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb5,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb6,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb7,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if ((((*(char *)(param_1 + 0xc0) == '\0') && (*(char *)(param_1 + 0xc1) == '\0')) &&
      (*(char *)(param_1 + 0xc2) == '\0')) && (*(char *)(param_1 + 0xc3) == '\0')) {
    bVar1 = *(char *)(param_1 + 0xc4) != '\0';
  }
  else {
    bVar1 = *(char *)(param_1 + 0xc4) != '\0' | 2;
  }
  if (*(char *)(param_1 + 0xc5) != '\0') {
    bVar1 = bVar1 | 4;
  }
  local_res8[0] = bVar1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if ((bVar1 & 2) != 0) {
    MapSerialiser__operator<<<ByteColor>(param_2,param_1 + 0xc0);
  }
  return;
}

