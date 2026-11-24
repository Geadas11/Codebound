// luaH_getn @ 141d14f40 (function luaH_getn)

ulonglong luaH_getn(longlong param_1)

{
  ulonglong uVar1;
  uint uVar2;
  uint uVar3;
  
  uVar3 = *(uint *)(param_1 + 0x40);
  if ((uVar3 != 0) &&
     (*(int *)(*(longlong *)(param_1 + 0x20) + 8 + (ulonglong)(uVar3 - 1) * 0x10) == 0)) {
    uVar1 = 0;
    if (1 < uVar3) {
      do {
        uVar2 = (int)uVar1 + uVar3 >> 1;
        if (*(int *)(*(longlong *)(param_1 + 0x20) + 8 + (ulonglong)(uVar2 - 1) * 0x10) != 0) {
          uVar1 = (ulonglong)uVar2;
          uVar2 = uVar3;
        }
        uVar3 = uVar2;
      } while (1 < uVar3 - (int)uVar1);
      return uVar1;
    }
    return 0;
  }
  if (*(undefined **)(param_1 + 0x28) == &DAT_1423c2f90) {
    return (ulonglong)uVar3;
  }
  uVar1 = unbound_search(param_1,uVar3);
  return uVar1;
}

