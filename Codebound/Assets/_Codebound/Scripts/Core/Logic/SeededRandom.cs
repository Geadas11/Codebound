using System;

namespace Codebound.Core
{
    /// <summary>
    /// Small deterministic PRNG (SplitMix64). Same seed => same sequence on every platform,
    /// which keeps procedural worlds identical for shared seeds and multiplayer clients.
    /// </summary>
    public sealed class SeededRandom
    {
        private ulong _state;

        public SeededRandom(long seed) { _state = unchecked((ulong)seed); }

        public ulong NextULong()
        {
            unchecked
            {
                _state += 0x9E3779B97F4A7C15UL;
                ulong z = _state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        /// <summary>Uniform double in [0, 1).</summary>
        public double NextDouble() => (NextULong() >> 11) * (1.0 / (1UL << 53));

        /// <summary>Uniform int in [min, maxExclusive).</summary>
        public int Range(int min, int maxExclusive)
        {
            if (maxExclusive <= min) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            return min + (int)(NextULong() % (ulong)(maxExclusive - min));
        }

        /// <summary>Stateless hash of (seed, x, y) to [0,1). Used for coordinate-based noise.</summary>
        public static double Hash01(long seed, int x, int y)
        {
            unchecked
            {
                ulong h = (ulong)seed ^ ((ulong)(uint)x * 0x9E3779B185EBCA87UL) ^ ((ulong)(uint)y * 0xC2B2AE3D27D4EB4FUL);
                h ^= h >> 33; h *= 0xFF51AFD7ED558CCDUL;
                h ^= h >> 33; h *= 0xC4CEB9FE1A85EC53UL;
                h ^= h >> 33;
                return (h >> 11) * (1.0 / (1UL << 53));
            }
        }
    }
}
