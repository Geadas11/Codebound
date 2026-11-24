// EntityGhost::save @ 14042fca0 (function EntityGhost::save)

void EntityGhost__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  Entity__save();
  if (*(char *)(param_2 + 0x14b) == '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x98,8);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  }
  WakeUpList__save(param_1 + 0x100,param_2);
  (**(code **)(**(longlong **)(param_1 + 0xa8) + 0x98))(*(longlong **)(param_1 + 0xa8),param_2);
  TargeterBase__save(param_1 + 0xb0,param_2);
  InsertPlan__save(param_1 + 0xd0,param_2);
  MapSerialiser__saveOptional<UniquePointer<TrainGhostData,SimpleDeleter<TrainGhostData>,1>_>
            (param_2,param_1 + 0x120);
  local_res8[0] = *(longlong *)(param_1 + 0x128) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0x128) != 0) {
    MapSerialiser__operator<<<std::map<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>,PropertyTree,std::less<void>,std::allocator<std::pair<std::basic_string<char,std::char_traits<char>,std::allocator<char>_>_const_,PropertyTree>_>_>_>
              (param_2);
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xa0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

