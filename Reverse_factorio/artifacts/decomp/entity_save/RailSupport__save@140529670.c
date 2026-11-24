// RailSupport::save @ 140529670 (function RailSupport::save)

void RailSupport__save(longlong param_1,longlong param_2)

{
  EntityWithOwner__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xa8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

