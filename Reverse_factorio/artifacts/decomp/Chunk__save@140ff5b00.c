// Chunk::save @ 140ff5b00 (function Chunk::save)

/* WARNING: Function: __chkstk replaced with injection: alloca_probe */

void Chunk__save(ulonglong param_1,longlong param_2,int *param_3)

{
  uint *puVar1;
  char cVar2;
  undefined8 *puVar3;
  longlong lVar4;
  ulonglong uVar5;
  ushort uVar6;
  ulonglong uVar7;
  int iVar8;
  uint uVar9;
  longlong *plVar10;
  undefined8 uVar11;
  undefined8 *puVar12;
  uint uVar13;
  longlong *plVar14;
  undefined8 *puVar15;
  char *pcVar16;
  longlong lVar17;
  undefined8 *puVar18;
  char *pcVar19;
  uint uVar20;
  undefined8 *puVar21;
  undefined8 *puVar22;
  undefined8 *puVar23;
  longlong *plVar24;
  ulonglong local_res8;
  undefined8 local_res18;
  char *local_12b8;
  longlong local_12b0;
  longlong local_12a8;
  longlong local_12a0;
  undefined1 local_1298 [24];
  char *local_1280;
  longlong local_1278;
  undefined1 local_1258 [4096];
  undefined2 local_258;
  undefined1 local_250 [512];
  undefined1 local_50;
  undefined8 local_48;
  undefined8 uStack_40;
  
  uStack_40 = 0x140ff5b27;
  local_res8 = param_1;
  if ((*(int *)(&DAT_00001450 + param_1) != *param_3) ||
     (*(int *)(&DAT_00001454 + param_1) != param_3[1])) {
    uVar11 = ssprintf(&local_1280,
                      "Saving chunk error, chunk position is (%d, %d), but saving as (%d, %d)",
                      *(int *)(&DAT_00001450 + param_1),*(undefined4 *)(&DAT_00001454 + param_1),
                      *param_3,param_3[1]);
    RuntimeError__RuntimeError(local_1298,uVar11);
                    /* WARNING: Subroutine does not return */
    _CxxThrowException(local_1298,(ThrowInfo *)&DAT_1427b2490);
  }
  local_1280 = "chunk";
  local_1278 = param_2;
  if (*(char *)(param_2 + 0xb) != '\0') {
    Serialiser__calculateWrites(param_2);
    if (*(longlong *)(param_2 + 0x30) != 0) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 0x20,&DAT_1421da9f0,1);
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (param_2 + 0x20,"chunk",5);
  }
  Serialiser__TagWriter__TagWriter
            (&local_12a8,param_2,"chunk","position=\"(%d, %d)\"",
             *(undefined4 *)(&DAT_00001450 + param_1),*(undefined4 *)(&DAT_00001454 + param_1));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),&DAT_1420c53b8,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1428,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  puVar21 = (undefined8 *)0x0;
  if ((*(char *)(param_2 + 9) != '\0') || (9 < *(byte *)(param_1 + 0x142c))) {
    local_12b8 = "tiles";
    local_12b0 = param_2;
    if (*(char *)(param_2 + 0xb) != '\0') {
      Serialiser__calculateWrites(param_2);
      if (*(longlong *)(param_2 + 0x30) != 0) {
        std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                  (param_2 + 0x20,&DAT_1421da9f0,1);
      }
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 0x20,"tiles",5);
    }
    pcVar16 = *(char **)(param_2 + 0x100);
    if (pcVar16 == (char *)0x0) {
      local_258 = 0;
      memset(local_250,0,0x200);
      local_50 = 0;
      local_48 = 0;
      Tile__prepareSaveData(local_1258,param_1,*(longlong *)(param_2 + 0xf8) != 0);
      Tile__savePreparedData(param_2,param_1,local_1258);
    }
    else {
      uVar9 = *(uint *)(pcVar16 + 0x140);
      if ((ulonglong)(*(longlong *)(pcVar16 + 0xd8) - *(longlong *)(pcVar16 + 0xd0) >> 3) <=
          (ulonglong)uVar9) {
        RuntimeError__RuntimeError
                  (local_1298,"Saving chunk tiles error, expecting data but thread has ended");
                    /* WARNING: Subroutine does not return */
        _CxxThrowException(local_1298,(ThrowInfo *)&DAT_1427b2490);
      }
      *(uint *)(pcVar16 + 0x140) = uVar9 + 1;
      iVar8 = _Mtx_lock(pcVar16 + 0x20);
      if (iVar8 != 0) {
                    /* WARNING: Subroutine does not return */
        std___Throw_Cpp_error(5);
      }
      if (*(int *)(pcVar16 + 0x6c) == 0x7fffffff) {
        pcVar16[0x6c] = -2;
        pcVar16[0x6d] = -1;
        pcVar16[0x6e] = -1;
        pcVar16[0x6f] = '\x7f';
                    /* WARNING: Subroutine does not return */
        std___Throw_Cpp_error(6);
      }
      cVar2 = *pcVar16;
      while( true ) {
        if (cVar2 != '\0') {
          _Mtx_unlock(pcVar16 + 0x20);
                    /* WARNING: Subroutine does not return */
          ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Map\\ThreadedChunkTileSaver.cpp"
                              ,0x31,"buffer");
        }
        lVar17 = *(longlong *)(pcVar16 + (ulonglong)(uVar9 & 0x1f) * 8 + 0x148);
        if (lVar17 != 0) break;
        _Cnd_wait(pcVar16 + 0x70,pcVar16 + 0x20);
        cVar2 = *pcVar16;
      }
      pcVar19 = pcVar16 + (ulonglong)(uVar9 & 0x1f) * 8 + 0x148;
      pcVar19[0] = '\0';
      pcVar19[1] = '\0';
      pcVar19[2] = '\0';
      pcVar19[3] = '\0';
      pcVar19[4] = '\0';
      pcVar19[5] = '\0';
      pcVar19[6] = '\0';
      pcVar19[7] = '\0';
      _Mtx_unlock(pcVar16 + 0x20);
      Tile__savePreparedData(param_2,param_1,lVar17);
      ThreadedChunkTileSaver__sendFreeData(pcVar16,lVar17);
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14207f270,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    Serialiser__SavingStatsWriter___SavingStatsWriter(&local_12b8);
  }
  local_res18 = (undefined8 *)
                CONCAT44(*(int *)(&DAT_00001454 + param_1) << 0xd,
                         *(int *)(&DAT_00001450 + param_1) << 0xd);
  *(undefined8 **)(param_2 + 0x138) = local_res18;
  local_res18 = (undefined8 *)
                (*(longlong *)(param_1 + 0x1510) - *(longlong *)(param_1 + 0x1508) >> 4);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res18);
  puVar23 = *(undefined8 **)(param_1 + 0x1510);
  for (puVar22 = *(undefined8 **)(param_1 + 0x1508); puVar22 != puVar23; puVar22 = puVar22 + 2) {
    local_res18 = puVar21;
    for (puVar18 = (undefined8 *)*puVar22; puVar18 != puVar22; puVar18 = (undefined8 *)*puVar18) {
      local_res18 = (undefined8 *)((longlong)local_res18 + 1);
    }
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
              (param_2,&local_res18);
    for (puVar18 = (undefined8 *)*puVar22; puVar18 != puVar22; puVar18 = (undefined8 *)*puVar18) {
      RawPointerToTargetable__save(param_2,puVar18[2]);
    }
  }
  puVar23 = (undefined8 *)(*(longlong *)(param_1 + 0x1568) - *(longlong *)(param_1 + 0x1560) >> 3);
  if (puVar23 != (undefined8 *)0x0) {
    plVar14 = (longlong *)(*(longlong *)(param_1 + 0x1560) + -8 + (longlong)puVar23 * 8);
    do {
      plVar10 = (longlong *)*plVar14;
      plVar24 = plVar10 + 8;
      for (; plVar10 != plVar24; plVar10 = plVar10 + 2) {
        if ((longlong *)*plVar10 != plVar10) goto LAB_140ff5ecb;
      }
      puVar23 = (undefined8 *)((longlong)puVar23 + -1);
      plVar14 = plVar14 + -1;
    } while (puVar23 != (undefined8 *)0x0);
  }
