// Rail::save @ 140508ba0 (function Rail::save)

void Rail__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  char cVar1;
  longlong *plVar2;
  undefined8 *puVar3;
  char *pcVar4;
  undefined8 *puVar5;
  longlong *plVar6;
  char *local_res8;
  undefined8 *local_res10;
  longlong *local_res18;
  
  EntityWithOwner__save();
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"direction",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"direction",9)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 9;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xa8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"direction",9)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 9;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    cVar1 = (**(code **)(**(longlong **)(param_1 + 0x48) + 0x7e0))();
    if (cVar1 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),param_1 + 0xaa,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),param_1 + 0xac,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    }
    pcVar4 = (char *)(param_2 + 8);
    puVar5 = (undefined8 *)(param_2 + 0x50);
    plVar6 = (longlong *)(param_2 + 0x10);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"front",param_2);
      *plVar6 = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,"front",5);
      *plVar6 = *plVar6 + 5;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
      *plVar6 = *plVar6 + 1;
    }
    local_res18 = (longlong *)(param_2 + 0x10);
    local_res10 = (undefined8 *)(param_2 + 0x50);
    local_res8 = pcVar4;
    if (*(char *)(param_2 + 0x149) == '\0') {
      local_res8._1_7_ = (undefined7)((ulonglong)pcVar4 >> 8);
      local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0xe0));
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&local_res8,1);
      *plVar6 = *plVar6 + 1;
    }
    plVar2 = local_res18;
    puVar3 = local_res10;
    if (*pcVar4 != '\0') {
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_14230f710,2);
      *plVar6 = *plVar6 + 2;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,"front",5);
      *plVar6 = *plVar6 + 5;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
      *plVar6 = *plVar6 + 1;
    }
    puVar5 = puVar3;
    plVar6 = plVar2;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar3 + 8))
                ((longlong *)*puVar3,&DAT_1421e010c,1,param_4,&DAT_14221a4b4,param_2);
      *plVar2 = *plVar2 + 1;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14221a4b4,4);
      *plVar2 = *plVar2 + 4;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
      *plVar2 = *plVar2 + 1;
      puVar5 = (undefined8 *)(param_2 + 0x50);
      plVar6 = (longlong *)(param_2 + 0x10);
    }
    if (*(char *)(param_2 + 0x149) == '\0') {
      local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x118));
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&local_res8,1);
      *plVar2 = *plVar2 + 1;
      plVar2 = plVar6;
      puVar3 = puVar5;
    }
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
      *plVar2 = *plVar2 + 2;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14221a4b4,4);
      *plVar2 = *plVar2 + 4;
      (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
      *plVar2 = *plVar2 + 1;
    }
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,param_1 + 0x120,1);
    *plVar2 = *plVar2 + 1;
    WakeUpList__save(param_1 + 0x128,param_2);
    local_res8 = (char *)(ulonglong)**(uint **)(param_1 + 0x168);
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    puVar3 = *(undefined8 **)(*(uint **)(param_1 + 0x168) + 2);
    puVar5 = puVar3 + **(uint **)(param_1 + 0x168);
    for (; puVar3 != puVar5; puVar3 = puVar3 + 1) {
      RawPointerToTargetable__save(param_2,*puVar3);
    }
  }
  return;
}

