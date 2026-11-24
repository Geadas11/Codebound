using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Codebound.Core.Online
{
    /// <summary>What a call to the service came back with. Offline = the service could not be reached (no internet, server down).</summary>
    public sealed class ApiResult
    {
        public bool Ok, Offline;
        public int Status;
        public string Error, Message;
        public object Data;                                       // parsed JSON (Dictionary / List / primitives)
        public Dictionary<string, object> Obj => Data as Dictionary<string, object>;
        public List<object> List => Data as List<object>;
        public string Str(string k) => Obj != null && Obj.TryGetValue(k, out var v) ? v as string : null;
        public static ApiResult NoNetwork(string why) => new ApiResult { Offline = true, Error = "offline", Message = why };
    }

    /// <summary>The HTTP layer, swappable for tests (and for UnityWebRequest if a platform ever needs it).</summary>
    public interface IHttpTransport { Task<(int status, string body)> Send(string method, string path, string json, string bearer); }

    /// <summary>System.Net.Http with a short timeout; any network failure surfaces as an exception the client turns into Offline.</summary>
    public sealed class HttpClientTransport : IHttpTransport
    {
        readonly HttpClient http;
        public HttpClientTransport(string baseUrl) { http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(12) }; }
        public async Task<(int, string)> Send(string method, string path, string json, string bearer)
        {
            var req = new HttpRequestMessage(new HttpMethod(method), path.TrimStart('/'));
            if (bearer != null) req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer);
            if (json != null) req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using (var resp = await http.SendAsync(req).ConfigureAwait(false))
                return ((int)resp.StatusCode, await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }

    /// <summary>A small text file. The sign-in keeps only the refresh token and who it belongs to there — never the password.</summary>
    public interface ITextStore { string Load(); void Save(string text); }

    public enum Connection { SignedOut, Offline, Online }

    /// <summary>
    /// The game's side of the online service. Every call is asynchronous and never throws for network trouble: an
    /// unreachable service just gives an Offline result and the game carries on locally. Access tokens are refreshed
    /// automatically (once per call); a refused refresh signs the account out on this device (local play is untouched).
    /// </summary>
    public sealed class OnlineClient
    {
        readonly IHttpTransport http;
        readonly ITextStore store;
        readonly Func<long> now;
        string access, refresh;
        long accessExpires;
        Task<bool> refreshing;

        public string PublicId { get; private set; }
        public string DisplayName { get; private set; }
        public string Email { get; private set; }
        public long LastOnline { get; private set; }               // when the service last confirmed this sign-in (unix seconds)
        public Connection State { get; private set; } = Connection.SignedOut;
        public string LastError { get; private set; }
        public bool SignedIn => refresh != null;

        public OnlineClient(IHttpTransport http, ITextStore store, Func<long> now)
        {
            this.http = http; this.store = store; this.now = now;
            try
            {
                if (store?.Load() is string s && Json.Parse(s) is Dictionary<string, object> d)
                {
                    refresh = d.TryGetValue("refresh", out var r) ? r as string : null;
                    PublicId = d.TryGetValue("publicId", out var p) ? p as string : null;
                    DisplayName = d.TryGetValue("name", out var n) ? n as string : null;
                    LastOnline = d.TryGetValue("lastOnline", out var l) && l is double ld ? (long)ld : 0;
                    if (refresh != null) State = Connection.Offline;   // until the service confirms it
                }
            }
            catch (Exception) { refresh = null; }   // a damaged file just means "signed out"
        }

        void Persist()
        {
            if (store == null) return;
            if (refresh == null) { store.Save(""); return; }
            store.Save(Json.Write(new Dictionary<string, object> { { "refresh", refresh }, { "publicId", PublicId }, { "name", DisplayName }, { "lastOnline", (double)LastOnline } }));
        }

        void SetState(Connection c, string error = null) { State = c; LastError = error; }

        void TakeTokens(Dictionary<string, object> d)
        {
            access = d["accessToken"] as string; refresh = d["refreshToken"] as string;
            accessExpires = now() + (long)(d.TryGetValue("accessExpiresIn", out var e) && e is double ed ? ed : 900) - 30;
            if (d.TryGetValue("account", out var a) && a is Dictionary<string, object> acc)
            {
                PublicId = acc["publicId"] as string; DisplayName = acc["displayName"] as string;
                if (acc.TryGetValue("email", out var em)) Email = em as string;
            }
            LastOnline = now();
            Persist();
            SetState(Connection.Online);
        }

        async Task<ApiResult> Raw(string method, string path, object body, string bearer)
        {
            (int status, string text) r;
            try { r = await http.Send(method, path, body == null ? null : Json.Write(body), bearer).ConfigureAwait(false); }
            catch (Exception e) when (!(e is OutOfMemoryException))   // network down, DNS, timeout, TLS: all mean "offline"
            {
                if (SignedIn) SetState(Connection.Offline, "service unreachable");
                return ApiResult.NoNetwork("The online service cannot be reached. You can keep playing offline.");
            }
            var res = new ApiResult { Status = r.status, Ok = r.status >= 200 && r.status < 300 };
            try { res.Data = string.IsNullOrEmpty(r.text) ? null : Json.Parse(r.text); } catch (Exception) { res.Data = null; }
            if (!res.Ok)
            {
                res.Error = res.Str("error") ?? "http_" + r.status; res.Message = res.Str("message") ?? "The service answered with an error.";
                if (r.status >= 500) { res.Offline = true; if (SignedIn) SetState(Connection.Offline, "service error"); }
            }
            else res.Message = res.Str("message");
            return res;
        }

        /// <summary>New tokens from the refresh token (one refresh at a time).</summary>
        public Task<bool> Refresh()
        {
            if (refreshing == null || refreshing.IsCompleted) refreshing = DoRefresh();
            return refreshing;
        }

        async Task<bool> DoRefresh()
        {
            if (refresh == null) return false;
            var r = await Raw("POST", "auth/refresh", new Dictionary<string, object> { { "refreshToken", refresh } }, null).ConfigureAwait(false);
            if (r.Ok) { TakeTokens(r.Obj); return true; }
            if (r.Status == 400 || r.Status == 401) SignOutLocally(r.Error);   // revoked, expired or reused: sign in again (a 429 or 5xx is not that)
            return false;
        }

        /// <summary>A call that needs the account: refreshes the access token when needed, once.</summary>
        public async Task<ApiResult> Call(string method, string path, object body = null)
        {
            if (refresh == null) return new ApiResult { Error = "not_signed_in", Message = "Sign in to use online features.", Status = 401 };
            if (access == null || now() >= accessExpires)
                if (!await Refresh().ConfigureAwait(false)) return refresh == null ? new ApiResult { Error = "not_signed_in", Message = "Sign in again.", Status = 401 } : ApiResult.NoNetwork("Offline.");
            var r = await Raw(method, path, body, access).ConfigureAwait(false);
            if (r.Status == 401 && (r.Error == "token_expired" || r.Error == "invalid_token"))
            {
                if (!await Refresh().ConfigureAwait(false)) return r;
                r = await Raw(method, path, body, access).ConfigureAwait(false);
            }
            if (r.Ok && State != Connection.Online) { LastOnline = now(); Persist(); SetState(Connection.Online); }
            return r;
        }

        /// <summary>Forgets the sign-in on this device (tokens), keeping the local profile and every local save.</summary>
        public void SignOutLocally(string why = null)
        {
            access = refresh = null; accessExpires = 0; Email = null;
            Persist();
            SetState(Connection.SignedOut, why);
        }

        // ---------------------------------------------------------------- account
        public Task<ApiResult> Register(string email, string password, string name) => Raw("POST", "auth/register", Body("email", email, "password", password, "displayName", name), null);
        public Task<ApiResult> Verify(string email, string code) => Raw("POST", "auth/verify", Body("email", email, "code", code), null);
        public Task<ApiResult> ResendCode(string email) => Raw("POST", "auth/verify/resend", Body("email", email), null);
        public Task<ApiResult> ForgotPassword(string email) => Raw("POST", "auth/password/forgot", Body("email", email), null);
        public Task<ApiResult> ResetPassword(string email, string code, string pw) => Raw("POST", "auth/password/reset", Body("email", email, "code", code, "newPassword", pw), null);

        public async Task<ApiResult> Login(string email, string password, string device)
        {
            var r = await Raw("POST", "auth/login", Body("email", email, "password", password, "device", device), null).ConfigureAwait(false);
            if (r.Ok) TakeTokens(r.Obj);
            return r;
        }

        public async Task<ApiResult> Logout()
        {
            var r = SignedIn ? await Call("POST", "auth/logout").ConfigureAwait(false) : new ApiResult { Ok = true };
            SignOutLocally();   // even offline: this device forgets the sign-in now; the server session expires on its own
            return r;
        }

        public Task<ApiResult> Me() => Call("GET", "me");
        public Task<ApiResult> UpdateProfile(Dictionary<string, object> changes) => Call("PATCH", "me/profile", changes);
        public Task<ApiResult> DeviceSessions() => Call("GET", "auth/sessions");
        public Task<ApiResult> RevokeDevice(string id) => Call("DELETE", "auth/sessions/" + Uri.EscapeDataString(id));
        public Task<ApiResult> ChangePassword(string current, string next) => Call("POST", "auth/password/change", Body("currentPassword", current, "newPassword", next));
        public Task<ApiResult> ChangeEmail(string password, string newEmail) => Call("POST", "auth/email/change", Body("password", password, "newEmail", newEmail));
        public Task<ApiResult> ConfirmEmail(string code) => Call("POST", "auth/email/confirm", Body("code", code));
        public async Task<ApiResult> DeleteAccount(string password)
        {
            var r = await Call("POST", "me/delete", Body("password", password, "confirm", "DELETE")).ConfigureAwait(false);
            if (r.Ok) SignOutLocally("account deleted");
            return r;
        }

        // ---------------------------------------------------------------- social
        public Task<ApiResult> Profile(string publicId) => Call("GET", "profiles/" + Uri.EscapeDataString(publicId));
        public Task<ApiResult> Search(string q) => Call("GET", "profiles/search?q=" + Uri.EscapeDataString(q));
        public Task<ApiResult> Presence(string status, string sessionId) => Call("POST", "me/presence", Body("status", status, "sessionId", sessionId));
        public Task<ApiResult> Friends() => Call("GET", "friends");
        public Task<ApiResult> FriendRequests() => Call("GET", "friends/requests");
        public Task<ApiResult> AddFriend(string publicId) => Call("POST", "friends/requests", Body("publicId", publicId));
        public Task<ApiResult> AcceptFriend(long id) => Call("POST", $"friends/requests/{id}/accept", Body());
        public Task<ApiResult> DeclineFriend(long id) => Call("POST", $"friends/requests/{id}/decline", Body());
        public Task<ApiResult> CancelFriendRequest(long id) => Call("DELETE", $"friends/requests/{id}");
        public Task<ApiResult> RemoveFriend(string publicId) => Call("DELETE", "friends/" + Uri.EscapeDataString(publicId));
        public Task<ApiResult> Blocks() => Call("GET", "blocks");
        public Task<ApiResult> Block(string publicId) => Call("POST", "blocks", Body("publicId", publicId));
        public Task<ApiResult> Unblock(string publicId) => Call("DELETE", "blocks/" + Uri.EscapeDataString(publicId));
        public Task<ApiResult> Report(string publicId, string reason) => Call("POST", "reports", Body("publicId", publicId, "reason", reason));
        public Task<ApiResult> Notifications() => Call("GET", "me/notifications");
        public Task<ApiResult> Favorites() => Call("GET", "me/favorites");
        public Task<ApiResult> AddFavorite(string publicId) => Call("POST", "me/favorites", Body("publicId", publicId));
        public Task<ApiResult> RemoveFavorite(string publicId) => Call("DELETE", "me/favorites/" + Uri.EscapeDataString(publicId));

        // ---------------------------------------------------------------- games
        public Task<ApiResult> HostSession(string name, string visibility, int maxPlayers, int port, int version, bool hasPassword, string lanAddress) =>
            Call("POST", "sessions", Body("name", name, "visibility", visibility, "maxPlayers", (double)maxPlayers, "port", (double)port, "version", (double)version, "hasPassword", hasPassword, "lanAddress", lanAddress));
        public Task<ApiResult> Heartbeat(string sessionId, int players) => Call("POST", $"sessions/{sessionId}/heartbeat", Body("players", (double)players));
        public Task<ApiResult> CloseSession(string sessionId) => Call("DELETE", "sessions/" + sessionId);
        public Task<ApiResult> Sessions(string query) => Call("GET", "sessions?q=" + Uri.EscapeDataString(query ?? ""));
        public Task<ApiResult> JoinSession(string sessionId) => Call("POST", $"sessions/{sessionId}/join", Body());
        public Task<ApiResult> VerifyTicket(string sessionId, string ticket) => Call("POST", $"sessions/{sessionId}/tickets/verify", Body("ticket", ticket));
        public Task<ApiResult> Kick(string sessionId, string publicId, bool ban) => Call("POST", $"sessions/{sessionId}/kick", Body("publicId", publicId, "ban", ban));
        public Task<ApiResult> PlayerLeft(string sessionId, string publicId) => Call("POST", $"sessions/{sessionId}/left", Body("publicId", publicId));
        public Task<ApiResult> History() => Call("GET", "me/sessions/history");
        public Task<ApiResult> Invites() => Call("GET", "invites");
        public Task<ApiResult> Invite(string sessionId, string publicId) => Call("POST", "invites", Body("sessionId", sessionId, "publicId", publicId));
        public Task<ApiResult> AcceptInvite(long id) => Call("POST", $"invites/{id}/accept", Body());
        public Task<ApiResult> DeclineInvite(long id) => Call("POST", $"invites/{id}/decline", Body());

        // ---------------------------------------------------------------- progress
        public Task<ApiResult> Stats() => Call("GET", "me/stats");
        public Task<ApiResult> SyncStats(StatBatch b) => Call("POST", "stats/sync", b.ToJson());
        public Task<ApiResult> CloudSaves() => Call("GET", "saves");
        public Task<ApiResult> CloudSave(string slot, int? version = null) => Call("GET", "saves/" + slot + (version.HasValue ? "?version=" + version.Value : ""));
        public Task<ApiResult> UploadSave(string slot, int baseVersion, string sha256, long clientTime, string device, byte[] data) =>
            Call("PUT", "saves/" + slot, Body("baseVersion", (double)baseVersion, "sha256", sha256, "clientTime", (double)clientTime, "device", device, "data", Convert.ToBase64String(data)));

        public async Task<bool> ServiceReachable() => !(await Raw("GET", "health", null, null).ConfigureAwait(false)).Offline;

        static Dictionary<string, object> Body(params object[] kv)
        {
            var d = new Dictionary<string, object>();
            for (int i = 0; i + 1 < kv.Length; i += 2) if (kv[i + 1] != null) d[(string)kv[i]] = kv[i + 1];
            return d;
        }
    }
}
