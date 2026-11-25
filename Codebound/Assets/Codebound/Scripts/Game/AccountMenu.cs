using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Codebound.Core;
using Codebound.Core.Online;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// Account, friends, online games, statistics and cloud saves (Codebound Services). Opened from the title screen or
    /// the Esc menu. Nothing here is needed to play: every screen says so when the service is unreachable.
    /// Passwords typed here go straight to the service over TLS and are cleared from memory right after.
    /// </summary>
    public sealed class AccountMenu : MonoBehaviour
    {
        static AccountMenu instance;
        public static bool IsOpen => instance != null && instance.open;
        public static void Open() { if (instance == null) instance = new GameObject("Codebound Account").AddComponent<AccountMenu>(); instance.Show(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => instance = null;

        bool open, busy;
        string screen = "login", tab = "profile", msg, url;
        string email = "", password = "", password2 = "", playerName = "", code = "", newPassword = "", newEmail = "", deleteText = "", search = "", query = "", reportReason = "", worldPassword = "";
        ApiResult me, friends, requests, blocks, results, sessions, favorites, history, stats, devices;
        List<SaveStatus> saves;
        Vector2 scroll;
        GUIStyle box, title, label, small, btn, tabOn, field, warn, toggle;
        int styleVersion = -1;

        void Awake() { DontDestroyOnLoad(gameObject); }

        void Show()
        {
            open = true; msg = Online.Notice; Online.Notice = null; url = Online.ServiceUrl;
            screen = Online.SignedIn ? "main" : "login";
            if (GameBootstrap.Session is GameSession s) { s.PanelOpen = true; s.KeyboardCaptured = true; }
            if (Online.SignedIn) Refresh(tab);
        }

        void Close()
        {
            open = false; password = password2 = newPassword = deleteText = "";
            if (GameBootstrap.Session is GameSession s && !TitleMenu.IsOpen) { s.PanelOpen = false; s.KeyboardCaptured = false; }
        }

        void Update()
        {
            if (open && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
        }

        /// <summary>Runs a service call; shows its message; never throws into the game.</summary>
        async void Do(Func<Task<ApiResult>> call, Action<ApiResult> ok = null, bool quiet = false)
        {
            if (Online.Client == null) { msg = Loc.T("online.notConfigured", "Offline mode · no online service set"); return; }
            busy = true;
            ApiResult r;
            try { r = await call(); }
            catch (Exception e) { r = new ApiResult { Error = "error", Message = e.Message }; }
            busy = false;
            if (r.Ok) { if (!quiet || r.Message != null) msg = r.Message; ok?.Invoke(r); }
            else msg = r.Message ?? r.Error;
            if (!Online.SignedIn && screen == "main") screen = "login";
        }

        void Refresh(string t)
        {
            tab = t; scroll = Vector2.zero;
            var c = Online.Client;
            switch (t)
            {
                case "profile": Do(c.Me, r => me = r, true); break;
                case "friends": Do(c.Friends, r => friends = r, true); Do(c.FriendRequests, r => requests = r, true); Do(c.Blocks, r => blocks = r, true); break;
                case "games": Do(() => c.Sessions(query), r => sessions = r, true); Do(c.Favorites, r => favorites = r, true); Do(c.History, r => history = r, true); _ = Online.Poll(); break;
                case "stats": Online.Journal.Seal(); Do(async () => { await Online.Journal.Sync(c); return await c.Stats(); }, r => stats = r, true); break;
                case "saves": LoadSaves(); break;
                case "account": Do(c.Me, r => me = r, true); Do(c.DeviceSessions, r => devices = r, true); break;
            }
        }

        async void LoadSaves()
        {
            busy = true;
            try { saves = await Online.Saves.Check(); msg = saves == null ? Loc.T("online.unreachable", "The online service cannot be reached. You can keep playing offline.") : null; }
            catch (Exception e) { msg = e.Message; }
            busy = false;
        }

        void Styles()
        {
            if (box != null && styleVersion == Settings.Version) return;
            styleVersion = Settings.Version;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.03f, 0.04f, 0.06f, 0.97f)); bg.Apply();
            var on = new Texture2D(1, 1); on.SetPixel(0, 0, new Color(0.12f, 0.3f, 0.24f)); on.Apply();
            box = new GUIStyle(UiTheme.Panel) { padding = new RectOffset(14, 14, 10, 10) };
            title = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(22), fontStyle = FontStyle.Bold, normal = { textColor = UiTheme.Accent } };
            label = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(14), wordWrap = true, richText = true, normal = { textColor = new Color(0.82f, 0.9f, 0.86f) } };
            small = new GUIStyle(label) { fontSize = UiGui.Font(12), normal = { textColor = UiTheme.Muted } };
            warn = new GUIStyle(label) { normal = { textColor = new Color(1f, 0.78f, 0.35f) } };
            btn = new GUIStyle(GUI.skin.button) { fontSize = UiGui.Font(14) };
            tabOn = new GUIStyle(btn) { normal = { background = UiTheme.RowSelTex, textColor = Color.white } };
            field = new GUIStyle(GUI.skin.textField) { fontSize = UiGui.Font(14) };
            toggle = new GUIStyle(GUI.skin.toggle) { fontSize = UiGui.Font(14), wordWrap = true, normal = { textColor = label.normal.textColor }, onNormal = { textColor = Color.white } };
        }

        void OnGUI()
        {
            if (!open) return;
            GUI.depth = -20;   // over the title screen and the HUD
            UiGui.Begin();
            Styles();
            float w = Mathf.Min(760, UiGui.W - 30), h = Mathf.Min(620, UiGui.H - 40);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.T("acc.title", "Codebound account"), title);
            GUILayout.FlexibleSpace();
            GUILayout.Label(Online.StatusLine(), small);
            if (GUILayout.Button(Loc.T("net.close", "Close (Esc)"), btn, GUILayout.Width(110))) Close();
            GUILayout.EndHorizontal();
            if (Online.Client == null || Online.Client.State == Connection.Offline)
                GUILayout.Label(Loc.T("acc.offlineNote", "Offline: your worlds, saves, building and programs all work as usual. Online features return when the service can be reached."), warn);
            if (busy) GUILayout.Label(Loc.T("acc.working", "Working…"), small);
            if (!string.IsNullOrEmpty(msg)) GUILayout.Label(msg, label);
            scroll = GUILayout.BeginScrollView(scroll);
            if (!Online.SignedIn) SignedOut(); else SignedIn();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        // ------------------------------------------------------------------ helpers
        string Field(string caption, string value, int max = 64, bool secret = false)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(caption, label, GUILayout.Width(170));
            value = secret ? GUILayout.PasswordField(value ?? "", '*', max, field) : GUILayout.TextField(value ?? "", max, field);
            GUILayout.EndHorizontal();
            return value;
        }

        bool Btn(string text, float width = 0) { GUI.enabled = !busy; bool b = width > 0 ? GUILayout.Button(text, btn, GUILayout.Width(width)) : GUILayout.Button(text, btn); GUI.enabled = true; return b; }

        static string S(object o, string k) => o is Dictionary<string, object> d && d.TryGetValue(k, out var v) && v != null ? (v is double n ? n.ToString("0") : v.ToString()) : "";
        static long L(object o, string k) => o is Dictionary<string, object> d && d.TryGetValue(k, out var v) && v is double n ? (long)n : 0;
        static List<object> Items(ApiResult r) => r?.List ?? new List<object>();
        static List<object> Items(ApiResult r, string k) => r?.Obj != null && r.Obj.TryGetValue(k, out var v) && v is List<object> l ? l : new List<object>();
        static string When(long unix) => unix <= 0 ? "—" : DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        static string Presence(string p) => Loc.T("presence." + p, p == "in_game" ? "in a game" : p);

        // ------------------------------------------------------------------ signed out
        void SignedOut()
        {
            if (Online.Client == null || screen == "service") { Service(); return; }
            switch (screen)
            {
                case "register":
                    GUILayout.Label(Loc.T("acc.registerTitle", "Create an account"), title);
                    email = Field(Loc.T("acc.email", "Email"), email, 254);
                    playerName = Field(Loc.T("acc.name", "Player name"), playerName, 16);
                    password = Field(Loc.T("acc.password", "Password (10+ characters)"), password, 128, true);
                    password2 = Field(Loc.T("acc.password2", "Repeat password"), password2, 128, true);
                    GUILayout.Label(Loc.T("acc.privacy", "Your email is private and is only used to sign in and recover the account. Play statistics are kept only while you allow it (Profile → statistics). You can export or delete everything at any time."), small);
                    if (Btn(Loc.T("acc.create", "Create account")))
                    {
                        if (password != password2) { msg = Loc.T("acc.mismatch", "The passwords are different."); break; }
                        Do(() => Online.Client.Register(email.Trim(), password, playerName.Trim()), _ => { screen = "verify"; password = password2 = ""; });
                    }
                    if (Btn(Loc.T("acc.back", "Back"))) screen = "login";
                    break;
                case "verify":
                    GUILayout.Label(Loc.T("acc.verifyTitle", "Confirm your email"), title);
                    GUILayout.Label(Loc.T("acc.verifyHelp", "We sent a 6-digit code to your email (valid for 30 minutes)."), small);
                    email = Field(Loc.T("acc.email", "Email"), email, 254);
                    code = Field(Loc.T("acc.code", "Code"), code, 6);
                    if (Btn(Loc.T("acc.verify", "Confirm"))) Do(() => Online.Client.Verify(email.Trim(), code.Trim()), _ => { screen = "login"; code = ""; });
                    if (Btn(Loc.T("acc.resend", "Send a new code"))) Do(() => Online.Client.ResendCode(email.Trim()));
                    if (Btn(Loc.T("acc.back", "Back"))) screen = "login";
                    break;
                case "forgot":
                    GUILayout.Label(Loc.T("acc.forgotTitle", "Recover the account"), title);
                    email = Field(Loc.T("acc.email", "Email"), email, 254);
                    if (Btn(Loc.T("acc.sendCode", "Send me a code"))) Do(() => Online.Client.ForgotPassword(email.Trim()), _ => screen = "reset");
                    if (Btn(Loc.T("acc.haveCode", "I already have a code"))) screen = "reset";
                    if (Btn(Loc.T("acc.back", "Back"))) screen = "login";
                    break;
                case "reset":
                    GUILayout.Label(Loc.T("acc.resetTitle", "Choose a new password"), title);
                    email = Field(Loc.T("acc.email", "Email"), email, 254);
                    code = Field(Loc.T("acc.code", "Code"), code, 6);
                    newPassword = Field(Loc.T("acc.newPassword", "New password"), newPassword, 128, true);
                    if (Btn(Loc.T("acc.reset", "Change password"))) Do(() => Online.Client.ResetPassword(email.Trim(), code.Trim(), newPassword), _ => { screen = "login"; newPassword = code = ""; });
                    if (Btn(Loc.T("acc.back", "Back"))) screen = "login";
                    break;
                default:
                    GUILayout.Label(Loc.T("acc.loginTitle", "Sign in"), title);
                    GUILayout.Label(Loc.T("acc.localNote", "Without an account you play with a local profile: everything works, only friends, online games, account statistics and cloud saves need one."), small);
                    email = Field(Loc.T("acc.email", "Email"), email, 254);
                    password = Field(Loc.T("acc.password1", "Password"), password, 128, true);
                    if (Btn(Loc.T("acc.login", "Sign in")))
                        Do(() => Online.Client.Login(email.Trim(), password, SystemInfo.deviceName), _ => { password = ""; screen = "main"; Online.SyncStats(); Refresh("profile"); });
                    GUILayout.BeginHorizontal();
                    if (Btn(Loc.T("acc.toRegister", "Create account"))) screen = "register";
                    if (Btn(Loc.T("acc.toForgot", "Forgot password"))) screen = "forgot";
                    if (Btn(Loc.T("acc.toVerify", "Confirm email"))) screen = "verify";
                    if (Btn(Loc.T("acc.toService", "Service…"))) screen = "service";
                    GUILayout.EndHorizontal();
                    break;
            }
        }

        void Service()
        {
            GUILayout.Label(Loc.T("acc.serviceTitle", "Online service"), title);
            GUILayout.Label(Loc.T("acc.serviceHelp", "The address of a Codebound Services server (https). Leave it empty to play offline only."), small);
            url = Field(Loc.T("acc.url", "Address"), url, 200);
            if (Btn(Loc.T("acc.save", "Save")))
            {
                msg = Online.SetUrl(url);
                if (msg == null) screen = Online.SignedIn ? "main" : "login";
            }
            if (Online.Client != null && Btn(Loc.T("acc.back", "Back"))) screen = "login";
        }

        // ------------------------------------------------------------------ signed in
        void SignedIn()
        {
            GUILayout.BeginHorizontal();
            foreach (var (id, text) in new[] { ("profile", Loc.T("acc.tab.profile", "Profile")), ("friends", Loc.T("acc.tab.friends", "Friends") + (Online.FriendRequests > 0 ? $" ({Online.FriendRequests})" : "")),
                                               ("games", Loc.T("acc.tab.games", "Games") + (Online.Invites.Count > 0 ? $" ({Online.Invites.Count})" : "")), ("stats", Loc.T("acc.tab.stats", "Statistics")),
                                               ("saves", Loc.T("acc.tab.saves", "Cloud saves")), ("account", Loc.T("acc.tab.account", "Account")) })
                if (GUILayout.Button(text, tab == id ? tabOn : btn)) Refresh(id);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
            switch (tab)
            {
                case "profile": Profile(); break;
                case "friends": Friends(); break;
                case "games": Games(); break;
                case "stats": Stats(); break;
                case "saves": Saves(); break;
                case "account": Account(); break;
            }
        }

        Dictionary<string, object> edit;

        void Profile()
        {
            var p = me?.Obj;
            if (p == null) { GUILayout.Label(Loc.T("acc.loading", "Loading… (needs the service)"), small); return; }
            edit ??= new Dictionary<string, object> { { "avatar", (double)L(p, "avatar") }, { "bio", S(p, "bio") }, { "visibility", S(p, "visibility") }, { "allowRequests", S(p, "allowRequests") },
                                                       { "showPresence", L(p, "showPresence") == 1 }, { "statsConsent", L(p, "statsConsent") == 1 } };
            GUILayout.Label($"<b>{S(p, "displayName")}</b>   <size=12>{S(p, "publicId")} · {Loc.T("acc.since", "since {0}", When(L(p, "createdAt")))}</size>", label);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.T("acc.avatar", "Avatar"), label, GUILayout.Width(170));
            int av = (int)(double)edit["avatar"];
            if (GUILayout.Button("<", btn, GUILayout.Width(30))) av = (av + 15) % 16;
            GUI.color = Color.HSVToRGB(av / 16f, 0.5f, 1f); GUILayout.Label("■ " + (av + 1), label, GUILayout.Width(50)); GUI.color = Color.white;
            if (GUILayout.Button(">", btn, GUILayout.Width(30))) av = (av + 1) % 16;
            edit["avatar"] = (double)av;
            GUILayout.EndHorizontal();
            edit["bio"] = Field(Loc.T("acc.bio", "About me"), (string)edit["bio"], 160);
            Choice("visibility", Loc.T("acc.visibility", "Who sees my profile"), "public", "friends", "private");
            Choice("allowRequests", Loc.T("acc.allowRequests", "Friend requests from"), "everyone", "nobody");
            edit["showPresence"] = GUILayout.Toggle((bool)edit["showPresence"], " " + Loc.T("acc.showPresence", "Show friends when I am online"), toggle);
            edit["statsConsent"] = GUILayout.Toggle((bool)edit["statsConsent"], " " + Loc.T("acc.statsConsent", "Keep my play statistics and achievements on the service (turning this off deletes them)"), toggle);
            if (Btn(Loc.T("acc.save", "Save")))
                Do(() => Online.Client.UpdateProfile(new Dictionary<string, object>(edit)), _ => { edit = null; Do(Online.Client.Me, r => me = r, true); msg = Loc.T("acc.saved", "Saved."); });
        }

        void Choice(string key, string caption, params string[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(caption, label, GUILayout.Width(170));
            foreach (var o in options)
                if (GUILayout.Button(Loc.T("acc.opt." + o, o), (string)edit[key] == o ? tabOn : btn)) edit[key] = o;
            GUILayout.EndHorizontal();
        }

        bool CanInvite => Online.HostedSession != null || (Net.IsGuest && Net.OnlineSession != null);

        void Friends()
        {
            var c = Online.Client;
            GUILayout.BeginHorizontal();
            search = GUILayout.TextField(search, 16, field);
            if (Btn(Loc.T("acc.search", "Find player"), 130)) Do(() => c.Search(search.Trim()), r => results = r);
            GUILayout.EndHorizontal();
            GUILayout.Label(Loc.T("acc.searchHelp", "Search by name (3+ letters) or by player id (CB-…)."), small);
            foreach (var o in Items(results))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{S(o, "displayName")} <size=11>{S(o, "publicId")}</size>", label);
                if (!(o is Dictionary<string, object> d && d.TryGetValue("friend", out var fr) && fr is true) && Btn(Loc.T("acc.add", "Add friend"), 110))
                    Do(() => c.AddFriend(S(o, "publicId")), _ => Refresh("friends"));
                if (Btn(Loc.T("acc.block", "Block"), 70)) Do(() => c.Block(S(o, "publicId")), _ => { results = null; Refresh("friends"); });
                if (Btn(Loc.T("acc.report", "Report"), 70)) { reportReason = ""; reportTarget = S(o, "publicId"); }
                GUILayout.EndHorizontal();
            }
            if (reportTarget != null)
            {
                reportReason = Field(Loc.T("acc.reportWhy", "What happened?"), reportReason, 500);
                GUILayout.BeginHorizontal();
                if (Btn(Loc.T("acc.send", "Send report"))) { var t = reportTarget; reportTarget = null; Do(() => c.Report(t, reportReason.Trim())); }
                if (Btn(Loc.T("acc.cancel", "Cancel"))) reportTarget = null;
                GUILayout.EndHorizontal();
            }

            var incoming = Items(requests, "incoming"); var outgoing = Items(requests, "outgoing");
            if (incoming.Count + outgoing.Count > 0) GUILayout.Label("<b>" + Loc.T("acc.requests", "Friend requests") + "</b>", label);
            foreach (var o in incoming)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Loc.T("acc.wantsFriend", "{0} wants to be your friend", S(o, "displayName")), label);
                if (Btn(Loc.T("acc.accept", "Accept"), 80)) Do(() => c.AcceptFriend(L(o, "id")), _ => Refresh("friends"));
                if (Btn(Loc.T("acc.decline", "Decline"), 80)) Do(() => c.DeclineFriend(L(o, "id")), _ => Refresh("friends"));
                if (Btn(Loc.T("acc.block", "Block"), 70)) Do(() => c.Block(S(o, "publicId")), _ => Refresh("friends"));
                GUILayout.EndHorizontal();
            }
            foreach (var o in outgoing)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Loc.T("acc.waiting", "Waiting for {0}", S(o, "displayName")), small);
                if (Btn(Loc.T("acc.cancel", "Cancel"), 80)) Do(() => c.CancelFriendRequest(L(o, "id")), _ => Refresh("friends"));
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("<b>" + Loc.T("acc.friends", "Friends") + "</b>", label);
            var list = Items(friends);
            if (list.Count == 0) GUILayout.Label(Loc.T("acc.noFriends", "No friends yet. Find players above."), small);
            foreach (var o in list)
            {
                GUILayout.BeginHorizontal();
                string pres = S(o, "presence");
                GUI.color = pres == "offline" ? new Color(0.6f, 0.6f, 0.6f) : pres == "busy" ? new Color(1f, 0.75f, 0.3f) : new Color(0.5f, 1f, 0.6f);
                GUILayout.Label("●", label, GUILayout.Width(16)); GUI.color = Color.white;
                GUILayout.Label($"{S(o, "displayName")} <size=11>{Presence(pres)}</size>", label);
                if (CanInvite && Btn(Loc.T("acc.invite", "Invite"), 70))
                    Do(() => c.Invite(Online.HostedSession ?? Net.OnlineSession, S(o, "publicId")));
                if (pres == "in_game" && S(o, "sessionId") is string sid && sid.Length > 0 && Net.Current == Net.Mode.Off && Btn(Loc.T("acc.join", "Join"), 60))
                    JoinGame(sid, 0);
                if (Btn(Loc.T("acc.remove", "Remove"), 75)) Do(() => c.RemoveFriend(S(o, "publicId")), _ => Refresh("friends"));
                if (Btn(Loc.T("acc.block", "Block"), 70)) Do(() => c.Block(S(o, "publicId")), _ => Refresh("friends"));
                GUILayout.EndHorizontal();
            }
            var bl = Items(blocks);
            if (bl.Count > 0) GUILayout.Label("<b>" + Loc.T("acc.blocked", "Blocked") + "</b>", label);
            foreach (var o in bl)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(S(o, "displayName"), small);
                if (Btn(Loc.T("acc.unblock", "Unblock"), 90)) Do(() => c.Unblock(S(o, "publicId")), _ => Refresh("friends"));
                GUILayout.EndHorizontal();
            }
        }

        string reportTarget;

        async void JoinGame(string sessionId, long inviteId)
        {
            busy = true;
            if (Net.MyName != Online.Client.DisplayName) Net.MyName = Online.Client.DisplayName;
            msg = await Online.Join(sessionId, worldPassword, inviteId);
            busy = false;
        }

        void Games()
        {
            var c = Online.Client;
            worldPassword = Field(Loc.T("acc.worldPassword", "World password (if the host set one)"), worldPassword, 64, true);
            if (Online.Invites.Count > 0) GUILayout.Label("<b>" + Loc.T("acc.invites", "Invites") + "</b>", label);
            foreach (var o in Online.Invites.ToArray())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Loc.T("acc.invitedBy", "{0} invites you to {1}", S(o, "fromName"), S(o, "sessionName")), label);
                if (Btn(Loc.T("acc.join", "Join"), 60)) { Online.Invites.Remove(o); JoinGame(null, L(o, "id")); }
                if (Btn(Loc.T("acc.decline", "Decline"), 80)) { Online.Invites.Remove(o); Do(() => c.DeclineInvite(L(o, "id"))); }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("<b>" + Loc.T("acc.onlineGames", "Online games") + "</b>  <size=11>" + Loc.T("acc.gamesHelp", "public games and your friends' games") + "</size>", label);
            GUILayout.BeginHorizontal();
            query = GUILayout.TextField(query, 40, field);
            if (Btn(Loc.T("acc.refresh", "Refresh"), 100)) Do(() => c.Sessions(query), r => sessions = r, true);
            GUILayout.EndHorizontal();
            var list = Items(sessions);
            if (list.Count == 0) GUILayout.Label(Loc.T("acc.noGames", "No games right now."), small);
            foreach (var o in list)
            {
                var host = o is Dictionary<string, object> d && d.TryGetValue("host", out var h) ? h : null;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{S(o, "name")} <size=11>{S(host, "displayName")} · {S(o, "players")}/{S(o, "maxPlayers")} · {Loc.T("online.vis." + S(o, "visibility"), S(o, "visibility"))}{(S(o, "hasPassword") == "True" ? " · 🔒" : "")}</size>", label);
                GUI.enabled = Net.Current == Net.Mode.Off;
                if (Btn(Loc.T("acc.join", "Join"), 60)) JoinGame(S(o, "id"), 0);
                GUI.enabled = true;
                if (Btn("★", 30)) Do(() => c.AddFavorite(S(host, "publicId")), _ => Do(c.Favorites, r => favorites = r, true));
                GUILayout.EndHorizontal();
            }
            var fav = Items(favorites);
            if (fav.Count > 0) GUILayout.Label("<b>" + Loc.T("acc.favorites", "Favourite hosts") + "</b>", label);
            foreach (var o in fav)
            {
                GUILayout.BeginHorizontal();
                string active = S(o, "activeSession");
                GUILayout.Label($"{S(o, "displayName")} <size=11>{(active.Length > 0 ? Loc.T("acc.playingNow", "playing now") : Loc.T("presence.offline", "offline"))}</size>", label);
                if (active.Length > 0 && Net.Current == Net.Mode.Off && Btn(Loc.T("acc.join", "Join"), 60)) JoinGame(active, 0);
                if (Btn("✕", 30)) Do(() => c.RemoveFavorite(S(o, "publicId")), _ => Do(c.Favorites, r => favorites = r, true));
                GUILayout.EndHorizontal();
            }
            var joined = Items(history, "joined"); var hosted = Items(history, "hosted");
            if (joined.Count + hosted.Count > 0) GUILayout.Label("<b>" + Loc.T("acc.history", "Recent games") + "</b>", label);
            foreach (var o in hosted) GUILayout.Label(Loc.T("acc.hosted", "Hosted {0} · {1}", S(o, "name"), When(L(o, "startedAt"))), small);
            foreach (var o in joined) GUILayout.Label(Loc.T("acc.joined", "Joined {0} ({1}) · {2}", S(o, "name"), S(o, "hostName"), When(L(o, "joinedAt"))), small);
            GUILayout.Label(Loc.T("acc.hostHelp", "To host: open the Multiplayer panel ({0}), host the world, then list it online.", Controls.Name("network")), small);
        }

        void Stats()
        {
            var j = Online.Journal;
            GUILayout.Label("<b>" + Loc.T("acc.localStats", "On this PC") + "</b>  <size=11>" + Loc.T("acc.pending", "{0} batches waiting to sync", j.Pending.Count) + "</size>", label);
            foreach (var kv in j.Totals) GUILayout.Label($"{Loc.T("stat." + kv.Key, kv.Key)}: {kv.Value}", small);
            if (Btn(Loc.T("acc.syncNow", "Sync now"), 140)) Refresh("stats");
            var o = stats?.Obj;
            if (o == null) return;
            GUILayout.Label("<b>" + Loc.T("acc.accountStats", "Account (checked by the service)") + "</b>", label);
            if (o.TryGetValue("stats", out var st) && st is Dictionary<string, object> sd) foreach (var kv in sd) GUILayout.Label($"{Loc.T("stat." + kv.Key, kv.Key)}: {S(sd, kv.Key)}", small);
            if (o.TryGetValue("verified", out var vf) && vf is Dictionary<string, object> vd) foreach (var kv in vd) GUILayout.Label($"{Loc.T("stat." + kv.Key, kv.Key)}: {S(vd, kv.Key)}", small);
            var got = new HashSet<string>();
            foreach (var a in Items(stats, "achievements")) got.Add(S(a, "achievement"));
            GUILayout.Label("<b>" + Loc.T("acc.achievements", "Achievements") + $"</b> {got.Count}/{Items(stats, "definitions").Count}", label);
            foreach (var d in Items(stats, "definitions"))
                GUILayout.Label((got.Contains(S(d, "id")) ? "★ " : "☆ ") + Loc.T("ach." + S(d, "id"), S(d, "name")), got.Contains(S(d, "id")) ? label : small);
        }

        void Saves()
        {
            GUILayout.Label(Loc.T("acc.savesHelp", "Cloud copies of your local saves. A local save is never deleted: when you take the cloud copy, the local one is first kept in saves/backups. If both changed, you choose."), small);
            if (Btn(Loc.T("acc.refresh", "Refresh"), 100)) LoadSaves();
            if (saves == null) return;
            foreach (var st in saves)
            {
                GUILayout.BeginHorizontal();
                string state = Loc.T("save.state." + st.State, st.State.ToString());
                GUILayout.Label($"<b>{st.Slot}</b> <size=11>{state}" + (st.CloudVersion > 0 ? $" · v{st.CloudVersion} {When(st.CloudTime)} {st.CloudDevice}" : "") + "</size>", label);
                switch (st.State)
                {
                    case SaveSyncState.LocalOnly: case SaveSyncState.LocalNewer:
                        if (Btn(Loc.T("acc.upload", "Upload"), 100)) Do(() => Online.Saves.Upload(st), _ => LoadSaves());
                        break;
                    case SaveSyncState.CloudOnly: case SaveSyncState.CloudNewer:
                        if (Btn(Loc.T("acc.download", "Download"), 100)) Do(() => Online.Saves.Download(st, asCopy: false), _ => LoadSaves());
                        break;
                    case SaveSyncState.Conflict:
                        if (Btn(Loc.T("acc.keepLocal", "Keep mine"), 100)) Do(() => Online.Saves.Upload(st, keepLocalOverCloud: true), _ => LoadSaves());
                        if (Btn(Loc.T("acc.keepCloud", "Keep cloud"), 100)) Do(() => Online.Saves.Download(st, asCopy: false), _ => LoadSaves());
                        if (Btn(Loc.T("acc.keepBoth", "Keep both"), 100)) Do(() => Online.Saves.Download(st, asCopy: true), _ => LoadSaves());
                        break;
                }
                GUILayout.EndHorizontal();
            }
        }

        void Account()
        {
            var c = Online.Client;
            var p = me?.Obj;
            GUILayout.Label($"{Loc.T("acc.email", "Email")}: {S(p, "email")}", label);
            GUILayout.Label("<b>" + Loc.T("acc.devices", "Signed-in devices") + "</b>", label);
            foreach (var d in Items(devices))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{S(d, "device")} <size=11>{Loc.T("acc.lastUsed", "last used {0}", When(L(d, "lastUsed")))}{(S(d, "current") == "1" ? " · " + Loc.T("acc.thisDevice", "this device") : "")}</size>", label);
                if (S(d, "current") != "1" && Btn(Loc.T("acc.signOutDevice", "Sign out"), 100)) Do(() => c.RevokeDevice(S(d, "id")), _ => Refresh("account"));
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("<b>" + Loc.T("acc.changePassword", "Change password") + "</b>", label);
            password = Field(Loc.T("acc.currentPassword", "Current password"), password, 128, true);
            newPassword = Field(Loc.T("acc.newPassword", "New password"), newPassword, 128, true);
            if (Btn(Loc.T("acc.reset", "Change password"), 180)) Do(() => c.ChangePassword(password, newPassword), _ => { password = newPassword = ""; });
            GUILayout.Label("<b>" + Loc.T("acc.changeEmail", "Change email") + "</b>  <size=11>" + Loc.T("acc.changeEmailHelp", "uses the current password above; a code goes to the new address") + "</size>", label);
            newEmail = Field(Loc.T("acc.newEmail", "New email"), newEmail, 254);
            GUILayout.BeginHorizontal();
            if (Btn(Loc.T("acc.sendCode", "Send me a code"))) Do(() => c.ChangeEmail(password, newEmail.Trim()));
            code = GUILayout.TextField(code, 6, field, GUILayout.Width(90));
            if (Btn(Loc.T("acc.verify", "Confirm"))) Do(() => c.ConfirmEmail(code.Trim()), _ => { code = ""; Refresh("account"); });
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (Btn(Loc.T("acc.export", "Export my data"))) Do(() => c.Call("GET", "me/export"), r => msg = ExportTo(r));
            if (Btn(Loc.T("acc.logout", "Sign out"))) Do(c.Logout, _ => { screen = "login"; msg = Loc.T("acc.loggedOut", "Signed out. Your local worlds and saves stay on this PC."); });
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
            GUILayout.Label("<b>" + Loc.T("acc.deleteTitle", "Delete account") + "</b>  <size=11>" + Loc.T("acc.deleteHelp", "Removes your email, profile, friends, statistics and cloud saves from the service. Local saves on this PC are not touched. Type DELETE and your current password above.") + "</size>", warn);
            GUILayout.BeginHorizontal();
            deleteText = GUILayout.TextField(deleteText, 6, field, GUILayout.Width(100));
            GUI.enabled = deleteText == "DELETE" && password.Length > 0 && !busy;
            if (GUILayout.Button(Loc.T("acc.deleteNow", "Delete my account"), btn)) Do(() => c.DeleteAccount(password), _ => { screen = "login"; password = deleteText = ""; msg = Loc.T("acc.deleted", "The account was deleted."); });
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        static string ExportTo(ApiResult r)
        {
            var path = Path.Combine(Application.persistentDataPath, "online", "my-data-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Json.Write(r.Data));
            return Loc.T("acc.exported", "Saved to {0}", path);
        }
    }
}
