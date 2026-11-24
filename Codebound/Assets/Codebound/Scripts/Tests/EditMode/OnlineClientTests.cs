using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Codebound.Core;
using Codebound.Core.Online;
using NUnit.Framework;

namespace Codebound.Tests
{
    /// <summary>The game's online client against a scripted service: tokens, offline behaviour, stats queue, cloud saves.</summary>
    public class OnlineClientTests
    {
        sealed class Mem : ITextStore { public string Text = ""; public string Load() => Text; public void Save(string t) => Text = t; }

        /// <summary>A fake service: routes answer by "METHOD path"; Down = no network at all.</summary>
        sealed class Fake : IHttpTransport
        {
            public bool Down;
            public readonly List<string> Calls = new List<string>();
            public readonly Dictionary<string, Func<Dictionary<string, object>, string, (int, string)>> Routes = new Dictionary<string, Func<Dictionary<string, object>, string, (int, string)>>();
            public Task<(int status, string body)> Send(string method, string path, string json, string bearer)
            {
                Calls.Add(method + " " + path);
                if (Down) throw new HttpRequestException("no route to host");
                string key = method + " " + path.Split('?')[0];
                if (!Routes.TryGetValue(key, out var f)) return Task.FromResult((404, "{\"error\":\"not_found\",\"message\":\"?\"}"));
                return Task.FromResult(f(json == null ? null : (Dictionary<string, object>)Json.Parse(json), bearer));
            }
        }

        long clock = 1_800_000_000;
        int issued;

        static string J(object o) => Json.Write(o);
        static Dictionary<string, object> D(params object[] kv) { var d = new Dictionary<string, object>(); for (int i = 0; i < kv.Length; i += 2) d[(string)kv[i]] = kv[i + 1]; return d; }

        (int, string) Tokens() { issued++; return (200, J(D("accessToken", "acc" + issued, "refreshToken", "ref" + issued, "accessExpiresIn", 900.0, "account", D("publicId", "CB-AAAA1111", "displayName", "Ana", "email", "ana@example.test")))); }

        Fake Service()
        {
            var f = new Fake();
            f.Routes["POST auth/login"] = (b, _) => (string)b["password"] == "correct horse battery" ? Tokens() : (401, J(D("error", "bad_credentials", "message", "Wrong email or password.")));
            f.Routes["POST auth/refresh"] = (b, _) => (string)b["refreshToken"] == "ref" + issued ? Tokens() : (401, J(D("error", "invalid_token", "message", "Sign in again.")));
            f.Routes["GET me"] = (_, bearer) => bearer == "acc" + issued ? (200, J(D("publicId", "CB-AAAA1111"))) : (401, J(D("error", "token_expired", "message", "x")));
            f.Routes["POST auth/logout"] = (_, __) => (200, "{}");
            return f;
        }

        OnlineClient Client(Fake f, Mem store) => new OnlineClient(f, store, () => clock);

        T Run<T>(Task<T> t) => t.GetAwaiter().GetResult();

        [Test]
        public void Login_KeepsOnlyTheRefreshToken_NeverThePassword()
        {
            var f = Service(); var store = new Mem(); var c = Client(f, store);
            Assert.AreEqual("bad_credentials", Run(c.Login("ana@example.test", "wrong password!", "pc")).Error);
            Assert.IsFalse(c.SignedIn);
            Assert.IsTrue(Run(c.Login("ana@example.test", "correct horse battery", "pc")).Ok);
            Assert.AreEqual(Connection.Online, c.State);
            StringAssert.DoesNotContain("correct horse", store.Text);
            StringAssert.DoesNotContain("acc1", store.Text, "the short-lived access token stays in memory");
            StringAssert.Contains("ref1", store.Text);
            var again = Client(f, store);   // the game restarts
            Assert.IsTrue(again.SignedIn); Assert.AreEqual("Ana", again.DisplayName);
            Assert.AreEqual(Connection.Offline, again.State, "remembered, but not confirmed until the service answers");
            Assert.IsTrue(Run(again.Me()).Ok);
            Assert.AreEqual(Connection.Online, again.State);
        }

        [Test]
        public void ExpiredAccess_IsRenewedOnce_AndARevokedSignInSignsOut()
        {
            var f = Service(); var c = Client(f, new Mem());
            Run(c.Login("ana@example.test", "correct horse battery", "pc"));
            clock += 1000;   // past the 15 minutes
            Assert.IsTrue(Run(c.Me()).Ok);
            CollectionAssert.Contains(f.Calls, "POST auth/refresh");
            issued += 10;    // the service no longer knows our tokens (revoked on another device)
            var r = Run(c.Me());
            Assert.AreEqual(401, r.Status);
            Assert.IsFalse(c.SignedIn);
            Assert.AreEqual(Connection.SignedOut, c.State);
        }

