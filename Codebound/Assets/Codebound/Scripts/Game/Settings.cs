using System.Collections.Generic;
using System.IO;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Codebound.Game
{
    /// <summary>Player options (PlayerPrefs, per machine). Accessibility settings from the Prompt §7 live here.</summary>
    public static class Settings
    {
        static int I(string k, int d) { try { return PlayerPrefs.GetInt(k, d); } catch { return d; } }
        static float F(string k, float d) { try { return PlayerPrefs.GetFloat(k, d); } catch { return d; } }
        static void Set(string k, int v) { PlayerPrefs.SetInt(k, v); PlayerPrefs.Save(); Version++; }
        static void Set(string k, float v) { PlayerPrefs.SetFloat(k, v); PlayerPrefs.Save(); Version++; }

        /// <summary>Bumped on every change so cached styles can rebuild.</summary>
        public static int Version { get; private set; }

        public static bool Peaceful { get => I("cb.peaceful", 0) == 1; set => Set("cb.peaceful", value ? 1 : 0); }
        public static bool ReduceEffects { get => I("cb.reduceEffects", 0) == 1; set => Set("cb.reduceEffects", value ? 1 : 0); }
        public static bool HighContrast { get => I("cb.contrast", 0) == 1; set => Set("cb.contrast", value ? 1 : 0); }
        /// <summary>Whole-screen colour-blind correction: 0 off, 1 deuteranopia, 2 protanopia, 3 tritanopia.</summary>
        public static int ColorFilter { get => Mathf.Clamp(I("cb.cfilter", 0), 0, 3); set => Set("cb.cfilter", Mathf.Clamp(value, 0, 3)); }
        /// <summary>Coding help: 0 guided, 1 normal, 2 advanced (Core.Assist).</summary>
        public static int Assist { get => Mathf.Clamp(I("cb.assist", 0), 0, 2); set => Set("cb.assist", Mathf.Clamp(value, 0, 2)); }
        public static bool ColorblindPalette { get => I("cb.colorblind", 0) == 1; set => Set("cb.colorblind", value ? 1 : 0); }
        public static float UiScale { get => Mathf.Clamp(F("cb.uiScale", 1f), 0.75f, 2f); set => Set("cb.uiScale", Mathf.Clamp(value, 0.75f, 2f)); }
        public static float MasterVolume { get => Mathf.Clamp01(F("cb.vol.master", 0.8f)); set => Set("cb.vol.master", Mathf.Clamp01(value)); }
        public static float MusicVolume { get => Mathf.Clamp01(F("cb.vol.music", 0.6f)); set => Set("cb.vol.music", Mathf.Clamp01(value)); }
        public static float AmbientVolume { get => Mathf.Clamp01(F("cb.vol.ambient", 0.7f)); set => Set("cb.vol.ambient", Mathf.Clamp01(value)); }
        public static float MachineVolume { get => Mathf.Clamp01(F("cb.vol.machines", 0.7f)); set => Set("cb.vol.machines", Mathf.Clamp01(value)); }
        public static float EffectsVolume { get => Mathf.Clamp01(F("cb.vol.effects", 0.8f)); set => Set("cb.vol.effects", Mathf.Clamp01(value)); }
        /// <summary>On-screen captions for important sounds, with the direction they come from.</summary>
        public static bool SoundCaptions { get => I("cb.captions", 0) == 1; set => Set("cb.captions", value ? 1 : 0); }
        /// <summary>UI language code (Loc.Languages). Defaults to the OS language when it is one we ship.</summary>
        public static string Language
        {
            get { try { return PlayerPrefs.GetString("cb.lang", SystemLanguage()); } catch { return "en"; } }
            set { PlayerPrefs.SetString("cb.lang", value); PlayerPrefs.Save(); ApplyLanguage(); Version++; }
        }

        static string SystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case UnityEngine.SystemLanguage.Portuguese: return "pt-PT";
                case UnityEngine.SystemLanguage.Spanish: return "es";
                case UnityEngine.SystemLanguage.French: return "fr";
                case UnityEngine.SystemLanguage.German: return "de";
                case UnityEngine.SystemLanguage.Italian: return "it";
                case UnityEngine.SystemLanguage.Japanese: return "ja";
                case UnityEngine.SystemLanguage.Chinese: case UnityEngine.SystemLanguage.ChineseSimplified: return "zh-Hans";
                case UnityEngine.SystemLanguage.Korean: return "ko";
                default: return "en";
            }
        }

        /// <summary>Loads the table for Language from StreamingAssets/Localization; anything missing stays English.</summary>
        public static void ApplyLanguage()
        {
            string code = Language, json = null;
            if (code != "en")
                try { json = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Localization", code + ".json")); }
                catch (IOException e) { Debug.LogWarning("Codebound: language table " + code + ": " + e.Message); }
            if (json == null || !Loc.Load(code, json)) Loc.Load("en", null);
        }

        static Font cjkFont;
        /// <summary>OS font with Japanese/Chinese/Korean glyphs (the built-in font has none); null for other languages.</summary>
        public static Font UiFont
        {
            get
            {
                if (!Loc.NeedsCjkFont) return null;
                string[] names = Loc.Current == "ja" ? new[] { "Yu Gothic UI", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Hiragino Sans" }
                               : Loc.Current == "ko" ? new[] { "Malgun Gothic", "Gulim", "Noto Sans CJK KR", "Apple SD Gothic Neo" }
                               : new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Noto Sans CJK SC", "PingFang SC" };
                if (cjkFont == null || cjkFont.fontNames[0] != names[0]) cjkFont = Font.CreateDynamicFontFromOSFont(names, 16);
                return cjkFont;
            }
        }

        public static float TextScale { get => Mathf.Clamp(F("cb.textScale", 1f), 0.75f, 2f); set => Set("cb.textScale", Mathf.Clamp(value, 0.75f, 2f)); }

        // status colours that stay distinguishable with red–green colour blindness (blue / orange instead of green / red)
        public static string Good => ColorblindPalette ? "#4da3ff" : "#5ef0a0";
        public static string Bad => ColorblindPalette ? "#ff9f1c" : "#ff6b6b";
        public static Color GoodColor => ColorblindPalette ? new Color(0.3f, 0.64f, 1f) : new Color(0.37f, 0.94f, 0.63f);
        public static Color BadColor => ColorblindPalette ? new Color(1f, 0.62f, 0.11f) : new Color(1f, 0.42f, 0.42f);
    }

    /// <summary>IMGUI scaling: every OnGUI starts with UiGui.Begin() and lays out in UiGui.W × UiGui.H.</summary>
    public static class UiGui
    {
        public static float S => Settings.UiScale;
        public static float W => Screen.width / S;
        public static float H => Screen.height / S;
        static Font defaultFont;
        public static void Begin()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(S, S, 1));
            if (defaultFont == null) defaultFont = GUI.skin.font;
            var f = Settings.UiFont ?? defaultFont;
            if (GUI.skin.font != f) GUI.skin.font = f;
        }
        /// <summary>Mouse position (Input System, bottom-left origin) in scaled IMGUI coordinates.</summary>
        public static Vector2 Mouse(Vector2 screen) => new Vector2(screen.x / S, (Screen.height - screen.y) / S);
        public static int Font(int size) => Mathf.RoundToInt(size * Settings.TextScale);
    }

    /// <summary>Remappable keyboard controls. Arrow keys always work for movement as well.</summary>
    public static class Controls
    {
        public static readonly (string action, string label, Key def)[] Actions =
        {
            ("up", "Move up", Key.W), ("down", "Move down", Key.S), ("left", "Move left", Key.A), ("right", "Move right", Key.D),
            ("build", "Build mode", Key.B), ("rotate", "Rotate", Key.R), ("interact", "Open machine / computer", Key.E),
            ("eat", "Eat", Key.F), ("research", "Research tree", Key.T), ("inventory", "Inventory", Key.Tab), ("journal", "Journal", Key.J), ("network", "Multiplayer", Key.N),
            ("save", "Quick save", Key.F5), ("load", "Quick load", Key.F9),
        };

        static readonly Dictionary<string, Key> cache = new Dictionary<string, Key>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => cache.Clear();

        public static Key Get(string action)
        {
            if (cache.TryGetValue(action, out var k)) return k;
            var def = Key.None;
            foreach (var a in Actions) if (a.action == action) def = a.def;
            int stored = 0;
            try { stored = PlayerPrefs.GetInt("cb.key." + action, (int)def); } catch { stored = (int)def; }
            cache[action] = k = (Key)stored;
            return k;
        }

        public static void Set(string action, Key key) { cache[action] = key; PlayerPrefs.SetInt("cb.key." + action, (int)key); PlayerPrefs.Save(); }

        public static void ResetAll() { foreach (var a in Actions) Set(a.action, a.def); }

        static KeyControl Control(string action)
        {
            var kb = Keyboard.current; var k = Get(action);
            return kb == null || k == Key.None ? null : kb[k];
        }

        public static bool Pressed(string action) => Control(action)?.wasPressedThisFrame ?? false;
        public static bool Held(string action) => Control(action)?.isPressed ?? false;
        public static string Label(string action, string english) => Loc.T("control." + action, english);

        public static string Name(string action) { var k = Get(action); return k == Key.None ? "—" : k.ToString(); }
    }
}
