using System;
using System.Collections.Generic;
using Codebound.Core;

namespace Codebound.World
{
    /// <summary>
    /// Bounded terrain + resource map. Etapa 1 uses a single fixed-size map; Etapa 5 replaces the
    /// storage with streamed chunks behind the same queries.
    /// </summary>
    public sealed class WorldMap : IWalkabilityQuery, ITerrainBuildQuery
    {
        private readonly TerrainType[] _terrain;
        private readonly Dictionary<GridPos, ResourceNode> _nodes = new Dictionary<GridPos, ResourceNode>();

        public WorldMap(int width, int height, long seed)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            Width = width;
            Height = height;
            Seed = seed;
            _terrain = new TerrainType[width * height];
        }

        public int Width { get; }
        public int Height { get; }
        public long Seed { get; }
        public GridPos SpawnPoint { get; set; }

        public event Action<ResourceNode> NodeChanged;
        public event Action<ResourceNode> NodeDepleted;

        public bool InBounds(GridPos c) => c.X >= 0 && c.Y >= 0 && c.X < Width && c.Y < Height;

        public TerrainType GetTerrain(GridPos c) => InBounds(c) ? _terrain[c.Y * Width + c.X] : TerrainType.Rock;

        public void SetTerrain(GridPos c, TerrainType t)
        {
            if (!InBounds(c)) throw new ArgumentOutOfRangeException(nameof(c));
            _terrain[c.Y * Width + c.X] = t;
        }

        public bool IsWalkable(GridPos c) => InBounds(c) && GetTerrain(c).IsWalkable() && !_nodes.ContainsKey(c);

        public bool IsBuildable(GridPos c) => InBounds(c) && GetTerrain(c).IsBuildable() && !_nodes.ContainsKey(c);

        public bool IsBuildableTerrain(GridPos c) => InBounds(c) && GetTerrain(c).IsBuildable();

        public bool HasResource(GridPos c) => _nodes.ContainsKey(c);

        public IReadOnlyCollection<ResourceNode> Nodes => _nodes.Values;

        public bool TryGetNode(GridPos c, out ResourceNode node) => _nodes.TryGetValue(c, out node);

        public void AddNode(ResourceNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!InBounds(node.Cell)) throw new ArgumentOutOfRangeException(nameof(node));
            _nodes[node.Cell] = node;
        }

        public bool RemoveNode(GridPos c) => _nodes.Remove(c);

        /// <summary>Harvests one yield from the node at <paramref name="c"/>. Depleted nodes are removed.</summary>
        public int Harvest(GridPos c, out string itemId, int max = -1)
        {
            itemId = null;
            if (!_nodes.TryGetValue(c, out var node)) return 0;
            itemId = node.ItemId;
            int taken = node.Take(max);
            if (node.Depleted)
            {
                _nodes.Remove(c);
                NodeDepleted?.Invoke(node);
            }
            else if (taken > 0) NodeChanged?.Invoke(node);
            return taken;
        }
    }
}
