using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Codebound.Services.Tests;

public class ProgressTests
{
    static long Now(Server s) => s.Clock.GetUtcNow().ToUnixTimeSeconds();

    [Fact]
    public async Task Stats_AreCappedByPlayTime_NotCountedTwice_AndRetriesAreSafe()
    {
        await using var s = new Server();
        var p = await s.Player("Ana");
        s.Advance(TimeSpan.FromHours(2)); p = await p.Renew();
        long now = Now(s);
        var r = await p.PostJson("stats/sync", new { batchId = "batch-0001", periodStart = now - 600, periodEnd = now,
            deltas = new Dictionary<string, long> { ["playtime_seconds"] = 600, ["buildings_built"] = 99999, ["lang:python"] = 1 } });
        Assert.Equal(600, r.GetProperty("accepted").GetProperty("playtime_seconds").GetInt64());
        Assert.Equal(3000, r.GetProperty("accepted").GetProperty("buildings_built").GetInt64());   // 5 per second at most
        Assert.Contains("buildings_built", r.GetProperty("clamped").EnumerateArray().Select(e => e.GetString()));
        var again = await p.PostJson("stats/sync", new { batchId = "batch-0001", periodStart = now - 600, periodEnd = now, deltas = new Dictionary<string, long> { ["playtime_seconds"] = 600 } });
        Assert.True(again.GetProperty("duplicate").GetBoolean());
        var overlap = await p.PostJson("stats/sync", new { batchId = "batch-0002", periodStart = now - 600, periodEnd = now, deltas = new Dictionary<string, long> { ["playtime_seconds"] = 600 } });
        Assert.False(overlap.GetProperty("accepted").TryGetProperty("playtime_seconds", out _), "the same 10 minutes again count for nothing");
        var stats = await p.GetJson("me/stats");
        Assert.Equal(600, stats.GetProperty("stats").GetProperty("playtime_seconds").GetInt64());
        Assert.Equal(1, stats.GetProperty("stats").GetProperty("languages_used").GetInt64());
        Assert.Equal("bad_key", (await Server.Error(await p.Post("stats/sync", new { batchId = "batch-0003", periodStart = now, periodEnd = now + 10, deltas = new Dictionary<string, long> { ["gold"] = 5 } }))).code);
        Assert.Equal("bad_period", (await Server.Error(await p.Post("stats/sync", new { batchId = "batch-0004", periodStart = now, periodEnd = now + 86400, deltas = new Dictionary<string, long>() }))).code);   // the future
        Assert.Equal("bad_value", (await Server.Error(await p.Post("stats/sync", new { batchId = "batch-0005", periodStart = now, periodEnd = now + 10, deltas = new Dictionary<string, long> { ["distance"] = -5 } }))).code);
    }

    [Fact]
    public async Task OfflineBatches_SyncLaterInOrder()
    {
        await using var s = new Server();
        var p = await s.Player("Bob");
        long t0 = Now(s);
        s.Advance(TimeSpan.FromDays(2)); p = await p.Renew();   // two days offline, three play sessions queued
        for (int i = 0; i < 3; i++)
            (await p.Post("stats/sync", new { batchId = "offline-" + i, periodStart = t0 + i * 3600, periodEnd = t0 + i * 3600 + 1800, deltas = new Dictionary<string, long> { ["playtime_seconds"] = 1800, ["distance"] = 100 } })).EnsureSuccessStatusCode();
        var st = (await p.GetJson("me/stats")).GetProperty("stats");
        Assert.Equal(5400, st.GetProperty("playtime_seconds").GetInt64());
        Assert.Equal(300, st.GetProperty("distance").GetInt64());
    }

    [Fact]
    public async Task Achievements_FromStats_AndFromWhatTheServiceSaw()
    {
        await using var s = new Server();
        var p = await s.Player("Cid");
        s.Advance(TimeSpan.FromHours(2)); p = await p.Renew();
        long now = Now(s);
        var r = await p.PostJson("stats/sync", new { batchId = "batch-ach1", periodStart = now - 3600, periodEnd = now, deltas = new Dictionary<string, long> { ["playtime_seconds"] = 3600, ["space_launches"] = 1 } });
        var fresh = r.GetProperty("achievements").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("first_hour", fresh); Assert.Contains("astronaut", fresh);
        await p.PostJson("sessions", new { name = "Cid's world", visibility = "public", port = 47820, version = 3 });
        var stats = await p.GetJson("me/stats");
        Assert.Equal(1, stats.GetProperty("verified").GetProperty("sessions_hosted").GetInt64());
        Assert.Contains("host", stats.GetProperty("achievements").EnumerateArray().Select(e => e.GetProperty("achievement").GetString()));
    }

