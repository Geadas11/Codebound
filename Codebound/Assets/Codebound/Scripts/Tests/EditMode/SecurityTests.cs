using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class SecurityTests
    {
        [Test]
        public void Password_IsSaltedHash_CheckedAndSaved()
        {
            var cs = new ComputerState();
            Assert.IsTrue(cs.CheckPassword("anything"), "no password: open");
            cs.SetPassword("hunter2");
            Assert.IsTrue(cs.HasPassword);
            StringAssert.DoesNotContain("hunter2", cs.PasswordHash);
            StringAssert.StartsWith("pbkdf2$", cs.PasswordHash);
            Assert.IsTrue(cs.CheckPassword("hunter2"));
            Assert.IsFalse(cs.CheckPassword("hunter3"));
            Assert.IsFalse(cs.CheckPassword(null));
            var other = new ComputerState(); other.SetPassword("hunter2");
            Assert.AreNotEqual(cs.PasswordHash, other.PasswordHash, "random salt");

            var g = new GameState(1);
            var s = g.World.Generator.FindSpawn();
            PlacedBuilding pc = null;
            for (int x = 1; pc == null && x < 40; x++) pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(s.X + x, s.Y), Rotation.R0);
            ((ComputerState)pc.State).SetPassword("hunter2");
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            StringAssert.DoesNotContain("hunter2", sw.ToString());
            var loaded = (ComputerState)SaveSystem.Read(new StringReader(sw.ToString())).Grid.At(pc.Origin).State;
            Assert.IsTrue(loaded.CheckPassword("hunter2"));
        }

        [Test]
        public void FirewallAndPassword_SlowDrones_AndIntrusionIsReported()
        {
            var g = new GameState(1);
            var cs = new ComputerState();
            Assert.AreEqual(CreatureSystem.HackSeconds, g.Creatures.HackTime(cs));
            cs.SetPassword("x");
            Assert.AreEqual(CreatureSystem.HackSeconds * 2, g.Creatures.HackTime(cs));
            g.Research.Complete(Research.Get("cybersecurity"));
            Assert.AreEqual(CreatureSystem.HackSeconds * 6, g.Creatures.HackTime(cs));

            var s = g.World.Generator.FindSpawn();
            PlacedBuilding pc = null;
            for (int x = 1; pc == null && x < 40; x++) pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(s.X + x, s.Y), Rotation.R0);
            PlacedBuilding attacked = null; g.Creatures.IntrusionStarted += (b, c) => attacked = b;
            bool compromised = false; g.Creatures.ComputerCompromised += _ => compromised = true;
            g.Creatures.Spawn(CreatureType.Drone, pc.CenterX + 6, pc.CenterY);
            for (float t = 0; t < 10; t += 0.05f) g.Creatures.Tick(0.05f, s.X + 25, s.Y + 25);
            Assert.AreSame(pc, attacked, "intrusion reported when the hack starts");
            Assert.IsFalse(compromised, "firewall alone: 9 s, not reached yet");
            for (float t = 0; t < 10; t += 0.05f) g.Creatures.Tick(0.05f, s.X + 25, s.Y + 25);
            Assert.IsTrue(compromised);
        }

        [Test]
        public void Repair_CostsStone_AndCapsAtMax()
        {
            var g = new GameState(1);
            var s = g.World.Generator.FindSpawn();
            PlacedBuilding wall = null;
            for (int x = 1; wall == null && x < 40; x++) wall = g.Grid.Place(BuildingDatabase.Get("wall"), new Cell(s.X + x, s.Y), Rotation.R0);
            Assert.AreEqual("not damaged", g.Repair(wall));
            wall.Health = 370;
            Assert.AreEqual("needs stone", g.Repair(wall));
            g.Inventory.Add(ItemDatabase.Stone, 5);
            Assert.IsNull(g.Repair(wall)); Assert.AreEqual(390f, wall.Health);
            Assert.IsNull(g.Repair(wall)); Assert.AreEqual(400f, wall.Health, "capped");
            Assert.AreEqual(3, g.Inventory.Count(ItemDatabase.Stone));
        }
    }
}
