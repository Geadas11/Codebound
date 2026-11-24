using System.Net;
using System.Net.Http.Json;

namespace Codebound.Services.Tests;

public class SessionTests
{
    static async Task Befriend(Player a, Player b)
    {
        await a.Post("friends/requests", new { publicId = b.PublicId });
        long id = (await b.GetJson("friends/requests")).GetProperty("incoming")[0].GetProperty("id").GetInt64();
        (await b.Post($"friends/requests/{id}/accept", new { })).EnsureSuccessStatusCode();
    }

    static async Task<string> Host(Player p, string visibility, int max = 8) =>
        (await p.PostJson("sessions", new { name = p.Name + "'s world", visibility, maxPlayers = max, port = 47820, version = 3, hasPassword = false })).GetProperty("sessionId").GetString()!;

    [Fact]
    public async Task Listing_RespectsVisibility_Friendship_AndBlocks()
    {
        await using var s = new Server();
        var host = await s.Player("Host"); var friend = await s.Player("Friend"); var stranger = await s.Player("Stranger");
        await Befriend(host, friend);
        string id = await Host(host, "friends");
        Assert.Equal(1, (await friend.GetJson("sessions")).GetArrayLength());
        Assert.Equal(0, (await stranger.GetJson("sessions")).GetArrayLength());
        Assert.Equal("friends_only", (await Server.Error(await stranger.Post($"sessions/{id}/join", new { }))).code);
        await Host(host, "public");   // replaces the first game
        Assert.Equal(1, (await stranger.GetJson("sessions?q=world")).GetArrayLength());
        Assert.Equal(0, (await stranger.GetJson("sessions?q=zzz")).GetArrayLength());
        await stranger.Post("blocks", new { publicId = host.PublicId });
        Assert.Equal(0, (await stranger.GetJson("sessions")).GetArrayLength());
        string priv = await Host(host, "private");
        Assert.Equal(0, (await friend.GetJson("sessions")).GetArrayLength());
        Assert.Equal("invite_only", (await Server.Error(await friend.Post($"sessions/{priv}/join", new { }))).code);
        s.Advance(TimeSpan.FromSeconds(SessionsApi.StaleSeconds + 1));   // no heartbeat: gone from the list
        await Host(host, "public");
        s.Advance(TimeSpan.FromSeconds(SessionsApi.StaleSeconds + 1));
        Assert.Equal(0, (await friend.GetJson("sessions")).GetArrayLength());
    }

    [Fact]
    public async Task JoinTickets_AreSingleUse_ExpireAndNameTheAccount()
    {
        await using var s = new Server();
        var host = await s.Player("Host"); var guest = await s.Player("Guest");
        string id = await Host(host, "public");
        var t = await guest.PostJson($"sessions/{id}/join", new { });
        string ticket = t.GetProperty("ticket").GetString()!;
        Assert.Equal("Host", t.GetProperty("host").GetProperty("displayName").GetString());
        Assert.Equal("not_host", (await Server.Error(await guest.Post($"sessions/{id}/tickets/verify", new { ticket }))).code);
        var who = await host.PostJson($"sessions/{id}/tickets/verify", new { ticket });
        Assert.Equal(guest.PublicId, who.GetProperty("publicId").GetString());
        Assert.Equal("invalid_ticket", (await Server.Error(await host.Post($"sessions/{id}/tickets/verify", new { ticket }))).code);   // used
        var late = (await guest.PostJson($"sessions/{id}/join", new { })).GetProperty("ticket").GetString();
        s.Advance(TimeSpan.FromSeconds(SessionsApi.TicketSeconds + 1));
        await host.Post($"sessions/{id}/heartbeat", new { players = 1 });
        Assert.Equal("invalid_ticket", (await Server.Error(await host.Post($"sessions/{id}/tickets/verify", new { ticket = late }))).code);   // expired
        Assert.Equal("invalid_ticket", (await Server.Error(await host.Post($"sessions/{id}/tickets/verify", new { ticket = "forged" }))).code);
    }

