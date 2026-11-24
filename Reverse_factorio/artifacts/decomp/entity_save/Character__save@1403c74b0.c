// Character::save @ 1403c74b0 (function Character::save)

void Character__save(longlong param_1,longlong param_2)

{
  undefined8 *puVar1;
  char *pcVar2;
  undefined8 *puVar3;
  longlong lVar4;
  longlong *plVar5;
  undefined8 *puVar6;
  undefined8 *local_res8;
  longlong local_58;
  longlong local_50;
  char *local_48;
  longlong local_40;
  char *local_38;
  longlong local_30;
  
  if ((*(longlong *)(param_1 + 0x760) == 0) ||
     (lVar4 = *(longlong *)(*(longlong *)(param_1 + 0x760) + 0x28), lVar4 == 0)) {
    pcVar2 = "<none>";
  }
  else {
    pcVar2 = (char *)(lVar4 + 0x4e0);
    if (0xf < *(ulonglong *)(lVar4 + 0x4f8)) {
      pcVar2 = *(char **)pcVar2;
    }
  }
  Serialiser__TagWriter__TagWriter(&local_58,param_2,"character","name=%s",pcVar2);
  EntityWithOwner__save(param_1,param_2);
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  local_48 = "character-data";
  puVar6 = (undefined8 *)(param_2 + 0x50);
  local_40 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e010c,1);
    plVar5 = (longlong *)(param_2 + 0x10);
    *plVar5 = *plVar5 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"character-data",0xe);
    *plVar5 = *plVar5 + 0xe;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  plVar5 = (longlong *)(param_2 + 0x10);
  Status__save(param_1 + 0x138,param_2);
  CharacterModifiers__save(param_1 + 0x1a0,param_2);
  ManualMiner__save(*(undefined8 *)(param_1 + 0x380),param_2);
  lVar4 = *(longlong *)(param_1 + 0x388);
  TargeterBase__save(lVar4 + 8,param_2);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,lVar4 + 0x28,1);
  *plVar5 = *plVar5 + 1;
  MapPosition__save(lVar4 + 0x2c,param_2);
  (**(code **)(*(longlong *)(param_1 + 0x390) + 0xd8))((longlong *)(param_1 + 0x390),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x3b8) + 0xd8))((longlong *)(param_1 + 0x3b8),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x400) + 0xd8))((longlong *)(param_1 + 0x400),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x438) + 0xd8))((longlong *)(param_1 + 0x438),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x470) + 0xd8))((longlong *)(param_1 + 0x470),param_2);
  WakeUpList__save(param_1 + 0x498,param_2);
  WakeUpList__save(param_1 + 0x4b8,param_2);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x4da,2);
  *plVar5 = *plVar5 + 2;
  ItemStack__save(param_1 + 0x4e0,param_2);
  ID<ItemPrototype,unsigned_short>__save(param_1 + 0x500,param_2);
  if (*(short *)(param_1 + 0x500) != 0) {
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x502);
  }
  CyclicFramePosition__save(param_1 + 0x504,param_2);
  ManualCrafter__save(*(undefined8 *)(param_1 + 0x508),param_2);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x510,1);
  *plVar5 = *plVar5 + 1;
  Shooter__save(*(undefined8 *)(param_1 + 0x518),param_2);
  Shooter__save(*(undefined8 *)(param_1 + 0x520),param_2);
  TargeterBase__save(param_1 + 0x528,param_2);
  TargeterBase__save(param_1 + 0x548,param_2);
  TargeterBase__save(param_1 + 0x568,param_2);
  TargeterBase__save(param_1 + 0x588,param_2);
  TargeterBase__save(param_1 + 0x5a8,param_2);
  local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x5c8));
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar5 = *plVar5 + 1;
  EntitySelector__save(*(undefined8 *)(param_1 + 0x5d0),param_2);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x5d8,8);
  *plVar5 = *plVar5 + 8;
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x5e0,8);
  *plVar5 = *plVar5 + 8;
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x5e8,8);
  *plVar5 = *plVar5 + 8;
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x5f0,1);
  *plVar5 = *plVar5 + 1;
  MapSerialiser__operator<<<GuiArrowSpecification>(param_2,*(undefined8 *)(param_1 + 0x5f8));
  MapPosition__save(param_1 + 0x600,param_2);
  MapPosition__save(param_1 + 0x608,param_2);
  local_38 = "light-orientation";
  local_30 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e010c,1);
    *plVar5 = *plVar5 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"light-orientation",0x11);
    *plVar5 = *plVar5 + 0x11;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x610,4);
  *plVar5 = *plVar5 + 4;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
    *plVar5 = *plVar5 + 2;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"light-orientation",0x11);
    *plVar5 = *plVar5 + 0x11;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  puVar6 = (undefined8 *)(param_2 + 0x50);
  plVar5 = (longlong *)(param_2 + 0x10);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x614,1);
  *plVar5 = *plVar5 + 1;
  GameStateManualBuilder__save(param_1 + 0x618,param_2);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x738,1);
  *plVar5 = *plVar5 + 1;
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x4d8,2);
  *plVar5 = *plVar5 + 2;
  local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x6c0) != 0);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar5 = *plVar5 + 1;
  if (*(longlong *)(param_1 + 0x6c0) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x6c0),param_2);
  }
  local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x6c8) != 0);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar5 = *plVar5 + 1;
  if (*(longlong *)(param_1 + 0x6c8) != 0) {
    LogisticPoint__save(*(longlong *)(param_1 + 0x6c8),param_2);
  }
  PortableRoboportProvider__save(param_1 + 0x6d0,param_2);
  local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x6f8) != 0);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar5 = *plVar5 + 1;
  if (*(longlong *)(param_1 + 0x6f8) != 0) {
    CharacterMapConnectSpecification__save(*(longlong *)(param_1 + 0x6f8),param_2);
  }
  puVar6 = (undefined8 *)(param_2 + 0x50);
  plVar5 = (longlong *)(param_2 + 0x10);
  local_38 = "custom-color";
  local_30 = param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e010c,1);
    *plVar5 = *plVar5 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"custom-color",0xc);
    *plVar5 = *plVar5 + 0xc;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  MapSerialiser__operator<<<Color>(param_2,param_1 + 0x728);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
    *plVar5 = *plVar5 + 2;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"custom-color",0xc);
    *plVar5 = *plVar5 + 0xc;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    puVar6 = (undefined8 *)(param_1 + 0x6b0);
    local_res8 = (undefined8 *)0x0;
    for (puVar1 = (undefined8 *)*puVar6; puVar1 != puVar6; puVar1 = (undefined8 *)*puVar1) {
      local_res8 = (undefined8 *)((longlong)local_res8 + 1);
    }
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    for (puVar1 = (undefined8 *)*puVar6; puVar1 != puVar6; puVar1 = (undefined8 *)*puVar1) {
      puVar3 = puVar1 + -0x1f;
      if (puVar1 == (undefined8 *)0x0) {
        puVar3 = (undefined8 *)0x0;
      }
      RawPointerToTargetable__save(param_2,puVar3);
    }
  }
  if (*(char *)(param_2 + 9) != '\0') {
    local_38 = "associated-player";
    local_30 = param_2;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"associated-player",0x11);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x768) != 0);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    if (*(longlong *)(param_1 + 0x768) != 0) {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),*(longlong *)(param_1 + 0x768) + 0x28,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    }
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"associated-player",0x11);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x11;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"character-data",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  if (*(char *)(local_50 + 8) != '\0') {
    (**(code **)(**(longlong **)(local_50 + 0x50) + 8))
              (*(longlong **)(local_50 + 0x50),&DAT_14230f710,2);
    *(longlong *)(local_50 + 0x10) = *(longlong *)(local_50 + 0x10) + 2;
    lVar4 = -1;
    do {
      lVar4 = lVar4 + 1;
    } while (*(char *)(local_58 + lVar4) != '\0');
    (**(code **)(**(longlong **)(local_50 + 0x50) + 8))
              (*(longlong **)(local_50 + 0x50),local_58,lVar4);
    *(longlong *)(local_50 + 0x10) = *(longlong *)(local_50 + 0x10) + lVar4;
    (**(code **)(**(longlong **)(local_50 + 0x50) + 8))
              (*(longlong **)(local_50 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(local_50 + 0x10) = *(longlong *)(local_50 + 0x10) + 1;
  }
  return;
}

