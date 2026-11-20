using System;
using System.Collections.Generic;
using Codebound.Items;

namespace Codebound.Building
{
    /// <summary>Structures available in Etapa 1 (shelter building blocks, storage).</summary>
    public static class StructureCatalog
    {
        public const string Wall = "structure.wall";
        public const string Floor = "structure.floor";
        public const string Crate = "structure.crate";
        public const string ShelterModule = "structure.shelter_module";
        public const string PowerPole = "structure.power_pole";
        public const string Computer = "structure.computer";
        public const string Drill = "structure.drill";
        public const string CoalGenerator = "structure.coal_generator";
        public const string EmergencyBattery = "structure.emergency_battery";

        public static readonly IReadOnlyList<StructureDefinition> Default = new[]
        {
            new StructureDefinition(Wall, 1, 1, new[] { new ItemStack(ItemCatalog.Scrap, 1), new ItemStack(ItemCatalog.Stone, 1) }, rotatable: false),
            new StructureDefinition(Floor, 1, 1, new[] { new ItemStack(ItemCatalog.Stone, 1) }, rotatable: false, blocksMovement: false),
            new StructureDefinition(Crate, 1, 1, new[] { new ItemStack(ItemCatalog.Wood, 2), new ItemStack(ItemCatalog.Scrap, 1) }, rotatable: false, inventorySlots: 16),
            new StructureDefinition(ShelterModule, 3, 2, new[] { new ItemStack(ItemCatalog.Scrap, 8), new ItemStack(ItemCatalog.Wood, 4) }),
            new StructureDefinition(PowerPole, 1, 1, new[] { new ItemStack(ItemCatalog.Wood, 2), new ItemStack(ItemCatalog.Scrap, 1) }, rotatable: false),
            new StructureDefinition(Computer, 1, 1, new[] { new ItemStack(ItemCatalog.Scrap, 6), new ItemStack(ItemCatalog.CopperOre, 3) }, rotatable: false),
            new StructureDefinition(Drill, 2, 2, new[] { new ItemStack(ItemCatalog.Scrap, 10), new ItemStack(ItemCatalog.Stone, 4) },
                inventorySlots: 1, placement: PlacementRule.OverResource),
            new StructureDefinition(CoalGenerator, 2, 2, new[] { new ItemStack(ItemCatalog.Scrap, 8), new ItemStack(ItemCatalog.Stone, 6) },
                inventorySlots: 2),
            // Salvaged from the crashed ship; placed by world setup, not buildable.
            new StructureDefinition(EmergencyBattery, 2, 1, Array.Empty<ItemStack>(), rotatable: false, playerBuildable: false),
        };

        /// <summary>Structures shown in the build palette, in order.</summary>
        public static readonly IReadOnlyList<StructureDefinition> Buildable = FilterBuildable();

        private static IReadOnlyList<StructureDefinition> FilterBuildable()
        {
            var list = new List<StructureDefinition>();
            foreach (var d in Default) if (d.PlayerBuildable) list.Add(d);
            return list;
        }

        public static StructureDefinition Find(string id)
        {
            foreach (var d in Default) if (d.Id == id) return d;
            return null;
        }
    }
}
