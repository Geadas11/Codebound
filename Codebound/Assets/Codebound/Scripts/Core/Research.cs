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
            new Tech { Id = "robotics", Name = "Robotics", Area = "Industry", Seconds = 30,
                Description = "Sorter belts (one item goes straight, the rest turn right), splitters and long inserters (2 cells). No program needed.",
                Cost = new[] { new ItemStack(ItemDatabase.IronGear, 10), new ItemStack(ItemDatabase.Circuit, 4) },
                Prereqs = new[] { "manufacturing" }, Unlocks = new[] { "sorter", "splitter", "long_inserter" } },
            new Tech { Id = "networking", Name = "Networking", Area = "Computing", Seconds = 30,
                Description = "Computers talk: computer.send(channel, data) and the 'message' event. Data servers (shared key-value store) and storage hubs (nearby chests act as one).",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 6), new ItemStack(ItemDatabase.CopperWire, 10) },
                Prereqs = new[] { "manufacturing" }, Unlocks = new[] { "server", "storage_hub" } },
            new Tech { Id = "programming", Name = "Programming", Area = "Computing", Seconds = 30,
                Description = "Optimised firmware: every computer makes 50% more world API calls per second.",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 6), new ItemStack(ItemDatabase.Silicon, 4) },
                Prereqs = new[] { "networking" } },
            new Tech { Id = "advanced_computing", Name = "Advanced Computing", Area = "Computing", Seconds = 40,
                Description = "Computers with 3x the CPU, double RAM and 4x storage.",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 10) },
                Prereqs = new[] { "manufacturing" }, Unlocks = new[] { "computer2" } },

            new Tech { Id = "energy", Name = "Renewable Energy", Area = "Energy", Seconds = 25,
                Description = "Solar panels (output follows daylight and weather) and batteries that store spare sunlight.",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 10), new ItemStack(ItemDatabase.CopperPlate, 10), new ItemStack(ItemDatabase.Silicon, 4) },
                Unlocks = new[] { "solar", "battery" } },
            new Tech { Id = "hydroponics", Name = "Hydroponics", Area = "Science", Seconds = 25,
                Description = "Hydroponic bays: grow anywhere under lamps (always powered), half the water, 1.5x faster. Glowcaps and fiberweed grow in greenhouses too.",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 6), new ItemStack(ItemDatabase.Silicon, 3), new ItemStack(ItemDatabase.Glowcap, 2) },
                Prereqs = new[] { "agriculture" }, Unlocks = new[] { "hydroponics" } },
            new Tech { Id = "ecology", Name = "Ecology", Area = "Science", Seconds = 25,
                Description = "Air scrubbers clean the pollution of their 16x16 region while powered (30 kW).",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 8), new ItemStack(ItemDatabase.CopperPlate, 6), new ItemStack(ItemDatabase.Silicon, 2) },
                Prereqs = new[] { "agriculture" }, Unlocks = new[] { "scrubber" } },
            new Tech { Id = "climate_control", Name = "Climate Control", Area = "Energy", Seconds = 25,
                Description = "Heaters keep the air at 21 °C within 5 cells (25 kW). Lightning rods catch thunderstorm strikes within 8 cells and charge your batteries.",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 8), new ItemStack(ItemDatabase.CopperWire, 10) },
                Prereqs = new[] { "energy" }, Unlocks = new[] { "heater", "lightning_rod" } },
            new Tech { Id = "materials", Name = "Materials", Area = "Industry", Seconds = 30,
                Description = "Reinforced walls (1000 HP) and shock traps that stun creatures (3 charges, recharged with copper wire).",
                Cost = new[] { new ItemStack(ItemDatabase.IronPlate, 12), new ItemStack(ItemDatabase.StoneBrick, 10) },
                Prereqs = new[] { "logistics" }, Unlocks = new[] { "reinforced_wall", "shock_trap" } },
            new Tech { Id = "exploration", Name = "Exploration", Area = "Science", Seconds = 25,
                Description = "Deep scanner: the journal (J) finds unread terminals and cave mouths 2.5x farther away.",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 3), new ItemStack(ItemDatabase.AlienCrystal, 2) } },
            new Tech { Id = "cybersecurity", Name = "Cybersecurity", Area = "Security", Seconds = 25,
                Description = "Firewalls on every computer: rogue drones need 3x longer to break in.",
                Cost = new[] { new ItemStack(ItemDatabase.CopperPlate, 10), new ItemStack(ItemDatabase.Silicon, 2) } },
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
                Description = "The final checks. Build the Launch Pad and fly the ascent with a program: pad.launch(), pad.throttle(x), pad.flight().",
                Cost = new[] { new ItemStack(ItemDatabase.Circuit, 30), new ItemStack(ItemDatabase.Oil, 10) },
                Prereqs = new[] { "ship_reactor", "ship_navigation", "ship_engines" }, Unlocks = new[] { "launchpad" } },
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
        public override bool AcceptsItem(string itemId) => true;
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("input", Input); } }
    }
}
