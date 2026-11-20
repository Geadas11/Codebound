using System.Collections.Generic;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class CoreTests
    {
        [Test]
        public void Direction_RotatesClockwiseAndBack()
        {
            Assert.AreEqual(Direction.East, Direction.North.RotateClockwise());
            Assert.AreEqual(Direction.North, Direction.West.RotateClockwise());
            Assert.AreEqual(Direction.West, Direction.North.RotateCounterClockwise());
            Assert.AreEqual(new GridPos(0, -1), Direction.South.ToOffset());
        }

        [Test]
        public void GridPos_FromWorld_FloorsNegatives()
        {
            Assert.AreEqual(new GridPos(-1, 2), GridPos.FromWorld(-0.2f, 2.9f));
        }

        [Test]
        public void SimulationClock_RunsWholeTicksAndKeepsRemainder()
        {
            var clock = new SimulationClock(20);
            var seen = new List<long>();
            clock.Ticked += t => seen.Add(t);
            Assert.AreEqual(1, clock.Advance(0.07));   // 1.4 ticks
            Assert.AreEqual(1, clock.Advance(0.03));   // remainder 0.4 + 0.6 = 1 tick
            CollectionAssert.AreEqual(new long[] { 1, 2 }, seen);
            clock.Paused = true;
            Assert.AreEqual(0, clock.Advance(1));
        }

        [Test]
        public void SimulationClock_CapsTicksAfterHitch()
        {
            var clock = new SimulationClock(20);
            Assert.AreEqual(SimulationClock.MaxTicksPerAdvance, clock.Advance(10));
            Assert.AreEqual(0, clock.Advance(0.01)); // backlog dropped
        }

        [Test]
        public void EventBus_DeliversAndSurvivesThrowingHandler()
        {
            var bus = new EventBus();
            var oldSink = CbLog.Sink;
            CbLog.Sink = (l, m, s) => { };
            try
            {
                int got = 0;
                bus.Subscribe<int>(_ => throw new System.Exception("boom"));
                bus.Subscribe<int>(v => got = v);
                bus.Publish(42);
                Assert.AreEqual(42, got);
            }
            finally { CbLog.Sink = oldSink; }
        }

        [Test]
        public void SeededRandom_IsDeterministic()
        {
            var a = new SeededRandom(123);
            var b = new SeededRandom(123);
            for (int i = 0; i < 100; i++) Assert.AreEqual(a.NextULong(), b.NextULong());
            Assert.AreEqual(SeededRandom.Hash01(5, 10, -3), SeededRandom.Hash01(5, 10, -3));
        }

        [Test]
        public void Loc_FallsBackToEnglishThenKey()
        {
            Loc.Register("en", new Dictionary<string, string> { { "test.only_en", "Hello" } });
            Loc.Register("pt-PT", new Dictionary<string, string> { { "test.both", "Olá" } });
            Loc.SetLanguage("pt-PT");
            Assert.AreEqual("Olá", Loc.Get("test.both"));
            Assert.AreEqual("Hello", Loc.Get("test.only_en"));
            Assert.AreEqual("test.missing", Loc.Get("test.missing"));
        }
    }
}
