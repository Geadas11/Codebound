using System.Net;
using System.Net.Http.Json;

namespace Codebound.Services.Tests;

public class AuthTests
{
    [Fact]
    public async Task Register_Verify_Login_Refresh_Logout()
    {
        await using var s = new Server();
        var c = s.Client();
        var reg = await c.PostAsJsonAsync("auth/register", new { email = "Ana@Example.test", password = "a long password 1", displayName = "Ana" });
        Assert.Equal(HttpStatusCode.Accepted, reg.StatusCode);
        Assert.Equal((HttpStatusCode.Forbidden, "email_not_verified"), await Server.Error(await c.PostAsJsonAsync("auth/login", new { email = "ana@example.test", password = "a long password 1" })));
        (await c.PostAsJsonAsync("auth/verify", new { email = "ana@example.test", code = s.CodeFor("ana@example.test") })).EnsureSuccessStatusCode();
        var login = await Server.Json(await c.PostAsJsonAsync("auth/login", new { email = "ANA@example.test", password = "a long password 1", device = "laptop" }));
        string access = login.GetProperty("accessToken").GetString()!, refresh = login.GetProperty("refreshToken").GetString()!;
        Assert.StartsWith("CB-", login.GetProperty("account").GetProperty("publicId").GetString());
        var me = await Server.Json(await s.Client(access).GetAsync("me"));
        Assert.Equal("Ana", me.GetProperty("displayName").GetString());
        // refresh rotates both tokens
        var r2 = await Server.Json(await c.PostAsJsonAsync("auth/refresh", new { refreshToken = refresh }));
        Assert.NotEqual(refresh, r2.GetProperty("refreshToken").GetString());
        Assert.Equal((HttpStatusCode.Unauthorized, "invalid_token"), await Server.Error(await s.Client(access).GetAsync("me")));   // the old access token went with it
        string access2 = r2.GetProperty("accessToken").GetString()!;
        (await s.Client(access2).PostAsync("auth/logout", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await s.Client(access2).GetAsync("me")).StatusCode);
        Assert.Equal((HttpStatusCode.Unauthorized, "invalid_token"), await Server.Error(await c.PostAsJsonAsync("auth/refresh", new { refreshToken = r2.GetProperty("refreshToken").GetString() })));
    }

    [Fact]
    public async Task Inputs_AreValidated_AndNamesAreUnique()
    {
        await using var s = new Server();
        var c = s.Client();
        Assert.Equal("bad_email", (await Server.Error(await c.PostAsJsonAsync("auth/register", new { email = "nope", password = "a long password 1", displayName = "Bea" }))).code);
        Assert.Equal("weak_password", (await Server.Error(await c.PostAsJsonAsync("auth/register", new { email = "b@example.test", password = "short", displayName = "Bea" }))).code);
        Assert.Equal("bad_name", (await Server.Error(await c.PostAsJsonAsync("auth/register", new { email = "b@example.test", password = "a long password 1", displayName = "<b>x</b>" }))).code);
        await s.Player("Bea");
        Assert.Equal((HttpStatusCode.Conflict, "name_taken"), await Server.Error(await c.PostAsJsonAsync("auth/register", new { email = "other@example.test", password = "a long password 1", displayName = "bea" })));
    }

    [Fact]
    public async Task AnEmailThatHasAnAccount_IsNotRevealed()
    {
        await using var s = new Server();
        var p = await s.Player("Cid");
        int before = s.Emails(p.Email);
        var again = await s.Client().PostAsJsonAsync("auth/register", new { email = p.Email, password = "another password 2", displayName = "Cid2" });
        Assert.Equal(HttpStatusCode.Accepted, again.StatusCode);                    // the same answer as a new account
        Assert.Equal(before + 1, s.Emails(p.Email));                                  // the owner is told instead
        Assert.Equal(HttpStatusCode.Accepted, (await s.Client().PostAsJsonAsync("auth/password/forgot", new { email = "nobody@example.test" })).StatusCode);
    }

