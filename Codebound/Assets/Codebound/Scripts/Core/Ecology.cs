using System;
using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    /// <summary>
    /// Air pollution on a coarse grid of 16×16-cell regions. Burning coal, smelting, drilling and assembling emit;
    /// pollution drifts to neighbouring regions, fades slowly on its own, faster where trees grow, and air scrubbers
    /// clean it. Effects: smog over 120 makes the air unbreathable, greenhouses grow slower, and a dirty base draws
    /// more creatures. Stopping the emissions lets the land recover.
    /// </summary>
    public sealed class Ecology
    {
        public const int Region = 16;
        public const float GeneratorEmission = 1f, FurnaceEmission = 0.4f, DrillEmission = 0.2f, AssemblerEmission = 0.2f;  // per second
        public const float Spread = 0.05f, Decay = 0.004f, TreeAbsorb = 0.004f, ScrubberRate = 2f, ScrubberKW = 30f;        // per second
        public const float SmogLevel = 120f;

        readonly GameState game;
        Dictionary<(int, int), float> air = new Dictionary<(int, int), float>();
        readonly Dictionary<(int, int), (int trees, double at)> treeCache = new Dictionary<(int, int), (int, double)>();
        float step;
        double clock;

        public Ecology(GameState g) { game = g; }

        static int Div(int a) => (int)Math.Floor(a / (double)Region);
        public static (int, int) RegionOf(Cell c) => (Div(c.X), Div(c.Y));

        public float At(Cell c) => air.TryGetValue(RegionOf(c), out var v) ? v : 0f;
        public float Total { get { float t = 0; foreach (var v in air.Values) t += v; return t; } }
        public int PollutedRegions => air.Count;

        /// <summary>Greenhouse growth multiplier: down to 40% in heavy smog.</summary>
        public float GrowthFactor(Cell c) => 1f - Math.Min(0.6f, At(c) / 250f);
        public bool IsSmog(Cell c) => At(c) >= SmogLevel;

        public float EmissionOf(PlacedBuilding b)
        {
            bool busy = game.Factory.HasAction(b);
            switch (b.Def.Kind)
            {
                case BuildingKind.Generator: return ((GeneratorState)b.State).LoadKW / Factory.GeneratorKW * GeneratorEmission;
                case BuildingKind.Furnace: return busy ? FurnaceEmission : 0f;
                case BuildingKind.Drill: return busy ? DrillEmission : 0f;
                case BuildingKind.Assembler: return busy ? AssemblerEmission : 0f;
                default: return 0f;
            }
        }

        int Trees((int x, int y) r)
        {
            if (treeCache.TryGetValue(r, out var t) && clock - t.at < 60) return t.trees;   // trees change rarely: recount each minute
            int n = 0;
            for (int x = 0; x < Region; x++)
                for (int y = 0; y < Region; y++)
                    if (game.World.DepositAt(new Cell(r.x * Region + x, r.y * Region + y)).Type == ResourceType.Tree) n++;
            treeCache[r] = (n, clock);
            return n;
        }

        void Add((int, int) r, float v) { air.TryGetValue(r, out var cur); air[r] = Math.Max(0f, cur + v); }

        /// <summary>Advances the simulation; works in whole 1-second steps so the result does not depend on frame rate.</summary>
        public void Tick(float dt)
        {
            step += dt;
            while (step >= 1f) { step -= 1f; clock += 1; Step(); }
        }

        void Step()
        {
            foreach (var b in game.Grid.Buildings)
            {
                float e = EmissionOf(b);
                if (e > 0) Add(RegionOf(b.Origin), e);
                if (b.State is ScrubberState s)
                {
                    float rate = ScrubberRate * s.PowerSatisfaction;
                    var r = RegionOf(b.Origin);
                    air.TryGetValue(r, out var before);
                    Add(r, -rate);
                    s.Cleaned += before - (air.TryGetValue(r, out var after) ? after : 0f);
                    s.Status = s.PowerSatisfaction <= 0 ? "no power" : before > 0.01f ? "cleaning" : "air clean";
                }
            }

            var next = new Dictionary<(int, int), float>(air.Count * 2);
            foreach (var kv in air)
            {
                var (x, y) = kv.Key;
                float v = kv.Value, outFlow = v * Spread;
                v -= outFlow + v * Decay + TreeAbsorb * Trees(kv.Key);
                next.TryGetValue(kv.Key, out var own); next[kv.Key] = own + Math.Max(0f, v);
                foreach (var n in new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) })
                { next.TryGetValue(n, out var cur); next[n] = cur + outFlow / 4f; }
            }
            air = new Dictionary<(int, int), float>(next.Count);
            foreach (var kv in next) if (kv.Value >= 0.01f) air[kv.Key] = kv.Value;
        }

        /// <summary>Multiplayer guests mirror the host's air.</summary>
        public void CopyFrom(Ecology o) => air = new Dictionary<(int, int), float>(o.air);

        public IEnumerable<string> SaveLines()
        {
            foreach (var kv in air) yield return $"eco {kv.Key.Item1} {kv.Key.Item2} {kv.Value.ToString("R", CultureInfo.InvariantCulture)}";
        }

        public void Load(string[] p) =>
            air[(int.Parse(p[1], CultureInfo.InvariantCulture), int.Parse(p[2], CultureInfo.InvariantCulture))] = float.Parse(p[3], CultureInfo.InvariantCulture);
    }
}
