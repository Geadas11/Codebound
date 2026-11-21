using System.Collections.Generic;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class EquipmentTests
    {
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
        public void ShockTrap_StunsAndHurtsCrawlers_RunsOutAndRecharges()
        {
            var (g, c) = Area();
            var trap = g.Grid.Place(BuildingDatabase.Get("shock_trap"), c, Rotation.R0);
            var st = (ShockTrapState)trap.State;
            Assert.IsTrue(trap.Def.Walkable);
            var crawler = g.Creatures.Spawn(CreatureType.Crawler, c.X + 0.5, c.Y + 0.5);
            float hp = crawler.Health;
            g.Creatures.Tick(0.05f, c.X + 50, c.Y + 50);
            Assert.AreEqual(hp - ShockTrapState.Damage, crawler.Health, 1e-3);
            Assert.AreEqual(ShockTrapState.StunSeconds, crawler.Stunned, 1e-3);
            Assert.AreEqual(2, st.Charges);
            st.Charges = 0;
            Assert.IsFalse(st.TryInsert(ItemDatabase.Stone, 1));
            Assert.IsTrue(st.TryInsert(ItemDatabase.CopperWire, 1));
            Assert.AreEqual(ShockTrapState.MaxCharges, st.Charges);
        }

        [Test]
        public void Armor_HalvesCreatureDamage_ThermalLinerTemperatesTheSuit_ReinforcedWallIsTough()
        {
            var g = new GameState(1);
            Assert.AreEqual(1f, Equipment.DamageTaken(g.Inventory));
            g.Inventory.Add(ItemDatabase.ArmorPlating, 1);
            Assert.AreEqual(0.5f, Equipment.DamageTaken(g.Inventory));
            var cold = new Surroundings { AmbientC = -18 };
            Assert.AreEqual(-18f, Equipment.Apply(g.Inventory, cold).AmbientC, 1e-3);
            g.Inventory.Add(ItemDatabase.ThermalSuit, 1);
            Assert.AreEqual(2f, Equipment.Apply(g.Inventory, cold).AmbientC, 1e-3);
            Assert.AreEqual(1000f, BuildingDatabase.Get("reinforced_wall").MaxHealth);
            Assert.AreEqual("materials", Research.UnlockerOf("reinforced_wall").Id);
            Assert.IsTrue(AssemblerState.Recipes.ContainsKey(ItemDatabase.LaserTool));
        }

        static Dictionary<string, object> A(params object[] kv)
        {
            var d = new Dictionary<string, object>();
            for (int i = 0; i < kv.Length; i += 2) d[(string)kv[i]] = kv[i + 1] is int n ? (double)n : kv[i + 1];
            return d;
        }

        [Test]
        public void Multiplayer_GuestsRemoveOnlyWhatTheyBuilt_UnlessAdmin()
        {
            var (g, c) = Area();
            var ana = g.GuestInventory("ana"); ana.Add(ItemDatabase.Wood, 40);
            var rui = g.GuestInventory("rui");
            double px = c.X + 0.5, py = c.Y - 1;
            Assert.IsNull(Multiplayer.Act(g, Multiplayer.Builder, ana, px, py, A("a", "build", "def", "chest", "x", c.X, "y", c.Y, "rot", 0), "ana"));
            Assert.AreEqual("ana", g.Grid.At(c).Owner);
            StringAssert.Contains("only remove what you built", Multiplayer.Act(g, Multiplayer.Builder, rui, px, py, A("a", "remove", "x", c.X, "y", c.Y), "rui"));
            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            Assert.AreEqual("ana", SaveSystem.Read(new System.IO.StringReader(sw.ToString())).Grid.At(c).Owner, "owner saved");
            Assert.IsNull(Multiplayer.Act(g, Multiplayer.Admin, rui, px, py, A("a", "remove", "x", c.X, "y", c.Y), "rui"));
            Assert.IsNull(g.Grid.At(c));
        }
    }
}
