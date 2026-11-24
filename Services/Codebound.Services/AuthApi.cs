using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace Codebound.Services;

public record RegisterReq(string? Email, string? Password, string? DisplayName);
public record EmailCodeReq(string? Email, string? Code);
public record EmailReq(string? Email);
public record LoginReq(string? Email, string? Password, string? Device);
public record RefreshReq(string? RefreshToken);
public record ResetReq(string? Email, string? Code, string? NewPassword);
public record ChangePasswordReq(string? CurrentPassword, string? NewPassword);
public record ChangeEmailReq(string? Password, string? NewEmail);
public record CodeReq(string? Code);
public record DeleteReq(string? Password, string? Confirm);

/// <summary>
/// Accounts and authentication. Emails are private (never shown to other players); answers never reveal whether an
/// email has an account (register, forgot password). Access tokens live 15 minutes; refresh tokens rotate on every use
/// and reusing one revokes the whole device session (stolen-token defence).
/// </summary>
public static class AuthApi
{
    public const int MaxCodeAttempts = 5, LockAfterFailures = 5;
    static readonly Regex EmailRx = new(@"^[^@\s]{1,64}@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled);
    static readonly Regex NameRx = new(@"^[A-Za-z0-9_-]{3,16}$", RegexOptions.Compiled);

    public static string NormEmail(string? e)
    {
        e = (e ?? "").Trim();
        if (e.Length > 254 || !EmailRx.IsMatch(e)) throw new ApiError(400, "bad_email", "Enter a valid email address.");
        return e.ToLowerInvariant();
    }

    public static void CheckPassword(string? p, string? email = null)
    {
        if (p == null || p.Length < 10 || p.Length > 128) throw new ApiError(400, "weak_password", "Use a password of 10 to 128 characters.");
        if (email != null && p.Equals(email, StringComparison.OrdinalIgnoreCase)) throw new ApiError(400, "weak_password", "The password cannot be your email.");
    }

    public static string CheckName(string? n)
    {
        n = (n ?? "").Trim();
        if (!NameRx.IsMatch(n)) throw new ApiError(400, "bad_name", "Player names have 3 to 16 letters, digits, _ or -.");
        if (n.StartsWith("deleted-", StringComparison.OrdinalIgnoreCase)) throw new ApiError(400, "bad_name", "That name is reserved.");
        return n;
    }

    public static void Audit(SqliteConnection c, long? account, string evt, long now, string? ip) =>
        Db.Exec(c, "INSERT INTO audit(account_id, event, at, ip) VALUES($a,$e,$t,$ip)", ("$a", account), ("$e", evt), ("$t", now), ("$ip", ip));

    static void SendCode(SqliteConnection c, IMailer mail, ServiceOptions o, long now, long account, string purpose, string to, string? newEmail = null)
    {
        Db.Exec(c, "UPDATE codes SET used = 1 WHERE account_id = $a AND purpose = $p", ("$a", account), ("$p", purpose));
        string code = Crypto.NewCode();
        Db.Exec(c, "INSERT INTO codes(account_id, purpose, code_hash, new_email, expires_at) VALUES($a,$p,$h,$n,$e)",
            ("$a", account), ("$p", purpose), ("$h", Crypto.Sha256(account + ":" + purpose + ":" + code)), ("$n", newEmail), ("$e", now + o.CodeMinutes * 60L));
        string what = purpose == "verify" ? "confirm your email" : purpose == "reset" ? "reset your password" : "confirm your new email";
        mail.Send(to, $"Codebound code: {code}", $"Your Codebound code to {what} is {code}.\nIt expires in {o.CodeMinutes} minutes. If you did not ask for it, ignore this email.");
    }

    /// <summary>Checks a code (limited attempts) and consumes it; returns its row.</summary>
    static Dictionary<string, object?> UseCode(SqliteConnection c, long now, long account, string purpose, string? code)
    {
        var row = Db.Row(c, "SELECT id, code_hash, new_email, attempts FROM codes WHERE account_id = $a AND purpose = $p AND used = 0 AND expires_at > $n ORDER BY id DESC LIMIT 1",
            ("$a", account), ("$p", purpose), ("$n", now));
        if (row == null) throw new ApiError(400, "invalid_code", "That code is wrong or has expired. Ask for a new one.");
        if ((long)row["attempts"]! >= MaxCodeAttempts) throw new ApiError(400, "invalid_code", "Too many wrong codes. Ask for a new one.");
        if ((string)row["code_hash"]! != Crypto.Sha256(account + ":" + purpose + ":" + (code ?? "").Trim()))
        {
            Db.Exec(c, "UPDATE codes SET attempts = attempts + 1 WHERE id = $i", ("$i", row["id"]));
            throw new ApiError(400, "invalid_code", "That code is wrong or has expired. Ask for a new one.");
        }
        Db.Exec(c, "UPDATE codes SET used = 1 WHERE id = $i", ("$i", row["id"]));
        return row;
    }

    static long? AccountByEmail(SqliteConnection c, string email) => Db.Scalar(c, "SELECT id FROM accounts WHERE email = $e AND deleted_at IS NULL", ("$e", email)) as long?;

    /// <summary>A new device session with its first token pair.</summary>
    public static object NewSession(SqliteConnection c, ServiceOptions o, long now, long account, string? device, string ip)
    {
        string sid = Crypto.NewToken(18).token;
        Db.Exec(c, "INSERT INTO auth_sessions(id, account_id, device, created_at, last_used, ip) VALUES($i,$a,$d,$n,$n,$ip)",
            ("$i", sid), ("$a", account), ("$d", string.IsNullOrWhiteSpace(device) ? "unknown device" : device.Trim()[..Math.Min(device.Trim().Length, 64)]), ("$n", now), ("$ip", ip));
        return Tokens(c, o, now, account, sid);
    }

    static object Tokens(SqliteConnection c, ServiceOptions o, long now, long account, string sid)
    {
        var (access, ah) = Crypto.NewToken();
        var (refresh, rh) = Crypto.NewToken();
        Db.Exec(c, "INSERT INTO access_tokens(token_hash, session_id, account_id, expires_at) VALUES($h,$s,$a,$e)", ("$h", ah), ("$s", sid), ("$a", account), ("$e", now + o.AccessTokenMinutes * 60L));
        Db.Exec(c, "INSERT INTO refresh_tokens(token_hash, session_id, expires_at) VALUES($h,$s,$e)", ("$h", rh), ("$s", sid), ("$e", now + o.RefreshTokenDays * 86400L));
        var p = Db.Row(c, "SELECT p.public_id, p.display_name, a.email FROM profiles p JOIN accounts a ON a.id = p.account_id WHERE a.id = $a", ("$a", account))!;
        return new { accessToken = access, refreshToken = refresh, accessExpiresIn = o.AccessTokenMinutes * 60, refreshExpiresIn = o.RefreshTokenDays * 86400, sessionId = sid,
                     account = new { publicId = p["public_id"], displayName = p["display_name"], email = p["email"] } };
    }

    static void RevokeSessions(SqliteConnection c, long account, long now, string? except = null) =>
        Db.Exec(c, "UPDATE auth_sessions SET revoked_at = $n WHERE account_id = $a AND revoked_at IS NULL AND ($x IS NULL OR id <> $x)", ("$n", now), ("$a", account), ("$x", except));

    public static void Map(WebApplication app)
    {
        var auth = app.MapGroup("/auth");

        auth.MapPost("/register", (RegisterReq r, HttpContext ctx, Db db, IMailer mail, ServiceOptions o, TimeProvider clock) =>
        {
            string email = NormEmail(r.Email); CheckPassword(r.Password, email); string name = CheckName(r.DisplayName);
            long now = clock.Now();
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            if (Db.Scalar(c, "SELECT 1 FROM profiles WHERE display_name = $n", ("$n", name)) != null) throw new ApiError(409, "name_taken", "That player name is taken.");
            if (AccountByEmail(c, email) is long existing)   // no enumeration: same answer; the owner is told by email instead
            {
                mail.Send(email, "Codebound: sign-up attempt", "Someone tried to create a Codebound account with this email. If it was you, sign in or reset your password.");
                tx.Commit();
                return Results.Json(new { message = "Check your email for a code to confirm your account." }, statusCode: 202);
            }
            Db.Exec(c, "INSERT INTO accounts(email, password_hash, created_at) VALUES($e,$h,$n)", ("$e", email), ("$h", Crypto.HashPassword(r.Password!, o.PasswordIterations)), ("$n", now));
            long id = (long)Db.Scalar(c, "SELECT last_insert_rowid()")!;
            string pid;
            do pid = Crypto.NewPublicId(); while (Db.Scalar(c, "SELECT 1 FROM profiles WHERE public_id = $p", ("$p", pid)) != null);
            Db.Exec(c, "INSERT INTO profiles(account_id, public_id, display_name, created_at) VALUES($a,$p,$n,$t)", ("$a", id), ("$p", pid), ("$n", name), ("$t", now));
            SendCode(c, mail, o, now, id, "verify", email);
            Audit(c, id, "register", now, App.IpOf(ctx));
            tx.Commit();
            return Results.Json(new { message = "Check your email for a code to confirm your account." }, statusCode: 202);
        }).RequireRateLimiting("auth");

        auth.MapPost("/verify", (EmailCodeReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            string email = NormEmail(r.Email); long now = clock.Now();
            using var c = db.Open();
            var id = AccountByEmail(c, email) ?? throw new ApiError(400, "invalid_code", "That code is wrong or has expired. Ask for a new one.");
            UseCode(c, now, id, "verify", r.Code);
            Db.Exec(c, "UPDATE accounts SET email_verified = 1 WHERE id = $a", ("$a", id));
            Audit(c, id, "email_verified", now, App.IpOf(ctx));
            return Results.Ok(new { message = "Email confirmed. You can sign in." });
        }).RequireRateLimiting("auth");

        auth.MapPost("/verify/resend", (EmailReq r, Db db, IMailer mail, ServiceOptions o, TimeProvider clock) =>
        {
            string email = NormEmail(r.Email); long now = clock.Now();
            using var c = db.Open();
            if (AccountByEmail(c, email) is long id && Db.Scalar(c, "SELECT email_verified FROM accounts WHERE id = $a", ("$a", id)) is 0L)
            {
                long lastSent = (Db.Scalar(c, "SELECT MAX(expires_at) FROM codes WHERE account_id = $a AND purpose = 'verify'", ("$a", id)) as long? ?? 0) - o.CodeMinutes * 60L;
                if (now - lastSent >= 60) SendCode(c, mail, o, now, id, "verify", email);   // at most one email a minute
            }
            return Results.Json(new { message = "If that account is waiting for confirmation, a new code is on its way." }, statusCode: 202);
        }).RequireRateLimiting("auth");

        auth.MapPost("/login", (LoginReq r, HttpContext ctx, Db db, ServiceOptions o, TimeProvider clock) =>
        {
            string email = NormEmail(r.Email); long now = clock.Now(); string ip = App.IpOf(ctx);
            using var c = db.Open();
            var a = Db.Row(c, "SELECT id, password_hash, email_verified, failed_logins, locked_until FROM accounts WHERE email = $e AND deleted_at IS NULL", ("$e", email));
            if (a == null) { Crypto.VerifyPassword(r.Password ?? "", Crypto.HashPassword("timing", o.PasswordIterations)); throw new ApiError(401, "invalid_credentials", "Wrong email or password."); }
            long id = (long)a["id"]!;
            if ((long)a["locked_until"]! > now) throw new ApiError(429, "account_locked", $"Too many wrong passwords. Try again in {(long)a["locked_until"]! - now} seconds, or reset your password.");
            if (!Crypto.VerifyPassword(r.Password ?? "", (string?)a["password_hash"]))
            {
                long fails = (long)a["failed_logins"]! + 1;
                long lockFor = fails >= LockAfterFailures ? Math.Min(3600, 60L << (int)Math.Min(6, fails - LockAfterFailures)) : 0;
                Db.Exec(c, "UPDATE accounts SET failed_logins = $f, locked_until = $l WHERE id = $a", ("$f", fails), ("$l", lockFor > 0 ? now + lockFor : 0), ("$a", id));
                Audit(c, id, "login_failed", now, ip);
                throw new ApiError(401, "invalid_credentials", "Wrong email or password.");
            }
            Db.Exec(c, "UPDATE accounts SET failed_logins = 0, locked_until = 0 WHERE id = $a", ("$a", id));
            if ((long)a["email_verified"]! == 0) throw new ApiError(403, "email_not_verified", "Confirm your email first (we sent you a code).");
            Audit(c, id, "login", now, ip);
            return Results.Ok(NewSession(c, o, now, id, r.Device, ip));
        }).RequireRateLimiting("auth");

        auth.MapPost("/refresh", (RefreshReq r, HttpContext ctx, Db db, ServiceOptions o, TimeProvider clock) =>
        {
            long now = clock.Now();
            using var c = db.Open();
            using var tx = c.BeginTransaction();
            var t = Db.Row(c, @"SELECT r.session_id, r.expires_at, r.used_at, s.account_id, s.revoked_at FROM refresh_tokens r JOIN auth_sessions s ON s.id = r.session_id
                                WHERE r.token_hash = $h", ("$h", Crypto.Sha256(r.RefreshToken ?? "")));
            if (t == null || t["revoked_at"] != null) throw new ApiError(401, "invalid_token", "Sign in again.");
            string sid = (string)t["session_id"]!; long account = (long)t["account_id"]!;
            if (t["used_at"] != null)   // a refresh token used twice: someone has a copy; end this device session
            {
                Db.Exec(c, "UPDATE auth_sessions SET revoked_at = $n WHERE id = $s", ("$n", now), ("$s", sid));
                Audit(c, account, "refresh_reuse", now, App.IpOf(ctx));
                tx.Commit();
                throw new ApiError(401, "token_reused", "This sign-in was used somewhere else. Sign in again.");
            }
            if ((long)t["expires_at"]! <= now) throw new ApiError(401, "token_expired", "Your sign-in expired. Sign in again.");
            Db.Exec(c, "UPDATE refresh_tokens SET used_at = $n WHERE token_hash = $h", ("$n", now), ("$h", Crypto.Sha256(r.RefreshToken!)));
            Db.Exec(c, "DELETE FROM access_tokens WHERE session_id = $s", ("$s", sid));
            Db.Exec(c, "UPDATE auth_sessions SET last_used = $n, ip = $ip WHERE id = $s", ("$n", now), ("$ip", App.IpOf(ctx)), ("$s", sid));
            var tokens = Tokens(c, o, now, account, sid);
            tx.Commit();
            return Results.Ok(tokens);
        }).RequireRateLimiting("auth");

        auth.MapPost("/logout", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            Db.Exec(c, "UPDATE auth_sessions SET revoked_at = $n WHERE id = $s", ("$n", clock.Now()), ("$s", me.SessionId));
            Db.Exec(c, "UPDATE profiles SET last_seen = 0, current_session = NULL WHERE account_id = $a", ("$a", me.AccountId));
            return Results.Ok(new { message = "Signed out." });
        });

        auth.MapGet("/sessions", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            return Db.Rows(c, "SELECT id, device, created_at AS createdAt, last_used AS lastUsed, (id = $s) AS current FROM auth_sessions WHERE account_id = $a AND revoked_at IS NULL ORDER BY last_used DESC",
                ("$a", me.AccountId), ("$s", me.SessionId));
        });

        auth.MapDelete("/sessions/{id}", (string id, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            if (Db.Exec(c, "UPDATE auth_sessions SET revoked_at = $n WHERE id = $i AND account_id = $a AND revoked_at IS NULL", ("$n", clock.Now()), ("$i", id), ("$a", me.AccountId)) == 0)
                throw new ApiError(404, "not_found", "No such session.");
            return Results.Ok(new { message = "That device was signed out." });
        });

        auth.MapPost("/password/forgot", (EmailReq r, Db db, IMailer mail, ServiceOptions o, TimeProvider clock) =>
        {
            string email = NormEmail(r.Email); long now = clock.Now();
            using var c = db.Open();
            if (AccountByEmail(c, email) is long id)
            {
                long lastSent = (Db.Scalar(c, "SELECT MAX(expires_at) FROM codes WHERE account_id = $a AND purpose = 'reset'", ("$a", id)) as long? ?? 0) - o.CodeMinutes * 60L;
                if (now - lastSent >= 60) SendCode(c, mail, o, now, id, "reset", email);
            }
            return Results.Json(new { message = "If that email has an account, a reset code is on its way." }, statusCode: 202);
        }).RequireRateLimiting("auth");

        auth.MapPost("/password/reset", (ResetReq r, HttpContext ctx, Db db, ServiceOptions o, TimeProvider clock) =>
        {
            string email = NormEmail(r.Email); CheckPassword(r.NewPassword, email); long now = clock.Now();
            using var c = db.Open();
            var id = AccountByEmail(c, email) ?? throw new ApiError(400, "invalid_code", "That code is wrong or has expired. Ask for a new one.");
            UseCode(c, now, id, "reset", r.Code);
            Db.Exec(c, "UPDATE accounts SET password_hash = $h, failed_logins = 0, locked_until = 0, email_verified = 1 WHERE id = $a",   // the code proved the email
                ("$h", Crypto.HashPassword(r.NewPassword!, o.PasswordIterations)), ("$a", id));
            RevokeSessions(c, id, now);
            Audit(c, id, "password_reset", now, App.IpOf(ctx));
            return Results.Ok(new { message = "Password changed. Sign in with the new one." });
        }).RequireRateLimiting("auth");

        auth.MapPost("/password/change", (ChangePasswordReq r, HttpContext ctx, Db db, ServiceOptions o, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            var a = Db.Row(c, "SELECT email, password_hash FROM accounts WHERE id = $a", ("$a", me.AccountId))!;
            if (!Crypto.VerifyPassword(r.CurrentPassword ?? "", (string?)a["password_hash"])) throw new ApiError(403, "wrong_password", "Your current password is not right.");
            CheckPassword(r.NewPassword, (string?)a["email"]);
            Db.Exec(c, "UPDATE accounts SET password_hash = $h WHERE id = $a", ("$h", Crypto.HashPassword(r.NewPassword!, o.PasswordIterations)), ("$a", me.AccountId));
            RevokeSessions(c, me.AccountId, now, except: me.SessionId);
            Audit(c, me.AccountId, "password_changed", now, App.IpOf(ctx));
            return Results.Ok(new { message = "Password changed. Other devices were signed out." });
        }).RequireRateLimiting("auth");

        auth.MapPost("/email/change", (ChangeEmailReq r, HttpContext ctx, Db db, IMailer mail, ServiceOptions o, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            string email = NormEmail(r.NewEmail);
            using var c = db.Open();
            if (!Crypto.VerifyPassword(r.Password ?? "", (string?)Db.Scalar(c, "SELECT password_hash FROM accounts WHERE id = $a", ("$a", me.AccountId))))
                throw new ApiError(403, "wrong_password", "Your password is not right.");
            if (AccountByEmail(c, email) == null) SendCode(c, mail, o, now, me.AccountId, "email", email, email);
            else mail.Send(email, "Codebound: email change attempt", "Someone tried to move a Codebound account to this email, which already has one. Nothing was changed.");
            return Results.Json(new { message = "We sent a code to the new email. Enter it to finish the change." }, statusCode: 202);
        }).RequireRateLimiting("auth");

        auth.MapPost("/email/confirm", (CodeReq r, HttpContext ctx, Db db, IMailer mail, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            var row = UseCode(c, now, me.AccountId, "email", r.Code);
            string newEmail = (string)row["new_email"]!;
            string old = (string)Db.Scalar(c, "SELECT email FROM accounts WHERE id = $a", ("$a", me.AccountId))!;
            if (AccountByEmail(c, newEmail) != null) throw new ApiError(409, "email_taken", "That email now belongs to another account.");
            Db.Exec(c, "UPDATE accounts SET email = $e, email_verified = 1 WHERE id = $a", ("$e", newEmail), ("$a", me.AccountId));
            mail.Send(old, "Codebound: your email was changed", "Your Codebound account now uses another email. If you did not do this, reset your password from the new address or contact support.");
            Audit(c, me.AccountId, "email_changed", now, App.IpOf(ctx));
            return Results.Ok(new { message = "Email changed." });
        }).RequireRateLimiting("auth");

        app.MapGet("/me", (HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            var r = Db.Row(c, @"SELECT a.email, a.created_at AS createdAt, p.public_id AS publicId, p.display_name AS displayName, p.avatar, p.bio, p.visibility,
                                p.allow_requests AS allowRequests, p.show_presence AS showPresence, p.stats_consent AS statsConsent, p.status
                                FROM accounts a JOIN profiles p ON p.account_id = a.id WHERE a.id = $a", ("$a", me.AccountId));
            return r;
        });

        app.MapPost("/me/delete", (DeleteReq r, HttpContext ctx, Db db, TimeProvider clock) =>
        {
            var me = AuthContext.Require(ctx, db, clock); long now = clock.Now();
            using var c = db.Open();
            if (r.Confirm != "DELETE") throw new ApiError(400, "confirm", "Type DELETE to confirm.");
            if (!Crypto.VerifyPassword(r.Password ?? "", (string?)Db.Scalar(c, "SELECT password_hash FROM accounts WHERE id = $a", ("$a", me.AccountId))))
                throw new ApiError(403, "wrong_password", "Your password is not right.");
            using var tx = c.BeginTransaction();
            string pid = (string)Db.Scalar(c, "SELECT public_id FROM profiles WHERE account_id = $a", ("$a", me.AccountId))!;
            // personal data goes now; the bare account row and its audit trail stay RetentionDays for abuse handling, then Maintenance removes them
            Db.Exec(c, "UPDATE accounts SET email = NULL, password_hash = NULL, deleted_at = $n WHERE id = $a", ("$n", now), ("$a", me.AccountId));
            Db.Exec(c, "UPDATE profiles SET display_name = $d, bio = '', avatar = 0, visibility = 'private', allow_requests = 'nobody', show_presence = 0, current_session = NULL WHERE account_id = $a",
                ("$d", "deleted-" + pid.Substring(3)), ("$a", me.AccountId));
            foreach (var t in new[] { "DELETE FROM friend_requests WHERE from_id = $a OR to_id = $a", "DELETE FROM friendships WHERE a = $a OR b = $a",
                                      "DELETE FROM blocks WHERE blocker = $a OR blocked = $a", "DELETE FROM invites WHERE from_id = $a OR to_id = $a",
                                      "DELETE FROM stats WHERE account_id = $a", "DELETE FROM stat_batches WHERE account_id = $a", "DELETE FROM achievements WHERE account_id = $a",
                                      "DELETE FROM cloud_saves WHERE account_id = $a", "DELETE FROM favorites WHERE account_id = $a OR host_id = $a",
                                      "DELETE FROM auth_sessions WHERE account_id = $a", "UPDATE game_sessions SET closed_at = $n WHERE host_id = $a AND closed_at IS NULL" })
                Db.Exec(c, t, ("$a", me.AccountId), ("$n", now));
            Audit(c, me.AccountId, "deleted", now, null);
            tx.Commit();
            return Results.Ok(new { message = "Your account was deleted." });
        });

        app.MapGet("/me/export", (HttpContext ctx, Db db, TimeProvider clock) =>   // a copy of everything kept about the caller
        {
            var me = AuthContext.Require(ctx, db, clock);
            using var c = db.Open();
            (string, object?) A = ("$a", me.AccountId);
            return new
            {
                account = Db.Row(c, "SELECT email, email_verified, created_at FROM accounts WHERE id = $a", A),
                profile = Db.Row(c, "SELECT * FROM profiles WHERE account_id = $a", A),
                sessions = Db.Rows(c, "SELECT device, created_at, last_used, ip FROM auth_sessions WHERE account_id = $a", A),
                friends = Db.Rows(c, "SELECT p.public_id, p.display_name, f.since FROM friendships f JOIN profiles p ON p.account_id = CASE WHEN f.a = $a THEN f.b ELSE f.a END WHERE f.a = $a OR f.b = $a", A),
                blocks = Db.Rows(c, "SELECT p.public_id FROM blocks b JOIN profiles p ON p.account_id = b.blocked WHERE b.blocker = $a", A),
                stats = Db.Rows(c, "SELECT key, value FROM stats WHERE account_id = $a", A),
                achievements = Db.Rows(c, "SELECT achievement, unlocked_at FROM achievements WHERE account_id = $a", A),
                saves = Db.Rows(c, "SELECT slot, version, size, server_time FROM cloud_saves WHERE account_id = $a", A),
                activity = Db.Rows(c, "SELECT event, at FROM audit WHERE account_id = $a ORDER BY at DESC LIMIT 200", A),
            };
        });
    }
}