    [Fact]
    public async Task WrongCodes_AreLimited_AndCodesExpire()
    {
        await using var s = new Server();
        var c = s.Client();
        await c.PostAsJsonAsync("auth/register", new { email = "dan@example.test", password = "a long password 1", displayName = "Dan" });
        string good = s.CodeFor("dan@example.test");
        string bad = good == "000000" ? "111111" : "000000";
        for (int i = 0; i < AuthApi.MaxCodeAttempts; i++) Assert.Equal("invalid_code", (await Server.Error(await c.PostAsJsonAsync("auth/verify", new { email = "dan@example.test", code = bad }))).code);
        Assert.Equal("invalid_code", (await Server.Error(await c.PostAsJsonAsync("auth/verify", new { email = "dan@example.test", code = good }))).code);   // locked after 5 tries
        s.Advance(TimeSpan.FromMinutes(2));
        await c.PostAsJsonAsync("auth/verify/resend", new { email = "dan@example.test" });
        string fresh = s.CodeFor("dan@example.test");
        s.Advance(TimeSpan.FromMinutes(31));
        Assert.Equal("invalid_code", (await Server.Error(await c.PostAsJsonAsync("auth/verify", new { email = "dan@example.test", code = fresh }))).code);   // expired
    }

    [Fact]
    public async Task ExpiredTokens_AndReusedRefreshTokens_AreRefused()
    {
        await using var s = new Server();
        var p = await s.Player("Eve");
        s.Advance(TimeSpan.FromMinutes(16));
        Assert.Equal((HttpStatusCode.Unauthorized, "token_expired"), await Server.Error(await p.Get("me")));
        var r = await Server.Json(await s.Client().PostAsJsonAsync("auth/refresh", new { refreshToken = p.Refresh }));
        Assert.Equal(HttpStatusCode.OK, (await s.Client(r.GetProperty("accessToken").GetString()).GetAsync("me")).StatusCode);
        // the old refresh token again (a stolen copy): the whole device session ends
        Assert.Equal("token_reused", (await Server.Error(await s.Client().PostAsJsonAsync("auth/refresh", new { refreshToken = p.Refresh }))).code);
        Assert.Equal(HttpStatusCode.Unauthorized, (await s.Client(r.GetProperty("accessToken").GetString()).GetAsync("me")).StatusCode);
        // refresh tokens expire too
        var q = await s.Player("Fay");
        s.Advance(TimeSpan.FromDays(31));
        Assert.Equal(HttpStatusCode.Unauthorized, (await s.Client().PostAsJsonAsync("auth/refresh", new { refreshToken = q.Refresh })).StatusCode);
    }

