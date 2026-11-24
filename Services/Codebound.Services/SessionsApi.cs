using Microsoft.Data.Sqlite;

namespace Codebound.Services;

public record HostReq(string? Name, string? Visibility, int? MaxPlayers, int? Port, int? Version, bool? HasPassword, string? LanAddress);
public record HeartbeatReq(int? Players);
public record TicketReq(string? Ticket);
public record KickReq(string? PublicId, bool? Ban);
public record InviteReq(string? SessionId, string? PublicId);

/// <summary>
/// Game sessions (Etapas 5–6 of the online mission). The world still runs on the host's PC (host-authoritative, direct
/// encrypted TCP); this service only lists sessions, decides who may join, and vouches for who a guest is: a guest asks
/// for a single-use join ticket, gives it to the host, and the host redeems it here to learn the guest's account. Names
/// can no longer be borrowed to take another player's inventory.
/// </summary>
public static class SessionsApi
{
    public const int StaleSeconds = 90, TicketSeconds = 120, InviteSeconds = 600, MaxPendingInvites = 20, MaxPlayersCap = 8;

    static Dictionary<string, object?> SessionRow(SqliteConnection c, string id) =>
        Db.Row(c, "SELECT * FROM game_sessions WHERE id = $i", ("$i", id)) ?? throw new ApiError(404, "not_found", "No such game.");

    static bool Open(Dictionary<string, object?> s, long now) => s["closed_at"] == null && (long)s["heartbeat_at"]! > now - StaleSeconds;

    static Dictionary<string, object?> HostedBy(SqliteConnection c, string id, long account)
    {
        var s = SessionRow(c, id);
        if ((long)s["host_id"]! != account) throw new ApiError(403, "not_host", "Only the host can do that.");
        return s;
    }

    static bool Member(SqliteConnection c, string id, long account) =>
        Db.Scalar(c, "SELECT 1 FROM session_members WHERE session_id = $s AND account_id = $a AND left_at IS NULL AND banned = 0", ("$s", id), ("$a", account)) != null;

    static object Card(SqliteConnection c, Dictionary<string, object?> s) => new
    {
        id = s["id"], name = s["name"], visibility = s["visibility"], players = s["players"], maxPlayers = s["max_players"], version = s["version"], hasPassword = (long)s["has_password"]! == 1,
        host = Db.Row(c, "SELECT public_id AS publicId, display_name AS displayName FROM profiles WHERE account_id = $h", ("$h", s["host_id"]))
    };

    /// <summary>A join ticket for an allowed player; `viaInvite` skips the visibility rule (the invite is the permission).</summary>
    static object Ticket(SqliteConnection c, Dictionary<string, object?> s, long account, long now, string callerIp, bool viaInvite)
    {
        string id = (string)s["id"]!; long host = (long)s["host_id"]!;
        if (!Open(s, now)) throw new ApiError(410, "session_closed", "That game has ended.");
        if (host == account) throw new ApiError(400, "own_session", "You are the host of this game.");
        if (SocialApi.Blocked(c, account, host)) throw new ApiError(404, "not_found", "No such game.");
        if (Db.Scalar(c, "SELECT banned FROM session_members WHERE session_id = $s AND account_id = $a", ("$s", id), ("$a", account)) is 1L) throw new ApiError(403, "banned", "The host removed you from this game.");
        if (!viaInvite)
        {
            string vis = (string)s["visibility"]!;
            if (vis == "private") throw new ApiError(403, "invite_only", "This game is by invitation only.");
            if (vis == "friends" && !SocialApi.Friends(c, account, host)) throw new ApiError(403, "friends_only", "Only the host's friends can join.");
        }
        if ((long)s["players"]! >= (long)s["max_players"]! && !Member(c, id, account)) throw new ApiError(409, "full", "The game is full.");
        var (ticket, hash) = Crypto.NewToken();
        Db.Exec(c, "INSERT INTO join_tickets(ticket_hash, session_id, account_id, expires_at) VALUES($h,$s,$a,$e)", ("$h", hash), ("$s", id), ("$a", account), ("$e", now + TicketSeconds));
        string address = (string)s["address"]!;   // shortcut: the host's public address only; players on the host's own LAN can still type its LAN address
        return new { ticket, address, port = s["port"], sessionId = id, sessionName = s["name"], hasPassword = (long)s["has_password"]! == 1, expiresIn = TicketSeconds, version = s["version"],
                     host = Db.Row(c, "SELECT public_id AS publicId, display_name AS displayName FROM profiles WHERE account_id = $h", ("$h", host)) };
    }

