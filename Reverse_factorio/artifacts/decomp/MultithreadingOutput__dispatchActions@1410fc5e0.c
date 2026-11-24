// MultithreadingOutput::dispatchActions @ 1410fc5e0 (function MultithreadingOutput::dispatchActions)

void MultithreadingOutput__dispatchActions(longlong *param_1)

{
  longlong lVar1;
  longlong lVar2;
  longlong lVar3;
  ulonglong uVar4;
  longlong lVar5;
  longlong lVar6;
  longlong lVar7;
  longlong lVar8;
  longlong lVar9;
  longlong lVar10;
  longlong *plVar11;
  longlong lVar12;
  longlong *plVar13;
  longlong *plVar14;
  longlong lVar15;
  ulonglong uVar16;
  longlong lVar17;
  undefined4 uVar18;
  longlong local_a8;
  longlong local_a0;
  longlong local_98;
  longlong local_90;
  longlong *local_88;
  longlong *plStack_80;
  longlong *local_78;
  undefined1 local_70;
  ulonglong local_48;
  
  uVar18 = 0;
  local_88 = (longlong *)0x0;
  plStack_80 = (longlong *)0x0;
  local_78 = (longlong *)0x0;
  local_70 = 0;
  plVar11 = (longlong *)*param_1;
  plVar13 = plVar11 + param_1[1] * 7;
  plVar14 = local_88;
  for (; local_88 = plVar14, plVar11 != plVar13; plVar11 = plVar11 + 7) {
    lVar1 = plVar11[4];
    lVar2 = plVar11[5];
    if (lVar1 != lVar2) {
      if (plStack_80 == local_78) {
        std__vector<_MultithreadingOutput::dispatchActions_::_2_::Head,std::allocator<_MultithreadingOutput::dispatchActions_::_2_::Head>_>___Emplace_reallocate<std::vector<std::function<void___cdecl(void)>,std::allocator<std::function<void___cdecl(void)>_>_>_&,std::vector<MultithreadingOutput::UpdateOrderAndActionCountPair,std::allocator<MultithreadingOutput::UpdateOrderAndActionCountPair>_>_&>
                  (&local_88,plStack_80,plVar11,plVar11 + 4);
      }
      else {
        lVar3 = *plVar11;
        lVar10 = plVar11[1];
        *plStack_80 = lVar3;
        plStack_80[1] = lVar10 - lVar3 >> 6;
        plStack_80[2] = lVar1;
        plStack_80[3] = lVar2 - lVar1 >> 4;
        plStack_80[4] = *(longlong *)plStack_80[2];
        plStack_80 = plStack_80 + 5;
      }
      uVar18 = std__push_heap<std::_Vector_iterator<std::_Vector_val<std::_Simple_types<_MultithreadingOutput::dispatchActions_::_2_::Head>_>_>,std::less<_MultithreadingOutput::dispatchActions_::_2_::Head>_>
                         (local_88,plStack_80);
    }
    plVar14 = local_88;
  }
  if (plVar14 != plStack_80) {
    do {
      lVar1 = *plVar14;
      lVar2 = plVar14[1];
      lVar3 = plVar14[2];
      uVar4 = plVar14[3];
      local_48 = plVar14[4];
      uVar18 = (undefined4)local_48;
      if (1 < ((longlong)plStack_80 - (longlong)plVar14 >> 3) * -0x3333333333333333) {
        plVar11 = plStack_80 + -5;
        lVar12 = *plVar11;
        lVar5 = plStack_80[-4];
        lVar6 = plStack_80[-3];
        lVar7 = plStack_80[-2];
        local_48 = plStack_80[-1];
        *plVar11 = lVar1;
        plStack_80[-4] = lVar2;
        lVar10 = plVar14[3];
        plStack_80[-3] = plVar14[2];
        plStack_80[-2] = lVar10;
        uVar18 = (undefined4)plVar14[4];
        plStack_80[-1] = plVar14[4];
        lVar15 = (longlong)plVar11 - (longlong)plVar14 >> 3;
        uVar16 = lVar15 * -0x3333333333333333;
        lVar10 = 0;
        lVar17 = (longlong)(uVar16 - 1) >> 1;
        lVar9 = lVar10;
        if (0 < lVar17) {
          do {
            lVar10 = lVar9 * 2 +
                     (2 - (ulonglong)
                          ((ulonglong)plVar14[lVar9 * 10 + 9] < (ulonglong)plVar14[lVar9 * 10 + 0xe]
                          ));
            lVar8 = (plVar14 + lVar10 * 5)[1];
            plVar14[lVar9 * 5] = plVar14[lVar10 * 5];
            (plVar14 + lVar9 * 5)[1] = lVar8;
            lVar8 = (plVar14 + lVar10 * 5 + 2)[1];
            plVar14[lVar9 * 5 + 2] = plVar14[lVar10 * 5 + 2];
            (plVar14 + lVar9 * 5 + 2)[1] = lVar8;
            uVar18 = (undefined4)plVar14[lVar10 * 5 + 4];
            plVar14[lVar9 * 5 + 4] = plVar14[lVar10 * 5 + 4];
            lVar9 = lVar10;
          } while (lVar10 < lVar17);
        }
        if ((lVar10 == lVar17) && ((uVar16 & 1) == 0)) {
          lVar9 = (plVar14 + lVar15 + -5)[1];
          plVar14[lVar10 * 5] = plVar14[lVar15 + -5];
          (plVar14 + lVar10 * 5)[1] = lVar9;
          lVar9 = (plVar14 + lVar15 + -3)[1];
          plVar14[lVar10 * 5 + 2] = plVar14[lVar15 + -3];
          (plVar14 + lVar10 * 5 + 2)[1] = lVar9;
          uVar18 = (undefined4)plVar14[lVar15 + -1];
          plVar14[lVar10 * 5 + 4] = plVar14[lVar15 + -1];
          lVar10 = uVar16 - 1;
        }
        while (0 < lVar10) {
          lVar9 = lVar10 + -1 >> 1;
          plVar11 = plVar14 + lVar9 * 5;
          if ((ulonglong)plVar11[4] <= local_48) break;
          lVar15 = plVar11[1];
          plVar14[lVar10 * 5] = *plVar11;
          (plVar14 + lVar10 * 5)[1] = lVar15;
          lVar15 = plVar11[3];
          plVar14[lVar10 * 5 + 2] = plVar11[2];
          (plVar14 + lVar10 * 5 + 2)[1] = lVar15;
          uVar18 = (undefined4)plVar11[4];
          plVar14[lVar10 * 5 + 4] = plVar11[4];
          lVar10 = lVar9;
        }
        plVar14[lVar10 * 5] = lVar12;
        (plVar14 + lVar10 * 5)[1] = lVar5;
        plVar14[lVar10 * 5 + 2] = lVar6;
        (plVar14 + lVar10 * 5 + 2)[1] = lVar7;
        plVar14[lVar10 * 5 + 4] = local_48;
        plVar14 = local_88;
      }
      plStack_80 = plStack_80 + -5;
      lVar10 = *(longlong *)(lVar3 + 8);
      for (lVar12 = lVar1; lVar12 != lVar10 * 0x40 + lVar1; lVar12 = lVar12 + 0x40) {
        if (*(longlong **)(lVar12 + 0x38) == (longlong *)0x0) {
                    /* WARNING: Subroutine does not return */
          std___Xbad_function_call();
        }
        uVar18 = (**(code **)(**(longlong **)(lVar12 + 0x38) + 0x10))();
        plVar14 = local_88;
      }
      if (1 < uVar4) {
        local_a8 = lVar3 + 0x10;
        local_a0 = uVar4 - 1;
        local_98 = lVar10 * 0x40 + lVar1;
        local_90 = lVar2 - lVar10;
        if (plStack_80 == local_78) {
          std__vector<_MultithreadingOutput::dispatchActions_::_2_::Head,std::allocator<_MultithreadingOutput::dispatchActions_::_2_::Head>_>___Emplace_reallocate<std::span<std::function<void___cdecl(void)>,_1>,std::span<MultithreadingOutput::UpdateOrderAndActionCountPair,_1>_>
                    (&local_88,plStack_80,&local_98,&local_a8);
        }
        else {
          *plStack_80 = local_98;
          plStack_80[1] = local_90;
          plStack_80[2] = local_a8;
          plStack_80[3] = local_a0;
          plStack_80[4] = *(longlong *)plStack_80[2];
          plStack_80 = plStack_80 + 5;
        }
        uVar18 = std__push_heap<std::_Vector_iterator<std::_Vector_val<std::_Simple_types<_MultithreadingOutput::dispatchActions_::_2_::Head>_>_>,std::less<_MultithreadingOutput::dispatchActions_::_2_::Head>_>
                           (local_88,plStack_80);
        plVar14 = local_88;
      }
    } while (plVar14 != plStack_80);
  }
  if (plVar14 != (longlong *)0x0) {
    std__allocator<_RailBuildPlan::removeUnnecesaryRailSupports_::_2_::ExtendedSupportData>__deallocate
              (uVar18,plVar14,((longlong)local_78 - (longlong)plVar14 >> 3) * -0x3333333333333333);
  }
  return;
}

