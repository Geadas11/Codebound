// Market::save @ 1404cdd50 (function Market::save)

void Market__save(longlong param_1,longlong param_2)

{
  undefined8 *puVar1;
  short *psVar2;
  short *psVar3;
  undefined8 *puVar4;
  longlong local_res8;
  
  EntityWithOwner__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xa8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  local_res8 = *(longlong *)(param_1 + 0xb8) - *(longlong *)(param_1 + 0xb0) >> 5;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar1 = *(undefined8 **)(param_1 + 0xb8);
  for (puVar4 = *(undefined8 **)(param_1 + 0xb0); puVar4 != puVar1; puVar4 = puVar4 + 4) {
    (**(code **)(*(longlong *)*puVar4 + 0x10))((longlong *)*puVar4,param_2);
    local_res8 = CONCAT44(local_res8._4_4_,(int)((longlong)(puVar4[2] - puVar4[1]) >> 3));
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,4)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    psVar2 = (short *)puVar4[2];
    for (psVar3 = (short *)puVar4[1]; psVar3 != psVar2; psVar3 = psVar3 + 4) {
      ID<ItemPrototype,unsigned_short>__save(psVar3,param_2);
      if (*psVar3 != 0) {
        MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,psVar3 + 1);
      }
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),psVar3 + 2,4);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    }
  }
  return;
}

