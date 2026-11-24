using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Codebound.Core.Online
{
    /// <summary>How long a remembered sign-in counts while the service cannot be reached. Local play never depends on it.</summary>
    public static class OfflinePolicy
    {
        public const long MaxOfflineSeconds = 30L * 86400, ClockSkew = 300;
        public static bool CachedSignInValid(long lastOnline, long now) => lastOnline > 0 && now - lastOnline <= MaxOfflineSeconds && now >= lastOnline - ClockSkew;
    }

    /// <summary>A piece of play time and what happened in it, sent once (the id makes retries safe).</summary>
    public sealed class StatBatch
    {
        public string Id, Account;
        public long Start, End;
        public Dictionary<string, long> Deltas = new Dictionary<string, long>();

        public Dictionary<string, object> ToJson()
        {
            var d = new Dictionary<string, object>();
            foreach (var kv in Deltas) d[kv.Key] = (double)kv.Value;
            return new Dictionary<string, object> { { "batchId", Id }, { "periodStart", (double)Start }, { "periodEnd", (double)End }, { "deltas", d } };
        }

        internal Dictionary<string, object> Save() { var j = ToJson(); if (Account != null) j["account"] = Account; return j; }

        internal static StatBatch Load(Dictionary<string, object> j)
        {
            var b = new StatBatch { Id = (string)j["batchId"], Start = (long)(double)j["periodStart"], End = (long)(double)j["periodEnd"], Account = j.TryGetValue("account", out var a) ? a as string : null };
            foreach (var kv in (Dictionary<string, object>)j["deltas"]) b.Deltas[kv.Key] = (long)(double)kv.Value;
            return b;
        }
    }

    /// <summary>
    /// Statistics kept on this PC, whether or not anyone is signed in. Play is cut into batches (at most 10 minutes each)
    /// that wait in a queue until the service can take them. Totals here are the local view; the account's numbers are
    /// whatever the service accepted (it caps each batch by the time it covers, so local edits cannot inflate them).
    /// </summary>
    public sealed class StatsJournal
    {
        public const long BatchSeconds = 600;
        public const int MaxPending = 2000;           // ~2 weeks of non-stop play; older batches are dropped from the queue, never from the totals
        readonly ITextStore store;
        readonly Func<long> now;
        public readonly Dictionary<string, long> Totals = new Dictionary<string, long>();
        public readonly List<StatBatch> Pending = new List<StatBatch>();
        StatBatch current;

        public StatsJournal(ITextStore store, Func<long> now)
        {
            this.store = store; this.now = now;
            try
            {
                if (store?.Load() is string s && s.Length > 0 && Json.Parse(s) is Dictionary<string, object> d)
                {
                    foreach (var kv in (Dictionary<string, object>)d["totals"]) Totals[kv.Key] = (long)(double)kv.Value;
                    foreach (var o in (List<object>)d["pending"]) Pending.Add(StatBatch.Load((Dictionary<string, object>)o));
                    if (d.TryGetValue("current", out var c) && c is Dictionary<string, object> cd) current = StatBatch.Load(cd);
                }
            }
            catch (Exception) { Totals.Clear(); Pending.Clear(); current = null; }   // a damaged journal starts over; saves are not touched
        }

        public void Add(string key, long amount, string account)
        {
            if (amount <= 0) return;
            long t = now();
            if (current != null && (current.Account != account || t - current.Start >= BatchSeconds)) Seal();
            if (current == null) current = new StatBatch { Id = Guid.NewGuid().ToString("N"), Start = t, End = t, Account = account };
            current.Deltas[key] = (current.Deltas.TryGetValue(key, out var v) ? v : 0) + amount;
            current.End = t;
            Totals[key] = (Totals.TryGetValue(key, out var tv) ? tv : 0) + amount;
        }

        /// <summary>Closes the batch being filled so it can be sent.</summary>
        public void Seal()
        {
            if (current == null) return;
            current.End = Math.Max(current.End, current.Start + 1);
            if (current.Deltas.TryGetValue("playtime_seconds", out var p)) current.End = Math.Max(current.End, current.Start + p);
            Pending.Add(current); current = null;
            if (Pending.Count > MaxPending) Pending.RemoveRange(0, Pending.Count - MaxPending);
            Save();
        }

        public void Save()
        {
            var tot = new Dictionary<string, object>();
            foreach (var kv in Totals) tot[kv.Key] = (double)kv.Value;
            var d = new Dictionary<string, object> { { "totals", tot }, { "pending", Pending.Select(b => (object)b.Save()).ToList() } };
            if (current != null) d["current"] = current.Save();
            store?.Save(Json.Write(d));
        }

        /// <summary>
        /// Sends the queue, oldest first. Batches played without an account go to whoever signs in on this PC; batches
        /// of another account wait for it. Stops at the first network failure; a batch the service refuses for good is
        /// dropped from the queue (the local totals keep it).
        /// </summary>
        public async Task<(int sent, int refused, bool stoppedOffline)> Sync(OnlineClient client)
        {
            int sent = 0, refused = 0;
            if (!client.SignedIn) return (0, 0, false);
            foreach (var b in Pending.ToList())
            {
                if (b.Account != null && b.Account != client.PublicId) continue;
                var r = await client.SyncStats(b).ConfigureAwait(false);
                if (r.Offline || r.Status == 401 || r.Status == 429) { Save(); return (sent, refused, r.Offline); }
                if (r.Error == "stats_off") break;                       // the player withdrew consent: keep them here, send nothing
                if (r.Ok) sent++; else refused++;
                Pending.Remove(b);
            }
            Save();
            return (sent, refused, false);
        }
    }

    public enum SaveSyncState { UpToDate, LocalOnly, CloudOnly, LocalNewer, CloudNewer, Conflict }

    /// <summary>Which cloud version a local save last matched.</summary>
    public sealed class SaveLink { public int Version; public string Sha; }

    public sealed class SaveStatus
    {
        public string Slot, LocalPath, LocalSha, CloudSha, CloudDevice;
        public int CloudVersion;
        public long LocalTime, CloudTime;
        public SaveSyncState State;
    }

    /// <summary>
    /// Cloud copies of local save files. Nothing here ever deletes a local save: downloading over a local file first
    /// copies it to saves/backups, and a conflict is only ever settled by the player choosing local, cloud, or both.
    /// </summary>
    public sealed class CloudSaves
    {
        readonly OnlineClient client;
        readonly string dir, device;
        readonly Func<long> now;
        public const string Extension = ".cbsave";

        public CloudSaves(OnlineClient client, string savesDir, string device, Func<long> now) { this.client = client; dir = savesDir; this.device = device; this.now = now; }

        public static string Sha(byte[] data) { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(data)).Replace("-", ""); }

        /// <summary>The decision for one save, from what each side has and what they last agreed on.</summary>
        public static SaveSyncState Decide(string localSha, SaveLink link, int cloudVersion, string cloudSha)
        {
            if (localSha == null) return cloudSha == null ? SaveSyncState.UpToDate : SaveSyncState.CloudOnly;
            if (cloudSha == null) return SaveSyncState.LocalOnly;
            if (string.Equals(localSha, cloudSha, StringComparison.OrdinalIgnoreCase)) return SaveSyncState.UpToDate;
            bool localChanged = link == null || !string.Equals(localSha, link.Sha, StringComparison.OrdinalIgnoreCase);
            bool cloudChanged = link == null || cloudVersion != link.Version;
            if (localChanged && cloudChanged) return SaveSyncState.Conflict;   // never guess: the player chooses
            return localChanged ? SaveSyncState.LocalNewer : cloudChanged ? SaveSyncState.CloudNewer : SaveSyncState.UpToDate;
        }

        string LinksPath => Path.Combine(dir, "cloud-links.json");

        public Dictionary<string, SaveLink> Links()
        {
            var r = new Dictionary<string, SaveLink>();
            try
            {
                if (File.Exists(LinksPath) && Json.Parse(File.ReadAllText(LinksPath)) is Dictionary<string, object> d)
                    foreach (var kv in d)
                        if (kv.Value is Dictionary<string, object> l) r[kv.Key] = new SaveLink { Version = (int)(double)l["version"], Sha = (string)l["sha"] };
            }
            catch (Exception) { }   // no links = every difference is asked about, which is the safe side
            return r;
        }

        void SetLink(string slot, int version, string sha)
        {
            var links = Links(); links[slot] = new SaveLink { Version = version, Sha = sha };
            var d = new Dictionary<string, object>();
            foreach (var kv in links) d[kv.Key] = new Dictionary<string, object> { { "version", (double)kv.Value.Version }, { "sha", kv.Value.Sha } };
            Directory.CreateDirectory(dir);
            File.WriteAllText(LinksPath, Json.Write(d));
        }

        public static string SlotOf(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            var chars = name.Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '-').ToArray();
            var s = new string(chars);
            return s.Length == 0 ? null : s.Length > 32 ? s.Substring(0, 32) : s;
        }

        string PathOf(string slot) => Path.Combine(dir, slot + Extension);

        /// <summary>Every local and cloud save with its state. Offline result → null.</summary>
        public async Task<List<SaveStatus>> Check()
        {
            var r = await client.CloudSaves().ConfigureAwait(false);
            if (!r.Ok) return null;
            var links = Links();
            var all = new Dictionary<string, SaveStatus>();
            if (Directory.Exists(dir))
                foreach (var f in Directory.GetFiles(dir, "*" + Extension))
                    if (SlotOf(f) is string slot)
                        all[slot] = new SaveStatus { Slot = slot, LocalPath = f, LocalSha = Sha(File.ReadAllBytes(f)), LocalTime = new DateTimeOffset(File.GetLastWriteTimeUtc(f)).ToUnixTimeSeconds() };
            foreach (var o in r.List ?? new List<object>())
                if (o is Dictionary<string, object> m && m["slot"] is string slot)
                {
                    if (!all.TryGetValue(slot, out var st)) all[slot] = st = new SaveStatus { Slot = slot };
                    st.CloudVersion = (int)(double)m["version"]; st.CloudSha = m["sha256"] as string;
                    st.CloudTime = m.TryGetValue("serverTime", out var t) && t is double td ? (long)td : 0; st.CloudDevice = m["device"] as string;
                }
            foreach (var st in all.Values) st.State = Decide(st.LocalSha, links.TryGetValue(st.Slot, out var l) ? l : null, st.CloudVersion, st.CloudSha);
            return all.Values.OrderBy(s => s.Slot).ToList();
        }

        /// <summary>Sends the local file. keepLocalOverCloud = the player chose this copy in a conflict (it becomes the next version; the older cloud versions stay recoverable).</summary>
        public async Task<ApiResult> Upload(SaveStatus st, bool keepLocalOverCloud = false)
        {
            if (st.LocalPath == null) return new ApiResult { Error = "no_local", Message = "There is no local copy." };
            var data = File.ReadAllBytes(st.LocalPath);
            var links = Links();
            int baseVersion = keepLocalOverCloud ? st.CloudVersion : links.TryGetValue(st.Slot, out var l) ? l.Version : 0;
            var sha = Sha(data);
            var r = await client.UploadSave(st.Slot, baseVersion, sha, now(), device, data).ConfigureAwait(false);
            if (r.Ok) SetLink(st.Slot, (int)(double)r.Obj["version"], sha);
            return r;
        }

        /// <summary>
        /// Brings the cloud copy down. asCopy = keep both (written as "&lt;slot&gt;-cloud.cbsave"); otherwise the local file is
        /// first copied to saves/backups and then replaced. The data is checked against its hash before anything is written.
        /// </summary>
        public async Task<ApiResult> Download(SaveStatus st, bool asCopy)
        {
            var r = await client.CloudSave(st.Slot).ConfigureAwait(false);
            if (!r.Ok) return r;
            byte[] data;
            try { data = Convert.FromBase64String((string)r.Obj["data"]); } catch (Exception) { return new ApiResult { Error = "integrity", Message = "The cloud copy could not be read." }; }
            var meta = (Dictionary<string, object>)r.Obj["meta"];
            var sha = Sha(data);
            if (!string.Equals(sha, meta["sha256"] as string, StringComparison.OrdinalIgnoreCase)) return new ApiResult { Error = "integrity", Message = "The cloud copy was damaged on the way. Nothing was changed." };
            Directory.CreateDirectory(dir);
            string target = asCopy ? NextFree(st.Slot + "-cloud") : PathOf(st.Slot);
            if (!asCopy && File.Exists(target)) BackUp(target);
            string tmp = target + ".part";
            File.WriteAllBytes(tmp, data);
            if (File.Exists(target)) File.Delete(target);   // already backed up above
            File.Move(tmp, target);
            if (!asCopy) SetLink(st.Slot, (int)(double)meta["version"], sha);
            return new ApiResult { Ok = true, Status = 200, Message = target };
        }

        public string BackUp(string path)
        {
            var b = Path.Combine(dir, "backups");
            Directory.CreateDirectory(b);
            var dest = Path.Combine(b, Path.GetFileNameWithoutExtension(path) + "-" + now() + Extension);
            File.Copy(path, dest, true);
            return dest;
        }

        string NextFree(string stem)
        {
            for (int i = 0; ; i++)
            {
                var p = PathOf(i == 0 ? stem : stem + "-" + i);
                if (!File.Exists(p)) return p;
            }
        }
    }
}
