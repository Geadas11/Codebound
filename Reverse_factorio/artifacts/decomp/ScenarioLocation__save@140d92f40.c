// ScenarioLocation::save @ 140d92f40 (function ScenarioLocation::save)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void ScenarioLocation__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong *plVar2;
  undefined8 *puVar3;
  longlong *plVar4;
  undefined8 local_res8;
  longlong local_48;
  undefined8 uStack_40;
  undefined8 local_38;
  ulonglong uStack_30;
  
  puVar3 = (undefined8 *)(param_2 + 0x50);
  plVar4 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e010c,1);
    *plVar4 = *plVar4 + 1;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"scenario-location",0x11);
    *plVar4 = *plVar4 + 0x11;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108);
    *plVar4 = *plVar4 + 1;
  }
  if (*(int *)(param_1 + 0x60) == 2) {
    local_38 = _DAT_142403b00;
    uStack_30 = _UNK_142403b08;
    local_48._0_1_ = s___tutorials_1421db028[0];
    local_48._1_1_ = s___tutorials_1421db028[1];
    local_48._2_1_ = s___tutorials_1421db028[2];
    local_48._3_1_ = s___tutorials_1421db028[3];
    local_48._4_1_ = s___tutorials_1421db028[4];
    local_48._5_1_ = s___tutorials_1421db028[5];
    local_48._6_1_ = s___tutorials_1421db028[6];
    local_48._7_1_ = s___tutorials_1421db028[7];
    uStack_40 = (ulonglong)CONCAT12(s___tutorials_1421db028[10],s___tutorials_1421db028._8_2_);
    MapSerialiser__operator<<<SizeLimitedString<500>_>(param_2,&local_48);
    if (0xf < uStack_30) {
      if ((0xfff < uStack_30 + 1) && (0x1f < (local_48 - *(longlong *)(local_48 + -8)) - 8U)) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
      operator_delete__();
    }
  }
  else {
    MapSerialiser__operator<<<SizeLimitedString<500>_>(param_2,param_1);
  }
  puVar3 = (undefined8 *)(param_2 + 0x50);
  plVar2 = (longlong *)(param_2 + 0x10);
  plVar4 = (longlong *)(param_1 + 0x20);
  local_res8 = *(undefined8 *)(param_1 + 0x30);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x30);
  if (0xf < *(ulonglong *)(param_1 + 0x38)) {
    plVar4 = (longlong *)*plVar4;
  }
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,plVar4,lVar1);
  *plVar2 = *plVar2 + lVar1;
  plVar4 = (longlong *)(param_1 + 0x40);
  local_res8 = *(undefined8 *)(param_1 + 0x50);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x50);
  if (0xf < *(ulonglong *)(param_1 + 0x58)) {
    plVar4 = (longlong *)*plVar4;
  }
  (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,plVar4,lVar1);
  *plVar2 = *plVar2 + lVar1;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
    *plVar2 = *plVar2 + 2;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"scenario-location",0x11);
    *plVar2 = *plVar2 + 0x11;
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  return;
}

