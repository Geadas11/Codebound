using System.Diagnostics;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 8 of the industrial simulation: ids and actions in saves, multiplayer with the new systems, a mixed base.</summary>
    public class IntegrationTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 91)
        {
            var g = new GameState(seed);
            var s = g.World.Generator.FindSpawn();
            for (int r = 3; r < 250; r++)
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

        static string Save(GameState g) { var w = new StringWriter(); SaveSystem.Write(g, w); return w.ToString(); }

        [Test]
        public void MachineIds_SurviveSaveAndLoad_EvenWithGaps()
        {
            var (g, x, y) = World(6, 2);
            var a = P(g, "chest", x, y); var b = P(g, "chest", x + 1, y); var c = P(g, "furnace", x + 2, y);
            g.Grid.Remove(b);                              // a gap: ids 1, 3 left (plus whatever the world started with)
            var d = P(g, "chest", x + 5, y);
            var h = SaveSystem.Read(new StringReader(Save(g)));
            Assert.AreEqual(a.InstanceId, h.Grid.At(new Cell(x, y)).InstanceId);
            Assert.AreEqual(c.InstanceId, h.Grid.At(new Cell(x + 2, y)).InstanceId, "programs that remember m-ids still find their machine");
            Assert.AreEqual(d.InstanceId, h.Grid.At(new Cell(x + 5, y)).InstanceId);
            Assert.Greater(P(h, "chest", x + 1, y).InstanceId, d.InstanceId, "new ids stay newer");
            // an old save (no ids) still loads, numbered in order
            var old = Save(g).Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < old.Length; i++) if (old[i].StartsWith("b ")) old[i] = old[i].Substring(0, old[i].LastIndexOf(' '));
            var o = SaveSystem.Read(new StringReader(string.Join("\n", old)));
            Assert.IsNotNull(o.Grid.At(new Cell(x + 5, y)));
        }

        [Test]
        public void ActionsInProgress_AreSaved_AndFinishOnTime()
        {
            var (g, x, y) = World(4, 2);
            var fb = P(g, "furnace", x, y);
            var f = (FurnaceState)fb.State;
            f.TryInsert(ItemDatabase.IronOre, 2); f.TryInsert(ItemDatabase.Coal, 2);
            Assert.IsNull(g.Factory.StartSmelt(fb, null));
            for (int i = 0; i < 30; i++) g.Step();          // 1.5 of 3.2 s
            var text = Save(g);
            StringAssert.Contains("act " + fb.InstanceId + " smelt", text);
            var h = SaveSystem.Read(new StringReader(text));
            var hf = (FurnaceState)h.Grid.ById(fb.InstanceId).State;
            Assert.IsTrue(h.Factory.HasAction(h.Grid.ById(fb.InstanceId)), "still smelting after loading");
            int ticks = 0;
            while (hf.Output.Count(ItemDatabase.IronPlate) == 0 && ticks < 200) { h.Step(); g.Step(); ticks++; }
            Assert.AreEqual(1, hf.Output.Count(ItemDatabase.IronPlate));
            Assert.AreEqual(1, f.Output.Count(ItemDatabase.IronPlate), "the unsaved world finished in the same tick");
            Assert.AreEqual(Simulation.StateHash(g), Simulation.StateHash(h));
        }

        [Test]
        public void Guests_SeeRobotsInFlight_LitLamps_AndTrains()
        {
            var (host, x, y) = World(16, 8);
            var port = (RoboportState)P(host, "roboport", x, y).State; port.TryInsert(ItemDatabase.Robot, 2);
            P(host, "solar", x + 2, y); host.Factory.SolarFactor = 1f;
            P(host, "provider_chest", x + 10, y).State.TryInsert(ItemDatabase.IronPlate, 10);
            ((StorageState)P(host, "requester_chest", x + 12, y + 3).State).SetRequests("iron_plate=5");
            P(host, "lamp", x + 4, y);
            for (int i = 0; i < 10; i++) host.Factory.Tick(0.05f);
            Assert.Greater(host.Factory.Robots.Count, 0);
            var guest = new GameState(host.Seed);
            Multiplayer.Apply(guest, SaveSystem.Read(new StringReader(Multiplayer.Snapshot(host))));
            Assert.AreEqual(host.Factory.Robots.Count, guest.Factory.Robots.Count, "robots in flight are drawn on guests too");
            Assert.AreEqual(host.Factory.Robots[0].X, guest.Factory.Robots[0].X, 1e-9);
            Assert.IsTrue(((LampState)guest.Grid.At(new Cell(x + 4, y)).State).Lit);
            Assert.AreEqual(host.Grid.At(new Cell(x + 12, y + 3)).InstanceId, guest.Grid.At(new Cell(x + 12, y + 3)).InstanceId, "same ids as the host");
        }

        /// <summary>Every new system at once, repeated over a large area: belts, arms, fluids, steam, signals, robots, trains.</summary>
        static GameState MixedBase(int blocks)
        {
            var g = new GameState(93);
            var s = g.World.Generator.FindSpawn();
            int made = 0;
            for (int r = 1; r < 60 && made < blocks; r++)
                for (int k = -r; k <= r && made < blocks; k++)
                    foreach (var (bx, by) in new[] { (k, -r), (k, r), (-r, k), (r, k) })
                    {
                        if (made >= blocks) break;
                        int ox = s.X + bx * 16, oy = s.Y + by * 12;
                        bool ok = true;
                        for (int dx = 0; dx < 16 && ok; dx++) for (int dy = 0; dy < 12 && ok; dy++) ok = g.CanPlace(BuildingDatabase.Get("wall"), new Cell(ox + dx, oy + dy), Rotation.R0);
                        if (!ok) continue;
                        Block(g, ox, oy);
                        made++;
                    }
            Assert.Greater(made, blocks / 2, "room for the benchmark");
            return g;
        }

        static void Block(GameState g, int x, int y)
        {
            PlacedBuilding Q(string id, int dx, int dy, Rotation r = Rotation.R0) => g.Grid.Place(BuildingDatabase.Get(id), new Cell(x + dx, y + dy), r);
            // steam power
            var t = (PipeState)Q("tank", 0, 0).State; t.LoadExtra("fluid", Fluids.Water); t.LoadExtra("amount", "2400");
            Q("pipe", 2, 0); Q("boiler", 3, 0, Rotation.R90).State.TryInsert(ItemDatabase.Coal, 50); Q("pipe", 5, 0); Q("steam_engine", 6, 0, Rotation.R90);
            Q("pole", 8, 1);
            // a belt loop with ore, fed and emptied by arms, a lamp watching a chest
            var src = (StorageState)Q("chest", 0, 3).State; src.TryInsert(ItemDatabase.IronOre, 200);
            Q("inserter", 1, 3, Rotation.R90);
            for (int i = 2; i < 8; i++) Q("belt", i, 3, Rotation.R90);
            Q("chest", 8, 3);
            Q("cable", 8, 4); Q("lamp", 9, 4).State.SetCondition("iron_ore>50");
            Q("computer", 10, 1);
            // robots between two chests
            var port = Q("roboport", 0, 6); port.State.TryInsert(ItemDatabase.Robot, 2);
            Q("provider_chest", 4, 6).State.TryInsert(ItemDatabase.CopperPlate, 100);
            var req = (StorageState)Q("requester_chest", 8, 7).State; req.SetRequests("copper_plate=40");
            Q("inserter", 9, 7, Rotation.R90); Q("chest", 10, 7);
            // a short train line
            for (int i = 0; i < 16; i++) { Q("rail", i, 10); Q("rail", i, 11); }
            Q("rail", 0, 9); Q("rail", 15, 9);
        }

        [Test]
        public void AMixedBase_WithEverySystem_TicksInsideTheBudget()
        {
            var g = MixedBase(40);
            int n = 0; foreach (var _ in g.Grid.Buildings) n++;
            for (int i = 0; i < 40; i++) g.Step();   // warm up
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 200; i++) g.Step();
            double ms = sw.Elapsed.TotalMilliseconds / 200;
            TestContext.WriteLine($"[perf] mixed base, {n} buildings: {ms:0.000} ms per tick");
            Assert.Less(ms, 10.0, $"{n} buildings took {ms:0.00} ms per tick (budget 50)");
        }

        [Test]
        public void AMixedBase_SleepingOrNot_SavedOrNot_GivesTheSameWorld()
        {
            var a = MixedBase(12); var b = MixedBase(12);
            b.Factory.SleepEnabled = false;
            for (int i = 0; i < 300; i++) { a.Step(); b.Step(); }
            Assert.AreEqual(Simulation.StateHash(b), Simulation.StateHash(a), "sleeping changes nothing");
            var c = SaveSystem.Read(new StringReader(Save(a)));
            for (int i = 0; i < 300; i++) { a.Step(); c.Step(); }
            Assert.AreEqual(Simulation.StateHash(a), Simulation.StateHash(c), "saving and loading mid-run changes nothing");
        }
    }
}
