using System;
using System.Collections.Generic;

namespace Codebound.Items
{
    /// <summary>Lookup of all item definitions known to the game.</summary>
    public sealed class ItemRegistry
    {
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        public void Register(ItemDefinition def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (_items.ContainsKey(def.Id)) throw new InvalidOperationException($"Item '{def.Id}' already registered");
            _items.Add(def.Id, def);
        }

        public bool Contains(string id) => id != null && _items.ContainsKey(id);

        public bool TryGet(string id, out ItemDefinition def)
        {
            def = null;
            return id != null && _items.TryGetValue(id, out def);
        }

        public ItemDefinition Get(string id)
        {
            if (TryGet(id, out var def)) return def;
            throw new KeyNotFoundException($"Unknown item '{id}'");
        }

        public IEnumerable<ItemDefinition> All => _items.Values;
        public int Count => _items.Count;
    }
}