    public static void Map(WebApplication app)
    {
        app.MapPost("/sessions", (HostReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            string name = (r.Name ?? "").Trim();
            if (name.Length < 3 || name.Length > 40) throw new ApiError(400, "bad_name", "Game names have 3 to 40 characters.");
            foreach (char ch in name) if (char.IsControl(ch) || ch == '<' || ch == '>') throw new ApiError(400, "bad_name", "That name has characters that are not allowed.");
            if (r.Visibility is not ("public" or "friends" or "private")) throw new ApiError(400, "bad_value", "public, friends or private.");
            int max = Math.Clamp(r.MaxPlayers ?? MaxPlayersCap, 1, MaxPlayersCap), port = r.Port ?? 47820;
            if (port < 1024 || port > 65535) throw new ApiError(400, "bad_port", "Port must be 1024–65535.");
            string address = App.IpOf(ctx);
            if (System.Net.IPAddress.TryParse(address, out var ip) && ip.IsIPv4MappedToIPv6) address = ip.MapToIPv4().ToString();
            if (System.Net.IPAddress.TryParse(address, out var ip2) && System.Net.IPAddress.IsLoopback(ip2) && System.Net.IPAddress.TryParse(r.LanAddress, out var lan)) address = lan.ToString();   // same machine as the service (development)
            using var c = db.Open();
            Db.Exec(c, "UPDATE game_sessions SET closed_at = $n WHERE host_id = $a AND closed_at IS NULL", ("$n", now), ("$a", me.AccountId));   // one game per host
            string id = Crypto.NewToken(12).token;
            Db.Exec(c, @"INSERT INTO game_sessions(id, host_id, name, visibility, max_players, address, port, version, has_password, created_at, heartbeat_at)
                         VALUES($i,$h,$n,$v,$m,$ad,$p,$ver,$pw,$t,$t)", ("$i", id), ("$h", me.AccountId), ("$n", name), ("$v", r.Visibility), ("$m", max), ("$ad", address), ("$p", port),
                         ("$ver", r.Version ?? 0), ("$pw", r.HasPassword == true ? 1 : 0), ("$t", now));
            Db.Exec(c, "UPDATE profiles SET current_session = $i, last_seen = $t WHERE account_id = $a", ("$i", id), ("$t", now), ("$a", me.AccountId));
            AuthApi.Audit(c, me.AccountId, "session_hosted", now, null);
            return Results.Ok(new { sessionId = id, address, port, heartbeatSeconds = StaleSeconds / 3 });
        });

        app.MapPost("/sessions/{id}/heartbeat", (string id, HeartbeatReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            var s = HostedBy(c, id, me.AccountId);
            if (s["closed_at"] != null) throw new ApiError(410, "session_closed", "That game has ended.");
            Db.Exec(c, "UPDATE game_sessions SET heartbeat_at = $n, players = $p WHERE id = $i", ("$n", now), ("$p", Math.Clamp(r.Players ?? 0, 0, (int)(long)s["max_players"]!)), ("$i", id));
            Db.Exec(c, "UPDATE profiles SET last_seen = $n WHERE account_id = $a", ("$n", now), ("$a", me.AccountId));
            return Results.Ok(new { ok = true });
        });

        app.MapDelete("/sessions/{id}", (string id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            HostedBy(c, id, me.AccountId);
            Db.Exec(c, "UPDATE game_sessions SET closed_at = $n WHERE id = $i AND closed_at IS NULL", ("$n", now), ("$i", id));
            Db.Exec(c, "UPDATE session_members SET left_at = $n WHERE session_id = $i AND left_at IS NULL", ("$n", now), ("$i", id));
            Db.Exec(c, "UPDATE profiles SET current_session = NULL WHERE current_session = $i", ("$i", id));
            return Results.Ok(new { message = "Game closed." });
        });

        app.MapGet("/sessions", (string? q, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            q = (q ?? "").Trim();
            if (q.Length > 40) throw new ApiError(400, "bad_query", "40 characters at most.");
            using var c = db.Open();
            var rows = Db.Rows(c, @"SELECT s.* FROM game_sessions s WHERE s.closed_at IS NULL AND s.heartbeat_at > $n - " + StaleSeconds + @" AND s.host_id <> $me
                                    AND (s.visibility = 'public' OR (s.visibility = 'friends' AND EXISTS (SELECT 1 FROM friendships f WHERE f.a = MIN(s.host_id, $me) AND f.b = MAX(s.host_id, $me))))
                                    AND NOT EXISTS (SELECT 1 FROM blocks b WHERE (b.blocker = $me AND b.blocked = s.host_id) OR (b.blocker = s.host_id AND b.blocked = $me))
                                    AND s.name LIKE $q ESCAPE '\' ORDER BY s.players DESC, s.created_at DESC LIMIT 50",
                ("$n", now), ("$me", me.AccountId), ("$q", "%" + q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%"));
            var list = new List<object>();
            foreach (var s in rows) list.Add(Card(c, s));
            return list;
        }).RequireRateLimiting("search");

        app.MapPost("/sessions/{id}/join", (string id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return Results.Ok(Ticket(c, SessionRow(c, id), me.AccountId, clock.Now(), App.IpOf(ctx), viaInvite: false));
        });

        app.MapPost("/sessions/{id}/tickets/verify", (string id, TicketReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            HostedBy(c, id, me.AccountId);
            var t = Db.Row(c, "SELECT account_id, expires_at, used_at FROM join_tickets WHERE ticket_hash = $h AND session_id = $s", ("$h", Crypto.Sha256(r.Ticket ?? "")), ("$s", id));
            if (t == null || t["used_at"] != null || (long)t["expires_at"]! <= now) throw new ApiError(403, "invalid_ticket", "That join ticket is not valid.");
            long guest = (long)t["account_id"]!;
            if (Db.Scalar(c, "SELECT banned FROM session_members WHERE session_id = $s AND account_id = $a", ("$s", id), ("$a", guest)) is 1L) throw new ApiError(403, "banned", "This player was banned from the game.");
            Db.Exec(c, "UPDATE join_tickets SET used_at = $n WHERE ticket_hash = $h", ("$n", now), ("$h", Crypto.Sha256(r.Ticket!)));
            Db.Exec(c, @"INSERT INTO session_members(session_id, account_id, joined_at) VALUES($s,$a,$n)
                         ON CONFLICT(session_id, account_id) DO UPDATE SET left_at = NULL", ("$s", id), ("$a", guest), ("$n", now));
            AuthApi.Audit(c, guest, "session_joined", now, null);
            var p = Db.Row(c, "SELECT public_id AS publicId, display_name AS displayName, avatar FROM profiles WHERE account_id = $a", ("$a", guest));
            tx.Commit();
            return Results.Ok(p);
        });

        app.MapPost("/sessions/{id}/kick", (string id, KickReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            HostedBy(c, id, me.AccountId);
            long t = SocialApi.IdOf(c, r.PublicId);
            Db.Exec(c, @"INSERT INTO session_members(session_id, account_id, joined_at, left_at, banned) VALUES($s,$a,$n,$n,$b)
                         ON CONFLICT(session_id, account_id) DO UPDATE SET left_at = $n, banned = MAX(banned, $b)", ("$s", id), ("$a", t), ("$n", now), ("$b", r.Ban == true ? 1 : 0));
            Db.Exec(c, "DELETE FROM join_tickets WHERE session_id = $s AND account_id = $a", ("$s", id), ("$a", t));
            Db.Exec(c, "UPDATE profiles SET current_session = NULL WHERE account_id = $a AND current_session = $s", ("$a", t), ("$s", id));
            return Results.Ok(new { message = r.Ban == true ? "Player banned from this game." : "Player removed." });
        });

        app.MapPost("/sessions/{id}/left", (string id, PublicIdReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            HostedBy(c, id, me.AccountId);
            Db.Exec(c, "UPDATE session_members SET left_at = $n WHERE session_id = $s AND account_id = $a AND left_at IS NULL", ("$n", clock.Now()), ("$s", id), ("$a", SocialApi.IdOf(c, r.PublicId)));
            return Results.Ok(new { ok = true });
        });

        app.MapGet("/me/sessions/history", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return new
            {
                hosted = Db.Rows(c, "SELECT id, name, created_at AS startedAt, closed_at AS endedAt FROM game_sessions WHERE host_id = $a ORDER BY created_at DESC LIMIT 50", ("$a", me.AccountId)),
                joined = Db.Rows(c, @"SELECT s.id, s.name, m.joined_at AS joinedAt, m.left_at AS leftAt, p.display_name AS hostName, p.public_id AS hostId FROM session_members m
                                      JOIN game_sessions s ON s.id = m.session_id JOIN profiles p ON p.account_id = s.host_id WHERE m.account_id = $a ORDER BY m.joined_at DESC LIMIT 50", ("$a", me.AccountId)),
            };
        });

        // ---------------------------------------------------------------- invites
        app.MapPost("/invites", (InviteReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            var s = SessionRow(c, r.SessionId ?? "");
            if (!Open(s, now)) throw new ApiError(410, "session_closed", "That game has ended.");
            string sid = (string)s["id"]!; long host = (long)s["host_id"]!;
            if (host != me.AccountId && !Member(c, sid, me.AccountId)) throw new ApiError(403, "not_in_game", "You are not in that game.");
            long t = SocialApi.IdOf(c, r.PublicId);
            if (!SocialApi.Friends(c, me.AccountId, t)) throw new ApiError(403, "not_friends", "You can only invite friends.");
            if (SocialApi.Blocked(c, t, host)) throw new ApiError(403, "not_allowed", "That player cannot join this game.");
            if (Db.Scalar(c, "SELECT banned FROM session_members WHERE session_id = $s AND account_id = $a", ("$s", sid), ("$a", t)) is 1L) throw new ApiError(403, "not_allowed", "That player cannot join this game.");
            if ((long)Db.Scalar(c, "SELECT COUNT(*) FROM invites WHERE from_id = $a AND status = 'pending' AND expires_at > $n", ("$a", me.AccountId), ("$n", now))! >= MaxPendingInvites)
                throw new ApiError(429, "too_many_invites", "Too many invites waiting for an answer.");
            if (Db.Exec(c, "UPDATE invites SET expires_at = $e, created_at = $n WHERE session_id = $s AND to_id = $t AND status = 'pending' AND expires_at > $n",
                    ("$e", now + InviteSeconds), ("$n", now), ("$s", sid), ("$t", t)) == 0)   // the same invite again just lasts longer
                Db.Exec(c, "INSERT INTO invites(session_id, from_id, to_id, created_at, expires_at) VALUES($s,$f,$t,$n,$e)", ("$s", sid), ("$f", me.AccountId), ("$t", t), ("$n", now), ("$e", now + InviteSeconds));
            return Results.Ok(new { message = "Invite sent.", expiresIn = InviteSeconds });
        });

        app.MapGet("/invites", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            return Db.Rows(c, @"SELECT i.id, i.session_id AS sessionId, s.name AS sessionName, p.public_id AS fromId, p.display_name AS fromName, i.expires_at AS expiresAt
                                FROM invites i JOIN game_sessions s ON s.id = i.session_id JOIN profiles p ON p.account_id = i.from_id
                                WHERE i.to_id = $a AND i.status = 'pending' AND i.expires_at > $n AND s.closed_at IS NULL ORDER BY i.id", ("$a", me.AccountId), ("$n", now));
        });

        app.MapPost("/invites/{id:long}/accept", (long id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            var i = Db.Row(c, "SELECT session_id, expires_at, status FROM invites WHERE id = $i AND to_id = $a", ("$i", id), ("$a", me.AccountId)) ?? throw new ApiError(404, "not_found", "No such invite.");
            if ((string)i["status"]! != "pending" || (long)i["expires_at"]! <= now) throw new ApiError(410, "invite_expired", "That invite has expired.");
            Db.Exec(c, "UPDATE invites SET status = 'accepted' WHERE id = $i", ("$i", id));
            var ticket = Ticket(c, SessionRow(c, (string)i["session_id"]!), me.AccountId, now, App.IpOf(ctx), viaInvite: true);
            tx.Commit();
            return Results.Ok(ticket);
        });

        app.MapPost("/invites/{id:long}/decline", (long id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            if (Db.Exec(c, "UPDATE invites SET status = 'declined' WHERE id = $i AND to_id = $a AND status = 'pending'", ("$i", id), ("$a", me.AccountId)) == 0) throw new ApiError(404, "not_found", "No such invite.");
            return Results.Ok(new { message = "Invite declined." });
        });
    }
}
