using System.Linq;
using System.Text;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>T: overview of the tech tree — main line (rebuild the ship) and branches. Research itself runs in labs,
    /// driven by programs (lab.research("id")). shortcut: IMGUI until the in-game UI framework.</summary>
    public sealed class ResearchPanel : MonoBehaviour
    {
        GameSession session;
        bool open;
        Vector2 scroll;
        GUIStyle box, label, small, title;

        public void Init(GameSession s) { session = s; }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || session.KeyboardCaptured) return;
            if (Controls.Pressed("research") || (open && kb.escapeKey.wasPressedThisFrame))
            {
                if (!open && (session.PanelOpen || OptionsMenu.IsOpen)) return;   // another panel owns the screen
                open = !open; session.PanelOpen = open;
            }
        }

        void Styles()
        {
            if (box != null) return;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.05f, 0.06f, 0.09f, 0.96f)); bg.Apply();
            box = new GUIStyle(GUI.skin.box) { normal = { background = bg }, padding = new RectOffset(12, 12, 10, 10) };
            label = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, wordWrap = true, normal = { textColor = new Color(0.86f, 0.95f, 0.9f) } };
            small = new GUIStyle(label) { fontSize = 12 };
            title = new GUIStyle(label) { fontSize = 18, fontStyle = FontStyle.Bold };
        }

        void OnGUI()
        {
            UiGui.Begin();
            if (!open) return;
            Styles();
            var r = session.State.Research;
            var rect = new Rect(UiGui.W * 0.12f, UiGui.H * 0.06f, UiGui.W * 0.76f, UiGui.H * 0.84f);
            GUILayout.BeginArea(rect, box);
            GUILayout.BeginHorizontal();
            GUILayout.Label(Loc.T("research.title", "Research"), title);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<color=#ffd43b>{Loc.T("research.ship", "Ship rebuilt: {0}/{1}", r.MainLineDone, r.MainLineTotal)}</color>   {Loc.T("research.close", "(T / Esc to close)")}", label);
            GUILayout.EndHorizontal();
            GUILayout.Label(Loc.T("research.how", "Research runs in a Research Lab commanded by a program. Put the cost items in the lab (E), then:") + "  " +
                            "<color=#9cdcfe>lab.research(\"logistics\")</color>   ·   SQL: <color=#9cdcfe>INSERT INTO commands VALUES ('m5','research','logistics');</color>", small);
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var group in Research.Tree.GroupBy(t => t.MainLine ? Loc.T("research.mainline", "Main line — rebuild the ship") : Loc.T("area." + t.Area, t.Area)))
            {
                GUILayout.Space(6);
                GUILayout.Label($"<b>{group.Key}</b>", label);
                foreach (var t in group)
                {
                    string status = r.Status(t);
                    string statusText = Loc.T("research." + status, status);
                    string col = status == "done" ? "#5ef0a0" : status == "available" ? "#ffe066" : "#8a8f99";
                    var cost = string.Join(", ", t.Cost.Select(c => $"{c.Count} {Loc.Item(c.ItemId)}"));
                    var sb = new StringBuilder();
                    sb.Append($"<color={col}><b>{Loc.Tech(t)}</b> [{statusText}]</color>  <color=#8a8f99>id:</color> <color=#9cdcfe>{t.Id}</color>  · {t.Seconds:0}s\n");
                    sb.Append($"{t.Description}\n<size=11>{Loc.T("research.cost", "Cost")}: {cost}");
                    if (t.Prereqs.Length > 0) sb.Append($"   {Loc.T("research.needs", "Needs")}: " + string.Join(", ", t.Prereqs.Select(p => Loc.Tech(Research.Get(p)))));
                    if (t.Unlocks.Length > 0) sb.Append($"   {Loc.T("research.unlocks", "Unlocks")}: " + string.Join(", ", t.Unlocks.Select(b => Loc.Building(BuildingDatabase.Get(b)))));
                    sb.Append("</size>");
                    GUILayout.Label(sb.ToString(), small);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