    [Fact]
    public async Task WrongPasswords_LockTheAccountForAWhile()
    {
        await using var s = new Server();
        var p = await s.Player("Gus");
        var c = s.Client();
        for (int i = 0; i < AuthApi.LockAfterFailures; i++) Assert.Equal("invalid_credentials", (await Server.Error(await c.PostAsJsonAsync("auth/login", new { email = p.Email, password = "wrong password!" }))).code);
        Assert.Equal((HttpStatusCode.TooManyRequests, "account_locked"), await Server.Error(await c.PostAsJsonAsync("auth/login", new { email = p.Email, password = p.Password })));
        s.Advance(TimeSpan.FromMinutes(2));
        (await c.PostAsJsonAsync("auth/login", new { email = p.Email, password = p.Password })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PasswordReset_ChangesIt_AndSignsEveryDeviceOut()
    {
        await using var s = new Server();
        var p = await s.Player("Hal");
        var c = s.Client();
        await c.PostAsJsonAsync("auth/password/forgot", new { email = p.Email });
        (await c.PostAsJsonAsync("auth/password/reset", new { email = p.Email, code = s.CodeFor(p.Email), newPassword = "a brand new password" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await p.Get("me")).StatusCode);
        Assert.Equal("invalid_credentials", (await Server.Error(await c.PostAsJsonAsync("auth/login", new { email = p.Email, password = p.Password }))).code);
        (await c.PostAsJsonAsync("auth/login", new { email = p.Email, password = "a brand new password" })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Sessions_CanBeListedAndRevoked_PasswordChangeSignsOthersOut()
    {
        await using var s = new Server();
        var p = await s.Player("Ivy");
        var second = await Server.Json(await s.Client().PostAsJsonAsync("auth/login", new { email = p.Email, password = p.Password, device = "phone" }));
        var list = await p.GetJson("auth/sessions");
        Assert.Equal(2, list.GetArrayLength());
        string phoneId = second.GetProperty("sessionId").GetString()!;
        (await p.Http.DeleteAsync("auth/sessions/" + phoneId)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await s.Client(second.GetProperty("accessToken").GetString()).GetAsync("me")).StatusCode);
        var third = await Server.Json(await s.Client().PostAsJsonAsync("auth/login", new { email = p.Email, password = p.Password, device = "tablet" }));
        Assert.Equal("wrong_password", (await Server.Error(await p.Post("auth/password/change", new { currentPassword = "nope", newPassword = "another long one" }))).code);
        (await p.Post("auth/password/change", new { currentPassword = p.Password, newPassword = "another long one" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, (await p.Get("me")).StatusCode);                                       // this device stays
        Assert.Equal(HttpStatusCode.Unauthorized, (await s.Client(third.GetProperty("accessToken").GetString()).GetAsync("me")).StatusCode);
    }

    [Fact]
    public async Task EmailChange_NeedsTheCodeFromTheNewAddress()
    {
        await using var s = new Server();
        var p = await s.Player("Joe");
        (await p.Post("auth/email/change", new { password = p.Password, newEmail = "joe.new@example.test" })).EnsureSuccessStatusCode();
        Assert.Equal("Joe", (await p.GetJson("me")).GetProperty("displayName").GetString());
        Assert.Equal(p.Email, (await p.GetJson("me")).GetProperty("email").GetString(), ignoreCase: true);    // not yet
        (await p.Post("auth/email/confirm", new { code = s.CodeFor("joe.new@example.test") })).EnsureSuccessStatusCode();
        Assert.Equal("joe.new@example.test", (await p.GetJson("me")).GetProperty("email").GetString());
        Assert.True(s.Emails(p.Email) >= 2, "the old address was told");
    }

    [Fact]
    public async Task DeletingTheAccount_RemovesPersonalData()
    {
        await using var s = new Server();
        var p = await s.Player("Kim"); var q = await s.Player("Lou");
        (await p.Post("friends/requests", new { publicId = q.PublicId })).EnsureSuccessStatusCode();
        Assert.Equal("confirm", (await Server.Error(await p.Post("me/delete", new { password = p.Password, confirm = "yes" }))).code);
        (await p.Post("me/delete", new { password = p.Password, confirm = "DELETE" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await p.Get("me")).StatusCode);
        Assert.Equal(0, (await q.GetJson("friends/requests")).GetProperty("incoming").GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound, (await q.Get("profiles/" + p.PublicId)).StatusCode);
        Assert.Equal("invalid_credentials", (await Server.Error(await s.Client().PostAsJsonAsync("auth/login", new { email = p.Email, password = p.Password }))).code);
        // the email can be used again, and after the retention period the row is gone
        Maintenance.RunOnce(s.Db, s.Clock.GetUtcNow().ToUnixTimeSeconds() + 31L * 86400, 30);
        using var c = s.Db.Open();
        Assert.Equal(1L, Db.Scalar(c, "SELECT COUNT(*) FROM accounts"));
    }

    [Fact]
    public async Task TooManyRequests_AreRefused()
    {
        await using var s = new Server(o => o.AuthRequestsPerMinute = 3);
        var c = s.Client();
        for (int i = 0; i < 3; i++) await c.PostAsJsonAsync("auth/login", new { email = "x@example.test", password = "whatever password" });
        Assert.Equal((HttpStatusCode.TooManyRequests, "rate_limited"), await Server.Error(await c.PostAsJsonAsync("auth/login", new { email = "x@example.test", password = "whatever password" })));
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("health")).StatusCode);   // other endpoints still answer
    }

    [Fact]
    public async Task ProductionSettings_RequireHttps_AndExportReturnsEverything()
    {
        await using var s = new Server(o => o.RequireHttps = true);
        Assert.Equal("https_required", (await Server.Error(await s.Client().GetAsync("health"))).code);
        await using var t = new Server();
        var p = await t.Player("Max");
        var data = await p.GetJson("me/export");
        Assert.Equal(p.Email, data.GetProperty("account").GetProperty("email").GetString());
        Assert.True(data.GetProperty("activity").GetArrayLength() >= 2);
    }
}
