using System;
using System.Collections.Generic;
using Codebound.Items;

namespace Codebound.Building
{
    /// <summary>Static description of a buildable structure. Footprint is given for the North facing.</summary>
    public sealed class StructureDefinition
    {
        public StructureDefinition(string id, int width, int height, IReadOnlyList<ItemStack> cost,
            bool rotatable = true, int inventorySlots = 0, bool blocksMovement = true, string nameKey = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Structure id required", nameof(id));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (inventorySlots < 0) throw new ArgumentOutOfRangeException(nameof(inventorySlots));
            Id = id;
            Width = width;
            Height = height;
            Cost = cost ?? Array.Empty<ItemStack>();
            Rotatable = rotatable;
            InventorySlots = inventorySlots;
            BlocksMovement = blocksMovement;
            NameKey = nameKey ?? "name." + id;
        }

        /// <summary>Stable id, e.g. "structure.wall".</summary>
        public string Id { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<ItemStack> Cost { get; }
        public bool Rotatable { get; }
        public int InventorySlots { get; }
        public bool HasInventory => InventorySlots > 0;
        public bool BlocksMovement { get; }
        public string NameKey { get; }

        public override string ToString() => Id;
    }
}
