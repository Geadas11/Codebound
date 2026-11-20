using System.Collections.Generic;
using Codebound.Core;
using Codebound.World;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Codebound.Game
{
    /// <summary>Renders terrain (Tilemap) and resource nodes (sprites) and keeps them in sync with <see cref="WorldMap"/>.</summary>
    public sealed class WorldView : MonoBehaviour
    {
        private const int Variants = 4;
        private readonly Dictionary<GridPos, SpriteRenderer> _nodeViews = new Dictionary<GridPos, SpriteRenderer>();
        private readonly Dictionary<(TerrainType, int), Tile> _tiles = new Dictionary<(TerrainType, int), Tile>();
        private WorldMap _world;
        private Transform _nodeRoot;

        public void Init(GameSession session)
        {
            _world = session.World;
            BuildTerrain();
            _nodeRoot = new GameObject("ResourceNodes").transform;
            _nodeRoot.SetParent(transform, false);
            foreach (var n in _world.Nodes) CreateNodeView(n);
            _world.NodeDepleted += OnNodeDepleted;
            _world.NodeChanged += OnNodeChanged;
        }

        private void OnDestroy()
        {
            if (_world == null) return;
            _world.NodeDepleted -= OnNodeDepleted;
            _world.NodeChanged -= OnNodeChanged;
        }

        private void BuildTerrain()
        {
            var gridGo = new GameObject("TerrainGrid");
            gridGo.transform.SetParent(transform, false);
            gridGo.AddComponent<Grid>();
            var tmGo = new GameObject("Terrain");
            tmGo.transform.SetParent(gridGo.transform, false);
            var tilemap = tmGo.AddComponent<Tilemap>();
            var tr = tmGo.AddComponent<TilemapRenderer>();
            tr.sortingOrder = SortingOrder.Terrain;

            int w = _world.Width, h = _world.Height;
            var positions = new Vector3Int[w * h];
            var tiles = new TileBase[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                var t = _world.GetTerrain(new GridPos(x, y));
                int variant = (int)(SeededRandom.Hash01(_world.Seed, x, y) * Variants);
                positions[i] = new Vector3Int(x, y, 0);
                tiles[i] = GetTile(t, variant);
            }
            tilemap.SetTiles(positions, tiles);
        }

        private Tile GetTile(TerrainType t, int variant)
        {
            if (_tiles.TryGetValue((t, variant), out var tile)) return tile;
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = PixelArt.Terrain(t, variant);
            tile.colliderType = Tile.ColliderType.None;
            _tiles[(t, variant)] = tile;
            return tile;
        }

        private void CreateNodeView(ResourceNode n)
        {
            var go = new GameObject(n.ItemId);
            go.transform.SetParent(_nodeRoot, false);
            go.transform.position = new Vector3(n.Cell.X, n.Cell.Y, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PixelArt.Node(n.ItemId);
            sr.sortingOrder = SortingOrder.ForY(n.Cell.Y);
            _nodeViews[n.Cell] = sr;
        }

        private void OnNodeChanged(ResourceNode n)
        {
            if (!_nodeViews.TryGetValue(n.Cell, out var sr)) return;
            // Visual feedback: shrink slightly as the deposit empties.
            float f = 0.7f + 0.3f * n.Remaining / (float)n.InitialAmount;
            sr.transform.localScale = new Vector3(f, f, 1);
            sr.transform.position = new Vector3(n.Cell.X + (1 - f) / 2f, n.Cell.Y, 0);
        }

        private void OnNodeDepleted(ResourceNode n)
        {
            if (!_nodeViews.TryGetValue(n.Cell, out var sr)) return;
            _nodeViews.Remove(n.Cell);
            Destroy(sr.gameObject);
        }
    }
}
