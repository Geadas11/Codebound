using System.Collections.Generic;
using System.Text;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>Ruins outposts: draws terminals/caches near the player, E reads a terminal or opens a cache,
    /// J shows the journal of recovered logs and a scanner bearing to the nearest unread outpost.</summary>
    public sealed class ExplorationController : MonoBehaviour
    {
        public const int ViewRadius = 48, ScannerRadius = 240;
        GameSession session;
        BuildController build;
        Exploration ex;
        readonly Dictionary<Cell, (SpriteRenderer terminal, SpriteRenderer cache)> views = new Dictionary<Cell, (SpriteRenderer, SpriteRenderer)>();
        float refresh;
        int shownLog = -1;    // log on screen, or -1
        bool journal;
        Vector2 scroll;
        GUIStyle box, label, title, small;
        int styleVersion = -1;

        public void Init(GameSession s, BuildController b) { session = s; build = b; ex = s.State.Exploration; }

        Cell PlayerCell => new Cell(Mathf.FloorToInt(session.Player.position.x), Mathf.FloorToInt(session.Player.position.y));

        void Update()
        {
            if ((refresh -= Time.deltaTime) <= 0) { refresh = 0.5f; RefreshViews(); }
            var kb = Keyboard.current;
            if (kb == null || session.KeyboardCaptured) return;
            bool showing = shownLog >= 0 || journal;
            if (showing && (kb.escapeKey.wasPressedThisFrame || Controls.Pressed("interact") || Controls.Pressed("journal")))
            {
                shownLog = -1; journal = false; session.PanelOpen = false;
                return;
            }
            if (showing || session.PanelOpen || OptionsMenu.IsOpen || build.Active) return;
            if (Controls.Pressed("journal")) { journal = true; session.PanelOpen = true; return; }
            if (!Controls.Pressed("interact")) return;

            var cell = build.MouseCell();
            var f = ex.FacilityAt(cell);
            if (f == null || Vector2.Distance(new Vector2(cell.X + 0.5f, cell.Y + 0.5f), session.Player.position) > MiningController.Reach) return;
            if (Net.IsGuest) Net.SendAct(new System.Collections.Generic.Dictionary<string, object> { { "a", "explore" }, { "x", (double)cell.X }, { "y", (double)cell.Y } });
            if (cell.Equals(f.Terminal))
            {
                int before = ex.LogsRead;
                shownLog = ex.Read(f);
                session.PanelOpen = true;
                if (ex.LogsRead > before) session.State.Hud.Notify(Loc.T("notify.log", "Log recovered: {0}", Lore.Title(shownLog)));
            }
            else if (!Net.IsGuest)
            {
                string err = ex.Loot(f, session.Inventory);
                session.State.Hud.Notify(err == null ? Loc.T("notify.loot", "Supply cache opened at {0}", f.Name)
                                       : err == "empty" ? Loc.T("explore.empty", "This cache is empty") : Loc.T("explore.full", "Inventory full"));
                RefreshViews();
            }
        }

        void RefreshViews()
        {
            var near = new HashSet<Cell>();
            foreach (var f in ex.Near(PlayerCell, ViewRadius))
            {
                near.Add(f.Terminal);
                if (!views.TryGetValue(f.Terminal, out var v))
                    views[f.Terminal] = v = (Make("Terminal", f.Terminal), Make("Cache", f.Cache));
                v.terminal.sprite = PixelArtFactory.Facility("terminal", false);
                v.cache.sprite = PixelArtFactory.Facility("cache", ex.IsLooted(f));
            }
            var gone = new List<Cell>();
            foreach (var kv in views) if (!near.Contains(kv.Key)) gone.Add(kv.Key);
            foreach (var c in gone) { Destroy(views[c].terminal.gameObject); Destroy(views[c].cache.gameObject); views.Remove(c); }
        }

        SpriteRenderer Make(string name, Cell c)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(c.X, c.Y, 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            return sr;
        }

        static string Bearing(int dx, int dy)
        {
            string ns = dy > Mathf.Abs(dx) / 2 ? "N" : dy < -Mathf.Abs(dx) / 2 ? "S" : "";
            string ew = dx > Mathf.Abs(dy) / 2 ? "E" : dx < -Mathf.Abs(dy) / 2 ? "W" : "";
            return ns + ew;
        }

        /// <summary>Scanner text: nearest outpost whose terminal has not been read.</summary>
        string Scanner()
        {
            var p = PlayerCell;
            Facility best = null; float bd = float.MaxValue;
            foreach (var f in ex.Near(p, ScannerRadius))
            {
                if (ex.IsRead(f)) continue;
                float d = Vector2.Distance(new Vector2(f.Terminal.X, f.Terminal.Y), new Vector2(p.X, p.Y));
                if (d < bd && d <= ScannerRadius) { bd = d; best = f; }
            }
            return best == null ? Loc.T("explore.nosignal", "Scanner: no unread outpost within {0} cells", ScannerRadius)
                                : Loc.T("explore.signal", "Scanner: unread outpost {0} cells {1}", Mathf.RoundToInt(bd), Bearing(best.Terminal.X - p.X, best.Terminal.Y - p.Y));
        }

        void Styles()
        {
            if (box != null && styleVersion == Settings.Version) return;
            styleVersion = Settings.Version;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, Settings.HighContrast ? Color.black : new Color(0.03f, 0.07f, 0.05f, 0.97f)); bg.Apply();
            box = new GUIStyle(GUI.skin.box) { normal = { background = bg }, padding = new RectOffset(16, 16, 12, 12) };
            label = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(15), richText = true, wordWrap = true, normal = { textColor = new Color(0.55f, 1f, 0.65f) } };
            small = new GUIStyle(label) { fontSize = UiGui.Font(12), normal = { textColor = new Color(0.6f, 0.75f, 0.65f) } };
            title = new GUIStyle(label) { fontSize = UiGui.Font(19), fontStyle = FontStyle.Bold };
        }

        void OnGUI()
        {
            UiGui.Begin();
            if (shownLog < 0 && !journal) return;
            Styles();
            float w = Mathf.Min(640, UiGui.W - 40), h = Mathf.Min(UiGui.H - 60, 460);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            if (journal)
            {
                GUILayout.Label(Loc.T("explore.journal", "Journal — recovered logs ({0}/{1})", ex.LogsRead, Lore.Logs.Length), title);
                GUILayout.Label(Scanner(), small);
                scroll = GUILayout.BeginScrollView(scroll);
                var sb = new StringBuilder();
                foreach (int i in ex.ReadLogIndices) sb.Append("<b>").Append(Lore.Title(i)).Append("</b>\n").Append(Lore.Text(i)).Append("\n\n");
                GUILayout.Label(sb.Length == 0 ? Loc.T("explore.nologs", "No logs yet. Survey outposts stand in the Ruins biome — read their terminals (E).") : sb.ToString(), label);
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label(Lore.Title(shownLog), title);
                scroll = GUILayout.BeginScrollView(scroll);
                GUILayout.Label(Lore.Text(shownLog), label);
                GUILayout.EndScrollView();
            }
            GUILayout.Label(Loc.T("explore.close", "({0} / Esc to close · {1} journal)", Controls.Name("interact"), Controls.Name("journal")), small);
            GUILayout.EndArea();
        }
    }
}
