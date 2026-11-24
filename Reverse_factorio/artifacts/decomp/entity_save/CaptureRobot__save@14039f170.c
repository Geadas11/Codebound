// CaptureRobot::save @ 14039f170 (function CaptureRobot::save)

void CaptureRobot__save(longlong param_1,longlong param_2)

{
  FlyingRobot__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xf8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x100,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  TargeterBase__save(param_1 + 0x108,param_2);
  return;
}

