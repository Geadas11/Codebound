using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class TutorialTests
    {
        [Test]
        public void Steps_FollowWhatThePlayerDoes()
        {
            var g = new GameState(42);
            var t = g.Tutorial;
            var done = new List<string>(); t.StepDone += s => done.Add(s.Id);
            t.Update(g); Assert.AreEqual(0, t.Index);
            g.Inventory.Add(ItemDatabase.Wood, 10); g.Inventory.Add(ItemDatabase.Stone, 10);
            t.Update(g);
            CollectionAssert.AreEqual(new[] { "wood", "stone" }, done, "two steps in one go when both are met");

            // a drill on ore, a fuelled generator, a computer in range of the drill
            Cell o = default; bool found = false;
            for (int x = -150; x < 150 && !found; x++)
                for (int y = -150; y < 150 && !found; y++)
                {
                    var c = new Cell(x, y); var dep = g.World.DepositAt(c);
                    if (WorldRules.DrillCanMine(dep) && dep.Amount >= 20 && g.CanPlace(BuildingDatabase.Get("drill"), c, Rotation.R0)
                        && g.CanPlace(BuildingDatabase.Get("generator"), new Cell(x - 3, y), Rotation.R0) && g.CanPlace(BuildingDatabase.Get("computer"), new Cell(x + 3, y), Rotation.R0))
                    { o = c; found = true; }
                }
            Assert.IsTrue(found);
            var drill = g.Grid.Place(BuildingDatabase.Get("drill"), o, Rotation.R0);
            t.Update(g); Assert.AreEqual("power", t.Current.Id);
            var gen = g.Grid.Place(BuildingDatabase.Get("generator"), new Cell(o.X - 3, o.Y), Rotation.R0);
            t.Update(g); Assert.AreEqual("power", t.Current.Id, "a generator without coal is not power");
            gen.State.TryInsert(ItemDatabase.Coal, 5);
            t.Update(g); Assert.AreEqual("computer", t.Current.Id);
            var pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(o.X + 3, o.Y), Rotation.R0);
            t.Update(g); Assert.AreEqual("program", t.Current.Id);
            ((ComputerState)pc.State).Record(new RunRecord { When = "t", Language = "javascript", Result = "done" });
            t.Update(g); Assert.AreEqual("auto", t.Current.Id);

            Assert.IsNull(g.Factory.StartMine(drill, null));   // what a program's drill.mine() does
            for (float s = 0; s < Factory.MineSeconds + 0.3f; s += 0.05f) g.Factory.Tick(0.05f);
            t.Update(g); Assert.AreEqual("research", t.Current.Id);
            g.Research.Complete(Research.Tree[0]);
            t.Update(g); Assert.IsTrue(t.Finished);
        }

        [Test]
        public void Progress_IsSaved_OldSavesSkipIt_AndSkipWorks()
        {
            var g = new GameState(3);
            g.Inventory.Add(ItemDatabase.Wood, 10); g.Tutorial.Update(g);
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            Assert.AreEqual(1, SaveSystem.Read(new StringReader(sw.ToString())).Tutorial.Index);

            string old = string.Join("\n", System.Array.FindAll(sw.ToString().Split('\n'), l => !l.StartsWith("tutorial")));
            Assert.IsTrue(SaveSystem.Read(new StringReader(old)).Tutorial.Finished, "a save from before the tutorial");

            g.Tutorial.Skip(); Assert.IsTrue(g.Tutorial.Finished);
            foreach (var s in Tutorial.Steps) { Assert.IsNotEmpty(Tutorial.Title(s)); Assert.IsNotEmpty(Tutorial.Hint(s)); }
        }
    }
}
