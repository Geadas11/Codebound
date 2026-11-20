using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Unity-side view of one running world: the Core GameState plus scene references.</summary>
    public sealed class GameSession
    {
        public readonly GameState State;
        public Transform Player;
        /// <summary>True while a text field (code editor) owns the keyboard: gameplay hotkeys must ignore input.</summary>
        public bool KeyboardCaptured;
        /// <summary>True while a modal panel is open: world clicks (mine/build) are ignored.</summary>
        public bool PanelOpen;
        /// <summary>True while the mouse is over a HUD box: world clicks are ignored.</summary>
        public bool PointerOverHud;
        /// <summary>True while the mouse is over a fixed IMGUI bar (build bar).</summary>
        public bool PointerOverImgui;
        public bool PointerBlocked => PointerOverHud || PointerOverImgui;
        /// <summary>Environment at the player this frame (weather, temperature...), for HUD and visuals.</summary>
        public Surroundings Surroundings;

        public GameSession(GameState state) { State = state; }

        public WorldState World => State.World;
        public BuildGrid Grid => State.Grid;
        public Inventory Inventory => State.Inventory;
        public Factory Factory => State.Factory;
        public int Seed => State.Seed;

        /// <summary>Movement blocking: liquids, structures and trees (ore patches are walkable, Factorio-style).</summary>
        public bool IsSolid(Cell c) =>
            !WorldRules.IsWalkable(World.Generator.TileAt(c)) || (Grid.At(c) is PlacedBuilding b && !b.Def.Walkable) || World.DepositAt(c).Type == ResourceType.Tree;

        public static Cell CellAt(Vector3 p) => new Cell(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
    }
}
