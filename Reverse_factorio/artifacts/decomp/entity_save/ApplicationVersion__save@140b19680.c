// ApplicationVersion::save @ 140b19680 (function ApplicationVersion::save)

void ApplicationVersion__save(undefined2 *param_1,undefined8 param_2,char *param_3)

{
  uint uVar1;
  char *pcVar2;
  undefined8 uVar3;
  double *pdVar4;
  longlong *_Dest;
  void *pvVar5;
  undefined4 *puVar6;
  undefined8 in_stack_ffffffffffffff88;
  undefined4 uVar7;
  char *local_68;
  longlong lStack_60;
  longlong local_58 [3];
  ulonglong local_40;
  undefined1 local_38 [32];
  
  uVar7 = (undefined4)((ulonglong)in_stack_ffffffffffffff88 >> 0x20);
  pcVar2 = (char *)std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__basic_string<char,std::char_traits<char>,std::allocator<char>_>
                             (local_38,param_3);
  uVar3 = param_2;
  if (*(longlong *)(pcVar2 + 0x10) != 0) {
    local_68 = pcVar2;
    if (0xf < *(ulonglong *)(pcVar2 + 0x18)) {
      local_68 = *(char **)pcVar2;
    }
    lStack_60 = *(longlong *)(pcVar2 + 0x10);
    uVar3 = PropertyTree__operator__(param_2,&local_68);
  }
  ssprintf(local_58,"%u.%u.%u",*param_1,param_1[1],CONCAT44(uVar7,(uint)(ushort)param_1[2]));
  local_68 = "game_version";
  lStack_60 = 0xc;
  uVar3 = PropertyTree__operator__(uVar3,&local_68);
  PropertyTree__clear(uVar3);
  PropertyTree__setInternal(uVar3,local_58);
  if (0xf < local_40) {
    if ((0xfff < local_40 + 1) && (0x1f < (local_58[0] - *(longlong *)(local_58[0] + -8)) - 8U)) {
                    /* WARNING: Subroutine does not return */
      _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
    }
    operator_delete__();
  }
  std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Tidy_deallocate(pcVar2);
  lStack_60 = *(longlong *)(param_3 + 0x10);
  if (lStack_60 != 0) {
    local_68 = param_3;
    if (0xf < *(ulonglong *)(param_3 + 0x18)) {
      local_68 = *(char **)param_3;
    }
    param_2 = PropertyTree__operator__(param_2,&local_68);
  }
  local_68 = "build_version";
  lStack_60 = 0xd;
  pdVar4 = (double *)PropertyTree__operator__(param_2,&local_68);
  PropertyTree__clear(pdVar4);
  uVar1 = *(uint *)(param_1 + 4);
  *(byte *)((longlong)pdVar4 + 0x29) = *(byte *)((longlong)pdVar4 + 0x29) & 0xc2 | 2;
  *pdVar4 = (double)uVar1;
  local_68 = "build_mode";
  lStack_60 = 10;
  _Dest = (longlong *)PropertyTree__operator__(param_2);
  PropertyTree__clear(_Dest);
  *(byte *)((longlong)_Dest + 0x29) = *(byte *)((longlong)_Dest + 0x29) & 0xc3 | 3;
  pvVar5 = memchr(&DAT_1422288b0,0,4);
  if (pvVar5 == (void *)0x0) {
    strncpy((char *)_Dest,"full",4);
    *(undefined1 *)((longlong)_Dest + 4) = 0;
    *(undefined1 *)((longlong)_Dest + 0x17) = 0;
  }
  else {
    *(undefined1 *)((longlong)_Dest + 0x17) = 1;
    puVar6 = (undefined4 *)operator_new__(5);
    *_Dest = (longlong)puVar6;
    *(undefined4 *)(_Dest + 1) = 4;
    *puVar6 = DAT_1422288b0;
    *(undefined1 *)(*_Dest + 4) = 0;
  }
  local_68 = "platform";
  lStack_60 = 8;
  uVar3 = PropertyTree__operator__(param_2,&local_68);
  PropertyTree__clear(uVar3);
  PropertyTree__setInternal(uVar3,param_1 + 8,0);
  std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Tidy_deallocate(param_3);
  return;
}

