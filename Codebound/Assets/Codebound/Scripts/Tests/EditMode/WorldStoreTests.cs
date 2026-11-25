using System;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Worlds screen data (list, rename, duplicate, delete without loss) and local profiles.</summary>
    public class WorldStoreTests
    {
        string dir;

        [SetUp] public void Make() { dir = Path.Combine(Path.GetTempPath(), "cb-worlds-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(dir); }
        [TearDown] public void Clean() { try { Directory.Delete(dir, true); } catch (IOException) { } }

        string SaveWorld(string id, int seed, int steps = 0)
        {
            var g = new GameState(seed);
            for (int i = 0; i < steps; i++) g.Step();
            using (var w = new StreamWriter(WorldStore.FileOf(dir, id))) SaveSystem.Write(g, w);
            return id;
        }

        [Test]
        public void List_ReadsHeader_AndReportsDamagedFiles()
        {
            SaveWorld("quicksave", 42, 40);
            File.WriteAllText(WorldStore.FileOf(dir, "broken"), "hello");
            File.WriteAllText(WorldStore.FileOf(dir, "planet_42_ember"), "CODEBOUND 1\nseed 1\n");   // old-layout planet file: not a world
            File.WriteAllText(WorldStore.FileOf(dir, "future"), "CODEBOUND 999\nseed 3\n");
            var all = WorldStore.List(dir);
            Assert.AreEqual(3, all.Count);
            var q = all.Find(w => w.Id == "quicksave");
            Assert.AreEqual(42, q.Seed); Assert.AreEqual(40, q.Tick); Assert.AreEqual(2.0, q.PlaySeconds, 1e-9); Assert.IsTrue(q.Loadable);
            Assert.AreEqual("damaged", all.Find(w => w.Id == "broken").Status);
            Assert.AreEqual("newer", all.Find(w => w.Id == "future").Status);
            Assert.AreEqual("quicksave", WorldStore.Latest(all).Id, "Continue never picks a file it cannot load");
            var sorted = WorldStore.Filter(all, "", WorldStore.Sort.Name);
            Assert.IsTrue(sorted[0].Loadable && !sorted[2].Loadable, "damaged files are listed last");
            Assert.AreEqual(1, WorldStore.Filter(all, "42", WorldStore.Sort.Recent).Count, "search by seed");
        }

        [Test]
        public void Rename_Duplicate_Delete_KeepEverything()
        {
            SaveWorld("alpha", 7);
            File.WriteAllText(WorldStore.PreviewOf(dir, "alpha"), "png");
            Directory.CreateDirectory(WorldStore.PlanetsOf(dir, "alpha"));
            File.WriteAllText(Path.Combine(WorldStore.PlanetsOf(dir, "alpha"), "ember.cbsave"), "planet");
            WorldStore.WriteMeta(dir, "alpha", name: "My base", created: 100, profile: "p1", biome: "Forest");

            WorldStore.Rename(dir, "alpha", "  Iron valley\n ");
            var a = WorldStore.Inspect(dir, "alpha");
            Assert.AreEqual("Iron valley", a.Name); Assert.AreEqual("p1", a.Profile); Assert.AreEqual(100, a.Created);
            Assert.Throws<ArgumentException>(() => WorldStore.Rename(dir, "alpha", "   "));

            string copy = WorldStore.Duplicate(dir, "alpha", "Iron valley (copy)", 500);
            Assert.AreEqual("iron-valley-copy", copy);
            var c = WorldStore.Inspect(dir, copy);
            Assert.AreEqual("Iron valley (copy)", c.Name); Assert.AreEqual(7, c.Seed); Assert.AreEqual(500, c.Created);
            Assert.IsTrue(File.Exists(Path.Combine(WorldStore.PlanetsOf(dir, copy), "ember.cbsave")), "visited planets are copied, not shared");
            Assert.AreNotEqual(copy, WorldStore.NewId(dir, "Iron valley (copy)"));

            string bin = WorldStore.Delete(dir, "alpha", 900);
            Assert.IsFalse(File.Exists(WorldStore.FileOf(dir, "alpha")));
            Assert.IsTrue(File.Exists(Path.Combine(bin, "alpha.cbsave")) && File.Exists(Path.Combine(bin, "alpha.png")) && File.Exists(Path.Combine(bin, "planets", "ember.cbsave")));
            Assert.AreEqual(1, WorldStore.List(dir).Count);
            Assert.AreEqual("world", WorldStore.NewId(dir, "???"));
            Assert.AreEqual("world-quicksave", WorldStore.NewId(dir, "Quicksave"), "never collides with the quick/auto save slots");
        }

        [Test]
        public void JoinAddress_AcceptsHostPortAndIPv6()
        {
            Assert.AreEqual(("192.168.1.5", Multiplayer.Port), Multiplayer.SplitAddress(" 192.168.1.5 "));
            Assert.AreEqual(("example.org", 5000), Multiplayer.SplitAddress("example.org:5000"));
            Assert.AreEqual(("::1", Multiplayer.Port), Multiplayer.SplitAddress("::1"), "a bare IPv6 address has no port");
            Assert.AreEqual(("fe80::2", 6000), Multiplayer.SplitAddress("[fe80::2]:6000"));
            Assert.AreEqual(Multiplayer.Port, Multiplayer.SplitAddress("host:abc").port, "a bad port is not taken");
        }

        [Test]
        public void Profiles_CreateSelectRenamePersist()
        {
            string path = Path.Combine(dir, "profiles.json");
            var s = new ProfileStore(path);
            Assert.IsNull(s.Selected);
            Assert.IsNull(s.Create("bad name!", 0, 10), "the profile name is also the multiplayer name");
            var ana = s.Create("Ana", 3, 10);
            var bob = s.Create("Bob", 99, 11);
            Assert.AreEqual(ProfileStore.Avatars - 1, bob.Avatar);
            Assert.AreEqual("taken", s.Validate("ana"));
            Assert.IsFalse(s.Rename(bob.Id, "ANA"));
            Assert.IsTrue(s.Rename(bob.Id, "Bruno"));
            s.Select(ana.Id, 20);

            var again = new ProfileStore(path);
            Assert.AreEqual(2, again.All.Count);
            Assert.AreEqual("Ana", again.Selected.Name); Assert.AreEqual(20, again.Selected.LastUsed);
            again.SignOut();
            Assert.IsNull(new ProfileStore(path).Selected);
            again.Delete(ana.Id);
            Assert.AreEqual("Bruno", new ProfileStore(path).All[0].Name);

            File.WriteAllText(path, "{ not json");
            Assert.AreEqual(0, new ProfileStore(path).All.Count);
            Assert.IsTrue(File.Exists(path + ".bad"), "a broken profiles file is kept");
        }
    }
}
