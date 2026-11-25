using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>The story's ending (Etapa 7): shown when the antenna unlock code is transmitted. The world goes on afterwards.</summary>
    public sealed class EndingView : MonoBehaviour
    {
        GameSession session;
        bool open;
        GUIStyle box, title, text, button;

        public void Init(GameSession s) { session = s; }

        public void Show() { open = true; session.PanelOpen = true; }

        void Close() { open = false; session.PanelOpen = false; }

        void Update()
        {
            if (open && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
        }

        void OnGUI()
        {
            if (!open) return;
            GUI.depth = -100;   // above the orbit readout and the HUD
            UiGui.Begin();
            if (box == null)
            {
                var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.02f, 0.03f, 0.07f, 0.97f)); bg.Apply();
                box = new GUIStyle(UiTheme.Panel) { padding = new RectOffset(24, 24, 18, 18) };
                title = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(24), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = UiTheme.Accent } };
                text = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(15), wordWrap = true, richText = true, normal = { textColor = UiTheme.Text } };
                button = new GUIStyle(GUI.skin.button) { fontSize = UiGui.Font(15) };
            }
            var st = session.State;
            float w = Mathf.Min(640, UiGui.W - 40), h = Mathf.Min(460, UiGui.H - 40);
            GUILayout.BeginArea(new Rect((UiGui.W - w) / 2, (UiGui.H - h) / 2, w, h), box);
            GUILayout.Label(Loc.T("end.title", "The antenna answers"), title);
            GUILayout.Space(10);
            GUILayout.Label(Loc.T("end.text",
                "The four fragments lock together and WARDEN's seal breaks. For the first time in years the long-range antenna turns toward the stars.\n\n" +
                "Your message goes out: Kepler Landing is ready. Weeks later, an answer: the settler fleet is under way — and it is bringing the programs you wrote with it.\n\n" +
                "WARDEN is quiet now. The custodians run your code."), text);
            GUILayout.Space(10);
            int built = 0; foreach (var _ in st.Grid.Buildings) built++;
            GUILayout.Label("<b>" + Loc.T("end.stats", "Day {0} · {1} buildings on this world · {2} technologies researched", st.Clock.Day, built, st.Research.Done.Count) + "</b>", text);
            GUILayout.FlexibleSpace();
            GUILayout.Label("<i>" + Loc.T("end.keepPlaying", "The story is complete, the world is yours: keep building, exploring and programming.") + "</i>", text);
            if (GUILayout.Button(Loc.T("end.continue", "Keep playing (Esc)"), button)) Close();
            GUILayout.EndArea();
        }
    }
}
