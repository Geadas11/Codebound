using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        public string PublicId;                                           // the guest's account, when the game is listed online (verified by the service)
        public System.Threading.Tasks.Task<Codebound.Core.Online.ApiResult> Verify;
        public double X, Y, MoveBudget = Multiplayer.MoveBurst;
        public float LastMine, LastPos, LastCorrect = -10, LastChat = -10, LastUnlock = -10, PcTimer;
        public readonly float Connected = Time.realtimeSinceStartup;
        public readonly HashSet<Cell> Unlocked = new HashSet<Cell>();   // computers this guest gave the right password for (this session)
        public Cell? View;                                                // computer whose panel the guest has open

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

        /// <summary>Encryption of this connection; null/not ready only during the handshake.</summary>
        public SecureChannel Chan;
        public byte[] Salt;
        public bool Secure => Chan != null && Chan.Ready;

        public void Send(Dictionary<string, object> msg)
        {
            if (!Secure) return;   // nothing but the handshake travels in the clear
            Write(Chan.Seal(Json.Write(msg)));
        }

        /// <summary>Handshake messages and the final kick of a failed handshake only.</summary>
        public void SendPlain(Dictionary<string, object> msg) => Write(Json.Write(msg));

        void Write(string line)
        {
            if (Closed) return;
            try { var b = Encoding.UTF8.GetBytes(line + "\n"); stream.Write(b, 0, b.Length); }
            catch (Exception) { Close(); }
        }

        public void Close() { Closed = true; try { client.Close(); } catch (Exception) { } }
    }

    /// <summary>
    /// Co-op over LAN or the internet (Etapa 6). Static so the connection survives the scene reload a guest does when it receives the world.
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
        public static string WorldPassword = "";   // host: every guest must know it; it keys the encryption and never travels
        static string joinPassword = "";
        /// <summary>Guest: the one-use ticket from the online service for the game being joined (null for LAN / direct joins).</summary>
        public static string JoinTicket, OnlineSession;
        /// <summary>Host settings (the multiplayer screen sets them; defaults from Settings).</summary>
        public static int HostPort = Multiplayer.Port, MaxGuests = Multiplayer.MaxGuests;
        public static string GuestRole = Multiplayer.Builder, SessionName;
        public static bool WorldReceived => worldReceived;

        /// <summary>What to do once the chosen world has loaded: host it (the multiplayer screen's "Start server").</summary>
        public sealed class HostPlan { public string Password = "", Listing; public bool Upnp; }
        public static HostPlan PendingHost;

        public static readonly List<(string name, double x, double y)> Others = new List<(string, double, double)>();
        public static readonly List<(string from, string text, float at)> Chat = new List<(string, string, float)>();

        // guest side
        public static readonly HashSet<Cell> GuestUnlocked = new HashSet<Cell>();
        public static string UnlockMessage;
        static readonly Dictionary<Cell, Dictionary<string, object>> remotePcs = new Dictionary<Cell, Dictionary<string, object>>();
        static float vitTimer;

        static TcpListener listener;
        static readonly List<Peer> peers = new List<Peer>();
        static Peer server;
        static bool worldReceived;
        static float snapTimer, posTimer;
        static (int seed, Cell spawn)? spawnCache;
        static string hostWorld;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Disconnect(); Chat.Clear(); }

        internal static IReadOnlyList<Peer> Peers => peers;
        public static Dictionary<string, object> RemotePc(Cell c) => remotePcs.TryGetValue(c, out var m) ? m : null;

        public static string StartHost(int port = 0)
        {
            Disconnect();
            hostWorld = null;
            HostPort = port > 0 ? port : HostPort;
            try { listener = new TcpListener(IPAddress.Any, HostPort); listener.Start(); }
            catch (Exception e) { listener = null; return Status = Loc.T("net.cantHost", "could not host: {0}", e.Message); }
            Current = Mode.Host;
            return Status = Loc.T("net.hosting", "hosting on port {0}", HostPort);
        }

        public static string Join(string address, string name, string password = "", int port = 0)
        {
            if (port <= 0) (address, port) = Multiplayer.SplitAddress(address);
            if (port < 1 || port > 65535 || address.Length == 0) return Status = Loc.T("net.badAddress", "enter an address, optionally with :port");
            Disconnect();
            if (!Multiplayer.ValidName(name)) return Status = Loc.T("net.badName", "name: 1-16 letters, digits, _ or -");
            try
            {
                var c = new TcpClient();
                if (!c.ConnectAsync(address, port).Wait(8000)) { c.Close(); return Status = Loc.T("net.noAnswer", "no answer from {0}", address); }
                server = new Peer(c, Multiplayer.MaxHostLine);
            }
            catch (Exception e) { return Status = Loc.T("net.cantConnect", "could not connect: {0}", (e.InnerException ?? e).Message.Split((char)0)[0].Replace('\r', ' ').Replace('\n', ' ').Trim()); }   // Windows socket messages carry CRLF and NUL padding
            MyName = name; Current = Mode.Guest; worldReceived = false; joinPassword = password ?? "";
            return Status = Loc.T("net.joining", "joining {0}…", address);   // hello goes out encrypted once the host's handshake arrives
        }

        public static void Disconnect()
        {
            bool wasHost = Current == Mode.Host;
            try { listener?.Stop(); } catch (Exception) { }
            listener = null;
            foreach (var p in peers) p.Close();
            peers.Clear();
            server?.Close(); server = null;
            Others.Clear(); GuestUnlocked.Clear(); remotePcs.Clear(); UnlockMessage = null;
            Current = Mode.Off; Status = "offline"; MyRole = Multiplayer.Builder;
            if (wasHost) ThreadPool.QueueUserWorkItem(_ => RouterPort.Close());
        }

        public static void SendAct(Dictionary<string, object> act)
        {
            act["t"] = "act";
            server?.Send(act);
        }

        /// <summary>Guest → host: save the computer's code/files/watches and do op (run, debug, stop, resume, bp, snippet, setpwd, clear, or "" = just save).</summary>
        public static void Program(Cell at, ComputerState cs, string op, object arg = null)
        {
            var files = new Dictionary<string, object>();
            foreach (var kv in cs.Files) files[kv.Key] = kv.Value;
            var m = new Dictionary<string, object> { { "t", "program" }, { "x", (double)at.X }, { "y", (double)at.Y }, { "lang", cs.Language }, { "code", cs.Code },
                                                     { "files", files }, { "watches", new List<object>(cs.Watches) }, { "op", op ?? "" } };
            if (arg != null) m["arg"] = arg;
            server?.Send(m);
        }

        public static void View(Cell? at)
        {
            var m = new Dictionary<string, object> { { "t", "view" } };
            if (at is Cell c) { m["x"] = (double)c.X; m["y"] = (double)c.Y; }
            server?.Send(m);
        }

        public static void Unlock(Cell at, string password)
        {
            UnlockMessage = Loc.T("net.checking", "checking…");
            server?.Send(new Dictionary<string, object> { { "t", "unlock" }, { "x", (double)at.X }, { "y", (double)at.Y }, { "password", password ?? "" } });
        }

        public static void SendChat(string text)
        {
            text = Multiplayer.CleanChat(text);
            if (text == null) return;
            if (IsHost) BroadcastChat(MyName, text);
            else server?.Send(new Dictionary<string, object> { { "t", "chat" }, { "text", text } });
        }

        static void BroadcastChat(string from, string text)
        {
            AddChat(from, text);
            foreach (var p in peers) if (p.Name != null) p.Send(new Dictionary<string, object> { { "t", "chat" }, { "from", from }, { "text", text } });
        }

        static void AddChat(string from, string text)
        {
            Chat.Add((from, text, Time.realtimeSinceStartup));
            if (Chat.Count > 50) Chat.RemoveAt(0);
        }

        public static void Kick(string name, bool ban = false)
        {
            foreach (var p in peers)
                if (p.Name == name)
                {
                    p.Send(Msg("kick", Loc.T("net.kicked", "removed by the host"))); p.Close();
                    if (p.PublicId != null && Online.HostedSession != null && Online.SignedIn) _ = Online.Client.Kick(Online.HostedSession, p.PublicId, ban);   // a ban keeps them out of this listing
                }
        }
        public static void SetRole(string name, string role) { foreach (var p in peers) if (p.Name == name) { p.Role = role; p.Send(Msg("msg", "the host made you a " + role)); } }

        static Dictionary<string, object> Msg(string t, string text) => new Dictionary<string, object> { { "t", t }, { "text", text } };
        static double D(Dictionary<string, object> m, string k) => m.TryGetValue(k, out var v) && v is double d && !double.IsNaN(d) && !double.IsInfinity(d) ? d : 0;
        static string S(Dictionary<string, object> m, string k) => m.TryGetValue(k, out var v) ? v as string : null;
        static Cell CellOf(Dictionary<string, object> m) => new Cell((int)Math.Max(-1e9, Math.Min(1e9, D(m, "x"))), (int)Math.Max(-1e9, Math.Min(1e9, D(m, "y"))));

        /// <summary>Called every frame by NetSession.</summary>
        internal static void Pump(GameSession s, ProgramManager programs)
        {
            if (IsHost) PumpHost(s, programs);
            else if (IsGuest) PumpGuest(s);
        }

        // ------------------------------------------------------------------ host
        static Cell Spawn(GameState g)
        {
            if (spawnCache.HasValue && spawnCache.Value.seed == g.Seed) return spawnCache.Value.spawn;
            var sp = g.World.Generator.FindSpawn();
            spawnCache = (g.Seed, sp);
            return sp;
        }

        static void PumpHost(GameSession s, ProgramManager programs)
        {
            try
            {
                while (listener != null && listener.Pending())
                {
                    var p = new Peer(listener.AcceptTcpClient(), Multiplayer.MaxGuestLine);
                    if (peers.Count >= Math.Min(MaxGuests, Multiplayer.MaxGuests)) { p.SendPlain(Msg("kick", Loc.T("net.full", "the world is full"))); p.Close(); continue; }
                    p.Chan = new SecureChannel(); p.Salt = SecureChannel.NewSalt();
                    p.SendPlain(new Dictionary<string, object> { { "t", "hs" }, { "v", (double)SecureChannel.Version }, { "salt", Convert.ToBase64String(p.Salt) }, { "pub", Convert.ToBase64String(p.Chan.Public) } });
                    peers.Add(p);
                }
            }
            catch (Exception e) { Debug.LogWarning("Codebound net accept: " + e.Message); }

            float now = Time.realtimeSinceStartup;
            string world = s.State.Seed + ":" + s.State.PlanetId;
            if (world != hostWorld)
            {
                if (hostWorld != null)
                    foreach (var p in peers)
                    {
                        if (p.Name == null) continue;
                        var body = Body(s.State, p.Name);
                        body.X = s.Player.position.x; body.Y = s.Player.position.y;   // everyone arrives next to the host
                        p.X = body.X; p.Y = body.Y; p.MoveBudget = Multiplayer.MoveBurst; p.LastPos = now;
                        p.Unlocked.Clear(); p.View = null;   // other world, other computers
                    }
                hostWorld = world; snapTimer = 0;
            }
            foreach (var p in peers)
            {
                if (p.Name == null && now - p.Connected > (p.Verify != null ? 25 : 10)) { p.Close(); continue; }   // never said hello (or the service never answered)
                if (p.Name == null && p.Verify != null) { if (p.Verify.IsCompleted) Verified(s, p); continue; }
                int budget = 64;   // messages per frame per guest
                while (budget-- > 0 && p.Inbox.TryDequeue(out var raw))
                {
                    if (!p.Secure) { if (!Handshake(p, raw)) break; continue; }
                    string line;
                    try { line = p.Chan.Open(raw); }
                    catch (CryptographicException)
                    {   // first message unreadable = the guest's password made different keys
                        p.SendPlain(Msg("kick", p.Name == null ? Loc.T("net.wrongWorldPassword", "wrong world password") : "bad message"));
                        p.Close(); break;
                    }
                    Dictionary<string, object> m;
                    try { m = Json.Parse(line) as Dictionary<string, object>; } catch (FormatException) { m = null; }
                    if (m == null || !(m.TryGetValue("t", out var tv) && tv is string t)) continue;
                    if (p.Name == null) { if (!Hello(s, p, t, m)) break; continue; }
                    switch (t)
                    {
                        case "pos":
                        {
                            double nx = D(m, "x"), ny = D(m, "y");
                            var sp = Spawn(s.State);
                            if (Multiplayer.CheckMove(s.IsSolid, ref p.MoveBudget, now - p.LastPos, p.X, p.Y, nx, ny, sp.X + 0.5, sp.Y + 0.5)) { p.X = nx; p.Y = ny; }
                            else if (now - p.LastCorrect > 0.5f)   // impossible move: put the guest back where the host saw it
                            {
                                p.LastCorrect = now;
                                p.Send(new Dictionary<string, object> { { "t", "setpos" }, { "x", p.X }, { "y", p.Y } });
                            }
                            p.LastPos = now;
                            var body = Body(s.State, p.Name); body.X = p.X; body.Y = p.Y;
                            break;
                        }
                        case "vit":
                            try { Multiplayer.LoadVitals(Body(s.State, p.Name).Vitals, S(m, "line")); } catch (FormatException) { }
                            break;
                        case "act":
                            string err;
                            if (m.TryGetValue("a", out var av) && av as string == "mine" && Time.time - p.LastMine < 0.25f) err = "slow down";
                            else
                            {
                                if (av as string == "mine") p.LastMine = Time.time;
                                err = Multiplayer.Act(s.State, p.Role, s.State.GuestInventory(p.Name), p.X, p.Y, m, p.Name);
                            }
                            if (err != null) p.Send(Msg("msg", err));
                            break;
                        case "program":
                            if (Program(s, programs, p, m) is string perr) p.Send(Msg("msg", perr));
                            p.PcTimer = 0;   // show the effect at once
                            break;
                        case "chat":
                            if (now - p.LastChat < 0.4f) { p.Send(Msg("msg", "slow down")); break; }
                            p.LastChat = now;
                            if (Multiplayer.CleanChat(S(m, "text")) is string text) BroadcastChat(p.Name, text);
                            break;
                        case "view":
                            p.View = m.ContainsKey("x") ? CellOf(m) : (Cell?)null; p.PcTimer = 0;
                            break;
                        case "unlock":
                        {
                            if (now - p.LastUnlock < 1f) { p.Send(new Dictionary<string, object> { { "t", "unlocked" }, { "ok", false }, { "text", "wait a second between attempts" } }); break; }
                            p.LastUnlock = now;
                            var b = s.Grid.At(CellOf(m));
                            bool ok = b?.State is ComputerState cs && Near(b, p) && cs.CheckPassword(S(m, "password"));
                            if (ok) { p.Unlocked.Add(b.Origin); snapTimer = 0; p.PcTimer = 0; }
                            p.Send(new Dictionary<string, object> { { "t", "unlocked" }, { "x", D(m, "x") }, { "y", D(m, "y") }, { "ok", ok } });
                            break;
                        }
                    }
                }
            }
            foreach (var p in peers.FindAll(x => x.Closed))
            {
                if (p.Name != null) s.State.Hud.Notify(Loc.T("notify.left", "{0} left the game", p.Name));
                if (p.PublicId != null && Online.HostedSession != null && Online.SignedIn) _ = Online.Client.PlayerLeft(Online.HostedSession, p.PublicId);
            }
            peers.RemoveAll(x => x.Closed);

            foreach (var p in peers)   // the open computer's console, debugger and monitor, 4×/s
                if (p.Name != null && p.View is Cell v && (p.PcTimer -= Time.unscaledDeltaTime) <= 0)
                {
                    p.PcTimer = 0.25f;
                    var b = s.Grid.At(v);
                    if (b?.State is ComputerState cs && (!cs.HasPassword || p.Unlocked.Contains(b.Origin))) p.Send(PcInfo(programs, b, cs));
                }

            if ((snapTimer -= Time.deltaTime) > 0 || peers.Count == 0) return;
            snapTimer = 0.5f;
            string shared = null;
            var creatures = Multiplayer.CreaturesOf(s.State);
            var players = new List<object> { new List<object> { MyName, (double)s.Player.position.x, (double)s.Player.position.y } };
            foreach (var p in peers) if (p.Name != null) players.Add(new List<object> { p.Name, p.X, p.Y });
            foreach (var p in peers)
            {
                if (p.Name == null) continue;
                string snap = p.Unlocked.Count > 0 ? Multiplayer.Snapshot(s.State, p.Unlocked) : shared ?? (shared = Multiplayer.Snapshot(s.State));
                var body = Body(s.State, p.Name);
                p.Send(new Dictionary<string, object> { { "t", "snap" }, { "save", snap }, { "role", p.Role }, { "creatures", creatures }, { "players", players },
                                                        { "inv", Multiplayer.InventoryOf(s.State.GuestInventory(p.Name)) },
                                                        { "you", new Dictionary<string, object> { { "x", body.X }, { "y", body.Y }, { "vit", body.Vitals.SaveLine() } } } });
            }
            Others.Clear();
            foreach (var p in peers) if (p.Name != null) Others.Add((p.Name, p.X, p.Y));
        }

        static GuestBody Body(GameState g, string name)
        {
            if (!g.GuestBodies.TryGetValue(name, out var b)) g.GuestBodies[name] = b = new GuestBody { X = g.PlayerX, Y = g.PlayerY };
            return b;
        }

        /// <summary>Host side: the guest's public value arrives in the clear; derive the keys from it and the world password.</summary>
        static bool Handshake(Peer p, string raw)
        {
            try
            {
                var m = Json.Parse(raw) as Dictionary<string, object>;
                if (m == null || S(m, "t") != "hs" || D(m, "v") != SecureChannel.Version) throw new FormatException("handshake");
                var guestPub = Convert.FromBase64String(S(m, "pub") ?? "");
                p.Chan.Complete(guestPub, WorldPassword, p.Salt, true, p.Chan.Public, guestPub);
                return true;
            }
            catch (Exception e) when (e is FormatException || e is CryptographicException)
            {
                p.SendPlain(Msg("kick", Loc.T("net.version", "different game version")));
                p.Close();
                return false;
            }
        }

        static bool Hello(GameSession s, Peer p, string t, Dictionary<string, object> m)
        {
            string name = S(m, "name");
            string why = t != "hello" ? "say hello first" : D(m, "v") != Multiplayer.ProtocolVersion ? Loc.T("net.version", "different game version") : null;
            if (why == null && Online.HostedSession != null)
            {   // listed online: who the guest is comes from the service (its one-use ticket), never from the name it sends
                if (!(S(m, "ticket") is string ticket) || !Online.SignedIn) why = Loc.T("net.needAccount", "this game needs a Codebound account: join it from the online games list");
                else { p.Verify = Online.Client.VerifyTicket(Online.HostedSession, ticket); return false; }
            }
            return why == null ? Admit(s, p, name) : Refuse(p, why);
        }

        static bool Refuse(Peer p, string why) { p.Send(Msg("kick", why)); p.Close(); return false; }   // encrypted: the guest has the keys by now

        static void Verified(GameSession s, Peer p)
        {
            var r = p.Verify.Result; p.Verify = null;
            if (!r.Ok) { Refuse(p, r.Offline ? Loc.T("net.cantVerify", "the host could not check your account right now; try again") : r.Message); return; }
            p.PublicId = r.Str("publicId");
            if (peers.Exists(o => o != p && o.PublicId == p.PublicId)) { Refuse(p, Loc.T("net.alreadyIn", "you are already in this game")); return; }
            Admit(s, p, r.Str("displayName"));
        }

        static bool Admit(GameSession s, Peer p, string name)
        {
            string why = !Multiplayer.ValidName(name) || name == MyName ? Loc.T("net.invalidName", "invalid name") : peers.Exists(o => o.Name == name) ? Loc.T("net.nameInUse", "name already in use") : null;
            if (why != null) return Refuse(p, why);
            p.Name = name;
            p.Role = GuestRole;   // the host's chosen permission for newcomers (can be changed per player in the N panel)
            bool known = s.State.GuestBodies.ContainsKey(name);
            var body = Body(s.State, name);
            if (!known) { body.X = s.Player.position.x; body.Y = s.Player.position.y; }   // first visit: appear next to the host
            p.X = body.X; p.Y = body.Y; p.LastPos = Time.realtimeSinceStartup;
            s.State.GuestInventory(name);
            s.State.Hud.Notify(Loc.T("notify.joined", "{0} joined the game", name));
            snapTimer = 0;
            return true;
        }

        static bool Near(PlacedBuilding b, Peer p) => Math.Sqrt(Math.Pow(b.CenterX - p.X, 2) + Math.Pow(b.CenterY - p.Y, 2)) <= Multiplayer.ActionReach + 3;

        static Dictionary<string, object> PcInfo(ProgramManager programs, PlacedBuilding b, ComputerState cs)
        {
            var dbg = programs.GetDebug(b); var st = programs.GetStats(b);
            var log = new List<object>();
            for (int i = Math.Max(0, cs.Log.Count - 80); i < cs.Log.Count; i++) log.Add(cs.Log[i]);
            var calls = new Dictionary<string, object>();
            if (programs.CallMap(b) is Dictionary<string, int> cm) foreach (var kv in cm) calls[kv.Key] = (double)kv.Value;
            return new Dictionary<string, object>
            {
                { "t", "pc" }, { "x", (double)b.Origin.X }, { "y", (double)b.Origin.Y }, { "run", programs.IsRunning(b) }, { "dbg", dbg.Debugging },
                { "line", (double)dbg.PausedLine }, { "vars", dbg.Vars }, { "stack", dbg.Stack }, { "watches", dbg.Watches }, { "log", log },
                { "err", cs.LastError }, { "errLine", (double)cs.LastErrorLine }, { "errFile", cs.LastErrorFile },
                { "cpu", (double)st.Cpu }, { "heap", st.HeapMb }, { "ram", (double)st.RamMb }, { "cps", (double)st.CallsPerSec }, { "waiting", st.Waiting }, { "calls", calls },
            };
        }

        /// <summary>A guest edits/runs/debugs a program on a host computer (shared projects). Runs in the host's sandbox.</summary>
        static string Program(GameSession s, ProgramManager programs, Peer p, Dictionary<string, object> m)
        {
            if (p.Role == Multiplayer.Visitor) return "you are a visitor";
            var b = s.Grid.At(CellOf(m));
            if (!(b?.State is ComputerState cs)) return "no computer there";
            if (!Near(b, p)) return "too far";
            if (cs.HasPassword && !p.Unlocked.Contains(b.Origin)) return "this computer is locked: unlock it with its password first";
            string code = S(m, "code"), lang = S(m, "lang");
            if (code != null)
            {
                if (code.Length > Multiplayer.MaxProgramChars) return "program too long";
                if (Array.IndexOf(ComputerState.Languages, lang) < 0) return "unknown language";
                var files = m.TryGetValue("files", out var fv) ? fv as Dictionary<string, object> : null;
                if (files != null)
                {
                    if (files.Count > ComputerState.MaxFiles) return "too many files";
                    foreach (var kv in files) if (!ComputerState.ValidFileName(kv.Key) || !(kv.Value is string src) || src.Length > Multiplayer.MaxProgramChars) return "bad file " + kv.Key;
                }
                if (!programs.IsRunning(b))   // a running program keeps the code it started with, as on the host's own screen
                {
                    cs.Code = code; cs.Language = lang;
                    if (files != null) { cs.Files.Clear(); foreach (var kv in files) cs.Files[kv.Key] = (string)kv.Value; }
                }
                if (m.TryGetValue("watches", out var wv) && wv is List<object> wl)
                {
                    cs.Watches.Clear();
                    foreach (var w in wl) if (w is string ws && ws.Length <= 80 && cs.Watches.Count < 12) cs.Watches.Add(ws);
                }
            }
            string arg = m.TryGetValue("arg", out var a) ? a as string : null;
            switch (S(m, "op") ?? "")
            {
                case "run": programs.StartProgram(b); break;
                case "debug": programs.StartProgram(b, debug: true); break;
                case "stop": programs.Stop(b); break;
                case "clear": cs.Log.Clear(); break;
                case "resume": if (arg == "continue" || arg == "step" || arg == "over" || arg == "out") programs.Resume(b, arg); break;
                case "bp": { double l = D(m, "arg"); if (l >= 1 && l <= 100000) programs.ToggleBreakpoint(b, (int)l); break; }
                case "snippet":
                    if (arg == null || arg.Length > Multiplayer.MaxSnippet) return "snippet too long";
                    if (!programs.IsRunning(b)) programs.StartProgram(b, snippet: arg);
                    break;
                case "setpwd":
                    if (arg == null || arg.Length > 64) return "password too long";
                    cs.SetPassword(arg.Length == 0 ? null : arg);
                    p.Unlocked.Add(b.Origin);
                    break;
            }
            return null;
        }

        // ------------------------------------------------------------------ guest
        static void PumpGuest(GameSession s)
        {
            if (server == null || server.Closed)
            {
                while (server != null && server.Inbox.TryDequeue(out var last))   // the host's reason may still be waiting (it closes right after a kick)
                {
                    if (!last.StartsWith("{")) { try { last = server.Secure ? server.Chan.Open(last) : null; } catch (CryptographicException) { last = null; } }
                    Dictionary<string, object> km = null;
                    try { if (last != null) km = Json.Parse(last) as Dictionary<string, object>; } catch (FormatException) { }
                    if (km != null && S(km, "t") == "kick") Status = "kicked: " + S(km, "text");
                }
                string why = Status.StartsWith("kicked") ? Status : Loc.T("net.lost", "connection to the host lost");
                Disconnect(); Status = why;
                s.State.Hud.Notify(Loc.T("notify.disconnected", "Multiplayer: {0}", why));
                return;
            }
            if (worldReceived && (posTimer -= Time.deltaTime) <= 0)
            {
                posTimer = 0.1f;
                server.Send(new Dictionary<string, object> { { "t", "pos" }, { "x", Math.Round(s.Player.position.x, 3) }, { "y", Math.Round(s.Player.position.y, 3) } });
            }
            if (worldReceived && (vitTimer -= Time.deltaTime) <= 0)
            {
                vitTimer = 2f;
                server.Send(new Dictionary<string, object> { { "t", "vit" }, { "line", s.State.Vitals.SaveLine() } });
            }
            string latest = null; Dictionary<string, object> latestMsg = null;
            while (server.Inbox.TryDequeue(out var raw))
            {
                Dictionary<string, object> m;
                string line = raw;
                if (raw.StartsWith("{"))   // in the clear: only the handshake, or the kick of a failed one
                {
                    try { m = Json.Parse(raw) as Dictionary<string, object>; } catch (FormatException) { continue; }
                    if (S(m, "t") == "kick") { Status = "kicked: " + S(m, "text"); server.Close(); return; }
                    if (S(m, "t") != "hs" || server.Secure) continue;
                    try
                    {
                        if (D(m, "v") != SecureChannel.Version) throw new FormatException("version");
                        var hostPub = Convert.FromBase64String(S(m, "pub") ?? "");
                        server.Chan = new SecureChannel();
                        server.Chan.Complete(hostPub, joinPassword, Convert.FromBase64String(S(m, "salt") ?? ""), false, hostPub, server.Chan.Public);
                    }
                    catch (Exception e) when (e is FormatException || e is CryptographicException) { Status = "kicked: " + Loc.T("net.version", "different game version"); server.Close(); return; }
                    server.SendPlain(new Dictionary<string, object> { { "t", "hs" }, { "v", (double)SecureChannel.Version }, { "pub", Convert.ToBase64String(server.Chan.Public) } });
                    var hello = new Dictionary<string, object> { { "t", "hello" }, { "name", MyName }, { "v", (double)Multiplayer.ProtocolVersion } };
                    if (JoinTicket != null) { hello["ticket"] = JoinTicket; JoinTicket = null; }   // one use
                    server.Send(hello);
                    continue;
                }
                if (!server.Secure) continue;
                try { line = server.Chan.Open(raw); }
                catch (CryptographicException) { Status = "kicked: " + Loc.T("net.badData", "the connection was tampered with"); server.Close(); return; }
                try { m = Json.Parse(line) as Dictionary<string, object>; } catch (FormatException) { continue; }
                if (m == null || !(m.TryGetValue("t", out var tv) && tv is string t)) continue;
                switch (t)
                {
                    case "snap": if (S(m, "save") is string save) { latest = save; latestMsg = m; } break;
                    case "msg": if (S(m, "text") is string text) s.State.Hud.Notify(Loc.T("net.fromHost", "Host: {0}", text)); break;
                    case "kick": Status = "kicked: " + S(m, "text"); server.Close(); return;
                    case "chat": if (S(m, "from") is string from && Multiplayer.ValidName(from) && Multiplayer.CleanChat(S(m, "text")) is string ct) AddChat(from, ct); break;
                    case "setpos": if (worldReceived) s.Player.position = new Vector3((float)D(m, "x"), (float)D(m, "y"), s.Player.position.z); break;
                    case "pc": remotePcs[CellOf(m)] = m; break;
                    case "unlocked":
                        if (m.TryGetValue("ok", out var ok) && ok is true) { GuestUnlocked.Add(CellOf(m)); UnlockMessage = null; }
                        else UnlockMessage = S(m, "text") ?? Loc.T("ed.wrongPassword", "Wrong password");
                        break;
                }
            }
            if (latest == null) return;   // only the newest snapshot matters
            GameState snap;
            try { snap = SaveSystem.Read(new StringReader(latest)); }
            catch (InvalidDataException e) { Debug.LogWarning("Codebound net: bad snapshot: " + e.Message); return; }
            MyRole = S(latestMsg, "role") ?? Multiplayer.Visitor;
            bool otherWorld = worldReceived && (snap.Seed != s.State.Seed || snap.PlanetId != s.State.PlanetId);
            if (otherWorld)   // the host's ship flew (or went underground): follow, with our own vitals and things
            {
                snap.Vitals.Load(s.State.Vitals.SaveLine().Split(' '));
                if (latestMsg.TryGetValue("you", out var yw) && yw is Dictionary<string, object> youW) { snap.PlayerX = D(youW, "x"); snap.PlayerY = D(youW, "y"); }
                Multiplayer.LoadInventory(snap.Inventory, latestMsg.TryGetValue("inv", out var ivw) ? ivw as List<object> : null);
                GuestUnlocked.Clear(); remotePcs.Clear();
                var to = StarSystem.Get(snap.HomeSeed, snap.PlanetId);
                snap.Hud.Notify(Loc.T("net.travelled", "The host's ship took everyone to {0}.", to?.Name ?? snap.PlanetId));
                GameBootstrap.LoadGuestState(snap);
                return;
            }
            if (!worldReceived)
            {
                worldReceived = true;
                Status = Loc.T("net.connectedAs", "connected as {0}", MyName);
                snap.Vitals.Respawn();
                if (latestMsg.TryGetValue("you", out var yv) && yv is Dictionary<string, object> you)   // back where (and as) we left
                {
                    snap.PlayerX = D(you, "x"); snap.PlayerY = D(you, "y");
                    try { Multiplayer.LoadVitals(snap.Vitals, S(you, "vit")); } catch (FormatException) { }
                    if (snap.Vitals.Dead) snap.Vitals.Respawn();
                }
                Multiplayer.LoadInventory(snap.Inventory, latestMsg.TryGetValue("inv", out var iv0) ? iv0 as List<object> : null);
                GameBootstrap.LoadGuestState(snap);   // rebuilds the scene around the host's world
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

        /// <summary>This PC's LAN addresses, for the host to tell friends on the same network.</summary>
        public static string FirstLanAddress() { var a = LocalAddresses(); return a == "?" ? null : a.Split(',')[0].Trim(); }

        public static string LocalAddresses()
        {
            try
            {
                var l = new List<string>();
                foreach (var a in Dns.GetHostAddresses(Dns.GetHostName()))
                    if (a.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a)) l.Add(a.ToString());
                return l.Count > 0 ? string.Join(", ", l) : "?";
            }
            catch (Exception) { return "?"; }
        }
    }

    /// <summary>Pumps the network each frame, draws the other players and the chat, and the N panel (host / join / players).</summary>
    public sealed class NetSession : MonoBehaviour
    {
        GameSession session;
        ProgramManager programs;
        Camera cam;
        readonly Dictionary<string, SpriteRenderer> ghosts = new Dictionary<string, SpriteRenderer>();
        bool open, chatting, focusChat;
        int chatOpenedFrame;
        string address = "127.0.0.1", joinPassword = "", chatInput = "", localAddresses;
        Vector2 chatScroll;
        static bool commandLineDone, quitHooked;
        GUIStyle box, label, small, chatStyle;
        int styleVersion = -1;

        static NetSession instance;

        /// <summary>The pause menu's "Multiplayer": opens the N panel (host / join / players).</summary>
        public static void OpenPanel() { if (instance != null && !instance.open) instance.ShowPanel(); }

        void ShowPanel() { open = true; localAddresses = null; session.PanelOpen = true; session.KeyboardCaptured = true; }

        public void Init(GameSession s, ProgramManager pm, Camera c)
        {
            session = s; programs = pm; cam = c; instance = this;
            try
            {
                if (Net.Current == Net.Mode.Off) Net.MyName = PlayerPrefs.GetString("cb.netName", "player");   // a guest keeps its name across the world reload
                address = PlayerPrefs.GetString("cb.netAddress", "127.0.0.1");
            }
            catch { }
            if (!quitHooked) { quitHooked = true; Application.quitting += RouterPort.Close; }   // never leave a port open on the router
            if (Net.Current == Net.Mode.Off) { Net.HostPort = Settings.NetPort; Net.MaxGuests = Settings.MaxGuests; Net.GuestRole = Settings.GuestRole; }
            if (Net.PendingHost is Net.HostPlan plan && !Net.IsGuest)   // "Start server" from the multiplayer screen, now that the world is loaded
            {
                Net.PendingHost = null;
                Net.WorldPassword = plan.Password ?? "";
                var st = Net.StartHost(Net.HostPort);
                Ui.Toast(st, Net.IsHost ? "good" : "bad");
                if (Net.IsHost && plan.Upnp) RouterPort.OpenAsync(Net.HostPort);
                if (Net.IsHost && plan.Listing != null && Online.SignedIn) ListAs(plan.Listing);
            }
            if (commandLineDone) return;
            commandLineDone = true;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-name" && i + 1 < args.Length) Net.MyName = args[i + 1];
                if (args[i] == "-worldpass" && i + 1 < args.Length) Net.WorldPassword = joinPassword = args[i + 1];
            }
            for (int i = 0; i < args.Length; i++) if (args[i] == "-host") Debug.Log("[Codebound] " + Net.StartHost());
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-join") Debug.Log("[Codebound] " + Net.Join(args[i + 1], Net.MyName, joinPassword));
        }

        static Color NameColor(string name) => Color.HSVToRGB((name.GetHashCode() & 0xff) / 255f, 0.45f, 1f);

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
                    sr.color = NameColor(name);
                    ghosts[name] = sr;
                }
                sr.transform.position = Vector3.Lerp(sr.transform.position, new Vector3((float)x, (float)y, 0), 1f - Mathf.Exp(-12f * Time.deltaTime));
            }
            foreach (var n in new List<string>(ghosts.Keys)) if (!seen.Contains(n)) { Destroy(ghosts[n].gameObject); ghosts.Remove(n); }

            var kb = Keyboard.current;
            if (kb == null) return;
            if (open && kb.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (chatting && kb.escapeKey.wasPressedThisFrame) { chatting = false; session.KeyboardCaptured = false; return; }
            bool free = !session.KeyboardCaptured && !session.PanelOpen && !OptionsMenu.IsOpen;
            if (!open && free && Controls.Pressed("network")) ShowPanel();
            else if (!chatting && free && Net.Current != Net.Mode.Off && Controls.Pressed("chat")) { chatting = focusChat = true; chatOpenedFrame = Time.frameCount; chatInput = ""; session.KeyboardCaptured = true; }
        }

        void Close() { open = false; session.PanelOpen = false; session.KeyboardCaptured = false; }

        void OnGUI()
        {
            UiGui.Begin();
            if (box == null || styleVersion != Settings.Version)
            {
                styleVersion = Settings.Version;
                box = new GUIStyle(UiTheme.Panel);
                label = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(14), richText = true, wordWrap = true, normal = { textColor = UiTheme.Text } };
                small = new GUIStyle(label) { fontSize = 11, alignment = TextAnchor.MiddleCenter, wordWrap = false };
                chatStyle = new GUIStyle(label) { fontSize = 13 };
            }
            foreach (var kv in ghosts)   // name tags
            {
                var sp = cam.WorldToScreenPoint(kv.Value.transform.position + new Vector3(0, 0.9f, 0));
                var p = new Vector2(sp.x / UiGui.S, (Screen.height - sp.y) / UiGui.S);
                GUI.Label(new Rect(p.x - 60, p.y - 10, 120, 18), kv.Key, small);
            }
            DrawChat();
            if (open) DrawPanel();
        }

        void DrawChat()
        {
            if (Net.Current == Net.Mode.Off && !chatting) return;
            float now = Time.realtimeSinceStartup, w = Mathf.Min(520, UiGui.W - 40);
            var recent = new List<(string from, string text, float at)>();
            foreach (var c in Net.Chat) if (chatting || now - c.at < 15f) recent.Add(c);
            if (recent.Count > (chatting ? 12 : 6)) recent.RemoveRange(0, recent.Count - (chatting ? 12 : 6));
            if (recent.Count == 0 && !chatting) return;
            var sb = new StringBuilder();
            foreach (var (from, text, _) in recent)
                sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(NameColor(from))).Append("><b>").Append(from).Append("</b></color>: ").Append(text).Append('\n');
            float h = (recent.Count + (chatting ? 2 : 0)) * 20 + 16;
            GUILayout.BeginArea(new Rect(20, UiGui.H - h - 90, w, h), chatting ? box : GUIStyle.none);
            GUILayout.Label(sb.ToString().TrimEnd('\n'), chatStyle);
            if (chatting)
            {
                var e = Event.current;
                bool send = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) && Time.frameCount > chatOpenedFrame;   // not the key press that opened it
                GUILayout.BeginHorizontal();
                GUI.SetNextControlName("chat");
                chatInput = GUILayout.TextField(chatInput, Multiplayer.MaxChat);
                if (focusChat) { GUI.FocusControl("chat"); focusChat = false; }
                if (GUILayout.Button(Loc.T("net.send", "Send"), GUILayout.Width(70)) || send)
                {
                    if (send) e.Use();
                    Net.SendChat(chatInput);
                    chatting = false; session.KeyboardCaptured = false; chatInput = "";
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        void DrawPanel()
        {
            float w = Mathf.Min(680, UiGui.W - 40);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, 50, w, Mathf.Min(UiGui.H - 100, 600)), box);
            GUILayout.Label(Loc.T("net.title2", "Multiplayer").ToUpperInvariant(), UiTheme.H1);
            GUILayout.Label(Net.Status + (Net.IsHost ? "  ·  " + Loc.T("net.playersCount", "{0}/{1} players", Net.Peers.Count + 1, Net.MaxGuests + 1) : ""), UiTheme.Muted_);
            Ui.Rule(UiTheme.Accent, 2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.T("net.name", "Name"), label, GUILayout.Width(110));
            Net.MyName = GUILayout.TextField(Net.MyName, 16);
            GUILayout.EndHorizontal();
            if (Net.Current == Net.Mode.Off)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Loc.T("net.address2", "Host address"), label, GUILayout.Width(110));
                address = GUILayout.TextField(address, 128);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(Loc.T("net.worldPassword", "World password"), label, GUILayout.Width(110));
                joinPassword = GUILayout.PasswordField(joinPassword, '*', 64);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Loc.T("net.host", "Host this world"))) { Remember(); Net.WorldPassword = joinPassword; Net.StartHost(); }
                if (GUILayout.Button(Loc.T("net.join", "Join"))) { Remember(); Net.Join(address.Trim(), Net.MyName, joinPassword); }
                GUILayout.EndHorizontal();
                GUILayout.Label("<size=11>" + Loc.T("net.help3", "Host: the password is optional; everyone who joins must know it. The connection is always encrypted (with a password, also against impostors). Join: the host's IP or name; on the internet the host must open port {0}. Guests keep their own inventory; programs run on the host's computer, in its sandbox.", Net.HostPort) + "</size>", label);
            }
            else
            {
                if (Net.IsHost)
                {
                    localAddresses ??= Net.LocalAddresses();
                    GUILayout.Label("<size=12>" + Loc.T("net.lan", "Same network: join at {0}", localAddresses) + (Net.WorldPassword.Length > 0 ? "  ·  " + Loc.T("net.withPassword", "password required") : "") + "</size>", label);
                    GUI.enabled = !RouterPort.Busy;
                    if (GUILayout.Button(Loc.T("net.openInternet", "Open to the internet (UPnP on the router)"))) RouterPort.OpenAsync(Net.HostPort);
                    GUI.enabled = true;
                    if (RouterPort.Status != null) GUILayout.Label("<size=12>" + RouterPort.Status + "</size>", label);
                    OnlineListing();
                    foreach (var p in Net.Peers)
                    {
                        if (p.Name == null) continue;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"<b>{p.Name}</b>  <color={UiTheme.Html(UiTheme.Muted)}>{Loc.T("net.role." + p.Role, p.Role)}</color>", label, GUILayout.ExpandWidth(true));
                        string next = p.Role == Multiplayer.Visitor ? Multiplayer.Builder : p.Role == Multiplayer.Builder ? Multiplayer.Admin : Multiplayer.Visitor;
                        if (GUILayout.Button(Loc.T("net.make", "Make {0}", Loc.T("net.role." + next, next)), GUILayout.Width(220))) Net.SetRole(p.Name, next);
                        if (GUILayout.Button(Loc.T("net.kick", "Kick"), UiTheme.DangerBtn, GUILayout.Width(100))) Net.Kick(p.Name);
                        if (p.PublicId != null && GUILayout.Button(Loc.T("net.ban", "Ban"), UiTheme.DangerBtn, GUILayout.Width(90))) Net.Kick(p.Name, ban: true);
                        GUILayout.EndHorizontal();
                    }
                }
                else GUILayout.Label(Loc.T("net.role", "Your role: {0}", Loc.T("net.role." + Net.MyRole, Net.MyRole)), label);
                GUILayout.Label("<size=11>" + Loc.T("net.chatHelp", "Chat: {0} opens the message line, Enter sends.", Controls.Get("chat")) + "</size>", label);
                chatScroll = GUILayout.BeginScrollView(chatScroll, GUILayout.Height(110));
                foreach (var (from, text, _) in Net.Chat) GUILayout.Label($"<b>{from}</b>: {text}", chatStyle);
                GUILayout.EndScrollView();
                if (GUILayout.Button(Net.IsHost ? Loc.T("net.closeServer", "Close the server (everyone is disconnected)") : Loc.T("net.leave", "Leave the game"), UiTheme.DangerBtn))
                { Online.ForgetJoin(); if (Net.IsHost) Online.Unpublish(); Net.Disconnect(); }
            }
            if (GUILayout.Button(Loc.T("net.close", "Close (Esc)"))) Close();
            GUILayout.EndArea();
        }

        string listingStatus;

        /// <summary>Host: list this world on the online service (friends / everyone / invite only), or stop listing it.</summary>
        void OnlineListing()
        {
            if (!Online.SignedIn) { GUILayout.Label("<size=11>" + Loc.T("net.signInToList", "Sign in (title screen, Account) to list this world online and invite friends.") + "</size>", label); return; }
            if (Online.HostedSession != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("<size=12>" + Loc.T("net.listed", "Listed online: {0}", Loc.T("online.vis." + Online.HostedVisibility, Online.HostedVisibility)) + "</size>", label);
                if (GUILayout.Button(Loc.T("net.unlist", "Stop listing"), GUILayout.Width(120))) Online.Unpublish();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                foreach (var v in new[] { "friends", "public", "private" })
                    if (GUILayout.Button(Loc.T("net.listAs", "List: {0}", Loc.T("online.vis." + v, v)))) ListAs(v);
                GUILayout.EndHorizontal();
            }
            if (listingStatus != null) GUILayout.Label("<size=11>" + listingStatus + "</size>", label);
        }

        async void ListAs(string visibility)
        {
            if (Online.Client.DisplayName != null) Net.MyName = Online.Client.DisplayName;
            listingStatus = Loc.T("net.checking", "checking…");
            listingStatus = await Online.Publish(visibility);
        }

        void Remember() { try { PlayerPrefs.SetString("cb.netName", Net.MyName); PlayerPrefs.SetString("cb.netAddress", address); PlayerPrefs.Save(); } catch { } }
        void OnDestroy() { foreach (var g in ghosts.Values) if (g != null) Destroy(g.gameObject); }
    }
}
