using System;
using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>What a chest is to the logistic robots.</summary>
    public enum LogisticRole : byte { None, Provider, Requester }

    public enum RobotTask : byte { ToPickup, ToDrop, Return }

    /// <summary>A logistic robot in flight (robots resting in a roboport are just items in it).</summary>
    public sealed class LogisticRobot
    {
        public double X, Y;
        public RobotTask Task;
        public Cell From, To, Home;   // provider, requester, the roboport it flies back to
        public string Item;
        public int Count;             // planned (on the way to pick up) or carried
    }

    /// <summary>
    /// A roboport: its robots (items) rest and charge here, it covers a square of ±<see cref="Radius"/> cells and roboports
    /// whose squares touch form one logistic network. Robots fly items from provider chests (then from the chests of a
    /// powered storage hub) to requester chests. Leftovers a robot could not deliver go into the roboport's cargo bay.
    /// </summary>
    public sealed class RoboportState : MachineState
    {
        public const int Radius = 12;
        public readonly Inventory Robots = new Inventory(1), Cargo = new Inventory(8);
        public RoboportState() { Status = "no robots"; }
        public override bool TryInsert(string itemId, int count) => itemId == ItemDatabase.Robot && Robots.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId) => itemId == ItemDatabase.Robot;
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("robots", Robots); yield return ("cargo", Cargo); } }
        public override Inventory Extractable => Cargo;
    }

    public sealed partial class Factory
    {
        public const double RobotSpeed = 3.0;    // cells per second at full power
        public const int RobotCapacity = 4, MaxDispatchPerTick = 8;
        public const float RoboportKW = 30f;

        sealed class LogisticNet
        {
            public readonly List<PlacedBuilding> Roboports = new List<PlacedBuilding>(), Providers = new List<PlacedBuilding>(),
                Storage = new List<PlacedBuilding>(), Requesters = new List<PlacedBuilding>();
            public readonly List<PlacedBuilding> Hubs = new List<PlacedBuilding>();   // whose chests are in Storage
        }

        /// <summary>Robots in flight, in the order they took off (part of the save and of the simulation order).</summary>
        public readonly List<LogisticRobot> Robots = new List<LogisticRobot>();
        readonly List<PlacedBuilding> roboports = new List<PlacedBuilding>();
        readonly List<LogisticNet> logisticNets = new List<LogisticNet>();
        readonly Dictionary<int, LogisticNet> logisticOf = new Dictionary<int, LogisticNet>();
        readonly Dictionary<(Cell, string), int> reserved = new Dictionary<(Cell, string), int>(), incoming = new Dictionary<(Cell, string), int>();
        bool logisticsDirty = true;

        static bool Covers(PlacedBuilding port, PlacedBuilding b) =>
            Math.Abs(b.CenterX - port.CenterX) <= RoboportState.Radius && Math.Abs(b.CenterY - port.CenterY) <= RoboportState.Radius;

        static bool Covers(PlacedBuilding port, double x, double y) =>
            Math.Abs(x - port.CenterX) <= RoboportState.Radius && Math.Abs(y - port.CenterY) <= RoboportState.Radius;

        void RebuildLogistics()
        {
            logisticNets.Clear(); logisticOf.Clear();
            var stack = new Stack<PlacedBuilding>();
            foreach (var start in roboports)
            {
                if (logisticOf.ContainsKey(start.InstanceId)) continue;
                var net = new LogisticNet();
                logisticNets.Add(net);
                logisticOf[start.InstanceId] = net; stack.Push(start);
                while (stack.Count > 0)
                {
                    var p = stack.Pop();
                    net.Roboports.Add(p);
                    foreach (var q in roboports)
                        if (!logisticOf.ContainsKey(q.InstanceId) && Math.Abs(q.CenterX - p.CenterX) <= 2 * RoboportState.Radius && Math.Abs(q.CenterY - p.CenterY) <= 2 * RoboportState.Radius)
                        { logisticOf[q.InstanceId] = net; stack.Push(q); }
                }
                net.Roboports.Sort(ById);
            }
            if (logisticNets.Count == 0) { logisticsDirty = false; return; }
            // chests and hubs join the network of the first (oldest) roboport that covers them
            foreach (var b in grid.Buildings)
            {
                if (!(b.State is StorageState) && !(b.State is StorageHubState)) continue;
                LogisticNet net = null;
                foreach (var port in roboports) if (Covers(port, b)) { net = logisticOf[port.InstanceId]; break; }
                if (net == null) continue;
                if (b.State is StorageHubState) { net.Hubs.Add(b); continue; }
                var role = ((StorageState)b.State).Role;
                if (role == LogisticRole.Provider) net.Providers.Add(b);
                else if (role == LogisticRole.Requester) net.Requesters.Add(b);
                logisticOf[b.InstanceId] = net;
            }
            // a hub in the network lends it its chests (plain chests within the hub's radius)
            foreach (var net in logisticNets)
                foreach (var hub in net.Hubs)
                {
                    int r = (int)Math.Ceiling(StorageHubState.Radius) + 2, cx = (int)Math.Floor(hub.CenterX), cy = (int)Math.Floor(hub.CenterY);
                    for (int dx = -r; dx <= r; dx++)
                        for (int dy = -r; dy <= r; dy++)
                            if (grid.At(new Cell(cx + dx, cy + dy)) is PlacedBuilding c && c.State is StorageState s && s.Role == LogisticRole.None
                                && c.DistanceTo(hub) <= StorageHubState.Radius && !net.Storage.Contains(c)) net.Storage.Add(c);
                    net.Storage.Sort(ById);
                }
            logisticsDirty = false;
        }

        static void Count(Dictionary<(Cell, string), int> d, Cell c, string item, int n) => d[(c, item)] = (d.TryGetValue((c, item), out var had) ? had : 0) + n;

        /// <summary>Robots, each tick: rebuild networks if chests or roboports changed, move every robot, then send idle ones on new jobs.</summary>
        void TickRobots(float dt)
        {
            if (logisticsDirty) RebuildLogistics();
            if (roboports.Count == 0 && Robots.Count == 0) return;
            for (int i = 0; i < Robots.Count; i++)
                if (MoveRobot(Robots[i], dt)) { Robots.RemoveAt(i); i--; }
            if (logisticNets.Count == 0) return;
            reserved.Clear(); incoming.Clear();
            foreach (var r in Robots)
            {
                if (r.Task == RobotTask.ToPickup) Count(reserved, r.From, r.Item, r.Count);
                if (r.Task != RobotTask.Return) Count(incoming, r.To, r.Item, r.Count);
            }
            foreach (var net in logisticNets) Dispatch(net);
            foreach (var p in roboports)
            {
                var rs = (RoboportState)p.State;
                int n = rs.Robots.Count(ItemDatabase.Robot);
                rs.Status = rs.PowerSatisfaction <= 0 ? "no power" : n == 0 ? "no robots" : n + " robots ready";
            }
        }

        PlacedBuilding NearestRoboport(double x, double y, bool covering)
        {
            PlacedBuilding best = null; double bd = double.MaxValue;
            foreach (var p in roboports)
            {
                if (covering && !Covers(p, x, y)) continue;
                double d = (p.CenterX - x) * (p.CenterX - x) + (p.CenterY - y) * (p.CenterY - y);
                if (d < bd) { bd = d; best = p; }
            }
            return best;
        }

        /// <summary>Moves a robot one tick; true when it has landed (and is gone from the list).</summary>
        bool MoveRobot(LogisticRobot r, float dt)
        {
            var home = grid.At(r.Home);
            float power = home?.State is RoboportState hs ? hs.PowerSatisfaction : 0f;
            double step = RobotSpeed * dt * Math.Max(0.25, power);   // without power robots crawl, they never stop in mid-air
            PlacedBuilding target = r.Task == RobotTask.ToPickup ? grid.At(r.From) : r.Task == RobotTask.ToDrop ? grid.At(r.To) : home;
            bool valid = r.Task == RobotTask.Return ? target?.State is RoboportState : target?.State is StorageState;
            if (!valid)
            {
                if (r.Task != RobotTask.Return) { r.Task = RobotTask.Return; }   // the chest is gone: go back
                var port = NearestRoboport(r.X, r.Y, true) ?? NearestRoboport(r.X, r.Y, false);
                if (port == null) return false;   // no roboport anywhere: it hovers until one is built
                r.Home = port.Origin;
                return false;
            }
            double dx = target.CenterX - r.X, dy = target.CenterY - r.Y, dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > step) { r.X += dx / dist * step; r.Y += dy / dist * step; return false; }
            r.X = target.CenterX; r.Y = target.CenterY;
            switch (r.Task)
            {
                case RobotTask.ToPickup:
                {
                    var from = ((StorageState)target.State).Items;
                    int take = Math.Min(r.Count, from.Count(r.Item));
                    if (take > 0) from.TryRemove(r.Item, take);
                    r.Count = take;
                    r.Task = take > 0 ? RobotTask.ToDrop : RobotTask.Return;
                    if (take == 0) r.Item = null;
                    return false;
                }
                case RobotTask.ToDrop:
                {
                    int left = ((StorageState)target.State).Items.Add(r.Item, r.Count);
                    r.Count = left;
                    if (left == 0) r.Item = null;
                    r.Task = RobotTask.Return;
                    var port = NearestRoboport(r.X, r.Y, true) ?? home;
                    if (port != null) r.Home = port.Origin;
                    return false;
                }
                default:
                {
                    var rs = (RoboportState)target.State;
                    if (r.Item != null && r.Count > 0)
                    {
                        int left = rs.Cargo.Add(r.Item, r.Count);
                        if (left > 0) { r.Count = left; return false; }   // cargo bay full: waits on the roboport until there is room
                    }
                    rs.Robots.Add(ItemDatabase.Robot, 1);
                    return true;
                }
            }
        }

        void Dispatch(LogisticNet net)
        {
            int sent = 0;
            foreach (var req in net.Requesters)
            {
                var rq = (StorageState)req.State;
                if (rq.Requests.Count == 0) continue;
                foreach (var item in rq.RequestOrder)
                {
                    int want = rq.Requests[item];
                    int deficit = want - rq.Items.Count(item) - (incoming.TryGetValue((req.Origin, item), out var inc) ? inc : 0);
                    while (deficit > 0 && sent < MaxDispatchPerTick)
                    {
                        var (src, avail) = Supplier(net, req, item);
                        if (src == null) break;
                        var port = ReadyRoboport(net, src);
                        if (port == null) return;   // no robot ready in this network
                        int n = Math.Min(Math.Min(deficit, RobotCapacity), avail);
                        ((RoboportState)port.State).Robots.TryRemove(ItemDatabase.Robot, 1);
                        Robots.Add(new LogisticRobot { X = port.CenterX, Y = port.CenterY, Task = RobotTask.ToPickup, From = src.Origin, To = req.Origin, Home = port.Origin, Item = item, Count = n });
                        Count(reserved, src.Origin, item, n); Count(incoming, req.Origin, item, n);
                        deficit -= n; sent++;
                    }
                    if (sent >= MaxDispatchPerTick) return;
                }
            }
        }

        /// <summary>The nearest provider chest with the item not already promised to another robot; then the hub's chests.</summary>
        (PlacedBuilding, int) Supplier(LogisticNet net, PlacedBuilding req, string item)
        {
            PlacedBuilding best = null; int bestAvail = 0; double bd = double.MaxValue;
            for (int pass = 0; pass < 2 && best == null; pass++)
            {
                var list = pass == 0 ? net.Providers : net.Storage;
                if (pass == 1 && !HubPowered(net)) break;
                foreach (var p in list)
                {
                    int avail = ((StorageState)p.State).Items.Count(item) - (reserved.TryGetValue((p.Origin, item), out var res) ? res : 0);
                    if (avail <= 0) continue;
                    double d = (p.CenterX - req.CenterX) * (p.CenterX - req.CenterX) + (p.CenterY - req.CenterY) * (p.CenterY - req.CenterY);
                    if (d < bd) { bd = d; best = p; bestAvail = avail; }
                }
            }
            return (best, bestAvail);
        }

        static bool HubPowered(LogisticNet net)
        {
            foreach (var h in net.Hubs) if (h.State.PowerSatisfaction > 0) return true;
            return false;
        }

        static PlacedBuilding ReadyRoboport(LogisticNet net, PlacedBuilding near)
        {
            PlacedBuilding best = null; double bd = double.MaxValue;
            foreach (var p in net.Roboports)
            {
                var rs = (RoboportState)p.State;
                if (rs.PowerSatisfaction <= 0 || rs.Robots.Count(ItemDatabase.Robot) == 0) continue;
                double d = (p.CenterX - near.CenterX) * (p.CenterX - near.CenterX) + (p.CenterY - near.CenterY) * (p.CenterY - near.CenterY);
                if (d < bd) { bd = d; best = p; }
            }
            return best;
        }

        /// <summary>For programs, signals and the panel: robots and items of the network a building belongs to (null when none).</summary>
        public Dictionary<string, object> LogisticInfo(PlacedBuilding b)
        {
            if (logisticsDirty) RebuildLogistics();
            if (b == null || !logisticOf.TryGetValue(b.InstanceId, out var net)) return null;
            int ready = 0, flying = 0;
            foreach (var p in net.Roboports) ready += ((RoboportState)p.State).Robots.Count(ItemDatabase.Robot);
            foreach (var r in Robots) { var h = grid.At(r.Home); if (h != null && logisticOf.TryGetValue(h.InstanceId, out var hn) && hn == net) flying++; }
            var items = new Dictionary<string, object>();
            foreach (var (k, v) in NetworkItems(net)) items[k] = v;
            return new Dictionary<string, object> { { "roboports", net.Roboports.Count }, { "robotsReady", ready }, { "robotsFlying", flying },
                { "providers", net.Providers.Count }, { "requesters", net.Requesters.Count }, { "storageChests", net.Storage.Count }, { "items", items } };
        }

        IEnumerable<(string, int)> NetworkItems(LogisticNet net)
        {
            var sum = new SortedDictionary<string, int>(StringComparer.Ordinal);
            bool hub = HubPowered(net);
            foreach (var list in new[] { net.Providers, net.Storage })
            {
                if (list == net.Storage && !hub) continue;
                foreach (var p in list)
                {
                    var inv = ((StorageState)p.State).Items;
                    for (int i = 0; i < inv.SlotCount; i++) if (!inv[i].IsEmpty) sum[inv[i].ItemId] = (sum.TryGetValue(inv[i].ItemId, out var n) ? n : 0) + inv[i].Count;
                }
            }
            foreach (var kv in sum) yield return (kv.Key, kv.Value);
        }

        /// <summary>A roboport on a signal network sends the items its logistic network can supply, and R = robots ready.</summary>
        void RoboportSignals(PlacedBuilding b, List<SignalNet> outs)
        {
            if (logisticsDirty) RebuildLogistics();
            if (!logisticOf.TryGetValue(b.InstanceId, out var net)) return;
            int ready = 0;
            foreach (var p in net.Roboports) ready += ((RoboportState)p.State).Robots.Count(ItemDatabase.Robot);
            foreach (var n in outs)
            {
                Add(n.Next, "R", ready);
                foreach (var (k, v) in NetworkItems(net)) Add(n.Next, k, v);
            }
        }

        // ------------------------------------------------------------ save
        public IEnumerable<string> RobotSaveLines()
        {
            var inv = CultureInfo.InvariantCulture;
            foreach (var r in Robots)
                yield return $"lrobot {(int)r.Task} {r.X.ToString("R", inv)} {r.Y.ToString("R", inv)} {r.From.X} {r.From.Y} {r.To.X} {r.To.Y} {r.Home.X} {r.Home.Y} {r.Item ?? "-"} {r.Count}";
        }

        public void LoadRobot(string[] p)
        {
            var inv = CultureInfo.InvariantCulture;
            if (p.Length < 12) return;
            int task = int.Parse(p[1], inv);
            if (task < 0 || task > 2) return;
            string item = p[10] == "-" ? null : p[10];
            if (item != null && !ItemDatabase.Exists(item)) item = null;
            int count = Math.Max(0, int.Parse(p[11], inv));
            Robots.Add(new LogisticRobot { Task = (RobotTask)task, X = double.Parse(p[2], inv), Y = double.Parse(p[3], inv),
                From = new Cell(int.Parse(p[4], inv), int.Parse(p[5], inv)), To = new Cell(int.Parse(p[6], inv), int.Parse(p[7], inv)),
                Home = new Cell(int.Parse(p[8], inv), int.Parse(p[9], inv)), Item = item, Count = item == null ? 0 : count });
        }
    }
}