        [Test]
        public void NoNetwork_IsOffline_NotAnError_AndKeepsTheSignIn()
        {
            var f = Service(); var c = Client(f, new Mem());
            Run(c.Login("ana@example.test", "correct horse battery", "pc"));
            f.Down = true; clock += 1000;
            var r = Run(c.Me());
            Assert.IsTrue(r.Offline); Assert.IsFalse(r.Ok);
            Assert.IsTrue(c.SignedIn, "losing the internet must not sign anyone out");
            Assert.AreEqual(Connection.Offline, c.State);
            Assert.IsTrue(Run(c.Register("x@example.test", "whatever123", "Xavi")).Offline);
            f.Down = false;
            Assert.IsTrue(Run(c.Me()).Ok);
            Assert.AreEqual(Connection.Online, c.State);
            f.Routes["GET me"] = (_, __) => (503, "");
            Assert.IsTrue(Run(c.Me()).Offline, "a broken service counts as unavailable");
            Assert.IsTrue(c.SignedIn);
        }

        [Test]
        public void Logout_WorksOffline_AndADamagedStoreIsJustSignedOut()
        {
            var f = Service(); var store = new Mem(); var c = Client(f, store);
            Run(c.Login("ana@example.test", "correct horse battery", "pc"));
            f.Down = true;
            Run(c.Logout());
            Assert.IsFalse(c.SignedIn);
            Assert.AreEqual("", store.Text);
            Assert.IsFalse(Client(f, new Mem { Text = "{not json" }).SignedIn);
        }

        [Test]
        public void CachedSignIn_HasALimitOffline()
        {
            long t = 1_800_000_000;
            Assert.IsTrue(OfflinePolicy.CachedSignInValid(t, t + 29L * 86400));
            Assert.IsFalse(OfflinePolicy.CachedSignInValid(t, t + 31L * 86400));
            Assert.IsFalse(OfflinePolicy.CachedSignInValid(0, t));
            Assert.IsFalse(OfflinePolicy.CachedSignInValid(t, t - 86400), "a clock turned back does not extend it");
        }

        [Test]
        public void StatsJournal_QueuesOffline_SyncsLater_AndRetriesAreSafe()
        {
            var f = Service(); var c = Client(f, new Mem());
            Run(c.Login("ana@example.test", "correct horse battery", "pc"));
            var seen = new HashSet<string>(); int accepted = 0;
            f.Routes["POST stats/sync"] = (b, _) => { if (seen.Add((string)b["batchId"])) accepted++; return (200, J(D("accepted", D()))); };
            var store = new Mem(); var j = new StatsJournal(store, () => clock);
            for (int i = 0; i < 25; i++) { j.Add("playtime_seconds", 60, "CB-AAAA1111"); clock += 60; }   // 25 minutes
            j.Add("lang:python", 1, null);   // played after signing out: a new batch
            j.Seal();
            Assert.AreEqual(4, j.Pending.Count);
            Assert.AreEqual(1500, j.Totals["playtime_seconds"]);
            j = new StatsJournal(store, () => clock);   // restart: the queue survived
            Assert.AreEqual(4, j.Pending.Count);
            f.Down = true;
            var off = Run(j.Sync(c));
            Assert.IsTrue(off.stoppedOffline); Assert.AreEqual(4, j.Pending.Count, "nothing lost while offline");
            f.Down = false;
            var on = Run(j.Sync(c));
            Assert.AreEqual(4, on.sent); Assert.AreEqual(0, j.Pending.Count); Assert.AreEqual(4, accepted);
            Assert.AreEqual(1500, j.Totals["playtime_seconds"], "local totals stay");
        }

        [Test]
        public void StatsJournal_KeepsOtherAccountsBatches_AndDropsRefusedOnes()
        {
            var f = Service(); var c = Client(f, new Mem());
            Run(c.Login("ana@example.test", "correct horse battery", "pc"));
            f.Routes["POST stats/sync"] = (b, _) => ((Dictionary<string, object>)b["deltas"]).ContainsKey("distance") ? (400, J(D("error", "bad_period", "message", "x"))) : (200, "{}");
            var j = new StatsJournal(new Mem(), () => clock);
            j.Add("playtime_seconds", 5, "CB-OTHER999"); j.Seal();
            j.Add("distance", 5, "CB-AAAA1111"); j.Seal();
            j.Add("playtime_seconds", 5, "CB-AAAA1111"); j.Seal();
            var r = Run(j.Sync(c));
            Assert.AreEqual(1, r.sent); Assert.AreEqual(1, r.refused);
            Assert.AreEqual(1, j.Pending.Count); Assert.AreEqual("CB-OTHER999", j.Pending[0].Account);
            f.Routes["POST stats/sync"] = (_, __) => (403, J(D("error", "stats_off", "message", "x")));
            j.Add("playtime_seconds", 5, null); j.Seal();
            Run(j.Sync(c));
            Assert.AreEqual(2, j.Pending.Count, "consent withdrawn: kept here, not sent");
        }

