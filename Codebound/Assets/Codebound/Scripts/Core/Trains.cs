using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Codebound.Core
{
    /// <summary>A rail cell (rails that touch are connected; corners and junctions follow from the neighbours). A rail
    /// signal is a rail cell that splits the track into blocks: a train only passes it in the direction it points (build it
    /// rotated), and only when the block beyond is free. Signals pointing the same way round a loop make it one-way.</summary>
    public sealed class RailState : MachineState
    {
        public readonly bool Signal;
        public RailState(bool signal) { Signal = signal; Status = signal ? "green" : "rail"; }
    }

    /// <summary>
    /// A named train stop next to a rail. While a train waits at it, the stop is the train's loading bay: arms that put
    /// into the stop fill the wagons (coal tops up the locomotive first), arms that take from it empty them.
    /// </summary>
    public sealed class TrainStopState : MachineState
    {
        public string Name { get; private set; } = "";
        internal Factory Owner; internal PlacedBuilding Self;
        public TrainStopState() { Status = "no name"; }

        /// <summary>The waiting train's depot, or null.</summary>
        public TrainDepotState Train => Owner?.WaitingTrainAt(Self);

        public string Rename(string text)
        {
            string n = (text ?? "").Trim();
            if (n.Length > 24) return "a stop name has at most 24 characters";
            foreach (char c in n) if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_')) return "use a-z, 0-9 and _ (e.g. iron_mine)";
            if (n != Name) { Name = n; Status = n.Length == 0 ? "no name" : n; Touch(); }
            return null;
        }

        public override bool TryInsert(string itemId, int count)
        {
            var t = Train;
            if (t == null) return false;
            if (itemId == ItemDatabase.Coal && t.Fuel.Count(ItemDatabase.Coal) < 20 && t.Fuel.TryAddAll(new[] { new ItemStack(itemId, count) })) return true;
            for (int i = 0; i < t.WagonCount; i++) if (t.Wagons[i].TryAddAll(new[] { new ItemStack(itemId, count) })) return true;
            return false;
        }
        public override bool AcceptsItem(string itemId) => Train != null;
        public override Inventory Extractable
        {
            get
            {
                var t = Train;
                if (t == null) return null;
                for (int i = 0; i < t.WagonCount; i++) if (TrainDepotState.HasItems(t.Wagons[i])) return t.Wagons[i];
                return null;
            }
        }
        public override void SaveExtra(IDictionary<string, string> d) { if (Name.Length > 0) d["name"] = Name; }
        public override void LoadExtra(string k, string v) { if (k == "name") Rename(v); }
    }

    /// <summary>One stop of a schedule: go to a station, then wait until the condition holds.</summary>
    public sealed class ScheduleEntry
    {
        public string Station, Wait;      // Wait: "full", "empty", a number of seconds, or a cargo condition like iron_ore>=100
        public float Seconds; public SignalCondition Cond;
        public string Text => Station + ":" + Wait;
    }

    public enum TrainPhase : byte { Moving, Waiting }

    /// <summary>
    /// A train depot: put a locomotive (and up to 4 cargo wagons) and coal into it and give it a schedule
    /// ("iron_mine:full,base:empty"); the train starts on the rail in front of the depot and runs the schedule for ever.
    /// The depot keeps everything the train is made of and carries (so it is saved, shown and refunded with it).
    /// </summary>
    public sealed class TrainDepotState : MachineState
    {
        public const int MaxWagons = 4, WagonSlots = 8;
        public readonly Inventory Garage = new Inventory(5), Fuel = new Inventory(1);
        public readonly Inventory[] Wagons = new Inventory[MaxWagons];
        public readonly List<ScheduleEntry> Schedule = new List<ScheduleEntry>();
        public string ScheduleText { get; private set; } = "";

        // the train (when it exists): its cells, head first; how far the head is into the next cell
        public bool HasTrain;
        public readonly List<Cell> Trail = new List<Cell>();
        public bool HasCameFrom; public Cell CameFrom;
        public float Progress, Waited, BurnLeftKJ;
        public int Index; public TrainPhase Phase;

        public TrainDepotState()
        {
            for (int i = 0; i < MaxWagons; i++) Wagons[i] = new Inventory(WagonSlots);
            Status = "no locomotive";
        }

        public int WagonCount => Math.Min(MaxWagons, Garage.Count(ItemDatabase.CargoWagon));
        public int Length => 1 + WagonCount;

        public override bool TryInsert(string itemId, int count)
        {
            if (itemId == ItemDatabase.Coal) return Fuel.TryAddAll(new[] { new ItemStack(itemId, count) });
            if (itemId == ItemDatabase.Locomotive && Garage.Count(itemId) + count > 1) return false;
            if (itemId == ItemDatabase.CargoWagon && Garage.Count(itemId) + count > MaxWagons) return false;
            return (itemId == ItemDatabase.Locomotive || itemId == ItemDatabase.CargoWagon) && Garage.TryAddAll(new[] { new ItemStack(itemId, count) });
        }
        public override bool AcceptsItem(string itemId) => itemId == ItemDatabase.Coal || itemId == ItemDatabase.Locomotive || itemId == ItemDatabase.CargoWagon;
        public override IEnumerable<(string, Inventory)> Inventories
        {
            get
            {
                yield return ("garage", Garage); yield return ("fuel", Fuel);
                for (int i = 0; i < MaxWagons; i++) yield return ("wagon" + (i + 1), Wagons[i]);
            }
        }

        /// <summary>"iron_mine:full,base:empty,depot:30" — wait: full, empty, seconds, or a cargo condition (iron_ore>=100). Null or why not.</summary>
        public string SetSchedule(string text)
        {
            var s = new StringBuilder();
            foreach (char c in text ?? "") if (!char.IsWhiteSpace(c)) s.Append(c);
            string t = s.ToString();
            if (t.Length > SignalSpec.MaxText) return "too long";
            var list = new List<ScheduleEntry>();
            if (t.Length > 0)
                foreach (var part in t.Split(','))
                {
                    int colon = part.IndexOf(':');
                    string st = colon < 0 ? part : part.Substring(0, colon), wait = colon < 0 ? "5" : part.Substring(colon + 1);
                    if (st.Length == 0 || st.Length > 24) return $"'{part}': write station:wait";
                    foreach (char c in st) if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_')) return $"'{st}' is not a stop name (a-z, 0-9, _)";
                    var e = new ScheduleEntry { Station = st, Wait = wait };
                    if (wait == "full" || wait == "empty") { }
                    else if (float.TryParse(wait.TrimEnd('s'), NumberStyles.Float, CultureInfo.InvariantCulture, out float sec) && sec >= 0 && sec <= 3600) { e.Seconds = sec; e.Wait = sec.ToString(CultureInfo.InvariantCulture); }
                    else
                    {
                        var err = SignalSpec.ParseCondition(wait, out var cond);
                        if (err != null || cond == null) return $"'{wait}': wait for full, empty, seconds or a cargo condition like iron_ore>=100";
                        e.Cond = cond; e.Wait = cond.Text;
                    }
                    list.Add(e);
                    if (list.Count > 8) return "at most 8 stops";
                }
            Schedule.Clear(); Schedule.AddRange(list);
            var sb = new StringBuilder();
            foreach (var e in Schedule) { if (sb.Length > 0) sb.Append(','); sb.Append(e.Text); }
            ScheduleText = sb.ToString();
            if (Index >= Schedule.Count) Index = 0;
            Touch();
            return null;
        }

        internal static bool HasItems(Inventory inv) { for (int s = 0; s < inv.SlotCount; s++) if (!inv[s].IsEmpty) return true; return false; }

        public int CargoCount(string item) { int n = 0; for (int i = 0; i < WagonCount; i++) n += Wagons[i].Count(item); return n; }

        internal bool CargoFull()
        {
            if (WagonCount == 0) return true;
            for (int i = 0; i < WagonCount; i++)
                for (int s = 0; s < WagonSlots; s++)
                    if (Wagons[i][s].IsEmpty || Wagons[i][s].Count < ItemDatabase.Get(Wagons[i][s].ItemId).MaxStack) return false;
            return true;
        }

        internal bool CargoEmpty() { for (int i = 0; i < WagonCount; i++) if (HasItems(Wagons[i])) return false; return true; }

        public override void SaveExtra(IDictionary<string, string> d)
        {
            var inv = CultureInfo.InvariantCulture;
            if (ScheduleText.Length > 0) d["sched"] = ScheduleText;
            if (BurnLeftKJ > 0) d["burn"] = F(BurnLeftKJ);
            if (!HasTrain) return;
            var sb = new StringBuilder();
            foreach (var c in Trail) { if (sb.Length > 0) sb.Append(';'); sb.Append(c.X.ToString(inv)).Append(',').Append(c.Y.ToString(inv)); }
            d["trail"] = sb.ToString();
            if (HasCameFrom) d["from"] = CameFrom.X.ToString(inv) + "," + CameFrom.Y.ToString(inv);
            d["train"] = $"{Index.ToString(inv)},{(int)Phase},{F(Progress)},{F(Waited)}";
        }

        public override void LoadExtra(string k, string v)
        {
            var inv = CultureInfo.InvariantCulture;
            if (k == "sched") SetSchedule(v);
            else if (k == "burn") BurnLeftKJ = Math.Max(0f, PF(v));
            else if (k == "trail")
            {
                Trail.Clear();
                foreach (var p in v.Split(';')) { var xy = p.Split(','); if (xy.Length == 2) Trail.Add(new Cell(int.Parse(xy[0], inv), int.Parse(xy[1], inv))); }
                HasTrain = Trail.Count > 0;
            }
            else if (k == "from") { var xy = v.Split(','); CameFrom = new Cell(int.Parse(xy[0], inv), int.Parse(xy[1], inv)); HasCameFrom = true; }
            else if (k == "train")
            {
                var f = v.Split(',');
                Index = Math.Max(0, int.Parse(f[0], inv)); Phase = (TrainPhase)(int.Parse(f[1], inv) == 1 ? 1 : 0);
                Progress = Math.Max(0f, Math.Min(0.999f, PF(f[2]))); Waited = Math.Max(0f, PF(f[3]));
            }
        }
    }

    public sealed partial class Factory
    {
        public const float TrainSpeed = 4f, WagonSlowdown = 0.4f, TrainKW = 150f;

        readonly List<PlacedBuilding> depots = new List<PlacedBuilding>(), stops = new List<PlacedBuilding>();
        readonly Dictionary<Cell, int> blockOf = new Dictionary<Cell, int>();   // rail cell -> block (signals: -1)
        int railVersion; bool railsDirty = true;
        readonly Dictionary<int, (int version, Cell head, bool hasFrom, Cell from, string target, List<Cell> path)> pathCache =
            new Dictionary<int, (int, Cell, bool, Cell, string, List<Cell>)>();

        bool IsRail(Cell c) => grid.At(c)?.State is RailState;
        bool IsSignalRail(Cell c) => grid.At(c)?.State is RailState r && r.Signal;

        void RebuildBlocks()
        {
            blockOf.Clear();
            int next = 0;
            var stack = new Stack<Cell>();
            foreach (var b in grid.Buildings)
            {
                if (!(b.State is RailState rs)) continue;
                if (rs.Signal) { blockOf[b.Origin] = -1; continue; }
                if (blockOf.ContainsKey(b.Origin)) continue;
                int id = next++;
                blockOf[b.Origin] = id; stack.Push(b.Origin);
                while (stack.Count > 0)
                {
                    var c = stack.Pop();
                    for (int r = 0; r < 4; r++)
                    {
                        var n = c.Move((Rotation)r);
                        if (blockOf.ContainsKey(n) || !(grid.At(n)?.State is RailState nr) || nr.Signal) continue;
                        blockOf[n] = id; stack.Push(n);
                    }
                }
            }
            railVersion++; railsDirty = false;
            pathCache.Clear();
        }

        int BlockAt(Cell c) => blockOf.TryGetValue(c, out var b) ? b : -2;

        /// <summary>The platform of a stop: the first rail next to it (up, right, down, left).</summary>
        bool PlatformOf(PlacedBuilding stop, out Cell platform)
        {
            for (int r = 0; r < 4; r++) { var c = stop.Origin.Move((Rotation)r); if (IsRail(c) && !IsSignalRail(c)) { platform = c; return true; } }
            platform = default; return false;
        }

        bool IsPlatformFor(Cell c, string station)
        {
            foreach (var s in stops)
            {
                var ts = (TrainStopState)s.State;
                if (ts.Name == station && PlatformOf(s, out var p) && p.Equals(c)) return true;
            }
            return false;
        }

        /// <summary>Shortest way along the rails (no U-turns) from the head to a platform of the station: the cells to drive through, or null.</summary>
        List<Cell> PathFor(PlacedBuilding depot, TrainDepotState t, string station)
        {
            var head = t.Trail[0];
            if (pathCache.TryGetValue(depot.InstanceId, out var cached) && cached.version == railVersion && cached.head.Equals(head)
                && cached.hasFrom == t.HasCameFrom && (!t.HasCameFrom || cached.from.Equals(t.CameFrom)) && cached.target == station)
                return cached.path;
            List<Cell> result = null;
            var goals = new HashSet<Cell>();
            foreach (var s in stops) if (((TrainStopState)s.State).Name == station && PlatformOf(s, out var p)) goals.Add(p);
            if (goals.Count > 0)
            {
                // breadth-first over (cell, came from): fixed neighbour order makes the chosen route deterministic
                var parent = new Dictionary<(Cell, Cell), (Cell, Cell)>();
                var queue = new Queue<(Cell cell, Cell from)>();
                var start = (head, t.HasCameFrom ? t.CameFrom : head);
                parent[start] = start; queue.Enqueue(start);
                (Cell, Cell)? found = null;
                while (queue.Count > 0 && found == null)
                {
                    var (c, from) = queue.Dequeue();
                    for (int r = 0; r < 4; r++)
                    {
                        var n = c.Move((Rotation)r);
                        if (n.Equals(from) || !IsRail(n)) continue;
                        if (IsSignalRail(n) && !n.Equals(c.Move(grid.At(n).Rotation))) continue;   // signals are one-way
                        var key = (n, c);
                        if (parent.ContainsKey(key)) continue;
                        parent[key] = (c, from);
                        if (goals.Contains(n)) { found = key; break; }
                        queue.Enqueue(key);
                    }
                }
                if (found != null)
                {
                    result = new List<Cell>();
                    var k = found.Value;
                    while (!k.Equals(start)) { result.Add(k.Item1); k = parent[k]; }
                    result.Reverse();
                }
            }
            pathCache[depot.InstanceId] = (railVersion, head, t.HasCameFrom, t.CameFrom, station, result);
            return result;
        }

        /// <summary>The cell a train will drive into next (for signals, other trains' reservations and drawing).</summary>
        public bool NextCell(PlacedBuilding depot, out Cell next)
        {
            next = default;
            var t = depot.State as TrainDepotState;
            if (t == null || !t.HasTrain || t.Phase != TrainPhase.Moving || t.Schedule.Count == 0) return false;
            var path = PathFor(depot, t, t.Schedule[t.Index].Station);
            if (path == null || path.Count == 0) return false;
            next = path[0];
            return true;
        }

        bool OccupiedByOther(Cell c, PlacedBuilding self)
        {
            foreach (var d in depots)
            {
                if (d == self) continue;
                var o = (TrainDepotState)d.State;
                if (o.HasTrain) foreach (var x in o.Trail) if (x.Equals(c)) return true;
            }
            return false;
        }

        /// <summary>Is a block free for this train: no other train in it, and none waiting on a signal into it?</summary>
        bool BlockFree(int block, PlacedBuilding self)
        {
            foreach (var d in depots)
            {
                if (d == self) continue;
                var o = (TrainDepotState)d.State;
                if (!o.HasTrain) continue;
                foreach (var x in o.Trail) if (BlockAt(x) == block) return false;
                if (IsSignalRail(o.Trail[0]) && o.Phase == TrainPhase.Moving && o.Schedule.Count > 0)
                {
                    var p = PathFor(d, o, o.Schedule[o.Index].Station);
                    if (p != null && p.Count > 0 && BlockAt(p[0]) == block) return false;   // it holds the signal into this block
                }
            }
            return true;
        }

        void TickTrains(float dt)
        {
            if (railsDirty) RebuildBlocks();
            foreach (var d in depots)
            {
                var t = (TrainDepotState)d.State;
                if (t.Garage.Count(ItemDatabase.Locomotive) == 0) { t.HasTrain = false; t.Trail.Clear(); t.Status = "no locomotive"; continue; }
                if (!t.HasTrain)
                {
                    // the train starts on the rail in front of the depot when that cell is free
                    Cell start = default; bool ok = false;
                    foreach (var c in d.FrontCells()) if (IsRail(c) && !IsSignalRail(c) && !OccupiedByOther(c, d)) { start = c; ok = true; break; }
                    if (!ok) { t.Status = "no rail in front"; continue; }
                    t.HasTrain = true; t.Trail.Clear(); t.Trail.Add(start); t.HasCameFrom = false; t.Progress = 0; t.Phase = TrainPhase.Moving; t.Waited = 0;
                }
                if (!IsRail(t.Trail[0])) { t.Status = "derailed: rebuild the rail"; continue; }   // its rail was removed under it
                while (t.Trail.Count > t.Length) t.Trail.RemoveAt(t.Trail.Count - 1);
                if (t.Schedule.Count == 0) { t.Status = "no schedule"; continue; }
                if (t.Index >= t.Schedule.Count) t.Index = 0;
                var entry = t.Schedule[t.Index];
                if (t.Phase == TrainPhase.Waiting)
                {
                    t.Waited += dt;
                    bool done = entry.Wait == "full" ? t.CargoFull() : entry.Wait == "empty" ? t.CargoEmpty()
                        : entry.Cond != null ? entry.Cond.Eval(name => t.CargoCount(name)) : t.Waited >= entry.Seconds;
                    t.Status = "at " + entry.Station;
                    if (!done) continue;
                    t.Index = (t.Index + 1) % t.Schedule.Count; t.Phase = TrainPhase.Moving; t.Waited = 0;
                    entry = t.Schedule[t.Index];
                }
                if (IsPlatformFor(t.Trail[0], entry.Station)) { t.Phase = TrainPhase.Waiting; t.Progress = 0; t.Waited = 0; t.Status = "at " + entry.Station; continue; }
                var path = PathFor(d, t, entry.Station);
                if (path == null || path.Count == 0) { t.Status = "no path to " + entry.Station; t.Progress = 0; continue; }
                if (t.BurnLeftKJ <= 0)
                {
                    if (!t.Fuel.TryRemove(ItemDatabase.Coal, 1)) { t.Status = "no fuel"; continue; }
                    t.BurnLeftKJ += CoalEnergyKJ;
                }
                float speed = Math.Max(1f, TrainSpeed - WagonSlowdown * t.WagonCount);
                t.Progress += speed * dt;
                t.BurnLeftKJ = Math.Max(0f, t.BurnLeftKJ - TrainKW * dt);
                t.Status = "to " + entry.Station;
                while (t.Progress >= 1f)
                {
                    path = PathFor(d, t, entry.Station);
                    if (path == null || path.Count == 0) { t.Progress = 0; break; }
                    var n = path[0];
                    if (OccupiedByOther(n, d)) { t.Progress = 0; t.Status = "blocked"; break; }
                    if (IsSignalRail(n) && path.Count > 1 && !BlockFree(BlockAt(path[1]), d)) { t.Progress = 0; t.Status = "waiting at signal"; break; }
                    t.HasCameFrom = true; t.CameFrom = t.Trail[0];
                    t.Trail.Insert(0, n);
                    while (t.Trail.Count > t.Length) t.Trail.RemoveAt(t.Trail.Count - 1);
                    t.Progress -= 1f;
                    if (IsPlatformFor(n, entry.Station)) { t.Phase = TrainPhase.Waiting; t.Progress = 0; t.Waited = 0; t.Status = "at " + entry.Station; break; }
                }
            }
        }

        /// <summary>The train waiting at a stop's platform, or null.</summary>
        internal TrainDepotState WaitingTrainAt(PlacedBuilding stop)
        {
            if (stop == null || !PlatformOf(stop, out var p)) return null;
            var name = ((TrainStopState)stop.State).Name;
            foreach (var d in depots)
            {
                var t = (TrainDepotState)d.State;
                if (t.HasTrain && t.Phase == TrainPhase.Waiting && t.Trail.Count > 0 && t.Trail[0].Equals(p) && t.Index < t.Schedule.Count && t.Schedule[t.Index].Station == name) return t;
            }
            return null;
        }

        /// <summary>Is the block behind this signal (any neighbouring block) holding a train? For the signal's light.</summary>
        public bool SignalRed(PlacedBuilding signal)
        {
            for (int r = 0; r < 4; r++)
            {
                int b = BlockAt(signal.Origin.Move((Rotation)r));
                if (b >= 0 && !BlockFree(b, null)) return true;
            }
            return false;
        }

        /// <summary>Trains for drawing: depot, its state, and where the head is heading.</summary>
        public IEnumerable<(PlacedBuilding depot, TrainDepotState train)> Trains()
        {
            foreach (var d in depots) { var t = (TrainDepotState)d.State; if (t.HasTrain && t.Trail.Count > 0) yield return (d, t); }
        }

        /// <summary>For programs and the panel.</summary>
        public Dictionary<string, object> TrainInfo(PlacedBuilding depot)
        {
            if (!(depot?.State is TrainDepotState t)) return null;
            var cargo = new Dictionary<string, object>();
            for (int i = 0; i < t.WagonCount; i++)
                for (int s = 0; s < TrainDepotState.WagonSlots; s++)
                    if (!t.Wagons[i][s].IsEmpty) cargo[t.Wagons[i][s].ItemId] = (cargo.TryGetValue(t.Wagons[i][s].ItemId, out var n) ? (int)n : 0) + t.Wagons[i][s].Count;
            var info = new Dictionary<string, object> { { "status", t.Status }, { "running", t.HasTrain }, { "wagons", t.WagonCount }, { "schedule", t.ScheduleText },
                { "stop", t.Schedule.Count > 0 ? t.Schedule[Math.Min(t.Index, t.Schedule.Count - 1)].Station : null }, { "waiting", t.HasTrain && t.Phase == TrainPhase.Waiting },
                { "fuel", t.Fuel.Count(ItemDatabase.Coal) }, { "cargo", cargo } };
            if (t.HasTrain) { info["x"] = t.Trail[0].X; info["y"] = t.Trail[0].Y; }
            return info;
        }
    }
}
