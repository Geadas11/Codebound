using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Codebound.Core
{
    /// <summary>An item riding on a belt lane. The object travels from belt to belt (no allocation per hop).</summary>
    public sealed class BeltItem
    {
        public string Id;
        /// <summary>How far into the cell, 0 (back edge) to 1 (front edge).</summary>
        public float Pos;
    }

    /// <summary>
    /// A belt, programmable conveyor, sorter or splitter cell with two lanes (0 = left, 1 = right, seen along the belt).
    /// Each lane holds up to 4 items at least <see cref="Gap"/> apart, front first. Items advance at
    /// <see cref="BeltSpeed"/>, close up behind each other and leave at the front edge: straight on, round a curve,
    /// or side-loaded onto the near lane of a perpendicular belt.
    /// <see cref="Contents"/> mirrors the lanes as an ordinary inventory, so programs (contents()), SQL, the machine
    /// panel, arms and refunds keep working; changing it from outside (an arm takes an item, an old save loads
    /// "bi item") is reflected back onto the lanes.
    /// </summary>
    public sealed class ConveyorState : MachineState
    {
        public const float BeltSpeed = 1.875f;   // cells per second (unchanged from the single-item belts)
        public const float Gap = 0.25f;          // minimum spacing on a lane: 4 items per lane per cell
        public const int Lanes = 2, PerLane = 4;
        const float Middle = 0.5f;               // where items from machines, arms and side-loading come in

        readonly List<BeltItem>[] lanes = { new List<BeltItem>(), new List<BeltItem>() };
        public readonly Inventory Contents = new Inventory(Lanes * PerLane);
        bool syncing;

        internal bool MovedNow;    // this tick (Factory)
        internal Cell LastOut;     // where the last item went (splitters alternate from it)
        bool running; string filter;
        /// <summary>Belts: always; programmable conveyors: start()/stop().</summary>
        public bool Running { get => running; set { if (running != value) { running = value; Touch(); } } }
        /// <summary>Programmable conveyors only accept this item when set (sorters: the item that goes straight).</summary>
        public string Filter { get => filter; set { if (filter != value) { filter = value; Touch(); } } }
        public int Turn;           // splitter: which output goes next
        public bool Sorts;         // sorter: the filter picks the exit instead of refusing items

        public ConveyorState() { Contents.Changed += Reconcile; }
        public override bool TakesCondition => true;

        public IReadOnlyList<BeltItem> Lane(int i) => lanes[i];
        internal List<BeltItem> LaneList(int i) => lanes[i];
        public int ItemCount => lanes[0].Count + lanes[1].Count;

        /// <summary>The item nearest the front (what used to be the belt's single item), or null.</summary>
        public string Item { get { var f = Front(); return f?.Id; } }
        /// <summary>How far the front-most item is (0..1), 0 when empty.</summary>
        public float Progress => Front()?.Pos ?? 0f;

        BeltItem Front()
        {
            BeltItem best = null;
            foreach (var l in lanes) if (l.Count > 0 && (best == null || l[0].Pos > best.Pos)) best = l[0];
            return best;
        }

        public bool Accepts(string itemId) => Filter == null || Sorts || Filter == itemId;
        public override bool AcceptsItem(string itemId) => Accepts(itemId);

        /// <summary>Is there room on a lane at this position (no item closer than Gap)?</summary>
        public bool CanEnter(int lane, float pos)
        {
            if (lanes[lane].Count >= PerLane) return false;
            foreach (var it in lanes[lane]) if (Math.Abs(it.Pos - pos) < Gap - 1e-4f) return false;
            return true;
        }

        /// <summary>No item can move: every lane is empty or closed up behind a front item waiting at the edge.</summary>
        internal bool Packed()
        {
            foreach (var l in lanes)
                for (int i = 0; i < l.Count; i++)
                    if (l[i].Pos < (i == 0 ? 1f : l[i - 1].Pos - Gap) - 1e-4f) return false;
            return true;
        }

        internal void Enter(BeltItem it, int lane, float pos)
        {
            it.Pos = pos;
            var l = lanes[lane];
            int i = 0;
            while (i < l.Count && l[i].Pos >= pos) i++;   // keep front first
            l.Insert(i, it);
            Sync();
        }

        internal BeltItem TakeFront(int lane)
        {
            var it = lanes[lane][0];
            lanes[lane].RemoveAt(0);
            Sync();
            return it;
        }

        /// <summary>From a machine, an arm or the player: onto the right lane if there is room in the middle, else the left.</summary>
        public override bool TryInsert(string itemId, int count)
        {
            if (count != 1 || !Accepts(itemId)) return false;
            for (int lane = 1; lane >= 0; lane--)
                if (CanEnter(lane, Middle)) { Enter(new BeltItem { Id = itemId }, lane, Middle); return true; }
            return false;
        }

        void Sync()
        {
            syncing = true;
            Contents.Clear();
            foreach (var l in lanes) foreach (var it in l) Contents.Add(it.Id, 1);
            syncing = false;
        }

        /// <summary>Contents was changed from outside: remove the front-most surplus items, place missing ones at the back.</summary>
        void Reconcile()
        {
            if (syncing) return;
            var have = new Dictionary<string, int>();
            foreach (var l in lanes) foreach (var it in l) have[it.Id] = (have.TryGetValue(it.Id, out var n) ? n : 0) + 1;
            foreach (var kv in new List<KeyValuePair<string, int>>(have))
            {
                int surplus = kv.Value - Contents.Count(kv.Key);
                while (surplus-- > 0)
                {
                    int bestLane = -1, bestIdx = -1;
                    for (int l = 0; l < Lanes; l++)
                        for (int i = 0; i < lanes[l].Count; i++)
                            if (lanes[l][i].Id == kv.Key && (bestLane < 0 || lanes[l][i].Pos > lanes[bestLane][bestIdx].Pos)) { bestLane = l; bestIdx = i; }
                    lanes[bestLane].RemoveAt(bestIdx);
                }
            }
            for (int s = 0; s < Contents.SlotCount; s++)
            {
                var st = Contents[s];
                if (st.IsEmpty) continue;
                int count = 0;
                foreach (var l in lanes) foreach (var it in l) if (it.Id == st.ItemId) count++;
                for (int need = Contents.Count(st.ItemId) - count; need > 0; need--)
                {
                    // the back of the lane with most room; if a lane is packed the item still goes on (it never vanishes)
                    int lane = lanes[1].Count <= lanes[0].Count ? 1 : 0;
                    float pos = lanes[lane].Count == 0 ? Middle : Math.Max(0f, Math.Min(Middle, lanes[lane][lanes[lane].Count - 1].Pos - Gap));
                    lanes[lane].Add(new BeltItem { Id = st.ItemId, Pos = pos });
                    lanes[lane].Sort((a, b) => b.Pos.CompareTo(a.Pos));
                }
            }
            Sync();
        }

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("item", Contents); } }
        public override Inventory Extractable => Contents;

        public override void SaveExtra(IDictionary<string, string> d)
        {
            d["on"] = Running ? "1" : "0";
            if (Filter != null) d["filter"] = Filter;
            if (ItemCount == 0) return;
            var sb = new StringBuilder();
            for (int l = 0; l < Lanes; l++)
            {
                if (l > 0) sb.Append('|');
                for (int i = 0; i < lanes[l].Count; i++) { if (i > 0) sb.Append(','); sb.Append(lanes[l][i].Id).Append('@').Append(F(lanes[l][i].Pos)); }
            }
            d["lanes"] = sb.ToString();
        }

        public override void LoadExtra(string k, string v)
        {
            if (k == "on") Running = v == "1";
            else if (k == "filter") Filter = v;
            else if (k == "lanes")   // exact positions (the "bi item" line already put the items on, roughly)
            {
                var parts = v.Split('|');
                for (int l = 0; l < Lanes; l++)
                {
                    lanes[l].Clear();
                    if (l >= parts.Length || parts[l].Length == 0) continue;
                    foreach (var e in parts[l].Split(','))
                    {
                        int at = e.LastIndexOf('@');
                        if (at <= 0 || !ItemDatabase.Exists(e.Substring(0, at))) continue;
                        lanes[l].Add(new BeltItem { Id = e.Substring(0, at), Pos = Math.Max(0f, Math.Min(1f, PF(e.Substring(at + 1)))) });
                    }
                    lanes[l].Sort((a, b) => b.Pos.CompareTo(a.Pos));
                }
                Sync();
            }
            else if (k == "prog")   // saves from the single-item belts: the item's progress
            {
                var f = Front();
                if (f != null) f.Pos = Math.Max(0f, Math.Min(1f, PF(v)));
            }
        }
    }

    /// <summary>
    /// An arm: swings to the building behind it, picks up one item the building in front will accept, swings to the
    /// front and puts it down there, waiting holding it if the front is full. <see cref="Arm"/> is the position
    /// (0 = at the pickup, 1 = at the drop) for animation; a full swing there and back takes <see cref="SwingSeconds"/>.
    /// </summary>
    public sealed class InserterState : MachineState
    {
        public const float SwingSeconds = 0.8f;
        /// <summary>The held item. An inventory, so it is saved, shown, and refunded if the arm is removed.</summary>
        public readonly Inventory Hand = new Inventory(1);
        public float Arm;
        bool running = true; string filter;
        public bool Running { get => running; set { if (running != value) { running = value; Touch(); } } }
        public string Filter { get => filter; set { if (filter != value) { filter = value; Touch(); } } }
        public int Moved;          // total items moved (for programs and UI)
        public string Held => Hand[0].IsEmpty ? null : Hand[0].ItemId;
        public override bool TakesCondition => true;

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("hand", Hand); } }

        public override void SaveExtra(IDictionary<string, string> d)
        {
            d["on"] = Running ? "1" : "0";
            if (Filter != null) d["filter"] = Filter;
            if (Arm > 0) d["arm"] = F(Arm);
        }
        public override void LoadExtra(string k, string v)
        {
            if (k == "on") Running = v == "1"; else if (k == "filter") Filter = v;
            else if (k == "arm") Arm = Math.Max(0f, Math.Min(1f, PF(v)));
        }
    }

    public sealed partial class Factory
    {
        /// <summary>
        /// Two passes, so the result does not depend on which belt was built first: every belt moves its items up
        /// (an item may end just past the front edge), then every belt hands its front items on, each landing as far
        /// into the next belt as its step carried it — the next belt has already moved, so a line of belts flows like one.
        /// </summary>
        void TickConveyors(float dt)
        {
            float step = ConveyorState.BeltSpeed * dt;
            foreach (var b in belts)
            {
                var c = (ConveyorState)b.State;
                c.MovedNow = false;
                if (c.Asleep || c.ItemCount == 0) continue;
                if (!c.Running || !c.SignalAllows) { c.Status = c.Running ? "disabled" : "stopped"; continue; }
                for (int l = 0; l < ConveyorState.Lanes; l++)
                {
                    var lane = c.LaneList(l);
                    for (int i = 0; i < lane.Count; i++)
                    {
                        var it = lane[i];
                        float limit = i == 0 ? (it.Pos >= 1f ? it.Pos : 1f + step) : Math.Min(lane[i - 1].Pos, 1f) - ConveyorState.Gap;
                        float np = Math.Min(it.Pos + step, limit);
                        if (np > it.Pos) { it.Pos = np; c.MovedNow = true; }
                    }
                }
                if (c.MovedNow) WakeAt(b.Origin);   // items moving up can make room at the back for a belt waiting on this one
            }
            foreach (var b in belts)
            {
                var c = (ConveyorState)b.State;
                if (c.Asleep || c.ItemCount == 0 || !c.Running || !c.SignalAllows) continue;
                bool blocked = false;
                for (int l = 0; l < ConveyorState.Lanes; l++)
                {
                    var lane = c.LaneList(l);
                    if (lane.Count == 0 || lane[0].Pos < 1f) continue;
                    if (PassFront(b, c, l, lane[0].Pos - 1f)) c.MovedNow = true;
                    else { blocked = true; lane[0].Pos = 1f; }
                }
                if (c.MovedNow) { c.Status = "moving"; continue; }
                if (!blocked) continue;
                c.Status = "blocked";
                if (!c.Packed()) continue;   // an item that came in this pass can still move up next tick
                // nothing can move until the way ahead changes (or this belt's own contents do)
                var (first, second) = Outputs(b, c, c.Item);
                bool split = b.Def.Id == "splitter" || b.Def.Id == "sorter";
                if (!Watchable(first) || (split && !Watchable(second))) continue;
                SleepOnExit(b, first);
                if (split) SleepOnExit(b, second);
            }
        }

        /// <summary>Waits on an exit cell, and on the cell behind it when it is a belt: a feeder appearing there turns a curve into a side-load.</summary>
        void SleepOnExit(PlacedBuilding b, Cell cell)
        {
            SleepOn(b, cell);
            var t = grid.At(cell);
            if (t?.State is ConveyorState) { var behind = t.Origin.Move(t.Rotation, -1); if (!behind.Equals(b.Origin)) SleepOn(b, behind); }
        }

        /// <summary>Where this belt sends an item first, and the alternative (splitters fall back to it).</summary>
        static (Cell first, Cell second) Outputs(PlacedBuilding b, ConveyorState c, string item)
        {
            var front = b.Origin.Move(b.Rotation);
            var right = b.Origin.Move(b.Rotation.Next());
            if (b.Def.Id == "sorter") return c.Filter == null || c.Filter == item ? (front, right) : (right, front);   // chosen item straight, others right
            if (b.Def.Id == "splitter") return c.Turn != 0 ? (right, front) : (front, right);                          // alternate
            return (front, front);
        }

        bool PassFront(PlacedBuilding b, ConveyorState c, int lane, float over)
        {
            string item = c.LaneList(lane)[0].Id;
            var (first, second) = Outputs(b, c, item);
            bool splitter = b.Def.Id == "splitter";
            bool ok = Hand(b, c, lane, first, over) || (splitter && Hand(b, c, lane, second, over));
            if (ok && splitter) c.Turn = c.LastOut.Equals(b.Origin.Move(b.Rotation)) ? 1 : 0;
            return ok;
        }

        /// <summary>Moves a lane's front item onto the building on `cell`.</summary>
        bool Hand(PlacedBuilding b, ConveyorState c, int lane, Cell cell, float over)
        {
            var target = grid.At(cell);
            if (target == null || target == b || target.State == null) return false;
            var it = c.LaneList(lane)[0];
            if (target.State is ConveyorState tc)
            {
                if (!tc.Accepts(it.Id)) return false;
                var (tl, pos) = EntryInto(b, target, lane);
                if (pos == 0f) pos = over;   // straight on or round a curve: keep the rest of the step
                if (tl < 0 || !tc.CanEnter(tl, pos)) return false;
                c.TakeFront(lane);
                tc.Enter(it, tl, pos);
            }
            else
            {
                if (!target.State.TryInsert(it.Id, 1)) return false;
                c.TakeFront(lane);
            }
            c.LastOut = cell;
            return true;
        }

        /// <summary>
        /// Which lane of the target belt an item from belt `b` (on `lane`) enters, and where:
        /// same direction → same lane at the back; head-on → nowhere; perpendicular → if the target has a belt feeding it
        /// from behind, side-load onto the near lane in the middle, otherwise it is a curve: same lane, at the back.
        /// </summary>
        (int lane, float pos) EntryInto(PlacedBuilding b, PlacedBuilding t, int lane)
        {
            Rotation travel = Rotation.R0;
            for (int r = 0; r < 4; r++) if (b.Origin.Move((Rotation)r).Equals(t.Origin)) travel = (Rotation)r;
            var rt = t.Rotation;
            if (rt == travel) return (lane, 0f);
            if (rt == travel.Next().Next()) return (-1, 0f);
            var behind = grid.At(t.Origin.Move(rt, -1));
            bool fedFromBehind = behind != null && behind != b && behind.State is ConveyorState && behind.Rotation == rt;
            if (!fedFromBehind) return (lane, 0f);
            bool fromRight = t.Origin.Move(rt.Next()).Equals(b.Origin);
            return (fromRight ? 1 : 0, 0.5f);
        }

        void TickInserters(float dt)
        {
            foreach (var b in inserters)
            {
                var ins = (InserterState)b.State;
                if (ins.Asleep) continue;
                if (!ins.Running) { ins.Status = "stopped"; continue; }
                if (!ins.SignalAllows) { ins.Status = "disabled"; continue; }
                bool longArm = b.Def.Id == "long_inserter";
                float halfSwing = (longArm ? InserterState.SwingSeconds * 1.5f : InserterState.SwingSeconds) / 2f;
                float v = dt / halfSwing;
                int reach = longArm ? 2 : 1;
                var srcCell = b.Origin.Move(b.Rotation, -reach); var dstCell = b.Origin.Move(b.Rotation, reach);
                var src = grid.At(srcCell); var dst = grid.At(dstCell);
                string held = ins.Held;
                if (held == null)
                {
                    if (ins.Arm > 0) { ins.Arm = Math.Max(0f, ins.Arm - v); ins.Status = "moving"; if (ins.Arm > 0) continue; }   // back to the pickup (picks up on arrival)
                    var from = src?.State?.Extractable;
                    if (from == null || dst?.State == null) { ins.Status = "no source/target"; continue; }
                    string pick = null;
                    for (int i = 0; i < from.SlotCount && pick == null; i++)
                    {
                        var s = from[i];
                        if (!s.IsEmpty && (ins.Filter == null || s.ItemId == ins.Filter) && dst.State.AcceptsItem(s.ItemId)) pick = s.ItemId;
                    }
                    if (pick == null)
                    {
                        ins.Status = "waiting";
                        if (Watchable(dstCell)) { SleepOn(b, srcCell); SleepOn(b, dstCell); }   // nothing it may take: wait for either side
                        continue;
                    }
                    from.TryRemove(pick, 1);
                    ins.Hand.Add(pick, 1);
                    ins.Status = "moving";
                    continue;
                }
                if (ins.Arm < 1) { ins.Arm = Math.Min(1f, ins.Arm + v); ins.Status = "moving"; if (ins.Arm < 1) continue; }   // carrying it over (drops on arrival)
                if (dst?.State != null && dst.State.TryInsert(held, 1))
                {
                    ins.Hand.TryRemove(held, 1);
                    ins.Moved++; ins.Status = "moving";
                    continue;
                }
                ins.Status = dst?.State == null ? "no source/target" : "target full";
                if (dst?.State != null && Watchable(dstCell)) SleepOn(b, dstCell);   // holding it until the front has room
            }
        }
    }
}
