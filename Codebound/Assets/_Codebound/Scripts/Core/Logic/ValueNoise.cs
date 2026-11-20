using System;

namespace Codebound.Core
{
    /// <summary>Deterministic 2D value noise with fractal octaves. Output roughly in [0,1].</summary>
    public static class ValueNoise
    {
        public static double Sample(long seed, double x, double y)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            double tx = Smooth(x - x0), ty = Smooth(y - y0);
            double a = SeededRandom.Hash01(seed, x0, y0);
            double b = SeededRandom.Hash01(seed, x0 + 1, y0);
            double c = SeededRandom.Hash01(seed, x0, y0 + 1);
            double d = SeededRandom.Hash01(seed, x0 + 1, y0 + 1);
            double top = a + (b - a) * tx;
            double bottom = c + (d - c) * tx;
            return top + (bottom - top) * ty;
        }

        public static double Fractal(long seed, double x, double y, int octaves = 4, double persistence = 0.5)
        {
            double sum = 0, amp = 1, norm = 0, freq = 1;
            for (int i = 0; i < octaves; i++)
            {
                sum += Sample(seed + i * 1013, x * freq, y * freq) * amp;
                norm += amp;
                amp *= persistence;
                freq *= 2;
            }
            return sum / norm;
        }

        private static double Smooth(double t) => t * t * (3 - 2 * t);
    }
}
