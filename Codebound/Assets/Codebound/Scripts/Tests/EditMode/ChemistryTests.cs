using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 12: oil refinery, chemical plant, advanced circuits.</summary>
    public class ChemistryTests
    {
        static (GameState g, int x, int y) World(int w, int h, int seed = 131)
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

        static PipeState Tank(GameState g, int x, int y, string fluid, float amount)
        {
            var t = (PipeState)P(g, "tank", x, y).State;
            if (fluid != null) { t.LoadExtra("fluid", fluid); t.LoadExtra("amount", amount.ToString(System.Globalization.CultureInfo.InvariantCulture)); }
            return t;
        }

        static void Run(GameState g, float s) { for (float t = 0; t < s - 1e-4f; t += 0.05f) g.Factory.Tick(0.05f); }

        /// <summary>A 3×3 machine at (x+2, y+2) facing up: a tank behind it (pipe at y+1), a tank in front (pipe at y+5), two generators at its right.</summary>
        static (GameState g, PlacedBuilding m, PipeState back, PipeState front, int x, int y) Setup(string machine, string backFluid, float backAmount)
        {
            var (g, x, y) = World(12, 10);
            int ox = x + 2, oy = y + 1;   // two free columns on the left for a side pipe
            var back = Tank(g, ox + 3, oy - 1, backFluid, backAmount); P(g, "pipe", ox + 3, oy + 1);
            var m = P(g, machine, ox + 2, oy + 2);
            P(g, "pipe", ox + 3, oy + 5); var front = Tank(g, ox + 3, oy + 6, null, 0);
            P(g, "generator", ox + 6, oy + 2).State.TryInsert(ItemDatabase.Coal, 100);
            P(g, "generator", ox + 6, oy + 4).State.TryInsert(ItemDatabase.Coal, 100);
            return (g, m, back, front, ox, oy);
        }

        [Test]
        public void TheRefinery_TurnsCrudeOilIntoPetroleumGas()
        {
            var (g, m, back, front, _, _) = Setup("refinery", Fluids.CrudeOil, 2000);
            var rf = (RefineryState)m.State;
            Run(g, 2f);
            Assert.AreEqual(0f, front.SegmentAmount, 1e-3f, "refineries start stopped");
            rf.Running = true;
            Run(g, 10f);
            Assert.AreEqual(Fluids.PetroleumGas, front.Fluid);
            float used = 2000 - back.SegmentAmount - rf.Crude.Amount;
            Assert.AreEqual(RefineryState.CrudePerSecond * 10, used, 15f, "20 crude oil per second");
            Assert.AreEqual(used * RefineryState.GasPerCrude, front.SegmentAmount + rf.Gas.Amount, 1f, "0.45 gas per crude");
        }

        [Test]
        public void TheChemicalPlant_MakesPlastic_FromGasAndCoal()
        {
            var (g, m, back, _, _, _) = Setup("chemical_plant", Fluids.PetroleumGas, 1000);
            var cp = (ChemicalPlantState)m.State;
            Assert.IsNotNull(Factory.Configure(m, "gold"), "unknown recipe");
            Assert.IsNull(Factory.Configure(m, "plastic"));
            Assert.IsTrue(cp.TryInsert(ItemDatabase.Coal, 20));
            Assert.IsFalse(cp.TryInsert(ItemDatabase.IronPlate, 1), "not an ingredient of plastic");
            cp.Running = true;
            Run(g, 5.04f);
            Assert.AreEqual(10, cp.Output.Count(ItemDatabase.Plastic), "2 bars per second");
            Assert.AreEqual(15, cp.Input.Count(ItemDatabase.Coal));
            Assert.AreEqual(1000 - 5 * 20, back.SegmentAmount + cp.FluidIn[0].Amount, 1e-2f, "20 gas each");
        }

        [Test]
        public void Sulfur_NeedsTwoFluids_FromTwoPipes_ThenAcidFlowsOut()
        {
            var (g, m, back, front, x, y) = Setup("chemical_plant", Fluids.PetroleumGas, 1000);
            Tank(g, x - 1, y + 2, Fluids.Water, 2000); P(g, "pipe", x + 1, y + 3);   // water from the left side
            var cp = (ChemicalPlantState)m.State;
            cp.SetRecipe("sulfur"); cp.Running = true;
            Run(g, 5.04f);
            Assert.AreEqual(10, cp.Output.Count(ItemDatabase.Sulfur));
            // acid: water + sulfur + iron -> 50 acid each, out to the tank in front
            cp.Output.TryRemove(ItemDatabase.Sulfur, 10);
            Assert.IsNull(cp.SetRecipe("sulfuric_acid"));
            cp.TryInsert(ItemDatabase.Sulfur, 10); cp.TryInsert(ItemDatabase.IronPlate, 2);
            Run(g, 3f);
            Assert.AreEqual(Fluids.SulfuricAcid, front.Fluid);
            Assert.AreEqual(100f, front.SegmentAmount + cp.FluidOut.Amount, 1e-2f, "two batches of 50");
        }

        [Test]
        public void AdvancedCircuits_UsePlastic()
        {
            var r = AssemblerState.Recipes[ItemDatabase.AdvancedCircuit];
            CollectionAssert.Contains(System.Array.ConvertAll(r.Inputs, i => i.ItemId), ItemDatabase.Plastic);
            CollectionAssert.Contains(System.Array.ConvertAll(AssemblerState.Recipes[ItemDatabase.SpeedModule].Inputs, i => i.ItemId), ItemDatabase.AdvancedCircuit);
            CollectionAssert.Contains(System.Array.ConvertAll(BuildingDatabase.Get("assembler3").Cost, i => i.ItemId), ItemDatabase.AdvancedCircuit);
        }

        [Test]
        public void ChemistryIsSaved_AndContinuesIdentically()
        {
            var (g, m, _, _, _, _) = Setup("chemical_plant", Fluids.PetroleumGas, 1000);
            var cp = (ChemicalPlantState)m.State;
            cp.SetRecipe("plastic"); cp.TryInsert(ItemDatabase.Coal, 30); cp.Running = true;
            for (int i = 0; i < 33; i++) g.Step();
            var w = new StringWriter(); SaveSystem.Write(g, w);
            StringAssert.Contains("bx recipe plastic", w.ToString());
            var h = SaveSystem.Read(new StringReader(w.ToString()));
            for (int i = 0; i < 100; i++) { g.Step(); h.Step(); }
            Assert.AreEqual(Simulation.StateHash(g), Simulation.StateHash(h));
        }
    }
}
