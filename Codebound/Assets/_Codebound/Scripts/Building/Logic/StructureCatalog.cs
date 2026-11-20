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

        public static readonly IReadOnlyList<StructureDefinition> Default = new[]
        {
            new StructureDefinition(Wall, 1, 1, new[] { new ItemStack(ItemCatalog.Scrap, 1), new ItemStack(ItemCatalog.Stone, 1) }, rotatable: false),
            new StructureDefinition(Floor, 1, 1, new[] { new ItemStack(ItemCatalog.Stone, 1) }, rotatable: false, blocksMovement: false),
            new StructureDefinition(Crate, 1, 1, new[] { new ItemStack(ItemCatalog.Wood, 2), new ItemStack(ItemCatalog.Scrap, 1) }, rotatable: false, inventorySlots: 16),
            new StructureDefinition(ShelterModule, 3, 2, new[] { new ItemStack(ItemCatalog.Scrap, 8), new ItemStack(ItemCatalog.Wood, 4) }),
        };

        public static StructureDefinition Find(string id)
        {
            foreach (var d in Default) if (d.Id == id) return d;
            return null;
        }
    }
}
