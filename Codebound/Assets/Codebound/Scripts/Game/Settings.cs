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
        // PlayerPrefs is the registry on Windows: far too slow to read per building per frame, so values are cached here
        static readonly Dictionary<string, int> ints = new Dictionary<string, int>();
        static readonly Dictionary<string, float> floats = new Dictionary<string, float>();
        static int I(string k, int d) { if (ints.TryGetValue(k, out var v)) return v; try { v = PlayerPrefs.GetInt(k, d); } catch { v = d; } return ints[k] = v; }
        static float F(string k, float d) { if (floats.TryGetValue(k, out var v)) return v; try { v = PlayerPrefs.GetFloat(k, d); } catch { v = d; } return floats[k] = v; }
        static void Set(string k, int v) { if (ints.TryGetValue(k, out var old) && old == v) return; ints[k] = v; PlayerPrefs.SetInt(k, v); PlayerPrefs.Save(); Version++; }
        static void Set(string k, float v) { if (floats.TryGetValue(k, out var old) && old == v) return; floats[k] = v; PlayerPrefs.SetFloat(k, v); PlayerPrefs.Save(); Version++; }

        /// <summary>Back to the defaults (the options screen's "Restore defaults").</summary>
        public static void Reset(params string[] keys)
        {
            foreach (var k in keys) { ints.Remove(k); floats.Remove(k); PlayerPrefs.DeleteKey(k); }
            PlayerPrefs.Save(); Version++;
        }

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

        // ---- interface
        /// <summary>No slides, fades or moving menu background (accessibility; also follows "reduce flashes").</summary>
        public static bool ReduceMotion { get => I("cb.reduceMotion", 0) == 1 || ReduceEffects; set => Set("cb.reduceMotion", value ? 1 : 0); }
        public static float UiVolume { get => Mathf.Clamp01(F("cb.vol.ui", 0.7f)); set => Set("cb.vol.ui", Mathf.Clamp01(value)); }
        /// <summary>The frame-rate counter in the corner of the HUD.</summary>
        public static bool ShowFps { get => I("cb.showFps", 1) == 1; set => Set("cb.showFps", value ? 1 : 0); }
        /// <summary>Pop-up notifications of the HUD (research done, waves, skills). Alarms with sound still play.</summary>
        public static bool Notifications { get => I("cb.notifications", 1) == 1; set => Set("cb.notifications", value ? 1 : 0); }

        // ---- gameplay
        public static int AutoSaveMinutes { get => Mathf.Clamp(I("cb.autosaveMin", 5), 0, 30); set => Set("cb.autosaveMin", Mathf.Clamp(value, 0, 30)); }

        // ---- graphics (applied by ApplyGraphics; resolution and window mode are also remembered by Unity itself)
        public static readonly int[] FpsLimits = { 30, 60, 120, 144, 240, 0 };
        public static int FpsLimit { get => I("cb.fps", 120); set { Set("cb.fps", value); ApplyGraphics(); } }
        public static bool VSync { get => I("cb.vsync", 0) == 1; set { Set("cb.vsync", value ? 1 : 0); ApplyGraphics(); } }
        public static int Quality { get => Mathf.Clamp(I("cb.quality", QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1)); set { Set("cb.quality", value); ApplyGraphics(); } }

        public static void ApplyGraphics()
        {
            if (Application.isBatchMode) return;
            if (QualitySettings.GetQualityLevel() != Quality) QualitySettings.SetQualityLevel(Quality, true);
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = VSync ? -1 : (FpsLimit <= 0 ? -1 : FpsLimit);
        }

        // ---- network
        public static int NetPort { get => Mathf.Clamp(I("cb.netPort", Multiplayer.Port), 1024, 65535); set => Set("cb.netPort", Mathf.Clamp(value, 1024, 65535)); }
        public static int MaxGuests { get => Mathf.Clamp(I("cb.maxGuests", Multiplayer.MaxGuests), 1, Multiplayer.MaxGuests); set => Set("cb.maxGuests", Mathf.Clamp(value, 1, Multiplayer.MaxGuests)); }
        public static string GuestRole
        {
            get { var r = PlayerPrefs.GetString("cb.guestRole", Multiplayer.Builder); return r == Multiplayer.Visitor || r == Multiplayer.Admin ? r : Multiplayer.Builder; }
            set { PlayerPrefs.SetString("cb.guestRole", value); PlayerPrefs.Save(); Version++; }
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
        /// <summary>Interface scale: the player's choice on top of an automatic factor, so 1440p and 4K look like 1080p, only sharper.</summary>
        public static float S => Settings.UiScale * Auto;
        public static float Auto => Mathf.Clamp(Mathf.Min(Screen.height / 1080f, Screen.width / 1600f), 0.7f, 4f);
        public static float W => Screen.width / S;
        public static float H => Screen.height / S;
        public static void Begin()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(S, S, 1));
            GUI.skin = UiTheme.Skin;   // one industrial look for every screen and panel
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
            ("eat", "Eat", Key.F), ("research", "Research tree", Key.T), ("inventory", "Inventory", Key.Tab), ("journal", "Journal", Key.J), ("network", "Multiplayer", Key.N), ("chat", "Chat", Key.Enter),
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
