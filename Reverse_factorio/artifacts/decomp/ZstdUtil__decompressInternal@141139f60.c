// ZstdUtil::decompressInternal @ 141139f60 (function ZstdUtil::decompressInternal)

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

ulonglong ZstdUtil__decompressInternal(undefined8 *param_1,undefined8 *param_2)

{
  ulonglong uVar1;
  undefined8 uVar2;
  longlong lVar3;
  ulonglong uVar4;
  longlong lVar5;
  undefined8 *puVar6;
  undefined8 *puVar7;
  undefined8 *local_a8;
  ulonglong local_a0;
  ulonglong local_98;
  undefined8 local_88;
  code *pcStack_80;
  undefined8 *local_78;
  undefined8 uStack_70;
  ulonglong local_68;
  ulonglong local_60 [2];
  undefined8 local_50;
  undefined8 local_28;
  
  if (param_1[2] == 0) {
    param_2[2] = 0;
    if (0xf < (ulonglong)param_2[3]) {
      param_2 = (undefined8 *)*param_2;
    }
LAB_141139f9f:
    *(undefined1 *)param_2 = 0;
    return 0;
  }
  puVar6 = param_1;
  if (0xf < (ulonglong)param_1[3]) {
    puVar6 = (undefined8 *)*param_1;
  }
  lVar3 = ZSTD_getFrameHeader_advanced(local_60,puVar6,param_1[2],0);
  if (lVar3 == 0) {
    if ((local_50._4_4_ == 1) || (local_60[0] == 0)) {
      param_2[2] = 0;
      if (0xf < (ulonglong)param_2[3]) {
        param_2 = (undefined8 *)*param_2;
      }
      goto LAB_141139f9f;
    }
    if (local_60[0] != 0xfffffffffffffffe) {
      uVar4 = param_2[2];
      if (local_60[0] != 0xffffffffffffffff) {
        if (uVar4 < local_60[0]) {
          std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                    (param_2,local_60[0] - uVar4,0);
        }
        else {
          param_2[2] = local_60[0];
          puVar6 = param_2;
          if (0xf < (ulonglong)param_2[3]) {
            puVar6 = (undefined8 *)*param_2;
          }
          *(undefined1 *)((longlong)puVar6 + local_60[0]) = 0;
        }
        puVar6 = param_1 + 2;
        if (0xf < (ulonglong)param_1[3]) {
          param_1 = (undefined8 *)*param_1;
        }
        puVar7 = param_2;
        if (0xf < (ulonglong)param_2[3]) {
          puVar7 = (undefined8 *)*param_2;
        }
        uVar4 = ZSTD_decompress(puVar7,param_2[2],param_1,*puVar6);
        if (0xffffffffffffff88 < uVar4) {
          return uVar4;
        }
        if ((ulonglong)param_2[2] < uVar4) {
          std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                    (param_2,uVar4 - param_2[2],0);
          return uVar4;
        }
        param_2[2] = uVar4;
        if (0xf < (ulonglong)param_2[3]) {
          param_2 = (undefined8 *)*param_2;
        }
        *(undefined1 *)((longlong)param_2 + uVar4) = 0;
        return uVar4;
      }
      uVar1 = param_1[2] * 2;
      if (uVar4 < uVar1) {
        std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                  (param_2,uVar1 - uVar4,0);
      }
      else {
        param_2[2] = uVar1;
        puVar6 = param_2;
        if (0xf < (ulonglong)param_2[3]) {
          puVar6 = (undefined8 *)*param_2;
        }
        *(undefined1 *)((longlong)puVar6 + uVar1) = 0;
      }
      uVar2 = DAT_1423f8920;
      local_88 = ram0x0001423f8910;
      pcStack_80 = _UNK_1423f8918;
      lVar5 = malloc(0x271f8);
      lVar3 = 0;
      if (lVar5 != 0) {
        *(undefined8 *)(lVar5 + 0x7120) = local_88;
        *(code **)(lVar5 + 0x7128) = pcStack_80;
        *(undefined8 *)(lVar5 + 0x7130) = uVar2;
        ZSTD_initDCtx_internal(lVar5);
        lVar3 = lVar5;
      }
      *(undefined4 *)(lVar3 + 0x7174) = 0;
      *(undefined4 *)(lVar3 + 0x71d4) = 0;
      puVar6 = *(undefined8 **)(lVar3 + 0x7158);
      if (puVar6 != (undefined8 *)0x0) {
        local_88 = puVar6[0xd09];
        pcStack_80 = (code *)puVar6[0xd0a];
        local_50 = puVar6[0xd0b];
        local_28 = local_50;
        if ((void *)*puVar6 != (void *)0x0) {
          if ((code *)puVar6[0xd0a] == (code *)0x0) {
            free((void *)*puVar6);
          }
          else {
            (*(code *)puVar6[0xd0a])(local_50);
          }
        }
        if (pcStack_80 == (code *)0x0) {
          free(puVar6);
        }
        else {
          (*pcStack_80)(local_50);
        }
      }
      *(undefined8 *)(lVar3 + 0x7158) = 0;
      *(undefined8 *)(lVar3 + 0x7160) = 0;
      *(undefined4 *)(lVar3 + 0x7170) = 0;
      local_78 = (undefined8 *)0x0;
      uStack_70 = 0;
      local_68 = 0;
      local_98 = 0;
      local_a8 = param_1;
      if (0xf < (ulonglong)param_1[3]) {
        local_a8 = (undefined8 *)*param_1;
      }
      local_a0 = param_1[2];
      if (local_a0 != 0) {
        do {
          local_78 = param_2;
          if (0xf < (ulonglong)param_2[3]) {
            local_78 = (undefined8 *)*param_2;
          }
          uStack_70 = param_2[2];
          uVar4 = ZSTD_decompressStream(lVar3,&local_78,&local_a8);
          if (0xffffffffffffff88 < uVar4) {
            ZSTD_freeDCtx(lVar3);
            return uVar4;
          }
          if (local_a0 <= local_98) break;
          uVar4 = param_2[2];
          uVar1 = uVar4 * 2;
          if (uVar4 < uVar1) {
            std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                      (param_2,uVar4,0);
          }
          else {
            param_2[2] = uVar1;
            puVar6 = param_2;
            if (0xf < (ulonglong)param_2[3]) {
              puVar6 = (undefined8 *)*param_2;
            }
            *(undefined1 *)((longlong)puVar6 + uVar1) = 0;
          }
        } while (local_98 < local_a0);
      }
      if ((ulonglong)param_2[2] < local_68) {
        std__basic_string<char,std::char_traits<char>,std::allocator<char>_>__append
                  (param_2,local_68 - param_2[2],0);
      }
      else {
        param_2[2] = local_68;
        puVar6 = param_2;
        if (0xf < (ulonglong)param_2[3]) {
          puVar6 = (undefined8 *)*param_2;
        }
        *(undefined1 *)((longlong)puVar6 + local_68) = 0;
      }
      uVar4 = param_2[2];
      ZSTD_freeDCtx(lVar3);
      return uVar4;
    }
  }
  RuntimeError__RuntimeError(local_60,"Decompression failed. Input stream is corrupted or invalid.")
  ;
                    /* WARNING: Subroutine does not return */
  _CxxThrowException(local_60,(ThrowInfo *)&DAT_1427b2490);
}

