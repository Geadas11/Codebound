// EntityWithHealth::save @ 140441af0 (function EntityWithHealth::save)

void EntityWithHealth__save(longlong param_1,longlong param_2)

{
  Entity__save();
  if ((*(float *)(param_1 + 0x80) != DAT_1424022c0) || (*(float *)(param_1 + 0x8c) != 0.0)) {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),(float *)(param_1 + 0x80),4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),param_1 + 0x8c,4);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  }
  if (((*(char *)(param_2 + 9) != '\0') || ((*(byte *)(param_1 + 0x6e) & 1) != 0)) &&
     (ID<EntityPrototype,unsigned_short>__save(param_1 + 0x86,param_2),
     *(short *)(param_1 + 0x86) != 0)) {
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x88);
  }
  return;
}

