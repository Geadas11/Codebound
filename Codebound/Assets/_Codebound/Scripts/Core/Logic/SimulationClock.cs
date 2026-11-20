using System;

namespace Codebound.Core
{
    /// <summary>
    /// Fixed-rate simulation clock. Real (frame) time is accumulated and converted into
    /// whole simulation ticks so machines, energy and program scheduling are frame-rate independent
    /// and deterministic (needed for multiplayer later).
    /// </summary>
    public sealed class SimulationClock
    {
        public const int DefaultTickRate = 20;
        /// <summary>Upper bound of ticks processed per Advance call, to avoid a spiral of death after a hitch.</summary>
        public const int MaxTicksPerAdvance = 10;

        private double _accumulator;

        public SimulationClock(int ticksPerSecond = DefaultTickRate)
        {
            if (ticksPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(ticksPerSecond));
            TicksPerSecond = ticksPerSecond;
        }

        public int TicksPerSecond { get; }
        public double TickDuration => 1.0 / TicksPerSecond;
        public long CurrentTick { get; private set; }
        public bool Paused { get; set; }

        /// <summary>Raised once per simulation tick with the new tick number.</summary>
        public event Action<long> Ticked;

        /// <summary>Feeds elapsed real seconds; runs as many whole ticks as are due. Returns ticks run.</summary>
        public int Advance(double deltaSeconds)
        {
            if (Paused || deltaSeconds <= 0) return 0;
            _accumulator += deltaSeconds;
            int ran = 0;
            while (_accumulator >= TickDuration && ran < MaxTicksPerAdvance)
            {
                _accumulator -= TickDuration;
                CurrentTick++;
                ran++;
                Ticked?.Invoke(CurrentTick);
            }
            if (ran == MaxTicksPerAdvance && _accumulator > TickDuration) _accumulator = 0; // drop backlog
            return ran;
        }
    }
}
