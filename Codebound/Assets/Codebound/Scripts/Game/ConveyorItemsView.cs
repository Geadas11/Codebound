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

        void Draw(string item, Vector2 at, float scale)
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
            r.sprite = PixelArtFactory.ItemIcon(item);
            r.transform.position = at;
            r.transform.localScale = Vector3.one * scale;
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
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }
    }
}
