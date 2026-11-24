// std::_Func_impl_no_alloc<`Map::createMultiThreadedUpdateRunner'::`2'::<lambda_1>,void,std::initializer_list<MultithreadedUpdatable_*>_>::_Do_call @ 1417cab60 (function std::_Func_impl_no_alloc<`Map::createMultiThreadedUpdateRunner'::`2'::<lambda_1>,void,std::initializer_list<MultithreadedUpdatable_*>_>::_Do_call)

void std___Func_impl_no_alloc<_Map::createMultiThreadedUpdateRunner_::_2_::<lambda_1>,void,std::initializer_list<MultithreadedUpdatable_*>_>___Do_call
               (longlong param_1,undefined8 *param_2)

{
  undefined8 *puVar1;
  uint uVar2;
  undefined8 *puVar3;
  undefined **local_60;
  undefined8 *local_58;
  undefined8 *local_50;
  uint local_48;
  undefined ***local_28;
  
  puVar1 = (undefined8 *)param_2[1];
  param_2 = (undefined8 *)*param_2;
  for (puVar3 = param_2; puVar3 != puVar1; puVar3 = puVar3 + 1) {
    (*(code *)**(undefined8 **)*puVar3)
              ((undefined8 *)*puVar3,*(undefined8 *)(*(longlong *)(param_1 + 8) + 0x1b0),
               *(undefined4 *)(param_1 + 0x20));
  }
  uVar2 = 0;
  *(undefined1 *)(*(longlong *)(param_1 + 8) + 0x760) = 1;
  if (*(int *)(param_1 + 0x20) != 0) {
    do {
      local_28 = &local_60;
      local_60 = std::
                 _Func_impl_no_alloc<`public:___cdecl_`public:_class_std::function<void___cdecl(std::initializer_list<MultithreadedUpdatable*___ptr64>)>___cdecl_Map::createMultiThreadedUpdateRunner(unsigned_int)___ptr64'::`2'::<lambda_1>::operator()(std::initializer_list<MultithreadedUpdatable*___ptr64>)const___ptr64'::`6'::<lambda_1>,void>
                 ::vftable;
      local_58 = param_2;
      local_50 = puVar1;
      local_48 = uVar2;
      ThreadPool__run(*(undefined8 *)(param_1 + 0x10),&local_60);
      uVar2 = uVar2 + 1;
    } while (uVar2 < *(uint *)(param_1 + 0x20));
  }
  ThreadPool__synchronize(*(undefined8 *)(param_1 + 0x10));
  *(undefined1 *)(*(longlong *)(param_1 + 8) + 0x760) = 0;
  for (; param_2 != puVar1; param_2 = param_2 + 1) {
    (**(code **)(*(longlong *)*param_2 + 0x10))();
  }
  return;
}

