using System.Diagnostics;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 8: a big base must simulate well inside the 20 Hz tick (50 ms) — budget here: 2.5 ms per tick (was 5 ms before the spatial lookups), 10 ms after a power edit (was 28).</summary>
    public class PerformanceTests
    {
        /// <summary>~2000 buildings: power poles, generators, belts with items, inserters, chests, furnaces, assemblers, hubs, computers.</summary>
        public static GameState BigBase(int target = 2000)
        {
            var g = new GameState(21);
            var s = g.World.Generator.FindSpawn();
            string[] pattern = { "belt", "belt", "belt", "inserter", "chest", "pole", "furnace", "assembler", "generator", "belt", "storage_hub", "computer", "lab", "greenhouse", "solar", "battery", "turret", "server" };
            int placed = 0, k = 0;
            for (int r = 2; r < 400 && placed < target; r++)
                for (int x = -r; x <= r && placed < target; x += 1)
                    foreach (int y in new[] { -r, r })
                    {
                        if (placed >= target) break;
                        var def = BuildingDatabase.Get(pattern[k % pattern.Length]);
                        var c = new Cell(s.X + x, s.Y + y);
                        if (!g.CanPlace(def, c, (Rotation)(k % 4))) continue;
                        var b = g.Grid.Place(def, c, (Rotation)(k % 4));
                        if (b == null) continue;
                        k++; placed++;
                        if (b.State is GeneratorState) b.State.TryInsert(ItemDatabase.Coal, 50);
                        if (b.State is ConveyorState) b.State.TryInsert(ItemDatabase.IronOre, 1);
                        if (b.State is StorageState) b.State.TryInsert(ItemDatabase.Stone, 20);
                    }
            return g;
        }

        static double MsPerTick(GameState g, int ticks)
        {
            for (int i = 0; i < 20; i++) Tick(g);   // warm up (first power rebuild, JIT)
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < ticks; i++) Tick(g);
            return sw.Elapsed.TotalMilliseconds / ticks;
        }

        static void Tick(GameState g)
        {
            g.Factory.Tick(0.05f);
            g.Creatures.Tick(0.05f, g.PlayerX, g.PlayerY);
            g.Ecology.Tick(0.05f);
        }

        [Test]
        public void BigBase_TicksWellInsideTheBudget()
        {
            var g = BigBase();
            int n = 0; foreach (var _ in g.Grid.Buildings) n++;
            Assert.GreaterOrEqual(n, 1800, "the benchmark base was built");
            double ms = MsPerTick(g, 200);
            TestContext.WriteLine($"[perf] {n} buildings: {ms:0.000} ms per tick");
            Assert.Less(ms, 2.5, $"{n} buildings took {ms:0.00} ms per tick");
        }

        [Test]
        public void Building_InABigBase_DoesNotStall()
        {
            var g = BigBase();
            MsPerTick(g, 5);
            var s = g.World.Generator.FindSpawn();
            var sw = Stopwatch.StartNew();
            int edits = 0;
            for (int x = 0; x < 400 && edits < 20; x++)   // place and remove poles: each one changes the power networks
            {
                var c = new Cell(s.X + x, s.Y - 420);
                if (!g.CanPlace(BuildingDatabase.Get("pole"), c, Rotation.R0)) continue;
                var p = g.Grid.Place(BuildingDatabase.Get("pole"), c, Rotation.R0);
                Tick(g);
                g.Grid.Remove(p);
                Tick(g);
                edits++;
            }
            double ms = sw.Elapsed.TotalMilliseconds / (edits * 2);
            TestContext.WriteLine($"[perf] tick after a power edit: {ms:0.000} ms");
            Assert.Less(ms, 10.0);
        }
    }
}
