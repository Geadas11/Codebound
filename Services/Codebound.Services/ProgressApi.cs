using Microsoft.Data.Sqlite;

namespace Codebound.Services;

public record StatBatch(string? BatchId, long? PeriodStart, long? PeriodEnd, Dictionary<string, long>? Deltas);
public record SaveReq(int? BaseVersion, string? Sha256, long? ClientTime, string? Device, string? Data);

/// <summary>
/// Statistics, achievements and cloud saves.
/// Statistics: the game sends batches (playing time windows with counts); the service never trusts them blindly — each
/// count is capped by what is physically possible in the batch's time window, windows cannot overlap (so the same
/// minutes are not counted twice), and a batch id makes every upload idempotent (offline queues can retry safely).
/// These stay "self-reported". "Verified" figures are the ones the service saw itself: games hosted and joined, players
/// met, friends.
/// Cloud saves: versioned, with SHA-256 integrity and optimistic concurrency (upload says which version it was based on;
/// a different latest version is a conflict for the player to resolve). Older versions are kept for recovery.
/// </summary>
public static class ProgressApi
{
    /// <summary>Per second of play: what nobody can beat honestly. Plus a small burst for short windows.</summary>
    public static readonly Dictionary<string, double> Rates = new()
    {
        ["playtime_seconds"] = 1, ["worlds_created"] = 0.01, ["buildings_built"] = 5, ["resources_mined"] = 20, ["items_produced"] = 60,
        ["programs_run"] = 2, ["code_lines"] = 2_000_000, ["distance"] = 15, ["research_unlocked"] = 0.05, ["space_launches"] = 0.01,
    };
    public static readonly string[] Languages = { "javascript", "typescript", "python", "sql", "php", "csharp", "c", "cpp" };
    public const long MaxBatchSeconds = 7 * 86400, ClockSkew = 300;
    public const int MaxSlots = 10, KeepVersions = 5, MaxSaveBytes = 8 * 1024 * 1024;

    public sealed record Achievement(string Id, string Name, Func<Dictionary<string, long>, bool> Done);
    public static readonly Achievement[] Achievements =
    {
        new("first_hour", "First hour", s => V(s, "playtime_seconds") >= 3600),
        new("builder", "Builder", s => V(s, "buildings_built") >= 100),
        new("industrialist", "Industrialist", s => V(s, "items_produced") >= 10_000),
        new("programmer", "Programmer", s => V(s, "programs_run") >= 10),
        new("polyglot", "Polyglot", s => V(s, "languages_used") >= 3),
        new("explorer", "Explorer", s => V(s, "distance") >= 10_000),
        new("scientist", "Scientist", s => V(s, "research_unlocked") >= 10),
        new("astronaut", "Astronaut", s => V(s, "space_launches") >= 1),
        new("host", "Host", s => V(s, "v_sessions_hosted") >= 1),        // verified by the service
        new("team_player", "Team player", s => V(s, "v_players_met") >= 3),
        new("friendly", "Friendly", s => V(s, "v_friends") >= 1),
    };
    static long V(Dictionary<string, long> s, string k) => s.TryGetValue(k, out var v) ? v : 0;

    /// <summary>Self-reported counts plus the verified ones (prefixed v_).</summary>
    public static Dictionary<string, long> AllStats(SqliteConnection c, long a)
    {
        var s = new Dictionary<string, long>();
        int langs = 0;
        foreach (var r in Db.Rows(c, "SELECT key, value FROM stats WHERE account_id = $a", ("$a", a)))
        {
            string k = (string)r["key"]!;
            if (k.StartsWith("lang:")) langs++; else s[k] = (long)r["value"]!;
        }
        s["languages_used"] = langs;
        s["v_sessions_hosted"] = (long)Db.Scalar(c, "SELECT COUNT(*) FROM game_sessions WHERE host_id = $a", ("$a", a))!;
        s["v_sessions_joined"] = (long)Db.Scalar(c, "SELECT COUNT(*) FROM session_members WHERE account_id = $a", ("$a", a))!;
        s["v_players_met"] = (long)Db.Scalar(c, @"SELECT COUNT(DISTINCT other) FROM (
              SELECT m2.account_id AS other FROM session_members m1 JOIN session_members m2 ON m2.session_id = m1.session_id AND m2.account_id <> m1.account_id WHERE m1.account_id = $a
              UNION SELECT s.host_id FROM session_members m JOIN game_sessions s ON s.id = m.session_id WHERE m.account_id = $a
              UNION SELECT m.account_id FROM game_sessions s JOIN session_members m ON m.session_id = s.id WHERE s.host_id = $a)", ("$a", a))!;
        s["v_friends"] = (long)Db.Scalar(c, "SELECT COUNT(*) FROM friendships WHERE a = $a OR b = $a", ("$a", a))!;
        return s;
    }

