using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 13: construction robots build planned buildings and take marked ones down.</summary>
    public class ConstructionTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 141)
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

        static PlacedBuilding P(GameState g, string id, int x, int y) =>
            g.Grid.Place(BuildingDatabase.Get(id), new Cell(x, y), Rotation.R0) ?? throw new AssertionException($"cannot place {id} at {x},{y}");

        static void Run(GameState g, float s) { for (float t = 0; t < s - 1e-4f; t += 0.05f) g.Factory.Tick(0.05f); }

        /// <summary>A powered roboport with 2 construction robots and a provider chest 8 cells away.</summary>
        static (GameState g, RoboportState port, StorageState provider, int x, int y) Base()
        {
            var (g, x, y) = World(14, 8);
            var port = (RoboportState)P(g, "roboport", x, y).State;
            P(g, "solar", x + 2, y); g.Factory.SolarFactor = 1f;
            Assert.IsTrue(port.TryInsert(ItemDatabase.ConstructionRobot, 2));
            var provider = (StorageState)P(g, "provider_chest", x + 8, y).State;
            return (g, port, provider, x, y);
        }

        [Test]
        public void APlannedChest_IsBuiltByRobots_FromTheNetworksMaterials()
        {
            var (g, port, provider, x, y) = Base();
            provider.TryInsert(ItemDatabase.Wood, 20);
            Assert.IsNull(g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 5, y + 5), Rotation.R0));
            Assert.IsNotNull(g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 5, y + 5), Rotation.R0), "already planned");
            Run(g, 0.1f);
            Assert.AreEqual(1, g.Factory.Builders.Count, "a robot set off");
            Run(g, 20f);
            Assert.AreEqual("chest", g.Grid.At(new Cell(x + 5, y + 5))?.Def.Id, "built");
            Assert.AreEqual(12, provider.Items.Count(ItemDatabase.Wood), "8 wood used");
            Assert.AreEqual(0, g.Factory.Ghosts.Count);
            Assert.AreEqual(0, g.Factory.Builders.Count, "back home");
            Assert.AreEqual(2, port.Robots.Count(ItemDatabase.ConstructionRobot));
        }

        [Test]
        public void Materials_CanComeFromSeveralChests_AndAPlanWaitsForWhatIsMissing()
        {
            var (g, port, provider, x, y) = Base();
            var second = (StorageState)P(g, "provider_chest", x + 10, y + 3).State;
            provider.TryInsert(ItemDatabase.Stone, 6); second.TryInsert(ItemDatabase.Stone, 6);   // a furnace needs 10
            g.Factory.PlaceGhost(BuildingDatabase.Get("furnace"), new Cell(x + 4, y + 5), Rotation.R0);
            g.Factory.PlaceGhost(BuildingDatabase.Get("generator"), new Cell(x + 7, y + 5), Rotation.R0);   // stone 8, iron ore 6, copper ore 4: nothing in the network
            Run(g, 25f);
            Assert.AreEqual("furnace", g.Grid.At(new Cell(x + 4, y + 5))?.Def.Id);
            Assert.AreEqual(2, provider.Items.Count(ItemDatabase.Stone) + second.Items.Count(ItemDatabase.Stone));
            Assert.IsNull(g.Grid.At(new Cell(x + 7, y + 5)), "no materials: the generator waits");
            Assert.AreEqual(1, g.Factory.Ghosts.Count);
            provider.TryInsert(ItemDatabase.Stone, 8); provider.TryInsert(ItemDatabase.IronOre, 6); provider.TryInsert(ItemDatabase.CopperOre, 4);
            Run(g, 25f);
            Assert.AreEqual("generator", g.Grid.At(new Cell(x + 7, y + 5))?.Def.Id, "built once the materials arrived");
        }

        [Test]
        public void AMarkedBuilding_IsTakenDown_AndItsRefundStored()
        {
            var (g, port, provider, x, y) = Base();
            var victim = P(g, "chest", x + 5, y + 5);
            ((StorageState)victim.State).Items.Add(ItemDatabase.Stone, 15);
            Assert.IsTrue(g.Factory.ToggleDemolish(victim));
            Assert.IsTrue(g.Factory.MarkedForDemolition(victim));
            Run(g, 20f);
            Assert.IsNull(g.Grid.At(new Cell(x + 5, y + 5)), "gone");
            Assert.AreEqual(8, provider.Items.Count(ItemDatabase.Wood), "its cost");
            Assert.AreEqual(15, provider.Items.Count(ItemDatabase.Stone), "and its contents");
            Assert.AreEqual(0, g.Factory.Demolish.Count);
            Assert.AreEqual(2, port.Robots.Count(ItemDatabase.ConstructionRobot));
        }

        [Test]
        public void Plans_AreRefusedWhereNothingCanBeBuilt_AndBuildingOverAPlanClearsIt()
        {
            var (g, port, provider, x, y) = Base();
            Assert.IsNotNull(g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 8, y), Rotation.R0), "a chest is there");
            Assert.IsNull(g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 3, y + 6), Rotation.R0));
            P(g, "wall", x + 3, y + 6);
            Assert.AreEqual(0, g.Factory.Ghosts.Count, "built over by hand: the plan is gone");
            Assert.IsNull(g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 4, y + 6), Rotation.R0));
            Assert.IsTrue(g.Factory.CancelGhost(new Cell(x + 4, y + 6)));
            Assert.AreEqual(0, g.Factory.Ghosts.Count);
        }

        [Test]
        public void Construction_IsSaved_AndContinuesIdentically()
        {
            var (g, port, provider, x, y) = Base();
            provider.TryInsert(ItemDatabase.Wood, 40);
            g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 5, y + 5), Rotation.R0);
            g.Factory.PlaceGhost(BuildingDatabase.Get("chest"), new Cell(x + 6, y + 6), Rotation.R0);
            var victim = P(g, "chest", x + 3, y + 6); g.Factory.ToggleDemolish(victim);
            for (int i = 0; i < 40; i++) g.Step();
            var w = new StringWriter(); SaveSystem.Write(g, w);
            StringAssert.Contains("crobot ", w.ToString());
            var h = SaveSystem.Read(new StringReader(w.ToString()));
            for (int i = 0; i < 600; i++) { g.Step(); h.Step(); }
            Assert.AreEqual(Simulation.StateHash(g), Simulation.StateHash(h));
        }
    }
}
