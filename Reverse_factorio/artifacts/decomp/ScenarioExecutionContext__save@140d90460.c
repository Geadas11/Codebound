// ScenarioExecutionContext::save @ 140d90460 (function ScenarioExecutionContext::save)

void ScenarioExecutionContext__save(longlong param_1,longlong param_2)

{
  undefined8 *puVar1;
  int *piVar2;
  int iVar3;
  longlong *plVar4;
  longlong lVar5;
  byte bVar6;
  longlong *plVar7;
  longlong *plVar8;
  longlong lVar9;
  char *pcVar10;
  longlong *plVar11;
  char *pcVar12;
  undefined4 uVar13;
  char *local_res8;
  longlong *local_res10;
  longlong *local_res18;
  char *local_res20;
  longlong *local_e8;
  longlong *plStack_e0;
  undefined8 local_d8;
  undefined8 local_d0;
  undefined4 local_c8;
  undefined4 uStack_c4;
  undefined2 local_c0;
  undefined8 local_be;
  char *local_a8;
  longlong local_a0;
  char *local_88;
  char *local_80;
  longlong lStack_78;
  char *local_68;
  longlong local_60;
  
  ScenarioLocation__save();
  local_a8 = "difficulty";
  pcVar12 = (char *)(param_2 + 8);
  puVar1 = (undefined8 *)(param_2 + 0x50);
  plVar7 = (longlong *)(param_2 + 0x10);
  local_a0 = param_2;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e010c,1);
    *plVar7 = *plVar7 + 1;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"difficulty",10);
    *plVar7 = *plVar7 + 10;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
  }
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x108));
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&local_res8,1);
  *plVar7 = *plVar7 + 1;
  if (*pcVar12 != '\0') {
    local_res8 = pcVar12;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_14230f710,2);
    *plVar7 = *plVar7 + 2;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"difficulty",10);
    *plVar7 = *plVar7 + 10;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
  }
  local_res8 = (char *)(param_2 + 8);
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x109,1);
  *plVar7 = *plVar7 + 1;
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x10a,1);
  *plVar7 = *plVar7 + 1;
  plVar8 = (longlong *)(param_1 + 0x110);
  local_res10 = *(longlong **)(param_1 + 0x120);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res10);
  local_res18 = *(longlong **)(param_1 + 0x120);
  if (0xf < *(ulonglong *)(param_1 + 0x128)) {
    plVar8 = (longlong *)*plVar8;
    pcVar12 = local_res8;
  }
  local_res10 = (longlong *)(param_2 + 0x10);
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,plVar8,local_res18);
  *plVar7 = *plVar7 + (longlong)local_res18;
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x130,1);
  *plVar7 = *plVar7 + 1;
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x131,1);
  *plVar7 = *plVar7 + 1;
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x132));
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&local_res8,1);
  *plVar7 = *plVar7 + 1;
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x133,1);
  *plVar7 = *plVar7 + 1;
  local_a8 = "current-version";
  local_a0 = param_2;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e010c,1);
    *plVar7 = *plVar7 + 1;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"current-version",0xf);
    *plVar7 = *plVar7 + 0xf;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    plVar7 = local_res10;
  }
  plVar8 = (longlong *)(param_2 + 0x50);
  ApplicationVersion__save(&DAT_142d373e0,param_2);
  pcVar10 = pcVar12;
  plVar4 = plVar7;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_14230f710,2);
    *plVar7 = *plVar7 + 2;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"current-version",0xf);
    *plVar7 = *plVar7 + 0xf;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    pcVar10 = (char *)(param_2 + 8);
    plVar4 = (longlong *)(param_2 + 0x10);
  }
  local_res18 = plVar4;
  local_res8 = pcVar10;
  local_res10 = (longlong *)(param_2 + 0x50);
  local_a8 = "allowed-commands";
  plVar4 = plVar7;
  local_a0 = param_2;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e010c,1);
    *plVar7 = *plVar7 + 1;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"allowed-commands",0x10);
    *plVar7 = *plVar7 + 0x10;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    plVar4 = local_res18;
    pcVar12 = local_res8;
  }
  plVar11 = local_res10;
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x168));
  (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&local_res8,1);
  *plVar7 = *plVar7 + 1;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_14230f710,2);
    *plVar7 = *plVar7 + 2;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"allowed-commands",0x10);
    *plVar7 = *plVar7 + 0x10;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    plVar7 = plVar4;
    plVar8 = plVar11;
  }
  (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,param_1 + 0x16c,4);
  *plVar7 = *plVar7 + 4;
  local_a8 = "active-mods";
  local_a0 = param_2;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e010c,1);
    *plVar7 = *plVar7 + 1;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"active-mods",0xb);
    *plVar7 = *plVar7 + 0xb;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    pcVar12 = (char *)(param_2 + 8);
  }
  plVar4 = (longlong *)
           ModManager__getActiveValidSortedModIDsByOrder
                     (*(undefined8 *)(DAT_142d34fa0 + 0x160),&local_88);
  local_res8 = (char *)((plVar4[1] - *plVar4 >> 4) * -0x5555555555555555);
  uVar13 = Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                     (param_2,&local_res8);
  lVar5 = plVar4[1];
  lVar9 = *plVar4;
  if (lVar9 != lVar5) {
    do {
      uVar13 = ModID__save(lVar9,param_2);
      lVar9 = lVar9 + 0x30;
    } while (lVar9 != lVar5);
    pcVar12 = (char *)(param_2 + 8);
    plVar8 = (longlong *)(param_2 + 0x50);
    plVar7 = (longlong *)(param_2 + 0x10);
  }
  if (local_88 != (char *)0x0) {
    pcVar10 = local_88;
    if (local_88 != local_80) {
      do {
        uVar13 = std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Tidy_deallocate
                           (pcVar10);
        pcVar10 = pcVar10 + 0x30;
      } while (pcVar10 != local_80);
      plVar8 = (longlong *)(param_2 + 0x50);
      plVar7 = (longlong *)(param_2 + 0x10);
      pcVar12 = (char *)(param_2 + 8);
    }
    std__allocator<Pair<unsigned_char,IconSequencePositioning>_>__deallocate
              (uVar13,local_88,(lStack_78 - (longlong)local_88 >> 4) * -0x5555555555555555);
    local_88 = (char *)0x0;
    local_80 = (char *)0x0;
    lStack_78 = 0;
  }
  pcVar10 = pcVar12;
  plVar4 = plVar8;
  plVar11 = plVar7;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_14230f710,2);
    *plVar7 = *plVar7 + 2;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"active-mods",0xb);
    *plVar7 = *plVar7 + 0xb;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    pcVar10 = (char *)(param_2 + 8);
    plVar4 = (longlong *)(param_2 + 0x50);
    plVar11 = (longlong *)(param_2 + 0x10);
  }
  local_res18 = plVar11;
  local_res10 = plVar4;
  local_res8 = pcVar10;
  local_a8 = "startup-mod-settings-crc";
  plVar4 = plVar7;
  plVar11 = plVar8;
  local_res20 = pcVar12;
  local_a0 = param_2;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e010c,1);
    *plVar7 = *plVar7 + 1;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"startup-mod-settings-crc",0x18);
    *plVar7 = *plVar7 + 0x18;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108,1);
    *plVar7 = *plVar7 + 1;
    local_res20 = local_res8;
    plVar4 = local_res18;
    plVar11 = local_res10;
  }
  uVar13 = ModSettingsTemplate<1>__getChecksum(*(undefined8 *)(DAT_142d34fa0 + 0x128));
  local_res8 = (char *)CONCAT44(local_res8._4_4_,uVar13);
  (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&local_res8);
  *plVar7 = *plVar7 + 4;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_14230f710,2);
    *plVar7 = *plVar7 + 2;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,"startup-mod-settings-crc",0x18);
    *plVar7 = *plVar7 + 0x18;
    (**(code **)(*(longlong *)*plVar8 + 8))((longlong *)*plVar8,&DAT_1421e0108);
    *plVar7 = *plVar7 + 1;
    pcVar12 = local_res20;
  }
  local_88 = "startup-mod-settings";
  local_80 = (char *)param_2;
  if (*pcVar12 != '\0') {
    (**(code **)(*(longlong *)*plVar11 + 8))((longlong *)*plVar11,&DAT_1421e010c,1);
    *plVar4 = *plVar4 + 1;
    (**(code **)(*(longlong *)*plVar11 + 8))((longlong *)*plVar11,"startup-mod-settings",0x14);
    *plVar4 = *plVar4 + 0x14;
    (**(code **)(*(longlong *)*plVar11 + 8))((longlong *)*plVar11,&DAT_1421e0108);
    *plVar4 = *plVar4 + 1;
  }
  local_d0 = 0;
  local_c8 = 0;
  local_be = 0;
  local_e8 = (longlong *)0x0;
  plStack_e0 = (longlong *)0x0;
  local_d8 = (longlong *)0x0;
  local_c0._0_1_ = 4;
  local_c0._1_1_ = 5;
  plVar7 = *(longlong **)(*(longlong *)(DAT_142d34fa0 + 0x128) + 8);
  plVar8 = *(longlong **)(*(longlong *)(DAT_142d34fa0 + 0x128) + 0x10);
  bVar6 = local_c0._1_1_;
  while( true ) {
    local_c0._1_1_ = bVar6;
    if (plVar7 == plVar8) {
      PropertyTree__saveInternal<MapSerialiser>(&local_e8,param_2);
      plVar7 = plStack_e0;
      bVar6 = local_c0._1_1_ & 0x1f;
      if (bVar6 == 3) {
        if (local_d8._7_1_ == '\x01') {
          operator_delete__(local_e8);
        }
        else if ((local_d8._7_1_ == '\x02') && (plStack_e0 != (longlong *)0x0)) {
          LOCK();
          plVar8 = plStack_e0 + 1;
          lVar5 = *plVar8;
          *(int *)plVar8 = (int)*plVar8 + -1;
          UNLOCK();
          if ((int)lVar5 == 1) {
            (**(code **)*plStack_e0)(plStack_e0);
            LOCK();
            piVar2 = (int *)((longlong)plVar7 + 0xc);
            iVar3 = *piVar2;
            *piVar2 = *piVar2 + -1;
            UNLOCK();
            if (iVar3 == 1) {
              (**(code **)(*plVar7 + 8))(plVar7);
            }
          }
        }
        local_e8 = (longlong *)0x0;
        plStack_e0 = (longlong *)((ulonglong)plStack_e0 & 0xffffffff00000000);
        local_d8 = (longlong *)CONCAT17(4,(undefined7)local_d8);
      }
      else if (((bVar6 == 4) || (bVar6 == 5)) && (plVar8 = local_e8, local_e8 != (longlong *)0x0)) {
        for (; plVar8 != plVar7; plVar8 = (longlong *)((longlong)plVar8 + 0x32)) {
          PropertyTree___PropertyTree(plVar8);
        }
        plVar7 = local_e8;
        if ((0xfff < (ulonglong)(((longlong)local_d8 - (longlong)local_e8 >> 1) * 2)) &&
           (plVar7 = (longlong *)local_e8[-1],
           0x1f < (ulonglong)((longlong)local_e8 + (-8 - (longlong)plVar7)))) {
                    /* WARNING: Subroutine does not return */
          _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
        }
        operator_delete__(plVar7);
        local_e8 = (longlong *)0x0;
        plStack_e0 = (longlong *)0x0;
        local_d8 = (longlong *)0x0;
      }
      bVar6 = (byte)local_c0;
      local_c0 = (ushort)(byte)local_c0;
      if (bVar6 == 1) {
        operator_delete__(local_d0);
      }
      else if ((bVar6 == 2) &&
              (plVar7 = (longlong *)CONCAT44(uStack_c4,local_c8), plVar7 != (longlong *)0x0)) {
        LOCK();
        plVar8 = plVar7 + 1;
        lVar5 = *plVar8;
        *(int *)plVar8 = (int)*plVar8 + -1;
        UNLOCK();
        if ((int)lVar5 == 1) {
          (**(code **)*plVar7)(plVar7);
          LOCK();
          piVar2 = (int *)((longlong)plVar7 + 0xc);
          iVar3 = *piVar2;
          *piVar2 = *piVar2 + -1;
          UNLOCK();
          if (iVar3 == 1) {
            (**(code **)(*plVar7 + 8))(plVar7);
          }
        }
      }
      local_d0 = 0;
      local_c8 = 0;
      local_c0 = CONCAT11(local_c0._1_1_,4);
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),"startup-mod-settings",0x14);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x14;
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      }
      EndingInfo__save(param_1 + 0x3d8,param_2);
      EndingInfo__save(param_1 + 0x548,param_2);
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),param_1 + 0x134,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      return;
    }
    lVar5 = *(longlong *)(*plVar7 + 8);
    local_a8 = (char *)(lVar5 + 8);
    if (0xf < *(ulonglong *)(lVar5 + 0x20)) {
      local_a8 = *(char **)(lVar5 + 8);
    }
    local_a0 = *(longlong *)(lVar5 + 0x18);
    if ((bVar6 & 0x1f) == 0) {
      local_e8 = (longlong *)0x0;
      plStack_e0 = (longlong *)0x0;
      local_d8 = (longlong *)0x0;
      bVar6 = bVar6 & 0xe5 | 5;
    }
    else if ((bVar6 & 0x1f) != 5) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__basic_string<char,std::char_traits<char>,std::allocator<char>_>
                (&local_68,"dictionary or none");
      PropertyTreeTypeError__PropertyTreeTypeError(&local_a8,&local_e8,&local_68);
                    /* WARNING: Subroutine does not return */
      _CxxThrowException(&local_a8,(ThrowInfo *)&DAT_1427b2020);
    }
    plVar4 = local_d8;
    local_c0._1_1_ = bVar6 | 0x80;
    if ((bVar6 & 0x1f) != 5) break;
    local_68 = local_a8;
    local_60 = local_a0;
    lVar5 = PropertyTree__findRaw(&local_e8,&local_68);
    if (lVar5 == ((longlong)plStack_e0 - (longlong)local_e8 >> 1) * 2 + (longlong)local_e8) {
      if (plStack_e0 == plVar4) {
        std__vector<PropertyTree,std::allocator<PropertyTree>_>___Emplace_reallocate<>(&local_e8);
      }
      else {
        plStack_e0[3] = 0;
        *(undefined4 *)(plStack_e0 + 4) = 0;
        *(undefined2 *)(plStack_e0 + 5) = 4;
        *(undefined8 *)((longlong)plStack_e0 + 0x2a) = 0;
        plStack_e0 = (longlong *)((longlong)plStack_e0 + 0x32);
      }
      plVar4 = plStack_e0;
      lVar5 = (longlong)plStack_e0 + -0x32;
      ImmutableStringTemplate<17>__operator_<std::basic_string_view<char,std::char_traits<char>_>_&>
                ((longlong)plStack_e0 + -0x1a,&local_a8);
      plVar4[-1] = (longlong)&local_e8;
    }
    PropertyTree__clear(lVar5);
    (**(code **)(*(longlong *)*plVar7 + 0x10))((longlong *)*plVar7,lVar5);
    plVar7 = plVar7 + 1;
    bVar6 = local_c0._1_1_;
  }
  std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__basic_string<char,std::char_traits<char>,std::allocator<char>_>
            (&local_68,"dictionary");
  PropertyTreeTypeError__PropertyTreeTypeError(&local_a8,&local_e8,&local_68);
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(&local_a8,(ThrowInfo *)&DAT_1427b2020);
}

