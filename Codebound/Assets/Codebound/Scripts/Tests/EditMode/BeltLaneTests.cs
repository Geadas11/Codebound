using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 2 of the industrial simulation: belts with two lanes, curves, side-loading, and arms with a hand.</summary>
    public class BeltLaneTests
    {
        static void Tick(GameState g) => g.Factory.Tick(0.05f);

        static (GameState g, int x, int y) World(int w, int h, int seed = 31)
        {
            var g = new GameState(seed);
            var s = g.World.Generator.FindSpawn();
            for (int r = 2; r < 120; r++)
                for (int x = -r; x <= r; x++)
                    foreach (int y in new[] { -r, r })
                    {
                        bool ok = true;
                        for (int dx = 0; dx < w && ok; dx++) for (int dy = 0; dy < h && ok; dy++) ok = g.CanPlace(BuildingDatabase.Get("belt"), new Cell(s.X + x + dx, s.Y + y + dy), Rotation.R0);
                        if (ok) return (g, s.X + x, s.Y + y);
                    }
            throw new AssertionException("no room");
        }

        static PlacedBuilding P(GameState g, string id, int x, int y, Rotation r = Rotation.R0) =>
            g.Grid.Place(BuildingDatabase.Get(id), new Cell(x, y), r) ?? throw new AssertionException($"cannot place {id} at {x},{y}");

        static void AssertSpaced(ConveyorState c)
        {
            for (int l = 0; l < ConveyorState.Lanes; l++)
            {
                var lane = c.Lane(l);
                Assert.LessOrEqual(lane.Count, ConveyorState.PerLane);
                for (int i = 1; i < lane.Count; i++)
                    Assert.GreaterOrEqual(lane[i - 1].Pos - lane[i].Pos, ConveyorState.Gap - 1e-4f, "items keep their spacing, front first");
            }
        }

        [Test]
        public void StraightLine_CarriesBothLanes_KeepsSpacing_AndBeatsTheOldOneItemBelts()
        {
            var (g, x, y) = World(8, 1);
            var belts = new ConveyorState[6];
            for (int i = 0; i < 6; i++) belts[i] = (ConveyorState)P(g, "belt", x + i, y, Rotation.R90).State;
            var chest = (StorageState)P(g, "chest", x + 6, y).State;
            for (int t = 0; t < 200; t++)   // 10 s, feeding the first belt as fast as it takes items
            {
                while (belts[0].TryInsert(ItemDatabase.IronOre, 1)) { }
                Tick(g);
                foreach (var b in belts) AssertSpaced(b);
            }
            int delivered = chest.Items.Count(ItemDatabase.IronOre);
            TestContext.WriteLine($"[belts] delivered {delivered} in 10 s");
            Assert.Greater(delivered, 80, "two lanes, 4 items per lane per cell (the single-item belts managed ~18 in 10 s)");
            Assert.LessOrEqual(delivered, (int)(10 * 2 * ConveyorState.BeltSpeed / ConveyorState.Gap) + 1, "never faster than the lanes allow");
        }

        [Test]
        public void Curve_KeepsTheLane()
        {
            var (g, x, y) = World(3, 3);
            var first = (ConveyorState)P(g, "belt", x + 1, y, Rotation.R0).State;        // up
            var corner = (ConveyorState)P(g, "belt", x + 1, y + 1, Rotation.R90).State;  // then right: nothing behind it, so a curve
            var chest = (StorageState)P(g, "chest", x + 2, y + 1).State;
            Assert.IsTrue(first.TryInsert(ItemDatabase.IronOre, 1));
            Assert.AreEqual(1, first.Lane(1).Count, "machines and arms put items on the right lane first");
            bool seen = false;
            for (int t = 0; t < 40; t++)
            {
                Tick(g);
                if (corner.ItemCount > 0) { seen = true; Assert.AreEqual(1, corner.Lane(1).Count, "a curve keeps the item on its lane"); }
            }
            Assert.IsTrue(seen);
            Assert.AreEqual(1, chest.Items.Count(ItemDatabase.IronOre));
        }

        [Test]
        public void SideLoading_FillsTheNearLane_FromEachSide()
        {
            var (g, x, y) = World(5, 3);
            int my = y + 1;
            for (int i = 0; i < 4; i++) P(g, "belt", x + i, my, Rotation.R90);           // main line, to the right
            var last = (ConveyorState)g.Grid.At(new Cell(x + 3, my)).State;
            var chest = (StorageState)P(g, "chest", x + 4, my).State;
            var below = (ConveyorState)P(g, "belt", x + 1, my - 1, Rotation.R0).State;    // feeds up into the main line from its right side
            var above = (ConveyorState)P(g, "belt", x + 2, my + 1, Rotation.R180).State;  // feeds down into it from its left side
            bool sawRight = false, sawLeft = false;
            for (int t = 0; t < 200; t++)
            {
                below.TryInsert(ItemDatabase.IronOre, 1);
                above.TryInsert(ItemDatabase.Coal, 1);
                Tick(g);
                foreach (var it in last.Lane(1)) { Assert.AreEqual(ItemDatabase.IronOre, it.Id, "from the right side: right lane"); sawRight = true; }
                foreach (var it in last.Lane(0)) { Assert.AreEqual(ItemDatabase.Coal, it.Id, "from the left side: left lane"); sawLeft = true; }
                AssertSpaced(last);
            }
            Assert.IsTrue(sawRight && sawLeft);
            Assert.Greater(chest.Items.Count(ItemDatabase.IronOre), 20);
            Assert.Greater(chest.Items.Count(ItemDatabase.Coal), 20);
        }

        [Test]
        public void HeadOn_Belts_DoNotPass()
        {
            var (g, x, y) = World(2, 1);
            var a = (ConveyorState)P(g, "belt", x, y, Rotation.R90).State;
            var b = (ConveyorState)P(g, "belt", x + 1, y, Rotation.R270).State;
            a.TryInsert(ItemDatabase.IronOre, 1);
            for (int t = 0; t < 40; t++) Tick(g);
            Assert.AreEqual(1, a.ItemCount); Assert.AreEqual(0, b.ItemCount);
            Assert.AreEqual("blocked", a.Status);
        }

        [Test]
        public void Arm_CarriesTheItemInItsHand_AndWaitsHoldingItWhenTheTargetIsFull()
        {
            var (g, x, y) = World(1, 3);
            var src = (StorageState)P(g, "chest", x, y).State;
            var arm = (InserterState)P(g, "inserter", x, y + 1, Rotation.R0).State;   // takes from below, puts above
            var dst = (ConveyorState)P(g, "conveyor", x, y + 2, Rotation.R90).State; // stopped: keeps what it gets
            src.TryInsert(ItemDatabase.IronOre, 3);
            Tick(g);
            Assert.AreEqual(ItemDatabase.IronOre, arm.Held, "picked up");
            Assert.AreEqual(2, src.Items.Count(ItemDatabase.IronOre));
            Assert.AreEqual(0, dst.ItemCount, "still on its way");
            int ticks = 1;
            while (dst.ItemCount == 0 && ticks < 100) { Tick(g); ticks++; }
            Assert.AreEqual(1, dst.ItemCount);
            Assert.AreEqual(InserterState.SwingSeconds / 2, ticks * 0.05f, 0.051f, "half a swing to the drop");
            for (int t = 0; t < 60; t++) Tick(g);
            Assert.AreEqual(2, dst.ItemCount, "the stopped conveyor's middle is full");
            Assert.AreEqual(ItemDatabase.IronOre, arm.Held, "the arm holds the third item");
            Assert.AreEqual(0, src.Items.Count(ItemDatabase.IronOre));
            Assert.AreEqual("target full", arm.Status);
            Assert.IsTrue(arm.Asleep, "and sleeps until the target changes");
            dst.Contents.TryRemove(ItemDatabase.IronOre, 2);
            Assert.IsFalse(arm.Asleep);
            Tick(g);
            Assert.IsNull(arm.Held, "dropped as soon as there is room");
            Assert.AreEqual(3, arm.Moved);
        }

        [Test]
        public void Arm_DoesNotPickUpWhatTheTargetNeverAccepts()
        {
            var (g, x, y) = World(1, 3);
            var src = (StorageState)P(g, "chest", x, y).State;
            var arm = (InserterState)P(g, "inserter", x, y + 1, Rotation.R0).State;
            var gen = P(g, "generator", x, y + 2).State;   // burns coal only
            src.TryInsert(ItemDatabase.Stone, 5);
            for (int t = 0; t < 40; t++) Tick(g);
            Assert.IsNull(arm.Held);
            Assert.AreEqual(5, src.Items.Count(ItemDatabase.Stone));
            src.TryInsert(ItemDatabase.Coal, 1);
            for (int t = 0; t < 40; t++) Tick(g);
            Assert.AreEqual(0, src.Items.Count(ItemDatabase.Coal), "coal goes in");
            Assert.IsNull(arm.Held);
        }

        [Test]
        public void LanesAndHand_SurviveSaveAndLoad_AndOldSingleItemSavesMigrate()
        {
            var (g, x, y) = World(4, 3);
            var belt = (ConveyorState)P(g, "belt", x, y, Rotation.R90).State;
            P(g, "chest", x + 2, y + 2).State.TryInsert(ItemDatabase.Coal, 1);
            var arm = (InserterState)P(g, "inserter", x + 2, y + 1, Rotation.R180).State;   // from the chest above down onto...
            P(g, "conveyor", x + 2, y, Rotation.R90);                                      // ...a stopped conveyor
            belt.TryInsert(ItemDatabase.IronOre, 1); belt.TryInsert(ItemDatabase.Stone, 1);
            Tick(g); Tick(g);
            Assert.IsNotNull(arm.Held); Assert.Greater(arm.Arm, 0f);
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var text = sw.ToString();
            StringAssert.Contains("bx lanes ", text);

            var g2 = SaveSystem.Read(new StringReader(text));
            var b2 = (ConveyorState)g2.Grid.At(new Cell(x, y)).State;
            for (int l = 0; l < 2; l++)
            {
                Assert.AreEqual(belt.Lane(l).Count, b2.Lane(l).Count);
                for (int i = 0; i < belt.Lane(l).Count; i++) { Assert.AreEqual(belt.Lane(l)[i].Id, b2.Lane(l)[i].Id); Assert.AreEqual(belt.Lane(l)[i].Pos, b2.Lane(l)[i].Pos); }
            }
            var a2 = (InserterState)g2.Grid.At(new Cell(x + 2, y + 1)).State;
            Assert.AreEqual(arm.Held, a2.Held); Assert.AreEqual(arm.Arm, a2.Arm);
            var sw2 = new StringWriter(); SaveSystem.Write(g2, sw2);
            Assert.AreEqual(text, sw2.ToString(), "stable format");

            // a save from the single-item belts: "bi item <id> 1" and "bx prog <p>", no lanes
            var old = new StringWriter(); SaveSystem.Write(g, old);
            var lines = old.ToString().Replace("\r\n", "\n").Split('\n');
            var sb = new System.Text.StringBuilder(); bool firstBelt = true;
            foreach (var line in lines)
            {
                if (line.StartsWith("bx lanes ")) { if (firstBelt) { sb.Append("bx prog 0.4\n"); firstBelt = false; } continue; }
                if (line.StartsWith("bi item " + ItemDatabase.Stone)) continue;   // old belts held one item
                if (line.StartsWith("bi hand ") || line.StartsWith("bx arm ")) continue;   // old arms had no hand
                sb.Append(line).Append('\n');
            }
            var g3 = SaveSystem.Read(new StringReader(sb.ToString()));
            var b3 = (ConveyorState)g3.Grid.At(new Cell(x, y)).State;
            Assert.AreEqual(1, b3.ItemCount);
            Assert.AreEqual(ItemDatabase.IronOre, b3.Item);
            Assert.AreEqual(0.4f, b3.Progress, 1e-5f, "the old progress becomes the item's position");
            Assert.IsNull(((InserterState)g3.Grid.At(new Cell(x + 2, y + 1)).State).Held);
        }

        [Test]
        public void TakingFromABelt_RemovesTheFrontMostItem()
        {
            var (g, x, y) = World(1, 1);
            var belt = (ConveyorState)P(g, "belt", x, y, Rotation.R0).State;
            belt.TryInsert(ItemDatabase.IronOre, 1);          // right lane, middle
            Tick(g); Tick(g);                                  // moves ahead
            belt.TryInsert(ItemDatabase.IronOre, 1);          // right lane is clear in the middle again? else left
            float front = belt.Progress;
            Assert.AreEqual(2, belt.ItemCount);
            Assert.IsTrue(belt.Contents.TryRemove(ItemDatabase.IronOre, 1));
            Assert.AreEqual(1, belt.ItemCount);
            Assert.Less(belt.Progress, front, "the item nearest the front went");
        }
    }
}
