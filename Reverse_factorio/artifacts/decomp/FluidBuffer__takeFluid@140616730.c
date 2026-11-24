// FluidBuffer::takeFluid @ 140616730 (function FluidBuffer::takeFluid)

longlong * FluidBuffer__takeFluid(short *param_1,longlong *param_2,short *param_3,longlong param_4)

{
  short sVar1;
  longlong lVar2;
  char cVar3;
  longlong lVar4;
  undefined1 local_res8 [8];
  
  sVar1 = *param_3;
  if ((((*param_1 == 0) || (sVar1 == 0)) || (*param_1 == sVar1)) &&
     (((param_1[0xc] == 0 || (sVar1 == 0)) || (param_1[0xc] == sVar1)))) {
    lVar4 = *(longlong *)(param_1 + 8) - *(longlong *)(param_1 + 4);
    lVar2 = *(longlong *)(param_3 + 4);
    if (lVar2 == param_4) {
      cVar3 = '\0';
    }
    else {
      cVar3 = -1;
      if (param_4 <= lVar2) {
        cVar3 = '\x01';
      }
    }
    if (cVar3 < '\0') {
      param_4 = lVar2;
    }
    if (param_4 == lVar4) {
      cVar3 = '\0';
    }
    else {
      cVar3 = -1;
      if (lVar4 <= param_4) {
        cVar3 = '\x01';
      }
    }
    if (cVar3 < '\0') {
      lVar4 = param_4;
    }
    if (lVar4 != 0) {
      Fluid__takeFluid(param_1,local_res8,param_3);
    }
    *(longlong *)(param_1 + 0x10) = *(longlong *)(param_1 + 0x10) + lVar4;
    *param_2 = lVar4;
    return param_2;
  }
  *param_2 = 0;
  return param_2;
}

