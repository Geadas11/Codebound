// Train::updateSpeedAndMove @ 14104b1b0 (function Train::updateSpeedAndMove)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void Train__updateSpeedAndMove(longlong param_1)

{
  double dVar1;
  ulonglong uVar2;
  undefined8 uVar3;
  char cVar4;
  longlong lVar5;
  ulonglong uVar6;
  longlong *plVar7;
  undefined4 uVar8;
  undefined4 uVar9;
  double dVar10;
  uint uVar11;
  double dVar12;
  uint uVar13;
  undefined1 local_res8 [8];
  undefined4 local_118;
  undefined4 uStack_114;
  undefined4 uStack_110;
  undefined4 uStack_10c;
  bool local_108;
  undefined4 local_100;
  undefined4 uStack_fc;
  undefined4 uStack_f8;
  undefined4 uStack_f4;
  undefined8 local_f0;
  undefined8 local_e8;
  longlong local_e0;
  undefined **local_d8;
  undefined8 local_d0;
  undefined8 local_c8;
  undefined8 uStack_c0;
  undefined1 local_b8;
  undefined8 local_b0;
  undefined **local_a8;
  undefined8 local_a0;
  undefined8 local_98;
  undefined8 uStack_90;
  undefined1 local_88;
  undefined8 local_80;
  undefined1 local_78 [24];
  undefined1 *local_60;
  undefined1 *local_58;
  undefined1 local_50 [24];
  undefined1 *local_38;
  undefined1 *local_30;
  
  dVar1 = *(double *)(param_1 + 0xb8);
  Train__updateSpeed();
  dVar10 = *(double *)(param_1 + 0xb8);
  dVar12 = (double)CONCAT44((uint)((ulonglong)dVar10 >> 0x20) & _UNK_142405184,
                            SUB84(dVar10,0) & _DAT_142405180) + *(double *)(param_1 + 200);
  *(double *)(param_1 + 200) = dVar12;
  if ((dVar10 != 0.0) &&
     ((DAT_1424027a8 < dVar12 ||
      (((uVar6 = *(longlong *)(param_1 + 0xc0) + 0x3c,
        uVar2 = *(ulonglong *)(**(longlong **)(param_1 + 0xa8) + 0x1b0), uVar6 != uVar2 &&
        (uVar6 < uVar2)) || (dVar1 == 0.0)))))) {
    *(undefined8 *)(param_1 + 200) = 0;
    *(undefined8 *)(param_1 + 0xc0) = *(undefined8 *)(**(longlong **)(param_1 + 0xa8) + 0x1b0);
    cVar4 = Train__isMovingFront(param_1);
    lVar5 = 0x150;
    if (cVar4 == '\0') {
      plVar7 = *(longlong **)(param_1 + 0x50);
      lVar5 = 0x150;
      if (plVar7[0x49] == 0) {
        lVar5 = 0x180;
      }
      lVar5 = lVar5 + (longlong)plVar7;
      local_108 = plVar7[0x49] != 0;
    }
    else {
      plVar7 = *(longlong **)(param_1 + 0x30);
      if (plVar7[0x44] != 0) {
        lVar5 = 0x180;
      }
      lVar5 = lVar5 + (longlong)plVar7;
      local_108 = plVar7[0x44] == 0;
    }
    local_118 = (undefined4)lVar5;
    uStack_114 = (undefined4)((ulonglong)lVar5 >> 0x20);
    uStack_110 = SUB84(plVar7,0);
    uStack_10c = (undefined4)((ulonglong)plVar7 >> 0x20);
    local_100 = local_118;
    uStack_fc = uStack_114;
    uStack_f8 = uStack_110;
    uStack_f4 = uStack_10c;
    local_f0 = CONCAT17(local_f0._7_1_,CONCAT25(local_f0._5_2_,CONCAT41(local_f0._1_4_,local_108)));
    (**(code **)(*plVar7 + 0x210))(plVar7,local_res8);
    uVar11 = (uint)*(undefined8 *)(param_1 + 0xb8) & _DAT_142405180;
    uVar13 = (uint)((ulonglong)*(undefined8 *)(param_1 + 0xb8) >> 0x20) & _UNK_142405184;
    dVar10 = ((double)CONCAT44(uVar13,uVar11) /
             ((*(double *)(param_1 + 0x188) + *(double *)(param_1 + 0x180)) /
             *(double *)(param_1 + 0x178))) * DAT_1424025f0;
    if (dVar10 <= DAT_1424027e8) {
      dVar10 = DAT_1424027e8;
    }
    dVar10 = (double)(**(code **)(**(longlong **)(param_1 + 0x98) + 0xd8))
                               (*(longlong **)(param_1 + 0x98),
                                dVar10 * (double)CONCAT44(uVar13,uVar11) + DAT_142402900 +
                                *(double *)
                                 (*(longlong *)(CONCAT44(uStack_fc,local_100) + 8) + 0x170) +
                                DAT_142402900,&local_100,local_res8[0]);
    if (dVar10 < DAT_142402958) {
      dVar10 = *(double *)(param_1 + 0xb8);
      uVar8 = SUB84(dVar10,0);
      uVar9 = (int)((ulonglong)dVar10 >> 0x20);
      if (dVar1 <= dVar10) {
        uVar8 = SUB84(dVar1,0);
        uVar9 = (int)((ulonglong)dVar1 >> 0x20);
      }
      *(ulonglong *)(param_1 + 0xb8) = CONCAT44(uVar9,uVar8);
      *(undefined8 *)(param_1 + 0xc0) = 0;
    }
  }
  dVar1 = *(double *)(param_1 + 0xb8);
  lVar5 = (**(code **)(**(longlong **)(param_1 + 0x98) + 0x20))();
  if ((lVar5 != 0) && (*(longlong *)(lVar5 + 0x100) != 0)) {
    dVar10 = (double)AutomatedTrainController__getRemainingDistance(lVar5);
    if (dVar1 <= 0.0) {
      dVar10 = (double)CONCAT44((uint)((ulonglong)dVar10 >> 0x20) ^ DAT_142405190._4_4_,
                                SUB84(dVar10,0) ^ (uint)DAT_142405190);
      if (dVar1 <= dVar10) {
        dVar1 = dVar10;
      }
    }
    else if (dVar10 <= dVar1) {
      dVar1 = dVar10;
    }
  }
  if (dVar1 == 0.0) {
    *(undefined8 *)(param_1 + 0xb8) = 0;
  }
  else {
    local_e8 = 0;
    local_d0 = 0;
    local_c8 = 0;
    uStack_c0 = 0;
    local_d8 = Targeter<Rail,0,0>::vftable;
    local_b8 = 0;
    local_b0 = 0;
    local_a0 = 0;
    local_98 = 0;
    uStack_90 = 0;
    local_a8 = Targeter<Rail,0,0>::vftable;
    local_88 = 0;
    local_80 = 0;
    local_60 = local_78;
    local_58 = local_78;
    local_38 = local_50;
    local_30 = local_50;
    local_e0 = param_1;
    cVar4 = TrainMover__execute(&local_e8,dVar1);
    if (cVar4 == '\0') {
      (**(code **)(**(longlong **)(param_1 + 0x98) + 0xb8))();
    }
    uVar3 = local_e8;
    TrainMover___TrainMover(&local_e8);
    *(undefined8 *)(param_1 + 0xb8) = uVar3;
  }
  return;
}

