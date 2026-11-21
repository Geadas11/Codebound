using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Procedural audio: every Codebound sound is synthesized here (oscillators, noise, envelopes, filters),
    /// so there are no third-party audio assets or licences. Mono float samples in [-1, 1].
    /// shortcut: simple synthesis until the Etapa 8 sound pass; recipe names are the stable contract.
    /// </summary>
    public static class Synth
    {
        public const int Rate = 22050;

        public sealed class Recipe { public string Name; public float Seconds; public bool Loop; public Func<double, Random, double> Wave; public float LowPass = 1f; }

        static double Sin(double f, double t) => Math.Sin(2 * Math.PI * f * t);
        static double Tri(double f, double t) { double p = t * f % 1.0; return 4 * Math.Abs(p - 0.5) - 1; }
        static double Saw(double f, double t) => 2 * (t * f % 1.0) - 1;
        static double Sq(double f, double t) => t * f % 1.0 < 0.5 ? 1 : -1;
        static double Dec(double t, double k) => Math.Exp(-t * k);                 // exponential decay envelope
        static double Pulse(double t, double period, double k) => Dec(t % period, k); // repeating decay
        static double Midi(int n) => 440.0 * Math.Pow(2, (n - 69) / 12.0);

        // a fixed pseudo-random "hit" pattern: true if a click starts in this small window
        static double Crackle(double t, Random r, double density) => r.NextDouble() < density / Rate ? (r.NextDouble() * 2 - 1) : 0;

        static readonly int[] Chords = { 57, 53, 48, 55 };   // A2 F2 C2 G2 roots, 4 s each → 16 s loop

        public static readonly Dictionary<string, Recipe> Recipes = new Dictionary<string, Recipe>();

        static void R(string name, float seconds, bool loop, Func<double, Random, double> wave, float lowPass = 1f) =>
            Recipes[name] = new Recipe { Name = name, Seconds = seconds, Loop = loop, Wave = wave, LowPass = lowPass };

        static Synth()
        {
            // ---- machines (loops; frequencies chosen to complete whole cycles in the loop length)
            R("drill", 1f, true, (t, r) => 0.5 * Saw(55, t) * (0.6 + 0.4 * Sin(8, t)) + 0.4 * Pulse(t, 0.125, 60) * (r.NextDouble() * 2 - 1), 0.25f);
            R("generator", 2f, true, (t, r) => 0.5 * Sin(50, t) + 0.25 * Sin(100, t) + 0.12 * Sin(150, t) + 0.08 * (r.NextDouble() * 2 - 1), 0.3f);
            R("furnace", 2f, true, (t, r) => 0.25 * (r.NextDouble() * 2 - 1) * (0.5 + 0.5 * Sin(1, t)) + 2.5 * Crackle(t, r, 30), 0.35f);
            R("assembler", 1f, true, (t, r) => Pulse(t, 0.5, 25) * (0.5 * Sq(400, t) + 0.3 * Sq(600, t)) + 0.1 * Sin(100, t), 0.5f);
            R("lab", 2f, true, (t, r) => 0.25 * Sin(t % 1 < 0.5 ? 880 : 1320, t) * Pulse(t, 0.5, 6) + 0.1 * Sin(220, t), 0.6f);
            R("computer", 2f, true, (t, r) => 0.25 * (r.NextDouble() * 2 - 1) + (t % 2 < 0.06 || (t % 2 > 1.2 && t % 2 < 1.24) ? 0.4 * Sq(1200, t) : 0), 0.3f);
            R("conveyor", 1f, true, (t, r) => 0.3 * (r.NextDouble() * 2 - 1) * (0.6 + 0.4 * Sin(4, t)) + 0.15 * Saw(110, t), 0.2f);
            R("greenhouse", 2f, true, (t, r) => 0.15 * (r.NextDouble() * 2 - 1) + 0.08 * Sin(1.5, t) * Sin(440, t), 0.1f);
            R("turret", 2f, true, (t, r) => 0.2 * Sin(60, t) + 0.05 * Sin(3000, t) * Pulse(t, 1, 30), 0.5f);

            // ---- ambience (loops)
            R("amb_forest", 8f, true, (t, r) => 0.35 * (r.NextDouble() * 2 - 1) * (0.5 + 0.5 * Sin(0.25, t))
                + ((t % 2.7) < 0.12 ? 0.25 * Sin(2400 + 1600 * (t % 2.7) / 0.12, t) * Dec(t % 2.7, 25) : 0), 0.12f);
            R("amb_desert", 8f, true, (t, r) => 0.5 * (r.NextDouble() * 2 - 1) * (0.4 + 0.6 * Math.Abs(Sin(0.125, t))), 0.08f);
            R("amb_volcanic", 8f, true, (t, r) => 0.5 * (r.NextDouble() * 2 - 1) + 0.4 * Sin(30, t) * (0.6 + 0.4 * Sin(0.5, t)), 0.04f);
            R("amb_radioactive", 8f, true, (t, r) => 0.1 * (r.NextDouble() * 2 - 1) + 3.0 * Crackle(t, r, 25), 0.8f);
            R("amb_ruins", 8f, true, (t, r) => 0.25 * Sin(60, t) + 0.1 * Sin(120, t) + ((t % 4) < 1 ? 0.2 * Sin(1760, t) * Dec(t % 4, 4) : 0), 0.4f);
            R("rain", 4f, true, (t, r) => 0.6 * (r.NextDouble() * 2 - 1) + 1.5 * Crackle(t, r, 120), 0.5f);
            R("storm", 4f, true, (t, r) => 0.7 * (r.NextDouble() * 2 - 1) * (0.6 + 0.4 * Sin(0.5, t)) + 0.4 * Sin(35, t), 0.15f);

            // ---- adaptive music layers (16 s loops in A minor: Am F C G)
            R("mus_calm", 16f, true, (t, r) =>
            {
                int root = Chords[(int)(t / 4) % 4]; double env = Math.Min(1, (t % 4) / 1.5) * Math.Min(1, (4 - t % 4) / 0.5);
                return env * (0.3 * Tri(Midi(root + 12), t) + 0.2 * Tri(Midi(root + 16 - (root == 57 ? 1 : 0)), t) + 0.2 * Tri(Midi(root + 19), t));
            }, 0.15f);
            R("mus_industry", 16f, true, (t, r) =>
            {
                int root = Chords[(int)(t / 4) % 4];
                return 0.6 * Saw(Midi(root - 12), t) * Pulse(t, 0.25, 14) + 0.25 * (r.NextDouble() * 2 - 1) * Pulse(t + 0.125, 0.5, 40);
            }, 0.2f);
            R("mus_danger", 16f, true, (t, r) => 0.4 * Saw(Midi(33), t) * (0.6 + 0.4 * Sin(0.25, t)) + 0.25 * Sin(Midi(46), t) * Math.Abs(Sin(0.0625, t)) + 0.3 * Pulse(t, 0.5, 30) * Sin(Midi(33), t), 0.12f);

            // ---- one-shots
            R("mine", 0.15f, false, (t, r) => Dec(t, 30) * (0.6 * (r.NextDouble() * 2 - 1) + 0.5 * Sin(120, t)), 0.4f);
            R("place", 0.25f, false, (t, r) => Dec(t, 18) * (0.6 * Sin(180, t) + 0.3 * (r.NextDouble() * 2 - 1)) + (t > 0.08 ? 0.3 * Dec(t - 0.08, 60) * Sq(900, t) : 0), 0.5f);
            R("remove", 0.25f, false, (t, r) => Dec(t, 14) * 0.6 * Sin(260 - 500 * t, t), 0.6f);
            R("eat", 0.3f, false, (t, r) => Pulse(t, 0.1, 40) * (r.NextDouble() * 2 - 1) * 0.7, 0.3f);
            R("drink", 0.4f, false, (t, r) => Pulse(t, 0.13, 25) * 0.5 * Sin(300 + 900 * (t % 0.13), t), 0.6f);
            R("shot", 0.2f, false, (t, r) => Dec(t, 12) * 0.5 * Sq(1800 - 7000 * t, t), 0.5f);
            R("dissolve", 0.6f, false, (t, r) => Dec(t, 5) * (0.4 * (r.NextDouble() * 2 - 1) + 0.3 * Sin(900 - 1200 * t, t)), 0.3f);
            R("thunder", 2.2f, false, (t, r) => (0.9 * Dec(t, 3) + 0.3 * Dec(Math.Max(0, t - 0.35), 2)) * (r.NextDouble() * 2 - 1), 0.08f);
            R("alarm", 1.2f, false, (t, r) => 0.45 * Sq(t % 0.6 < 0.3 ? 880 : 660, t) * Math.Min(1, (1.2 - t) * 10), 0.4f);
            R("chime", 0.8f, false, (t, r) => 0.4 * Dec(t, 4) * Sin(1046.5, t) + (t > 0.15 ? 0.4 * Dec(t - 0.15, 4) * Sin(1318.5, t) : 0), 1f);
            R("beep", 0.15f, false, (t, r) => 0.35 * Sin(1000, t) * Math.Min(1, (0.15 - t) * 40), 1f);
            R("error", 0.3f, false, (t, r) => 0.4 * Sq(110, t) * Math.Min(1, (0.3 - t) * 20), 0.3f);
            R("click", 0.05f, false, (t, r) => Dec(t, 120) * Sq(2000, t) * 0.4, 1f);
            R("hurt", 0.25f, false, (t, r) => Dec(t, 10) * 0.5 * Saw(160 - 300 * t, t), 0.4f);
        }

        /// <summary>Renders a recipe: one-pole low-pass, loops cross-faded at the seam, normalised to 0.8 peak.</summary>
        public static float[] Render(string name)
        {
            var rec = Recipes[name];
            int n = Math.Max(1, (int)(rec.Seconds * Rate));
            var data = new float[n];
            int seed = 17; foreach (char ch in name) seed = seed * 31 + ch;   // stable across runtimes (string.GetHashCode is not)
            var rng = new Random(seed & 0x7fffffff);
            double y = 0;
            for (int i = 0; i < n; i++)
            {
                double x = rec.Wave(i / (double)Rate, rng);
                y += (x - y) * rec.LowPass;
                data[i] = (float)y;
            }
            if (rec.Loop)
            {
                // cross-fade the last 20 ms into the first 20 ms so the loop has no click
                int fade = Math.Min(n / 4, Rate / 50);
                for (int i = 0; i < fade; i++)
                {
                    float k = i / (float)fade;
                    data[i] = data[i] * k + data[n - fade + i] * (1 - k);
                }
                Array.Resize(ref data, n - fade);
            }
            else
            {
                int fade = Math.Min(n / 4, Rate / 200);   // 5 ms fade-out avoids a click at the end
                for (int i = 0; i < fade; i++) data[n - 1 - i] *= i / (float)fade;
            }
            float peak = 1e-6f;
            foreach (var v in data) peak = Math.Max(peak, Math.Abs(v));
            float g = 0.8f / peak;
            for (int i = 0; i < data.Length; i++) data[i] *= g;
            return data;
        }
    }
}
