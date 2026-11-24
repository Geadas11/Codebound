// FluidBuffer::getFlowRate @ 140616590 (function FluidBuffer::getFlowRate)

longlong * FluidBuffer__getFlowRate(longlong *param_1,longlong param_2,longlong param_3)

{
  longlong lVar1;
  longlong lVar2;
  longlong lVar3;
  char cVar4;
  longlong lVar5;
  double dVar6;
  double dVar7;
  
  lVar5 = *(longlong *)(param_3 + 0x10);
  lVar1 = *(longlong *)(param_2 + 0x10);
  if (lVar1 == lVar5) {
    cVar4 = '\0';
  }
  else {
    cVar4 = -1;
    if (lVar5 <= lVar1) {
      cVar4 = '\x01';
    }
  }
  lVar2 = lVar5;
  if (cVar4 < '\0') {
    lVar2 = lVar1;
  }
  dVar6 = 0.0;
  dVar7 = 0.0;
  if (lVar5 != 0) {
    dVar7 = (double)(longlong)((double)*(longlong *)(param_3 + 8) / ((double)lVar5 * DAT_142401f90))
            * DAT_142401f90;
  }
  if (lVar1 != 0) {
    dVar6 = (double)(longlong)((double)*(longlong *)(param_2 + 8) / ((double)lVar1 * DAT_142401f90))
            * DAT_142401f90;
  }
  if (DAT_1424027a8 - dVar7 <= dVar6) {
    dVar6 = DAT_1424027a8 - dVar7;
  }
  lVar2 = (longlong)(dVar6 * (double)(longlong)((double)lVar2 * DAT_1424026e0));
  if (lVar2 == 0x199999) {
    cVar4 = '\0';
  }
  else {
    cVar4 = -1;
    if (0x199998 < lVar2) {
      cVar4 = '\x01';
    }
  }
  lVar3 = 0x199999;
  if ('\0' < cVar4) {
    lVar3 = lVar2;
  }
  lVar5 = lVar5 - *(longlong *)(param_3 + 8);
  lVar2 = *(longlong *)(param_2 + 8);
  if (*(ushort *)(param_2 + 0x1a) != 0) {
    lVar1 = *(longlong *)(param_2 + 8) -
            (longlong)
            ((double)((uint)*(ushort *)(param_2 + 0x1a) << 9) * DAT_142401f90 * (double)lVar1);
    if (lVar1 == 0) {
      cVar4 = '\0';
    }
    else {
      cVar4 = -1;
      if (-1 < lVar1) {
        cVar4 = '\x01';
      }
    }
    lVar2 = 0;
    if ('\0' < cVar4) {
      lVar2 = lVar1;
    }
  }
  if (lVar2 == lVar5) {
    cVar4 = '\0';
  }
  else {
    cVar4 = -1;
    if (lVar5 <= lVar2) {
      cVar4 = '\x01';
    }
  }
  if (cVar4 < '\0') {
    lVar5 = lVar2;
  }
  if (lVar3 == lVar5) {
    cVar4 = '\0';
  }
  else {
    cVar4 = -1;
    if (lVar5 <= lVar3) {
      cVar4 = '\x01';
    }
  }
  if (cVar4 < '\0') {
    lVar5 = lVar3;
  }
  *param_1 = lVar5;
  return param_1;
}

