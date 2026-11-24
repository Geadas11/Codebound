// MultithreadingOutput::finish @ 1410fc550 (function MultithreadingOutput::finish)

void MultithreadingOutput__finish(longlong *param_1,undefined8 param_2)

{
  undefined8 *puVar1;
  ulonglong uVar2;
  longlong local_res8;
  undefined8 local_res10;
  
  uVar2 = param_1[1] - *param_1 >> 6;
  if (uVar2 <= (ulonglong)param_1[3]) {
    param_1[3] = uVar2;
    return;
  }
  local_res8 = uVar2 - param_1[3];
  puVar1 = (undefined8 *)param_1[5];
  if (puVar1 != (undefined8 *)param_1[6]) {
    *puVar1 = param_2;
    puVar1[1] = local_res8;
    param_1[5] = param_1[5] + 0x10;
    param_1[3] = uVar2;
    return;
  }
  local_res10 = param_2;
  std__vector<Pair<ChunkPosition,Chunk_const_*>,std::allocator<Pair<ChunkPosition,Chunk_const_*>_>_>___Emplace_reallocate<ChunkPosition_&,Chunk_const_*>
            (param_1 + 4,puVar1,&local_res10,&local_res8);
  param_1[3] = uVar2;
  return;
}

