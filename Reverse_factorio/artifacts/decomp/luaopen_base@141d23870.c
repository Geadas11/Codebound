// luaopen_base @ 141d23870 (function luaopen_base)

undefined8 luaopen_base(longlong param_1)

{
  undefined *puVar1;
  longlong *plVar2;
  int iVar3;
  undefined **ppuVar4;
  longlong lVar5;
  
  lua_rawgeti(param_1,0xfff0b9d8,2);
  lua_rawgeti(param_1,0xfff0b9d8,2);
  lua_setlfield(param_1,0xfffffffe,&DAT_1421fe2b0,2);
  ppuVar4 = &PTR_s_assert_1423c3e30;
  luaL_checkversion_(param_1,DAT_142402d70);
  iVar3 = checkstack_locked(param_1,0x14);
  if (iVar3 == 0) {
                    /* WARNING: Subroutine does not return */
    luaL_error(param_1,"stack overflow (%s)","too many upvalues");
  }
  do {
    puVar1 = ppuVar4[1];
    checkstack_locked(param_1,1);
    plVar2 = *(longlong **)(param_1 + 0x18);
    *plVar2 = (longlong)puVar1;
    *(undefined4 *)(plVar2 + 1) = 0x16;
    *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x10;
    if (*(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8) < *(ulonglong *)(param_1 + 0x18)) {
                    /* WARNING: Subroutine does not return */
      lua_traceandabort("L->top <= L->ci->top stack overflow");
    }
    lVar5 = -1;
    do {
      lVar5 = lVar5 + 1;
    } while ((*ppuVar4)[lVar5] != '\0');
    lua_setlfield(param_1,0xfffffffe);
    ppuVar4 = ppuVar4 + 2;
  } while (*ppuVar4 != (undefined *)0x0);
  lua_settop(param_1,0xffffffff);
  lua_pushlstring(param_1,"Lua 5.2",7);
  lua_setlfield(param_1,0xfffffffe,"_VERSION",8);
  return 1;
}

