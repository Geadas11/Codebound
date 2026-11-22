// Multiplayer guest checks against a real host on 127.0.0.1:47820 (world password "sesame"; a computer at <pcX> <pcY>
// with password "hunter2", next to the host player). Speaks the same encrypted protocol as the game.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Codebound.Core;

static class Program
{
    static bool allOk = true;
    static void Check(string name, bool cond, object info = null)
    {
        Console.WriteLine((cond ? "ok    " : "FAIL  ") + name + (cond ? "" : "  -> " + (info is Dictionary<string, object> d ? Json.Write(d) : info)?.ToString()?.Substring(0, Math.Min(400, ((info is Dictionary<string, object> d2 ? Json.Write(d2) : info)?.ToString() ?? "").Length))));
        allOk &= cond;
    }

    sealed class Conn : IDisposable
    {
        readonly TcpClient c; readonly StreamReader r; readonly Stream s;
        public SecureChannel Chan;
        public readonly List<string> RawSeen = new List<string>();
        public Conn() { c = new TcpClient("127.0.0.1", 47820); s = c.GetStream(); r = new StreamReader(s, new UTF8Encoding(false)); }
        public void Raw(string line) { var b = Encoding.UTF8.GetBytes(line + "\n"); s.Write(b, 0, b.Length); }
        public void Send(Dictionary<string, object> m) => Raw(Chan.Seal(Json.Write(m)));
        public Dictionary<string, object> Recv(Func<Dictionary<string, object>, bool> until, double seconds = 10)
        {
            var end = DateTime.UtcNow.AddSeconds(seconds);
            c.ReceiveTimeout = 200;
            while (DateTime.UtcNow < end)
            {
                string line;
                try { line = r.ReadLine(); } catch (IOException) { continue; }
                if (line == null) return null;
                RawSeen.Add(line);
                var m = (Dictionary<string, object>)Json.Parse(line.StartsWith("{") ? line : Chan.Open(line));
                if (until(m)) return m;
            }
            return null;
        }
        /// <summary>Handshake + hello; returns the first message after it (a snapshot, or a kick).</summary>
        public Dictionary<string, object> Join(string password, string name = "ana")
        {
            var hs = Recv(m => (string)m["t"] == "hs");
            var hostPub = Convert.FromBase64String((string)hs["pub"]);
            Chan = new SecureChannel();
            Chan.Complete(hostPub, password, Convert.FromBase64String((string)hs["salt"]), false, hostPub, Chan.Public);
            Raw(Json.Write(new Dictionary<string, object> { { "t", "hs" }, { "v", (double)SecureChannel.Version }, { "pub", Convert.ToBase64String(Chan.Public) } }));
            Send(new Dictionary<string, object> { { "t", "hello" }, { "name", name }, { "v", 3.0 } });
            return Recv(m => (string)m["t"] == "snap" || (string)m["t"] == "kick");
        }
        public void Dispose() => c.Close();
    }

    static string Str(Dictionary<string, object> m, string k) => m != null && m.TryGetValue(k, out var v) ? v as string : null;
    static bool LogHas(Dictionary<string, object> m, string text) { if (m?["log"] is List<object> l) foreach (var x in l) if (x is string s && s.Contains(text)) return true; return false; }

    /// <summary>"travel" mode: join, then wait until the host's ship flies to another world; the guest must arrive with it.</summary>
    static int Travel()
    {
        using (var c = new Conn())
        {
            var first = c.Join("");
            Check("joined", Str(first, "t") == "snap", first);
            Console.WriteLine("joined; waiting for the host to travel");
            var moved = c.Recv(m => Str(m, "t") == "snap" && ((string)m["save"]).Contains("\nplanet volcanic"), 120);
            Check("the guest follows the host to the volcanic world", moved != null);
            if (moved != null)
            {
                var you = (Dictionary<string, object>)moved["you"];
                var host = (List<object>)((List<object>)moved["players"])[0];
                double dx = (double)you["x"] - (double)host[1], dy = (double)you["y"] - (double)host[2];
                Check("and lands next to the host", Math.Sqrt(dx * dx + dy * dy) < 3, $"{dx:0.0},{dy:0.0}");
            }
        }
        Console.WriteLine(allOk ? "TRAVEL CHECK PASSED" : "TRAVEL CHECK FAILED");
        return allOk ? 0 : 1;
    }

