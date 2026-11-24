// ElectricNetwork::update @ 14033aff0 (function ElectricNetwork::update)

void ElectricNetwork__update(longlong param_1,undefined8 param_2)

{
  longlong *plVar1;
  longlong lVar2;
  longlong lVar3;
  longlong *plVar4;
  longlong *plVar5;
  longlong *plVar6;
  undefined8 local_d8;
  undefined8 uStack_d0;
  undefined8 local_c8;
  undefined8 uStack_c0;
  undefined8 local_b8;
  undefined8 uStack_b0;
  undefined8 local_a8;
  undefined8 uStack_a0;
  undefined8 local_98;
  undefined8 local_88;
  undefined8 uStack_80;
  undefined8 local_78;
  undefined8 uStack_70;
  undefined8 local_68;
  undefined8 uStack_60;
  undefined8 local_58;
  undefined8 uStack_50;
  undefined8 local_48;
  undefined8 uStack_40;
  undefined8 local_38;
  undefined8 uStack_30;
  undefined8 local_28;
  
  plVar6 = (longlong *)0x0;
  if (*(char *)(param_1 + 0x38) == '\x01') {
    if ((*(int *)(param_1 + 0x118) == 0) && (*(int *)(param_1 + 0x11c) == 0)) {
      return;
    }
    lVar3 = *(longlong *)(param_1 + 0xf8);
    if (lVar3 != 0) {
      plVar1 = *(longlong **)(lVar3 + 0x48);
      if (plVar1 == (longlong *)0x0) {
        if (*(longlong *)(lVar3 + 0x50) == 0) goto LAB_14033b084;
        lVar3 = *(longlong *)(*(longlong *)(lVar3 + 0x50) + 0x38);
        plVar1 = plVar6;
        if ((lVar3 != 0) && (plVar5 = *(longlong **)(lVar3 + 0x10), plVar5 != (longlong *)0x0)) {
          plVar1 = (longlong *)(**(code **)(*plVar5 + 0x80))();
        }
        if (plVar1 == (longlong *)0x0) goto LAB_14033b084;
      }
      lVar3 = *(longlong *)(param_1 + 0xf8);
      lVar2 = (**(code **)(*plVar1 + 0x708))();
      if (lVar2 != lVar3) {
        return;
      }
    }
  }
LAB_14033b084:
  if (*(char *)(param_1 + 0x38) == '\0') {
    lVar2 = *(longlong *)(param_1 + 0x128);
    for (lVar3 = *(longlong *)(param_1 + 0x120); lVar3 != lVar2; lVar3 = lVar3 + 8) {
      *(undefined4 *)(lVar3 + 4) = 0;
    }
    lVar2 = *(longlong *)(param_1 + 0x140);
    for (lVar3 = *(longlong *)(param_1 + 0x138); lVar3 != lVar2; lVar3 = lVar3 + 8) {
      *(undefined4 *)(lVar3 + 4) = 0;
    }
    lVar2 = *(longlong *)(param_1 + 0x158);
    for (lVar3 = *(longlong *)(param_1 + 0x150); lVar3 != lVar2; lVar3 = lVar3 + 8) {
      *(undefined4 *)(lVar3 + 4) = 0;
    }
  }
  plVar5 = *(longlong **)(param_1 + 0x28);
  plVar1 = (longlong *)(param_1 + 0x28);
  local_d8._0_4_ = 0;
  local_d8._4_4_ = 0;
  uStack_d0._0_4_ = 0;
  uStack_d0._4_4_ = 0;
  local_c8._0_4_ = 0;
  local_c8._4_4_ = 0;
  uStack_c0._0_4_ = 0;
  uStack_c0._4_4_ = 0;
  local_d8 = 0;
  uStack_d0 = 0;
  local_b8._0_4_ = 0;
  local_b8._4_4_ = 0;
  uStack_b0._0_4_ = 0;
  uStack_b0._4_4_ = 0;
  local_c8 = 0;
  uStack_c0 = 0;
  local_a8._0_4_ = 0;
  local_a8._4_4_ = 0;
  uStack_a0._0_4_ = 0;
  uStack_a0._4_4_ = 0;
  local_b8 = 0;
  uStack_b0 = 0;
  local_98 = 0;
  local_a8 = 0;
  uStack_a0 = 0;
  if (plVar5 != plVar1) {
    do {
      plVar4 = plVar5 + -3;
      if (plVar5 == (longlong *)0x0) {
        plVar4 = plVar6;
      }
      ElectricSubNetwork__getProductionAndDemand(plVar4,&local_d8);
      plVar5 = (longlong *)*plVar5;
    } while (plVar5 != plVar1);
  }
  *(undefined4 *)(param_1 + 0x40) = (undefined4)local_d8;
  *(undefined4 *)(param_1 + 0x44) = local_d8._4_4_;
  *(undefined4 *)(param_1 + 0x48) = (undefined4)uStack_d0;
  *(undefined4 *)(param_1 + 0x4c) = uStack_d0._4_4_;
  *(undefined4 *)(param_1 + 0x50) = (undefined4)local_c8;
  *(undefined4 *)(param_1 + 0x54) = local_c8._4_4_;
  *(undefined4 *)(param_1 + 0x58) = (undefined4)uStack_c0;
  *(undefined4 *)(param_1 + 0x5c) = uStack_c0._4_4_;
  *(undefined4 *)(param_1 + 0x60) = (undefined4)local_b8;
  *(undefined4 *)(param_1 + 100) = local_b8._4_4_;
  *(undefined4 *)(param_1 + 0x68) = (undefined4)uStack_b0;
  *(undefined4 *)(param_1 + 0x6c) = uStack_b0._4_4_;
  *(undefined4 *)(param_1 + 0x70) = (undefined4)local_a8;
  *(undefined4 *)(param_1 + 0x74) = local_a8._4_4_;
  *(undefined4 *)(param_1 + 0x78) = (undefined4)uStack_a0;
  *(undefined4 *)(param_1 + 0x7c) = uStack_a0._4_4_;
  *(undefined8 *)(param_1 + 0x80) = local_98;
  ElectricNetworkInputValues__computeUsage(&local_d8,&local_88);
  *(undefined8 *)(param_1 + 0x88) = local_88;
  *(undefined8 *)(param_1 + 0x90) = uStack_80;
  *(undefined8 *)(param_1 + 0x98) = local_78;
  *(undefined8 *)(param_1 + 0xa0) = uStack_70;
  *(undefined8 *)(param_1 + 0xa8) = local_68;
  *(undefined8 *)(param_1 + 0xb0) = uStack_60;
  *(undefined8 *)(param_1 + 0xb8) = local_58;
  *(undefined8 *)(param_1 + 0xc0) = uStack_50;
  *(undefined8 *)(param_1 + 200) = local_48;
  *(undefined8 *)(param_1 + 0xd0) = uStack_40;
  *(undefined8 *)(param_1 + 0xd8) = local_38;
  *(undefined8 *)(param_1 + 0xe0) = uStack_30;
  *(undefined8 *)(param_1 + 0xe8) = local_28;
  for (plVar5 = (longlong *)*plVar1; plVar5 != plVar1; plVar5 = (longlong *)*plVar5) {
    plVar4 = plVar5 + -3;
    if (plVar5 == (longlong *)0x0) {
      plVar4 = plVar6;
    }
    ElectricSubNetwork__useProductionAndDemand(plVar4,&local_88,param_1 + 0x120);
  }
  if (*(char *)(param_1 + 0x38) == '\0') {
    ElectricNetworkStatisticsBuffer__apply(param_1 + 0x120,*(undefined8 *)(param_1 + 0x110),param_2)
    ;
  }
  return;
}

