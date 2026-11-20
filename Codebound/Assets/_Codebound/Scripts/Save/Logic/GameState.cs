using System;
using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
using Codebound.Machines;
using Codebound.Player;
using Codebound.World;

namespace Codebound.Save
{
    /// <summary>
    /// Every simulation object of one world, wired together. Used both for new worlds and for loaded saves
    /// so there is exactly one way the pieces are connected.
    /// </summary>
    public sealed class GameState
    {
        public const int PlayerInventorySlots = 24;

        private GameState(ItemRegistry items, WorldMap world)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            World = world ?? throw new ArgumentNullException(nameof(world));
            Clock = new SimulationClock();
            Grid = new BuildGrid(World, Items);
            Walkability = Grid.WithTerrain(World);
            PlayerInventory = new ItemInventory(PlayerInventorySlots, Items);
            Motor = new PlayerMotor(World.SpawnPoint.Center);
            Interaction = new InteractionSystem(World, Grid);
            Machines = new MachineSystem(World, Grid, Clock.TickDuration);
            Interaction.IsInspectable = s => Machines.GetPowerNode(s) != null;
            Clock.Ticked += Machines.Tick;
        }

        /// <summary>Fresh world from a seed, with the crash-site wreckage placed.</summary>
        public static GameState CreateNew(long seed, ItemRegistry items = null)
        {
            items ??= ItemCatalog.CreateDefault();
            var state = new GameState(items, TestWorldGenerator.Generate(seed));
            state.PlaceWreckage();
            return state;
        }

        /// <summary>World regenerated from the seed, without wreckage (a save restores it). Used by the loader.</summary>
        internal static GameState CreateForLoad(long seed, int width, int height, ItemRegistry items) =>
            new GameState(items, TestWorldGenerator.Generate(seed, width, height));

        public ItemRegistry Items { get; }
        public WorldMap World { get; }
        public SimulationClock Clock { get; }
        public BuildGrid Grid { get; }
        public IWalkabilityQuery Walkability { get; }
        public ItemInventory PlayerInventory { get; }
        public PlayerMotor Motor { get; }
        public InteractionSystem Interaction { get; }
        public MachineSystem Machines { get; }

        private void PlaceWreckage()
        {
            var def = StructureCatalog.Find(StructureCatalog.EmergencyBattery);
            var spawn = World.SpawnPoint;
            for (int r = 2; r < 8; r++)
            for (int dy = -r; dy <= r; dy++)
            for (int dx = -r; dx <= r; dx++)
                if (Grid.TryPlace(def, new GridPos(spawn.X + dx, spawn.Y + dy), Direction.North, null, out _) == PlacementStatus.Ok)
                    return;
            CbLog.Warn("World", "Could not place the emergency battery near spawn");
        }
    }
}
