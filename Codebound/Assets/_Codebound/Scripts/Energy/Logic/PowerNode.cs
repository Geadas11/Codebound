using System;
using System.Collections.Generic;
using Codebound.Core;

namespace Codebound.Energy
{
    /// <summary>
    /// Anything attached to an electrical network: consumers (machines, computers), producers (generators)
    /// and storage (batteries). Power is in watts; energy in joules. One simulation tick lasts
    /// <c>1 / SimulationClock.TicksPerSecond</c> seconds.
    /// </summary>
    public abstract class PowerNode
    {
        protected PowerNode(IReadOnlyList<GridPos> cells)
        {
            Cells = cells ?? throw new ArgumentNullException(nameof(cells));
        }

        /// <summary>Cells occupied; a node is connected if any of them is inside a pole's supply area.</summary>
        public IReadOnlyList<GridPos> Cells { get; }

        /// <summary>Watts this node wants this tick.</summary>
        public virtual double DemandWatts => 0;

        /// <summary>Watts this node can produce this tick.</summary>
        public virtual double ProductionWatts => 0;

        /// <summary>Fraction (0..1) of this tick's demand that was met. Set by the grid every tick.</summary>
        public double Satisfaction { get; internal set; }

        /// <summary>Network this node is attached to, or -1 when not connected.</summary>
        public int NetworkId { get; internal set; } = -1;

        public bool Connected => NetworkId >= 0;

        /// <summary>Called after the grid balanced the network: how many watts of <see cref="ProductionWatts"/> were used.</summary>
        protected internal virtual void OnProductionUsed(double watts, double tickSeconds) { }
    }

    /// <summary>Energy storage. Charges from surplus, covers deficits.</summary>
    public class Battery : PowerNode
    {
        public Battery(IReadOnlyList<GridPos> cells, double capacityJoules, double maxRateWatts, double initialJoules = 0)
            : base(cells)
        {
            if (capacityJoules <= 0) throw new ArgumentOutOfRangeException(nameof(capacityJoules));
            CapacityJoules = capacityJoules;
            MaxRateWatts = maxRateWatts;
            StoredJoules = Math.Min(capacityJoules, Math.Max(0, initialJoules));
        }

        public double CapacityJoules { get; }
        public double MaxRateWatts { get; }
        public double StoredJoules { get; internal set; }
        public double ChargeFraction => StoredJoules / CapacityJoules;
    }

    /// <summary>Power pole: connects to other poles within <see cref="LinkRange"/> and powers nodes within <see cref="SupplyRadius"/>.</summary>
    public readonly struct PowerPole
    {
        public const int DefaultSupplyRadius = 2;
        public const int DefaultLinkRange = 7;

        public PowerPole(GridPos cell, int supplyRadius = DefaultSupplyRadius, int linkRange = DefaultLinkRange)
        {
            Cell = cell;
            SupplyRadius = supplyRadius;
            LinkRange = linkRange;
        }

        public readonly GridPos Cell;
        public readonly int SupplyRadius;
        public readonly int LinkRange;

        public bool Supplies(GridPos c) => Math.Abs(c.X - Cell.X) <= SupplyRadius && Math.Abs(c.Y - Cell.Y) <= SupplyRadius;

        public bool LinksTo(PowerPole other) =>
            Math.Max(Math.Abs(other.Cell.X - Cell.X), Math.Abs(other.Cell.Y - Cell.Y)) <= Math.Min(LinkRange, other.LinkRange);
    }
}
