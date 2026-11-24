// Furnace::save @ 140469840 (function Furnace::save)

void Furnace__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  CraftingMachine__save();
  SerialiserHelper<MapSerialiser>__save<ID<RecipePrototype,unsigned_short>_>
            (param_2,param_1 + 0x2f8);
  if (*(short *)(param_1 + 0x2f8) != 0) {
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x2fa);
  }
  local_res8[0] = *(longlong *)(param_1 + 0x300) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x300) != 0) {
    FurnaceControlBehavior__save(*(longlong *)(param_1 + 0x300),param_2);
  }
  return;
}

