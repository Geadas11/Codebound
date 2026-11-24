using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Codebound.Core;
using Codebound.Core.Online;
using UnityEngine;

namespace Codebound.Game
{
    sealed class FileText : ITextStore
    {
        readonly string path;
        public FileText(string path) { this.path = path; }
        public string Load() { try { return File.Exists(path) ? File.ReadAllText(path) : ""; } catch (IOException) { return ""; } }
        public void Save(string text)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path + ".part", text);
                if (File.Exists(path)) File.Delete(path);
                File.Move(path + ".part", path);
            }
            catch (IOException e) { Debug.LogWarning("Codebound online: could not write " + Path.GetFileName(path) + ": " + e.Message); }
        }
    }

    /// <summary>
    /// The game's link to Codebound Services (accounts, friends, online games, stats, cloud saves). Everything here is
    /// optional: with no service configured, no internet, or no account, the game plays exactly as before.
    /// </summary>
    public static class Online
    {
        public static OnlineClient Client { get; private set; }
        public static StatsJournal Journal { get; private set; }
        public static CloudSaves Saves { get; private set; }
        public static string ServiceUrl { get; private set; } = "";
        public static bool Configured => Client != null;
        public static int FriendRequests;
        public static List<object> Invites = new List<object>();
        public static string HostedSession, HostedVisibility;
        public static string Notice;                 // last thing worth telling the player (shown by the account panel / title)
        static string Dir => Path.Combine(Application.persistentDataPath, "online");
        public static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Client = null; Journal = null; Saves = null; ServiceUrl = ""; HostedSession = null; Invites = new List<object>(); FriendRequests = 0; Notice = null; }

        /// <summary>
        /// Where the service is: -online &lt;url&gt; on the command line, then the URL saved in the account panel, then (only in
        /// the Editor and development builds) a service on this PC. Plain http is only accepted for this PC.
        /// </summary>
        public static void Init()
        {
            if (Journal != null) return;
            Journal = new StatsJournal(new FileText(Path.Combine(Dir, "stats.json")), Now);
            string url = null;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-online") url = args[i + 1];
            if (url == null) try { url = PlayerPrefs.GetString("cb.onlineUrl", ""); } catch { url = ""; }
            if (string.IsNullOrEmpty(url) && (Application.isEditor || Debug.isDebugBuild)) url = "http://127.0.0.1:5080";
            SetUrl(url, save: false);
        }

        /// <summary>null = accepted; otherwise why not.</summary>
        public static string CheckUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u)) return Loc.T("online.badUrl", "That is not a web address.");
            if (u.Scheme == "https") return null;
            if (u.Scheme == "http" && (u.Host == "localhost" || u.Host == "127.0.0.1" || u.Host == "[::1]")) return null;
            return Loc.T("online.needHttps", "The service must use https (plain http only for a service on this PC).");
        }

        public static string SetUrl(string url, bool save = true)
        {
            url = (url ?? "").Trim();
            if (CheckUrl(url) is string why) return why;
            if (save) try { PlayerPrefs.SetString("cb.onlineUrl", url); PlayerPrefs.Save(); } catch { }
            if (url == ServiceUrl && Client != null) return null;
            ServiceUrl = url;
            if (url.Length == 0) { Client = null; Saves = null; return null; }
            var store = new FileText(Path.Combine(Dir, "signin-" + Hash(url) + ".json"));   // one sign-in per service
            Client = new OnlineClient(new HttpClientTransport(url), store, Now);
            Saves = new CloudSaves(Client, Path.Combine(Application.persistentDataPath, "saves"), SystemInfo.deviceName, Now);
            return null;
        }

        static string Hash(string s) => CloudSaves.Sha(System.Text.Encoding.UTF8.GetBytes(s)).Substring(0, 12).ToLowerInvariant();

        public static bool SignedIn => Client != null && Client.SignedIn;
        public static bool OnlineNow => Client != null && Client.State == Connection.Online;
        /// <summary>The account this PC's play counts for: signed in and seen by the service in the last 30 days.</summary>
        public static string Account => SignedIn && OfflinePolicy.CachedSignInValid(Client.LastOnline, Now()) ? Client.PublicId : null;

        public static void Record(string key, long amount) { if (amount > 0) Journal?.Add(key, amount, Account); }

        /// <summary>One line for menus: who, and whether the service can be reached.</summary>
        public static string StatusLine()
        {
            if (Client == null) return Loc.T("online.notConfigured", "Offline mode · no online service set");
            if (!Client.SignedIn) return Loc.T("online.signedOut", "Local profile · not signed in");
            string who = Client.DisplayName ?? "?";
            return Client.State == Connection.Online ? Loc.T("online.online", "{0} · online", who)
                 : Loc.T("online.offline", "{0} · offline (playing locally)", who);
        }

        public static async void SyncStats()
        {
            if (!SignedIn) return;
            Journal.Seal();
            try { await Journal.Sync(Client); } catch (Exception e) { Debug.LogWarning("Codebound online: stats sync: " + e.Message); }
        }

        public static async Task Poll()
        {
            if (!SignedIn) return;
            await Client.Presence(Net.Current == Net.Mode.Off ? "online" : "busy", Net.IsHost ? HostedSession : Net.IsGuest ? Net.OnlineSession : null);   // friends see "in a game"
            var n = await Client.Notifications();
            if (n.Ok)
            {
                FriendRequests = n.Obj.TryGetValue("friendRequests", out var fr) && fr is double d ? (int)d : 0;
                Invites = n.Obj.TryGetValue("invites", out var iv) && iv is List<object> l ? l : new List<object>();
            }
        }

        // ------------------------------------------------------------------ online games
        /// <summary>Lists this hosted world on the service so friends (or everyone) can find it. The host keeps authority.</summary>
        public static async Task<string> Publish(string visibility)
        {
            if (!Net.IsHost) return Loc.T("online.hostFirst", "Host the world first.");
            var g = GameBootstrap.Session?.State;
            string name = Loc.T("online.worldOf", "{0}'s world", Client.DisplayName);
            var r = await Client.HostSession(name, visibility, Multiplayer.MaxGuests + 1, Multiplayer.Port, Multiplayer.ProtocolVersion, Net.WorldPassword.Length > 0, Net.FirstLanAddress());
            if (!r.Ok) return r.Message;
            HostedSession = r.Str("sessionId"); HostedVisibility = visibility;
            return Loc.T("online.published", "Listed online ({0}). Only players with an account can join it now.", Loc.T("online.vis." + visibility, visibility));
        }

        public static async void Unpublish()
        {
            var id = HostedSession; HostedSession = null;
            if (id != null && SignedIn) await Client.CloseSession(id);
        }

        static string lastJoinSession, lastJoinPassword;
        static int rejoinTries;
        static float rejoinAt = -1;

        /// <summary>Asks the service for a ticket and connects to the host with it (the host checks the ticket with the service).</summary>
        public static async Task<string> Join(string sessionId, string worldPassword, long inviteId = 0)
        {
            var r = inviteId != 0 ? await Client.AcceptInvite(inviteId) : await Client.JoinSession(sessionId);
            if (!r.Ok) return r.Message;
            lastJoinSession = r.Str("sessionId"); lastJoinPassword = worldPassword; rejoinTries = 0;
            return Connect(r);
        }

        static string Connect(ApiResult r)
        {
            Net.JoinTicket = r.Str("ticket"); Net.OnlineSession = r.Str("sessionId");
            string addr = r.Str("address");
            return Net.Join(addr, Client.DisplayName, lastJoinPassword ?? "");
        }

        /// <summary>Called when a guest's connection drops (not a kick): up to 3 new tickets, 5 / 15 / 45 s apart.</summary>
        public static void ConnectionLost(string why)
        {
            if (lastJoinSession == null || why == "offline" || why.StartsWith("kicked") || rejoinTries >= 3) { lastJoinSession = null; return; }
            rejoinAt = Time.realtimeSinceStartup + 5 * (float)Math.Pow(3, rejoinTries++);
            Notice = Loc.T("online.reconnecting", "Connection lost. Reconnecting ({0}/3)…", rejoinTries);
        }

        public static void ForgetJoin() { lastJoinSession = null; rejoinAt = -1; }

        internal static async void TickRejoin()
        {
            if (rejoinAt < 0 || Time.realtimeSinceStartup < rejoinAt || Net.Current != Net.Mode.Off) return;
            rejoinAt = -1;
            var r = await Client.JoinSession(lastJoinSession);
            if (r.Ok) Notice = Connect(r);
            else if (r.Offline) ConnectionLost("offline");
            else { Notice = r.Message; lastJoinSession = null; }
        }
    }

    /// <summary>Keeps the online side ticking across scene reloads: play statistics, presence, sync, heartbeats, reconnection.</summary>
    public sealed class OnlineDriver : MonoBehaviour
    {
        static OnlineDriver instance;
        GameSession hooked;
        double playAcc, distAcc;
        Vector3 lastPos;
        float pollAt = 2, syncAt = 30, heartbeatAt, retryAt, retryDelay = 15;
        bool hostWas;
        Net.Mode lastMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create()
        {
            if (instance != null || Application.isBatchMode) return;
            Online.Init();
            instance = new GameObject("Codebound Online").AddComponent<OnlineDriver>();
            DontDestroyOnLoad(instance.gameObject);
            ProgramManager.ProgramStarted += (lang, lines) => { Online.Record("programs_run", 1); Online.Record("lang:" + lang, 1); Online.Record("code_lines", lines); };
        }

        void Hook(GameSession s)
        {
            hooked = s;
            if (s == null) return;
            lastPos = s.Player != null ? s.Player.position : Vector3.zero;
            if (Net.IsGuest) return;   // a guest's world is the host's: its buildings count for the host
            s.Grid.Placed += _ => Online.Record("buildings_built", 1);
            s.Factory.ActionCompleted += (_, item) => Online.Record("items_produced", 1);
            s.Factory.Research.Completed += _ => Online.Record("research_unlocked", 1);
            s.Factory.FlightEnded += (_, how) => { if (how == "orbit") Online.Record("space_launches", 1); };
            s.World.DepositChanged += _ => Online.Record("resources_mined", 1);
        }

        void Update()
        {
            var s = GameBootstrap.Session;
            if (s != hooked) Hook(s);
            float dt = Time.unscaledDeltaTime, now = Time.realtimeSinceStartup;
            if (s != null && Time.timeScale > 0 && !TitleMenu.IsOpen && Application.isFocused)
            {
                if ((playAcc += dt) >= 1) { Online.Record("playtime_seconds", (long)playAcc); playAcc -= (long)playAcc; }
                if (s.Player != null)
                {
                    var p = s.Player.position;
                    float step = (p - lastPos).magnitude; lastPos = p;
                    if (step < 2) distAcc += step;   // larger jumps are teleports / world changes
                    if (distAcc >= 1) { Online.Record("distance", (long)distAcc); distAcc -= (long)distAcc; }
                }
            }
            var c = Online.Client;
            if (c == null) return;

            if (lastMode == Net.Mode.Guest && Net.Current == Net.Mode.Off) Online.ConnectionLost(Net.Status ?? "");
            lastMode = Net.Current;
            Online.TickRejoin();

            if (hostWas && !Net.IsHost) Online.Unpublish();   // stopped hosting: close the listing
            hostWas = Net.IsHost;
            if (Online.HostedSession != null && now >= heartbeatAt)
            {
                heartbeatAt = now + 30;
                Heartbeat();
            }
            if (!c.SignedIn) return;
            if (c.State == Connection.Offline)
            {
                if (now < retryAt) return;
                retryAt = now + retryDelay; retryDelay = Mathf.Min(retryDelay * 2, 300);   // backoff while the service is unreachable
                Reconnect();
                return;
            }
            retryDelay = 15;
            if (now >= pollAt) { pollAt = now + 60; _ = Online.Poll(); }
            if (now >= syncAt) { syncAt = now + StatsJournal.BatchSeconds; Online.SyncStats(); }
        }

        async void Heartbeat()
        {
            var id = Online.HostedSession;
            int players = 0; foreach (var p in Net.Peers) if (p.Name != null) players++;
            var r = await Online.Client.Heartbeat(id, players);
            if (!r.Ok && !r.Offline && Online.HostedSession == id) { Online.HostedSession = null; Online.Notice = r.Message; }
        }

        async void Reconnect()
        {
            if (!await Online.Client.ServiceReachable()) return;
            var me = await Online.Client.Me();   // re-authenticates with the refresh token
            if (me.Ok) { Online.Notice = Loc.T("online.backOnline", "Back online. Syncing…"); Online.SyncStats(); _ = Online.Poll(); }
        }

        void OnApplicationQuit()
        {
            Online.Journal?.Seal();   // nothing to the network here: the queue goes out next time
            if (Online.HostedSession != null) _ = Online.Client.CloseSession(Online.HostedSession);   // best effort; the listing expires on its own otherwise
        }
    }
}
