using System;
using System.Collections.Generic;
using System.Linq;

namespace Codebound.Core
{
    public sealed class Tech
    {
        public string Id, Name, Area, Description;
        public bool MainLine;                       // the campaign: rebuilding the ship
        public float Seconds;                       // lab time at full power
        public ItemStack[] Cost = Array.Empty<ItemStack>();
        public string[] Prereqs = Array.Empty<string>();
        public string[] Unlocks = Array.Empty<string>(); // building ids
    }

    /// <summary>
    /// Hybrid tech tree: a main line that rebuilds the ship plus optional branches. Research happens in
    /// programmable labs (lab.research("id")), consuming the cost from the lab's input.
    /// </summary>
    public sealed class Research
    {
        public static readonly List<Tech> Tree = new List<Tech>
        {
            new Tech { Id = "logistics", Name = "Logistics", Area = "Industry", Seconds = 20,
                Description = "Belts, programmable conveyors and inserters move items between machines.",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 10), new ItemStack(ItemDatabase.CopperPlate, 5) },
                Unlocks = new[] { "belt", "conveyor", "inserter" } },
            new Tech { Id = "agriculture", Name = "Agriculture", Area = "Science", Seconds = 20,
                Description = "Greenhouses grow alien fruit with water and light.",
                Cost = new[] { new ItemStack(ItemDatabase.AlienFruit, 3), new ItemStack(ItemDatabase.StoneBrick, 5) },
                Unlocks = new[] { "greenhouse" } },
            new Tech { Id = "manufacturing", Name = "Manufacturing", Area = "Industry", Seconds = 30,
                Description = "Assemblers craft gears, copper wire and circuits.",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 15), new ItemStack(ItemDatabase.CopperPlate, 10), new ItemStack(ItemDatabase.StoneBrick, 5) },
                Prereqs = new[] { "logistics" }, Unlocks = new[] { "assembler" } },
            new Tech { Id = "advanced_computing", Name = "Advanced Computing", Area = "Computing", Seconds = 40,
                Description = "Computers with 3x the CPU, double RAM and 4x storage.",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 10) },
                Prereqs = new[] { "manufacturing" }, Unlocks = new[] { "computer2" } },

            new Tech { Id = "defense", Name = "Defense", Area = "Defense", Seconds = 25,
                Description = "Programmable turrets: turret.targets() and turret.fire(id).",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 10), new ItemStack(ItemDatabase.CopperPlate, 10), new ItemStack(ItemDatabase.StoneBrick, 5) },
                Unlocks = new[] { "turret" } },

            // ---- main line: rebuild the ship
            new Tech { Id = "ship_frame", Name = "Ship: Hull Frame", Area = "Space", MainLine = true, Seconds = 45,
                Description = "Repair the crashed hull's frame.",
                Cost = new[] { new ItemStack(ItemDatabase.IronGear, 30), new ItemStack(ItemDatabase.StoneBrick, 20) },
                Prereqs = new[] { "manufacturing" } },
            new Tech { Id = "ship_reactor", Name = "Ship: Reactor", Area = "Space", MainLine = true, Seconds = 60,
                Description = "A uranium reactor to power the ship.",
                Cost = new[] { new ItemStack(ItemDatabase.UraniumOre, 20), new ItemStack(ItemDatabase.Circuit, 20) },
                Prereqs = new[] { "ship_frame", "advanced_computing" } },
            new Tech { Id = "ship_navigation", Name = "Ship: Navigation", Area = "Space", MainLine = true, Seconds = 60,
                Description = "Navigation computers tuned with alien crystals.",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 40), new ItemStack(ItemDatabase.AlienCrystal, 10) },
                Prereqs = new[] { "ship_frame", "advanced_computing" } },
            new Tech { Id = "ship_engines", Name = "Ship: Engines", Area = "Space", MainLine = true, Seconds = 60,
                Description = "Titanium engines for escape velocity.",
                Cost = new[] { new ItemStack(ItemDatabase.TitaniumOre, 40), new ItemStack(ItemDatabase.IronGear, 40) },
                Prereqs = new[] { "ship_frame" } },
            new Tech { Id = "ship_launch", Name = "Ship: Launch Sequence", Area = "Space", MainLine = true, Seconds = 90,
                Description = "The final checks. The ship can leave the planet.",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 30), new ItemStack(ItemDatabase.Oil, 10) },
                Prereqs = new[] { "ship_reactor", "ship_navigation", "ship_engines" } },
        };

        static readonly Dictionary<string, Tech> byId = Tree.ToDictionary(t => t.Id);
        public static Tech Get(string id) => id != null && byId.TryGetValue(id, out var t) ? t : null;

        /// <summary>The tech that unlocks a building, or null if it is available from the start.</summary>
        public static Tech UnlockerOf(string buildingId) => Tree.FirstOrDefault(t => t.Unlocks.Contains(buildingId));

        public readonly HashSet<string> Done = new HashSet<string>();
        public event Action<Tech> Completed;

        public bool IsDone(string id) => Done.Contains(id);
        public bool PrereqsMet(Tech t) => t.Prereqs.All(Done.Contains);
        public bool IsUnlocked(BuildingDef def) { var t = UnlockerOf(def.Id); return t == null || Done.Contains(t.Id); }

        public string Status(Tech t) => Done.Contains(t.Id) ? "done" : PrereqsMet(t) ? "available" : "locked";

        public int MainLineDone => Tree.Count(t => t.MainLine && Done.Contains(t.Id));
        public int MainLineTotal => Tree.Count(t => t.MainLine);

        public void Complete(Tech t)
        {
            if (Done.Add(t.Id)) Completed?.Invoke(t);
        }

        public string SaveLine() => Done.Count == 0 ? null : "research " + string.Join(",", Done.OrderBy(x => x));

        public void Load(string[] p)
        {
            if (p.Length < 2) return;
            foreach (var id in p[1].Split(','))
                if (Get(id) != null) Done.Add(id);   // ids from an older/newer tree are ignored, not fatal
        }
    }

    /// <summary>Lab: holds research materials; research is a timed, powered action started by a program.</summary>
    public sealed class LabState : MachineState
    {
        public readonly Inventory Input = new Inventory(8);
        public string Current;   // tech being researched

        public override bool TryInsert(string itemId, int count) => Input.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("input", Input); } }
    }
}
