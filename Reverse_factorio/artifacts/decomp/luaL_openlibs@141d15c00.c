// luaL_openlibs @ 141d15c00 (function luaL_openlibs)

void luaL_openlibs(longlong param_1,int param_2)

{
  longlong lVar1;
  longlong lVar2;
  undefined8 *puVar3;
  undefined *puVar4;
  undefined8 uVar5;
  undefined **ppuVar6;
  code *pcVar7;
  
                    /* 0x1d15c00  34  luaL_openlibs */
  ppuVar6 = &PTR_luaopen_base_1423c3008;
  puVar4 = PTR_luaopen_base_1423c3008;
  do {
    luaL_requiref(param_1,ppuVar6[-1],puVar4,1);
    if ((longlong)
        ((*(longlong *)(param_1 + 0x18) - **(longlong **)(param_1 + 0x28)) - 0x10U &
        0xfffffffffffffff0) < 0x10) goto LAB_141d15ded;
    ppuVar6 = ppuVar6 + 2;
    *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + -0x10;
    puVar4 = *ppuVar6;
  } while (puVar4 != (undefined *)0x0);
  pcVar7 = luaopen_fulldebug;
  if (param_2 == 0) {
    pcVar7 = luaopen_partialdebug;
  }
  luaL_requiref(param_1,"debug",pcVar7,1);
  if (0xf < (longlong)
            ((*(longlong *)(param_1 + 0x18) - **(longlong **)(param_1 + 0x28)) - 0x10U &
            0xfffffffffffffff0)) {
    *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + -0x10;
    lua_getlfield(param_1,0xfff0b9d8,"_PRELOAD",8);
    lVar1 = *(longlong *)(param_1 + 0x18);
    if ((longlong)((lVar1 - **(longlong **)(param_1 + 0x28)) - 0x10U & 0xfffffffffffffff0) < 0x10) {
                    /* WARNING: Subroutine does not return */
      lua_traceandabort("idx != 0 && -idx <= L->top - (ci->func + 1) invalid index");
    }
    if (((undefined8 *)(lVar1 + -0x10) == &DAT_1423c31f0) ||
       (((byte)*(undefined4 *)(lVar1 + -8) & 0xf) != 5)) {
      *(undefined8 **)(param_1 + 0x18) = (undefined8 *)(lVar1 + -0x10);
      lVar1 = *(longlong *)(param_1 + 0x20);
      lVar2 = *(longlong *)(lVar1 + 0x18);
      if (0 < lVar2) {
        if ((*(char *)(lVar1 + 0x57) == '\0') || (*(char *)(lVar1 + 0x58) != '\0')) {
          *(undefined8 *)(lVar1 + 0x18) = 0xfffffffffffff380;
          *(longlong *)(lVar1 + 0x10) = *(longlong *)(lVar1 + 0x10) + lVar2 + 0xc80;
        }
        else {
          luaC_forcestep(param_1);
        }
      }
      uVar5 = luaH_new(param_1);
      checkstack_locked(param_1,1);
      puVar3 = *(undefined8 **)(param_1 + 0x18);
      *puVar3 = uVar5;
      *(undefined4 *)(puVar3 + 1) = 0x45;
      *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
      if (*(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8) < *(ulonglong *)(param_1 + 0x18)) {
                    /* WARNING: Subroutine does not return */
        lua_traceandabort("L->top <= L->ci->top stack overflow");
      }
      lua_pushvalue(param_1,0xffffffff);
      lua_setlfield(param_1,0xfff0b9d8,"_PRELOAD",8);
    }
    if (0xf < (longlong)
              ((*(longlong *)(param_1 + 0x18) - **(longlong **)(param_1 + 0x28)) - 0x10U &
              0xfffffffffffffff0)) {
      *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + -0x10;
      return;
    }
  }
LAB_141d15ded:
                    /* WARNING: Subroutine does not return */
  lua_traceandabort("-(idx+1) <= (L->top - (func + 1)) invalid new top");
}

