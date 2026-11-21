using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>Slot-based inventory with per-item stack limits.</summary>
    public sealed class Inventory
    {
        readonly ItemStack[] slots;
        public event Action Changed;

        public Inventory(int slotCount)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount));
            slots = new ItemStack[slotCount];
        }

        public int SlotCount => slots.Length;

        public void Clear() { for (int i = 0; i < slots.Length; i++) slots[i] = default; Changed?.Invoke(); }
        public ItemStack this[int i] => slots[i];

        public int Count(string itemId)
        {
            int n = 0;
            foreach (var s in slots) if (!s.IsEmpty && s.ItemId == itemId) n += s.Count;
            return n;
        }

        /// <summary>Adds as much as fits. Returns the amount that did NOT fit.</summary>
        public int Add(string itemId, int amount)
        {
            if (amount <= 0) return 0;
            int max = ItemDatabase.Get(itemId).MaxStack;
            int left = amount;
            // top up existing stacks first, then empty slots
            for (int pass = 0; pass < 2 && left > 0; pass++)
            {
                for (int i = 0; i < slots.Length && left > 0; i++)
                {
                    bool match = pass == 0 ? (!slots[i].IsEmpty && slots[i].ItemId == itemId) : slots[i].IsEmpty;
                    if (!match) continue;
                    int cur = slots[i].IsEmpty ? 0 : slots[i].Count;
                    int put = Math.Min(max - cur, left);
                    if (put <= 0) continue;
                    slots[i] = new ItemStack(itemId, cur + put);
                    left -= put;
                }
            }
            if (left != amount) Changed?.Invoke();
            return left;
        }

        /// <summary>Removes exactly <paramref name="amount"/> or nothing. Returns success.</summary>
        public bool TryRemove(string itemId, int amount)
        {
            if (amount <= 0) return true;
            if (Count(itemId) < amount) return false;
            int left = amount;
            for (int i = slots.Length - 1; i >= 0 && left > 0; i--)
            {
                if (slots[i].IsEmpty || slots[i].ItemId != itemId) continue;
                int take = Math.Min(slots[i].Count, left);
                slots[i].Count -= take;
                if (slots[i].Count == 0) slots[i] = default;
                left -= take;
            }
            Changed?.Invoke();
            return true;
        }

        public bool Has(IReadOnlyList<ItemStack> cost)
        {
            foreach (var c in cost) if (Count(c.ItemId) < c.Count) return false;
            return true;
        }

        /// <summary>Atomic: adds all stacks or none (used for refunds so nothing is silently lost).</summary>
        public bool TryAddAll(IReadOnlyList<ItemStack> stacks)
        {
            var backup = (ItemStack[])slots.Clone();
            foreach (var s in stacks)
            {
                if (Add(s.ItemId, s.Count) > 0)
                {
                    Array.Copy(backup, slots, slots.Length);
                    Changed?.Invoke();
                    return false;
                }
            }
            return true;
        }

        /// <summary>Atomic: removes all costs or none.</summary>
        public bool TryRemoveAll(IReadOnlyList<ItemStack> cost)
        {
            if (!Has(cost)) return false;
            foreach (var c in cost) TryRemove(c.ItemId, c.Count);
            return true;
        }
    }
}
