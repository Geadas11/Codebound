// WallConnectable::save @ 1405db4b0 (function WallConnectable::save)

void WallConnectable__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  undefined8 uVar2;
  char *pcVar3;
  char local_res10 [8];
  
  EntityWithOwner__save();
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 200,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  if (*(char *)(param_2 + 9) != '\0') {
    pcVar3 = "";
    do {
      local_res10[0] = *pcVar3;
      if (local_res10[0] == '\0') {
        lVar1 = *(longlong *)(param_1 + 0xa8);
      }
      else if (local_res10[0] == '\x04') {
        lVar1 = *(longlong *)(param_1 + 0xb0);
      }
      else if (local_res10[0] == '\b') {
        lVar1 = *(longlong *)(param_1 + 0xb8);
      }
      else {
        if (local_res10[0] != '\f') {
          AutomatedTrainState__operator_enum_AutomatedTrainState__Enum(local_res10);
                    /* WARNING: Subroutine does not return */
          Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                    ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/FourDirectionPointers.hpp"
                     ,0x20);
        }
        lVar1 = *(longlong *)(param_1 + 0xc0);
      }
      if (lVar1 != 0) {
        if (*(char *)(param_2 + 8) != '\0') {
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),"connection",10);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        }
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),pcVar3,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        local_res10[0] = *pcVar3;
        if (local_res10[0] == '\0') {
          uVar2 = *(undefined8 *)(param_1 + 0xa8);
        }
        else if (local_res10[0] == '\x04') {
          uVar2 = *(undefined8 *)(param_1 + 0xb0);
        }
        else if (local_res10[0] == '\b') {
          uVar2 = *(undefined8 *)(param_1 + 0xb8);
        }
        else {
          if (local_res10[0] != '\f') {
            AutomatedTrainState__operator_enum_AutomatedTrainState__Enum(local_res10);
                    /* WARNING: Subroutine does not return */
            Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                      ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/FourDirectionPointers.hpp"
                       ,0x20);
          }
          uVar2 = *(undefined8 *)(param_1 + 0xc0);
        }
        EntityWithOwner__saveUnitNumber(uVar2,param_2);
        if (*(char *)(param_2 + 8) != '\0') {
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),"connection",10);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        }
      }
      pcVar3 = pcVar3 + 1;
    } while (pcVar3 != "");
  }
  return;
}

