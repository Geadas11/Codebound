// LuaState::LuaState @ 140f0ba30 (function LuaState::LuaState)

undefined8 * LuaState__LuaState(undefined8 *param_1)

{
  longlong lVar1;
  
  *param_1 = 0;
  param_1[1] = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  param_1[4] = 7;
  *(undefined2 *)(param_1 + 1) = 0;
  lVar1 = lua_newstate(l_alloc,0);
  if (lVar1 != 0) {
    *(code **)(*(longlong *)(lVar1 + 0x20) + 0x108) = panic;
  }
  param_1[5] = lVar1;
  *(undefined8 **)(lVar1 + 0xe0) = param_1;
  return param_1;
}

