using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 5 of the industrial simulation: signal cables, combinators, conditions, lamps.</summary>
    public class SignalTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 61)
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

        static void Tick(GameState g, int n = 1) { for (int i = 0; i < n; i++) g.Factory.Tick(0.05f); }

        static int Sig(GameState g, int x, int y, string name) => g.Factory.SignalsAt(g.Grid.At(new Cell(x, y))).TryGetValue(name, out var v) ? v : 0;

        [Test]
        public void Settings_AreParsedStrictly_AndWrittenCanonically()
        {
            Assert.IsNull(SignalSpec.ParseCondition(" iron_plate < 100 ", out var c)); Assert.AreEqual("iron_plate<100", c.Text);
            Assert.IsNull(SignalSpec.ParseCondition("A>=B", out c)); Assert.AreEqual("A>=B", c.Text);
            Assert.IsNull(SignalSpec.ParseCondition("fluid_water!=-5", out c)); Assert.AreEqual("fluid_water!=-5", c.Text);
            Assert.IsNull(SignalSpec.ParseCondition("", out c)); Assert.IsNull(c, "empty = no condition");
            Assert.IsNotNull(SignalSpec.ParseCondition("gold<3", out _), "unknown signal");
            Assert.IsNotNull(SignalSpec.ParseCondition("A<<3", out _));
            Assert.IsNotNull(SignalSpec.ParseCondition("A", out _));
            var d = new Dictionary<string, int>();
            Assert.IsNull(SignalSpec.ParsePairs("B=2, A=-5", d)); Assert.AreEqual("A=-5,B=2", SignalSpec.FormatPairs(d));
            Assert.IsNotNull(SignalSpec.ParsePairs("A=x", d)); Assert.IsNotNull(SignalSpec.ParsePairs("a=1", d), "lower-case single letters are not signals");
            var ar = new CombinatorState(BuildingKind.ArithmeticCombinator);
            Assert.IsNull(ar.Configure("iron_plate / 10 -> A")); Assert.AreEqual("iron_plate/10->A", ar.Config);
            Assert.IsNull(ar.Configure("EACH*-2->EACH")); Assert.AreEqual("EACH*-2->EACH", ar.Config);
            Assert.IsNotNull(ar.Configure("A*2->EACH"), "EACH out needs EACH in");
            Assert.AreEqual("EACH*-2->EACH", ar.Config, "a bad setting keeps the old one");
            var de = new CombinatorState(BuildingKind.DeciderCombinator);
            Assert.IsNull(de.Configure("A>5->B:input")); Assert.AreEqual("A>5->B:input", de.Config);
            Assert.IsNotNull(de.Configure(new string('A', 300)), "too long");
            Assert.AreEqual(0, SignalSpec.Apply("/", 7, 0), "x/0 = 0, no exception");
            Assert.AreEqual(int.MinValue, SignalSpec.Apply("/", int.MinValue, -1), "wraps instead of throwing");
            Assert.AreEqual(int.MinValue, SignalSpec.Apply("+", int.MaxValue, 1));
        }

        [Test]
        public void ChestContents_LightALamp_WithACondition()
        {
            var (g, x, y) = World(6, 3);
            var chest = (StorageState)P(g, "chest", x, y).State;
            P(g, "cable", x + 1, y); P(g, "cable", x + 2, y);
            var lamp = (LampState)P(g, "lamp", x + 3, y).State;
            P(g, "solar", x + 4, y); g.Factory.SolarFactor = 1f;
            Assert.IsNull(Factory.Configure(g.Grid.At(new Cell(x + 3, y)), "iron_plate>5"));
            Tick(g, 2);
            Assert.IsFalse(lamp.Lit, "empty chest: off");
            Assert.AreEqual(0f, g.Factory.NetworkStats(g.Grid.At(new Cell(x + 3, y))).demandKW, 1e-3f, "an off lamp uses nothing");
            chest.TryInsert(ItemDatabase.IronPlate, 6);
            Tick(g, 2);
            Assert.AreEqual(6, Sig(g, x + 1, y, ItemDatabase.IronPlate), "the cable carries the chest's contents");
            Assert.IsTrue(lamp.Lit);
            Assert.AreEqual("lit", lamp.Status);
        }

        [Test]
        public void AnArm_WorksOnlyWhileItsConditionHolds()
        {
            var (g, x, y) = World(3, 3);
            var a = (StorageState)P(g, "chest", x, y).State;
            var arm = (InserterState)P(g, "inserter", x, y + 1, Rotation.R0).State;   // from a (below) to b (above)
            var b = (StorageState)P(g, "chest", x, y + 2).State;
            P(g, "cable", x + 1, y + 1); P(g, "cable", x + 1, y + 2);                  // touches the arm and chest b only
            arm.SetCondition("iron_ore<3");
            a.TryInsert(ItemDatabase.IronOre, 10);
            Tick(g, 200);
            Assert.AreEqual(3, b.Items.Count(ItemDatabase.IronOre), "stops once b holds 3");
            Assert.AreEqual("disabled", arm.Status);
            Assert.IsFalse(arm.Asleep, "a switched arm keeps checking its signals");
            b.Items.TryRemove(ItemDatabase.IronOre, 2);
            Tick(g, 60);
            Assert.AreEqual(3, b.Items.Count(ItemDatabase.IronOre), "topped up again");
            arm.SetCondition("");
            Tick(g, 200);
            Assert.AreEqual(10 - 2, b.Items.Count(ItemDatabase.IronOre), "no condition: moves everything");
        }

        [Test]
        public void Combinators_Chain_OneTickPerStep()
        {
            var (g, x, y) = World(7, 1);
            Assert.IsNull(Factory.Configure(P(g, "constant_combinator", x, y), "A=5,B=1"));
            P(g, "cable", x + 1, y);
            Assert.IsNull(Factory.Configure(P(g, "arithmetic_combinator", x + 2, y, Rotation.R90), "A*3->B"));   // reads x+1, writes x+3
            P(g, "cable", x + 3, y);
            Assert.IsNull(Factory.Configure(P(g, "decider_combinator", x + 4, y, Rotation.R90), "B>10->B:input"));
            P(g, "cable", x + 5, y);
            Tick(g); Assert.AreEqual(5, Sig(g, x + 1, y, "A")); Assert.AreEqual(0, Sig(g, x + 3, y, "B"));
            Tick(g); Assert.AreEqual(15, Sig(g, x + 3, y, "B")); Assert.AreEqual(0, Sig(g, x + 5, y, "C"));
            Tick(g); Assert.AreEqual(15, Sig(g, x + 5, y, "B"), "the decider passes B's input value on");
            Assert.AreEqual(0, Sig(g, x + 5, y, "A"), "and nothing else");
        }

        [Test]
        public void AFeedbackLoop_CountsOnePerTick_AndSurvivesSaveAndLoad()
        {
            var (g, x, y) = World(5, 3);
            // an arithmetic combinator whose input and output are the same network: A+1 -> A counts ticks
            Assert.IsNull(Factory.Configure(P(g, "arithmetic_combinator", x + 1, y, Rotation.R90), "A+1->A"));
            P(g, "cable", x, y); P(g, "cable", x, y + 1); P(g, "cable", x + 1, y + 1); P(g, "cable", x + 2, y + 1); P(g, "cable", x + 2, y);
            var pc = (ComputerState)P(g, "computer", x + 3, y).State;   // next to the cable at x+2: adds 1000 to the count
            Tick(g, 10);
            Assert.AreEqual(10, Sig(g, x, y, "A"), "one per tick");
            pc.SignalOut["A"] = 1000;
            Tick(g, 1);
            Assert.AreEqual(1011, Sig(g, x, y, "A"), "the computer's value is added to the network");
            pc.SignalOut.Clear();
            var a = SaveSystem.Read(new StringReader(Save(g)));
            Tick(g, 20); Tick(a, 20);
            Assert.AreEqual(Sig(g, x, y, "A"), Sig(a, x, y, "A"), "the loaded world counts on from the same value");
            Assert.AreEqual(Save(g), Save(a));
        }

        static string Save(GameState g) { var w = new StringWriter(); SaveSystem.Write(g, w); return w.ToString(); }

        [Test]
        public void TanksSendTheirLevel_EachSegmentOnce()
        {
            var (g, x, y) = World(5, 3);
            var tank = (PipeState)P(g, "tank", x, y).State;
            tank.LoadExtra("fluid", Fluids.Water); tank.LoadExtra("amount", "1234.9");
            P(g, "cable", x + 2, y); P(g, "cable", x + 2, y + 1);   // touches the tank twice
            Tick(g, 2);
            Assert.AreEqual(1234, Sig(g, x + 2, y, "fluid_water"));
        }

        [Test]
        public void Conditions_AndNetworkValues_AreSaved_AndWorldsStayIdentical()
        {
            GameState Build()
            {
                var (g, x, y) = World(4, 3, 63);
                var a = (StorageState)P(g, "chest", x, y).State;
                var arm = (InserterState)P(g, "inserter", x, y + 1, Rotation.R0).State;
                P(g, "chest", x, y + 2);
                P(g, "cable", x + 1, y + 1); P(g, "cable", x + 1, y + 2);
                arm.SetCondition("iron_ore<4");
                a.TryInsert(ItemDatabase.IronOre, 20);
                return g;
            }
            var w1 = Build(); var w2 = Build();
            for (int i = 0; i < 23; i++) { w1.Step(); w2.Step(); }
            var text = Save(w2);
            StringAssert.Contains("bx cond iron_ore<4", text);
            var w3 = SaveSystem.Read(new StringReader(text));
            for (int i = 0; i < 120; i++) { w1.Step(); w2.Step(); w3.Step(); }
            Assert.AreEqual(Simulation.StateHash(w1), Simulation.StateHash(w2));
            Assert.AreEqual(Simulation.StateHash(w1), Simulation.StateHash(w3), "saving and loading mid-run changes nothing");
        }
    }
}
