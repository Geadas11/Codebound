// crc32_16bytes @ 141dc7dd0 (function crc32_16bytes)

uint crc32_16bytes(uint *param_1,ulonglong param_2,uint param_3)

{
  uint uVar1;
  uint uVar2;
  uint uVar3;
  uint uVar4;
  uint uVar5;
  uint uVar6;
  uint uVar7;
  uint uVar8;
  uint uVar9;
  uint *puVar10;
  ulonglong local_res20;
  
  param_3 = ~param_3;
  if (0x3f < param_2) {
    local_res20 = param_2 >> 6;
    param_2 = param_2 + local_res20 * -0x40;
    do {
      uVar1 = param_1[1];
      uVar2 = param_1[2];
      uVar3 = param_1[3];
      uVar4 = param_1[5];
      uVar5 = param_1[6];
      uVar6 = param_1[7];
      param_3 = *param_1 ^ param_3;
      uVar9 = *(uint *)(&DAT_1423f7c50 + ((ulonglong)(param_3 >> 0x10) & 0xff) * 4) ^
              *(uint *)(&DAT_1423f8050 + ((ulonglong)(param_3 >> 8) & 0xff) * 4) ^
              *(uint *)(&DAT_1423f6c50 + (ulonglong)(byte)(uVar1 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f7050 + (ulonglong)(byte)(uVar1 >> 8) * 4) ^
              *(uint *)(&DAT_1423f5c50 + (ulonglong)(byte)(uVar2 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f6050 + (ulonglong)(byte)(uVar2 >> 8) * 4) ^
              *(uint *)(&DAT_1423f4c50 + (ulonglong)(byte)(uVar3 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f5050 + (ulonglong)(byte)(uVar3 >> 8) * 4) ^
              *(uint *)(&DAT_1423f7850 + (ulonglong)(param_3 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f6850 + (ulonglong)(uVar1 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f5850 + (ulonglong)(uVar2 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f4850 + (ulonglong)(uVar3 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f8450 + ((ulonglong)param_3 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f7450 + (ulonglong)(uVar1 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f6450 + (ulonglong)(uVar2 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f5450 + (ulonglong)(uVar3 & 0xff) * 4) ^ param_1[4];
      uVar1 = param_1[9];
      uVar2 = param_1[10];
      uVar3 = param_1[0xb];
      puVar10 = param_1 + 0xc;
      uVar7 = param_1[0xe];
      uVar8 = param_1[0xf];
      uVar5 = *(uint *)(&DAT_1423f7c50 + ((ulonglong)(uVar9 >> 0x10) & 0xff) * 4) ^
              *(uint *)(&DAT_1423f8050 + ((ulonglong)(uVar9 >> 8) & 0xff) * 4) ^
              *(uint *)(&DAT_1423f6c50 + (ulonglong)(byte)(uVar4 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f7050 + (ulonglong)(byte)(uVar4 >> 8) * 4) ^
              *(uint *)(&DAT_1423f5c50 + (ulonglong)(byte)(uVar5 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f6050 + (ulonglong)(byte)(uVar5 >> 8) * 4) ^
              *(uint *)(&DAT_1423f4c50 + (ulonglong)(byte)(uVar6 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f5050 + (ulonglong)(byte)(uVar6 >> 8) * 4) ^
              *(uint *)(&DAT_1423f7850 + (ulonglong)(uVar9 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f6850 + (ulonglong)(uVar4 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f5850 + (ulonglong)(uVar5 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f4850 + (ulonglong)(uVar6 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f8450 + ((ulonglong)uVar9 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f7450 + (ulonglong)(uVar4 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f6450 + (ulonglong)(uVar5 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f5450 + (ulonglong)(uVar6 & 0xff) * 4) ^ param_1[8];
      uVar4 = param_1[0xd];
      param_1 = param_1 + 0x10;
      uVar1 = *(uint *)(&DAT_1423f6c50 + (ulonglong)(byte)(uVar1 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f7050 + (ulonglong)(byte)(uVar1 >> 8) * 4) ^
              *(uint *)(&DAT_1423f5c50 + (ulonglong)(byte)(uVar2 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f6050 + (ulonglong)(byte)(uVar2 >> 8) * 4) ^
              *(uint *)(&DAT_1423f4c50 + (ulonglong)(byte)(uVar3 >> 0x10) * 4) ^
              *(uint *)(&DAT_1423f5050 + (ulonglong)(byte)(uVar3 >> 8) * 4) ^
              *(uint *)(&DAT_1423f7c50 + ((ulonglong)(uVar5 >> 0x10) & 0xff) * 4) ^
              *(uint *)(&DAT_1423f8050 + ((ulonglong)(uVar5 >> 8) & 0xff) * 4) ^
              *(uint *)(&DAT_1423f6850 + (ulonglong)(uVar1 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f5850 + (ulonglong)(uVar2 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f4850 + (ulonglong)(uVar3 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f7850 + (ulonglong)(uVar5 >> 0x18) * 4) ^
              *(uint *)(&DAT_1423f7450 + (ulonglong)(uVar1 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f6450 + (ulonglong)(uVar2 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f5450 + (ulonglong)(uVar3 & 0xff) * 4) ^
              *(uint *)(&DAT_1423f8450 + ((ulonglong)uVar5 & 0xff) * 4) ^ *puVar10;
      param_3 = *(uint *)(&DAT_1423f7c50 + ((ulonglong)(uVar1 >> 0x10) & 0xff) * 4) ^
                *(uint *)(&DAT_1423f8050 + ((ulonglong)(uVar1 >> 8) & 0xff) * 4) ^
                *(uint *)(&DAT_1423f6c50 + (ulonglong)(byte)(uVar4 >> 0x10) * 4) ^
                *(uint *)(&DAT_1423f7050 + (ulonglong)(byte)(uVar4 >> 8) * 4) ^
                *(uint *)(&DAT_1423f5c50 + (ulonglong)(byte)(uVar7 >> 0x10) * 4) ^
                *(uint *)(&DAT_1423f6050 + (ulonglong)(byte)(uVar7 >> 8) * 4) ^
                *(uint *)(&DAT_1423f4c50 + (ulonglong)(byte)(uVar8 >> 0x10) * 4) ^
                *(uint *)(&DAT_1423f5050 + (ulonglong)(byte)(uVar8 >> 8) * 4) ^
                *(uint *)(&DAT_1423f7850 + (ulonglong)(uVar1 >> 0x18) * 4) ^
                *(uint *)(&DAT_1423f6850 + (ulonglong)(uVar4 >> 0x18) * 4) ^
                *(uint *)(&DAT_1423f5850 + (ulonglong)(uVar7 >> 0x18) * 4) ^
                *(uint *)(&DAT_1423f4850 + (ulonglong)(uVar8 >> 0x18) * 4) ^
                *(uint *)(&DAT_1423f8450 + ((ulonglong)uVar1 & 0xff) * 4) ^
                *(uint *)(&DAT_1423f7450 + (ulonglong)(uVar4 & 0xff) * 4) ^
                *(uint *)(&DAT_1423f6450 + (ulonglong)(uVar7 & 0xff) * 4) ^
                *(uint *)(&DAT_1423f5450 + (ulonglong)(uVar8 & 0xff) * 4);
      local_res20 = local_res20 - 1;
    } while (local_res20 != 0);
  }
  for (; param_2 != 0; param_2 = param_2 - 1) {
    uVar1 = *param_1;
    param_1 = (uint *)((longlong)param_1 + 1);
    param_3 = param_3 >> 8 ^
              *(uint *)(&DAT_1423f4850 + ((ulonglong)(param_3 & 0xff) ^ (ulonglong)(byte)uVar1) * 4)
    ;
  }
  return ~param_3;
}

