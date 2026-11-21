using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class InventoryTests
    {
        [Test]
        public void Add_StacksThenOverflows()
        {
            var inv = new Inventory(2); // wood stacks to 100
            Assert.AreEqual(0, inv.Add(ItemDatabase.Wood, 150));
            Assert.AreEqual(150, inv.Count(ItemDatabase.Wood));
            Assert.AreEqual(50, inv.Add(ItemDatabase.Wood, 100), "only 50 more fit in 2x100");
            Assert.AreEqual(200, inv.Count(ItemDatabase.Wood));
        }

        [Test]
        public void TryRemoveAll_IsAtomic()
        {
            var inv = new Inventory(10);
            inv.Add(ItemDatabase.Stone, 6);
            inv.Add(ItemDatabase.IronOre, 3);
            var cost = BuildingDatabase.Get("drill").Cost; // 6 stone + 8 iron
            Assert.IsFalse(inv.TryRemoveAll(cost));
            Assert.AreEqual(6, inv.Count(ItemDatabase.Stone), "nothing removed on failure");
            inv.Add(ItemDatabase.IronOre, 5);
            Assert.IsTrue(inv.TryRemoveAll(cost));
            Assert.AreEqual(0, inv.Count(ItemDatabase.Stone));
            Assert.AreEqual(0, inv.Count(ItemDatabase.IronOre));
        }
    }

    public class InventoryRefundTests
    {
        [Test]
        public void TryAddAll_RollsBackWhenFull()
        {
            var inv = new Inventory(1);
            inv.Add(ItemDatabase.Stone, 95);
            var refund = new[] { new ItemStack(ItemDatabase.Stone, 3), new ItemStack(ItemDatabase.Wood, 1) };
            Assert.IsFalse(inv.TryAddAll(refund), "wood has no slot");
            Assert.AreEqual(95, inv.Count(ItemDatabase.Stone), "stone add rolled back");
            Assert.IsTrue(inv.TryAddAll(new[] { new ItemStack(ItemDatabase.Stone, 5) }));
            Assert.AreEqual(100, inv.Count(ItemDatabase.Stone));
        }
    }

    public class BuildGridTests
    {
        [Test]
        public void RotatedFootprint_SwapsSize_AndBlocksOverlap()
        {
            var grid = new BuildGrid((d, c) => true);
            var computer = BuildingDatabase.Get("computer"); // 1x2
            var b = grid.Place(computer, new Cell(0, 0), Rotation.R90); // becomes 2x1
            Assert.NotNull(b);
            CollectionAssert.AreEquivalent(new[] { new Cell(0, 0), new Cell(1, 0) }, b.Cells().ToArray());
            Assert.IsFalse(grid.CanPlace(BuildingDatabase.Get("wall"), new Cell(1, 0), Rotation.R0));
            Assert.IsTrue(grid.CanPlace(BuildingDatabase.Get("wall"), new Cell(0, 1), Rotation.R0));
            Assert.IsTrue(grid.Remove(b));
            Assert.IsFalse(grid.IsOccupied(new Cell(1, 0)));
        }

        [Test]
        public void TerrainPredicate_BlocksPlacement()
        {
            var grid = new BuildGrid((d, c) => c.X < 1); // x>=1 is "water"
            Assert.IsFalse(grid.CanPlace(BuildingDatabase.Get("furnace"), new Cell(0, 0), Rotation.R0));
            Assert.IsTrue(grid.CanPlace(BuildingDatabase.Get("furnace"), new Cell(-1, 0), Rotation.R0));
        }
    }

    public class WorldTests
    {
        [Test]
        public void SameSeed_SameWorld_DifferentSeed_DifferentWorld()
        {
            var a = new WorldGenerator(1234); var b = new WorldGenerator(1234); var c = new WorldGenerator(999);
            int diff = 0;
            for (int x = -200; x < 200; x += 7)
                for (int y = -200; y < 200; y += 7)
                {
                    var cell = new Cell(x, y);
                    Assert.AreEqual(a.TileAt(cell), b.TileAt(cell));
                    Assert.AreEqual(a.DepositAt(cell).Type, b.DepositAt(cell).Type);
                    if (a.TileAt(cell) != c.TileAt(cell) || a.BiomeAt(cell) != c.BiomeAt(cell)) diff++;
                }
            Assert.Greater(diff, 0);
        }

        [Test]
        public void AllBiomesAndCoreResourcesExist_InLargeArea()
        {
            var g = new WorldGenerator(42);
            var biomes = new System.Collections.Generic.HashSet<Biome>();
            var res = new System.Collections.Generic.HashSet<ResourceType>();
            for (int x = -3000; x < 3000; x += 13)
                for (int y = -3000; y < 3000; y += 13)
                {
                    var cell = new Cell(x, y);
                    biomes.Add(g.BiomeAt(cell));
                    res.Add(g.DepositAt(cell).Type);
                }
            foreach (Biome bio in System.Enum.GetValues(typeof(Biome))) if (bio != Biome.Cavern) Assert.Contains(bio, biomes.ToList(), "missing biome");
            foreach (var r in new[] { ResourceType.Iron, ResourceType.Copper, ResourceType.Coal, ResourceType.Stone, ResourceType.Tree })
                Assert.Contains(r, res.ToList(), "missing resource " + r);
        }

        [Test]
        public void Spawn_IsWalkable_AndStartingResourcesNearby()
        {
            var g = new WorldGenerator(7);
            var s = g.FindSpawn();
            Assert.IsTrue(WorldRules.IsWalkable(g.TileAt(s)));
            var near = new System.Collections.Generic.HashSet<ResourceType>();
            for (int x = -60; x <= 60; x++)
                for (int y = -60; y <= 60; y++) near.Add(g.DepositAt(new Cell(s.X + x, s.Y + y)).Type);
            Assert.Contains(ResourceType.Tree, near.ToList());
            Assert.Contains(ResourceType.Stone, near.ToList());
        }

        [Test]
        public void WorldState_Extract_DepletesDeposit()
        {
            var state = new WorldState(new WorldGenerator(5));
            Cell found = default; bool ok = false;
            for (int x = 0; x < 300 && !ok; x++)
                for (int y = 0; y < 300 && !ok; y++)
                    if (!state.DepositAt(new Cell(x, y)).IsNone) { found = new Cell(x, y); ok = true; }
            Assert.IsTrue(ok);
            int total = state.DepositAt(found).Amount;
            Assert.AreEqual(total, state.Extract(found, 100000));
            Assert.IsTrue(state.DepositAt(found).IsNone);
            Assert.AreEqual(0, state.Extract(found, 1));
        }

        [Test]
        public void FloorDiv_HandlesNegatives()
        {
            Assert.AreEqual(-1, Cell.FloorDiv(-1, 32));
            Assert.AreEqual(-1, Cell.FloorDiv(-32, 32));
            Assert.AreEqual(-2, Cell.FloorDiv(-33, 32));
            Assert.AreEqual(0, Cell.FloorDiv(31, 32));
        }
    }
}

