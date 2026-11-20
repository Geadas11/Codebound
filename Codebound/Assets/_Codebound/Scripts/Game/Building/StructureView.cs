using System.Collections.Generic;
using Codebound.Building;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Spawns/destroys sprites for structures as the <see cref="BuildGrid"/> changes.</summary>
    public sealed class StructureView : MonoBehaviour
    {
        private readonly Dictionary<int, GameObject> _views = new Dictionary<int, GameObject>();
        private BuildGrid _grid;

        public void Init(GameSession session)
        {
            _grid = session.Grid;
            foreach (var s in _grid.Structures) OnPlaced(s);
            _grid.Placed += OnPlaced;
            _grid.Removed += OnRemoved;
        }

        private void OnDestroy()
        {
            if (_grid == null) return;
            _grid.Placed -= OnPlaced;
            _grid.Removed -= OnRemoved;
        }

        private void OnPlaced(PlacedStructure s)
        {
            var go = new GameObject(s.ToString());
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(s.Origin.X, s.Origin.Y, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Structure(s.Definition, s.Facing);
            sr.sortingOrder = s.Definition.BlocksMovement ? SortingOrder.ForY(s.Origin.Y)
                : s.Definition.Id == StructureCatalog.Conveyor ? SortingOrder.Floor + 5 : SortingOrder.Floor;
            _views[s.InstanceId] = go;
        }

        private void OnRemoved(PlacedStructure s)
        {
            if (!_views.TryGetValue(s.InstanceId, out var go)) return;
            _views.Remove(s.InstanceId);
            Destroy(go);
        }
    }
}
