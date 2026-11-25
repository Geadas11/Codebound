using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Codebound.Core
{
    /// <summary>What the worlds screen knows about one save file, read from its header and its sidecar (never invented).</summary>
    public sealed class WorldInfo
    {
        public string Id, Path, Name, Planet = "home", Profile, Biome, GameVersion, Status = "ok", Error;
        public int Seed, Version;
        public long Tick, Created, LastPlayed, Size;
        public bool HasMeta;
        /// <summary>Simulated time in the world (20 ticks per second): the play time that actually happened in it.</summary>
        public double PlaySeconds => Tick / 20.0;
        public bool Loadable => Status == "ok";
    }

    /// <summary>
    /// A world is one save file `saves/&lt;id&gt;.cbsave` (so quicksaves, autosaves and cloud copies are worlds too), with an
    /// optional `&lt;id&gt;.meta.json` (name, dates, profile, biome) and `&lt;id&gt;.png` (map preview). Other planets visited from
    /// it live in `saves/planets/&lt;id&gt;/`. Deleting moves everything to `saves/deleted/` — nothing is destroyed.
    /// </summary>
    public static class WorldStore
    {
        public const string Ext = ".cbsave";
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string FileOf(string dir, string id) => System.IO.Path.Combine(dir, id + Ext);
        public static string MetaOf(string dir, string id) => System.IO.Path.Combine(dir, id + ".meta.json");
        public static string PreviewOf(string dir, string id) => System.IO.Path.Combine(dir, id + ".png");
        public static string PlanetsOf(string dir, string id) => System.IO.Path.Combine(dir, "planets", id);

        /// <summary>Every world in `dir` (planet files of the old layout are not worlds of their own).</summary>
        public static List<WorldInfo> List(string dir)
        {
            var list = new List<WorldInfo>();
            if (!Directory.Exists(dir)) return list;
            foreach (var f in Directory.GetFiles(dir, "*" + Ext))
            {
                string id = System.IO.Path.GetFileNameWithoutExtension(f);
                if (id.StartsWith("planet_", StringComparison.Ordinal)) continue;
                list.Add(Inspect(dir, id));
            }
            return list;
        }

        /// <summary>Reads the header lines (version, planet, seed, tick) and the sidecar. A broken file is reported, not hidden.</summary>
        public static WorldInfo Inspect(string dir, string id)
        {
            string path = FileOf(dir, id);
            var w = new WorldInfo { Id = id, Path = path, Name = id };
            try
            {
                var fi = new FileInfo(path);
                w.Size = fi.Length;
                w.LastPlayed = new DateTimeOffset(fi.LastWriteTimeUtc).ToUnixTimeSeconds();
                w.Created = new DateTimeOffset(fi.CreationTimeUtc).ToUnixTimeSeconds();
                using (var r = new StreamReader(path))
                {
                    string head = r.ReadLine();
                    if (head == null || !head.StartsWith("CODEBOUND ", StringComparison.Ordinal) || !int.TryParse(head.Substring(10), NumberStyles.Integer, Inv, out w.Version))
                    { w.Status = "damaged"; w.Error = "not a Codebound save"; }
                    else
                    {
                        if (w.Version > SaveSystem.Version) w.Status = "newer";
                        bool seed = false, tick = false;
                        for (int i = 0; i < 8 && !(seed && tick); i++)
                        {
                            string line = r.ReadLine();
                            if (line == null) break;
                            var p = line.Split(' ');
                            if (p[0] == "planet" && p.Length > 1) w.Planet = p[1];
                            else if (p[0] == "seed" && p.Length > 1) seed = int.TryParse(p[1], NumberStyles.Integer, Inv, out w.Seed);
                            else if (p[0] == "tick" && p.Length > 1) tick = long.TryParse(p[1], NumberStyles.Integer, Inv, out w.Tick);
                        }
                        if (!seed && w.Status == "ok") { w.Status = "damaged"; w.Error = "no seed in the header"; }
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { w.Status = "damaged"; w.Error = e.Message; }
            ReadMeta(dir, w);
            return w;
        }

        static void ReadMeta(string dir, WorldInfo w)
        {
            string mp = MetaOf(dir, w.Id);
            if (!File.Exists(mp)) return;
            try
            {
                if (!(Json.Parse(File.ReadAllText(mp)) is Dictionary<string, object> m)) return;
                w.HasMeta = true;
                if (m.TryGetValue("name", out var n) && n is string ns && ns.Trim().Length > 0) w.Name = ns.Trim();
                if (m.TryGetValue("created", out var c) && c is double cd) w.Created = (long)cd;
                if (m.TryGetValue("lastPlayed", out var l) && l is double ld) w.LastPlayed = (long)ld;
                if (m.TryGetValue("profile", out var pr) && pr is string ps) w.Profile = ps;
                if (m.TryGetValue("biome", out var b) && b is string bs) w.Biome = bs;
                if (m.TryGetValue("gameVersion", out var gv) && gv is string gs) w.GameVersion = gs;
            }
            catch (Exception e) when (e is IOException || e is FormatException || e is UnauthorizedAccessException) { }   // a bad sidecar only loses the nice name
        }

        /// <summary>Writes (or updates) the sidecar; fields left null keep their stored value.</summary>
        public static void WriteMeta(string dir, string id, string name = null, long? created = null, long? lastPlayed = null, string profile = null, string biome = null, string gameVersion = null)
        {
            var m = new Dictionary<string, object>();
            string mp = MetaOf(dir, id);
            try { if (File.Exists(mp) && Json.Parse(File.ReadAllText(mp)) is Dictionary<string, object> old) m = old; }
            catch (FormatException) { }
            if (name != null) m["name"] = CleanName(name);
            if (created != null) m["created"] = (double)created.Value;
            if (lastPlayed != null) m["lastPlayed"] = (double)lastPlayed.Value;
            if (profile != null) m["profile"] = profile;
            if (biome != null) m["biome"] = biome;
            if (gameVersion != null) m["gameVersion"] = gameVersion;
            Directory.CreateDirectory(dir);
            File.WriteAllText(mp, Json.Write(m), new UTF8Encoding(false));
        }

        public const int MaxName = 40;

        /// <summary>A display name: trimmed, control characters removed, at most 40 characters.</summary>
        public static string CleanName(string name)
        {
            var sb = new StringBuilder();
            foreach (char ch in name ?? "") if (!char.IsControl(ch)) sb.Append(ch);
            var s = sb.ToString().Trim();
            return s.Length > MaxName ? s.Substring(0, MaxName).Trim() : s;
        }

        /// <summary>A file id from a display name: lower-case ASCII letters, digits and '-', unique in `dir`.</summary>
        public static string NewId(string dir, string name)
        {
            var sb = new StringBuilder();
            foreach (char ch in (name ?? "").ToLowerInvariant())
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9')) sb.Append(ch);
                else if (sb.Length > 0 && sb[sb.Length - 1] != '-') sb.Append('-');
                if (sb.Length >= 32) break;
            }
            string b = sb.ToString().Trim('-');
            if (b.Length == 0 || b == "quicksave" || b == "autosave" || b.StartsWith("planet", StringComparison.Ordinal)) b = "world" + (b.Length > 0 ? "-" + b : "");
            string id = b;
            for (int i = 2; File.Exists(FileOf(dir, id)) || File.Exists(MetaOf(dir, id)) || Directory.Exists(PlanetsOf(dir, id)); i++) id = b + "-" + i;
            return id;
        }

        /// <summary>Renaming only changes the display name: the file keeps its id (cloud links and backups stay valid).</summary>
        public static void Rename(string dir, string id, string newName)
        {
            if (CleanName(newName).Length == 0) throw new ArgumentException("empty name");
            WriteMeta(dir, id, name: newName);
        }

        /// <summary>Copies the save, its sidecar, preview and visited planets to a new id. Returns the new id.</summary>
        public static string Duplicate(string dir, string id, string newName, long now)
        {
            string src = FileOf(dir, id);
            if (!File.Exists(src)) throw new FileNotFoundException("no such world", src);
            string nid = NewId(dir, newName);
            File.Copy(src, FileOf(dir, nid));
            if (File.Exists(MetaOf(dir, id))) File.Copy(MetaOf(dir, id), MetaOf(dir, nid));
            if (File.Exists(PreviewOf(dir, id))) File.Copy(PreviewOf(dir, id), PreviewOf(dir, nid));
            if (Directory.Exists(PlanetsOf(dir, id)))
            {
                Directory.CreateDirectory(PlanetsOf(dir, nid));
                foreach (var f in Directory.GetFiles(PlanetsOf(dir, id))) File.Copy(f, System.IO.Path.Combine(PlanetsOf(dir, nid), System.IO.Path.GetFileName(f)));
            }
            WriteMeta(dir, nid, name: newName, created: now);
            return nid;
        }

        /// <summary>Moves the world (save, .bak, sidecar, preview, planets) to `saves/deleted/&lt;id&gt;-&lt;time&gt;/`. Returns that folder.</summary>
        public static string Delete(string dir, string id, long now)
        {
            string dest = System.IO.Path.Combine(dir, "deleted", id + "-" + now.ToString(Inv));
            Directory.CreateDirectory(dest);
            foreach (var f in new[] { FileOf(dir, id), FileOf(dir, id) + ".bak", MetaOf(dir, id), PreviewOf(dir, id) })
                if (File.Exists(f)) File.Move(f, System.IO.Path.Combine(dest, System.IO.Path.GetFileName(f)));
            if (Directory.Exists(PlanetsOf(dir, id))) Directory.Move(PlanetsOf(dir, id), System.IO.Path.Combine(dest, "planets"));
            return dest;
        }

        public enum Sort { Recent, Name, PlayTime, Created }

        /// <summary>Search (name, id or seed) and sort for the worlds list. Damaged files stay listed, at the end.</summary>
        public static List<WorldInfo> Filter(IEnumerable<WorldInfo> all, string query, Sort sort, string profile = null)
        {
            var list = new List<WorldInfo>();
            string q = (query ?? "").Trim();
            foreach (var w in all)
            {
                if (profile != null && w.Profile != null && w.Profile != profile) continue;
                if (q.Length > 0 && w.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0 && w.Id.IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0
                    && w.Seed.ToString(Inv) != q) continue;
                list.Add(w);
            }
            list.Sort((a, b) =>
            {
                if (a.Loadable != b.Loadable) return a.Loadable ? -1 : 1;
                switch (sort)
                {
                    case Sort.Name: return string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
                    case Sort.PlayTime: return b.Tick.CompareTo(a.Tick);
                    case Sort.Created: return b.Created.CompareTo(a.Created);
                    default: return b.LastPlayed.CompareTo(a.LastPlayed);
                }
            });
            return list;
        }

        /// <summary>The world "Continue" opens: the most recently played one that can be loaded.</summary>
        public static WorldInfo Latest(IEnumerable<WorldInfo> all)
        {
            WorldInfo best = null;
            foreach (var w in all) if (w.Loadable && (best == null || w.LastPlayed > best.LastPlayed)) best = w;
            return best;
        }
    }

    /// <summary>A local player profile: a name (also the multiplayer name) and an avatar. No password: it is not an account.</summary>
    public sealed class Profile
    {
        public string Id, Name;
        public int Avatar;
        public long Created, LastUsed;
    }

    /// <summary>Local profiles in one JSON file. Online accounts (Core/Online) are separate and optional.</summary>
    public sealed class ProfileStore
    {
        public const int Avatars = 12;
        readonly string path;
        public readonly List<Profile> All = new List<Profile>();
        public string SelectedId { get; private set; }

        public ProfileStore(string path) { this.path = path; Load(); }

        public Profile Selected { get { foreach (var p in All) if (p.Id == SelectedId) return p; return null; } }

        void Load()
        {
            All.Clear(); SelectedId = null;
            try
            {
                if (!File.Exists(path) || !(Json.Parse(File.ReadAllText(path)) is Dictionary<string, object> root)) return;
                if (root.TryGetValue("profiles", out var ps) && ps is List<object> list)
                    foreach (var o in list)
                        if (o is Dictionary<string, object> d && d.TryGetValue("id", out var id) && id is string ids && d.TryGetValue("name", out var n) && n is string ns && Multiplayer.ValidName(ns))
                            All.Add(new Profile
                            {
                                Id = ids, Name = ns,
                                Avatar = d.TryGetValue("avatar", out var a) && a is double ad ? Math.Max(0, Math.Min(Avatars - 1, (int)ad)) : 0,
                                Created = d.TryGetValue("created", out var c) && c is double cd ? (long)cd : 0,
                                LastUsed = d.TryGetValue("lastUsed", out var l) && l is double ld ? (long)ld : 0,
                            });
                if (root.TryGetValue("selected", out var s) && s is string ss && All.Exists(p => p.Id == ss)) SelectedId = ss;
            }
            catch (Exception e) when (e is IOException || e is FormatException || e is UnauthorizedAccessException)
            {
                All.Clear();   // a broken file: start over, keeping a copy of the old one
                try { File.Copy(path, path + ".bad", true); } catch (Exception) { }
            }
        }

        public void Save()
        {
            var list = new List<object>();
            foreach (var p in All)
                list.Add(new Dictionary<string, object> { { "id", p.Id }, { "name", p.Name }, { "avatar", (double)p.Avatar }, { "created", (double)p.Created }, { "lastUsed", (double)p.LastUsed } });
            var root = new Dictionary<string, object> { { "profiles", list } };
            if (SelectedId != null) root["selected"] = SelectedId;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, Json.Write(root), new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(tmp, path, null); else File.Move(tmp, path);
        }

        /// <summary>Null on success, otherwise why not ("name" invalid, "taken").</summary>
        public string Validate(string name, string exceptId = null)
        {
            if (!Multiplayer.ValidName(name)) return "name";
            foreach (var p in All) if (p.Id != exceptId && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return "taken";
            return null;
        }

        public Profile Create(string name, int avatar, long now)
        {
            name = (name ?? "").Trim();
            if (Validate(name) != null) return null;
            string id;
            int i = All.Count + 1;
            do id = "p" + now.ToString(CultureInfo.InvariantCulture) + "-" + i++; while (All.Exists(p => p.Id == id));
            var pr = new Profile { Id = id, Name = name, Avatar = Math.Max(0, Math.Min(Avatars - 1, avatar)), Created = now, LastUsed = now };
            All.Add(pr);
            SelectedId = id;
            Save();
            return pr;
        }

        public bool Rename(string id, string name)
        {
            name = (name ?? "").Trim();
            var p = All.Find(x => x.Id == id);
            if (p == null || Validate(name, id) != null) return false;
            p.Name = name; Save(); return true;
        }

        public void SetAvatar(string id, int avatar) { var p = All.Find(x => x.Id == id); if (p == null) return; p.Avatar = Math.Max(0, Math.Min(Avatars - 1, avatar)); Save(); }

        public void Select(string id, long now) { var p = All.Find(x => x.Id == id); if (p == null) return; SelectedId = id; p.LastUsed = now; Save(); }

        /// <summary>Signs out of the profile (the entry screen shows again); the profile itself stays.</summary>
        public void SignOut() { SelectedId = null; Save(); }

        /// <summary>Removes the profile only. Worlds it created stay on disk and in the worlds list.</summary>
        public void Delete(string id) { All.RemoveAll(p => p.Id == id); if (SelectedId == id) SelectedId = null; Save(); }
    }
}