        [Test]
        public void SaveDecisions_NeverGuessInAConflict()
        {
            var link = new SaveLink { Version = 3, Sha = "AA" };
            Assert.AreEqual(SaveSyncState.LocalOnly, CloudSaves.Decide("AA", null, 0, null));
            Assert.AreEqual(SaveSyncState.CloudOnly, CloudSaves.Decide(null, null, 1, "BB"));
            Assert.AreEqual(SaveSyncState.UpToDate, CloudSaves.Decide("AA", link, 3, "AA"));
            Assert.AreEqual(SaveSyncState.LocalNewer, CloudSaves.Decide("CC", link, 3, "AA"));
            Assert.AreEqual(SaveSyncState.CloudNewer, CloudSaves.Decide("AA", link, 4, "DD"));
            Assert.AreEqual(SaveSyncState.Conflict, CloudSaves.Decide("CC", link, 4, "DD"));
            Assert.AreEqual(SaveSyncState.Conflict, CloudSaves.Decide("CC", null, 1, "DD"), "never synced on this PC and both differ");
        }

        [Test]
        public void CloudSaves_DownloadNeverLosesTheLocalSave()
        {
            string dir = Path.Combine(Path.GetTempPath(), "cb-cloud-" + Guid.NewGuid().ToString("N"));
            try
            {
                var f = Service(); var c = Client(f, new Mem());
                Run(c.Login("ana@example.test", "correct horse battery", "pc"));
                var cloud = System.Text.Encoding.UTF8.GetBytes("cloud world");
                string cloudSha = CloudSaves.Sha(cloud), sent = null;
                f.Routes["GET saves"] = (_, __) => (200, J(new List<object> { D("slot", "quicksave", "version", 2.0, "sha256", cloudSha, "serverTime", 5.0, "device", "laptop") }));
                f.Routes["GET saves/quicksave"] = (_, __) => (200, J(D("meta", D("slot", "quicksave", "version", 2.0, "sha256", cloudSha), "data", Convert.ToBase64String(cloud))));
                f.Routes["PUT saves/quicksave"] = (b, _) => { sent = (string)b["data"]; return (200, J(D("version", 3.0))); };
                Directory.CreateDirectory(dir);
                string local = Path.Combine(dir, "quicksave.cbsave");
                File.WriteAllText(local, "local world");
                var cs = new CloudSaves(c, dir, "pc", () => clock);
                var st = Run(cs.Check()).Find(s => s.Slot == "quicksave");
                Assert.AreEqual(SaveSyncState.Conflict, st.State);

                Assert.IsTrue(Run(cs.Download(st, asCopy: true)).Ok);   // keep both
                Assert.AreEqual("local world", File.ReadAllText(local));
                Assert.AreEqual("cloud world", File.ReadAllText(Path.Combine(dir, "quicksave-cloud.cbsave")));

                Assert.IsTrue(Run(cs.Download(st, asCopy: false)).Ok);  // keep the cloud one: the local one goes to backups first
                Assert.AreEqual("cloud world", File.ReadAllText(local));
                var backups = Directory.GetFiles(Path.Combine(dir, "backups"));
                Assert.AreEqual(1, backups.Length); Assert.AreEqual("local world", File.ReadAllText(backups[0]));
                Assert.AreEqual(SaveSyncState.UpToDate, Run(cs.Check()).Find(s => s.Slot == "quicksave").State);

                File.WriteAllText(local, "played more");
                st = Run(cs.Check()).Find(s => s.Slot == "quicksave");
                Assert.AreEqual(SaveSyncState.LocalNewer, st.State);
                Assert.IsTrue(Run(cs.Upload(st)).Ok);
                Assert.AreEqual("played more", System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(sent)));

                f.Routes["GET saves/quicksave"] = (_, __) => (200, J(D("meta", D("slot", "quicksave", "version", 4.0, "sha256", "00"), "data", Convert.ToBase64String(cloud))));
                Assert.AreEqual("integrity", Run(cs.Download(st, asCopy: false)).Error);
                Assert.AreEqual("played more", File.ReadAllText(local), "a damaged download changes nothing");

                f.Down = true;
                Assert.IsNull(Run(cs.Check()), "offline: no decisions, nothing touched");
            }
            finally { try { Directory.Delete(dir, true); } catch (IOException) { } }
        }
    }
}
