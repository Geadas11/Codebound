using System;
using Codebound.Building;
using Codebound.Energy;

namespace Codebound.Machines
{
    public enum MachineState
    {
        NoProgram,      // not controlled by a computer running a valid program
        NoPower,        // not connected, or the network delivers nothing
        Idle,           // controlled and powered, waiting for a command
        Working,
        OutputBlocked,  // output storage full
        Depleted        // nothing left to work on (e.g. no ore under a drill)
    }

    /// <summary>Context passed to machines on every simulation tick.</summary>
    public sealed class MachineContext
    {
        public MachineContext(Codebound.World.WorldMap world, BuildGrid grid, double tickSeconds)
        {
            World = world; Grid = grid; TickSeconds = tickSeconds;
        }

        public Codebound.World.WorldMap World { get; }
        public BuildGrid Grid { get; }
        public double TickSeconds { get; }
        public long Tick { get; internal set; }
    }

    /// <summary>
    /// A structure that performs work on the simulation tick. Design rule: every machine needs a valid program —
    /// it only acts while it is linked to a powered <see cref="Computer"/> whose program is running.
    /// The public methods (e.g. <see cref="Drill.RequestMine"/>) are the Codebound API surface that player
    /// programs call through the sandbox (Etapa 3).
    /// </summary>
    public abstract class Machine : PowerNode
    {
        protected Machine(PlacedStructure structure) : base(structure.Cells)
        {
            Structure = structure ?? throw new ArgumentNullException(nameof(structure));
        }

        public PlacedStructure Structure { get; }
        public MachineState State { get; protected set; } = MachineState.NoProgram;

        /// <summary>Computer controlling this machine, or null.</summary>
        public Computer ControlledBy { get; internal set; }

        /// <summary>True while a powered computer runs a program that controls this machine.</summary>
        public virtual bool HasValidProgram => ControlledBy != null && ControlledBy.IsRunningProgram;

        /// <summary>Whether a computer may link to this machine.</summary>
        public virtual bool IsProgrammable => true;

        public abstract void Tick(MachineContext ctx);

        /// <summary>Common gate: program and power. Returns false (and sets <see cref="State"/>) when the machine cannot act.</summary>
        protected bool CanOperate()
        {
            if (!HasValidProgram) { State = MachineState.NoProgram; return false; }
            if (!Connected) { State = MachineState.NoPower; return false; }
            return true;
        }
    }
}
