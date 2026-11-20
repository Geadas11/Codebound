using System.Collections.Generic;
using Codebound.Building;
using Codebound.Items;

namespace Codebound.Machines
{
    /// <summary>A smelting recipe: input → output in <see cref="Ticks"/> at full power.</summary>
    public sealed class SmeltingRecipe
    {
        public SmeltingRecipe(string input, int inputCount, string output, int outputCount, int ticks)
        {
            Input = input; InputCount = inputCount; Output = output; OutputCount = outputCount; Ticks = ticks;
        }

        public string Input { get; }
        public int InputCount { get; }
        public string Output { get; }
        public int OutputCount { get; }
        public int Ticks { get; }

        public static readonly IReadOnlyList<SmeltingRecipe> All = new[]
        {
            new SmeltingRecipe(ItemCatalog.IronOre, 1, ItemCatalog.IronPlate, 1, 60),
            new SmeltingRecipe(ItemCatalog.CopperOre, 1, ItemCatalog.CopperPlate, 1, 60),
            new SmeltingRecipe(ItemCatalog.Stone, 2, ItemCatalog.StoneBrick, 1, 60),
        };

        public static SmeltingRecipe ForInput(string itemId)
        {
            foreach (var r in All) if (r.Input == itemId) return r;
            return null;
        }

        public static bool IsProduct(string itemId)
        {
            foreach (var r in All) if (r.Output == itemId) return true;
            return false;
        }
    }

    /// <summary>
    /// Electric furnace. <see cref="RequestSmelt"/> (API: <c>furnace.smelt()</c>) runs one cycle of the first recipe whose
    /// input is in the furnace storage. Products are pushed one per tick to the cell in front (conveyor or storage).
    /// </summary>
    public sealed class Furnace : Machine
    {
        public const double WorkingWatts = 120;
        public const double IdleWatts = 3;

        public Furnace(PlacedStructure structure) : base(structure)
        {
            if (structure.Inventory == null) throw new System.ArgumentException("Furnace needs storage", nameof(structure));
        }

        public bool CyclePending { get; private set; }
        public SmeltingRecipe Current { get; private set; }
        public double Progress { get; private set; }
        public int TotalProduced { get; private set; }

        public override double DemandWatts => HasValidProgram ? (CyclePending ? WorkingWatts : IdleWatts) : 0;

        /// <summary>Restores cycle state (save games).</summary>
        public void RestoreState(bool pending, SmeltingRecipe current, double progress, int produced)
        {
            CyclePending = pending;
            Current = current;
            Progress = System.Math.Max(0, System.Math.Min(1, progress));
            TotalProduced = System.Math.Max(0, produced);
        }

        public bool RequestSmelt()
        {
            if (CyclePending) return false;
            CyclePending = true;
            return true;
        }

        public Codebound.Core.GridPos OutputCell => MachineGeometry.FrontCell(Structure);

        public override void Tick(MachineContext ctx)
        {
            PushProducts(ctx);
            if (!CanOperate()) return;
            if (!CyclePending) { State = MachineState.Idle; return; }

            if (Current == null)
            {
                Current = PickRecipe();
                if (Current == null) { State = MachineState.Depleted; CyclePending = false; return; }
                Structure.Inventory.TryRemove(Current.Input, Current.InputCount); // input is consumed at cycle start
            }
            if (Satisfaction <= 0) { State = MachineState.NoPower; return; }

            State = MachineState.Working;
            Progress += Satisfaction / Current.Ticks;
            if (Progress < 1) return;
            if (Structure.Inventory.SpaceFor(Current.Output) < Current.OutputCount) { State = MachineState.OutputBlocked; Progress = 1; return; }
            Structure.Inventory.Add(Current.Output, Current.OutputCount);
            TotalProduced += Current.OutputCount;
            Current = null;
            Progress = 0;
            CyclePending = false;
        }

        private SmeltingRecipe PickRecipe()
        {
            foreach (var r in SmeltingRecipe.All)
                if (Structure.Inventory.Has(r.Input, r.InputCount)) return r;
            return null;
        }

        private void PushProducts(MachineContext ctx)
        {
            var inv = Structure.Inventory;
            for (int i = 0; i < inv.SlotCount; i++)
            {
                var slot = inv.GetSlot(i);
                if (slot.IsEmpty || !SmeltingRecipe.IsProduct(slot.ItemId)) continue;
                if (ctx.Grid.GetAt(OutputCell) == Structure) return;
                if (ctx.Deliver(OutputCell, slot.ItemId)) inv.TryRemove(slot.ItemId, 1);
                return;
            }
        }
    }

    /// <summary>Shared footprint helpers for machines.</summary>
    public static class MachineGeometry
    {
        /// <summary>Cell just outside the footprint, centred on the facing side.</summary>
        public static Codebound.Core.GridPos FrontCell(PlacedStructure s)
        {
            var (w, h) = Footprint.RotatedSize(s.Definition.Width, s.Definition.Height, s.Facing);
            var o = s.Origin;
            switch (s.Facing)
            {
                case Codebound.Core.Direction.North: return new Codebound.Core.GridPos(o.X + (w - 1) / 2, o.Y + h);
                case Codebound.Core.Direction.East: return new Codebound.Core.GridPos(o.X + w, o.Y + (h - 1) / 2);
                case Codebound.Core.Direction.South: return new Codebound.Core.GridPos(o.X + (w - 1) / 2, o.Y - 1);
                default: return new Codebound.Core.GridPos(o.X - 1, o.Y + (h - 1) / 2);
            }
        }
    }
}
