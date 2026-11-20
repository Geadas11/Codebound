using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class ResearchTests
    {
        static void Run(GameState g, float seconds) { for (float t = 0; t < seconds; t += 0.05f) g.Factory.Tick(0.05f); }

        [Test]
        public void Tree_IsValid_NoCycles_UnlocksRealBuildings_MainLineEndsInLaunch()
        {
            var ids = new HashSet<string>(Research.Tree.Select(t => t.Id));
            Assert.AreEqual(Research.Tree.Count, ids.Count, "unique ids");
            foreach (var t in Research.Tree)
            {
                foreach (var p in t.Prereqs) Assert.IsTrue(ids.Contains(p), $"{t.Id} → unknown prereq {p}");
                foreach (var b in t.Unlocks) Assert.DoesNotThrow(() => BuildingDatabase.Get(b), $"{t.Id} unlocks unknown {b}");
                foreach (var c in t.Cost) Assert.IsTrue(ItemDatabase.Exists(c.ItemId), $"{t.Id} costs unknown {c.ItemId}");
            }
            // topological order exists ⇒ no cycles
            var done = new HashSet<string>(); bool progress = true;
            while (progress) { progress = false; foreach (var t in Research.Tree) if (!done.Contains(t.Id) && t.Prereqs.All(done.Contains)) { done.Add(t.Id); progress = true; } }
            Assert.AreEqual(ids.Count, done.Count, "every tech is reachable (no cycles)");
            Assert.IsTrue(Research.Get("ship_launch").MainLine);
            Assert.AreEqual(5, new Research().MainLineTotal);
        }

        [Test]
        public void Buildings_LockedUntilResearched()
        {
            var g = new GameState(3);
            Assert.IsTrue(g.Research.IsUnlocked(BuildingDatabase.Get("lab")));
            Assert.IsTrue(g.Research.IsUnlocked(BuildingDatabase.Get("drill")));
            Assert.IsFalse(g.Research.IsUnlocked(BuildingDatabase.Get("belt")));
            var s = g.World.Generator.FindSpawn();
            g.Inventory.Add(ItemDatabase.IronPlate, 5);
            PlacedBuilding b = null;
            for (int x = 1; x < 30 && b == null; x++) b = g.TryBuild(BuildingDatabase.Get("belt"), new Cell(s.X + x, s.Y), Rotation.R0);
            Assert.IsNull(b, "locked");
            Assert.AreEqual(5, g.Inventory.Count(ItemDatabase.IronPlate), "nothing paid");
            g.Research.Complete(Research.Get("logistics"));
            for (int x = 1; x < 30 && b == null; x++) b = g.TryBuild(BuildingDatabase.Get("belt"), new Cell(s.X + x, s.Y), Rotation.R0);
            Assert.NotNull(b);
        }

        [Test]
        public void Lab_ResearchesWithMaterialsAndPower_ThenSaves()
        {
            var g = new GameState(42);
            var s = g.World.Generator.FindSpawn();
            PlacedBuilding lab = null, gen = null;
            for (int r = 1; r < 100 && gen == null; r++)
                for (int x = -r; x <= r && gen == null; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    if (!g.CanPlace(BuildingDatabase.Get("lab"), c, Rotation.R0) || !g.CanPlace(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y), Rotation.R0)) continue;
                    lab = g.Grid.Place(BuildingDatabase.Get("lab"), c, Rotation.R0);
                    gen = g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(c.X + 2, c.Y), Rotation.R0);
                }
            Assert.NotNull(gen);
            var st = (LabState)lab.State;
            Assert.AreEqual("unknown_tech", g.Factory.StartResearch(lab, "time_travel", null));
            Assert.AreEqual("prerequisites_missing", g.Factory.StartResearch(lab, "manufacturing", null));
            StringAssert.StartsWith("missing_", g.Factory.StartResearch(lab, "logistics", null));
            st.TryInsert(ItemDatabase.IronPlate, 10); st.TryInsert(ItemDatabase.CopperPlate, 6);
            Assert.AreEqual("no_power", g.Factory.StartResearch(lab, "logistics", null));
            gen.State.TryInsert(ItemDatabase.Coal, 10);
            string done = "pending"; Tech completed = null;
            g.Research.Completed += t => completed = t;
            Assert.IsNull(g.Factory.StartResearch(lab, "logistics", r => done = r));
            Run(g, 10);
            Assert.AreEqual("pending", done, "takes the tech's time");
            Run(g, 11);
            Assert.AreEqual("logistics", done);
            Assert.AreEqual("logistics", completed?.Id);
            Assert.IsTrue(g.Research.IsDone("logistics"));
            Assert.AreEqual(0, st.Input.Count(ItemDatabase.IronPlate)); Assert.AreEqual(1, st.Input.Count(ItemDatabase.CopperPlate));
            Assert.AreEqual("already_done", g.Factory.StartResearch(lab, "logistics", null));

            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            Assert.IsTrue(SaveSystem.Read(new StringReader(sw.ToString())).Research.IsDone("logistics"));
        }
    }
}
