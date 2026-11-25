using System;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// The front end, shown over a paused world once per launch (and again after "Exit to main menu"): the profile entry
    /// screen, then the main menu and its screens (Game/Ui). Skipped for -selftest, -host/-join, -seed and batch mode.
    /// </summary>
    public sealed class TitleMenu : MonoBehaviour
    {
        static bool shownThisLaunch, showAgain;
        static TitleMenu instance;
        GameSession session;
        bool open;
        UiNav nav;
        int accountFrame = -10;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { shownThisLaunch = showAgain = false; instance = null; IsOpen = false; }

        public static bool IsOpen { get; private set; }

        /// <summary>The next scene load shows the main menu again (pause menu → Exit to main menu).</summary>
        public static void ShowAgain() => showAgain = true;

        public void Init(GameSession s, GameBootstrap b)
        {
            session = s; instance = this;
            var args = Environment.GetCommandLineArgs();
            bool skip = Application.isBatchMode || SelfTest.Requested || Array.IndexOf(args, "-host") >= 0 || Array.IndexOf(args, "-join") >= 0
                        || Array.IndexOf(args, "-seed") >= 0 || Net.Current != Net.Mode.Off;
            if ((shownThisLaunch && !showAgain) || skip) { shownThisLaunch = true; showAgain = false; IsOpen = false; Time.timeScale = 1; return; }
            shownThisLaunch = true; showAgain = false;
            if (FrontEnd.Player != null) Net.MyName = FrontEnd.Player.Name;
            nav = new UiNav();
            nav.OnEmpty = () => nav.Ask(Loc.T("ui.quitQ", "Quit Codebound?"), null, Loc.T("menu.quit", "Quit game"), Application.Quit);
            nav.Reset(FrontEnd.Player == null ? new ProfilesScreen(true) : (UiScreen)new MainScreen());
            Open();
        }

        void Open() { open = IsOpen = true; session.PanelOpen = true; session.KeyboardCaptured = true; Time.timeScale = 0; MenuStage.Open(); }

        void Close() { open = IsOpen = false; session.PanelOpen = false; session.KeyboardCaptured = false; Time.timeScale = 1; }

        /// <summary>A world is about to load from the menus: the menu steps aside.</summary>
        public static void Leaving() { if (instance != null && instance.open) instance.Close(); }

        /// <summary>The load failed: the menu comes back.</summary>
        public static void Back() { if (instance != null && !instance.open && instance.nav != null) instance.Open(); }

        /// <summary>Opens a world from any menu screen, with a loading state and a clear error if it cannot be read.</summary>
        public static void Play(UiNav nav, WorldInfo w)
        {
            if (w == null || !w.Loadable) return;
            nav.Run(Loc.T("ui.loading", "Loading {0}…", w.Name), () =>
            {
                bool title = instance != null && instance.open;
                Leaving();
                if (GameBootstrap.OpenWorld(w.Id, out var err)) return;
                if (title) Back();
                nav.Tell(Loc.T("ui.loadFailed", "Could not load the world"), err);
            });
        }

        void Update()
        {
            if (!open) return;
            if (AccountMenu.IsOpen) { accountFrame = Time.frameCount; return; }
            if (Time.frameCount <= accountFrame + 1) return;   // the Esc that closed the account panel is not also a Back
            nav.Update();
        }

        void OnGUI()
        {
            if (!open || AccountMenu.IsOpen) return;
            UiGui.Begin();
            GUI.depth = -10;
            var full = new Rect(0, 0, UiGui.W, UiGui.H);
            var stage = MenuStage.Instance;
            bool onTerminal = stage != null && MenuStage.Active && (nav.Depth > 1 || nav.Busy != null || nav.Blocked);
            if (onTerminal && stage.Progress > 0.02f) DrawOnTerminal(stage);
            else nav.Draw(full);
            Ui.DrawToasts();
        }

        float powered;   // 0..1: the terminal glass powering on (and off when the camera leaves)

        /// <summary>The screens opened from the main menu, drawn on the terminal's glass: power-on, scanlines, reflection.</summary>
        void DrawOnTerminal(MenuStage stage)
        {
            powered = Mathf.MoveTowards(powered, stage.AtTerminal ? 1 : 0, Ui.Dt / 0.35f);
            if (!Ui.Animate) powered = stage.AtTerminal ? 1 : 0;
            if (powered <= 0) return;
            var glass = stage.ScreenRect();
            // power-on: a bright line opens vertically into the picture
            float open = Mathf.SmoothStep(0, 1, Mathf.Clamp01((powered - 0.15f) / 0.85f));
            float h = Mathf.Max(2, glass.height * open);
            var visible = new Rect(glass.x, glass.center.y - h / 2, glass.width, h);
            if (Event.current.type == EventType.Repaint && powered < 1)
            {
                float lineW = glass.width * Mathf.Clamp01(powered / 0.15f);
                Ui.Fill(new Rect(glass.center.x - lineW / 2, glass.center.y - 1.5f, lineW, 3), new Color(1f, 0.85f, 0.6f, 1 - open * 0.8f));
            }
            if (open <= 0.01f) return;
            MenuBackdrop.Terminal = true;
            GUI.BeginClip(visible);
            nav.Draw(new Rect(0, (visible.height - glass.height) / 2, glass.width, glass.height));
            GUI.EndClip();
            MenuBackdrop.Terminal = false;
            MenuBackdrop.Glass(visible);
        }
    }
}