namespace Codebound.Tests
{
    public class JsonTests
    {
        [NUnit.Framework.Test]
        public void RoundTrip_ProtocolMessages()
        {
            const string line = "{\"t\":\"call\",\"id\":12,\"obj\":\"m7\",\"fn\":\"mine\",\"args\":[1.5,\"a\\\"b\\nc\",true,null,{\"k\":[]}]}";
            var m = (System.Collections.Generic.Dictionary<string, object>)Codebound.Core.Json.Parse(line);
            NUnit.Framework.Assert.AreEqual("call", m["t"]);
            NUnit.Framework.Assert.AreEqual(12.0, m["id"]);
            var args = (System.Collections.Generic.List<object>)m["args"];
            NUnit.Framework.Assert.AreEqual(1.5, args[0]);
            NUnit.Framework.Assert.AreEqual("a\"b\nc", args[1]);
            NUnit.Framework.Assert.AreEqual(true, args[2]);
            NUnit.Framework.Assert.IsNull(args[3]);
            NUnit.Framework.Assert.AreEqual(line, Codebound.Core.Json.Write(m));
            NUnit.Framework.Assert.AreEqual("\"ol\u00e1 \\u0001\"", Codebound.Core.Json.Write("ol\u00e1 \u0001"));
            NUnit.Framework.Assert.Throws<System.FormatException>(() => Codebound.Core.Json.Parse("{\"a\":1} x"));
        }
    }
}
