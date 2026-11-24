// FluidWagon::save @ 140462080 (function FluidWagon::save)

void FluidWagon__save(longlong param_1,undefined8 param_2)

{
  longlong lVar1;
  longlong lVar2;
  longlong local_res8;
  
  RollingStock__save();
  MapSerialiser__operator<<<Fluid>(param_2,param_1 + 0x300);
  local_res8 = *(longlong *)(param_1 + 0x328) - *(longlong *)(param_1 + 800) >> 5;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x328);
  for (lVar2 = *(longlong *)(param_1 + 800); lVar2 != lVar1; lVar2 = lVar2 + 0x20) {
    TargeterBase__save(lVar2,param_2);
  }
  return;
}

