using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class ShelterTests
    {
        static void Run(GameState g, float s) { for (float t = 0; t < s; t += 0.05f) g.Factory.Tick(0.05f); }

        static (GameState g, Cell c) Area()
        {
            var g = new GameState(9);
            var s = g.World.Generator.FindSpawn();
            var wall = BuildingDatabase.Get("wall");
            for (int r = 3; r < 150; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    bool ok = true;
                    for (int dx = -4; dx <= 4 && ok; dx++) for (int dy = -4; dy <= 4 && ok; dy++) ok = g.CanPlace(wall, new Cell(c.X + dx, c.Y + dy), Rotation.R0);
                    if (ok) return (g, c);
                }
            Assert.Fail("no room"); return default;
        }

        [Test]
        public void Heater_WarmsWithPower_WallsShelterFromStorms()
        {
            var (g, c) = Area();
            var cold = new Surroundings { AmbientC = -10, Breathable = false, Weather = Weather.Storm, Rain = true, Description = "Forest" };
            g.Grid.Place(BuildingDatabase.Get("heater"), new Cell(c.X + 2, c.Y), Rotation.R0);
            Run(g, 0.1f);
            Assert.AreEqual(-10, Shelter.Apply(g, c, cold).AmbientC, 1e-3, "no power: no heat");
            var gen = g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y + 2), Rotation.R0);
            ((GeneratorState)gen.State).Fuel.Add(ItemDatabase.Coal, 10);
            Run(g, 0.5f);
            var warm = Shelter.Apply(g, c, cold);
            Assert.AreEqual(Shelter.HeaterC, warm.AmbientC, 1e-3);
            Assert.IsTrue(warm.Rain, "a heater is not a roof");

            foreach (var r in new[] { Rotation.R0, Rotation.R90, Rotation.R180, Rotation.R270 })
                if (g.Grid.At(c.Move(r, 1)) == null) g.Grid.Place(BuildingDatabase.Get("wall"), c.Move(r, 1), Rotation.R0);
            Assert.IsTrue(Shelter.Enclosed(g, c));
            var inside = Shelter.Apply(g, c, cold);
            Assert.IsFalse(inside.Rain);
            Assert.AreEqual(g.World.Generator.BiomeAt(c) != Biome.Volcanic && g.World.Generator.BiomeAt(c) != Biome.Radioactive, inside.Breathable);
            StringAssert.Contains("sheltered", inside.Description);
        }

        [Test]
        public void Lightning_DamagesBuildings_RodsTakeTheStrikeAndChargeBatteries()
        {
            var (g, c) = Area();
            var chest = g.Grid.Place(BuildingDatabase.Get("chest"), c, Rotation.R0);
            float dmg = g.Lightning.Strike(c.X + 0.5, c.Y + 0.5, c.X + 0.5, c.Y + 1.0);
            Assert.AreEqual(Lightning.PlayerDamage, dmg, "standing next to the strike hurts");
            Assert.AreEqual(chest.Def.MaxHealth - Lightning.BuildingDamage, chest.Health, 1e-3);

            var rod = g.Grid.Place(BuildingDatabase.Get("lightning_rod"), new Cell(c.X + 3, c.Y), Rotation.R0);
            var bat = (BatteryState)g.Grid.Place(BuildingDatabase.Get("battery"), new Cell(c.X + 4, c.Y), Rotation.R0).State;
            bool hitRod = false; g.Lightning.Struck += (x, y, r) => hitRod = r;
            float before = chest.Health;
            Assert.AreEqual(0f, g.Lightning.Strike(c.X + 0.5, c.Y + 0.5, c.X + 0.5, c.Y + 1.0));
            Assert.IsTrue(hitRod);
            Assert.AreEqual(before, chest.Health, 1e-3, "the rod protected the chest");
            Assert.AreEqual(Lightning.RodChargeKJ, bat.StoredKJ, 1e-3);
            Assert.AreEqual(1, ((LightningRodState)rod.State).Strikes);
        }
    }
}
