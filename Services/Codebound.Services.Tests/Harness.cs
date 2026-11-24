using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Codebound.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Codebound.Services.Tests;

/// <summary>The real service in this process: a temporary database, emails in the outbox, a clock the test can move.</summary>
public sealed class Server : IAsyncDisposable
{
    public readonly AdjustableClock Clock = new();
    public readonly Uri Base;
    public readonly Db Db;
    readonly WebApplication app;
    readonly string dbPath;

    public Server(Action<ServiceOptions>? configure = null)
    {
        dbPath = Path.Combine(Path.GetTempPath(), "cb-test-" + Guid.NewGuid().ToString("N") + ".db");
        var b = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        b.WebHost.UseUrls("http://127.0.0.1:0");
        b.Logging.ClearProviders();
        app = App.Build(b, o =>
        {
            o.Database = dbPath; o.DevOutbox = true; o.PasswordIterations = 1000; o.RequireHttps = false;
            o.GlobalRequestsPerMinute = 100_000; o.AuthRequestsPerMinute = 100_000; o.SearchRequestsPerMinute = 100_000;
            configure?.Invoke(o);
        }, Clock);
        app.StartAsync().GetAwaiter().GetResult();
        Base = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First() + "/");
        Db = app.Services.GetRequiredService<Db>();
    }

    public HttpClient Client(string? token = null)
    {
        var c = new HttpClient { BaseAddress = Base };
        if (token != null) c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return c;
    }

    /// <summary>The newest 6-digit code emailed to this address.</summary>
    OutboxMailer Outbox => (OutboxMailer)app.Services.GetRequiredService<IMailer>();

    public string CodeFor(string email)
    {
        var mail = Outbox.Recent().FirstOrDefault(m => m.to == email);
        if (mail.body == null) throw new Exception("no email for " + email);
        return Regex.Match(mail.body, @"\b\d{6}\b").Value;
    }

    public int Emails(string email) => Outbox.Recent().Count(m => m.to == email);

    public void Advance(TimeSpan t) => Clock.Offset += t;

    /// <summary>A registered, verified, signed-in player.</summary>
    public async Task<Player> Player(string name)
    {
        string email = name.ToLowerInvariant() + "@example.test", pw = "correct horse " + name;
        var c = Client();
        (await c.PostAsJsonAsync("auth/register", new { email, password = pw, displayName = name })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync("auth/verify", new { email, code = CodeFor(email) })).EnsureSuccessStatusCode();
        var login = await Json(await c.PostAsJsonAsync("auth/login", new { email, password = pw, device = "test" }));
        return new Player(this, name, email, pw, login.GetProperty("accessToken").GetString()!, login.GetProperty("refreshToken").GetString()!, login.GetProperty("account").GetProperty("publicId").GetString()!);
    }

    public static async Task<JsonElement> Json(HttpResponseMessage r)
    {
        string s = await r.Content.ReadAsStringAsync();
        if (!r.IsSuccessStatusCode) throw new HttpRequestException($"{(int)r.StatusCode}: {s}", null, r.StatusCode);
        return JsonDocument.Parse(s).RootElement;
    }

    /// <summary>The error code of a failed call (and its status).</summary>
    public static async Task<(HttpStatusCode status, string code)> Error(HttpResponseMessage r)
    {
        Assert.False(r.IsSuccessStatusCode, "expected an error, got " + (int)r.StatusCode);
        var e = JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;
        return (r.StatusCode, e.GetProperty("error").GetString()!);
    }

    public async ValueTask DisposeAsync()
    {
        await app.StopAsync(); await app.DisposeAsync();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var f in new[] { dbPath, dbPath + "-wal", dbPath + "-shm" }) try { File.Delete(f); } catch (IOException) { }
    }
}

public sealed record Player(Server S, string Name, string Email, string Password, string Access, string Refresh, string PublicId)
{
    public HttpClient Http => S.Client(Access);
    public Task<HttpResponseMessage> Post(string path, object body) => Http.PostAsJsonAsync(path, body);
    public Task<HttpResponseMessage> Get(string path) => Http.GetAsync(path);
    public async Task<JsonElement> GetJson(string path) => await Server.Json(await Http.GetAsync(path));
    public async Task<JsonElement> PostJson(string path, object body) => await Server.Json(await Http.PostAsJsonAsync(path, body));

    /// <summary>After the clock moved on: new tokens from the refresh token, as the game does.</summary>
    public async Task<Player> Renew()
    {
        var r = await Server.Json(await S.Client().PostAsJsonAsync("auth/refresh", new { refreshToken = Refresh }));
        return this with { Access = r.GetProperty("accessToken").GetString()!, Refresh = r.GetProperty("refreshToken").GetString()! };
    }
}
