using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
using Codebound.Player;
using Codebound.World;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class WorldAndPlayerTests
    {
        [Test]
        public void Generator_IsDeterministicPerSeed()
        {
            var a = TestWorldGenerator.Generate(42, 64, 64);
            var b = TestWorldGenerator.Generate(42, 64, 64);
            var c = TestWorldGenerator.Generate(43, 64, 64);
            bool differs = false;
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                var p = new GridPos(x, y);
                Assert.AreEqual(a.GetTerrain(p), b.GetTerrain(p));
                Assert.AreEqual(a.TryGetNode(p, out _), b.TryGetNode(p, out _));
                if (a.GetTerrain(p) != c.GetTerrain(p)) differs = true;
            }
            Assert.AreEqual(a.Nodes.Count, b.Nodes.Count);
            Assert.IsTrue(differs, "different seeds should give different terrain");
        }

        [Test]
        public void Generator_SpawnIsWalkableAndStarterResourcesExist([Values(1, 2, 3, 99, 2026)] long seed)
        {
            var map = TestWorldGenerator.Generate(seed);
            Assert.IsTrue(map.IsWalkable(map.SpawnPoint));
            int scrap = 0, iron = 0, copper = 0, wood = 0, stone = 0;
            foreach (var n in map.Nodes)
            {
                if (n.ItemId == ItemCatalog.Scrap) scrap++;
                if (n.ItemId == ItemCatalog.IronOre && n.Cell.ManhattanDistance(map.SpawnPoint) < 30) iron++;
                if (n.ItemId == ItemCatalog.CopperOre && n.Cell.ManhattanDistance(map.SpawnPoint) < 30) copper++;
                if (n.ItemId == ItemCatalog.Wood) wood++;
                if (n.ItemId == ItemCatalog.Stone) stone++;
            }
            Assert.Greater(scrap, 5);
            Assert.Greater(iron, 0);
            Assert.Greater(copper, 0);
            Assert.Greater(wood, 0);
            Assert.Greater(stone, 0);
        }

        [Test]
        public void Generator_StarterResourcesAreReachableOnFoot([Values(1, 2, 3, 99, 2026, 777, 31337)] long seed)
        {
            var map = TestWorldGenerator.Generate(seed);
            // Flood fill walkable cells from spawn.
            var seen = new System.Collections.Generic.HashSet<GridPos> { map.SpawnPoint };
            var queue = new System.Collections.Generic.Queue<GridPos>();
            queue.Enqueue(map.SpawnPoint);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    var n = c + ((Direction)d).ToOffset();
                    if (map.IsWalkable(n) && seen.Add(n)) queue.Enqueue(n);
                }
            }
            bool Reachable(string item)
            {
                foreach (var node in map.Nodes)
                {
                    if (node.ItemId != item) continue;
                    for (int d = 0; d < 4; d++)
                        if (seen.Contains(node.Cell + ((Direction)d).ToOffset())) return true;
                }
                return false;
            }
            foreach (var item in new[] { ItemCatalog.Scrap, ItemCatalog.Wood, ItemCatalog.Stone, ItemCatalog.IronOre, ItemCatalog.CopperOre, ItemCatalog.Coal })
                Assert.IsTrue(Reachable(item), $"seed {seed}: no reachable {item}");
            Assert.Greater(seen.Count, 2000, $"seed {seed}: walkable area around spawn too small");
        }

        [Test]
        public void Motor_MovesAndNormalizesDiagonal()
        {
            var map = new WorldMap(20, 20, 0);
            var m = new PlayerMotor(new Vec2(10.5f, 10.5f), speed: 4f);
            m.Move(new Vec2(1, 1), 0.5f, map);
            float moved = Vec2.Distance(new Vec2(10.5f, 10.5f), m.Position);
            Assert.AreEqual(2f, moved, 1e-3f);
            Assert.AreEqual(Direction.North, m.Facing); // tie goes to vertical
        }

        [Test]
        public void Motor_StopsFlushAgainstWallAndSlides()
        {
            var map = new WorldMap(20, 20, 0);
            map.SetTerrain(new GridPos(12, 10), TerrainType.Rock);
            map.SetTerrain(new GridPos(12, 11), TerrainType.Rock);
            var m = new PlayerMotor(new Vec2(10.5f, 10.5f), speed: 5f, halfSize: 0.3f);
            for (int i = 0; i < 20; i++) m.Move(new Vec2(1, 0.2f), 0.05f, map);
            Assert.LessOrEqual(m.Position.X + m.HalfSize, 12f);
            Assert.Greater(m.Position.X + m.HalfSize, 11.98f);
            Assert.Greater(m.Position.Y, 10.5f + 0.5f, "should slide along the wall in Y");
            Assert.IsTrue(m.Fits(m.Position, map));
        }

        [Test]
        public void Motor_NeverTunnelsThroughThinWallAtHighSpeed()
        {
            var map = new WorldMap(40, 5, 0);
            map.SetTerrain(new GridPos(20, 2), TerrainType.Rock);
            var m = new PlayerMotor(new Vec2(18.5f, 2.5f), speed: 100f);
            m.Move(new Vec2(1, 0), 0.2f, map);
            Assert.Less(m.Position.X, 20f);
        }

        [Test]
        public void Motor_BlockedByStructuresButNotFloors()
        {
            var items = ItemCatalog.CreateDefault();
            var map = new WorldMap(20, 20, 0);
            var grid = new BuildGrid(map, items);
            grid.TryPlace(StructureCatalog.Find(StructureCatalog.Floor), new GridPos(11, 10), Direction.North, null, out _);
            grid.TryPlace(StructureCatalog.Find(StructureCatalog.Wall), new GridPos(13, 10), Direction.North, null, out _);
            var walk = grid.WithTerrain(map);
            var m = new PlayerMotor(new Vec2(10.5f, 10.5f));
            for (int i = 0; i < 40; i++) m.Move(new Vec2(1, 0), 0.05f, walk);
            Assert.Greater(m.Position.X, 12f);
            Assert.LessOrEqual(m.Position.X + m.HalfSize, 13f);
        }

        [Test]
        public void Interaction_HarvestsNearestNodeUntilDepleted()
        {
            var items = ItemCatalog.CreateDefault();
            var map = new WorldMap(20, 20, 0);
            map.AddNode(new ResourceNode(new GridPos(11, 10), ItemCatalog.IronOre, 3, yieldPerHarvest: 2));
            map.AddNode(new ResourceNode(new GridPos(15, 10), ItemCatalog.Coal, 3));
            var grid = new BuildGrid(map, items);
            var sys = new InteractionSystem(map, grid);
            var inv = new ItemInventory(4, items);
            var pos = new Vec2(10.5f, 10.5f);

            var t = sys.FindTarget(pos);
            Assert.AreEqual(InteractionKind.Harvest, t.Kind);
            Assert.AreEqual(new GridPos(11, 10), t.Cell);
            Assert.AreEqual(HarvestResult.Ok, sys.Harvest(t, inv, out int g1));
            Assert.AreEqual(2, g1);
            Assert.AreEqual(HarvestResult.Ok, sys.Harvest(sys.FindTarget(pos), inv, out int g2));
            Assert.AreEqual(1, g2);
            Assert.IsFalse(map.TryGetNode(new GridPos(11, 10), out _));
            Assert.AreEqual(3, inv.CountOf(ItemCatalog.IronOre));
            Assert.AreEqual(InteractionKind.None, sys.FindTarget(pos).Kind, "coal is out of range");
        }

        [Test]
        public void Interaction_FullInventoryLosesNothing()
        {
            var items = ItemCatalog.CreateDefault();
            var map = new WorldMap(20, 20, 0);
            map.AddNode(new ResourceNode(new GridPos(11, 10), ItemCatalog.IronOre, 10));
            var sys = new InteractionSystem(map, new BuildGrid(map, items));
            var inv = new ItemInventory(1, items);
            inv.Add(ItemCatalog.Coal, 1);
            Assert.AreEqual(HarvestResult.InventoryFull, sys.Harvest(sys.FindTarget(new Vec2(10.5f, 10.5f)), inv, out _));
            Assert.IsTrue(map.TryGetNode(new GridPos(11, 10), out var node));
            Assert.AreEqual(10, node.Remaining);
        }

        [Test]
        public void Interaction_PrefersCellUnderCursorAndFindsStorage()
        {
            var items = ItemCatalog.CreateDefault();
            var map = new WorldMap(20, 20, 0);
            map.AddNode(new ResourceNode(new GridPos(11, 10), ItemCatalog.IronOre, 10));
            var grid = new BuildGrid(map, items);
            grid.TryPlace(StructureCatalog.Find(StructureCatalog.Crate), new GridPos(10, 11), Direction.North, null, out var crate);
            var sys = new InteractionSystem(map, grid);
            var t = sys.FindTarget(new Vec2(10.5f, 10.5f), new GridPos(10, 11));
            Assert.AreEqual(InteractionKind.OpenStorage, t.Kind);
            Assert.AreSame(crate, t.Structure);
        }
    }
}
