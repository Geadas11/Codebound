// std::_Func_impl_no_alloc<``Map::createMultiThreadedUpdateRunner'::`2'::<lambda_1>::operator()'::`6'::<lambda_1>,void>::_Do_call @ 1417cad00 (function std::_Func_impl_no_alloc<``Map::createMultiThreadedUpdateRunner'::`2'::<lambda_1>::operator()'::`6'::<lambda_1>,void>::_Do_call)

void std___Func_impl_no_alloc<__Map::createMultiThreadedUpdateRunner_::_2_::<lambda_1>::operator()_::_6_::<lambda_1>,void>___Do_call
               (longlong param_1)

{
  undefined8 *puVar1;
  undefined8 *puVar2;
  
  puVar1 = *(undefined8 **)(param_1 + 0x10);
  for (puVar2 = *(undefined8 **)(param_1 + 8); puVar2 != puVar1; puVar2 = puVar2 + 1) {
    (**(code **)(*(longlong *)*puVar2 + 8))((longlong *)*puVar2,*(undefined4 *)(param_1 + 0x18));
  }
  return;
}

