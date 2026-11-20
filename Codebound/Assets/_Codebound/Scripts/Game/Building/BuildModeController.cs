using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Build mode: select a structure, see a ghost preview coloured by validity (green ok, yellow missing
    /// resources, red blocked), rotate, place (charged from the player inventory) and deconstruct (refund).
    /// </summary>
    public sealed class BuildModeController : MonoBehaviour
    {
        private static readonly Color Valid = new Color(0.35f, 1f, 0.45f, 0.55f);
        private static readonly Color NoResources = new Color(1f, 0.85f, 0.25f, 0.55f);
        private static readonly Color Blocked = new Color(1f, 0.3f, 0.3f, 0.6f);

        private readonly List<SpriteRenderer> _cellMarkers = new List<SpriteRenderer>();
        private GameSession _session;
        private Camera _cam;
        private SpriteRenderer _ghost;
        private PlacementCheck _lastCheck;

        public bool Active { get; private set; }
        public int SelectedIndex { get; private set; }
        public Direction Facing { get; private set; } = Direction.North;
        public StructureDefinition Selected => StructureCatalog.Default[SelectedIndex];
        public PlacementCheck LastCheck => _lastCheck;

        /// <summary>Set by the HUD every frame: true when the mouse is over a HUD panel.</summary>
        public System.Func<bool> PointerOverHud { get; set; } = () => false;

        public void Init(GameSession session, Camera cam)
        {
            _session = session;
            _cam = cam;
            var g = new GameObject("Ghost");
            g.transform.SetParent(transform, false);
            _ghost = g.AddComponent<SpriteRenderer>();
            _ghost.sortingOrder = SortingOrder.Ghost;
            SetActive(false);
        }

        public void SetActive(bool on)
        {
            Active = on;
            _ghost.enabled = on;
            if (!on) foreach (var m in _cellMarkers) m.enabled = false;
        }

        public void Select(int index)
        {
            if (index < 0 || index >= StructureCatalog.Default.Count) return;
            SelectedIndex = index;
            if (!Active) SetActive(true);
        }

        private void Update()
        {
            if (_session == null) return;
            if (GameInput.Pressed(GameInput.Action.ToggleBuild)) SetActive(!Active);
            int n = GameInput.NumberPressed();
            if (n > 0) Select(n - 1);
            if (!Active) return;
            if (GameInput.Pressed(GameInput.Action.Cancel)) { SetActive(false); return; }
            if (GameInput.Pressed(GameInput.Action.Rotate) && Selected.Rotatable) Facing = Facing.RotateClockwise();

            var cursor = MouseCell();
            var def = Selected;
            var origin = Footprint.OriginCenteredOn(cursor, def.Width, def.Height, def.Rotatable ? Facing : Direction.North);
            var facing = def.Rotatable ? Facing : Direction.North;
            _lastCheck = _session.Grid.Validate(def, origin, facing, _session.PlayerInventory);
            DrawGhost(def, origin, facing, _lastCheck);

            if (PointerOverHud()) return;
            if (GameInput.PrimaryPressed) Place(def, origin, facing);
            else if (GameInput.SecondaryPressed) Deconstruct(cursor);
        }

        private void Place(StructureDefinition def, GridPos origin, Direction facing)
        {
            var status = _session.Grid.TryPlace(def, origin, facing, _session.PlayerInventory, out var placed);
            switch (status)
            {
                case PlacementStatus.Ok: Toast(Loc.Format("toast.placed", Loc.Get(placed.Definition.NameKey))); break;
                case PlacementStatus.InsufficientResources: Toast(Loc.Get("toast.no_resources"), true); break;
                case PlacementStatus.Occupied: Toast(Loc.Get("toast.occupied"), true); break;
                default: Toast(Loc.Get("toast.blocked"), true); break;
            }
        }

        private void Deconstruct(GridPos cell)
        {
            var status = _session.Grid.TryRemove(cell, _session.PlayerInventory, out var removed);
            if (status == RemovalStatus.Ok) Toast(Loc.Format("toast.removed", Loc.Get(removed.Definition.NameKey)));
            else if (status == RemovalStatus.NoSpaceForRefund) Toast(Loc.Get("toast.no_space_refund"), true);
        }

        private void DrawGhost(StructureDefinition def, GridPos origin, Direction facing, PlacementCheck check)
        {
            _ghost.sprite = PixelArt.Structure(def, facing);
            _ghost.transform.position = new Vector3(origin.X, origin.Y, 0);
            var color = check.IsValid ? Valid : check.Status == PlacementStatus.InsufficientResources ? NoResources : Blocked;
            _ghost.color = color;

            // Per-cell red markers on blocked cells.
            int needed = check.BlockedCells.Count;
            while (_cellMarkers.Count < needed)
            {
                var go = new GameObject("BlockedCell");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PixelArt.White;
                sr.color = new Color(1f, 0.15f, 0.15f, 0.35f);
                sr.sortingOrder = SortingOrder.Ghost + 1;
                _cellMarkers.Add(sr);
            }
            for (int i = 0; i < _cellMarkers.Count; i++)
            {
                bool on = i < needed;
                _cellMarkers[i].enabled = on;
                if (on) _cellMarkers[i].transform.position = new Vector3(check.BlockedCells[i].X, check.BlockedCells[i].Y, 0);
            }
        }

        private GridPos MouseCell()
        {
            var m = GameInput.MousePosition;
            var w = _cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -_cam.transform.position.z));
            return GridPos.FromWorld(w.x, w.y);
        }

        private void Toast(string text, bool error = false) => _session.Events.Publish(new ToastEvent(text, error));
    }
}
