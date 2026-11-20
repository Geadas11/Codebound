using System;
using Codebound.Core;

namespace Codebound.World
{
    /// <summary>A harvestable deposit occupying one cell (ore vein, tree, salvage pile...).</summary>
    public sealed class ResourceNode
    {
        public ResourceNode(GridPos cell, string itemId, int amount, int yieldPerHarvest = 1)
            : this(cell, itemId, amount, amount, yieldPerHarvest) { }

        /// <summary>Restores a partially harvested node (save games).</summary>
        public ResourceNode(GridPos cell, string itemId, int remaining, int initialAmount, int yieldPerHarvest)
        {
            int amount = remaining;
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(remaining));
            if (initialAmount < remaining) throw new ArgumentOutOfRangeException(nameof(initialAmount));
            if (yieldPerHarvest <= 0) throw new ArgumentOutOfRangeException(nameof(yieldPerHarvest));
            Cell = cell;
            ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
            Remaining = amount;
            InitialAmount = initialAmount;
            YieldPerHarvest = yieldPerHarvest;
        }

        public GridPos Cell { get; }
        public string ItemId { get; }
        public int Remaining { get; private set; }
        public int InitialAmount { get; }
        public int YieldPerHarvest { get; }
        public bool Depleted => Remaining <= 0;

        /// <summary>Takes up to <paramref name="max"/> units (default: one harvest). Returns units taken.</summary>
        public int Take(int max = -1)
        {
            if (max < 0) max = YieldPerHarvest;
            int taken = Math.Min(max, Remaining);
            Remaining -= taken;
            return taken;
        }
    }
}
