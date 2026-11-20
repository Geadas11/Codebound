using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
using Codebound.Machines;
using Codebound.Player;
using Codebound.Save;
using Codebound.World;

namespace Codebound.Game
{
    /// <summary>
    /// Presentation-side view of one running world: the simulation <see cref="GameState"/> plus a UI event bus.
    /// Holds no Unity objects.
    /// </summary>
    public sealed class GameSession
    {
        public GameSession(GameState state)
        {
            State = state ?? throw new System.ArgumentNullException(nameof(state));
            Events = new EventBus();
        }

        public static GameSession CreateNew(long seed) => new GameSession(GameState.CreateNew(seed));

        public GameState State { get; }
        public EventBus Events { get; }
        public SimulationClock Clock => State.Clock;
        public ItemRegistry Items => State.Items;
        public WorldMap World => State.World;
        public BuildGrid Grid => State.Grid;
        public IWalkabilityQuery Walkability => State.Walkability;
        public ItemInventory PlayerInventory => State.PlayerInventory;
        public PlayerMotor Motor => State.Motor;
        public InteractionSystem Interaction => State.Interaction;
        public MachineSystem Machines => State.Machines;
    }

    /// <summary>Published on the session event bus for short on-screen feedback messages.</summary>
    public readonly struct ToastEvent
    {
        public ToastEvent(string text, bool isError = false) { Text = text; IsError = isError; }
        public readonly string Text;
        public readonly bool IsError;
    }
}
