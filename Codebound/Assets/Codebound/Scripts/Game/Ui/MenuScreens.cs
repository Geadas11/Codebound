using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>Front-end state shared by the screens: local profiles and the saves folder.</summary>
    public static class FrontEnd
    {
        static ProfileStore profiles;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => profiles = null;

        public static ProfileStore Profiles => profiles ??= new ProfileStore(Path.Combine(Application.persistentDataPath, "profiles.json"));
        public static Profile Player => Profiles.Selected;

        /// <summary>Plays as this profile: remembered for the next launch, and its name is the multiplayer name.</summary>
        public static void Use(Profile p)
        {
            Profiles.Select(p.Id, Online.Now());
            Net.MyName = p.Name;
            try { PlayerPrefs.SetString("cb.netName", p.Name); PlayerPrefs.Save(); } catch { }
        }

        public static string Version => Loc.T("ui.version", "Version {0} · save format {1}", Application.version, SaveSystem.Version);

        public static string BiomeName(string biome) => biome == null ? "—" : Loc.T("biome." + biome, biome);

        /// <summary>Previews by world id: the stored PNG, or one made from the save in the background (then stored).</summary>
        static readonly Dictionary<string, Texture2D> previews = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Task<Color32[]>> making = new Dictionary<string, Task<Color32[]>>();

        public static Texture2D Preview(WorldInfo w)
        {
            if (previews.TryGetValue(w.Id + w.LastPlayed, out var t) && t != null) return t;
            string png = WorldStore.PreviewOf(GameBootstrap.SavesDir, w.Id);
            if (File.Exists(png))
            {
                t = new Texture2D(2, 2) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
                if (t.LoadImage(File.ReadAllBytes(png))) { t.filterMode = FilterMode.Point; return previews[w.Id + w.LastPlayed] = t; }
            }
            if (!w.Loadable) return null;
            if (!making.TryGetValue(w.Id, out var job)) making[w.Id] = job = WorldPreview.FromSave(w.Path);
            if (!job.IsCompleted) return null;
            making.Remove(w.Id);
            if (job.IsFaulted) { previews[w.Id + w.LastPlayed] = null; return null; }
            t = WorldPreview.ToTexture(job.Result);
            try { File.WriteAllBytes(png, t.EncodeToPNG()); } catch (IOException) { }
            return previews[w.Id + w.LastPlayed] = t;
        }

        public static void ForgetPreviews() { previews.Clear(); making.Clear(); }
    }

    // ===================================================================== entry: profiles
    /// <summary>The dedicated entry screen: choose, create or edit a local profile, then play. Online accounts are optional.</summary>
    public sealed class ProfilesScreen : UiScreen
    {
        readonly bool root;
        Profile editing;          // null: picking; editing.Id == null: creating
        string name = "";
        int avatar;
        string error;

        public ProfilesScreen(bool root) { this.root = root; }
        public override bool IsRoot => root;
        public override string Title => Loc.T("ui.profiles", "Profiles");

        public override void OnShow() { if (FrontEnd.Profiles.All.Count == 0) StartCreate(); }

        void StartCreate() { editing = new Profile(); name = ""; avatar = UnityEngine.Random.Range(0, ProfileStore.Avatars); error = null; }

        public override bool OnBack()
        {
            if (editing != null && FrontEnd.Profiles.All.Count > 0) { editing = null; return false; }
            return !root || FrontEnd.Player != null;
        }

        public override void Draw(Rect r)
        {
            MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, 0.35f);
            float w = Mathf.Min(640, r.width - 48), h = Mathf.Min(editing != null ? 440 : 120 + 80 * Mathf.Min(4, FrontEnd.Profiles.All.Count), r.height - 200);
            var box = new Rect(r.center.x - w / 2, r.center.y - (h + 160) / 2 + 110, w, h);
            GUI.Label(new Rect(box.x, box.y - 110, w, 70), "CODEBOUND", new GUIStyle(UiTheme.Title) { alignment = TextAnchor.MiddleCenter });
            GUI.Label(new Rect(box.x, box.y - 46, w, 26), (editing == null ? Loc.T("ui.whoPlays", "Who is playing?") : editing.Id == null ? Loc.T("ui.newProfile", "New profile") : Loc.T("ui.editProfile", "Edit profile")).ToUpperInvariant(),
                      new GUIStyle(UiTheme.H2) { alignment = TextAnchor.MiddleCenter });
            GUILayout.BeginArea(box, UiTheme.Panel);
            if (editing == null) Pick(); else Edit();
            GUILayout.EndArea();
            GUI.Label(new Rect(box.x, box.yMax + 10, w, 40), Loc.T("ui.profileNote", "Profiles live on this computer and need no password. An online account (optional) adds friends and online games."),
                      new GUIStyle(UiTheme.Small) { alignment = TextAnchor.UpperCenter });
        }

        Vector2 scroll;

        void Pick()
        {
            var ps = FrontEnd.Profiles;
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var p in new List<Profile>(ps.All))
            {
                bool sel = p.Id == ps.SelectedId;
                var row = GUILayoutUtility.GetRect(10, 76, GUILayout.ExpandWidth(true));
                if (Ui.Button(row, "", sel ? UiTheme.RowSel : UiTheme.Row)) { FrontEnd.Use(p); Ui.Confirm(); Nav.Reset(new MainScreen()); return; }
                Avatars.Draw(new Rect(row.x + 12, row.y + 10, 56, 56), p.Avatar, sel);
                GUI.Label(new Rect(row.x + 84, row.y + 12, row.width - 300, 30), p.Name, UiTheme.H1);
                GUI.Label(new Rect(row.x + 86, row.y + 44, row.width - 300, 22), Loc.T("ui.lastPlayed", "Last played {0}", Ui.Ago(p.LastUsed)), UiTheme.Small);
                if (Ui.Button(new Rect(row.xMax - 200, row.y + 20, 96, 36), Loc.T("ui.edit", "Edit"), UiTheme.Ghost)) { editing = p; name = p.Name; avatar = p.Avatar; error = null; }
                if (Ui.Button(new Rect(row.xMax - 100, row.y + 20, 90, 36), Loc.T("ui.play", "Play"), UiTheme.Primary)) { FrontEnd.Use(p); Ui.Confirm(); Nav.Reset(new MainScreen()); return; }
            }
            GUILayout.EndScrollView();
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (Ui.Button("+  " + Loc.T("ui.createProfile", "Create profile"), null, ps.All.Count < 16, GUILayout.Height(40), GUILayout.Width(220))) StartCreate();
            GUILayout.FlexibleSpace();
            if (Ui.Button(Loc.T("ui.onlineAccount", "Online account"), UiTheme.Ghost, true, GUILayout.Height(40))) AccountMenu.Open();
            GUILayout.EndHorizontal();
        }

        void Edit()
        {
            var ps = FrontEnd.Profiles;
            GUILayout.Label(Loc.T("ui.avatar", "Avatar").ToUpperInvariant(), UiTheme.Caption);
            var grid = GUILayoutUtility.GetRect(10, 2 * 76, GUILayout.ExpandWidth(true));
            float gx = grid.x + (grid.width - 6 * 84) / 2;
            for (int i = 0; i < ProfileStore.Avatars; i++)
            {
                var cell = new Rect(gx + (i % 6) * 84 + 4, grid.y + (i / 6) * 76, 68, 68);
                Avatars.Draw(cell, i, i == avatar);
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { avatar = i; Ui.Click(); }
            }
            GUILayout.Space(10);
            name = Ui.Field(Loc.T("ui.profileName", "Name (also your multiplayer name)"), name, 16, "pname", false, GUILayout.Height(36));
            GUILayout.Label(Loc.T("ui.nameRule", "1–16 letters, digits, _ or -"), UiTheme.Small);
            if (error != null) GUILayout.Label(error, new GUIStyle(UiTheme.Label) { normal = { textColor = UiTheme.Bad } });
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (editing.Id != null && ps.All.Count > 0 && Ui.Button(Loc.T("ui.deleteProfile", "Delete profile"), UiTheme.DangerBtn, true, GUILayout.Height(40)))
            {
                var target = editing;
                Nav.Ask(Loc.T("ui.deleteProfileQ", "Delete the profile {0}?", target.Name), Loc.T("ui.deleteProfileBody", "Only the profile is removed. Its worlds stay in the worlds list, and an online account is not affected."),
                        Loc.T("ui.delete", "Delete"), () => { ps.Delete(target.Id); editing = null; if (ps.All.Count == 0) StartCreate(); Ui.Toast(Loc.T("ui.profileDeleted", "Profile deleted")); }, true);
            }
            GUILayout.FlexibleSpace();
            if (ps.All.Count > 0 && Ui.Button(Loc.T("ui.cancel", "Cancel"), null, true, GUILayout.Height(40), GUILayout.Width(130))) editing = null;
            if (Ui.Button(editing.Id == null ? Loc.T("ui.createAndPlay", "Create and play") : Loc.T("ui.save", "Save"), UiTheme.Primary, true, GUILayout.Height(40), GUILayout.Width(190)) || (Ui.NavConfirm() && GUI.GetNameOfFocusedControl() == "pname"))
                Commit();
            GUILayout.EndHorizontal();
        }

        void Commit()
        {
            var ps = FrontEnd.Profiles;
            string why = ps.Validate(name.Trim(), editing.Id);
            if (why != null) { error = why == "taken" ? Loc.T("ui.nameTaken", "Another profile already has that name.") : Loc.T("ui.nameRule", "1–16 letters, digits, _ or -"); AudioDirector.Ui("error"); return; }
            if (editing.Id == null)
            {
                var p = ps.Create(name.Trim(), avatar, Online.Now());
                FrontEnd.Use(p); Ui.Confirm();
                Nav.Reset(new MainScreen());
                return;
            }
            ps.Rename(editing.Id, name.Trim()); ps.SetAvatar(editing.Id, avatar);
            if (editing.Id == ps.SelectedId) FrontEnd.Use(editing);
            editing = null; Ui.Toast(Loc.T("ui.saved", "Saved"), "good");
        }
    }

    // ===================================================================== main menu
    public sealed class MainScreen : UiScreen
    {
        public override string Title => "Codebound";
        public override bool IsRoot => true;
        int sel;
        List<WorldInfo> worlds;
        float refresh;

        public override void OnShow()
        {
            worlds = WorldStore.List(GameBootstrap.SavesDir); refresh = 0;
            var st = MenuStage.Instance;
            if (st != null && MenuStage.Active && !st.AtOverview) st.GoOverview();   // back from the terminal: the camera returns
        }

        WorldInfo Latest => worlds == null ? null : WorldStore.Latest(worlds);

        (string text, string hint, bool enabled, Action go, int badge)[] Items()
        {
            var last = Latest;
            return new (string, string, bool, Action, int)[]
            {
                (Loc.T("title.continue", "Continue"), last != null ? last.Name + " · " + Ui.Ago(last.LastPlayed) : Loc.T("ui.noWorlds", "No saved world yet"), last != null, () => TitleMenu.Play(Nav, last), 0),
                (Loc.T("ui.singleplayer", "Singleplayer"), Loc.T("ui.singleHint", "New world or load one"), true, () => Nav.Push(new SingleplayerScreen()), 0),
                (Loc.T("ui.multiplayer", "Multiplayer"), Loc.T("ui.multiHint", "Host a world or join a game"), true, () => Nav.Push(new MultiplayerScreen()), Online.Invites.Count),
                (Loc.T("ui.worlds", "Maps / Saves"), Loc.T("ui.worldsHint", "{0} world(s) · rename, copy, delete", worlds?.Count ?? 0), true, () => Nav.Push(new WorldsScreen()), 0),
                (Loc.T("ui.options", "Options"), Loc.T("ui.optionsHint", "Graphics, audio, controls, accessibility"), true, () => Nav.Push(new OptionsScreen(false)), 0),
                (Loc.T("ui.credits", "Credits"), Loc.T("ui.creditsHint", "Who made what"), true, () => Nav.Push(new CreditsScreen()), 0),
                (Loc.T("ui.quit", "Quit"), "", true, () => Nav.Ask(Loc.T("ui.quitQ", "Quit Codebound?"), null, Loc.T("menu.quit", "Quit game"), Application.Quit), 0),
            };
        }

        const int QuitIndex = 6;

        /// <summary>Runs an entry: every entry except Quit first flies the camera to the terminal, then opens on its screen.</summary>
        void Activate(int i, Action go)
        {
            var st = MenuStage.Instance;
            if (i == QuitIndex || st == null || !MenuStage.Active) { go(); return; }
            if (st.Moving || travelling >= 0) return;
            travelling = i;
            st.GoTerminal(() => { travelling = -1; go(); });
        }

        int travelling = -1;
        bool Busy => travelling >= 0 || (MenuStage.Instance != null && MenuStage.Active && !MenuStage.Instance.AtOverview);

        public override void Update()
        {
            if ((refresh += Ui.Dt) > 3) { refresh = 0; worlds = WorldStore.List(GameBootstrap.SavesDir); }
            if (Busy) return;
            var items = Items();
            int d = Ui.NavVertical();
            if (d != 0) { for (int k = 0; k < items.Length; k++) { sel = (sel + d + items.Length) % items.Length; if (items[sel].enabled) break; } Ui.Click(); }
            if (Ui.NavConfirm() && items[sel].enabled) { Ui.Confirm(); Activate(sel, items[sel].go); }
        }

        static readonly MainMenuArt.Icon[] Icons =
        {
            MainMenuArt.Icon.Play, MainMenuArt.Icon.Person, MainMenuArt.Icon.People, MainMenuArt.Icon.Map,
            MainMenuArt.Icon.Gear, MainMenuArt.Icon.Scroll, MainMenuArt.Icon.Power,
        };
        readonly float[] hot = new float[7];
        float cardHover, linkA, linkB;
        bool keyboard;   // the highlighted entry follows the keyboard / gamepad until the mouse moves

        public override void Draw(Rect r)
        {
            if (!MainMenuArt.Ready) { DrawPlain(r); return; }   // art files missing: the plain themed menu still works
            var e = Event.current;
            var st = MenuStage.Instance;
            bool stage = st != null && MenuStage.Active;
            if (!stage) MainMenuArt.Background(r);   // without the 3D stage: the still image
            float away = stage ? Mathf.Clamp01(st.Progress * 1.8f) : 0;   // the console slides out as the camera leaves
            if (away >= 0.999f) return;
            if (e.type == EventType.MouseMove) keyboard = false;
            if (Ui.NavVertical() != 0) keyboard = true;
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, 1 - away);
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && !Busy;
            float shift = -away * 120;

            // the console slab
            float panelW = Mathf.Clamp(r.width * 0.29f, 480, 620);
            MainMenuArt.Slab(new Rect(r.x - 4 + shift, r.y, panelW, r.height));
            float x = r.x + 30 + shift;
            float scale = Mathf.Clamp01((r.height - 640) / 440f);

            // emblem + wordmark on the top plate
            float em = Mathf.Lerp(78, 104, scale), top = r.y + Mathf.Lerp(26, 40, scale);
            MainMenuArt.Emblem(new Rect(x + 4, top, em, em));
            float ww = panelW - em - 78;
            float wh = MainMenuArt.WordmarkHeight(ww);
            MainMenuArt.Wordmark(new Rect(x + em + 22, top + (em - wh) / 2 - 10, ww, 0));
            MainMenuArt.Text(new Rect(x + em + 24, top + (em + wh) / 2 - 2, ww + 40, 20), Loc.T("title.tagline", "Build. Code. Survive. Explore.").ToUpperInvariant().Replace(" ", "  "), MainMenuArt.Styles.Caption, UiTheme.Muted);
            float plateBottom = top + em + Mathf.Lerp(26, 40, scale);   // the seam under the logo plate
            Ui.Fill(new Rect(r.x + shift, plateBottom, panelW - 24, 3), new Color(0.03f, 0.035f, 0.04f, 0.95f));
            Ui.Fill(new Rect(r.x + shift, plateBottom + 3, panelW - 24, 1), new Color(0.35f, 0.38f, 0.42f, 0.6f));
            MainMenuArt.Vent(new Rect(x + 6, plateBottom - 20, 120, 12));
            MainMenuArt.Hazard(new Rect(x + panelW - 200, plateBottom - 18, 110, 8));

            // the buttons
            var items = Items();
            if (!items[sel].enabled) sel = 1;
            float bw = panelW - 80, bh = Mathf.Lerp(50, 60, scale), gap = Mathf.Lerp(8, 12, scale), ph = Mathf.Lerp(58, 70, scale);
            float stack = ph + (items.Length - 1) * (bh + gap) + Mathf.Lerp(10, 22, scale);
            float y = plateBottom + Mathf.Max(Mathf.Lerp(18, 34, scale), (r.yMax - 80 - plateBottom - stack) * 0.42f);   // the stack sits in the free height
            int hover = -1;
            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                if (i == QuitIndex) y += Mathf.Lerp(10, 22, scale);
                float h = i == 0 ? ph : bh;
                var row = new Rect(x + 4, y, bw, h);
                if (it.enabled && row.Contains(e.mousePosition) && GUI.enabled) hover = i;
                bool selected = (keyboard && i == sel) || travelling == i;
                string sub = i == 0 ? it.hint : (selected || hover == i ? it.hint : "");
                if (MainMenuArt.ConsoleButton(row, Icons[i], it.text.ToUpperInvariant(), sub, i == 0, it.enabled, ref hot[i], selected, it.badge))
                { sel = i; Activate(i, it.go); }
                y += h + gap;
            }
            if (hover >= 0 && !keyboard) sel = hover;
            MainMenuArt.Vent(new Rect(x + 6, r.yMax - 64, 120, 12));
            MainMenuArt.Text(new Rect(x + 8, r.yMax - 44, panelW - 60, 22), FrontEnd.Version, MainMenuArt.Styles.Small, UiTheme.Dim);

            // right side: profile and the last session (real data)
            float sideW = Mathf.Min(440, r.width * 0.25f), sx = r.xMax - sideW - 36 - shift;
            ProfileTag(new Rect(sx, r.y - 2, sideW, 112));
            var last = Latest;
            if (last != null && r.width >= 1200) LastWorld(new Rect(sx, r.yMax - 230, sideW, 172), last, () => Activate(0, items[0].go));
            var hs = MainMenuArt.Styles.Small; var al = hs.alignment; hs.alignment = TextAnchor.MiddleRight;
            MainMenuArt.Text(new Rect(r.xMax - 760, r.yMax - 40, 720, 30), Loc.T("ui.navHelp", "↑ ↓ choose · Enter confirm · Esc back"), hs, UiTheme.Dim);
            hs.alignment = al;
            GUI.enabled = wasEnabled;
            GUI.color = old;
        }

        /// <summary>Profile hanging from the top edge: avatar, name, online state and the two profile actions.</summary>
        void ProfileTag(Rect r)
        {
            var p = FrontEnd.Player;
            MainMenuArt.Plate(r);
            if (p != null) Avatars.Draw(new Rect(r.x + 34, r.y + 22, 58, 58), p.Avatar, true);
            MainMenuArt.Text(new Rect(r.x + 108, r.y + 18, r.width - 120, 30), p?.Name ?? "—", MainMenuArt.Styles.Title, UiTheme.Text);
            bool on = Online.OnlineNow;
            MainMenuArt.Node(new Vector2(r.x + 113, r.y + 58), on, on ? 0.6f : 0);
            MainMenuArt.Text(new Rect(r.x + 126, r.y + 48, r.width - 140, 20), Online.StatusLine(), MainMenuArt.Styles.Small, UiTheme.Muted);
            if (MainMenuArt.Link(new Rect(r.x + 108, r.y + 76, 140, 24), Loc.T("ui.switchProfile", "Switch profile").ToUpperInvariant(), ref linkA)) Activate(99, () => Nav.Push(new ProfilesScreen(false)));
            if (MainMenuArt.Link(new Rect(r.x + 260, r.y + 76, 170, 24), Loc.T("ui.onlineAccount", "Online account").ToUpperInvariant(), ref linkB)) AccountMenu.Open();
            int pending = Online.FriendRequests + Online.Invites.Count;
            if (pending > 0) Ui.Badge(new Rect(r.xMax - 46, r.y + 20, 30, 22), pending.ToString());
        }

        /// <summary>The world "Continue" opens, with its real map, on an asymmetric plate. Clicking it continues.</summary>
        void LastWorld(Rect r, WorldInfo w, Action go)
        {
            var e = Event.current;
            bool inside = r.Contains(e.mousePosition) && GUI.enabled;
            cardHover = Mathf.MoveTowards(cardHover, inside ? 1 : 0, Ui.Dt * 7f);
            MainMenuArt.Plate(r);
            float th = r.height - 44;
            var pr = new Rect(r.x + 26, r.y + 24, th, th);
            Ui.Fill(new Rect(pr.x - 2, pr.y - 2, pr.width + 4, pr.height + 4), Color.Lerp(UiTheme.Hex(0x3A434E), UiTheme.Accent, cardHover));
            var tex = FrontEnd.Preview(w);
            if (tex != null) GUI.DrawTexture(pr, tex); else Ui.Spinner(pr);
            MainMenuArt.Hazard(new Rect(pr.x, pr.yMax + 6, pr.width, 5));
            float tx = pr.xMax + 18, tw = r.xMax - tx - 14;
            MainMenuArt.Text(new Rect(tx, r.y + 18, tw, 18), Loc.T("ui.lastSession", "Last session").ToUpperInvariant(), MainMenuArt.Styles.Caption, UiTheme.Accent);
            MainMenuArt.Text(new Rect(tx, r.y + 38, tw, 30), w.Name, MainMenuArt.Styles.Title, UiTheme.Text);
            MainMenuArt.Text(new Rect(tx, r.y + 70, tw, 18), Loc.T("ui.seed", "Seed") + "  " + w.Seed + "   ·   " + FrontEnd.BiomeName(w.Biome), MainMenuArt.Styles.Hint, UiTheme.Muted);
            MainMenuArt.Text(new Rect(tx, r.y + 88, tw, 18), Ui.Duration(w.PlaySeconds) + "   ·   " + Ui.Ago(w.LastPlayed), MainMenuArt.Styles.Hint, UiTheme.Muted);
            var cc = Color.Lerp(UiTheme.Text, UiTheme.AccentHi, cardHover);
            if (e.type == EventType.Repaint)
            {
                var old = GUI.color; GUI.color = cc * old;
                GUI.DrawTexture(new Rect(tx + 4 * cardHover, r.yMax - 46, 20, 20), MainMenuArt.IconTex(MainMenuArt.Icon.Play));
                GUI.color = old;
            }
            MainMenuArt.Text(new Rect(tx + 30 + 4 * cardHover, r.yMax - 50, tw - 30, 28), Loc.T("title.continue", "Continue").ToUpperInvariant(), MainMenuArt.Styles.Title, cc);
            if (e.type == EventType.MouseUp && e.button == 0 && inside) { e.Use(); Ui.Confirm(); go(); }
        }

        /// <summary>The previous themed layout, used only if the menu art cannot be loaded.</summary>
        void DrawPlain(Rect r)
        {
            MenuBackdrop.Draw(r);
            float x = r.x + Mathf.Max(40, r.width * 0.07f), y = r.y + Mathf.Max(40, r.height * 0.12f);
            // logo
            Ui.Fill(new Rect(x, y + 10, 8, 62), UiTheme.Accent);
            GUI.Label(new Rect(x + 24, y, 700, 80), "CODEBOUND", UiTheme.Title);
            GUI.Label(new Rect(x + 28, y + 76, 700, 24), Loc.T("title.tagline", "Build. Code. Survive. Explore.").ToUpperInvariant(), new GUIStyle(UiTheme.Caption) { fontSize = UiGui.Font(14), normal = { textColor = UiTheme.Muted } });

            var items = Items();
            if (!items[sel].enabled) sel = 1;
            float iy = y + 140, iw = Mathf.Min(440, r.width - x - 40), ih = 58;
            for (int i = 0; i < items.Length; i++)
            {
                var it = items[i];
                var row = new Rect(x, iy + i * (ih + 8) + (i == items.Length - 1 ? 14 : 0), iw, ih);
                bool hover = row.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseMove && it.enabled) sel = i;
                var style = i == sel && it.enabled ? new GUIStyle(UiTheme.MenuItem) { normal = UiTheme.MenuItem.hover } : UiTheme.MenuItem;
                if (Ui.Button(row, "", style, it.enabled)) { sel = i; Ui.Confirm(); it.go(); }
                var c = it.enabled ? (i == sel ? Color.white : UiTheme.Text) : UiTheme.Dim;
                GUI.Label(new Rect(row.x + 24, it.hint.Length > 0 ? row.y + 6 : row.y + 13, row.width - 40, 30), it.text, new GUIStyle(UiTheme.H1) { fontSize = UiGui.Font(21), normal = { textColor = c } });
                if (it.hint.Length > 0) GUI.Label(new Rect(row.x + 25, row.y + 33, row.width - 40, 20), it.hint, UiTheme.MenuHint);
                if (it.badge > 0) Ui.Badge(new Rect(row.xMax - 44, row.y + 17, 30, 22), it.badge.ToString());
            }

            ProfileCard(new Rect(r.xMax - Mathf.Min(420, r.width * 0.32f) - 40, r.y + 40, Mathf.Min(420, r.width * 0.32f), 150));
            GUI.Label(new Rect(x, r.yMax - 82, 600, 22), FrontEnd.Version, UiTheme.Small);
            GUI.Label(new Rect(r.xMax - 640, r.yMax - 82, 600, 22), Loc.T("ui.navHelp", "↑ ↓ choose · Enter confirm · Esc back"), new GUIStyle(UiTheme.Small) { alignment = TextAnchor.UpperRight });
        }

        void ProfileCard(Rect r)
        {
            var p = FrontEnd.Player;
            GUI.Box(r, GUIContent.none, UiTheme.Panel);
            if (p != null) Avatars.Draw(new Rect(r.x + 16, r.y + 16, 64, 64), p.Avatar, true);
            GUI.Label(new Rect(r.x + 96, r.y + 14, r.width - 110, 30), p?.Name ?? "—", UiTheme.H1);
            GUI.Label(new Rect(r.x + 97, r.y + 46, r.width - 110, 20), Loc.T("ui.localProfile", "Local profile").ToUpperInvariant(), UiTheme.Caption);
            var status = Online.StatusLine();
            GUI.Label(new Rect(r.x + 97, r.y + 64, r.width - 110, 20), status, new GUIStyle(UiTheme.Small) { normal = { textColor = Online.OnlineNow ? UiTheme.Good : UiTheme.Muted } });
            float bw = (r.width - 44) / 2;
            if (Ui.Button(new Rect(r.x + 16, r.yMax - 50, bw, 36), Loc.T("ui.switchProfile", "Switch profile"))) Nav.Push(new ProfilesScreen(false));
            int pending = Online.FriendRequests + Online.Invites.Count;
            if (Ui.Button(new Rect(r.x + 28 + bw, r.yMax - 50, bw, 36), Loc.T("ui.onlineAccount", "Online account"))) AccountMenu.Open();
            if (pending > 0) Ui.Badge(new Rect(r.xMax - 40, r.yMax - 60, 30, 22), pending.ToString());
        }
    }

    // ===================================================================== singleplayer hub
    public sealed class SingleplayerScreen : UiScreen
    {
        public override string Title => Loc.T("ui.singleplayer", "Singleplayer");
        List<WorldInfo> recent;
        public override void OnShow() => recent = WorldStore.Filter(WorldStore.List(GameBootstrap.SavesDir), "", WorldStore.Sort.Recent);

        public override void Draw(Rect r)
        {
            MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, 0.45f);
            var c = UiNav.Frame(this, r);
            float cw = Mathf.Min(c.width, 980), x = c.x, half = (cw - 20) / 2;
            if (Card(new Rect(x, c.y, half, 150), Loc.T("ui.newWorld", "New world"), Loc.T("ui.newWorldHint", "Choose a name and a seed, see the map before you start."))) Nav.Push(new NewWorldScreen(false));
            if (Card(new Rect(x + half + 20, c.y, half, 150), Loc.T("ui.loadWorld", "Load world"), Loc.T("ui.loadWorldHint", "All your worlds, with previews, search and sorting."))) Nav.Push(new WorldsScreen());
            GUI.Label(new Rect(x, c.y + 176, cw, 26), Loc.T("ui.recent", "Recent").ToUpperInvariant(), UiTheme.H2);
            float y = c.y + 210;
            int n = 0;
            foreach (var w in recent)
            {
                if (!w.Loadable || n++ >= 4) continue;
                var row = new Rect(x, y, cw, 64);
                if (Ui.Button(row, "", UiTheme.Row)) TitleMenu.Play(Nav, w);
                var tex = FrontEnd.Preview(w);
                if (tex != null) GUI.DrawTexture(new Rect(row.x + 10, row.y + 6, 52, 52), tex);
                GUI.Label(new Rect(row.x + 76, row.y + 8, cw - 300, 26), w.Name, new GUIStyle(UiTheme.H2) { normal = { textColor = UiTheme.Text } });
                GUI.Label(new Rect(row.x + 76, row.y + 34, cw - 300, 22), Loc.T("ui.worldLine", "Seed {0} · {1} · {2}", w.Seed, FrontEnd.BiomeName(w.Biome), Ui.Duration(w.PlaySeconds)), UiTheme.Small);
                GUI.Label(new Rect(row.xMax - 220, row.y + 20, 200, 24), Ui.Ago(w.LastPlayed), new GUIStyle(UiTheme.Small) { alignment = TextAnchor.MiddleRight });
                y += 70;
            }
            if (n == 0) GUI.Label(new Rect(x, y, cw, 26), Loc.T("ui.noWorlds", "No saved world yet"), UiTheme.Muted_);
        }

        static bool Card(Rect r, string title, string hint)
        {
            bool hit = Ui.Button(r, "", UiTheme.MenuItem);
            Ui.Fill(new Rect(r.x + 22, r.y + 26, 40, 3), UiTheme.Accent);
            GUI.Label(new Rect(r.x + 22, r.y + 36, r.width - 44, 40), title, UiTheme.H1);
            GUI.Label(new Rect(r.x + 22, r.y + 80, r.width - 44, 50), hint, UiTheme.Muted_);
            return hit;
        }
    }

    // ===================================================================== worlds
    /// <summary>Every world on disk: preview, seed, biome, dates, play time, version, state; play, rename, duplicate, delete.</summary>
    public sealed class WorldsScreen : UiScreen
    {
        public override string Title => Loc.T("ui.worlds", "Maps / Saves");
        readonly Action<WorldInfo> pick;   // set when choosing a world to host
        readonly string pickLabel;
        List<WorldInfo> all = new List<WorldInfo>(), shown = new List<WorldInfo>();
        string query = "", renaming, newName = "";
        int sort;
        bool mine;
        WorldInfo sel;
        Vector2 scroll;

        public WorldsScreen(Action<WorldInfo> pick = null, string pickLabel = null) { this.pick = pick; this.pickLabel = pickLabel; }

        public override void OnShow() => Reload();

        void Reload()
        {
            all = WorldStore.List(GameBootstrap.SavesDir);
            Refilter();
            if (sel != null) sel = all.Find(w => w.Id == sel.Id);
            if (sel == null && shown.Count > 0) sel = shown[0];
        }

        void Refilter() => shown = WorldStore.Filter(all, query, (WorldStore.Sort)sort, mine ? FrontEnd.Player?.Id : null);

        public override bool OnBack() { if (renaming != null) { renaming = null; return false; } return true; }

        public override void Update()
        {
            int d = Ui.NavVertical();
            if (d != 0 && shown.Count > 0 && renaming == null)
            {
                int i = Mathf.Clamp(shown.IndexOf(sel) + d, 0, shown.Count - 1);
                sel = shown[i]; Ui.Click();
            }
            if (Ui.NavConfirm() && sel != null && renaming == null && GUIUtility.keyboardControl == 0) Go(sel);
        }

        void Go(WorldInfo w)
        {
            if (!w.Loadable) return;
            if (pick != null) { pick(w); Nav.Pop(); return; }
            TitleMenu.Play(Nav, w);
        }

        public override void Draw(Rect r)
        {
            MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, 0.55f);
            var c = UiNav.Frame(this, r, Loc.T("ui.worldsFolder", "Folder: {0}", GameBootstrap.SavesDir));
            float listW = Mathf.Max(420, c.width * 0.58f), detW = c.width - listW - 20;

            // toolbar
            GUILayout.BeginArea(new Rect(c.x, c.y, listW, 92));
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(listW * 0.45f));
            string q = Ui.Field(Loc.T("ui.search", "Search (name or seed)"), query, 40, "wsearch", false, GUILayout.Height(34));
            if (q != query) { query = q; Refilter(); }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label(Loc.T("ui.sort", "Sort").ToUpperInvariant(), UiTheme.Caption);
            var sorts = new[] { Loc.T("ui.sortRecent", "Last played"), Loc.T("ui.sortName", "Name"), Loc.T("ui.sortTime", "Play time"), Loc.T("ui.sortCreated", "Created") };
            if (Ui.Button(sorts[sort], null, true, GUILayout.Height(34))) { sort = (sort + 1) % sorts.Length; Refilter(); }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Space(22);
            bool m = GUILayout.Toggle(mine, " " + Loc.T("ui.onlyMine", "Only this profile"));
            if (m != mine) { mine = m; Ui.Click(); Refilter(); }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // list
            var listR = new Rect(c.x, c.y + 96, listW, c.height - 96 - (pick == null ? 54 : 0));
            GUILayout.BeginArea(listR, UiTheme.Inset);
            scroll = GUILayout.BeginScrollView(scroll);
            if (shown.Count == 0) GUILayout.Label(all.Count == 0 ? Loc.T("ui.noWorlds", "No saved world yet") : Loc.T("ui.noMatch", "No world matches the search."), UiTheme.Muted_);
            foreach (var w in shown) Row(w, listW - 40);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            if (pick == null && Ui.Button(new Rect(c.x, listR.yMax + 12, 240, 42), "+  " + Loc.T("ui.newWorld", "New world"), UiTheme.Primary)) Nav.Push(new NewWorldScreen(false));

            if (sel != null) Details(new Rect(c.x + listW + 20, c.y, detW, c.height), sel);
        }

        void Row(WorldInfo w, float width)
        {
            var row = GUILayoutUtility.GetRect(width, 84);
            bool isSel = sel != null && sel.Id == w.Id;
            if (Ui.Button(row, "", isSel ? UiTheme.RowSel : UiTheme.Row))
            {
                if (isSel && Event.current.clickCount == 2) Go(w);
                sel = w; renaming = null;
            }
            var tex = FrontEnd.Preview(w);
            var pr = new Rect(row.x + 10, row.y + 8, 68, 68);
            Ui.Fill(pr, UiTheme.Line);
            if (tex != null) GUI.DrawTexture(new Rect(pr.x + 1, pr.y + 1, pr.width - 2, pr.height - 2), tex);
            else if (w.Loadable) Ui.Spinner(pr);
            float tx = row.x + 94;
            GUI.Label(new Rect(tx, row.y + 8, row.width - 260, 28), w.Name, new GUIStyle(UiTheme.H2) { fontSize = UiGui.Font(18), normal = { textColor = w.Loadable ? UiTheme.Text : UiTheme.Dim } });
            GUI.Label(new Rect(tx, row.y + 36, row.width - 120, 20), w.Loadable ? Loc.T("ui.worldLine", "Seed {0} · {1} · {2}", w.Seed, FrontEnd.BiomeName(w.Biome), Ui.Duration(w.PlaySeconds)) : StatusText(w), UiTheme.Small);
            GUI.Label(new Rect(tx, row.y + 56, row.width - 120, 20), Loc.T("ui.playedAgo", "Played {0}", Ui.Ago(w.LastPlayed)), UiTheme.Small);
            if (!w.Loadable) Ui.Badge(new Rect(row.xMax - 120, row.y + 10, 108, 22), w.Status == "newer" ? Loc.T("ui.stNewer", "NEWER") : Loc.T("ui.stDamaged", "DAMAGED"));
            else if (w.Id == "quicksave" || w.Id == "autosave") GUI.Label(new Rect(row.xMax - 160, row.y + 10, 150, 22), w.Id.ToUpperInvariant(), new GUIStyle(UiTheme.Caption) { alignment = TextAnchor.MiddleRight });
        }

        static string StatusText(WorldInfo w) => w.Status == "newer" ? Loc.T("ui.newerSave", "Saved by a newer version of the game (format {0}).", w.Version)
                                               : w.Status == "damaged" ? Loc.T("ui.damagedSave", "Cannot be read: {0}", w.Error) : Loc.T("ui.stOk", "Ready");

        void Details(Rect r, WorldInfo w)
        {
            GUILayout.BeginArea(r, UiTheme.Panel);
            float pw = Mathf.Min(r.width - 28, 300);
            var tex = FrontEnd.Preview(w);
            var pr = GUILayoutUtility.GetRect(pw, Mathf.Min(300, r.height * 0.36f), GUILayout.ExpandWidth(true));
            Ui.Fill(pr, UiTheme.Bg0);
            if (tex != null) { float s = Mathf.Min(pr.width, pr.height); GUI.DrawTexture(new Rect(pr.center.x - s / 2, pr.y, s, s), tex); }
            else GUI.Label(pr, w.Loadable ? Loc.T("ui.makingPreview", "Drawing the map…") : "", new GUIStyle(UiTheme.Small) { alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(8);
            if (renaming == w.Id)
            {
                newName = Ui.Field(Loc.T("ui.newName", "New name"), newName, WorldStore.MaxName, "wname", false, GUILayout.Height(34));
                GUILayout.BeginHorizontal();
                if (Ui.Button(Loc.T("ui.cancel", "Cancel"))) renaming = null;
                if (Ui.Button(Loc.T("ui.save", "Save"), UiTheme.Primary, WorldStore.CleanName(newName).Length > 0))
                {
                    try { WorldStore.Rename(GameBootstrap.SavesDir, w.Id, newName); renaming = null; Reload(); Ui.Toast(Loc.T("ui.renamed", "Renamed"), "good"); }
                    catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { Ui.Toast(e.Message, "bad"); }
                }
                GUILayout.EndHorizontal();
            }
            else GUILayout.Label(w.Name, UiTheme.H1);
            Ui.KeyValue(Loc.T("ui.seed", "Seed"), w.Seed.ToString());
            Ui.KeyValue(Loc.T("ui.biome", "Start biome"), FrontEnd.BiomeName(w.Biome));
            if (w.Planet != "home") Ui.KeyValue(Loc.T("ui.planet", "Planet"), w.Planet);
            Ui.KeyValue(Loc.T("ui.playTime", "Play time"), Ui.Duration(w.PlaySeconds));
            Ui.KeyValue(Loc.T("ui.lastSession", "Last session"), Ui.Ago(w.LastPlayed));
            Ui.KeyValue(Loc.T("ui.created", "Created"), Ui.Ago(w.Created));
            Ui.KeyValue(Loc.T("ui.worldVersion", "Version"), Loc.T("ui.formatV", "format {0}", w.Version) + (w.GameVersion != null ? " · " + Loc.T("ui.gameV", "game {0}", w.GameVersion) : ""));
            Ui.KeyValue(Loc.T("ui.state", "State"), StatusText(w) + $"  ({w.Size / 1024.0:0} KB)");
            GUILayout.FlexibleSpace();
            if (Ui.Button(pick != null ? pickLabel : Loc.T("ui.play", "Play"), UiTheme.Primary, w.Loadable && renaming == null, GUILayout.Height(44))) Go(w);
            if (pick == null)
            {
                GUILayout.BeginHorizontal();
                if (Ui.Button(Loc.T("ui.rename", "Rename"), null, renaming == null)) { renaming = w.Id; newName = w.Name; }
                if (Ui.Button(Loc.T("ui.duplicate", "Duplicate"), null, w.Loadable))
                {
                    try
                    {
                        var id = WorldStore.Duplicate(GameBootstrap.SavesDir, w.Id, Loc.T("ui.copyOf", "{0} (copy)", w.Name), Online.Now());
                        Reload(); sel = all.Find(x => x.Id == id); Ui.Toast(Loc.T("ui.duplicated", "Copy created"), "good");
                    }
                    catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { Ui.Toast(e.Message, "bad"); }
                }
                if (Ui.Button(Loc.T("ui.delete", "Delete"), UiTheme.DangerBtn, GameBootstrap.CurrentWorld != w.Id))
                    Nav.Ask(Loc.T("ui.deleteWorldQ", "Delete \"{0}\"?", w.Name),
                            Loc.T("ui.deleteWorldBody", "The world disappears from the game. Its files are moved to the folder saves/deleted, where you can still recover them by hand."),
                            Loc.T("ui.delete", "Delete"), () =>
                            {
                                try { WorldStore.Delete(GameBootstrap.SavesDir, w.Id, Online.Now()); sel = null; Reload(); Ui.Toast(Loc.T("ui.worldDeleted", "World moved to saves/deleted"), "good"); }
                                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { Ui.Toast(e.Message, "bad"); }
                            }, true);
                GUILayout.EndHorizontal();
                if (Ui.Button(Loc.T("ui.openFolder", "Open saves folder"), UiTheme.Ghost)) Application.OpenURL("file:///" + GameBootstrap.SavesDir.Replace('\\', '/'));
            }
            GUILayout.EndArea();
        }
    }

    // ===================================================================== new world
    /// <summary>Name, seed (random or typed) and a live preview generated from that seed: start biome and nearby deposits.</summary>
    public sealed class NewWorldScreen : UiScreen
    {
        readonly bool forHosting;
        string name, seedText;
        int previewSeed = int.MinValue;
        float debounce;
        Task<(Color32[] px, Biome biome, Dictionary<ResourceType, int> near)> job;
        Texture2D tex;
        Biome biome;
        Dictionary<ResourceType, int> near;
        public Action<string, int> Created;   // multiplayer: create, then host

        public NewWorldScreen(bool forHosting) { this.forHosting = forHosting; }
        public override string Title => Loc.T("ui.newWorld", "New world");

        public override void OnShow()
        {
            if (name != null) return;
            int n = WorldStore.List(GameBootstrap.SavesDir).Count + 1;
            name = Loc.T("ui.worldN", "World {0}", n);
            seedText = UnityEngine.Random.Range(1, int.MaxValue).ToString();
        }

        bool SeedOk(out int seed) => int.TryParse(seedText.Trim(), out seed);

        public override void Update()
        {
            if (SeedOk(out int s) && s != previewSeed && (debounce += Ui.Dt) > 0.35f && (job == null || job.IsCompleted)) { previewSeed = s; debounce = 0; job = WorldPreview.FromSeed(s); }
            if (job != null && job.IsCompleted)
            {
                if (!job.IsFaulted) { if (tex != null) UnityEngine.Object.Destroy(tex); tex = WorldPreview.ToTexture(job.Result.px); biome = job.Result.biome; near = job.Result.near; }
                job = null;
            }
        }

        public override void Draw(Rect r)
        {
            MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, 0.55f);
            var c = UiNav.Frame(this, r, Loc.T("ui.newWorldSub", "The map is generated from the seed: the same seed always gives the same world."));
            float left = Mathf.Min(520, c.width * 0.45f);
            GUILayout.BeginArea(new Rect(c.x, c.y, left, c.height), UiTheme.Panel);
            Ui.Section(Loc.T("ui.world", "World"));
            name = Ui.Field(Loc.T("ui.worldName", "Name"), name, WorldStore.MaxName, "nwname", false, GUILayout.Height(36));
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            seedText = Ui.Field(Loc.T("ui.seed", "Seed"), seedText, 11, "nwseed", false, GUILayout.Height(36));
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.Space(22);
            if (Ui.Button(Loc.T("ui.random", "Random"), null, true, GUILayout.Height(36))) seedText = UnityEngine.Random.Range(1, int.MaxValue).ToString();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            bool seedOk = SeedOk(out int seed);
            if (!seedOk) GUILayout.Label(Loc.T("ui.badSeed", "The seed is a whole number (−2147483648 to 2147483647)."), new GUIStyle(UiTheme.Small) { normal = { textColor = UiTheme.Bad } });
            Ui.Section(Loc.T("ui.generation", "Generation"));
            GUILayout.Label(Loc.T("ui.genNote", "The generator has one setting, the seed: biomes, deposits and ruins all follow from it. The preview on the right is the real start area."), UiTheme.Muted_);
            Ui.Section(Loc.T("ui.difficulty", "Difficulty"));
            Settings.Peaceful = !Ui.Toggle(Loc.T("ui.hostiles", "Hostile creatures"), !Settings.Peaceful, Loc.T("ui.hostilesHint", "A game setting: it applies to every world and can be changed later in Options."));
            GUILayout.FlexibleSpace();
            bool ok = seedOk && WorldStore.CleanName(name).Length > 0;
            if (Ui.Button(forHosting ? Loc.T("ui.createHost", "Create and host") : Loc.T("ui.createPlay", "Create and play"), UiTheme.Primary, ok, GUILayout.Height(46)))
            {
                Online.Record("worlds_created", 1);
                if (Created != null) Created(name, seed);
                else Nav.Run(Loc.T("ui.generating", "Generating the world…"), () => { TitleMenu.Leaving(); GameBootstrap.CreateWorld(name, seed); });
            }
            GUILayout.EndArea();

            var pr = new Rect(c.x + left + 20, c.y, c.width - left - 20, c.height);
            GUI.Box(pr, GUIContent.none, UiTheme.Panel);
            bool side = pr.width >= 620;   // info beside the map when there is room, under it otherwise
            float s = side ? Mathf.Min(pr.height - 70, pr.width - 300) : Mathf.Min(pr.width - 40, pr.height - 260);
            var img = new Rect(pr.x + 20, pr.y + 20, s, s);
            Ui.Fill(img, UiTheme.Bg0);
            if (tex != null) GUI.DrawTexture(img, tex);
            if (job != null || tex == null) Ui.Spinner(new Rect(img.center.x - 24, img.center.y - 24, 48, 48));
            float ix = side ? img.xMax + 24 : pr.x + 20, iw = side ? pr.xMax - ix - 20 : pr.width - 40;
            float iy = side ? img.y : img.yMax + 14;
            GUI.Label(new Rect(ix, iy, iw, 24), Loc.T("ui.startArea", "Start area").ToUpperInvariant(), UiTheme.H2);
            GUI.Label(new Rect(ix, iy + 30, iw, 24), Loc.T("ui.biome", "Start biome") + ": " + (tex != null ? FrontEnd.BiomeName(biome.ToString()) : "…"), UiTheme.Label);
            float ry = iy + 60;
            if (near != null)
            {
                GUI.Label(new Rect(ix, ry, iw, 22), Loc.T("ui.nearby", "Deposits within 40 cells:"), UiTheme.Muted_);
                ry += 26;
                if (near.Count == 0) GUI.Label(new Rect(ix, ry, iw, 22), "—", UiTheme.Small);
                foreach (var kv in near)
                {
                    GUI.Label(new Rect(ix + 8, ry, iw - 8, 22), Loc.T("res." + kv.Key, kv.Key.ToString()) + "  ·  " + kv.Value, UiTheme.Label);
                    ry += 22;
                }
            }
            GUI.Label(new Rect(pr.x + 20, img.yMax + (side ? 8 : ry - img.yMax + 8), pr.width - 40, 22), Loc.T("ui.legend", "White cross: landing site · amber: buildings · 2 cells per pixel"), UiTheme.Small);
        }
    }

    // ===================================================================== credits
    public sealed class CreditsScreen : UiScreen
    {
        public override string Title => Loc.T("ui.credits", "Credits");
        Vector2 scroll;
        string notices;

        public override void OnShow()
        {
            if (notices != null) return;
            try { notices = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "THIRD_PARTY_NOTICES.md")); }
            catch (Exception) { notices = null; }
        }

        public override void Draw(Rect r)
        {
            MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, 0.55f);
            var c = UiNav.Frame(this, r);
            GUILayout.BeginArea(new Rect(c.x, c.y, Mathf.Min(900, c.width), c.height), UiTheme.Panel);
            scroll = GUILayout.BeginScrollView(scroll);
            Ui.Section("Codebound");
            GUILayout.Label(Loc.T("ui.creditsBody", "Game design, code, pixel art, music and sound made for Codebound. All sprites, sounds and music are generated by the game's own code."), UiTheme.Wrap);
            Ui.Section(Loc.T("ui.thirdParty", "Third-party software"));
            GUILayout.Label(Loc.T("ui.thirdPartyBody", "Unity (engine) · Node.js (JavaScript and TypeScript programs) · TypeScript compiler (Apache-2.0) · CPython (Python programs) · .NET / Roslyn (C# programs) · ASP.NET Core and SQLite (online service)."), UiTheme.Wrap);
            if (notices != null) { GUILayout.Space(8); GUILayout.Label(notices, UiTheme.Small); }
            else GUILayout.Label(Loc.T("ui.noticesFile", "Full licence texts: THIRD_PARTY_NOTICES.md in the game's folder."), UiTheme.Small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