    [Fact]
    public async Task Kicking_AndBanning_AFullGame_AndClosing()
    {
        await using var s = new Server();
        var host = await s.Player("Host"); var a = await s.Player("Aaa"); var b = await s.Player("Bbb");
        string id = await Host(host, "public", max: 1);
        var ta = (await a.PostJson($"sessions/{id}/join", new { })).GetProperty("ticket").GetString();
        await host.Post($"sessions/{id}/tickets/verify", new { ticket = ta });
        await host.Post($"sessions/{id}/heartbeat", new { players = 1 });
        Assert.Equal("full", (await Server.Error(await b.Post($"sessions/{id}/join", new { }))).code);
        (await host.Post($"sessions/{id}/kick", new { publicId = a.PublicId, ban = true })).EnsureSuccessStatusCode();
        await host.Post($"sessions/{id}/heartbeat", new { players = 0 });
        Assert.Equal("banned", (await Server.Error(await a.Post($"sessions/{id}/join", new { }))).code);
        (await b.Post($"sessions/{id}/join", new { })).EnsureSuccessStatusCode();
        (await host.Http.DeleteAsync($"sessions/{id}")).EnsureSuccessStatusCode();
        Assert.Equal("session_closed", (await Server.Error(await b.Post($"sessions/{id}/join", new { }))).code);
        var hist = await a.GetJson("me/sessions/history");
        Assert.Equal(1, hist.GetProperty("joined").GetArrayLength());
        Assert.Equal(1, (await host.GetJson("me/sessions/history")).GetProperty("hosted").GetArrayLength());
    }

    [Fact]
    public async Task Invites_LetFriendsIntoPrivateGames_AndExpire()
    {
        await using var s = new Server();
        var host = await s.Player("Host"); var friend = await s.Player("Friend"); var other = await s.Player("Other");
        await Befriend(host, friend);
        string id = await Host(host, "private");
        Assert.Equal("not_friends", (await Server.Error(await host.Post("invites", new { sessionId = id, publicId = other.PublicId }))).code);
        Assert.Equal("not_in_game", (await Server.Error(await other.Post("invites", new { sessionId = id, publicId = friend.PublicId }))).code);
        (await host.Post("invites", new { sessionId = id, publicId = friend.PublicId })).EnsureSuccessStatusCode();
        (await host.Post("invites", new { sessionId = id, publicId = friend.PublicId })).EnsureSuccessStatusCode();   // again: no duplicate
        var inv = await friend.GetJson("invites");
        Assert.Equal(1, inv.GetArrayLength());
        Assert.Equal(1, (await friend.GetJson("me/notifications")).GetProperty("invites").GetArrayLength());
        var t = await friend.PostJson($"invites/{inv[0].GetProperty("id").GetInt64()}/accept", new { });
        Assert.False(string.IsNullOrEmpty(t.GetProperty("ticket").GetString()), "an accepted invite is a ticket into the private game");
        // a second invite that runs out
        await Befriend(host, other);
        await host.Post("invites", new { sessionId = id, publicId = other.PublicId });
        long oid = (await other.GetJson("invites"))[0].GetProperty("id").GetInt64();
        s.Advance(TimeSpan.FromSeconds(SessionsApi.InviteSeconds + 1));
        await host.Post($"sessions/{id}/heartbeat", new { players = 1 });
        Assert.Equal(0, (await other.GetJson("invites")).GetArrayLength());
        Assert.Equal("invite_expired", (await Server.Error(await other.Post($"invites/{oid}/accept", new { }))).code);
        // declined invites are gone
        await host.Post("invites", new { sessionId = id, publicId = other.PublicId });
        long did = (await other.GetJson("invites"))[0].GetProperty("id").GetInt64();
        (await other.Post($"invites/{did}/decline", new { })).EnsureSuccessStatusCode();
        Assert.Equal(0, (await other.GetJson("invites")).GetArrayLength());
    }

    [Fact]
    public async Task Favorites_ShowTheHostsActiveGame()
    {
        await using var s = new Server();
        var host = await s.Player("Host"); var fan = await s.Player("Fan");
        (await fan.Post("me/favorites", new { publicId = host.PublicId })).EnsureSuccessStatusCode();
        Assert.Equal(System.Text.Json.JsonValueKind.Null, (await fan.GetJson("me/favorites"))[0].GetProperty("activeSession").ValueKind);
        string id = await Host(host, "public");
        Assert.Equal(id, (await fan.GetJson("me/favorites"))[0].GetProperty("activeSession").GetString());
        (await fan.Http.DeleteAsync("me/favorites/" + host.PublicId)).EnsureSuccessStatusCode();
        Assert.Equal(0, (await fan.GetJson("me/favorites")).GetArrayLength());
    }
}
