// Sticker::save @ 140598dc0 (function Sticker::save)

void Sticker__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong lVar2;
  int local_res8 [2];
  
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  TargeterBase__save(param_1 + 0xc0,param_2);
  MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0xe0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xf8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x100,param_2);
  MapPosition__save(param_1 + 0x120,param_2);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0x128);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0x134);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x148,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x150,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<EffectTriggerer>(param_2,param_1 + 0x158);
  local_res8[0] =
       (int)(*(longlong *)(param_1 + 0xb0) - *(longlong *)(param_1 + 0xa8) >> 3) * -0x55555555;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  lVar1 = *(longlong *)(param_1 + 0xb0);
  for (lVar2 = *(longlong *)(param_1 + 0xa8); lVar2 != lVar1; lVar2 = lVar2 + 0x18) {
    Vector2<double,Vector>__save(lVar2,param_2);
    CyclicFramePosition__save(lVar2 + 0x10,param_2);
  }
  return;
}

