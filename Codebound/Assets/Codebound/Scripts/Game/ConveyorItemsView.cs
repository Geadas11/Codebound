using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Draws the items riding on belts (two lanes each) and the item held in each arm's hand.</summary>
    public sealed class ConveyorItemsView : MonoBehaviour
    {
        GameSession session;
        readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        Transform root;
        int used;

        public void Init(GameSession s) { session = s; root = new GameObject("BeltItems").transform; }

        static Vector2 Dir(Rotation r) =>
            r == Rotation.R0 ? Vector2.up : r == Rotation.R90 ? Vector2.right : r == Rotation.R180 ? Vector2.down : Vector2.left;

        void Draw(string item, Vector2 at, float scale, Sprite sprite = null, int order = 3, float angle = 0f)
        {
            if (used == pool.Count)
            {
                var sr = new GameObject("item").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(root, false);
                sr.sortingOrder = 3;
                pool.Add(sr);
            }
            var r = pool[used++];
            r.enabled = true;
            r.sprite = sprite ?? PixelArtFactory.ItemIcon(item);
            r.sortingOrder = order;
            r.transform.position = at;
            r.transform.localScale = Vector3.one * scale;
            r.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        void LateUpdate()
        {
            used = 0;
            foreach (var b in session.Grid.Buildings)
            {
                var centre = new Vector2(b.Origin.X + 0.5f, b.Origin.Y + 0.5f);
                var fwd = Dir(b.Rotation);
                if (b.State is ConveyorState c && c.ItemCount > 0)
                {
                    var right = Dir(b.Rotation.Next());
                    for (int l = 0; l < ConveyorState.Lanes; l++)
                        foreach (var it in c.Lane(l))
                            Draw(it.Id, centre + fwd * (it.Pos - 0.5f) + right * (l == 1 ? 0.22f : -0.22f), 0.55f);
                }
                else if (b.State is InserterState ins && ins.Held != null)
                {
                    float reach = b.Def.Id == "long_inserter" ? 2f : 1f;
                    Draw(ins.Held, centre + fwd * Mathf.Lerp(-reach, reach, ins.Arm) * 0.6f, 0.6f);   // the hand travels from the pickup side to the drop side
                }
            }
            foreach (var (depot, t) in session.Factory.Trains())   // trains: each unit slides from its cell towards the one ahead
            {
                bool moving = session.Factory.NextCell(depot, out var next);
                float p = moving ? t.Progress : 0f;
                for (int i = 0; i < t.Length && i < t.Trail.Count; i++)
                {
                    var from = t.Trail[i]; var to = i == 0 ? (moving ? next : from) : t.Trail[i - 1];
                    var a = new Vector2(from.X + 0.5f, from.Y + 0.5f); var b = new Vector2(to.X + 0.5f, to.Y + 0.5f);
                    var dir = b - a;
                    if (dir.sqrMagnitude < 1e-4f && i + 1 < t.Trail.Count) dir = a - new Vector2(t.Trail[i + 1].X + 0.5f, t.Trail[i + 1].Y + 0.5f);
                    float ang = dir.sqrMagnitude < 1e-4f ? 0f : Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    Draw(null, Vector2.Lerp(a, b, p), 1.05f, PixelArtFactory.TrainUnit(i == 0), 5, ang);
                }
            }
            foreach (var r in session.Factory.Robots)   // logistic robots in flight, with what they carry
            {
                var at = new Vector2((float)r.X, (float)r.Y + 0.35f);
                Draw(null, at, 1.1f, PixelArtFactory.Robot(), 6);
                if (r.Item != null && r.Task == RobotTask.ToDrop) Draw(r.Item, at + new Vector2(0, -0.3f), 0.45f, null, 7);
            }
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }
    }
}
