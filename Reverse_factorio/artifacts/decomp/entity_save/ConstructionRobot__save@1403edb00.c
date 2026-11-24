// ConstructionRobot::save @ 1403edb00 (function ConstructionRobot::save)

void ConstructionRobot__save(longlong param_1,longlong param_2)

{
  int iVar1;
  longlong lVar2;
  char cVar3;
  longlong lVar4;
  undefined4 local_res8 [2];
  
  RobotWithLogisticInterface__save();
  iVar1 = *(int *)(param_1 + 0x470);
  cVar3 = RobotWithLogisticInterface__shouldBeLinked(param_1);
  if ((iVar1 != 0x7fffffff) != (bool)cVar3) {
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\ConstructionRobot.cpp"
                        ,0x6e,"this->busyRobotLink.isLinked() == this->shouldBeLinked()");
  }
  local_res8[0] = (undefined4)(*(longlong *)(param_1 + 0x340) - *(longlong *)(param_1 + 0x338) >> 7)
  ;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  lVar2 = *(longlong *)(param_1 + 0x340);
  for (lVar4 = *(longlong *)(param_1 + 0x338); lVar4 != lVar2; lVar4 = lVar4 + 0x80) {
    WorkerRobotOrder__save(lVar4,param_2);
  }
  (**(code **)(*(longlong *)(param_1 + 0x3b8) + 0xd8))((longlong *)(param_1 + 0x3b8),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x3f9,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapPosition__save(param_1 + 0x3fc,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x3f8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  CyclicFramePosition__save(param_1 + 0x3e0,param_2);
  CyclicFramePosition__save(param_1 + 1000,param_2);
  CyclicFramePosition__save(param_1 + 0x3ec,param_2);
  if (*(char *)(param_2 + 9) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x478,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

