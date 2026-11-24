using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>Who gets power first when a network is short: programs, defences and life support, then everything else.</summary>
    public enum PowerPriority : byte { Critical, Normal }

    /// <summary>What a power network did in the last tick (kW), by source and by priority. Read by programs (energy.stats()) and the machine panel.</summary>
    public sealed class PowerStats
    {
        public int Network;                       // id of the network: the lowest id among its poles, generators, panels, batteries, engines
        public float SolarKW, SteamKW, CoalKW, BatteryKW, ChargeKW;               // supplied, by source; batteries charging
        public float SolarCapacityKW, SteamCapacityKW, CoalCapacityKW, BatteryCapacityKW, CapacityKW;
        public float CriticalDemandKW, NormalDemandKW;
        public float CriticalSatisfaction = 1f, NormalSatisfaction = 1f;
        public float StoredKJ, StorageKJ;         // in the batteries, and what they can hold
        public int Nodes, Consumers;
        public float SuppliedKW => SolarKW + SteamKW + CoalKW + BatteryKW;
    }

    public sealed partial class Factory
    {
        sealed class Network
        {
            // every list is sorted by id, however the network was put together (built up, merged or split)
            public readonly List<PlacedBuilding> Nodes = new List<PlacedBuilding>(), Generators = new List<PlacedBuilding>(),
                Solars = new List<PlacedBuilding>(), Batteries = new List<PlacedBuilding>(), Engines = new List<PlacedBuilding>(),
                Consumers = new List<PlacedBuilding>();
            public float StoredKJ, CapacityKW, DemandKW, Satisfaction;
            public readonly PowerStats Stats = new PowerStats();
        }

        readonly Dictionary<int, Network> networkOf = new Dictionary<int, Network>();
        readonly List<Network> networks = new List<Network>();
        // nodes and consumers by square of PoleReach cells: every link (≤ PoleReach) and supply (≤ 4.5 + half a building) is in the 3×3 around
        readonly Dictionary<(int, int), List<PlacedBuilding>> nodeCells = new Dictionary<(int, int), List<PlacedBuilding>>(),
            consumerCells = new Dictionary<(int, int), List<PlacedBuilding>>();
        readonly List<Network> linked = new List<Network>();
        readonly Stack<PlacedBuilding> flood = new Stack<PlacedBuilding>();
        static readonly IComparer<PlacedBuilding> ById = Comparer<PlacedBuilding>.Create((p, q) => p.InstanceId.CompareTo(q.InstanceId));

        public static PowerPriority PriorityOf(BuildingDef def) =>
            def.Kind == BuildingKind.Computer || def.Kind == BuildingKind.Server || def.Kind == BuildingKind.Turret || def.Kind == BuildingKind.Heater
                ? PowerPriority.Critical : PowerPriority.Normal;

        static (int, int) Square(PlacedBuilding b) => ((int)Math.Floor(b.CenterX / PoleReach), (int)Math.Floor(b.CenterY / PoleReach));

        static void InsertSorted(List<PlacedBuilding> l, PlacedBuilding b)
        {
            int i = l.BinarySearch(b, ById);
            if (i < 0) l.Insert(~i, b);
        }

        static void AddTo(Dictionary<(int, int), List<PlacedBuilding>> cells, PlacedBuilding b)
        {
            var k = Square(b);
            if (!cells.TryGetValue(k, out var l)) cells[k] = l = new List<PlacedBuilding>();
            InsertSorted(l, b);
        }

        static void RemoveFrom(Dictionary<(int, int), List<PlacedBuilding>> cells, PlacedBuilding b)
        {
            var k = Square(b);
            if (!cells.TryGetValue(k, out var l)) return;
            RemoveSorted(l, b);
            if (l.Count == 0) cells.Remove(k);
        }

        static bool Supplies(PlacedBuilding node, PlacedBuilding consumer)
        {
            var (cw, ch) = consumer.Size;
            double reach = (node.Def.Kind == BuildingKind.Pole ? PoleSupply : GeneratorSupply) + Math.Max(cw, ch) / 2.0;
            return node.DistanceTo(consumer) <= reach;
        }

        static void AddMember(Network net, PlacedBuilding n)
        {
            InsertSorted(net.Nodes, n);
            switch (n.Def.Kind)
            {
                case BuildingKind.Generator: InsertSorted(net.Generators, n); break;
                case BuildingKind.Solar: InsertSorted(net.Solars, n); break;
                case BuildingKind.Battery: InsertSorted(net.Batteries, n); break;
                case BuildingKind.SteamEngine: InsertSorted(net.Engines, n); break;
            }
        }

        static void MergeInto(List<PlacedBuilding> to, List<PlacedBuilding> from) { if (from.Count == 0) return; to.AddRange(from); to.Sort(ById); }

        // ------------------------------------------------------------ keeping the networks up to date

        void PowerPlaced(PlacedBuilding b)
        {
            if (IsNode(b)) AddNode(b);
            if (IsConsumer(b)) { AddTo(consumerCells, b); Assign(b); }
        }

        void PowerRemoved(PlacedBuilding b)
        {
            if (IsConsumer(b))
            {
                RemoveFrom(consumerCells, b);
                if (networkOf.TryGetValue(b.InstanceId, out var net)) { RemoveSorted(net.Consumers, b); networkOf.Remove(b.InstanceId); }
            }
            if (IsNode(b)) RemoveNode(b);
        }

        void AddNode(PlacedBuilding n)
        {
            AddTo(nodeCells, n);
            linked.Clear();
            var (kx, ky) = Square(n);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (nodeCells.TryGetValue((kx + dx, ky + dy), out var l))
                        foreach (var m in l)
                            if (m != n && n.DistanceTo(m) <= PoleReach && networkOf.TryGetValue(m.InstanceId, out var mn) && !linked.Contains(mn)) linked.Add(mn);
            Network net;
            if (linked.Count == 0) { net = new Network(); networks.Add(net); }
            else
            {
                net = linked[0];
                for (int i = 1; i < linked.Count; i++)   // the new node joins networks that were apart
                {
                    var o = linked[i];
                    foreach (var x in o.Nodes) networkOf[x.InstanceId] = net;
                    foreach (var x in o.Consumers) networkOf[x.InstanceId] = net;
                    MergeInto(net.Nodes, o.Nodes); MergeInto(net.Generators, o.Generators); MergeInto(net.Solars, o.Solars);
                    MergeInto(net.Batteries, o.Batteries); MergeInto(net.Engines, o.Engines); MergeInto(net.Consumers, o.Consumers);
                    networks.Remove(o);
                }
            }
            networkOf[n.InstanceId] = net;
            AddMember(net, n);
            // consumers nobody supplied: the new node is the only one in reach, so it is theirs
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (consumerCells.TryGetValue((kx + dx, ky + dy), out var l))
                        foreach (var c in l)
                            if (!networkOf.ContainsKey(c.InstanceId) && Supplies(n, c)) { networkOf[c.InstanceId] = net; InsertSorted(net.Consumers, c); }
        }

        void RemoveNode(PlacedBuilding n)
        {
            RemoveFrom(nodeCells, n);
            if (!networkOf.TryGetValue(n.InstanceId, out var old)) return;
            networks.Remove(old);
            foreach (var x in old.Nodes) networkOf.Remove(x.InstanceId);
            foreach (var x in old.Consumers) networkOf.Remove(x.InstanceId);
            // what is left of the old network may now be several
            foreach (var start in old.Nodes)
            {
                if (start == n || networkOf.ContainsKey(start.InstanceId)) continue;
                var net = new Network();
                networks.Add(net);
                networkOf[start.InstanceId] = net;
                flood.Push(start);
                while (flood.Count > 0)
                {
                    var p = flood.Pop();
                    net.Nodes.Add(p);
                    switch (p.Def.Kind)
                    {
                        case BuildingKind.Generator: net.Generators.Add(p); break;
                        case BuildingKind.Solar: net.Solars.Add(p); break;
                        case BuildingKind.Battery: net.Batteries.Add(p); break;
                        case BuildingKind.SteamEngine: net.Engines.Add(p); break;
                    }
                    var (kx, ky) = Square(p);
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                            if (nodeCells.TryGetValue((kx + dx, ky + dy), out var l))
                                foreach (var m in l)
                                    if (!networkOf.ContainsKey(m.InstanceId) && p.DistanceTo(m) <= PoleReach) { networkOf[m.InstanceId] = net; flood.Push(m); }
                }
                net.Nodes.Sort(ById); net.Generators.Sort(ById); net.Solars.Sort(ById); net.Batteries.Sort(ById); net.Engines.Sort(ById);
            }
            foreach (var c in old.Consumers) Assign(c);
        }

        /// <summary>A consumer belongs to the network of the oldest node that reaches it (or to none).</summary>
        void Assign(PlacedBuilding c)
        {
            PlacedBuilding best = null;
            var (kx, ky) = Square(c);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (nodeCells.TryGetValue((kx + dx, ky + dy), out var l))
                        foreach (var n in l)
                        {
                            if (best != null && n.InstanceId > best.InstanceId) break;   // lists are by id: nothing older further on
                            if (Supplies(n, c)) best = n;
                        }
            if (best == null) return;
            var net = networkOf[best.InstanceId];
            networkOf[c.InstanceId] = net;
            InsertSorted(net.Consumers, c);
        }

        /// <summary>Builds every network from nothing (the reference the incremental updates must always match; tests compare them).</summary>
        public void RebuildPowerNetworks()
        {
            networks.Clear(); networkOf.Clear(); nodeCells.Clear(); consumerCells.Clear();
            foreach (var n in nodeList) AddNode(n);
            foreach (var c in consumers) { AddTo(consumerCells, c); Assign(c); }
        }

        /// <summary>Id of the network a building is on (its lowest node id), or 0.</summary>
        public int NetworkIdOf(PlacedBuilding b) => networkOf.TryGetValue(b.InstanceId, out var n) && n.Nodes.Count > 0 ? n.Nodes[0].InstanceId : 0;

        public int NetworkCount => networks.Count;

        // ------------------------------------------------------------ each tick

        /// <summary>Power, prepare: reset what is computed each tick.</summary>
        void PreparePower()
        {
            foreach (var b in consumers) b.State.PowerSatisfaction = 0f;
        }

        /// <summary>Power, update: balance each network. Supply: sunlight, then steam already made, then coal, then batteries.
        /// Demand: critical machines (computers, servers, turrets, heaters) are served before the rest.</summary>
        void UpdatePower(float dt)
        {
            float inv = 1f / Math.Max(dt, 1e-4f);
            foreach (var net in networks)
            {
                running.Clear();
                foreach (var g in net.Generators) { var gs = (GeneratorState)g.State; gs.LoadKW = 0; if (GeneratorCanRun(gs)) running.Add(gs); }
                float solarKW = net.Solars.Count * SolarKW * SolarFactor;
                foreach (var s in net.Solars) ((SolarState)s.State).OutputKW = SolarKW * SolarFactor;
                float genKW = running.Count * GeneratorKW, battKW = 0, engKW = 0;
                foreach (var e in net.Engines) engKW += EngineCapacityKW(e, dt);
                foreach (var b in net.Batteries) battKW += Math.Min(BatteryState.MaxKW, ((BatteryState)b.State).StoredKJ * inv);
                net.CapacityKW = solarKW + engKW + genKW + battKW;
                float critical = 0, normal = 0;
                foreach (var c in net.Consumers) { float d = DemandOf(c); if (PriorityOf(c.Def) == PowerPriority.Critical) critical += d; else normal += d; }
                net.DemandKW = critical + normal;

                float rem = net.DemandKW;
                float useSolar = Math.Min(rem, solarKW); rem -= useSolar;
                float useEng = Math.Min(rem, engKW); rem -= useEng;
                foreach (var e in net.Engines) { float cap = EngineCapacityKW(e, dt); RunEngine(e, engKW > 0 ? useEng * cap / engKW : 0f, dt); }
                float useGen = Math.Min(rem, genKW); rem -= useGen;
                float useBatt = Math.Min(rem, battKW); rem -= useBatt;
                float supplied = net.DemandKW - rem;
                net.Satisfaction = net.DemandKW <= 0 ? (net.CapacityKW > 0 ? 1f : 0f) : 1f - rem / net.DemandKW;
                float satCritical = critical > 0 ? Math.Min(1f, supplied / critical) : net.Satisfaction;
                float satNormal = normal > 0 ? Math.Max(0f, Math.Min(1f, (supplied - critical * (critical > 0 ? satCritical : 0f)) / normal)) : net.Satisfaction;

                // batteries: discharge what was used, charge from spare sunlight (never burn coal just to fill them)
                float spare = solarKW - useSolar, charged = 0, battUsed = useBatt;
                net.StoredKJ = 0;
                foreach (var b in net.Batteries)
                {
                    var bs = (BatteryState)b.State;
                    float take = useBatt > 0 ? Math.Min(useBatt, Math.Min(BatteryState.MaxKW, bs.StoredKJ * inv)) : 0;
                    useBatt -= take; bs.StoredKJ -= take * dt;
                    float give = Math.Min(spare, Math.Min(BatteryState.MaxKW, (BatteryState.CapacityKJ - bs.StoredKJ) * inv));
                    spare -= give; charged += give; bs.StoredKJ = Math.Max(0, Math.Min(BatteryState.CapacityKJ, bs.StoredKJ + give * dt));
                    int pct = (int)Math.Round(bs.StoredKJ / BatteryState.CapacityKJ * 100), rate = (int)Math.Round(take > 0 ? -take : give);
                    if (pct != bs.ShownPct || rate != bs.ShownRate)   // the text only changes when the numbers do: no string per tick
                    {
                        bs.ShownPct = pct; bs.ShownRate = rate;
                        bs.Status = $"{pct}% ({(rate < 0 ? rate.ToString() : rate > 0 ? "+" + rate : "0")} kW)";
                    }
                    net.StoredKJ += bs.StoredKJ;
                }

                foreach (var gs in running)
                {
                    float share = useGen / running.Count;
                    gs.LoadKW = share;
                    gs.BurnLeftKJ -= share * dt;
                    while (gs.BurnLeftKJ <= 0 && share > 0 && gs.Fuel.TryRemove(ItemDatabase.Coal, 1)) gs.BurnLeftKJ += CoalEnergyKJ;
                    if (gs.BurnLeftKJ < 0) gs.BurnLeftKJ = 0;
                    int kw = gs.BurnLeftKJ > 0 || gs.Fuel.Count(ItemDatabase.Coal) > 0 ? (int)Math.Round(share) : -1;
                    if (kw != gs.ShownKW) { gs.ShownKW = kw; gs.Status = kw < 0 ? "no fuel" : kw + " kW"; }
                }
                foreach (var c in net.Consumers) c.State.PowerSatisfaction = PriorityOf(c.Def) == PowerPriority.Critical ? satCritical : satNormal;

                var st = net.Stats;
                st.Network = net.Nodes.Count > 0 ? net.Nodes[0].InstanceId : 0;
                st.SolarKW = useSolar; st.SteamKW = useEng; st.CoalKW = useGen; st.BatteryKW = battUsed; st.ChargeKW = charged;
                st.SolarCapacityKW = solarKW; st.SteamCapacityKW = engKW; st.CoalCapacityKW = genKW; st.BatteryCapacityKW = battKW; st.CapacityKW = net.CapacityKW;
                st.CriticalDemandKW = critical; st.NormalDemandKW = normal; st.CriticalSatisfaction = satCritical; st.NormalSatisfaction = satNormal;
                st.StoredKJ = net.StoredKJ; st.StorageKJ = net.Batteries.Count * BatteryState.CapacityKJ;
                st.Nodes = net.Nodes.Count; st.Consumers = net.Consumers.Count;
            }
        }

        /// <summary>True when the building is on a network with something that can supply power now.</summary>
        public bool HasPower(PlacedBuilding b)
        {
            if (!networkOf.TryGetValue(b.InstanceId, out var net)) return false;
            foreach (var g in net.Generators) if (GeneratorCanRun((GeneratorState)g.State)) return true;
            if (net.Solars.Count > 0 && SolarFactor > 0.01f) return true;
            foreach (var e in net.Engines) if (((SteamEngineState)e.State).Steam.Amount > 0) return true;
            foreach (var x in net.Batteries) if (((BatteryState)x.State).StoredKJ > 1f) return true;
            return false;
        }

        public (float capacityKW, float demandKW) NetworkStats(PlacedBuilding b) =>
            networkOf.TryGetValue(b.InstanceId, out var n) ? (n.CapacityKW, n.DemandKW) : (0f, 0f);

        /// <summary>The last tick's figures for the network a building is on, or null.</summary>
        public PowerStats NetworkDetails(PlacedBuilding b) => networkOf.TryGetValue(b.InstanceId, out var n) ? n.Stats : null;

        /// <summary>Puts energy into the batteries of b's network (lightning). Returns what fitted.</summary>
        public float StoreEnergy(PlacedBuilding b, float kj)
        {
            if (!networkOf.TryGetValue(b.InstanceId, out var n)) return 0f;
            float stored = 0;
            foreach (var bat in n.Batteries)
            {
                var s = (BatteryState)bat.State;
                float add = Math.Min(kj - stored, BatteryState.CapacityKJ - s.StoredKJ);
                if (add > 0) { s.StoredKJ += add; stored += add; }
            }
            return stored;
        }

        /// <summary>Energy stored in the batteries of a building's network (kJ).</summary>
        public float NetworkStoredKJ(PlacedBuilding b) => networkOf.TryGetValue(b.InstanceId, out var n) ? n.StoredKJ : 0f;

        /// <summary>All networks together: supply capacity, demand served, stored energy (for the HUD).</summary>
        public (float capacityKW, float usedKW, float storedKJ) Totals()
        {
            float cap = 0, used = 0, kj = 0;
            foreach (var n in networks) { cap += n.CapacityKW; used += Math.Min(n.CapacityKW, n.DemandKW); kj += n.StoredKJ; }
            return (cap, used, kj);
        }
    }
}
