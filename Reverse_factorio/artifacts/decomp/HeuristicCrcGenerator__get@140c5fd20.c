// HeuristicCrcGenerator::get @ 140c5fd20 (function HeuristicCrcGenerator::get)

undefined4 HeuristicCrcGenerator__get(undefined8 param_1)

{
  longlong lVar1;
  undefined4 uVar2;
  longlong *plVar3;
  longlong *plVar4;
  undefined1 local_res10 [24];
  undefined **local_188;
  undefined4 local_180;
  undefined ***local_178;
  undefined8 local_170;
  undefined4 local_168;
  longlong local_160;
  undefined1 local_158 [336];
  
  local_168 = 0x138;
  plVar3 = (longlong *)std__chrono__steady_clock__now(local_res10);
  lVar1 = *plVar3;
  local_188 = CrcWriteStream::vftable;
  local_180 = 0;
  local_160 = lVar1;
  local_180 = crc32_16bytes(0,0,0);
  local_178 = &local_188;
  local_170 = 0;
  MapSerialiser__MapSerialiser(local_158,&local_178,0,0);
  HeuristicCrcGenerator__saveMap(param_1,local_158);
  MapSerialiser___MapSerialiser(local_158);
  uVar2 = local_180;
  local_188 = WriteStream::vftable;
  plVar4 = (longlong *)std__chrono__steady_clock__now(local_res10);
  LOCK();
  plVar3 = (longlong *)(*(longlong *)(DAT_142d34fa0 + 0x1d0) + 0x158);
  *plVar3 = *plVar3 + (*plVar4 - lVar1);
  UNLOCK();
  return uVar2;
}

