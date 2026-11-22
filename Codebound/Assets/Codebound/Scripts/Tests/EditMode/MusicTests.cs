using System;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class MusicTests
    {
        [Test]
        public void Layers_AreTheSameLength_Audible_NotClipping_AndLoopSeamlessly()
        {
            int len = -1;
            foreach (var layer in Music.Layers)
            {
                var d = Synth.Render(layer);
                if (len < 0) len = d.Length;
                Assert.AreEqual(len, d.Length, layer + ": every layer has the same length, so they stay in sync");
                Assert.AreEqual(Music.Seconds * Synth.Rate, d.Length, 1, layer);
                double sum = 0; float peak = 0;
                foreach (var v in d) { Assert.IsFalse(float.IsNaN(v), layer); sum += v * v; peak = Math.Max(peak, Math.Abs(v)); }
                Assert.LessOrEqual(peak, 0.81f, layer + " clips");
                Assert.Greater(Math.Sqrt(sum / d.Length), 0.02, layer + " is (almost) silent");
                Assert.Less(Math.Abs(d[d.Length - 1] - d[0]), 0.2f, layer + " loop seam clicks");
            }
        }

        [Test]
        public void Music_IsDeterministic() => CollectionAssert.AreEqual(Synth.Render("mus_night"), Synth.Render("mus_night"));
    }
}
