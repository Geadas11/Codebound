using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 1 (industrial simulation): the fixed tick is ordered, deterministic and survives save/load.</summary>
    public class DeterminismTests
    {
        /// <summary>A small working factory: coal power with poles and running computers, solar + battery, a closed belt
        /// loop with items going round, a chest feeding the loop through an inserter, and a drill a "program" keeps mining.</summary>
        static (GameState g, PlacedBuilding drill) Factory(int seed = 5)
        {
            var g = new GameState(seed);
            var s = g.World.Generator.FindSpawn();
            Cell? Free(int w, int h)
            {
                for (int r = 2; r < 120; r++)
                    for (int x = -r; x <= r; x++)
                        foreach (int y in new[] { -r, r })
                        {
                            bool ok = true;
                            for (int dx = 0; dx < w && ok; dx++) for (int dy = 0; dy < h && ok; dy++) ok = g.CanPlace(BuildingDatabase.Get("belt"), new Cell(s.X + x + dx, s.Y + y + dy), Rotation.R0);
                            if (ok) return new Cell(s.X + x, s.Y + y);
                        }
                return null;
            }
            PlacedBuilding P(string id, int x, int y, Rotation r = Rotation.R0) => g.Grid.Place(BuildingDatabase.Get(id), new Cell(x, y), r) ?? throw new AssertionException($"cannot place {id} at {x},{y}");

            var o = Free(15, 8) ?? throw new AssertionException("no room");
            int ox = o.X, oy = o.Y;
            // power: two generators, poles, three running computers, solar + battery
            P("generator", ox, oy).State.TryInsert(ItemDatabase.Coal, 20);
            P("generator", ox, oy + 2).State.TryInsert(ItemDatabase.Coal, 20);
            P("pole", ox + 3, oy + 1);
            for (int i = 0; i < 3; i++) ((ComputerState)P("computer", ox + 4 + i, oy).State).Running = true;
            P("solar", ox + 4, oy + 3); P("battery", ox + 6, oy + 3);
            // a 4x4 ring of belts (clockwise) with items on it, fed from a chest by an inserter
            int bx = ox + 10, by = oy;
            for (int i = 0; i < 3; i++) P("belt", bx, by + i, Rotation.R0);            // left column, up
            for (int i = 0; i < 3; i++) P("belt", bx + i, by + 3, Rotation.R90);       // top row, right
            for (int i = 0; i < 3; i++) P("belt", bx + 3, by + 3 - i, Rotation.R180);  // right column, down
            for (int i = 0; i < 3; i++) P("belt", bx + 3 - i, by, Rotation.R270);      // bottom row, left
            int k = 0; foreach (var b in g.Grid.Buildings) if (b.State is ConveyorState c && k++ % 3 == 0) c.TryInsert(ItemDatabase.IronOre, 1);
            var chest = P("chest", bx - 2, by + 1); chest.State.TryInsert(ItemDatabase.Stone, 30);
            P("inserter", bx - 1, by + 1, Rotation.R90);   // takes from the chest behind it, drops on the belt in front
            // a drill on ore, powered, pushing into a chest
            PlacedBuilding drill = null;
            for (int x = -150; x < 150 && drill == null; x++)
                for (int y = -150; y < 150 && drill == null; y++)
                {
                    var c = new Cell(x, y); var d = g.World.DepositAt(c);
                    if (!WorldRules.DrillCanMine(d) || d.Amount < 30 || !g.CanPlace(BuildingDatabase.Get("drill"), c, Rotation.R0)
                        || !g.CanPlace(BuildingDatabase.Get("generator"), new Cell(x - 3, y), Rotation.R0) || !g.CanPlace(BuildingDatabase.Get("chest"), new Cell(x, y + 2), Rotation.R0)) continue;
                    drill = g.Grid.Place(BuildingDatabase.Get("drill"), c, Rotation.R0);
                    g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(x - 3, y), Rotation.R0).State.TryInsert(ItemDatabase.Coal, 20);
                    g.Grid.Place(BuildingDatabase.Get("chest"), new Cell(x, y + 2), Rotation.R0);
                }
            Assert.NotNull(drill, "no ore for the drill");
            return (g, drill);
        }

        /// <summary>The "program": restart mining whenever the drill is idle (inputs are part of the deterministic run).</summary>
        static void Drive(GameState g, PlacedBuilding drill) { if (!g.Factory.HasAction(drill)) g.Factory.StartMine(drill, null); }

        static List<ulong> Run(GameState g, PlacedBuilding drill, int ticks, int every)
        {
            var hashes = new List<ulong>();
            for (int t = 1; t <= ticks; t++) { Drive(g, drill); g.Step(); if (t % every == 0) hashes.Add(Simulation.StateHash(g)); }
            return hashes;
        }

        [Test]
        public void SameInputs_GiveTheSameWorld_TickByTick()
        {
            var (a, da) = Factory(); var (b, db) = Factory();
            Assert.AreEqual(Simulation.StateHash(a), Simulation.StateHash(b), "same start");
            ulong start = Simulation.StateHash(a);
            var ha = Run(a, da, 600, 50); var hb = Run(b, db, 600, 50);
            CollectionAssert.AreEqual(ha, hb, "two runs diverged");
            Assert.AreNotEqual(start, ha.Last(), "the factory must actually have changed");
            Assert.AreEqual(600, a.Tick);
            Assert.Greater(((StorageState)a.Grid.At(new Cell(da.Origin.X, da.Origin.Y + 2)).State).Items.Count(WorldRules.ItemFor(a.World.DepositAt(da.Origin).Type)), 5, "the drill mined into its chest");
        }

        [Test]
        public void SavingAndLoading_MidRun_ContinuesExactlyLikeNotSaving()
        {
            var (a, da) = Factory();
            Run(a, da, 287, 1000);
            // the timed action the "program" started is in progress; finish it so both worlds restart it on the same tick
            while (a.Factory.HasAction(da)) a.Step();
            var w = new StringWriter(); SaveSystem.Write(a, w);
            var b = SaveSystem.Read(new StringReader(w.ToString()));
            var db = b.Grid.At(da.Origin);
            Assert.AreEqual(Simulation.StateHash(a), Simulation.StateHash(b), "the reloaded world equals the saved one");
            CollectionAssert.AreEqual(Run(a, da, 400, 25), Run(b, db, 400, 25), "continuing after a reload diverged");
        }

        [Test]
        public void TickOrder_IsExplicit()
        {
            var g = new GameState(1);
            CollectionAssert.AreEqual(new[] { "clock & weather", "power", "machine actions", "outputs", "signals", "belts", "inserters", "fluids", "greenhouses", "launch pads", "storage hubs", "robots", "trains", "creatures", "ecology", "lightning" },
                g.Simulation.Systems.Select(s => s.Name).ToArray());
        }

        [Test]
        public void Clock_AdvancesPerTick_AndTheTickIsSaved()
        {
            var g = new GameState(2);
            double t0 = g.Clock.Seconds;
            for (int i = 0; i < 40; i++) g.Step();
            Assert.AreEqual(2.0, g.Clock.Seconds - t0, 1e-4, "40 ticks = 2 s of game time, whatever the frame rate");
            var w = new StringWriter(); SaveSystem.Write(g, w);
            Assert.AreEqual(40, SaveSystem.Read(new StringReader(w.ToString())).Tick);
            string old = string.Join("\n", w.ToString().Split('\n').Where(l => !l.StartsWith("tick ")));
            Assert.AreEqual(0, SaveSystem.Read(new StringReader(old)).Tick, "saves from before the tick counter load at tick 0");
        }

        [Test]
        public void Buildings_AreAlwaysInBuildOrder_EvenAfterRemovals()
        {
            var (g, _) = Factory();
            var list = g.Grid.Buildings.ToList();
            g.Grid.Remove(list[3]); g.Grid.Remove(list[7]);
            var s = g.World.Generator.FindSpawn();
            for (int x = 0; x < 200; x++) if (g.Grid.Place(BuildingDatabase.Get("wall"), new Cell(s.X + x, s.Y - 30), Rotation.R0) != null) break;
            var ids = g.Grid.Buildings.Select(b => b.InstanceId).ToList();
            CollectionAssert.AreEqual(ids.OrderBy(i => i).ToList(), ids, "a new building comes last, not in a freed slot");
        }
    }
}
