// LuaState::LuaState @ 140037ac0 (function LuaState::LuaState)

undefined8 * LuaState__LuaState(undefined8 *param_1,undefined8 *param_2)

{
  undefined8 *puVar1;
  undefined8 uVar2;
  undefined8 *puVar3;
  
  *param_1 = 0;
  puVar1 = param_1 + 1;
  *puVar1 = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  param_1[4] = 7;
  *(undefined2 *)puVar1 = 0;
  param_1[5] = 0;
  if (param_1 != param_2) {
    *param_1 = *param_2;
    puVar3 = param_2 + 1;
    if (puVar1 != puVar3) {
      if (7 < (ulonglong)param_1[4]) {
        std__basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t>_>___Deallocate_for_capacity
                  (puVar1,*puVar1);
      }
      param_1[3] = 0;
      param_1[4] = 7;
      *(undefined2 *)puVar1 = 0;
      uVar2 = param_2[2];
      *puVar1 = *puVar3;
      param_1[2] = uVar2;
      uVar2 = param_2[4];
      param_1[3] = param_2[3];
      param_1[4] = uVar2;
      param_2[3] = 0;
      param_2[4] = 7;
      *(undefined2 *)puVar3 = 0;
    }
    param_1[5] = param_2[5];
    param_2[5] = 0;
    *param_2 = 0;
    param_2[3] = 0;
    if (7 < (ulonglong)param_2[4]) {
      puVar3 = (undefined8 *)*puVar3;
    }
    *(undefined2 *)puVar3 = 0;
    if (param_1[5] != 0) {
      *(undefined8 **)(param_1[5] + 0xe0) = param_1;
    }
  }
  return param_1;
}

