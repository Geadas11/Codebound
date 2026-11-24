using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>A data server: a small key–value store that every program in range can read and write (shared state
    /// between computers). Needs power; saved with the world.</summary>
    public sealed class ServerState : MachineState
    {
        public const int MaxKeys = 256, MaxKeyLength = 64, MaxValueLength = 4096;
        public readonly SortedDictionary<string, string> Data = new SortedDictionary<string, string>(StringComparer.Ordinal);

        public ServerState() { Status = "idle"; }

        /// <summary>Null on success, otherwise why not.</summary>
        public string Set(string key, string value)
        {
            if (PowerSatisfaction <= 0) return "no power";
            if (string.IsNullOrEmpty(key) || key.Length > MaxKeyLength) return $"key must be 1-{MaxKeyLength} characters";
            if (value == null) { Data.Remove(key); return null; }
            if (value.Length > MaxValueLength) return $"value longer than {MaxValueLength} characters";
            if (!Data.ContainsKey(key) && Data.Count >= MaxKeys) return $"server full ({MaxKeys} keys)";
            Data[key] = value;
            Status = $"{Data.Count} keys";
            return null;
        }

        public override void SaveExtra(IDictionary<string, string> d)
        {
            int i = 0;
            foreach (var kv in Data) d["kv" + i++] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(kv.Key)) + ":" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(kv.Value));
        }

        public override void LoadExtra(string k, string v)
        {
            if (!k.StartsWith("kv")) return;
            var p = v.Split(':');
            if (p.Length == 2) Data[System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(p[0]))] = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(p[1]));
            Status = $"{Data.Count} keys";
        }
    }

    /// <summary>
    /// Automatic storage: chests within 6 cells of a powered hub behave as one store. Items put into the hub (belts,
    /// inserters, the player) go to a chest that already holds that item, otherwise to any chest with room; inserters
    /// taking from the hub take from the chests. The Factory links the chests every tick.
    /// </summary>
    public sealed class StorageHubState : MachineState
    {
        public const double Radius = 6;
        public List<StorageState> Chests = new List<StorageState>();
        internal int ShownChests = int.MinValue;

        public StorageHubState() { Status = "no chests"; }

        public override bool TryInsert(string itemId, int count)
        {
            if (PowerSatisfaction <= 0) return false;
            foreach (var c in Chests) if (c.Items.Count(itemId) > 0 && c.TryInsert(itemId, count)) return true;
            foreach (var c in Chests) if (c.TryInsert(itemId, count)) return true;
            return false;
        }

        /// <summary>For inserters: the first chest that has anything.</summary>
        public override Inventory Extractable
        {
            get
            {
                if (PowerSatisfaction <= 0) return null;
                foreach (var c in Chests) for (int i = 0; i < c.Items.SlotCount; i++) if (!c.Items[i].IsEmpty) return c.Items;
                return null;
            }
        }

        public SortedDictionary<string, int> Totals()
        {
            var t = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var c in Chests)
                for (int i = 0; i < c.Items.SlotCount; i++)
                    if (!c.Items[i].IsEmpty) t[c.Items[i].ItemId] = (t.TryGetValue(c.Items[i].ItemId, out var n) ? n : 0) + c.Items[i].Count;
            return t;
        }
    }
}
