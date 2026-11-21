using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>Grid build mode: preview, rotation, cost check, placement and removal with refund.</summary>
    public sealed class BuildController : MonoBehaviour
    {
        public const float Reach = 10f; // cells from the player

        GameSession session;
        Camera cam;
        SpriteRenderer preview;
        PlayerController player;

        public bool Active { get; private set; }
        public int Selected { get; private set; }
        public Rotation Rotation { get; private set; }
        public string LastMessage { get; private set; }
        public float MessageTime { get; private set; }

        public BuildingDef SelectedDef => BuildingDatabase.All[Selected];

        public void Init(GameSession s, Camera c)
        {
            session = s; cam = c;
            player = s.Player.GetComponent<PlayerController>();
            var go = new GameObject("BuildPreview");
            preview = go.AddComponent<SpriteRenderer>();
            preview.sortingOrder = 5;
            preview.enabled = false;
        }

        /// <summary>Select a structure (build bar click or hotkey) and enter build mode.</summary>
        public void Select(int index) { if (index >= 0 && index < BuildingDatabase.All.Count) { Selected = index; Active = true; } }

        void Say(string msg) { LastMessage = msg; MessageTime = Time.time; }

        public Cell MouseCell()
        {
            var m = Mouse.current;
            Vector2 sp = m != null ? m.position.ReadValue() : Vector2.zero;
            var wp = cam.ScreenToWorldPoint(new Vector3(sp.x, sp.y, -cam.transform.position.z));
            return new Cell(Mathf.FloorToInt(wp.x), Mathf.FloorToInt(wp.y));
        }

        /// <summary>Footprint origin so the mouse cell sits at the footprint centre.</summary>
        Cell OriginAt(Cell mouse)
        {
            var (w, h) = SelectedDef.Size(Rotation);
            return new Cell(mouse.X - (w - 1) / 2, mouse.Y - (h - 1) / 2);
        }

        bool InReach(Cell c) => Vector2.Distance(new Vector2(c.X + 0.5f, c.Y + 0.5f), session.Player.position) <= Reach;

        void Update()
        {
            var kb = Keyboard.current; var mouse = Mouse.current;
            if (kb == null || mouse == null) return;
            if (session.KeyboardCaptured) { preview.enabled = false; return; }

            if (Controls.Pressed("build")) Active = !Active;
            if (Active && (kb.escapeKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame)) Active = false;
            for (int i = 0; i < BuildingDatabase.All.Count && i < 10; i++)
                if (kb[i == 9 ? Key.Digit0 : Key.Digit1 + i].wasPressedThisFrame) { Selected = i; Active = true; }
            if (Active && Controls.Pressed("rotate")) Rotation = Rotation.Next();

            preview.enabled = Active;
            if (!Active) return;

            var def = SelectedDef;
            var mc = MouseCell();
            var origin = OriginAt(mc);
            bool ok = session.State.CanPlace(def, origin, Rotation, player.Overlaps) && InReach(mc);
            bool afford = session.Inventory.Has(def.Cost);
            bool unlocked = session.State.Research.IsUnlocked(def);
            ok &= unlocked;

            preview.sprite = PixelArtFactory.Building(def);
            BuildingViews.Pose(def, origin, Rotation, preview.transform);
            preview.color = ok && afford ? new Color(0.5f, 1f, 0.5f, 0.65f) : new Color(1f, 0.35f, 0.35f, 0.55f);

            if (session.PanelOpen || session.PointerBlocked) return;
            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (!unlocked) Say($"Requires research: {Research.UnlockerOf(def.Id).Name} (T to see the tech tree)");
                else if (!ok) Say(!InReach(mc) ? "Too far" : def.Kind == BuildingKind.Drill ? "Drills must cover ore (not trees) on free land" : "Blocked: terrain, resource, structure or you are in the way");
                else if (Net.IsGuest) { Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "build" }, { "def", def.Id }, { "x", (double)origin.X }, { "y", (double)origin.Y }, { "rot", (double)(int)Rotation } }); }
                else if (session.State.TryBuild(def, origin, Rotation, player.Overlaps) == null) Say("Not enough materials");
                else Say(def.Programmable ? $"{def.DisplayName} placed — it only works when a computer program drives it" : $"{def.DisplayName} placed");
            }

            if (mouse.rightButton.wasPressedThisFrame)
            {
                var b = session.Grid.At(mc);
                if (b == null) return;
                if (!InReach(mc)) { Say("Too far"); return; }
                if (Net.IsGuest) { Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "remove" }, { "x", (double)mc.X }, { "y", (double)mc.Y } }); return; }
                if (!session.State.TryDeconstruct(b)) { Say("Inventory full: cannot refund cost and contents"); return; }
                Say($"{b.Def.DisplayName} removed (refunded)");
            }
        }
    }
}
