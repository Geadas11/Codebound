using System;
using System.Linq;

namespace Codebound.Core
{
    /// <summary>
    /// The guided first missions (Etapa 8): from gathering wood to a program that mines on its own and the first research.
    /// Each step checks the world, so it also completes when the player does things in another order.
    /// Saves older than the tutorial load with it finished.
    /// </summary>
    public sealed class Tutorial
    {
        public sealed class Step
        {
            public string Id, Title, Hint;
            public Func<GameState, Tutorial, bool> Done;
        }

        public static readonly Step[] Steps =
        {
            new Step { Id = "wood", Title = "Gather wood", Hint = "Hold the left mouse button on a tree until you have 10 wood.",
                       Done = (g, t) => g.Inventory.Count(ItemDatabase.Wood) >= 10 },
            new Step { Id = "stone", Title = "Gather stone", Hint = "Mine grey rock the same way: 10 stone builds your first machines.",
                       Done = (g, t) => g.Inventory.Count(ItemDatabase.Stone) >= 10 },
            new Step { Id = "drill", Title = "Build a drill on ore", Hint = "Press B for build mode, pick the Electric Drill and place it on an ore patch (iron, copper or coal).",
                       Done = (g, t) => Any(g, BuildingKind.Drill) },
            new Step { Id = "power", Title = "Make electricity", Hint = "Build a Coal Generator within 3 cells of the drill (or connect them with Power Poles) and put coal in it with E.",
                       Done = (g, t) => g.Grid.Buildings.Any(b => b.State is GeneratorState gs && (gs.BurnLeftKJ > 0 || gs.Fuel.Count(ItemDatabase.Coal) > 0)) },
            new Step { Id = "computer", Title = "Build a computer", Hint = "Programs control machines within 8 cells: build a Computer next to the drill and give it power too.",
                       Done = (g, t) => g.Grid.Buildings.Any(b => b.State is ComputerState && g.Factory.MachinesNear(b).Any(m => m.Def.Kind == BuildingKind.Drill)) },
            new Step { Id = "program", Title = "Run your first program", Hint = "Press E on the computer, pick the \"Drill loop\" example, read it and press Run.",
                       Done = (g, t) => g.Grid.Buildings.Any(b => b.State is ComputerState cs && (cs.Running || cs.History.Count > 0)) },
            new Step { Id = "auto", Title = "Let the program work", Hint = "Watch the drill: your program is mining for you. Wait for the first ore it mines on its own.",
                       Done = (g, t) => t.AutoMined },
            new Step { Id = "research", Title = "Research something", Hint = "Press T for the research tree. Build a Lab near a computer, feed it the materials and start a research.",
                       Done = (g, t) => g.Research.Done.Count > 0 },
        };

        public int Index;           // current step; Steps.Length = finished
        public bool AutoMined;      // a drill finished an action (drills only act when a program commands them)
        public bool Finished => Index >= Steps.Length;
        public Step Current => Finished ? null : Steps[Index];
        public event Action<Step> StepDone;

        static bool Any(GameState g, BuildingKind k) => g.Grid.Buildings.Any(b => b.Def.Kind == k);

        public static string Title(Step s) => Loc.T("tut." + s.Id + ".title", s.Title);
        public static string Hint(Step s) => Loc.T("tut." + s.Id + ".hint", s.Hint);

        /// <summary>Advances past every step already satisfied (call about once a second).</summary>
        public void Update(GameState g)
        {
            while (!Finished && Steps[Index].Done(g, this)) { var s = Steps[Index]; Index++; StepDone?.Invoke(s); }
        }

        public void Skip() => Index = Steps.Length;

        public string SaveLine() => "tutorial " + Index + (AutoMined ? " 1" : " 0");
        public void Load(string[] p)
        {
            Index = Math.Max(0, Math.Min(Steps.Length, int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture)));
            AutoMined = p.Length > 2 && p[2] == "1";
        }
    }
}
