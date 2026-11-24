// TrainMover::execute @ 1410579d0 (function TrainMover::execute)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

int TrainMover__execute(undefined8 *param_1,double param_2)

{
  longlong lVar1;
  uint uVar2;
  uint uVar3;
  int iVar4;
  undefined8 uVar5;
  double dVar6;
  double dVar7;
  double dVar8;
  
  uVar3 = _UNK_142405184;
  uVar2 = _DAT_142405180;
  iVar4 = 1;
  while( true ) {
    dVar7 = *(double *)(param_1[1] + 0xb8);
    dVar6 = param_2;
    if (param_2 <= 0.0) {
      if (param_2 <= dVar7) {
        dVar6 = dVar7;
      }
    }
    else if (dVar7 <= param_2) {
      dVar6 = dVar7;
    }
    if (dVar6 == 0.0) {
      return iVar4;
    }
    uVar5 = TrainMover__tryToMove(param_1);
    lVar1 = param_1[1];
    dVar7 = (double)CONCAT44((uint)((ulonglong)*(undefined8 *)(lVar1 + 0xb8) >> 0x20) & uVar3,
                             (uint)*(undefined8 *)(lVar1 + 0xb8) & uVar2) *
            *(double *)(lVar1 + 0x178);
    dVar6 = (double)Train__collides(lVar1,dVar7);
    lVar1 = param_1[1];
    if (*(char *)(lVar1 + 0x1a8) != '\0') {
      TrainMover__clearBackupData(param_1,0);
      return iVar4;
    }
    if (*(longlong *)(lVar1 + 0x30) == 0) break;
    if (dVar6 == 0.0) {
      TrainRollingStocks__rewriteBackupPosition(lVar1 + 0x28);
      TrainMover__clearBackupData(param_1,0);
      *(undefined8 *)(param_1[1] + 0xb8) = uVar5;
      Train__transmitSpeedToStocks();
      *param_1 = uVar5;
      return iVar4;
    }
    TrainMover__goToBackupPosition(param_1);
    lVar1 = param_1[1];
    if (dVar7 == dVar6) {
      dVar8 = 0.0;
    }
    else {
      dVar8 = *(double *)(lVar1 + 0xb8);
      iVar4 = -1;
      if (0.0 < dVar8) {
        iVar4 = 1;
      }
      dVar7 = (double)iVar4 * ((dVar7 - dVar6) / *(double *)(lVar1 + 0x178));
      if (dVar8 <= 0.0) {
        if (dVar8 <= dVar7) {
          dVar8 = dVar7;
        }
      }
      else if (dVar7 <= dVar8) {
        dVar8 = dVar7;
      }
    }
    *(double *)(lVar1 + 0xb8) = dVar8;
    Train__transmitSpeedToStocks();
    if (*(double *)(param_1[1] + 0xb8) == 0.0) {
      return 0;
    }
    iVar4 = 0;
  }
  return iVar4;
}

