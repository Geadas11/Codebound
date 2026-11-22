using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>The guided first missions: the current step and what to do, top right; checked once a second.</summary>
    public sealed class TutorialView : MonoBehaviour
    {
        GameSession session;
        float timer;
        GUIStyle box, title, text, small;

        public void Init(GameSession s)
        {
            session = s;
            s.State.Tutorial.StepDone += step =>
            {
                var t = s.State.Tutorial;
                AudioDirector.Instance?.Play("chime", null, null);
                s.State.Hud.Notify(t.Finished
                    ? Loc.T("tut.finished", "Tutorial complete! Next goal: research the ship (T) and fly to orbit with a program. Coding help can be changed in Options.")
                    : Loc.T("tut.stepDone", "Done: {0}", Tutorial.Title(step)));
            };
        }

        bool Visible => !session.State.Tutorial.Finished && !Net.IsGuest && !TitleMenu.IsOpen && !OptionsMenu.IsOpen;

        void Update()
        {
            if (!Visible || (timer -= Time.unscaledDeltaTime) > 0) return;
            timer = 1f;
            session.State.Tutorial.Update(session.State);
        }

        void OnGUI()
        {
            if (!Visible || session.PanelOpen) return;
            UiGui.Begin();
            if (box == null)
            {
                var bg = new Texture2D(1, 1); bg.SetPixel(0, 0, new Color(0.04f, 0.07f, 0.1f, 0.88f)); bg.Apply();
                box = new GUIStyle(GUI.skin.box) { normal = { background = bg }, padding = new RectOffset(12, 12, 8, 8) };
                title = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(14), fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = new Color(0.55f, 0.95f, 0.75f) } };
                text = new GUIStyle(GUI.skin.label) { fontSize = UiGui.Font(13), wordWrap = true, normal = { textColor = new Color(0.9f, 0.95f, 1f) } };
                small = new GUIStyle(GUI.skin.button) { fontSize = UiGui.Font(11) };
            }
            var t = session.State.Tutorial;
            var step = t.Current;
            float w = Mathf.Min(330, UiGui.W * 0.4f);
            string head = Loc.T("tut.header", "Tutorial {0}/{1}: {2}", t.Index + 1, Tutorial.Steps.Length, Tutorial.Title(step)), hint = Tutorial.Hint(step);
            float inner = w - box.padding.horizontal;
            float h = box.padding.vertical + title.CalcHeight(new GUIContent(head), inner) + text.CalcHeight(new GUIContent(hint), inner) + small.CalcHeight(new GUIContent("X"), inner) + 14;
            GUILayout.BeginArea(new Rect(UiGui.W - w - 12, UiGui.H * 0.3f, w, h), box);   // just as tall as its text
            GUILayout.Label(head, title);
            GUILayout.Label(hint, text);
            if (GUILayout.Button(Loc.T("tut.skip", "Skip the tutorial"), small)) t.Skip();
            GUILayout.EndArea();
        }
    }
}
