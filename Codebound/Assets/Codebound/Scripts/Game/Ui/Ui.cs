using System;
using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebound.Game
{
    /// <summary>
    /// Reusable IMGUI components drawn with <see cref="UiTheme"/>: buttons, fields, toggles, sliders, steppers, sections,
    /// stat rows, badges, toasts. Immediate mode like the rest of the game's panels, so any screen or panel can use them.
    /// </summary>
    public static class Ui
    {
        public static void Click() => AudioDirector.Ui("click");
        public static void Confirm() => AudioDirector.Ui("ui_confirm");

        /// <summary>Seconds since the last frame, ignoring pause (menus run with timeScale 0).</summary>
        public static float Dt => Mathf.Min(Time.unscaledDeltaTime, 0.1f);
        public static bool Animate => !Settings.ReduceMotion;

        public static bool Button(string text, GUIStyle style = null, bool enabled = true, params GUILayoutOption[] opts)
        {
            bool was = GUI.enabled;
            GUI.enabled = was && enabled;
            var st = style ?? GUI.skin.button;
            if (!GUI.enabled) st = Disabled(st);
            bool hit = GUILayout.Button(text, st, opts);
            GUI.enabled = was;
            if (hit) Click();
            return hit;
        }

        public static bool Button(Rect r, string text, GUIStyle style = null, bool enabled = true)
        {
            bool was = GUI.enabled;
            GUI.enabled = was && enabled;
            var st = style ?? GUI.skin.button;
            if (!GUI.enabled) st = Disabled(st);
            bool hit = GUI.Button(r, text, st);
            GUI.enabled = was;
            if (hit) Click();
            return hit;
        }

        static readonly Dictionary<GUIStyle, GUIStyle> disabled = new Dictionary<GUIStyle, GUIStyle>();
        static int disabledVersion = -1;

        /// <summary>The style's disabled look: flat plate, dim text (IMGUI has no disabled state of its own).</summary>
        public static GUIStyle Disabled(GUIStyle s)
        {
            if (disabledVersion != Settings.Version) { disabled.Clear(); disabledVersion = Settings.Version; }
            if (!disabled.TryGetValue(s, out var d))
            {
                d = new GUIStyle(s);
                d.normal.background = d.hover.background = d.active.background = s.normal.background == null || s.normal.background == UiTheme.Clear ? UiTheme.Clear : UiTheme.BtnOffTex;
                d.normal.textColor = d.hover.textColor = d.active.textColor = UiTheme.Dim;
                disabled[s] = d;
            }
            return d;
        }

        /// <summary>A caption above a text field. Tab moves between fields.</summary>
        public static string Field(string label, string value, int max, string name = null, bool password = false, params GUILayoutOption[] opts)
        {
            if (label != null) GUILayout.Label(label.ToUpperInvariant(), UiTheme.Caption);
            if (name != null) GUI.SetNextControlName(name);
            return password ? GUILayout.PasswordField(value ?? "", '•', max, opts) : GUILayout.TextField(value ?? "", max, opts);
        }

        public static bool Toggle(string label, bool value, string hint = null)
        {
            GUILayout.BeginHorizontal();
            bool v = GUILayout.Toggle(value, " " + label);
            GUILayout.EndHorizontal();
            if (hint != null) GUILayout.Label(hint, UiTheme.Small);
            if (v != value) Click();
            return v;
        }

        /// <summary>Label on the left, value on the right, slider below. Returns the new value.</summary>
        public static float Slider(string label, float value, float min, float max, string shown)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, UiTheme.Label);
            GUILayout.FlexibleSpace();
            GUILayout.Label(shown, UiTheme.Value, GUILayout.Width(90));
            GUILayout.EndHorizontal();
            return GUILayout.HorizontalSlider(value, min, max);
        }

        /// <summary>"Label ........ ‹ value ›": arrows (or a click on the value) step through options. Returns -1, 0 or +1.</summary>
        public static int Stepper(string label, string value, bool enabled = true, string hint = null)
        {
            int d = 0;
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, UiTheme.Label, GUILayout.ExpandWidth(true));
            if (Button("‹", null, enabled, GUILayout.Width(34))) d = -1;
            if (Button(value, null, enabled, GUILayout.Width(290))) d = 1;
            if (Button("›", null, enabled, GUILayout.Width(34))) d = 1;
            GUILayout.EndHorizontal();
            if (hint != null) GUILayout.Label(hint, UiTheme.Small);
            return d;
        }

        public static int Choice(string label, int index, string[] options, bool enabled = true, string hint = null)
        {
            if (options.Length == 0) return index;
            index = Mathf.Clamp(index, 0, options.Length - 1);
            int d = Stepper(label, options[index], enabled, hint);
            return d == 0 ? index : ((index + d) % options.Length + options.Length) % options.Length;
        }

        /// <summary>A section title with a rule under it.</summary>
        public static void Section(string title)
        {
            GUILayout.Space(10);
            GUILayout.Label(title.ToUpperInvariant(), UiTheme.H2);
            Rule(UiTheme.Line);
            GUILayout.Space(4);
        }

        public static void Rule(Color c, float h = 1)
        {
            var r = GUILayoutUtility.GetRect(1, h, GUILayout.ExpandWidth(true));
            Fill(r, c);
        }

        public static void Fill(Rect r, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;
            var old = GUI.color; GUI.color = c * old;
            GUI.DrawTexture(r, UiTheme.White);
            GUI.color = old;
        }

        public static void KeyValue(string key, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(key, UiTheme.Muted_, GUILayout.Width(150));
            GUILayout.Label(value, UiTheme.Label);
            GUILayout.EndHorizontal();
        }

        public static void Badge(Rect r, string text) => GUI.Label(r, text, UiTheme.Badge);

        /// <summary>A rotating segment ring for "working…" states.</summary>
        public static void Spinner(Rect r)
        {
            if (Event.current.type != EventType.Repaint) return;
            float t = Time.unscaledTime * 8f;
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2, k = Mathf.Repeat(i - t, 8) / 8f;
                var c = Color.Lerp(UiTheme.Accent, UiTheme.Line, k);
                var p = r.center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r.width * 0.38f;
                Fill(new Rect(p.x - 2, p.y - 2, 4, 4), c);
            }
        }

        // ---------------------------------------------------------------- toasts
        sealed class ToastItem { public string Text; public Color Color; public float Born; }
        static readonly List<ToastItem> toasts = new List<ToastItem>();

        /// <summary>A short message in the corner (feedback for an action). Kinds: good, bad, info.</summary>
        public static void Toast(string text, string kind = "info")
        {
            if (string.IsNullOrEmpty(text)) return;
            var c = kind == "good" ? UiTheme.Good : kind == "bad" ? UiTheme.Bad : UiTheme.Accent;
            toasts.Add(new ToastItem { Text = text, Color = c, Born = Time.unscaledTime });
            if (toasts.Count > 4) toasts.RemoveAt(0);
            if (kind == "bad") AudioDirector.Ui("error");
        }

        public static void DrawToasts()
        {
            float y = UiGui.H - 24, now = Time.unscaledTime;
            toasts.RemoveAll(t => now - t.Born > 5f);
            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                var t = toasts[i];
                float age = now - t.Born, a = Mathf.Clamp01(Mathf.Min(age * 6f, (5f - age) * 2f));
                var content = new GUIContent(t.Text);
                float w = Mathf.Min(460, UiGui.W - 40), h = UiTheme.Toast.CalcHeight(content, w - 6) + 2;
                float slide = Animate ? (1 - Mathf.Clamp01(age * 6f)) * 30 : 0;
                var r = new Rect(UiGui.W - w - 24 + slide, y - h, w, h);
                var old = GUI.color; GUI.color = new Color(1, 1, 1, a);
                GUI.Box(r, content, UiTheme.Toast);
                Fill(new Rect(r.x, r.y + 3, 3, r.height - 6), t.Color);
                GUI.color = old;
                y -= h + 8;
            }
        }

        // ---------------------------------------------------------------- keyboard / controller
        /// <summary>Up / down / confirm / back from the keyboard or a gamepad (d-pad, left stick, A, B).</summary>
        public static int NavVertical()
        {
            var kb = Keyboard.current; var gp = Gamepad.current;
            int d = 0;
            if (kb != null) { if (kb.downArrowKey.wasPressedThisFrame) d = 1; if (kb.upArrowKey.wasPressedThisFrame) d = -1; }
            if (gp != null) { if (gp.dpad.down.wasPressedThisFrame || gp.leftStick.down.wasPressedThisFrame) d = 1; if (gp.dpad.up.wasPressedThisFrame || gp.leftStick.up.wasPressedThisFrame) d = -1; }
            return d;
        }

        public static bool NavConfirm()
        {
            var kb = Keyboard.current; var gp = Gamepad.current;
            return (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)) || (gp != null && gp.buttonSouth.wasPressedThisFrame);
        }

        public static bool NavBack()
        {
            var kb = Keyboard.current; var gp = Gamepad.current;
            return (kb != null && kb.escapeKey.wasPressedThisFrame) || (gp != null && gp.buttonEast.wasPressedThisFrame);
        }

        public static string Ago(long unix)
        {
            if (unix <= 0) return "—";
            var t = DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime();
            double s = (DateTimeOffset.Now - t).TotalSeconds;
            if (s < 60) return Loc.T("ui.justNow", "just now");
            if (s < 3600) return Loc.T("ui.minAgo", "{0} min ago", (int)(s / 60));
            if (s < 86400) return Loc.T("ui.hAgo", "{0} h ago", (int)(s / 3600));
            return t.ToString("yyyy-MM-dd HH:mm");
        }

        public static string Duration(double seconds)
        {
            int m = (int)(seconds / 60);
            return m < 60 ? Loc.T("ui.minutes", "{0} min", m) : Loc.T("ui.hoursMinutes", "{0} h {1:00} min", m / 60, m % 60);
        }
    }

    /// <summary>One full screen of the menus (profiles, main, worlds, multiplayer, options…), shown by a <see cref="UiNav"/>.</summary>
    public abstract class UiScreen
    {
        public UiNav Nav;
        public abstract string Title { get; }
        /// <summary>Draws the screen into `r` (the whole scaled screen).</summary>
        public abstract void Draw(Rect r);
        public virtual void Update() { }
        public virtual void OnShow() { }
        /// <summary>Back / Esc: return false to stay (e.g. to close an inner editor first).</summary>
        public virtual bool OnBack() => true;
        /// <summary>The top screen of a stack shows no Back button.</summary>
        public virtual bool IsRoot => false;
    }

    /// <summary>
    /// Screen stack with fade / slide transitions, a confirmation dialog, a busy overlay that blocks input during
    /// incompatible operations, and Esc / gamepad B as Back everywhere.
    /// </summary>
    public sealed class UiNav
    {
        readonly List<UiScreen> stack = new List<UiScreen>();
        float t = 1;            // transition progress 0 → 1
        int dir = 1;
        public string Busy;     // non-null: a spinner covers the screen and input is ignored
        public Action OnEmpty;  // the last screen was popped

        sealed class Dialog { public string Title, Body, Ok, Cancel; public bool Destructive; public Action OnOk; }
        Dialog dialog;
        int lastBackFrame = -1;

        public UiScreen Top => stack.Count > 0 ? stack[stack.Count - 1] : null;
        public int Depth => stack.Count;

        public void Push(UiScreen s) { s.Nav = this; stack.Add(s); t = 0; dir = 1; s.OnShow(); }
        public void Replace(UiScreen s) { if (stack.Count > 0) stack.RemoveAt(stack.Count - 1); Push(s); }
        public void Reset(UiScreen s) { stack.Clear(); dialog = null; Busy = null; Push(s); }
        public void Pop()
        {
            if (stack.Count == 0) return;
            stack.RemoveAt(stack.Count - 1);
            t = 0; dir = -1;
            if (Top != null) Top.OnShow(); else OnEmpty?.Invoke();
        }

        public void Back()
        {
            if (Time.frameCount == lastBackFrame) return;
            lastBackFrame = Time.frameCount;
            if (dialog != null) { dialog = null; return; }
            if (Busy != null || Top == null) return;
            if (Top.IsRoot && Top.OnBack()) { OnEmpty?.Invoke(); return; }
            if (Top.OnBack()) { Ui.Click(); Pop(); }
        }

        /// <summary>Asks before something that cannot be undone from the game.</summary>
        public void Ask(string title, string body, string ok, Action onOk, bool destructive = false, string cancel = null) =>
            dialog = new Dialog { Title = title, Body = body, Ok = ok, Cancel = cancel ?? Loc.T("ui.cancel", "Cancel"), Destructive = destructive, OnOk = onOk };

        public void Tell(string title, string body) => dialog = new Dialog { Title = title, Body = body, Ok = Loc.T("ui.ok", "OK") };

        public bool Blocked => dialog != null || Busy != null;

        Action pending;
        int pendingFrames;
        /// <summary>Esc while busy (e.g. cancel a connection attempt); null = the busy state cannot be cancelled.</summary>
        public Action OnBusyCancel;

        /// <summary>Shows the busy overlay first, then runs `a` (loading a world freezes a frame or two: the player sees why).</summary>
        public void Run(string busy, Action a) { Busy = busy; pending = a; pendingFrames = 2; }

        public void Update()
        {
            if (pending != null && --pendingFrames <= 0)
            {
                var a = pending; pending = null;
                try { a(); }
                catch (Exception e) { Debug.LogException(e); Ui.Toast(e.Message, "bad"); }
                finally { Busy = null; }
            }
            if (dialog != null)
            {
                if (Ui.NavBack()) { lastBackFrame = Time.frameCount; dialog = null; }
                else if (Ui.NavConfirm() && dialog.Cancel == null) dialog = null;
                return;
            }
            if (Busy != null)
            {
                if (OnBusyCancel != null && Ui.NavBack()) { lastBackFrame = Time.frameCount; var c = OnBusyCancel; OnBusyCancel = null; Busy = null; c(); }
                return;
            }
            if (Ui.NavBack()) { Back(); return; }
            Top?.Update();
        }

        public void Draw(Rect r)
        {
            t = Mathf.MoveTowards(t, 1, Ui.Dt / 0.22f);
            var top = Top;
            if (top != null)
            {
                float e = Ui.Animate ? 1 - Mathf.Pow(1 - t, 3) : 1;
                var old = GUI.color;
                GUI.color = new Color(1, 1, 1, Mathf.Lerp(0f, 1f, e));
                bool was = GUI.enabled;
                GUI.enabled = !Blocked && was;
                top.Draw(new Rect(r.x + (1 - e) * 36 * dir, r.y, r.width, r.height));
                GUI.enabled = was;
                GUI.color = old;
            }
            if (Busy != null)
            {
                Ui.Fill(r, new Color(0, 0, 0, 0.55f));
                var box = new Rect(r.center.x - 200, r.center.y - 40, 400, 80);
                GUI.Box(box, GUIContent.none, UiTheme.Panel);
                Ui.Spinner(new Rect(box.x + 20, box.y + 20, 40, 40));
                GUI.Label(new Rect(box.x + 76, box.y, box.width - 90, box.height), Busy, UiTheme.Label);
            }
            if (dialog != null) DrawDialog(r);
        }

        void DrawDialog(Rect r)
        {
            Ui.Fill(r, new Color(0, 0, 0, 0.6f));
            float w = Mathf.Min(520, r.width - 40);
            var body = new GUIContent(dialog.Body ?? "");
            float h = 150 + UiTheme.Wrap.CalcHeight(body, w - 40);
            var box = new Rect(r.center.x - w / 2, r.center.y - h / 2, w, h);
            GUILayout.BeginArea(box, UiTheme.Panel);
            GUILayout.Label(dialog.Title, UiTheme.H1);
            Ui.Rule(dialog.Destructive ? UiTheme.Danger : UiTheme.Accent, 2);
            GUILayout.Space(8);
            GUILayout.Label(body, UiTheme.Wrap);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (dialog.Cancel != null && Ui.Button(dialog.Cancel, null, true, GUILayout.Width(150), GUILayout.Height(38))) dialog = null;
            if (dialog != null && Ui.Button(dialog.Ok, dialog.Destructive ? UiTheme.DangerBtn : UiTheme.Primary, true, GUILayout.Width(170), GUILayout.Height(38)))
            {
                var ok = dialog.OnOk; dialog = null; ok?.Invoke();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary>The standard frame of a sub-screen: a top bar with Back and the title, and a content rect under it.</summary>
        public static Rect Frame(UiScreen s, Rect r, string subtitle = null)
        {
            float pad = Mathf.Max(24, r.width * 0.04f), cw = Mathf.Min(1320, r.width - pad * 2);
            float x = r.x + (r.width - cw) / 2, y = r.y + 26;
            if (!s.IsRoot && Ui.Button(new Rect(x, y + 6, 120, 38), "‹  " + Loc.T("ui.back", "Back"), UiTheme.Ghost)) s.Nav.Back();
            float tx = s.IsRoot ? x : x + 140;
            GUI.Label(new Rect(tx, y, cw - 140, 50), s.Title.ToUpperInvariant(), UiTheme.H1);
            if (subtitle != null) GUI.Label(new Rect(tx, y + 46, cw - 140, 22), subtitle, UiTheme.Muted_);
            Ui.Fill(new Rect(x, y + 74, cw, 1), UiTheme.Line);
            Ui.Fill(new Rect(x, y + 74, 64, 2), UiTheme.Accent);
            return new Rect(x, y + 92, cw, r.yMax - (y + 92) - 30);
        }
    }
}
