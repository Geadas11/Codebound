using System.Diagnostics;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace Codebound.Services;

/// <summary>Builds the service: settings, database, mail, rate limits, middleware and every endpoint group.</summary>
public static class App
{
    public const int Version = 1;

    public static WebApplication Build(WebApplicationBuilder builder, Action<ServiceOptions>? configure = null, TimeProvider? clock = null)
    {
        var opts = new ServiceOptions();
        builder.Configuration.GetSection("Codebound").Bind(opts);
        configure?.Invoke(opts);
        var s = builder.Services;
        s.AddSingleton(opts);
        s.AddSingleton(clock ?? TimeProvider.System);
        s.AddSingleton<Db>();
        if (opts.DevOutbox) s.AddSingleton<IMailer, OutboxMailer>(); else s.AddSingleton<IMailer, SmtpMailer>();
        s.AddHostedService<Maintenance>();
        s.Configure<ForwardedHeadersOptions>(f =>
        {
            f.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var p in opts.TrustedProxies) if (IPAddress.TryParse(p, out var ip)) f.KnownProxies.Add(ip);
        });
        s.AddRateLimiter(r =>
        {
            r.RejectionStatusCode = 429;
            r.OnRejected = async (ctx, ct) => await ctx.HttpContext.Response.WriteAsJsonAsync(new { error = "rate_limited", message = "Too many requests. Try again in a minute." }, ct);
            r.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx => RateLimitPartition.GetFixedWindowLimiter(IpOf(ctx),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = opts.GlobalRequestsPerMinute, Window = TimeSpan.FromMinutes(1) }));
            r.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(IpOf(ctx),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = opts.AuthRequestsPerMinute, Window = TimeSpan.FromMinutes(1) }));
            r.AddPolicy("search", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Request.Headers.Authorization.ToString() is { Length: > 0 } a ? a : IpOf(ctx),
                _ => new FixedWindowRateLimiterOptions { PermitLimit = opts.SearchRequestsPerMinute, Window = TimeSpan.FromMinutes(1) }));
        });

        var app = builder.Build();
        Migrations.Apply(app.Services.GetRequiredService<Db>());
        app.UseForwardedHeaders();
        app.Use(async (ctx, next) =>   // errors as JSON, HTTPS in production, a log line per request (never bodies or tokens)
        {
            var log = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("http");
            var sw = Stopwatch.StartNew();
            ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
            ctx.Response.Headers["Cache-Control"] = "no-store";
            try
            {
                if (opts.RequireHttps && !ctx.Request.IsHttps) throw new ApiError(400, "https_required", "Use HTTPS.");
                await next();
            }
            catch (ApiError e)
            {
                ctx.Response.StatusCode = e.Status;
                await ctx.Response.WriteAsJsonAsync(new { error = e.Code, message = e.Message });
            }
            catch (Exception e) when (e is Microsoft.AspNetCore.Http.BadHttpRequestException || e is System.Text.Json.JsonException)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { error = "bad_request", message = "The request could not be read." });
            }
            catch (Exception e)
            {
                log.LogError(e, "Unhandled error on {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
                ctx.Response.StatusCode = 500;
                await ctx.Response.WriteAsJsonAsync(new { error = "server_error", message = "Something went wrong on the server." });
            }
            log.LogInformation("{Method} {Path} -> {Status} in {Ms} ms", ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
        });
        app.UseRateLimiter();

        app.MapGet("/health", (TimeProvider c) => new { status = "ok", version = Version, time = c.GetUtcNow().ToUnixTimeSeconds() });
        if (opts.DevOutbox)   // development only, and only from this machine: read the emails that would have been sent
            app.MapGet("/dev/outbox", (HttpContext ctx, IMailer mail) =>
            {
                if (ctx.Connection.RemoteIpAddress is not { } ip || !IPAddress.IsLoopback(ip)) throw new ApiError(404, "not_found", "Not found.");
                return ((OutboxMailer)mail).Recent().Take(20).Select(m => new { recipient = m.to, m.subject, m.body, createdAt = m.at });
            });
        AuthApi.Map(app);
        SocialApi.Map(app);
        SessionsApi.Map(app);
        ProgressApi.Map(app);
        return app;
    }

    public static string IpOf(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    public static long Now(this TimeProvider c) => c.GetUtcNow().ToUnixTimeSeconds();
}

