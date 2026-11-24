// Splitter::save @ 140593470 (function Splitter::save)

void Splitter__save(longlong param_1,longlong param_2)

{
  char cVar1;
  longlong lVar2;
  undefined8 uVar3;
  longlong *plVar4;
  char *pcVar5;
  undefined8 *puVar6;
  char *pcVar7;
  undefined8 *local_res8;
  longlong *local_res10;
  char *local_res18;
  
  TransportBeltConnectable__save();
  MapSerialiser__operator<<<SplitterLogic>(param_2,param_1 + 0x270);
  MapSerialiser__operator<<<SplitterLogic>(param_2,param_1 + 0x2b0);
  local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x2f0) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x2f0) != 0) {
    SplitterControlBehavior__save(*(longlong *)(param_1 + 0x2f0),param_2);
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    MapSerialiser__operator<<<SplitterStructureAnimation>(param_2,param_1 + 600);
    TransportLineConnector__save(param_1 + 0x168,param_2);
    TransportLineConnector__save(param_1 + 400,param_2);
    TransportLineConnector__save(param_1 + 0x1b8,param_2);
    TransportLineConnector__save(param_1 + 0x1e0,param_2);
    TransportLineConnector__save(param_1 + 0x208,param_2);
    TransportLineConnector__save(param_1 + 0x230,param_2);
  }
  puVar6 = (undefined8 *)(param_2 + 0x50);
  plVar4 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 9) != '\0') {
    pcVar7 = (char *)(param_2 + 8);
    if (*pcVar7 != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"left-belt",9);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 9;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_res18 = (char *)(param_2 + 8);
    local_res10 = (longlong *)(param_2 + 0x10);
    local_res8 = (undefined8 *)(param_2 + 0x50);
    MapPosition__save(param_1 + 0x148,param_2);
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x158,4);
    *plVar4 = *(longlong *)(param_2 + 0x10) + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x15c,4);
    *plVar4 = *plVar4 + 4;
    pcVar5 = pcVar7;
    if (*pcVar7 != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar4 = *plVar4 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"left-belt",9);
      *plVar4 = *plVar4 + 9;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
      plVar4 = local_res10;
      pcVar5 = local_res18;
      puVar6 = local_res8;
    }
    if (*pcVar7 != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e010c,1);
      *plVar4 = *plVar4 + 1;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"right-belt",10);
      *plVar4 = *plVar4 + 10;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
      pcVar7 = pcVar5;
    }
    MapPosition__save(param_1 + 0x150,param_2);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x160,4);
    *plVar4 = *plVar4 + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x164,4);
    *plVar4 = *plVar4 + 4;
    if (*pcVar7 != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar4 = *plVar4 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"right-belt",10);
      *plVar4 = *plVar4 + 10;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
    pcVar5 = "";
    do {
      cVar1 = *pcVar5;
      local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,cVar1);
      if (cVar1 == '\0') {
        lVar2 = *(longlong *)(param_1 + 0x128);
      }
      else if (cVar1 == '\x04') {
        lVar2 = *(longlong *)(param_1 + 0x130);
      }
      else if (cVar1 == '\b') {
        lVar2 = *(longlong *)(param_1 + 0x138);
      }
      else {
        if (cVar1 != '\f') {
          AutomatedTrainState__operator_enum_AutomatedTrainState__Enum(&local_res8);
                    /* WARNING: Subroutine does not return */
          Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                    ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/FourDirectionPointers.hpp"
                     ,0x20);
        }
        lVar2 = *(longlong *)(param_1 + 0x140);
      }
      if (lVar2 != 0) {
        if (*pcVar7 != '\0') {
          (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e010c,1);
          *plVar4 = *plVar4 + 1;
          (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"connection",10);
          *plVar4 = *plVar4 + 10;
          (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
          *plVar4 = *plVar4 + 1;
        }
        (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,pcVar5,1);
        *plVar4 = *plVar4 + 1;
        cVar1 = *pcVar5;
        local_res8 = (undefined8 *)CONCAT71(local_res8._1_7_,cVar1);
        if (cVar1 == '\0') {
          uVar3 = *(undefined8 *)(param_1 + 0x128);
        }
        else if (cVar1 == '\x04') {
          uVar3 = *(undefined8 *)(param_1 + 0x130);
        }
        else if (cVar1 == '\b') {
          uVar3 = *(undefined8 *)(param_1 + 0x138);
        }
        else {
          if (cVar1 != '\f') {
            AutomatedTrainState__operator_enum_AutomatedTrainState__Enum(&local_res8);
                    /* WARNING: Subroutine does not return */
            Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                      ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/FourDirectionPointers.hpp"
                       ,0x20);
          }
          uVar3 = *(undefined8 *)(param_1 + 0x140);
        }
        EntityWithOwner__saveUnitNumber(uVar3,param_2);
        if (*pcVar7 != '\0') {
          (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
          *plVar4 = *plVar4 + 2;
          (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"connection",10);
          *plVar4 = *plVar4 + 10;
          (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
          *plVar4 = *plVar4 + 1;
        }
      }
      pcVar5 = pcVar5 + 1;
    } while (pcVar5 != "");
  }
  return;
}

