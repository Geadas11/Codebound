// FlyingRobot::moveTo @ 140464100 (function FlyingRobot::moveTo)

longlong FlyingRobot__moveTo(longlong param_1,longlong param_2,undefined8 param_3)

{
  undefined4 *puVar1;
  int local_res8;
  int iStackX_c;
  double local_28;
  double local_20;
  
  local_28 = (double)((int)param_3 - *(int *)(param_1 + 0x50)) * DAT_142402278;
  local_20 = (double)((int)((ulonglong)param_3 >> 0x20) - *(int *)(param_1 + 0x54)) * DAT_142402278;
  if (DAT_142402210 < local_20 * local_20 + local_28 * local_28) {
    *(undefined8 *)(param_1 + 0xe8) = *(undefined8 *)(*(longlong *)(param_1 + 0x78) + 0x1b0);
    puVar1 = (undefined4 *)Vector__getOrientation(&local_28,&local_res8);
    *(undefined4 *)(param_1 + 0xf4) = *puVar1;
  }
  Vector__setDistanceUpTo(&local_28,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    local_res8 = *(int *)(param_1 + 0x50) - (int)(local_28 * DAT_1424034a8);
    iStackX_c = *(int *)(param_1 + 0x54) - (int)(local_20 * DAT_1424034a8);
    param_3 = CONCAT44(iStackX_c,local_res8);
  }
  Entity__changePositionWithoutCollisionOrAABBCheck(param_1,param_3,param_1 + 0xa8);
  return param_2;
}

