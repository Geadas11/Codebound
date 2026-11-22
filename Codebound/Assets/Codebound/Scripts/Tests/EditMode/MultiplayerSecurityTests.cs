using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>Etapa 6 completion: chat, movement checks, guest bodies, code of unlocked computers, world password, UPnP text.</summary>
    public class MultiplayerSecurityTests
    {
        [Test]
        public void Chat_IsCleaned()
        {
            Assert.AreEqual("hi ‹b›there‹/b›", Multiplayer.CleanChat("  hi <b>there</b>\n\u0007 "));
            Assert.IsNull(Multiplayer.CleanChat("   \t"));
            Assert.IsNull(Multiplayer.CleanChat(null));
            Assert.AreEqual(Multiplayer.MaxChat, Multiplayer.CleanChat(new string('a', 999)).Length);
        }

        [Test]
        public void Moves_AreLimitedBySpeed_Walls_AndAllowRespawn()
        {
            bool Wall(Cell c) => c.X == 10;
            double budget = Multiplayer.MoveBurst;
            Assert.IsTrue(Multiplayer.CheckMove(Wall, ref budget, 0.1, 0, 0, 0.5, 0, 100, 100), "a normal step");
            Assert.IsFalse(Multiplayer.CheckMove(Wall, ref budget, 0.1, 0.5, 0, 40, 0, 100, 100), "teleport");
            budget = 0;
            Assert.IsFalse(Multiplayer.CheckMove(Wall, ref budget, 0.1, 0, 0, 2, 0, 100, 100), "faster than walking");
            budget = Multiplayer.MoveBurst;
            Assert.IsFalse(Multiplayer.CheckMove(Wall, ref budget, 0.1, 9.6, 0.5, 10.3, 0.5, 100, 100), "into a wall");
            Assert.IsTrue(Multiplayer.CheckMove(Wall, ref budget, 0.1, 500, 500, 100.5, 100.5, 100, 100), "respawn");
            Assert.IsFalse(Multiplayer.CheckMove(Wall, ref budget, 0.1, 0, 0, double.NaN, 0, 100, 100));

            // claiming 10 cells/s for 3 s: only burst + 1.2 × walking speed gets through (no speed hack with many small moves)
            budget = Multiplayer.MoveBurst; double x = 0; int accepted = 0;
            for (int i = 0; i < 30; i++) if (Multiplayer.CheckMove(_ => false, ref budget, 0.1, x, 0, x + 1, 0, 1e6, 1e6)) { x += 1; accepted++; }
            Assert.LessOrEqual(accepted, Multiplayer.MoveBurst + Multiplayer.MoveSpeed * 1.2 * 3);
            Assert.Less(accepted, 30);
        }

        [Test]
        public void GuestBodies_AreSaved_AndNotSentToOtherGuests()
        {
            var g = new GameState(4);
            var b = new GuestBody { X = 12.5, Y = -3.25 };
            b.Vitals.Health = 42; b.Vitals.Hunger = 17;
            g.GuestBodies["ana"] = b;
            var sw = new StringWriter(); SaveSystem.Write(g, sw);
            var back = SaveSystem.Read(new StringReader(sw.ToString())).GuestBodies["ana"];
            Assert.AreEqual(12.5, back.X); Assert.AreEqual(-3.25, back.Y);
            Assert.AreEqual(42f, back.Vitals.Health); Assert.AreEqual(17f, back.Vitals.Hunger);
            StringAssert.DoesNotContain("gbody", Multiplayer.Snapshot(g));
        }

        [Test]
        public void GuestVitals_AreClamped()
        {
            var v = new Vitals();
            Multiplayer.LoadVitals(v, "vitals 9999 -5 50 NaN 80");
            Assert.AreEqual(Vitals.Max, v.Health); Assert.AreEqual(0f, v.Hunger); Assert.AreEqual(50f, v.Thirst);
            Assert.AreEqual(Vitals.Max, v.Oxygen); Assert.AreEqual(45f, v.BodyTemp);
            Assert.Throws<System.FormatException>(() => Multiplayer.LoadVitals(v, "nope"));
        }

        [Test]
        public void LockedComputer_CodeOnlyForGuestsWhoUnlockedIt_HashNever()
        {
            var g = new GameState(5);
            var s = g.World.Generator.FindSpawn();
            PlacedBuilding pc = null;
            for (int x = 1; pc == null && x < 40; x++) pc = g.Grid.Place(BuildingDatabase.Get("computer"), new Cell(s.X + x, s.Y), Rotation.R0);
            var cs = (ComputerState)pc.State;
            cs.Code = "log('secret plan')"; cs.Files["lib.js"] = "exports.x = 1;";
            cs.SetPassword("hunter2");
            string hash = cs.PasswordHash;

            string closed = Multiplayer.Snapshot(g), opened = Multiplayer.Snapshot(g, new HashSet<Cell> { pc.Origin });
            StringAssert.DoesNotContain(hash, closed); StringAssert.DoesNotContain(hash, opened);
            var c1 = (ComputerState)SaveSystem.Read(new StringReader(closed)).Grid.At(pc.Origin).State;
            var c2 = (ComputerState)SaveSystem.Read(new StringReader(opened)).Grid.At(pc.Origin).State;
            Assert.AreNotEqual(cs.Code, c1.Code); Assert.IsEmpty(c1.Files);
            Assert.AreEqual(cs.Code, c2.Code); Assert.AreEqual("exports.x = 1;", c2.Files["lib.js"]);
            Assert.IsTrue(c2.HasPassword, "still shown as locked");
            Assert.IsFalse(c2.CheckPassword("hunter2"), "the guest copy can't check passwords: the host does");
        }

        [Test]
        public void Upnp_FindsTheRouterService_AndBuildsRequests()
        {
            Assert.AreEqual("http://192.168.1.1:5000/rootDesc.xml", Upnp.Location("HTTP/1.1 200 OK\r\nCACHE-CONTROL: max-age=120\r\nLOCATION: http://192.168.1.1:5000/rootDesc.xml\r\nST: x\r\n\r\n"));
            Assert.IsNull(Upnp.Location("HTTP/1.1 200 OK\r\nLOCATION: file:///etc/passwd\r\n"));
            string desc = "<root><device><serviceList>" +
                          "<service><serviceType>urn:schemas-upnp-org:service:Layer3Forwarding:1</serviceType><controlURL>/l3f</controlURL></service>" +
                          "<service><serviceType>urn:schemas-upnp-org:service:WANIPConnection:1</serviceType><controlURL>/ctl/IPConn</controlURL></service>" +
                          "</serviceList></device></root>";
            var svc = Upnp.WanService(desc, "http://192.168.1.1:5000/rootDesc.xml");
            Assert.AreEqual("urn:schemas-upnp-org:service:WANIPConnection:1", svc.Value.serviceType);
            Assert.AreEqual("http://192.168.1.1:5000/ctl/IPConn", svc.Value.controlUrl);
            Assert.IsNull(Upnp.WanService("<root/>", "http://x/"));
            string add = Upnp.AddPortMapping("urn:x:WANIPConnection:1", 47820, "192.168.1.20");
            StringAssert.Contains("<NewExternalPort>47820</NewExternalPort>", add);
            StringAssert.Contains("<NewInternalClient>192.168.1.20</NewInternalClient>", add);
            StringAssert.Contains("&lt;b&gt;", Upnp.Soap("t", "A", ("n", "<b>")));
            Assert.AreEqual("203.0.113.7", Upnp.Tag("<s:Body><u:R><NewExternalIPAddress>203.0.113.7</NewExternalIPAddress></u:R></s:Body>", "NewExternalIPAddress"));
        }
    }
}
