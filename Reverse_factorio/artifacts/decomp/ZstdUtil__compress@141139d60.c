// ZstdUtil::compress @ 141139d60 (function ZstdUtil::compress)

undefined8 * ZstdUtil__compress(undefined8 *param_1,undefined8 *param_2)

{
  ulonglong uVar1;
  undefined8 uVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  undefined8 *puVar5;
  undefined8 *puVar6;
  longlong lVar7;
  undefined1 local_70 [24];
  undefined1 local_58 [32];
  undefined1 local_38 [32];
  
  uVar1 = param_2[2];
  uVar4 = 0;
  if (uVar1 < 0x20000) {
    uVar4 = 0x20000 - uVar1 >> 0xb;
  }
  *param_1 = 0;
  param_1[1] = 0;
  param_1[2] = 0;
  param_1[3] = 0xf;
  *(undefined1 *)param_1 = 0;
  lVar7 = (uVar1 >> 8) + uVar4 + uVar1;
  if (lVar7 == 0) {
    param_1[2] = 0;
    *(undefined1 *)param_1 = 0;
  }
  else {
    std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append(param_1,lVar7,0);
  }
  puVar6 = param_2 + 2;
  if (0xf < (ulonglong)param_2[3]) {
    param_2 = (undefined8 *)*param_2;
  }
  puVar5 = param_1;
  if (0xf < (ulonglong)param_1[3]) {
    puVar5 = (undefined8 *)*param_1;
  }
  uVar1 = ZSTD_compress(puVar5,lVar7,param_2,*puVar6,9);
  if (uVar1 < 0xffffffffffffff89) {
    if ((ulonglong)param_1[2] < uVar1) {
      std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                (param_1,uVar1 - param_1[2],0);
    }
    else {
      param_1[2] = uVar1;
      puVar6 = param_1;
      if (0xf < (ulonglong)param_1[3]) {
        puVar6 = (undefined8 *)*param_1;
      }
      *(undefined1 *)((longlong)puVar6 + uVar1) = 0;
    }
    return param_1;
  }
  uVar2 = ZSTD_getErrorName(uVar1);
  uVar3 = std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__basic_string<char,std::char_traits<char>,std::allocator<char>_>
                    (local_58,"Compression failed: ");
  uVar2 = std__operator_<char,std::char_traits<char>,std::allocator<char>_>(local_38,uVar3,uVar2);
  RuntimeError__RuntimeError(local_70,uVar2);
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(local_70,(ThrowInfo *)&DAT_1427b2490);
}

