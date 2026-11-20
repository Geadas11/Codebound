namespace Codebound.Items
{
    /// <summary>Ids and default definitions of the base-game items available so far.</summary>
    public static class ItemCatalog
    {
        // Crash-site salvage (first building material).
        public const string Scrap = "item.scrap";
        public const string Stone = "item.stone";
        public const string Wood = "item.wood";
        // Raw resources from the design document.
        public const string IronOre = "item.iron_ore";
        public const string CopperOre = "item.copper_ore";
        public const string Coal = "item.coal";
        public const string Silicon = "item.silicon";
        public const string TitaniumOre = "item.titanium_ore";
        public const string UraniumOre = "item.uranium_ore";
        public const string Oil = "item.oil";
        public const string AlienCrystal = "item.alien_crystal";
        // Processed materials.
        public const string IronPlate = "item.iron_plate";
        public const string CopperPlate = "item.copper_plate";
        public const string StoneBrick = "item.stone_brick";

        public static ItemRegistry CreateDefault()
        {
            var r = new ItemRegistry();
            void Raw(string id) => r.Register(new ItemDefinition(id, ItemCategory.RawResource, 50, "name." + id));
            Raw(Scrap); Raw(Stone); Raw(Wood);
            Raw(IronOre); Raw(CopperOre); Raw(Coal); Raw(Silicon);
            Raw(TitaniumOre); Raw(UraniumOre); Raw(Oil); Raw(AlienCrystal);
            void Mat(string id) => r.Register(new ItemDefinition(id, ItemCategory.Material, 100, "name." + id));
            Mat(IronPlate); Mat(CopperPlate); Mat(StoneBrick);
            return r;
        }
    }
}
