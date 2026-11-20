using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
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
    }

    /// <summary>Published on the session event bus for short on-screen feedback messages.</summary>
    public readonly struct ToastEvent
    {
        public ToastEvent(string text, bool isError = false) { Text = text; IsError = isError; }
        public readonly string Text;
        public readonly bool IsError;
    }
}
