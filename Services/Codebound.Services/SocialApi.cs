using Microsoft.Data.Sqlite;

namespace Codebound.Services;

public record ProfileReq(string? DisplayName, int? Avatar, string? Bio, string? Visibility, string? AllowRequests, bool? ShowPresence, bool? StatsConsent);
public record PresenceReq(string? Status, string? SessionId);
public record PublicIdReq(string? PublicId);
public record ReportReq(string? PublicId, string? Reason);

/// <summary>
/// Profiles, search, presence, friends, blocks, reports, notifications, favourites. Blocking either way hides two
/// players from each other everywhere (search, profiles, requests, invites, friend lists, sessions); answers to a
/// blocked player look like "not found" or "not allowed", never "blocked".
/// </summary>
public static class SocialApi
{
    public const int OnlineSeconds = 120, MaxFriends = 500, MaxPendingOut = 50, MaxReportsPerDay = 10, Avatars = 16;

    public static long IdOf(SqliteConnection c, string? publicId) =>
        Db.Scalar(c, "SELECT p.account_id FROM profiles p JOIN accounts a ON a.id = p.account_id WHERE p.public_id = $p AND a.deleted_at IS NULL", ("$p", (publicId ?? "").Trim().ToUpperInvariant())) as long?
        ?? throw new ApiError(404, "not_found", "No player with that id.");

    public static bool Blocked(SqliteConnection c, long a, long b) =>
        Db.Scalar(c, "SELECT 1 FROM blocks WHERE (blocker = $a AND blocked = $b) OR (blocker = $b AND blocked = $a)", ("$a", a), ("$b", b)) != null;

    public static bool Friends(SqliteConnection c, long a, long b) =>
        Db.Scalar(c, "SELECT 1 FROM friendships WHERE a = $x AND b = $y", ("$x", Math.Min(a, b)), ("$y", Math.Max(a, b))) != null;

    /// <summary>online | busy | in_game | offline | hidden (the player hides presence; shown as offline to others).</summary>
    public static string Presence(Dictionary<string, object?> p, long now)
    {
        if ((long)p["show_presence"]! == 0) return "offline";
        if ((long)p["last_seen"]! < now - OnlineSeconds) return "offline";
        if (p["current_session"] != null) return "in_game";
        return (string)p["status"]! == "busy" ? "busy" : "online";
    }

