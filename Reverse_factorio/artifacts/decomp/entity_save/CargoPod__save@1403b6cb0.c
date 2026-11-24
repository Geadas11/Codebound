// CargoPod::save @ 1403b6cb0 (function CargoPod::save)

void CargoPod__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xb8,param_2);
  CargoUnit__save(param_1 + 0xf8,param_2);
  local_res8[0] = *(undefined1 *)(param_1 + 0x200);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x208,param_2);
  TargeterBase__save(param_1 + 0x228,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x248,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  Vector2<double,Vector>__save(param_1 + 0x250,param_2);
  Vector2<double,Vector>__save(param_1 + 0x260,param_2);
  Vector2<double,Vector>__save(param_1 + 0x270,param_2);
  MapPosition__save(param_1 + 0x280,param_2);
  local_res8[0] = *(undefined1 *)(param_1 + 0x288);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x28c,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x290,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x294,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x298,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x29c,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2a0,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2a4,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2a8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  MapSerialiser__operator<<<ID<ProcessionPrototype,unsigned_short>_>(param_2,param_1 + 0x2ac);
  MapSerialiser__operator<<<ID<ProcessionPrototype,unsigned_short>_>(param_2,param_1 + 0x2ae);
  MapSerialiser__operator<<<ID<ProcessionPrototype,unsigned_short>_>(param_2,param_1 + 0x2b0);
  SerialiserHelper<MapSerialiser>__saveValue<ID<ProcessionLayerInheritanceGroup,unsigned_char>,ProcessionInheritedValues,std::less<ID<ProcessionLayerInheritanceGroup,unsigned_char>_>_>
            (param_2,param_1 + 0x2b8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x2d0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

