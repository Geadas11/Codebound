using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class CaveTests
    {
        static (GameState g, Cell mouth) WithMouth()
        {
            var g = new GameState(21);
            var s = g.World.Generator.FindSpawn();
            var m = Caves.Near(g.World.Generator, s, 600).OrderBy(c => System.Math.Abs(c.X - s.X) + System.Math.Abs(c.Y - s.Y)).FirstOrDefault();
            Assert.IsTrue(Caves.IsPortal(g.World.Generator, m), "a cave mouth near the spawn");
            return (g, m);
        }

        [Test]
        public void CaveMouths_AreDeterministic_OnDryLand_AndNotBuildable()
        {
            var (g, m) = WithMouth();
            Assert.IsTrue(Caves.IsPortal(new GameState(21).World.Generator, m));
            Assert.IsTrue(WorldRules.IsWalkable(g.World.Generator.TileAt(m)));
            Assert.IsFalse(g.CanPlace(BuildingDatabase.Get("wall"), m, Rotation.R0));
        }

        [Test]
        public void GoingDown_LeadsToADarkRichCave_WithTheLadderUnderfoot_AndBack()
        {
            var (g, m) = WithMouth();
            g.Inventory.Add(ItemDatabase.IronPlate, 9);
            g.Research.Complete(Research.Get("logistics"));
            g.Skills.Add("mining", 100);
            var under = Caves.Pass(g, m, null);
            Assert.AreEqual(PlanetKind.Underground, under.PlanetKind);
            Assert.AreEqual("home-under", under.PlanetId);
            Assert.AreEqual(9, under.Inventory.Count(ItemDatabase.IronPlate));
            Assert.IsTrue(under.Research.IsDone("logistics"));
            Assert.AreEqual(g.Skills.Level("mining"), under.Skills.Level("mining"));
            Assert.IsTrue(Caves.IsPortal(under.World.Generator, m), "the ladder is right there");
            for (int dx = -2; dx <= 2; dx++) for (int dy = -2; dy <= 2; dy++)
                Assert.IsTrue(WorldRules.IsWalkable(under.World.Generator.TileAt(new Cell(m.X + dx, m.Y + dy))), "open space around the ladder");
            Assert.AreEqual(Biome.Cavern, under.World.Generator.BiomeAt(m));
            Assert.AreEqual(Weather.Clear, under.Clock.WeatherAt(Biome.Cavern));
            Assert.AreEqual(Caves.CaveC, Vitals.At(under.World.Generator, m, under.Clock).AmbientC, 1e-3);

            int walls = 0, ore = 0, crystals = 0;
            for (int x = -150; x < 150; x += 3) for (int y = -150; y < 150; y += 3)
            {
                var c = new Cell(m.X + x, m.Y + y);
                if (under.World.Generator.TileAt(c) == TileType.CaveWall) { walls++; Assert.IsTrue(under.World.DepositAt(c).IsNone); }
                var d = under.World.DepositAt(c);
                if (!d.IsNone) ore++;
                if (d.Type == ResourceType.AlienCrystal) crystals++;
            }
            Assert.Greater(walls, 1000, "tunnels between rock walls");
            Assert.Greater(ore, 50);
            Assert.Greater(crystals, 0, "crystals underground");

            var sw = new System.IO.StringWriter(); SaveSystem.Write(under, sw);
            var back = SaveSystem.Read(new System.IO.StringReader(sw.ToString()));
            Assert.AreEqual(PlanetKind.Underground, back.PlanetKind);

            var up = Caves.Pass(under, m, g);
            Assert.AreSame(g, up, "the surface layer comes back as it was saved");
            Assert.AreEqual("home", up.PlanetId);
        }

        [Test]
        public void LaunchPads_CannotLaunchUnderground()
        {
            var lp = new LaunchPadState { Underground = true };
            var r = new Research();
            foreach (var t in Research.Tree.Where(t => t.MainLine)) r.Complete(t);
            lp.TryInsert(ItemDatabase.Coal, 50);
            StringAssert.Contains("underground", lp.Launch(r));
        }

        [Test]
        public void RuinKinds_WrecksHaveOnlySalvage_RelaysOnlyATerminal()
        {
            var g = new GameState(7);
            Facility wreck = null, relay = null;
            for (int rx = -60; rx <= 60 && (wreck == null || relay == null); rx++)
                for (int ry = -60; ry <= 60; ry++)
                    if (g.Exploration.FacilityInRegion(rx, ry) is Facility f)
                    { if (f.Kind == FacilityKind.Wreck) wreck = wreck ?? f; if (f.Kind == FacilityKind.Relay) relay = relay ?? f; }
            Assert.IsNotNull(wreck); Assert.IsNotNull(relay);
            Assert.IsNull(g.Exploration.FacilityAt(wreck.Terminal), "a wreck has no terminal");
            Assert.IsNotNull(g.Exploration.FacilityAt(wreck.Cache));
            Assert.IsNull(g.Exploration.Loot(wreck, g.Inventory));
            Assert.Greater(g.Inventory.Count(ItemDatabase.IronGear), 3, "salvage");
            Assert.IsNull(g.Exploration.FacilityAt(relay.Cache), "a relay has no cache");
            Assert.AreEqual("empty", g.Exploration.Loot(relay, g.Inventory));
            StringAssert.StartsWith("Relay", relay.Name);
        }

        [Test]
        public void Skills_LevelUpWithUse_AndAreSaved()
        {
            var g = new GameState(1);
            int ups = 0; g.Skills.LeveledUp += (s, l) => ups++;
            Assert.AreEqual(0, g.Skills.Level("mining"));
            g.Skills.Add("mining", 20);
            Assert.AreEqual(1, g.Skills.Level("mining"));
            g.Skills.Add("mining", 10000);
            Assert.AreEqual(Skills.MaxLevel, g.Skills.Level("mining"));
            Assert.AreEqual(2, ups);
            Assert.AreEqual(1.6f, g.Skills.MiningSpeed, 1e-4);
            g.Skills.Add("cooking", 5);   // unknown skills are ignored
            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            Assert.AreEqual(Skills.MaxLevel, SaveSystem.Read(new System.IO.StringReader(sw.ToString())).Skills.Level("mining"));
        }
    }
}
