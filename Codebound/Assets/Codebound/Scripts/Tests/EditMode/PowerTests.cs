using System.Collections.Generic;
using System.Diagnostics;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 4 of the industrial simulation: networks kept up to date on every edit, priorities, statistics.</summary>
    public class PowerTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 51)
        {
            var g = new GameState(seed);
            var s = g.World.Generator.FindSpawn();
            for (int r = 3; r < 150; r++)
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

        /// <summary>Edits that change the networks: demolish and rebuild nodes and consumers, add poles in new places.</summary>
        static void Edit(GameState g, int t)
        {
            var all = g.Grid.Buildings;
            var b = all[(t * 7919) % all.Count];
            var def = b.Def; var o = b.Origin; var r = b.Rotation;
            switch (t % 3)
            {
                case 0: g.Grid.Remove(b); break;                                   // a hole: networks may split, machines lose power
                case 1: g.Grid.Remove(b); g.Grid.Place(def, o, r); break;          // rebuilt: newest id
                default:
                    var c = new Cell(o.X + (t % 7) - 3, o.Y + (t % 5) - 2);
                    if (g.CanPlace(BuildingDatabase.Get("pole"), c, Rotation.R0)) g.Grid.Place(BuildingDatabase.Get("pole"), c, Rotation.R0);   // may join networks
                    break;
            }
        }

        static int[] Membership(GameState g)
        {
            var ids = new List<int>();
            foreach (var b in g.Grid.Buildings) { ids.Add(b.InstanceId); ids.Add(g.Factory.NetworkIdOf(b)); }
            return ids.ToArray();
        }

        [Test]
        public void KeepingNetworksUpToDate_GivesTheSameNetworks_AsBuildingThemFromScratch()
        {
            var g = PerformanceTests.BigBase(1500);
            for (int t = 1; t <= 400; t++)
            {
                Edit(g, t);
                if (t % 25 != 0) continue;
                var kept = Membership(g);
                g.Factory.RebuildPowerNetworks();
                CollectionAssert.AreEqual(kept, Membership(g), $"after edit {t}");
            }
        }

        [Test]
        public void KeepingNetworksUpToDate_GivesTheSameWorld_AsRebuildingEveryTick()
        {
            var a = PerformanceTests.BigBase(1200);
            var b = PerformanceTests.BigBase(1200);
            for (int t = 1; t <= 300; t++)
            {
                if (t % 3 == 0) { Edit(a, t); Edit(b, t); }
                b.Factory.RebuildPowerNetworks();
                a.Step(); b.Step();
                if (t % 50 == 0) Assert.AreEqual(Simulation.StateHash(b), Simulation.StateHash(a), $"diverged by tick {t}");
            }
        }

        [Test]
        public void ComputersTurretsServersAndHeaters_GetPowerFirst()
        {
            var (g, x, y) = World(12, 6);
            P(g, "solar", x, y);                                   // 40 kW in full sun
            g.Factory.SolarFactor = 1f;
            P(g, "pole", x + 3, y);
            var pc = (ComputerState)P(g, "computer", x + 4, y).State;   // critical, 20 kW
            pc.Running = true;
            var pumps = new List<PumpState>();
            for (int i = 0; i < 3; i++) { var p = (PumpState)P(g, "pump", x + 5 + i, y + 2).State; p.Running = true; pumps.Add(p); }   // normal, 3 x 20 kW
            Run(g, 0.1f);
            Assert.AreEqual(1f, pc.PowerSatisfaction, 1e-4f, "the computer gets all it needs");
            foreach (var p in pumps) Assert.AreEqual(20f / 60f, p.PowerSatisfaction, 1e-4f, "the pumps share what is left");
            var st = g.Factory.NetworkDetails(g.Grid.At(new Cell(x + 4, y)));
            Assert.AreEqual(20f, st.CriticalDemandKW, 1e-3f); Assert.AreEqual(60f, st.NormalDemandKW, 1e-3f);
            Assert.AreEqual(40f, st.SolarKW, 1e-3f);
            // a second panel covers everything
            P(g, "solar", x, y + 2);
            Run(g, 0.1f);
            foreach (var p in pumps) Assert.AreEqual(1f, p.PowerSatisfaction, 1e-4f);
        }

        [Test]
        public void Statistics_ShowEachSource_InSupplyOrder()
        {
            var (g, x, y) = World(12, 6);
            g.Factory.SolarFactor = 0.5f;                          // 20 kW of sun
            P(g, "solar", x, y);
            var gen = P(g, "generator", x + 2, y); gen.State.TryInsert(ItemDatabase.Coal, 5);
            var bat = (BatteryState)P(g, "battery", x + 4, y).State;
            bat.StoredKJ = 500f;
            P(g, "pole", x + 5, y);
            for (int i = 0; i < 3; i++) ((ComputerState)P(g, "computer", x + 6 + i, y).State).Running = true;   // 60 kW
            Run(g, 0.05f);
            var st = g.Factory.NetworkDetails(gen);
            Assert.AreEqual(20f, st.SolarKW, 1e-3f, "sun first");
            Assert.AreEqual(40f, st.CoalKW, 1e-3f, "coal for the rest");
            Assert.AreEqual(0f, st.BatteryKW, 1e-3f, "batteries last");
            Assert.AreEqual(Factory.GeneratorKW + 20f + BatteryState.MaxKW, st.CapacityKW, 1e-3f);
            Assert.AreEqual(500f, st.StoredKJ, 1e-2f); Assert.AreEqual(BatteryState.CapacityKJ, st.StorageKJ, 1e-3f);
            Assert.AreEqual(4, st.Nodes); Assert.AreEqual(3, st.Consumers);
            Assert.AreEqual(g.Grid.At(new Cell(x, y)).InstanceId, st.Network, "a network is named after its oldest node");
            // no coal: the batteries cover the gap
            g.Grid.Remove(gen);
            Run(g, 0.05f);
            st = g.Factory.NetworkDetails(g.Grid.At(new Cell(x + 5, y)));
            Assert.AreEqual(40f, st.BatteryKW, 1e-3f);
            Assert.AreEqual(0f, st.CoalKW, 1e-3f);
        }

        [Test]
        public void RemovingAPole_SplitsTheNetwork_AndAddingItBackJoinsIt()
        {
            var (g, x, y) = World(30, 2);
            var left = P(g, "pole", x, y); var mid = P(g, "pole", x + 8, y); var right = P(g, "pole", x + 16, y);
            var pcL = P(g, "computer", x + 1, y); var pcR = P(g, "computer", x + 17, y);
            Assert.AreEqual(1, g.Factory.NetworkCount);
            Assert.AreEqual(g.Factory.NetworkIdOf(pcL), g.Factory.NetworkIdOf(pcR));
            g.Grid.Remove(mid);
            Assert.AreEqual(2, g.Factory.NetworkCount, "the gap is too wide");
            Assert.AreEqual(left.InstanceId, g.Factory.NetworkIdOf(pcL));
            Assert.AreEqual(right.InstanceId, g.Factory.NetworkIdOf(pcR));
            P(g, "pole", x + 8, y);
            Assert.AreEqual(1, g.Factory.NetworkCount);
            Assert.AreEqual(left.InstanceId, g.Factory.NetworkIdOf(pcR), "joined again, named after the oldest pole");
        }

        [Test]
        public void EditingTheMiddleOfABigNetwork_StaysCheap()
        {
            var g = PerformanceTests.BigBase(2000);
            for (int i = 0; i < 20; i++) g.Factory.Tick(0.05f);
            var poles = new List<PlacedBuilding>();
            foreach (var b in g.Grid.Buildings) if (b.Def.Id == "pole") poles.Add(b);
            var sw = Stopwatch.StartNew(); int n = 0;
            for (int i = 0; i < poles.Count && n < 20; i += poles.Count / 20 + 1)
            {
                var o = poles[i].Origin;
                g.Grid.Remove(poles[i]); g.Factory.Tick(0.05f);
                g.Grid.Place(BuildingDatabase.Get("pole"), o, Rotation.R0); g.Factory.Tick(0.05f);
                n++;
            }
            double ms = sw.Elapsed.TotalMilliseconds / (n * 2);
            TestContext.WriteLine($"[perf] tick after removing/adding a pole inside the network: {ms:0.000} ms");
            Assert.Less(ms, 15.0);   // ~3 ms measured (the old full rebuild alone was ~4.7 ms, on every edit)
        }
    }
}
