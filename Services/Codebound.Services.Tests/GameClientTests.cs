using System.Text;
using Codebound.Core.Online;

namespace Codebound.Services.Tests;

/// <summary>The game's own client code (Codebound/Assets/.../Core/Online, compiled in here) against the real service.</summary>
public class GameClientTests
{
    sealed class Mem : ITextStore { public string Text = ""; public string Load() => Text; public void Save(string t) => Text = t; }

    /// <summary>The real HTTP transport with a switch that cuts the network.</summary>
    sealed class Cable : IHttpTransport
    {
        readonly HttpClientTransport real;
        public bool Unplugged;
        public Cable(Uri b) { real = new HttpClientTransport(b.ToString()); }
        public Task<(int status, string body)> Send(string m, string p, string j, string? bearer) =>
            Unplugged ? throw new HttpRequestException("network unreachable") : real.Send(m, p, j, bearer);
    }

    static long Now(Server s) => s.Clock.GetUtcNow().ToUnixTimeSeconds();

    static async Task<(OnlineClient c, Cable cable, Mem store)> SignedIn(Server s, string name)
    {
        var cable = new Cable(s.Base); var store = new Mem();
        var c = new OnlineClient(cable, store, () => Now(s));
        string email = name.ToLowerInvariant() + "@example.test", pw = "correct horse " + name;
        Assert.True((await c.Register(email, pw, name)).Ok);
        Assert.Equal("email_not_verified", (await c.Login(email, pw, "pc")).Error);
        Assert.True((await c.Verify(email, s.CodeFor(email))).Ok);
        var login = await c.Login(email, pw, "pc");
        Assert.True(login.Ok, login.Message);
        return (c, cable, store);
    }

    [Fact]
    public async Task SignIn_Renewal_Restart_AndSignOut()
    {
        await using var s = new Server();
        var (c, _, store) = await SignedIn(s, "Ana");
        Assert.Equal("Ana", c.DisplayName);
        Assert.DoesNotContain("correct horse", store.Text);
        s.Advance(TimeSpan.FromMinutes(40));                       // access token long expired: renewed on the next call
        Assert.True((await c.Me()).Ok);
        var restarted = new OnlineClient(new HttpClientTransport(s.Base.ToString()), store, () => Now(s));   // the game restarts
        Assert.True((await restarted.Friends()).Ok);
        await restarted.Logout();
        Assert.False(restarted.SignedIn);
        Assert.Equal("", store.Text);
    }

    [Fact]
    public async Task RevokedDevice_IsSignedOut_OnItsNextCall()
    {
        await using var s = new Server();
        var (pc, _, _) = await SignedIn(s, "Bob");
        var laptop = new OnlineClient(new HttpClientTransport(s.Base.ToString()), new Mem(), () => Now(s));
        Assert.True((await laptop.Login("bob@example.test", "correct horse Bob", "laptop")).Ok);
        var devices = (await pc.DeviceSessions()).List!;
        Assert.Equal(2, devices.Count);
        var other = devices.Cast<Dictionary<string, object>>().First(d => (double)d["current"] == 0);
        Assert.True((await pc.RevokeDevice((string)other["id"])).Ok);
        s.Advance(TimeSpan.FromMinutes(20));
        var r = await laptop.Me();
        Assert.False(r.Ok); Assert.False(laptop.SignedIn);
    }

    [Fact]
    public async Task LosingTheInternet_QueuesStats_AndRecoveryShipsThem()
    {
        await using var s = new Server();
        var (c, cable, _) = await SignedIn(s, "Cid");
        var journal = new StatsJournal(new Mem(), () => Now(s));
        cable.Unplugged = true;
        for (int i = 0; i < 30; i++) { s.Advance(TimeSpan.FromMinutes(1)); journal.Add("playtime_seconds", 60, c.PublicId); journal.Add("distance", 100, c.PublicId); }
        journal.Seal();
        var down = await c.Me();
        Assert.True(down.Offline); Assert.True(c.SignedIn); Assert.Equal(Connection.Offline, c.State);
        Assert.True((await journal.Sync(c)).stoppedOffline);
        Assert.Equal(3, journal.Pending.Count);
        cable.Unplugged = false;                                    // back online
        var (sent, refused, _) = await journal.Sync(c);
        Assert.Equal(3, sent); Assert.Equal(0, refused);
        Assert.Equal(Connection.Online, c.State);
        var st = (await c.Stats()).Obj!;
        var stats = (Dictionary<string, object>)st["stats"];
        Assert.Equal(1800.0, stats["playtime_seconds"]);
        Assert.Equal(3000.0, stats["distance"]);
        Assert.Contains("first_hour", ((List<object>)st["definitions"]).Cast<Dictionary<string, object>>().Select(d => (string)d["id"]));
    }

