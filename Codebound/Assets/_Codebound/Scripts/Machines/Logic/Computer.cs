using System;
using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;

namespace Codebound.Machines
{
    /// <summary>The ten real programming languages of Codebound. Never replaced by pseudocode.</summary>
    public enum ProgrammingLanguage { Html, Css, JavaScript, Sql, Python, Php, TypeScript, C, Cpp, CSharp }

    public enum ProgramStatus
    {
        None,       // no program stored
        Loaded,     // source stored, not (yet) validated/started by the execution runtime
        Running,
        Stopped,
        Error
    }

    /// <summary>Hardware of a computer. Limits what its programs may use (enforced by the sandbox in Etapa 3).</summary>
    public sealed class ComputerHardware
    {
        public static readonly ComputerHardware Basic = new ComputerHardware("hw.basic", cpuFuelPerTick: 2_000_000,
            ramBytes: 16L * 1024 * 1024, storageBytes: 64 * 1024, networkPorts: 4, networkRange: 8, powerWatts: 30);

        public ComputerHardware(string id, ulong cpuFuelPerTick, long ramBytes, int storageBytes, int networkPorts, int networkRange, double powerWatts)
        {
            Id = id; CpuFuelPerTick = cpuFuelPerTick; RamBytes = ramBytes; StorageBytes = storageBytes;
            NetworkPorts = networkPorts; NetworkRange = networkRange; PowerWatts = powerWatts;
        }

        public string Id { get; }
        /// <summary>Instruction budget per simulation tick (Wasm fuel).</summary>
        public ulong CpuFuelPerTick { get; }
        public long RamBytes { get; }
        /// <summary>Bytes available for program files.</summary>
        public int StorageBytes { get; }
        /// <summary>Maximum number of linked machines.</summary>
        public int NetworkPorts { get; }
        /// <summary>Maximum distance (cells, Chebyshev, between footprints) to a linked machine.</summary>
        public int NetworkRange { get; }
        public double PowerWatts { get; }
    }

    public enum LinkResult { Ok, AlreadyLinked, LinkedToOtherComputer, OutOfRange, NoFreePorts, NotProgrammable }

    /// <summary>
    /// Programmable computer: stores the player's program source, links to machines it controls, and consumes power.
    /// The execution runtime (Etapa 3) reports program state through <see cref="MarkRunning"/>/<see cref="MarkError"/>.
    /// </summary>
    public sealed class Computer : Machine
    {
        private readonly List<Machine> _links = new List<Machine>();

        public Computer(PlacedStructure structure, ComputerHardware hardware) : base(structure)
        {
            Hardware = hardware ?? throw new ArgumentNullException(nameof(hardware));
        }

        public ComputerHardware Hardware { get; }
        public IReadOnlyList<Machine> Links => _links;

        public ProgramStatus ProgramStatus { get; private set; } = ProgramStatus.None;
        public ProgrammingLanguage? Language { get; private set; }
        public string Source { get; private set; } = string.Empty;
        public string LastError { get; private set; }

        /// <summary>Program is running and the computer has power.</summary>
        public bool IsRunningProgram => ProgramStatus == ProgramStatus.Running && Connected && Satisfaction > 0;

        public override bool HasValidProgram => true; // the computer hosts programs; it is not controlled by one
        public override bool IsProgrammable => false;
        public override double DemandWatts => Hardware.PowerWatts;

        /// <summary>Stores source code. Fails if it exceeds the computer's storage.</summary>
        public bool LoadProgram(ProgrammingLanguage language, string source)
        {
            source ??= string.Empty;
            if (System.Text.Encoding.UTF8.GetByteCount(source) > Hardware.StorageBytes)
            {
                LastError = "storage_full";
                return false;
            }
            Language = language;
            Source = source;
            ProgramStatus = source.Length == 0 ? ProgramStatus.None : ProgramStatus.Loaded;
            LastError = null;
            return true;
        }

        /// <summary>Called by the execution runtime once the program started successfully.</summary>
        public void MarkRunning()
        {
            if (ProgramStatus == ProgramStatus.None) throw new InvalidOperationException("No program loaded");
            ProgramStatus = ProgramStatus.Running;
            LastError = null;
        }

        /// <summary>Called by the execution runtime on compile/runtime errors or limit violations.</summary>
        public void MarkError(string message)
        {
            ProgramStatus = ProgramStatus.Error;
            LastError = message;
        }

        public void Stop()
        {
            if (ProgramStatus == ProgramStatus.Running) ProgramStatus = ProgramStatus.Stopped;
        }

        public LinkResult Link(Machine machine)
        {
            if (machine == null) throw new ArgumentNullException(nameof(machine));
            if (!machine.IsProgrammable) return LinkResult.NotProgrammable;
            if (machine.ControlledBy == this) return LinkResult.AlreadyLinked;
            if (machine.ControlledBy != null) return LinkResult.LinkedToOtherComputer;
            if (_links.Count >= Hardware.NetworkPorts) return LinkResult.NoFreePorts;
            if (FootprintDistance(Structure, machine.Structure) > Hardware.NetworkRange) return LinkResult.OutOfRange;
            _links.Add(machine);
            machine.ControlledBy = this;
            return LinkResult.Ok;
        }

        public bool Unlink(Machine machine)
        {
            if (machine == null || !_links.Remove(machine)) return false;
            machine.ControlledBy = null;
            return true;
        }

        public void UnlinkAll()
        {
            foreach (var m in _links) m.ControlledBy = null;
            _links.Clear();
        }

        public override void Tick(MachineContext ctx)
        {
            if (!Connected || Satisfaction <= 0) { State = MachineState.NoPower; return; }
            switch (ProgramStatus)
            {
                case ProgramStatus.Running: State = MachineState.Working; break;
                case ProgramStatus.None: State = MachineState.NoProgram; break;
                default: State = MachineState.Idle; break;
            }
        }

        /// <summary>Chebyshev distance between the closest cells of two footprints.</summary>
        public static int FootprintDistance(PlacedStructure a, PlacedStructure b)
        {
            int best = int.MaxValue;
            foreach (var ca in a.Cells)
            foreach (var cb in b.Cells)
                best = Math.Min(best, Math.Max(Math.Abs(ca.X - cb.X), Math.Abs(ca.Y - cb.Y)));
            return best;
        }
    }
}