    static int Main(string[] args)
    {
        if (args[0] == "travel") return Travel();
        int px = int.Parse(args[0]), py = int.Parse(args[1]);
        string pcLine = $"b computer {px} {py} ";
        string Block(string save) { int i = save.IndexOf(pcLine); if (i < 0) return ""; int j = save.IndexOf("\nb ", i + 1); return j < 0 ? save.Substring(i) : save.Substring(i, j - i); }

        using (var c = new Conn())
        {
            var m = c.Join("nope");
            Check("wrong world password is refused", Str(m, "t") == "kick", m);
        }

        var conn = new Conn();
        var snap = conn.Join("sesame");
        Check("right password: encrypted join, world received", Str(snap, "t") == "snap", snap);
        bool hidden = true;
        foreach (var line in conn.RawSeen) if (!line.StartsWith("{\"t\":\"hs\"") && (line.Contains("computer") || line.Contains("snap"))) hidden = false;
        Check("nothing but the handshake travels in the clear", hidden);
        string block = Block((string)snap["save"]);
        Check("locked computer arrives without code or hash", block.Contains("bx pwd locked") && !block.Contains("bx code"), block);
        var you = (Dictionary<string, object>)snap["you"];
        double x = (double)you["x"], y = (double)you["y"];

        conn.Send(new Dictionary<string, object> { { "t", "pos" }, { "x", x + 0.3 }, { "y", y } });
        Check("a normal step is accepted", conn.Recv(mm => Str(mm, "t") == "setpos", 1.5) == null);
        System.Threading.Thread.Sleep(600);
        conn.Send(new Dictionary<string, object> { { "t", "pos" }, { "x", x + 60 }, { "y", y } });
        var sp = conn.Recv(mm => Str(mm, "t") == "setpos", 3);
        Check("a teleport is corrected by the host", sp != null && Math.Abs((double)sp["x"] - (x + 0.3)) < 0.01, sp);

        conn.Send(new Dictionary<string, object> { { "t", "chat" }, { "text", "<b>hi</b>\u0007" } });
        var ch = conn.Recv(mm => Str(mm, "t") == "chat", 3);
        Check("chat is broadcast and cleaned", Str(ch, "from") == "ana" && Str(ch, "text") == "‹b›hi‹/b›", ch);

        var prog = new Dictionary<string, object> { { "t", "program" }, { "x", (double)px }, { "y", (double)py }, { "lang", "javascript" }, { "code", "log('guest code ' + (6 * 7));" },
                                                    { "files", new Dictionary<string, object>() }, { "watches", new List<object>() }, { "op", "run" } };
        conn.Send(prog);
        var msg = conn.Recv(mm => Str(mm, "t") == "msg", 3);
        Check("locked computer refuses a guest program", Str(msg, "text")?.Contains("locked") == true, msg);
        conn.Send(new Dictionary<string, object> { { "t", "unlock" }, { "x", (double)px }, { "y", (double)py }, { "password", "wrong" } });
        var u = conn.Recv(mm => Str(mm, "t") == "unlocked", 3);
        Check("wrong computer password", u != null && u["ok"] is false, u);
        System.Threading.Thread.Sleep(1100);
        conn.Send(new Dictionary<string, object> { { "t", "unlock" }, { "x", (double)px }, { "y", (double)py }, { "password", "hunter2" } });
        u = conn.Recv(mm => Str(mm, "t") == "unlocked", 3);
        Check("right computer password unlocks", u != null && u["ok"] is true, u);
        var s2 = conn.Recv(mm => Str(mm, "t") == "snap" && Block((string)mm["save"]).Contains("bx code"), 3);
        Check("after unlocking, the snapshot carries the code (never the hash)", s2 != null && !((string)s2["save"]).Contains("pbkdf2"));

        conn.Send(new Dictionary<string, object> { { "t", "view" }, { "x", (double)px }, { "y", (double)py } });
        conn.Send(prog);
        Check("guest program runs on the host, console reaches the guest", conn.Recv(mm => Str(mm, "t") == "pc" && LogHas(mm, "guest code 42"), 15) != null);
        conn.Send(new Dictionary<string, object>(prog) { ["op"] = "stop" });
        System.Threading.Thread.Sleep(1000);
        conn.Send(new Dictionary<string, object>(prog) { ["op"] = "snippet", ["arg"] = "log('snip ' + (2 + 3));" });
        Check("terminal snippet from a guest", conn.Recv(mm => Str(mm, "t") == "pc" && LogHas(mm, "snip 5"), 15) != null);

        conn.Send(new Dictionary<string, object> { { "t", "vit" }, { "line", "vitals 33 44 55 66 37" } });
        System.Threading.Thread.Sleep(500);
        conn.Dispose();
        System.Threading.Thread.Sleep(1500);
        using (var c = new Conn())
        {
            var again = c.Join("sesame");
            var y2 = again?["you"] as Dictionary<string, object>;
            Check("rejoin restores vitals and position", Str(y2, "vit")?.StartsWith("vitals 33 44 55 66") == true, again);
        }
        Console.WriteLine(allOk ? "ALL MULTIPLAYER HOST CHECKS PASSED" : "SOME CHECKS FAILED");
        return allOk ? 0 : 1;
    }
}
