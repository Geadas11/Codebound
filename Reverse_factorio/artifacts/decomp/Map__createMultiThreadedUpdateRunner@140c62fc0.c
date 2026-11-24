// Map::createMultiThreadedUpdateRunner @ 140c62fc0 (function Map::createMultiThreadedUpdateRunner)

undefined8 *
Map__createMultiThreadedUpdateRunner(undefined8 param_1,undefined8 *param_2,undefined4 param_3)

{
  undefined8 *puVar1;
  
  puVar1 = (undefined8 *)operator_new(0x150);
  *puVar1 = 0;
  puVar1[1] = 0;
  *(undefined4 *)(puVar1 + 1) = 1;
  *(undefined4 *)((longlong)puVar1 + 0xc) = 1;
  *puVar1 = std::_Ref_count_obj2<ThreadPool>::vftable;
  ThreadPool__ThreadPool(puVar1 + 2,param_3,"MultiThreadedUpdateRunner",0x20000);
  *param_2 = std::
             _Func_impl_no_alloc<`public:_class_std::function<void___cdecl(std::initializer_list<MultithreadedUpdatable*___ptr64>)>___cdecl_Map::createMultiThreadedUpdateRunner(unsigned_int)___ptr64'::`2'::<lambda_1>,void,std::initializer_list<MultithreadedUpdatable*___ptr64>_>
             ::vftable;
  param_2[1] = param_1;
  param_2[2] = puVar1 + 2;
  param_2[3] = puVar1;
  *(undefined4 *)(param_2 + 4) = param_3;
  param_2[7] = param_2;
  return param_2;
}