    static string Clean(string s)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char ch in s) if (!char.IsControl(ch) || ch == '\n') sb.Append(ch == '<' ? '‹' : ch == '>' ? '›' : ch);   // no rich-text tags in the game's UI
        return sb.ToString().Trim();
    }

    /// <summary>What `viewer` may see of `target`: everything public, or a card (id, name, avatar) when the profile is private to them.</summary>
    public static object View(SqliteConnection c, long viewer, long target, long now)
    {
        var p = Db.Row(c, "SELECT * FROM profiles WHERE account_id = $t", ("$t", target))!;
        bool self = viewer == target, friend = Friends(c, viewer, target);
        string vis = (string)p["visibility"]!;
        bool full = self || vis == "public" || (vis == "friends" && friend);
        var card = new Dictionary<string, object?> { ["publicId"] = p["public_id"], ["displayName"] = p["display_name"], ["avatar"] = p["avatar"], ["friend"] = friend,
                                                    ["canRequest"] = !friend && !self && (string)p["allow_requests"]! == "everyone" };
        if (!full) { card["visibility"] = "private"; return card; }
        card["bio"] = p["bio"]; card["createdAt"] = p["created_at"]; card["visibility"] = vis;
        card["presence"] = self || friend || vis == "public" ? Presence(p, now) : "offline";
        if ((long)p["stats_consent"]! == 1)
        {
            card["stats"] = Db.Rows(c, "SELECT key, value FROM stats WHERE account_id = $t AND key NOT LIKE 'lang:%'", ("$t", target));
            card["achievements"] = Db.Rows(c, "SELECT achievement, unlocked_at AS unlockedAt FROM achievements WHERE account_id = $t ORDER BY unlocked_at", ("$t", target));
        }
        card["recentActivity"] = Db.Rows(c, "SELECT event, at FROM audit WHERE account_id = $t AND event IN ('session_hosted','session_joined','achievement') ORDER BY at DESC LIMIT 10", ("$t", target));
        return card;
    }

    public static void Map(WebApplication app)
    {
        app.MapPatch("/me/profile", (ProfileReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            if (r.DisplayName != null)
            {
                string n = AuthApi.CheckName(r.DisplayName);
                if (Db.Scalar(c, "SELECT 1 FROM profiles WHERE display_name = $n AND account_id <> $a", ("$n", n), ("$a", me.AccountId)) != null) throw new ApiError(409, "name_taken", "That player name is taken.");
                Db.Exec(c, "UPDATE profiles SET display_name = $n WHERE account_id = $a", ("$n", n), ("$a", me.AccountId));
            }
            if (r.Avatar is int av) { if (av < 0 || av >= Avatars) throw new ApiError(400, "bad_avatar", "Pick one of the avatars."); Db.Exec(c, "UPDATE profiles SET avatar = $v WHERE account_id = $a", ("$v", av), ("$a", me.AccountId)); }
            if (r.Bio != null) { string b = Clean(r.Bio); if (b.Length > 160) throw new ApiError(400, "bio_too_long", "160 characters at most."); Db.Exec(c, "UPDATE profiles SET bio = $v WHERE account_id = $a", ("$v", b), ("$a", me.AccountId)); }
            if (r.Visibility != null) { if (r.Visibility is not ("public" or "friends" or "private")) throw new ApiError(400, "bad_value", "public, friends or private."); Db.Exec(c, "UPDATE profiles SET visibility = $v WHERE account_id = $a", ("$v", r.Visibility), ("$a", me.AccountId)); }
            if (r.AllowRequests != null) { if (r.AllowRequests is not ("everyone" or "nobody")) throw new ApiError(400, "bad_value", "everyone or nobody."); Db.Exec(c, "UPDATE profiles SET allow_requests = $v WHERE account_id = $a", ("$v", r.AllowRequests), ("$a", me.AccountId)); }
            if (r.ShowPresence is bool sp) Db.Exec(c, "UPDATE profiles SET show_presence = $v WHERE account_id = $a", ("$v", sp ? 1 : 0), ("$a", me.AccountId));
            if (r.StatsConsent is bool sc)
            {
                Db.Exec(c, "UPDATE profiles SET stats_consent = $v WHERE account_id = $a", ("$v", sc ? 1 : 0), ("$a", me.AccountId));
                if (!sc) { Db.Exec(c, "DELETE FROM stats WHERE account_id = $a", ("$a", me.AccountId)); Db.Exec(c, "DELETE FROM achievements WHERE account_id = $a", ("$a", me.AccountId)); }   // consent withdrawn: the data goes
            }
            return View(c, me.AccountId, me.AccountId, clock.Now());
        });

        app.MapGet("/profiles/{publicId}", (string publicId, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            long t = IdOf(c, publicId);
            if (Blocked(c, me.AccountId, t)) throw new ApiError(404, "not_found", "No player with that id.");
            return View(c, me.AccountId, t, clock.Now());
        });

        app.MapGet("/profiles/search", (string? q, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            q = (q ?? "").Trim();
            if (q.Length < 3 || q.Length > 16) throw new ApiError(400, "bad_query", "Search with 3 to 16 characters, or a player id (CB-...).");
            using var c = db.Open();
            string like = q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            var rows = Db.Rows(c, @"SELECT p.account_id, p.public_id, p.display_name, p.avatar, p.visibility FROM profiles p JOIN accounts a ON a.id = p.account_id
                                    WHERE a.deleted_at IS NULL AND (p.display_name LIKE $l ESCAPE '\' OR p.public_id = $id) AND p.account_id <> $me
                                    AND NOT EXISTS (SELECT 1 FROM blocks b WHERE (b.blocker = $me AND b.blocked = p.account_id) OR (b.blocker = p.account_id AND b.blocked = $me))
                                    ORDER BY length(p.display_name) LIMIT 40", ("$l", like), ("$id", q.ToUpperInvariant()), ("$me", me.AccountId));
            var list = new List<object>();
            foreach (var r in rows)
            {
                long t = (long)r["account_id"]!;
                if ((string)r["visibility"]! == "private" && (string)r["public_id"]! != q.ToUpperInvariant() && !Friends(c, me.AccountId, t)) continue;   // private: only by exact id
                list.Add(new { publicId = r["public_id"], displayName = r["display_name"], avatar = r["avatar"], friend = Friends(c, me.AccountId, t) });
                if (list.Count >= 20) break;
            }
            return list;
        }).RequireRateLimiting("search");

        app.MapPost("/me/presence", (PresenceReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            if (r.Status is not ("online" or "busy")) throw new ApiError(400, "bad_value", "online or busy.");
            using var c = db.Open();
            string? session = null;
            if (!string.IsNullOrEmpty(r.SessionId) && Db.Scalar(c, @"SELECT 1 FROM game_sessions s WHERE s.id = $s AND s.closed_at IS NULL AND (s.host_id = $a OR EXISTS
                     (SELECT 1 FROM session_members m WHERE m.session_id = s.id AND m.account_id = $a AND m.left_at IS NULL AND m.banned = 0))", ("$s", r.SessionId), ("$a", me.AccountId)) != null)
                session = r.SessionId;
            Db.Exec(c, "UPDATE profiles SET status = $s, last_seen = $n, current_session = $g WHERE account_id = $a", ("$s", r.Status), ("$n", clock.Now()), ("$g", session), ("$a", me.AccountId));
            return Results.Ok(new { presence = r.Status, inGame = session != null });
        });

        // ---------------------------------------------------------------- friends
        app.MapGet("/friends", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            var rows = Db.Rows(c, @"SELECT p.*, f.since FROM friendships f JOIN profiles p ON p.account_id = CASE WHEN f.a = $a THEN f.b ELSE f.a END
                                    WHERE f.a = $a OR f.b = $a ORDER BY p.display_name", ("$a", me.AccountId));
            var list = new List<object>();
            foreach (var p in rows)
            {
                string pres = Presence(p, now);
                list.Add(new { publicId = p["public_id"], displayName = p["display_name"], avatar = p["avatar"], presence = pres, since = p["since"],
                               sessionId = pres == "in_game" ? p["current_session"] : null });
            }
            return list;
        });

        app.MapGet("/friends/requests", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return new
            {
                incoming = Db.Rows(c, "SELECT r.id, p.public_id AS publicId, p.display_name AS displayName, p.avatar, r.created_at AS createdAt FROM friend_requests r JOIN profiles p ON p.account_id = r.from_id WHERE r.to_id = $a ORDER BY r.id", ("$a", me.AccountId)),
                outgoing = Db.Rows(c, "SELECT r.id, p.public_id AS publicId, p.display_name AS displayName, p.avatar, r.created_at AS createdAt FROM friend_requests r JOIN profiles p ON p.account_id = r.to_id WHERE r.from_id = $a ORDER BY r.id", ("$a", me.AccountId)),
            };
        });

        app.MapPost("/friends/requests", (PublicIdReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            long t = IdOf(c, r.PublicId);
            if (t == me.AccountId) throw new ApiError(400, "self", "That is you.");
            if (Blocked(c, me.AccountId, t)) throw new ApiError(403, "not_allowed", "You cannot send a request to this player.");
            if (Friends(c, me.AccountId, t)) throw new ApiError(409, "already_friends", "You are already friends.");
            if (Db.Scalar(c, "SELECT 1 FROM friend_requests WHERE from_id = $a AND to_id = $t", ("$a", me.AccountId), ("$t", t)) != null) throw new ApiError(409, "already_sent", "You already sent a request.");
            if (Db.Scalar(c, "SELECT id FROM friend_requests WHERE from_id = $t AND to_id = $a", ("$a", me.AccountId), ("$t", t)) is long back)   // they asked first: that is a yes
            {
                MakeFriends(c, me.AccountId, t, now);
                tx.Commit();
                return Results.Ok(new { friends = true, message = "You are now friends." });
            }
            if ((string)Db.Scalar(c, "SELECT allow_requests FROM profiles WHERE account_id = $t", ("$t", t))! != "everyone") throw new ApiError(403, "not_allowed", "This player does not accept friend requests.");
            if ((long)Db.Scalar(c, "SELECT COUNT(*) FROM friend_requests WHERE from_id = $a", ("$a", me.AccountId))! >= MaxPendingOut) throw new ApiError(429, "too_many_requests", "You have too many pending requests.");
            if (FriendCount(c, me.AccountId) >= MaxFriends) throw new ApiError(409, "friend_limit", $"{MaxFriends} friends at most.");
            Db.Exec(c, "INSERT INTO friend_requests(from_id, to_id, created_at) VALUES($a,$t,$n)", ("$a", me.AccountId), ("$t", t), ("$n", now));
            tx.Commit();
            return Results.Ok(new { friends = false, message = "Request sent." });
        });

        app.MapPost("/friends/requests/{id:long}/accept", (long id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            var r = Db.Row(c, "SELECT from_id FROM friend_requests WHERE id = $i AND to_id = $a", ("$i", id), ("$a", me.AccountId)) ?? throw new ApiError(404, "not_found", "No such request.");
            long from = (long)r["from_id"]!;
            if (Blocked(c, me.AccountId, from)) { Db.Exec(c, "DELETE FROM friend_requests WHERE id = $i", ("$i", id)); tx.Commit(); throw new ApiError(403, "not_allowed", "Not allowed."); }
            if (FriendCount(c, me.AccountId) >= MaxFriends) throw new ApiError(409, "friend_limit", $"{MaxFriends} friends at most.");
            MakeFriends(c, me.AccountId, from, clock.Now());
            tx.Commit();
            return Results.Ok(new { message = "You are now friends." });
        });

        app.MapPost("/friends/requests/{id:long}/decline", (long id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            if (Db.Exec(c, "DELETE FROM friend_requests WHERE id = $i AND to_id = $a", ("$i", id), ("$a", me.AccountId)) == 0) throw new ApiError(404, "not_found", "No such request.");
            return Results.Ok(new { message = "Request declined." });
        });

        app.MapDelete("/friends/requests/{id:long}", (long id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            if (Db.Exec(c, "DELETE FROM friend_requests WHERE id = $i AND from_id = $a", ("$i", id), ("$a", me.AccountId)) == 0) throw new ApiError(404, "not_found", "No such request.");
            return Results.Ok(new { message = "Request cancelled." });
        });

        app.MapDelete("/friends/{publicId}", (string publicId, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            long t = IdOf(c, publicId);
            if (Db.Exec(c, "DELETE FROM friendships WHERE a = $x AND b = $y", ("$x", Math.Min(t, me.AccountId)), ("$y", Math.Max(t, me.AccountId))) == 0) throw new ApiError(404, "not_found", "You are not friends.");
            return Results.Ok(new { message = "Friend removed." });
        });

        // ---------------------------------------------------------------- blocks and reports
        app.MapGet("/blocks", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return Db.Rows(c, "SELECT p.public_id AS publicId, p.display_name AS displayName FROM blocks b JOIN profiles p ON p.account_id = b.blocked WHERE b.blocker = $a", ("$a", me.AccountId));
        });

        app.MapPost("/blocks", (PublicIdReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            long t = IdOf(c, r.PublicId);
            if (t == me.AccountId) throw new ApiError(400, "self", "That is you.");
            Db.Exec(c, "INSERT OR IGNORE INTO blocks(blocker, blocked, created_at) VALUES($a,$t,$n)", ("$a", me.AccountId), ("$t", t), ("$n", clock.Now()));
            Db.Exec(c, "DELETE FROM friendships WHERE a = $x AND b = $y", ("$x", Math.Min(t, me.AccountId)), ("$y", Math.Max(t, me.AccountId)));
            Db.Exec(c, "DELETE FROM friend_requests WHERE (from_id = $a AND to_id = $t) OR (from_id = $t AND to_id = $a)", ("$a", me.AccountId), ("$t", t));
            Db.Exec(c, "DELETE FROM invites WHERE (from_id = $a AND to_id = $t) OR (from_id = $t AND to_id = $a)", ("$a", me.AccountId), ("$t", t));
            Db.Exec(c, "DELETE FROM favorites WHERE (account_id = $a AND host_id = $t)", ("$a", me.AccountId), ("$t", t));
            tx.Commit();
            return Results.Ok(new { message = "Player blocked." });
        });

        app.MapDelete("/blocks/{publicId}", (string publicId, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            Db.Exec(c, "DELETE FROM blocks WHERE blocker = $a AND blocked = $t", ("$a", me.AccountId), ("$t", IdOf(c, publicId)));
            return Results.Ok(new { message = "Player unblocked." });
        });

        app.MapPost("/reports", (ReportReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            string reason = Clean(r.Reason ?? "");
            if (reason.Length < 3 || reason.Length > 500) throw new ApiError(400, "bad_reason", "Say briefly what happened (3 to 500 characters).");
            using var c = db.Open();
            long t = IdOf(c, r.PublicId);
            if ((long)Db.Scalar(c, "SELECT COUNT(*) FROM reports WHERE reporter = $a AND created_at > $n - 86400", ("$a", me.AccountId), ("$n", now))! >= MaxReportsPerDay)
                throw new ApiError(429, "too_many_reports", "You sent many reports today; try tomorrow.");
            Db.Exec(c, "INSERT INTO reports(reporter, target, reason, created_at) VALUES($a,$t,$r,$n)", ("$a", me.AccountId), ("$t", t), ("$r", reason), ("$n", now));
            return Results.Ok(new { message = "Thank you. The report was sent." });
        }).RequireRateLimiting("auth");

        // ---------------------------------------------------------------- notifications and favourites
        app.MapGet("/me/notifications", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            return new
            {
                friendRequests = Db.Scalar(c, "SELECT COUNT(*) FROM friend_requests WHERE to_id = $a", ("$a", me.AccountId)),
                invites = Db.Rows(c, @"SELECT i.id, i.session_id AS sessionId, s.name AS sessionName, p.public_id AS fromId, p.display_name AS fromName, i.expires_at AS expiresAt
                                       FROM invites i JOIN game_sessions s ON s.id = i.session_id JOIN profiles p ON p.account_id = i.from_id
                                       WHERE i.to_id = $a AND i.status = 'pending' AND i.expires_at > $n AND s.closed_at IS NULL ORDER BY i.id", ("$a", me.AccountId), ("$n", now)),
            };
        });

        app.MapGet("/me/favorites", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            return Db.Rows(c, @"SELECT p.public_id AS publicId, p.display_name AS displayName,
                                (SELECT s.id FROM game_sessions s WHERE s.host_id = f.host_id AND s.closed_at IS NULL AND s.heartbeat_at > $n - " + SessionsApi.StaleSeconds + @" LIMIT 1) AS activeSession
                                FROM favorites f JOIN profiles p ON p.account_id = f.host_id WHERE f.account_id = $a", ("$a", me.AccountId), ("$n", now));
        });

        app.MapPost("/me/favorites", (PublicIdReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            long t = IdOf(c, r.PublicId);
            if (Blocked(c, me.AccountId, t)) throw new ApiError(404, "not_found", "No player with that id.");
            if ((long)Db.Scalar(c, "SELECT COUNT(*) FROM favorites WHERE account_id = $a", ("$a", me.AccountId))! >= 100) throw new ApiError(409, "limit", "100 favourites at most.");
            Db.Exec(c, "INSERT OR IGNORE INTO favorites(account_id, host_id) VALUES($a,$t)", ("$a", me.AccountId), ("$t", t));
            return Results.Ok(new { message = "Added to favourites." });
        });

        app.MapDelete("/me/favorites/{publicId}", (string publicId, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            Db.Exec(c, "DELETE FROM favorites WHERE account_id = $a AND host_id = $t", ("$a", me.AccountId), ("$t", IdOf(c, publicId)));
            return Results.Ok(new { message = "Removed from favourites." });
        });
    }

    static long FriendCount(SqliteConnection c, long a) => (long)Db.Scalar(c, "SELECT COUNT(*) FROM friendships WHERE a = $a OR b = $a", ("$a", a))!;

    static void MakeFriends(SqliteConnection c, long x, long y, long now)
    {
        Db.Exec(c, "DELETE FROM friend_requests WHERE (from_id = $x AND to_id = $y) OR (from_id = $y AND to_id = $x)", ("$x", x), ("$y", y));
        Db.Exec(c, "INSERT OR IGNORE INTO friendships(a, b, since) VALUES($a,$b,$n)", ("$a", Math.Min(x, y)), ("$b", Math.Max(x, y)), ("$n", now));
    }
}
