using System;
using System.Collections.Generic;
using Codebound.Items;

namespace Codebound.Inventory
{
    /// <summary>
    /// Slot-based inventory with stacking. All mutations are all-or-nothing where the method
    /// name says "Try"; <see cref="Add"/> inserts as much as fits and returns the remainder.
    /// </summary>
    public sealed class ItemInventory
    {
        private readonly ItemStack[] _slots;
        private readonly ItemRegistry _registry;

        public ItemInventory(int slotCount, ItemRegistry registry)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount));
            _slots = new ItemStack[slotCount];
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public event Action Changed;

        public int SlotCount => _slots.Length;
        public ItemStack GetSlot(int index) => _slots[index];

        /// <summary>Adds up to <paramref name="count"/> items. Returns how many did NOT fit.</summary>
        public int Add(string itemId, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return 0;
            int max = _registry.Get(itemId).MaxStack;
            int remaining = count;

            // Fill existing stacks first, then empty slots.
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (_slots[i].ItemId != itemId || _slots[i].IsEmpty) continue;
                int put = Math.Min(max - _slots[i].Count, remaining);
                if (put <= 0) continue;
                _slots[i] = new ItemStack(itemId, _slots[i].Count + put);
                remaining -= put;
            }
            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (!_slots[i].IsEmpty) continue;
                int put = Math.Min(max, remaining);
                _slots[i] = new ItemStack(itemId, put);
                remaining -= put;
            }
            if (remaining != count) Changed?.Invoke();
            return remaining;
        }

        /// <summary>How many more of this item could be added right now.</summary>
        public int SpaceFor(string itemId)
        {
            int max = _registry.Get(itemId).MaxStack;
            int space = 0;
            foreach (var s in _slots)
            {
                if (s.IsEmpty) space += max;
                else if (s.ItemId == itemId) space += max - s.Count;
            }
            return space;
        }

        public int CountOf(string itemId)
        {
            int total = 0;
            foreach (var s in _slots) if (!s.IsEmpty && s.ItemId == itemId) total += s.Count;
            return total;
        }

        public bool Has(string itemId, int count) => CountOf(itemId) >= count;

        public bool HasAll(IReadOnlyList<ItemStack> costs)
        {
            if (costs == null) return true;
            // Aggregate duplicates so ("a",2),("a",2) needs 4.
            var need = Aggregate(costs);
            foreach (var kv in need) if (CountOf(kv.Key) < kv.Value) return false;
            return true;
        }

        /// <summary>Removes exactly <paramref name="count"/> items or nothing.</summary>
        public bool TryRemove(string itemId, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return true;
            if (CountOf(itemId) < count) return false;
            RemoveUnchecked(itemId, count);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Removes all listed costs atomically, or nothing if any is missing.</summary>
        public bool TryConsume(IReadOnlyList<ItemStack> costs)
        {
            if (!HasAll(costs)) return false;
            if (costs == null || costs.Count == 0) return true;
            foreach (var kv in Aggregate(costs)) RemoveUnchecked(kv.Key, kv.Value);
            Changed?.Invoke();
            return true;
        }

        /// <summary>Adds all stacks atomically, or nothing if they do not all fit.</summary>
        public bool TryAddAll(IReadOnlyList<ItemStack> stacks)
        {
            if (stacks == null || stacks.Count == 0) return true;
            var snapshot = (ItemStack[])_slots.Clone();
            foreach (var s in stacks)
            {
                if (s.IsEmpty) continue;
                if (AddSilently(s.ItemId, s.Count) > 0)
                {
                    Array.Copy(snapshot, _slots, _slots.Length);
                    return false;
                }
            }
            Changed?.Invoke();
            return true;
        }

        /// <summary>Moves the stack in <paramref name="slot"/> into <paramref name="target"/> as far as it fits.</summary>
        public int TransferSlotTo(int slot, ItemInventory target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            var stack = _slots[slot];
            if (stack.IsEmpty || ReferenceEquals(target, this)) return 0;
            int left = target.Add(stack.ItemId, stack.Count);
            int moved = stack.Count - left;
            if (moved > 0)
            {
                _slots[slot] = left > 0 ? new ItemStack(stack.ItemId, left) : default;
                Changed?.Invoke();
            }
            return moved;
        }

        public List<ItemStack> ToList()
        {
            var list = new List<ItemStack>();
            foreach (var s in _slots) if (!s.IsEmpty) list.Add(s);
            return list;
        }

        public void Clear()
        {
            Array.Clear(_slots, 0, _slots.Length);
            Changed?.Invoke();
        }

        private int AddSilently(string itemId, int count)
        {
            var handler = Changed;
            Changed = null;
            try { return Add(itemId, count); }
            finally { Changed = handler; }
        }

        private void RemoveUnchecked(string itemId, int count)
        {
            // Take from the last stacks first so partially filled stacks at the front stay tidy.
            for (int i = _slots.Length - 1; i >= 0 && count > 0; i--)
            {
                if (_slots[i].IsEmpty || _slots[i].ItemId != itemId) continue;
                int take = Math.Min(_slots[i].Count, count);
                int left = _slots[i].Count - take;
                _slots[i] = left > 0 ? new ItemStack(itemId, left) : default;
                count -= take;
            }
        }

        private static Dictionary<string, int> Aggregate(IReadOnlyList<ItemStack> stacks)
        {
            var d = new Dictionary<string, int>();
            foreach (var s in stacks)
            {
                if (s.IsEmpty) continue;
                d.TryGetValue(s.ItemId, out int n);
                d[s.ItemId] = n + s.Count;
            }
            return d;
        }
    }
}
