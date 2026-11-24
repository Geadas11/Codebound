// SmokeWithTrigger::save @ 14056d6c0 (function SmokeWithTrigger::save)

void SmokeWithTrigger__save(longlong param_1,longlong param_2)

{
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  CyclicFramePosition__save(param_1 + 0xa8,param_2);
  Vector2<double,Vector>__save(param_1 + 0xb0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xc0,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xc4,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),param_1 + 200,4)
  ;
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0xd1);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0xe0,param_2);
  MapPosition__save(param_1 + 0x100,param_2);
  TargeterBase__save(param_1 + 0x108,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x128,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),param_1 + 300,4)
  ;
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  return;
}

