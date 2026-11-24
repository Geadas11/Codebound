// SimpleEntity::save @ 140567820 (function SimpleEntity::save)

void SimpleEntity__save(longlong param_1,longlong param_2)

{
  EntityWithHealth__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x90,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

