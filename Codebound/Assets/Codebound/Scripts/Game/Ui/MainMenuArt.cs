using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace Codebound.Game
{
    /// <summary>
    /// Art of the main menu only. The factory is a seamless 6 s loop rendered in Blender (Tools/art/menu_scene.py →
    /// StreamingAssets/Art/menu_loop.mp4), played by a VideoPlayer; menu_bg.jpg (its first frame, 4K) shows until the
    /// video runs, or instead of it. The logotype is menu_logo.png (Tools/art/menu_logo.py). Everything else is drawn
    /// here: a slanted console with a steel edge, a circuit rail with diamond nodes, an arrow-tipped selection blade,
    /// asymmetric chamfered plates, a slanted footer strip, vector icons (signed distance fields), dust and a vignette.
    /// Motion stops with "reduce motion" (the video shows its first frame).
    /// </summary>
    public static class MainMenuArt
    {
        static Texture2D still, logo, glow, dot, vignette, console, blade, plate, strip, node, hazard;
        static Texture2D[] icons;
        static bool loaded, missing;
        static int built = -1;

        public enum Icon { Play, Person, People, Map, Gear, Scroll, Power }

        // ---------------------------------------------------------------- loading
        static Texture2D Load(string file)
        {
            try
            {
                var bytes = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "Art", file));
                var t = new Texture2D(2, 2, TextureFormat.RGBA32, true) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
                if (!t.LoadImage(bytes, true)) return null;
                t.filterMode = FilterMode.Trilinear; t.anisoLevel = 4;
                return t;
            }
            catch (IOException e) { Debug.LogWarning("Codebound: menu art " + file + ": " + e.Message); return null; }
        }

        public static bool Ready
        {
            get
            {
                if (!loaded) { loaded = true; still = Load("menu_bg.jpg"); logo = Load("menu_logo.png"); missing = still == null; }
                if (built != Settings.Version) Build();
                return !missing;
            }
        }

        // ---------------------------------------------------------------- the factory loop
        sealed class VideoHost : MonoBehaviour
        {
            public VideoPlayer Player;
            public RenderTexture Target;
            public int LastDraw;
            void Update()
            {
                // the menu stopped drawing (a world is being played): stop decoding
                if (Player != null && Player.isPlaying && Time.frameCount - LastDraw > 3) Player.Pause();
            }
            void OnDestroy() { if (Target != null) { Target.Release(); Destroy(Target); } }
        }

        static VideoHost video;
        static bool videoFailed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { video = null; videoFailed = false; }

        static Texture VideoFrame()
        {
            if (videoFailed || !Ui.Animate) return null;
            string path = Path.Combine(Application.streamingAssetsPath, "Art", "menu_loop.mp4");
            if (video == null)
            {
                if (!File.Exists(path)) { videoFailed = true; return null; }
                var go = new GameObject("Codebound Menu Video") { hideFlags = HideFlags.HideAndDontSave };
                UnityEngine.Object.DontDestroyOnLoad(go);
                video = go.AddComponent<VideoHost>();
                var vp = go.AddComponent<VideoPlayer>();
                video.Target = new RenderTexture(2560, 1440, 0, RenderTextureFormat.ARGB32) { name = "MenuVideo" };
                vp.playOnAwake = false; vp.isLooping = true; vp.skipOnDrop = true;
                vp.source = VideoSource.Url; vp.url = path;
                vp.renderMode = VideoRenderMode.RenderTexture; vp.targetTexture = video.Target;
                vp.audioOutputMode = VideoAudioOutputMode.None;
                vp.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;   // the title screen runs with timeScale 0
                vp.errorReceived += (p, msg) => { videoFailed = true; Debug.LogWarning("Codebound: menu video: " + msg); };
                video.Player = vp;
                vp.Prepare();
            }
            video.LastDraw = Time.frameCount;
            var v = video.Player;
            if (v.isPrepared && !v.isPlaying) v.Play();
            return v.isPlaying && v.frame > 0 ? video.Target : null;
        }

        // ---------------------------------------------------------------- generated textures
        static Texture2D Tex(int w, int h, Func<int, int, Color> f, FilterMode filter = FilterMode.Bilinear)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp, filterMode = filter };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = f(x, y);
            t.SetPixels(px); t.Apply();
            return t;
        }

        static float Hash(int x, int y) => (float)Codebound.Core.WorldGenerator.Hash01(x, y, 99);

        /// <summary>A polygon (top-left origin, pixels) filled by `fill(x, y, distanceInside)`, anti-aliased.</summary>
        static Texture2D Shape(int w, int h, Vector2[] poly, Func<int, int, float, Color> fill)
        {
            return Tex(w, h, (x, ty) =>
            {
                int y = h - 1 - ty;
                float d = Poly(new Vector2(x + 0.5f, y + 0.5f), poly);
                float a = Mathf.Clamp01(0.5f - d);
                if (a <= 0) return Color.clear;
                var c = fill(x, y, -d);
                c.a *= a;
                return c;
            });
        }

        static float SegDist(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float k = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * k).magnitude;
        }

        static void Build()
        {
            built = Settings.Version;
            bool hc = Settings.HighContrast;
            var steel = UiTheme.Hex(0x3C4652); var amber = UiTheme.Accent;
            glow = Tex(64, 64, (x, y) => { float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32f; float a = Mathf.Clamp01(1 - d); return new Color(1, 1, 1, a * a * a); });
            dot = Tex(16, 16, (x, y) => { float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f)) / 8f; return new Color(1, 1, 1, Mathf.Clamp01(1 - d) * Mathf.Clamp01(1 - d)); });
            vignette = Tex(128, 72, (x, y) =>
            {
                float dx = (x - 63.5f) / 64f, dy = (y - 35.5f) / 36f;
                float d = Mathf.Sqrt(dx * dx * 0.7f + dy * dy);
                return new Color(0, 0, 0, Mathf.Clamp01((d - 0.6f) * 1.2f) * 0.8f);
            });

            // the console: a dark slab whose right edge is a diagonal (bottom narrower than top)
            const int CW = 256, CH = 256; const float bottom = 0.72f;
            console = Shape(CW, CH, new[] { new Vector2(0, 0), new Vector2(CW, 0), new Vector2(CW * bottom, CH), new Vector2(0, CH) }, (x, y, inside) =>
            {
                float edgeX = Mathf.Lerp(CW, CW * bottom, y / (float)CH);
                float k = x / edgeX;
                var c = hc ? Color.black : UiTheme.Hex(0x0A0C0F);
                c.a = Mathf.Lerp(0.9f, 0.8f, k) * (0.97f + 0.03f * Hash(x / 3, y));
                return c;
            });

            // the selection blade: slanted back like the console edge, arrow tip at the front
            const int BW = 256, BH = 64;
            var bp = new[] { new Vector2(14, 0), new Vector2(226, 0), new Vector2(254, 32), new Vector2(226, 64), new Vector2(0, 64) };
            blade = Shape(BW, BH, bp, (x, y, inside) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (SegDist(p, bp[4], bp[0]) < 4) return amber;                                        // lit back edge
                if (x > 222 && (SegDist(p, bp[1], bp[2]) < 2 || SegDist(p, bp[2], bp[3]) < 2)) return amber; // tip
                if (y < 1.5f) return Color.Lerp(steel, Color.white, 0.25f);
                var c = Color.Lerp(hc ? UiTheme.Hex(0x303030) : UiTheme.Hex(0x2E3640), hc ? UiTheme.Hex(0x181818) : UiTheme.Hex(0x1A1F26), x / (float)BW);
                c = Color.Lerp(c, amber, 0.16f * Mathf.Pow(1 - x / (float)BW, 3));
                c *= 0.97f + 0.05f * Hash(x / 5, y);
                c.a = 0.95f;
                return c;
            });

            // asymmetric plate: large cut top-left, small cut bottom-right, amber light along the big cut, a tick on top
            const int PW = 128, PH = 128;
            var pp = new[] { new Vector2(30, 0), new Vector2(PW, 0), new Vector2(PW, PH - 12), new Vector2(PW - 12, PH), new Vector2(0, PH), new Vector2(0, 30) };
            plate = Shape(PW, PH, pp, (x, y, inside) =>
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (SegDist(p, pp[5], pp[0]) < 2.2f) return amber;
                if (y < 3 && x > 40 && x < 70) return amber;
                if (inside < 1.2f) return steel;
                var c = Color.Lerp(hc ? Color.black : UiTheme.Hex(0x1E242C), hc ? Color.black : UiTheme.Hex(0x111418), y / (float)PH);
                c *= 0.97f + 0.05f * Hash(x / 5, y);
                c.a = 0.94f;
                return c;
            });

            // footer strip: square left end at the screen edge, slanted right end
            strip = Shape(128, 32, new[] { new Vector2(0, 0), new Vector2(104, 0), new Vector2(127, 32), new Vector2(0, 32) }, (x, y, inside) =>
            {
                if (y < 1.5f) return x < 40 ? amber : steel;
                var c = UiTheme.Hex(0x0B0D10); c.a = 0.92f; return c;
            });

            node = Tex(24, 24, (x, y) =>
            {
                float d = (Mathf.Abs(x - 11.5f) + Mathf.Abs(y - 11.5f)) - 9f;
                return new Color(1, 1, 1, Mathf.Clamp01(0.7f - d));
            });
            hazard = Tex(64, 16, (x, y) => ((x + y) / 6) % 2 == 0 ? amber : UiTheme.Hex(0x111111), FilterMode.Bilinear);
            BuildConsole(hc, steel, amber);
            icons = new Texture2D[7];
            for (int i = 0; i < icons.Length; i++) icons[i] = MakeIcon((Icon)i);
        }

        // ---------------------------------------------------------------- the control console (left slab, buttons, emblem)
        static Texture2D slab, vent, btnIdle, btnHot, btnPrimary, btnPrimaryHot, btnDown, well, emblem;

        /// <summary>Brushed dark steel: vertical gradient, horizontal grain, a few scratches.</summary>
        static Color Steel(int x, int y, int h, Color top, Color bottom)
        {
            var c = Color.Lerp(top, bottom, y / (float)h);
            c *= 0.96f + 0.05f * Hash(1, y) + 0.015f * Hash(x / 3, y / 3);   // brushed along the length (stretches cleanly)
            return c;
        }

        static void BuildConsole(bool hc, Color steel, Color amber)
        {
            // the slab: wide at the top, a stepped cut down the right edge, bolts, a lit seam along the edge
            const int W = 256, H = 512;
            var sp = new[] { new Vector2(0, 0), new Vector2(232, 0), new Vector2(256, 24), new Vector2(256, 150), new Vector2(240, 166), new Vector2(240, 360),
                             new Vector2(222, 378), new Vector2(222, 512), new Vector2(0, 512) };
            slab = Shape(W, H, sp, (x, y, inside) =>
            {
                if (inside < 1.6f) return x > 200 && y > 170 && y < 350 ? amber : Color.Lerp(steel, Color.white, 0.2f);
                if (inside < 4f) return UiTheme.Hex(0x07080A);
                var c = Steel(x, y, H, hc ? Color.black : UiTheme.Hex(0x23272D), hc ? Color.black : UiTheme.Hex(0x15181C));
                foreach (var b in new[] { new Vector2(14, 14), new Vector2(214, 14), new Vector2(14, 498), new Vector2(206, 498), new Vector2(226, 180), new Vector2(226, 340) })
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), b);
                    if (d < 4.2f) return d < 2.2f ? UiTheme.Hex(0x5A6370) : UiTheme.Hex(0x0B0C0E);
                }
                c.a = 0.97f;
                return c;
            });
            vent = Tex(96, 24, (x, y) =>
            {
                int k = (x + y) % 12;
                if (x < 2 || y < 2 || x > 93 || y > 21) return UiTheme.Hex(0x0B0C0E);
                return k < 5 ? UiTheme.Hex(0x08090B) : UiTheme.Hex(0x30353C);
            });

            // buttons: chamfered back, arrow tip; idle steel, hot with a lit rim, primary in safety orange
            var bp = new[] { new Vector2(12, 0), new Vector2(228, 0), new Vector2(255, 32), new Vector2(228, 64), new Vector2(0, 64), new Vector2(0, 12) };
            Func<Color, Color, Color, float, Texture2D> button = (top, bottom, rim, rimW) => Shape(256, 64, bp, (x, y, inside) =>
            {
                if (inside < rimW) return rim;
                if (inside < rimW + 1.5f) return UiTheme.Hex(0x060708);
                var c = Steel(x, y, 64, top, bottom);
                if (y < rimW + 3) c = Color.Lerp(c, Color.white, 0.12f);   // top bevel highlight
                c.a = 0.97f;
                return c;
            });
            btnIdle = button(hc ? UiTheme.Hex(0x202020) : UiTheme.Hex(0x2C3138), hc ? Color.black : UiTheme.Hex(0x1A1D22), hc ? Color.white : UiTheme.Hex(0x4A525C), 1.4f);
            btnHot = button(hc ? UiTheme.Hex(0x303030) : UiTheme.Hex(0x343A42), hc ? UiTheme.Hex(0x202020) : UiTheme.Hex(0x1F2328), amber, 2.2f);
            btnDown = button(UiTheme.Hex(0x3A2812), UiTheme.Hex(0x22180C), amber, 2.2f);
            btnPrimary = button(UiTheme.Hex(0xF59A2E), UiTheme.Hex(0xC4580C), UiTheme.Hex(0xFFC066), 1.6f);
            btnPrimaryHot = button(UiTheme.Hex(0xFFB24A), UiTheme.Hex(0xD9670F), Color.white, 2.0f);
            well = Shape(64, 64, new[] { new Vector2(8, 0), new Vector2(64, 0), new Vector2(64, 64), new Vector2(0, 64), new Vector2(0, 8) }, (x, y, inside) =>
                inside < 1.2f ? UiTheme.Hex(0x0A0B0D) : new Color(0, 0, 0, 0.42f));

            // the Codebound emblem: a gear ring around a C, safety orange with a darker rim
            emblem = Tex(192, 192, (x, ty) =>
            {
                int y = 191 - ty;
                var p = new Vector2((x + 0.5f) / 96f - 1, (y + 0.5f) / 96f - 1);
                float ang = Mathf.Atan2(p.y, p.x), r = p.magnitude;
                float teeth = 0.86f + 0.11f * Mathf.Clamp(Mathf.Cos(ang * 10) * 2.6f, -1, 1);
                float gear = Mathf.Max(r - teeth, 0.62f - r);                                     // ring with teeth
                float cRing = Mathf.Abs(r - 0.38f) - 0.13f;                                        // the C...
                float a = Mathf.Abs(Mathf.DeltaAngle(ang * Mathf.Rad2Deg, 0)) * Mathf.Deg2Rad;      // ...open towards the right
                float cut = (0.62f - a) * 0.4f;
                float c = Mathf.Max(cRing, cut);
                float d = Mathf.Min(gear, c) * 96f;
                float alpha = Mathf.Clamp01(0.5f - d);
                if (alpha <= 0) return Color.clear;
                var col = Color.Lerp(UiTheme.Hex(0xFFB04A), UiTheme.Hex(0xC4500A), (p.y + 1) / 2);
                if (d > -2.2f) col = Color.Lerp(col, UiTheme.Hex(0x5A2606), 0.6f);              // rim
                col.a = alpha;
                return col;
            });
        }

        public static void Slab(Rect r) { if (Event.current.type == EventType.Repaint) GUI.DrawTexture(r, slab); }
        public static void Vent(Rect r) { if (Event.current.type == EventType.Repaint) GUI.DrawTexture(r, vent); }
        public static void Emblem(Rect r)
        {
            if (Event.current.type != EventType.Repaint) return;
            Draw(new Rect(r.x + 3, r.y + 5, r.width, r.height), emblem, new Color(0, 0, 0, 0.7f));
            GUI.DrawTexture(r, emblem);
        }

        /// <summary>The logotype with BOUND in safety orange; a rule with a slanted cut under it.</summary>
        public static void Wordmark(Rect r)
        {
            if (Event.current.type != EventType.Repaint || logo == null) return;
            const float split = 0.452f;   // where B starts in menu_logo.png
            var full = new Rect(0, 0.205f, 1, 0.795f);
            float h = r.width * logo.height * full.height / logo.width;
            var dst = new Rect(r.x, r.y, r.width, h);
            var old = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTextureWithTexCoords(new Rect(dst.x + 4, dst.y + 6, dst.width, dst.height), logo, full);
            GUI.color = old;
            GUI.DrawTextureWithTexCoords(new Rect(dst.x, dst.y, dst.width * split, dst.height), logo, new Rect(0, 0.205f, split, 0.795f));
            GUI.color = new Color(1f, 0.6f, 0.22f) * 1.35f;
            GUI.DrawTextureWithTexCoords(new Rect(dst.x + dst.width * split, dst.y, dst.width * (1 - split), dst.height), logo, new Rect(split, 0.205f, 1 - split, 0.795f));
            GUI.color = old;
        }

        public static float WordmarkHeight(float width) => logo == null ? 0 : width * logo.height * 0.795f / logo.width;

        /// <summary>A console button. `hot` 0..1 animates hover/selection. Returns true when clicked.</summary>
        public static bool ConsoleButton(Rect r, Icon icon, string label, string sub, bool primary, bool enabled, ref float hot, bool selected, int badge)
        {
            var e = Event.current;
            bool inside = r.Contains(e.mousePosition) && GUI.enabled;
            hot = Mathf.MoveTowards(hot, enabled && (inside || selected) ? 1 : 0, Ui.Dt * 8f);
            int id = GUIUtility.GetControlID(FocusType.Passive);
            bool pressed = GUIUtility.hotControl == id, clicked = false;
            switch (e.type)
            {
                case EventType.MouseDown when inside && enabled && e.button == 0: GUIUtility.hotControl = id; e.Use(); break;
                case EventType.MouseUp when GUIUtility.hotControl == id: GUIUtility.hotControl = 0; e.Use(); clicked = inside; break;
                case EventType.Repaint:
                {
                    float a = enabled ? 1 : 0.45f, slide = 6 * hot;
                    var rr = new Rect(r.x + slide, r.y, r.width, r.height);
                    if (pressed) rr = new Rect(rr.x + 2, rr.y + 2, rr.width - 2, rr.height - 2);
                    var old = GUI.color;
                    if (hot > 0.01f && Ui.Animate)   // warm glow behind the lit button
                        Draw(new Rect(rr.x - 30, rr.y - 26, rr.width + 60, rr.height + 52), glow, new Color(1f, 0.5f, 0.12f, (primary ? 0.35f : 0.22f) * hot * a));
                    GUI.color = new Color(1, 1, 1, a);
                    Sliced(rr, primary ? btnPrimary : btnIdle, 14, 30, 0, 0, new Color(1, 1, 1, a));
                    if (hot > 0.01f) Sliced(rr, pressed ? btnDown : primary ? btnPrimaryHot : btnHot, 14, 30, 0, 0, new Color(1, 1, 1, a * hot));
                    float wsz = rr.height - 12;
                    var wr = new Rect(rr.x + 8, rr.y + 6, wsz, wsz);
                    GUI.DrawTexture(wr, well);
                    var ic = primary ? Color.Lerp(new Color(1, 0.96f, 0.9f), Color.white, hot) : Color.Lerp(UiTheme.Hex(0x9AA3AD), UiTheme.AccentHi, hot);
                    ic.a = a;
                    float isz = wsz * 0.56f;
                    Draw(new Rect(wr.center.x - isz / 2, wr.center.y - isz / 2, isz, isz), icons[(int)icon], ic);
                    var lc = primary ? Color.white : Color.Lerp(UiTheme.Hex(0xD8D3C8), Color.white, hot); lc.a = a;
                    float tx = wr.xMax + 18;
                    bool two = !string.IsNullOrEmpty(sub);
                    Text(new Rect(tx, rr.y + (two ? 5 : 0), rr.width - tx + rr.x - 40, two ? rr.height * 0.6f : rr.height), label, primary ? Styles.Primary : Styles.Item, lc);
                    if (two)
                    {
                        var sc = primary ? new Color(0.18f, 0.08f, 0.02f, 0.9f * a) : new Color(UiTheme.Muted.r, UiTheme.Muted.g, UiTheme.Muted.b, a * (0.55f + 0.45f * hot));
                        Text(new Rect(tx + 1, rr.y + rr.height * 0.58f, rr.width - tx + rr.x - 40, rr.height * 0.35f), sub, Styles.Hint, sc);
                    }
                    if (hot > 0.01f) Line(new Vector2(rr.xMax - 36, rr.y + rr.height / 2 - 7), new Vector2(rr.xMax - 29, rr.y + rr.height / 2), 2, new Color(1, 1, 1, hot * a));
                    if (hot > 0.01f) Line(new Vector2(rr.xMax - 29, rr.y + rr.height / 2), new Vector2(rr.xMax - 36, rr.y + rr.height / 2 + 7), 2, new Color(1, 1, 1, hot * a));
                    if (badge > 0) Ui.Badge(new Rect(rr.xMax - 80, rr.y + (rr.height - 22) / 2, 30, 22), badge.ToString());
                    GUI.color = old;
                    break;
                }
            }
            if (clicked) Ui.Confirm();
            return clicked;
        }

        // ---------------------------------------------------------------- vector icons (signed distance fields)
        static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;
        static float Segment(Vector2 p, Vector2 a, Vector2 b, float r) => SegDist(p, a, b) - r;
        static float Box(Vector2 p, Vector2 c, Vector2 b)
        {
            Vector2 d = new Vector2(Mathf.Abs(p.x - c.x) - b.x, Mathf.Abs(p.y - c.y) - b.y);
            return new Vector2(Mathf.Max(d.x, 0), Mathf.Max(d.y, 0)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0);
        }
        static float Poly(Vector2 p, Vector2[] v)
        {
            float d = Vector2.Dot(p - v[0], p - v[0]), s = 1;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                Vector2 e = v[j] - v[i], w = p - v[i];
                Vector2 b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        }
        static float Union(float a, float b) => Mathf.Min(a, b);
        static float Cut(float a, float b) => Mathf.Max(a, -b);

        static float IconSdf(Icon i, Vector2 p)
        {
            switch (i)
            {
                case Icon.Play:
                    return Union(Cut(Circle(p, Vector2.zero, 0.86f), Circle(p, Vector2.zero, 0.72f)),
                                 Poly(p, new[] { new Vector2(-0.26f, -0.42f), new Vector2(0.46f, 0f), new Vector2(-0.26f, 0.42f) }));
                case Icon.Person:
                    return Union(Circle(p, new Vector2(0, 0.38f), 0.3f), Mathf.Max(Circle(p, new Vector2(0, -0.78f), 0.68f), -(p.y + 0.86f)));
                case Icon.People:
                {
                    float a = Union(Circle(p, new Vector2(-0.3f, 0.36f), 0.24f), Mathf.Max(Circle(p, new Vector2(-0.3f, -0.66f), 0.54f), -(p.y + 0.86f)));
                    float b = Union(Circle(p, new Vector2(0.36f, 0.28f), 0.21f), Mathf.Max(Circle(p, new Vector2(0.36f, -0.66f), 0.46f), -(p.y + 0.86f)));
                    return Union(a, Cut(b, Circle(p, new Vector2(-0.3f, -0.4f), 0.64f)));
                }
                case Icon.Map:
                {
                    var outline = Mathf.Abs(Poly(p, new[] { new Vector2(-0.84f, -0.62f), new Vector2(-0.3f, -0.8f), new Vector2(0.3f, -0.62f), new Vector2(0.84f, -0.8f),
                                                           new Vector2(0.84f, 0.62f), new Vector2(0.3f, 0.8f), new Vector2(-0.3f, 0.62f), new Vector2(-0.84f, 0.8f) })) - 0.07f;
                    return Union(Union(outline, Segment(p, new Vector2(-0.3f, -0.8f), new Vector2(-0.3f, 0.62f), 0.06f)), Segment(p, new Vector2(0.3f, -0.62f), new Vector2(0.3f, 0.8f), 0.06f));
                }
                case Icon.Gear:
                {
                    float ang = Mathf.Atan2(p.y, p.x), r = p.magnitude;
                    float teeth = 0.66f + 0.18f * Mathf.Clamp(Mathf.Cos(ang * 8) * 2.2f, -1, 1);
                    return Cut(r - teeth, Circle(p, Vector2.zero, 0.27f));
                }
                case Icon.Scroll:
                {
                    float d = Mathf.Abs(Box(p, Vector2.zero, new Vector2(0.62f, 0.8f)) - 0.04f) - 0.06f;
                    for (int k = 0; k < 3; k++) d = Union(d, Segment(p, new Vector2(-0.34f, 0.38f - k * 0.38f), new Vector2(0.34f, 0.38f - k * 0.38f), 0.07f));
                    return d;
                }
                default:
                {
                    float ring = Mathf.Abs(Circle(p, new Vector2(0, -0.08f), 0.66f)) - 0.1f;
                    ring = Cut(ring, Poly(p, new[] { new Vector2(0, -0.08f), new Vector2(-0.55f, 1.2f), new Vector2(0.55f, 1.2f) }));
                    return Union(ring, Segment(p, new Vector2(0, 0.1f), new Vector2(0, 0.86f), 0.1f));
                }
            }
        }

        static Texture2D MakeIcon(Icon i)
        {
            const int S = 96;
            return Tex(S, S, (x, y) =>
            {
                var p = new Vector2((x + 0.5f) / S * 2 - 1, (y + 0.5f) / S * 2 - 1) * 1.08f;
                float d = IconSdf(i, p) * S / 2.16f;
                return new Color(1, 1, 1, Mathf.Clamp01(0.5f - d));
            });
        }

        public static Texture2D IconTex(Icon i) => icons[(int)i];

        // ---------------------------------------------------------------- drawing primitives
        static float T => Ui.Animate ? Time.unscaledTime : 0f;

        static void Draw(Rect r, Texture t, Color c) { var old = GUI.color; GUI.color = c * old; GUI.DrawTexture(r, t); GUI.color = old; }

        static readonly GUIStyle sliced = new GUIStyle();

        static void Sliced(Rect r, Texture2D tex, int left, int right, int top, int bottom, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;   // GUIStyle.Draw only paints
            sliced.normal.background = tex; sliced.border = new RectOffset(left, right, top, bottom);
            var old = GUI.color; GUI.color = c * old;
            sliced.Draw(r, false, false, false, false);
            GUI.color = old;
        }

        /// <summary>A line from a to b (any angle), `w` thick.</summary>
        public static void Line(Vector2 a, Vector2 b, float w, Color c)
        {
            if (Event.current.type != EventType.Repaint) return;
            var d = b - a;
            var m = GUI.matrix;
            // GUIUtility.RotateAroundPivot ignores the interface scale already in GUI.matrix: compose by hand
            GUI.matrix = m * Matrix4x4.TRS(new Vector3(a.x, a.y, 0), Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg), Vector3.one);
            Ui.Fill(new Rect(0, -w / 2, d.magnitude, w), c);
            GUI.matrix = m;
        }

        // ---------------------------------------------------------------- the screen layers
        public static void Background(Rect r)
        {
            if (Event.current.type != EventType.Repaint) return;
            Ui.Fill(r, UiTheme.Hex(0x07090B));
            Texture tex = VideoFrame() ?? still;
            float aspect = 16f / 9f, w = r.width, h = w / aspect;
            if (h < r.height) { h = r.height; w = h * aspect; }
            float t = T, zoom = 1.02f + 0.008f * Mathf.Sin(t * 0.05f);
            w *= zoom; h *= zoom;
            GUI.DrawTexture(new Rect(r.x + (r.width - w) * 0.6f, r.y + (r.height - h) * 0.45f, w, h), tex);
            if (Ui.Animate)   // dust drifting through the lamp light
                for (int i = 0; i < 60; i++)
                {
                    float h1 = Hash(i, 1), h2 = Hash(i, 2), h3 = Hash(i, 3);
                    float life = Mathf.Repeat(t * (0.02f + 0.03f * h3) + h1, 1f);
                    float x = r.x + r.width * (0.35f + 0.65f * h2) + Mathf.Sin(t * 0.4f + i) * 18;
                    float y = r.y + r.height * (0.95f - life * 0.9f);
                    float a = Mathf.Sin(life * Mathf.PI) * (0.15f + 0.28f * h3), sz = 2 + 5 * h3;
                    Draw(new Rect(x, y, sz, sz), dot, new Color(1f, 0.78f, 0.5f, a));
                }
            GUI.DrawTexture(r, vignette);
        }

        /// <summary>The slanted console on the left. Returns the x of its edge at a given y.</summary>
        public static Func<float, float> Console(Rect r, float topW)
        {
            float bottomW = topW * 0.72f;
            if (Event.current.type == EventType.Repaint)
            {
                GUI.DrawTexture(new Rect(r.x, r.y, topW, r.height), console);
                // steel edge with an amber segment and a hazard block at its foot
                var a = new Vector2(r.x + topW, r.y); var b = new Vector2(r.x + bottomW, r.yMax);
                Line(a, b, 2, UiTheme.Hex(0x4A5461, 0.9f));
                Line(Vector2.Lerp(a, b, 0.18f), Vector2.Lerp(a, b, 0.3f), 4, UiTheme.Accent);
                Line(Vector2.Lerp(a, b, 0.9f) + new Vector2(6, 0), Vector2.Lerp(a, b, 0.97f) + new Vector2(6, 0), 3, UiTheme.Accent);
                for (int i = 0; i < 6; i++)   // gauge ticks along the edge
                {
                    var p = Vector2.Lerp(a, b, 0.36f + i * 0.03f);
                    Line(p + new Vector2(-14, 0), p + new Vector2(-6, 0), 1, UiTheme.Hex(0x5A6573, 0.9f));
                }
            }
            return y => r.x + Mathf.Lerp(topW, bottomW, (y - r.y) / r.height);
        }

        public static void Logo(Rect r)
        {
            if (Event.current.type != EventType.Repaint || logo == null) return;
            var src = new Rect(0, 0.205f, 1, 0.795f);
            float h = r.width * logo.height * src.height / logo.width;
            var dst = new Rect(r.x, r.y, r.width, h);
            var old = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTextureWithTexCoords(new Rect(dst.x + 5, dst.y + 7, dst.width, dst.height), logo, src);
            GUI.color = old;
            GUI.DrawTextureWithTexCoords(dst, logo, src);
            // a rule under the letters that ends in a slanted cut, then a short hazard block
            float y = dst.yMax + 12;
            Ui.Fill(new Rect(r.x, y, r.width * 0.62f, 4), UiTheme.Accent);
            Line(new Vector2(r.x + r.width * 0.62f, y + 2), new Vector2(r.x + r.width * 0.62f + 14, y - 8), 3, UiTheme.Accent);
            Ui.Fill(new Rect(r.x + r.width * 0.62f + 14, y - 9, r.width * 0.25f, 2), UiTheme.Hex(0x4A5461));
            GUI.DrawTexture(new Rect(r.x + r.width * 0.9f, y - 12, r.width * 0.1f, 8), hazard);
        }

        public static float LogoHeight(float width) => logo == null ? 0 : width * logo.height * 0.795f / logo.width + 18;

        /// <summary>The rail the navigation hangs on: a line through the nodes, with a short hazard tick at each group break.</summary>
        public static void Rail(Vector2 top, Vector2 bottom)
        {
            Line(top, bottom, 2, UiTheme.Hex(0x3A434E, 0.95f));
            float pulse = Mathf.Repeat(T * 0.35f, 1f);   // a signal travelling down the rail
            if (Ui.Animate) Line(Vector2.Lerp(top, bottom, pulse), Vector2.Lerp(top, bottom, Mathf.Min(1, pulse + 0.06f)), 2, new Color(UiTheme.Accent.r, UiTheme.Accent.g, UiTheme.Accent.b, 0.8f));
        }

        public static void Break(Vector2 at)
        {
            if (Event.current.type != EventType.Repaint) return;
            Line(at + new Vector2(-5, 0), at + new Vector2(34, 0), 1, UiTheme.Hex(0x4A5461));
            GUI.DrawTexture(new Rect(at.x + 40, at.y - 3, 26, 6), hazard);
        }

        public static void Node(Vector2 c, bool lit, float glowAmount)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (lit && glowAmount > 0.01f) Draw(new Rect(c.x - 22, c.y - 22, 44, 44), glow, new Color(1f, 0.55f, 0.15f, 0.55f * glowAmount));
            Draw(new Rect(c.x - 7, c.y - 7, 14, 14), node, lit ? UiTheme.AccentHi : UiTheme.Hex(0x55606E));
            if (!lit) Draw(new Rect(c.x - 4, c.y - 4, 8, 8), node, UiTheme.Hex(0x0A0C0F));
        }

        public static void Blade(Rect r, float alpha) => Sliced(r, blade, 22, 36, 0, 0, new Color(1, 1, 1, alpha));

        public static void Plate(Rect r) => Sliced(r, plate, 34, 16, 34, 16, Color.white);

        public static void Strip(Rect r) => Sliced(r, strip, 6, 30, 0, 0, Color.white);

        public static void Hazard(Rect r) { if (Event.current.type == EventType.Repaint) GUI.DrawTexture(r, hazard); }

        // ---------------------------------------------------------------- text styles
        public static class Styles
        {
            static int v = -1;
            static GUIStyle item, primary, hint, small, caption, title, link;

            static void Ensure()
            {
                if (v == Settings.Version && item != null) return;
                v = Settings.Version;
                item = new GUIStyle { font = UiTheme.Heading, fontSize = UiGui.Font(23), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow, normal = { textColor = UiTheme.Text } };
                primary = new GUIStyle(item) { fontSize = UiGui.Font(31) };
                hint = new GUIStyle { font = UiTheme.Body, fontSize = UiGui.Font(13), alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip, normal = { textColor = UiTheme.Muted } };
                small = new GUIStyle(hint) { fontSize = UiGui.Font(12), richText = true };
                caption = new GUIStyle { font = UiTheme.Heading, fontSize = UiGui.Font(13), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = UiTheme.Accent } };
                title = new GUIStyle(item) { fontSize = UiGui.Font(21) };
                link = new GUIStyle(hint) { fontSize = UiGui.Font(13), fontStyle = FontStyle.Bold, font = UiTheme.Heading };
            }

            public static GUIStyle Item { get { Ensure(); return item; } }
            public static GUIStyle Primary { get { Ensure(); return primary; } }
            public static GUIStyle Hint { get { Ensure(); return hint; } }
            public static GUIStyle Small { get { Ensure(); return small; } }
            public static GUIStyle Caption { get { Ensure(); return caption; } }
            public static GUIStyle Title { get { Ensure(); return title; } }
            public static GUIStyle Link { get { Ensure(); return link; } }
        }

        public static void Text(Rect r, string s, GUIStyle st, Color c)
        {
            var old = st.normal.textColor; st.normal.textColor = c;
            GUI.Label(r, s, st);
            st.normal.textColor = old;
        }

        /// <summary>A text link with an underline that grows on hover. Returns true when clicked.</summary>
        public static bool Link(Rect r, string s, ref float hover)
        {
            var e = Event.current;
            bool inside = r.Contains(e.mousePosition) && GUI.enabled;
            hover = Mathf.MoveTowards(hover, inside ? 1 : 0, Ui.Dt * 8f);
            Text(r, s, Styles.Link, Color.Lerp(UiTheme.Muted, UiTheme.AccentHi, hover));
            if (e.type == EventType.Repaint)
            {
                float w = Styles.Link.CalcSize(new GUIContent(s)).x;
                Ui.Fill(new Rect(r.x, r.y + r.height - 3, w * (0.25f + 0.75f * hover), 1), Color.Lerp(UiTheme.Hex(0x4A5461), UiTheme.Accent, hover));
            }
            if (inside && e.type == EventType.MouseUp && e.button == 0) { e.Use(); Ui.Click(); return true; }
            return false;
        }
    }
}
