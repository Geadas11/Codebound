// LaneSplitter::save @ 1404b01f0 (function LaneSplitter::save)

void LaneSplitter__save(longlong param_1,longlong param_2)

{
  TransportBeltConnectable__save();
  MapSerialiser__operator<<<SplitterLogic>(param_2,param_1 + 400);
  if (*(char *)(param_2 + 0x149) == '\0') {
    MapSerialiser__operator<<<SplitterStructureAnimation>(param_2,param_1 + 0x178);
    TransportLineConnector__save(param_1 + 0x128,param_2);
    TransportLineConnector__save(param_1 + 0x150,param_2);
  }
  return;
}

