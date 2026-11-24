// luaopen_math @ 141d2db90 (function luaopen_math)

undefined8 luaopen_math(longlong param_1)

{
  longlong lVar1;
  undefined8 *puVar2;
  undefined *puVar3;
  longlong *plVar4;
  int iVar5;
  undefined8 uVar6;
  undefined **ppuVar7;
  longlong lVar8;
  
  lVar8 = *(longlong *)(param_1 + 0x20);
  lVar1 = *(longlong *)(lVar8 + 0x18);
  if (0 < lVar1) {
    if ((*(char *)(lVar8 + 0x57) == '\0') || (*(char *)(lVar8 + 0x58) != '\0')) {
      *(undefined8 *)(lVar8 + 0x18) = 0xfffffffffffff380;
      *(longlong *)(lVar8 + 0x10) = *(longlong *)(lVar8 + 0x10) + lVar1 + 0xc80;
    }
    else {
      luaC_forcestep(param_1);
    }
  }
  uVar6 = luaH_new(param_1);
  checkstack_locked(param_1,1);
  puVar2 = *(undefined8 **)(param_1 + 0x18);
  *puVar2 = uVar6;
  *(undefined4 *)(puVar2 + 1) = 0x45;
  *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
  if (*(ulonglong *)(param_1 + 0x18) <= *(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8)) {
    luaH_resize(param_1,uVar6,0,0x1c);
    ppuVar7 = &PTR_DAT_1423c4a60;
    luaL_checkversion_(param_1,DAT_142402d70);
    iVar5 = checkstack_locked(param_1,0x14);
    if (iVar5 == 0) {
                    /* WARNING: Subroutine does not return */
      luaL_error(param_1,"stack overflow (%s)","too many upvalues");
    }
    do {
      puVar3 = ppuVar7[1];
      checkstack_locked(param_1,1);
      plVar4 = *(longlong **)(param_1 + 0x18);
      *plVar4 = (longlong)puVar3;
      *(undefined4 *)(plVar4 + 1) = 0x16;
      *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
      if (*(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8) < *(ulonglong *)(param_1 + 0x18))
      goto LAB_141d2dd7f;
      lVar8 = -1;
      do {
        lVar8 = lVar8 + 1;
      } while ((*ppuVar7)[lVar8] != '\0');
      lua_setlfield(param_1,0xfffffffe);
      ppuVar7 = ppuVar7 + 2;
    } while (*ppuVar7 != (undefined *)0x0);
    lua_settop(param_1,0xffffffff);
    checkstack_locked(param_1,1);
    puVar2 = *(undefined8 **)(param_1 + 0x18);
    *puVar2 = 0x400921fb54442d18;
    *(undefined4 *)(puVar2 + 1) = 3;
    *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
    if (*(ulonglong *)(param_1 + 0x18) <= *(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8)) {
      lua_setlfield(param_1,0xfffffffe,&DAT_1422a5134,2);
      checkstack_locked(param_1,1);
      puVar2 = *(undefined8 **)(param_1 + 0x18);
      *puVar2 = 0x7ff0000000000000;
      *(undefined4 *)(puVar2 + 1) = 3;
      *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
      if (*(ulonglong *)(param_1 + 0x18) <= *(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8)) {
        lua_setlfield(param_1,0xfffffffe,&DAT_1423c49c0,4);
        return 1;
      }
    }
  }
LAB_141d2dd7f:
                    /* WARNING: Subroutine does not return */
  lua_traceandabort("L->top <= L->ci->top stack overflow");
}

