using System;
using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// Esc: pause + options. Fully keyboard-navigable (↑/↓ select, ←/→ or Enter change, Esc back).
    /// Gameplay, accessibility (UI/text scale, contrast, colour-blind palette, reduced effects) and key remapping.
    /// </summary>
    public sealed class OptionsMenu : MonoBehaviour
    {
        sealed class Item
        {
            public string Label; public Func<string> Value; public Action<int> Change; public bool Header;
        }

        GameSession session;
        GameBootstrap boot;
        BuildController build;
        bool open;
        int selected;
        string rebinding;            // action waiting for a key
        float prevTimeScale = 1;
        readonly List<Item> items = new List<Item>();
        GUIStyle box, label, row, rowSel, title, small;
        int styleVersion = -1;
        Vector2 scroll;

        public static bool IsOpen { get; private set; }

        public void Init(GameSession s, GameBootstrap b, BuildController bc)
        {
            session = s; boot = b; build = bc;
            BuildItems();
        }

        void BuildItems()
        {
            items.Clear();
            items.Add(new Item { Label = Loc.T("menu.resume", "Resume"), Change = _ => Close() });
            items.Add(new Item { Label = Loc.T("menu.save", "Save game") + " (" + Controls.Name("save") + ")", Change = _ => boot.Save() });
            items.Add(new Item { Label = Loc.T("menu.load", "Load game") + " (" + Controls.Name("load") + ")", Change = _ => { Close(); boot.Load(); } });
            items.Add(new Item { Header = true, Label = Loc.T("menu.gameplay", "Gameplay") });
            items.Add(new Item { Label = Loc.T("menu.peaceful", "Peaceful mode (no creatures)"), Value = () => OnOff(Settings.Peaceful), Change = _ => Settings.Peaceful = !Settings.Peaceful });
            items.Add(new Item { Header = true, Label = Loc.T("menu.accessibility", "Accessibility") });
            items.Add(new Item { Label = "Language / Idioma / 言語", Value = () => LanguageName(Settings.Language), Change = d => { CycleLanguage(d); BuildItems(); } });
            items.Add(new Item { Label = Loc.T("menu.uiscale", "Interface scale"), Value = () => $"{Settings.UiScale:0.00}x", Change = d => Settings.UiScale = Mathf.Round((Settings.UiScale + 0.25f * d) * 4) / 4 });
            items.Add(new Item { Label = Loc.T("menu.textsize", "Text size (HUD boxes, code editor)"), Value = () => $"{Settings.TextScale:0.00}x", Change = d => Settings.TextScale = Mathf.Round((Settings.TextScale + 0.25f * d) * 4) / 4 });
            items.Add(new Item { Label = Loc.T("menu.contrast", "High contrast"), Value = () => OnOff(Settings.HighContrast), Change = _ => Settings.HighContrast = !Settings.HighContrast });
            items.Add(new Item { Label = Loc.T("menu.colorblind", "Colour-blind friendly palette"), Value = () => OnOff(Settings.ColorblindPalette), Change = _ => Settings.ColorblindPalette = !Settings.ColorblindPalette });
            items.Add(new Item { Label = Loc.T("menu.assist", "Coding help"), Value = () => Loc.T("assist." + Codebound.Core.Assist.Levels[Settings.Assist], Codebound.Core.Assist.Levels[Settings.Assist]),
                                 Change = d => Settings.Assist = (Settings.Assist + (d < 0 ? 2 : 1)) % 3 });
            items.Add(new Item { Label = Loc.T("menu.cfilter", "Colour-blind screen filter"), Value = () => Loc.T("cfilter." + Codebound.Core.ColorVision.Modes[Settings.ColorFilter], Codebound.Core.ColorVision.Modes[Settings.ColorFilter]),
                                 Change = d => Settings.ColorFilter = (Settings.ColorFilter + (d < 0 ? 3 : 1)) % 4 });
            items.Add(new Item { Label = Loc.T("menu.effects", "Reduce flashes and particles"), Value = () => OnOff(Settings.ReduceEffects), Change = _ => Settings.ReduceEffects = !Settings.ReduceEffects });
            items.Add(new Item { Header = true, Label = Loc.T("menu.audio", "Audio") });
            AddVolume(Loc.T("menu.vol.master", "Master volume"), () => Settings.MasterVolume, v => Settings.MasterVolume = v);
            AddVolume(Loc.T("menu.vol.music", "Music"), () => Settings.MusicVolume, v => Settings.MusicVolume = v);
            AddVolume(Loc.T("menu.vol.ambient", "Ambience & weather"), () => Settings.AmbientVolume, v => Settings.AmbientVolume = v);
            AddVolume(Loc.T("menu.vol.machines", "Machines"), () => Settings.MachineVolume, v => Settings.MachineVolume = v);
            AddVolume(Loc.T("menu.vol.effects", "Effects & alarms"), () => Settings.EffectsVolume, v => Settings.EffectsVolume = v);
            items.Add(new Item { Label = Loc.T("menu.captions", "Sound captions (visual indicators)"), Value = () => OnOff(Settings.SoundCaptions), Change = _ => Settings.SoundCaptions = !Settings.SoundCaptions });
            items.Add(new Item { Header = true, Label = Loc.T("menu.controls", "Controls (Enter, then press a key)") });
            foreach (var a in Controls.Actions)
            {
                var act = a.action;
                items.Add(new Item { Label = Controls.Label(act, a.label), Value = () => rebinding == act ? Loc.T("menu.presskey", "press a key…") : Controls.Name(act), Change = _ => rebinding = act });
            }
            items.Add(new Item { Label = Loc.T("menu.resetcontrols", "Reset controls to defaults"), Change = _ => Controls.ResetAll() });
            items.Add(new Item { Header = true, Label = "" });
            items.Add(new Item { Label = Loc.T("menu.quit", "Quit game"), Change = _ => { boot.Save(); Application.Quit(); } });
        }

        static string OnOff(bool v) => v ? Loc.T("menu.on", "ON") : Loc.T("menu.off", "off");

        static string LanguageName(string code) { foreach (var l in Loc.Languages) if (l.code == code) return l.native; return code; }

        static void CycleLanguage(int d)
        {
            int n = Loc.Languages.Length, i = 0;
            for (int k = 0; k < n; k++) if (Loc.Languages[k].code == Settings.Language) i = k;
            Settings.Language = Loc.Languages[((i + (d < 0 ? -1 : 1)) % n + n) % n].code;
        }

        void AddVolume(string label, Func<float> get, Action<float> set) =>
            items.Add(new Item { Label = label, Value = () => $"{Mathf.RoundToInt(get() * 100)}%", Change = d => set(Mathf.Clamp01(Mathf.Round((get() + 0.1f * d) * 10) / 10)) });

        void Open()
        {
            open = IsOpen = true; session.PanelOpen = true; selected = 0; rebinding = null;
            prevTimeScale = Time.timeScale; Time.timeScale = 0;   // single player: the world pauses
        }

        void Close()
        {
            open = IsOpen = false; session.PanelOpen = false; rebinding = null;
            Time.timeScale = prevTimeScale <= 0 ? 1 : prevTimeScale;
        }

        void OnDestroy() { if (open) Time.timeScale = 1; IsOpen = false; }

        void Update()
        {
            session.State.Creatures.Enabled = !Settings.Peaceful;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!open)
            {
                // Esc opens the menu only when nothing else is using it (panels and build mode close themselves first)
                if (kb.escapeKey.wasPressedThisFrame && !blockedLastFrame && !session.PanelOpen && !session.KeyboardCaptured && !build.Active) Open();
                return;
            }
            if (rebinding != null)
            {
                foreach (var key in kb.allKeys)
                {
                    if (key == null || !key.wasPressedThisFrame) continue;
                    if (key.keyCode != Key.Escape) Controls.Set(rebinding, key.keyCode);
                    rebinding = null;
                    break;
                }
                return;
            }
            if (kb.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (kb.upArrowKey.wasPressedThisFrame) Move(-1);
            if (kb.downArrowKey.wasPressedThisFrame) Move(1);
            var it = items[selected];
            if (kb.leftArrowKey.wasPressedThisFrame) it.Change?.Invoke(-1);
            if (kb.rightArrowKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) it.Change?.Invoke(1);
        }

        // Esc that closed a panel or left build mode in this frame must not also open the menu (script order varies)
        bool blockedLastFrame;
        void LateUpdate() => blockedLastFrame = !open && (session.PanelOpen || session.KeyboardCaptured || build.Active);

        void Move(int d)
        {
            do selected = (selected + d + items.Count) % items.Count; while (items[selected].Header);
        }

        void Styles()
        {
            if (box != null && styleVersion == Settings.Version) return;
            styleVersion = Settings.Version;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, Settings.HighContrast ? Color.black : new Color(0.04f, 0.05f, 0.08f, 0.97f)); bg.Apply();
            var sel = new Texture2D(1, 1); sel.SetPixel(0, 0, Settings.HighContrast ? new Color(0.25f, 0.25f, 0.05f) : new Color(0.12f, 0.22f, 0.2f)); sel.Apply();
            var text = Settings.HighContrast ? Color.white : new Color(0.86f, 0.95f, 0.9f);
            box = new GUIStyle(GUI.skin.box) { normal = { background = bg }, padding = new RectOffset(16, 16, 12, 12) };
            label = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(15), richText = true, normal = { textColor = text } };
            small = new GUIStyle(label) { fontSize = UiGui.Font(12) };
            title = new GUIStyle(label) { fontSize = UiGui.Font(20), fontStyle = FontStyle.Bold };
            row = new GUIStyle(label) { padding = new RectOffset(8, 8, 3, 3) };
            rowSel = new GUIStyle(row) { normal = { background = sel, textColor = Settings.HighContrast ? Color.yellow : Color.white } };
        }

        void OnGUI()
        {
            if (!open) return;
            UiGui.Begin();
            Styles();
            float w = Mathf.Min(560, UiGui.W - 40), h = Mathf.Min(UiGui.H - 40, 640);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.Label(Loc.T("menu.title", "PAUSED — Options"), title);
            GUILayout.Label(Loc.T("menu.help", "Up/Down select · Left/Right or Enter change · Esc back · mouse works too"), small);
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it.Header) { GUILayout.Space(6); if (it.Label != "") GUILayout.Label("<b>" + it.Label + "</b>", label); continue; }
                GUILayout.BeginHorizontal(i == selected ? rowSel : row);
                if (GUILayout.Button(it.Label, i == selected ? rowSel : row)) { selected = i; it.Change?.Invoke(1); }
                GUILayout.FlexibleSpace();
                if (it.Value != null) GUILayout.Label(it.Value(), i == selected ? rowSel : row);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
