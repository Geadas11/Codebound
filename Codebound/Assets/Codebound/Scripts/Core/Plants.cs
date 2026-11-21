using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>A crop. Seeds are the item itself (a fruit, a cap, a stalk), planted in greenhouses or hydroponic bays.</summary>
    public sealed class PlantSpecies
    {
        public string Id, Seed, Produce;
        public int Yield;
        public float GrowSeconds;
        public bool NeedsLight;
    }

    public static class Plants
    {
        public static readonly PlantSpecies[] All =
        {
            new PlantSpecies { Id = "fruit", Seed = ItemDatabase.AlienFruit, Produce = ItemDatabase.AlienFruit, Yield = 3, GrowSeconds = 180f, NeedsLight = true },
            new PlantSpecies { Id = "glowcap", Seed = ItemDatabase.Glowcap, Produce = ItemDatabase.Glowcap, Yield = 2, GrowSeconds = 240f, NeedsLight = false },  // grows in the dark
            new PlantSpecies { Id = "fiber", Seed = ItemDatabase.Fiber, Produce = ItemDatabase.Fiber, Yield = 4, GrowSeconds = 120f, NeedsLight = true },
        };

        public static PlantSpecies Get(string id) { foreach (var p in All) if (p.Id == id) return p; return null; }
        public static PlantSpecies BySeed(string item) { foreach (var p in All) if (p.Seed == item) return p; return null; }
        public static bool IsSeed(string item) => BySeed(item) != null;

        public const float MinGene = 0.5f, MaxGene = 2.5f, Mutation = 0.15f;

        /// <summary>Selective breeding: each harvest mutates the plant's vigor by up to ±15%; the best strain is kept.
        /// Deterministic (hash of harvest count, plot and species) so saves and tests reproduce.</summary>
        public static float Mutate(float gene, int harvests, int plot, string species)
        {
            double h = WorldGenerator.Hash01(harvests, plot * 31 + species.Length * 7 + species[0], 4471);
            return Math.Max(MinGene, Math.Min(MaxGene, gene * (float)(1 + (h * 2 - 1) * Mutation)));
        }

        /// <summary>Harvest size for a plant with this vigor.</summary>
        public static int YieldOf(PlantSpecies s, float gene) => Math.Max(1, (int)Math.Round(s.Yield * gene));
    }
}
