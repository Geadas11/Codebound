// GameRenderer::prepare @ 1406d7380 (function GameRenderer::prepare)

void GameRenderer__prepare(longlong param_1,longlong param_2)

{
  undefined8 *puVar1;
  longlong lVar2;
  undefined8 *puVar3;
  
  lVar2 = 0xec;
  if (*(char *)(*(longlong *)(DAT_142d34fa0 + 0x220) + 0xed) == '\0') {
    lVar2 = 0x1a00;
  }
  DAT_1429b98c4 = *(undefined1 *)(lVar2 + *(longlong *)(DAT_142d34fa0 + 0x220));
  RenderParameters__operator_(param_1);
  GameRenderer__switchToMode
            (param_1,*(undefined1 *)(param_2 + 0x1b8),0,
             CONCAT31((uint3)(*(uint *)(param_2 + 0x228) >> 0x1f),
                      ~(byte)(*(uint *)(param_2 + 0x228) >> 0x17)) & 0xffffff01);
  if (DAT_142d3eb88 == '\0') {
    *(undefined1 *)(param_1 + 0x234) = 0;
    *(undefined4 *)(param_1 + 0x230) = 0;
    if (((((*(uint *)(param_1 + 0x228) & 2) != 0) || ((*(uint *)(param_1 + 0x228) & 4) != 0)) &&
        (lVar2 = *(longlong *)(param_1 + 0x6c8), lVar2 != 0)) &&
       (((*(longlong *)(lVar2 + 0x78) != 0 && ('\0' < *(char *)(lVar2 + 0xd))) &&
        (lVar2 = *(longlong *)(lVar2 + 8 + (longlong)*(char *)(lVar2 + 0xd) * 8), lVar2 != 0)))) {
      *(undefined1 *)(lVar2 + 0x480b) = 1;
      *(undefined1 *)(lVar2 + 0x4790) = 0;
    }
    DrawEngine__prepare(param_1 + 0x238,param_1);
    puVar3 = (undefined8 *)(*(longlong *)(param_1 + 0x6b8) + 0x10);
    puVar1 = puVar3 + *(longlong *)(*(longlong *)(param_1 + 0x6b8) + 0x78);
    for (; puVar3 != puVar1; puVar3 = puVar3 + 1) {
      (**(code **)(*(longlong *)*puVar3 + 0x28))((longlong *)*puVar3,param_1);
    }
  }
  return;
}