    [Fact]
    public async Task WithoutConsent_NoStatsAreKept()
    {
        await using var s = new Server();
        var p = await s.Player("Dan");
        s.Advance(TimeSpan.FromHours(1)); p = await p.Renew(); long now = Now(s);
        await p.PostJson("stats/sync", new { batchId = "batch-c1", periodStart = now - 60, periodEnd = now, deltas = new Dictionary<string, long> { ["playtime_seconds"] = 60 } });
        (await p.Http.PatchAsJsonAsync("me/profile", new { statsConsent = false })).EnsureSuccessStatusCode();
        Assert.Equal("stats_off", (await Server.Error(await p.Post("stats/sync", new { batchId = "batch-c2", periodStart = now, periodEnd = now + 10, deltas = new Dictionary<string, long>() }))).code);
        Assert.False((await p.GetJson("me/stats")).GetProperty("stats").TryGetProperty("playtime_seconds", out _), "withdrawn consent deletes what was kept");
    }

    static object Upload(byte[] data, int baseVersion, string device = "pc") =>
        new { baseVersion, sha256 = Convert.ToHexString(SHA256.HashData(data)), clientTime = 1, device, data = Convert.ToBase64String(data) };

    [Fact]
    public async Task CloudSaves_VersionsIntegrityConflictsAndRecovery()
    {
        await using var s = new Server();
        var p = await s.Player("Eve");
        var v1 = Encoding.UTF8.GetBytes("CODEBOUND 1\nseed 5\nend\n");
        var up = await Server.Json(await p.Http.PutAsJsonAsync("saves/world1", Upload(v1, 0)));
        Assert.Equal(1, up.GetProperty("version").GetInt32());
        // a second device that also started from nothing: conflict, nothing overwritten
        var conflict = await p.Http.PutAsJsonAsync("saves/world1", Upload(Encoding.UTF8.GetBytes("other device"), 0, "laptop"));
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        // damaged on the way
        var bad = new { baseVersion = 1, sha256 = "00", clientTime = 1, device = "pc", data = Convert.ToBase64String(v1) };
        Assert.Equal("integrity", (await Server.Error(await p.Http.PutAsJsonAsync("saves/world1", bad))).code);
        for (int i = 2; i <= 7; i++) (await p.Http.PutAsJsonAsync("saves/world1", Upload(Encoding.UTF8.GetBytes("version " + i), i - 1))).EnsureSuccessStatusCode();
        var versions = await p.GetJson("saves/world1/versions");
        Assert.Equal(ProgressApi.KeepVersions, versions.GetArrayLength());           // older ones pruned, recent ones kept for recovery
        var old = await p.GetJson("saves/world1?version=4");
        Assert.Equal("version 4", Encoding.UTF8.GetString(Convert.FromBase64String(old.GetProperty("data").GetString()!)));
        var latest = await p.GetJson("saves/world1");
        Assert.Equal(7, latest.GetProperty("meta").GetProperty("version").GetInt32());
        Assert.Equal(1, (await p.GetJson("saves")).GetArrayLength());
        Assert.Equal("confirm", (await Server.Error(await p.Http.DeleteAsync("saves/world1"))).code);
        (await p.Http.DeleteAsync("saves/world1?confirm=yes")).EnsureSuccessStatusCode();
        Assert.Equal(0, (await p.GetJson("saves")).GetArrayLength());
    }

    [Fact]
    public async Task CloudSaves_HaveLimits()
    {
        await using var s = new Server();
        var p = await s.Player("Fay");
        for (int i = 0; i < ProgressApi.MaxSlots; i++) (await p.Http.PutAsJsonAsync("saves/s" + i, Upload(new byte[] { 1, 2, 3 }, 0))).EnsureSuccessStatusCode();
        Assert.Equal("slot_limit", (await Server.Error(await p.Http.PutAsJsonAsync("saves/one-more", Upload(new byte[] { 1 }, 0)))).code);
        Assert.Equal("bad_slot", (await Server.Error(await p.Http.PutAsJsonAsync("saves/bad%20slot", Upload(new byte[] { 1 }, 0)))).code);
        Assert.Equal(HttpStatusCode.Unauthorized, (await s.Client().GetAsync("saves")).StatusCode);
    }
}
