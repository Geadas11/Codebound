// ProgrammableSpeaker::save @ 1404ede80 (function ProgrammableSpeaker::save)

void ProgrammableSpeaker__save
               (longlong param_1,longlong param_2,undefined8 param_3,undefined8 param_4)

{
  longlong lVar1;
  undefined1 local_res8 [8];
  
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  (**(code **)(**(longlong **)(param_1 + 0xd8) + 0x50))(*(longlong **)(param_1 + 0xd8),param_2);
  ProgrammableSpeakerParameters__save(param_1 + 0xe0,param_2);
  ProgrammableSpeakerAlertParameters__save(param_1 + 0xf0,param_2);
  local_res8[0] = *(longlong *)(param_1 + 0x140) != 0;
  (**(code **)(**(longlong **)(param_2 + 0x50) + 8))(*(longlong **)(param_2 + 0x50),local_res8,1);
  *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
  lVar1 = *(longlong *)(param_1 + 0x140);
  if (lVar1 != 0) {
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e010c,1,param_4,
                 "programmable-speaker-control-behavior",param_2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"programmable-speaker-control-behavior",0x25);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x25;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
    CircuitConnector__save(lVar1 + 0x20,param_2);
    CircuitCondition__save(lVar1 + 0x168,param_2);
    ProgrammableSpeakerCircuitParameters__save
              (lVar1 + 0x184,param_2,*(undefined8 *)(*(longlong *)(lVar1 + 8) + 0x48));
    if (*(char *)(param_2 + 8) != '\0') {
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_14230f710,2);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 2;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),"programmable-speaker-control-behavior",0x25);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 0x25;
      (**(code **)(**(longlong **)(param_2 + 0x50) + 8))
                (*(longlong **)(param_2 + 0x50),&DAT_1421e0108,1);
      *(longlong *)(param_2 + 0x10) = *(longlong *)(param_2 + 0x10) + 1;
    }
  }
  return;
}

