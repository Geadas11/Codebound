// SegmentedUnit::save @ 1405e0b40 (function SegmentedUnit::save)

void SegmentedUnit__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"segmented-unit",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"segmented-unit",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"segment",7);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 7;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  EntityWithOwner__save(param_1,param_2);
  MapSerialiser__operator<<<EffectTriggerer>(param_2,param_1 + 0xb8);
  MapSerialiser__operator<<<EffectTriggerer>(param_2,param_1 + 0xf8);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x138,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"segment",7);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 7;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x150,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  Shooter__save(param_1 + 0x158,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"segmented-unit",0xe);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xe;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

