// InfinityPipe::save @ 140489640 (function InfinityPipe::save)

void InfinityPipe__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  FluidBox__save(param_1 + 0xb8,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x1c0) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x1c0) != 0) {
    SingleFluidBoxControlBehavior__save(*(longlong *)(param_1 + 0x1c0),param_2);
  }
  if ((*(short *)(param_1 + 0x1f8) == 0) &&
     (((*(double *)(param_1 + 0x200) != 0.0 || (*(float *)(param_1 + 0x208) != 0.0)) ||
      (*(char *)(param_1 + 0x20c) != '\0')))) {
                    /* WARNING: Subroutine does not return */
    Logging__logAndAbortOrThrow
              ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\InfinityPipeSettings.cpp"
               ,0x32,9,
               "Attempting to save filter with zero ID and non-default amount/temperature/mode. This will cause save/load instability and desyncs."
              );
  }
  ID<FluidPrototype,unsigned_short>__save((short *)(param_1 + 0x1f8),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x200,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x208,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  local_res8[0] = *(undefined1 *)(param_1 + 0x20c);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  UpdatableEntity__save(param_1 + 0x1c8,param_2);
  return;
}

