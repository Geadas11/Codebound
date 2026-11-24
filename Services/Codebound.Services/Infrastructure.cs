using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Codebound.Services;

/// <summary>Settings (appsettings.json + environment variables CODEBOUND__...). No secret has a default here.</summary>
public sealed class ServiceOptions
{
    public string Database { get; set; } = "codebound.db";
    public int PasswordIterations { get; set; } = 600_000;           // PBKDF2-SHA256 (OWASP 2023); tests lower it
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
    public int CodeMinutes { get; set; } = 30;                        // email verification / reset / email change codes
    public int GlobalRequestsPerMinute { get; set; } = 240;           // per IP
    public int AuthRequestsPerMinute { get; set; } = 20;              // per IP, for login/register/codes
    public int SearchRequestsPerMinute { get; set; } = 30;            // per account
    public bool RequireHttps { get; set; }                            // production: true (behind a TLS proxy)
    public string[] TrustedProxies { get; set; } = Array.Empty<string>();
    public bool DevOutbox { get; set; }                               // development: emails go to a local table, readable at /dev/outbox
    public SmtpOptions Smtp { get; set; } = new();
    public int RetentionDays { get; set; } = 30;                      // audit records of deleted accounts
}

public sealed class SmtpOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? User { get; set; }
    public string? Password { get; set; }                            // only from the environment (CODEBOUND__SMTP__PASSWORD)
    public string From { get; set; } = "Codebound <no-reply@localhost>";
}

/// <summary>An error the API reports to the client: { "error": code, "message": text }.</summary>
public sealed class ApiError : Exception
{
    public int Status { get; }
    public string Code { get; }
    public ApiError(int status, string code, string message) : base(message) { Status = status; Code = code; }
}

/// <summary>SQLite access. One connection per call (pooled by Microsoft.Data.Sqlite); WAL for concurrent readers.</summary>
public sealed class Db
{
    readonly string cs;
    public Db(ServiceOptions o) { cs = new SqliteConnectionStringBuilder { DataSource = o.Database, Pooling = true }.ToString(); }

    public SqliteConnection Open()
    {
        var c = new SqliteConnection(cs);
        c.Open();
        using var p = c.CreateCommand();
        p.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
        p.ExecuteNonQuery();
        return c;
    }

    public static SqliteCommand Cmd(SqliteConnection c, string sql, params (string name, object? value)[] args)
    {
        var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (n, v) in args) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
        return cmd;
    }

    public static int Exec(SqliteConnection c, string sql, params (string, object?)[] args) { using var cmd = Cmd(c, sql, args); return cmd.ExecuteNonQuery(); }
    public static object? Scalar(SqliteConnection c, string sql, params (string, object?)[] args) { using var cmd = Cmd(c, sql, args); var v = cmd.ExecuteScalar(); return v is DBNull ? null : v; }

    public static List<Dictionary<string, object?>> Rows(SqliteConnection c, string sql, params (string, object?)[] args)
    {
        using var cmd = Cmd(c, sql, args);
        using var r = cmd.ExecuteReader();
        var list = new List<Dictionary<string, object?>>();
        while (r.Read())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < r.FieldCount; i++) row[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
            list.Add(row);
        }
        return list;
    }

    public static Dictionary<string, object?>? Row(SqliteConnection c, string sql, params (string, object?)[] args) { var l = Rows(c, sql, args); return l.Count > 0 ? l[0] : null; }

    /// <summary>Online copy of the database to a file (safe while the service runs).</summary>
    public void Backup(string path)
    {
        using var src = Open();
        using var dst = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path }.ToString());
        dst.Open();
        src.BackupDatabase(dst);
    }
}

/// <summary>Passwords, tokens and codes. Only standard-library primitives.</summary>
public static class Crypto
{
    public static string HashPassword(string password, int iterations)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2-sha256${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string? stored)
    {
        if (stored == null) return false;
        var p = stored.Split('$');
        if (p.Length != 4 || p[0] != "pbkdf2-sha256" || !int.TryParse(p[1], out int it)) return false;
        var salt = Convert.FromBase64String(p[2]); var want = Convert.FromBase64String(p[3]);
        var got = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, it, HashAlgorithmName.SHA256, want.Length);
        return CryptographicOperations.FixedTimeEquals(got, want);
    }

    /// <summary>A random token (sent to the client once) and the hash the database keeps.</summary>
    public static (string token, string hash) NewToken(int bytes = 32)
    {
        var t = Base64Url(RandomNumberGenerator.GetBytes(bytes));
        return (t, Sha256(t));
    }

    public static string Sha256(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
    public static string Base64Url(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    public static string NewCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    public static string NewPublicId()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";   // no 0/O, 1/I: easy to read out to a friend
        var sb = new StringBuilder("CB-");
        for (int i = 0; i < 8; i++) sb.Append(alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]);
        return sb.ToString();
    }
}

/// <summary>Sends email. Development: a local outbox table (and the log); production: SMTP from settings.</summary>
public interface IMailer { void Send(string to, string subject, string body); }

/// <summary>Development: emails are kept in memory (the newest 200), never sent; readable at /dev/outbox from this machine.
/// Not in the database, so sending inside a database transaction never waits on it.</summary>
public sealed class OutboxMailer : IMailer
{
    readonly ILogger<OutboxMailer> log; readonly TimeProvider clock;
    readonly LinkedList<(string to, string subject, string body, long at)> mails = new();
    public OutboxMailer(ILogger<OutboxMailer> log, TimeProvider clock) { this.log = log; this.clock = clock; }
    public void Send(string to, string subject, string body)
    {
        lock (mails) { mails.AddFirst((to, subject, body, clock.GetUtcNow().ToUnixTimeSeconds())); if (mails.Count > 200) mails.RemoveLast(); }
        log.LogInformation("Development email to {To}: {Subject}", to, subject);
    }
    public List<(string to, string subject, string body, long at)> Recent() { lock (mails) return mails.ToList(); }
}

public sealed class SmtpMailer : IMailer
{
    readonly SmtpOptions o; readonly ILogger<SmtpMailer> log;
    public SmtpMailer(ServiceOptions opts, ILogger<SmtpMailer> log) { o = opts.Smtp; this.log = log; }
    public void Send(string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(o.Host)) throw new ApiError(503, "email_unavailable", "Email is not configured on this server.");
        using var client = new SmtpClient(o.Host, o.Port) { EnableSsl = true, DeliveryMethod = SmtpDeliveryMethod.Network };
        if (!string.IsNullOrEmpty(o.User)) client.Credentials = new System.Net.NetworkCredential(o.User, o.Password);
        client.Send(new MailMessage(o.From, to, subject, body));
        log.LogInformation("Email sent: {Subject}", subject);   // never the address or the body (codes) in the log
    }
}

/// <summary>A clock tests can move (expired tokens, codes, invites).</summary>
public sealed class AdjustableClock : TimeProvider
{
    public TimeSpan Offset;
    public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + Offset;
}
