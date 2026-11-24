using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 6 of the industrial simulation: roboports, logistic networks, robots, provider and requester chests.</summary>
    public class RobotTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 71)
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

        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds - 1e-4f; t += 0.05f) g.Factory.Tick(0.05f); }

        /// <summary>A powered roboport with robots at (x, y), a provider chest 9 cells away and a requester chest near the edge of its area.</summary>
        static (GameState g, RoboportState port, StorageState provider, StorageState requester, int x, int y) Base(int robots = 2)
        {
            var (g, x, y) = World(16, 6);
            var port = (RoboportState)P(g, "roboport", x, y).State;
            P(g, "solar", x + 2, y); g.Factory.SolarFactor = 1f;
            if (robots > 0) Assert.IsTrue(port.TryInsert(ItemDatabase.Robot, robots));
            var provider = (StorageState)P(g, "provider_chest", x + 10, y).State;
            var requester = (StorageState)P(g, "requester_chest", x + 12, y + 3).State;
            return (g, port, provider, requester, x, y);
        }

        static int Total(GameState g, string item)
        {
            int n = 0;
            foreach (var b in g.Grid.Buildings) if (b.State != null) foreach (var (_, inv) in b.State.Inventories) n += inv.Count(item);
            foreach (var r in g.Factory.Robots) if (r.Item == item && r.Task == RobotTask.ToDrop || r.Item == item && r.Task == RobotTask.Return) n += r.Count;
            return n;
        }

        [Test]
        public void Robots_TopUpARequesterChest_FromAProvider_AndComeBack()
        {
            var (g, port, provider, requester, _, _) = Base();
            provider.TryInsert(ItemDatabase.IronPlate, 20);
            Assert.IsNull(requester.SetRequests("iron_plate = 10"));
            Run(g, 1f);
            Assert.AreEqual(2, g.Factory.Robots.Count, "both robots took off");
            Assert.AreEqual(0, port.Robots.Count(ItemDatabase.Robot));
            Run(g, 30f);
            Assert.AreEqual(10, requester.Items.Count(ItemDatabase.IronPlate), "exactly what was asked");
            Assert.AreEqual(10, provider.Items.Count(ItemDatabase.IronPlate));
            Assert.AreEqual(0, g.Factory.Robots.Count, "all landed");
            Assert.AreEqual(2, port.Robots.Count(ItemDatabase.Robot));
            requester.Items.TryRemove(ItemDatabase.IronPlate, 6);   // an arm or the player takes some: robots top it up again
            Run(g, 20f);
            Assert.AreEqual(10, requester.Items.Count(ItemDatabase.IronPlate));
            Assert.AreEqual(4, provider.Items.Count(ItemDatabase.IronPlate));
        }

        [Test]
        public void TwoRequesters_ShareWhatThereIs_AndNothingIsPromisedTwice()
        {
            var (g, port, provider, requester, x, y) = Base(robots: 6);
            var second = (StorageState)P(g, "requester_chest", x + 12, y + 5).State;
            provider.TryInsert(ItemDatabase.CopperPlate, 12);
            requester.SetRequests("copper_plate=10"); second.SetRequests("copper_plate=10");
            int before = Total(g, ItemDatabase.CopperPlate);
            for (int i = 0; i < 800; i++)
            {
                g.Factory.Tick(0.05f);
                Assert.AreEqual(before, Total(g, ItemDatabase.CopperPlate), "items are never made or lost in flight");
            }
            Assert.AreEqual(12, requester.Items.Count(ItemDatabase.CopperPlate) + second.Items.Count(ItemDatabase.CopperPlate));
            Assert.AreEqual(10, requester.Items.Count(ItemDatabase.CopperPlate), "the older requester is served first");
            Assert.AreEqual(0, provider.Items.Count(ItemDatabase.CopperPlate));
        }

        [Test]
        public void APoweredStorageHub_LendsItsChests()
        {
            var (g, port, provider, requester, x, y) = Base();
            P(g, "storage_hub", x + 5, y + 3);
            P(g, "pole", x + 4, y + 3);   // the hub needs power too
            var chest = (StorageState)P(g, "chest", x + 7, y + 3).State;
            chest.TryInsert(ItemDatabase.Stone, 30);
            requester.SetRequests("stone=8");
            Run(g, 30f);
            Assert.AreEqual(8, requester.Items.Count(ItemDatabase.Stone), "from the hub's chest");
            Assert.AreEqual(22, chest.Items.Count(ItemDatabase.Stone));
            var info = g.Factory.LogisticInfo(g.Grid.At(new Cell(x, y)));
            Assert.AreEqual(1, info["storageChests"]);
        }

        [Test]
        public void NoPower_NoNewFlights_AndAMissingChest_SendsTheRobotHome()
        {
            var (g, port, provider, requester, x, y) = Base();
            g.Factory.SolarFactor = 0f;
            provider.TryInsert(ItemDatabase.IronPlate, 8);
            requester.SetRequests("iron_plate=8");
            Run(g, 2f);
            Assert.AreEqual(0, g.Factory.Robots.Count, "an unpowered roboport sends nobody");
            g.Factory.SolarFactor = 1f;
            Run(g, 0.5f);
            Assert.AreEqual(2, g.Factory.Robots.Count);
            g.Grid.Remove(g.Grid.At(new Cell(x + 10, y)));   // the provider goes before they arrive
            Run(g, 15f);
            Assert.AreEqual(0, g.Factory.Robots.Count, "they went back");
            Assert.AreEqual(2, port.Robots.Count(ItemDatabase.Robot));
            Assert.AreEqual(0, requester.Items.Count(ItemDatabase.IronPlate));
        }

        [Test]
        public void RobotsInFlight_AreSaved_AndTheWorldContinuesIdentically()
        {
            var (g, port, provider, requester, x, y) = Base(robots: 3);
            provider.TryInsert(ItemDatabase.IronGear, 30);
            requester.SetRequests("iron_gear=25");
            for (int i = 0; i < 53; i++) g.Step();
            Assert.Greater(g.Factory.Robots.Count, 0);
            var w = new StringWriter(); SaveSystem.Write(g, w);
            StringAssert.Contains("lrobot ", w.ToString());
            StringAssert.Contains("bx req iron_gear=25", w.ToString());
            var h = SaveSystem.Read(new StringReader(w.ToString()));
            for (int i = 0; i < 400; i++) { g.Step(); h.Step(); }
            Assert.AreEqual(Simulation.StateHash(g), Simulation.StateHash(h));
            int got = ((StorageState)h.Grid.At(new Cell(x + 12, y + 3)).State).Items.Count(ItemDatabase.IronGear);   // Step() runs the real clock: the light (power, speed) varies
            Assert.Greater(got, 0, "deliveries went on after loading");
            Assert.AreEqual(requester.Items.Count(ItemDatabase.IronGear), got);
        }

        [Test]
        public void ARoboport_OnASignalCable_SendsTheNetworksItems_AndReadyRobots()
        {
            var (g, port, provider, requester, x, y) = Base(robots: 3);
            provider.TryInsert(ItemDatabase.Coal, 7);
            P(g, "cable", x, y + 2);
            g.Factory.Tick(0.05f); g.Factory.Tick(0.05f);
            var sig = g.Factory.SignalsAt(g.Grid.At(new Cell(x, y + 2)));
            Assert.AreEqual(7, sig[ItemDatabase.Coal]);
            Assert.AreEqual(3, sig["R"]);
        }

        [Test]
        public void Requests_AreValidated()
        {
            var (g, port, provider, requester, _, _) = Base();
            Assert.IsNotNull(requester.SetRequests("A=5"), "items only");
            Assert.IsNotNull(requester.SetRequests("iron_plate=99999"));
            Assert.IsNotNull(provider.SetRequests("iron_plate=5"), "providers take no requests");
            Assert.IsNull(requester.SetRequests("copper_plate=3,iron_plate=5"));
            Assert.AreEqual("copper_plate=3,iron_plate=5", requester.RequestText);
            Assert.IsNull(requester.SetRequests(""));
            Assert.AreEqual(0, requester.Requests.Count);
        }
    }
}
