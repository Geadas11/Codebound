using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 7 of the industrial simulation: rails, signals, stops, depots, schedules, cargo.</summary>
    public class TrainTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 81)
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

        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds - 1e-4f; t += 0.05f) g.Factory.Tick(0.05f); }

        /// <summary>A 12×6 loop of rail at (lx, ly), a depot under it, stop "a" under the bottom row, stop "b" over the top row.
        /// With signals: one-way, clockwise (bottom row leftwards, top row rightwards), four blocks.</summary>
        static (GameState g, int lx, int ly) Loop(bool signals = false)
        {
            var (g, x, y) = World(14, 12);
            int lx = x + 1, ly = y + 4;
            for (int i = 0; i < 12; i++)
            {
                bool sigB = signals && (i == 2 || i == 9), sigT = signals && (i == 3 || i == 8);
                P(g, sigB ? "rail_signal" : "rail", lx + i, ly, sigB ? Rotation.R270 : Rotation.R0);
                P(g, sigT ? "rail_signal" : "rail", lx + i, ly + 5, sigT ? Rotation.R90 : Rotation.R0);
            }
            for (int j = 1; j < 5; j++) { P(g, "rail", lx, ly + j); P(g, "rail", lx + 11, ly + j); }
            return (g, lx, ly);
        }

        static TrainDepotState Depot(GameState g, int x, int y, string schedule, int wagons = 1)
        {
            var d = (TrainDepotState)P(g, "train_depot", x, y).State;   // facing up: the train starts on the rail above
            Assert.IsTrue(d.TryInsert(ItemDatabase.Locomotive, 1));
            if (wagons > 0) Assert.IsTrue(d.TryInsert(ItemDatabase.CargoWagon, wagons));
            Assert.IsTrue(d.TryInsert(ItemDatabase.Coal, 20));
            Assert.IsNull(d.SetSchedule(schedule));
            return d;
        }

        [Test]
        public void Schedules_AreParsedStrictly()
        {
            var d = new TrainDepotState();
            Assert.IsNull(d.SetSchedule(" mine : full , base:empty, depot:30s, x:iron_ore >= 100"));
            Assert.AreEqual("mine:full,base:empty,depot:30,x:iron_ore>=100", d.ScheduleText);
            Assert.IsNotNull(d.SetSchedule("Mine:full"), "lower case names");
            Assert.IsNotNull(d.SetSchedule("mine:sometimes"));
            Assert.IsNotNull(d.SetSchedule(":full"));
            Assert.AreEqual(4, d.Schedule.Count, "a bad schedule keeps the old one");
            Assert.IsFalse(d.TryInsert(ItemDatabase.Locomotive, 2), "one locomotive per depot");
            Assert.IsFalse(d.TryInsert(ItemDatabase.CargoWagon, 5), "at most 4 wagons");
            var s = new TrainStopState();
            Assert.IsNull(s.Rename("iron_mine")); Assert.IsNotNull(s.Rename("Iron Mine"));
        }

        [Test]
        public void ATrain_CarriesOre_FromStopAToStopB_LoadedAndUnloadedByArms()
        {
            var (g, lx, ly) = Loop();
            var depot = Depot(g, lx + 3, ly - 2, "a:iron_ore>=20,b:empty");
            // a: chest -> arm -> stop (under the bottom row); b: stop -> arm -> chest (over the top row)
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 8, ly - 1), "a"));
            P(g, "inserter", lx + 8, ly - 2, Rotation.R0); var src = (StorageState)P(g, "chest", lx + 8, ly - 3).State;
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 5, ly + 6), "b"));
            P(g, "inserter", lx + 5, ly + 7, Rotation.R0); var dst = (StorageState)P(g, "chest", lx + 5, ly + 8).State;
            src.TryInsert(ItemDatabase.IronOre, 60);
            int coal = depot.Fuel.Count(ItemDatabase.Coal);
            Run(g, 1f);
            Assert.IsTrue(depot.HasTrain, "the train left the depot");
            Run(g, 120f);
            Assert.GreaterOrEqual(dst.Items.Count(ItemDatabase.IronOre), 20, "at least one full round");
            int inArms = 0;
            foreach (var b in g.Grid.Buildings) if (b.State is InserterState ins && ins.Held == ItemDatabase.IronOre) inArms++;
            Assert.AreEqual(60, src.Items.Count(ItemDatabase.IronOre) + dst.Items.Count(ItemDatabase.IronOre) + depot.CargoCount(ItemDatabase.IronOre) + inArms, "no ore made or lost");
            Assert.Less(depot.Fuel.Count(ItemDatabase.Coal), coal, "it burns coal");
        }

        [Test]
        public void Signals_KeepTwoTrains_OutOfEachOthersBlocks()
        {
            var (g, lx, ly) = Loop(signals: true);
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 6, ly - 1), "a"));
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 6, ly + 6), "b"));
            var d1 = Depot(g, lx + 3, ly - 3 + 1, "b:1,a:1", wagons: 2);
            var d2 = Depot(g, lx + 7, ly - 3 + 1, "b:1,a:1", wagons: 2);
            var heads = new HashSet<Cell>();
            int arrivals1 = 0, arrivals2 = 0; var was1 = TrainPhase.Moving; var was2 = TrainPhase.Moving;
            for (int i = 0; i < 2400; i++)
            {
                g.Factory.Tick(0.05f);
                if (d1.Phase == TrainPhase.Waiting && was1 != TrainPhase.Waiting) arrivals1++;
                if (d2.Phase == TrainPhase.Waiting && was2 != TrainPhase.Waiting) arrivals2++;
                was1 = d1.Phase; was2 = d2.Phase;
                var cells = new HashSet<Cell>();
                foreach (var d in new[] { d1, d2 })
                {
                    if (!d.HasTrain) continue;
                    heads.Add(d.Trail[0]);
                    foreach (var c in d.Trail) Assert.IsTrue(cells.Add(c), $"two trains on one cell at tick {i}");
                }
            }
            Assert.Greater(heads.Count, 4, "the trains moved");
            Assert.GreaterOrEqual(arrivals1, 4, "train 1 keeps running its schedule (no deadlock)");
            Assert.GreaterOrEqual(arrivals2, 4, "and so does train 2");
        }

        [Test]
        public void NoUTurns_ADeadEnd_MeansNoPath()
        {
            var (g, x, y) = World(10, 6);
            for (int i = 0; i < 8; i++) P(g, "rail", x + i, y + 3);
            Assert.IsNull(Factory.Configure(P(g, "train_stop", x + 7, y + 4), "end"));
            Assert.IsNull(Factory.Configure(P(g, "train_stop", x, y + 4), "start"));
            var d = Depot(g, x + 3, y + 1, "end:1,start:1");
            Run(g, 10f);
            StringAssert.StartsWith("no path", d.Status, "it cannot turn round at the end of the line");
            Assert.AreEqual(new Cell(x + 7, y + 3), d.Trail[0]);
        }

        [Test]
        public void Trains_AreSaved_AndTheWorldContinuesIdentically()
        {
            var (g, lx, ly) = Loop(signals: true);
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 6, ly - 1), "a"));
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 6, ly + 6), "b"));
            Depot(g, lx + 3, ly - 2, "b:2,a:2", wagons: 2);
            for (int i = 0; i < 77; i++) g.Step();
            var w = new StringWriter(); SaveSystem.Write(g, w);
            StringAssert.Contains("bx trail ", w.ToString());
            var h = SaveSystem.Read(new StringReader(w.ToString()));
            for (int i = 0; i < 400; i++) { g.Step(); h.Step(); }
            Assert.AreEqual(Simulation.StateHash(g), Simulation.StateHash(h));
        }

        [Test]
        public void RemovingTheDepot_RefundsTheTrainAndItsCargo_AndARemovedRailStopsIt()
        {
            var (g, lx, ly) = Loop();
            Assert.IsNull(Factory.Configure(P(g, "train_stop", lx + 6, ly + 6), "b"));
            var depotB = P(g, "train_depot", lx + 3, ly - 2);
            var d = (TrainDepotState)depotB.State;
            d.TryInsert(ItemDatabase.Locomotive, 1); d.TryInsert(ItemDatabase.CargoWagon, 1); d.TryInsert(ItemDatabase.Coal, 5);
            d.SetSchedule("b:5");
            d.Wagons[0].Add(ItemDatabase.Stone, 30);
            Run(g, 2f);
            Assert.IsTrue(d.HasTrain);
            var head = d.Trail[0];
            g.Grid.Remove(g.Grid.At(head));
            Run(g, 1f);
            StringAssert.StartsWith("derailed", d.Status);
            P(g, "rail", head.X, head.Y);
            g.Inventory.Clear();
            Assert.IsTrue(g.TryDeconstruct(depotB));
            Assert.AreEqual(1, g.Inventory.Count(ItemDatabase.Locomotive));
            Assert.AreEqual(1, g.Inventory.Count(ItemDatabase.CargoWagon));
            Assert.AreEqual(30, g.Inventory.Count(ItemDatabase.Stone), "the cargo comes back too");
            CollectionAssert.IsEmpty(new List<(PlacedBuilding, TrainDepotState)>(g.Factory.Trains()));
        }
    }
}
