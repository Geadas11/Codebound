// Arrow::save @ 140366db0 (function Arrow::save)

void Arrow__save(longlong *param_1,longlong param_2)

{
  longlong lVar1;
  undefined1 *puVar2;
  undefined8 *puVar3;
  longlong *plVar4;
  undefined8 *puVar5;
  longlong *plVar6;
  undefined2 uStackX_8;
  ushort auStackX_18 [8];
  longlong lStack_68;
  longlong lStack_60;
  char *pcStack_58;
  longlong lStack_50;
  undefined1 auStack_48 [16];
  
  lVar1 = param_1[9];
  if (lVar1 == 0) {
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\Entity.cpp"
                        ,0xb8,"this->prototype");
  }
  if (((ulonglong)*(ushort *)(lVar1 + 0x1c8) < (ulonglong)(DAT_1429c6218 - DAT_1429c6210 >> 3)) &&
     (*(longlong *)(DAT_1429c6210 + (ulonglong)*(ushort *)(lVar1 + 0x1c8) * 8) == lVar1)) {
    puVar3 = (undefined8 *)(lVar1 + 8);
    if (0xf < *(ulonglong *)(lVar1 + 0x20)) {
      puVar3 = (undefined8 *)*puVar3;
    }
    Serialiser__TagWriter__TagWriter(&lStack_68,param_2,"entity-data","name=%s",puVar3);
    uStackX_8 = *(ushort *)(param_1[9] + 0x1c8);
    ID<EntityPrototype,unsigned_short>__save(&uStackX_8,param_2);
    MapPosition__save(param_1 + 10,param_2);
    auStackX_18[0] = *(ushort *)((longlong)param_1 + 0x6e) & 0xcfbf;
    lVar1 = (**(code **)(*param_1 + 0xc50))(param_1);
    if ((lVar1 != 0) &&
       ((*(float *)(lVar1 + 0x80) != DAT_1424022c0 || (*(float *)(lVar1 + 0x8c) != 0.0)))) {
      auStackX_18[0] = auStackX_18[0] | 0x2000;
    }
    puVar3 = (undefined8 *)(param_2 + 0x50);
    (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,auStackX_18,2);
    plVar4 = (longlong *)(param_2 + 0x10);
    *plVar4 = *plVar4 + 2;
    if (*(char *)(param_2 + 9) != '\0') {
      pcStack_58 = "bounding-box";
      lStack_50 = param_2;
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e010c,1);
        *plVar4 = *plVar4 + 1;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"bounding-box",0xc);
        *plVar4 = *plVar4 + 0xc;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
        *plVar4 = *plVar4 + 1;
      }
      BoundingBox__save(param_1 + 0xb,param_2);
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
        *plVar4 = *plVar4 + 2;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"bounding-box",0xc);
        *plVar4 = *plVar4 + 0xc;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
        *plVar4 = *plVar4 + 1;
      }
      plVar6 = (longlong *)(param_2 + 0x10);
      puVar5 = (undefined8 *)(param_2 + 0x50);
      MapSerialiser__operator<<<CollisionMask>
                (param_2,DAT_142f45838 + (ulonglong)*(ushort *)((longlong)param_1 + 0x6c) * 0x28);
      pcStack_58 = "has-surface";
      lStack_50 = param_2;
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e010c,1);
        *plVar6 = *plVar4 + 1;
        (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,"has-surface",0xb);
        *plVar6 = *plVar6 + 0xb;
        (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
        *plVar6 = *plVar6 + 1;
      }
      plVar4 = (longlong *)*puVar5;
      if (param_1[0xe] == 0) {
        uStackX_8 = (ushort)uStackX_8._1_1_ << 8;
        (**(code **)(*plVar4 + 8))(plVar4,&uStackX_8);
        *plVar6 = *plVar6 + 1;
      }
      else {
        uStackX_8 = CONCAT11(uStackX_8._1_1_,1);
        (**(code **)(*plVar4 + 8))();
        *plVar6 = *plVar6 + 1;
        Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned_int>
                  (param_2,param_1[0xe] + 0x28);
      }
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_14230f710,2);
        *plVar6 = *plVar6 + 2;
        (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,"has-surface",0xb);
        *plVar6 = *plVar6 + 0xb;
        (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
        *plVar6 = *plVar6 + 1;
      }
      if ((param_1[0xe] != 0) && ((*(byte *)((longlong)param_1 + 0x6e) & 4) == 0)) {
        pcStack_58 = "entity-status";
        lStack_50 = param_2;
        if (*(char *)(param_2 + 8) != '\0') {
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),"entity-status",0xd);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xd;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        }
        puVar2 = (undefined1 *)(**(code **)(*param_1 + 0x628))(param_1,auStack_48);
        uStackX_8 = CONCAT11(uStackX_8._1_1_,*puVar2);
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),&uStackX_8,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        if (*(char *)(param_2 + 8) != '\0') {
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),"entity-status",0xd);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xd;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        }
      }
      uStackX_8 = CONCAT11(uStackX_8._1_1_,(char)(*(uint *)((longlong)param_1 + 0x14) >> 1)) &
                  0xff01;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&uStackX_8,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    puVar3 = (undefined8 *)(param_2 + 0x50);
    plVar4 = (longlong *)(param_2 + 0x10);
    Targetable__save(param_1,param_2);
    if ((*(char *)(param_2 + 9) != '\0') &&
       (lVar1 = (**(code **)(*param_1 + 0x7b8))(param_1), lVar1 != 0)) {
      pcStack_58 = "effect";
      lStack_50 = param_2;
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e010c,1);
        *plVar4 = *plVar4 + 1;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"effect",6);
        *plVar4 = *plVar4 + 6;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
        *plVar4 = *plVar4 + 1;
      }
      Effect__save(lVar1,param_2);
      if (*(char *)(param_2 + 8) != '\0') {
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_14230f710,2);
        *plVar4 = *plVar4 + 2;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,"effect",6);
        *plVar4 = *plVar4 + 6;
        (**(code **)(*(longlong *)*puVar3 + 8))((longlong *)*puVar3,&DAT_1421e0108,1);
        *plVar4 = *plVar4 + 1;
      }
    }
    if (*(char *)(lStack_60 + 8) != '\0') {
      (**(code **)(**(longlong **)(lStack_60 + 0x50) + 8))
                (*(longlong **)(lStack_60 + 0x50),&DAT_14230f710,2);
      *(longlong *)(lStack_60 + 0x10) = *(longlong *)(lStack_60 + 0x10) + 2;
      lVar1 = -1;
      do {
        lVar1 = lVar1 + 1;
      } while (*(char *)(lStack_68 + lVar1) != '\0');
      (**(code **)(**(longlong **)(lStack_60 + 0x50) + 8))
                (*(longlong **)(lStack_60 + 0x50),lStack_68,lVar1);
      *(longlong *)(lStack_60 + 0x10) = *(longlong *)(lStack_60 + 0x10) + lVar1;
      (**(code **)(**(longlong **)(lStack_60 + 0x50) + 8))
                (*(longlong **)(lStack_60 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(lStack_60 + 0x10) = *(longlong *)(lStack_60 + 0x10) + 1;
    }
    return;
  }
                    /* WARNING: Subroutine does not return */
  ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\Entity.cpp"
                      ,0xb9,"this->prototype->getID().getPrototypeSafe() == this->prototype");
}