    /// <summary>Unlocks what the stats now reach; returns the new ones.</summary>
    public static List<string> Evaluate(SqliteConnection c, long a, long now)
    {
        var fresh = new List<string>();
        if (Db.Scalar(c, "SELECT stats_consent FROM profiles WHERE account_id = $a", ("$a", a)) is not 1L) return fresh;
        var s = AllStats(c, a);
        foreach (var ach in Achievements)
            if (ach.Done(s) && Db.Exec(c, "INSERT OR IGNORE INTO achievements(account_id, achievement, unlocked_at) VALUES($a,$i,$n)", ("$a", a), ("$i", ach.Id), ("$n", now)) > 0)
            {
                fresh.Add(ach.Id);
                AuthApi.Audit(c, a, "achievement", now, null);
            }
        return fresh;
    }

    static string SlotName(string slot)
    {
        if (slot.Length < 1 || slot.Length > 32) throw new ApiError(400, "bad_slot", "Slot names have 1 to 32 characters.");
        foreach (char ch in slot) if (!(char.IsAsciiLetterOrDigit(ch) || ch == '_' || ch == '-')) throw new ApiError(400, "bad_slot", "Slot names use letters, digits, _ and -.");
        return slot.ToLowerInvariant();
    }

    static object Meta(Dictionary<string, object?> r) => new { slot = r["slot"], version = r["version"], sha256 = r["sha256"], size = r["size"], clientTime = r["client_time"], serverTime = r["server_time"], device = r["device"] };

    public static void Map(WebApplication app)
    {
        app.MapPost("/stats/sync", (StatBatch b, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            string id = (b.BatchId ?? "").Trim();
            if (id.Length < 8 || id.Length > 64) throw new ApiError(400, "bad_batch", "A batch needs an id.");
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            if (Db.Scalar(c, "SELECT stats_consent FROM profiles WHERE account_id = $a", ("$a", me.AccountId)) is not 1L) throw new ApiError(403, "stats_off", "Statistics are switched off for this account.");
            if (Db.Scalar(c, "SELECT 1 FROM stat_batches WHERE account_id = $a AND batch_id = $b", ("$a", me.AccountId), ("$b", id)) != null)
                return Results.Ok(new { duplicate = true, accepted = new Dictionary<string, long>(), clamped = new List<string>(), achievements = new List<string>() });   // already counted: a retry
            long start = b.PeriodStart ?? 0, end = b.PeriodEnd ?? 0;
            long created = (long)Db.Scalar(c, "SELECT created_at FROM accounts WHERE id = $a", ("$a", me.AccountId))!;
            if (end <= start || end - start > MaxBatchSeconds || end > now + ClockSkew || start < created - ClockSkew) throw new ApiError(400, "bad_period", "That play period is not possible.");
            long lastEnd = Db.Scalar(c, "SELECT MAX(period_end) FROM stat_batches WHERE account_id = $a", ("$a", me.AccountId)) as long? ?? 0;
            long seconds = Math.Max(0, end - Math.Max(start, lastEnd));   // time already counted by an earlier batch is not counted again
            var accepted = new Dictionary<string, long>(); var clamped = new List<string>();
            foreach (var (key, value) in b.Deltas ?? new())
            {
                if (value < 0) throw new ApiError(400, "bad_value", "Counts cannot go down.");
                if (key.StartsWith("lang:"))
                {
                    if (Array.IndexOf(Languages, key.Substring(5)) < 0) throw new ApiError(400, "bad_key", "Unknown language.");
                    if (value > 0) { Db.Exec(c, "INSERT OR IGNORE INTO stats(account_id, key, value) VALUES($a,$k,1)", ("$a", me.AccountId), ("$k", key)); accepted[key] = 1; }
                    continue;
                }
                if (!Rates.TryGetValue(key, out double rate)) throw new ApiError(400, "bad_key", $"Unknown statistic '{key}'.");
                long cap = (long)Math.Floor(rate * seconds) + (rate < 1 && seconds > 0 ? 1 : 0);
                long v = Math.Min(value, cap);
                if (v < value) clamped.Add(key);
                if (v == 0) continue;
                Db.Exec(c, "INSERT INTO stats(account_id, key, value) VALUES($a,$k,$v) ON CONFLICT(account_id, key) DO UPDATE SET value = value + $v", ("$a", me.AccountId), ("$k", key), ("$v", v));
                accepted[key] = v;
            }
            Db.Exec(c, "INSERT INTO stat_batches(account_id, batch_id, period_start, period_end, received_at) VALUES($a,$b,$s,$e,$n)",
                ("$a", me.AccountId), ("$b", id), ("$s", start), ("$e", Math.Max(end, lastEnd)), ("$n", now));
            var fresh = Evaluate(c, me.AccountId, now);
            tx.Commit();
            return Results.Ok(new { duplicate = false, accepted, clamped, achievements = fresh });
        });

        app.MapGet("/me/stats", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            Evaluate(c, me.AccountId, clock.Now());   // verified ones may have changed (a friend, a game)
            var all = AllStats(c, me.AccountId);
            return new
            {
                stats = all.Where(kv => !kv.Key.StartsWith("v_")).ToDictionary(kv => kv.Key, kv => kv.Value),
                verified = all.Where(kv => kv.Key.StartsWith("v_")).ToDictionary(kv => kv.Key.Substring(2), kv => kv.Value),
                achievements = Db.Rows(c, "SELECT achievement, unlocked_at AS unlockedAt FROM achievements WHERE account_id = $a ORDER BY unlocked_at", ("$a", me.AccountId)),
                lastPeriodEnd = Db.Scalar(c, "SELECT MAX(period_end) FROM stat_batches WHERE account_id = $a", ("$a", me.AccountId)) ?? 0L,
                definitions = Achievements.Select(a => new { id = a.Id, name = a.Name }),
            };
        });

