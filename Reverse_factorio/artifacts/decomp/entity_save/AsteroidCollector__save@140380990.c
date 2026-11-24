// AsteroidCollector::save @ 140380990 (function AsteroidCollector::save)

void AsteroidCollector__save
               (longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  uint *puVar1;
  char cVar2;
  ushort uVar3;
  longlong lVar4;
  ushort *puVar5;
  longlong *plVar6;
  ulonglong uVar7;
  longlong *plVar8;
  longlong lVar9;
  longlong *plVar10;
  ushort *puVar11;
  undefined8 *puVar12;
  longlong local_res8;
  
  puVar12 = (undefined8 *)(param_2 + 0x50);
  plVar10 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar12 + 8))
              ((longlong *)*puVar12,&DAT_1421e010c,1,param_4,"asteroid-collector",param_2);
    *plVar10 = *plVar10 + 1;
    (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,"asteroid-collector",0x12);
    *plVar10 = *plVar10 + 0x12;
    (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,&DAT_1421e0108,1);
    *plVar10 = *plVar10 + 1;
  }
  EntityWithOwner__save(param_1,param_2);
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  MapSerialiser__operator<<<InventoryWithBar>(param_2,param_1 + 0xd0);
  WakeUpList__save(param_1 + 0xf8,param_2);
  (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x118,1);
  *plVar10 = *plVar10 + 1;
  (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x119,1);
  *plVar10 = *plVar10 + 1;
  (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x11a,1);
  *plVar10 = *plVar10 + 1;
  (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x11b,1);
  *plVar10 = *plVar10 + 1;
  (**(code **)(**(longlong **)(param_1 + 0x138) + 0x50))(*(longlong **)(param_1 + 0x138),param_2);
  local_res8 = (*(longlong *)(param_1 + 0x128) - *(longlong *)(param_1 + 0x120) >> 4) *
               -0x1084210842108421;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar4 = *(longlong *)(param_1 + 0x128);
  for (lVar9 = *(longlong *)(param_1 + 0x120); lVar9 != lVar4; lVar9 = lVar9 + 0x1f0) {
    AsteroidCollectorArm__save(lVar9,param_2);
  }
  local_res8 = *(longlong *)(param_1 + 0x148) - *(longlong *)(param_1 + 0x140) >> 1;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  puVar5 = *(ushort **)(param_1 + 0x148);
  for (puVar11 = *(ushort **)(param_1 + 0x140); puVar11 != puVar5; puVar11 = puVar11 + 1) {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),puVar11,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    uVar3 = *puVar11;
    if ((uVar3 != 0) && (lVar4 = *(longlong *)(param_2 + 0x48), lVar4 != 0)) {
      if (*(ulonglong *)(lVar4 + 0x3b8) <= (ulonglong)uVar3) {
        std__vector<bool,std::allocator<bool>_>__resize
                  ((longlong *)(lVar4 + 0x3a0),(ulonglong)uVar3 + 1);
      }
      puVar1 = (uint *)(*(longlong *)(lVar4 + 0x3a0) + (ulonglong)(uVar3 >> 5) * 4);
      *puVar1 = *puVar1 | 1 << (uVar3 & 0x1f);
    }
  }
  uVar7 = (ulonglong)local_res8 >> 8;
  local_res8 = CONCAT71((int7)uVar7,*(longlong *)(param_1 + 0x158) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x158) != 0) {
    AsteroidCollectorControlBehavior__save(*(longlong *)(param_1 + 0x158),param_2);
  }
  local_res8 = (*(longlong *)(param_1 + 0x168) - *(longlong *)(param_1 + 0x160) >> 3) *
               -0x3333333333333333;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar4 = *(longlong *)(param_1 + 0x168);
  for (lVar9 = *(longlong *)(param_1 + 0x160); lVar9 != lVar4; lVar9 = lVar9 + 0x28) {
    TargeterBase__save(lVar9,param_2);
  }
  local_res8 = *(longlong *)(param_1 + 0x180);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  plVar10 = (longlong *)**(longlong **)(param_1 + 0x178);
  cVar2 = *(char *)((longlong)plVar10 + 0x19);
  while (cVar2 == '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar10 + 4,8)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
    local_res8 = (plVar10[6] - plVar10[5] >> 3) * -0x3333333333333333;
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar4 = plVar10[6];
    for (lVar9 = plVar10[5]; lVar9 != lVar4; lVar9 = lVar9 + 0x28) {
      TargeterBase__save(lVar9,param_2);
    }
    plVar6 = (longlong *)plVar10[2];
    if (*(char *)((longlong)plVar6 + 0x19) == '\0') {
      cVar2 = *(char *)(*plVar6 + 0x19);
      plVar10 = plVar6;
      plVar6 = (longlong *)*plVar6;
      while (cVar2 == '\0') {
        cVar2 = *(char *)(*plVar6 + 0x19);
        plVar10 = plVar6;
        plVar6 = (longlong *)*plVar6;
      }
    }
    else {
      cVar2 = *(char *)(plVar10[1] + 0x19);
      plVar8 = (longlong *)plVar10[1];
      plVar6 = plVar10;
      while ((plVar10 = plVar8, cVar2 == '\0' && (plVar6 == (longlong *)plVar10[2]))) {
        cVar2 = *(char *)(plVar10[1] + 0x19);
        plVar8 = (longlong *)plVar10[1];
        plVar6 = plVar10;
      }
    }
    cVar2 = *(char *)((longlong)plVar10 + 0x19);
  }
  puVar12 = (undefined8 *)(param_2 + 0x50);
  plVar10 = (longlong *)(param_2 + 0x10);
  (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x188,8);
  *plVar10 = *plVar10 + 8;
  (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x11c,1);
  *plVar10 = *plVar10 + 1;
  if (*(char *)(param_2 + 9) != '\0') {
    Vector2<double,Vector>__save(param_1 + 400,param_2);
    (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,param_1 + 0x1a0,8);
    *plVar10 = *plVar10 + 8;
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,&DAT_14230f710,2);
    *plVar10 = *plVar10 + 2;
    (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,"asteroid-collector",0x12);
    *plVar10 = *plVar10 + 0x12;
    (**(code **)(*(longlong *)*puVar12 + 8))((longlong *)*puVar12,&DAT_1421e0108,1);
    *plVar10 = *plVar10 + 1;
  }
  return;
}

