// RailSignalBase::save @ 1405207c0 (function RailSignalBase::save)

void RailSignalBase__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  undefined1 *puVar1;
  undefined1 *puVar2;
  longlong local_res8;
  
  EntityWithOwner__save();
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xf9,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8 = CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0xfa));
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(char *)(param_2 + 0x149) == '\0') {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"state",param_2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"state",5);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 5;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    local_res8 = CONCAT71(local_res8._1_7_,*(undefined1 *)(param_1 + 0xfc));
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"state",5);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 5;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    if ((*(longlong *)(param_1 + 0x110) == 0) != (*(int *)(param_1 + 0x128) == 0)) {
                    /* WARNING: Subroutine does not return */
      ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\RailSignalBase.cpp"
                          ,0x57,"!this->reservedFor == (this->reservedForCount == 0)");
    }
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x100,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    TargeterBase__save(param_1 + 0x108,param_2);
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned_int>(param_2,param_1 + 0x128)
    ;
    local_res8 = (*(longlong *)(param_1 + 0xd8) - *(longlong *)(param_1 + 0xd0) >> 3) *
                 -0x3333333333333333;
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    puVar1 = *(undefined1 **)(param_1 + 0xd8);
    for (puVar2 = *(undefined1 **)(param_1 + 0xd0); puVar2 != puVar1; puVar2 = puVar2 + 0x28) {
      TargeterBase__save(puVar2 + 8,param_2);
      local_res8 = CONCAT71(local_res8._1_7_,*puVar2);
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&local_res8,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),puVar2 + 1,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0xf0) != 0);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),&local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0xf0) != 0) {
    RailSignalBaseControlBehavior__save(*(longlong *)(param_1 + 0xf0),param_2);
  }
  return;
}