        // ---------------------------------------------------------------- cloud saves
        app.MapGet("/saves", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return Db.Rows(c, @"SELECT slot, version, sha256, size, client_time, server_time, device FROM cloud_saves s WHERE account_id = $a
                                AND version = (SELECT MAX(version) FROM cloud_saves t WHERE t.account_id = s.account_id AND t.slot = s.slot) ORDER BY server_time DESC", ("$a", me.AccountId)).Select(Meta);
        });

        app.MapGet("/saves/{slot}", (string slot, int? version, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            slot = SlotName(slot);
            using var c = db.Open();
            var r = Db.Row(c, @"SELECT * FROM cloud_saves WHERE account_id = $a AND slot = $s AND version = COALESCE($v, (SELECT MAX(version) FROM cloud_saves WHERE account_id = $a AND slot = $s))",
                ("$a", me.AccountId), ("$s", slot), ("$v", version)) ?? throw new ApiError(404, "not_found", "No such save.");
            return Results.Ok(new { meta = Meta(r), data = Convert.ToBase64String((byte[])r["data"]!) });
        });

        app.MapGet("/saves/{slot}/versions", (string slot, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return Db.Rows(c, "SELECT slot, version, sha256, size, client_time, server_time, device FROM cloud_saves WHERE account_id = $a AND slot = $s ORDER BY version DESC", ("$a", me.AccountId), ("$s", SlotName(slot))).Select(Meta);
        });

        app.MapPut("/saves/{slot}", (string slot, SaveReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            slot = SlotName(slot);
            byte[] data;
            try { data = Convert.FromBase64String(r.Data ?? ""); } catch (FormatException) { throw new ApiError(400, "bad_data", "The save is not readable."); }
            if (data.Length == 0 || data.Length > MaxSaveBytes) throw new ApiError(413, "too_big", $"Saves up to {MaxSaveBytes / (1024 * 1024)} MB.");
            string sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));
            if (!sha.Equals(r.Sha256 ?? "", StringComparison.OrdinalIgnoreCase)) throw new ApiError(400, "integrity", "The save was damaged on the way. Try again.");
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            long latest = Db.Scalar(c, "SELECT MAX(version) FROM cloud_saves WHERE account_id = $a AND slot = $s", ("$a", me.AccountId), ("$s", slot)) as long? ?? 0;
            if ((r.BaseVersion ?? 0) != latest)   // someone (another device) uploaded since this copy was downloaded
            {
                var cur = Db.Row(c, "SELECT * FROM cloud_saves WHERE account_id = $a AND slot = $s AND version = $v", ("$a", me.AccountId), ("$s", slot), ("$v", latest))!;
                return Results.Json(new { error = "conflict", message = "The cloud has a newer version of this save.", server = Meta(cur) }, statusCode: 409);
            }
            if (latest == 0 && (long)Db.Scalar(c, "SELECT COUNT(DISTINCT slot) FROM cloud_saves WHERE account_id = $a", ("$a", me.AccountId))! >= MaxSlots)
                throw new ApiError(409, "slot_limit", $"{MaxSlots} cloud saves at most.");
            string device = string.IsNullOrWhiteSpace(r.Device) ? "unknown device" : r.Device.Trim()[..Math.Min(r.Device.Trim().Length, 64)];
            Db.Exec(c, "INSERT INTO cloud_saves(account_id, slot, version, sha256, size, client_time, server_time, device, data) VALUES($a,$s,$v,$h,$z,$ct,$st,$d,$data)",
                ("$a", me.AccountId), ("$s", slot), ("$v", latest + 1), ("$h", sha), ("$z", data.Length), ("$ct", r.ClientTime ?? now), ("$st", now), ("$d", device), ("$data", data));
            Db.Exec(c, "DELETE FROM cloud_saves WHERE account_id = $a AND slot = $s AND version <= $v", ("$a", me.AccountId), ("$s", slot), ("$v", latest + 1 - KeepVersions));
            tx.Commit();
            return Results.Ok(new { slot, version = latest + 1, sha256 = sha, serverTime = now });
        });

        app.MapDelete("/saves/{slot}", (string slot, string? confirm, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            if (confirm != "yes") throw new ApiError(400, "confirm", "Add ?confirm=yes to delete a cloud save (your local copy is not touched).");
            using var c = db.Open();
            Db.Exec(c, "DELETE FROM cloud_saves WHERE account_id = $a AND slot = $s", ("$a", me.AccountId), ("$s", SlotName(slot)));
            return Results.Ok(new { message = "Cloud save deleted. Local saves are untouched." });
        });
    }
}
