// ProxyContainer::save @ 1404f63d0 (function ProxyContainer::save)

void ProxyContainer__save(longlong param_1,longlong param_2)

{
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  WakeUpList__save(param_1 + 0xa8,param_2);
  TargeterBase__save(param_1 + 200,param_2);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
            (*(longlong **)(param_2 + 0x50),param_1 + 0xe9,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  local_res8[0] = *(longlong *)(param_1 + 0xf0) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  if (*(longlong *)(param_1 + 0xf0) != 0) {
    ProxyContainerControlBehavior__save(*(longlong *)(param_1 + 0xf0),param_2);
  }
  return;
}

