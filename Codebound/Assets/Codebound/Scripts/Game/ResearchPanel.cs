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
            GUILayout.Label("Research", title);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<color=#ffd43b>Ship rebuilt: {r.MainLineDone}/{r.MainLineTotal}</color>   (T / Esc to close)", label);
            GUILayout.EndHorizontal();
            GUILayout.Label("Research runs in a <b>Research Lab</b> commanded by a program. Put the cost items in the lab (E), then:  " +
                            "<color=#9cdcfe>lab.research(\"logistics\")</color>   ·   SQL: <color=#9cdcfe>INSERT INTO commands VALUES ('m5','research','logistics');</color>", small);
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var group in Research.Tree.GroupBy(t => t.MainLine ? "Main line — rebuild the ship" : t.Area))
            {
                GUILayout.Space(6);
                GUILayout.Label($"<b>{group.Key}</b>", label);
                foreach (var t in group)
                {
                    string status = r.Status(t);
                    string col = status == "done" ? "#5ef0a0" : status == "available" ? "#ffe066" : "#8a8f99";
                    var cost = string.Join(", ", t.Cost.Select(c => $"{c.Count} {ItemDatabase.Get(c.ItemId).DisplayName}"));
                    var sb = new StringBuilder();
                    sb.Append($"<color={col}><b>{t.Name}</b> [{status}]</color>  <color=#8a8f99>id:</color> <color=#9cdcfe>{t.Id}</color>  · {t.Seconds:0}s\n");
                    sb.Append($"{t.Description}\n<size=11>Cost: {cost}");
                    if (t.Prereqs.Length > 0) sb.Append("   Needs: " + string.Join(", ", t.Prereqs.Select(p => Research.Get(p).Name)));
                    if (t.Unlocks.Length > 0) sb.Append("   Unlocks: " + string.Join(", ", t.Unlocks.Select(b => BuildingDatabase.Get(b).DisplayName)));
                    sb.Append("</size>");
                    GUILayout.Label(sb.ToString(), small);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
