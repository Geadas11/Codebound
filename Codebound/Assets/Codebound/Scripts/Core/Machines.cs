using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>Runtime state attached to a placed building (inventories, fuel, program...).</summary>
    public abstract class MachineState
    {
        /// <summary>0..1 share of requested electric power received last tick (1 for machines that need none).</summary>
        public float PowerSatisfaction = 1f;
        public string Status = "idle";

        /// <summary>Insert from a conveyor/drill/player. All-or-nothing.</summary>
        public virtual bool TryInsert(string itemId, int count) => false;

        /// <summary>Named inventories, in a stable order (UI and save files use the names).</summary>
        public virtual IEnumerable<(string role, Inventory inv)> Inventories { get { yield break; } }

        /// <summary>The inventory an inserter may take from (outputs, chest contents), or null.</summary>
        public virtual Inventory Extractable => null;

        public virtual void SaveExtra(IDictionary<string, string> data) { }
        public virtual void LoadExtra(string key, string value) { }

        protected static string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);
        protected static float PF(string s) => float.Parse(s, CultureInfo.InvariantCulture);
    }

    public sealed class StorageState : MachineState
    {
        public readonly Inventory Items = new Inventory(16);
        public override bool TryInsert(string itemId, int count) => Items.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("items", Items); } }
        public override Inventory Extractable => Items;
    }

    public sealed class FurnaceState : MachineState
    {
        public static readonly Dictionary<string, string> Recipes = new Dictionary<string, string>
        {
            { ItemDatabase.IronOre, ItemDatabase.IronPlate },
            { ItemDatabase.CopperOre, ItemDatabase.CopperPlate },
            { ItemDatabase.Stone, ItemDatabase.StoneBrick },
        };

        public readonly Inventory Input = new Inventory(1), Fuel = new Inventory(1), Output = new Inventory(1);
        public float BurnLeftKJ;

        public override bool TryInsert(string itemId, int count)
        {
            if (itemId == ItemDatabase.Coal) return Fuel.TryAddAll(new[] { new ItemStack(itemId, count) });
            if (Recipes.ContainsKey(itemId)) return Input.TryAddAll(new[] { new ItemStack(itemId, count) });
            return false;
        }

        public override IEnumerable<(string, Inventory)> Inventories
        {
            get { yield return ("input", Input); yield return ("fuel", Fuel); yield return ("output", Output); }
        }
        public override Inventory Extractable => Output;

        public override void SaveExtra(IDictionary<string, string> d) => d["burn"] = F(BurnLeftKJ);
        public override void LoadExtra(string k, string v) { if (k == "burn") BurnLeftKJ = PF(v); }
    }

    public sealed class DrillState : MachineState
    {
        public readonly Inventory Output = new Inventory(1);
        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("output", Output); } }
        public override Inventory Extractable => Output;
    }

    /// <summary>Belt or programmable conveyor cell: carries one item at a time towards the front.</summary>
    public sealed class ConveyorState : MachineState
    {
        public const float BeltSpeed = 1.875f; // cells per second
        public readonly Inventory Slot = new Inventory(1);
        public float Progress;     // 0 = just entered, 1 = at the front edge
        public bool Running;       // belts: always; programmable conveyors: start()/stop()
        public string Filter;      // programmable conveyors only accept this item when set
        public bool ArrivedThisTick;

        public string Item => Slot[0].IsEmpty ? null : Slot[0].ItemId;

        public override bool TryInsert(string itemId, int count)
        {
            if (count != 1 || Item != null || (Filter != null && Filter != itemId)) return false;
            Slot.Add(itemId, 1);
            Progress = 0; ArrivedThisTick = true;
            return true;
        }

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("item", Slot); } }
        public override Inventory Extractable => Slot;
        public override void SaveExtra(IDictionary<string, string> d)
        {
            d["prog"] = F(Progress); d["on"] = Running ? "1" : "0";
            if (Filter != null) d["filter"] = Filter;
        }
        public override void LoadExtra(string k, string v)
        {
            if (k == "prog") Progress = PF(v); else if (k == "on") Running = v == "1"; else if (k == "filter") Filter = v;
        }
    }

    /// <summary>Arm that moves one item at a time from the building behind it to the building in front.</summary>
    public sealed class InserterState : MachineState
    {
        public const float SwingSeconds = 0.8f;
        public float Progress;     // seconds since the last swing
        public bool Running = true;
        public string Filter;
        public int Moved;          // total items moved (for programs and UI)

        public override void SaveExtra(IDictionary<string, string> d)
        {
            d["on"] = Running ? "1" : "0";
            if (Filter != null) d["filter"] = Filter;
        }
        public override void LoadExtra(string k, string v)
        {
            if (k == "on") Running = v == "1"; else if (k == "filter") Filter = v;
        }
    }

    /// <summary>
    /// Four plots for alien fruit. Programs plant (uses 1 fruit as seed), water (from the tank, filled by a
    /// nearby lake) and harvest (3 fruit). Plants grow with light: daylight, or grow lamps when powered.
    /// </summary>
    public sealed class GreenhouseState : MachineState
    {
        public const int Plots = 4;
        public const float GrowSeconds = 180f, WaterPerSecond = 1f / 90f, TankRefill = 2f, WaterPerDrink = 0.5f;
        public const int Yield = 3;
        public readonly Inventory Seeds = new Inventory(1), Output = new Inventory(1);
        public readonly float[] Growth = new float[Plots];     // -1 = empty plot, 0..1 growing, 1 = ripe
        public readonly float[] Soil = new float[Plots];       // water in the soil 0..1
        public float Tank;                                     // 0..100 water units
        public bool NearWater;                                 // set by the factory when placed
        public float Light;                                    // last tick light level 0..1

        public GreenhouseState() { for (int i = 0; i < Plots; i++) Growth[i] = -1; }

        public override bool TryInsert(string itemId, int count) =>
            itemId == ItemDatabase.AlienFruit && Seeds.TryAddAll(new[] { new ItemStack(itemId, count) });

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("seeds", Seeds); yield return ("output", Output); } }
        public override Inventory Extractable => Output;

        public override void SaveExtra(IDictionary<string, string> d)
        {
            var parts = new List<string>();
            for (int i = 0; i < Plots; i++) parts.Add(F(Growth[i]) + ":" + F(Soil[i]));
            d["plots"] = string.Join(",", parts);
            d["tank"] = F(Tank);
        }

        public override void LoadExtra(string k, string v)
        {
            if (k == "tank") Tank = PF(v);
            else if (k == "plots")
            {
                var p = v.Split(',');
                for (int i = 0; i < Plots && i < p.Length; i++) { var gs = p[i].Split(':'); Growth[i] = PF(gs[0]); Soil[i] = PF(gs[1]); }
            }
        }
    }

    /// <summary>Crafts components from inputs; each craft() is a timed, powered action.</summary>
    public sealed class AssemblerState : MachineState
    {
        public sealed class Recipe
        {
            public string Product; public int Amount; public float Seconds; public ItemStack[] Inputs;
        }

        public static readonly Dictionary<string, Recipe> Recipes = new Dictionary<string, Recipe>
        {
            { ItemDatabase.IronGear, new Recipe { Product = ItemDatabase.IronGear, Amount = 1, Seconds = 1f, Inputs = new[] { new ItemStack(ItemDatabase.IronPlate, 2) } } },
            { ItemDatabase.CopperWire, new Recipe { Product = ItemDatabase.CopperWire, Amount = 2, Seconds = 0.5f, Inputs = new[] { new ItemStack(ItemDatabase.CopperPlate, 1) } } },
            { ItemDatabase.Circuit, new Recipe { Product = ItemDatabase.Circuit, Amount = 1, Seconds = 3f,
                Inputs = new[] { new ItemStack(ItemDatabase.IronPlate, 1), new ItemStack(ItemDatabase.CopperWire, 3), new ItemStack(ItemDatabase.Silicon, 1) } } },
        };

        public readonly Inventory Input = new Inventory(6), Output = new Inventory(1);

        static bool IsIngredient(string item)
        {
            foreach (var r in Recipes.Values) foreach (var i in r.Inputs) if (i.ItemId == item) return true;
            return false;
        }

        public override bool TryInsert(string itemId, int count) =>
            IsIngredient(itemId) && Input.TryAddAll(new[] { new ItemStack(itemId, count) });

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("input", Input); yield return ("output", Output); } }
        public override Inventory Extractable => Output;
    }

    public sealed class GeneratorState : MachineState
    {
        public readonly Inventory Fuel = new Inventory(1);
        public float BurnLeftKJ;
        public float LoadKW; // last tick output, for UI and the scripting API

        public override bool TryInsert(string itemId, int count) =>
            itemId == ItemDatabase.Coal && Fuel.TryAddAll(new[] { new ItemStack(itemId, count) });

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("fuel", Fuel); } }
        public override void SaveExtra(IDictionary<string, string> d) => d["burn"] = F(BurnLeftKJ);
        public override void LoadExtra(string k, string v) { if (k == "burn") BurnLeftKJ = PF(v); }
    }

    public sealed class ComputerState : MachineState
    {
        /// <summary>Hardware tiers: the Advanced Computer is crafted from circuits.</summary>
        public static ComputerState ForModel(string defId) => defId == "computer2"
            ? new ComputerState { CallsPerSecond = 60, RamMb = 64, MaxCodeBytes = 256 * 1024 }
            : new ComputerState();

        // Hardware of the Basic Computer.
        public int CallsPerSecond = 20;  // CPU: world API calls per second
        public int RamMb = 32;           // heap limit of the program's isolated worker
        public int MaxCodeBytes = 64 * 1024; // storage
        public string Language = "javascript";
        public string Code = DefaultProgram;
        public bool Running;             // player intent: should the program be running
        public string LastError;
        public int LastErrorLine;
        /// <summary>Taken over by a rogue drone: programs stop until the system is restored (Run restores it).</summary>
        public bool Compromised;        // 0 = unknown
        public readonly HashSet<int> Breakpoints = new HashSet<int>();
        public static readonly string[] Languages = { "javascript", "typescript", "python", "sql" };
        /// <summary>SQL programs: the player's own tables (SQLite dump), kept with the computer.</summary>
        public string SqlDb = "";
        public readonly List<string> Log = new List<string>();
        public const int MaxLogLines = 200;

        public const string DefaultProgram =
            "// Codebound — Basic Computer (JavaScript)\n" +
            "// Machines within 8 cells are yours to command.\n" +
            "log(\"Hello from \" + computer.name);\n" +
            "while (true) {\n" +
            "  if (energy.available() && drill) {\n" +
            "    const item = drill.mine();\n" +
            "    if (item) log(\"mined \" + item);\n" +
            "  } else {\n" +
            "    sleep(1);\n" +
            "  }\n" +
            "}\n";

        public void AddLog(string line)
        {
            Log.Add(line);
            if (Log.Count > MaxLogLines) Log.RemoveRange(0, Log.Count - MaxLogLines);
        }

        public override void SaveExtra(IDictionary<string, string> d)
        {
            d["lang"] = Language;
            d["code"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Code ?? ""));
            d["run"] = Running ? "1" : "0";
            if (Compromised) d["comp"] = "1";
            if (Breakpoints.Count > 0) d["bps"] = string.Join(",", Breakpoints);
            if (!string.IsNullOrEmpty(SqlDb)) d["sqldb"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SqlDb));
        }

        public override void LoadExtra(string k, string v)
        {
            if (k == "lang") Language = v;
            else if (k == "code") Code = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(v));
            else if (k == "run") Running = v == "1";
            else if (k == "comp") Compromised = v == "1";
            else if (k == "sqldb") SqlDb = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(v));
            else if (k == "bps") foreach (var part in v.Split(',')) if (int.TryParse(part, out int l)) Breakpoints.Add(l);
        }
    }
}
