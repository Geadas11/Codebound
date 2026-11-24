// Inserter::save @ 14048c530 (function Inserter::save)

void Inserter__save(longlong param_1,longlong param_2)

{
  bool bVar1;
  bool bVar2;
  longlong *plVar3;
  uint uVar4;
  longlong lVar5;
  char *pcVar6;
  byte bVar7;
  char *pcVar8;
  longlong *plVar9;
  uint uVar10;
  uint uVar11;
  char local_res8 [8];
  char *local_res10;
  char *local_res18;
  char *local_58;
  double dStack_50;
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  local_res18 = (char *)(param_2 + 0x149);
  pcVar8 = (char *)(param_2 + 0x50);
  pcVar6 = pcVar8;
  if (*local_res18 == '\0') {
    ItemStack__save(param_1 + 0x148,param_2);
    TargeterBase__save(param_1 + 0x160,param_2);
    local_58 = "pickup-taget";
    dStack_50 = (double)param_2;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,"pickup-taget",0xc);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xc;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    TargeterBase__save(param_1 + 0x188,param_2);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,"pickup-taget",0xc);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xc;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_res10 = (char *)(param_2 + 0x50);
    local_58 = "belt-item-pickup-target";
    dStack_50 = (double)param_2;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,"belt-item-pickup-target",0x17);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x17;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    pcVar6 = local_res10;
    local_res10 = (char *)CONCAT71(local_res10._1_7_,
                                   *(char *)(param_1 + 0x1b1) * '\x02' |
                                   *(char *)(param_1 + 0x1b0) == '\x01');
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x1b4,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&local_res10,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x1b2,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x1b8,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,"belt-item-pickup-target",0x17);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x17;
      (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  local_res18 = (char *)(param_2 + 0x149);
  plVar9 = (longlong *)(param_2 + 0x10);
  local_res10 = (char *)(CONCAT62(local_res10._2_6_,*(undefined2 *)(param_1 + 0x1bc)) &
                        0xffffffffffffff9f);
  (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&local_res10,2);
  *plVar9 = *plVar9 + 2;
  pcVar8 = pcVar6;
  if (*(char *)(param_2 + 0x149) == '\0') {
    local_58 = "drop-target";
    dStack_50 = (double)param_2;
    if (*(char *)(param_2 + 8) != '\0') {
      local_res10 = (char *)(param_2 + 0x149);
      (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&DAT_1421e010c,1);
      *plVar9 = *plVar9 + 1;
      (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,"drop-target",0xb);
      *plVar9 = *plVar9 + 0xb;
      (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&DAT_1421e0108,1);
      *plVar9 = *plVar9 + 1;
      pcVar8 = (char *)(param_2 + 0x50);
    }
    local_res10 = (char *)(param_2 + 0x149);
    DropTarget__save(param_1 + 0x1c0,param_2);
    if (*(char *)(param_2 + 8) == '\0') {
      local_res18 = (char *)(param_2 + 0x149);
      pcVar8 = pcVar6;
    }
    else {
      (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&DAT_14230f710,2);
      *plVar9 = *plVar9 + 2;
      (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,"drop-target",0xb);
      *plVar9 = *plVar9 + 0xb;
      (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&DAT_1421e0108,1);
      *plVar9 = *plVar9 + 1;
      local_res18 = local_res10;
    }
  }
  plVar3 = (longlong *)(param_2 + 0x10);
  (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,param_1 + 0x210,1);
  *plVar3 = *plVar9 + 1;
  (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x211,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x212,1);
  *plVar3 = *plVar3 + 1;
  if (*local_res18 == '\0') {
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x218,8);
    *plVar3 = *plVar3 + 8;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x240,4);
    *plVar3 = *plVar3 + 4;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x248,8);
    *plVar3 = *plVar3 + 8;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,param_1 + 0x244,4);
    *plVar3 = *plVar3 + 4;
  }
  local_res10._0_1_ = *(undefined1 *)(param_1 + 0x214);
  (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&local_res10,1);
  *plVar3 = *plVar3 + 1;
  (**(code **)(**(longlong **)(param_1 + 0x250) + 0x50))(*(longlong **)(param_1 + 0x250),param_2);
  local_res10 = (char *)CONCAT71(local_res10._1_7_,*(longlong *)(param_1 + 600) != 0);
  (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&local_res10,1);
  *plVar3 = *plVar3 + 1;
  pcVar6 = pcVar8;
  if (*(longlong *)(param_1 + 600) != 0) {
    local_res10 = pcVar8;
    InserterControlBehavior__save(*(longlong *)(param_1 + 600),param_2);
    pcVar6 = (char *)(param_2 + 0x50);
  }
  local_58 = "filters";
  local_res10 = pcVar6;
  dStack_50 = (double)param_2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e010c,1);
    *plVar3 = *plVar3 + 1;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,"filters",7);
    *plVar3 = *plVar3 + 7;
    (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&DAT_1421e0108,1);
    *plVar3 = *plVar3 + 1;
  }
  plVar9 = (longlong *)(param_2 + 0x10);
  (**(code **)(**(longlong **)pcVar8 + 8))
            (*(longlong **)pcVar8,*(longlong *)(param_1 + 0x48) + 0x73b,1);
  pcVar8 = local_res10;
  *plVar9 = *plVar3 + 1;
  bVar7 = 0;
  if (*(char *)(*(longlong *)(param_1 + 0x48) + 0x73b) != '\0') {
    do {
      lVar5 = param_1 + (ulonglong)bVar7 * 4;
      ID<ItemPrototype,unsigned_short>__save(lVar5 + 0x260,param_2);
      MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,lVar5 + 0x262);
      if (*(char *)(lVar5 + 0x262) != '\0') {
        local_res10 = (char *)CONCAT71(local_res10._1_7_,*(undefined1 *)(lVar5 + 0x263));
        (**(code **)(**(longlong **)pcVar8 + 8))(*(longlong **)pcVar8,&local_res10,1);
        *plVar9 = *plVar9 + 1;
      }
      bVar7 = bVar7 + 1;
    } while (bVar7 < *(byte *)(*(longlong *)(param_1 + 0x48) + 0x73b));
    pcVar6 = (char *)(param_2 + 0x50);
  }
  plVar9 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&DAT_14230f710,2);
    *plVar9 = *plVar9 + 2;
    (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,"filters",7);
    *plVar9 = *plVar9 + 7;
    (**(code **)(**(longlong **)pcVar6 + 8))(*(longlong **)pcVar6,&DAT_1421e0108,1);
    *plVar9 = *plVar9 + 1;
  }
  lVar5 = *(longlong *)(param_1 + 0x48);
  local_58 = *(char **)(lVar5 + 0x748);
  dStack_50 = *(double *)(lVar5 + 0x750);
  uVar10 = (uint)DAT_142405190;
  uVar11 = (uint)((ulonglong)DAT_142405190 >> 0x20);
  uVar4 = *(ushort *)(param_1 + 0x1bc) & 0x2000;
  if ((*(ushort *)(param_1 + 0x1bc) & 0x2000) != 0) {
    local_58 = (char *)CONCAT44(*(uint *)(lVar5 + 0x74c) ^ uVar11,*(uint *)(lVar5 + 0x748) ^ uVar10)
    ;
  }
  Vector2<double,Vector>__rotate(&local_58,*(undefined1 *)(param_1 + 0x210));
  if (*(double *)(param_1 + 0x280) == dStack_50 && *(double *)(param_1 + 0x278) == (double)local_58)
  {
    local_58 = *(char **)(lVar5 + 0x758);
    dStack_50 = *(double *)(lVar5 + 0x760);
    if (uVar4 != 0) {
      local_58 = (char *)CONCAT44(*(uint *)(lVar5 + 0x75c) ^ uVar11,
                                  *(uint *)(lVar5 + 0x758) ^ uVar10);
    }
    Vector2<double,Vector>__rotate();
    bVar1 = *(double *)(param_1 + 0x288) != (double)local_58;
    bVar2 = *(double *)(param_1 + 0x290) != dStack_50;
    local_res8[0] = bVar2 || bVar1;
    if (!bVar2 && !bVar1) goto LAB_14048cc98;
  }
  local_res8[0] = true;
LAB_14048cc98:
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (local_res8[0] != '\0') {
    if (*(char *)(*(longlong *)(param_1 + 0x48) + 0x732) == '\0') {
                    /* WARNING: Subroutine does not return */
      Logging__logAndAbortOrThrow
                ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\Inserter.cpp"
                 ,0xed,9,"Saving in this state will result in a desync when loaded.");
    }
    Vector2<double,Vector>__save(param_1 + 0x278,param_2);
    Vector2<double,Vector>__save(param_1 + 0x288,param_2);
  }
  if (*(char *)(param_2 + 9) != '\0') {
    local_58 = "max-belt-stack-size";
    dStack_50 = (double)param_2;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"max-belt-stack-size",0x13);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x13;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x213,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"max-belt-stack-size",0x13);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x13;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  return;
}

