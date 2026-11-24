using System.Net;
using System.Net.Http.Json;

namespace Codebound.Services.Tests;

public class SocialTests
{
    static async Task<string[]> Names(Player p, string path, string prop = "displayName")
    {
        var a = await p.GetJson(path);
        return a.EnumerateArray().Select(e => e.GetProperty(prop).GetString()!).ToArray();
    }

    [Fact]
    public async Task FriendRequests_AcceptDeclineCancelRemove_WithoutDuplicates()
    {
        await using var s = new Server();
        var ana = await s.Player("Ana"); var bob = await s.Player("Bob"); var cid = await s.Player("Cid");
        (await ana.Post("friends/requests", new { publicId = bob.PublicId })).EnsureSuccessStatusCode();
        Assert.Equal("already_sent", (await Server.Error(await ana.Post("friends/requests", new { publicId = bob.PublicId }))).code);
        Assert.Equal("self", (await Server.Error(await ana.Post("friends/requests", new { publicId = ana.PublicId }))).code);
        var incoming = (await bob.GetJson("friends/requests")).GetProperty("incoming");
        Assert.Equal(1, incoming.GetArrayLength());
        (await bob.Post($"friends/requests/{incoming[0].GetProperty("id").GetInt64()}/accept", new { })).EnsureSuccessStatusCode();
        Assert.Equal(new[] { "Bob" }, await Names(ana, "friends"));
        Assert.Equal(new[] { "Ana" }, await Names(bob, "friends"));
        Assert.Equal("already_friends", (await Server.Error(await bob.Post("friends/requests", new { publicId = ana.PublicId }))).code);
        // crossing requests: the second one is a yes
        (await ana.Post("friends/requests", new { publicId = cid.PublicId })).EnsureSuccessStatusCode();
        var crossed = await cid.PostJson("friends/requests", new { publicId = ana.PublicId });
        Assert.True(crossed.GetProperty("friends").GetBoolean());
        Assert.Equal(0, (await ana.GetJson("friends/requests")).GetProperty("outgoing").GetArrayLength());
        // decline and cancel
        (await bob.Post("friends/requests", new { publicId = cid.PublicId })).EnsureSuccessStatusCode();
        long id = (await cid.GetJson("friends/requests")).GetProperty("incoming")[0].GetProperty("id").GetInt64();
        (await cid.Post($"friends/requests/{id}/decline", new { })).EnsureSuccessStatusCode();
        (await bob.Post("friends/requests", new { publicId = cid.PublicId })).EnsureSuccessStatusCode();
        id = (await bob.GetJson("friends/requests")).GetProperty("outgoing")[0].GetProperty("id").GetInt64();
        Assert.Equal(HttpStatusCode.NotFound, (await cid.Http.DeleteAsync($"friends/requests/{id}")).StatusCode);   // only the sender can cancel
        (await bob.Http.DeleteAsync($"friends/requests/{id}")).EnsureSuccessStatusCode();
        // remove
        (await ana.Http.DeleteAsync("friends/" + bob.PublicId)).EnsureSuccessStatusCode();
        Assert.Equal(new[] { "Cid" }, await Names(ana, "friends"));
        Assert.Empty(await Names(bob, "friends"));
    }

    [Fact]
    public async Task Blocking_HidesPlayersFromEachOther_Everywhere()
    {
        await using var s = new Server();
        var ana = await s.Player("Ana"); var bob = await s.Player("Bobby");
        (await ana.Post("friends/requests", new { publicId = bob.PublicId })).EnsureSuccessStatusCode();
        (await bob.Post("blocks", new { publicId = ana.PublicId })).EnsureSuccessStatusCode();
        Assert.Equal(0, (await bob.GetJson("friends/requests")).GetProperty("incoming").GetArrayLength());   // the pending request went
        Assert.Equal("not_allowed", (await Server.Error(await ana.Post("friends/requests", new { publicId = bob.PublicId }))).code);
        Assert.Equal(HttpStatusCode.NotFound, (await ana.Get("profiles/" + bob.PublicId)).StatusCode);
        Assert.Empty(await Names(ana, "profiles/search?q=Bob"));
        Assert.Empty(await Names(bob, "profiles/search?q=Ana"));
        Assert.Equal(new[] { "Ana" }, await Names(bob, "blocks"));
        (await bob.Http.DeleteAsync("blocks/" + ana.PublicId)).EnsureSuccessStatusCode();
        Assert.Equal(new[] { "Bobby" }, await Names(ana, "profiles/search?q=Bob"));
    }

