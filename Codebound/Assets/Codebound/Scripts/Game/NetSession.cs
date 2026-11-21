using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>One TCP connection speaking newline-terminated JSON. A reader thread fills the inbox; the main thread sends.</summary>
    sealed class Peer
    {
        readonly TcpClient client;
        readonly NetworkStream stream;
        readonly int maxLine;
        public readonly ConcurrentQueue<string> Inbox = new ConcurrentQueue<string>();
        public volatile bool Closed;
        public string Name, Role = Multiplayer.Builder;
        public double X, Y;
        public float LastMine;

        public Peer(TcpClient c, int maxLine)
        {
            client = c; this.maxLine = maxLine;
            c.NoDelay = true; c.SendTimeout = 3000;
            stream = c.GetStream();
            new Thread(ReadLoop) { IsBackground = true, Name = "codebound-net" }.Start();
        }

        void ReadLoop()
        {
            var buf = new byte[64 * 1024];
            var line = new MemoryStream();
            try
            {
                int n;
                while (!Closed && (n = stream.Read(buf, 0, buf.Length)) > 0)
                    for (int i = 0; i < n; i++)
                    {
                        if (buf[i] != (byte)'\n') { line.WriteByte(buf[i]); if (line.Length > maxLine) throw new IOException("line too long"); continue; }
                        Inbox.Enqueue(Encoding.UTF8.GetString(line.GetBuffer(), 0, (int)line.Length));
                        line.SetLength(0);
                    }
            }
            catch (Exception) { }
            Closed = true;
        }

        public void Send(Dictionary<string, object> msg)
        {
            if (Closed) return;
            try { var b = Encoding.UTF8.GetBytes(Json.Write(msg) + "\n"); stream.Write(b, 0, b.Length); }
            catch (Exception) { Close(); }
        }

        public void Close() { Closed = true; try { client.Close(); } catch (Exception) { } }
    }

    /// <summary>
    /// LAN co-op (Etapa 6). Static so the connection survives the scene reload a guest does when it receives the world.
    /// The host keeps the authoritative GameState; rules for guest actions are in Core/Multiplayer.cs.
    /// </summary>
    public static class Net
    {
        public enum Mode { Off, Host, Guest }
        public static Mode Current { get; private set; }
        public static bool IsGuest => Current == Mode.Guest;
        public static bool IsHost => Current == Mode.Host;
        public static string Status { get; private set; } = "offline";
        public static string MyName = "player", MyRole = Multiplayer.Builder;
        public static readonly List<(string name, double x, double y)> Others = new List<(string, double, double)>();

        static TcpListener listener;
        static readonly List<Peer> peers = new List<Peer>();
        static Peer server;
        static bool worldReceived;
        static float snapTimer, posTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Disconnect();

        internal static IReadOnlyList<Peer> Peers => peers;

        public static string StartHost()
        {
            Disconnect();
            try { listener = new TcpListener(IPAddress.Any, Multiplayer.Port); listener.Start(); }
            catch (Exception e) { listener = null; return Status = "could not host: " + e.Message; }
            Current = Mode.Host;
            return Status = $"hosting on port {Multiplayer.Port}";
        }

        public static string Join(string address, string name)
        {
            Disconnect();
            if (!Multiplayer.ValidName(name)) return Status = "name: 1-16 letters, digits, _ or -";
            try
            {
                var c = new TcpClient();
                if (!c.ConnectAsync(address, Multiplayer.Port).Wait(4000)) { c.Close(); return Status = "no answer from " + address; }
                server = new Peer(c, Multiplayer.MaxHostLine);
            }
            catch (Exception e) { return Status = "could not connect: " + (e.InnerException ?? e).Message; }
            MyName = name; Current = Mode.Guest; worldReceived = false;
            server.Send(new Dictionary<string, object> { { "t", "hello" }, { "name", name }, { "v", (double)Multiplayer.ProtocolVersion } });
            return Status = "joining " + address + "…";
        }

        public static void Disconnect()
        {
            try { listener?.Stop(); } catch (Exception) { }
            listener = null;
            foreach (var p in peers) p.Close();
            peers.Clear();
            server?.Close(); server = null;
            Others.Clear();
            Current = Mode.Off; Status = "offline"; MyRole = Multiplayer.Builder;
        }

        public static void SendAct(Dictionary<string, object> act)
        {
            act["t"] = "act";
            server?.Send(act);
        }

        public static void Program(Cell at, string lang, string code, string op, string password)
        {
            var m = new Dictionary<string, object> { { "t", "program" }, { "x", (double)at.X }, { "y", (double)at.Y }, { "lang", lang }, { "code", code }, { "op", op } };
            if (password != null) m["password"] = password;
            server?.Send(m);
        }

        public static void Kick(string name) { foreach (var p in peers) if (p.Name == name) { p.Send(Msg("kick", "removed by the host")); p.Close(); } }
        public static void SetRole(string name, string role) { foreach (var p in peers) if (p.Name == name) { p.Role = role; p.Send(Msg("msg", "the host made you a " + role)); } }

        static Dictionary<string, object> Msg(string t, string text) => new Dictionary<string, object> { { "t", t }, { "text", text } };
        static double D(Dictionary<string, object> m, string k) => m.TryGetValue(k, out var v) && v is double d && !double.IsNaN(d) && !double.IsInfinity(d) ? d : 0;

        /// <summary>Called every frame by NetSession.</summary>
        internal static void Pump(GameSession s, ProgramManager programs)
        {
            if (IsHost) PumpHost(s, programs);
            else if (IsGuest) PumpGuest(s);
        }

        // ------------------------------------------------------------------ host
        static void PumpHost(GameSession s, ProgramManager programs)
        {
            try { while (listener != null && listener.Pending()) peers.Add(new Peer(listener.AcceptTcpClient(), Multiplayer.MaxGuestLine)); }
            catch (Exception e) { Debug.LogWarning("Codebound net accept: " + e.Message); }

            foreach (var p in peers)
            {
                int budget = 64;   // messages per frame per guest
                while (budget-- > 0 && p.Inbox.TryDequeue(out var line))
                {
                    Dictionary<string, object> m;
                    try { m = Json.Parse(line) as Dictionary<string, object>; } catch (FormatException) { m = null; }
                    if (m == null || !(m.TryGetValue("t", out var tv) && tv is string t)) continue;
                    if (p.Name == null)
                    {
                        string name = m.TryGetValue("name", out var nv) ? nv as string : null;
                        string why = t != "hello" ? "say hello first" : D(m, "v") != Multiplayer.ProtocolVersion ? "different game version"
                                   : !Multiplayer.ValidName(name) || name == MyName ? "invalid name" : peers.Exists(o => o.Name == name) ? "name already in use" : null;
                        if (why != null) { p.Send(Msg("kick", why)); p.Close(); break; }
                        p.Name = name; p.X = s.Player.position.x; p.Y = s.Player.position.y;
                        s.State.GuestInventory(name);
                        s.State.Hud.Notify(Loc.T("notify.joined", "{0} joined the game", name));
                        snapTimer = 0;
                        continue;
                    }
                    switch (t)
                    {
                        case "pos": p.X = D(m, "x"); p.Y = D(m, "y"); break;
                        case "act":
                            string err;
                            if (m.TryGetValue("a", out var av) && av as string == "mine" && Time.time - p.LastMine < 0.25f) err = "slow down";
                            else
                            {
                                if (av as string == "mine") p.LastMine = Time.time;
                                err = Multiplayer.Act(s.State, p.Role, s.State.GuestInventory(p.Name), p.X, p.Y, m);
                            }
                            if (err != null) p.Send(Msg("msg", err));
                            break;
                        case "program": p.Send(Msg("msg", Program(s, programs, p, m) ?? "program updated")); break;
                    }
                }
            }
            foreach (var p in peers.FindAll(x => x.Closed))
                if (p.Name != null) s.State.Hud.Notify(Loc.T("notify.left", "{0} left the game", p.Name));
            peers.RemoveAll(x => x.Closed);

            if ((snapTimer -= Time.deltaTime) > 0 || peers.Count == 0) return;
            snapTimer = 0.5f;
            string snap = Multiplayer.Snapshot(s.State);
            var creatures = Multiplayer.CreaturesOf(s.State);
            var players = new List<object> { new List<object> { MyName, (double)s.Player.position.x, (double)s.Player.position.y } };
            foreach (var p in peers) if (p.Name != null) players.Add(new List<object> { p.Name, p.X, p.Y });
            foreach (var p in peers)
                if (p.Name != null)
                    p.Send(new Dictionary<string, object> { { "t", "snap" }, { "save", snap }, { "role", p.Role }, { "creatures", creatures }, { "players", players },
                                                            { "inv", Multiplayer.InventoryOf(s.State.GuestInventory(p.Name)) } });
            Others.Clear();
            foreach (var p in peers) if (p.Name != null) Others.Add((p.Name, p.X, p.Y));
        }

        /// <summary>A guest edits/runs/stops a program on a host computer (shared projects). Runs in the host's sandbox.</summary>
        static string Program(GameSession s, ProgramManager programs, Peer p, Dictionary<string, object> m)
        {
            if (p.Role != Multiplayer.Builder) return "you are a visitor";
            var b = s.Grid.At(new Cell((int)D(m, "x"), (int)D(m, "y")));
            if (!(b?.State is ComputerState cs)) return "no computer there";
            if (Math.Sqrt(Math.Pow(b.CenterX - p.X, 2) + Math.Pow(b.CenterY - p.Y, 2)) > Multiplayer.ActionReach) return "too far";
            if (!cs.CheckPassword(m.TryGetValue("password", out var pw) ? pw as string : null)) return "wrong password for this computer";
            string code = m.TryGetValue("code", out var cv) ? cv as string : null, lang = m.TryGetValue("lang", out var lv) ? lv as string : null;
            if (code != null)
            {
                if (code.Length > Multiplayer.MaxProgramChars) return "program too long";
                if (Array.IndexOf(ComputerState.Languages, lang) < 0) return "unknown language";
                cs.Code = code; cs.Language = lang;
            }
            string op = m.TryGetValue("op", out var ov) ? ov as string : "";
            if (op == "run") programs.StartProgram(b);
            else if (op == "stop") programs.Stop(b);
            return null;
        }

        // ------------------------------------------------------------------ guest
        static void PumpGuest(GameSession s)
        {
            if (server == null || server.Closed)
            {
                string why = Status.StartsWith("kicked") ? Status : "connection to the host lost";
                Disconnect(); Status = why;
                s.State.Hud.Notify(Loc.T("notify.disconnected", "Multiplayer: {0}", why));
                return;
            }
            if (worldReceived && (posTimer -= Time.deltaTime) <= 0)
            {
                posTimer = 0.1f;
                server.Send(new Dictionary<string, object> { { "t", "pos" }, { "x", Math.Round(s.Player.position.x, 3) }, { "y", Math.Round(s.Player.position.y, 3) } });
            }
            string latest = null; Dictionary<string, object> latestMsg = null;
            while (server.Inbox.TryDequeue(out var line))
            {
                Dictionary<string, object> m;
                try { m = Json.Parse(line) as Dictionary<string, object>; } catch (FormatException) { continue; }
                if (m == null || !(m.TryGetValue("t", out var tv) && tv is string t)) continue;
                if (t == "snap" && m.TryGetValue("save", out var sv) && sv is string save) { latest = save; latestMsg = m; }
                else if (t == "msg" && m.TryGetValue("text", out var x) && x is string text) s.State.Hud.Notify("Host: " + text);
                else if (t == "kick") { Status = "kicked: " + (m.TryGetValue("text", out var k) ? k : ""); server.Close(); return; }
            }
            if (latest == null) return;   // only the newest snapshot matters
            GameState snap;
            try { snap = SaveSystem.Read(new StringReader(latest)); }
            catch (InvalidDataException e) { Debug.LogWarning("Codebound net: bad snapshot: " + e.Message); return; }
            MyRole = latestMsg.TryGetValue("role", out var rv) && rv is string role ? role : Multiplayer.Visitor;
            if (!worldReceived)
            {
                worldReceived = true;
                Status = "connected as " + MyName;
                snap.Vitals.Respawn();
                Multiplayer.LoadInventory(snap.Inventory, latestMsg.TryGetValue("inv", out var iv0) ? iv0 as List<object> : null);
                GameBootstrap.LoadState(snap);   // rebuilds the scene around the host's world
                return;
            }
            Multiplayer.Apply(s.State, snap, MachinePanel.Editing);
            Multiplayer.LoadInventory(s.Inventory, latestMsg.TryGetValue("inv", out var iv) ? iv as List<object> : null);
            s.State.Creatures.ReplaceAll(Multiplayer.ParseCreatures(latestMsg.TryGetValue("creatures", out var cv) ? cv as List<object> : null));
            Others.Clear();
            if (latestMsg.TryGetValue("players", out var pv) && pv is List<object> pl)
                foreach (var o in pl)
                    if (o is List<object> e && e.Count == 3 && e[0] is string n && n != MyName && e[1] is double x && e[2] is double y) Others.Add((n, x, y));
        }
    }

    /// <summary>Pumps the network each frame, draws the other players, and the N panel (host / join / players).</summary>
    public sealed class NetSession : MonoBehaviour
    {
        GameSession session;
        ProgramManager programs;
        Camera cam;
        readonly Dictionary<string, SpriteRenderer> ghosts = new Dictionary<string, SpriteRenderer>();
        bool open;
        string address = "127.0.0.1";
        static bool commandLineDone;
        GUIStyle box, label, small;

        public void Init(GameSession s, ProgramManager pm, Camera c)
        {
            session = s; programs = pm; cam = c;
            try
            {
                if (Net.Current == Net.Mode.Off) Net.MyName = PlayerPrefs.GetString("cb.netName", "player");   // a guest keeps its name across the world reload
                address = PlayerPrefs.GetString("cb.netAddress", "127.0.0.1");
            }
            catch { }
            if (commandLineDone) return;
            commandLineDone = true;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-name" && i + 1 < args.Length) Net.MyName = args[i + 1];
                if (args[i] == "-host") Debug.Log("[Codebound] " + Net.StartHost());
            }
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-join") Debug.Log("[Codebound] " + Net.Join(args[i + 1], Net.MyName));
        }

        void Update()
        {
            Net.Pump(session, programs);
            var seen = new HashSet<string>();
            foreach (var (name, x, y) in Net.Others)
            {
                seen.Add(name);
                if (!ghosts.TryGetValue(name, out var sr))
                {
                    sr = new GameObject("Player " + name).AddComponent<SpriteRenderer>();
                    sr.sprite = PixelArtFactory.Player(); sr.sortingOrder = 5;
                    sr.color = Color.HSVToRGB((name.GetHashCode() & 0xff) / 255f, 0.45f, 1f);
                    ghosts[name] = sr;
                }
                sr.transform.position = Vector3.Lerp(sr.transform.position, new Vector3((float)x, (float)y, 0), 1f - Mathf.Exp(-12f * Time.deltaTime));
            }
            foreach (var n in new List<string>(ghosts.Keys)) if (!seen.Contains(n)) { Destroy(ghosts[n].gameObject); ghosts.Remove(n); }

            var kb = Keyboard.current;
            if (kb == null) return;
            if (open && kb.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (!open && !session.KeyboardCaptured && !session.PanelOpen && !OptionsMenu.IsOpen && Controls.Pressed("network"))
            { open = true; session.PanelOpen = true; session.KeyboardCaptured = true; }
        }

        void Close() { open = false; session.PanelOpen = false; session.KeyboardCaptured = false; }

        void OnGUI()
        {
            UiGui.Begin();
            if (box == null)
            {
                var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.05f, 0.06f, 0.09f, 0.96f)); bg.Apply();
                box = new GUIStyle(GUI.skin.box) { normal = { background = bg }, padding = new RectOffset(14, 14, 10, 10) };
                label = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, normal = { textColor = new Color(0.86f, 0.95f, 0.9f) } };
                small = new GUIStyle(label) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
            }
            foreach (var kv in ghosts)   // name tags
            {
                var sp = cam.WorldToScreenPoint(kv.Value.transform.position + new Vector3(0, 0.9f, 0));
                var p = new Vector2(sp.x / UiGui.S, (Screen.height - sp.y) / UiGui.S);
                GUI.Label(new Rect(p.x - 60, p.y - 10, 120, 18), kv.Key, small);
            }
            if (!open) return;
            float w = Mathf.Min(460, UiGui.W - 40);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, 60, w, Mathf.Min(UiGui.H - 120, 420)), box);
            GUILayout.Label("<b>" + Loc.T("net.title", "LAN multiplayer") + "</b>   <size=11>" + Net.Status + "</size>", label);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.T("net.name", "Name"), label, GUILayout.Width(70));
            Net.MyName = GUILayout.TextField(Net.MyName, 16);
            GUILayout.EndHorizontal();
            if (Net.Current == Net.Mode.Off)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Loc.T("net.address", "Host IP"), label, GUILayout.Width(70));
                address = GUILayout.TextField(address, 64);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Loc.T("net.host", "Host this world"))) { Remember(); Net.StartHost(); }
                if (GUILayout.Button(Loc.T("net.join", "Join"))) { Remember(); Net.Join(address.Trim(), Net.MyName); }
                GUILayout.EndHorizontal();
                GUILayout.Label("<size=11>" + Loc.T("net.help", "Same network only (port {0}). Guests play in the host's world with their own inventory; programs run on the host's computer, in its sandbox.", Multiplayer.Port) + "</size>", label);
            }
            else
            {
                if (Net.IsHost)
                    foreach (var p in Net.Peers)
                    {
                        if (p.Name == null) continue;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{p.Name}  <size=11>({p.Role})</size>", label);
                        if (GUILayout.Button(p.Role == Multiplayer.Builder ? Loc.T("net.makevisitor", "Make visitor") : Loc.T("net.makebuilder", "Make builder"), GUILayout.Width(110)))
                            Net.SetRole(p.Name, p.Role == Multiplayer.Builder ? Multiplayer.Visitor : Multiplayer.Builder);
                        if (GUILayout.Button(Loc.T("net.kick", "Kick"), GUILayout.Width(60))) Net.Kick(p.Name);
                        GUILayout.EndHorizontal();
                    }
                else GUILayout.Label(Loc.T("net.role", "Your role: {0}", Net.MyRole), label);
                if (GUILayout.Button(Loc.T("net.disconnect", "Disconnect"))) Net.Disconnect();
            }
            if (GUILayout.Button(Loc.T("net.close", "Close (Esc)"))) Close();
            GUILayout.EndArea();
        }

        void Remember() { try { PlayerPrefs.SetString("cb.netName", Net.MyName); PlayerPrefs.SetString("cb.netAddress", address); PlayerPrefs.Save(); } catch { } }
        void OnDestroy() { foreach (var g in ghosts.Values) if (g != null) Destroy(g.gameObject); }
    }
}
