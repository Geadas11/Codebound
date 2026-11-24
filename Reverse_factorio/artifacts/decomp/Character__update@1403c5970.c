// Character::update @ 1403c5970 (function Character::update)

void Character__update(longlong param_1)

{
  undefined1 uVar1;
  ushort uVar2;
  short sVar3;
  char cVar4;
  byte bVar5;
  short sVar6;
  ushort uVar7;
  uint uVar8;
  undefined4 uVar9;
  longlong lVar10;
  longlong lVar11;
  longlong lVar12;
  undefined8 *puVar13;
  undefined8 uVar14;
  undefined8 uVar15;
  int *piVar16;
  uint uVar17;
  ulonglong uVar18;
  int iVar19;
  double dVar20;
  undefined8 extraout_XMM0_Qa;
  undefined1 auVar21 [16];
  double dVar22;
  double dVar23;
  undefined4 local_res8;
  undefined8 local_res10;
  undefined8 in_stack_fffffffffffffe58;
  undefined4 uVar24;
  undefined8 in_stack_fffffffffffffe60;
  undefined4 uVar25;
  ulonglong in_stack_fffffffffffffe68;
  undefined4 uVar26;
  undefined8 in_stack_fffffffffffffe70;
  undefined4 uVar27;
  undefined ***in_stack_fffffffffffffe78;
  double local_178;
  double local_170;
  int local_168;
  undefined **local_158;
  longlong local_150;
  undefined ***local_120;
  undefined **local_118;
  longlong local_110;
  undefined ***local_e0;
  undefined **local_d8;
  longlong local_d0;
  undefined ***local_a0;
  undefined **local_98;
  longlong local_90;
  undefined ***local_60;
  
  uVar24 = (undefined4)((ulonglong)in_stack_fffffffffffffe58 >> 0x20);
  uVar25 = (undefined4)((ulonglong)in_stack_fffffffffffffe60 >> 0x20);
  uVar27 = (undefined4)((ulonglong)in_stack_fffffffffffffe70 >> 0x20);
  lVar11 = *(longlong *)(*(longlong *)(param_1 + -0x30) + 0x1b0);
  if (*(longlong *)(param_1 + 0x540) == lVar11) {
    return;
  }
  *(longlong *)(param_1 + 0x540) = lVar11;
  if ((*(longlong *)(param_1 + 0x6b8) != 0) &&
     (lVar11 = *(longlong *)(*(longlong *)(param_1 + 0x6b8) + 0x28), lVar11 != 0)) {
    lVar12 = *(longlong *)(*(longlong *)(lVar11 + 0x20) + 0x4b0);
    if (((lVar12 != 0) &&
        ((lVar10 = *(longlong *)(lVar12 + 0x80), lVar10 != 0 ||
         ((*(longlong *)(lVar12 + 0x68) != 0 &&
          (lVar10 = *(longlong *)(*(longlong *)(lVar12 + 0x68) + 0x40), lVar10 != 0)))))) &&
       (lVar10 == lVar11)) {
      piVar16 = (int *)(param_1 + 0x438);
      if (((*(short *)(param_1 + 0x450) != *(short *)(param_1 + 0x43c)) ||
          (*(char *)(param_1 + 0x452) != *(char *)(param_1 + 0x43e))) ||
         (*(int *)(param_1 + 0x454) != *piVar16)) {
        DAT_142d34f6c = 1;
        uVar15 = ItemStack__getDump(piVar16,&local_178);
        uVar26 = (undefined4)(in_stack_fffffffffffffe68 >> 0x20);
        uVar15 = std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__c_str(uVar15)
        ;
        uVar9 = SurfaceIndex__asIndex(piVar16);
        uVar14 = ItemStack__getID(extraout_XMM0_Qa,&local_res8);
        uVar7 = ID<AchievementPrototype,unsigned_short>__getIndex(uVar14);
        uVar8 = (uint)uVar7;
        uVar7 = ID<AchievementPrototype,unsigned_short>__getIndex(param_1 + 0x450);
                    /* WARNING: Subroutine does not return */
        Logging__logAndAbortOrThrow
                  ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Item\\CursorStackChangeGuard.cpp"
                   ,0x10,9,
                   "possible external cursor stack modification detected: %u != %u || %u != %u. %s",
                   CONCAT44(uVar24,(uint)uVar7),CONCAT44(uVar25,uVar8),
                   CONCAT44(uVar26,*(undefined4 *)(param_1 + 0x454)),CONCAT44(uVar27,uVar9),uVar15);
      }
    }
    lVar11 = (**(code **)(**(longlong **)(lVar11 + 0x868) + 0x8c8))();
    if (lVar11 == 0) {
      local_178 = (double)(((ulonglong)local_178 >> 8 & 0xffffff) << 8);
      *(double *)(param_1 + 0xc0) = local_178;
      *(undefined4 *)(param_1 + 200) = 0;
      lVar11 = *(longlong *)(param_1 + 0x2d8);
      local_178 = 0.0;
      *(undefined8 *)(lVar11 + 0x58) = 0;
      *(undefined4 *)(lVar11 + 0x60) = 0;
      ManualMiner__updateMiningTarget(lVar11,1);
      if (*(int *)(lVar11 + 0x58) == 0) {
        lVar12 = (**(code **)(**(longlong **)(lVar11 + 8) + 0x188))();
        *(undefined1 *)(lVar12 + 0x32) = 0;
        puVar13 = (undefined8 *)
                  (**(code **)**(undefined8 **)(lVar11 + 8))
                            (*(undefined8 **)(lVar11 + 8),&local_res8);
        *(undefined8 *)(lVar11 + 0x78) = *puVar13;
      }
    }
  }
  lVar11 = param_1 + -0xa8;
  Gate__requestToOpenNearbyGates(lVar11);
  *(undefined8 *)(param_1 + 0x558) = *(undefined8 *)(param_1 + 0x560);
  *(undefined8 *)(param_1 + 0x560) = *(undefined8 *)(param_1 + -0x58);
  if (((*(char *)(param_1 + 0xf0) != '\0') ||
      ((*(longlong *)(param_1 + 0x4c8) == 0 &&
       (cVar4 = Character__isLootInReach(lVar11), cVar4 != '\0')))) &&
     (lVar12 = *(longlong *)
                (*(longlong *)(*(longlong *)(param_1 + -0x30) + 1000) +
                (ulonglong)*(byte *)(param_1 + -0x18) * 8),
     GameStateManualBuilder__pickItems
               (param_1 + 0x570,*(undefined1 *)(param_1 + 0xf0),
                *(double *)(*(longlong *)(param_1 + -0x60) + 0x878) + *(double *)(lVar12 + 0x480) +
                (double)*(uint *)(param_1 + 0x200),
                *(double *)(*(longlong *)(param_1 + -0x60) + 0x880) + *(double *)(lVar12 + 0x498) +
                (double)*(uint *)(param_1 + 0x220)), (*(byte *)(param_1 + -0x3a) & 0x10) != 0)) {
    return;
  }
  Character__updateEnemyShootingTarget(lVar11,param_1 + 0xc4);
  *(undefined1 *)(param_1 + 0xf1) = 0;
  if (*(char *)(param_1 + 0xc0) == '\0') {
    if (*(longlong *)(param_1 + 0x6a0) != 0) {
      if (*(longlong *)(param_1 + 0x6b0) == 0) {
        *(undefined8 *)(*(longlong *)(param_1 + 0x6a0) + 8) = *(undefined8 *)(param_1 + 0x6a8);
      }
      else {
        *(undefined8 *)(*(longlong *)(param_1 + 0x6b0) + 0x10) = *(undefined8 *)(param_1 + 0x6a8);
      }
      if (*(longlong *)(param_1 + 0x6a8) != 0) {
        *(undefined8 *)(*(longlong *)(param_1 + 0x6a8) + 0x18) = *(undefined8 *)(param_1 + 0x6b0);
      }
      *(undefined8 *)(param_1 + 0x6a0) = 0;
    }
  }
  else {
    cVar4 = ShooterLogic__shoot<Character>(lVar11);
    if ((cVar4 == '\0') &&
       (Character__attackWithTool(lVar11), (*(byte *)(param_1 + -0x3a) & 0x10) != 0)) {
      return;
    }
  }
  dVar22 = DAT_142402278;
  uVar15 = *(undefined8 *)(param_1 + 0x470);
  local_178 = (double)(*(int *)(param_1 + 0x560) - *(int *)(param_1 + 0x558)) * DAT_142402278;
  local_170 = (double)(*(int *)(param_1 + 0x564) - *(int *)(param_1 + 0x55c)) * DAT_142402278;
  uVar1 = *(undefined1 *)(param_1 + -0x18);
  uVar14 = Character__getSurface(lVar11);
  uVar7 = *(ushort *)(param_1 + 0x432);
  if (uVar7 < *(ushort *)(param_1 + 0x368)) {
    uVar2 = *(ushort *)(*(longlong *)(param_1 + 0x360) + 4 + (ulonglong)uVar7 * 0x18);
    local_res8._0_3_ =
         CONCAT12(*(undefined1 *)(*(longlong *)(param_1 + 0x360) + 6 + (ulonglong)uVar7 * 0x18),
                  uVar2);
    if ((uVar2 != 0) &&
       (lVar12 = (**(code **)(**(longlong **)(DAT_1429c56f8 + (ulonglong)uVar2 * 8) + 0x128))(),
       uVar8 = local_res8, lVar12 != 0)) goto LAB_1403c5cec;
  }
  uVar8 = 0;
