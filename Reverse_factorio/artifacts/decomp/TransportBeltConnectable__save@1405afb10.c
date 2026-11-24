// TransportBeltConnectable::save @ 1405afb10 (function TransportBeltConnectable::save)

void TransportBeltConnectable__save(longlong *param_1,longlong param_2)

{
  byte *pbVar1;
  longlong lVar2;
  byte bVar3;
  longlong lVar4;
  undefined8 uVar5;
  longlong lVar6;
  byte bVar7;
  char *pcVar8;
  char local_res8 [8];
  undefined1 local_res18 [8];
  ulonglong local_res20;
  char *local_58;
  longlong local_50;
  undefined *local_48;
  longlong local_40;
  
  EntityWithOwner__save();
  TransportLineConnector__save(param_1 + 0x1a,param_2);
  TransportLineConnector__save(param_1 + 0x1f,param_2);
  pbVar1 = (byte *)(param_1 + 0x24);
  if ((*pbVar1 & 3) != 0) {
    uVar5 = Direction__str(pbVar1);
                    /* WARNING: Subroutine does not return */
    Logging__logAndAbortOrThrow
              ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\TransportBeltConnectable.cpp"
               ,0x74,9,"Saving with invalid transport belt direction: %s",uVar5);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),pbVar1,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),(longlong)param_1 + 0x121,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  if (*(char *)(param_2 + 9) != '\0') {
    pcVar8 = "";
    do {
      local_res8[0] = *pcVar8;
      if (local_res8[0] == '\0') {
        lVar4 = param_1[0x16];
      }
      else if (local_res8[0] == '\x04') {
        lVar4 = param_1[0x17];
      }
      else if (local_res8[0] == '\b') {
        lVar4 = param_1[0x18];
      }
      else {
        if (local_res8[0] != '\f') {
          AutomatedTrainState__operator_enum_AutomatedTrainState__Enum(local_res8);
                    /* WARNING: Subroutine does not return */
          Logging__logEnumAndAbortOrThrow<enum_SegmentedUnitController::ActivityMode>
                    ("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/FourDirectionPointers.hpp"
                     ,0x20);
        }
        lVar4 = param_1[0x19];
      }
      if (lVar4 != 0) {
        local_58 = "connection";
        local_50 = param_2;
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
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),pcVar8,1);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        uVar5 = FourDirectionPointers<TransportBeltConnectable>__operator__(param_1 + 0x16,*pcVar8);
        EntityWithOwner__saveUnitNumber(uVar5,param_2);
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
      pcVar8 = pcVar8 + 1;
    } while (pcVar8 != "");
    bVar3 = (**(code **)(*param_1 + 0x12f8))(param_1);
    bVar7 = 1;
    if (bVar3 != 0) {
      do {
        lVar6 = (**(code **)(*param_1 + 0x12f0))(param_1,bVar7);
        lVar4 = *(longlong *)(lVar6 + 0x10);
        if (lVar4 == 0) {
          local_res20 = local_res20 & 0xffffffffffffff00;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&local_res20,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
        }
        else {
          local_58 = (char *)0x0;
          local_res8[0] = '\0';
          local_res18[0] = 0;
          if (*(int *)(lVar6 + 4) == 0) {
            if (*(longlong *)(lVar4 + 0x70) == 0) goto LAB_1405aff73;
            local_58 = *(char **)(*(longlong *)
                                   (*(longlong *)
                                     (*(longlong *)(*(longlong *)(lVar4 + 0x70) + 0x118) + -8) +
                                   0x20) + 0x98);
            local_res8[0] =
                 *(char *)(*(longlong *)(*(longlong *)(*(longlong *)(lVar4 + 0x70) + 0x118) + -8) +
                          1);
            local_res18[0] = 0x73;
            if (*(int *)(lVar4 + 0x80) == 0) {
              local_res18[0] = 100;
            }
          }
          else {
            lVar2 = (ulonglong)(*(int *)(lVar6 + 4) - 1) * 8;
            local_58 = *(char **)(*(longlong *)
                                   (*(longlong *)(*(longlong *)(lVar4 + 0x110) + lVar2) + 0x20) +
                                 0x98);
            local_res8[0] = *(char *)(*(longlong *)(*(longlong *)(lVar4 + 0x110) + lVar2) + 1);
            local_res18[0] = 0x69;
          }
          local_48 = &DAT_1421fb784;
          local_40 = param_2;
          if (*(char *)(param_2 + 8) != '\0') {
            (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                      (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
            *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
            (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                      (*(longlong **)(param_2 + 0x50),&DAT_1421fb784,4);
            *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
            (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                      (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
            *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          }
          local_res20 = param_1[0x13];
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&local_res20,8);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),lVar6 + 1,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),&local_58,8);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),local_res8,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                    (*(longlong **)(param_2 + 0x50),local_res18,1);
          *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          if (*(char *)(param_2 + 8) != '\0') {
            (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                      (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
            *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
            (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                      (*(longlong **)(param_2 + 0x50),&DAT_1421fb784,4);
            *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
            (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                      (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
            *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
          }
        }
LAB_1405aff73:
        bVar7 = bVar7 + 1;
      } while (bVar7 <= bVar3);
    }
  }
  return;
}

