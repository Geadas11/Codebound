using System;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class SynthTests
    {
        [Test]
        public void EveryRecipe_IsAudible_NotClipping_AndLoopsAreSeamless()
        {
            Assert.Greater(Synth.Recipes.Count, 25);
            foreach (var r in Synth.Recipes.Values)
            {
                var d = Synth.Render(r.Name);
                Assert.Greater(d.Length, Synth.Rate * r.Seconds * 0.9f, r.Name + " length");
                double sum = 0; float peak = 0;
                foreach (var v in d) { sum += v * v; peak = Math.Max(peak, Math.Abs(v)); Assert.IsFalse(float.IsNaN(v), r.Name + " NaN"); }
                Assert.LessOrEqual(peak, 0.81f, r.Name + " clips");
                Assert.Greater(Math.Sqrt(sum / d.Length), 0.01, r.Name + " is (almost) silent");
                if (r.Loop) Assert.Less(Math.Abs(d[d.Length - 1] - d[0]), 0.25f, r.Name + " loop seam clicks");
                else Assert.Less(Math.Abs(d[d.Length - 1]), 0.05f, r.Name + " ends with a click");
            }
        }

        [Test]
        public void Rendering_IsDeterministic()
        {
            CollectionAssert.AreEqual(Synth.Render("rain"), Synth.Render("rain"));
        }
    }
}
