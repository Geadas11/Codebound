// TrainMover::tryToMove @ 141057b90 (function TrainMover::tryToMove)

undefined8 TrainMover__tryToMove(longlong param_1,uint param_2)

{
  char cVar1;
  longlong lVar2;
  longlong lVar3;
  longlong lVar4;
  bool bVar5;
  bool bVar6;
  undefined8 uVar7;
  
  lVar2 = *(longlong *)(param_1 + 8);
  cVar1 = Train__isMovingFront(lVar2);
  if (cVar1 == '\0') {
    lVar2 = *(longlong *)(lVar2 + 0x50);
  }
  else {
    lVar2 = *(longlong *)(lVar2 + 0x30);
  }
  lVar3 = *(longlong *)(lVar2 + 0x220);
  if ((lVar3 != 0) && (*(longlong *)(lVar2 + 0x248) != 0)) {
LAB_141057ce9:
                    /* WARNING: Subroutine does not return */
    Logging__logAndAbortOrThrow
              ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\RollingStock.cpp"
               ,0x40c,9,
               "Internal error in RollingStock::getNeighborDirection, neighbor not specified. Maybe related to https://forums.factorio.com/101113 ?"
              );
  }
  bVar5 = lVar3 == 0;
  lVar4 = *(longlong *)(param_1 + 8) + 0xa0;
  if ((lVar2 == *(longlong *)(*(longlong *)(param_1 + 8) + 0x30)) == (lVar3 == 0)) {
    uVar7 = RollingStock__move(lVar2,param_1,param_2,lVar4,1);
  }
  else {
    uVar7 = RollingStock__move(lVar2,param_1,param_2 ^ (uint)DAT_142405190,lVar4,1);
    uVar7 = CONCAT44((uint)((ulonglong)uVar7 >> 0x20) ^ DAT_142405190._4_4_,
                     (uint)uVar7 ^ (uint)DAT_142405190);
  }
  if (*(longlong *)(lVar2 + 0x220) == 0) {
    bVar6 = *(char *)(lVar2 + 0x260) == '\0';
    lVar3 = *(longlong *)(lVar2 + 0x248);
    if (lVar3 == 0) {
      return uVar7;
    }
  }
  else {
    if (*(longlong *)(lVar2 + 0x248) != 0) goto LAB_141057ce9;
    lVar3 = *(longlong *)(lVar2 + 0x220);
    bVar6 = *(char *)(lVar2 + 0x238) == '\0';
  }
  do {
    TrainMover__moveStockConnectedToOtherStock(param_1,lVar3,bVar6 == false,lVar2,bVar5);
    if (bVar6 == false) {
      cVar1 = *(char *)(lVar3 + 0x238);
      lVar4 = *(longlong *)(lVar3 + 0x220);
    }
    else {
      cVar1 = *(char *)(lVar3 + 0x260);
      lVar4 = *(longlong *)(lVar3 + 0x248);
    }
    lVar2 = lVar3;
    lVar3 = lVar4;
    bVar5 = bVar6;
    bVar6 = cVar1 == '\0';
  } while (lVar4 != 0);
  return uVar7;
}

