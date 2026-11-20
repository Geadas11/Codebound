using Codebound.Inventory;
using Codebound.Items;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class InventoryTests
    {
        private ItemRegistry _items;

        [SetUp]
        public void SetUp() => _items = ItemCatalog.CreateDefault();

        [Test]
        public void Add_StacksThenUsesNewSlots_ReturnsRemainder()
        {
            var inv = new ItemInventory(2, _items); // 2 x 50
            Assert.AreEqual(0, inv.Add(ItemCatalog.IronOre, 30));
            Assert.AreEqual(0, inv.Add(ItemCatalog.IronOre, 30));
            Assert.AreEqual(60, inv.CountOf(ItemCatalog.IronOre));
            Assert.AreEqual(50, inv.GetSlot(0).Count);
            Assert.AreEqual(15, inv.Add(ItemCatalog.IronOre, 55));
            Assert.AreEqual(100, inv.CountOf(ItemCatalog.IronOre));
            Assert.AreEqual(0, inv.SpaceFor(ItemCatalog.Coal));
        }

        [Test]
        public void TryRemove_IsAllOrNothing()
        {
            var inv = new ItemInventory(4, _items);
            inv.Add(ItemCatalog.Coal, 5);
            Assert.IsFalse(inv.TryRemove(ItemCatalog.Coal, 6));
            Assert.AreEqual(5, inv.CountOf(ItemCatalog.Coal));
            Assert.IsTrue(inv.TryRemove(ItemCatalog.Coal, 5));
            Assert.AreEqual(0, inv.CountOf(ItemCatalog.Coal));
            Assert.IsTrue(inv.GetSlot(0).IsEmpty);
        }

        [Test]
        public void TryConsume_AggregatesDuplicateCostsAndIsAtomic()
        {
            var inv = new ItemInventory(4, _items);
            inv.Add(ItemCatalog.Scrap, 3);
            inv.Add(ItemCatalog.Stone, 1);
            var costs = new[] { new ItemStack(ItemCatalog.Scrap, 2), new ItemStack(ItemCatalog.Scrap, 2) };
            Assert.IsFalse(inv.TryConsume(costs));
            Assert.AreEqual(3, inv.CountOf(ItemCatalog.Scrap));
            Assert.IsTrue(inv.TryConsume(new[] { new ItemStack(ItemCatalog.Scrap, 3), new ItemStack(ItemCatalog.Stone, 1) }));
            Assert.AreEqual(0, inv.CountOf(ItemCatalog.Scrap));
        }

        [Test]
        public void TryAddAll_RollsBackWhenNotEverythingFits()
        {
            var inv = new ItemInventory(1, _items);
            inv.Add(ItemCatalog.Wood, 10);
            int changes = 0;
            inv.Changed += () => changes++;
            Assert.IsFalse(inv.TryAddAll(new[] { new ItemStack(ItemCatalog.Wood, 10), new ItemStack(ItemCatalog.Stone, 1) }));
            Assert.AreEqual(10, inv.CountOf(ItemCatalog.Wood));
            Assert.AreEqual(0, inv.CountOf(ItemCatalog.Stone));
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void TransferSlot_MovesWhatFits()
        {
            var a = new ItemInventory(1, _items);
            var b = new ItemInventory(1, _items);
            a.Add(ItemCatalog.Wood, 40);
            b.Add(ItemCatalog.Wood, 30);
            Assert.AreEqual(20, a.TransferSlotTo(0, b));
            Assert.AreEqual(20, a.CountOf(ItemCatalog.Wood));
            Assert.AreEqual(50, b.CountOf(ItemCatalog.Wood));
        }

        [Test]
        public void UnknownItem_Throws()
        {
            var inv = new ItemInventory(1, _items);
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => inv.Add("item.nope", 1));
        }
    }
}
