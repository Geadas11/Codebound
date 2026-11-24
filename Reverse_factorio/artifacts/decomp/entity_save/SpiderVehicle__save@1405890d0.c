// SpiderVehicle::save @ 1405890d0 (function SpiderVehicle::save)

void SpiderVehicle__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  ulonglong uVar2;
  char *pcVar3;
  longlong *plVar4;
  ulonglong uVar5;
  undefined8 *puVar6;
  longlong *plVar7;
  char *local_res8;
  char *local_res10;
  undefined8 *local_res18;
  
  Vehicle__save();
  plVar4 = (longlong *)(param_1 + 0x150);
  local_res8 = *(char **)(param_1 + 0x160);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0x160);
  if (0xf < *(ulonglong *)(param_1 + 0x168)) {
    plVar4 = (longlong *)*plVar4;
  }
  puVar6 = (undefined8 *)(param_2 + 0x50);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,plVar4,lVar1);
  plVar4 = (longlong *)(param_2 + 0x10);
  *plVar4 = *plVar4 + lVar1;
  (**(code **)(**(longlong **)(param_1 + 0x178) + 0x50))(*(longlong **)(param_1 + 0x178),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x180) + 0xd8))((longlong *)(param_1 + 0x180),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x1b8) + 0xd8))((longlong *)(param_1 + 0x1b8),param_2);
  WakeUpList__save(param_1 + 0x1f0,param_2);
  WakeUpList__save(param_1 + 0x210,param_2);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x260,1);
  *plVar4 = *plVar4 + 1;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))
              ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"trash-slots",param_2);
    *plVar4 = *plVar4 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"trash-slots",0xb);
    *plVar4 = *plVar4 + 0xb;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
  }
  (**(code **)(*(longlong *)(param_1 + 0x268) + 0xd8))((longlong *)(param_1 + 0x268),param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
    *plVar4 = *plVar4 + 2;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"trash-slots",0xb);
    *plVar4 = *plVar4 + 0xb;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))
              ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"request-point",param_2);
    *plVar4 = *plVar4 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"request-point",0xd);
    *plVar4 = *plVar4 + 0xd;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
  }
  plVar4 = (longlong *)(param_2 + 0x10);
  puVar6 = (undefined8 *)(param_2 + 0x50);
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x290) != 0);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar4 = *plVar4 + 1;
  if (*(longlong *)(param_1 + 0x290) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x290),param_2);
  }
  pcVar3 = (char *)(param_2 + 8);
  if (*pcVar3 != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
    *plVar4 = *plVar4 + 2;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"request-point",0xd);
    *plVar4 = *plVar4 + 0xd;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
  }
  if (*pcVar3 != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))
              ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"provider-point",param_2);
    *plVar4 = *plVar4 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"provider-point",0xe);
    *plVar4 = *plVar4 + 0xe;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
  }
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x298) != 0);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar4 = *plVar4 + 1;
  if (*(longlong *)(param_1 + 0x298) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x298),param_2);
  }
  plVar4 = (longlong *)(param_2 + 0x10);
  puVar6 = (undefined8 *)(param_2 + 0x50);
  if (*pcVar3 != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
    *plVar4 = *plVar4 + 2;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"provider-point",0xe);
    *plVar4 = *plVar4 + 0xe;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x2a0,1);
    *plVar4 = *plVar4 + 1;
    Vector2<double,Vector>__save(param_1 + 0x2a8,param_2);
    MapSerialiser__operator<<<SpiderEngine>(param_2,param_1 + 0x2c8);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x328,4);
    *plVar4 = *plVar4 + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x32c,4);
    *plVar4 = *plVar4 + 4;
    Vector2<double,Vector>__save(param_1 + 0x330,param_2);
    Vector2<double,Vector>__save(param_1 + 0x340,param_2);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x350,4);
    *plVar4 = *plVar4 + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x358,8);
    *plVar4 = *plVar4 + 8;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x360,4);
    *plVar4 = *plVar4 + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x364,4);
    *plVar4 = *plVar4 + 4;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))
                ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"autopilot",param_2);
      *plVar4 = *plVar4 + 1;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"autopilot",9);
      *plVar4 = *plVar4 + 9;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
    local_res8 = *(char **)(param_1 + 0x388);
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    uVar5 = *(ulonglong *)(param_1 + 0x380);
    uVar2 = *(longlong *)(param_1 + 0x388) + uVar5;
    for (; uVar5 != uVar2; uVar5 = uVar5 + 1) {
      MapPosition__save(*(longlong *)
                         (*(longlong *)(param_1 + 0x370) +
                         (uVar5 >> 1 & *(longlong *)(param_1 + 0x378) - 1U) * 8) +
                        (ulonglong)((uint)uVar5 & 1) * 8,param_2);
    }
    puVar6 = (undefined8 *)(param_2 + 0x50);
    plVar4 = (longlong *)(param_2 + 0x10);
    TargeterBase__save(param_1 + 0x390,param_2);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x3b0,4);
    *plVar4 = *plVar4 + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x3b4,1);
    *plVar4 = *plVar4 + 1;
    Vector2<double,Vector>__save(param_1 + 0x3b8,param_2);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar4 = *plVar4 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"autopilot",9);
      *plVar4 = *plVar4 + 9;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x3c8,4);
    *plVar4 = *plVar4 + 4;
  }
  puVar6 = (undefined8 *)(param_2 + 0x50);
  pcVar3 = (char *)(param_2 + 0x149);
  plVar4 = (longlong *)(param_2 + 0x10);
  MapSerialiser__operator<<<Color>(param_2,param_1 + 0x3cc);
  if (*pcVar3 == '\0') {
    local_res8 = pcVar3;
    local_res18 = puVar6;
    if ((*(longlong *)(*(longlong *)(param_1 + 0x48) + 0x920) ==
        *(longlong *)(*(longlong *)(param_1 + 0x48) + 0x928)) ==
        (*(longlong *)(param_1 + 0x230) != 0)) {
                    /* WARNING: Subroutine does not return */
      ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\SpiderVehicle.cpp"
                          ,0x139,"bool(this->getPrototype()->guns.empty()) != bool(this->shooter)");
    }
    MapSerialiser__saveOptional<std::unique_ptr<Shooter,std::default_delete<Shooter>_>_>(param_2);
  }
  local_res18 = (undefined8 *)(param_2 + 0x50);
  local_res8 = (char *)(param_2 + 0x149);
  plVar7 = (longlong *)(param_2 + 0x10);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x238,1);
  *plVar4 = *plVar4 + 1;
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x239,1);
  *plVar4 = *plVar4 + 1;
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x23a,1);
  *plVar4 = *plVar4 + 1;
  local_res10 = pcVar3;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))
              ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"gun-selection",param_2);
    *plVar4 = *plVar4 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"gun-selection",0xd);
    *plVar4 = *plVar4 + 0xd;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
    local_res10 = local_res8;
  }
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x23c,2);
  *plVar4 = *plVar4 + 2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
    *plVar4 = *plVar4 + 2;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"gun-selection",0xd);
    *plVar4 = *plVar4 + 0xd;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar4 = *plVar4 + 1;
    pcVar3 = local_res10;
  }
  puVar6 = local_res18;
  if (*pcVar3 == '\0') {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*local_res18 + 8))
                ((longlong *)*local_res18,&DAT_1421e010c,1,param_4,"shooting-target",param_2);
      *plVar7 = *plVar7 + 1;
      plVar4 = (longlong *)*puVar6;
      (**(code **)(*plVar4 + 8))(plVar4,"shooting-target",0xf);
      *plVar7 = *plVar7 + 0xf;
      plVar4 = (longlong *)*puVar6;
      (**(code **)(*plVar4 + 8))(plVar4,&DAT_1421e0108,1);
      *plVar7 = *plVar7 + 1;
    }
    puVar6 = (undefined8 *)(param_2 + 0x50);
    plVar4 = (longlong *)(param_2 + 0x10);
    TargeterBase__save(param_1 + 0x240,param_2);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar4 = *plVar4 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"shooting-target",0xf);
      *plVar4 = *plVar4 + 0xf;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
  }
  return;
}

