using Codebound.Core;
using Codebound.Player;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Shows what the player can interact with (highlight frame) and performs it on E / left click:
    /// harvesting resource nodes (hold to keep harvesting) and opening storage.
    /// </summary>
    public sealed class InteractionController : MonoBehaviour
    {
        public const float HarvestInterval = 0.35f;

        private GameSession _session;
        private Camera _cam;
        private BuildModeController _build;
        private DevHud _hud;
        private SpriteRenderer _highlight;
        private float _cooldown;

        public InteractionTarget Current { get; private set; } = InteractionTarget.None;

        public void Init(GameSession session, Camera cam, BuildModeController build, DevHud hud)
        {
            _session = session;
            _cam = cam;
            _build = build;
            _hud = hud;
            var go = new GameObject("TargetHighlight");
            go.transform.SetParent(transform, false);
            _highlight = go.AddComponent<SpriteRenderer>();
            _highlight.sprite = PixelArt.Frame;
            _highlight.sortingOrder = SortingOrder.Highlight;
            _highlight.color = new Color(1f, 0.95f, 0.5f, 0.9f);
        }

        private void Update()
        {
            if (_session == null) return;
            _cooldown -= Time.deltaTime;

            if (_hud.LinkSource != null)
            {
                _highlight.enabled = false;
                if (GameInput.PrimaryPressed && !_hud.PointerOverHud()) TryLink();
                return;
            }

            if (_build.Active)
            {
                Current = InteractionTarget.None;
                _highlight.enabled = false;
                return;
            }

            var m = GameInput.MousePosition;
            var w = _cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -_cam.transform.position.z));
            var mouseCell = GridPos.FromWorld(w.x, w.y);
            Current = _session.Interaction.FindTarget(_session.Motor.Position, mouseCell);

            _highlight.enabled = Current.Kind != InteractionKind.None;
            if (_highlight.enabled) _highlight.transform.position = new Vector3(Current.Cell.X, Current.Cell.Y, 0);

            bool overHud = _hud.PointerOverHud();
            bool trigger = GameInput.Held(GameInput.Action.Interact) || (!overHud && GameInput.PrimaryHeld);
            bool pressedNow = GameInput.Pressed(GameInput.Action.Interact) || (!overHud && GameInput.PrimaryPressed);
            if (!trigger || Current.Kind == InteractionKind.None) return;

            if (Current.Kind == InteractionKind.Harvest && _cooldown <= 0f)
            {
                _cooldown = HarvestInterval;
                var result = _session.Interaction.Harvest(Current, _session.PlayerInventory, out int gained);
                if (result == HarvestResult.Ok)
                    _session.Events.Publish(new ToastEvent(Loc.Format("toast.gained", gained, Loc.Get("name." + Current.Node.ItemId))));
                else if (result == HarvestResult.InventoryFull)
                    _session.Events.Publish(new ToastEvent(Loc.Get("toast.inventory_full"), true));
            }
            else if (Current.Kind == InteractionKind.OpenStorage && pressedNow)
            {
                _hud.OpenStorage(Current.Structure);
            }
            else if (Current.Kind == InteractionKind.Inspect && pressedNow)
            {
                _hud.Inspect(Current.Structure);
            }
        }

        private void TryLink()
        {
            var computer = _hud.LinkSource;
            _hud.LinkSource = null;
            var m = GameInput.MousePosition;
            var w = _cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -_cam.transform.position.z));
            var target = _session.Machines.Get(_session.Grid.GetAt(GridPos.FromWorld(w.x, w.y)));
            if (target == null) { _session.Events.Publish(new ToastEvent(Loc.Get("toast.link.no_machine"), true)); return; }
            var result = computer.Link(target);
            bool ok = result == Codebound.Machines.LinkResult.Ok;
            _session.Events.Publish(new ToastEvent(Loc.Get("toast.link." + result), !ok));
        }
    }
}
