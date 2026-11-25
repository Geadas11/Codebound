using System;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// The Codebound look: charcoal and graphite panels with chamfered corners, steel edges and an industrial amber accent.
    /// Every texture is generated here (no imported art), and <see cref="Skin"/> is the GUISkin every IMGUI screen and
    /// panel draws with (UiGui.Begin installs it), so menus, machine panels, the code editor frame and the pause menu match.
    /// Rebuilt when settings change (high contrast, colour-blind palette, text size, language font).
    /// </summary>
    public static class UiTheme
    {
        // ---- palette (tokens). High contrast swaps them for pure black / white / yellow.
        public static Color Bg0, Bg1, Bg2, Bg3, Line, LineHi, Text, Muted, Dim, Accent, AccentHi, AccentDim, OnAccent, Danger, Good, Bad, Info;

        static void Palette()
        {
            bool hc = Settings.HighContrast;
            Bg0 = hc ? Color.black : Hex(0x0C0E11);
            Bg1 = hc ? Color.black : Hex(0x15181D);
            Bg2 = hc ? Hex(0x101010) : Hex(0x1E232A);
            Bg3 = hc ? Hex(0x303030) : Hex(0x29313A);
            Line = hc ? Color.white : Hex(0x353E49);
            LineHi = hc ? Color.white : Hex(0x55606E);
            Text = hc ? Color.white : Hex(0xE8E3D7);
            Muted = hc ? Hex(0xE0E0E0) : Hex(0x9CA5AF);
            Dim = hc ? Hex(0xA0A0A0) : Hex(0x68717B);
            Accent = hc ? Hex(0xFFE000) : Hex(0xF2A23A);
            AccentHi = hc ? Hex(0xFFF060) : Hex(0xFFC066);
            AccentDim = hc ? Hex(0x806F00) : Hex(0x7A4E1A);
            OnAccent = Hex(0x121212);
            Danger = Settings.ColorblindPalette ? Hex(0xD9822B) : Hex(0xD9573F);
            Good = Settings.GoodColor; Bad = Settings.BadColor;
            Info = hc ? Color.white : Hex(0x6FB3D9);
        }

        public static Color Hex(int rgb, float a = 1) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, a);
        public static string Html(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);

        // ---- textures
        public static Texture2D White, Clear, PanelTex, PanelHeadTex, InsetTex, BtnTex, BtnHoverTex, BtnDownTex, BtnOffTex, PrimaryTex, PrimaryHoverTex, DangerTex, DangerHoverTex,
                                FieldTex, FieldFocusTex, CheckOff, CheckOn, CheckOffHover, CheckOnHover, TrackTex, ThumbTex, ThumbHoverTex, ScrollTex, ScrollThumbTex, RowHoverTex, RowSelTex, TabOnTex;

        // ---- fonts
        public static Font Heading, Body;

        // ---- styles beyond the skin
        public static GUIStyle Title, H1, H2, Label, Muted_, Small, Caption, Value, Wrap, Mono, MenuItem, MenuHint, Primary, DangerBtn, Ghost, Tab, TabOn, Row, RowSel, Badge, Panel, PanelHead, Inset, Toast;

        static GUISkin skin;
        static int version = -1;
        static string lang;
        static float textScale = -1;

        /// <summary>The themed skin (rebuilt lazily when settings changed). Only call from OnGUI.</summary>
        public static GUISkin Skin { get { Ensure(); return skin; } }

        public static void Ensure()
        {
            if (skin != null && version == Settings.Version && lang == Codebound.Core.Loc.Current && textScale == Settings.TextScale) return;
            version = Settings.Version; lang = Codebound.Core.Loc.Current; textScale = Settings.TextScale;
            Palette();
            Textures();
            Fonts();
            Build();
        }

        static int F(int size) => UiGui.Font(size);

        static Texture2D Make(int w, int h, Func<int, int, Color> f)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            t.SetPixels(px); t.Apply();
            return t;
        }

        /// <summary>A chamfered plate: corners `c` cut at top-left and bottom-right, 1 px edge, faint top highlight, vertical shade.</summary>
        static Texture2D Plate(Color fill, Color edge, int c = 3, Color? bar = null, int size = 16, float shade = 0.06f)
        {
            return Make(size, size, (x, y) =>
            {
                int top = size - 1 - y;                       // texture rows go bottom-up
                int dTL = x + top, dBR = (size - 1 - x) + y;
                if (dTL < c || dBR < c) return Color.clear;
                bool rim = x == 0 || y == 0 || x == size - 1 || top == 0 || dTL == c || dBR == c;
                if (rim) return edge;
                if (bar.HasValue && x >= 1 && x <= 3) return bar.Value;
                var col = Color.Lerp(fill, fill * 0.82f, (float)top / size * shade * 10f);
                if (top == 1) col = Color.Lerp(col, Color.white, 0.07f);
                col.a = fill.a;
                return col;
            });
        }

        static Texture2D Check(bool on, bool hover)
        {
            const int s = 18;
            return Make(s + 1, s, (x, y) =>
            {
                if (x >= s) return Color.clear;
                int top = s - 1 - y;
                bool rim = x == 0 || y == 0 || x == s - 1 || top == 0;
                if (rim) return on || hover ? Accent : LineHi;
                if (!on) return Bg0;
                // a check mark: two strokes
                int cx = x - 4, cy = top - 4;
                bool mark = (cy >= 4 && cy <= 9 && Mathf.Abs(cy - (cx + 3)) <= 1 && cx <= 4) || (cx >= 3 && cx <= 10 && Mathf.Abs(cy - (13 - cx)) <= 1);
                return mark ? OnAccent : Accent;
            });
        }

        static void Textures()
        {
            White = Make(1, 1, (x, y) => Color.white);
            Clear = Make(1, 1, (x, y) => Color.clear);
            PanelTex = Plate(new Color(Bg1.r, Bg1.g, Bg1.b, Settings.HighContrast ? 1f : 0.97f), Line, 4);
            PanelHeadTex = Make(4, 4, (x, y) => y == 0 ? Accent : Bg2);
            InsetTex = Plate(Bg0, Line, 2);
            BtnTex = Plate(Bg2, Line, 3);
            BtnHoverTex = Plate(Bg3, LineHi, 3, Accent);
            BtnDownTex = Plate(AccentDim, Accent, 3);
            BtnOffTex = Plate(Bg1, Bg2, 3);
            PrimaryTex = Plate(Accent, AccentDim, 3, null, 16, 0.03f);
            PrimaryHoverTex = Plate(AccentHi, Accent, 3, null, 16, 0.03f);
            DangerTex = Plate(Color.Lerp(Bg2, Danger, 0.35f), Danger, 3);
            DangerHoverTex = Plate(Color.Lerp(Bg2, Danger, 0.6f), Danger, 3);
            FieldTex = Plate(Bg0, Line, 2);
            FieldFocusTex = Plate(Bg0, Accent, 2);
            CheckOff = Check(false, false); CheckOn = Check(true, false); CheckOffHover = Check(false, true); CheckOnHover = Check(true, true);
            TrackTex = Make(8, 6, (x, y) => y == 0 || y == 5 ? Line : Bg0);
            ThumbTex = Make(10, 18, (x, y) => x == 0 || x == 9 || y == 0 || y == 17 ? AccentDim : Accent);
            ThumbHoverTex = Make(10, 18, (x, y) => x == 0 || x == 9 || y == 0 || y == 17 ? Accent : AccentHi);
            ScrollTex = Make(10, 8, (x, y) => x == 0 ? Line : Bg0);
            ScrollThumbTex = Plate(LineHi, LineHi, 0, null, 10);
            RowHoverTex = Make(4, 4, (x, y) => x == 0 ? Accent : new Color(1, 1, 1, 0.04f));
            RowSelTex = Make(4, 4, (x, y) => x == 0 ? Accent : new Color(Accent.r, Accent.g, Accent.b, 0.14f));
            TabOnTex = Make(4, 4, (x, y) => y == 0 ? Accent : Bg3);
        }

        static Font osHeading, osBody;

        static void Fonts()
        {
            var cjk = Settings.UiFont;   // Japanese / Chinese / Korean need an OS font with those glyphs
            if (cjk != null) { Heading = Body = cjk; return; }
            if (osHeading == null)
            {
                osHeading = Font.CreateDynamicFontFromOSFont(new[] { "Bahnschrift", "Segoe UI Semibold", "DIN Alternate", "Roboto Condensed", "Arial" }, 16);
                osBody = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Helvetica Neue", "Roboto", "Noto Sans", "Arial" }, 16);
            }
            Heading = osHeading; Body = osBody;
        }

        // an explicit transparent texture: a null background lets IMGUI fall back to a grey plate
        static void State(GUIStyleState s, Texture2D bg, Color text) { s.background = bg ?? Clear; s.scaledBackgrounds = new Texture2D[0]; s.textColor = text; }

        static GUIStyle Text_(Font f, int size, Color c, FontStyle st = FontStyle.Normal, TextAnchor a = TextAnchor.MiddleLeft, bool wrap = false) =>
            new GUIStyle { font = f, fontSize = F(size), fontStyle = st, alignment = a, wordWrap = wrap, richText = true, normal = { textColor = c }, padding = new RectOffset(2, 2, 2, 2), margin = new RectOffset(2, 2, 2, 2) };

        static void Build()
        {
            if (skin == null) { skin = ScriptableObject.CreateInstance<GUISkin>(); skin.hideFlags = HideFlags.HideAndDontSave; }
            skin.font = Body;

            var box = new GUIStyle { border = new RectOffset(5, 5, 5, 5), padding = new RectOffset(14, 14, 12, 12), margin = new RectOffset(4, 4, 4, 4), font = Body, fontSize = F(14), richText = true, wordWrap = true };
            State(box.normal, PanelTex, Text);
            skin.box = box; skin.window = new GUIStyle(box) { padding = new RectOffset(14, 14, 30, 12), alignment = TextAnchor.UpperCenter, fontStyle = FontStyle.Bold };
            State(skin.window.onNormal, PanelTex, Text);

            var label = Text_(Body, 14, Text, wrap: true);
            label.alignment = TextAnchor.UpperLeft;
            skin.label = label;

            var button = new GUIStyle { font = Body, fontSize = F(14), alignment = TextAnchor.MiddleCenter, richText = true, border = new RectOffset(5, 5, 5, 5),
                                        padding = new RectOffset(12, 12, 6, 7), margin = new RectOffset(3, 3, 3, 3), clipping = TextClipping.Clip };
            State(button.normal, BtnTex, Text); State(button.hover, BtnHoverTex, Color.white); State(button.active, BtnDownTex, Color.white);
            State(button.focused, BtnHoverTex, Color.white);
            State(button.onNormal, BtnDownTex, Color.white); State(button.onHover, BtnDownTex, Color.white); State(button.onActive, BtnDownTex, Color.white);
            skin.button = button;

            var field = new GUIStyle { font = Body, fontSize = F(14), alignment = TextAnchor.MiddleLeft, border = new RectOffset(4, 4, 4, 4), padding = new RectOffset(8, 8, 6, 6),
                                       margin = new RectOffset(3, 3, 3, 3), clipping = TextClipping.Clip };
            State(field.normal, FieldTex, Text); State(field.hover, FieldTex, Color.white); State(field.focused, FieldFocusTex, Color.white); State(field.active, FieldFocusTex, Color.white);
            field.onNormal.background = FieldFocusTex;
            skin.textField = field;
            skin.textArea = new GUIStyle(field) { alignment = TextAnchor.UpperLeft, wordWrap = true };
            skin.settings.cursorColor = Accent;
            skin.settings.selectionColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f);
            skin.settings.doubleClickSelectsWord = true;

            var toggle = new GUIStyle { font = Body, fontSize = F(14), alignment = TextAnchor.MiddleLeft, border = new RectOffset(19, 0, 0, 0), padding = new RectOffset(26, 4, 2, 2),
                                        margin = new RectOffset(3, 3, 4, 4), fixedHeight = 18, richText = true };
            State(toggle.normal, CheckOff, Text); State(toggle.hover, CheckOffHover, Color.white); State(toggle.active, CheckOffHover, Color.white);
            State(toggle.onNormal, CheckOn, Text); State(toggle.onHover, CheckOnHover, Color.white); State(toggle.onActive, CheckOnHover, Color.white);
            skin.toggle = toggle;

            skin.horizontalSlider = new GUIStyle { border = new RectOffset(2, 2, 2, 2), fixedHeight = 6, margin = new RectOffset(4, 4, 10, 10), normal = { background = TrackTex } };
            skin.horizontalSliderThumb = new GUIStyle { fixedWidth = 10, fixedHeight = 18, normal = { background = ThumbTex }, hover = { background = ThumbHoverTex }, active = { background = ThumbHoverTex } };
            skin.verticalSlider = new GUIStyle(skin.horizontalSlider) { fixedHeight = 0, fixedWidth = 6 };
            skin.verticalSliderThumb = new GUIStyle(skin.horizontalSliderThumb) { fixedWidth = 18, fixedHeight = 10 };

            skin.verticalScrollbar = new GUIStyle { fixedWidth = 10, border = new RectOffset(1, 1, 1, 1), margin = new RectOffset(4, 0, 0, 0), normal = { background = ScrollTex } };
            skin.verticalScrollbarThumb = new GUIStyle { fixedWidth = 10, border = new RectOffset(2, 2, 2, 2), normal = { background = ScrollThumbTex }, hover = { background = BtnHoverTex }, active = { background = PrimaryTex } };
            skin.verticalScrollbarUpButton = new GUIStyle { fixedWidth = 0, fixedHeight = 0 };
            skin.verticalScrollbarDownButton = new GUIStyle { fixedWidth = 0, fixedHeight = 0 };
            skin.horizontalScrollbar = new GUIStyle { fixedHeight = 10, border = new RectOffset(1, 1, 1, 1), margin = new RectOffset(0, 0, 4, 0), normal = { background = ScrollTex } };
            skin.horizontalScrollbarThumb = new GUIStyle { fixedHeight = 10, border = new RectOffset(2, 2, 2, 2), normal = { background = ScrollThumbTex }, hover = { background = BtnHoverTex } };
            skin.horizontalScrollbarLeftButton = new GUIStyle { fixedWidth = 0, fixedHeight = 0 };
            skin.horizontalScrollbarRightButton = new GUIStyle { fixedWidth = 0, fixedHeight = 0 };
            skin.scrollView = new GUIStyle();

            // ---- named styles
            Title = Text_(Heading, 64, Text, FontStyle.Bold, TextAnchor.MiddleLeft);
            H1 = Text_(Heading, 30, Text, FontStyle.Bold);
            H2 = Text_(Heading, 18, Accent, FontStyle.Bold);
            Label = Text_(Body, 15, Text, wrap: true);
            Muted_ = Text_(Body, 14, Muted, wrap: true);
            Small = Text_(Body, 12, Muted, wrap: true);
            Caption = Text_(Heading, 12, Dim, FontStyle.Bold);
            Value = Text_(Body, 15, AccentHi, a: TextAnchor.MiddleRight);
            Wrap = Text_(Body, 15, Text, wrap: true);
            Mono = Text_(Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Cascadia Mono", "Menlo", "DejaVu Sans Mono", "Courier New" }, 14), 14, Text);

            MenuItem = new GUIStyle(button) { font = Heading, fontSize = F(21), alignment = TextAnchor.MiddleLeft, padding = new RectOffset(22, 16, 0, 0), fixedHeight = 0, fontStyle = FontStyle.Bold };
            MenuHint = Text_(Body, 12, Muted);
            Primary = new GUIStyle(button) { font = Heading, fontStyle = FontStyle.Bold, fontSize = F(15) };
            State(Primary.normal, PrimaryTex, OnAccent); State(Primary.hover, PrimaryHoverTex, OnAccent); State(Primary.active, BtnDownTex, Color.white); State(Primary.focused, PrimaryHoverTex, OnAccent);
            DangerBtn = new GUIStyle(button);
            State(DangerBtn.normal, DangerTex, Text); State(DangerBtn.hover, DangerHoverTex, Color.white); State(DangerBtn.active, DangerHoverTex, Color.white);
            Ghost = new GUIStyle(button);
            State(Ghost.normal, null, Muted); State(Ghost.hover, RowHoverTex, Color.white); State(Ghost.active, RowSelTex, Color.white);
            Tab = new GUIStyle(button) { font = Heading, fontSize = F(14), fontStyle = FontStyle.Bold, padding = new RectOffset(14, 14, 8, 9), border = new RectOffset(1, 1, 2, 1) };
            State(Tab.normal, null, Muted); State(Tab.hover, RowHoverTex, Color.white); State(Tab.active, TabOnTex, Color.white);
            TabOn = new GUIStyle(Tab);
            State(TabOn.normal, TabOnTex, Color.white); State(TabOn.hover, TabOnTex, Color.white);
            Row = new GUIStyle(button) { alignment = TextAnchor.MiddleLeft, border = new RectOffset(3, 1, 1, 1), padding = new RectOffset(12, 10, 8, 8), margin = new RectOffset(0, 0, 1, 1) };
            State(Row.normal, null, Text); State(Row.hover, RowHoverTex, Color.white); State(Row.active, RowSelTex, Color.white);
            RowSel = new GUIStyle(Row);
            State(RowSel.normal, RowSelTex, Color.white); State(RowSel.hover, RowSelTex, Color.white);
            Badge = new GUIStyle { font = Heading, fontSize = F(11), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, padding = new RectOffset(6, 6, 2, 2), border = new RectOffset(2, 2, 2, 2), normal = { background = PrimaryTex, textColor = OnAccent } };
            Panel = box;
            PanelHead = new GUIStyle { border = new RectOffset(1, 1, 1, 2), padding = new RectOffset(16, 16, 10, 10), font = Heading, fontSize = F(18), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { background = PanelHeadTex, textColor = Text } };
            Inset = new GUIStyle(box) { padding = new RectOffset(10, 10, 8, 8) };
            State(Inset.normal, InsetTex, Text);
            Toast = new GUIStyle(box) { padding = new RectOffset(16, 16, 10, 10), alignment = TextAnchor.MiddleLeft, fontSize = F(14) };
        }
    }
}
