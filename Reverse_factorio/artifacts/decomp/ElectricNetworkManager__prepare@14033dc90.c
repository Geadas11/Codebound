// ElectricNetworkManager::prepare @ 14033dc90 (function ElectricNetworkManager::prepare)

undefined1 ElectricNetworkManager__prepare(longlong param_1,undefined8 param_2)

{
  undefined1 uVar1;
  
  *(undefined8 *)(param_1 + 0x70) = param_2;
  LOCK();
  uVar1 = *(undefined1 *)(param_1 + 0x78);
  *(undefined1 *)(param_1 + 0x78) = 0;
  UNLOCK();
  return uVar1;
}