LAB_1403c5cec:
  lVar12 = CONCAT71((int7)((ulonglong)in_stack_fffffffffffffe58 >> 8),uVar1);
  Shooter__update(uVar15,0,uVar8,uVar14,lVar12,&local_178,
                  in_stack_fffffffffffffe68 & 0xffffffffffffff00,param_1 + -0x58);
  if (((*(byte *)(param_1 + -0x3a) & 0x10) == 0) &&
     (Shooter__updateNoDelayedShoot(*(undefined8 *)(param_1 + 0x478)),
     (*(byte *)(param_1 + -0x3a) & 0x10) == 0)) {
    Character__updateHeartBeat(lVar11);
    if ((*(char *)(param_1 + 0xf1) == '\0') && (*(char *)(param_1 + 0x468) == '\0')) {
      local_98 = std::
                 _Func_impl_no_alloc<`private:_void___cdecl_Character::updateAnimationState(void)___ptr64'::`2'::<lambda_4>,void>
                 ::vftable;
      local_60 = &local_98;
      local_d8 = std::
                 _Func_impl_no_alloc<`private:_void___cdecl_Character::updateAnimationState(void)___ptr64'::`2'::<lambda_3>,void>
                 ::vftable;
      local_a0 = &local_d8;
      local_118 = std::
                  _Func_impl_no_alloc<`private:_void___cdecl_Character::updateAnimationState(void)___ptr64'::`2'::<lambda_2>,void,Footprint,Vector_const&___ptr64,RotatedAnimation_const&___ptr64>
                  ::vftable;
      local_e0 = &local_118;
      local_158 = std::
                  _Func_impl_no_alloc<`private:_void___cdecl_Character::updateAnimationState(void)___ptr64'::`2'::<lambda_1>,void>
                  ::vftable;
      local_120 = &local_158;
      lVar12 = param_1 + 0x90;
      in_stack_fffffffffffffe78 = &local_d8;
      local_150 = lVar11;
      local_110 = lVar11;
      local_d0 = lVar11;
      local_90 = lVar11;
      Character__updateAnimationState
                (param_1 + 0x45c,lVar11,param_1 + 0x80,*(undefined8 *)(param_1 + 0x2d8),lVar12,
                 param_1 + 0xd0,&local_158,&local_118,in_stack_fffffffffffffe78,&local_98);
      if (local_120 != (undefined ***)0x0) {
        (*(code *)(*local_120)[4])(local_120,local_120 != &local_158);
        local_120 = (undefined ***)0x0;
      }
      if (local_e0 != (undefined ***)0x0) {
        (*(code *)(*local_e0)[4])(local_e0,local_e0 != &local_118);
        local_e0 = (undefined ***)0x0;
      }
      if (local_a0 != (undefined ***)0x0) {
        (*(code *)(*local_a0)[4])(local_a0,local_a0 != &local_d8);
        local_a0 = (undefined ***)0x0;
      }
      if (local_60 != (undefined ***)0x0) {
        (*(code *)(*local_60)[4])(local_60,local_60 != &local_98);
      }
    }
    if ((*(longlong *)(param_1 + 0x6b8) != 0) &&
       (lVar11 = GuiTarget__getEntity(*(longlong *)(param_1 + 0x6b8) + 0x30), lVar11 != 0)) {
      uVar15 = GuiTarget__getEntity(*(longlong *)(param_1 + 0x6b8) + 0x30);
      cVar4 = Character__canOpenGui(param_1 + -0xa8,uVar15,0);
      if ((cVar4 == '\0') &&
         (GuiTarget__clear(*(longlong *)(param_1 + 0x6b8) + 0x30,0,1),
         (*(byte *)(param_1 + -0x3a) & 0x10) != 0)) {
        return;
      }
    }
    if ((*(longlong *)(param_1 + 0x530) == 0) ||
       ((uVar18 = *(longlong *)(*(longlong *)(param_1 + -0x30) + 0x1b0) -
                  *(longlong *)(param_1 + 0x530), uVar18 != *(uint *)(param_1 + 0x34c) &&
        (*(uint *)(param_1 + 0x34c) <= uVar18)))) {
      piVar16 = (int *)Vector__getOrientation(param_1 + 0xb0,&local_res8);
      bVar5 = (byte)((uint)(*piVar16 + 0x10000000) >> 0x1c) & 0xe;
    }
    else {
      bVar5 = *(byte *)(param_1 + 0x548);
    }
    uVar8 = (uint)bVar5 * 0x10000000 - *(int *)(param_1 + 0x568);
    if (uVar8 < 0x80000000) {
      uVar17 = 0xcccccd0;
      if (uVar8 < 0xcccccd0) {
        uVar17 = uVar8;
      }
    }
    else {
      uVar17 = 0xcccccd0;
      if (-uVar8 < 0xcccccd0) {
        uVar17 = -uVar8;
      }
      uVar17 = -uVar17;
    }
    *(uint *)(param_1 + 0x568) = uVar17 + *(int *)(param_1 + 0x568);
    uVar15 = Character__getGrid(param_1 + -0xa8);
    PortableRoboportProvider__update(param_1 + 0x628,uVar15);
    if ((*(byte *)(param_1 + -0x3a) & 0x10) == 0) {
      lVar11 = Character__getGrid(param_1 + -0xa8);
      if (lVar11 != 0) {
        sVar6 = EquipmentGrid__calculateInventorySizeBonus(lVar11);
        sVar3 = *(short *)(lVar11 + 0x5a);
        *(short *)(lVar11 + 0x5a) = sVar6;
        if ((sVar6 != sVar3) &&
           (Character__updateInventorySize(param_1 + -0xa8),
           (*(byte *)(param_1 + -0x3a) & 0x10) != 0)) {
          return;
        }
      }
      if (*(char *)(param_1 + 0x690) == '\0') {
        *(undefined8 *)(param_1 + 0xa8) = *(undefined8 *)(param_1 + 0x90);
        *(undefined8 *)(param_1 + 0xb0) = *(undefined8 *)(param_1 + 0x98);
        *(undefined8 *)(param_1 + 0xb8) = *(undefined8 *)(param_1 + 0xa0);
        cVar4 = ManualRepairer__performRepair(*(undefined8 *)(param_1 + 0x2e0));
        if ((*(byte *)(param_1 + -0x3a) & 0x10) == 0) {
          lVar11 = *(longlong *)(param_1 + 0x2e0);
          if (cVar4 == '\0') {
            local_178 = (double)(((ulonglong)local_178 >> 8 & 0xffffff) << 8);
            *(double *)(lVar11 + 0x28) = local_178;
            *(undefined4 *)(lVar11 + 0x30) = 0;
          }
          else if (*(longlong *)(lVar11 + 0x10) != 0) {
            *(undefined1 *)(param_1 + 0xa8) = 0;
            dVar23 = (double)(*(int *)(*(longlong *)(lVar11 + 0x10) + 0x50) -
                             *(int *)(param_1 + -0x58)) * dVar22;
            dVar22 = (double)(*(int *)(*(longlong *)(lVar11 + 0x10) + 0x54) -
                             *(int *)(param_1 + -0x54)) * dVar22;
            if (dVar22 != 0.0 || dVar23 != 0.0) {
              dVar20 = dVar22 * dVar22 + dVar23 * dVar23;
              if (dVar20 < 0.0) {
                dVar20 = sqrt(dVar20);
              }
              else {
                auVar21._8_8_ = 0;
                auVar21._0_8_ = dVar20;
                auVar21 = sqrtpd(ZEXT816(0),auVar21);
                dVar20 = auVar21._0_8_;
              }
              dVar22 = dVar22 / dVar20;
              dVar23 = dVar23 / dVar20;
            }
            *(double *)(param_1 + 0xb0) = dVar23;
            *(double *)(param_1 + 0xb8) = dVar22;
          }
          ManualMiner__update(*(undefined8 *)(param_1 + 0x2d8));
          if ((*(byte *)(param_1 + -0x3a) & 0x10) == 0) {
            Character__updateFlyingState(param_1 + -0xa8,param_1 + 0x80);
            uVar24 = (undefined4)((ulonglong)lVar12 >> 0x20);
            uVar25 = (undefined4)((ulonglong)in_stack_fffffffffffffe78 >> 0x20);
            local_res10 = *(undefined8 *)(param_1 + -0x58);
            if ((((*(char *)(param_1 + 0xa8) == '\0') || (*(char *)(param_1 + 0x468) != '\0')) ||
                (iVar19 = *(int *)(*(longlong *)(param_1 + 0x2d8) + 0x58), iVar19 == 2)) ||
               ((iVar19 == 3 || (*(char *)(param_1 + 0xf1) != '\0')))) {
              piVar16 = (int *)Vector__getOrientation(param_1 + 0xb0,&local_res8);
              bVar5 = *(byte *)(param_1 + 0x548);
              if ((bVar5 != ((byte)((uint)(*piVar16 + 0x10000000) >> 0x1c) & 0xe)) &&
                 ((*(longlong *)(param_1 + 0x530) != 0 &&
                  ((ulonglong)
                   (*(longlong *)(*(longlong *)(param_1 + -0x30) + 0x1b0) -
                   *(longlong *)(param_1 + 0x530)) < (ulonglong)*(uint *)(param_1 + 0x34c))))) {
                uVar15 = (&DAT_142007e78)[(ulonglong)bVar5 * 2];
                *(undefined **)(param_1 + 0xb0) = (&PTR_142007e70)[(ulonglong)bVar5 * 2];
                *(undefined8 *)(param_1 + 0xb8) = uVar15;
              }
            }
            else {
              Character__calculateMovementVectorAndStepCount
                        (param_1 + -0xa8,&local_178,param_1 + 0x80,1);
              iVar19 = local_168;
              while( true ) {
                uVar24 = (undefined4)((ulonglong)lVar12 >> 0x20);
                uVar25 = (undefined4)((ulonglong)in_stack_fffffffffffffe78 >> 0x20);
                if (iVar19 == 0) break;
                iVar19 = iVar19 + -1;
                if ((*(byte *)(param_1 + -0x3a) & 0x10) != 0) {
                  return;
                }
                local_168 = iVar19;
                Character__changePosition(param_1 + -0xa8,&local_178);
              }
              if ((*(byte *)(param_1 + -0x3a) & 0x10) != 0) {
                return;
              }
              if (*(char *)(param_1 + 0xd0) == '\0') {
                Character__triggerFootstepParticles
                          (param_1 + -0xa8,*(longlong *)(param_1 + -0x60) + 0x920);
              }
            }
            lVar11 = param_1 + -0xa8;
            if (*(char *)(param_1 + 0xd0) != '\0') {
              lVar12 = Character__getArmorAnimation(lVar11);
              if (*(longlong *)(lVar12 + 0x3d8) != *(longlong *)(lVar12 + 0x3e0)) {
                bVar5 = Character__isGameStatePlayerDuplicatedInLatencyState(lVar11);
                piVar16 = (int *)Vector__getOrientation(param_1 + 0xb0,&local_res8);
                local_res8 = *piVar16 + 0x10000000U & 0xe0000000;
                uVar27 = *(undefined4 *)(lVar12 + 0x3f4);
                uVar9 = *(undefined4 *)(lVar12 + 0x3f0);
                uVar15 = *(undefined8 *)(*(longlong *)(param_1 + -0x30) + 0x1b0);
                uVar14 = Character__getSurface(lVar11);
                SmokeSources__tryToCreateSmoke
                          ((longlong *)(lVar12 + 0x3d8),uVar14,uVar15,uVar9,CONCAT44(uVar24,uVar27),
                           &local_res10,param_1 + -0x58,&local_res8,CONCAT44(uVar25,(uint)bVar5));
              }
            }
            if (((((*(longlong *)(param_1 + 0x4a8) != 0) || (*(longlong *)(param_1 + 0x4c8) != 0))
                 || (*(longlong *)(param_1 + 0x4e8) != 0)) ||
                ((cVar4 = Character__hasBeltImmunity(lVar11), cVar4 != '\0' ||
                 (MovableByTransportBelt__moveByTransportBelt(param_1 + 0x28),
                 (*(byte *)(param_1 + -0x3a) & 0x10) == 0)))) &&
               (*(longlong *)(*(longlong *)(param_1 + 0x460) + 0x90) !=
                *(longlong *)(*(longlong *)(param_1 + 0x460) + 0x98))) {
              ManualCrafter__performCrafting();
            }
          }
        }
      }
      else {
        *(undefined1 *)(param_1 + 0x690) = 0;
      }
    }
  }
  return;
}

