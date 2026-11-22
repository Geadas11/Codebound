using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>Cave mouths and ladders: drawn near the player, E goes down (or up). Underground it is dark, so the
    /// player carries a lamp. Single-player only for now (multiplayer guests stay on the host's layer).</summary>
    public sealed class CaveController : MonoBehaviour
    {
        GameSession session;
        BuildController build;
        GameBootstrap boot;
        readonly Dictionary<Cell, SpriteRenderer> views = new Dictionary<Cell, SpriteRenderer>();
        float refresh;

        public void Init(GameSession s, BuildController b, GameBootstrap gb) { session = s; build = b; boot = gb; }

        void Update()
        {
            if ((refresh -= Time.deltaTime) <= 0) { refresh = 0.5f; Refresh(); }
            var kb = Keyboard.current;
            if (kb == null || session.KeyboardCaptured || session.PanelOpen || OptionsMenu.IsOpen || build.Active || !Controls.Pressed("interact")) return;
            var cell = build.MouseCell();
            if (!Caves.IsPortal(session.World.Generator, cell)) return;
            if (Vector2.Distance(new Vector2(cell.X + 0.5f, cell.Y + 0.5f), session.Player.position) > MiningController.Reach) return;
            if (Net.IsGuest) { session.State.Hud.Notify(Loc.T("cave.mpGuest", "Only the host can take the group underground (everyone follows)")); return; }
            bool down = !Caves.IsUnderground(session.State);
            string target = down ? Caves.UndergroundId(session.State.PlanetId) : Caves.SurfaceId(session.State.PlanetId);
            boot.SwitchWorld(target, saved => Caves.Pass(session.State, cell, saved),
                down ? Loc.T("cave.down", "You climb down into the caves. Rich ore, crystals — and no daylight. The ladder (E) leads back up.")
                     : Loc.T("cave.up", "Back on the surface."));
        }

        void Refresh()
        {
            var p = GameSession.CellAt(session.Player.position);
            bool up = Caves.IsUnderground(session.State);
            var near = new HashSet<Cell>(Caves.Near(session.World.Generator, p, 48));
            foreach (var c in near)
            {
                if (views.ContainsKey(c)) continue;
                var sr = new GameObject(up ? "Ladder" : "CaveMouth").AddComponent<SpriteRenderer>();
                sr.sprite = PixelArtFactory.CavePortal(up); sr.sortingOrder = 2;
                sr.transform.SetParent(transform, false);
                sr.transform.position = new Vector3(c.X, c.Y, 0);
                views[c] = sr;
            }
            foreach (var c in new List<Cell>(views.Keys)) if (!near.Contains(c)) { Destroy(views[c].gameObject); views.Remove(c); }
        }
    }
}
