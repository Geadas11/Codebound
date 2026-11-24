// TransportLineManager::update @ 14020b7b0 (function TransportLineManager::update)

void TransportLineManager__update(longlong param_1,uint param_2)

{
  longlong *plVar1;
  undefined8 *puVar2;
  ulonglong *puVar3;
  undefined8 uVar4;
  longlong lVar5;
  undefined8 *puVar6;
  ulonglong uVar7;
  undefined8 *puVar8;
  longlong lVar9;
  longlong lVar10;
  longlong lVar11;
  undefined **local_48;
  longlong local_40;
  longlong *local_38;
  longlong local_30;
  
  uVar4 = *(undefined8 *)(*(longlong *)(param_1 + 8) + 0x1b0);
  if ((ulonglong)
      ((*(longlong *)(param_1 + 0x70) - *(longlong *)(param_1 + 0x68) >> 3) * 0x6db6db6db6db6db7) <=
      (ulonglong)param_2) {
                    /* WARNING: Subroutine does not return */
    ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Belt\\TransportLineManager.cpp"
                        ,0x6d,"threadIndex < this->updateThreadOutputs.size()");
  }
  lVar11 = (ulonglong)param_2 * 0x38 + *(longlong *)(param_1 + 0x68);
  local_48 = `private:_virtual_void___cdecl_TransportLineManager::update(unsigned_int)___ptr64'::
             __l2::ThreadContext::vftable;
  LOCK();
  puVar3 = (ulonglong *)(param_1 + 0x80);
  uVar7 = *puVar3;
  *puVar3 = *puVar3 + 1;
  UNLOCK();
  lVar9 = *(longlong *)(param_1 + 0x28);
  if (uVar7 < (ulonglong)(*(longlong *)(param_1 + 0x30) - lVar9 >> 3)) {
    local_40 = lVar11;
    do {
      lVar9 = *(longlong *)(lVar9 + uVar7 * 8);
      plVar1 = (longlong *)(lVar9 + 0x10);
      lVar5 = *plVar1;
      *plVar1 = (longlong)&local_48;
      puVar2 = (undefined8 *)(lVar9 + 0x48);
      puVar6 = (undefined8 *)*puVar2;
      *(undefined8 **)(lVar9 + 0x58) = puVar6;
      local_38 = plVar1;
      local_30 = lVar5;
      while (puVar6 != puVar2) {
        puVar8 = puVar6 + -4;
        if (puVar6 == (undefined8 *)0x0) {
          puVar8 = (undefined8 *)0x0;
        }
        puVar6 = (undefined8 *)*puVar6;
        *(undefined8 **)(lVar9 + 0x58) = puVar6;
        if (puVar6 != puVar2) {
          lVar10 = 5;
          do {
            lVar10 = lVar10 + -1;
          } while (lVar10 != 0);
        }
        TransportLine__update(puVar8,uVar4,0);
        puVar6 = *(undefined8 **)(lVar9 + 0x58);
      }
      *(undefined8 *)(lVar9 + 0x58) = 0;
      *plVar1 = lVar5;
      MultithreadingOutput__finish(lVar11,*(undefined4 *)(lVar9 + 0x38));
      LOCK();
      puVar3 = (ulonglong *)(param_1 + 0x80);
      uVar7 = *puVar3;
      *puVar3 = *puVar3 + 1;
      UNLOCK();
      lVar9 = *(longlong *)(param_1 + 0x28);
    } while (uVar7 < (ulonglong)(*(longlong *)(param_1 + 0x30) - lVar9 >> 3));
  }
  return;
}

