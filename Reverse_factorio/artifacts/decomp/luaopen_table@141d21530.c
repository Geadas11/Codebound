// luaopen_table @ 141d21530 (function luaopen_table)

undefined8 luaopen_table(longlong param_1)

{
  undefined8 *puVar1;
  undefined *puVar2;
  longlong *plVar3;
  int iVar4;
  undefined8 uVar5;
  longlong lVar6;
  undefined **ppuVar7;
  longlong lVar8;
  
  lVar8 = *(longlong *)(param_1 + 0x20);
  lVar6 = *(longlong *)(lVar8 + 0x18);
  if (0 < lVar6) {
    if ((*(char *)(lVar8 + 0x57) == '\0') || (*(char *)(lVar8 + 0x58) != '\0')) {
      *(undefined8 *)(lVar8 + 0x18) = 0xfffffffffffff380;
      *(longlong *)(lVar8 + 0x10) = *(longlong *)(lVar8 + 0x10) + lVar6 + 0xc80;
    }
    else {
      luaC_forcestep(param_1);
    }
  }
  uVar5 = luaH_new(param_1);
  checkstack_locked(param_1,1);
  puVar1 = *(undefined8 **)(param_1 + 0x18);
  *puVar1 = uVar5;
  *(undefined4 *)(puVar1 + 1) = 0x45;
  *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
  if (*(ulonglong *)(param_1 + 0x18) <= *(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8)) {
    luaH_resize(param_1,uVar5,0,8);
    ppuVar7 = &PTR_s_concat_1423c3ab0;
    luaL_checkversion_(param_1,DAT_142402d70);
    iVar4 = checkstack_locked(param_1,0x14);
    if (iVar4 == 0) {
                    /* WARNING: Subroutine does not return */
      luaL_error(param_1,"stack overflow (%s)","too many upvalues");
    }
    while( true ) {
      puVar2 = ppuVar7[1];
      checkstack_locked(param_1,1);
      plVar3 = *(longlong **)(param_1 + 0x18);
      *plVar3 = (longlong)puVar2;
      *(undefined4 *)(plVar3 + 1) = 0x16;
      *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
      if (*(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8) < *(ulonglong *)(param_1 + 0x18)) break;
      lVar8 = -1;
      do {
        lVar8 = lVar8 + 1;
      } while ((*ppuVar7)[lVar8] != '\0');
      lua_setlfield(param_1,0xfffffffe);
      ppuVar7 = ppuVar7 + 2;
      if (*ppuVar7 == (undefined *)0x0) {
        lua_settop(param_1,0xffffffff);
        lua_getlfield(param_1,0xffffffff,"unpack",6);
        lVar8 = *(longlong *)(*(longlong *)(param_1 + 0x20) + 0x40);
        if (0x10 < (longlong)
                   (*(longlong *)(param_1 + 0x18) - **(longlong **)(param_1 + 0x28) &
                   0xfffffffffffffff0U)) {
          if (*(int *)(lVar8 + 0x40) < 2) {
            rehash(param_1,lVar8,2);
          }
          lVar8 = *(longlong *)(lVar8 + 0x20);
          checkstack_locked(param_1,1);
          plVar3 = *(longlong **)(param_1 + 0x18);
          *(longlong **)(param_1 + 0x18) = plVar3 + 2;
          lVar6 = luaS_newlstr(param_1,"unpack",6);
          *plVar3 = lVar6;
          *(uint *)(plVar3 + 1) = *(byte *)(lVar6 + 0x10) | 0x40;
          luaV_settable(param_1,lVar8 + 0x10,*(longlong *)(param_1 + 0x18) + -0x10,
                        *(longlong *)(param_1 + 0x18) + -0x20);
          *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + -0x20;
          return 1;
        }
                    /* WARNING: Subroutine does not return */
        lua_traceandabort("(1) < (L->top - L->ci->func) not enough elements in the stack");
      }
    }
  }
                    /* WARNING: Subroutine does not return */
  lua_traceandabort("L->top <= L->ci->top stack overflow");
}

