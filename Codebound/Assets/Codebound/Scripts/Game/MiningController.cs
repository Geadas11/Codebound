using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>Manual mining / wood cutting: hold LMB on a deposit within reach. Also shows the hover cursor.</summary>
    public sealed class MiningController : MonoBehaviour
    {
        public const float Reach = 3f, AttackDamage = 12f, AttackInterval = 0.4f;
        float attackTimer;

        GameSession session;
        Camera cam;
        BuildController build;
        SpriteRenderer cursor;
        Cell target;
        float progress;

        public float Progress01 { get; private set; }
        public Cell HoverCell { get; private set; }
        public string LastMessage { get; private set; }
        public float MessageTime { get; private set; }

        public void Init(GameSession s, Camera c, BuildController b)
        {
            session = s; cam = c; build = b;
            cursor = new GameObject("HoverCursor").AddComponent<SpriteRenderer>();
            cursor.sprite = PixelArtFactory.CellOutline();
            cursor.sortingOrder = 6;
        }

        void Update()
        {
            var mouse = Mouse.current;
            HoverCell = build.MouseCell();
            cursor.enabled = !build.Active;
            cursor.transform.position = new Vector3(HoverCell.X, HoverCell.Y, 0);

            var dep = session.World.DepositAt(HoverCell);
            bool inReach = Vector2.Distance(new Vector2(HoverCell.X + 0.5f, HoverCell.Y + 0.5f), session.Player.position) <= Reach;
            cursor.color = dep.IsNone ? new Color(1, 1, 1, 0.35f) : inReach ? new Color(1, 0.9f, 0.3f, 0.9f) : new Color(1, 0.4f, 0.4f, 0.7f);

            // fighting: the multitool hurts creatures under the cursor within reach (they take priority)
            var wp = cam.ScreenToWorldPoint(new Vector3(mouse != null ? mouse.position.ReadValue().x : 0, mouse != null ? mouse.position.ReadValue().y : 0, -cam.transform.position.z));
            Creature foe = null;
            foreach (var c in session.State.Creatures.All)
                if (c.Alive && Vector2.Distance(new Vector2((float)c.X, (float)c.Y), wp) < 0.7f
                    && Vector2.Distance(new Vector2((float)c.X, (float)c.Y), session.Player.position) <= Reach + 0.5f) { foe = c; break; }
            if (foe != null)
            {
                cursor.color = new Color(1f, 0.3f, 0.3f, 0.95f);
                cursor.transform.position = new Vector3(Mathf.Floor((float)foe.X), Mathf.Floor((float)foe.Y), 0);
                if (!build.Active && !session.PanelOpen && !session.PointerBlocked && mouse != null && mouse.leftButton.isPressed)
                {
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0) { session.State.Creatures.Hurt(foe, AttackDamage); attackTimer = AttackInterval; AudioDirector.Instance?.Play("mine", new Vector2((float)foe.X, (float)foe.Y), null); }
                    progress = 0; Progress01 = 0;
                    return;
                }
            }

            // drinking: hold LMB on (toxic) water within reach
            var tile = session.World.Generator.TileAt(HoverCell);
            bool water = tile == TileType.Water || tile == TileType.ToxicWater;
            if (water && dep.IsNone) cursor.color = inReach ? new Color(0.4f, 0.8f, 1f, 0.9f) : new Color(1, 0.4f, 0.4f, 0.7f);
            bool canAct = !build.Active && !session.PanelOpen && !session.PointerBlocked && mouse != null && mouse.leftButton.isPressed && inReach;
            if (canAct && water && dep.IsNone)
            {
                if (HoverCell != target) { target = HoverCell; progress = 0; }
                progress += Time.deltaTime;
                Progress01 = Mathf.Clamp01(progress / 0.5f);
                if (progress < 0.5f) return;
                progress = 0;
                session.State.Vitals.Drink(tile == TileType.ToxicWater);
                AudioDirector.Instance?.Play("drink", null, null);
                if (tile == TileType.ToxicWater) { LastMessage = "Toxic water! It quenches thirst but hurts"; MessageTime = Time.time; }
                return;
            }

            if (!canAct || dep.IsNone || session.Grid.IsOccupied(HoverCell))
            {
                progress = 0; Progress01 = 0;
                return;
            }

            if (HoverCell != target) { target = HoverCell; progress = 0; }
            float need = WorldRules.ManualMineTime(dep.Type);
            progress += Time.deltaTime;
            Progress01 = Mathf.Clamp01(progress / need);
            if (progress < need) return;
            progress = 0;

            string item = WorldRules.ItemFor(dep.Type);
            if (session.Inventory.Add(item, 1) > 0) { LastMessage = "Inventory full"; MessageTime = Time.time; return; }
            session.World.Extract(HoverCell, 1);
            AudioDirector.Instance?.Play("mine", new Vector2(HoverCell.X + 0.5f, HoverCell.Y + 0.5f), null);
            // trees sometimes carry edible alien fruit
            if (dep.Type == ResourceType.Tree && Random.value < 0.3f && session.Inventory.Add(ItemDatabase.AlienFruit, 1) == 0)
            { LastMessage = "Found an alien fruit (F to eat)"; MessageTime = Time.time; }
        }
    }
}
