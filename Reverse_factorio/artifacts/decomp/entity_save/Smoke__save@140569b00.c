// Smoke::save @ 140569b00 (function Smoke::save)

void Smoke__save(longlong param_1,longlong param_2)

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
  return;
}

