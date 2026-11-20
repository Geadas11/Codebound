using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class CreatureTests
    {
        static GameState World() => new GameState(42);

        static PlacedBuilding Place(GameState g, string id, Cell near, int minDist = 0)
        {
            for (int r = minDist; r < 80; r++)
                for (int x = -r; x <= r; x++)
                    for (int y = -r; y <= r; y++)
                    {
                        if (System.Math.Max(System.Math.Abs(x), System.Math.Abs(y)) != r) continue;
                        var b = g.Grid.Place(BuildingDatabase.Get(id), new Cell(near.X + x, near.Y + y), Rotation.R0);
                        if (b != null) return b;
                    }
            Assert.Fail("no room for " + id); return null;
        }

        static void Tick(GameState g, float seconds, double px = double.NaN, double py = double.NaN)
        {
            if (double.IsNaN(px)) { var s = g.World.Generator.FindSpawn(); px = s.X + 25; py = s.Y + 25; }
            for (float t = 0; t < seconds; t += 0.05f) { g.Factory.Tick(0.05f); g.Creatures.Tick(0.05f, px, py); }
        }

        [Test]
        public void QuietBase_IsLeftAlone_NoisyBase_AttractsWaves()
        {
            var g = World();
            var s = g.World.Generator.FindSpawn();
            Place(g, "chest", s);
            Tick(g, 300, s.X, s.Y);
            Assert.AreEqual(0, g.Creatures.All.Count, "no industry, no attention");
            Assert.AreEqual(0f, g.Creatures.Attention);

            var gen = Place(g, "generator", s);
            gen.State.TryInsert(ItemDatabase.Coal, 50);
            var pc = Place(g, "computer", new Cell(gen.Origin.X + 2, gen.Origin.Y));
            ((ComputerState)pc.State).Running = true;          // draws power → generator works → noise
            Tick(g, 1);
            Assert.Greater(g.Creatures.Noise(), 0f, "a loaded generator makes noise");
            int waves = 0; g.Creatures.WaveArrived += n => waves++;
            g.Creatures.Attention = CreatureSystem.AttentionPerWave + 1;
            Tick(g, 0.1f);
            Assert.AreEqual(1, waves, "enough attention brings a wave");
            Assert.Greater(g.Creatures.All.Count, 0);
        }

        [Test]
        public void Crawler_WalksToBuilding_DamagesAndDestroysIt()
        {
            var g = World();
            var s = g.World.Generator.FindSpawn();
            var chest = Place(g, "chest", s);
            PlacedBuilding destroyed = null; g.Creatures.BuildingDestroyed += b => destroyed = b;
            var c = g.Creatures.Spawn(CreatureType.Crawler, chest.CenterX + 6, chest.CenterY);
            Tick(g, 3);
            Assert.Less(System.Math.Abs(c.X - chest.CenterX) + System.Math.Abs(c.Y - chest.CenterY), 6, "moved closer");
            Tick(g, 20);
            Assert.AreSame(chest, destroyed);
            Assert.IsNull(g.Grid.ById(chest.InstanceId));
        }

        [Test]
        public void Crawler_ChasesNearbyPlayer_AndHurts()
        {
            var g = World();
            var s = g.World.Generator.FindSpawn();
            g.Creatures.Spawn(CreatureType.Crawler, s.X + 3.5, s.Y + 0.5);
            Tick(g, 5, s.X + 0.5, s.Y + 0.5);
            Assert.Less(g.Vitals.Health, 100f);
        }

        [Test]
        public void Drone_CompromisesComputer()
        {
            var g = World();
            var s = g.World.Generator.FindSpawn();
            var pc = Place(g, "computer", s);
            var cs = (ComputerState)pc.State; cs.Running = true;
            PlacedBuilding hacked = null; g.Creatures.ComputerCompromised += b => hacked = b;
            g.Creatures.Spawn(CreatureType.Drone, pc.CenterX + 8, pc.CenterY);
            Tick(g, 10);
            Assert.AreSame(pc, hacked);
            Assert.IsTrue(cs.Compromised); Assert.IsFalse(cs.Running);
            Assert.AreEqual(0, g.Creatures.All.Count, "the drone shuts down after uploading itself");
        }

        [Test]
        public void Turret_NeedsPower_RespectsRangeAndCooldown_Kills()
        {
            var g = World();
            var s = g.World.Generator.FindSpawn();
            var turret = Place(g, "turret", s);
            var c = g.Creatures.Spawn(CreatureType.Crawler, turret.CenterX + 4, turret.CenterY);
            Assert.AreEqual("no_power", g.Creatures.Fire(turret, c.Id, 0));
            var gen = Place(g, "generator", s, 1);
            gen.State.TryInsert(ItemDatabase.Coal, 10);
            Assert.AreEqual(1, g.Creatures.InRange(turret).Count);
            Assert.IsNull(g.Creatures.Fire(turret, c.Id, 0));
            Assert.AreEqual("cooldown", g.Creatures.Fire(turret, c.Id, 0.1));
            Assert.IsNull(g.Creatures.Fire(turret, c.Id, 1));
            Assert.IsFalse(c.Alive, "40 HP, 25 per shot: two shots");
            Assert.AreEqual("no_such_target", g.Creatures.Fire(turret, c.Id, 2));
            int died = 0; g.Creatures.Died += _ => died++;
            g.Creatures.Tick(0.05f, s.X + 25, s.Y + 25);
            Assert.AreEqual(1, died);
            var far = g.Creatures.Spawn(CreatureType.Crawler, turret.CenterX + 30, turret.CenterY);
            Assert.AreEqual("out_of_range", g.Creatures.Fire(turret, far.Id, 5));
        }

        [Test]
        public void DamagedBuildings_AndCompromise_AreSaved()
        {
            var g = World();
            var s = g.World.Generator.FindSpawn();
            var wall = Place(g, "wall", s);
            var pc = Place(g, "computer", s, 2);
            wall.Health = 123.5f; ((ComputerState)pc.State).Compromised = true;
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var g2 = SaveSystem.Read(new StringReader(sw.ToString()));
            Assert.AreEqual(123.5f, g2.Grid.At(wall.Origin).Health);
            Assert.AreEqual(400f, BuildingDatabase.Get("wall").MaxHealth);
            Assert.IsTrue(((ComputerState)g2.Grid.At(pc.Origin).State).Compromised);
        }
    }
}