    [Fact]
    public async Task Privacy_ProfilesSearchAndRequests()
    {
        await using var s = new Server();
        var ana = await s.Player("Ana"); var bob = await s.Player("Bob");
        (await bob.Http.PatchAsJsonAsync("me/profile", new { visibility = "private", allowRequests = "nobody", bio = "I build <b>trains</b>", avatar = 3 })).EnsureSuccessStatusCode();
        var card = await ana.GetJson("profiles/" + bob.PublicId);
        Assert.Equal("private", card.GetProperty("visibility").GetString());
        Assert.False(card.TryGetProperty("bio", out _), "a private profile shows only its card");
        Assert.Empty(await Names(ana, "profiles/search?q=Bob"));                         // private: only by exact id
        Assert.Single(await Names(ana, "profiles/search?q=" + bob.PublicId));
        Assert.Equal("not_allowed", (await Server.Error(await ana.Post("friends/requests", new { publicId = bob.PublicId }))).code);
        var mine = await bob.GetJson("profiles/" + bob.PublicId);
        Assert.Equal("I build ‹b›trains‹/b›", mine.GetProperty("bio").GetString());     // no rich-text tags
        Assert.Equal("bad_query", (await Server.Error(await ana.Get("profiles/search?q=ab"))).code);
        Assert.Equal("name_taken", (await Server.Error(await ana.Http.PatchAsJsonAsync("me/profile", new { displayName = "bob" }))).code);
    }

    [Fact]
    public async Task Presence_OnlineBusyInGameAndOffline()
    {
        await using var s = new Server();
        var ana = await s.Player("Ana"); var bob = await s.Player("Bob");
        await ana.Post("friends/requests", new { publicId = bob.PublicId });
        long id = (await bob.GetJson("friends/requests")).GetProperty("incoming")[0].GetProperty("id").GetInt64();
        await bob.Post($"friends/requests/{id}/accept", new { });
        (await bob.Post("me/presence", new { status = "busy" })).EnsureSuccessStatusCode();
        Assert.Equal("busy", (await ana.GetJson("friends"))[0].GetProperty("presence").GetString());
        var game = await bob.PostJson("sessions", new { name = "Bob's base", visibility = "friends", port = 47820, version = 3 });
        await bob.Post("me/presence", new { status = "online", sessionId = game.GetProperty("sessionId").GetString() });
        var f = (await ana.GetJson("friends"))[0];
        Assert.Equal("in_game", f.GetProperty("presence").GetString());
        Assert.Equal(game.GetProperty("sessionId").GetString(), f.GetProperty("sessionId").GetString());
        s.Advance(TimeSpan.FromMinutes(3));
        (await ana.Post("me/presence", new { status = "online" })).EnsureSuccessStatusCode();
        Assert.Equal("offline", (await ana.GetJson("friends"))[0].GetProperty("presence").GetString());   // no heartbeat for 3 minutes
        await bob.Http.PatchAsJsonAsync("me/profile", new { showPresence = false });
        await bob.Post("me/presence", new { status = "online" });
        Assert.Equal("offline", (await ana.GetJson("friends"))[0].GetProperty("presence").GetString());   // hidden
    }

    [Fact]
    public async Task Reports_AndNotifications()
    {
        await using var s = new Server();
        var ana = await s.Player("Ana"); var bob = await s.Player("Bob");
        Assert.Equal("bad_reason", (await Server.Error(await ana.Post("reports", new { publicId = bob.PublicId, reason = "" }))).code);
        for (int i = 0; i < SocialApi.MaxReportsPerDay; i++) (await ana.Post("reports", new { publicId = bob.PublicId, reason = "spam in chat" })).EnsureSuccessStatusCode();
        Assert.Equal("too_many_reports", (await Server.Error(await ana.Post("reports", new { publicId = bob.PublicId, reason = "spam in chat" }))).code);
        await ana.Post("friends/requests", new { publicId = bob.PublicId });
        Assert.Equal(1, (await bob.GetJson("me/notifications")).GetProperty("friendRequests").GetInt64());
    }
}
