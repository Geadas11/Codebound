using System.Linq;
using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
using Codebound.World;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class BuildingTests
    {
        private ItemRegistry _items;
        private WorldMap _map;
        private BuildGrid _grid;
        private ItemInventory _player;

        [SetUp]
        public void SetUp()
        {
            _items = ItemCatalog.CreateDefault();
            _map = new WorldMap(16, 16, 1); // all grass
            _grid = new BuildGrid(_map, _items);
            _player = new ItemInventory(10, _items);
        }

        private static StructureDefinition Def(string id) => StructureCatalog.Find(id);

        [Test]
        public void Footprint_RotationSwapsAxes()
        {
            var north = Footprint.Cells(new GridPos(2, 2), 3, 2, Direction.North);
            var east = Footprint.Cells(new GridPos(2, 2), 3, 2, Direction.East);
            Assert.AreEqual(6, north.Count);
            Assert.IsTrue(north.Contains(new GridPos(4, 3)));
            Assert.IsFalse(north.Contains(new GridPos(3, 4)));
            Assert.IsTrue(east.Contains(new GridPos(3, 4)));
            Assert.IsFalse(east.Contains(new GridPos(4, 3)));
        }

        [Test]
        public void Footprint_OriginCenteredOnCursor()
        {
            Assert.AreEqual(new GridPos(4, 5), Footprint.OriginCenteredOn(new GridPos(5, 5), 3, 2, Direction.North));
            Assert.AreEqual(new GridPos(5, 4), Footprint.OriginCenteredOn(new GridPos(5, 5), 3, 2, Direction.East));
        }

        [Test]
        public void Place_ChargesCostAndOccupiesAllCells()
        {
            _player.Add(ItemCatalog.Scrap, 8);
            _player.Add(ItemCatalog.Wood, 4);
            var status = _grid.TryPlace(Def(StructureCatalog.ShelterModule), new GridPos(1, 1), Direction.East, _player, out var s);
            Assert.AreEqual(PlacementStatus.Ok, status);
            Assert.AreEqual(0, _player.CountOf(ItemCatalog.Scrap));
            Assert.AreEqual(6, s.Cells.Count);
            Assert.AreSame(s, _grid.GetAt(new GridPos(2, 3)));
            Assert.IsNull(_grid.GetAt(new GridPos(3, 1)));
        }

        [Test]
        public void Validate_ReportsInsufficientResourcesWithoutCharging()
        {
            _player.Add(ItemCatalog.Scrap, 1);
            var check = _grid.Validate(Def(StructureCatalog.Wall), new GridPos(0, 0), Direction.North, _player);
            Assert.AreEqual(PlacementStatus.InsufficientResources, check.Status);
            Assert.AreEqual(PlacementStatus.InsufficientResources,
                _grid.TryPlace(Def(StructureCatalog.Wall), new GridPos(0, 0), Direction.North, _player, out _));
            Assert.AreEqual(1, _player.CountOf(ItemCatalog.Scrap));
        }

        [Test]
        public void Validate_DetectsOverlapAndTerrainObstacles()
        {
            _map.SetTerrain(new GridPos(5, 5), TerrainType.Water);
            _map.AddNode(new ResourceNode(new GridPos(8, 8), ItemCatalog.IronOre, 10));
            var wall = Def(StructureCatalog.Wall);
            Assert.AreEqual(PlacementStatus.Ok, _grid.TryPlace(wall, new GridPos(1, 1), Direction.North, null, out _));

            var overlap = _grid.Validate(Def(StructureCatalog.ShelterModule), new GridPos(0, 0), Direction.North, null);
            Assert.AreEqual(PlacementStatus.Occupied, overlap.Status);
            CollectionAssert.AreEqual(new[] { new GridPos(1, 1) }, overlap.BlockedCells.ToArray());

            Assert.AreEqual(PlacementStatus.TerrainBlocked, _grid.Validate(wall, new GridPos(5, 5), Direction.North, null).Status);
            Assert.AreEqual(PlacementStatus.TerrainBlocked, _grid.Validate(wall, new GridPos(8, 8), Direction.North, null).Status);
            Assert.AreEqual(PlacementStatus.TerrainBlocked, _grid.Validate(wall, new GridPos(-1, 0), Direction.North, null).Status);
            Assert.AreEqual(PlacementStatus.TerrainBlocked,
                _grid.Validate(Def(StructureCatalog.ShelterModule), new GridPos(14, 14), Direction.North, null).Status);
        }

        [Test]
        public void NonRotatable_IgnoresFacing()
        {
            _grid.TryPlace(Def(StructureCatalog.Crate), new GridPos(3, 3), Direction.West, null, out var s);
            Assert.AreEqual(Direction.North, s.Facing);
            Assert.IsNotNull(s.Inventory);
            Assert.AreEqual(16, s.Inventory.SlotCount);
        }

        [Test]
        public void Remove_RefundsCostAndContents_OrRefusesIfNoSpace()
        {
            var crateDef = Def(StructureCatalog.Crate);
            _grid.TryPlace(crateDef, new GridPos(3, 3), Direction.North, null, out var crate);
            crate.Inventory.Add(ItemCatalog.IronOre, 50);

            var tiny = new ItemInventory(1, _items);
            Assert.AreEqual(RemovalStatus.NoSpaceForRefund, _grid.TryRemove(new GridPos(3, 3), tiny, out _));
            Assert.IsNotNull(_grid.GetAt(new GridPos(3, 3)));

            Assert.AreEqual(RemovalStatus.Ok, _grid.TryRemove(new GridPos(3, 3), _player, out _));
            Assert.AreEqual(50, _player.CountOf(ItemCatalog.IronOre));
            Assert.AreEqual(2, _player.CountOf(ItemCatalog.Wood));
            Assert.AreEqual(1, _player.CountOf(ItemCatalog.Scrap));
            Assert.IsNull(_grid.GetAt(new GridPos(3, 3)));
            Assert.AreEqual(RemovalStatus.NothingThere, _grid.TryRemove(new GridPos(3, 3), _player, out _));
        }

        [Test]
        public void Walkability_FloorIsWalkableWallIsNot()
        {
            _grid.TryPlace(Def(StructureCatalog.Wall), new GridPos(2, 2), Direction.North, null, out _);
            _grid.TryPlace(Def(StructureCatalog.Floor), new GridPos(3, 2), Direction.North, null, out _);
            var walk = _grid.WithTerrain(_map);
            Assert.IsFalse(walk.IsWalkable(new GridPos(2, 2)));
            Assert.IsTrue(walk.IsWalkable(new GridPos(3, 2)));
            Assert.IsTrue(walk.IsWalkable(new GridPos(4, 2)));
        }

        [Test]
        public void Events_FireOnPlaceAndRemove()
        {
            int placed = 0, removed = 0;
            _grid.Placed += _ => placed++;
            _grid.Removed += _ => removed++;
            _grid.TryPlace(Def(StructureCatalog.Wall), new GridPos(2, 2), Direction.North, null, out _);
            _grid.TryRemove(new GridPos(2, 2), null, out _);
            Assert.AreEqual(1, placed);
            Assert.AreEqual(1, removed);
        }
    }
}
