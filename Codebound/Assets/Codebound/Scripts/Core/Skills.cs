using System;
using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>
    /// The player gets better at what they do: mining (faster by hand), combat (more damage), engineering (repairs go
    /// further). Level = √(xp / 20), up to 10; each level is worth 6% (mining, combat) or 10% (engineering).
    /// </summary>
    public sealed class Skills
    {
        public static readonly string[] Names = { "mining", "combat", "engineering" };
        public const int MaxLevel = 10;
        readonly Dictionary<string, int> xp = new Dictionary<string, int>();

        public event Action<string, int> LeveledUp;

        public int Xp(string skill) => xp.TryGetValue(skill, out var v) ? v : 0;
        public int Level(string skill) => Math.Min(MaxLevel, (int)Math.Floor(Math.Sqrt(Xp(skill) / 20.0)));

        public void Add(string skill, int amount = 1)
        {
            if (Array.IndexOf(Names, skill) < 0 || amount <= 0) return;
            int before = Level(skill);
            xp[skill] = Xp(skill) + amount;
            int after = Level(skill);
            if (after > before) LeveledUp?.Invoke(skill, after);
        }

        public float MiningSpeed => 1f + 0.06f * Level("mining");
        public float CombatDamage => 1f + 0.06f * Level("combat");
        public float RepairBonus => 1f + 0.1f * Level("engineering");

        public void CopyFrom(Skills o) { xp.Clear(); foreach (var kv in o.xp) xp[kv.Key] = kv.Value; }

        public string SaveLine() => "skills " + string.Join(" ", Array.ConvertAll(Names, n => Xp(n).ToString(CultureInfo.InvariantCulture)));
        public void Load(string[] p) { for (int i = 0; i < Names.Length && i + 1 < p.Length; i++) xp[Names[i]] = int.Parse(p[i + 1], CultureInfo.InvariantCulture); }
    }
}
