using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Codebound.Game
{
    /// <summary>Streams 32x32 chunks of the infinite world into two tilemaps around the camera.</summary>
    public sealed class ChunkRenderer : MonoBehaviour
    {
        const int CS = WorldRules.ChunkSize;
        const int MaxLoadsPerFrame = 3;

        GameSession session;
        Camera cam;
        Tilemap ground, resources;
        readonly HashSet<(int, int)> loaded = new HashSet<(int, int)>();
        readonly Dictionary<(TileType, int), Tile> groundTiles = new Dictionary<(TileType, int), Tile>();
        readonly Dictionary<ResourceType, Tile> resourceTiles = new Dictionary<ResourceType, Tile>();
        readonly List<(int, int)> scratch = new List<(int, int)>();
        readonly TileBase[] block = new TileBase[CS * CS];
        static readonly TileBase[] Empty = new TileBase[CS * CS];

        public int LoadedChunks => loaded.Count;

        public void Init(GameSession s, Camera c)
        {
            session = s; cam = c;
            var gridGo = new GameObject("WorldGrid", typeof(Grid));
            ground = MakeLayer(gridGo.transform, "Ground", 0);
            resources = MakeLayer(gridGo.transform, "Resources", 1);
            session.World.DepositChanged += OnDepositChanged;
            Update(); // fill the first view immediately
        }

        static Tilemap MakeLayer(Transform parent, string name, int order)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<TilemapRenderer>().sortingOrder = order;
            return go.GetComponent<Tilemap>();
        }

        Tile GroundTile(TileType t, int v)
        {
            if (!groundTiles.TryGetValue((t, v), out var tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = PixelArtFactory.Tile(t, v);
                tile.colliderType = Tile.ColliderType.None;
                groundTiles[(t, v)] = tile;
            }
            return tile;
        }

        Tile ResourceTile(ResourceType r)
        {
            if (r == ResourceType.None) return null;
            if (!resourceTiles.TryGetValue(r, out var tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = PixelArtFactory.Resource(r);
                tile.colliderType = Tile.ColliderType.None;
                resourceTiles[r] = tile;
            }
            return tile;
        }

        void Update()
        {
            float hh = cam.orthographicSize, hw = hh * cam.aspect;
            var p = cam.transform.position;
            int x0 = Cell.FloorDiv(Mathf.FloorToInt(p.x - hw), CS) - 1, x1 = Cell.FloorDiv(Mathf.FloorToInt(p.x + hw), CS) + 1;
            int y0 = Cell.FloorDiv(Mathf.FloorToInt(p.y - hh), CS) - 1, y1 = Cell.FloorDiv(Mathf.FloorToInt(p.y + hh), CS) + 1;

            // unload chunks well outside the view (one-chunk hysteresis avoids thrashing)
            scratch.Clear();
            foreach (var k in loaded)
                if (k.Item1 < x0 - 1 || k.Item1 > x1 + 1 || k.Item2 < y0 - 1 || k.Item2 > y1 + 1) scratch.Add(k);
            foreach (var k in scratch) Unload(k);

            int budget = loaded.Count == 0 ? int.MaxValue : MaxLoadsPerFrame;
            int pcx = Cell.FloorDiv(Mathf.FloorToInt(p.x), CS), pcy = Cell.FloorDiv(Mathf.FloorToInt(p.y), CS);
            // nearest chunks first
            for (int r = 0; budget > 0 && r <= Mathf.Max(x1 - x0, y1 - y0); r++)
                for (int cx = pcx - r; cx <= pcx + r && budget > 0; cx++)
                    for (int cy = pcy - r; cy <= pcy + r && budget > 0; cy++)
                    {
                        if (Mathf.Max(Mathf.Abs(cx - pcx), Mathf.Abs(cy - pcy)) != r) continue;
                        if (cx < x0 || cx > x1 || cy < y0 || cy > y1 || loaded.Contains((cx, cy))) continue;
                        Load(cx, cy); budget--;
                    }
        }

        void Load(int cx, int cy)
        {
            var gen = session.World.Generator;
            int ox = cx * CS, oy = cy * CS;
            var bounds = new BoundsInt(ox, oy, 0, CS, CS, 1);
            for (int y = 0; y < CS; y++)
                for (int x = 0; x < CS; x++)
                {
                    var c = new Cell(ox + x, oy + y);
                    int v = (int)(WorldGenerator.Hash01(c.X, c.Y, 5) * PixelArtFactory.TileVariants);
                    block[y * CS + x] = GroundTile(gen.TileAt(c), v);
                }
            ground.SetTilesBlock(bounds, block);
            for (int y = 0; y < CS; y++)
                for (int x = 0; x < CS; x++)
                    block[y * CS + x] = ResourceTile(session.World.DepositAt(new Cell(ox + x, oy + y)).Type);
            resources.SetTilesBlock(bounds, block);
            loaded.Add((cx, cy));
        }

        void Unload((int, int) k)
        {
            var bounds = new BoundsInt(k.Item1 * CS, k.Item2 * CS, 0, CS, CS, 1);
            ground.SetTilesBlock(bounds, Empty);
            resources.SetTilesBlock(bounds, Empty);
            loaded.Remove(k);
        }

        void OnDepositChanged(Cell c)
        {
            if (!loaded.Contains((Cell.FloorDiv(c.X, CS), Cell.FloorDiv(c.Y, CS)))) return;
            resources.SetTile(new Vector3Int(c.X, c.Y, 0), ResourceTile(session.World.DepositAt(c).Type));
        }

        void OnDestroy()
        {
            if (session != null) session.World.DepositChanged -= OnDepositChanged;
        }
    }
}
