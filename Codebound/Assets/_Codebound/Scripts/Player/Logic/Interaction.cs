using System;
using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.World;

namespace Codebound.Player
{
    public enum InteractionKind { None, Harvest, OpenStorage }

    /// <summary>What the player would interact with if they pressed the interact key now.</summary>
    public readonly struct InteractionTarget
    {
        public InteractionTarget(InteractionKind kind, GridPos cell, ResourceNode node, PlacedStructure structure)
        {
            Kind = kind; Cell = cell; Node = node; Structure = structure;
        }

        public readonly InteractionKind Kind;
        public readonly GridPos Cell;
        public readonly ResourceNode Node;
        public readonly PlacedStructure Structure;

        public static readonly InteractionTarget None = new InteractionTarget(InteractionKind.None, GridPos.Zero, null, null);
    }

    public enum HarvestResult { Ok, NoTarget, InventoryFull }

    /// <summary>Finds and executes player interactions (harvest resource nodes, open storage).</summary>
    public sealed class InteractionSystem
    {
        public const float DefaultRange = 1.6f;

        private readonly WorldMap _world;
        private readonly BuildGrid _grid;

        public InteractionSystem(WorldMap world, BuildGrid grid)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        public float Range { get; set; } = DefaultRange;

        /// <summary>
        /// Picks a target. If <paramref name="preferredCell"/> (e.g. under the mouse) is in range and interactable it wins;
        /// otherwise the nearest interactable cell within range of <paramref name="position"/>.
        /// </summary>
        public InteractionTarget FindTarget(Vec2 position, GridPos? preferredCell = null)
        {
            if (preferredCell.HasValue && InRange(position, preferredCell.Value))
            {
                var t = TargetAt(preferredCell.Value);
                if (t.Kind != InteractionKind.None) return t;
            }

            var best = InteractionTarget.None;
            float bestDist = float.MaxValue;
            int r = (int)Math.Ceiling(Range) + 1;
            var center = GridPos.FromWorld(position.X, position.Y);
            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                var c = new GridPos(center.X + dx, center.Y + dy);
                float d = DistanceToCell(position, c);
                if (d > Range || d >= bestDist) continue;
                var t = TargetAt(c);
                if (t.Kind == InteractionKind.None) continue;
                best = t;
                bestDist = d;
            }
            return best;
        }

        public InteractionTarget TargetAt(GridPos c)
        {
            if (_world.TryGetNode(c, out var node)) return new InteractionTarget(InteractionKind.Harvest, c, node, null);
            var s = _grid.GetAt(c);
            if (s != null && s.Inventory != null) return new InteractionTarget(InteractionKind.OpenStorage, c, null, s);
            return InteractionTarget.None;
        }

        /// <summary>Harvests one yield of the node into <paramref name="into"/>. Nothing is lost if the inventory is full.</summary>
        public HarvestResult Harvest(InteractionTarget target, ItemInventory into, out int gained)
        {
            gained = 0;
            if (target.Kind != InteractionKind.Harvest || target.Node == null) return HarvestResult.NoTarget;
            if (!_world.TryGetNode(target.Cell, out var node)) return HarvestResult.NoTarget;
            int space = into.SpaceFor(node.ItemId);
            if (space <= 0) return HarvestResult.InventoryFull;
            int taken = _world.Harvest(target.Cell, out var itemId, Math.Min(space, node.YieldPerHarvest));
            into.Add(itemId, taken);
            gained = taken;
            return HarvestResult.Ok;
        }

        public bool InRange(Vec2 position, GridPos cell) => DistanceToCell(position, cell) <= Range;

        /// <summary>Distance from a point to the nearest point of a cell's square (0 if inside).</summary>
        public static float DistanceToCell(Vec2 p, GridPos c)
        {
            float cx = Math.Max(c.X, Math.Min(p.X, c.X + 1));
            float cy = Math.Max(c.Y, Math.Min(p.Y, c.Y + 1));
            return Vec2.Distance(p, new Vec2(cx, cy));
        }
    }
}
