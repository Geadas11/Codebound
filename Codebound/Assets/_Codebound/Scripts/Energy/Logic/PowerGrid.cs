using System;
using System.Collections.Generic;

namespace Codebound.Energy
{
    /// <summary>Per-network figures from the last tick (for HUD, programs and debugging).</summary>
    public sealed class NetworkStats
    {
        public int Id;
        public double ProductionWatts;   // available from generators
        public double DemandWatts;
        public double SuppliedWatts;     // actually delivered to consumers
        public double StoredJoules;
        public double CapacityJoules;
        public int PoleCount;
        public int NodeCount;
        public double Satisfaction => DemandWatts <= 0 ? 1 : SuppliedWatts / DemandWatts;
    }

    /// <summary>
    /// Builds electrical networks from poles (union-find over pole links), attaches nodes, and balances
    /// production, demand and storage every tick. Consumers on an underpowered network all receive the
    /// same fraction of their demand (brown-out), which slows machines down instead of stopping them at random.
    /// </summary>
    public sealed class PowerGrid
    {
        private readonly List<PowerPole> _poles = new List<PowerPole>();
        private readonly List<PowerNode> _nodes = new List<PowerNode>();
        private readonly List<NetworkStats> _stats = new List<NetworkStats>();
        private int[] _poleNetwork = Array.Empty<int>();
        private bool _dirty = true;

        public IReadOnlyList<PowerPole> Poles => _poles;
        public IReadOnlyList<PowerNode> Nodes => _nodes;
        public IReadOnlyList<NetworkStats> Networks => _stats;

        public void AddPole(PowerPole pole) { _poles.Add(pole); _dirty = true; }

        public bool RemovePoleAt(Codebound.Core.GridPos cell)
        {
            int i = _poles.FindIndex(p => p.Cell == cell);
            if (i < 0) return false;
            _poles.RemoveAt(i);
            _dirty = true;
            return true;
        }

        public void AddNode(PowerNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!_nodes.Contains(node)) { _nodes.Add(node); _dirty = true; }
        }

        public bool RemoveNode(PowerNode node)
        {
            if (!_nodes.Remove(node)) return false;
            node.NetworkId = -1;
            node.Satisfaction = 0;
            _dirty = true;
            return true;
        }

        public NetworkStats GetNetwork(int id) => id >= 0 && id < _stats.Count ? _stats[id] : null;

        /// <summary>Recomputes network membership if poles or nodes changed.</summary>
        public void Rebuild()
        {
            if (!_dirty) return;
            _dirty = false;
            int n = _poles.Count;
            var parent = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
            int Find(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                if (_poles[i].LinksTo(_poles[j])) parent[Find(i)] = Find(j);

            // Compact root ids to 0..k-1.
            var rootToId = new Dictionary<int, int>();
            _poleNetwork = new int[n];
            _stats.Clear();
            for (int i = 0; i < n; i++)
            {
                int r = Find(i);
                if (!rootToId.TryGetValue(r, out int id))
                {
                    id = rootToId.Count;
                    rootToId[r] = id;
                    _stats.Add(new NetworkStats { Id = id });
                }
                _poleNetwork[i] = id;
                _stats[id].PoleCount++;
            }

            foreach (var node in _nodes)
            {
                node.NetworkId = -1;
                for (int p = 0; p < n && node.NetworkId < 0; p++)
                    foreach (var c in node.Cells)
                        if (_poles[p].Supplies(c)) { node.NetworkId = _poleNetwork[p]; break; }
                if (node.NetworkId >= 0) _stats[node.NetworkId].NodeCount++;
            }
        }

        /// <summary>Balances every network for one tick of <paramref name="tickSeconds"/>.</summary>
        public void Tick(double tickSeconds)
        {
            if (tickSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(tickSeconds));
            Rebuild();
            foreach (var s in _stats)
            {
                s.ProductionWatts = s.DemandWatts = s.SuppliedWatts = s.StoredJoules = s.CapacityJoules = 0;
            }

            foreach (var node in _nodes)
            {
                if (node.NetworkId < 0) { node.Satisfaction = 0; continue; }
                var s = _stats[node.NetworkId];
                s.ProductionWatts += Math.Max(0, node.ProductionWatts);
                s.DemandWatts += Math.Max(0, node.DemandWatts);
            }

            foreach (var s in _stats) Balance(s, tickSeconds);
        }

        private void Balance(NetworkStats s, double dt)
        {
            double production = s.ProductionWatts, demand = s.DemandWatts;
            double fromGenerators = Math.Min(production, demand);
            double deficit = demand - fromGenerators;
            double surplus = production - fromGenerators;

            // Batteries of this network.
            var batteries = new List<Battery>();
            foreach (var node in _nodes)
                if (node.NetworkId == s.Id && node is Battery b) batteries.Add(b);

            double fromBatteries = 0;
            if (deficit > 0)
            {
                foreach (var b in batteries)
                {
                    double canWatts = Math.Min(b.MaxRateWatts, b.StoredJoules / dt);
                    double take = Math.Min(canWatts, deficit - fromBatteries);
                    b.StoredJoules -= take * dt;
                    fromBatteries += take;
                    if (fromBatteries >= deficit) break;
                }
            }

            double chargeWatts = 0;
            if (surplus > 0)
            {
                foreach (var b in batteries)
                {
                    double room = (b.CapacityJoules - b.StoredJoules) / dt;
                    double put = Math.Min(Math.Min(b.MaxRateWatts, room), surplus - chargeWatts);
                    b.StoredJoules += put * dt;
                    chargeWatts += put;
                    if (chargeWatts >= surplus) break;
                }
            }

            s.SuppliedWatts = fromGenerators + fromBatteries;
            double satisfaction = demand <= 0 ? 1 : s.SuppliedWatts / demand;
            double producedUsed = fromGenerators + chargeWatts;
            double usedFraction = production <= 0 ? 0 : producedUsed / production;

            foreach (var node in _nodes)
            {
                if (node.NetworkId != s.Id) continue;
                node.Satisfaction = node.DemandWatts > 0 ? satisfaction : 1;
                if (node.ProductionWatts > 0) node.OnProductionUsed(node.ProductionWatts * usedFraction, dt);
                if (node is Battery b) { s.StoredJoules += b.StoredJoules; s.CapacityJoules += b.CapacityJoules; }
            }
        }
    }
}
