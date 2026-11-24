// SpiderUnit::save @ 140583b60 (function SpiderUnit::save)

void SpiderUnit__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"enemy-data",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"enemy-data",10);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  EntityWithOwner__save(param_1,param_2);
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  Commandable__save(param_1 + 0xd0,param_2);
  MapSerialiser__operator<<<SpiderEngine>(param_2,param_1 + 0x168);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1c8,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1cc,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  AISettings__save(param_1 + 0x1d0,param_2);
  Vector2<double,Vector>__save(param_1 + 0x208,param_2);
  Vector2<double,Vector>__save(param_1 + 0x218,param_2);
  Vector2<double,Vector>__save(param_1 + 0x228,param_2);
  Vector2<double,Vector>__save(param_1 + 0x238,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x248,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x24c,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x250,4);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
  Shooter__save(param_1 + 600,param_2);
  EnemySpawnerLink__save(param_1 + 0x2c8,param_2);
  MapSerialiser__operator<<<ForceChartedArea>(param_2,param_1 + 0x2f0);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"enemy-data",10);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 10;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  return;
}

