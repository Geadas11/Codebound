// Pump::save @ 1404f8af0 (function Pump::save)

void Pump__save(longlong param_1,longlong param_2)

{
  longlong *plVar1;
  ushort uVar2;
  uint uVar3;
  longlong lVar4;
  ushort local_res8 [4];
  
  lVar4 = *(longlong *)(*(longlong *)(param_1 + 0x48) + 0xba0);
  if (((*(longlong *)(*(longlong *)(param_1 + 0x48) + 0xba8) - lVar4 == 0x1d0) &&
      (*(char *)(lVar4 + 0x51) == '\x02')) && (*(char *)(lVar4 + 0x139) == '\x01')) {
LAB_1404f8bb7:
    EntityWithOwner__save(param_1);
    UpdatableEntity__save(param_1 + 0xa8,param_2);
    (**(code **)(**(longlong **)(param_1 + 0x1f0) + 0x50))(*(longlong **)(param_1 + 0x1f0),param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x1f8,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    CyclicFramePosition__save(param_1 + 0x1fc,param_2);
    PumpRailConnector__save(param_1 + 0x208,param_2);
    PumpRailConnector__save(param_1 + 0x2b8,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x368,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x369,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    WakeUpList__save(param_1 + 0x370,param_2);
    FluidBox__save(param_1 + 0xe8,param_2);
    if ((*(byte *)(param_1 + 0x6e) & 0x20) != 0) {
      local_res8[0] = *(ushort *)(param_1 + 0x108);
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),local_res8,2);
      uVar2 = local_res8[0];
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      if ((local_res8[0] != 0) && (lVar4 = *(longlong *)(param_2 + 0x48), lVar4 != 0)) {
        uVar3 = (uint)local_res8[0];
        if (*(ulonglong *)(lVar4 + 0x1b8) <= (ulonglong)local_res8[0]) {
          std__vector<bool,std::allocator<bool>_>__resize
                    (lVar4 + 0x1a0,(ulonglong)local_res8[0] + 1);
        }
        *(uint *)(*(longlong *)(lVar4 + 0x1a0) + (ulonglong)(uVar2 >> 5) * 4) =
             *(uint *)(*(longlong *)(lVar4 + 0x1a0) + (ulonglong)(uVar2 >> 5) * 4) |
             1 << (uVar3 & 0x1f);
      }
    }
    local_res8[0] = CONCAT11(local_res8[0]._1_1_,*(longlong *)(param_1 + 0x200) != 0);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    plVar1 = *(longlong **)(param_1 + 0x200);
    if (plVar1 != (longlong *)0x0) {
      (**(code **)(*plVar1 + 0x118))(plVar1,param_2);
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x390,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    return;
  }
  for (lVar4 = *(longlong *)(param_1 + 0x208); lVar4 != *(longlong *)(param_1 + 0x210);
      lVar4 = lVar4 + 0x20) {
    if (*(longlong *)(lVar4 + 8) != 0) goto LAB_1404f8d59;
  }
  if (*(longlong *)(param_1 + 0x228) == 0) {
    for (lVar4 = *(longlong *)(param_1 + 0x2b8); lVar4 != *(longlong *)(param_1 + 0x2c0);
        lVar4 = lVar4 + 0x20) {
      if (*(longlong *)(lVar4 + 8) != 0) goto LAB_1404f8d59;
    }
    if (*(longlong *)(param_1 + 0x2d8) == 0) goto LAB_1404f8bb7;
  }
LAB_1404f8d59:
                    /* WARNING: Subroutine does not return */
  Logging__logAndAbortOrThrow
            ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\Pump.cpp",
             0x75,9,"Trying to save in a connector that would result in save/load instability.");
}

