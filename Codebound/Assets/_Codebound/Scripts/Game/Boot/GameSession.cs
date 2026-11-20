using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
using Codebound.Machines;
using Codebound.Player;
using Codebound.World;

namespace Codebound.Game
{
    /// <summary>
    /// Plain container of the simulation objects for one running world. Created by <see cref="GameBootstrap"/>
    /// and handed to every presentation component. Holds no Unity objects.
    /// </summary>
    public sealed class GameSession
    {
        public const int PlayerInventorySlots = 24;

        public GameSession(long seed)
        {
            Events = new EventBus();
            Clock = new SimulationClock();
            Items = ItemCatalog.CreateDefault();
            World = TestWorldGenerator.Generate(seed);
            Grid = new BuildGrid(World, Items);
            Walkability = Grid.WithTerrain(World);
            PlayerInventory = new ItemInventory(PlayerInventorySlots, Items);
            Motor = new PlayerMotor(World.SpawnPoint.Center);
            Interaction = new InteractionSystem(World, Grid);
            Machines = new MachineSystem(World, Grid, Clock.TickDuration);
            Interaction.IsInspectable = s => Machines.GetPowerNode(s) != null;
            Clock.Ticked += Machines.Tick;
            PlaceWreckage();
        }

        /// <summary>Puts the crashed ship's emergency battery next to the spawn point.</summary>
        private void PlaceWreckage()
        {
            var def = StructureCatalog.Find(StructureCatalog.EmergencyBattery);
            var spawn = World.SpawnPoint;
            for (int r = 2; r < 8; r++)
            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
            {
                var origin = new GridPos(spawn.X + dx, spawn.Y + dy);
                if (Grid.TryPlace(def, origin, Direction.North, null, out _) == PlacementStatus.Ok) return;
            }
            CbLog.Warn("World", "Could not place the emergency battery near spawn");
        }

        public EventBus Events { get; }
        public SimulationClock Clock { get; }
        public ItemRegistry Items { get; }
        public WorldMap World { get; }
        public BuildGrid Grid { get; }
        public IWalkabilityQuery Walkability { get; }
        public ItemInventory PlayerInventory { get; }
        public PlayerMotor Motor { get; }
        public InteractionSystem Interaction { get; }
        public MachineSystem Machines { get; }
    }

    /// <summary>Published on the session event bus for short on-screen feedback messages.</summary>
    public readonly struct ToastEvent
    {
        public ToastEvent(string text, bool isError = false) { Text = text; IsError = isError; }
        public readonly string Text;
        public readonly bool IsError;
    }
}
