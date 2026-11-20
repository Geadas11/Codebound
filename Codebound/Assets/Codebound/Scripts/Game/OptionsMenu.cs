using System;
using System.Collections.Generic;
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
            items.Add(new Item { Label = "Resume", Change = _ => Close() });
            items.Add(new Item { Label = "Save game (" + Controls.Name("save") + ")", Change = _ => boot.Save() });
            items.Add(new Item { Label = "Load game (" + Controls.Name("load") + ")", Change = _ => { Close(); boot.Load(); } });
            items.Add(new Item { Header = true, Label = "Gameplay" });
            items.Add(new Item { Label = "Peaceful mode (no creatures)", Value = () => OnOff(Settings.Peaceful), Change = _ => Settings.Peaceful = !Settings.Peaceful });
            items.Add(new Item { Header = true, Label = "Accessibility" });
            items.Add(new Item { Label = "Interface scale", Value = () => $"{Settings.UiScale:0.00}x", Change = d => Settings.UiScale = Mathf.Round((Settings.UiScale + 0.25f * d) * 4) / 4 });
            items.Add(new Item { Label = "Text size (HUD boxes, code editor)", Value = () => $"{Settings.TextScale:0.00}x", Change = d => Settings.TextScale = Mathf.Round((Settings.TextScale + 0.25f * d) * 4) / 4 });
            items.Add(new Item { Label = "High contrast", Value = () => OnOff(Settings.HighContrast), Change = _ => Settings.HighContrast = !Settings.HighContrast });
            items.Add(new Item { Label = "Colour-blind friendly palette", Value = () => OnOff(Settings.ColorblindPalette), Change = _ => Settings.ColorblindPalette = !Settings.ColorblindPalette });
            items.Add(new Item { Label = "Reduce flashes and particles", Value = () => OnOff(Settings.ReduceEffects), Change = _ => Settings.ReduceEffects = !Settings.ReduceEffects });
            items.Add(new Item { Header = true, Label = "Audio" });
            AddVolume("Master volume", () => Settings.MasterVolume, v => Settings.MasterVolume = v);
            AddVolume("Music", () => Settings.MusicVolume, v => Settings.MusicVolume = v);
            AddVolume("Ambience & weather", () => Settings.AmbientVolume, v => Settings.AmbientVolume = v);
            AddVolume("Machines", () => Settings.MachineVolume, v => Settings.MachineVolume = v);
            AddVolume("Effects & alarms", () => Settings.EffectsVolume, v => Settings.EffectsVolume = v);
            items.Add(new Item { Label = "Sound captions (visual indicators)", Value = () => OnOff(Settings.SoundCaptions), Change = _ => Settings.SoundCaptions = !Settings.SoundCaptions });
            items.Add(new Item { Header = true, Label = "Controls (Enter, then press a key)" });
            foreach (var a in Controls.Actions)
            {
                var act = a.action;
                items.Add(new Item { Label = a.label, Value = () => rebinding == act ? "press a key…" : Controls.Name(act), Change = _ => rebinding = act });
            }
            items.Add(new Item { Label = "Reset controls to defaults", Change = _ => Controls.ResetAll() });
            items.Add(new Item { Header = true, Label = "" });
            items.Add(new Item { Label = "Quit game", Change = _ => { boot.Save(); Application.Quit(); } });
        }

        static string OnOff(bool v) => v ? "ON" : "off";

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
            GUILayout.Label("PAUSED — Options", title);
            GUILayout.Label("↑/↓ select · ←/→ or Enter change · Esc back · mouse works too", small);
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