    [Fact]
    public async Task OnlineGame_TicketVerifiedByTheHost_InvitesAndBans()
    {
        await using var s = new Server();
        var (host, _, _) = await SignedIn(s, "Host1");
        var (guest, _, _) = await SignedIn(s, "Guest1");
        var pub = await host.HostSession("Host1's world", "public", 4, 47820, 3, false, "192.168.1.20");
        Assert.True(pub.Ok, pub.Message);
        string sid = pub.Str("sessionId");
        Assert.Contains((await guest.Sessions("")).List!, o => (string)((Dictionary<string, object>)o)["id"] == sid);
        var join = await guest.JoinSession(sid);
        Assert.True(join.Ok, join.Message);
        string ticket = join.Str("ticket");
        var who = await host.VerifyTicket(sid, ticket);               // what the game's host does when the guest says hello
        Assert.Equal(guest.PublicId, who.Str("publicId"));
        Assert.Equal("Guest1", who.Str("displayName"));
        Assert.False((await host.VerifyTicket(sid, ticket)).Ok, "a ticket works once");
        Assert.False((await guest.VerifyTicket(sid, ticket)).Ok, "only the host can check tickets");

        // invites: friends only, then joining through the invite
        Assert.Equal("not_friends", (await host.Invite(sid, guest.PublicId)).Error);
        await guest.AddFriend(host.PublicId);
        var req = (List<object>)(await host.FriendRequests()).Obj!["incoming"];
        Assert.True((await host.AcceptFriend((long)(double)((Dictionary<string, object>)req[0])["id"])).Ok);
        Assert.True((await host.Invite(sid, guest.PublicId)).Ok);
        var inv = (List<object>)(await guest.Notifications()).Obj!["invites"];
        var accepted = await guest.AcceptInvite((long)(double)((Dictionary<string, object>)inv[0])["id"]);
        Assert.True(accepted.Ok); Assert.NotNull(accepted.Str("ticket"));

        // presence shows the game to friends
        await guest.Presence("busy", sid);
        var f = (await host.Friends()).List!.Cast<Dictionary<string, object>>().Single();
        Assert.Equal("in_game", f["presence"]);

        // a ban keeps the guest out of this listing
        Assert.True((await host.Kick(sid, guest.PublicId, ban: true)).Ok);
        Assert.Equal("banned", (await guest.JoinSession(sid)).Error);
        Assert.True((await host.CloseSession(sid)).Ok);
        Assert.Equal("session_closed", (await guest.JoinSession(sid)).Error);
    }

    [Fact]
    public async Task CloudSaves_TwoPcs_ConflictIsTheirChoice_AndNothingLocalIsLost()
    {
        await using var s = new Server();
        var (pc, _, _) = await SignedIn(s, "Dee");
        var laptop = new OnlineClient(new HttpClientTransport(s.Base.ToString()), new Mem(), () => Now(s));
        Assert.True((await laptop.Login("dee@example.test", "correct horse Dee", "laptop")).Ok);
        string dirA = Path.Combine(Path.GetTempPath(), "cb-a-" + Guid.NewGuid().ToString("N")), dirB = dirA.Replace("cb-a-", "cb-b-");
        try
        {
            Directory.CreateDirectory(dirA); Directory.CreateDirectory(dirB);
            File.WriteAllText(Path.Combine(dirA, "quicksave.cbsave"), "world from the pc");
            var a = new CloudSaves(pc, dirA, "pc", () => Now(s));
            var b = new CloudSaves(laptop, dirB, "laptop", () => Now(s));
            var stA = (await a.Check())!.Single();
            Assert.Equal(SaveSyncState.LocalOnly, stA.State);
            Assert.True((await a.Upload(stA)).Ok);

            File.WriteAllText(Path.Combine(dirB, "quicksave.cbsave"), "world from the laptop");   // played offline on the laptop
            var stB = (await b.Check())!.Single();
            Assert.Equal(SaveSyncState.Conflict, stB.State);
            Assert.Equal("conflict", (await b.Upload(stB)).Error);                                 // never a silent overwrite
            Assert.True((await b.Download(stB, asCopy: true)).Ok);                                  // the player keeps both
            Assert.Equal("world from the laptop", File.ReadAllText(Path.Combine(dirB, "quicksave.cbsave")));
            Assert.Equal("world from the pc", File.ReadAllText(Path.Combine(dirB, "quicksave-cloud.cbsave")));

            stB = (await b.Check())!.Single(x => x.Slot == "quicksave");
            Assert.True((await b.Upload(stB, keepLocalOverCloud: true)).Ok);                       // then chooses the laptop's
            stA = (await a.Check())!.Single();
            Assert.Equal(SaveSyncState.CloudNewer, stA.State);
            Assert.True((await a.Download(stA, asCopy: false)).Ok);
            Assert.Equal("world from the laptop", File.ReadAllText(Path.Combine(dirA, "quicksave.cbsave")));
            Assert.Equal("world from the pc", File.ReadAllText(Directory.GetFiles(Path.Combine(dirA, "backups")).Single()));
            var versions = (await pc.Call("GET", "saves/quicksave/versions")).List!;
            Assert.Equal(2, versions.Count);                                                        // the old cloud version is still recoverable
        }
        finally { foreach (var d in new[] { dirA, dirB }) try { Directory.Delete(d, true); } catch (IOException) { } }
    }

    [Fact]
    public async Task ServiceDown_EverythingSaysOffline_NothingThrows()
    {
        var s = new Server();
        var (c, _, _) = await SignedIn(s, "Eve");
        var url = s.Base;
        await s.DisposeAsync();                                     // the server is gone
        var r = await c.Friends();
        Assert.True(r.Offline); Assert.True(c.SignedIn);
        Assert.False(await c.ServiceReachable());
        var fresh = new OnlineClient(new HttpClientTransport(url.ToString()), new Mem(), () => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Assert.True((await fresh.Login("eve@example.test", "x", "pc")).Offline);
        Assert.Null(await new CloudSaves(c, Path.GetTempPath(), "pc", () => 0).Check());
    }
}