/// <summary>Who is calling: the account behind a valid access token.</summary>
public readonly record struct Caller(long AccountId, string SessionId);

public static class AuthContext
{
    /// <summary>The caller of an endpoint that needs a signed-in, verified account (401 otherwise).</summary>
    public static Caller Require(HttpContext ctx, Db db, TimeProvider clock)
    {
        string h = ctx.Request.Headers.Authorization.ToString();
        if (!h.StartsWith("Bearer ", StringComparison.Ordinal)) throw new ApiError(401, "not_signed_in", "Sign in first.");
        string hash = Crypto.Sha256(h.Substring(7).Trim());
        using var c = db.Open();
        var row = Db.Row(c, @"SELECT t.account_id, t.session_id, t.expires_at, s.revoked_at, a.deleted_at FROM access_tokens t
                              JOIN auth_sessions s ON s.id = t.session_id JOIN accounts a ON a.id = t.account_id WHERE t.token_hash = $h", ("$h", hash));
        if (row == null || row["revoked_at"] != null || row["deleted_at"] != null) throw new ApiError(401, "invalid_token", "Your session is no longer valid. Sign in again.");
        if ((long)row["expires_at"]! <= clock.Now()) throw new ApiError(401, "token_expired", "Your session expired. Refresh it.");
        return new Caller((long)row["account_id"]!, (string)row["session_id"]!);
    }
}

/// <summary>Housekeeping every minute: expired tokens, codes, tickets and invites; stale game sessions; old audit records.</summary>
public sealed class Maintenance : BackgroundService
{
    readonly Db db; readonly TimeProvider clock; readonly ServiceOptions opts; readonly ILogger<Maintenance> log;
    public Maintenance(Db db, TimeProvider clock, ServiceOptions opts, ILogger<Maintenance> log) { this.db = db; this.clock = clock; this.opts = opts; this.log = log; }

    public static void RunOnce(Db db, long now, int retentionDays)
    {
        using var c = db.Open();
        Db.Exec(c, "DELETE FROM access_tokens WHERE expires_at <= $n", ("$n", now));
        Db.Exec(c, "DELETE FROM refresh_tokens WHERE expires_at <= $n", ("$n", now));
        Db.Exec(c, "DELETE FROM codes WHERE expires_at <= $n OR used = 1", ("$n", now));
        Db.Exec(c, "DELETE FROM join_tickets WHERE expires_at <= $n", ("$n", now));
        Db.Exec(c, "DELETE FROM invites WHERE expires_at <= $n - 86400", ("$n", now));
        Db.Exec(c, "UPDATE game_sessions SET closed_at = $n WHERE closed_at IS NULL AND heartbeat_at < $n - " + SessionsApi.StaleSeconds, ("$n", now));
        Db.Exec(c, "DELETE FROM audit WHERE at < $n - $r * 86400 AND account_id IN (SELECT id FROM accounts WHERE deleted_at IS NOT NULL)", ("$n", now), ("$r", retentionDays));
        Db.Exec(c, "DELETE FROM accounts WHERE deleted_at IS NOT NULL AND deleted_at < $n - $r * 86400", ("$n", now), ("$r", retentionDays));
    }

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        while (!stop.IsCancellationRequested)
        {
            try { RunOnce(db, clock.Now(), opts.RetentionDays); }
            catch (Exception e) { log.LogError(e, "Maintenance failed"); }
            try { await Task.Delay(TimeSpan.FromMinutes(1), stop); } catch (TaskCanceledException) { }
        }
    }
}
