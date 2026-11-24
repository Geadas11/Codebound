// Ghost::save @ 140480740 (function Ghost::save)

void Ghost__save(longlong param_1,longlong param_2)

{
  Entity__save();
  if (*(char *)(param_2 + 0x14b) == '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x98,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  }
  return;
}

