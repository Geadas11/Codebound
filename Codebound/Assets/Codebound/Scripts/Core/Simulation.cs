using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Codebound.Core
{
    /// <summary>
    /// One system of the fixed simulation tick. Prepare runs first (sequential setup: rebuild what changed, reset
    /// per-tick values), Update does the work, Finish applies deferred results in a fixed order (completed machine
    /// actions, events). Keeping the three apart is what would let a system's Update run on several threads later
    /// without changing results: it may only write its own state, and Finish applies the rest in order.
    /// </summary>
    public sealed class SimSystem
    {
        public readonly string Name;
        public readonly Action Prepare, Update, Finish;
        public double LastMs, AverageMs;   // filled only while Simulation.Profile is on

        public SimSystem(string name, Action update, Action prepare = null, Action finish = null)
        {
            Name = name; Update = update; Prepare = prepare; Finish = finish;
        }
    }

    /// <summary>
    /// The fixed 20 Hz simulation step: named systems in an explicit, documented order (see TECHNICAL_ARCHITECTURE.md).
    /// The same inputs per tick give the same state (tested by DeterminismTests).
    /// Parallel updates are deliberately not used: a 2000-building base costs ~1 ms of the 50 ms tick (measured),
    /// so threads would add risk without a measured gain.
    /// </summary>
    public sealed class Simulation
    {
        public const float TickSeconds = 0.05f;

        readonly List<SimSystem> systems = new List<SimSystem>();
        public IReadOnlyList<SimSystem> Systems => systems;
        public bool Profile;

        public void Add(SimSystem s) => systems.Add(s);

        /// <summary>A fingerprint of everything the save keeps (FNV-1a 64 of the canonical save text): equal hashes mean
        /// equal worlds. Used to catch divergence between runs and between "saved and reloaded" and "kept running".</summary>
        public static ulong StateHash(GameState g)
        {
            var w = new System.IO.StringWriter();
            SaveSystem.Write(g, w);
            ulong h = 14695981039346656037UL;
            foreach (char c in w.ToString()) { h ^= c; h *= 1099511628211UL; }
            return h;
        }

        /// <summary>Runs every system once: each one's Prepare, Update and Finish before the next system starts,
        /// because later systems read what earlier ones produced (power before machines, machines before belts...).</summary>
        public void Run()
        {
            foreach (var s in systems)
            {
                long t0 = Profile ? Stopwatch.GetTimestamp() : 0;
                s.Prepare?.Invoke();
                s.Update();
                s.Finish?.Invoke();
                if (!Profile) continue;
                s.LastMs = (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;
                s.AverageMs = s.AverageMs <= 0 ? s.LastMs : s.AverageMs * 0.95 + s.LastMs * 0.05;
            }
        }
    }
}
