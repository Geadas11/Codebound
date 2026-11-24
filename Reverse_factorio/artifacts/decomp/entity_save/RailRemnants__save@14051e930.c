// RailRemnants::save @ 14051e930 (function RailRemnants::save)

void RailRemnants__save(longlong param_1,longlong param_2)

{
  Corpse__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xdc,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

