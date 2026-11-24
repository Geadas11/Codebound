// lua_next @ 141d10250 (function lua_next)

int lua_next(longlong param_1,undefined4 param_2)

{
  ulonglong uVar1;
  int iVar2;
  undefined8 *puVar3;
  
                    /* 0x1d10250  90  lua_next */
  checkstack_locked(param_1,1);
  puVar3 = (undefined8 *)index2addr(param_1,param_2);
  if (*(int *)(puVar3 + 1) != 0x45) {
                    /* WARNING: Subroutine does not return */
    lua_traceandabort("((((t))->tt_) == (((5) | (1 << 6)))) table expected");
  }
  iVar2 = luaH_next(param_1,*puVar3,*(longlong *)(param_1 + 0x18) + -0x10);
  if (iVar2 == 0) {
    *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + -0x10;
  }
  else {
    uVar1 = *(longlong *)(param_1 + 0x18) + 0x10;
    *(ulonglong *)(param_1 + 0x18) = uVar1;
    if (*(ulonglong *)(*(longlong *)(param_1 + 0x28) + 8) < uVar1) {
                    /* WARNING: Subroutine does not return */
      lua_traceandabort("L->top <= L->ci->top stack overflow");
    }
  }
  return iVar2;
}

