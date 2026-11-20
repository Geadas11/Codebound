using System.Collections.Generic;
using Codebound.Core;
using Codebound.Inventory;

namespace Codebound.Building
{
    /// <summary>A structure instance that exists on the grid.</summary>
    public sealed class PlacedStructure
    {
        internal PlacedStructure(int instanceId, StructureDefinition def, GridPos origin, Direction facing,
            IReadOnlyList<GridPos> cells, ItemInventory inventory)
        {
            InstanceId = instanceId;
            Definition = def;
            Origin = origin;
            Facing = facing;
            Cells = cells;
            Inventory = inventory;
        }

        public int InstanceId { get; }
        public StructureDefinition Definition { get; }
        public GridPos Origin { get; }
        public Direction Facing { get; }
        public IReadOnlyList<GridPos> Cells { get; }
        /// <summary>Null when the structure has no storage.</summary>
        public ItemInventory Inventory { get; }

        /// <summary>World-space centre of the footprint.</summary>
        public Vec2 Center
        {
            get
            {
                var (w, h) = Footprint.RotatedSize(Definition.Width, Definition.Height, Facing);
                return new Vec2(Origin.X + w / 2f, Origin.Y + h / 2f);
            }
        }

        public override string ToString() => $"{Definition.Id}#{InstanceId}@{Origin}";
    }
}
