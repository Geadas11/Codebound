using System;

namespace Codebound.Items
{
    /// <summary>An amount of one item type. Also used to express costs and yields.</summary>
    [Serializable]
    public readonly struct ItemStack : IEquatable<ItemStack>
    {
        public readonly string ItemId;
        public readonly int Count;

        public ItemStack(string itemId, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            ItemId = itemId;
            Count = count;
        }

        public bool IsEmpty => ItemId == null || Count == 0;

        public bool Equals(ItemStack other) => ItemId == other.ItemId && Count == other.Count;
        public override bool Equals(object obj) => obj is ItemStack o && Equals(o);
        public override int GetHashCode() => (ItemId?.GetHashCode() ?? 0) * 31 + Count;
        public override string ToString() => IsEmpty ? "(empty)" : $"{Count}x {ItemId}";
    }
}
