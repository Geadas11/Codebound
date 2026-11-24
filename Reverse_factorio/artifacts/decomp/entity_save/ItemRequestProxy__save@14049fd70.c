// ItemRequestProxy::save @ 14049fd70 (function ItemRequestProxy::save)

void ItemRequestProxy__save(longlong param_1,longlong param_2)

{
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  LogisticPointFilters__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1a0,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1a8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x1b0,param_2);
  MapSerialiser__operator<<<FlatMap<IDWithQuality<ID<ItemPrototype,unsigned_short>_>,unsigned_int,std::less<void>_>_>
            (param_2,param_1 + 0x1d8);
  MapSerialiser__operator<<<FlatMap<IDWithQuality<ID<ItemPrototype,unsigned_short>_>,unsigned_int,std::less<void>_>_>
            (param_2,param_1 + 0x1f0);
  InsertPlan__save(param_1 + 0x208,param_2);
  InsertPlan__save(param_1 + 0x220,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1ab,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (((*(byte *)(param_1 + 0xa0) & 1) == 0) && (*(longlong *)(param_1 + 0x1b8) == 0)) {
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Entity\\ItemRequestProxy.cpp"
                        ,0x5d,"this->isActive() || this->entityTarget");
  }
  if (*(char *)(param_2 + 9) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x1a9,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

