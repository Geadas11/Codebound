// FluidTurret::save @ 14045e080 (function FluidTurret::save)

void FluidTurret__save(longlong param_1,longlong param_2)

{
  Turret__save();
  MapSerialiser__operator<<<Fluid>(param_2,param_1 + 0x308);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x318,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  FluidBox__save(param_1 + 0x200,param_2);
  return;
}

