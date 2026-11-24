using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Fluids (Etapa 3 of the industrial simulation). Connected pipes and tanks form one <b>segment</b>: a single fluid,
    /// one amount, one capacity (the sum of its parts), so flow inside a segment is instant and costs nothing per pipe.
    /// Pumps move fluid from the segment behind them to the one in front; machines keep small boxes of their own and
    /// exchange with the segment their connection touches. A segment never holds two fluids (placing a pipe that would
    /// join two different fluids is refused).
    /// </summary>
    public static class Fluids
    {
        public const string Water = "water", Steam = "steam", CrudeOil = "crude_oil", PetroleumGas = "petroleum_gas", SulfuricAcid = "sulfuric_acid";
        public static readonly string[] All = { Water, Steam, CrudeOil, PetroleumGas, SulfuricAcid };
        public static bool Exists(string id) => Array.IndexOf(All, id) >= 0;

        public const float PipeCapacity = 100f, TankCapacity = 2500f, BoxCapacity = 20f;
        public const float PumpRate = 120f, OffshoreRate = 60f, PumpjackRate = 10f;   // units per second
        public const float SteamKJ = 30f;              // energy in one unit of steam
        public const float BoilerKW = 300f, EngineKW = 150f;
        public const float PumpKW = 20f, PumpjackKW = 60f, FillerKW = 30f;
        public const int OilPerDeposit = 10;           // fluid units from one unit of an oil deposit
        public const float FillerCrude = 20f, FillerSeconds = 2f;   // 20 crude oil -> 1 canister
    }

    /// <summary>A machine's own fluid buffer for one fluid.</summary>
    public sealed class FluidBox
    {
        public readonly string Fluid;
        public readonly float Capacity;
        public float Amount;
        public FluidBox(string fluid, float capacity = Fluids.BoxCapacity) { Fluid = fluid; Capacity = capacity; }
        public float Free => Capacity - Amount;
        public float Take(float a) { a = Math.Min(a, Amount); Amount -= a; if (Amount < 1e-4f) Amount = 0; return a; }
    }

    /// <summary>Pipe or storage tank: part of a segment.</summary>
    public sealed class PipeState : MachineState
    {
        public readonly float Capacity;
        /// <summary>Pipe to ground: joins only the pipe behind it and its partner (the next one ahead, facing back, within 10 cells).</summary>
        public bool Underground;
        internal Factory.FluidSegment Segment;
        internal string LoadFluid; internal float LoadAmount;   // its share, kept while segments are rebuilt (and from saves)
        public PipeState(float capacity) { Capacity = capacity; }

        public string Fluid => Segment != null ? Segment.Fluid : LoadFluid;
        /// <summary>This pipe's share of its segment.</summary>
        public float Amount => Segment != null ? (Segment.Capacity > 0 ? Segment.Amount * Capacity / Segment.Capacity : 0) : LoadAmount;
        public float SegmentAmount => Segment != null ? Segment.Amount : LoadAmount;
        public float SegmentCapacity => Segment != null ? Segment.Capacity : Capacity;

        public override string Status
        {
            get => Fluid == null ? "empty" : $"{Fluid} {SegmentAmount:0}/{SegmentCapacity:0}";
            set { }
        }
        public override IEnumerable<(string, float)> FluidContents { get { if (Fluid != null) yield return (Fluid, SegmentAmount); } }

        public override void SaveExtra(IDictionary<string, string> d)
        {
            // the whole segment's amount goes in its first pipe and the others say 0: loading sums them back exactly
            string f = Fluid;
            if (f == null) return;
            d["fluid"] = f;
            d["amount"] = F(Segment == null ? LoadAmount : Segment.Pipes[0].State == this ? Segment.Amount : 0f);
        }
        public override void LoadExtra(string k, string v)
        {
            if (k == "fluid" && Fluids.Exists(v)) LoadFluid = v;
            else if (k == "amount") LoadAmount = Math.Max(0f, PF(v));
        }
    }

    /// <summary>Base for the fluid machines that programs can start and stop (pump, pumpjack, filler). They start stopped.</summary>
    public abstract class SwitchedFluidState : MachineState
    {
        bool running;
        public bool Running { get => running; set { if (running != value) { running = value; Touch(); } } }
        protected SwitchedFluidState() { Status = "stopped"; }
        public override bool TakesCondition => true;
        /// <summary>Started by a program and allowed by its signal condition.</summary>
        public bool Working => Running && SignalAllows;
        public override void SaveExtra(IDictionary<string, string> d) { d["on"] = Running ? "1" : "0"; SaveMore(d); }
        public override void LoadExtra(string k, string v) { if (k == "on") Running = v == "1"; else LoadMore(k, v); }
        protected virtual void SaveMore(IDictionary<string, string> d) { }
        protected virtual void LoadMore(string k, string v) { }
    }

    /// <summary>Moves fluid from the pipe behind it to the pipe in front (powered, start()/stop()).</summary>
    public sealed class PumpState : SwitchedFluidState
    {
        public float Moved;   // total units moved
        public float RatePerSecond;   // last tick
    }

    /// <summary>On a shore: pumps water from the water tile behind it into the pipe in front. Passive, no power.</summary>
    public sealed class OffshorePumpState : MachineState { public OffshorePumpState() { Status = "no pipe"; } }

    /// <summary>On an oil field: extracts crude oil (powered, start()/stop()) into the pipe in front.</summary>
    public sealed class PumpjackState : SwitchedFluidState
    {
        public readonly FluidBox Box = new FluidBox(Fluids.CrudeOil);
        public override IEnumerable<(string, float)> FluidContents { get { yield return (Fluids.CrudeOil, Box.Amount); } }
        protected override void SaveMore(IDictionary<string, string> d) { if (Box.Amount > 0) d["crude"] = F(Box.Amount); }
        protected override void LoadMore(string k, string v) { if (k == "crude") Box.Amount = Clamp(PF(v), Box.Capacity); }
        internal static float Clamp(float v, float cap) => Math.Max(0f, Math.Min(cap, v));
    }

    /// <summary>Burns coal to turn water (from the pipe behind) into steam (to the pipe in front). Passive.</summary>
    public sealed class BoilerState : MachineState
    {
        public readonly Inventory Fuel = new Inventory(1);
        public readonly FluidBox Water = new FluidBox(Fluids.Water), Steam = new FluidBox(Fluids.Steam);
        public float BurnLeftKJ;
        public BoilerState() { Status = "no fuel"; }
        public override bool TryInsert(string itemId, int count) => itemId == ItemDatabase.Coal && Fuel.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId) => itemId == ItemDatabase.Coal;
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("fuel", Fuel); } }
        public override IEnumerable<(string, float)> FluidContents { get { yield return (Fluids.Water, Water.Amount); yield return (Fluids.Steam, Steam.Amount); } }
        public override void SaveExtra(IDictionary<string, string> d)
        {
            d["burn"] = F(BurnLeftKJ);
            if (Water.Amount > 0) d["water"] = F(Water.Amount);
            if (Steam.Amount > 0) d["steam"] = F(Steam.Amount);
        }
        public override void LoadExtra(string k, string v)
        {
            if (k == "burn") BurnLeftKJ = Math.Max(0f, PF(v));
            else if (k == "water") Water.Amount = PumpjackState.Clamp(PF(v), Water.Capacity);
            else if (k == "steam") Steam.Amount = PumpjackState.Clamp(PF(v), Steam.Capacity);
        }
    }

    /// <summary>Turns steam (from the pipe behind or in front) into electricity for its power network. Passive.</summary>
    public sealed class SteamEngineState : MachineState
    {
        public readonly FluidBox Steam = new FluidBox(Fluids.Steam);
        public float OutputKW;
        internal int ShownKW = int.MinValue;
        public SteamEngineState() { Status = "no steam"; }
        public override IEnumerable<(string, float)> FluidContents { get { yield return (Fluids.Steam, Steam.Amount); } }
        public override void SaveExtra(IDictionary<string, string> d) { if (Steam.Amount > 0) d["steam"] = F(Steam.Amount); }
        public override void LoadExtra(string k, string v) { if (k == "steam") Steam.Amount = PumpjackState.Clamp(PF(v), Steam.Capacity); }
    }

    /// <summary>Fills crude oil canisters (the item the launch pad burns) from the pipe behind (powered, start()/stop()).</summary>
    public sealed class FillerState : SwitchedFluidState
    {
        public readonly FluidBox Crude = new FluidBox(Fluids.CrudeOil, 40f);
        public readonly Inventory Output = new Inventory(1);
        public float Progress;
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("output", Output); } }
        public override Inventory Extractable => Output;
        public override IEnumerable<(string, float)> FluidContents { get { yield return (Fluids.CrudeOil, Crude.Amount); } }
        protected override void SaveMore(IDictionary<string, string> d)
        {
            if (Crude.Amount > 0) d["crude"] = F(Crude.Amount);
            if (Progress > 0) d["prog"] = F(Progress);
        }
        protected override void LoadMore(string k, string v)
        {
            if (k == "crude") Crude.Amount = PumpjackState.Clamp(PF(v), Crude.Capacity);
            else if (k == "prog") Progress = PumpjackState.Clamp(PF(v), Fluids.FillerSeconds);
        }
    }

    public sealed partial class Factory
    {
        /// <summary>Connected pipes and tanks: one fluid, one amount.</summary>
        internal sealed class FluidSegment
        {
            public readonly List<PlacedBuilding> Pipes = new List<PlacedBuilding>();   // by id
            public string Fluid;
            public float Amount, Capacity;
            public float Free => Capacity - Amount;
            public bool Accepts(string f) => Fluid == null || Fluid == f;
            public void Add(string f, float a) { if (a <= 0) return; Fluid = f; Amount += a; }
            public float Take(float a)
            {
                a = Math.Min(a, Amount); Amount -= a;
                if (Amount < 1e-4f) { Amount = 0; Fluid = null; }
                return a;
            }
        }

        readonly List<PlacedBuilding> fluidPipes = new List<PlacedBuilding>(), fluidMachines = new List<PlacedBuilding>();
        readonly List<FluidSegment> segments = new List<FluidSegment>();
        bool fluidDirty = true;

        static bool IsFluidMachine(MachineState s) => s is PumpState || s is OffshorePumpState || s is PumpjackState
            || s is BoilerState || s is SteamEngineState || s is FillerState || s is RefineryState || s is ChemicalPlantState;

        void RebuildSegments()
        {
            // keep each pipe's share of its old segment, then regroup
            foreach (var p in fluidPipes)
            {
                var ps = (PipeState)p.State;
                if (ps.Segment == null) continue;
                ps.LoadFluid = ps.Segment.Fluid; ps.LoadAmount = ps.Amount; ps.Segment = null;
            }
            segments.Clear();
            var stack = new Stack<PlacedBuilding>();
            foreach (var start in fluidPipes)
            {
                if (((PipeState)start.State).Segment != null) continue;
                var seg = new FluidSegment();
                segments.Add(seg);
                ((PipeState)start.State).Segment = seg;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    var p = stack.Pop();
                    seg.Pipes.Add(p);
                    foreach (var q in PipeLinks(p))
                    {
                        var qs = (PipeState)q.State;
                        if (qs.Segment != null) continue;
                        qs.Segment = seg; stack.Push(q);
                    }
                }
                seg.Pipes.Sort((a, b) => a.InstanceId.CompareTo(b.InstanceId));
                // the fluid with the most in it wins (two fluids never meet in play: placement refuses it)
                var byFluid = new Dictionary<string, float>();
                foreach (var p in seg.Pipes)
                {
                    var ps = (PipeState)p.State;
                    seg.Capacity += ps.Capacity;
                    if (ps.LoadFluid != null && ps.LoadAmount > 0) byFluid[ps.LoadFluid] = (byFluid.TryGetValue(ps.LoadFluid, out var n) ? n : 0) + ps.LoadAmount;
                    ps.LoadFluid = null; ps.LoadAmount = 0;
                }
                foreach (var f in Fluids.All) if (byFluid.TryGetValue(f, out var a) && (seg.Fluid == null || a > byFluid[seg.Fluid])) seg.Fluid = f;
                if (seg.Fluid != null) seg.Amount = Math.Min(seg.Capacity, byFluid[seg.Fluid]);
            }
            fluidDirty = false;
        }

        FluidSegment SegmentAt(Cell c) => (grid.At(c)?.State as PipeState)?.Segment;

        public const int PipeUndergroundReach = 10;

        /// <summary>A pipe to ground's partner: the first pipe to ground ahead within reach, if it faces back.</summary>
        PlacedBuilding PipePartner(PlacedBuilding p)
        {
            for (int d = 1; d <= PipeUndergroundReach; d++)
            {
                var o = grid.At(p.Origin.Move(p.Rotation, d));
                if (o?.State is PipeState os && os.Underground) return o.Rotation == p.Rotation.Next().Next() ? o : null;
            }
            return null;
        }

        /// <summary>Pipes and tanks a pipe or tank is joined to.</summary>
        IEnumerable<PlacedBuilding> PipeLinks(PlacedBuilding p)
        {
            var ps = (PipeState)p.State;
            var cand = new List<PlacedBuilding>();
            if (ps.Underground) { cand.Add(grid.At(p.Origin.Move(p.Rotation, -1))); cand.Add(PipePartner(p)); }
            else foreach (var c in p.Cells()) for (int r = 0; r < 4; r++) cand.Add(grid.At(c.Move((Rotation)r)));
            foreach (var q in cand)
            {
                if (q == null || q == p || !(q.State is PipeState qs)) continue;
                if (qs.Underground && grid.At(q.Origin.Move(q.Rotation, -1)) != p && PipePartner(q) != p) continue;   // its side does not connect
                yield return q;
            }
        }

        /// <summary>Would placing this pipe or tank join two segments holding different fluids?</summary>
        public bool FluidConflict(BuildingDef def, Cell origin, Rotation rot)
        {
            if (def.Kind != BuildingKind.Pipe && def.Kind != BuildingKind.Tank && def.Kind != BuildingKind.UndergroundPipe) return false;
            if (fluidDirty) RebuildSegments();
            if (def.Kind == BuildingKind.UndergroundPipe)   // the pipe behind it and whatever it would reach underground
            {
                string fb = SegmentAt(origin.Move(rot, -1))?.Fluid, fa = null;
                for (int d = 1; d <= PipeUndergroundReach; d++)
                {
                    var o = grid.At(origin.Move(rot, d));
                    if (o?.State is PipeState os && os.Underground) { if (o.Rotation == rot.Next().Next()) fa = os.Segment?.Fluid; break; }
                }
                return fb != null && fa != null && fb != fa;
            }
            var (w, h) = def.Size(rot);
            string seen = null;
            for (int x = -1; x <= w; x++)
                for (int y = -1; y <= h; y++)
                {
                    bool inside = x >= 0 && x < w && y >= 0 && y < h, corner = (x < 0 || x >= w) && (y < 0 || y >= h);
                    if (inside || corner) continue;
                    var f = SegmentAt(new Cell(origin.X + x, origin.Y + y))?.Fluid;
                    if (f == null) continue;
                    if (seen != null && seen != f) return true;
                    seen = f;
                }
            return false;
        }

        FluidSegment FirstSegment(IEnumerable<Cell> cells, string accepting)
        {
            foreach (var c in cells) { var s = SegmentAt(c); if (s != null && (accepting == null || s.Accepts(accepting))) return s; }
            return null;
        }

        static void Pull(FluidSegment s, FluidBox box)
        {
            if (s == null || s.Fluid != box.Fluid) return;
            box.Amount += s.Take(box.Free);
        }

        static void Push(FluidBox box, FluidSegment s)
        {
            if (s == null || box.Amount <= 0 || !s.Accepts(box.Fluid)) return;
            s.Add(box.Fluid, box.Take(s.Free));
        }

        void TickFluids(float dt)
        {
            if (fluidDirty) RebuildSegments();
            foreach (var b in fluidMachines)
            {
                switch (b.State)
                {
                    case OffshorePumpState op:
                    {
                        var seg = FirstSegment(b.FrontCells(), Fluids.Water);
                        if (seg == null) { op.Status = "no pipe"; break; }
                        float add = Math.Min(Fluids.OffshoreRate * dt, seg.Free);
                        seg.Add(Fluids.Water, add);
                        op.Status = add > 0 ? "pumping" : "pipe full";
                        break;
                    }
                    case PumpjackState pj:
                    {
                        if (!pj.Working) { pj.Status = pj.Running ? "disabled" : "stopped"; break; }
                        float want = Fluids.PumpjackRate * dt * pj.PowerSatisfaction;
                        if (pj.Box.Amount < want && pj.Box.Free >= Fluids.OilPerDeposit)
                            foreach (var c in b.Cells())
                                if (world.Extract(c, 1) > 0) { pj.Box.Amount += Fluids.OilPerDeposit; break; }
                        Push(pj.Box, FirstSegment(b.FrontCells(), Fluids.CrudeOil));
                        pj.Status = pj.PowerSatisfaction <= 0 ? "no power" : OilLeftUnder(b) ? (pj.Box.Free < Fluids.OilPerDeposit ? "pipe full" : "pumping") : "oil field empty";
                        break;
                    }
                    case PumpState pu:
                    {
                        pu.RatePerSecond = 0;
                        if (!pu.Working) { pu.Status = pu.Running ? "disabled" : "stopped"; break; }
                        if (pu.PowerSatisfaction <= 0) { pu.Status = "no power"; break; }
                        var src = FirstSegment(b.BackCells(), null); var dst = FirstSegment(b.FrontCells(), null);
                        if (src == null || dst == null) { pu.Status = "no pipe"; break; }
                        if (src == dst || src.Fluid == null || !dst.Accepts(src.Fluid)) { pu.Status = src.Fluid == null ? "nothing to pump" : "blocked"; break; }
                        string f = src.Fluid;
                        float move = Math.Min(Fluids.PumpRate * dt * pu.PowerSatisfaction, Math.Min(src.Amount, dst.Free));
                        dst.Add(f, src.Take(move));
                        pu.Moved += move; pu.RatePerSecond = move / dt;
                        pu.Status = move > 0 ? "pumping" : "blocked";
                        break;
                    }
                    case BoilerState bo:
                    {
                        Pull(FirstSegment(b.BackCells(), Fluids.Water), bo.Water);
                        float make = Math.Min(Fluids.BoilerKW * dt / Fluids.SteamKJ, Math.Min(bo.Water.Amount, bo.Steam.Free));
                        while (bo.BurnLeftKJ < make * Fluids.SteamKJ && bo.Fuel.TryRemove(ItemDatabase.Coal, 1)) bo.BurnLeftKJ += CoalEnergyKJ;
                        make = Math.Min(make, bo.BurnLeftKJ / Fluids.SteamKJ);
                        if (make > 0)
                        {
                            bo.BurnLeftKJ = Math.Max(0f, bo.BurnLeftKJ - make * Fluids.SteamKJ);
                            bo.Water.Take(make); bo.Steam.Amount += make;
                        }
                        Push(bo.Steam, FirstSegment(b.FrontCells(), Fluids.Steam));
                        bo.Status = make > 0 ? "boiling" : bo.BurnLeftKJ <= 0 && bo.Fuel.Count(ItemDatabase.Coal) == 0 ? "no fuel" : bo.Water.Amount <= 0 ? "no water" : "steam full";
                        break;
                    }
                    case SteamEngineState se:
                        Pull(FirstSegment(b.BackCells(), Fluids.Steam), se.Steam);
                        Pull(FirstSegment(b.FrontCells(), Fluids.Steam), se.Steam);
                        break;
                    case RefineryState rf: TickRefinery(b, rf, dt); break;
                    case ChemicalPlantState cp: TickChemicalPlant(b, cp, dt); break;
                    case FillerState fi:
                    {
                        Pull(FirstSegment(b.BackCells(), Fluids.CrudeOil), fi.Crude);
                        if (!fi.Working) { fi.Status = fi.Running ? "disabled" : "stopped"; break; }
                        if (fi.Crude.Amount < Fluids.FillerCrude) { fi.Status = "no oil"; break; }
                        if (!fi.Output[0].IsEmpty && fi.Output[0].Count >= ItemDatabase.Get(ItemDatabase.Oil).MaxStack) { fi.Status = "output full"; break; }
                        fi.Progress += dt * fi.PowerSatisfaction;
                        fi.Status = fi.PowerSatisfaction <= 0 ? "no power" : "filling";
                        if (fi.Progress >= Fluids.FillerSeconds)
                        {
                            fi.Progress = 0; fi.Crude.Take(Fluids.FillerCrude);
                            fi.Output.Add(ItemDatabase.Oil, 1);
                        }
                        break;
                    }
                }
            }
            // greenhouses next to a water pipe fill their tank from it (as well as from a nearby lake)
            foreach (var b in greenhouses)
            {
                var g = (GreenhouseState)b.State;
                if (g.Tank >= 100f) continue;
                var seg = FirstSegment(Around(b), Fluids.Water);
                if (seg == null || seg.Fluid != Fluids.Water) continue;
                g.Tank += seg.Take(Math.Min(100f - g.Tank, GreenhouseState.PipeRefill * dt));
            }
        }

        bool OilLeftUnder(PlacedBuilding b)
        {
            foreach (var c in b.Cells()) if (world.DepositAt(c).Type == ResourceType.Oil) return true;
            return false;
        }

        /// <summary>The cells touching a building's sides (no corners).</summary>
        static IEnumerable<Cell> Around(PlacedBuilding b)
        {
            var (w, h) = b.Size;
            for (int x = 0; x < w; x++) { yield return new Cell(b.Origin.X + x, b.Origin.Y - 1); yield return new Cell(b.Origin.X + x, b.Origin.Y + h); }
            for (int y = 0; y < h; y++) { yield return new Cell(b.Origin.X - 1, b.Origin.Y + y); yield return new Cell(b.Origin.X + w, b.Origin.Y + y); }
        }

        /// <summary>Steam engines' available power this tick, and using it (called from the power balance).</summary>
        static float EngineCapacityKW(PlacedBuilding e, float dt) => Math.Min(Fluids.EngineKW, ((SteamEngineState)e.State).Steam.Amount * Fluids.SteamKJ / Math.Max(dt, 1e-4f));

        static void RunEngine(PlacedBuilding e, float kw, float dt)
        {
            var se = (SteamEngineState)e.State;
            se.OutputKW = kw;
            se.Steam.Take(kw * dt / Fluids.SteamKJ);
            int shown = se.Steam.Amount <= 0 && kw <= 0 ? -1 : (int)Math.Round(kw);
            if (shown != se.ShownKW) { se.ShownKW = shown; se.Status = shown < 0 ? "no steam" : shown + " kW"; }
        }
    }
}
