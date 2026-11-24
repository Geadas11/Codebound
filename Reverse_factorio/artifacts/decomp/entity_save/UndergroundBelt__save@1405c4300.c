// UndergroundBelt::save @ 1405c4300 (function UndergroundBelt::save)

void UndergroundBelt__save(longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  undefined1 local_res8 [8];
  
  TransportBeltConnectable__save();
  local_res8[0] = *(undefined1 *)(param_1 + 0x128);
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  TransportLineConnector__save(param_1 + 0x138,param_2);
  TransportLineConnector__save(param_1 + 0x160,param_2);
  if ((*(char *)(param_2 + 9) != '\0') && (*(longlong *)(param_1 + 0x130) != 0)) {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,"underground-connection",
                 param_2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"underground-connection",0x16);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x16;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    EntityWithOwner__saveUnitNumber(*(undefined8 *)(param_1 + 0x130),param_2);
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"underground-connection",0x16);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x16;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  return;
}

