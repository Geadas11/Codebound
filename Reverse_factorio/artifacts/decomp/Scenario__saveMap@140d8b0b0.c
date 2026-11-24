// Scenario::saveMap @ 140d8b0b0 (function Scenario::saveMap)

void Scenario__saveMap(longlong param_1,uint param_2,longlong *param_3,undefined8 param_4,
                      undefined8 param_5,byte *param_6)

{
  ulonglong uVar1;
  uint uVar2;
  longlong ***ppplVar3;
  code *pcVar4;
  longlong *plVar5;
  char cVar6;
  byte bVar7;
  int iVar8;
  longlong *plVar9;
  longlong *****ppppplVar10;
  undefined8 uVar11;
  longlong *****ppppplVar12;
  longlong *plVar13;
  undefined8 *puVar14;
  uint uVar15;
  ulonglong uVar16;
  longlong lVar17;
  longlong *****ppppplVar18;
  undefined4 uVar19;
  uint uVar20;
  ulonglong uVar21;
  char *pcVar22;
  double _X;
  double extraout_XMM0_Qa;
  undefined8 in_stack_fffffffffffffa38;
  undefined4 uVar23;
  undefined1 local_580 [8];
  longlong ****local_578;
  ulonglong uStack_570;
  longlong local_568;
  ulonglong uStack_560;
  undefined8 local_558;
  longlong ****local_550;
  longlong ****local_548;
  undefined8 local_540;
  undefined8 uStack_538;
  longlong local_530;
  ulonglong local_528;
  longlong ****local_520;
  ulonglong uStack_518;
  longlong local_510;
  ulonglong uStack_508;
  longlong ****local_500 [2];
  undefined **local_4f0;
  undefined4 local_4e8;
  longlong local_4e0;
  longlong local_4d8;
  longlong local_4d0;
  undefined8 uStack_4c8;
  longlong local_4c0;
  ulonglong local_4b8;
  undefined8 *local_4b0;
  undefined8 uStack_4a8;
  longlong *local_4a0;
  longlong *local_498;
  char local_47c [12];
  undefined1 local_470 [8];
  undefined1 local_468;
  undefined1 local_467;
  undefined1 local_466;
  char local_465;
  longlong *local_430;
  longlong ****local_378;
  undefined1 *local_370;
  undefined1 local_300 [640];
  char local_80;
  
  uVar23 = (undefined4)((ulonglong)in_stack_fffffffffffffa38 >> 0x20);
  ppppplVar18 = (longlong *****)0x0;
  uVar19 = 0;
  local_558 = (longlong *****)((ulonglong)local_558 & 0xffffffff00000000);
  uStack_538 = 0;
  local_530 = 5;
  local_528 = 0xf;
  local_540 = (char *)(ulonglong)CONCAT14(s_level_1421ebbb0[4],s_level_1421ebbb0._0_4_);
  local_580[0] = param_2 == 3;
  if ((bool)local_580[0]) {
    pcVar22 = local_47c + 1;
    uVar21 = *(ulonglong *)(*(longlong *)(param_1 + 0x720) + 0x1a8);
    do {
      pcVar22 = pcVar22 + -1;
      uVar16 = uVar21 / 10;
      *pcVar22 = (char)uVar21 + (char)uVar16 * -10 + '0';
      uVar21 = uVar16;
    } while (uVar16 != 0);
    local_578 = (longlong ****)0x0;
    uStack_570 = 0;
    local_568 = 0;
    uStack_560 = 0;
    if (pcVar22 == local_47c + 1) {
      local_568 = 0;
      uStack_560 = 0xf;
      local_578 = (longlong ****)0x0;
    }
    else {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Construct<1,char_*>
                (&local_578,pcVar22,local_47c + (1 - (longlong)pcVar22));
    }
    plVar9 = (longlong *)
             std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__insert
                       (&local_578,0,"_with_tags_tick_");
    local_520 = (longlong ****)*plVar9;
    uStack_518 = plVar9[1];
    local_510 = plVar9[2];
    uStack_508 = plVar9[3];
    plVar9[2] = 0;
    plVar9[3] = 0xf;
    *(undefined1 *)plVar9 = 0;
    uVar19 = 0x1f;
    ppppplVar10 = &local_520;
    if (0xf < uStack_508) {
      ppppplVar10 = (longlong *****)local_520;
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (&local_540,ppppplVar10);
    if (0xf < uStack_508) {
      if ((0xfff < uStack_508 + 1) &&
         (0x1f < (ulonglong)((longlong)local_520 + (-8 - (longlong)local_520[-1])))) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
      operator_delete__();
    }
    if (0xf < uStack_560) {
      if ((0xfff < uStack_560 + 1) &&
         (0x1f < (ulonglong)((longlong)local_578 + (-8 - (longlong)local_578[-1])))) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
      operator_delete__();
    }
  }
  std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
            (&local_540,&DAT_1422ad850);
  uVar21 = local_528;
  local_500[0] = (longlong ****)0x0;
  local_548 = (longlong ****)0x0;
  ppppplVar10 = ppppplVar18;
  if (param_2 == 4) {
    if (local_528 < 0xd) {
      uVar16 = 0x7fffffffffffffff;
      if (local_528 <= 0x7fffffffffffffff - (local_528 >> 1)) {
        uVar1 = local_528 + (local_528 >> 1);
        uVar16 = 0xf;
        if (0xf < uVar1) {
          uVar16 = uVar1;
        }
      }
      pcVar22 = (char *)std___Allocate<16,std::_Default_allocate_traits>(uVar16 + 1);
      local_530 = 0xd;
      *(undefined8 *)pcVar22 = s_blueprint_dat_1421e89f8._0_8_;
      *(undefined4 *)(pcVar22 + 8) = s_blueprint_dat_1421e89f8._8_4_;
      pcVar22[0xc] = s_blueprint_dat_1421e89f8[0xc];
      pcVar22[0xd] = '\0';
      local_528 = uVar16;
      if (0xf < uVar21) {
        if ((0xfff < uVar21 + 1) &&
           ((char *)0x1f < local_540 + (-8 - *(longlong *)(local_540 + -8)))) {
                    /* WARNING: Subroutine does not return */
          _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
        }
        operator_delete__();
      }
    }
    else {
      pcVar22 = (char *)&local_540;
      if (0xf < local_528) {
        pcVar22 = local_540;
      }
      local_530 = 0xd;
      memcpy(pcVar22,"blueprint.dat",0xd);
      pcVar22[0xd] = '\0';
      pcVar22 = local_540;
    }
    local_540 = pcVar22;
    cVar6 = (**(code **)(*param_3 + 0x28))(param_3);
    if (cVar6 == '\0') goto LAB_140d8b4f0;
    ppppplVar10 = (longlong *****)operator_new(0x98);
    local_568 = 9;
    uStack_560 = 0xf;
    local_578 = (longlong ****)s_blueprint_1421e3f48._0_8_;
    uStack_570 = (ulonglong)(byte)s_blueprint_1421e3f48[8];
    local_558 = (longlong *****)(CONCAT44(local_558._4_4_,uVar19) | 0x80);
    local_520 = (longlong ****)ppppplVar10;
    ZipPackageWriter__ZipPackageWriter
              (ppppplVar10,param_3 + 5,&local_578,&local_578,CONCAT44(uVar23,1));
    *ppppplVar10 = (longlong ****)ZipPackageWriter::vftable;
    local_558 = (longlong *****)(CONCAT44(local_558._4_4_,uVar19) | 0x180);
    if (0xf < uStack_560) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>___Deallocate_for_capacity
                (&local_578,local_578);
    }
    ppplVar3 = (*ppppplVar10)[1];
    local_500[0] = (longlong ****)ppppplVar10;
    uVar11 = Filesystem__Path__Path(&local_578,&local_540);
    ppppplVar12 = (longlong *****)(*(code *)ppplVar3)(ppppplVar10,&local_558,uVar11);
    puVar14 = local_558;
    if (&local_548 != ppppplVar12) {
      ppppplVar18 = (longlong *****)*ppppplVar12;
      *ppppplVar12 = (longlong ****)0x0;
      local_548 = (longlong ****)ppppplVar18;
    }
    local_558 = (longlong *****)0x0;
    if (puVar14 != (undefined8 *)0x0) {
      (**(code **)*puVar14)(puVar14,1);
    }
    if (7 < uStack_560) {
      std__basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t>_>___Deallocate_for_capacity
                (&local_578,local_578);
    }
    local_568 = 0;
    uStack_560 = 7;
    local_578 = (longlong ****)((ulonglong)local_578 & 0xffffffffffff0000);
    if (ppppplVar18 == (longlong *****)0x0) goto LAB_140d8b4f0;
  }
  else {
LAB_140d8b4f0:
    pcVar22 = (char *)&local_540;
    if (0xf < local_528) {
      pcVar22 = local_540;
    }
    if (((local_530 == 9) && (iVar8 = memcmp(pcVar22,"level.dat",9), iVar8 == 0)) &&
       (*(char *)(*(longlong *)(DAT_142d34fa0 + 0x240) + 0x2968) != '\0')) {
      pcVar4 = *(code **)(*param_3 + 0x18);
      uVar11 = Filesystem__Path__Path(&local_578,&local_540);
      ppppplVar12 = (longlong *****)(*pcVar4)(param_3,&local_558,uVar11);
      puVar14 = local_558;
      if (&local_548 != ppppplVar12) {
        ppppplVar18 = (longlong *****)*ppppplVar12;
        *ppppplVar12 = (longlong ****)0x0;
        local_548 = (longlong ****)ppppplVar18;
      }
      local_558 = (longlong *****)0x0;
      if (puVar14 != (undefined8 *)0x0) {
        (**(code **)*puVar14)(puVar14,1);
      }
    }
    else {
      pcVar4 = *(code **)(*param_3 + 0x10);
      uVar11 = Filesystem__Path__Path(&local_578,&local_540);
      ppppplVar12 = (longlong *****)(*pcVar4)(param_3,&local_558,uVar11);
      puVar14 = local_558;
      if (&local_548 != ppppplVar12) {
        ppppplVar18 = (longlong *****)*ppppplVar12;
        *ppppplVar12 = (longlong ****)0x0;
        local_548 = (longlong ****)ppppplVar18;
      }
      local_558 = (longlong *****)0x0;
      if (puVar14 != (undefined8 *)0x0) {
        (**(code **)*puVar14)(puVar14,1);
      }
    }
    if (7 < uStack_560) {
      std__basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t>_>___Deallocate_for_capacity
                (&local_578,local_578);
    }
  }
  plVar9 = (longlong *)0x0;
  local_548 = (longlong ****)0x0;
  local_558 = ppppplVar18;
  local_550 = (longlong ****)ppppplVar18;
  MapSerialiser__MapSerialiser(local_470,&local_558,param_4);
  local_468 = local_580[0];
  local_467 = local_580[0];
  local_466 = param_2 == 4;
  if (param_2 != 3) {
    cVar6 = *(char *)(*(longlong *)(DAT_142d34fa0 + 0x240) + 600);
    plVar13 = local_430;
    if (local_465 != cVar6) {
      plVar13 = plVar9;
      local_465 = cVar6;
      if (cVar6 != '\0') {
        local_520 = (longlong ****)operator_new(0x10);
        plVar13 = (longlong *)
                  std__map<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>,unsigned_int,std::less<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_>,std::allocator<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned_int>_>_>__map<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>,unsigned_int,std::less<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_>,std::allocator<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned_int>_>_>
                            (local_520);
      }
      plVar5 = local_430;
      if (local_430 != (longlong *)0x0) {
        lVar17 = *local_430;
        local_430 = plVar13;
        std___Tree_val<std::_Tree_simple_types<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned_char>_>_>___Erase_tree<std::allocator<std::_Tree_node<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned_char>,void_*>_>_>
                  (plVar5,plVar5,*(undefined8 *)(lVar17 + 8));
        operator_delete__(*plVar5,0x48);
        operator_delete__(plVar5,0x10);
        plVar13 = local_430;
      }
    }
    local_430 = plVar13;
    if (*(char *)(*(longlong *)(DAT_142d34fa0 + 0x240) + 0x24e8) != '\0') {
      local_520 = (longlong ****)operator_new(200);
      *local_520 = (longlong ***)local_520;
      local_520[1] = (longlong ***)local_520;
      local_520[2] = (longlong ***)0x0;
      local_520[3] = (longlong ***)0x0;
      local_520[4] = (longlong ***)0x0;
      local_520[5] = (longlong ***)0x0;
      local_520[6] = (longlong ***)0x0;
      local_520[7] = (longlong ***)0x0;
      local_520[8] = (longlong ***)0x0;
      local_520[9] = (longlong ***)0x0;
      local_520[10] = (longlong ***)0x0;
      local_520[0xb] = (longlong ***)0x0;
      local_520[0xc] = (longlong ***)0x0;
      local_520[0xd] = (longlong ***)0x0;
      local_520[0xe] = (longlong ***)0x0;
      local_520[0xf] = (longlong ***)0x0;
      local_520[0x10] = (longlong ***)0x0;
      local_520[0x11] = (longlong ***)0x0;
      local_520[0x12] = (longlong ***)0x0;
      local_520[0x13] = (longlong ***)0x0;
      ppppplVar18 = (longlong *****)(local_520 + 0x14);
      *ppppplVar18 = (longlong ****)ppppplVar18;
      local_520[0x15] = (longlong ***)ppppplVar18;
      ppppplVar18 = (longlong *****)(local_520 + 0x16);
      *ppppplVar18 = (longlong ****)ppppplVar18;
      local_520[0x17] = (longlong ***)ppppplVar18;
      *(undefined1 *)(local_520 + 0x18) = 0;
      DAT_142d358bc = (short)(DAT_1429c51f8 - DAT_1429c51f0 >> 3) + -1;
      local_378 = local_520;
    }
    if (param_2 == 4) goto LAB_140d8b78a;
  }
  ScenarioExecutionContext__save(param_1 + 8,local_470);
LAB_140d8b78a:
  local_80 = '\0';
  if (((param_2 != 3) && ((param_2 & 0xfffffffd) == 0)) &&
     (*(char *)(*(longlong *)(DAT_142d34fa0 + 0x240) + 0x1b28) != '\0')) {
    ThreadedChunkTileSaver__ThreadedChunkTileSaver
              (local_300,*(undefined8 *)(param_1 + 0x720),local_470);
    local_80 = '\x01';
    local_370 = local_300;
  }
  Map__save(*(undefined8 *)(param_1 + 0x720),local_470);
  if (((longlong *****)local_378 == (longlong *****)0x0) || (*(char *)(local_378 + 0x18) == '\0')) {
    bVar7 = 0;
  }
  else {
    bVar7 = 1;
  }
  *param_6 = *param_6 | bVar7;
  if (local_80 != '\0') {
    ThreadedChunkTileSaver___scalar_deleting_destructor_(local_300);
  }
  MapSerialiser___MapSerialiser(local_470);
  if (ppppplVar10 != (longlong *****)0x0) {
    (*(code *)**ppppplVar10)(ppppplVar10,1);
  }
  if (param_2 == 3) {
    pcVar4 = *(code **)(*param_3 + 0x10);
    uVar11 = wstringConvert();
    std__wstring_convert<std::codecvt_utf8<wchar_t,1114111,0>,wchar_t,std::allocator<wchar_t>,std::allocator<char>_>__from_bytes
              (uVar11,&local_520,"latest_input_actions.dat","");
    local_578 = local_520;
    uStack_570 = uStack_518;
    local_568 = local_510;
    uStack_560 = uStack_508;
    puVar14 = (undefined8 *)(*pcVar4)(param_3,local_500,&local_578);
    local_4a0 = (longlong *)*puVar14;
    *puVar14 = 0;
    local_4f0 = Serialiser::vftable;
    local_4e8 = 0;
    local_4e0 = 0;
    local_4d8 = 0;
    uStack_4c8 = 0;
    local_4c0 = 0;
    local_4b8 = 0xf;
    local_4d0 = 0;
    local_4b0 = (undefined8 *)0x0;
    uStack_4a8 = 0;
    local_498 = local_4a0;
    if (7 < uStack_560) {
      std__basic_string<wchar_t,std::char_traits<wchar_t>,std::allocator<wchar_t>_>___Deallocate_for_capacity
                (&local_578,local_578);
    }
    local_568 = 0;
    uStack_560 = 7;
    local_578 = (longlong ****)((ulonglong)local_578 & 0xffffffffffff0000);
    local_4e8 = CONCAT22(local_4e8._2_2_,0x101);
    lVar17 = *(longlong *)(param_1 + 0x720);
    uVar2 = *(uint *)(lVar17 + 0x698);
    if (uVar2 < 0xff) {
      local_580[0] = (undefined1)uVar2;
      _X = (double)(**(code **)(*local_4a0 + 8))();
      local_4e0 = local_4e0 + 1;
    }
    else {
      local_580[0] = 0xff;
      (**(code **)(*local_4a0 + 8))(local_4a0,local_580);
      local_4e0 = local_4e0 + 1;
      local_558 = (longlong *****)CONCAT44(local_558._4_4_,uVar2);
      _X = (double)(**(code **)(*local_4a0 + 8))(local_4a0,&local_558);
      local_4e0 = local_4e0 + 4;
    }
    uVar2 = *(uint *)(lVar17 + 0x698);
    if (uVar2 != 0) {
      do {
        uVar15 = *(int *)(lVar17 + 0x694) + (int)plVar9;
        uVar20 = uVar15 - *(int *)(lVar17 + 0x690);
        if (uVar15 < *(uint *)(lVar17 + 0x690)) {
          uVar20 = uVar15;
        }
        _X = (double)InputAction__save((ulonglong)uVar20 * 0x80 + *(longlong *)(lVar17 + 0x688),
                                       &local_4f0);
        uVar20 = (int)plVar9 + 1;
        plVar9 = (longlong *)(ulonglong)uVar20;
      } while (uVar20 != uVar2);
    }
    plVar9 = local_498;
    local_4f0 = Serialiser::vftable;
    local_4a0 = (longlong *)0x0;
    local_498 = (undefined8 *)0x0;
    if (plVar9 != (longlong *)0x0) {
      _X = (double)(**(code **)*plVar9)(plVar9,1);
    }
    if (local_4e8._3_1_ != '\0') {
      if (local_4c0 != 0) {
        lVar17 = local_4e0 - local_4d8;
        plVar9 = (longlong *)
                 std__map<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>,unsigned___int64,std::less<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_>,std::allocator<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned___int64>_>_>___Try_emplace<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_&>
                           (local_4b0,&local_520);
        *(longlong *)(*plVar9 + 0x40) = *(longlong *)(*plVar9 + 0x40) + lVar17;
        _X = extraout_XMM0_Qa;
      }
      plVar9 = *(longlong **)*local_4b0;
      cVar6 = *(char *)((longlong)plVar9 + 0x19);
      local_4d8 = local_4e0;
      while (cVar6 == '\0') {
        if (plVar9[8] != 0) {
          _X = Logging__log(_X);
        }
        plVar13 = (longlong *)plVar9[2];
        if (*(char *)((longlong)plVar13 + 0x19) == '\0') {
          cVar6 = *(char *)(*plVar13 + 0x19);
          plVar9 = plVar13;
          plVar13 = (longlong *)*plVar13;
          while (cVar6 == '\0') {
            cVar6 = *(char *)(*plVar13 + 0x19);
            plVar9 = plVar13;
            plVar13 = (longlong *)*plVar13;
          }
        }
        else {
          cVar6 = *(char *)(plVar9[1] + 0x19);
          plVar5 = (longlong *)plVar9[1];
          plVar13 = plVar9;
          while ((plVar9 = plVar5, cVar6 == '\0' && (plVar13 == (longlong *)plVar9[2]))) {
            cVar6 = *(char *)(plVar9[1] + 0x19);
            plVar5 = (longlong *)plVar9[1];
            plVar13 = plVar9;
          }
        }
        cVar6 = *(char *)((longlong)plVar9 + 0x19);
      }
    }
    plVar9 = local_498;
    local_498 = (longlong *)0x0;
    if (plVar9 != (undefined8 *)0x0) {
      (**(code **)*plVar9)(plVar9,1);
    }
    puVar14 = local_4b0;
    if (local_4b0 != (undefined8 *)0x0) {
      std___Tree_val<std::_Tree_simple_types<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned_char>_>_>___Erase_tree<std::allocator<std::_Tree_node<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,unsigned_char>,void_*>_>_>
                (local_4b0,local_4b0);
      operator_delete__(*puVar14,0x48);
      operator_delete__(puVar14,0x10);
    }
    if (0xf < local_4b8) {
      if ((0xfff < local_4b8 + 1) && (0x1f < (local_4d0 - *(longlong *)(local_4d0 + -8)) - 8U)) {
                    /* WARNING: Subroutine does not return */
        _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
      }
      operator_delete__();
    }
  }
  if (0xf < local_528) {
    if ((0xfff < local_528 + 1) && ((char *)0x1f < local_540 + (-8 - *(longlong *)(local_540 + -8)))
       ) {
                    /* WARNING: Subroutine does not return */
      _invoke_watson((wchar_t *)0x0,(wchar_t *)0x0,(wchar_t *)0x0,0,0);
    }
    operator_delete__();
  }
  return;
}

