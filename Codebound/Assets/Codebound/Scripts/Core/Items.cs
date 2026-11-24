using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    public sealed class ItemDef
    {
        public readonly string Id;
        public readonly string NameKey; // localization key (Etapa 8); English fallback below
        public readonly string DisplayName;
        public readonly int MaxStack;

        public ItemDef(string id, string displayName, int maxStack)
        {
            Id = id; DisplayName = displayName; MaxStack = maxStack; NameKey = "item." + id;
        }
    }

    /// <summary>Registry of every item. Ids are stable (used by saves and the scripting API).</summary>
    public static class ItemDatabase
    {
        static readonly Dictionary<string, ItemDef> items = new Dictionary<string, ItemDef>();

        public const string Wood = "wood", Stone = "stone", IronOre = "iron_ore", CopperOre = "copper_ore",
            Coal = "coal", Silicon = "silicon", TitaniumOre = "titanium_ore", UraniumOre = "uranium_ore",
            Oil = "crude_oil", AlienCrystal = "alien_crystal", IronPlate = "iron_plate", CopperPlate = "copper_plate", StoneBrick = "stone_brick", CopperWire = "copper_wire", AlienFruit = "alien_fruit", IronGear = "iron_gear", Circuit = "circuit",
            Glowcap = "glowcap", Fiber = "fiber", Ration = "ration", LaserTool = "laser_tool", ArmorPlating = "armor_plating", ThermalSuit = "thermal_suit",
            Robot = "logistic_robot", Locomotive = "locomotive", CargoWagon = "cargo_wagon";

        static ItemDatabase()
        {
            Add(Wood, "Wood", 100);
            Add(Stone, "Stone", 100);
            Add(IronOre, "Iron Ore", 100);
            Add(CopperOre, "Copper Ore", 100);
            Add(Coal, "Coal", 100);
            Add(Silicon, "Silicon", 100);
            Add(TitaniumOre, "Titanium Ore", 50);
            Add(UraniumOre, "Uranium Ore", 50);
            Add(Oil, "Crude Oil (canister)", 20);
            Add(AlienCrystal, "Alien Crystal", 50);
            Add(IronPlate, "Iron Plate", 100);
            Add(CopperPlate, "Copper Plate", 100);
            Add(StoneBrick, "Stone Brick", 100);
            Add(CopperWire, "Copper Wire", 200);
            Add(AlienFruit, "Alien Fruit", 50);
            Add(IronGear, "Iron Gear", 100);
            Add(Circuit, "Circuit", 100);
            Add(Glowcap, "Glowcap", 50);
            Add(Fiber, "Fiberweed", 100);
            Add(Ration, "Ration", 50);
            Add(LaserTool, "Laser Multitool", 1);
            Add(ArmorPlating, "Armor Plating", 1);
            Add(ThermalSuit, "Thermal Suit Liner", 1);
            Add(Robot, "Logistic Robot", 50);
            Add(Locomotive, "Locomotive", 5);
            Add(CargoWagon, "Cargo Wagon", 10);
        }

        static void Add(string id, string name, int stack) => items[id] = new ItemDef(id, name, stack);

        public static ItemDef Get(string id) =>
            items.TryGetValue(id, out var d) ? d : throw new ArgumentException("Unknown item id: " + id);

        public static bool Exists(string id) => items.ContainsKey(id);
        public static IEnumerable<ItemDef> All => items.Values;
    }

    public struct ItemStack
    {
        public string ItemId;
        public int Count;
        public bool IsEmpty => ItemId == null || Count <= 0;
        public ItemStack(string id, int count) { ItemId = id; Count = count; }
    }
}
