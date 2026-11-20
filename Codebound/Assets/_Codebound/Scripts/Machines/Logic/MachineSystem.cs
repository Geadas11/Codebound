using System;
using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;
using Codebound.Energy;
using Codebound.World;

namespace Codebound.Machines
{
    /// <summary>
    /// Owns the runtime objects behind placed structures (machines, poles, batteries), keeps them in sync with
    /// the <see cref="BuildGrid"/>, and runs the power grid and every machine on each simulation tick.
    /// </summary>
    public sealed class MachineSystem
    {
        public const double EmergencyBatteryJoules = 120_000;   // ~1 h of a basic computer
        public const double EmergencyBatteryWatts = 200;

        private readonly BuildGrid _grid;
        private readonly Dictionary<int, Machine> _machines = new Dictionary<int, Machine>();
        private readonly Dictionary<int, Battery> _batteries = new Dictionary<int, Battery>();
        private readonly List<Machine> _tickOrder = new List<Machine>();
        private readonly MachineContext _ctx;

        public MachineSystem(WorldMap world, BuildGrid grid, double tickSeconds)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Power = new PowerGrid();
            _ctx = new MachineContext(world, grid, tickSeconds);
            foreach (var s in grid.Structures) OnPlaced(s);
            grid.Placed += OnPlaced;
            grid.Removed += OnRemoved;
        }

        public PowerGrid Power { get; }
        public IReadOnlyCollection<Machine> Machines => _machines.Values;

        public Machine Get(PlacedStructure s) => s != null && _machines.TryGetValue(s.InstanceId, out var m) ? m : null;
        public T Get<T>(PlacedStructure s) where T : Machine => Get(s) as T;
        public Battery GetBattery(PlacedStructure s) => s != null && _batteries.TryGetValue(s.InstanceId, out var b) ? b : null;

        /// <summary>Power node (machine or battery) behind a structure, if any.</summary>
        public PowerNode GetPowerNode(PlacedStructure s) => (PowerNode)Get(s) ?? GetBattery(s);

        public void Tick(long tick)
        {
            _ctx.Tick = tick;
            Power.Tick(_ctx.TickSeconds);
            // Computers first so machines see this tick's program state.
            foreach (var m in _tickOrder) if (m is Computer) m.Tick(_ctx);
            foreach (var m in _tickOrder) if (!(m is Computer)) m.Tick(_ctx);
        }

        private void OnPlaced(PlacedStructure s)
        {
            switch (s.Definition.Id)
            {
                case StructureCatalog.Drill: AddMachine(new Drill(s)); break;
                case StructureCatalog.CoalGenerator: AddMachine(new CoalGenerator(s)); break;
                case StructureCatalog.Computer: AddMachine(new Computer(s, ComputerHardware.Basic)); break;
                case StructureCatalog.PowerPole: Power.AddPole(new PowerPole(s.Origin)); break;
                case StructureCatalog.EmergencyBattery:
                    var b = new Battery(s.Cells, EmergencyBatteryJoules, EmergencyBatteryWatts, EmergencyBatteryJoules);
                    _batteries[s.InstanceId] = b;
                    Power.AddNode(b);
                    break;
            }
        }

        private void AddMachine(Machine m)
        {
            _machines[m.Structure.InstanceId] = m;
            _tickOrder.Add(m);
            Power.AddNode(m);
        }

        private void OnRemoved(PlacedStructure s)
        {
            if (_machines.TryGetValue(s.InstanceId, out var m))
            {
                if (m is Computer c) c.UnlinkAll();
                else m.ControlledBy?.Unlink(m);
                _machines.Remove(s.InstanceId);
                _tickOrder.Remove(m);
                Power.RemoveNode(m);
            }
            if (_batteries.TryGetValue(s.InstanceId, out var b))
            {
                _batteries.Remove(s.InstanceId);
                Power.RemoveNode(b);
            }
            if (s.Definition.Id == StructureCatalog.PowerPole) Power.RemovePoleAt(s.Origin);
        }
    }
}
