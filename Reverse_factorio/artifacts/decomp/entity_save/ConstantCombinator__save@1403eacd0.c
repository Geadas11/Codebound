// ConstantCombinator::save @ 1403eacd0 (function ConstantCombinator::save)

void ConstantCombinator__save
               (longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  longlong *plVar2;
  longlong lVar3;
  undefined8 *puVar4;
  undefined8 local_res8;
  
  EntityWithOwner__save();
  puVar1 = (undefined8 *)(param_2 + 0x50);
  plVar2 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))
              ((longlong *)*puVar1,&DAT_1421e010c,1,param_4,"constant-combinator-control-behavior",
               param_2);
    *plVar2 = *plVar2 + 1;
    (**(code **)(*(longlong *)*puVar1 + 8))
              ((longlong *)*puVar1,"constant-combinator-control-behavior",0x24);
    *plVar2 = *plVar2 + 0x24;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  CircuitConnector__save(param_1 + 200,param_2);
  MapSerialiser__operator<<<CompiledLogisticFilters>(param_2,param_1 + 0x210);
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x260,1);
  *plVar2 = *plVar2 + 1;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_14230f710,2);
    *plVar2 = *plVar2 + 2;
    (**(code **)(*(longlong *)*puVar1 + 8))
              ((longlong *)*puVar1,"constant-combinator-control-behavior",0x24);
    *plVar2 = *plVar2 + 0x24;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar2 = *plVar2 + 1;
  }
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x268,1);
  *plVar2 = *plVar2 + 1;
  puVar4 = (undefined8 *)(param_1 + 0x270);
  local_res8 = *(undefined8 *)(param_1 + 0x280);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar3 = *(longlong *)(param_1 + 0x280);
  if (0xf < *(ulonglong *)(param_1 + 0x288)) {
    puVar4 = (undefined8 *)*puVar4;
  }
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,puVar4,lVar3);
  *plVar2 = *plVar2 + lVar3;
  return;
}

