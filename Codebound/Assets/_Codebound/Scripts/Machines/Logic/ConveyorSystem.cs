using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;

namespace Codebound.Machines
{
    /// <summary>One belt cell: carries at most one item towards <see cref="Direction"/>.</summary>
    public sealed class Belt
    {
        internal Belt(PlacedStructure structure) { Structure = structure; }

        public PlacedStructure Structure { get; }
        public GridPos Cell => Structure.Origin;
        public Direction Direction => Structure.Facing;
        public string Item { get; internal set; }
        /// <summary>0..1 position of the item along this cell.</summary>
        public double Progress { get; internal set; }
        public bool IsEmpty => Item == null;
    }

    /// <summary>
    /// Basic conveyor belts. Design decision D17: basic belts are passive logistics infrastructure and move items
    /// without a program (they are not "machines"); smart/programmable belts (Etapa 4, later) will be machines.
    /// An item takes <see cref="TicksPerCell"/> ticks per cell; at the end it moves onto the next belt or into the
    /// storage/machine the belt points at, or waits.
    /// </summary>
    public sealed class ConveyorSystem
    {
        public const int TicksPerCell = 8; // 2.5 cells/s

        private readonly Dictionary<GridPos, Belt> _belts = new Dictionary<GridPos, Belt>();
        private readonly BuildGrid _grid;

        public ConveyorSystem(BuildGrid grid) { _grid = grid; }

        public IReadOnlyCollection<Belt> Belts => _belts.Values;

        public Belt GetBelt(GridPos cell) => _belts.TryGetValue(cell, out var b) ? b : null;

        internal void Add(PlacedStructure s) => _belts[s.Origin] = new Belt(s);

        /// <summary>Removes a belt; the item it carried is returned (or null).</summary>
        internal string Remove(PlacedStructure s)
        {
            if (!_belts.TryGetValue(s.Origin, out var b)) return null;
            _belts.Remove(s.Origin);
            return b.Item;
        }

        /// <summary>Puts an item on the belt at <paramref name="cell"/> if it is empty.</summary>
        public bool TryInsert(GridPos cell, string itemId)
        {
            if (!_belts.TryGetValue(cell, out var b) || !b.IsEmpty) return false;
            b.Item = itemId;
            b.Progress = 0;
            return true;
        }

        /// <summary>Restores an item on a belt (save games).</summary>
        public void Restore(GridPos cell, string itemId, double progress)
        {
            if (_belts.TryGetValue(cell, out var b)) { b.Item = itemId; b.Progress = progress; }
        }

        public void Tick(System.Func<GridPos, string, bool> deliverToStructure)
        {
            const double step = 1.0 / TicksPerCell;
            // Snapshot which cells are free at the start of the tick: an item may only enter a belt that was
            // empty before this tick, which keeps results independent of iteration order.
            var freeAtStart = new HashSet<GridPos>();
            foreach (var b in _belts.Values) if (b.IsEmpty) freeAtStart.Add(b.Cell);

            foreach (var b in _belts.Values)
            {
                if (b.IsEmpty) continue;
                if (b.Progress < 1) { b.Progress = System.Math.Min(1, b.Progress + step); continue; }

                var next = b.Cell + b.Direction.ToOffset();
                if (_belts.TryGetValue(next, out var nb))
                {
                    if (!freeAtStart.Contains(next) || !nb.IsEmpty) continue;
                    nb.Item = b.Item;
                    nb.Progress = 0;
                    freeAtStart.Remove(next);
                    b.Item = null;
                    b.Progress = 0;
                }
                else if (deliverToStructure(next, b.Item))
                {
                    b.Item = null;
                    b.Progress = 0;
                }
            }
        }
    }
}
