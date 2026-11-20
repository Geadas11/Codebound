using System;

namespace Codebound.Items
{
    public enum ItemCategory { RawResource, Material, Component, Structure, Food, Tool }

    /// <summary>Static description of an item type. Referenced everywhere by its stable <see cref="Id"/>.</summary>
    public sealed class ItemDefinition
    {
        public ItemDefinition(string id, ItemCategory category, int maxStack, string nameKey = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Item id required", nameof(id));
            if (maxStack <= 0) throw new ArgumentOutOfRangeException(nameof(maxStack));
            Id = id;
            Category = category;
            MaxStack = maxStack;
            NameKey = nameKey ?? id;
        }

        /// <summary>Stable id, e.g. "item.iron_ore". Never changes once shipped (saves depend on it).</summary>
        public string Id { get; }
        public ItemCategory Category { get; }
        public int MaxStack { get; }
        /// <summary>Localization key for the display name.</summary>
        public string NameKey { get; }

        public override string ToString() => Id;
    }
}
