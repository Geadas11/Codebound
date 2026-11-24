using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Machine simulation: electric networks, timed machine actions (requested by programs), output transfer.
    /// Deterministic given the same sequence of Tick(dt) and action requests.
    /// </summary>
    public enum MachineActivity : byte { None, Idle, Working, Blocked, NoPower }

    public sealed partial class Factory
    {
        public const float CoalEnergyKJ = 4000f;
        public const float GeneratorKW = 150f, DrillKW = 90f, ComputerKW = 20f, FurnaceFuelKW = 90f;
        public const double PoleReach = 9, PoleSupply = 4.5, GeneratorSupply = 3, ComputerRange = 8;
        public const float MineSeconds = 1f, SmeltSeconds = 3.2f;

        sealed class PendingAction
        {
            public PlacedBuilding Machine;
            public float Remaining;
            public Func<string> Complete;
            public Action<string> Done;
        }

        readonly WorldState world;
        readonly BuildGrid grid;
        readonly List<PendingAction> actions = new List<PendingAction>();
        bool hubsDirty = true;   // a chest or hub was built or removed: hubs look for their chests again

        // Each system walks only its own entities, in build order (ids ascending) — the Codebound analogue of
        // grouping active entities by type. Appending keeps them sorted because ids only grow.
        readonly List<PlacedBuilding> consumers = new List<PlacedBuilding>(), nodeList = new List<PlacedBuilding>(),
            producers = new List<PlacedBuilding>(), belts = new List<PlacedBuilding>(), inserters = new List<PlacedBuilding>(),
            greenhouses = new List<PlacedBuilding>(), pads = new List<PlacedBuilding>(), hubs = new List<PlacedBuilding>();
        readonly List<GeneratorState> running = new List<GeneratorState>();

        // Sleep/wake. A belt or arm with nothing to do sleeps and registers on the cells it waits for; any change there
        // (an inventory of the building on that cell changes, its filter or on/off changes, a building appears or goes)
        // wakes it. Sleepers stay in their lists, so a machine woken mid-tick still runs in its usual order: results are
        // exactly those of retrying every tick.
        readonly Dictionary<Cell, List<PlacedBuilding>> sleepersAt = new Dictionary<Cell, List<PlacedBuilding>>();

        void WakeAt(Cell c)
        {
            if (!sleepersAt.TryGetValue(c, out var l)) return;
            foreach (var s in l) if (s.State != null) s.State.Asleep = false;
            sleepersAt.Remove(c);
        }

        void WakeAround(PlacedBuilding b) { foreach (var c in b.Cells()) WakeAt(c); }

        /// <summary>Off = machines retry every tick (the reference behaviour sleep/wake must match exactly; tests compare both).</summary>
        public bool SleepEnabled = true;

        void SleepOn(PlacedBuilding sleeper, Cell c)
        {
            if (!SleepEnabled) return;
            if (!sleepersAt.TryGetValue(c, out var l)) sleepersAt[c] = l = new List<PlacedBuilding>();
            l.Add(sleeper);
            sleeper.State.Asleep = true;
        }

        /// <summary>Can a machine waiting on this cell sleep? Only if what it accepts depends just on its inventories and
        /// settings, which raise events. Hubs (power and other chests) and shock traps (charges) are retried every tick.</summary>
        bool Watchable(Cell c) => !(grid.At(c)?.State is StorageHubState || grid.At(c)?.State is ShockTrapState);

        IEnumerable<List<PlacedBuilding>> ListsOf(PlacedBuilding b)
        {
            if (IsConsumer(b)) yield return consumers;
            if (IsNode(b)) yield return nodeList;
            var st = b.State;
            if (st is DrillState || st is FurnaceState || st is AssemblerState || st is GreenhouseState || st is FillerState) yield return producers;
            if (st is ConveyorState) yield return belts;
            if (st is InserterState) yield return inserters;
            if (st is GreenhouseState) yield return greenhouses;
            if (st is LaunchPadState) yield return pads;
            if (st is StorageHubState) yield return hubs;
            if (st is PipeState) yield return fluidPipes;
            if (IsFluidMachine(st)) yield return fluidMachines;
        }

        static void RemoveSorted(List<PlacedBuilding> l, PlacedBuilding b)
        {
            int lo = 0, hi = l.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1, c = l[mid].InstanceId.CompareTo(b.InstanceId);
                if (c == 0) { l.RemoveAt(mid); return; }
                if (c < 0) lo = mid + 1; else hi = mid - 1;
            }
        }

        /// <summary>A machine finished an action with a product (drives "item_mined"/"item_smelted" program events).</summary>
        public event Action<PlacedBuilding, string> ActionCompleted;

        public Factory(WorldState world, BuildGrid grid)
        {
            this.world = world; this.grid = grid;
            grid.Placed += b =>
            {
                b.State = CreateState(b.Def);
                if (b.State is GreenhouseState g) g.NearWater = NearWater(b);
                foreach (var l in ListsOf(b)) l.Add(b);
                if (b.State != null) foreach (var (_, inv) in b.State.Inventories) inv.Changed += () => { if (b.State != null) b.State.Asleep = false; WakeAround(b); };   // its own contents changed too: a belt whose item an arm took, then got a new one
                if (b.State != null) b.State.Touched += () => { if (b.State != null) b.State.Asleep = false; WakeAround(b); };
                WakeAround(b);   // whoever waited for something to appear here
                PowerPlaced(b);
                if (b.State is StorageState || b.State is StorageHubState) hubsDirty = true;
                if (b.State is PipeState) fluidDirty = true;
            };
            grid.Removed += b =>
            {
                foreach (var l in ListsOf(b)) RemoveSorted(l, b);
                WakeAround(b);
                for (int i = actions.Count - 1; i >= 0; i--)
                    if (actions[i].Machine == b) { var a = actions[i]; actions.RemoveAt(i); a.Done?.Invoke(null); }
                PowerRemoved(b);
                if (b.State is StorageState || b.State is StorageHubState) hubsDirty = true;
                if (b.State is PipeState) fluidDirty = true;
            };
        }

        public static MachineState CreateState(BuildingDef def)
        {
            switch (def.Kind)
            {
                case BuildingKind.Storage: return new StorageState();
                case BuildingKind.Furnace: return new FurnaceState();
                case BuildingKind.Drill: return new DrillState();
                case BuildingKind.Generator: return new GeneratorState();
                case BuildingKind.Computer: return ComputerState.ForModel(def.Id);
                case BuildingKind.Greenhouse: return new GreenhouseState();
                case BuildingKind.Assembler: return new AssemblerState();
                case BuildingKind.Lab: return new LabState();
                case BuildingKind.Turret: return new TurretState();
                case BuildingKind.Solar: return new SolarState();
                case BuildingKind.Battery: return new BatteryState();
                case BuildingKind.Scrubber: return new ScrubberState { Status = "idle" };
                case BuildingKind.LaunchPad: return new LaunchPadState();
                case BuildingKind.Server: return new ServerState();
                case BuildingKind.Heater: return new HeaterState();
                case BuildingKind.ShockTrap: return new ShockTrapState();
                case BuildingKind.LightningRod: return new LightningRodState();
                case BuildingKind.StorageHub: return new StorageHubState();
                case BuildingKind.Belt: return new ConveyorState { Running = true, Status = "moving", Sorts = def.Id == "sorter" };
                case BuildingKind.Conveyor: return new ConveyorState { Status = "stopped" };
                case BuildingKind.Inserter: return new InserterState();
                case BuildingKind.Pipe: return new PipeState(Fluids.PipeCapacity);
                case BuildingKind.Tank: return new PipeState(Fluids.TankCapacity);
                case BuildingKind.Pump: return new PumpState();
                case BuildingKind.OffshorePump: return new OffshorePumpState();
                case BuildingKind.Pumpjack: return new PumpjackState();
                case BuildingKind.Boiler: return new BoilerState();
                case BuildingKind.SteamEngine: return new SteamEngineState();
                case BuildingKind.Filler: return new FillerState();
                default: return null;
            }
        }

        // ------------------------------------------------------------ power
        static bool IsNode(PlacedBuilding b) => b.Def.Kind == BuildingKind.Pole || b.Def.Kind == BuildingKind.Generator
            || b.Def.Kind == BuildingKind.Solar || b.Def.Kind == BuildingKind.Battery || b.Def.Kind == BuildingKind.LightningRod
            || b.Def.Kind == BuildingKind.SteamEngine;
        public const float SolarKW = 40f;
        /// <summary>Sunlight reaching solar panels 0..1 (daylight × clouds/rain/storm), set every tick by the game.</summary>
        public float SolarFactor = 1f;
        static bool IsConsumer(PlacedBuilding b) => b.Def.Kind == BuildingKind.Drill || b.Def.Kind == BuildingKind.Computer
            || b.Def.Kind == BuildingKind.Greenhouse || b.Def.Kind == BuildingKind.Assembler || b.Def.Kind == BuildingKind.Lab || b.Def.Kind == BuildingKind.Turret
            || b.Def.Kind == BuildingKind.Scrubber || b.Def.Kind == BuildingKind.Server || b.Def.Kind == BuildingKind.StorageHub || b.Def.Kind == BuildingKind.Heater
            || b.Def.Kind == BuildingKind.Pump || b.Def.Kind == BuildingKind.Pumpjack || b.Def.Kind == BuildingKind.Filler;
        public const float GrowLampKW = 30f, AssemblerKW = 60f, LabKW = 40f;
        /// <summary>Tech progress of the world (set by GameState).</summary>
        public Research Research = new Research();
        /// <summary>Outdoor light 0..1, set every tick by the game from the world clock.</summary>
        public float Daylight = 1f;
        /// <summary>Set by GameState; null in bare factory tests.</summary>
        public Ecology Ecology;

        float DemandOf(PlacedBuilding c)
        {
            if (c.Def.Kind == BuildingKind.Computer) return ((ComputerState)c.State).Running ? ComputerKW : 0f;
            if (c.Def.Kind == BuildingKind.Drill) return HasAction(c) ? DrillKW : 0f;
            if (c.Def.Kind == BuildingKind.Assembler) return HasAction(c) ? AssemblerKW : 0f;
            if (c.Def.Kind == BuildingKind.Lab) return HasAction(c) ? LabKW : 0f;
            if (c.Def.Kind == BuildingKind.Turret) return 10f;
            if (c.Def.Kind == BuildingKind.Scrubber) return Ecology.ScrubberKW;
            if (c.Def.Kind == BuildingKind.Server) return 15f;
            if (c.Def.Kind == BuildingKind.Heater) return Shelter.HeaterKW;
            if (c.Def.Kind == BuildingKind.StorageHub) return 20f;
            if (c.State is SwitchedFluidState sw && !sw.Running) return 0f;
            if (c.Def.Kind == BuildingKind.Pump) return Fluids.PumpKW;
            if (c.Def.Kind == BuildingKind.Pumpjack) return Fluids.PumpjackKW;
            if (c.Def.Kind == BuildingKind.Filler) return Fluids.FillerKW;
            if (c.State is GreenhouseState gh) { foreach (var g in gh.Growth) if (g >= 0 && g < 1) return GrowLampKW; return 0f; }
            return 0f;
        }

        static bool GeneratorCanRun(GeneratorState g) => g.BurnLeftKJ > 0 || g.Fuel.Count(ItemDatabase.Coal) > 0;

        // ------------------------------------------------------------ actions
        /// <summary>What a machine is doing right now, for its on-map indicator.</summary>
        public MachineActivity Activity(PlacedBuilding b)
        {
            var st = b.State;
            if (st == null || st is StorageState || st is PipeState || b.Def.Kind == BuildingKind.Pole) return MachineActivity.None;
            if (IsConsumer(b) && st.PowerSatisfaction <= 0 && (HasAction(b) || st is ComputerState cs && cs.Running || st is ScrubberState || st is TurretState || st is SwitchedFluidState sw && sw.Running)) return MachineActivity.NoPower;
            string s = (st.Status ?? "").ToLowerInvariant();
            if (s.Contains("blocked") || s.Contains("full") || s.Contains("no fuel") || s.Contains("no source") || s.Contains("tank") || s.Contains("no power")
                || s.Contains("no pipe") || s.Contains("no water") || s.Contains("no oil") || s.Contains("no steam") || s.Contains("empty") || s.Contains("nothing to pump")) return MachineActivity.Blocked;
            bool working = HasAction(b)
                || st is GeneratorState g && g.LoadKW > 0 || st is SolarState so && so.OutputKW > 0
                || st is ConveyorState cv && cv.Item != null && cv.Running
                || st is InserterState ins && ins.Status == "moving"
                || st is ComputerState pc && pc.Running
                || st is ScrubberState sc && sc.Status == "cleaning"
                || st is LaunchPadState lp && lp.Phase == "ascent"
                || s == "pumping" || s == "boiling" || s == "filling" || st is SteamEngineState se && se.OutputKW > 0;
            if (!working && st is GreenhouseState gh) foreach (var v in gh.Growth) if (v >= 0 && v < 1) working = true;
            return working ? MachineActivity.Working : MachineActivity.Idle;
        }

        public bool HasAction(PlacedBuilding b)
        {
            foreach (var a in actions) if (a.Machine == b) return true;
            return false;
        }

        static bool CanAccept(Inventory inv, string itemId)
        {
            var s = inv[0];
            return s.IsEmpty || (s.ItemId == itemId && s.Count < ItemDatabase.Get(itemId).MaxStack);
        }

        Deposit FirstDeposit(PlacedBuilding drill, out Cell where)
        {
            foreach (var c in drill.Cells())
            {
                var d = world.DepositAt(c);
                if (WorldRules.DrillCanMine(d)) { where = c; return d; }
            }
            where = default; return default;
        }

        /// <summary>Starts one mining cycle. Returns an error code, or null when started (done fires with the item id or null).</summary>
        public string StartMine(PlacedBuilding drill, Action<string> done)
        {
            if (drill?.Def.Kind != BuildingKind.Drill) return "not_a_drill";
            if (HasAction(drill)) return "busy";
            var st = (DrillState)drill.State;
            var dep = FirstDeposit(drill, out _);
            if (dep.IsNone) { st.Status = "depleted"; return "depleted"; }
            if (!CanAccept(st.Output, WorldRules.ItemFor(dep.Type))) { st.Status = "output full"; return "output_full"; }
            if (!HasPower(drill)) { st.Status = "no power"; return "no_power"; }
            st.Status = "mining";
            actions.Add(new PendingAction
            {
                Machine = drill, Remaining = MineSeconds, Done = done,
                Complete = () =>
                {
                    var d = FirstDeposit(drill, out var cell);
                    if (d.IsNone) return null;
                    string item = WorldRules.ItemFor(d.Type);
                    if (!CanAccept(st.Output, item)) return null;
                    world.Extract(cell, 1);
                    st.Output.Add(item, 1);
                    return item;
                }
            });
            return null;
        }

        /// <summary>Starts smelting one input unit. Error code or null (done fires with the product id or null).</summary>
        public string StartSmelt(PlacedBuilding furnace, Action<string> done)
        {
            if (furnace?.Def.Kind != BuildingKind.Furnace) return "not_a_furnace";
            if (HasAction(furnace)) return "busy";
            var st = (FurnaceState)furnace.State;
            var input = st.Input[0];
            if (input.IsEmpty) { st.Status = "no input"; return "no_input"; }
            string product = FurnaceState.Recipes[input.ItemId];
            if (!CanAccept(st.Output, product)) { st.Status = "output full"; return "output_full"; }
            if (st.BurnLeftKJ <= 0 && st.Fuel.Count(ItemDatabase.Coal) == 0) { st.Status = "no fuel"; return "no_fuel"; }
            st.Status = "smelting";
            actions.Add(new PendingAction
            {
                Machine = furnace, Remaining = SmeltSeconds, Done = done,
                Complete = () =>
                {
                    var s = st.Input[0];
                    if (s.IsEmpty) return null;
                    string p = FurnaceState.Recipes[s.ItemId];
                    if (!CanAccept(st.Output, p)) return null;
                    st.Input.TryRemove(s.ItemId, 1);
                    st.Output.Add(p, 1);
                    return p;
                }
            });
            return null;
        }

        /// <summary>Cancels a machine's running action (program stopped). done fires with null.</summary>
        public void Cancel(PlacedBuilding b)
        {
            for (int i = actions.Count - 1; i >= 0; i--)
                if (actions[i].Machine == b) { var a = actions[i]; actions.RemoveAt(i); a.Machine.State.Status = "idle"; a.Done?.Invoke(null); }
        }

        /// <summary>Machine actions, update: progress with power or fuel.</summary>
        void ProgressActions(float dt)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var a = actions[i];
                float rate;
                if (a.Machine.Def.Kind == BuildingKind.Furnace)
                {
                    var f = (FurnaceState)a.Machine.State;
                    if (f.BurnLeftKJ <= 0 && f.Fuel.TryRemove(ItemDatabase.Coal, 1)) f.BurnLeftKJ += CoalEnergyKJ;
                    rate = f.BurnLeftKJ > 0 ? 1f : 0f;
                    if (rate > 0) f.BurnLeftKJ = Math.Max(0, f.BurnLeftKJ - FurnaceFuelKW * dt);
                    f.Status = rate > 0 ? "smelting" : "no fuel";
                }
                else
                {
                    rate = a.Machine.State.PowerSatisfaction;
                    var st = a.Machine.State;
                    if (rate <= 0) st.Status = "no power";
                    else if (st is LabState ls) { if (st.Status == null || !st.Status.StartsWith("researching")) st.Status = "researching " + ls.Current; }
                    else st.Status = a.Machine.Def.Kind == BuildingKind.Drill ? "mining" : "crafting";
                }
                a.Remaining -= rate * dt;
            }
        }

        /// <summary>Machine actions, finish: complete the finished ones in the order they were started (callbacks and events).</summary>
        void CompleteActions()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var a = actions[i];
                if (a.Remaining > 0) continue;
                actions.RemoveAt(i--);
                string result = a.Complete();
                a.Machine.State.Status = "idle";
                a.Done?.Invoke(result);
                if (result != null) ActionCompleted?.Invoke(a.Machine, result);
            }
        }

        // ------------------------------------------------------------ logistics
        void PushOutputs()
        {
            foreach (var b in producers)
            {
                Inventory output = b.State is DrillState d ? d.Output : b.State is FurnaceState f ? f.Output
                    : b.State is AssemblerState asm ? asm.Output : b.State is GreenhouseState gr ? gr.Output : b.State is FillerState fi ? fi.Output : null;
                if (output == null || output[0].IsEmpty) continue;
                string item = output[0].ItemId;
                foreach (var c in b.FrontCells())
                {
                    var target = grid.At(c);
                    if (target == null || target == b || target.State == null) continue;
                    if (target.State.TryInsert(item, 1)) { output.TryRemove(item, 1); break; }
                }
            }
        }

        /// <summary>The factory's systems in tick order (used by Simulation; Tick(dt) runs the same steps).</summary>
        public IEnumerable<SimSystem> Systems()
        {
            float dt = Simulation.TickSeconds;
            yield return new SimSystem("power", () => UpdatePower(dt), PreparePower);
            yield return new SimSystem("machine actions", () => ProgressActions(dt), null, CompleteActions);
            yield return new SimSystem("outputs", PushOutputs);
            yield return new SimSystem("belts", () => TickConveyors(dt));
            yield return new SimSystem("inserters", () => TickInserters(dt));
            yield return new SimSystem("fluids", () => TickFluids(dt));
            yield return new SimSystem("greenhouses", () => TickGreenhouses(dt));
            yield return new SimSystem("launch pads", () => TickLaunchPads(dt));
            yield return new SimSystem("storage hubs", LinkStorageHubs);
        }

        public void Tick(float dt)
        {
            PreparePower();
            UpdatePower(dt);
            ProgressActions(dt);
            CompleteActions();
            PushOutputs();
            TickConveyors(dt);
            TickInserters(dt);
            TickFluids(dt);
            TickGreenhouses(dt);
            TickLaunchPads(dt);
            LinkStorageHubs();
        }

        readonly List<PlacedBuilding> found = new List<PlacedBuilding>();

        /// <summary>Hubs see the chests within their radius. Relinked when something is built or removed (and once a second,
        /// in case a building's state object was replaced), looking only at the cells around each hub.</summary>
        void LinkStorageHubs()
        {
            bool relink = hubsDirty;   // which chests a hub serves only changes when chests or hubs are built or removed
            hubsDirty = false;
            int r = (int)Math.Ceiling(StorageHubState.Radius) + 2;
            foreach (var b in hubs)
            {
                var hub = (StorageHubState)b.State;
                if (relink)
                {
                    found.Clear();
                    int cx = (int)Math.Floor(b.CenterX), cy = (int)Math.Floor(b.CenterY);
                    for (int dx = -r; dx <= r; dx++)
                        for (int dy = -r; dy <= r; dy++)
                            if (grid.At(new Cell(cx + dx, cy + dy)) is PlacedBuilding c && c.State is StorageState && c.DistanceTo(b) <= StorageHubState.Radius && !found.Contains(c)) found.Add(c);
                    found.Sort((p, q) => p.InstanceId.CompareTo(q.InstanceId));   // same order as before: the order they were built
                    hub.Chests.Clear();
                    foreach (var c in found) hub.Chests.Add((StorageState)c.State);
                }
                int shown = hub.PowerSatisfaction <= 0 ? -1 : hub.Chests.Count;   // rebuild the text only when it changes
                if (shown != hub.ShownChests) { hub.ShownChests = shown; hub.Status = shown < 0 ? "no power" : shown + " chests"; }
            }
        }

        /// <summary>(pad, "orbit" | "aborted") when an ascent ends.</summary>
        public event Action<PlacedBuilding, string> FlightEnded;

        void TickLaunchPads(float dt)
        {
            foreach (var b in pads)
            {
                var lp = (LaunchPadState)b.State;
                lp.Underground = world.Generator.Kind == PlanetKind.Underground;
                if (lp.Phase != "ascent" || lp.Flight == null) continue;
                lp.Flight.Step(dt);
                var f = lp.Flight;
                lp.Status = $"ascent {f.Altitude:0} km {f.Velocity:0.0} km/s heat {f.Heat:0}% fuel {f.Fuel:0}";
                if (f.Phase == "orbit") { lp.Phase = "orbit"; lp.Status = "in orbit"; lp.LastResult = "orbit"; FlightEnded?.Invoke(b, "orbit"); }
                else if (f.Phase == "aborted")
                {
                    lp.Phase = "ready"; lp.Status = "ready"; lp.LastResult = "aborted: " + f.Reason; lp.Flight = null;   // capsule parachutes back, fuel is spent
                    FlightEnded?.Invoke(b, "aborted: " + f.Reason);
                }
            }
        }

        // ------------------------------------------------------------ agriculture
        bool NearWater(PlacedBuilding b)
        {
            foreach (var c in b.Cells())
                for (int dx = -4; dx <= 4; dx++)
                    for (int dy = -4; dy <= 4; dy++)
                    {
                        var t = world.Generator.TileAt(new Cell(c.X + dx, c.Y + dy));
                        if (t == TileType.Water || t == TileType.ToxicWater) return true;
                    }
            return false;
        }

        void TickGreenhouses(float dt)
        {
            foreach (var b in greenhouses)
            {
                var g = (GreenhouseState)b.State;
                bool hydro = b.Def.Id == "hydroponics";   // indoor bay: lamps only, recycles its water, faster
                if (g.NearWater) g.Tank = Math.Min(100f, g.Tank + GreenhouseState.TankRefill * dt);
                if (hydro && g.PowerSatisfaction > 0) g.Tank = Math.Min(100f, g.Tank + 0.5f * dt);
                g.Light = hydro ? Math.Max(0f, g.PowerSatisfaction) : Math.Max(Daylight, g.PowerSatisfaction > 0 ? g.PowerSatisfaction : 0f);
                int growing = 0, ripe = 0;
                for (int i = 0; i < GreenhouseState.Plots; i++)
                {
                    if (g.Growth[i] < 0) continue;
                    if (g.Growth[i] >= 1) { ripe++; continue; }
                    growing++;
                    if (g.Soil[i] <= 0) continue;   // dry soil: no growth
                    var sp = Plants.Get(g.Species[i] ?? "fruit") ?? Plants.All[0];
                    g.Soil[i] = Math.Max(0, g.Soil[i] - GreenhouseState.WaterPerSecond * dt * (hydro ? 0.5f : 1f));
                    float light = sp.NeedsLight ? g.Light : 1f;
                    float rate = dt / sp.GrowSeconds * light * (Ecology?.GrowthFactor(b.Origin) ?? 1f) * (0.5f + 0.5f * g.Gene[i]) * (hydro ? 1.5f : 1f);
                    g.Growth[i] = Math.Min(1f, g.Growth[i] + rate);
                }
                g.Status = growing + ripe == 0 ? "empty" : $"{growing} growing, {ripe} ripe, tank {g.Tank:0}";
            }
        }

        int PickPlot(GreenhouseState g, object plotArg, Func<int, bool> ok)
        {
            if (plotArg is double d) { int i = (int)d; return i >= 0 && i < GreenhouseState.Plots && ok(i) ? i : -1; }
            for (int i = 0; i < GreenhouseState.Plots; i++) if (ok(i)) return i;
            return -1;
        }

        /// <summary>Instant greenhouse operations: plant, water, harvest, plots. Returns (value, error code).</summary>
        public (object value, string error) Greenhouse(PlacedBuilding b, string op, object plotArg, object speciesArg = null)
        {
            if (!(b?.State is GreenhouseState g)) return (null, "not_a_greenhouse");
            switch (op)
            {
                case "plant":
                {
                    PlantSpecies sp = speciesArg is string sid ? Plants.Get(sid) : null;
                    if (speciesArg is string && sp == null) return (false, "unknown_species");
                    if (sp == null) foreach (var s in Plants.All) if (g.Seeds.Count(s.Seed) > 0) { sp = s; break; }   // first species with seeds
                    if (sp == null || g.Seeds.Count(sp.Seed) == 0) return (false, "no_seeds");
                    int i = PickPlot(g, plotArg, k => g.Growth[k] < 0);
                    if (i < 0) return (false, "no_free_plot");
                    g.Seeds.TryRemove(sp.Seed, 1);
                    g.Growth[i] = 0; g.Soil[i] = 0; g.Species[i] = sp.Id; g.Gene[i] = g.StrainOf(sp.Id);
                    return (i, null);
                }
                case "water":
                {
                    int i = PickPlot(g, plotArg, k => g.Growth[k] >= 0 && g.Growth[k] < 1 && g.Soil[k] < 0.5f);
                    if (i < 0) return (false, "nothing_to_water");
                    float need = Math.Min(1f - g.Soil[i], GreenhouseState.WaterPerDrink * 2) * 20f;  // 20 tank units = full soil
                    if (g.Tank < 1f) return (false, "tank_empty");
                    float used = Math.Min(need, g.Tank);
                    g.Tank -= used; g.Soil[i] = Math.Min(1f, g.Soil[i] + used / 20f);
                    return (i, null);
                }
                case "harvest":
                {
                    int i = PickPlot(g, plotArg, k => g.Growth[k] >= 1);
                    if (i < 0) return (0, "nothing_ripe");
                    var sp = Plants.Get(g.Species[i] ?? "fruit") ?? Plants.All[0];
                    int n = Plants.YieldOf(sp, g.Gene[i]);
                    if (!g.Output.TryAddAll(new[] { new ItemStack(sp.Produce, n) })) return (0, "output_full");
                    // selective breeding: the harvested plant's offspring may be stronger
                    float child = Plants.Mutate(g.Gene[i], ++g.Harvests, i, sp.Id);
                    if (child > g.StrainOf(sp.Id)) g.Strain[sp.Id] = child;
                    g.Growth[i] = -1; g.Soil[i] = 0; g.Species[i] = null;
                    return (n, null);
                }
                case "plots":
                {
                    var list = new List<object>();
                    for (int i = 0; i < GreenhouseState.Plots; i++)
                        list.Add(new Dictionary<string, object> {
                            { "plot", i }, { "planted", g.Growth[i] >= 0 }, { "growth", Math.Round(Math.Max(0, g.Growth[i]), 3) },
                            { "ripe", g.Growth[i] >= 1 }, { "water", Math.Round(g.Soil[i], 3) }, { "species", g.Species[i] }, { "vigor", Math.Round(g.Gene[i], 3) } });
                    var strains = new Dictionary<string, object>(); foreach (var s in Plants.All) strains[s.Id] = Math.Round(g.StrainOf(s.Id), 3);
                    return (new Dictionary<string, object> { { "plots", list }, { "tank", Math.Round(g.Tank, 1) }, { "light", Math.Round(g.Light, 2) }, { "strains", strains } }, null);
                }
            }
            return (null, "unknown_operation");
        }

        // ------------------------------------------------------------ research
        /// <summary>Starts researching a tech in a lab. Error code or null (done fires with the tech id or null).</summary>
        public string StartResearch(PlacedBuilding lab, string techId, Action<string> done)
        {
            if (!(lab?.State is LabState st)) return "not_a_lab";
            var t = Research.Get(techId);
            if (t == null) return "unknown_tech";
            if (Research.IsDone(t.Id)) return "already_done";
            if (!Research.PrereqsMet(t)) return "prerequisites_missing";
            if (HasAction(lab)) return "busy";
            foreach (var c in t.Cost) if (st.Input.Count(c.ItemId) < c.Count) { st.Status = "missing " + c.ItemId; return "missing_" + c.ItemId; }
            if (!HasPower(lab)) { st.Status = "no power"; return "no_power"; }
            st.Current = t.Id; st.Status = "researching " + t.Id;
            actions.Add(new PendingAction
            {
                Machine = lab, Remaining = t.Seconds, Done = done,
                Complete = () =>
                {
                    st.Current = null;
                    if (Research.IsDone(t.Id) || !st.Input.TryRemoveAll(t.Cost)) return null;
                    Research.Complete(t);
                    return t.Id;
                }
            });
            return null;
        }

        // ------------------------------------------------------------ manufacturing
        /// <summary>Starts crafting one recipe. Error code or null (done fires with the product id or null).</summary>
        public string StartCraft(PlacedBuilding asm, string product, Action<string> done)
        {
            if (!(asm?.State is AssemblerState st)) return "not_an_assembler";
            if (product == null || !AssemblerState.Recipes.TryGetValue(product, out var r)) return "unknown_recipe";
            if (HasAction(asm)) return "busy";
            foreach (var i in r.Inputs) if (st.Input.Count(i.ItemId) < i.Count) { st.Status = "missing " + i.ItemId; return "missing_" + i.ItemId; }
            var o = st.Output[0];
            if (!o.IsEmpty && (o.ItemId != r.Product || o.Count + r.Amount > ItemDatabase.Get(r.Product).MaxStack)) { st.Status = "output full"; return "output_full"; }
            if (!HasPower(asm)) { st.Status = "no power"; return "no_power"; }
            st.Status = "crafting";
            actions.Add(new PendingAction
            {
                Machine = asm, Remaining = r.Seconds, Done = done,
                Complete = () =>
                {
                    if (!st.Input.TryRemoveAll(r.Inputs)) return null;
                    st.Output.Add(r.Product, r.Amount);
                    return r.Product;
                }
            });
            return null;
        }

        /// <summary>Buildings a computer can command (centre within ComputerRange), nearest first.</summary>
        public List<PlacedBuilding> MachinesNear(PlacedBuilding computer)
        {
            var list = new List<PlacedBuilding>();
            int r = (int)Math.Ceiling(ComputerRange) + 2, cx = (int)Math.Floor(computer.CenterX), cy = (int)Math.Floor(computer.CenterY);
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                    if (grid.At(new Cell(cx + dx, cy + dy)) is PlacedBuilding b && b != computer && b.State != null && b.DistanceTo(computer) <= ComputerRange && !list.Contains(b))
                        list.Add(b);
            list.Sort((x, y) => { int c = x.DistanceTo(computer).CompareTo(y.DistanceTo(computer)); return c != 0 ? c : x.InstanceId.CompareTo(y.InstanceId); });
            return list;
        }
    }
}
