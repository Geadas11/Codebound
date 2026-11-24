// CharacterCorpse::save @ 1403d8290 (function CharacterCorpse::save)

void CharacterCorpse__save(longlong param_1,longlong param_2)

{
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  (**(code **)(*(longlong *)(param_1 + 0xa8) + 0xd8))((longlong *)(param_1 + 0xa8),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xd0,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  MapSerialiser__operator<<<Color>(param_2,param_1 + 0xd4);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  LocalisedString__save(param_1 + 0xf0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x158,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x15c,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x15d,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

