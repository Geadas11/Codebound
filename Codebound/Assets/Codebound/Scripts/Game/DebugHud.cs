using System.Text;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// shortcut: IMGUI developer HUD for Etapa 1. The real HUD (stable frame + movable, resizable,
    /// player-programmable HTML/CSS panels with the "+" button) is Etapa 4 and will replace this.
    /// </summary>
    public sealed class DebugHud : MonoBehaviour
    {
        GameSession session;
        BuildController build;
        MiningController mining;
        GameBootstrap boot;
        bool showInventory = true;
        float fps;
        GUIStyle box, label, small, btn;
        Rect barRect;

        public void Init(GameSession s, BuildController b, MiningController m, GameBootstrap gb) { session = s; build = b; mining = m; boot = gb; }

        void Update()
        {
            // IMGUI bars swallow world clicks (screen y is bottom-up, IMGUI rects top-down)
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                var mp = mouse.position.ReadValue();
                session.PointerOverImgui = barRect.Contains(UiGui.Mouse(mp));
            }
            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-4f), 0.05f);
            var kb = Keyboard.current;
            if (kb != null && !session.KeyboardCaptured && Controls.Pressed("inventory")) showInventory = !showInventory;
        }

        void Styles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box) { normal = { background = Tex(new Color(0.06f, 0.07f, 0.1f, 0.82f)) }, padding = new RectOffset(8, 8, 6, 6) };
            label = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(0.85f, 0.95f, 0.9f) }, richText = true };
            small = new GUIStyle(label) { fontSize = 12 };
        }

        static Texture2D Tex(Color c) { var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t; }

        void OnGUI()
        {
            UiGui.Begin();
            if (session == null) return;
            Styles();
            var p = session.Player.position;
            var pc = new Cell(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
            var gen = session.World.Generator;

            // status
            GUILayout.BeginArea(new Rect(10, 10, 330, 184), box);
            GUILayout.Label("<b><color=#5ef0a0>CODEBOUND</color></b>  <size=11>BUILD. CODE. SURVIVE. EXPLORE.</size>", label);
            var clk = session.State.Clock; var env = session.Surroundings;
            GUILayout.Label($"Seed {session.Seed}   FPS {fps:0}   Day {clk.Day} {clk.ClockText} {clk.Season}", small);
            GUILayout.Label($"{WorldClock.WeatherName(env.Weather, gen.BiomeAt(pc))}   air {env.AmbientC:0}°C{(env.Breathable ? "" : "  <color=#ffb347>unbreathable</color>")}{(env.Radiation > 0 ? "  <color=#9cff6b>radiation</color>" : "")}", small);
            GUILayout.Label($"Pos {pc}  Biome {gen.BiomeAt(pc)}  Tile {gen.TileAt(pc)}", small);
            var hd = session.World.DepositAt(mining.HoverCell);
            var hb = session.Grid.At(mining.HoverCell);
            string hover = hb != null ? Loc.Building(hb.Def) + (hb.State != null ? $" [{hb.State.Status}]  E to open" : "") : hd.IsNone ? gen.TileAt(mining.HoverCell).ToString() : $"{hd.Type} x{hd.Amount}";
            GUILayout.Label($"Cursor {mining.HoverCell}: {hover}", small);
            var vt = session.State.Vitals;
            string V(float v) => v < 25 ? $"<color={Settings.Bad}>{v:0}</color>" : $"{v:0}";
            string warn = vt.Warning != "" ? $"  <color=#ffb347><b>{vt.Warning}</b></color>" : "";
            GUILayout.Label($"HP {V(vt.Health)}  Food {V(vt.Hunger)}  Water {V(vt.Thirst)}  O2 {V(vt.Oxygen)}  {vt.BodyTemp:0.0}°C{warn}", small);
            GUILayout.EndArea();

            // mining progress
            if (mining.Progress01 > 0)
            {
                var r = new Rect(UiGui.W / 2f - 60, UiGui.H / 2f + 40, 120, 10);
                GUI.DrawTexture(r, Texture2D.blackTexture);
                GUI.DrawTexture(new Rect(r.x + 1, r.y + 1, (r.width - 2) * mining.Progress01, r.height - 2), Texture2D.whiteTexture);
            }

            // inventory
            if (showInventory)
            {
                var sb = new StringBuilder();
                int lines = 0;
                for (int i = 0; i < session.Inventory.SlotCount; i++)
                {
                    var st = session.Inventory[i];
                    if (st.IsEmpty) continue;
                    sb.Append(Loc.Item(st.ItemId)).Append("  x").Append(st.Count).Append('\n');
                    lines++;
                }
                GUILayout.BeginArea(new Rect(UiGui.W - 240, 10, 230, 52 + Mathf.Max(1, lines) * 16), box);
                GUILayout.Label($"<b>{Loc.T("hud.inventory", "Inventory")}</b> <size=11>({Controls.Name("inventory")})</size>", label);
                GUILayout.Label(sb.Length == 0 ? "<i>" + Loc.T("hud.inventory.empty", "empty — hold LMB on trees/ore") + "</i>" : sb.ToString(), small);
                GUILayout.EndArea();
            }

            // build bar: clickable, wraps into rows (keys 1-9,0 select the first ten)
            int n = BuildingDatabase.All.Count;
            float barW = Mathf.Min(UiGui.W - 20, 1120);
            int perRow = Mathf.Max(1, Mathf.FloorToInt((barW - 16) / 104f));
            int rows = Mathf.CeilToInt(n / (float)perRow);
            float barH = rows * 46 + 14;
            barRect = new Rect(10, UiGui.H - barH - 8, barW, barH);
            if (btn == null) btn = new GUIStyle(GUI.skin.button) { richText = true, fontSize = 11, alignment = TextAnchor.UpperLeft, wordWrap = true, padding = new RectOffset(4, 4, 3, 3) };
            GUILayout.BeginArea(barRect, box);
            for (int r = 0; r < rows; r++)
            {
                GUILayout.BeginHorizontal();
                for (int i = r * perRow; i < Mathf.Min(n, (r + 1) * perRow); i++)
                {
                    var d = BuildingDatabase.All[i];
                    bool sel = build.Active && build.Selected == i;
                    bool can = session.Inventory.Has(d.Cost);
                    var cost = new StringBuilder();
                    foreach (var c in d.Cost) cost.Append(c.Count).Append(' ').Append(Loc.Item(c.ItemId)).Append(", ");
                    string col = sel ? "#ffe066" : can ? "#d8f5e6" : "#8a8f99";
                    string key = i < 10 ? $"<b>{(i + 1) % 10}</b> " : "";
                    var locker = session.State.Research.IsUnlocked(d) ? null : Research.UnlockerOf(d.Id);
                    if (locker != null) { col = "#5a5f69"; cost.Clear(); cost.Append(Loc.T("hud.needs", "research: {0}", Loc.Tech(locker))); }
                    if (GUILayout.Button($"<color={col}>{key}{Loc.Building(d)}\n<size=9>{cost.ToString().TrimEnd(',', ' ')}</size></color>", btn, GUILayout.Width(100), GUILayout.Height(42)))
                        build.Select(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
            string mode = build.Active ? Loc.T("hud.buildmode", "BUILD MODE — R rotate ({0}) · LMB place · RMB remove · Esc exit", build.Rotation) : Loc.T("hud.help", "{0} build · hold LMB mine/drink · {1} eat · {2} open machine · {3} research · move {4} · wheel zoom · Esc options", Controls.Name("build"), Controls.Name("eat"), Controls.Name("interact"), Controls.Name("research"), Controls.Name("up") + Controls.Name("left") + Controls.Name("down") + Controls.Name("right"));
            GUI.Label(new Rect(14, barRect.y - 22, 760, 22), mode, small);

            // messages (whichever is newest, shown for 3 s)
            string msg = build.LastMessage; float t = build.MessageTime;
            if (mining.MessageTime > t) { msg = mining.LastMessage; t = mining.MessageTime; }
            if (boot.MessageTime > t) { msg = boot.LastMessage; t = boot.MessageTime; }
            if (!string.IsNullOrEmpty(msg) && Time.time - t < 3f)
                GUI.Label(new Rect(UiGui.W / 2f - 250, 60, 500, 24), $"<b>{msg}</b>", new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
        }
    }
}
