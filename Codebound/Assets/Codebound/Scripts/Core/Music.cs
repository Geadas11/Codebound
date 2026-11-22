using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Codebound's soundtrack, composed as notes and rendered by small synthesizer instruments (no audio assets).
    /// One 16-bar piece in A minor at 96 BPM (sections A and B, ~40 s, loops seamlessly), arranged as four layers of the
    /// same length and harmony so the game can mix them freely: day (melody, arpeggio, pad), night (sparse bells),
    /// industry (bass and drums) and danger (drone, timpani, tremolo strings).
    /// </summary>
    public static class Music
    {
        public const double Bpm = 96, Beat = 60.0 / Bpm, Bar = Beat * 4;
        public const int Bars = 16;
        public static readonly string[] Layers = { "mus_calm", "mus_night", "mus_industry", "mus_danger" };
        public static bool Has(string name) => Array.IndexOf(Layers, name) >= 0;

        // the progression: root (MIDI, octave 3) and whether the chord is minor
        static readonly (int root, bool minor)[] Chords =
        {
            (57, true), (53, false), (48, false), (55, false), (57, true), (53, false), (55, false), (52, false),   // A: Am F C G Am F G E
            (53, false), (55, false), (52, true), (57, true), (50, true), (53, false), (55, false), (52, false),    // B: F G Em Am Dm F G E
        };

        // the melody in eighth notes: MIDI note, H = hold the previous note, R = rest
        const int H = -2, R = -1;
        static readonly int[][] Melody =
        {
            new[] { 76, H, H, 74, 72, H, 69, H }, new[] { 72, H, H, 74, 76, H, 77, H }, new[] { 79, H, H, 77, 76, H, 72, H }, new[] { 74, H, H, H, 71, H, 74, H },
            new[] { 76, H, H, 74, 72, H, 69, H }, new[] { 72, H, 74, H, 76, H, 81, H }, new[] { 79, H, 77, H, 74, H, 71, H }, new[] { 68, H, H, H, H, H, R, R },
            new[] { 81, H, H, 79, 77, H, 76, H }, new[] { 74, H, H, 76, 79, H, H, H }, new[] { 76, H, H, 74, 71, H, 67, H }, new[] { 69, H, H, H, 72, H, 76, H },
            new[] { 77, H, H, 76, 74, H, 72, H }, new[] { 72, H, 74, H, 77, H, 81, H }, new[] { 83, H, 81, H, 79, H, 74, H }, new[] { 80, H, H, H, 76, H, 71, H },
        };

        static int[] Triad((int root, bool minor) c) => new[] { c.root, c.root + (c.minor ? 3 : 4), c.root + 7 };
        static double Hz(double midi) => 440.0 * Math.Pow(2, (midi - 69) / 12.0);

        struct Note { public double Start, Length, Freq, Vel; public Func<double, double, double, double> Voice; }

        // ---- instruments: (time since the note started, frequency, note length) → sample
        static double S(double f, double t) => Math.Sin(2 * Math.PI * f * t);
        static double Env(double t, double len, double a, double r) => Math.Min(1, t / a) * (t < len ? 1 : Math.Max(0, 1 - (t - len) / r));
        static double Pad(double t, double f, double len) => Env(t, len, 0.6, 0.8) * (0.5 * Tri(f, t) + 0.35 * Tri(f * 1.004, t) + 0.15 * S(f * 2, t));
        static double Pluck(double t, double f, double len) => Math.Exp(-t * 5) * (S(f, t) + 0.3 * S(2 * f, t) + 0.1 * S(3 * f, t)) * Math.Min(1, t / 0.004);
        static double Lead(double t, double f, double len)
        {
            double vib = 1 + 0.004 * Math.Sin(2 * Math.PI * 5 * t) * Math.Min(1, Math.Max(0, (t - 0.2) / 0.3));
            double env = Math.Min(1, t / 0.03) * (0.75 + 0.25 * Math.Exp(-t * 4)) * (t < len ? 1 : Math.Max(0, 1 - (t - len) / 0.15));
            return env * (S(f * vib, t) + 0.18 * S(2 * f * vib, t) + 0.05 * S(3 * f, t));
        }
        static double Bell(double t, double f, double len) => Math.Min(1, t / 0.003) * (Math.Exp(-t * 2.2) * S(f, t) + 0.5 * Math.Exp(-t * 3.5) * S(f * 2.76, t) + 0.25 * Math.Exp(-t * 6) * S(f * 5.4, t));
        static double Bass(double t, double f, double len) => Env(t, len, 0.01, 0.08) * (0.75 + 0.25 * Math.Exp(-t * 6)) * (S(f, t) + 0.5 * S(2 * f, t) + 0.3 * S(3 * f, t) + 0.15 * S(4 * f, t));
        static double Kick(double t, double f, double len) => Math.Exp(-t * 9) * Math.Sin(2 * Math.PI * (48 * t + 2.6 * (1 - Math.Exp(-30 * t))));
        static double Tom(double t, double f, double len) => Math.Exp(-t * 4) * Math.Sin(2 * Math.PI * (f * t + 1.5 * (1 - Math.Exp(-20 * t))));
        static double Hat(double t, double f, double len) => Math.Exp(-t * 70) * Noise(t * 22050 + f);
        static double Strings(double t, double f, double len) => Env(t, len, 1.2, 1.0) * (0.65 + 0.35 * Math.Sin(2 * Math.PI * 9 * t)) * (S(f, t) + 0.5 * S(2 * f * 1.002, t) + 0.33 * S(3 * f, t) + 0.25 * S(4 * f, t));
        static double Drone(double t, double f, double len) => Env(t, len, 1.5, 1.5) * (S(f, t) + 0.6 * S(f * 1.5, t) + 0.3 * S(f * 2.01, t)) * (0.7 + 0.3 * Math.Sin(2 * Math.PI * 0.25 * t));
        static double Tri(double f, double t) { double p = t * f % 1.0; return 4 * Math.Abs(p - 0.5) - 1; }
        static double Noise(double i) { uint x = (uint)(long)i * 2654435761u; x ^= x >> 13; x *= 1274126177u; return ((x >> 8) & 0xffff) / 32768.0 - 1; }

        static List<Note> Score(string layer)
        {
            var n = new List<Note>();
            void Add(double start, double len, double midi, double vel, Func<double, double, double, double> v) => n.Add(new Note { Start = start, Length = len, Freq = Hz(midi), Vel = vel, Voice = v });
            for (int bar = 0; bar < Bars; bar++)
            {
                double t0 = bar * Bar;
                var c = Chords[bar]; var tri = Triad(c);
                bool b = bar >= 8;   // second section: fuller
                switch (layer)
                {
                    case "mus_calm":
                        foreach (int m in tri) Add(t0, Bar, m + 12, 0.10, Pad);
                        for (int i = 0; i < 8; i++)   // arpeggio in eighths: up and down the chord
                        {
                            int[] order = { 0, 1, 2, 1, 0, 1, 2, 1 };
                            Add(t0 + i * Beat / 2, Beat / 2, tri[order[i]] + 24, b ? 0.13 : 0.10, Pluck);
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            int m = Melody[bar][i];
                            if (m < 0) continue;
                            int len = 1; while (i + len < 8 && Melody[bar][i + len] == H) len++;
                            Add(t0 + i * Beat / 2, len * Beat / 2, m, 0.22, Lead);
                        }
                        break;
                    case "mus_night":
                        foreach (int m in tri) Add(t0, Bar, m, 0.08, Pad);
                        Add(t0, Bar, tri[0] + 24, 0.2, Bell);                              // one bell per bar
                        if (bar % 2 == 1) Add(t0 + Beat * 2.5, Beat, Melody[bar][0] > 0 ? Melody[bar][0] + 12 : tri[2] + 24, 0.12, Bell);   // and an echo of the tune
                        break;
                    case "mus_industry":
                        for (int i = 0; i < 8; i++)
                        {
                            int m = (i % 4 == 2 ? tri[2] : tri[0]) - 12;
                            Add(t0 + i * Beat / 2, Beat / 2 * 0.9, m, i % 2 == 0 ? 0.32 : 0.22, Bass);
                        }
                        for (int i = 0; i < 4; i++) Add(t0 + i * Beat, 0.4, 0, i % 2 == 0 ? 0.55 : (b ? 0.4 : 0), Kick);
                        for (int i = 0; i < 16; i++) Add(t0 + i * Beat / 4, 0.05, i % 8, i % 4 == 2 ? 0.14 : 0.07, Hat);   // the "frequency" only seeds the noise
                        break;
                    case "mus_danger":
                        Add(t0, Bar, tri[0] - 12, 0.22, Drone);
                        foreach (int m in tri) Add(t0, Bar, m + 12, 0.07, Strings);
                        Add(t0, 0.8, tri[0] - 12, 0.4, Tom);                                // timpani on 1 and the "and" of 3
                        Add(t0 + Beat * 2.5, 0.8, tri[0] - 12, 0.3, Tom);
                        if (b) Add(t0 + Beat * 3.5, 0.5, tri[2] - 12, 0.25, Tom);
                        break;
                }
            }
            return n;
        }

        public static double Seconds => Bars * Bar;

        /// <summary>Renders one layer: notes summed into a loop of exactly Bars bars (tails wrap to the start), a soft echo on
        /// the melodic layers, normalised to 0.8 peak.</summary>
        public static float[] Render(string layer, int rate)
        {
            int len = (int)Math.Round(Seconds * rate);
            var buf = new double[len];
            foreach (var note in Score(layer))
            {
                if (note.Vel <= 0) continue;
                int s0 = (int)(note.Start * rate);
                int count = (int)((note.Length + 2.0) * rate);   // room for releases and decays
                for (int i = 0; i < count; i++)
                {
                    double t = i / (double)rate;
                    double v = note.Voice(t, note.Freq, note.Length);
                    if (t > note.Length + 0.5 && Math.Abs(v) < 1e-4) break;
                    buf[(s0 + i) % len] += note.Vel * v;
                }
            }
            if (layer == "mus_calm" || layer == "mus_night")   // echo of three eighths, twice around so the loop seam has it too
            {
                int d = (int)(Beat * 1.5 * rate);
                for (int pass = 0; pass < 2; pass++) for (int i = 0; i < len; i++) buf[i] += 0.28 * buf[(i - d + len) % len] * (pass == 0 ? 1 : 0.3);
            }
            double peak = 1e-9; foreach (var v in buf) peak = Math.Max(peak, Math.Abs(v));
            var o = new float[len];
            for (int i = 0; i < len; i++) o[i] = (float)(buf[i] * 0.8 / peak);
            return o;
        }
    }
}
