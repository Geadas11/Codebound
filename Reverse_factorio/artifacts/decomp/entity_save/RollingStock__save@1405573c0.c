// RollingStock::save @ 1405573c0 (function RollingStock::save)

void RollingStock__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  char cVar2;
  char *pcVar3;
  longlong *plVar4;
  longlong *plVar5;
  longlong *plVar6;
  char *pcVar7;
  char *pcVar8;
  char *pcVar9;
  char *pcVar10;
  longlong *plVar11;
  char *local_res8;
  longlong *local_res10;
  longlong *local_res18;
  
  Vehicle__save();
  RollingStock__Carriage__save(param_1 + 0x148,param_2);
  puVar1 = (undefined8 *)(param_2 + 0x50);
  plVar5 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))
              ((longlong *)*puVar1,&DAT_1421e010c,1,param_4,"connected-to-front",param_2);
    *plVar5 = *plVar5 + 1;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"connected-to-front",0x12);
    *plVar5 = *plVar5 + 0x12;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  TargeterBase__save(param_1 + 0x218,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_14230f710,2);
    *plVar5 = *plVar5 + 2;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"connected-to-front",0x12);
    *plVar5 = *plVar5 + 0x12;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  local_res10 = (longlong *)(param_2 + 0x10);
  local_res8 = (char *)(param_2 + 0x50);
  pcVar9 = (char *)(param_2 + 8);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(*(longlong *)*puVar1 + 8))
              ((longlong *)*puVar1,&DAT_1421e010c,1,param_4,"front-connection-side",param_2);
    *plVar5 = *plVar5 + 1;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"front-connection-side",0x15);
    *plVar5 = *plVar5 + 0x15;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  plVar6 = local_res10;
  pcVar10 = local_res8;
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x238));
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&local_res8,1);
  *plVar5 = *plVar5 + 1;
  if (*pcVar9 != '\0') {
    local_res8 = pcVar9;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_14230f710,2);
    *plVar5 = *plVar5 + 2;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"front-connection-side",0x15);
    *plVar5 = *plVar5 + 0x15;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar5 = *plVar5 + 1;
  }
  local_res8 = (char *)(param_2 + 8);
  pcVar7 = pcVar10;
  plVar5 = plVar6;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)pcVar10 + 8))
              (*(longlong **)pcVar10,&DAT_1421e010c,1,param_4,"connected-to-back",param_2);
    *plVar6 = *plVar6 + 1;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,"connected-to-back",0x11);
    *plVar6 = *plVar6 + 0x11;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_1421e0108,1);
    *plVar6 = *plVar6 + 1;
    pcVar7 = (char *)(param_2 + 0x50);
    plVar5 = (longlong *)(param_2 + 0x10);
  }
  pcVar3 = local_res8;
  TargeterBase__save(param_1 + 0x240,param_2);
  pcVar8 = pcVar3;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_14230f710,2);
    *plVar6 = *plVar6 + 2;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,"connected-to-back",0x11);
    *plVar6 = *plVar6 + 0x11;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_1421e0108,1);
    *plVar6 = *plVar6 + 1;
    plVar6 = plVar5;
    pcVar8 = (char *)(param_2 + 8);
    pcVar10 = pcVar7;
  }
  pcVar9 = pcVar3;
  plVar5 = (longlong *)pcVar10;
  local_res18 = plVar6;
  if (*pcVar3 != '\0') {
    (**(code **)(**(longlong **)pcVar10 + 8))
              (*(longlong **)pcVar10,&DAT_1421e010c,1,param_4,"back-connection-side",param_2);
    *plVar6 = *plVar6 + 1;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,"back-connection-side",0x14);
    *plVar6 = *plVar6 + 0x14;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_1421e0108,1);
    *plVar6 = *plVar6 + 1;
    pcVar9 = pcVar8;
    plVar5 = (longlong *)(param_2 + 0x50);
    local_res18 = (longlong *)(param_2 + 0x10);
  }
  local_res10 = plVar5;
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x260));
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&local_res8,1);
  *plVar6 = *plVar6 + 1;
  plVar5 = plVar6;
  plVar11 = (longlong *)pcVar10;
  if (*pcVar3 != '\0') {
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_14230f710,2);
    *plVar6 = *plVar6 + 2;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,"back-connection-side",0x14);
    *plVar6 = *plVar6 + 0x14;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_1421e0108,1);
    *plVar6 = *plVar6 + 1;
    plVar5 = local_res18;
    plVar11 = local_res10;
  }
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)pcVar10 + 8))
              (*(longlong **)pcVar10,&DAT_1421e010c,1,param_4,"position-between-ties",param_2);
    *plVar6 = *plVar6 + 1;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,"position-between-ties",0x15);
    *plVar6 = *plVar6 + 0x15;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_1421e0108,1);
    *plVar6 = *plVar6 + 1;
    plVar6 = plVar5;
    pcVar10 = (char *)plVar11;
  }
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,param_1 + 0x270,8);
  *plVar6 = *plVar6 + 8;
  if (*pcVar9 != '\0') {
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_14230f710,2);
    *plVar6 = *plVar6 + 2;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,"position-between-ties",0x15);
    *plVar6 = *plVar6 + 0x15;
    (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&DAT_1421e0108,1);
    *plVar6 = *plVar6 + 1;
  }
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,param_1 + 0x278,8);
  *plVar6 = *plVar6 + 8;
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,param_1 + 0x280,1);
  *plVar6 = *plVar6 + 1;
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,param_1 + 0x281,1);
  *plVar6 = *plVar6 + 1;
  MapSerialiser__operator<<<std::optional<Color>_>(param_2,param_1 + 0x284);
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,param_1 + 0x298,4);
  *plVar6 = *plVar6 + 4;
  local_res8 = *(char **)(param_1 + 0x2a8);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  plVar5 = (longlong *)**(longlong **)(param_1 + 0x2a0);
  if (*(char *)((longlong)plVar5 + 0x19) == '\0') {
    do {
      (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,(longlong)plVar5 + 0x1c,2);
      *plVar6 = *plVar6 + 2;
      (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,plVar5 + 4,4);
      *plVar6 = *plVar6 + 4;
      plVar11 = (longlong *)plVar5[2];
      if (*(char *)((longlong)plVar11 + 0x19) == '\0') {
        cVar2 = *(char *)(*plVar11 + 0x19);
        plVar5 = plVar11;
        plVar11 = (longlong *)*plVar11;
        while (cVar2 == '\0') {
          cVar2 = *(char *)(*plVar11 + 0x19);
          plVar5 = plVar11;
          plVar11 = (longlong *)*plVar11;
        }
      }
      else {
        cVar2 = *(char *)(plVar5[1] + 0x19);
        plVar4 = (longlong *)plVar5[1];
        plVar11 = plVar5;
        while ((plVar5 = plVar4, cVar2 == '\0' && (plVar11 == (longlong *)plVar5[2]))) {
          cVar2 = *(char *)(plVar5[1] + 0x19);
          plVar4 = (longlong *)plVar5[1];
          plVar11 = plVar5;
        }
      }
    } while (*(char *)((longlong)plVar5 + 0x19) == '\0');
    pcVar10 = (char *)(param_2 + 0x50);
    plVar6 = (longlong *)(param_2 + 0x10);
  }
  local_res8 = (char *)CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0x282));
  (**(code **)(**(longlong **)pcVar10 + 8))(*(longlong **)pcVar10,&local_res8,1);
  *plVar6 = *plVar6 + 1;
  return;
}

