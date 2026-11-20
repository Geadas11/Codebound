using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Draws the item riding on each belt/conveyor, sliding toward the front as Progress grows.</summary>
    public sealed class ConveyorItemsView : MonoBehaviour
    {
        GameSession session;
        readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        Transform root;

        public void Init(GameSession s) { session = s; root = new GameObject("BeltItems").transform; }

        static Vector2 Dir(Rotation r) =>
            r == Rotation.R0 ? Vector2.up : r == Rotation.R90 ? Vector2.right : r == Rotation.R180 ? Vector2.down : Vector2.left;

        void LateUpdate()
        {
            int used = 0;
            foreach (var b in session.Grid.Buildings)
            {
                if (!(b.State is ConveyorState c) || c.Item == null) continue;
                if (used == pool.Count)
                {
                    var sr = new GameObject("item").AddComponent<SpriteRenderer>();
                    sr.transform.SetParent(root, false);
                    sr.sortingOrder = 3;
                    pool.Add(sr);
                }
                var r = pool[used++];
                r.enabled = true;
                r.sprite = PixelArtFactory.ItemIcon(c.Item);
                var centre = new Vector2(b.Origin.X + 0.5f, b.Origin.Y + 0.5f);
                r.transform.position = centre + Dir(b.Rotation) * (c.Progress - 0.5f) * 0.9f;
            }
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }
    }
}
