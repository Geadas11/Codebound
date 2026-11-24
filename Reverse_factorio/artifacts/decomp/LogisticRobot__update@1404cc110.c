// LogisticRobot::update @ 1404cc110 (function LogisticRobot::update)

void LogisticRobot__update(longlong param_1)

{
  longlong lVar1;
  longlong lVar2;
  char cVar3;
  char cVar4;
  
  *(undefined1 *)(param_1 + 0xe1) = 1;
  lVar1 = param_1 + -0xa8;
  RobotWithLogisticInterface__updateMovement(lVar1);
  if (((((*(byte *)(param_1 + -0x3a) & 0x10) == 0) && (*(longlong *)(param_1 + 0xe8) != 0)) &&
      (*(char *)(param_1 + 0xe2) == '\0')) && (*(char *)(param_1 + 0xe3) == '\x02')) {
    LogisticRobot__cancelInvalidOrders(lVar1);
    lVar2 = *(longlong *)(param_1 + 0x290);
    if (*(longlong *)(param_1 + 0x298) != lVar2) {
      cVar3 = *(char *)(lVar2 + 0x18);
      if (cVar3 == '\0') {
        cVar4 = *(char *)(param_1 + 0xe0) == '\0';
      }
      else {
        if (cVar3 != '\a') {
                    /* WARNING: Subroutine does not return */
          Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                    ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\LogisticRobot.cpp"
                     ,0xc9);
        }
        cVar4 = *(char *)(param_1 + 0xe0);
      }
      if (cVar4 == '\0') {
        if (cVar3 == '\0') {
          cVar3 = PickupJob__update(lVar2 + 0x20,lVar1);
        }
        else {
          if (cVar3 != '\a') {
                    /* WARNING: Subroutine does not return */
            Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                      ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\LogisticRobot.cpp"
                       ,0xf2);
          }
          cVar3 = DeliverJob__update(lVar2 + 0x20,lVar1);
        }
        if ((cVar3 != '\0') && (*(longlong *)(param_1 + 0x298) != *(longlong *)(param_1 + 0x290))) {
          LogisticRobot__processQueue(lVar1);
        }
        goto LAB_1404cc1e1;
      }
      if (cVar3 == '\0') {
        RobotWithLogisticInterface__cancelCurrentOrder(lVar1);
      }
      RobotWithLogisticInterface__cancelCurrentOrder(lVar1);
    }
    if (*(longlong *)(param_1 + 0x298) == *(longlong *)(param_1 + 0x290)) {
      LogisticNetwork__updateLogisticRobot(*(undefined8 *)(param_1 + 0xe8),lVar1,1);
    }
  }
LAB_1404cc1e1:
  RobotWithLogisticInterface__handleChargingAndStationing(lVar1);
  *(undefined1 *)(param_1 + 0xe1) = 0;
  return;
}

