// MiningDrill::save @ 1404d0800 (function MiningDrill::save)

void MiningDrill__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong lVar2;
  ulonglong local_res8;
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x150,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8._0_1_ = *(undefined1 *)(param_1 + 0x151);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x158,param_2);
  DropTarget__save(param_1 + 0x180,param_2);
  (**(code **)(*(longlong *)(param_1 + 0x1d0) + 0xd8))((longlong *)(param_1 + 0x1d0),param_2);
  MapSerialiser__operator<<<StuckItems>(param_2,param_1 + 0x1f8);
  WakeUpList__save(param_1 + 0x220,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x242,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x243,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(*(longlong *)(param_1 + 0x248) + 0xd8))((longlong *)(param_1 + 0x248),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x278,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x280,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x290,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_1 + 0x2b0) + 0x50))(*(longlong **)(param_1 + 0x2b0),param_2);
  MapSerialiser__operator<<<EffectReceiver>(param_2,param_1 + 0x2b8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x300,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x302,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<std::optional<FluidBox>_>(param_2,param_1 + 0x308);
  MapSerialiser__operator<<<std::optional<FluidBox>_>(param_2,param_1 + 0x418);
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x538) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x538) != 0) {
    MiningDrillControlBehavior__save(*(longlong *)(param_1 + 0x538),param_2);
  }
  local_res8 = *(longlong *)(param_1 + 0x548) - *(longlong *)(param_1 + 0x540) >> 5;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x548);
  for (lVar2 = *(longlong *)(param_1 + 0x540); lVar2 != lVar1; lVar2 = lVar2 + 0x20) {
    TargeterBase__save(lVar2,param_2);
  }
  MiningDrillFilter__save(param_1 + 0x528,param_2);
  local_res8 = CONCAT71(local_res8._1_7_,(char)(*(uint *)(param_1 + 0x14) >> 2)) &
               0xffffffffffffff01;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

