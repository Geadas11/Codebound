using System;
using System.IO;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// Start screen, shown once per launch over a paused world: Continue (newest of quicksave/autosave), New game
    /// (optional seed), language, quit. Skipped for -selftest, -host/-join, -seed and batch mode.
    /// </summary>
    public sealed class TitleMenu : MonoBehaviour
    {
        static bool shownThisLaunch;
        GameSession session;
        GameBootstrap boot;
        bool open;
        string seedText = "";
        int selected;
        GUIStyle box, title, label, btn, btnSel, small;
        int styleVersion = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => shownThisLaunch = false;

        public static bool IsOpen { get; private set; }

        public void Init(GameSession s, GameBootstrap b)
        {
            session = s; boot = b;
            var args = Environment.GetCommandLineArgs();
            bool skip = Application.isBatchMode || SelfTest.Requested || Array.IndexOf(args, "-host") >= 0 || Array.IndexOf(args, "-join") >= 0
                        || Array.IndexOf(args, "-seed") >= 0 || Net.Current != Net.Mode.Off;
            if (shownThisLaunch || skip) { shownThisLaunch = true; return; }
            shownThisLaunch = true;
            open = IsOpen = true; session.PanelOpen = true; session.KeyboardCaptured = true;
            Time.timeScale = 0;
        }

        static string Newest()
        {
            string best = null;
            foreach (var p in new[] { GameBootstrap.SavePath, GameBootstrap.AutoSavePath })
                if (File.Exists(p) && (best == null || File.GetLastWriteTimeUtc(p) > File.GetLastWriteTimeUtc(best))) best = p;
            return best;
        }

        void Close() { open = IsOpen = false; session.PanelOpen = false; session.KeyboardCaptured = false; Time.timeScale = 1; }

        string[] Entries => new[]
        {
            Loc.T("title.continue", "Continue"), Loc.T("title.new", "New game"),
            Loc.T("menu.language", "Language") + ": " + LanguageName(), Loc.T("menu.quit", "Quit game"),
        };

        static string LanguageName() { foreach (var l in Loc.Languages) if (l.code == Settings.Language) return l.native; return Settings.Language; }

        void Choose(int i)
        {
            switch (i)
            {
                case 0:
                    var p = Newest();
                    if (p == null) return;
                    Close(); boot.LoadFrom(p); return;
                case 1:
                    Close();
                    if (int.TryParse(seedText.Trim(), out int seed)) GameBootstrap.NewGame(seed);   // otherwise keep the fresh random world already running
                    return;
                case 2:
                    int k = 0; for (int j = 0; j < Loc.Languages.Length; j++) if (Loc.Languages[j].code == Settings.Language) k = j;
                    Settings.Language = Loc.Languages[(k + 1) % Loc.Languages.Length].code; return;
                case 3: Application.Quit(); return;
            }
        }

        void Update()
        {
            if (!open) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.downArrowKey.wasPressedThisFrame) selected = (selected + 1) % 4;
            if (kb.upArrowKey.wasPressedThisFrame) selected = (selected + 3) % 4;
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) Choose(selected);
        }

        void Styles()
        {
            if (box != null && styleVersion == Settings.Version) return;
            styleVersion = Settings.Version;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.05f, 0.86f)); bg.Apply();
            var sel = new Texture2D(1, 1); sel.SetPixel(0, 0, new Color(0.12f, 0.3f, 0.24f)); sel.Apply();
            box = new GUIStyle(GUI.skin.box) { normal = { background = bg } };
            title = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(46), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.37f, 0.94f, 0.63f) } };
            label = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(15), alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(0.8f, 0.9f, 0.85f) } };
            small = new GUIStyle(label) { fontSize = UiGui.Font(12) };
            btn = new GUIStyle(GUI.skin.button) { fontSize = UiGui.Font(18), fixedHeight = 40 };
            btnSel = new GUIStyle(btn) { normal = { background = sel, textColor = Color.white } };
        }

        void OnGUI()
        {
            if (!open) return;
            UiGui.Begin();
            Styles();
            GUI.Box(new Rect(0, 0, UiGui.W, UiGui.H), GUIContent.none, box);
            float w = Mathf.Min(420, UiGui.W - 40);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, UiGui.H * 0.16f, w, UiGui.H * 0.8f));
            GUILayout.Label("CODEBOUND", title);
            GUILayout.Label(Loc.T("title.tagline", "Build. Code. Survive. Explore."), label);
            GUILayout.Space(24);
            var e = Entries;
            bool canContinue = Newest() != null;
            for (int i = 0; i < e.Length; i++)
            {
                GUI.enabled = i != 0 || canContinue;
                if (GUILayout.Button(e[i], i == selected ? btnSel : btn)) { selected = i; Choose(i); }
                if (i == 1)
                {
                    GUI.enabled = true;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Loc.T("title.seed", "Seed (optional)"), small, GUILayout.Width(130));
                    seedText = GUILayout.TextField(seedText, 11);
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(6);
            }
            GUI.enabled = true;
            GUILayout.Label(Loc.T("title.keys", "Up/Down and Enter, or the mouse · options later with Esc"), small);
            GUILayout.EndArea();
        }
    }
}
