using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class SurvivalTests
    {
        static readonly Surroundings Mild = new Surroundings { AmbientC = 20, Breathable = true };

        static void Run(Vitals v, Surroundings env, float seconds) { for (float t = 0; t < seconds; t += 0.1f) v.Tick(0.1f, env); }

        [Test]
        public void HungerAndThirst_DecaySlowly_NotPunishing()
        {
            var v = new Vitals();
            Run(v, Mild, 600); // 10 minutes
            Assert.AreEqual(75f, v.Hunger, 0.5f);
            Assert.AreEqual(60f, v.Thirst, 0.5f);
            Assert.AreEqual(100f, v.Health, 0.01f, "healthy player loses no health");
            Assert.AreEqual("", v.Warning);
        }

        [Test]
        public void UnbreathableAir_DrainsSuitOxygen_ThenHurts_AndRefillsOutside()
        {
            var v = new Vitals();
            var ash = new Surroundings { AmbientC = 20, Breathable = false };
            Run(v, ash, 150);
            Assert.AreEqual(25f, v.Oxygen, 0.5f);
            Assert.AreEqual("OXYGEN LOW", v.Warning);
            Run(v, ash, 60);
            Assert.AreEqual(0f, v.Oxygen);
            Assert.Less(v.Health, 80f, "suffocating hurts fast");
            Assert.AreEqual("NO OXYGEN", v.Warning);
            Run(v, Mild, 15);
            Assert.AreEqual(100f, v.Oxygen, 0.01f);
        }

        [Test]
        public void Heat_RaisesBodyTemperature_AndDamages()
        {
            var v = new Vitals();
            Run(v, new Surroundings { AmbientC = 80, Breathable = true }, 300); // next to lava
            Assert.Greater(v.BodyTemp, Vitals.HotLimit);
            Assert.Less(v.Health, 100f);
            Assert.AreEqual("OVERHEATING", v.Warning);
            var desert = new Vitals();
            Run(desert, new Surroundings { AmbientC = Vitals.BiomeTemperature(Biome.Desert), Breathable = true }, 600);
            Assert.LessOrEqual(desert.BodyTemp, Vitals.HotLimit, "the suit copes with the desert");
        }

        [Test]
        public void Starving_Hurts_EatingAndDrinkingHelp_DeathAndRespawn()
        {
            var v = new Vitals { Hunger = 0, Thirst = 0 };
            Run(v, Mild, 10);
            Assert.AreEqual(100f - 0.6f * 10, v.Health, 0.2f);
            Assert.IsTrue(v.Eat(ItemDatabase.AlienFruit));
            Assert.IsFalse(v.Eat(ItemDatabase.Stone), "stone is not food");
            Assert.AreEqual(18f, v.Hunger, 0.01f);
            v.Drink(false); Assert.Greater(v.Thirst, 5f);
            float h = v.Health; v.Drink(true); Assert.Less(v.Health, h, "toxic water hurts");
            v.Health = 0.1f; v.Hunger = 0;
            Run(v, Mild, 1);
            Assert.IsTrue(v.Dead);
            v.Respawn();
            Assert.IsFalse(v.Dead); Assert.AreEqual(60f, v.Health); Assert.GreaterOrEqual(v.Hunger, 50f);
        }

        [Test]
        public void Environment_FollowsBiomes()
        {
            var g = new WorldGenerator(42);
            bool sawUnbreathable = false, sawRadiation = false;
            for (int x = -3000; x < 3000 && !(sawUnbreathable && sawRadiation); x += 37)
                for (int y = -3000; y < 3000; y += 37)
                {
                    var c = new Cell(x, y);
                    var env = Vitals.At(g, c);
                    var b = g.BiomeAt(c);
                    Assert.AreEqual(b != Biome.Volcanic && b != Biome.Radioactive, env.Breathable);
                    sawUnbreathable |= !env.Breathable; sawRadiation |= env.Radiation > 0;
                }
            Assert.IsTrue(sawUnbreathable && sawRadiation);
            Assert.IsTrue(Vitals.At(g, g.FindSpawn()).Breathable, "spawn is safe");
        }

        [Test]
        public void Vitals_AreSaved()
        {
            var g = new GameState(4);
            g.Vitals.Health = 42.5f; g.Vitals.Oxygen = 10; g.Vitals.BodyTemp = 38.25f;
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var g2 = SaveSystem.Read(new StringReader(sw.ToString()));
            Assert.AreEqual(42.5f, g2.Vitals.Health); Assert.AreEqual(10f, g2.Vitals.Oxygen); Assert.AreEqual(38.25f, g2.Vitals.BodyTemp);
        }
    }
}
