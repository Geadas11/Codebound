using Codebound.Building;
using Codebound.Items;

namespace Codebound.Machines
{
    /// <summary>
    /// Burns coal from its storage to produce power. Fuel is only consumed for the power actually drawn.
    /// Program API: <c>generator.enable(bool)</c>, <c>generator.fuel()</c>.
    /// </summary>
    public sealed class CoalGenerator : Machine
    {
        public const double MaxWatts = 300;
        public const double JoulesPerCoal = 9000; // 30 s at full output

        public CoalGenerator(PlacedStructure structure) : base(structure)
        {
            if (structure.Inventory == null) throw new System.ArgumentException("Generator needs a fuel inventory", nameof(structure));
        }

        /// <summary>Set by the player's program. Generators start disabled.</summary>
        public bool Enabled { get; set; }
        public double BufferedJoules { get; private set; }

        /// <summary>Restores burner state (save games).</summary>
        public void RestoreState(bool enabled, double bufferedJoules)
        {
            Enabled = enabled;
            BufferedJoules = System.Math.Max(0, bufferedJoules);
        }
        public int FuelCount => Structure.Inventory.CountOf(ItemCatalog.Coal);
        public double LastOutputWatts { get; private set; }

        private bool CanRun => HasValidProgram && Enabled && (BufferedJoules > 0 || FuelCount > 0);

        public override double ProductionWatts => CanRun ? MaxWatts : 0;

        protected internal override void OnProductionUsed(double watts, double tickSeconds)
        {
            LastOutputWatts = watts;
            double needed = watts * tickSeconds;
            while (BufferedJoules < needed && Structure.Inventory.TryRemove(ItemCatalog.Coal, 1))
                BufferedJoules += JoulesPerCoal;
            BufferedJoules = System.Math.Max(0, BufferedJoules - needed);
        }

        public override void Tick(MachineContext ctx)
        {
            // A generator controlled by a computer does not need grid power itself, only the program.
            if (!HasValidProgram) { State = MachineState.NoProgram; LastOutputWatts = 0; return; }
            if (!Enabled) { State = MachineState.Idle; LastOutputWatts = 0; return; }
            if (BufferedJoules <= 0 && FuelCount == 0) { State = MachineState.Depleted; LastOutputWatts = 0; return; }
            State = LastOutputWatts > 0 ? MachineState.Working : MachineState.Idle;
        }
    }
}
