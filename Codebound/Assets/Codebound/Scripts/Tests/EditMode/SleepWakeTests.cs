using System.Collections.Generic;
using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Sleep/wake must be invisible: the same world with sleeping on and off gives the same state every tick.</summary>
    public class SleepWakeTests
    {
        /// <summary>Things players and programs do to a factory while it runs: change filters, stop and start arms,
        /// demolish and rebuild, add and take items. Deterministic from the tick number.</summary>
        static void Interfere(GameState g, int t)
        {
            var all = g.Grid.Buildings;
            if (all.Count == 0 || t % 7 != 0) return;
            var b = all[(t * 7919) % all.Count];
            switch ((t / 7) % 6)
            {
                case 0: if (b.State is ConveyorState c) c.Filter = c.Filter == null ? ItemDatabase.IronOre : null; break;
                case 1: if (b.State is InserterState i) i.Running = !i.Running; break;
                case 2: if (b.State is StorageState st) st.Items.TryRemove(ItemDatabase.Stone, 5); break;
                case 3: b.State?.TryInsert(ItemDatabase.IronOre, 1); break;
                case 4:
                {
                    var def = b.Def; var o = b.Origin; var r = b.Rotation;
                    g.Grid.Remove(b);
                    g.Grid.Place(def, o, r);   // rebuilt: new id, last in order, fresh state
                    break;
                }
                default: if (b.State is ConveyorState cv) cv.Running = !cv.Running; break;
            }
        }

        static List<ulong> Trace(bool sleep, int buildings, int ticks)
        {
            var g = PerformanceTests.BigBase(buildings);
            g.Factory.SleepEnabled = sleep;
            var h = new List<ulong>();
            for (int t = 1; t <= ticks; t++)
            {
                Interfere(g, t);
                g.Step();
                if (t % 20 == 0) h.Add(Simulation.StateHash(g));
            }
            return h;
        }

        [Test]
        public void SleepingMachines_GiveExactlyTheSameWorld_AsRetryingEveryTick()
        {
            var awake = Trace(false, 1500, 600);
            var sleepy = Trace(true, 1500, 600);
            Assert.AreEqual(awake.Count, sleepy.Count);
            for (int i = 0; i < awake.Count; i++) Assert.AreEqual(awake[i], sleepy[i], $"diverged by tick {(i + 1) * 20}");
        }

        [Test]
        public void IdleMachines_Sleep_AndWakeWhenTheirWayClears()
        {
            var g = new GameState(9);
            var s = g.World.Generator.FindSpawn();
            Cell o = default; bool found = false;
            for (int r = 2; r < 80 && !found; r++)
                for (int x = -r; x <= r && !found; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r); bool ok = true;
                    for (int dy = 0; dy < 4 && ok; dy++) ok = g.CanPlace(BuildingDatabase.Get("belt"), new Cell(c.X, c.Y + dy), Rotation.R0);
                    if (ok) { o = c; found = true; }
                }
            Assert.IsTrue(found);
            var belt = g.Grid.Place(BuildingDatabase.Get("belt"), o, Rotation.R0);   // a belt pointing at nothing
            belt.State.TryInsert(ItemDatabase.Stone, 1);
            for (int i = 0; i < 20; i++) g.Step();
            Assert.IsTrue(belt.State.Asleep, "blocked at the end of the line: asleep");
            // an arm takes its item and another arrives: its own contents changed, so it must wake (found by the 8000-building probe)
            var c0 = (ConveyorState)belt.State;
            c0.Slot.TryRemove(ItemDatabase.Stone, 1);
            for (int i = 0; i < 5; i++) g.Step();
            Assert.IsTrue(c0.TryInsert(ItemDatabase.Stone, 1));
            Assert.IsFalse(c0.Asleep, "a new item on a sleeping belt wakes it");
            for (int i = 0; i < 20; i++) g.Step();
            Assert.IsTrue(c0.Asleep, "and it sleeps again once blocked");
            var chest = g.Grid.Place(BuildingDatabase.Get("chest"), new Cell(o.X, o.Y + 1), Rotation.R0);
            Assert.IsFalse(belt.State.Asleep, "a chest appeared in front: awake");
            g.Step();
            Assert.AreEqual(1, ((StorageState)chest.State).Items.Count(ItemDatabase.Stone), "and the item went in");
        }
    }
}
