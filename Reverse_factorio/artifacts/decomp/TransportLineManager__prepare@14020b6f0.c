// TransportLineManager::prepare @ 14020b6f0 (function TransportLineManager::prepare)

void TransportLineManager__prepare(longlong param_1,ulonglong param_2,ulonglong param_3)

{
  longlong lVar1;
  
  lVar1 = *(longlong *)(param_1 + 0x58);
  while (lVar1 != 0) {
    lVar1 = **(longlong **)(param_1 + 0x50);
    if ((*(ulonglong *)(lVar1 + 0x28) != param_2) && (param_2 <= *(ulonglong *)(lVar1 + 0x28)))
    break;
    if (*(char *)(*(longlong *)(*(longlong *)(lVar1 + 0x20) + 0x30) + 0x3c) != '\0') {
      TransportLineGroup__extract();
    }
    TransportLineManager__remergeTransportLineSequence
              (param_1,*(undefined8 *)(lVar1 + 0x20),*(undefined1 *)(lVar1 + 0x30));
    lVar1 = *(longlong *)(param_1 + 0x58);
  }
  LOCK();
  *(undefined8 *)(param_1 + 0x80) = 0;
  UNLOCK();
  if ((*(longlong *)(param_1 + 0x70) - *(longlong *)(param_1 + 0x68) >> 3) * 0x6db6db6db6db6db7 -
      (param_3 & 0xffffffff) != 0) {
    std__vector<MultithreadingOutput,std::allocator<MultithreadingOutput>_>__resize();
  }
  *(undefined1 *)(param_1 + 0x65) = 1;
  return;
}

