// MapHeader::save @ 140c629a0 (function MapHeader::save)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void MapHeader__save(longlong param_1,longlong param_2)

{
  undefined8 uVar1;
  ulonglong uVar2;
  longlong lVar3;
  undefined8 ****ppppuVar4;
  ulonglong uVar5;
  longlong lVar6;
  longlong lVar7;
  ulonglong uVar8;
  char *pcVar9;
  undefined8 ***local_b8;
  undefined8 uStack_b0;
  undefined8 local_a8;
  ulonglong uStack_a0;
  longlong local_98;
  longlong local_90;
  char local_74 [24];
  char local_5c [24];
  char local_44 [28];
  
  uVar2 = _UNK_142403a58;
  uVar1 = _DAT_142403a50;
  pcVar9 = local_74 + 1;
  uVar8 = *(ulonglong *)(param_1 + 0x10);
  do {
    pcVar9 = pcVar9 + -1;
    uVar5 = uVar8 / 10;
    *pcVar9 = (char)uVar8 + (char)uVar5 * -10 + '0';
    uVar8 = uVar5;
  } while (uVar5 != 0);
  local_b8 = (undefined8 ****)0x0;
  uStack_b0 = 0;
  local_a8 = 0;
  uStack_a0 = 0;
  if (pcVar9 == local_74 + 1) {
    local_a8 = _DAT_142403a50;
    uStack_a0 = _UNK_142403a58;
    local_b8 = (undefined8 ****)0x0;
  }
  else {
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Construct<1,char_*>
              (&local_b8,pcVar9,local_74 + (1 - (longlong)pcVar9));
  }
  ppppuVar4 = &local_b8;
  if (0xf < uStack_a0) {
    ppppuVar4 = (undefined8 ****)local_b8;
  }
  Serialiser__TagWriter__TagWriter(&local_98,param_2,"update-tick","value=%s",ppppuVar4);
  if (0xf < uStack_a0) {
    if (0xfff < uStack_a0 + 1) {
      if (0x1f < (ulonglong)((longlong)local_b8 + (-8 - (longlong)local_b8[-1]))) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
    }
    operator_delete__();
  }
  local_a8 = uVar1;
  uStack_a0 = uVar2;
  local_b8 = (undefined8 ***)((ulonglong)local_b8 & 0xffffffffffffff00);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x10,8);
  lVar3 = local_90;
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  lVar6 = -1;
  if (*(char *)(local_90 + 8) != '\0') {
    (**(code **)(**(longlong **)(local_90 + 0x50) + 8))
              (*(longlong **)(local_90 + 0x50),&DAT_14230f710,2);
    *(longlong *)(lVar3 + 0x10) = *(longlong *)(lVar3 + 0x10) + 2;
    lVar7 = -1;
    do {
      lVar7 = lVar7 + 1;
    } while (*(char *)(local_98 + lVar7) != '\0');
    (**(code **)(**(longlong **)(lVar3 + 0x50) + 8))(*(longlong **)(lVar3 + 0x50),local_98,lVar7);
    *(longlong *)(lVar3 + 0x10) = *(longlong *)(lVar3 + 0x10) + lVar7;
    (**(code **)(**(longlong **)(lVar3 + 0x50) + 8))(*(longlong **)(lVar3 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(lVar3 + 0x10) = *(longlong *)(lVar3 + 0x10) + 1;
  }
  pcVar9 = local_5c + 1;
  uVar8 = *(ulonglong *)(param_1 + 0x18);
  do {
    pcVar9 = pcVar9 + -1;
    uVar5 = uVar8 / 10;
    *pcVar9 = (char)uVar8 + (char)uVar5 * -10 + '0';
    uVar8 = uVar5;
  } while (uVar5 != 0);
  local_b8 = (undefined8 ****)0x0;
  uStack_b0 = 0;
  local_a8 = 0;
  uStack_a0 = 0;
  if (pcVar9 == local_5c + 1) {
    local_a8 = uVar1;
    uStack_a0 = uVar2;
    local_b8 = (undefined8 ****)0x0;
  }
  else {
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Construct<1,char_*>
              (&local_b8,pcVar9,local_5c + (1 - (longlong)pcVar9));
  }
  ppppuVar4 = &local_b8;
  if (0xf < uStack_a0) {
    ppppuVar4 = (undefined8 ****)local_b8;
  }
  Serialiser__TagWriter__TagWriter(&local_98,param_2,"entity-tick","value=%s",ppppuVar4);
  if (0xf < uStack_a0) {
    if (0xfff < uStack_a0 + 1) {
      if (0x1f < (ulonglong)((longlong)local_b8 + (-8 - (longlong)local_b8[-1]))) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
    }
    operator_delete__();
  }
  local_a8 = uVar1;
  uStack_a0 = uVar2;
  local_b8 = (undefined8 ***)((ulonglong)local_b8 & 0xffffffffffffff00);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x18,8);
  lVar3 = local_90;
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*(char *)(local_90 + 8) != '\0') {
    (**(code **)(**(longlong **)(local_90 + 0x50) + 8))
              (*(longlong **)(local_90 + 0x50),&DAT_14230f710,2);
    *(longlong *)(lVar3 + 0x10) = *(longlong *)(lVar3 + 0x10) + 2;
    lVar7 = -1;
    do {
      lVar7 = lVar7 + 1;
    } while (*(char *)(local_98 + lVar7) != '\0');
    (**(code **)(**(longlong **)(lVar3 + 0x50) + 8))(*(longlong **)(lVar3 + 0x50),local_98,lVar7);
    *(longlong *)(lVar3 + 0x10) = *(longlong *)(lVar3 + 0x10) + lVar7;
    (**(code **)(**(longlong **)(lVar3 + 0x50) + 8))(*(longlong **)(lVar3 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(lVar3 + 0x10) = *(longlong *)(lVar3 + 0x10) + 1;
  }
  pcVar9 = local_44 + 1;
  uVar8 = *(ulonglong *)(param_1 + 0x20);
  do {
    pcVar9 = pcVar9 + -1;
    uVar5 = uVar8 / 10;
    *pcVar9 = (char)uVar8 + (char)uVar5 * -10 + '0';
    uVar8 = uVar5;
  } while (uVar5 != 0);
  local_b8 = (undefined8 ****)0x0;
  uStack_b0 = 0;
  local_a8 = 0;
  uStack_a0 = 0;
  if (pcVar9 == local_44 + 1) {
    local_a8 = uVar1;
    uStack_a0 = uVar2;
    local_b8 = (undefined8 ****)0x0;
  }
  else {
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Construct<1,char_*>
              (&local_b8,pcVar9,local_44 + (1 - (longlong)pcVar9));
  }
  ppppuVar4 = &local_b8;
  if (0xf < uStack_a0) {
    ppppuVar4 = (undefined8 ****)local_b8;
  }
  Serialiser__TagWriter__TagWriter(&local_98,param_2,"ticks-played","value=%s",ppppuVar4);
  if (0xf < uStack_a0) {
    if (0xfff < uStack_a0 + 1) {
      if (0x1f < (ulonglong)((longlong)local_b8 + (-8 - (longlong)local_b8[-1]))) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
    }
    operator_delete__();
  }
  local_a8 = uVar1;
  uStack_a0 = uVar2;
  local_b8 = (undefined8 ***)((ulonglong)local_b8 & 0xffffffffffffff00);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x20,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*(char *)(local_90 + 8) != '\0') {
    (**(code **)(**(longlong **)(local_90 + 0x50) + 8))
              (*(longlong **)(local_90 + 0x50),&DAT_14230f710,2);
    *(longlong *)(local_90 + 0x10) = *(longlong *)(local_90 + 0x10) + 2;
    do {
      lVar6 = lVar6 + 1;
    } while (*(char *)(local_98 + lVar6) != '\0');
    (**(code **)(**(longlong **)(local_90 + 0x50) + 8))
              (*(longlong **)(local_90 + 0x50),local_98,lVar6);
    *(longlong *)(local_90 + 0x10) = *(longlong *)(local_90 + 0x10) + lVar6;
    (**(code **)(**(longlong **)(local_90 + 0x50) + 8))
              (*(longlong **)(local_90 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(local_90 + 0x10) = *(longlong *)(local_90 + 0x10) + 1;
  }
  return;
}

