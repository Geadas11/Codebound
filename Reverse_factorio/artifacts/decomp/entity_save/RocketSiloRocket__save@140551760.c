// RocketSiloRocket::save @ 140551760 (function RocketSiloRocket::save)

void RocketSiloRocket__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  local_res8[0] = *(undefined1 *)(param_1 + 200);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  CyclicFramePosition__save(param_1 + 0xec,param_2);
  CyclicFramePosition__save(param_1 + 0xf0,param_2);
  CyclicFramePosition__save(param_1 + 0xf4,param_2);
  CyclicFramePosition__save(param_1 + 0xf8,param_2);
  CyclicFramePosition__save(param_1 + 0xfc,param_2);
  CyclicFramePosition__save(param_1 + 0x100,param_2);
  MapPosition__save(param_1 + 0x104,param_2);
  TargeterBase__save(param_1 + 0x110,param_2);
  TargeterBase__save(param_1 + 0x130,param_2);
  TargeterBase__save(param_1 + 0x150,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x170,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x178,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  return;
}

