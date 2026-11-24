// ArtilleryProjectile::save @ 140369e20 (function ArtilleryProjectile::save)

void ArtilleryProjectile__save
               (longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  ulonglong uVar2;
  longlong *plVar3;
  longlong local_res8;
  
  Entity__save();
  UpdatableEntity__save(param_1 + 0x80,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xc0,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0xc1);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),param_1 + 200,8)
  ;
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"damage-estimate",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"damage-estimate",0xf);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xf;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  local_res8 = *(longlong *)(param_1 + 0xd8) - *(longlong *)(param_1 + 0xd0) >> 2;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  uVar2 = *(longlong *)(param_1 + 0xd8) - *(longlong *)(param_1 + 0xd0) & 0xfffffffffffffffc;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),*(longlong *)(param_1 + 0xd0),uVar2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + uVar2;
  local_res8 = *(longlong *)(param_1 + 0xf0) - *(longlong *)(param_1 + 0xe8) >> 2;
  Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>(param_2,&local_res8);
  uVar2 = *(longlong *)(param_1 + 0xf0) - *(longlong *)(param_1 + 0xe8) & 0xfffffffffffffffc;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),*(longlong *)(param_1 + 0xe8),uVar2);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + uVar2;
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),"damage-estimate",0xf);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0xf;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  plVar3 = (longlong *)(param_2 + 0x10);
  puVar1 = (undefined8 *)(param_2 + 0x50);
  (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,param_1 + 0x100,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TargeterBase__save(param_1 + 0x108,param_2);
  MapPosition__save(param_1 + 0x128,param_2);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0x130);
  MapSerialiser__operator<<<TriggerModifierData>(param_2,param_1 + 0x13c);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x150,8);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 8;
  TargeterBase__save(param_1 + 0x158,param_2);
  MapPosition__save(param_1 + 0x1b8,param_2);
  BoundingBox__save(param_1 + 0x1c0,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x1d4,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  MapPosition__save(param_1 + 0x1d8,param_2);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"targets",param_2);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),"targets",7);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 7;
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
    *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  }
  MapSerialiser__operator<<<std::vector<Targeter<CargoBay,19,16>,std::allocator<Targeter<CargoBay,19,16>_>_>_>
            (param_2,param_1 + 0x1e0);
  if (*(char *)(param_2 + 8) != '\0') {
    (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
              (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
    *plVar3 = *(longlong *)(param_2 + 0x10) + 2;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,"targets",7);
    *plVar3 = *plVar3 + 7;
    (**(code **)(*(longlong *)*puVar1 + 8))((longlong *)*puVar1,&DAT_1421e0108,1);
    *plVar3 = *plVar3 + 1;
  }
  return;
}

