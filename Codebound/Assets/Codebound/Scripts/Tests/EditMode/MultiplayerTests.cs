using System.Collections.Generic;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    public class MultiplayerTests
    {
        static Cell FreeCell(GameState g, string def)
        {
            var s = g.World.Generator.FindSpawn();
            for (int r = 1; r < 60; r++)
                for (int x = -r; x <= r; x++)
                {
                    var c = new Cell(s.X + x, s.Y + r);
                    if (g.CanPlace(BuildingDatabase.Get(def), c, Rotation.R0)) return c;
                }
            Assert.Fail("no room"); return default;
        }

        static Dictionary<string, object> A(params object[] kv)
        {
            var d = new Dictionary<string, object>();
            for (int i = 0; i < kv.Length; i += 2) d[(string)kv[i]] = kv[i + 1] is int n ? (double)n : kv[i + 1];
            return d;
        }

        [Test]
        public void GuestActions_UseTheirOwnInventory_AndAreValidated()
        {
            var host = new GameState(3);
            var guest = host.GuestInventory("ana");
            var c = FreeCell(host, "chest");
            double px = c.X + 0.5, py = c.Y - 1;

            Assert.AreEqual("not enough materials", Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "build", "def", "chest", "x", c.X, "y", c.Y, "rot", 0)));
            guest.Add(ItemDatabase.Wood, 20);
            Assert.AreEqual("too far", Multiplayer.Act(host, Multiplayer.Builder, guest, px + 50, py, A("a", "build", "def", "chest", "x", c.X, "y", c.Y, "rot", 0)));
            Assert.AreEqual("you are a visitor (ask the host to make you a builder)", Multiplayer.Act(host, Multiplayer.Visitor, guest, px, py, A("a", "build", "def", "chest", "x", c.X, "y", c.Y, "rot", 0)));
            Assert.IsNull(Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "build", "def", "chest", "x", c.X, "y", c.Y, "rot", 0)));
            Assert.IsNotNull(host.Grid.At(c));
            Assert.AreEqual(12, guest.Count(ItemDatabase.Wood), "guest paid 8 wood");
            Assert.AreEqual(0, host.Inventory.Count(ItemDatabase.Wood), "host's inventory untouched");

            Assert.IsNull(Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "put", "x", c.X, "y", c.Y, "item", "wood", "count", 5)));
            Assert.AreEqual(7, guest.Count(ItemDatabase.Wood));
            Assert.IsNull(Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "take", "x", c.X, "y", c.Y, "item", "wood", "count", 2)));
            Assert.AreEqual(9, guest.Count(ItemDatabase.Wood));

            Assert.AreEqual("unknown building", Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "build", "def", "nope", "x", c.X, "y", c.Y, "rot", 0)));
            Assert.That(Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "build", "x", "evil")), Does.StartWith("malformed"));
            Assert.IsNull(Multiplayer.Act(host, Multiplayer.Builder, guest, px, py, A("a", "remove", "x", c.X, "y", c.Y)));
            Assert.AreEqual(20, guest.Count(ItemDatabase.Wood), "refund and contents go to the guest");

            var sw = new System.IO.StringWriter(); SaveSystem.Write(host, sw);
            Assert.AreEqual(20, SaveSystem.Read(new System.IO.StringReader(sw.ToString())).GuestInventory("ana").Count(ItemDatabase.Wood), "guest inventory saved");
        }

        [Test]
        public void Snapshot_AppliesInPlace_AndHidesLockedComputers()
        {
            var host = new GameState(3);
            var c = FreeCell(host, "computer");
            var pc = host.Grid.Place(BuildingDatabase.Get("computer"), c, Rotation.R0);
            var cs = (ComputerState)pc.State;
            cs.Code = "secret_code()"; cs.SetPassword("hunter2");
            var c2 = FreeCell(host, "chest");
            var chest = host.Grid.Place(BuildingDatabase.Get("chest"), c2, Rotation.R0);
            ((StorageState)chest.State).Items.Add(ItemDatabase.Stone, 7);
            host.Research.Complete(Research.Get("logistics"));
            host.Clock.Seconds = 1234;

            var guest = new GameState(3);
            var gone = guest.Grid.Place(BuildingDatabase.Get("wall"), FreeCell(guest, "wall"), Rotation.R0);
            string snap = Multiplayer.Snapshot(host);
            StringAssert.DoesNotContain("pbkdf2", snap);
            StringAssert.DoesNotContain(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("secret_code()")), snap);

            Multiplayer.Apply(guest, SaveSystem.Read(new System.IO.StringReader(snap)));
            Assert.IsNull(guest.Grid.ById(gone.InstanceId), "stale building removed");
            Assert.AreEqual(7, ((StorageState)guest.Grid.At(c2).State).Items.Count(ItemDatabase.Stone));
            var gpc = (ComputerState)guest.Grid.At(c).State;
            Assert.IsTrue(gpc.HasPassword);
            Assert.IsFalse(gpc.CheckPassword("hunter2"), "guests cannot check the password offline");
            Assert.AreNotEqual("secret_code()", gpc.Code);
            Assert.IsTrue(guest.Research.IsDone("logistics"));
            Assert.AreEqual(1234, guest.Clock.Seconds, 1e-6);

            var before = guest.Grid.At(c2);
            Multiplayer.Apply(guest, SaveSystem.Read(new System.IO.StringReader(Multiplayer.Snapshot(host))));
            Assert.AreSame(before, guest.Grid.At(c2), "unchanged buildings are kept, not re-created");
        }

        [Test]
        public void Names_AndCreatureRoundTrip()
        {
            Assert.IsTrue(Multiplayer.ValidName("Ana_2"));
            Assert.IsFalse(Multiplayer.ValidName("a b"));
            Assert.IsFalse(Multiplayer.ValidName("../x"));
            Assert.IsFalse(Multiplayer.ValidName(new string('a', 17)));
            var g = new GameState(1);
            g.Creatures.Spawn(CreatureType.Crawler, 3.5, 4.25);
            var json = Json.Write(Multiplayer.CreaturesOf(g));
            var back = Multiplayer.ParseCreatures((List<object>)Json.Parse(json));
            Assert.AreEqual(1, back.Count);
            Assert.AreEqual(4.25, back[0].Y, 1e-9);
            Assert.AreEqual(CreatureType.Crawler, back[0].Type);
        }
    }
}
