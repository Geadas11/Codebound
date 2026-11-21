using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class ExplorationTests
    {
        static (GameState g, Facility f, Facility f2) WithTwoFacilities()
        {
            var g = new GameState(7);
            var found = new System.Collections.Generic.List<Facility>();
            for (int r = 0; r < 120 && found.Count < 2; r++)
                for (int rx = -r; rx <= r && found.Count < 2; rx++)
                    for (int ry = -r; ry <= r && found.Count < 2; ry++)
                        if ((System.Math.Abs(rx) == r || System.Math.Abs(ry) == r) && g.Exploration.FacilityInRegion(rx, ry) is Facility f && f.Kind == FacilityKind.Outpost) found.Add(f);
            Assert.AreEqual(2, found.Count, "facilities exist in the ruins");
            return (g, found[0], found[1]);
        }

        [Test]
        public void Facilities_AreDeterministic_InRuins_AndBlockBuilding()
        {
            var (g, f, _) = WithTwoFacilities();
            var again = new GameState(7).Exploration.FacilityInRegion(f.RegionX, f.RegionY);
            Assert.AreEqual(f.Terminal, again.Terminal);
            Assert.AreEqual(Biome.Ruins, g.World.Generator.BiomeAt(f.Terminal));
            Assert.IsNotNull(g.Exploration.FacilityAt(f.Cache));
            Assert.IsNull(g.Exploration.FacilityAt(new Cell(f.Terminal.X + 1, f.Terminal.Y)), "gap between terminal and cache is free");
            Assert.IsFalse(g.CanPlace(BuildingDatabase.Get("wall"), f.Terminal, Rotation.R0));
            Assert.IsTrue(g.Exploration.Near(f.Terminal, 4).Any(x => x.Terminal.Equals(f.Terminal)));
        }

        [Test]
        public void Logs_ComeInStoryOrder_CacheLootsOnce_AndBothAreSaved()
        {
            var (g, f, f2) = WithTwoFacilities();
            Assert.AreEqual(0, g.Exploration.Read(f2), "first terminal read is always log 1");
            Assert.AreEqual(1, g.Exploration.Read(f));
            Assert.AreEqual(0, g.Exploration.Read(f2), "re-reading keeps the log");

            Assert.IsNull(g.Exploration.Loot(f, g.Inventory));
            Assert.Greater(g.Inventory.Count("circuit"), 1);
            Assert.AreEqual("empty", g.Exploration.Loot(f, g.Inventory));

            var sw = new System.IO.StringWriter(); SaveSystem.Write(g, sw);
            var l = SaveSystem.Read(new System.IO.StringReader(sw.ToString()));
            Assert.AreEqual(2, l.Exploration.LogsRead);
            Assert.IsTrue(l.Exploration.IsLooted(f));
            Assert.IsFalse(l.Exploration.IsLooted(f2));
            Assert.AreEqual(1, l.Exploration.Read(f));
        }

        [Test]
        public void LastLog_IsReusedWhenThereAreMoreOutpostsThanLogs()
        {
            var g = new GameState(7);
            int n = 0, last = -1;
            for (int rx = -60; rx <= 60 && n <= Lore.Logs.Length; rx++)
                for (int ry = -60; ry <= 60 && n <= Lore.Logs.Length; ry++)
                    if (g.Exploration.FacilityInRegion(rx, ry) is Facility f && f.HasTerminal) { last = g.Exploration.Read(f); n++; }
            Assert.Greater(n, Lore.Logs.Length, "enough outposts in a 121x121-region square");
            Assert.AreEqual(Lore.Logs.Length - 1, last);
        }
    }
}
