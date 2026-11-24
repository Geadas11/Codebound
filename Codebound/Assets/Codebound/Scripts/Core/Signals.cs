using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Codebound.Core
{
    /// <summary>
    /// Circuit signals (Etapa 5 of the industrial simulation). Signal cables connected to each other form a signal
    /// network; every tick each network carries the sum of what is connected to it: chest contents, tank and pipe
    /// levels ("fluid_water"), combinator outputs, computer outputs (signals.send). Machines next to a network can be
    /// switched by a condition ("iron_plate&lt;100"); combinators read the network behind them and write to the one in front,
    /// one tick later, so loops are well defined. Every setting is one short text, the same in the machine panel,
    /// programs (configure), SQL, multiplayer and saves.
    /// </summary>
    public static class SignalSpec
    {
        public const int MaxText = 200, MaxPairs = 16;
        public static readonly string[] Comparisons = { "<=", ">=", "!=", "<", ">", "=" };
        public static readonly string[] Operators = { "+", "-", "*", "/", "%" };

        /// <summary>Signal names: item ids (iron_plate), fluids (fluid_water), and the virtual A–Z and 0–9.</summary>
        public static bool IsName(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length > 40) return false;
            if (s.Length == 1) return (s[0] >= 'A' && s[0] <= 'Z') || (s[0] >= '0' && s[0] <= '9');
            if (s.StartsWith("fluid_")) return Fluids.Exists(s.Substring(6));
            return ItemDatabase.Exists(s);
        }

        public static string FluidSignal(string fluid) => fluid == Fluids.Water ? "fluid_water" : fluid == Fluids.Steam ? "fluid_steam" : fluid == Fluids.CrudeOil ? "fluid_crude_oil" : "fluid_" + fluid;

        static string Strip(string s)
        {
            var sb = new StringBuilder();
            foreach (char ch in s ?? "") if (!char.IsWhiteSpace(ch)) sb.Append(ch);
            return sb.ToString();
        }

        /// <summary>A number or a signal name; error text or null.</summary>
        static string Operand(string s, out string name, out int value)
        {
            name = null; value = 0;
            if (int.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value)) return null;
            if (IsName(s)) { name = s; return null; }
            return $"'{s}' is not a number or a signal";
        }

        static int Find(string s, string[] ops, out string op)
        {
            foreach (var o in ops)
            {
                int i = o == "-" ? s.IndexOf('-', 1) : s.IndexOf(o, StringComparison.Ordinal);   // a leading '-' is a sign
                if (o == "<" || o == ">") while (i >= 0 && i + 1 < s.Length && s[i + 1] == '=') i = s.IndexOf(o, i + 1, StringComparison.Ordinal);
                if (i > 0) { op = o; return i; }
            }
            op = null; return -1;
        }

        /// <summary>"iron_plate&lt;100", "A>=B". Empty text = no condition (returns null condition, no error).</summary>
        public static string ParseCondition(string text, out SignalCondition cond)
        {
            cond = null;
            string s = Strip(text);
            if (s.Length == 0) return null;
            if (s.Length > MaxText) return "too long";
            int i = Find(s, Comparisons, out var op);
            if (i < 0) return "a condition looks like iron_plate<100 (< > = != <= >=)";
            string l = s.Substring(0, i), r = s.Substring(i + op.Length);
            if (!IsName(l)) return $"'{l}' is not a signal";
            var err = Operand(r, out var rn, out var rv);
            if (err != null) return err;
            cond = new SignalCondition(l, op, rn, rv);
            return null;
        }

        /// <summary>"A=5,iron_plate=-3".</summary>
        public static string ParsePairs(string text, Dictionary<string, int> into)
        {
            into.Clear();
            string s = Strip(text);
            if (s.Length == 0) return null;
            if (s.Length > MaxText) return "too long";
            foreach (var part in s.Split(','))
            {
                int eq = part.IndexOf('=');
                if (eq <= 0) return $"'{part}': write name=value";
                string n = part.Substring(0, eq);
                if (!IsName(n)) return $"'{n}' is not a signal";
                if (!int.TryParse(part.Substring(eq + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int v)) return $"'{part}': the value must be a whole number";
                into[n] = into.TryGetValue(n, out var had) ? unchecked(had + v) : v;
                if (into.Count > MaxPairs) return $"at most {MaxPairs} signals";
            }
            return null;
        }

        public static string FormatPairs(IDictionary<string, int> d)
        {
            var keys = new List<string>(d.Keys); keys.Sort(StringComparer.Ordinal);
            var sb = new StringBuilder();
            foreach (var k in keys) { if (sb.Length > 0) sb.Append(','); sb.Append(k).Append('=').Append(d[k].ToString(CultureInfo.InvariantCulture)); }
            return sb.ToString();
        }

        public static int Apply(string op, int a, int b)
        {
            unchecked
            {
                switch (op)
                {
                    case "+": return a + b;
                    case "-": return a - b;
                    case "*": return a * b;
                    case "/": return b == 0 ? 0 : b == -1 ? -a : a / b;   // no exceptions: x/0 = 0, int.MinValue/-1 wraps
                    default: return b == 0 || b == -1 ? 0 : a % b;
                }
            }
        }

        internal static bool Compare(string op, int a, int b) =>
            op == "<" ? a < b : op == ">" ? a > b : op == "=" ? a == b : op == "!=" ? a != b : op == "<=" ? a <= b : a >= b;
    }

    /// <summary>A signal compared with a number or another signal.</summary>
    public sealed class SignalCondition
    {
        public readonly string Left, Op, RightName;
        public readonly int RightValue;
        public SignalCondition(string left, string op, string rightName, int rightValue) { Left = left; Op = op; RightName = rightName; RightValue = rightValue; }
        public string Text => Left + Op + (RightName ?? RightValue.ToString(CultureInfo.InvariantCulture));
        public bool Eval(Func<string, int> read) => SignalSpec.Compare(Op, read(Left), RightName != null ? read(RightName) : RightValue);
    }

    /// <summary>A signal cable: cables that touch form one network.</summary>
    public sealed class CableState : MachineState
    {
        internal Factory.SignalNet Net;
        internal string Loaded;   // a saved network's values (kept by its first cable)
        public IReadOnlyDictionary<string, int> Signals => Net != null ? (IReadOnlyDictionary<string, int>)Net.Current : Empty;
        static readonly Dictionary<string, int> Empty = new Dictionary<string, int>();
        public override string Status { get => Net == null || Net.Current.Count == 0 ? "no signals" : Net.Current.Count + " signals"; set { } }
        public override void SaveExtra(IDictionary<string, string> d)
        {
            if (Net == null) { if (!string.IsNullOrEmpty(Loaded)) d["sig"] = Loaded; return; }
            if (Net.Cables[0].State == this && Net.Current.Count > 0) d["sig"] = SignalSpec.FormatPairs(Net.Current);
        }
        public override void LoadExtra(string k, string v) { if (k == "sig") Loaded = v; }
    }

    /// <summary>Constant, arithmetic and decider combinators. Settings are one text (see <see cref="SignalSpec"/>).</summary>
    public sealed class CombinatorState : MachineState
    {
        public readonly BuildingKind Kind;
        public string Config { get; private set; } = "";
        public string LastError { get; private set; }
        internal readonly Dictionary<string, int> Constants = new Dictionary<string, int>();
        internal readonly Dictionary<string, int> Output = new Dictionary<string, int>();   // what it wrote last tick
        // arithmetic: Left (signal or EACH) Op Right -> Out (signal or EACH); decider: Cond -> Out (1, or the input value with :input)
        internal string Left, Op, RightName, Out; internal int RightValue; internal SignalCondition Cond; internal bool PassInput;

        public CombinatorState(BuildingKind kind) { Kind = kind; Status = "not set"; }
        public IReadOnlyDictionary<string, int> LastOutput => Output;

        /// <summary>Applies a setting; returns null or why it is not valid (the old setting stays).</summary>
        public string Configure(string text)
        {
            string s = text ?? "";
            if (s.Length > SignalSpec.MaxText) return LastError = "too long";
            string err;
            if (Kind == BuildingKind.ConstantCombinator)
            {
                var d = new Dictionary<string, int>();
                err = SignalSpec.ParsePairs(s, d);
                if (err == null) { Constants.Clear(); foreach (var kv in d) Constants[kv.Key] = kv.Value; Config = SignalSpec.FormatPairs(Constants); }
            }
            else err = Kind == BuildingKind.ArithmeticCombinator ? ParseArithmetic(s) : ParseDecider(s);
            LastError = err;
            if (err == null) { Status = Config.Length == 0 ? "not set" : Config; Touch(); }
            return err;
        }

        string ParseArithmetic(string text)
        {
            string s = Strip(text);
            if (s.Length == 0) { Left = null; Config = ""; return null; }
            int arrow = s.IndexOf("->", StringComparison.Ordinal);
            if (arrow < 0) return "write left op right->output, e.g. iron_plate*2->A or EACH/10->EACH";
            string expr = s.Substring(0, arrow), outp = s.Substring(arrow + 2);
            string op = null; int at = -1;
            for (int i = 1; i < expr.Length && at < 0; i++)   // the first operator; a '-' right after one is the sign of the number
                if ("+-*/%".IndexOf(expr[i]) >= 0 && "+-*/%".IndexOf(expr[i - 1]) < 0) { op = expr[i].ToString(); at = i; }
            if (at < 0) return "missing + - * / or %";
            string l = expr.Substring(0, at), r = expr.Substring(at + 1);
            bool each = l == "EACH";
            if (!each && !SignalSpec.IsName(l)) return $"'{l}' is not a signal (or EACH)";
            if (!int.TryParse(r, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int rv)) { if (!SignalSpec.IsName(r)) return $"'{r}' is not a number or a signal"; }
            else r = null;
            if (outp == "EACH" ? !each : !SignalSpec.IsName(outp)) return each ? "with EACH the output is a signal or EACH" : $"'{outp}' is not a signal";
            Left = l; Op = op; RightName = r; RightValue = r == null ? rv : 0; Out = outp;
            Config = l + op + (r ?? rv.ToString(CultureInfo.InvariantCulture)) + "->" + outp;
            return null;
        }

        string ParseDecider(string text)
        {
            string s = Strip(text);
            if (s.Length == 0) { Cond = null; Config = ""; return null; }
            int arrow = s.IndexOf("->", StringComparison.Ordinal);
            if (arrow < 0) return "write condition->output, e.g. iron_plate<100->A or A>5->B:input";
            var err = SignalSpec.ParseCondition(s.Substring(0, arrow), out var cond);
            if (err != null) return err;
            if (cond == null) return "missing condition";
            string outp = s.Substring(arrow + 2);
            bool pass = outp.EndsWith(":input", StringComparison.Ordinal);
            if (pass) outp = outp.Substring(0, outp.Length - 6);
            if (!SignalSpec.IsName(outp)) return $"'{outp}' is not a signal";
            Cond = cond; Out = outp; PassInput = pass;
            Config = cond.Text + "->" + outp + (pass ? ":input" : "");
            return null;
        }

        static string Strip(string s) { var sb = new StringBuilder(); foreach (char c in s) if (!char.IsWhiteSpace(c)) sb.Append(c); return sb.ToString(); }

        /// <summary>Works out this tick's output from the input network's values (read = the input's value of a signal).</summary>
        internal void Compute(Func<string, int> read, IEnumerable<KeyValuePair<string, int>> inputs)
        {
            Output.Clear();
            switch (Kind)
            {
                case BuildingKind.ConstantCombinator:
                    foreach (var kv in Constants) Output[kv.Key] = kv.Value;
                    break;
                case BuildingKind.ArithmeticCombinator:
                    if (Left == null) break;
                    if (Left == "EACH")
                    {
                        foreach (var kv in inputs)
                        {
                            int v = SignalSpec.Apply(Op, kv.Value, RightName != null ? read(RightName) : RightValue);
                            string name = Out == "EACH" ? kv.Key : Out;
                            Output[name] = Output.TryGetValue(name, out var had) ? unchecked(had + v) : v;
                        }
                    }
                    else Output[Out] = SignalSpec.Apply(Op, read(Left), RightName != null ? read(RightName) : RightValue);
                    break;
                default:
                    if (Cond != null && Cond.Eval(read)) Output[Out] = PassInput ? read(Out) : 1;
                    break;
            }
            foreach (var kv in Output) if (kv.Value == 0) zeros.Add(kv.Key);
            foreach (var k in zeros) Output.Remove(k);   // a zero signal is no signal
            zeros.Clear();
        }
        readonly List<string> zeros = new List<string>();

        public override void SaveExtra(IDictionary<string, string> d) { if (Config.Length > 0) d["cfg"] = Config; }
        public override void LoadExtra(string k, string v) { if (k == "cfg") Configure(v); }
    }

    /// <summary>A lamp: lights when powered and its condition (if it is on a network and has one) holds.</summary>
    public sealed class LampState : MachineState
    {
        public bool Lit;
        public override bool TakesCondition => true;
        public override void SaveExtra(IDictionary<string, string> d) { if (Lit) d["lit"] = "1"; }   // guests see it lit
        public override void LoadExtra(string k, string v) { if (k == "lit") Lit = v == "1"; }
    }

    public sealed partial class Factory
    {
        internal sealed class SignalNet
        {
            public readonly List<PlacedBuilding> Cables = new List<PlacedBuilding>();
            public Dictionary<string, int> Current = new Dictionary<string, int>(), Next = new Dictionary<string, int>();
            public readonly HashSet<FluidSegment> Counted = new HashSet<FluidSegment>();
        }

        sealed class SignalLink { public readonly List<SignalNet> In = new List<SignalNet>(), Out = new List<SignalNet>(); }

        readonly List<PlacedBuilding> cables = new List<PlacedBuilding>(), signalUsers = new List<PlacedBuilding>(), lamps = new List<PlacedBuilding>(),
            linkedUsers = new List<PlacedBuilding>();
        readonly List<SignalNet> signalNets = new List<SignalNet>();
        readonly Dictionary<int, SignalLink> links = new Dictionary<int, SignalLink>();
        readonly Dictionary<string, int> merged = new Dictionary<string, int>();
        bool signalDirty = true;

        static bool IsSignalUser(MachineState s) => s is StorageState || s is PipeState || s is CombinatorState || s is ComputerState || s is RoboportState || s is TrainStopState || s.TakesCondition;
        public const float LampKW = 5f;

        void SignalPlaced(PlacedBuilding b)
        {
            if (b.State is CableState) { signalDirty = true; return; }
            if (b.State != null && IsSignalUser(b.State) && !signalDirty && cables.Count > 0) Link(b);
        }

        void SignalRemoved(PlacedBuilding b)
        {
            if (b.State is CableState) { signalDirty = true; return; }
            if (links.Remove(b.InstanceId)) RemoveSorted(linkedUsers, b);
        }

        void RebuildSignals()
        {
            var keep = new Dictionary<PlacedBuilding, Dictionary<string, int>>();
            foreach (var c in cables) { var cs = (CableState)c.State; if (cs.Net != null) keep[c] = cs.Net.Current; cs.Net = null; }
            signalNets.Clear();
            var stack = new Stack<PlacedBuilding>();
            foreach (var start in cables)
            {
                if (((CableState)start.State).Net != null) continue;
                var net = new SignalNet();
                signalNets.Add(net);
                ((CableState)start.State).Net = net;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    var p = stack.Pop();
                    net.Cables.Add(p);
                    for (int r = 0; r < 4; r++)
                    {
                        var q = grid.At(p.Origin.Move((Rotation)r));
                        if (q?.State is CableState qs && qs.Net == null) { qs.Net = net; stack.Push(q); }
                    }
                }
                net.Cables.Sort(ById);
                // values carry over from the old network of its first cable (or from the save), so editing cables does not blank a network for a tick
                var head = net.Cables[0]; var hs = (CableState)head.State;
                if (keep.TryGetValue(head, out var old)) foreach (var kv in old) net.Current[kv.Key] = kv.Value;
                else if (!string.IsNullOrEmpty(hs.Loaded)) SignalSpec.ParsePairs(hs.Loaded, net.Current);
                foreach (var c in net.Cables) ((CableState)c.State).Loaded = null;
            }
            links.Clear(); linkedUsers.Clear();
            foreach (var u in signalUsers) { u.State.SignalAllows = true; Link(u); }
            signalDirty = false;
        }

        static void AddNet(List<SignalNet> l, SignalNet n) { if (n != null && !l.Contains(n)) l.Add(n); }

        void Link(PlacedBuilding b)
        {
            var link = new SignalLink();
            if (b.State is CombinatorState cb && cb.Kind != BuildingKind.ConstantCombinator)
            {
                foreach (var c in b.BackCells()) AddNet(link.In, (grid.At(c)?.State as CableState)?.Net);
                foreach (var c in b.FrontCells()) AddNet(link.Out, (grid.At(c)?.State as CableState)?.Net);
            }
            else
            {
                foreach (var c in Around(b)) AddNet(link.In, (grid.At(c)?.State as CableState)?.Net);
                link.Out.AddRange(link.In);
            }
            if (link.In.Count == 0 && link.Out.Count == 0) { b.State.SignalAllows = true; return; }
            links[b.InstanceId] = link;
            InsertSorted(linkedUsers, b);
        }

        static int Read(List<SignalNet> nets, string name)
        {
            int v = 0;
            foreach (var n in nets) if (n.Current.TryGetValue(name, out var x)) v = unchecked(v + x);
            return v;
        }

        static void Add(Dictionary<string, int> d, string k, int v)
        {
            if (v == 0) return;
            d[k] = d.TryGetValue(k, out var had) ? unchecked(had + v) : v;
        }

        /// <summary>Signals: every source writes into its networks; combinators read last tick's values; then conditions are checked.</summary>
        void TickSignals()
        {
            if (signalDirty) RebuildSignals();
            if (signalNets.Count > 0)
            {
                foreach (var n in signalNets) { n.Next.Clear(); n.Counted.Clear(); }
                foreach (var b in linkedUsers)
                {
                    var link = links[b.InstanceId];
                    switch (b.State)
                    {
                        case StorageState s:
                            for (int i = 0; i < s.Items.SlotCount; i++) if (!s.Items[i].IsEmpty) foreach (var n in link.Out) Add(n.Next, s.Items[i].ItemId, s.Items[i].Count);
                            break;
                        case PipeState p:
                            var seg = p.Segment;
                            if (seg?.Fluid == null) break;
                            foreach (var n in link.Out) if (n.Counted.Add(seg)) Add(n.Next, SignalSpec.FluidSignal(seg.Fluid), (int)seg.Amount);   // each segment once per network
                            break;
                        case TrainStopState tstop:   // a waiting train's cargo, and T = 1
                        {
                            var tr = tstop.Train;
                            if (tr == null) break;
                            foreach (var n in link.Out)
                            {
                                Add(n.Next, "T", 1);
                                for (int w = 0; w < tr.WagonCount; w++)
                                    for (int sl = 0; sl < TrainDepotState.WagonSlots; sl++)
                                        if (!tr.Wagons[w][sl].IsEmpty) Add(n.Next, tr.Wagons[w][sl].ItemId, tr.Wagons[w][sl].Count);
                            }
                            break;
                        }
                        case RoboportState _:
                            RoboportSignals(b, link.Out);
                            break;
                        case ComputerState pc:
                            foreach (var kv in pc.SignalOut) foreach (var n in link.Out) Add(n.Next, kv.Key, kv.Value);
                            break;
                        case CombinatorState cb:
                        {
                            merged.Clear();
                            if (cb.Kind == BuildingKind.ArithmeticCombinator && cb.Left == "EACH")
                                foreach (var n in link.In) foreach (var kv in n.Current) Add(merged, kv.Key, kv.Value);
                            var inNets = link.In;
                            cb.Compute(name => Read(inNets, name), merged);
                            foreach (var kv in cb.Output) foreach (var n in link.Out) Add(n.Next, kv.Key, kv.Value);
                            break;
                        }
                    }
                }
                foreach (var n in signalNets) { var t = n.Current; n.Current = n.Next; n.Next = t; }
                foreach (var b in linkedUsers)
                {
                    var st = b.State;
                    if (!st.TakesCondition) continue;
                    var inNets = links[b.InstanceId].In;
                    st.SignalAllows = st.Condition == null || st.Condition.Eval(name => Read(inNets, name));
                }
            }
            foreach (var b in lamps)
            {
                var l = (LampState)b.State;
                bool lit = l.SignalAllows && l.PowerSatisfaction > 0;
                if (lit != l.Lit || l.Status == "idle") { l.Lit = lit; l.Status = lit ? "lit" : l.PowerSatisfaction <= 0 ? "no power" : "off"; }
            }
        }

        /// <summary>What a building reads from the signal networks next to it (combinators: the one behind), for programs and the panel.</summary>
        public Dictionary<string, int> SignalsAt(PlacedBuilding b)
        {
            var d = new Dictionary<string, int>();
            if (b?.State is CableState cs) { if (cs.Net != null) foreach (var kv in cs.Net.Current) d[kv.Key] = kv.Value; return d; }
            if (b == null || !links.TryGetValue(b.InstanceId, out var link)) return d;
            foreach (var n in link.In) foreach (var kv in n.Current) Add(d, kv.Key, kv.Value);
            return d;
        }

        /// <summary>Is this building connected to a signal network?</summary>
        public bool OnSignalNetwork(PlacedBuilding b) => b != null && (b.State is CableState || links.ContainsKey(b.InstanceId));

        /// <summary>Applies a setting text to a combinator or a machine's condition. Returns null or why not.</summary>
        public static string Configure(PlacedBuilding b, string text)
        {
            if (b?.State == null) return "nothing there";
            if (text != null && text.Length > SignalSpec.MaxText) return "too long";
            if (b.State is CombinatorState cb) return cb.Configure(text);
            if (b.State is StorageState ss && ss.Role == LogisticRole.Requester) return ss.SetRequests(text);
            if (b.State is TrainDepotState td) return td.SetSchedule(text);
            if (b.State is ChemicalPlantState chem) return chem.SetRecipe(text);
            if (b.State is TrainStopState tsp) return tsp.Rename(text);
            if (b.State.TakesCondition) return b.State.SetCondition(text);
            return "this machine has no signal settings";
        }
    }
}
