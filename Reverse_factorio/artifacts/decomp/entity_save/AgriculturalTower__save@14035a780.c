// AgriculturalTower::save @ 14035a780 (function AgriculturalTower::save)

void AgriculturalTower__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  int iVar2;
  short *psVar3;
  longlong *plVar4;
  uint uVar5;
  uint uVar6;
  short *psVar7;
  longlong lVar8;
  int *piVar9;
  int iVar10;
  longlong local_res8;
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_1 + 0xd8) + 0x50))(*(longlong **)(param_1 + 0xd8),param_2);
  MapSerialiser__operator<<<EffectReceiver>(param_2,param_1 + 0xe0);
  (**(code **)(*(longlong *)(param_1 + 0x128) + 0xd8))((longlong *)(param_1 + 0x128),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x150) + 0xd8))((longlong *)(param_1 + 0x150),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x178) + 0xd8))((longlong *)(param_1 + 0x178),param_2);
  MapSerialiser__operator<<<StuckItems>(param_2,param_1 + 0x1a8);
  WakeUpList__save(param_1 + 0x1d0,param_2);
  WakeUpList__save(param_1 + 0x1f0,param_2);
  AgriculturalCraneState__save(param_1 + 0x220,param_2);
  AgriculturalCraneState__save(param_1 + 0x240,param_2);
  local_res8 = *(longlong *)(param_1 + 0x318) - *(longlong *)(param_1 + 0x310) >> 5;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  psVar7 = *(short **)(param_1 + 0x310);
  psVar3 = *(short **)(param_1 + 0x318);
  if (psVar7 != psVar3) {
    piVar9 = (int *)(psVar7 + 0xc);
    do {
      if (*psVar7 == 0) {
                    /* WARNING: Subroutine does not return */
        ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/SerialiserHelper.hpp"
                            ,0xeb,"bool(item.first)");
      }
      ID<EntityPrototype,unsigned_short>__save(psVar7,param_2);
      Serialiser__saveContainerSize<unsigned_int>(param_2,*piVar9);
      iVar2 = *piVar9;
      if (iVar2 != 0) {
        iVar10 = 0;
        do {
          uVar6 = piVar9[-1] + iVar10;
          uVar5 = uVar6 - piVar9[-2];
          if (uVar6 < (uint)piVar9[-2]) {
            uVar5 = uVar6;
          }
          lVar1 = *(longlong *)(piVar9 + -4) + (ulonglong)uVar5 * 0x28;
          TargeterBase__save(lVar1,param_2);
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),lVar1 + 0x20,8);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
          iVar10 = iVar10 + 1;
        } while (iVar10 != iVar2);
      }
      psVar7 = psVar7 + 0x10;
      piVar9 = piVar9 + 8;
    } while (psVar7 != psVar3);
  }
  local_res8 = *(longlong *)(param_1 + 0x330) - *(longlong *)(param_1 + 0x328) >> 5;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x330);
  for (lVar8 = *(longlong *)(param_1 + 0x328); lVar8 != lVar1; lVar8 = lVar8 + 0x20) {
    TargeterBase__save(lVar8,param_2);
  }
  MapSerialiser__operator<<<CraftingMachineAnimationState>(param_2,param_1 + 0x340);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x358,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  plVar4 = *(longlong **)(param_2 + 0x50);
  if (*(char *)(param_1 + 0x308) == '\0') {
    local_res8 = (ulonglong)local_res8._1_7_ << 8;
    (**(code **)(*plVar4 + 8))(plVar4,&local_res8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  else {
    local_res8 = CONCAT71(local_res8._1_7_,1);
    (**(code **)(*plVar4 + 8))();
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    AgriculturalTower__Task__save(param_1 + 0x288,param_2);
  }
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x360) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  lVar1 = *(longlong *)(param_1 + 0x360);
  if (lVar1 != 0) {
    GenericOnOffControlBehavior__save(lVar1,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),lVar1 + 0x218,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),lVar1 + 0x219,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),lVar1 + 0x21a,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    CircuitCondition__save(lVar1 + 0x238,param_2);
    CircuitCondition__save(lVar1 + 0x254,param_2);
  }
  return;
}

