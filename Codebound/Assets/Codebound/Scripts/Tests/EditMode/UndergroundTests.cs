using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 9: underground belts and pipes to ground.</summary>
    public class UndergroundTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 101)
        {
            var g = new GameState(seed);
            var s = g.World.Generator.FindSpawn();
            for (int r = 3; r < 200; r++)
                for (int x = -r; x <= r; x++)
                    foreach (int y in new[] { -r, r })
                    {
                        bool ok = true;
                        for (int dx = 0; dx < w && ok; dx++) for (int dy = 0; dy < h && ok; dy++) ok = g.CanPlace(BuildingDatabase.Get("wall"), new Cell(s.X + x + dx, s.Y + y + dy), Rotation.R0);
                        if (ok) return (g, s.X + x, s.Y + y);
                    }
            throw new AssertionException("no room");
        }

        static PlacedBuilding P(GameState g, string id, int x, int y, Rotation r = Rotation.R0) =>
            g.Grid.Place(BuildingDatabase.Get(id), new Cell(x, y), r) ?? throw new AssertionException($"cannot place {id} at {x},{y}");

        static void Run(GameState g, float s) { for (float t = 0; t < s - 1e-4f; t += 0.05f) g.Factory.Tick(0.05f); }

        [Test]
        public void ItemsPassUnderWalls_FromEntranceToExit()
        {
            var (g, x, y) = World(9, 1);
            var first = (ConveyorState)P(g, "belt", x, y, Rotation.R90).State;
            var entrance = (ConveyorState)P(g, "underground_belt", x + 1, y, Rotation.R90).State;
            for (int i = 2; i <= 4; i++) P(g, "wall", x + i, y);
            var exit = (ConveyorState)P(g, "underground_belt", x + 5, y, Rotation.R90).State;
            P(g, "belt", x + 6, y, Rotation.R90);
            var chest = (StorageState)P(g, "chest", x + 7, y).State;
            Assert.IsFalse(entrance.Exit); Assert.IsTrue(exit.Exit, "the second one, facing the same way, is the exit");
            for (int t = 0; t < 200; t++) { while (first.TryInsert(ItemDatabase.IronOre, 1)) { } g.Factory.Tick(0.05f); }
            int got = chest.Items.Count(ItemDatabase.IronOre);
            Assert.Greater(got, 70, "full belt speed through the tunnel");
        }

        [Test]
        public void Pairing_NeedsTheSameDirection_AndReach()
        {
            var (g, x, y) = World(12, 3);
            var a = (ConveyorState)P(g, "underground_belt", x, y, Rotation.R90).State;
            var far = (ConveyorState)P(g, "underground_belt", x + 6, y, Rotation.R90).State;   // 6 cells: too far
            Assert.IsFalse(far.Exit, "out of reach: it is another entrance");
            var other = (ConveyorState)P(g, "underground_belt", x + 3, y + 2, Rotation.R270).State;
            Assert.IsFalse(other.Exit);
            a.TryInsert(ItemDatabase.Stone, 1);
            Run(g, 3f);
            Assert.AreEqual(1, a.ItemCount, "no exit: the item waits at the entrance");
            Assert.AreEqual("blocked", a.Status);
            var b = (ConveyorState)P(g, "underground_belt", x + 4, y, Rotation.R90).State;
            Assert.IsTrue(b.Exit, "now it has an exit");
            Run(g, 3f);
            Assert.AreEqual(0, a.ItemCount);
            Assert.AreEqual(1, b.ItemCount, "came out (and waits: nothing after the exit)");
        }

        [Test]
        public void AnExit_TakesNothingFromABeltBehindIt()
        {
            var (g, x, y) = World(8, 1);
            P(g, "underground_belt", x, y, Rotation.R90);
            var feeder = (ConveyorState)P(g, "belt", x + 2, y, Rotation.R90).State;   // points into the exit's back
            var exit = (ConveyorState)P(g, "underground_belt", x + 3, y, Rotation.R90).State;
            Assert.IsTrue(exit.Exit);
            feeder.TryInsert(ItemDatabase.Coal, 1);
            Run(g, 3f);
            Assert.AreEqual(1, feeder.ItemCount);
            Assert.AreEqual(0, exit.ItemCount);
        }

        [Test]
        public void PipesToGround_JoinUnderWalls_ButNotTheirSides()
        {
            var (g, x, y) = World(9, 3);
            var src = (PipeState)P(g, "pipe", x, y + 1).State;
            src.LoadExtra("fluid", Fluids.Water); src.LoadExtra("amount", "300");
            P(g, "pipe_to_ground", x + 1, y + 1, Rotation.R90);                        // back = x, dives towards the right
            for (int i = 2; i <= 5; i++) P(g, "wall", x + i, y + 1);
            P(g, "pipe_to_ground", x + 6, y + 1, Rotation.R270);                       // faces back: the partner
            var far = (PipeState)P(g, "pipe", x + 7, y + 1).State;
            var beside = (PipeState)P(g, "pipe", x + 1, y).State;                       // touches the first one's side only
            Run(g, 0.05f);
            Assert.AreEqual(400f, far.SegmentCapacity, 1e-3f, "pipe + 2 pipes to ground + pipe: one segment");
            Assert.AreEqual(300f, far.SegmentAmount, 1e-3f);
            Assert.IsNull(beside.Fluid, "a pipe at its side is not connected");
            Assert.AreEqual(100f, beside.SegmentCapacity, 1e-3f);
        }

        [Test]
        public void Tunnels_AreSaved()
        {
            var (g, x, y) = World(8, 1);
            P(g, "underground_belt", x, y, Rotation.R90);
            P(g, "underground_belt", x + 3, y, Rotation.R90);
            P(g, "underground_belt", x + 5, y, Rotation.R90);   // a new entrance after the exit
            var w = new StringWriter(); SaveSystem.Write(g, w);
            var h = SaveSystem.Read(new StringReader(w.ToString()));
            Assert.IsFalse(((ConveyorState)h.Grid.At(new Cell(x, y)).State).Exit);
            Assert.IsTrue(((ConveyorState)h.Grid.At(new Cell(x + 3, y)).State).Exit);
            Assert.IsFalse(((ConveyorState)h.Grid.At(new Cell(x + 5, y)).State).Exit);
            var w2 = new StringWriter(); SaveSystem.Write(h, w2);
            Assert.AreEqual(w.ToString(), w2.ToString());
        }
    }
}
