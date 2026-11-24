// math_random @ 141d2da30 (function math_random)

undefined8 math_random(longlong param_1)

{
  int iVar1;
  undefined4 uVar2;
  double dVar3;
  undefined4 uVar5;
  double dVar4;
  double dVar6;
  
  iVar1 = rand();
  dVar6 = (double)(iVar1 % 0x7fff) / DAT_142402ea0;
  iVar1 = (int)((*(longlong *)(param_1 + 0x18) - **(longlong **)(param_1 + 0x28)) + -0x10 >> 4);
  if (iVar1 != 0) {
    if (iVar1 == 1) {
      dVar3 = (double)luaL_checknumber(param_1,1);
      uVar2 = SUB84(DAT_1424027a8,0);
      uVar5 = (undefined4)((ulonglong)DAT_1424027a8 >> 0x20);
      if (dVar3 < DAT_1424027a8) {
                    /* WARNING: Subroutine does not return */
        luaL_argerror(param_1,1,"interval is empty");
      }
    }
    else {
      if (iVar1 != 2) {
                    /* WARNING: Subroutine does not return */
        luaL_error(param_1,"wrong number of arguments");
      }
      dVar3 = (double)luaL_checknumber(param_1,1);
      uVar2 = SUB84(dVar3,0);
      uVar5 = (undefined4)((ulonglong)dVar3 >> 0x20);
      dVar4 = (double)luaL_checknumber(param_1,2);
      if (dVar4 < dVar3) {
                    /* WARNING: Subroutine does not return */
        luaL_argerror(param_1,2,"interval is empty");
      }
      dVar3 = (dVar4 - dVar3) + DAT_1424027a8;
    }
    dVar6 = floor(dVar3 * dVar6);
    dVar6 = dVar6 + (double)CONCAT44(uVar5,uVar2);
  }
  lua_pushnumber(param_1,SUB84(dVar6,0));
  return 1;
}

