using System.IO;
using System.Linq;
using Codebound.Building;
using Codebound.Core;
using Codebound.Items;
using Codebound.Machines;
using Codebound.Save;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class SaveTests
    {
        private static GameState RoundTrip(GameState s)
        {
            using var ms = new MemoryStream();
            SaveSystem.Save(s, ms);
            ms.Position = 0;
            return SaveSystem.Load(ms);
        }

        private static PlacedStructure Place(GameState s, string id, GridPos at, Direction f = Direction.North)
        {
            Assert.AreEqual(PlacementStatus.Ok, s.Grid.TryPlace(StructureCatalog.Find(id), at, f, null, out var p), id);
            return p;
        }

        [Test]
        public void NewWorld_HasEmergencyBatteryNearSpawn()
        {
            var s = GameState.CreateNew(2026);
            var battery = s.Grid.Structures.Single(x => x.Definition.Id == StructureCatalog.EmergencyBattery);
            Assert.LessOrEqual(battery.Origin.ManhattanDistance(s.World.SpawnPoint), 16);
            Assert.IsNotNull(s.Machines.GetBattery(battery));
        }

        [Test]
        public void RoundTrip_PreservesPlayerWorldStructuresAndMachines()
        {
            var s = GameState.CreateNew(77);
            var spawn = s.World.SpawnPoint;
            s.Motor.Teleport(new Vec2(spawn.X + 0.25f, spawn.Y + 1.75f));
            s.PlayerInventory.Add(ItemCatalog.Scrap, 17);
            s.PlayerInventory.Add(ItemCatalog.Wood, 3);
            for (int i = 0; i < 20; i++) s.Clock.Advance(0.05); // 20 ticks

            // Harvest a node partially and fully deplete another.
            var nodes = s.World.Nodes.ToList();
            s.World.Harvest(nodes[0].Cell, out _, 1);
            int depletedCount = nodes[1].Remaining;
            var depletedCell = nodes[1].Cell;
            s.World.Harvest(depletedCell, out _, depletedCount);

            // A small powered setup with a drill on ore.
            var ore = s.World.Nodes.First(n => n.ItemId == ItemCatalog.IronOre);
            var drillOrigin = FindDrillOrigin(s, ore.Cell);
            var drill = s.Machines.Get<Drill>(Place(s, StructureCatalog.Drill, drillOrigin, Direction.East));
            var computerCell = FindFree(s, drillOrigin, 3);
            var computer = s.Machines.Get<Computer>(Place(s, StructureCatalog.Computer, computerCell));
            computer.LoadProgram(ProgrammingLanguage.Python, "while True:\n    drill.mine()");
            computer.MarkRunning();
            Assert.AreEqual(LinkResult.Ok, computer.Link(drill));
            drill.RestoreState(true, 0.4, 5);
            var crate = Place(s, StructureCatalog.Crate, FindFree(s, drillOrigin, 6));
            crate.Inventory.Add(ItemCatalog.Coal, 9);
            var battery = s.Machines.GetBattery(s.Grid.Structures.Single(x => x.Definition.Id == StructureCatalog.EmergencyBattery));
            battery.RestoreCharge(1234.5);

            var l = RoundTrip(s);

            Assert.AreEqual(s.World.Seed, l.World.Seed);
            Assert.AreEqual(20, l.Clock.CurrentTick);
            Assert.AreEqual(s.Motor.Position, l.Motor.Position);
            Assert.AreEqual(17, l.PlayerInventory.CountOf(ItemCatalog.Scrap));
            Assert.AreEqual(3, l.PlayerInventory.CountOf(ItemCatalog.Wood));
            Assert.AreEqual(s.World.Nodes.Count, l.World.Nodes.Count);
            Assert.IsFalse(l.World.TryGetNode(depletedCell, out _), "depleted node stays gone");
            Assert.IsTrue(l.World.TryGetNode(nodes[0].Cell, out var partial));
            Assert.AreEqual(nodes[0].Remaining, partial.Remaining);
            Assert.AreEqual(nodes[0].InitialAmount, partial.InitialAmount);

            Assert.AreEqual(s.Grid.Structures.Count, l.Grid.Structures.Count);
            var lDrill = l.Machines.Get<Drill>(l.Grid.GetAt(drillOrigin));
            Assert.IsNotNull(lDrill);
            Assert.AreEqual(Direction.East, lDrill.Structure.Facing);
            Assert.IsTrue(lDrill.CyclePending);
            Assert.AreEqual(0.4, lDrill.Progress, 1e-9);
            Assert.AreEqual(5, lDrill.TotalMined);

            var lComputer = l.Machines.Get<Computer>(l.Grid.GetAt(computerCell));
            Assert.AreEqual(ProgrammingLanguage.Python, lComputer.Language);
            Assert.AreEqual(computer.Source, lComputer.Source);
            Assert.AreEqual(ProgramStatus.Loaded, lComputer.ProgramStatus, "runtime must restart programs after load");
            Assert.AreSame(lComputer, lDrill.ControlledBy);

            Assert.AreEqual(9, l.Grid.GetAt(crate.Origin).Inventory.CountOf(ItemCatalog.Coal));
            var lBattery = l.Machines.GetBattery(l.Grid.Structures.Single(x => x.Definition.Id == StructureCatalog.EmergencyBattery));
            Assert.AreEqual(1234.5, lBattery.StoredJoules, 1e-9);
        }

        [Test]
        public void RoundTrip_IsStable()
        {
            var s = GameState.CreateNew(5);
            s.PlayerInventory.Add(ItemCatalog.Stone, 4);
            byte[] a, b;
            using (var ms = new MemoryStream()) { SaveSystem.Save(s, ms); a = ms.ToArray(); }
            using (var ms = new MemoryStream(a)) { var l = SaveSystem.Load(ms); using var ms2 = new MemoryStream(); SaveSystem.Save(l, ms2); b = ms2.ToArray(); }
            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void Load_RejectsGarbageAndTruncatedFiles()
        {
            Assert.Throws<SaveFormatException>(() => SaveSystem.Load(new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })));
            byte[] data;
            using (var ms = new MemoryStream()) { SaveSystem.Save(GameState.CreateNew(3), ms); data = ms.ToArray(); }
            var truncated = data.Take(data.Length / 2).ToArray();
            Assert.Throws<SaveFormatException>(() => SaveSystem.Load(new MemoryStream(truncated)));
        }

        private static GridPos FindDrillOrigin(GameState s, GridPos ore)
        {
            var def = StructureCatalog.Find(StructureCatalog.Drill);
            for (int dy = -1; dy <= 0; dy++)
            for (int dx = -1; dx <= 0; dx++)
            {
                var o = new GridPos(ore.X + dx, ore.Y + dy);
                if (s.Grid.Validate(def, o, Direction.East, null).IsValid) return o;
            }
            Assert.Fail("no drill spot");
            return default;
        }

        private static GridPos FindFree(GameState s, GridPos near, int minDist)
        {
            for (int r = minDist; r <= 7; r++)
            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                var c = new GridPos(near.X + dx, near.Y + dy);
                if (s.World.IsBuildable(c) && !s.Grid.IsOccupied(c)) return c;
            }
            Assert.Fail("no free cell");
            return default;
        }
    }
}
