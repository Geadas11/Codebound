using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Machine simulation: electric networks, timed machine actions (requested by programs), output transfer.
    /// Deterministic given the same sequence of Tick(dt) and action requests.
    /// </summary>
    public sealed class Factory
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

        sealed class Network
        {
            public readonly List<PlacedBuilding> Generators = new List<PlacedBuilding>();
            public readonly List<PlacedBuilding> Consumers = new List<PlacedBuilding>();
            public float CapacityKW, DemandKW, Satisfaction;
        }

        readonly WorldState world;
        readonly BuildGrid grid;
        readonly List<PendingAction> actions = new List<PendingAction>();
        readonly Dictionary<int, Network> networkOf = new Dictionary<int, Network>();
        readonly List<Network> networks = new List<Network>();
        bool powerDirty = true;

        /// <summary>A machine finished an action with a product (drives "item_mined"/"item_smelted" program events).</summary>
        public event Action<PlacedBuilding, string> ActionCompleted;

        public Factory(WorldState world, BuildGrid grid)
        {
            this.world = world; this.grid = grid;
            grid.Placed += b => { b.State = CreateState(b.Def); if (b.State is GreenhouseState g) g.NearWater = NearWater(b); powerDirty = true; };
            grid.Removed += b =>
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                    if (actions[i].Machine == b) { var a = actions[i]; actions.RemoveAt(i); a.Done?.Invoke(null); }
                powerDirty = true;
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
                case BuildingKind.Belt: return new ConveyorState { Running = true, Status = "moving" };
                case BuildingKind.Conveyor: return new ConveyorState { Status = "stopped" };
                case BuildingKind.Inserter: return new InserterState();
                default: return null;
            }
        }

        // ------------------------------------------------------------ power
        static bool IsNode(PlacedBuilding b) => b.Def.Kind == BuildingKind.Pole || b.Def.Kind == BuildingKind.Generator;
        static bool IsConsumer(PlacedBuilding b) => b.Def.Kind == BuildingKind.Drill || b.Def.Kind == BuildingKind.Computer
            || b.Def.Kind == BuildingKind.Greenhouse || b.Def.Kind == BuildingKind.Assembler || b.Def.Kind == BuildingKind.Lab || b.Def.Kind == BuildingKind.Turret;
        public const float GrowLampKW = 30f, AssemblerKW = 60f, LabKW = 40f;
        /// <summary>Tech progress of the world (set by GameState).</summary>
        public Research Research = new Research();
        /// <summary>Outdoor light 0..1, set every tick by the game from the world clock.</summary>
        public float Daylight = 1f;

        void RebuildNetworks()
        {
            networks.Clear(); networkOf.Clear();
            var nodes = new List<PlacedBuilding>();
            foreach (var b in grid.Buildings) if (IsNode(b)) nodes.Add(b);

            foreach (var start in nodes)
            {
                if (networkOf.ContainsKey(start.InstanceId)) continue;
                var net = new Network();
                networks.Add(net);
                var stack = new Stack<PlacedBuilding>();
                stack.Push(start); networkOf[start.InstanceId] = net;
                while (stack.Count > 0)
                {
                    var n = stack.Pop();
                    if (n.Def.Kind == BuildingKind.Generator) net.Generators.Add(n);
                    foreach (var m in nodes)
                        if (!networkOf.ContainsKey(m.InstanceId) && n.DistanceTo(m) <= PoleReach) { networkOf[m.InstanceId] = net; stack.Push(m); }
                }
            }

            foreach (var c in grid.Buildings)
            {
                if (!IsConsumer(c)) continue;
                var (cw, ch) = c.Size;
                double pad = Math.Max(cw, ch) / 2.0;
                foreach (var n in nodes)
                {
                    double reach = (n.Def.Kind == BuildingKind.Pole ? PoleSupply : GeneratorSupply) + pad;
                    if (n.DistanceTo(c) <= reach) { var net = networkOf[n.InstanceId]; net.Consumers.Add(c); networkOf[c.InstanceId] = net; break; }
                }
            }
            powerDirty = false;
        }

        float DemandOf(PlacedBuilding c)
        {
            if (c.Def.Kind == BuildingKind.Computer) return ((ComputerState)c.State).Running ? ComputerKW : 0f;
            if (c.Def.Kind == BuildingKind.Drill) return HasAction(c) ? DrillKW : 0f;
            if (c.Def.Kind == BuildingKind.Assembler) return HasAction(c) ? AssemblerKW : 0f;
            if (c.Def.Kind == BuildingKind.Lab) return HasAction(c) ? LabKW : 0f;
            if (c.Def.Kind == BuildingKind.Turret) return 10f;
            if (c.State is GreenhouseState gh) { foreach (var g in gh.Growth) if (g >= 0 && g < 1) return GrowLampKW; return 0f; }
            return 0f;
        }

        static bool GeneratorCanRun(GeneratorState g) => g.BurnLeftKJ > 0 || g.Fuel.Count(ItemDatabase.Coal) > 0;

        void TickPower(float dt)
        {
            if (powerDirty) RebuildNetworks();
            foreach (var b in grid.Buildings) if (IsConsumer(b)) b.State.PowerSatisfaction = 0f;

            foreach (var net in networks)
            {
                var running = new List<GeneratorState>();
                foreach (var g in net.Generators) { var gs = (GeneratorState)g.State; gs.LoadKW = 0; if (GeneratorCanRun(gs)) running.Add(gs); }
                net.CapacityKW = running.Count * GeneratorKW;
                net.DemandKW = 0;
                foreach (var c in net.Consumers) net.DemandKW += DemandOf(c);
                net.Satisfaction = net.DemandKW <= 0 ? (net.CapacityKW > 0 ? 1f : 0f) : Math.Min(1f, net.CapacityKW / net.DemandKW);

                float used = Math.Min(net.DemandKW, net.CapacityKW);
                foreach (var gs in running)
                {
                    float share = used / running.Count;
                    gs.LoadKW = share;
                    gs.BurnLeftKJ -= share * dt;
                    while (gs.BurnLeftKJ <= 0 && share > 0 && gs.Fuel.TryRemove(ItemDatabase.Coal, 1)) gs.BurnLeftKJ += CoalEnergyKJ;
                    if (gs.BurnLeftKJ < 0) gs.BurnLeftKJ = 0;
                    gs.Status = gs.BurnLeftKJ > 0 || gs.Fuel.Count(ItemDatabase.Coal) > 0 ? $"{share:0} kW" : "no fuel";
                }
                foreach (var c in net.Consumers) c.State.PowerSatisfaction = net.Satisfaction;
            }
        }

        /// <summary>True when the building is on a network with at least one fuelled generator.</summary>
        public bool HasPower(PlacedBuilding b)
        {
            if (powerDirty) RebuildNetworks();
            return networkOf.TryGetValue(b.InstanceId, out var net) && net.Generators.Exists(g => GeneratorCanRun((GeneratorState)g.State));
        }

        public (float capacityKW, float demandKW) NetworkStats(PlacedBuilding b)
        {
            if (powerDirty) RebuildNetworks();
            return networkOf.TryGetValue(b.InstanceId, out var n) ? (n.CapacityKW, n.DemandKW) : (0f, 0f);
        }

        // ------------------------------------------------------------ actions
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

        void TickActions(float dt)
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
                    a.Machine.State.Status = rate > 0 ? (a.Machine.Def.Kind == BuildingKind.Drill ? "mining" : a.Machine.State is LabState ls ? "researching " + ls.Current : "crafting") : "no power";
                }
                a.Remaining -= rate * dt;
            }

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
            foreach (var b in grid.Buildings)
            {
                Inventory output = b.State is DrillState d ? d.Output : b.State is FurnaceState f ? f.Output
                    : b.State is AssemblerState asm ? asm.Output : b.State is GreenhouseState gr ? gr.Output : null;
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

        void TickConveyors(float dt)
        {
            foreach (var b in grid.Buildings) if (b.State is ConveyorState c) c.ArrivedThisTick = false;
            foreach (var b in grid.Buildings)
            {
                if (!(b.State is ConveyorState c) || c.Item == null || c.ArrivedThisTick) continue;
                if (!c.Running) { c.Status = "stopped"; continue; }
                c.Progress = Math.Min(1f, c.Progress + ConveyorState.BeltSpeed * dt);
                c.Status = "moving";
                if (c.Progress < 1f) continue;
                foreach (var cell in b.FrontCells())
                {
                    var target = grid.At(cell);
                    string item = c.Item;
                    if (target?.State != null && target.State.TryInsert(item, 1)) { c.Slot.TryRemove(item, 1); c.Progress = 0; }
                    else c.Status = "blocked";
                }
            }
        }

        void TickInserters(float dt)
        {
            foreach (var b in grid.Buildings)
            {
                if (!(b.State is InserterState ins)) continue;
                if (!ins.Running) { ins.Status = "stopped"; continue; }
                ins.Progress = Math.Min(InserterState.SwingSeconds, ins.Progress + dt);
                if (ins.Progress < InserterState.SwingSeconds) continue;
                PlacedBuilding src = null, dst = null;
                foreach (var c in b.BackCells()) src = grid.At(c);
                foreach (var c in b.FrontCells()) dst = grid.At(c);
                var from = src?.State?.Extractable;
                if (from == null || dst?.State == null) { ins.Status = "no source/target"; continue; }
                ins.Status = "waiting";
                for (int i = 0; i < from.SlotCount; i++)
                {
                    var s = from[i];
                    if (s.IsEmpty || (ins.Filter != null && s.ItemId != ins.Filter)) continue;
                    if (!dst.State.TryInsert(s.ItemId, 1)) { ins.Status = "target full"; continue; }
                    from.TryRemove(s.ItemId, 1);
                    ins.Progress = 0; ins.Moved++; ins.Status = "moving";
                    break;
                }
            }
        }

        public void Tick(float dt)
        {
            TickPower(dt);
            TickActions(dt);
            PushOutputs();
            TickConveyors(dt);
            TickInserters(dt);
            TickGreenhouses(dt);
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
            foreach (var b in grid.Buildings)
            {
                if (!(b.State is GreenhouseState g)) continue;
                if (g.NearWater) g.Tank = Math.Min(100f, g.Tank + GreenhouseState.TankRefill * dt);
                g.Light = Math.Max(Daylight, g.PowerSatisfaction > 0 ? g.PowerSatisfaction : 0f);
                int growing = 0, ripe = 0;
                for (int i = 0; i < GreenhouseState.Plots; i++)
                {
                    if (g.Growth[i] < 0) continue;
                    if (g.Growth[i] >= 1) { ripe++; continue; }
                    growing++;
                    if (g.Soil[i] <= 0) continue;   // dry soil: no growth
                    g.Soil[i] = Math.Max(0, g.Soil[i] - GreenhouseState.WaterPerSecond * dt);
                    g.Growth[i] = Math.Min(1f, g.Growth[i] + dt / GreenhouseState.GrowSeconds * g.Light);
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
        public (object value, string error) Greenhouse(PlacedBuilding b, string op, object plotArg)
        {
            if (!(b?.State is GreenhouseState g)) return (null, "not_a_greenhouse");
            switch (op)
            {
                case "plant":
                {
                    if (g.Seeds.Count(ItemDatabase.AlienFruit) == 0) return (false, "no_seeds");
                    int i = PickPlot(g, plotArg, k => g.Growth[k] < 0);
                    if (i < 0) return (false, "no_free_plot");
                    g.Seeds.TryRemove(ItemDatabase.AlienFruit, 1);
                    g.Growth[i] = 0; g.Soil[i] = 0;
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
                    if (!g.Output.TryAddAll(new[] { new ItemStack(ItemDatabase.AlienFruit, GreenhouseState.Yield) })) return (0, "output_full");
                    g.Growth[i] = -1; g.Soil[i] = 0;
                    return (GreenhouseState.Yield, null);
                }
                case "plots":
                {
                    var list = new List<object>();
                    for (int i = 0; i < GreenhouseState.Plots; i++)
                        list.Add(new Dictionary<string, object> {
                            { "plot", i }, { "planted", g.Growth[i] >= 0 }, { "growth", Math.Round(Math.Max(0, g.Growth[i]), 3) },
                            { "ripe", g.Growth[i] >= 1 }, { "water", Math.Round(g.Soil[i], 3) } });
                    return (new Dictionary<string, object> { { "plots", list }, { "tank", Math.Round(g.Tank, 1) }, { "light", Math.Round(g.Light, 2) } }, null);
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
            foreach (var b in grid.Buildings)
                if (b != computer && b.State != null && b.DistanceTo(computer) <= ComputerRange) list.Add(b);
            list.Sort((x, y) => x.DistanceTo(computer).CompareTo(y.DistanceTo(computer)));
            return list;
        }
    }
}
