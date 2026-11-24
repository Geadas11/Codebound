// DrawEngine::prepare @ 1406b25d0 (function DrawEngine::prepare)

void DrawEngine__prepare(longlong param_1,longlong param_2)

{
  double dVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined8 uVar5;
  undefined1 uVar6;
  longlong lVar7;
  undefined8 uVar8;
  double *pdVar9;
  undefined8 *puVar10;
  undefined8 uVar11;
  longlong lVar12;
  float fVar13;
  float fVar14;
  undefined1 local_res8 [8];
  undefined8 local_288;
  undefined8 uStack_280;
  undefined4 local_278;
  undefined1 local_268 [32];
  undefined1 local_248 [456];
  undefined8 local_80;
  undefined8 local_78;
  uint local_20;
  
  RenderParameters__RenderParameters(local_248);
  if (*(char *)(param_1 + 0x51) == '\0') {
    local_20 = local_20 & 0xfffffff7;
  }
  else {
    local_20 = local_20 | 8;
  }
  lVar7 = RenderParameters__getSurface(param_2);
  if (lVar7 == 0) {
    lVar7 = 0;
LAB_1406b26a1:
    *(undefined1 *)(param_1 + 0x53) = 0;
    if (lVar7 != 0) goto LAB_1406b26aa;
  }
  else {
    lVar7 = Surface__getSurfaceRenderParameters(lVar7);
    if ((lVar7 == 0) || (*(char *)(lVar7 + 0x1c8) == '\0')) goto LAB_1406b26a1;
    lVar12 = 0xea;
    if (*(char *)(*(longlong *)(DAT_142d34fa0 + 0x220) + 0xeb) == '\0') {
      lVar12 = 0x11c0;
    }
    if ((*(char *)(lVar12 + *(longlong *)(DAT_142d34fa0 + 0x220)) == '\0') ||
       ((((*(longlong *)(DAT_142d34fa0 + 0x140) != 0 &&
          (*(longlong *)(*(longlong *)(DAT_142d34fa0 + 0x140) + 0x20) != 0)) ||
         (lVar12 = RenderParameters__getSurface(param_2), *(char *)(lVar12 + 0x819) == '\0')) ||
        ((*(uint *)(param_2 + 0x228) & 0x1000000) != 0)))) goto LAB_1406b26a1;
    *(undefined1 *)(param_1 + 0x53) = 1;
LAB_1406b26aa:
    if (((*(char *)(lVar7 + 0x348) != '\0') &&
        (*(char *)(*(longlong *)(DAT_142d34fa0 + 0x220) + 0x1280) != '\0')) &&
       ((*(uint *)(param_2 + 0x228) & 0x2000000) == 0)) {
      uVar6 = 1;
      goto LAB_1406b26dc;
    }
  }
  uVar6 = 0;
LAB_1406b26dc:
  *(undefined1 *)(param_1 + 0x54) = uVar6;
  *(byte *)(param_1 + 0x55) = ~(byte)((uint)*(undefined4 *)(param_2 + 0x228) >> 0x19) & 1;
  lVar7 = RenderParameters__getSurface(param_2);
  uVar2 = *(undefined4 *)(lVar7 + 0x2e04);
  uVar3 = *(undefined4 *)(lVar7 + 0x2e08);
  uVar4 = *(undefined4 *)(lVar7 + 0x2e0c);
  *(undefined4 *)(param_1 + 0xd0) = *(undefined4 *)(lVar7 + 0x2e00);
  *(undefined4 *)(param_1 + 0xd4) = uVar2;
  *(undefined4 *)(param_1 + 0xd8) = uVar3;
  *(undefined4 *)(param_1 + 0xdc) = uVar4;
  lVar7 = RenderParameters__getSurface(param_2);
  dVar1 = *(double *)(lVar7 + 0x840);
  *(float *)(param_1 + 0xe0) = (float)*(double *)(lVar7 + 0x838);
  *(float *)(param_1 + 0xe4) = (float)dVar1;
  ColorLookupEngine__prepare(param_1 + 400,local_248);
  if (((*(uint *)(param_2 + 0x228) & 0x10000) == 0) && (*(char *)(param_1 + 0x52) == '\0')) {
    uVar6 = 1;
  }
  else {
    uVar6 = 0;
  }
  *(undefined1 *)(param_1 + 0x50) = uVar6;
  lVar7 = RenderParameters__getSurface(param_2);
  uVar8 = 0;
  if (lVar7 != 0) {
    uVar8 = Surface__getSurfaceRenderParameters(lVar7);
  }
  *(undefined8 *)(param_1 + 0x208) = uVar8;
  lVar7 = RenderParameters__getSurface(param_2);
  fVar13 = (float)DayTime__getDarkness(*(undefined8 *)(lVar7 + 0x830),DAT_1424027a8);
  fVar14 = DAT_1424022c0;
  fVar13 = DAT_1424022c0 - fVar13;
  *(float *)(param_1 + 0xe8) = fVar13;
  if ((*(longlong *)(param_2 + 0x30) != 0) &&
     (lVar7 = *(longlong *)(*(longlong *)(param_2 + 0x30) + 0x50), lVar7 != 0)) {
    if (*(longlong *)(lVar7 + 0x48) != *(longlong *)(lVar7 + 0x50)) {
      pdVar9 = (double *)
               BezierInterpolation<BezierScalar>__interpolate
                         (lVar7 + 0x30,local_res8,*(undefined8 *)(param_2 + 0x50));
      fVar14 = (float)*pdVar9;
    }
    fVar13 = fVar14 * fVar13;
    *(float *)(param_1 + 0xe8) = fVar13;
  }
  DAT_1428b95c0 = fVar13;
  DrawEngine__prepareLightningAttractorRanges(param_1);
  *(undefined8 *)(param_1 + 0xc0) = local_78;
  *(undefined8 *)(param_1 + 200) = local_80;
  puVar10 = (undefined8 *)RenderParameters__getRenderBoundingBox(param_2,local_268);
  uVar8 = *puVar10;
  uVar5 = puVar10[1];
  uVar2 = *(undefined4 *)(puVar10 + 2);
  uVar11 = RenderParameters__getSurface(param_2);
  local_288 = uVar8;
  uStack_280 = uVar5;
  local_278 = uVar2;
  DrawEngine__prepareThrusterJetFlames(param_1,uVar11,&local_288);
  *(float *)(param_1 + 0xec) = (float)*(double *)(param_2 + 0x1e0);
  *(float *)(param_1 + 0xf0) = (float)*(double *)(param_2 + 0x1e8);
  *(undefined4 *)(param_1 + 0xf4) = *(undefined4 *)(param_2 + 0x1f0);
  puVar10 = (undefined8 *)RenderParameters__getPlatformBackdropParameters(param_2,&local_288);
  *(undefined8 *)(param_1 + 0xf8) = *puVar10;
  *(undefined4 *)(param_1 + 0x100) = *(undefined4 *)(puVar10 + 1);
  *(undefined8 *)(param_1 + 0x168) = 0;
  *(undefined2 *)(param_1 + 0x170) = 0xff;
  *(undefined4 *)(param_1 + 0x174) = 0;
  RenderParameters___RenderParameters(local_248);
  return;
}

