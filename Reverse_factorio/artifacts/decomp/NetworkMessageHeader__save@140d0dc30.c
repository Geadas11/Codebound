// NetworkMessageHeader::save @ 140d0dc30 (function NetworkMessageHeader::save)

void NetworkMessageHeader__save(byte *param_1,longlong param_2)

{
  longlong lVar1;
  char cVar2;
  __time64_t _Var3;
  uint uVar4;
  uint *puVar5;
  uint uVar6;
  uint uVar7;
  byte local_res8 [8];
  ushort local_res10 [4];
  
  lVar1 = *(longlong *)ThreadLocalStoragePointer;
  puVar5 = (uint *)(lVar1 + 0x650);
  if ((*(uint *)(lVar1 + 0x6d4) & 1) == 0) {
    *(uint *)(lVar1 + 0x6d4) = *(uint *)(lVar1 + 0x6d4) | 1;
    _Var3 = _time64((__time64_t *)0x0);
    uVar4 = (uint)_Var3;
    *puVar5 = uVar4;
    *(uint *)(lVar1 + 0x654) = uVar4;
    *(uint *)(lVar1 + 0x658) = uVar4;
  }
  else {
    uVar4 = *puVar5;
  }
  local_res8[0] = *param_1;
  if (param_1[1] != 0) {
    local_res8[0] = local_res8[0] | 0x40;
  }
  if (param_1[2] != 0) {
    local_res8[0] = local_res8[0] | 0x80;
  }
  uVar7 = (uVar4 >> 0xd ^ uVar4 & 0x7ffc0) >> 6 ^ (uVar4 & 0xfffffffe) << 0xc;
  *puVar5 = uVar7;
  uVar4 = *(uint *)(lVar1 + 0x654);
  uVar6 = (uVar4 >> 2 ^ uVar4 & 0x3f800000) >> 0x17 ^ (uVar4 & 0xfffffff8) << 4;
  *(uint *)(lVar1 + 0x654) = uVar6;
  uVar4 = *(uint *)(lVar1 + 0x658);
  uVar4 = (uVar4 >> 3 ^ uVar4 & 0x1fffff00) >> 8 ^ (uVar4 & 0xfffffff0) << 0x11;
  *(uint *)(lVar1 + 0x658) = uVar4;
  if (((uVar7 ^ uVar4 ^ uVar6) & 1) != 0) {
    local_res8[0] = local_res8[0] | 0x20;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res10[0] = *(ushort *)(param_1 + 4);
  if (*(longlong *)(param_1 + 8) != *(longlong *)(param_1 + 0x10)) {
    local_res10[0] = local_res10[0] | 0x8000;
  }
  cVar2 = NetworkMessageType__reliable(param_1);
  if ((cVar2 != '\0') || (param_1[1] != 0)) {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res10,2)
    ;
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    if (param_1[1] != 0) {
      Serialiser__saveSpaceOptimized<unsigned_char,unsigned_short,unsigned_short>
                (param_2,param_1 + 6);
    }
  }
  if (*(longlong *)(param_1 + 8) != *(longlong *)(param_1 + 0x10)) {
    MapSerialiser__operator<<<std::vector<unsigned_int,std::allocator<unsigned_int>_>_>
              (param_2,param_1 + 8);
  }
  return;
}

