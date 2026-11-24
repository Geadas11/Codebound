// TransportLineManager::finish @ 14020bce0 (function TransportLineManager::finish)

void TransportLineManager__finish(longlong param_1)

{
  longlong lVar1;
  longlong *plVar2;
  longlong lVar3;
  longlong local_18;
  longlong local_10;
  
  *(undefined1 *)(param_1 + 0x65) = 0;
  lVar3 = *(longlong *)(param_1 + 0x68);
  lVar1 = *(longlong *)(param_1 + 0x70) - lVar3 >> 3;
  local_10 = lVar1 * 0x6db6db6db6db6db7;
  local_18 = lVar3;
  MultithreadingOutput__dispatchActions(&local_18);
  lVar1 = lVar1 * 8 + lVar3;
  if (lVar3 != lVar1) {
    plVar2 = (longlong *)(lVar3 + 0x28);
    do {
      std__vector<std::function<void___cdecl(void)>,std::allocator<std::function<void___cdecl(void)>_>_>__clear
                (lVar3);
      plVar2[-2] = 0;
      if (plVar2[-1] != *plVar2) {
        *plVar2 = plVar2[-1];
      }
      lVar3 = lVar3 + 0x38;
      plVar2 = plVar2 + 7;
    } while (lVar3 != lVar1);
  }
  return;
}

