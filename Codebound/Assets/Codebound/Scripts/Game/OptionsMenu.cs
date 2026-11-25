using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// Esc in game: the pause menu (resume, save, load another world, options, multiplayer, account, exit to the main menu,
    /// quit), drawn with the same screens and look as the title menu. Single player pauses the world; a hosted game keeps
    /// running for the guests. Also draws the toasts of in-game actions.
    /// </summary>
    public sealed class OptionsMenu : MonoBehaviour
    {
        GameSession session;
        GameBootstrap boot;
        BuildController build;
        bool open;
        float prevTimeScale = 1;
        UiNav nav;
        int accountFrame = -10;

        public static bool IsOpen { get; private set; }

        public void Init(GameSession s, GameBootstrap b, BuildController bc) { session = s; boot = b; build = bc; }

        void Open()
        {
            open = IsOpen = true; session.PanelOpen = true;
            prevTimeScale = Time.timeScale;
            if (Net.Current == Net.Mode.Off) Time.timeScale = 0;   // single player: the world pauses (a host's guests keep playing)
            nav = new UiNav { OnEmpty = Close };
            nav.Reset(new PauseScreen(this));
        }

        public void Close()
        {
            if (!open) return;
            open = IsOpen = false; session.PanelOpen = false;
            if (Net.Current == Net.Mode.Off || Time.timeScale == 0) Time.timeScale = prevTimeScale <= 0 ? 1 : prevTimeScale;
        }

        void OnDestroy() { if (open) Time.timeScale = 1; IsOpen = false; }

        void Update()
        {
            session.State.Creatures.Enabled = !Settings.Peaceful;
            var kb = Keyboard.current;
            if (!open)
            {
                // Esc opens the menu only when nothing else is using it (panels and build mode close themselves first)
                bool esc = (kb != null && kb.escapeKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
                if (esc && !blockedLastFrame && !session.PanelOpen && !session.KeyboardCaptured && !build.Active && !TitleMenu.IsOpen && !AccountMenu.IsOpen) Open();
                return;
            }
            if (AccountMenu.IsOpen) { accountFrame = Time.frameCount; return; }
            if (Time.frameCount <= accountFrame + 1) return;
            nav.Update();
        }

        // Esc that closed a panel or left build mode in this frame must not also open the menu (script order varies)
        bool blockedLastFrame;
        void LateUpdate() => blockedLastFrame = !open && (session.PanelOpen || session.KeyboardCaptured || build.Active || AccountMenu.IsOpen);

        void OnGUI()
        {
            UiGui.Begin();
            if (open && !AccountMenu.IsOpen)
            {
                GUI.depth = -5;
                nav.Draw(new Rect(0, 0, UiGui.W, UiGui.H));
            }
            if (!TitleMenu.IsOpen) Ui.DrawToasts();
        }

        /// <summary>The root of the pause menu.</summary>
        sealed class PauseScreen : UiScreen
        {
            readonly OptionsMenu m;
            int sel;
            public PauseScreen(OptionsMenu m) { this.m = m; }
            public override string Title => Net.Current == Net.Mode.Off ? Loc.T("ui.paused", "Paused") : Loc.T("ui.menu", "Menu");
            public override bool IsRoot => true;

            (string text, string hint, bool enabled, System.Action go)[] Items()
            {
                bool guest = Net.IsGuest;
                return new (string, string, bool, System.Action)[]
                {
                    (Loc.T("menu.resume", "Resume"), "Esc", true, m.Close),
                    (Loc.T("menu.save", "Save game"), guest ? Loc.T("ui.hostSaves", "In multiplayer the host saves the world") : Controls.Name("save"), !guest, () => { m.boot.Save(); Ui.Toast(Loc.T("ui.saved", "Saved"), "good"); }),
                    (Loc.T("ui.loadWorld", "Load world"), guest ? Loc.T("ui.leaveFirst", "Leave the multiplayer game first") : Loc.T("ui.loadHint", "This world is saved first"), !guest, () => Nav.Push(new WorldsScreen(w =>
                    {
                        if (GameBootstrap.CurrentWorld != null) m.boot.Save();
                        Online.ForgetJoin(); Online.Unpublish(); Net.Disconnect();
                        TitleMenu.Play(Nav, w);
                    }, Loc.T("ui.play", "Play")))),
                    (Loc.T("ui.options", "Options"), Loc.T("ui.optionsHint", "Graphics, audio, controls, accessibility"), true, () => Nav.Push(new OptionsScreen(true))),
                    (Loc.T("ui.multiplayer", "Multiplayer"), Net.Current == Net.Mode.Off ? Loc.T("ui.mpPanelHint", "Host this world or join one ({0})", Controls.Name("network")) : Net.Status, true, () => { m.Close(); NetSession.OpenPanel(); }),
                    (Loc.T("ui.onlineAccount", "Online account"), Online.StatusLine(), true, AccountMenu.Open),
                    (Loc.T("ui.toTitle", "Exit to main menu"), guest ? Loc.T("ui.leavesGame", "Leaves the multiplayer game") : Loc.T("ui.savesFirst", "The world is saved first"), true,
                        () => Nav.Ask(Loc.T("ui.toTitleQ", "Exit to the main menu?"), Net.IsHost ? Loc.T("ui.closesServer", "This closes the server: the other players are disconnected.") : null, Loc.T("ui.toTitle", "Exit to main menu"), () => m.boot.ReturnToTitle())),
                    (Loc.T("menu.quit", "Quit game"), guest ? "" : Loc.T("ui.savesFirst", "The world is saved first"), true,
                        () => Nav.Ask(Loc.T("ui.quitQ", "Quit Codebound?"), null, Loc.T("menu.quit", "Quit game"), () => { if (!Net.IsGuest) m.boot.Save(); Application.Quit(); })),
                };
            }

            public override void Update()
            {
                var items = Items();
                int d = Ui.NavVertical();
                if (d != 0) { for (int k = 0; k < items.Length; k++) { sel = (sel + d + items.Length) % items.Length; if (items[sel].enabled) break; } Ui.Click(); }
                if (Ui.NavConfirm() && items[sel].enabled) { Ui.Confirm(); items[sel].go(); }
            }

            public override void Draw(Rect r)
            {
                Ui.Fill(r, new Color(0.02f, 0.025f, 0.03f, 0.82f));
                float w = Mathf.Min(460, r.width - 40), x = r.x + Mathf.Max(30, r.width * 0.07f), y = r.y + Mathf.Max(30, r.height * 0.12f);
                Ui.Fill(new Rect(x, y + 6, 6, 44), UiTheme.Accent);
                GUI.Label(new Rect(x + 20, y, w, 56), Title.ToUpperInvariant(), new GUIStyle(UiTheme.H1) { fontSize = UiGui.Font(40) });
                string where = GameBootstrap.CurrentWorld != null ? WorldStore.Inspect(GameBootstrap.SavesDir, GameBootstrap.CurrentWorld).Name : Loc.T("ui.unsavedWorld", "World without a save slot (F5 → quicksave)");
                if (Net.IsGuest) where = Loc.T("ui.guestIn", "Guest in {0}'s world", Net.Others.Count > 0 ? Net.Others[0].name : "host");
                GUI.Label(new Rect(x + 22, y + 56, w, 22), where, UiTheme.Muted_);
                var items = Items();
                float iy = y + 100;
                for (int i = 0; i < items.Length; i++)
                {
                    var it = items[i];
                    var row = new Rect(x, iy + i * 58 + (i >= items.Length - 2 ? 14 : 0), w, 52);
                    if (row.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseMove && it.enabled) sel = i;
                    var style = i == sel && it.enabled ? new GUIStyle(UiTheme.MenuItem) { normal = UiTheme.MenuItem.hover } : UiTheme.MenuItem;
                    if (Ui.Button(row, "", style, it.enabled)) { sel = i; it.go(); }
                    GUI.Label(new Rect(row.x + 22, row.y + 4, row.width - 30, 28), it.text, new GUIStyle(UiTheme.H1) { fontSize = UiGui.Font(19), normal = { textColor = it.enabled ? (i == sel ? Color.white : UiTheme.Text) : UiTheme.Dim } });
                    if (!string.IsNullOrEmpty(it.hint)) GUI.Label(new Rect(row.x + 23, row.y + 29, row.width - 30, 20), it.hint, UiTheme.MenuHint);
                }
                GUI.Label(new Rect(x, r.yMax - 40, 700, 22), FrontEnd.Version + "   ·   " + Loc.T("ui.navHelp", "↑ ↓ choose · Enter confirm · Esc back"), UiTheme.Small);
            }
        }
    }
}
