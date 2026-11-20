using System;
using System.Collections.Generic;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;

namespace Codebound.Building
{
    public enum PlacementStatus
    {
        Ok,
        TerrainBlocked,     // rock, water, resource node, out of bounds
        Occupied,           // another structure is there
        InsufficientResources,
        NoResourceUnderneath // OverResource structures must cover a deposit
    }

    public enum RemovalStatus { Ok, NothingThere, NoSpaceForRefund }

    /// <summary>Result of validating a placement; used both for the ghost preview and for actually building.</summary>
    public sealed class PlacementCheck
    {
        public PlacementCheck(PlacementStatus status, List<GridPos> cells, List<GridPos> blockedCells)
        {
            Status = status;
            Cells = cells;
            BlockedCells = blockedCells;
        }

        public PlacementStatus Status { get; }
        public bool IsValid => Status == PlacementStatus.Ok;
        public IReadOnlyList<GridPos> Cells { get; }
        /// <summary>Cells that are blocked by terrain or other structures (for red highlighting).</summary>
        public IReadOnlyList<GridPos> BlockedCells { get; }
    }

    /// <summary>
    /// Authoritative record of every structure on the grid: placement validation (obstacles,
    /// overlap, cost), placement, removal with refund, and cell lookups.
    /// </summary>
    public sealed class BuildGrid
    {
        private readonly ITerrainBuildQuery _terrain;
        private readonly ItemRegistry _items;
        private readonly Dictionary<GridPos, PlacedStructure> _byCell = new Dictionary<GridPos, PlacedStructure>();
        private readonly Dictionary<int, PlacedStructure> _byId = new Dictionary<int, PlacedStructure>();
        private int _nextId = 1;

        public BuildGrid(ITerrainBuildQuery terrain, ItemRegistry items)
        {
            _terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public event Action<PlacedStructure> Placed;
        public event Action<PlacedStructure> Removed;

        public IReadOnlyCollection<PlacedStructure> Structures => _byId.Values;

        public bool IsOccupied(GridPos cell) => _byCell.ContainsKey(cell);

        public PlacedStructure GetAt(GridPos cell) => _byCell.TryGetValue(cell, out var s) ? s : null;

        public PlacedStructure GetById(int id) => _byId.TryGetValue(id, out var s) ? s : null;

        /// <summary>Checks a placement without changing anything. <paramref name="payer"/> null = skip cost check.</summary>
        public PlacementCheck Validate(StructureDefinition def, GridPos origin, Direction facing, ItemInventory payer)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (!def.Rotatable) facing = Direction.North;
            var cells = Footprint.Cells(origin, def.Width, def.Height, facing);
            var blocked = new List<GridPos>();
            var status = PlacementStatus.Ok;
            bool overResource = def.Placement == PlacementRule.OverResource;
            bool coversResource = false;
            foreach (var c in cells)
            {
                if (overResource && _terrain.HasResource(c)) coversResource = true;
                if (overResource ? !_terrain.IsBuildableTerrain(c) : !_terrain.IsBuildable(c))
                {
                    blocked.Add(c);
                    if (status == PlacementStatus.Ok) status = PlacementStatus.TerrainBlocked;
                }
                else if (_byCell.ContainsKey(c))
                {
                    blocked.Add(c);
                    if (status == PlacementStatus.Ok) status = PlacementStatus.Occupied;
                }
            }
            if (status == PlacementStatus.Ok && overResource && !coversResource)
                status = PlacementStatus.NoResourceUnderneath;
            if (status == PlacementStatus.Ok && payer != null && !payer.HasAll(def.Cost))
                status = PlacementStatus.InsufficientResources;
            return new PlacementCheck(status, cells, blocked);
        }

        /// <summary>Validates, charges <paramref name="payer"/> (if given) and places the structure.</summary>
        public PlacementStatus TryPlace(StructureDefinition def, GridPos origin, Direction facing, ItemInventory payer,
            out PlacedStructure placed)
        {
            placed = null;
            if (def != null && !def.Rotatable) facing = Direction.North;
            var check = Validate(def, origin, facing, payer);
            if (!check.IsValid) return check.Status;
            if (payer != null && !payer.TryConsume(def.Cost)) return PlacementStatus.InsufficientResources;

            var inv = def.HasInventory ? new ItemInventory(def.InventorySlots, _items) : null;
            placed = new PlacedStructure(_nextId++, def, origin, facing, check.Cells, inv);
            foreach (var c in placed.Cells) _byCell[c] = placed;
            _byId[placed.InstanceId] = placed;
            Placed?.Invoke(placed);
            return PlacementStatus.Ok;
        }

        /// <summary>
        /// Removes the structure covering <paramref name="cell"/>. Its build cost and stored items are returned to
        /// <paramref name="refundTo"/>; if they do not fit, nothing is removed.
        /// </summary>
        public RemovalStatus TryRemove(GridPos cell, ItemInventory refundTo, out PlacedStructure removed)
        {
            removed = null;
            if (!_byCell.TryGetValue(cell, out var s)) return RemovalStatus.NothingThere;
            if (refundTo != null)
            {
                var refund = new List<ItemStack>(s.Definition.Cost);
                if (s.Inventory != null) refund.AddRange(s.Inventory.ToList());
                if (!refundTo.TryAddAll(refund)) return RemovalStatus.NoSpaceForRefund;
            }
            foreach (var c in s.Cells) _byCell.Remove(c);
            _byId.Remove(s.InstanceId);
            removed = s;
            Removed?.Invoke(s);
            return RemovalStatus.Ok;
        }

        /// <summary>Walkability that combines terrain with movement-blocking structures.</summary>
        public IWalkabilityQuery WithTerrain(IWalkabilityQuery terrainWalkability) => new CombinedWalkability(terrainWalkability, this);

        private sealed class CombinedWalkability : IWalkabilityQuery
        {
            private readonly IWalkabilityQuery _terrain;
            private readonly BuildGrid _grid;

            public CombinedWalkability(IWalkabilityQuery terrain, BuildGrid grid)
            {
                _terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
                _grid = grid;
            }

            public bool IsWalkable(GridPos cell)
            {
                if (!_terrain.IsWalkable(cell)) return false;
                var s = _grid.GetAt(cell);
                return s == null || !s.Definition.BlocksMovement;
            }
        }
    }
}
