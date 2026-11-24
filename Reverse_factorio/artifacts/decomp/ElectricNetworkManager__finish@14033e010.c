// ElectricNetworkManager::finish @ 14033e010 (function ElectricNetworkManager::finish)

void ElectricNetworkManager__finish(longlong param_1)

{
  longlong *plVar1;
  longlong *plVar2;
  
  if (*(char *)(*(longlong *)(param_1 + 8) + 10) == '\0') {
    plVar2 = *(longlong **)(param_1 + 0x10);
    if (plVar2 != (longlong *)(param_1 + 0x10)) {
      do {
        plVar1 = plVar2 + -3;
        if (plVar2 == (longlong *)0x0) {
          plVar1 = (longlong *)0x0;
        }
        if (((char)plVar1[7] != '\x01') && ((char)plVar1[7] == '\0')) {
          ElectricNetwork__checkAchievements(plVar1,*(undefined8 *)(param_1 + 8));
        }
        plVar2 = (longlong *)*plVar2;
      } while (plVar2 != (longlong *)(param_1 + 0x10));
    }
  }
  return;
}

