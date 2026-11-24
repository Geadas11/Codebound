// Combinator::save @ 1403e82d0 (function Combinator::save)

void Combinator__save(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong *plVar2;
  undefined8 local_res8;
  
  EntityWithOwner__save();
  (**(code **)(**(longlong **)(param_1 + 0xb0) + 0x50))(*(longlong **)(param_1 + 0xb0),param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  plVar2 = (longlong *)(param_1 + 0xc0);
  local_res8 = *(undefined8 *)(param_1 + 0xd0);
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  lVar1 = *(longlong *)(param_1 + 0xd0);
  if (0xf < *(ulonglong *)(param_1 + 0xd8)) {
    plVar2 = (longlong *)*plVar2;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),plVar2,lVar1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + lVar1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xb9,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

