// Heartbeat::saveBase @ 140cfef50 (function Heartbeat::saveBase)

void Heartbeat__saveBase(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong lVar2;
  longlong lVar3;
  undefined8 uVar4;
  longlong local_res8;
  
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),param_1 + 8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  lVar1 = *(longlong *)(param_1 + 0x18);
  lVar3 = *(longlong *)(param_1 + 0x10);
  lVar2 = lVar3;
  if (lVar3 != lVar1) {
    do {
      if ((*(longlong *)(lVar2 + 8) != *(longlong *)(lVar2 + 0x10)) ||
         (*(longlong *)(lVar2 + 0x20) != *(longlong *)(lVar2 + 0x28))) {
        uVar4 = 0;
        goto LAB_140cfefb7;
      }
      lVar2 = lVar2 + 0x38;
    } while (lVar2 != lVar1);
    uVar4 = 1;
LAB_140cfefb7:
    local_res8 = (lVar1 - lVar3 >> 3) * 0x6db6db6db6db6db7;
    if (local_res8 == 1) {
      TickClosure__save(lVar3,param_2,uVar4);
      return;
    }
    Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8)
    ;
    lVar1 = *(longlong *)(param_1 + 0x18);
    for (lVar3 = *(longlong *)(param_1 + 0x10); lVar3 != lVar1; lVar3 = lVar3 + 0x38) {
      TickClosure__save(lVar3,param_2,uVar4);
    }
  }
  return;
}

