using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>Runtime state attached to a placed building (inventories, fuel, program...).</summary>
    public abstract class MachineState
    {
        /// <summary>0..1 share of requested electric power received last tick (1 for machines that need none).</summary>
        public float PowerSatisfaction = 1f;
        string status = "idle";
        public virtual string Status { get => status; set => status = value; }

        /// <summary>Sleeping: nothing to do until something it depends on changes (Factory wakes it). Not saved: a loaded
        /// world starts awake and machines go back to sleep on their first idle tick.</summary>
        public bool Asleep { get; internal set; }
        /// <summary>Raised when a setting that decides what this machine accepts or does changes (filter, start/stop).</summary>
        public event System.Action Touched;
        protected void Touch() => Touched?.Invoke();

        /// <summary>Machines a signal condition can switch (arms, belts, pumps, pumpjacks, fillers, lamps).</summary>
        public virtual bool TakesCondition => false;
        /// <summary>Works only while this holds — when the machine is next to a signal network. Null = always.</summary>
        public SignalCondition Condition { get; private set; }
        /// <summary>The condition's result this tick (true when there is no condition or no network).</summary>
        public bool SignalAllows { get; internal set; } = true;
        /// <summary>Sets the condition from text like "iron_plate&lt;100" ("" clears it). Returns null or why not.</summary>
        public string SetCondition(string text)
        {
            if (!TakesCondition) return "this machine has no condition";
            var err = SignalSpec.ParseCondition(text, out var c);
            if (err != null) return err;
            if ((Condition?.Text ?? "") != (c?.Text ?? "")) { Condition = c; Touch(); }
            return null;
        }

        /// <summary>Insert from a conveyor/drill/player. All-or-nothing.</summary>
        public virtual bool TryInsert(string itemId, int count) => false;

        /// <summary>Module slots (machines with ModuleSlots &gt; 0 and beacons; null otherwise). One module per slot.</summary>
        public Inventory Modules;
        /// <summary>Productivity modules: the part of a free product earned so far.</summary>
        public float ProductivityBonus;

        /// <summary>Everything it holds, modules included (saves, refunds, the panel, programs).</summary>
        public IEnumerable<(string role, Inventory inv)> AllInventories
        {
            get { foreach (var x in Inventories) yield return x; if (Modules != null) yield return ("modules", Modules); }
        }

        /// <summary>The player (or a guest) puts one item in: modules go to the module slots, anything else as usual.</summary>
        public bool PutByHand(string itemId)
        {
            if (Modules != null && Core.Modules.IsModule(itemId))
            {
                if (this is BeaconState && itemId == ItemDatabase.ProductivityModule) return false;   // beacons pass on speed and efficiency only
                int n = 0; for (int i = 0; i < Modules.SlotCount; i++) n += Modules[i].Count;
                return n < Modules.SlotCount && Modules.Add(itemId, 1) == 0;
            }
            return TryInsert(itemId, 1);
        }
        /// <summary>Would this machine ever take the item (ignoring whether it has room now)? Arms only pick up what the target accepts.</summary>
        public virtual bool AcceptsItem(string itemId) => false;

        /// <summary>Named inventories, in a stable order (UI and save files use the names).</summary>
        public virtual IEnumerable<(string role, Inventory inv)> Inventories { get { yield break; } }

        /// <summary>The inventory an inserter may take from (outputs, chest contents), or null.</summary>
        public virtual Inventory Extractable => null;

        /// <summary>Fluids held (pipes report their whole segment), for programs and the machine panel.</summary>
        public virtual IEnumerable<(string fluid, float amount)> FluidContents { get { yield break; } }

        public virtual void SaveExtra(IDictionary<string, string> data) { }
        public virtual void LoadExtra(string key, string value) { }

        protected static string F(float v) => v.ToString("R", CultureInfo.InvariantCulture);
        protected static float PF(string s) => float.Parse(s, CultureInfo.InvariantCulture);
    }

    public sealed class StorageState : MachineState
    {
        public readonly Inventory Items = new Inventory(16);
        /// <summary>Plain chest, provider (robots may take from it) or requester (robots bring what it asks for).</summary>
        public LogisticRole Role;
        /// <summary>Requester chests: how many of each item to keep (robots top it up).</summary>
        public readonly Dictionary<string, int> Requests = new Dictionary<string, int>();
        internal readonly List<string> RequestOrder = new List<string>();
        public string RequestText => SignalSpec.FormatPairs(Requests);

        /// <summary>"iron_plate=50,copper_plate=20" ("" clears). Returns null or why not.</summary>
        public string SetRequests(string text)
        {
            if (Role != LogisticRole.Requester) return "only requester chests take requests";
            var d = new Dictionary<string, int>();
            var err = SignalSpec.ParsePairs(text, d);
            if (err != null) return err;
            foreach (var kv in d)
            {
                if (!ItemDatabase.Exists(kv.Key)) return $"'{kv.Key}' is not an item";
                if (kv.Value < 0 || kv.Value > 1600) return $"'{kv.Key}': ask for 0 to 1600";
            }
            Requests.Clear(); RequestOrder.Clear();
            foreach (var kv in d) if (kv.Value > 0) { Requests[kv.Key] = kv.Value; RequestOrder.Add(kv.Key); }
            RequestOrder.Sort(System.StringComparer.Ordinal);
            Status = Requests.Count == 0 ? "idle" : "requests " + RequestText;
            Touch();
            return null;
        }
        public override void SaveExtra(IDictionary<string, string> d) { if (Requests.Count > 0) d["req"] = RequestText; }
        public override void LoadExtra(string k, string v) { if (k == "req") SetRequests(v); }
        public override bool TryInsert(string itemId, int count) => Items.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId) => true;
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
        /// <summary>The electric furnace: powered from the grid, no coal.</summary>
        public bool Electric;

        public override bool TryInsert(string itemId, int count)
        {
            if (itemId == ItemDatabase.Coal) return !Electric && Fuel.TryAddAll(new[] { new ItemStack(itemId, count) });
            if (Recipes.ContainsKey(itemId)) return Input.TryAddAll(new[] { new ItemStack(itemId, count) });
            return false;
        }
        public override bool AcceptsItem(string itemId) => (itemId == ItemDatabase.Coal && !Electric) || Recipes.ContainsKey(itemId);

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

    /// <summary>
    /// Four plots for alien fruit. Programs plant (uses 1 fruit as seed), water (from the tank, filled by a
    /// nearby lake) and harvest (3 fruit). Plants grow with light: daylight, or grow lamps when powered.
    /// </summary>
    public sealed class GreenhouseState : MachineState
    {
        public const int Plots = 4;
        public const float GrowSeconds = 180f, WaterPerSecond = 1f / 90f, TankRefill = 2f, WaterPerDrink = 0.5f, PipeRefill = 5f;
        public const int Yield = 3;
        public readonly Inventory Seeds = new Inventory(3), Output = new Inventory(2);
        public readonly float[] Growth = new float[Plots];     // -1 = empty plot, 0..1 growing, 1 = ripe
        public readonly string[] Species = new string[Plots];  // plant species id per plot
        public readonly float[] Gene = new float[Plots];       // vigor of the plant in each plot
        public readonly Dictionary<string, float> Strain = new Dictionary<string, float>();   // best vigor bred so far, per species
        public int Harvests;
        public readonly float[] Soil = new float[Plots];       // water in the soil 0..1
        public float Tank;                                     // 0..100 water units
        public bool NearWater;                                 // set by the factory when placed
        public float Light;                                    // last tick light level 0..1

        public GreenhouseState() { for (int i = 0; i < Plots; i++) { Growth[i] = -1; Gene[i] = 1f; } }

        public float StrainOf(string species) => Strain.TryGetValue(species, out var g) ? g : 1f;

        public override bool TryInsert(string itemId, int count) =>
            Plants.IsSeed(itemId) && Seeds.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId) => Plants.IsSeed(itemId);

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("seeds", Seeds); yield return ("output", Output); } }
        public override Inventory Extractable => Output;

        public override void SaveExtra(IDictionary<string, string> d)
        {
            var parts = new List<string>();
            for (int i = 0; i < Plots; i++) parts.Add(F(Growth[i]) + ":" + F(Soil[i]) + ":" + (Species[i] ?? "") + ":" + F(Gene[i]));
            d["plots"] = string.Join(",", parts);
            d["tank"] = F(Tank);
            d["harvests"] = Harvests.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var st = new List<string>(); foreach (var kv in Strain) st.Add(kv.Key + "=" + F(kv.Value));
            if (st.Count > 0) d["strains"] = string.Join(";", st);
        }

        public override void LoadExtra(string k, string v)
        {
            if (k == "tank") Tank = PF(v);
            else if (k == "harvests") Harvests = int.Parse(v, System.Globalization.CultureInfo.InvariantCulture);
            else if (k == "strains") foreach (var e in v.Split(';')) { var kv = e.Split('='); if (kv.Length == 2) Strain[kv[0]] = PF(kv[1]); }
            else if (k == "plots")
            {
                var p = v.Split(',');
                for (int i = 0; i < Plots && i < p.Length; i++)
                {
                    var gs = p[i].Split(':'); Growth[i] = PF(gs[0]); Soil[i] = PF(gs[1]);
                    Species[i] = gs.Length > 2 && gs[2].Length > 0 ? gs[2] : Growth[i] >= 0 ? "fruit" : null;   // older saves: fruit
                    Gene[i] = gs.Length > 3 ? PF(gs[3]) : 1f;
                }
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
            { ItemDatabase.Robot, new Recipe { Product = ItemDatabase.Robot, Amount = 1, Seconds = 5f,
                Inputs = new[] { new ItemStack(ItemDatabase.Circuit, 2), new ItemStack(ItemDatabase.IronGear, 2), new ItemStack(ItemDatabase.CopperWire, 4) } } },
            { ItemDatabase.SpeedModule, new Recipe { Product = ItemDatabase.SpeedModule, Amount = 1, Seconds = 8f,
                Inputs = new[] { new ItemStack(ItemDatabase.Circuit, 5), new ItemStack(ItemDatabase.AdvancedCircuit, 5) } } },
            { ItemDatabase.EfficiencyModule, new Recipe { Product = ItemDatabase.EfficiencyModule, Amount = 1, Seconds = 8f,
                Inputs = new[] { new ItemStack(ItemDatabase.Circuit, 5), new ItemStack(ItemDatabase.AdvancedCircuit, 5), new ItemStack(ItemDatabase.Silicon, 2) } } },
            { ItemDatabase.ProductivityModule, new Recipe { Product = ItemDatabase.ProductivityModule, Amount = 1, Seconds = 8f,
                Inputs = new[] { new ItemStack(ItemDatabase.Circuit, 5), new ItemStack(ItemDatabase.AdvancedCircuit, 5), new ItemStack(ItemDatabase.AlienCrystal, 1) } } },
            { ItemDatabase.ConstructionRobot, new Recipe { Product = ItemDatabase.ConstructionRobot, Amount = 1, Seconds = 5f,
                Inputs = new[] { new ItemStack(ItemDatabase.Circuit, 2), new ItemStack(ItemDatabase.IronGear, 2), new ItemStack(ItemDatabase.BatteryCell, 1) } } },
            { ItemDatabase.AdvancedCircuit, new Recipe { Product = ItemDatabase.AdvancedCircuit, Amount = 1, Seconds = 6f,
                Inputs = new[] { new ItemStack(ItemDatabase.Circuit, 2), new ItemStack(ItemDatabase.Plastic, 2), new ItemStack(ItemDatabase.CopperWire, 4) } } },
            { ItemDatabase.Locomotive, new Recipe { Product = ItemDatabase.Locomotive, Amount = 1, Seconds = 10f,
                Inputs = new[] { new ItemStack(ItemDatabase.IronPlate, 20), new ItemStack(ItemDatabase.IronGear, 10), new ItemStack(ItemDatabase.Circuit, 5) } } },
            { ItemDatabase.CargoWagon, new Recipe { Product = ItemDatabase.CargoWagon, Amount = 1, Seconds = 5f,
                Inputs = new[] { new ItemStack(ItemDatabase.IronPlate, 12), new ItemStack(ItemDatabase.IronGear, 6) } } },
            // food processing
            { ItemDatabase.Ration, new Recipe { Product = ItemDatabase.Ration, Amount = 2, Seconds = 2f,
                Inputs = new[] { new ItemStack(ItemDatabase.AlienFruit, 2), new ItemStack(ItemDatabase.Glowcap, 1) } } },
            // player equipment (kept in the inventory: it works while you carry it)
            { ItemDatabase.LaserTool, new Recipe { Product = ItemDatabase.LaserTool, Amount = 1, Seconds = 5f,
                Inputs = new[] { new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.Circuit, 2), new ItemStack(ItemDatabase.AlienCrystal, 1) } } },
            { ItemDatabase.ArmorPlating, new Recipe { Product = ItemDatabase.ArmorPlating, Amount = 1, Seconds = 4f,
                Inputs = new[] { new ItemStack(ItemDatabase.IronPlate, 8), new ItemStack(ItemDatabase.StoneBrick, 4) } } },
            { ItemDatabase.ThermalSuit, new Recipe { Product = ItemDatabase.ThermalSuit, Amount = 1, Seconds = 4f,
                Inputs = new[] { new ItemStack(ItemDatabase.Fiber, 10), new ItemStack(ItemDatabase.CopperWire, 2) } } },
        };

        public readonly Inventory Input = new Inventory(6), Output = new Inventory(1);

        static bool IsIngredient(string item)
        {
            foreach (var r in Recipes.Values) foreach (var i in r.Inputs) if (i.ItemId == item) return true;
            return false;
        }

        public override bool TryInsert(string itemId, int count) =>
            IsIngredient(itemId) && Input.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId) => IsIngredient(itemId);

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("input", Input); yield return ("output", Output); } }
        public override Inventory Extractable => Output;
    }

    /// <summary>Air scrubber: passive, cleans its pollution region while powered (Ecology).</summary>
    public sealed class ScrubberState : MachineState
    {
        public float Cleaned;    // pollution units removed so far
    }

    public sealed class SolarState : MachineState
    {
        public float OutputKW;   // last tick, for UI and programs
    }

    public sealed class BatteryState : MachineState
    {
        public const float CapacityKJ = 5000f, MaxKW = 100f;
        public float StoredKJ;
        internal int ShownPct = int.MinValue, ShownRate = int.MinValue;   // what Status shows
        public override void SaveExtra(IDictionary<string, string> d) => d["kj"] = F(StoredKJ);
        public override void LoadExtra(string k, string v) { if (k == "kj") StoredKJ = PF(v); }
    }

    public sealed class GeneratorState : MachineState
    {
        public readonly Inventory Fuel = new Inventory(1);
        public float BurnLeftKJ;
        public float LoadKW; // last tick output, for UI and the scripting API
        internal int ShownKW = int.MinValue;   // what Status shows, so it is rebuilt only when it changes

        public override bool TryInsert(string itemId, int count) =>
            itemId == ItemDatabase.Coal && Fuel.TryAddAll(new[] { new ItemStack(itemId, count) });
        public override bool AcceptsItem(string itemId) => itemId == ItemDatabase.Coal;

        public override IEnumerable<(string, Inventory)> Inventories { get { yield return ("fuel", Fuel); } }
        public override void SaveExtra(IDictionary<string, string> d) => d["burn"] = F(BurnLeftKJ);
        public override void LoadExtra(string k, string v) { if (k == "burn") BurnLeftKJ = PF(v); }
    }

    /// <summary>One program run, for the computer's history tab.</summary>
    public sealed class RunRecord
    {
        public string When, Language, Result, Message;   // Result: done | error | stopped
        public float Seconds;
        public int Calls;

        public List<object> ToList() => new List<object> { When, Language, Result, Message, (double)Seconds, (double)Calls };
        public static RunRecord FromList(List<object> l) => l.Count < 6 ? null : new RunRecord
        {
            When = l[0] as string, Language = l[1] as string, Result = l[2] as string, Message = l[3] as string,
            Seconds = l[4] is double s ? (float)s : 0, Calls = l[5] is double c ? (int)c : 0,
        };
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
        /// <summary>What this computer puts on the signal networks next to it (signals.send).</summary>
        public readonly Dictionary<string, int> SignalOut = new Dictionary<string, int>();
        public string Code = DefaultProgram;
        public bool Running;             // player intent: should the program be running
        public string LastError;
        public int LastErrorLine;
        /// <summary>Taken over by a rogue drone: programs stop until the system is restored (Run restores it).</summary>
        public bool Compromised;
        /// <summary>
        /// "pbkdf2$iterations$salt$hash" (base64); empty = no password. Players may reuse real passwords, so only a
        /// salted, slow hash is ever stored or saved.
        /// </summary>
        public string PasswordHash = "";
        public bool HasPassword => PasswordHash.Length > 0;
        const int Iterations = 20000;

        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) { PasswordHash = ""; return; }
            var salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(salt);
            PasswordHash = $"pbkdf2${Iterations}${System.Convert.ToBase64String(salt)}${System.Convert.ToBase64String(Derive(password, salt, Iterations))}";
        }

        public bool CheckPassword(string password)
        {
            if (!HasPassword) return true;
            var p = PasswordHash.Split('$');
            if (p.Length != 4 || p[0] != "pbkdf2" || !int.TryParse(p[1], out int it)) return false;
            var expected = System.Convert.FromBase64String(p[3]);
            var actual = Derive(password ?? "", System.Convert.FromBase64String(p[2]), it);
            int diff = expected.Length ^ actual.Length;   // constant-time compare
            for (int i = 0; i < expected.Length && i < actual.Length; i++) diff |= expected[i] ^ actual[i];
            return diff == 0;
        }

        static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var kdf = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, iterations, System.Security.Cryptography.HashAlgorithmName.SHA256))
                return kdf.GetBytes(32);
        }        // 0 = unknown
        public readonly HashSet<int> Breakpoints = new HashSet<int>();
        public static readonly string[] Languages = { "javascript", "typescript", "python", "sql", "php", "csharp", "c", "cpp" };
        /// <summary>SQL programs: the player's own tables (SQLite dump), kept with the computer.</summary>
        public string SqlDb = "";
        public readonly List<string> Log = new List<string>();
        public const int MaxLogLines = 200;

        // ---- several files per computer (Code is the main file; these are the others), debugger watches, run history
        public const int MaxFiles = 8, MaxHistory = 20;
        public readonly SortedDictionary<string, string> Files = new SortedDictionary<string, string>(System.StringComparer.Ordinal);
        public readonly List<string> Watches = new List<string>();
        public readonly List<RunRecord> History = new List<RunRecord>();
        public string LastErrorFile;   // null = the main file

        static readonly System.Text.RegularExpressions.Regex FileNameRx = new System.Text.RegularExpressions.Regex(@"^[A-Za-z_][A-Za-z0-9_]{0,23}(\.[A-Za-z]{1,4})?$");
        /// <summary>Plain names only (they become module/file names in every language): letters, digits, _ and an extension.</summary>
        public static bool ValidFileName(string n) => n != null && FileNameRx.IsMatch(n) && n != "main";

        public int TotalCodeBytes
        {
            get { int n = System.Text.Encoding.UTF8.GetByteCount(Code ?? ""); foreach (var f in Files.Values) n += System.Text.Encoding.UTF8.GetByteCount(f); return n; }
        }

        public void Record(RunRecord r) { History.Add(r); if (History.Count > MaxHistory) History.RemoveRange(0, History.Count - MaxHistory); }

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
            if (HasPassword) d["pwd"] = PasswordHash;
            if (Breakpoints.Count > 0) d["bps"] = string.Join(",", Breakpoints);
            string B64(string s) => System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s));
            if (Files.Count > 0) { var f = new Dictionary<string, object>(); foreach (var kv in Files) f[kv.Key] = kv.Value; d["files"] = B64(Json.Write(f)); }
            if (Watches.Count > 0) d["watch"] = B64(Json.Write(new List<object>(Watches)));
            if (History.Count > 0) { var h = new List<object>(); foreach (var r in History) h.Add(r.ToList()); d["hist"] = B64(Json.Write(h)); }
            if (!string.IsNullOrEmpty(SqlDb)) d["sqldb"] = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SqlDb));
            if (SignalOut.Count > 0) d["sigout"] = SignalSpec.FormatPairs(SignalOut);
        }

        public override void LoadExtra(string k, string v)
        {
            if (k == "lang") Language = v;
            else if (k == "code") Code = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(v));
            else if (k == "run") Running = v == "1";
            else if (k == "comp") Compromised = v == "1";
            else if (k == "pwd") PasswordHash = v;
            else if (k == "sqldb") SqlDb = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(v));
            else if (k == "sigout") SignalSpec.ParsePairs(v, SignalOut);
            else if (k == "files" || k == "watch" || k == "hist")
            {
                object j = Json.Parse(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(v)));
                if (k == "files" && j is Dictionary<string, object> fd) foreach (var kv in fd) { if (ValidFileName(kv.Key) && kv.Value is string src) Files[kv.Key] = src; }
                else if (k == "watch" && j is List<object> wl) foreach (var w in wl) { if (w is string ws) Watches.Add(ws); }
                else if (j is List<object> hl) foreach (var h in hl) { if (h is List<object> hr && RunRecord.FromList(hr) is RunRecord rr) History.Add(rr); }
            }
            else if (k == "bps") foreach (var part in v.Split(',')) if (int.TryParse(part, out int l)) Breakpoints.Add(l);
        }
    }
}
