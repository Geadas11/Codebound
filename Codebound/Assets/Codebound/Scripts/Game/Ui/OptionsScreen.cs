using System;
using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// All settings by category. Every option here changes something real and is stored (PlayerPrefs) at once;
    /// each category can be put back to its defaults. Shared by the title screen and the pause menu.
    /// </summary>
    public sealed class OptionsScreen : UiScreen
    {
        readonly bool inGame;
        int cat;
        string rebinding;
        Vector2 scroll;
        string portText, lan;

        public OptionsScreen(bool inGame) { this.inGame = inGame; }
        public override string Title => Loc.T("ui.options", "Options");

        string[] Cats => new[]
        {
            Loc.T("opt.graphics", "Graphics"), Loc.T("opt.audio", "Audio"), Loc.T("opt.controls", "Controls"), Loc.T("opt.gameplay", "Gameplay"),
            Loc.T("opt.interface", "Interface"), Loc.T("opt.language", "Language"), Loc.T("opt.access", "Accessibility"), Loc.T("opt.network", "Network"),
        };

        public override bool OnBack() { if (rebinding != null) { rebinding = null; return false; } return true; }

        public override void Update()
        {
            if (rebinding == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            foreach (var key in kb.allKeys)
            {
                if (key == null || !key.wasPressedThisFrame || key.keyCode == Key.Escape) continue;
                Controls.Set(rebinding, key.keyCode);
                var clash = Conflict(rebinding);
                if (clash != null) Ui.Toast(Loc.T("opt.conflictToast", "{0} is also used by {1}", Controls.Name(rebinding), clash), "bad");
                else Ui.Confirm();
                rebinding = null;
                break;
            }
        }

        static string Conflict(string action)
        {
            var k = Controls.Get(action);
            if (k == Key.None) return null;
            foreach (var a in Controls.Actions) if (a.action != action && Controls.Get(a.action) == k) return Controls.Label(a.action, a.label);
            return null;
        }

        public override void Draw(Rect r)
        {
            if (!inGame) MenuBackdrop.Draw(r);
            MenuBackdrop.Dim(r, inGame ? 0.45f : 0.55f);
            var c = UiNav.Frame(this, r, Loc.T("opt.sub", "Changes apply and are saved immediately."));
            float tabsW = 230;
            GUILayout.BeginArea(new Rect(c.x, c.y, tabsW, c.height));
            var cats = Cats;
            for (int i = 0; i < cats.Length; i++)
                if (Ui.Button(cats[i], i == cat ? UiTheme.RowSel : UiTheme.Row, true, GUILayout.Height(44))) { cat = i; scroll = Vector2.zero; rebinding = null; }
            GUILayout.EndArea();

            var body = new Rect(c.x + tabsW + 20, c.y, Mathf.Min(c.width - tabsW - 20, 860), c.height);
            GUILayout.BeginArea(body, UiTheme.Panel);
            GUILayout.Label(cats[cat], UiTheme.H1);
            Ui.Rule(UiTheme.Accent, 2);
            scroll = GUILayout.BeginScrollView(scroll);
            switch (cat)
            {
                case 0: Graphics(); break;
                case 1: Audio(); break;
                case 2: ControlsTab(); break;
                case 3: Gameplay(); break;
                case 4: Interface(); break;
                case 5: Language(); break;
                case 6: Access(); break;
                case 7: Network(); break;
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (Ui.Button(Loc.T("opt.reset", "Restore defaults"), UiTheme.Ghost, true, GUILayout.Height(36)))
                Nav.Ask(Loc.T("opt.resetQ", "Restore the defaults of {0}?", cats[cat]), null, Loc.T("opt.reset", "Restore defaults"), () => { ResetCategory(cat); Ui.Toast(Loc.T("opt.resetDone", "Defaults restored"), "good"); });
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        static void ResetCategory(int cat)
        {
            switch (cat)
            {
                case 0: Settings.Reset("cb.fps", "cb.vsync", "cb.quality", "cb.uiScale"); Settings.ApplyGraphics(); break;
                case 1: Settings.Reset("cb.vol.master", "cb.vol.music", "cb.vol.ambient", "cb.vol.machines", "cb.vol.effects", "cb.vol.ui", "cb.captions"); break;
                case 2: Controls.ResetAll(); break;
                case 3: Settings.Reset("cb.peaceful", "cb.assist", "cb.autosaveMin"); break;
                case 4: Settings.Reset("cb.uiScale", "cb.textScale", "cb.reduceMotion", "cb.showFps", "cb.notifications"); break;
                case 5: Settings.Reset("cb.lang"); Settings.ApplyLanguage(); break;
                case 6: Settings.Reset("cb.contrast", "cb.colorblind", "cb.cfilter", "cb.reduceEffects", "cb.textScale", "cb.captions", "cb.reduceMotion"); break;
                case 7: Settings.Reset("cb.netPort", "cb.maxGuests", "cb.guestRole"); break;
            }
        }

        static string OnOff(bool v) => v ? Loc.T("menu.on", "ON") : Loc.T("menu.off", "off");

        // ------------------------------------------------------------------ graphics
        static List<(int w, int h)> resolutions;

        void Graphics()
        {
            Ui.Section(Loc.T("opt.display", "Display"));
            if (resolutions == null)
            {
                resolutions = new List<(int, int)>();
                foreach (var res in Screen.resolutions) if (!resolutions.Contains((res.width, res.height))) resolutions.Add((res.width, res.height));
                if (resolutions.Count == 0) resolutions.Add((Screen.width, Screen.height));
            }
            int cur = resolutions.FindIndex(x => x.w == Screen.width && x.h == Screen.height);
            var names = resolutions.ConvertAll(x => $"{x.w} × {x.h}").ToArray();
            string resHint = Application.isEditor ? Loc.T("opt.editorRes", "In the Unity editor the window decides the size; this works in the game build.") : null;
            if (cur < 0) { names = new List<string>(names) { $"{Screen.width} × {Screen.height}" }.ToArray(); cur = names.Length - 1; }
            int ni = Ui.Choice(Loc.T("opt.resolution", "Resolution"), cur, names, !Application.isEditor, resHint);
            if (ni != cur && ni < resolutions.Count) Screen.SetResolution(resolutions[ni].w, resolutions[ni].h, Screen.fullScreenMode);

            var modes = new[] { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
            var modeNames = new[] { Loc.T("opt.fullscreen", "Full screen"), Loc.T("opt.borderless", "Borderless window"), Loc.T("opt.windowed", "Window") };
            int mi = Array.IndexOf(modes, Screen.fullScreenMode); if (mi < 0) mi = 1;
            int nm = Ui.Choice(Loc.T("opt.windowMode", "Screen mode"), mi, modeNames, !Application.isEditor);
            if (nm != mi) Screen.fullScreenMode = modes[nm];

            Ui.Section(Loc.T("opt.performance", "Performance"));
            Settings.VSync = Ui.Toggle(Loc.T("opt.vsync", "Vertical sync (VSync)"), Settings.VSync, Loc.T("opt.vsyncHint", "Matches the monitor's refresh rate; replaces the frame limit."));
            var limits = Settings.FpsLimits;
            var limitNames = Array.ConvertAll(limits, l => l == 0 ? Loc.T("opt.unlimited", "Unlimited") : l + " FPS");
            int li = Array.IndexOf(limits, Settings.FpsLimit); if (li < 0) li = 2;
            int nl = Ui.Choice(Loc.T("opt.fpsLimit", "Frame rate limit"), li, limitNames, !Settings.VSync);
            if (nl != li) Settings.FpsLimit = limits[nl];
            var q = QualitySettings.names;
            if (q.Length > 1)
            {
                int nq = Ui.Choice(Loc.T("opt.quality", "Graphics quality"), Settings.Quality, q);
                if (nq != Settings.Quality) Settings.Quality = nq;
            }
            Ui.Section(Loc.T("opt.scale", "Interface scale"));
            ScaleSlider();
        }

        static void ScaleSlider()
        {
            float v = Ui.Slider(Loc.T("menu.uiscale", "Interface scale"), Settings.UiScale, 0.75f, 2f, $"{Settings.UiScale:0.00}×");
            v = Mathf.Round(v * 20) / 20f;
            if (Mathf.Abs(v - Settings.UiScale) > 0.001f) Settings.UiScale = v;
            GUILayout.Label(Loc.T("opt.scaleHint", "Automatic part for this screen: {0:0.00}× (1080p = 1×). Total: {1:0.00}×.", UiGui.Auto, UiGui.S), UiTheme.Small);
        }

        // ------------------------------------------------------------------ audio
        static void Volume(string label, Func<float> get, Action<float> set)
        {
            float v = Ui.Slider(label, get(), 0, 1, Mathf.RoundToInt(get() * 100) + "%");
            v = Mathf.Round(v * 100) / 100f;
            if (Mathf.Abs(v - get()) > 0.001f) set(v);
        }

        void Audio()
        {
            Ui.Section(Loc.T("opt.volume", "Volume"));
            Volume(Loc.T("menu.vol.master", "Master volume"), () => Settings.MasterVolume, v => Settings.MasterVolume = v);
            Volume(Loc.T("menu.vol.music", "Music"), () => Settings.MusicVolume, v => Settings.MusicVolume = v);
            Volume(Loc.T("menu.vol.effects", "Effects & alarms"), () => Settings.EffectsVolume, v => Settings.EffectsVolume = v);
            Volume(Loc.T("menu.vol.machines", "Machines"), () => Settings.MachineVolume, v => Settings.MachineVolume = v);
            Volume(Loc.T("menu.vol.ambient", "Ambience & weather"), () => Settings.AmbientVolume, v => Settings.AmbientVolume = v);
            Volume(Loc.T("opt.vol.ui", "Interface"), () => Settings.UiVolume, v => Settings.UiVolume = v);
            GUILayout.Space(6);
            if (Ui.Button(Loc.T("opt.testSound", "Test interface sound"), null, true, GUILayout.Width(240))) Ui.Confirm();
            Ui.Section(Loc.T("opt.captionsSec", "Captions"));
            Settings.SoundCaptions = Ui.Toggle(Loc.T("menu.captions", "Sound captions (visual indicators)"), Settings.SoundCaptions);
        }

        // ------------------------------------------------------------------ controls
        static readonly (string title, string english, string[] actions)[] Groups =
        {
            ("opt.grp.move", "Movement", new[] { "up", "down", "left", "right" }),
            ("opt.grp.build", "Building", new[] { "build", "rotate", "interact" }),
            ("opt.grp.ui", "Interface", new[] { "inventory", "research", "journal", "network", "chat" }),
            ("opt.grp.game", "Game", new[] { "eat", "save", "load" }),
        };

        void ControlsTab()
        {
            GUILayout.Label(Loc.T("opt.rebindHelp", "Click a key, then press the new one (Esc cancels). Arrow keys always move as well."), UiTheme.Muted_);
            foreach (var (title, english, actions) in Groups)
            {
                Ui.Section(Loc.T(title, english));
                foreach (var act in actions)
                {
                    string label = act;
                    foreach (var a in Controls.Actions) if (a.action == act) label = Controls.Label(act, a.label);
                    var clash = Conflict(act);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label, UiTheme.Label);
                    if (clash != null) GUILayout.Label(Loc.T("opt.conflict", "conflict: {0}", clash), new GUIStyle(UiTheme.Small) { normal = { textColor = UiTheme.Bad }, alignment = TextAnchor.MiddleRight });
                    GUILayout.FlexibleSpace();
                    if (Ui.Button(rebinding == act ? Loc.T("menu.presskey", "press a key…") : Controls.Name(act), rebinding == act ? UiTheme.Primary : (clash != null ? UiTheme.DangerBtn : null), true, GUILayout.Width(170)))
                        rebinding = act;
                    GUILayout.EndHorizontal();
                }
            }
            Ui.Section(Loc.T("opt.grp.code", "Programming (fixed)"));
            foreach (var (k, what) in new[] { ("F8", Loc.T("opt.k.continue", "Debugger: continue")), ("F10", Loc.T("opt.k.over", "Debugger: step over")), ("F11", Loc.T("opt.k.into", "Debugger: step into")),
                                               ("Shift+F11", Loc.T("opt.k.out", "Debugger: step out")), ("Tab", Loc.T("opt.k.tab", "Code editor: indent")), ("Esc", Loc.T("opt.k.esc", "Close panel / menu / back")) })
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(what, UiTheme.Label);
                GUILayout.FlexibleSpace();
                GUILayout.Label(k, new GUIStyle(UiTheme.Value) { normal = { textColor = UiTheme.Muted } }, GUILayout.Width(170));
                GUILayout.EndHorizontal();
            }
        }

        // ------------------------------------------------------------------ gameplay
        void Gameplay()
        {
            Ui.Section(Loc.T("opt.world", "World"));
            Settings.Peaceful = !Ui.Toggle(Loc.T("ui.hostiles", "Hostile creatures"), !Settings.Peaceful, Loc.T("opt.peacefulHint", "Off: peaceful mode, no creature waves."));
            var auto = new[] { 0, 1, 2, 5, 10, 15, 30 };
            var autoNames = Array.ConvertAll(auto, m => m == 0 ? Loc.T("menu.off", "off") : Loc.T("ui.minutes", "{0} min", m));
            int ai = Array.IndexOf(auto, Settings.AutoSaveMinutes); if (ai < 0) ai = 3;
            int na = Ui.Choice(Loc.T("opt.autosave", "Autosave every"), ai, autoNames, true, Loc.T("opt.autosaveHint", "Saves the world you are playing; the previous file is kept as .bak."));
            if (na != ai) Settings.AutoSaveMinutes = auto[na];
            Ui.Section(Loc.T("opt.programming", "Programming"));
            int asl = Ui.Choice(Loc.T("menu.assist", "Coding help"), Settings.Assist, Array.ConvertAll(Codebound.Core.Assist.Levels, l => Loc.T("assist." + l, l)));
            if (asl != Settings.Assist) Settings.Assist = asl;
        }

        // ------------------------------------------------------------------ interface
        void Interface()
        {
            Ui.Section(Loc.T("opt.size", "Size"));
            ScaleSlider();
            TextSlider();
            Ui.Section(Loc.T("opt.hud", "HUD and notifications"));
            Settings.ShowFps = Ui.Toggle(Loc.T("opt.showFps", "Show frame rate (FPS) in the status box"), Settings.ShowFps);
            Settings.Notifications = Ui.Toggle(Loc.T("opt.notifications", "Pop-up notifications (research, waves, skills)"), Settings.Notifications, Loc.T("opt.notificationsHint", "Alarms keep their sound and captions."));
            Ui.Section(Loc.T("opt.motion", "Motion"));
            Settings.ReduceMotion = Ui.Toggle(Loc.T("opt.reduceMotion", "Reduce motion (no transitions, still menu background)"), Settings.ReduceMotion);
        }

        static void TextSlider()
        {
            float v = Ui.Slider(Loc.T("menu.textsize", "Text size (HUD boxes, code editor)"), Settings.TextScale, 0.75f, 2f, $"{Settings.TextScale:0.00}×");
            v = Mathf.Round(v * 20) / 20f;
            if (Mathf.Abs(v - Settings.TextScale) > 0.001f) Settings.TextScale = v;
        }

        // ------------------------------------------------------------------ language
        void Language()
        {
            GUILayout.Label(Loc.T("opt.langHelp", "Every menu, panel, HUD box and message uses the chosen language. Texts that are not translated yet appear in English."), UiTheme.Muted_);
            GUILayout.Space(6);
            foreach (var (code, native) in Loc.Languages)
            {
                bool on = Settings.Language == code;
                if (Ui.Button(native, on ? UiTheme.RowSel : UiTheme.Row, true, GUILayout.Height(40)) && !on) Settings.Language = code;
            }
        }

        // ------------------------------------------------------------------ accessibility
        void Access()
        {
            Ui.Section(Loc.T("opt.vision", "Vision"));
            TextSlider();
            Settings.HighContrast = Ui.Toggle(Loc.T("menu.contrast", "High contrast"), Settings.HighContrast);
            Settings.ColorblindPalette = Ui.Toggle(Loc.T("menu.colorblind", "Colour-blind friendly palette"), Settings.ColorblindPalette, Loc.T("opt.cbHint", "Status colours use blue and orange instead of green and red."));
            int f = Ui.Choice(Loc.T("menu.cfilter", "Colour-blind screen filter"), Settings.ColorFilter, Array.ConvertAll(Codebound.Core.ColorVision.Modes, m => Loc.T("cfilter." + m, m)));
            if (f != Settings.ColorFilter) Settings.ColorFilter = f;
            Ui.Section(Loc.T("opt.motionFlashes", "Motion and flashes"));
            Settings.ReduceEffects = Ui.Toggle(Loc.T("menu.effects", "Reduce flashes and particles"), Settings.ReduceEffects);
            Settings.ReduceMotion = Ui.Toggle(Loc.T("opt.reduceMotion", "Reduce motion (no transitions, still menu background)"), Settings.ReduceMotion);
            Ui.Section(Loc.T("opt.hearing", "Hearing"));
            Settings.SoundCaptions = Ui.Toggle(Loc.T("menu.captions", "Sound captions (visual indicators)"), Settings.SoundCaptions);
        }

        // ------------------------------------------------------------------ network
        void Network()
        {
            Ui.Section(Loc.T("opt.hosting", "Hosting"));
            portText ??= Settings.NetPort.ToString();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            portText = Ui.Field(Loc.T("opt.port", "Port (TCP)"), portText, 5, "port", false, GUILayout.Width(160), GUILayout.Height(34));
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Space(22);
            bool valid = int.TryParse(portText, out int port) && port >= 1024 && port <= 65535;
            if (Ui.Button(Loc.T("ui.apply", "Apply"), null, valid && port != Settings.NetPort, GUILayout.Width(120), GUILayout.Height(34))) { Settings.NetPort = port; if (!Net.IsHost) Net.HostPort = port; Ui.Toast(Loc.T("opt.portSet", "Port {0} will be used the next time you host.", port), "good"); }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (!valid) GUILayout.Label(Loc.T("opt.portRule", "A number from 1024 to 65535 (default {0}).", Multiplayer.Port), new GUIStyle(UiTheme.Small) { normal = { textColor = UiTheme.Bad } });
            int mg = Ui.Choice(Loc.T("opt.maxPlayers", "Players besides you"), Settings.MaxGuests - 1, BuildNumbers(Multiplayer.MaxGuests));
            if (mg != Settings.MaxGuests - 1) Settings.MaxGuests = mg + 1;
            var roles = new[] { Multiplayer.Visitor, Multiplayer.Builder, Multiplayer.Admin };
            int ri = Array.IndexOf(roles, Settings.GuestRole);
            int nr = Ui.Choice(Loc.T("opt.guestRole", "Newcomers join as"), ri, Array.ConvertAll(roles, x => Loc.T("net.role." + x, x)), true, Loc.T("opt.guestRoleHint", "Visitors look around; builders build; admins may also remove what others built."));
            if (nr != ri) Settings.GuestRole = roles[nr];
            Ui.Section(Loc.T("opt.connection", "Connection"));
            Ui.KeyValue(Loc.T("opt.netState", "State"), Net.Current == Net.Mode.Off ? Loc.T("opt.notConnected", "not in a multiplayer game") : Net.Status);
            Ui.KeyValue(Loc.T("opt.lan", "This PC on the LAN"), lan ??= Net.LocalAddresses());
            Ui.KeyValue(Loc.T("opt.player", "Your name"), Net.MyName);
            GUILayout.Label(Loc.T("opt.latencyNote", "The game does not measure latency yet; the N panel shows who is connected."), UiTheme.Small);
        }

        static string[] BuildNumbers(int n) { var a = new string[n]; for (int i = 0; i < n; i++) a[i] = (i + 1).ToString(); return a; }
    }
}
