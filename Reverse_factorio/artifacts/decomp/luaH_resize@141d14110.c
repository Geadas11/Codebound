// luaH_resize @ 141d14110 (function luaH_resize)

void luaH_resize(longlong param_1,longlong param_2,int param_3,undefined4 param_4)

{
  byte bVar1;
  int iVar2;
  undefined8 *puVar3;
  undefined *puVar4;
  int iVar5;
  undefined8 uVar6;
  longlong lVar7;
  ulonglong uVar8;
  undefined8 *puVar9;
  uint uVar10;
  longlong lVar11;
  
  bVar1 = *(byte *)(param_2 + 0x13);
  iVar2 = *(int *)(param_2 + 0x40);
  puVar3 = *(undefined8 **)(param_2 + 0x48);
  puVar4 = *(undefined **)(param_2 + 0x28);
  iVar5 = 0x400;
  if (param_3 < 0x401) {
    iVar5 = param_3;
  }
  if (iVar2 < iVar5) {
    if (0xfffffffffffffff < (ulonglong)(longlong)(iVar5 + 1)) {
LAB_141d14346:
                    /* WARNING: Subroutine does not return */
      luaM_toobig(param_1);
    }
    lVar11 = (longlong)iVar5;
    uVar6 = luaM_realloc_(param_1,*(undefined8 *)(param_2 + 0x20),(longlong)iVar2 << 4,lVar11 << 4);
    *(undefined8 *)(param_2 + 0x20) = uVar6;
    lVar7 = (longlong)*(int *)(param_2 + 0x40);
    if (lVar7 < lVar11) {
      lVar11 = lVar11 - lVar7;
      lVar7 = lVar7 << 4;
      do {
        *(undefined4 *)(lVar7 + 8 + *(longlong *)(param_2 + 0x20)) = 0;
        lVar11 = lVar11 + -1;
        lVar7 = lVar7 + 0x10;
      } while (lVar11 != 0);
    }
    *(int *)(param_2 + 0x40) = iVar5;
    setnodevector(param_1,param_2,param_4);
  }
  else {
    setnodevector(param_1,param_2,param_4);
    if (iVar5 < iVar2) {
      uVar8 = (ulonglong)iVar5;
      *(int *)(param_2 + 0x40) = iVar5;
      if ((longlong)uVar8 < (longlong)iVar2) {
        lVar7 = uVar8 << 4;
        lVar11 = (longlong)iVar2 - uVar8;
        do {
          uVar10 = (int)uVar8 + 1;
          if (*(int *)(lVar7 + 8 + *(longlong *)(param_2 + 0x20)) != 0) {
            luaH_setint(param_1,param_2,uVar10,
                        (longlong)(int)uVar8 * 0x10 + *(longlong *)(param_2 + 0x20));
          }
          lVar7 = lVar7 + 0x10;
          uVar8 = (ulonglong)uVar10;
          lVar11 = lVar11 + -1;
        } while (lVar11 != 0);
      }
      if (0xfffffffffffffff < (ulonglong)(longlong)(iVar5 + 1)) goto LAB_141d14346;
      uVar6 = luaM_realloc_(param_1,*(undefined8 *)(param_2 + 0x20),(longlong)iVar2 << 4,
                            (longlong)iVar5 << 4);
      *(undefined8 *)(param_2 + 0x20) = uVar6;
    }
  }
  *(undefined8 *)(param_2 + 0x48) = 0;
  *(undefined8 *)(param_2 + 0x50) = 0;
  for (; puVar3 != (undefined8 *)0x0; puVar3 = (undefined8 *)puVar3[5]) {
    if (*(int *)(puVar3 + 1) != 0) {
      puVar9 = (undefined8 *)luaH_get(param_1,param_2,puVar3 + 2);
      if (puVar9 == &DAT_1423c31f0) {
        puVar9 = (undefined8 *)luaH_newkey(param_1,param_2,puVar3 + 2);
      }
      *puVar9 = *puVar3;
      *(undefined4 *)(puVar9 + 1) = *(undefined4 *)(puVar3 + 1);
    }
  }
  if (puVar4 != &DAT_1423c2f90) {
    puVar3 = *(undefined8 **)(param_1 + 0x20);
    lVar11 = (longlong)(1 << (bVar1 & 0x1f)) * 0x38;
    (*(code *)*puVar3)(puVar3[1],puVar4,lVar11,0);
    lVar7 = 0;
    if (puVar4 != (undefined *)0x0) {
      lVar7 = lVar11;
    }
    puVar3[3] = puVar3[3] - lVar7;
  }
  return;
}

