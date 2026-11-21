using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class PlantTests
    {
        static (GameState g, PlacedBuilding gh) With(string def)
        {
            var g = new GameState(9);
            var s = g.World.Generator.FindSpawn();
            var d = BuildingDatabase.Get(def);
            for (int r = 3; r < 150; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    if (!g.CanPlace(d, c, Rotation.R0) || !g.CanPlace(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y), Rotation.R0)) continue;
                    var gh = g.Grid.Place(d, c, Rotation.R0);
                    var gen = g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y), Rotation.R0);
                    ((GeneratorState)gen.State).Fuel.Add(ItemDatabase.Coal, 50);
                    return (g, gh);
                }
            Assert.Fail("no room"); return default;
        }

        static void Grow(GameState g, GreenhouseState s, float seconds)
        {
            for (float t = 0; t < seconds; t += 0.25f)
            {
                s.Tank = 100;
                for (int i = 0; i < GreenhouseState.Plots; i++) if (s.Growth[i] >= 0 && s.Growth[i] < 1) s.Soil[i] = 1;
                g.Factory.Tick(0.25f);
            }
        }

        [Test]
        public void Glowcaps_GrowInTheDark_FruitDoesNot()
        {
            var (g, gh) = With("greenhouse");
            var s = (GreenhouseState)gh.State;
            Assert.IsTrue(s.TryInsert(ItemDatabase.Glowcap, 1));
            Assert.IsTrue(s.TryInsert(ItemDatabase.AlienFruit, 1));
            Assert.IsFalse(s.TryInsert(ItemDatabase.Stone, 1));
            Assert.AreEqual(0, g.Factory.Greenhouse(gh, "plant", 0.0, "glowcap").value);
            Assert.AreEqual(1, g.Factory.Greenhouse(gh, "plant", 1.0, "fruit").value);
            Assert.AreEqual("unknown_species", g.Factory.Greenhouse(gh, "plant", 2.0, "cactus").error);
            g.Factory.Daylight = 0f;
            // no grow lamps either: drain the generator
            foreach (var b in g.Grid.Buildings) if (b.State is GeneratorState gen) { gen.Fuel.Clear(); gen.BurnLeftKJ = 0; }
            Grow(g, s, 250f);
            Assert.AreEqual(1f, s.Growth[0], 1e-3, "glowcap ripe in the dark");
            Assert.Less(s.Growth[1], 0.05f, "fruit needs light");
            Assert.AreEqual(2, g.Factory.Greenhouse(gh, "harvest", 0.0).value);
            Assert.AreEqual(2, s.Output.Count(ItemDatabase.Glowcap));
        }

        [Test]
        public void Breeding_KeepsTheBestStrain_AndVigorRaisesYield()
        {
            var (g, gh) = With("greenhouse");
            var s = (GreenhouseState)gh.State;
            g.Factory.Daylight = 1f;
            for (int gen = 0; gen < 12; gen++)
            {
                s.Seeds.Add(ItemDatabase.Fiber, 1);
                Assert.IsNull(g.Factory.Greenhouse(gh, "plant", 0.0, "fiber").error);
                Grow(g, s, 260f);
                Assert.IsNull(g.Factory.Greenhouse(gh, "harvest", 0.0).error);
                s.Output.Clear();
            }
            Assert.Greater(s.StrainOf("fiber"), 1.05f, "selection improved the strain");
            Assert.LessOrEqual(s.StrainOf("fiber"), Plants.MaxGene);
            Assert.Greater(Plants.YieldOf(Plants.Get("fiber"), 1.5f), Plants.YieldOf(Plants.Get("fiber"), 1f));

            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            var l = (GreenhouseState)SaveSystem.Read(new System.IO.StringReader(sw.ToString())).Grid.At(gh.Origin).State;
            Assert.AreEqual(s.StrainOf("fiber"), l.StrainOf("fiber"), 1e-4);
            Assert.AreEqual(s.Harvests, l.Harvests);
        }

        [Test]
        public void Hydroponics_GrowsUnderLampsAtNight_FasterThanAGreenhouse()
        {
            var (g, bay) = With("hydroponics");
            var s = (GreenhouseState)bay.State;
            g.Factory.Daylight = 0f;
            s.Seeds.Add(ItemDatabase.AlienFruit, 1);
            g.Factory.Greenhouse(bay, "plant", 0.0, "fruit");
            Grow(g, s, 130f);   // 180 s / 1.5 = 120 s
            Assert.AreEqual(1f, s.Growth[0], 1e-3);
        }

        [Test]
        public void Rations_AreCraftedAndFeedMoreThanFruit()
        {
            var r = AssemblerState.Recipes[ItemDatabase.Ration];
            Assert.AreEqual(2, r.Amount);
            var v = new Vitals { Hunger = 10, Health = 40 };
            v.Eat(ItemDatabase.Ration);
            Assert.AreEqual(65, v.Hunger, 1e-3);
            v.Eat(ItemDatabase.Glowcap);
            Assert.AreEqual(75, v.Health, 1e-3, "ration +10, glowcap +25");
        }
    }
}
