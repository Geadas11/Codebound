using System.Text;
using Codebound.Core.Hud;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Editor for one HUD box: title, HTML and CSS tabs, Apply/Revert, problems with line numbers and the list
    /// of data keys. shortcut: IMGUI like the code editor; moves to the in-game UI framework with it.
    /// </summary>
    public sealed class HudEditor : MonoBehaviour
    {
        GameSession session;
        HudLayer hud;
        HudPanel editing;
        string title, html, css;
        int tab;
        Vector2 scroll, side;
        GUIStyle box, label, small, mono, monoNum, err, titleStyle, btn;

        public void Init(GameSession s, HudLayer h) { session = s; hud = h; }

        void Update()
        {
            if (hud.EditRequest != null)
            {
                editing = hud.EditRequest; hud.EditRequest = null;
                title = editing.Title; html = editing.Html; css = editing.Css; tab = 0;
                session.PanelOpen = true; session.KeyboardCaptured = true;
            }
            if (editing != null && !session.State.Hud.Panels.Contains(editing)) Close();
        }

        void Close() { editing = null; session.PanelOpen = false; session.KeyboardCaptured = false; GUIUtility.keyboardControl = 0; }

        void Styles()
        {
            if (box != null) return;
            var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.05f, 0.06f, 0.09f, 0.97f)); bg.Apply();
            box = new GUIStyle(UiTheme.Panel) { padding = new RectOffset(10, 10, 8, 8) };
            label = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true, normal = { textColor = UiTheme.Text } };
            small = new GUIStyle(label) { fontSize = 12, wordWrap = true };
            titleStyle = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Bold };
            err = new GUIStyle(small) { normal = { textColor = new Color(1f, 0.55f, 0.4f) } };
            btn = new GUIStyle(GUI.skin.button) { fontSize = 13 };
            var font = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Menlo", "DejaVu Sans Mono", "Liberation Mono", "Courier New" }, 14);
            mono = new GUIStyle(GUI.skin.textArea) { font = font, fontSize = 14, wordWrap = false };
            mono.normal.textColor = mono.focused.textColor = UiTheme.Text;
            monoNum = new GUIStyle(label) { font = font, fontSize = 14, alignment = TextAnchor.UpperRight, padding = mono.padding, normal = { textColor = UiTheme.Muted } };
        }

        void OnGUI()
        {
            UiGui.Begin();
            if (editing == null) return;
            Styles();
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { Close(); e.Use(); return; }
            var rect = new Rect(UiGui.W * 0.05f, UiGui.H * 0.05f, UiGui.W * 0.9f, UiGui.H * 0.85f);
            GUILayout.BeginArea(rect, box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("HUD box", titleStyle, GUILayout.Width(80));
            title = GUILayout.TextField(title ?? "", 40, GUILayout.Width(220));
            GUILayout.Space(10);
            if (GUILayout.Toggle(tab == 0, "HTML", btn, GUILayout.Width(70))) tab = 0;
            if (GUILayout.Toggle(tab == 1, "CSS", btn, GUILayout.Width(70))) tab = 1;
            GUILayout.FlexibleSpace();
            bool dirty = title != editing.Title || html != editing.Html || css != editing.Css;
            GUI.enabled = dirty;
            if (GUILayout.Button("Apply", btn, GUILayout.Width(80))) { editing.Title = title; editing.Html = html; editing.Css = css; session.State.Hud.Touch(); }
            if (GUILayout.Button("Revert", btn, GUILayout.Width(80))) { title = editing.Title; html = editing.Html; css = editing.Css; }
            GUI.enabled = true;
            if (GUILayout.Button("Close (Esc)", btn)) { Close(); GUILayout.EndHorizontal(); GUILayout.EndArea(); return; }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(rect.width * 0.64f));
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(rect.height * 0.7f));
            GUILayout.BeginHorizontal();
            string text = tab == 0 ? html : css;
            int lines = 1; foreach (char c in text) if (c == '\n') lines++;
            var nums = new StringBuilder(); for (int i = 1; i <= lines; i++) nums.Append(i).Append('\n');
            GUILayout.Label(nums.ToString(), monoNum, GUILayout.Width(36));
            string edited = GUILayout.TextArea(text, mono, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            if (tab == 0) html = edited; else css = edited;
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.Label("<b>Problems</b> (of the applied version)", small);
            var problems = hud.ProblemsOf(editing);
            if (problems.Count == 0) GUILayout.Label("<color=#5ef0a0>none</color>", small);
            for (int i = 0; i < problems.Count && i < 8; i++) GUILayout.Label(problems[i], err);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(box);
            side = GUILayout.BeginScrollView(side);
            GUILayout.Label("<b>Live values</b> — write {{key}} in text or attributes", small);
            foreach (var k in HudLayer.BuiltInKeys)
            {
                string v = k.Contains("<") ? "" : " = " + hud.Lookup(k);
                GUILayout.Label($"<color=#9cdcfe>{k}</color>{v}", small);
            }
            GUILayout.Label("<b>Published by your programs</b> — hud.set(\"key\", value)", small);
            if (session.State.Hud.Values.Count == 0) GUILayout.Label("<i>none yet</i>", small);
            foreach (var kv in session.State.Hud.Values) GUILayout.Label($"<color=#ffd43b>{kv.Key}</color> = {kv.Value.Replace("<", "<​")}", small);
            GUILayout.Space(6);
            GUILayout.Label("<b>HTML</b>: div p h1-h3 span b strong i em u small code ul ol li br hr progress(value, max)", small);
            GUILayout.Label("<b>CSS</b>: tag .class #id, descendant (a b), lists (a, b). color, background-color, font-size, font-weight, font-style, text-align, padding, margin, border, border-radius, width, height, min/max sizes, display, flex-direction, justify-content, align-items, flex-grow, flex-wrap, opacity. Inline style=\"...\" too.", small);
            GUILayout.Label("<b>Programs</b>: hud.set(key, value) shows data; hud.notify(text) pops an alert; hud.alarm(text) sounds an alarm (SQL: INSERT INTO hud VALUES ('alarm', 'text')).", small);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
