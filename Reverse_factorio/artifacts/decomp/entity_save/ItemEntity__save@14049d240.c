// ItemEntity::save @ 14049d240 (function ItemEntity::save)

void ItemEntity__save(longlong param_1,longlong param_2)

{
  Entity__save();
  ItemStack__save(param_1 + 0x80,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0x98,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  return;
}

