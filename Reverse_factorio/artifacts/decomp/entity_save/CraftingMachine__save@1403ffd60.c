// CraftingMachine::save @ 1403ffd60 (function CraftingMachine::save)

void CraftingMachine__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  char cVar1;
  longlong *plVar2;
  longlong *plVar3;
  longlong *plVar4;
  undefined8 *puVar5;
  undefined8 *puVar6;
  undefined8 local_res8;
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(*(longlong *)(param_1 + 0x140) + 0xd8))((longlong *)(param_1 + 0x140),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x168) + 0xd8))((longlong *)(param_1 + 0x168),param_2);
  (**(code **)(*(longlong *)(param_1 + 400) + 0xd8))((longlong *)(param_1 + 400),param_2);
  (**(code **)(*(longlong *)(param_1 + 0x1c0) + 0xd8))((longlong *)(param_1 + 0x1c0),param_2);
  (**(code **)(**(longlong **)(param_1 + 0x1e8) + 0x50))(*(longlong **)(param_1 + 0x1e8),param_2);
  MapSerialiser__operator<<<EffectReceiver>(param_2,param_1 + 0x1f8);
  puVar6 = (undefined8 *)(param_2 + 0x50);
  plVar3 = (longlong *)(param_2 + 0x10);
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x1f0,8);
    *plVar3 = *plVar3 + 8;
  }
  SerialiserHelper<MapSerialiser>__save<ID<RecipePrototype,unsigned_short>_>
            (param_2,param_1 + 0x284);
  if (*(short *)(param_1 + 0x284) != 0) {
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x286);
  }
  if (*(char *)(param_2 + 0x149) == '\0') {
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x240,8);
    *plVar3 = *plVar3 + 8;
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x248);
    MapSerialiser__operator<<<ID<QualityPrototype,unsigned_char>_>(param_2,param_1 + 0x249);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x24c,4);
    *plVar3 = *plVar3 + 4;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x250,8);
    *plVar3 = *plVar3 + 8;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 600,8);
    *plVar3 = *plVar3 + 8;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x260,8);
    *plVar3 = *plVar3 + 8;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))
                ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"spoil-ticks",param_2);
      *plVar3 = *plVar3 + 1;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"spoil-ticks",0xb);
      *plVar3 = *plVar3 + 0xb;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
    puVar5 = (undefined8 *)(param_2 + 0x50);
    plVar4 = (longlong *)(param_2 + 0x10);
    MapSerialiser__saveOptional<std::unique_ptr<std::map<ID<ItemPrototype,unsigned_short>,MapTick,std::less<ID<ItemPrototype,unsigned_short>_>,std::allocator<std::pair<ID<ItemPrototype,unsigned_short>_const_,MapTick>_>_>,std::default_delete<std::map<ID<ItemPrototype,unsigned_short>,MapTick,std::less<ID<ItemPrototype,unsigned_short>_>,std::allocator<std::pair<ID<ItemPrototype,unsigned_short>_const_,MapTick>_>_>_>_>_>
              (param_2,param_1 + 0x268);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar3 = *plVar3 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"spoil-ticks",0xb);
      *plVar3 = *plVar3 + 0xb;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar5 + 8))
                ((longlong *)*puVar5,&DAT_1421e010c,1,param_4,"ingredient-temperatures",param_2);
      *plVar4 = *plVar4 + 1;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,"ingredient-temperatures",0x17);
      *plVar4 = *plVar4 + 0x17;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
    local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x270) != 0);
    (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&local_res8,1);
    *plVar4 = *plVar4 + 1;
    puVar6 = *(undefined8 **)(param_1 + 0x270);
    if (puVar6 != (undefined8 *)0x0) {
      local_res8 = puVar6[1];
      Serialiser__saveSpaceOptimized<unsigned_char,unsigned_int,unsigned___int64>
                (param_2,&local_res8);
      plVar3 = *(longlong **)*puVar6;
      cVar1 = *(char *)((longlong)plVar3 + 0x19);
      while (cVar1 == '\0') {
        if (*(short *)((longlong)plVar3 + 0x1c) == 0) {
                    /* WARNING: Subroutine does not return */
          ReleaseAssertFailed("C:\\Users\\build\\AppData\\Local\\Temp\\factorio-build-JmH45D\\src\\Util/SerialiserHelper.hpp"
                              ,0xd9,"bool(item.first)");
        }
        ID<FluidPrototype,unsigned_short>__save((longlong)plVar3 + 0x1c,param_2);
        (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                  (*(longlong **)(param_2 + 0x50),plVar3 + 4,4);
        *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 4;
        plVar4 = (longlong *)plVar3[2];
        if (*(char *)((longlong)plVar4 + 0x19) == '\0') {
          cVar1 = *(char *)(*plVar4 + 0x19);
          plVar3 = plVar4;
          plVar4 = (longlong *)*plVar4;
          while (cVar1 == '\0') {
            cVar1 = *(char *)(*plVar4 + 0x19);
            plVar3 = plVar4;
            plVar4 = (longlong *)*plVar4;
          }
        }
        else {
          cVar1 = *(char *)(plVar3[1] + 0x19);
          plVar2 = (longlong *)plVar3[1];
          plVar4 = plVar3;
          while ((plVar3 = plVar2, cVar1 == '\0' && (plVar4 == (longlong *)plVar3[2]))) {
            cVar1 = *(char *)(plVar3[1] + 0x19);
            plVar2 = (longlong *)plVar3[1];
            plVar4 = plVar3;
          }
        }
        cVar1 = *(char *)((longlong)plVar3 + 0x19);
      }
    }
    puVar6 = (undefined8 *)(param_2 + 0x50);
    plVar3 = (longlong *)(param_2 + 0x10);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_14230f710,2);
      *plVar3 = *plVar3 + 2;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,"ingredient-temperatures",0x17);
      *plVar3 = *plVar3 + 0x17;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
    local_res8 = CONCAT71(local_res8._1_7_,*(longlong *)(param_1 + 0x278) != 0);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
    *plVar3 = *plVar3 + 1;
    if (*(longlong *)(param_1 + 0x278) != 0) {
      DropTarget__save(*(longlong *)(param_1 + 0x278),param_2);
    }
  }
  puVar6 = (undefined8 *)(param_2 + 0x50);
  plVar3 = (longlong *)(param_2 + 0x10);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x280,1);
  *plVar3 = *plVar3 + 1;
  local_res8._0_1_ = *(undefined1 *)(param_1 + 0x281);
  (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
  *plVar3 = *plVar3 + 1;
  WakeUpList__save(param_1 + 0x2a8,param_2);
  WakeUpList__save(param_1 + 0x2c8,param_2);
  FluidBoxManager__save(*(undefined8 *)(param_1 + 0x2e8),param_2);
  if (*(char *)(param_2 + 0x149) == '\0') {
    local_res8._0_1_ = *(byte *)(param_1 + 0x282) & 0x31;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
    *plVar3 = *plVar3 + 1;
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x288,4);
    *plVar3 = *plVar3 + 4;
    MapSerialiser__operator<<<CraftingMachineAnimationState>(param_2,param_1 + 0x290);
    local_res8._0_1_ = *(undefined1 *)(param_1 + 0x283);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&local_res8,1);
    *plVar3 = *plVar3 + 1;
  }
  puVar5 = (undefined8 *)(param_2 + 0x50);
  if (*(char *)(param_2 + 9) != '\0') {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar6 + 8))
                ((longlong *)*puVar6,&DAT_1421e010c,1,param_4,"cached-recipe-production-bonus",
                 param_2);
      *plVar3 = *plVar3 + 1;
      (**(code **)(*(longlong *)*puVar6 + 8))
                ((longlong *)*puVar6,"cached-recipe-production-bonus",0x1e);
      *plVar3 = *plVar3 + 0x1e;
      (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,&DAT_1421e0108,1);
      *plVar3 = *plVar3 + 1;
    }
    plVar4 = (longlong *)(param_2 + 0x10);
    (**(code **)(*(longlong *)*puVar6 + 8))((longlong *)*puVar6,param_1 + 0x28c,4);
    *plVar4 = *plVar3 + 4;
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_14230f710,2);
      *plVar4 = *plVar4 + 2;
      (**(code **)(*(longlong *)*puVar5 + 8))
                ((longlong *)*puVar5,"cached-recipe-production-bonus",0x1e);
      *plVar4 = *plVar4 + 0x1e;
      (**(code **)(*(longlong *)*puVar5 + 8))((longlong *)*puVar5,&DAT_1421e0108,1);
      *plVar4 = *plVar4 + 1;
    }
  }
  return;
}

