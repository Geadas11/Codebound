// luaV_lessthan @ 141d17ec0 (function luaV_lessthan)

ulonglong luaV_lessthan(longlong param_1,double *param_2,double *param_3)

{
  code *pcVar1;
  uint uVar2;
  int iVar3;
  ulonglong uVar4;
  
  if ((*(int *)(param_2 + 1) == 3) && (*(int *)(param_3 + 1) == 3)) {
    return (ulonglong)(*param_2 <= *param_3 && *param_3 != *param_2);
  }
  if ((((byte)*(int *)(param_2 + 1) & 0xf) == 4) &&
     (((byte)*(undefined4 *)(param_3 + 1) & 0xf) == 4)) {
    uVar2 = l_strcmp(*param_2,*param_3);
    return (ulonglong)(uVar2 >> 0x1f);
  }
  iVar3 = call_binTM(param_1,param_2,param_3,*(undefined8 *)(param_1 + 0x18),0xd);
  if (iVar3 == 0) {
    luaG_ordererror(param_1,param_2,param_3);
    pcVar1 = (code *)swi(3);
    uVar4 = (*pcVar1)();
    return uVar4;
  }
  iVar3 = (*(int **)(param_1 + 0x18))[2];
  if ((iVar3 != 0) && ((iVar3 != 1 || (**(int **)(param_1 + 0x18) != 0)))) {
    return 1;
  }
  return 0;
}

