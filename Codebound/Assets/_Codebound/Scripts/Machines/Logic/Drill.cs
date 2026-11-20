using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;
using Codebound.World;

namespace Codebound.Machines
{
    /// <summary>
    /// Electric drill standing on ore. Each <see cref="RequestMine"/> (API: <c>drill.mine()</c>) performs one
    /// mining cycle: progress scales with power satisfaction, then one unit of ore is extracted from a deposit
    /// under the footprint into the drill's output slot, which is pushed into a storage in front of the drill.
    /// </summary>
    public sealed class Drill : Machine
    {
        public const int CycleTicks = 40;          // 2 s at 20 ticks/s and full power
        public const double WorkingWatts = 90;
        public const double IdleWatts = 5;

        private int _nextCell;

        public Drill(PlacedStructure structure) : base(structure)
        {
            if (structure.Inventory == null) throw new System.ArgumentException("Drill needs an output inventory", nameof(structure));
        }

        public bool CyclePending { get; private set; }
        /// <summary>0..1 progress of the current cycle.</summary>
        public double Progress { get; private set; }
        public int TotalMined { get; private set; }

        public override double DemandWatts => HasValidProgram ? (CyclePending ? WorkingWatts : IdleWatts) : 0;

        /// <summary>Restores cycle state (save games).</summary>
        public void RestoreState(bool cyclePending, double progress, int totalMined)
        {
            CyclePending = cyclePending;
            Progress = System.Math.Max(0, System.Math.Min(1, progress));
            TotalMined = System.Math.Max(0, totalMined);
        }

        /// <summary>Queues one mining cycle. Returns false if a cycle is already queued (API: drill.mine()).</summary>
        public bool RequestMine()
        {
            if (CyclePending) return false;
            CyclePending = true;
            return true;
        }

        /// <summary>Remaining ore units under the drill (API: drill.ore_remaining()).</summary>
        public int OreRemaining(WorldMap world)
        {
            int n = 0;
            foreach (var c in Structure.Cells) if (world.TryGetNode(c, out var node)) n += node.Remaining;
            return n;
        }

        /// <summary>Cell just outside the footprint, centred on the facing side, where output is delivered.</summary>
        public GridPos OutputCell
        {
            get
            {
                var (w, h) = Footprint.RotatedSize(Structure.Definition.Width, Structure.Definition.Height, Structure.Facing);
                var o = Structure.Origin;
                switch (Structure.Facing)
                {
                    case Direction.North: return new GridPos(o.X + (w - 1) / 2, o.Y + h);
                    case Direction.East: return new GridPos(o.X + w, o.Y + (h - 1) / 2);
                    case Direction.South: return new GridPos(o.X + (w - 1) / 2, o.Y - 1);
                    default: return new GridPos(o.X - 1, o.Y + (h - 1) / 2);
                }
            }
        }

        public override void Tick(MachineContext ctx)
        {
            PushOutput(ctx.Grid);
            if (!CanOperate()) return;
            if (OreRemaining(ctx.World) == 0) { State = MachineState.Depleted; CyclePending = false; Progress = 0; return; }
            if (!CyclePending) { State = MachineState.Idle; return; }
            if (Satisfaction <= 0) { State = MachineState.NoPower; return; }

            State = MachineState.Working;
            Progress += Satisfaction / CycleTicks;
            if (Progress < 1) return;

            // Cycle complete: needs room for one unit of the ore found.
            var cell = NextOreCell(ctx.World);
            ctx.World.TryGetNode(cell, out var node);
            if (Structure.Inventory.SpaceFor(node.ItemId) <= 0) { State = MachineState.OutputBlocked; Progress = 1; return; }
            int taken = ctx.World.Harvest(cell, out var itemId, 1);
            Structure.Inventory.Add(itemId, taken);
            TotalMined += taken;
            Progress = 0;
            CyclePending = false;
            PushOutput(ctx.Grid);
        }

        private GridPos NextOreCell(WorldMap world)
        {
            var cells = Structure.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[(_nextCell + i) % cells.Count];
                if (world.TryGetNode(c, out _)) { _nextCell = (_nextCell + i + 1) % cells.Count; return c; }
            }
            return cells[0];
        }

        /// <summary>Moves output into a storage structure in front of the drill, if any.</summary>
        private void PushOutput(BuildGrid grid)
        {
            var target = grid.GetAt(OutputCell);
            if (target?.Inventory == null || target == Structure) return;
            var inv = Structure.Inventory;
            for (int i = 0; i < inv.SlotCount; i++) inv.TransferSlotTo(i, target.Inventory);
        }
    }
}
