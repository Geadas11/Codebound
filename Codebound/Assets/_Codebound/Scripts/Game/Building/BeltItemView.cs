using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Draws the items travelling on conveyor belts (pooled sprites, interpolated by belt progress).</summary>
    public sealed class BeltItemView : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>();
        private GameSession _session;

        public void Init(GameSession session) => _session = session;

        private void LateUpdate()
        {
            if (_session == null) return;
            int used = 0;
            foreach (var belt in _session.Machines.Conveyors.Belts)
            {
                if (belt.IsEmpty) continue;
                var sr = Get(used++);
                sr.sprite = PixelArt.Icon(belt.Item);
                var from = belt.Cell.Center;
                var off = belt.Direction.ToOffset();
                // Item travels from the cell centre towards the next cell's edge (0.5 cell).
                float t = (float)belt.Progress * 0.5f;
                float x = from.X + off.X * t, y = from.Y + off.Y * t;
                sr.transform.position = new Vector3(x - 0.25f, y - 0.25f, 0);
                sr.sortingOrder = SortingOrder.Floor + 10;
            }
            for (int i = used; i < _pool.Count; i++) _pool[i].enabled = false;
        }

        private SpriteRenderer Get(int i)
        {
            while (_pool.Count <= i)
            {
                var go = new GameObject("BeltItem");
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                _pool.Add(go.AddComponent<SpriteRenderer>());
            }
            _pool[i].enabled = true;
            return _pool[i];
        }
    }
}
