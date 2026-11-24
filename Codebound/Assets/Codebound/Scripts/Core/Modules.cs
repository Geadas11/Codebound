using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Modules (Etapa 11): put by hand into a machine's module slots (one per slot). Speed: +20 % speed, +50 % power.
    /// Efficiency: −30 % power (never below 20 % of normal). Productivity: −5 % speed, +40 % power, +4 % extra product
    /// (each time the bonus adds up to a whole one, a free product comes out). Beacons pass on half the effect of their
    /// speed and efficiency modules to every machine with module slots within 3 cells, while powered.
    /// </summary>
    public static class Modules
    {
        public static bool IsModule(string item) => item == ItemDatabase.SpeedModule || item == ItemDatabase.EfficiencyModule || item == ItemDatabase.ProductivityModule;

        public static (float speed, float energy, float productivity) EffectOf(string item)
        {
            if (item == ItemDatabase.SpeedModule) return (0.2f, 0.5f, 0f);
            if (item == ItemDatabase.EfficiencyModule) return (0f, -0.3f, 0f);
            if (item == ItemDatabase.ProductivityModule) return (-0.05f, 0.4f, 0.04f);
            return (0f, 0f, 0f);
        }
    }

    /// <summary>A beacon: two module slots whose effect (halved) reaches the machines around it.</summary>
    public sealed class BeaconState : MachineState
    {
        public const int Range = 3;
        public override string Status
        {
            get
            {
                int n = 0; if (Modules != null) for (int i = 0; i < Modules.SlotCount; i++) n += Modules[i].Count;
                return n == 0 ? "no modules" : PowerSatisfaction <= 0 ? "no power" : n + " modules";
            }
            set { }
        }
    }

    public sealed partial class Factory
    {
        public const float BeaconKW = 60f;
        readonly List<PlacedBuilding> beacons = new List<PlacedBuilding>();

        static bool InBeaconRange(PlacedBuilding beacon, PlacedBuilding m)
        {
            var (bw, bh) = beacon.Size; var (mw, mh) = m.Size;
            int r = BeaconState.Range;
            return m.Origin.X <= beacon.Origin.X + bw - 1 + r && m.Origin.X + mw - 1 >= beacon.Origin.X - r
                && m.Origin.Y <= beacon.Origin.Y + bh - 1 + r && m.Origin.Y + mh - 1 >= beacon.Origin.Y - r;
        }

        /// <summary>The summed module effects on a machine: its own modules plus half of every powered beacon in range.</summary>
        public (float speed, float energy, float productivity) Effects(PlacedBuilding b)
        {
            float s = 0, e = 0, p = 0;
            var own = b.State?.Modules;
            if (own != null)
                for (int i = 0; i < own.SlotCount; i++)
                    if (!own[i].IsEmpty) { var (ms, me, mp) = Modules.EffectOf(own[i].ItemId); s += ms * own[i].Count; e += me * own[i].Count; p += mp * own[i].Count; }
            if (b.Def.ModuleSlots > 0 && !(b.State is BeaconState))
                foreach (var bc in beacons)
                {
                    if (bc.State.PowerSatisfaction <= 0 || !InBeaconRange(bc, b)) continue;
                    var inv = bc.State.Modules;
                    for (int i = 0; i < inv.SlotCount; i++)
                        if (!inv[i].IsEmpty) { var (ms, me, _) = Modules.EffectOf(inv[i].ItemId); s += 0.5f * ms * inv[i].Count; e += 0.5f * me * inv[i].Count; }
                }
            return (s, e, p);
        }

        /// <summary>A product made by a machine, plus the productivity bonus: a free one each time the bonus reaches 1.</summary>
        void Produce(PlacedBuilding m, Inventory output, string item, int amount)
        {
            output.Add(item, amount);
            float p = Effects(m).productivity;
            if (p <= 0) return;
            m.State.ProductivityBonus += p * amount;
            while (m.State.ProductivityBonus >= 1f && output.Add(item, 1) == 0) m.State.ProductivityBonus -= 1f;
        }
    }
}
