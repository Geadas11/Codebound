using System.IO;
using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 3 of the industrial simulation: pipes as segments, pumps, offshore pumps, boilers, steam engines, oil.</summary>
    public class FluidTests
    {
        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds - 1e-4f; t += 0.05f) g.Factory.Tick(0.05f); }

        static (GameState g, int x, int y) World(int w, int h, int seed = 41)
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

        static PipeState Fill(PlacedBuilding pipe, string fluid, float amount)   // as a save would (before the first tick)
        {
            var ps = (PipeState)pipe.State;
            ps.LoadExtra("fluid", fluid); ps.LoadExtra("amount", amount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return ps;
        }

        static void Power(GameState g, int x, int y)   // a fuelled coal generator with a pole at (x, y)
        {
            P(g, "generator", x, y).State.TryInsert(ItemDatabase.Coal, 50);
            P(g, "pole", x + 2, y);
        }

        [Test]
        public void Pipes_ShareOneSegment_SplitAndRejoin()
        {
            var (g, x, y) = World(5, 1);
            var pipes = Enumerable.Range(0, 5).Select(i => P(g, "pipe", x + i, y)).ToArray();
            Fill(pipes[0], Fluids.Water, 300);
            Run(g, 0.05f);
            var p0 = (PipeState)pipes[0].State;
            Assert.AreEqual(300f, p0.SegmentAmount, 1e-3f, "one segment holds it all");
            Assert.AreEqual(500f, p0.SegmentCapacity, 1e-3f);
            Assert.AreEqual(60f, ((PipeState)pipes[4].State).Amount, 1e-3f, "each pipe holds its share");

            g.Grid.Remove(pipes[2]);   // its 60 units go with it
            Run(g, 0.05f);
            Assert.AreEqual(120f, p0.SegmentAmount, 1e-3f);
            Assert.AreEqual(120f, ((PipeState)pipes[4].State).SegmentAmount, 1e-3f);
            Assert.AreEqual(200f, p0.SegmentCapacity, 1e-3f, "two segments of two pipes");

            P(g, "pipe", x + 2, y);
            Run(g, 0.05f);
            Assert.AreEqual(240f, p0.SegmentAmount, 1e-3f, "rejoined");
            CollectionAssert.AreEqual(new[] { (Fluids.Water, 240f) }, p0.FluidContents.Select(f => (f.Item1, (float)System.Math.Round(f.Item2, 3))).ToArray());
        }

        [Test]
        public void APipe_ThatWouldMixTwoFluids_CannotBePlaced()
        {
            var (g, x, y) = World(5, 2);
            Fill(P(g, "pipe", x, y), Fluids.Water, 50);
            Fill(P(g, "pipe", x + 2, y), Fluids.Steam, 50);
            Run(g, 0.05f);
            Assert.IsFalse(g.CanPlace(BuildingDatabase.Get("pipe"), new Cell(x + 1, y), Rotation.R0), "water and steam would meet");
            Assert.IsTrue(g.CanPlace(BuildingDatabase.Get("pipe"), new Cell(x + 1, y + 1), Rotation.R0), "next to one fluid only is fine");
            Assert.IsTrue(g.CanPlace(BuildingDatabase.Get("wall"), new Cell(x + 1, y), Rotation.R0), "other buildings are not affected");
        }

        [Test]
        public void Pump_MovesFluidForward_OnlyWhenStartedAndPowered()
        {
            var (g, x, y) = World(8, 6);
            var src = Fill(P(g, "pipe", x, y), Fluids.Water, 100);
            var pump = (PumpState)P(g, "pump", x + 1, y, Rotation.R90).State;   // facing right: from x to x+2
            P(g, "pipe", x + 2, y); P(g, "pipe", x + 3, y);
            var dst = (PipeState)g.Grid.At(new Cell(x + 2, y)).State;
            Run(g, 1f);
            Assert.AreEqual(100f, src.SegmentAmount, 1e-3f, "pumps start stopped");
            pump.Running = true;
            Run(g, 1f);
            Assert.AreEqual(100f, src.SegmentAmount, 1e-3f, "no power, no pumping");
            Assert.AreEqual("no power", pump.Status);
            Power(g, x + 3, y + 2);
            Run(g, 0.5f);
            Assert.AreEqual(Fluids.PumpRate * 0.5f, dst.SegmentAmount, Fluids.PumpRate * 0.05f + 0.01f, "120 units per second (give or take a tick)");
            Run(g, 1f);
            Assert.AreEqual(0f, src.SegmentAmount, 1e-3f);
            Assert.AreEqual(100f, dst.SegmentAmount, 1e-3f, "nothing lost");
            Assert.IsNull(src.Fluid, "an empty segment holds no fluid");
            pump.Running = false;
            Run(g, 0.05f);
            Assert.AreEqual(0f, g.Factory.NetworkStats(g.Grid.At(new Cell(x + 1, y))).demandKW, 1e-3f, "a stopped pump uses no power");
        }

        [Test]
        public void Boiler_AndSteamEngine_PowerAComputer_WithoutACoalGenerator()
        {
            var (g, x, y) = World(10, 8);
            Fill(P(g, "tank", x, y), Fluids.Water, 2000);                 // (x..x+1, y..y+1)
            P(g, "pipe", x + 2, y);
            var boiler = (BoilerState)P(g, "boiler", x + 3, y, Rotation.R90).State;   // facing right: water from x+2, steam to x+5
            boiler.TryInsert(ItemDatabase.Coal, 5);
            P(g, "pipe", x + 5, y);
            var engine = (SteamEngineState)P(g, "steam_engine", x + 6, y, Rotation.R90).State;   // steam from x+5
            var pc = (ComputerState)P(g, "computer", x + 6, y + 2).State;
            pc.Running = true;
            Run(g, 3f);
            Assert.AreEqual(1f, pc.PowerSatisfaction, 1e-3f, "the computer runs on steam");
            Assert.AreEqual(Factory.ComputerKW, engine.OutputKW, 0.01f);
            Assert.AreEqual("boiling", boiler.Status);
            Assert.Less(((PipeState)g.Grid.At(new Cell(x, y)).State).SegmentAmount, 2000f, "water was used");
            Assert.AreEqual("20 kW", engine.Status);
            // a coal generator on the same network only runs when the steam is not enough
            var gen = (GeneratorState)P(g, "generator", x + 8, y + 2).State;
            gen.TryInsert(ItemDatabase.Coal, 5);
            Run(g, 1f);
            Assert.AreEqual(0f, gen.LoadKW, 1e-3f, "steam first");
            g.Grid.Remove(g.Grid.At(new Cell(x + 6, y)));
            Run(g, 1f);
            Assert.AreEqual(Factory.ComputerKW, gen.LoadKW, 0.01f, "then coal");
            Assert.AreEqual(1f, pc.PowerSatisfaction, 1e-3f);
        }

        [Test]
        public void OffshorePump_FillsPipesFromTheShore()
        {
            var g = new GameState(41);
            var s = g.World.Generator.FindSpawn();
            Cell at = default; Rotation rot = Rotation.R0; bool found = false;
            for (int r = 2; r < 200 && !found; r++)
                for (int dx = -r; dx <= r && !found; dx++)
                    foreach (int dy in new[] { -r, r })
                    {
                        var c = new Cell(s.X + dx, s.Y + dy);
                        for (int k = 0; k < 4 && !found; k++)
                        {
                            var q = (Rotation)k;
                            if (g.CanPlace(BuildingDatabase.Get("offshore_pump"), c, q) && g.CanPlace(BuildingDatabase.Get("pipe"), c.Move(q), Rotation.R0)
                                && g.CanPlace(BuildingDatabase.Get("pipe"), c.Move(q, 2), Rotation.R0)) { at = c; rot = q; found = true; }
                        }
                    }
            Assert.IsTrue(found, "a shore near spawn");
            Assert.IsFalse(g.CanPlace(BuildingDatabase.Get("offshore_pump"), at.Move(rot, 2), rot), "not inland");
            var op = P(g, "offshore_pump", at.X, at.Y, rot).State;
            P(g, "pipe", at.Move(rot).X, at.Move(rot).Y); P(g, "pipe", at.Move(rot, 2).X, at.Move(rot, 2).Y);
            Run(g, 1f);
            var seg = (PipeState)g.Grid.At(at.Move(rot)).State;
            Assert.AreEqual(Fluids.OffshoreRate, seg.SegmentAmount, 1f, "60 units per second");
            Assert.AreEqual(Fluids.Water, seg.Fluid);
            Run(g, 5f);
            Assert.AreEqual(200f, seg.SegmentAmount, 1e-3f, "until the pipes are full");
            Assert.AreEqual("pipe full", op.Status);
        }

        [Test]
        public void Pumpjack_OnAnOilField_FeedsAFiller_ThatMakesLaunchFuel()
        {
            GameState g = null; Cell oil = default; bool found = false;
            for (int seed = 1; seed < 40 && !found; seed++)
            {
                g = new GameState(seed);
                var s = g.World.Generator.FindSpawn();
                for (int r = 2; r < 160 && !found; r++)
                    for (int dx = -r; dx <= r && !found; dx++)
                        foreach (int dy in new[] { -r, r })
                        {
                            var c = new Cell(s.X + dx, s.Y + dy);
                            if (g.World.DepositAt(c).Type != ResourceType.Oil) continue;
                            // pumpjack on the field facing up, then pipe, filler, and room for power
                            bool ok = g.CanPlace(BuildingDatabase.Get("pumpjack"), c, Rotation.R0);
                            for (int yy = 2; yy < 8 && ok; yy++) for (int xx = 0; xx < 6 && ok; xx++) ok = g.CanPlace(BuildingDatabase.Get("wall"), new Cell(c.X + xx, c.Y + yy), Rotation.R0);
                            if (ok) { oil = c; found = true; }
                        }
            }
            Assert.IsTrue(found, "an oil field with room around it");
            var pj = (PumpjackState)P(g, "pumpjack", oil.X, oil.Y, Rotation.R0).State;   // out to (x, y+2)
            P(g, "pipe", oil.X, oil.Y + 2);
            var filler = (FillerState)P(g, "filler", oil.X, oil.Y + 3, Rotation.R0).State;   // crude from (x, y+2)
            Power(g, oil.X + 2, oil.Y + 3);
            int before = 0; foreach (var c in new[] { oil, new Cell(oil.X + 1, oil.Y), new Cell(oil.X, oil.Y + 1), new Cell(oil.X + 1, oil.Y + 1) }) before += g.World.DepositAt(c).Amount;
            Run(g, 2f);
            Assert.AreEqual(0, filler.Output.Count(ItemDatabase.Oil), "both start stopped");
            pj.Running = true; filler.Running = true;
            Run(g, 10f);
            int canisters = filler.Output.Count(ItemDatabase.Oil);
            Assert.GreaterOrEqual(canisters, 3, "10 crude/s, 20 per canister, 2 s each");
            int after = 0; foreach (var c in new[] { oil, new Cell(oil.X + 1, oil.Y), new Cell(oil.X, oil.Y + 1), new Cell(oil.X + 1, oil.Y + 1) }) after += g.World.DepositAt(c).Amount;
            Assert.Greater(before, after, "the oil field is depleted as it is pumped");
        }

        [Test]
        public void Greenhouse_DrinksFromAWaterPipe()
        {
            var (g, x, y) = World(4, 3);
            var gh = (GreenhouseState)P(g, "greenhouse", x, y).State;
            if (gh.NearWater) Assert.Ignore("this spot is near a lake: the pipe would not be the only source");
            var pipe = Fill(P(g, "pipe", x + 2, y), Fluids.Water, 100);
            Run(g, 2f);
            Assert.AreEqual(GreenhouseState.PipeRefill * 2f, gh.Tank, 0.3f);
            Assert.AreEqual(100f - gh.Tank, pipe.SegmentAmount, 1e-3f, "what the tank gained the pipe lost");
        }

        static GameState Scenario()
        {
            var (g, x, y) = World(12, 8, 43);
            Fill(P(g, "tank", x, y), Fluids.Water, 1500);
            P(g, "pipe", x + 2, y);
            P(g, "boiler", x + 3, y, Rotation.R90).State.TryInsert(ItemDatabase.Coal, 10);
            P(g, "pipe", x + 5, y);
            P(g, "steam_engine", x + 6, y, Rotation.R90);
            ((ComputerState)P(g, "computer", x + 6, y + 2).State).Running = true;
            Fill(P(g, "pipe", x, y + 3), Fluids.Water, 80);
            ((PumpState)P(g, "pump", x + 1, y + 3, Rotation.R90).State).Running = true;
            P(g, "pole", x + 3, y + 2);   // links to the engine and feeds the pump
            P(g, "pipe", x + 2, y + 3); P(g, "pipe", x + 3, y + 3);
            return g;
        }

        [Test]
        public void Fluids_AreDeterministic_AndSurviveSaveAndLoadMidRun()
        {
            var a = Scenario(); var b = Scenario();
            for (int i = 0; i < 37; i++) { a.Step(); b.Step(); }
            var sw = new StringWriter(); SaveSystem.Write(b, sw);
            StringAssert.Contains("bx fluid water", sw.ToString());
            var c = SaveSystem.Read(new StringReader(sw.ToString()));
            for (int i = 0; i < 60; i++) { a.Step(); b.Step(); c.Step(); }
            Assert.AreEqual(Simulation.StateHash(a), Simulation.StateHash(b), "same inputs, same world");
            Assert.AreEqual(Simulation.StateHash(a), Simulation.StateHash(c), "saving and loading mid-run changes nothing");
        }
    }
}