LAB_140ff5ecb:
  local_res18 = puVar23;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res18);
  puVar22 = puVar21;
  if (puVar23 != (undefined8 *)0x0) {
    do {
      puVar18 = *(undefined8 **)(*(longlong *)(local_res8 + 0x1560) + (longlong)puVar22 * 8);
      puVar15 = puVar18;
      local_res18 = puVar21;
      if (puVar18 == puVar18 + 8) {
        local_res18 = (undefined8 *)0x0;
        Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                  (param_2,&local_res18);
      }
      else {
        do {
          puVar12 = puVar21;
          for (puVar3 = (undefined8 *)*puVar15; puVar3 != puVar15; puVar3 = (undefined8 *)*puVar3) {
            puVar12 = (undefined8 *)((longlong)puVar12 + 1);
          }
          local_res18 = (undefined8 *)((longlong)local_res18 + (longlong)puVar12);
          puVar15 = puVar15 + 2;
        } while (puVar15 != puVar18 + 8);
        Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                  (param_2,&local_res18);
        puVar15 = puVar18 + 8;
        do {
          for (puVar3 = (undefined8 *)*puVar18; puVar3 != puVar18; puVar3 = (undefined8 *)*puVar3) {
            puVar12 = puVar3 + -1;
            if (puVar3 == (undefined8 *)0x0) {
              puVar12 = puVar21;
            }
            uVar11 = (**(code **)*puVar12)();
            RawPointerToTargetable__save(param_2,uVar11);
          }
          puVar18 = puVar18 + 2;
        } while (puVar18 != puVar15);
      }
      puVar22 = (undefined8 *)(ulonglong)((int)puVar22 + 1);
    } while (puVar22 < puVar23);
  }
  uVar7 = local_res8;
  MapSerialiser__operator<<<ActiveEntities>(param_2,local_res8 + 0x1480);
  MapSerialiser__operator<<<ActiveEntities>(param_2,uVar7 + 0x1490);
  local_12b8 = "entity";
  local_12b0 = param_2;
  if (*(char *)(param_2 + 0xb) != '\0') {
    Serialiser__calculateWrites(param_2);
    if (*(longlong *)(param_2 + 0x30) != 0) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_2 + 0x20,&DAT_1421da9f0,1);
    }
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
              (param_2 + 0x20,"entity",6);
  }
  uVar5 = uVar7;
  for (plVar14 = (longlong *)(uVar7 + 0xc28); plVar14 != (longlong *)(uVar7 + 0x1428);
      plVar14 = plVar14 + 0x10) {
    for (plVar24 = plVar14; plVar24 != plVar14 + 0x10; plVar24 = plVar24 + 1) {
      lVar17 = *plVar24;
      if (lVar17 != 0) {
        for (lVar4 = *(longlong *)(lVar17 + 8); lVar4 != 0; lVar4 = *(longlong *)(lVar4 + 8)) {
          lVar17 = lVar4;
        }
        do {
          Chunk__saveNonRecursive();
          lVar17 = *(longlong *)(lVar17 + 0x10);
        } while (lVar17 != 0);
      }
    }
    uVar5 = local_res8;
  }
  local_res8 = local_res8 & 0xffffffffffff0000;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  uVar6 = (ushort)local_res8;
  if (((ushort)local_res8 != 0) && (lVar17 = *(longlong *)(param_2 + 0x48), lVar17 != 0)) {
    uVar9 = (uint)(ushort)local_res8;
    if (*(ulonglong *)(lVar17 + 0x58) <= (local_res8 & 0xffff)) {
      std__vector<bool,std::allocator<bool>_>__resize(lVar17 + 0x40,(local_res8 & 0xffff) + 1);
    }
    puVar1 = (uint *)(*(longlong *)(lVar17 + 0x40) + (ulonglong)(uVar6 >> 5) * 4);
    *puVar1 = *puVar1 | 1 << (uVar9 & 0x1f);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),&DAT_1420ca680,2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
  Serialiser__SavingStatsWriter___SavingStatsWriter(&local_12b8);
  local_12b8 = "tick-of-optional-activation";
  local_12b0 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-of-optional-activation",0x1b);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x1b;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),uVar5 + 0x14a0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-of-optional-activation",0x1b);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x1b;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_12b8 = "tick-of-last-change-that-could-affect-charting";
  local_12b0 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-of-last-change-that-could-affect-charting",0x2e)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x2e;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),uVar5 + 0x14a8,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  pcVar16 = (char *)(param_2 + 8);
  if (*pcVar16 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"tick-of-last-change-that-could-affect-charting",0x2e)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x2e;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_12b8 = "pollution";
  local_12b0 = param_2;
  if (*pcVar16 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"pollution",9)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 9;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),&DAT_000014b0 + uVar5,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*pcVar16 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"pollution",9)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 9;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_12b8 = "trivial-smokes";
  local_12b0 = param_2;
  if (*pcVar16 != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"trivial-smokes",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  Serialiser__saveContainerSize<unsigned_int>(param_2,*(undefined4 *)(uVar5 + 0x1468));
  uVar9 = *(uint *)(uVar5 + 0x1468);
  if (uVar9 != 0) {
    do {
      uVar13 = *(int *)(uVar5 + 0x1464) + (int)puVar21;
      uVar20 = uVar13 - *(uint *)(uVar5 + 0x1460);
      if (uVar13 < *(uint *)(uVar5 + 0x1460)) {
        uVar20 = uVar13;
      }
      TrivialSmoke__save((ulonglong)uVar20 * 0x20 + *(longlong *)(uVar5 + 0x1458),param_2);
      uVar20 = (int)puVar21 + 1;
      puVar21 = (undefined8 *)(ulonglong)uVar20;
    } while (uVar20 != uVar9);
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"trivial-smokes",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  ChunkDecoratives__save(uVar5 + 0x1430,param_2);
  if (*(char *)(param_2 + 9) != '\0') {
    local_12b8 = "player-building-count";
    local_12b0 = param_2;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"player-building-count",0x15);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x15;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    pcVar16 = (char *)(param_2 + 8);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14e0,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"player-building-count",0x15);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x15;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_12b8 = "spawner-count";
    local_12b0 = param_2;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"spawner-count",0xd);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xd;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14e4,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"spawner-count",0xd);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xd;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    pcVar19 = (char *)(param_2 + 8);
    local_12b8 = "loot-count";
    local_12b0 = param_2;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"loot-count",10);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14e8,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*pcVar19 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"loot-count",10);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_12b8 = "trees-count";
    local_12b0 = param_2;
    if (*pcVar19 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"trees-count",0xb);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xb;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    pcVar16 = (char *)(param_2 + 8);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14ec,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*pcVar19 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"trees-count",0xb);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xb;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_12b8 = "gate-count";
    local_12b0 = param_2;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"gate-count",10);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14f4,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"gate-count",10);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14f0,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14f6,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    local_12b8 = "emissions-per-tick";
    local_12b0 = param_2;
    if (*pcVar16 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"emissions-per-tick",0x12);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x12;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14c8,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),uVar5 + 0x14d0,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_000014d8 + uVar5,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"emissions-per-tick",0x12);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x12;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  ChunkComponents__save(uVar5 + 0x1580,param_2);
  if (*(char *)(local_12a0 + 8) != '\0') {
    (**(code **)(**(longlong **)(local_12a0 + 0x50) + 8))
              (*(longlong **)(local_12a0 + 0x50),&DAT_14230f710,2);
    *(longlong *)(local_12a0 + 0x10) = *(longlong *)(local_12a0 + 0x10) + 2;
    lVar17 = -1;
    do {
      lVar17 = lVar17 + 1;
    } while (*(char *)(local_12a8 + lVar17) != '\0');
    (**(code **)(**(longlong **)(local_12a0 + 0x50) + 8))
              (*(longlong **)(local_12a0 + 0x50),local_12a8,lVar17);
    *(longlong *)(local_12a0 + 0x10) = *(longlong *)(local_12a0 + 0x10) + lVar17;
    (**(code **)(**(longlong **)(local_12a0 + 0x50) + 8))
              (*(longlong **)(local_12a0 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(local_12a0 + 0x10) = *(longlong *)(local_12a0 + 0x10) + 1;
  }
  Serialiser__SavingStatsWriter___SavingStatsWriter(&local_1280);
  return;
}

