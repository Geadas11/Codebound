using System;
using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Multiplayer from the title screen, on the existing host-authoritative game (Game/NetSession):
    /// host a world (existing or new) with a name, player limit, newcomers' permission, password, LAN / UPnP / online listing
    /// and port; or join by address[:port], from the history and favourites, or from the online service's listings
    /// (only when signed in — there is no LAN discovery or public matchmaking server of our own).
    /// In-game, the N panel manages the session: players, roles, kick / ban, invites, leave / close.
    /// </summary>
    public sealed class MultiplayerScreen : UiScreen
    {
        int tab;
        // host
        WorldInfo world;
        string sessionName, password = "";
        int access;           // 0 LAN, 1 internet via UPnP
        int listing;          // 0 not listed, 1 friends, 2 everyone, 3 invite only
        // join
        string address, joinPass = "", search = "", onlineStatus;
        List<object> online;
        bool fetching;
        Vector2 scrollA, scrollB;
        List<ServerHistory.Entry> hist;

        static readonly string[] Listings = { null, "friends", "public", "private" };

        public override string Title => Loc.T("ui.multiplayer", "Multiplayer");

        public override void OnShow()
        {
            sessionName ??= Loc.T("online.worldOf", "{0}'s world", FrontEnd.Player?.Name ?? Net.MyName);
            try { address ??= PlayerPrefs.GetString("cb.netAddress", ""); } catch { address = ""; }
            if (world == null) world = WorldStore.Latest(WorldStore.List(GameBootstrap.SavesDir));
        }

        public override void Update()
        {
            // a join started here: wait for the host's world (the scene reloads) or for the failure
            if (Nav.Busy != null && joining)
            {
                if (Net.Current == Net.Mode.Off) { joining = false; Nav.Busy = null; Nav.OnBusyCancel = null; Ui.Toast(Net.Status, "bad"); }
                else Nav.Busy = Net.Status;
            }
        }

        bool joining;

        public override void Draw(Rect r)
        {
            MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, 0.55f);
            var c = UiNav.Frame(this, r, Loc.T("mp.sub", "The host's game is the authority: guests see and act on the host's world."));
            GUILayout.BeginArea(new Rect(c.x, c.y, c.width, 46));
            GUILayout.BeginHorizontal();
            if (Ui.Button(Loc.T("mp.host", "Host a game"), tab == 0 ? UiTheme.TabOn : UiTheme.Tab, true, GUILayout.Height(42), GUILayout.Width(220))) tab = 0;
            if (Ui.Button(Loc.T("mp.join", "Join a game"), tab == 1 ? UiTheme.TabOn : UiTheme.Tab, true, GUILayout.Height(42), GUILayout.Width(220))) tab = 1;
            GUILayout.FlexibleSpace();
            GUILayout.Label(Loc.T("mp.playingAs", "Playing as {0}", Net.MyName), UiTheme.Muted_);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            var body = new Rect(c.x, c.y + 54, c.width, c.height - 54);
            if (tab == 0) Host(body); else Join(body);
        }

        // ------------------------------------------------------------------ host
        void Host(Rect r)
        {
            float half = (r.width - 20) / 2;
            GUILayout.BeginArea(new Rect(r.x, r.y, half, r.height), UiTheme.Panel);
            Ui.Section(Loc.T("mp.world", "World"));
            if (world != null)
            {
                GUILayout.BeginHorizontal();
                var tex = FrontEnd.Preview(world);
                var pr = GUILayoutUtility.GetRect(88, 88, GUILayout.Width(88));
                Ui.Fill(pr, UiTheme.Line);
                if (tex != null) GUI.DrawTexture(new Rect(pr.x + 1, pr.y + 1, 86, 86), tex);
                GUILayout.BeginVertical();
                GUILayout.Label(world.Name, UiTheme.H2);
                GUILayout.Label(Loc.T("ui.worldLine", "Seed {0} · {1} · {2}", world.Seed, FrontEnd.BiomeName(world.Biome), Ui.Duration(world.PlaySeconds)), UiTheme.Small);
                GUILayout.Label(Loc.T("ui.playedAgo", "Played {0}", Ui.Ago(world.LastPlayed)), UiTheme.Small);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else GUILayout.Label(Loc.T("ui.noWorlds", "No saved world yet"), UiTheme.Muted_);
            GUILayout.BeginHorizontal();
            if (Ui.Button(Loc.T("mp.chooseWorld", "Choose world…"))) Nav.Push(new WorldsScreen(w => world = w, Loc.T("mp.useWorld", "Use this world")));
            if (Ui.Button(Loc.T("ui.newWorld", "New world") + "…"))
            {
                var nw = new NewWorldScreen(true);
                nw.Created = (name, seed) => Start(() => GameBootstrap.CreateWorld(name, seed));
                Nav.Push(nw);
            }
            GUILayout.EndHorizontal();

            Ui.Section(Loc.T("mp.session", "Session"));
            sessionName = Ui.Field(Loc.T("mp.sessionName", "Session name (shown in online listings)"), sessionName, 48, "sname", false, GUILayout.Height(34));
            password = Ui.Field(Loc.T("net.worldPassword", "World password"), password, 64, "spass", true, GUILayout.Height(34));
            GUILayout.Label(Loc.T("mp.passHint", "Optional. Everyone who joins must type it; it also protects the encrypted connection against impostors."), UiTheme.Small);
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(r.x + half + 20, r.y, half, r.height), UiTheme.Panel);
            Ui.Section(Loc.T("mp.players", "Players and permissions"));
            int mg = Ui.Choice(Loc.T("opt.maxPlayers", "Players besides you"), Settings.MaxGuests - 1, Numbers(Multiplayer.MaxGuests));
            if (mg != Settings.MaxGuests - 1) Settings.MaxGuests = mg + 1;
            var roles = new[] { Multiplayer.Visitor, Multiplayer.Builder, Multiplayer.Admin };
            int ri = Array.IndexOf(roles, Settings.GuestRole);
            int nr = Ui.Choice(Loc.T("opt.guestRole", "Newcomers join as"), ri, Array.ConvertAll(roles, x => Loc.T("net.role." + x, x)));
            if (nr != ri) Settings.GuestRole = roles[nr];
            Ui.Section(Loc.T("mp.access", "Access"));
            access = Ui.Choice(Loc.T("mp.reach", "Reachable from"), access, new[] { Loc.T("mp.lan", "This network (LAN)"), Loc.T("mp.internet", "Internet (open the port with UPnP)") }, true,
                               access == 1 ? Loc.T("mp.upnpHint", "Asks the router to forward port {0}. If the router refuses, forward it by hand.", Settings.NetPort) : null);
            var listNames = new[] { Loc.T("mp.notListed", "Not listed (share your address)"), Loc.T("mp.listFriends", "Listed for friends"), Loc.T("mp.listPublic", "Listed for everyone"), Loc.T("mp.listInvite", "Invite only") };
            listing = Ui.Choice(Loc.T("mp.listing", "Online listing"), Online.SignedIn ? listing : 0, listNames, Online.SignedIn,
                                Online.SignedIn ? null : Loc.T("mp.listNeedsAccount", "Needs a signed-in online account (main menu → Online account)."));
            Ui.KeyValue(Loc.T("opt.port", "Port (TCP)"), Settings.NetPort + "  ·  " + Loc.T("mp.portWhere", "change in Options → Network"));
            GUILayout.FlexibleSpace();
            if (Ui.Button(Loc.T("mp.start", "Start server"), UiTheme.Primary, world != null && world.Loadable, GUILayout.Height(46)))
            {
                var w = world;
                Start(() => { if (!GameBootstrap.OpenWorld(w.Id, out var err)) throw new Exception(err); });
            }
            GUILayout.EndArea();
        }

        void Start(Action load)
        {
            Net.SessionName = sessionName;
            Net.HostPort = Settings.NetPort; Net.MaxGuests = Settings.MaxGuests; Net.GuestRole = Settings.GuestRole;
            Net.PendingHost = new Net.HostPlan { Password = password, Upnp = access == 1, Listing = Online.SignedIn ? Listings[listing] : null };
            Nav.Run(Loc.T("mp.starting", "Loading the world and opening the server…"), () =>
            {
                TitleMenu.Leaving();
                try { load(); }
                catch { Net.PendingHost = null; TitleMenu.Back(); throw; }
            });
        }

        static string[] Numbers(int n) { var a = new string[n]; for (int i = 0; i < n; i++) a[i] = (i + 1).ToString(); return a; }

        // ------------------------------------------------------------------ join
        void Join(Rect r)
        {
            float left = Mathf.Min(520, r.width * 0.42f);
            GUILayout.BeginArea(new Rect(r.x, r.y, left, r.height), UiTheme.Panel);
            Ui.Section(Loc.T("mp.direct", "Direct connection"));
            address = Ui.Field(Loc.T("mp.address", "Address (IP or name, optionally :port)"), address, 128, "jaddr", false, GUILayout.Height(36));
            joinPass = Ui.Field(Loc.T("net.worldPassword", "World password"), joinPass, 64, "jpass", true, GUILayout.Height(36));
            GUILayout.Space(6);
            var (host, port) = Multiplayer.SplitAddress(address);
            bool ok = host.Length > 0 && port > 0 && port < 65536;
            if (Ui.Button(Loc.T("net.join", "Join"), UiTheme.Primary, ok, GUILayout.Height(44)) || (ok && Ui.NavConfirm() && GUI.GetNameOfFocusedControl() == "jaddr")) Connect(address.Trim());
            if (Net.Current == Net.Mode.Off && Net.Status != "offline") GUILayout.Label(Net.Status, new GUIStyle(UiTheme.Small) { normal = { textColor = UiTheme.Bad } });
            GUILayout.Label(Loc.T("mp.joinHint", "Same network: the host's LAN address (shown in its N panel). Internet: its public address, with the port open on its router."), UiTheme.Small);

            hist ??= ServerHistory.Load();
            scrollA = GUILayout.BeginScrollView(scrollA);
            foreach (var fav in new[] { true, false })
            {
                Ui.Section(fav ? Loc.T("mp.favourites", "Favourites") : Loc.T("mp.history", "Recent servers"));
                int shown = 0;
                foreach (var e in hist)
                {
                    if (e.Favourite != fav) continue;
                    shown++;
                    GUILayout.BeginHorizontal();
                    if (Ui.Button(e.Favourite ? "★" : "☆", UiTheme.Ghost, true, GUILayout.Width(40))) { ServerHistory.Toggle(e.Address); hist = null; }
                    if (Ui.Button(e.Address + "   <color=" + UiTheme.Html(UiTheme.Dim) + ">" + Ui.Ago(e.Last) + "</color>", UiTheme.Row)) address = e.Address;
                    if (Ui.Button("✕", UiTheme.Ghost, true, GUILayout.Width(36))) { ServerHistory.Remove(e.Address); hist = null; }
                    GUILayout.EndHorizontal();
                    if (hist == null) break;
                }
                if (hist == null) break;
                if (shown == 0) GUILayout.Label(fav ? Loc.T("mp.noFav", "Star a recent server to keep it here.") : Loc.T("mp.noHistory", "Servers you join by address appear here."), UiTheme.Small);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(r.x + left + 20, r.y, r.width - left - 20, r.height), UiTheme.Panel);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.T("mp.onlineGames", "Online games"), UiTheme.H1);
            GUILayout.FlexibleSpace();
            if (Online.SignedIn && Ui.Button(fetching ? Loc.T("acc.working", "Working…") : Loc.T("mp.refresh", "Refresh"), null, !fetching, GUILayout.Width(150), GUILayout.Height(36))) Fetch();
            GUILayout.EndHorizontal();
            Ui.Rule(UiTheme.Accent, 2);
            search = Ui.Field(Loc.T("mp.search", "Search by name"), search, 40, "jsearch", false, GUILayout.Height(34));
            if (!Online.SignedIn)
            {
                GUILayout.Space(10);
                GUILayout.Label(Loc.T("mp.signInList", "Games listed on the Codebound online service appear here when you are signed in to an online account. Without one, join by address."), UiTheme.Wrap);
                if (Ui.Button(Loc.T("ui.onlineAccount", "Online account"), null, true, GUILayout.Width(220), GUILayout.Height(38))) AccountMenu.Open();
            }
            else
            {
                if (online == null && !fetching && onlineStatus == null) Fetch();
                if (onlineStatus != null) GUILayout.Label(onlineStatus, UiTheme.Muted_);
                scrollB = GUILayout.BeginScrollView(scrollB);
                if (online != null && online.Count == 0) GUILayout.Label(Loc.T("mp.noGames", "No game is listed for you right now."), UiTheme.Muted_);
                if (online != null)
                    foreach (var o in online)
                    {
                        if (!(o is Dictionary<string, object> s)) continue;
                        string name = s.TryGetValue("name", out var n) ? n as string : "?";
                        string hostName = s.TryGetValue("host", out var h) && h is Dictionary<string, object> hd && hd.TryGetValue("displayName", out var dn) ? dn as string : "?";
                        if (search.Length > 0 && name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0 && hostName.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                        int players = s.TryGetValue("players", out var p) && p is double pd ? (int)pd : 0, max = s.TryGetValue("maxPlayers", out var m) && m is double md ? (int)md : 0;
                        bool locked = s.TryGetValue("hasPassword", out var hp) && hp is bool hb && hb;
                        bool compatible = !(s.TryGetValue("version", out var v) && v is double vd) || (int)vd == Multiplayer.ProtocolVersion;
                        var row = GUILayoutUtility.GetRect(10, 64, GUILayout.ExpandWidth(true));
                        GUI.Box(row, GUIContent.none, UiTheme.Row);
                        GUI.Label(new Rect(row.x + 14, row.y + 8, row.width - 260, 26), name + (locked ? "  · " + Loc.T("net.withPassword", "password required") : ""), new GUIStyle(UiTheme.H2) { normal = { textColor = UiTheme.Text } });
                        GUI.Label(new Rect(row.x + 14, row.y + 36, row.width - 260, 20), Loc.T("mp.hostedBy", "Host: {0} · {1}/{2} players", hostName, players, max) + (compatible ? "" : " · " + Loc.T("mp.otherVersion", "other game version")), UiTheme.Small);
                        string id = s.TryGetValue("id", out var sid) ? sid as string : null;
                        if (Ui.Button(new Rect(row.xMax - 130, row.y + 14, 116, 36), Loc.T("net.join", "Join"), UiTheme.Primary, id != null && compatible && players < max + 1)) JoinOnline(id, locked);
                    }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        async void Fetch()
        {
            fetching = true; onlineStatus = null;
            var res = await Online.Client.Sessions(search);
            fetching = false;
            if (res.Ok) online = res.List ?? new List<object>();
            else onlineStatus = res.Offline ? Loc.T("online.unreachable", "The online service cannot be reached. You can keep playing offline.") : res.Message;
        }

        void Connect(string addr)
        {
            Online.ForgetJoin();
            Net.JoinTicket = null; Net.OnlineSession = null;
            try { PlayerPrefs.SetString("cb.netAddress", addr); PlayerPrefs.Save(); } catch { }
            ServerHistory.Add(addr, Online.Now()); hist = null;
            BeginJoin(Net.Join(addr, Net.MyName, joinPass));
        }

        async void JoinOnline(string id, bool locked)
        {
            if (locked && joinPass.Length == 0) { Ui.Toast(Loc.T("mp.needPass", "This game has a password: type it in the password field first."), "bad"); return; }
            Nav.Busy = Loc.T("mp.ticket", "Asking the service for a join ticket…");
            var msg = await Online.Join(id, joinPass);
            Nav.Busy = null;
            BeginJoin(msg);
        }

        void BeginJoin(string status)
        {
            if (Net.Current != Net.Mode.Guest) { Ui.Toast(status, "bad"); return; }
            joining = true;
            Nav.Busy = status;
            Nav.OnBusyCancel = () => { joining = false; Online.ForgetJoin(); Net.Disconnect(); Ui.Toast(Loc.T("mp.cancelled", "Connection cancelled")); };
        }
    }

    /// <summary>Servers joined by address: the last 12, with favourites kept. PlayerPrefs (per machine).</summary>
    public static class ServerHistory
    {
        public sealed class Entry { public string Address; public long Last; public bool Favourite; }
        const string Key = "cb.serverHistory";

        public static List<Entry> Load()
        {
            var list = new List<Entry>();
            try
            {
                if (Json.Parse(PlayerPrefs.GetString(Key, "[]")) is List<object> arr)
                    foreach (var o in arr)
                        if (o is Dictionary<string, object> d && d.TryGetValue("a", out var a) && a is string s)
                            list.Add(new Entry { Address = s, Last = d.TryGetValue("t", out var t) && t is double td ? (long)td : 0, Favourite = d.TryGetValue("f", out var f) && f is bool fb && fb });
            }
            catch (FormatException) { }
            list.Sort((x, y) => y.Last.CompareTo(x.Last));
            return list;
        }

        static void Save(List<Entry> list)
        {
            var arr = new List<object>();
            int recent = 0;
            foreach (var e in list)
            {
                if (!e.Favourite && ++recent > 12) continue;
                arr.Add(new Dictionary<string, object> { { "a", e.Address }, { "t", (double)e.Last }, { "f", e.Favourite } });
            }
            PlayerPrefs.SetString(Key, Json.Write(arr)); PlayerPrefs.Save();
        }

        public static void Add(string address, long now)
        {
            var list = Load();
            var e = list.Find(x => string.Equals(x.Address, address, StringComparison.OrdinalIgnoreCase));
            if (e == null) list.Insert(0, e = new Entry { Address = address });
            e.Last = now;
            list.Sort((x, y) => y.Last.CompareTo(x.Last));
            Save(list);
        }

        public static void Toggle(string address) { var list = Load(); var e = list.Find(x => x.Address == address); if (e != null) { e.Favourite = !e.Favourite; Save(list); } }
        public static void Remove(string address) { var list = Load(); list.RemoveAll(x => x.Address == address); Save(list); }
    }
}
