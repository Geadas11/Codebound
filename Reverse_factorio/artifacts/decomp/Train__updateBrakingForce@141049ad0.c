// Train::updateBrakingForce @ 141049ad0 (function Train::updateBrakingForce)

void Train__updateBrakingForce(longlong param_1,char param_2)

{
  double dVar1;
  char cVar2;
  double dVar3;
  double dVar4;
  longlong lVar5;
  bool bVar6;
  
  if (*(char *)(param_1 + 0x1a8) == '\0') {
    lVar5 = *(longlong *)(param_1 + 0x30);
    dVar1 = *(double *)(param_1 + 0x180);
    *(undefined8 *)(param_1 + 0x180) = 0;
    dVar4 = DAT_1424027a8;
    if (lVar5 == 0) {
      dVar3 = 0.0;
    }
    else {
      if ((*(longlong *)(lVar5 + 0x220) != 0) && (*(longlong *)(lVar5 + 0x248) != 0)) {
                    /* WARNING: Subroutine does not return */
        Logging__logAndAbortOrThrow
                  ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\RollingStock.cpp"
                   ,0x40c,9,
                   "Internal error in RollingStock::getNeighborDirection, neighbor not specified. Maybe related to https://forums.factorio.com/101113 ?"
                  );
      }
      bVar6 = *(longlong *)(lVar5 + 0x220) == 0;
      dVar3 = 0.0;
      do {
        dVar3 = (*(double *)
                  (*(longlong *)
                    (*(longlong *)(*(longlong *)(lVar5 + 0x78) + 1000) +
                    (ulonglong)*(byte *)(lVar5 + 0x90) * 8) + 0x4f8) + dVar4) *
                *(double *)(*(longlong *)(lVar5 + 0x48) + 0x720) + dVar3;
        *(double *)(param_1 + 0x180) = dVar3;
        if (bVar6) {
          cVar2 = *(char *)(lVar5 + 0x260);
          lVar5 = *(longlong *)(lVar5 + 0x248);
        }
        else {
          cVar2 = *(char *)(lVar5 + 0x238);
          lVar5 = *(longlong *)(lVar5 + 0x220);
        }
        bVar6 = cVar2 == '\0';
      } while (lVar5 != 0);
    }
    if ((param_2 == '\0') && (dVar1 != dVar3)) {
                    /* WARNING: Could not recover jumptable at 0x000141049bdf. Too many branches */
                    /* WARNING: Treating indirect jump as call */
      (**(code **)(**(longlong **)(param_1 + 0x98) + 0x88))();
      return;
    }
  }
  return;
}

