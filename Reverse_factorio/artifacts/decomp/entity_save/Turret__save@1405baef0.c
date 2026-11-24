// Turret::save @ 1405baef0 (function Turret::save)

void Turret__save(longlong param_1,longlong param_2)

{
  byte bVar1;
  longlong lVar2;
  longlong *plVar3;
  longlong lVar4;
  longlong local_res8;
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  Shooter__save(param_1 + 0xd0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x140,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  local_res8 = CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x148));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x149,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x14b,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x150,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x170,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  bVar1 = *(byte *)(param_1 + 0x170);
  if ((bVar1 != 0) && (lVar2 = *(longlong *)(param_2 + 0x48), lVar2 != 0)) {
    if (*(ulonglong *)(lVar2 + 0x2d8) <= (ulonglong)bVar1) {
      std__vector<bool,std::allocator<bool>_>__resize(lVar2 + 0x2c0,(ulonglong)bVar1 + 1);
    }
    *(uint *)(*(longlong *)(lVar2 + 0x2c0) + (ulonglong)(bVar1 >> 5) * 4) =
         *(uint *)(*(longlong *)(lVar2 + 0x2c0) + (ulonglong)(bVar1 >> 5) * 4) | 1 << (bVar1 & 0x1f)
    ;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x171,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x174,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x198,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1a0,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1a8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  TargeterBase__save(param_1 + 0x178,param_2);
  local_res8 = *(longlong *)(param_1 + 0x1b8) - *(longlong *)(param_1 + 0x1b0) >> 1;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar2 = *(longlong *)(param_1 + 0x1b8);
  for (lVar4 = *(longlong *)(param_1 + 0x1b0); lVar4 != lVar2; lVar4 = lVar4 + 2) {
    ID<EntityPrototype,unsigned_short>__save(lVar4,param_2);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x14a,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x1f0) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  plVar3 = *(longlong **)(param_1 + 0x1f0);
  if (plVar3 != (longlong *)0x0) {
                    /* WARNING: Could not recover jumptable at 0x0001405bb16f. Too many branches */
                    /* WARNING: Treating indirect jump as call */
    (**(code **)(*plVar3 + 0x118))(plVar3,param_2);
    return;
  }
  return;
}

