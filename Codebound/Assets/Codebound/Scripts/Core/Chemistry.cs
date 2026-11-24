using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Oil refinery (Etapa 12): crude oil from the pipe behind becomes petroleum gas in the pipe in front
    /// (20 crude/s → 9 gas/s at speed 1). Powered, start()/stop(), module slots.
    /// </summary>
    public sealed class RefineryState : SwitchedFluidState
    {
        public const float CrudePerSecond = 20f, GasPerCrude = 0.45f, KW = 200f;
        public readonly FluidBox Crude = new FluidBox(Fluids.CrudeOil, 200f), Gas = new FluidBox(Fluids.PetroleumGas, 200f);
        public override IEnumerable<(string, float)> FluidContents { get { yield return (Fluids.CrudeOil, Crude.Amount); yield return (Fluids.PetroleumGas, Gas.Amount); } }
        protected override void SaveMore(IDictionary<string, string> d)
        {
            if (Crude.Amount > 0) d["crude"] = F(Crude.Amount);
            if (Gas.Amount > 0) d["gas"] = F(Gas.Amount);
        }
        protected override void LoadMore(string k, string v)
        {
            if (k == "crude") Crude.Amount = PumpjackState.Clamp(PF(v), Crude.Capacity);
            else if (k == "gas") Gas.Amount = PumpjackState.Clamp(PF(v), Gas.Capacity);
        }
    }

    /// <summary>A chemical plant recipe: fluids and items in, items or a fluid out.</summary>
    public sealed class ChemRecipe
    {
        public string Id; public float Seconds;
        public (string fluid, float amount)[] FluidsIn = new (string, float)[0];
        public ItemStack[] ItemsIn = new ItemStack[0];
        public ItemStack ItemOut; public (string fluid, float amount)? FluidOut;
    }

    /// <summary>
    /// Chemical plant (Etapa 12): pick a recipe with configure("plastic" | "sulfur" | "sulfuric_acid" | "battery").
    /// Fluids come from pipes at its back and sides, items from arms; a fluid product goes to the pipe in front, items wait
    /// in its output for arms. Powered, start()/stop(), module slots.
    /// </summary>
    public sealed class ChemicalPlantState : SwitchedFluidState
    {
        public const float KW = 150f;
        public static readonly Dictionary<string, ChemRecipe> Recipes = new Dictionary<string, ChemRecipe>
        {
            { "plastic", new ChemRecipe { Id = "plastic", Seconds = 1f, FluidsIn = new[] { (Fluids.PetroleumGas, 20f) },
                ItemsIn = new[] { new ItemStack(ItemDatabase.Coal, 1) }, ItemOut = new ItemStack(ItemDatabase.Plastic, 2) } },
            { "sulfur", new ChemRecipe { Id = "sulfur", Seconds = 1f, FluidsIn = new[] { (Fluids.PetroleumGas, 30f), (Fluids.Water, 30f) },
                ItemOut = new ItemStack(ItemDatabase.Sulfur, 2) } },
            { "sulfuric_acid", new ChemRecipe { Id = "sulfuric_acid", Seconds = 1f, FluidsIn = new[] { (Fluids.Water, 100f) },
                ItemsIn = new[] { new ItemStack(ItemDatabase.Sulfur, 5), new ItemStack(ItemDatabase.IronPlate, 1) }, FluidOut = (Fluids.SulfuricAcid, 50f) } },
            { "battery", new ChemRecipe { Id = "battery", Seconds = 4f, FluidsIn = new[] { (Fluids.SulfuricAcid, 20f) },
                ItemsIn = new[] { new ItemStack(ItemDatabase.IronPlate, 1), new ItemStack(ItemDatabase.CopperPlate, 1) }, ItemOut = new ItemStack(ItemDatabase.BatteryCell, 1) } },
        };

        public ChemRecipe Recipe { get; private set; }
        public FluidBox[] FluidIn = new FluidBox[0];
        public FluidBox FluidOut;
        public readonly Inventory Input = new Inventory(4), Output = new Inventory(2);
        public float Progress;

        /// <summary>Picks a recipe ("" = none). Changing recipe empties its fluid boxes. Null or why not.</summary>
        public string SetRecipe(string text)
        {
            string id = (text ?? "").Trim();
            if (id.Length == 0) { Recipe = null; FluidIn = new FluidBox[0]; FluidOut = null; Progress = 0; Touch(); return null; }
            if (!Recipes.TryGetValue(id, out var r)) return "recipes: plastic, sulfur, sulfuric_acid, battery";
            if (Recipe == r) return null;
            Recipe = r; Progress = 0;
            FluidIn = new FluidBox[r.FluidsIn.Length];
            for (int i = 0; i < FluidIn.Length; i++) FluidIn[i] = new FluidBox(r.FluidsIn[i].fluid, r.FluidsIn[i].amount * 2);
            FluidOut = r.FluidOut.HasValue ? new FluidBox(r.FluidOut.Value.fluid, r.FluidOut.Value.amount * 2) : null;
            // a save lists its keys alphabetically: amounts and progress arrive before the recipe, so they wait here
            for (int i = 0; i < FluidIn.Length; i++) if (pending[i] >= 0) FluidIn[i].Amount = PumpjackState.Clamp(pending[i], FluidIn[i].Capacity);
            if (FluidOut != null && pending[2] >= 0) FluidOut.Amount = PumpjackState.Clamp(pending[2], FluidOut.Capacity);
            if (pending[3] >= 0) Progress = pending[3];
            pending[0] = pending[1] = pending[2] = pending[3] = -1;
            Touch();
            return null;
        }
        readonly float[] pending = { -1, -1, -1, -1 };   // in0, in1, out, progress read before the recipe

        public override bool TryInsert(string itemId, int count) => AcceptsItem(itemId) && Input.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId)
        {
            if (Recipe == null) return false;
            foreach (var s in Recipe.ItemsIn) if (s.ItemId == itemId) return true;
            return false;
        }
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("input", Input); yield return ("output", Output); } }
        public override Inventory Extractable => Output;
        public override IEnumerable<(string, float)> FluidContents
        {
            get { foreach (var b in FluidIn) yield return (b.Fluid, b.Amount); if (FluidOut != null) yield return (FluidOut.Fluid, FluidOut.Amount); }
        }

        protected override void SaveMore(IDictionary<string, string> d)
        {
            if (Recipe != null) d["recipe"] = Recipe.Id;
            for (int i = 0; i < FluidIn.Length; i++) if (FluidIn[i].Amount > 0) d["in" + i] = F(FluidIn[i].Amount);
            if (FluidOut != null && FluidOut.Amount > 0) d["out"] = F(FluidOut.Amount);
            if (Progress > 0) d["prog"] = F(Progress);
        }
        protected override void LoadMore(string k, string v)
        {
            if (k == "recipe") SetRecipe(v);
            else if (k == "in0") pending[0] = Math.Max(0f, PF(v));
            else if (k == "in1") pending[1] = Math.Max(0f, PF(v));
            else if (k == "out") pending[2] = Math.Max(0f, PF(v));
            else if (k == "prog") pending[3] = Math.Max(0f, PF(v));
        }

        internal bool HasInputs()
        {
            for (int i = 0; i < FluidIn.Length; i++) if (FluidIn[i].Amount < Recipe.FluidsIn[i].amount - 1e-4f) return false;
            foreach (var s in Recipe.ItemsIn) if (Input.Count(s.ItemId) < s.Count) return false;
            return true;
        }

        internal bool HasRoom()
        {
            if (Recipe.ItemOut.ItemId != null)
            {
                var o = Output[0];
                if (!o.IsEmpty && (o.ItemId != Recipe.ItemOut.ItemId || o.Count + Recipe.ItemOut.Count * 2 > ItemDatabase.Get(o.ItemId).MaxStack)) return false;
            }
            if (FluidOut != null && FluidOut.Free < Recipe.FluidOut.Value.amount * 1.5f) return false;
            return true;
        }
    }

    public sealed partial class Factory
    {
        IEnumerable<Cell> BackAndSides(PlacedBuilding b)
        {
            var front = new HashSet<Cell>(b.FrontCells());
            foreach (var c in Around(b)) if (!front.Contains(c)) yield return c;
        }

        void TickRefinery(PlacedBuilding b, RefineryState rf, float dt)
        {
            Pull(FirstSegment(b.BackCells(), Fluids.CrudeOil), rf.Crude);
            if (!rf.Working) { rf.Status = rf.Running ? "disabled" : "stopped"; }
            else if (rf.PowerSatisfaction <= 0) rf.Status = "no power";
            else
            {
                float prod = 1f + Effects(b).productivity;
                float crude = Math.Min(RefineryState.CrudePerSecond * dt * SpeedOf(b) * rf.PowerSatisfaction, rf.Crude.Amount);
                crude = Math.Min(crude, rf.Gas.Free / (RefineryState.GasPerCrude * prod));
                if (crude > 0) { rf.Crude.Take(crude); rf.Gas.Amount += crude * RefineryState.GasPerCrude * prod; }
                rf.Status = crude > 0 ? "refining" : rf.Crude.Amount <= 0 ? "no oil" : "output full";
            }
            Push(rf.Gas, FirstSegment(b.FrontCells(), Fluids.PetroleumGas));
        }

        void TickChemicalPlant(PlacedBuilding b, ChemicalPlantState cp, float dt)
        {
            var r = cp.Recipe;
            if (r == null) { cp.Status = "no recipe"; return; }
            foreach (var box in cp.FluidIn) Pull(FirstSegment(BackAndSides(b), box.Fluid), box);
            if (cp.FluidOut != null) Push(cp.FluidOut, FirstSegment(b.FrontCells(), cp.FluidOut.Fluid));
            if (!cp.Working) { cp.Status = cp.Running ? "disabled" : "stopped"; return; }
            if (cp.PowerSatisfaction <= 0) { cp.Status = "no power"; return; }
            if (!cp.HasInputs()) { cp.Status = "missing input"; return; }
            if (!cp.HasRoom()) { cp.Status = "output full"; return; }
            cp.Progress += dt * SpeedOf(b) * cp.PowerSatisfaction;
            cp.Status = "reacting";
            if (cp.Progress < r.Seconds) return;
            cp.Progress -= r.Seconds;
            for (int i = 0; i < cp.FluidIn.Length; i++) cp.FluidIn[i].Take(r.FluidsIn[i].amount);
            foreach (var s in r.ItemsIn) cp.Input.TryRemove(s.ItemId, s.Count);
            if (r.ItemOut.ItemId != null) Produce(b, cp.Output, r.ItemOut.ItemId, r.ItemOut.Count);
            if (r.FluidOut.HasValue) cp.FluidOut.Amount = Math.Min(cp.FluidOut.Capacity, cp.FluidOut.Amount + r.FluidOut.Value.amount * (1f + Effects(b).productivity));
        }
    }
}
