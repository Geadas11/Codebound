using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class LogisticsTests
    {
        static void Run(GameState g, float s) { for (float t = 0; t < s; t += 0.05f) g.Factory.Tick(0.05f); }

        /// <summary>A free 5×5 area: returns its centre.</summary>
        static (GameState g, Cell c) Area()
        {
            var g = new GameState(9);
            var s = g.World.Generator.FindSpawn();
            var wall = BuildingDatabase.Get("wall");
            for (int r = 3; r < 120; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    bool ok = true;
                    for (int dx = -2; dx <= 2 && ok; dx++) for (int dy = -2; dy <= 2 && ok; dy++) ok = g.CanPlace(wall, new Cell(c.X + dx, c.Y + dy), Rotation.R0);
                    if (ok) return (g, c);
                }
            Assert.Fail("no room"); return default;
        }

        static StorageState Chest(GameState g, Cell c) => (StorageState)g.Grid.Place(BuildingDatabase.Get("chest"), c, Rotation.R0).State;

        [Test]
        public void Sorter_SendsTheChosenItemStraight_AndTheRestRight()
        {
            var (g, c) = Area();
            var sorter = g.Grid.Place(BuildingDatabase.Get("sorter"), c, Rotation.R90);   // facing right: front = x+1, right = y-1
            var straight = Chest(g, c.Move(Rotation.R90));
            var side = Chest(g, c.Move(Rotation.R180));
            var st = (ConveyorState)sorter.State;
            st.Filter = ItemDatabase.IronPlate;
            foreach (var item in new[] { ItemDatabase.IronPlate, ItemDatabase.Stone, ItemDatabase.IronPlate, ItemDatabase.Coal })
            {
                Assert.IsTrue(st.TryInsert(item, 1));
                Run(g, 1f);
            }
            Assert.AreEqual(2, straight.Items.Count(ItemDatabase.IronPlate));
            Assert.AreEqual(0, straight.Items.Count(ItemDatabase.Stone));
            Assert.AreEqual(1, side.Items.Count(ItemDatabase.Stone));
            Assert.AreEqual(1, side.Items.Count(ItemDatabase.Coal));
        }

        [Test]
        public void Splitter_Alternates_AndUsesTheOtherSideWhenOneIsBlocked()
        {
            var (g, c) = Area();
            var sp = (ConveyorState)g.Grid.Place(BuildingDatabase.Get("splitter"), c, Rotation.R0).State;   // up: front = y+1, right = x+1
            var front = Chest(g, c.Move(Rotation.R0));
            var right = Chest(g, c.Move(Rotation.R90));
            for (int i = 0; i < 6; i++) { Assert.IsTrue(sp.TryInsert(ItemDatabase.Stone, 1)); Run(g, 1f); }
            Assert.AreEqual(3, front.Items.Count(ItemDatabase.Stone));
            Assert.AreEqual(3, right.Items.Count(ItemDatabase.Stone));

            g.Grid.Remove(g.Grid.At(c.Move(Rotation.R90)));   // right side gone: everything goes forward
            for (int i = 0; i < 2; i++) { Assert.IsTrue(sp.TryInsert(ItemDatabase.Stone, 1)); Run(g, 1f); }
            Assert.AreEqual(5, front.Items.Count(ItemDatabase.Stone));
        }

        [Test]
        public void LongInserter_ReachesTwoCells()
        {
            var (g, c) = Area();
            g.Grid.Place(BuildingDatabase.Get("long_inserter"), c, Rotation.R0);
            var from = Chest(g, c.Move(Rotation.R0, -2));
            var to = Chest(g, c.Move(Rotation.R0, 2));
            from.Items.Add(ItemDatabase.Coal, 3);
            Run(g, 5f);
            Assert.AreEqual(0, from.Items.Count(ItemDatabase.Coal));
            Assert.AreEqual(3, to.Items.Count(ItemDatabase.Coal));
            Assert.IsFalse(g.Research.IsUnlocked(BuildingDatabase.Get("long_inserter")), "needs Robotics");
        }

        [Test]
        public void Activity_ShowsWorkingBlockedAndNoPower()
        {
            var (g, c) = Area();
            var pc = g.Grid.Place(BuildingDatabase.Get("computer"), c, Rotation.R0);
            ((ComputerState)pc.State).Running = true;
            g.Factory.Tick(0.05f);
            Assert.AreEqual(MachineActivity.NoPower, g.Factory.Activity(pc));
            var belt = g.Grid.Place(BuildingDatabase.Get("belt"), c.Move(Rotation.R90, 2), Rotation.R0);
            Assert.AreEqual(MachineActivity.Idle, g.Factory.Activity(belt));
            ((ConveyorState)belt.State).TryInsert(ItemDatabase.Stone, 1);
            g.Factory.Tick(0.05f);
            Assert.AreEqual(MachineActivity.Working, g.Factory.Activity(belt));
            Run(g, 1f);   // nothing in front: the item cannot leave
            Assert.AreEqual(MachineActivity.Blocked, g.Factory.Activity(belt));
            Assert.AreEqual(MachineActivity.None, g.Factory.Activity(g.Grid.Place(BuildingDatabase.Get("chest"), c.Move(Rotation.R270, 2), Rotation.R0)));
        }
    }
}
