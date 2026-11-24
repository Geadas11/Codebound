// SpeechBubble::save @ 140577b20 (function SpeechBubble::save)

void SpeechBubble__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  LocalisedString__save(param_1 + 0xa8,param_2);
  TargeterBase__save(param_1 + 0x110,param_2);
  (**(code **)(*(longlong *)(param_1 + 0x200) + 0x30))((longlong *)(param_1 + 0x200),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x130) + 0x30))((longlong *)(param_1 + 0x130),param_2);
  local_res8[0] = *(undefined1 *)(param_1 + 0x308);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x30c,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  return;
}

