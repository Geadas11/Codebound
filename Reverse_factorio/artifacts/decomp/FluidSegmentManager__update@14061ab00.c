// FluidSegmentManager::update @ 14061ab00 (function FluidSegmentManager::update)

void FluidSegmentManager__update(longlong param_1)

{
  longlong *plVar1;
  longlong lVar2;
  longlong lVar3;
  ulonglong uVar4;
  undefined8 *puVar5;
  longlong *plVar6;
  longlong *plVar7;
  double _X;
  double dVar8;
  double dVar9;
  double dVar10;
  byte *pbVar11;
  undefined8 *puVar12;
  longlong *plVar13;
  double dVar14;
  undefined1 local_res8 [8];
  
  dVar10 = DAT_1424027a8;
  dVar9 = DAT_142402488;
  dVar8 = DAT_142401f90;
  plVar1 = *(longlong **)(param_1 + 0x18);
  for (plVar13 = *(longlong **)(param_1 + 0x10); plVar13 != plVar1; plVar13 = plVar13 + 1) {
    lVar2 = *plVar13;
    lVar3 = *(longlong *)(lVar2 + 0x38);
    *(undefined8 *)(lVar2 + 0x38) = 0;
    _X = (double)(longlong)((double)lVar3 / ((double)*(longlong *)(lVar2 + 0x28) * dVar8)) * dVar8 *
         dVar9 + *(double *)(lVar2 + 8);
    *(double *)(lVar2 + 8) = _X;
    if (dVar10 < _X) {
      dVar14 = floor(_X);
      *(double *)(lVar2 + 8) = _X - dVar14;
    }
    if (*(short *)(lVar2 + 0x18) != 0) {
      *(short *)(lVar2 + 0x40) = *(short *)(lVar2 + 0x18);
      *(undefined8 *)(lVar2 + 0x48) =
           *(undefined8 *)(*(longlong *)(*(longlong *)(lVar2 + 0x10) + 8) + 0x1b0);
    }
    if (((*(byte *)(lVar2 + 0x34) & 1) != 0) && ((*(byte *)(lVar2 + 0x34) & 2) != 0)) {
      if (*(char *)(lVar2 + 0xa8) == '\0') {
        FluidSegment__rebuildAlertingFluidBoxes(lVar2);
      }
      uVar4 = *(ulonglong *)(*(longlong *)(*(longlong *)(lVar2 + 0x10) + 8) + 0x1a8);
      if ((int)uVar4 + (int)(uVar4 / 0x3c) * -0x3c == 0) {
        puVar5 = *(undefined8 **)(lVar2 + 0x98);
        for (puVar12 = *(undefined8 **)(lVar2 + 0x90); puVar12 != puVar5; puVar12 = puVar12 + 1) {
          plVar6 = (longlong *)*puVar12;
          plVar7 = (longlong *)*plVar6;
          lVar2 = plVar7[0xf];
          pbVar11 = (byte *)(**(code **)(*plVar7 + 0x210))(plVar7,local_res8);
          ForceData__addAlert(*(undefined8 *)(*(longlong *)(lVar2 + 1000) + (ulonglong)*pbVar11 * 8)
                              ,*plVar6,0,0x10,0);
        }
      }
    }
  }
  Util__removeIf<std::vector<MixingConnectionData,std::allocator<MixingConnectionData>_>,_FluidSegmentManager::reconcileMixingConnections_::_2_::<lambda_1>_>
            ((undefined8 *)(param_1 + 0x28),local_res8[0]);
  uVar4 = *(ulonglong *)(*(longlong *)(param_1 + 8) + 0x1a8);
  if ((int)uVar4 + (int)(uVar4 / 0x3c) * -0x3c == 0) {
    puVar5 = *(undefined8 **)(param_1 + 0x30);
    for (puVar12 = *(undefined8 **)(param_1 + 0x28); puVar12 != puVar5; puVar12 = puVar12 + 2) {
      plVar13 = *(longlong **)*puVar12;
      lVar2 = plVar13[0xf];
      pbVar11 = (byte *)(**(code **)(*plVar13 + 0x210))(plVar13,local_res8);
      ForceData__addAlert(*(undefined8 *)(*(longlong *)(lVar2 + 1000) + (ulonglong)*pbVar11 * 8),
                          *(undefined8 *)*puVar12,0,0x11,0);
    }
  }
  return;
}

