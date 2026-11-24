// ElectricTurret::save @ 140414230 (function ElectricTurret::save)

void ElectricTurret__save(longlong param_1,undefined8 param_2)

{
  Turret__save();
                    /* WARNING: Could not recover jumptable at 0x00014041425c. Too many branches */
                    /* WARNING: Treating indirect jump as call */
  (**(code **)(**(longlong **)(param_1 + 0x200) + 0x50))(*(longlong **)(param_1 + 0x200),param_2);
  return;
}

