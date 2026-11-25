using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// The menus' living background: two parallax factory skylines with chimneys, blinking lights, rising smoke and a
    /// conveyor carrying crates along the bottom. Generated in code; static when "reduce motion" is on.
    /// </summary>
    public static class MenuBackdrop
    {
        static Texture2D far, near, smoke, glow, crate, sky, shade;
        static int built = -1;
        static readonly List<Vector2> chimneysFar = new List<Vector2>(), chimneysNear = new List<Vector2>(), lights = new List<Vector2>();
        const int FarW = 640, FarH = 150, NearW = 512, NearH = 190;

        static float Hash(int x, int s) => (float)WorldGenerator.Hash01(x, 7, s);

        static Texture2D Skyline(int w, int h, int seed, Color body, Color edge, List<Vector2> chimneys, bool windows)
        {
            var px = new Color[w * h];
            var tops = new int[w];
            int x = 0;
            while (x < w)
            {
                int bw = 18 + (int)(Hash(x, seed) * 46), bh = (int)(h * (0.25f + Hash(x, seed + 1) * 0.5f));
                bool tank = Hash(x, seed + 2) > 0.78f;
                for (int i = 0; i < bw && x + i < w; i++)
                {
                    int top = bh;
                    if (tank) { float u = (i - bw / 2f) / (bw / 2f); top = (int)(h * 0.22f + Mathf.Sqrt(Mathf.Max(0, 1 - u * u)) * h * 0.12f); }
                    else if (Hash(x, seed + 3) > 0.5f) top = bh + (int)(Mathf.Repeat(i, 12) * 0.9f);   // saw-tooth roof
                    tops[x + i] = Mathf.Max(tops[x + i], top);
                }
                if (Hash(x, seed + 4) > 0.45f && x + bw / 2 < w)   // a chimney
                {
                    int cx = x + bw / 3, ch = bh + 30 + (int)(Hash(x, seed + 5) * 40);
                    for (int i = 0; i < 6 && cx + i < w; i++) tops[cx + i] = Mathf.Max(tops[cx + i], Mathf.Min(h - 2, ch));
                    chimneys?.Add(new Vector2((cx + 3f) / w, Mathf.Min(h - 2, ch) / (float)h));
                }
                x += bw + (int)(Hash(x, seed + 6) * 6);
            }
            for (int yy = 0; yy < h; yy++)
                for (int xx = 0; xx < w; xx++)
                {
                    int t = tops[xx];
                    Color c = Color.clear;
                    if (yy < t) c = yy == t - 1 ? edge : body;
                    if (windows && yy < t - 6 && yy > 8 && xx % 7 < 2 && yy % 9 < 2 && Hash(xx * 31 + yy, seed + 9) > 0.93f) { c = UiTheme.Hex(0xF2A23A, 0.85f); if (lights.Count < 60) lights.Add(new Vector2(xx / (float)w, yy / (float)h)); }
                    px[yy * w + xx] = c;
                }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat, hideFlags = HideFlags.HideAndDontSave };
            tex.SetPixels(px); tex.Apply();
            return tex;
        }

        static Texture2D Soft(int s, Color c)
        {
            var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(s / 2f, s / 2f)) / (s / 2f);
                    t.SetPixel(x, y, new Color(c.r, c.g, c.b, c.a * Mathf.Clamp01(1 - d) * Mathf.Clamp01(1 - d)));
                }
            t.Apply();
            return t;
        }

        static void Build()
        {
            built = Settings.Version;
            chimneysFar.Clear(); chimneysNear.Clear(); lights.Clear();
            bool hc = Settings.HighContrast;
            far = Skyline(FarW, FarH, 11, hc ? Color.black : UiTheme.Hex(0x1A2027), hc ? Color.black : UiTheme.Hex(0x262E37), chimneysFar, false);
            near = Skyline(NearW, NearH, 23, hc ? Color.black : UiTheme.Hex(0x0F1216), hc ? Color.black : UiTheme.Hex(0x1C2128), chimneysNear, true);
            smoke = Soft(32, new Color(0.62f, 0.64f, 0.68f, 0.22f));
            glow = Soft(32, UiTheme.Hex(0xF2A23A, 0.55f));
            crate = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++) crate.SetPixel(x, y, x == 0 || y == 0 || x == 7 || y == 7 ? UiTheme.Hex(0x5A3A14) : (x == y || x == 7 - y ? UiTheme.Hex(0x8A5A22) : UiTheme.Hex(0xB07A32)));
            crate.Apply();
            sky = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < 64; y++) sky.SetPixel(0, y, hc ? Color.black : Color.Lerp(UiTheme.Hex(0x0B0D10), UiTheme.Hex(0x1B2129), y / 63f));
            sky.Apply();
            shade = new Texture2D(64, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            for (int x = 0; x < 64; x++) shade.SetPixel(x, 0, new Color(0, 0, 0, 0.6f * Mathf.Pow(1 - x / 63f, 1.6f)));
            shade.Apply();
        }

        /// <summary>True while the menus are drawn on the 3D terminal's glass (TitleMenu): the backdrop is the screen itself.</summary>
        public static bool Terminal;
        static Texture2D scan, sheen, glowTex;

        /// <summary>The terminal glass: a dark phosphor field with a faint grid.</summary>
        static void TerminalField(Rect r)
        {
            Ui.Fill(r, UiTheme.Hex(0x0B1C21));
            var c = r.center; float gw = r.width * 0.9f, gh = r.height * 0.9f;   // phosphor glow in the middle of the tube
            if (glowTex == null) { glowTex = new Texture2D(32, 32, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave }; for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++) { float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 16f; glowTex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(1 - d) * Mathf.Clamp01(1 - d))); } glowTex.Apply(); }
            var old = GUI.color; GUI.color = new Color(0.25f, 0.55f, 0.55f, 0.35f) * old; GUI.DrawTexture(new Rect(c.x - gw / 2, c.y - gh / 2, gw, gh), glowTex); GUI.color = old;
            if (scan == null)
            {
                scan = new Texture2D(1, 4, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
                scan.SetPixels(new[] { new Color(0, 0, 0, 0.2f), new Color(0, 0, 0, 0.0f), new Color(0, 0, 0, 0.0f), new Color(0, 0, 0, 0.07f) }); scan.Apply();
                sheen = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                    {
                        float dx = (x - 31.5f) / 32f, dy = (y - 31.5f) / 32f, d = Mathf.Sqrt(dx * dx + dy * dy);
                        float vig = Mathf.Clamp01((d - 0.7f) * 1.4f) * 0.45f;              // dark corners of the tube
                        float refl = Mathf.Clamp01(1 - Mathf.Abs((x + (63 - y)) / 126f - 0.32f) * 8f) * 0.05f;   // a diagonal reflection
                        sheen.SetPixel(x, y, vig > refl ? new Color(0, 0, 0, vig) : new Color(1, 0.95f, 0.85f, refl));
                    }
                sheen.Apply();
            }
            var g = new Color(0.25f, 0.55f, 0.55f, 0.06f);
            for (float x = r.x + 40; x < r.xMax; x += 40) Ui.Fill(new Rect(x, r.y, 1, r.height), g);
            for (float y = r.y + 40; y < r.yMax; y += 40) Ui.Fill(new Rect(r.x, y, r.width, 1), g);
        }

        /// <summary>The dark veil screens lay over the factory backdrop; on the terminal glass the screen is its own backdrop.</summary>
        public static void Dim(Rect r, float a) { if (!Terminal) Ui.Fill(r, new Color(0, 0, 0, a)); }

        /// <summary>Over the whole picture on the glass: scanlines, the tube's dark corners, a reflection.</summary>
        public static void Glass(Rect r)
        {
            if (Event.current.type != EventType.Repaint || scan == null) return;
            GUI.DrawTextureWithTexCoords(r, scan, new Rect(0, 0, 1, r.height / 3f));
            GUI.DrawTexture(r, sheen);
        }

        public static void Draw(Rect r)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (Terminal) { TerminalField(r); return; }
            if (far == null || built != Settings.Version) Build();
            float time = Ui.Animate ? Time.unscaledTime : 0;
            GUI.DrawTexture(r, sky);
            // a dim warm horizon glow
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, 0.35f);
            GUI.DrawTexture(new Rect(r.x - r.width * 0.2f, r.yMax - r.height * 0.55f, r.width * 1.4f, r.height * 0.6f), glow);
            GUI.color = old;

            float ground = r.yMax - 46;
            Layer(r, far, FarW, FarH, ground - 30, r.height * 0.30f, time * 6f, chimneysFar, 0.8f, time);
            Layer(r, near, NearW, NearH, ground, r.height * 0.30f, time * 14f, chimneysNear, 1.1f, time);

            // conveyor along the bottom
            Ui.Fill(new Rect(r.x, ground, r.width, r.yMax - ground), UiTheme.Hex(0x090B0D));
            Ui.Fill(new Rect(r.x, ground, r.width, 2), UiTheme.Hex(0x2A3139));
            float belt = ground + 12, step = 24, off = Mathf.Repeat(time * 40f, step);
            Ui.Fill(new Rect(r.x, belt, r.width, 14), UiTheme.Hex(0x1A1F25));
            for (float x = r.x - step + off; x < r.xMax; x += step) Ui.Fill(new Rect(x, belt + 5, 10, 3), UiTheme.Hex(0x3A434D));
            float cs = 18, gap = 150, coff = Mathf.Repeat(time * 40f, gap);
            for (float x = r.x - gap + coff; x < r.xMax; x += gap) GUI.DrawTexture(new Rect(x, belt - cs + 3, cs, cs), crate);
            // shade from the left keeps the menu text readable
            GUI.DrawTexture(new Rect(r.x, r.y, r.width * 0.7f, r.height), shade);
        }

        static void Layer(Rect r, Texture2D tex, int tw, int th, float baseY, float height, float scroll, List<Vector2> chimneys, float smokeScale, float time)
        {
            float scale = height / th, w = tw * scale;
            float off = Mathf.Repeat(scroll, w);
            for (float x = r.x - off; x < r.xMax; x += w)
            {
                GUI.DrawTexture(new Rect(x, baseY - height, w, height), tex);
                foreach (var c in chimneys)
                {
                    float cx = x + c.x * w, cy = baseY - c.y * height;
                    if (cx < r.x - 60 || cx > r.xMax + 60) continue;
                    for (int i = 0; i < 6; i++)   // puffs rising and fading
                    {
                        float age = Mathf.Repeat(time * 0.25f + i / 6f + c.x * 3.1f, 1f);
                        float size = (16 + age * 70) * smokeScale;
                        var old = GUI.color; GUI.color = new Color(1, 1, 1, (1 - age) * 0.9f);
                        GUI.DrawTexture(new Rect(cx - size / 2 + age * 40 * smokeScale, cy - size / 2 - age * 120 * smokeScale, size, size), smoke);
                        GUI.color = old;
                    }
                }
            }
            if (tex == near)
                foreach (var l in lights)   // a few windows pulse
                {
                    float k = 0.5f + 0.5f * Mathf.Sin(time * 2f + l.x * 40f);
                    if (k < 0.75f) continue;
                    for (float x = r.x - off; x < r.xMax; x += w)
                    {
                        var old = GUI.color; GUI.color = new Color(1, 1, 1, (k - 0.75f) * 2.4f);
                        GUI.DrawTexture(new Rect(x + l.x * w - 10, baseY - l.y * height - 10, 20, 20), glow);
                        GUI.color = old;
                    }
                }
        }
    }

    /// <summary>The 12 profile avatars: pixel-art helmets in six colours with two visor shapes.</summary>
    public static class Avatars
    {
        static readonly Texture2D[] cache = new Texture2D[ProfileStore.Avatars];
        static readonly int[] Colors = { 0xF2A23A, 0x5EC4D9, 0x8BD45E, 0xE0604A, 0xB98AE8, 0xE8E3D7 };

        public static Texture2D Get(int i)
        {
            i = Mathf.Clamp(i, 0, ProfileStore.Avatars - 1);
            if (cache[i] != null) return cache[i];
            var main = UiTheme.Hex(Colors[i % 6]);
            bool slit = i >= 6;
            var t = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    int top = 15 - y;
                    float dx = x - 7.5f, dy = top - 7f;
                    Color c = UiTheme.Hex(0x15181D);
                    bool head = dx * dx / 42f + dy * dy / 40f <= 1f && top >= 2;
                    if (head)
                    {
                        c = Color.Lerp(main, main * 0.6f, top / 16f);
                        bool visor = slit ? (top >= 7 && top <= 8 && Mathf.Abs(dx) <= 4.5f) : (top >= 6 && top <= 10 && Mathf.Abs(dx) <= 4f && !(top == 10 && Mathf.Abs(dx) > 3));
                        if (visor) c = top == (slit ? 7 : 7) && dx < -1 ? UiTheme.Hex(0x9FE8FF) : UiTheme.Hex(0x10161C);
                        if (top == 3 && Mathf.Abs(dx) < 1) c = Color.white * 0.9f;
                    }
                    if (top >= 13) c = UiTheme.Hex(0x2A3139);   // shoulders
                    if (top == 1 && Mathf.Abs(dx) < 1) c = main;   // antenna
                    if (top == 0 && Mathf.Abs(dx) < 1) c = UiTheme.Hex(0xFFC066);
                    t.SetPixel(x, y, c);
                }
            t.Apply();
            return cache[i] = t;
        }

        public static void Draw(Rect r, int i, bool selected = false)
        {
            if (Event.current.type != EventType.Repaint) return;
            Ui.Fill(r, selected ? UiTheme.Accent : UiTheme.Line);
            float b = selected ? 3 : 1;
            GUI.DrawTexture(new Rect(r.x + b, r.y + b, r.width - 2 * b, r.height - 2 * b), Get(i));
        }
    }

    /// <summary>
    /// Map previews made from real world data: terrain and deposits from the world generator of the save's seed,
    /// buildings from the save's own "b" lines (or the live state when saving). Cached per world as &lt;id&gt;.png.
    /// </summary>
    public static class WorldPreview
    {
        public const int Size = 112;
        static Color32[] tileColors;

        static Color32 Tile(TileType t)
        {
            if (tileColors == null)
            {
                var n = Enum.GetValues(typeof(TileType)).Length;
                tileColors = new Color32[n];
                for (int i = 0; i < n; i++) tileColors[i] = PixelArtFactory.BaseOf((TileType)i);
            }
            return tileColors[(int)t];
        }

        static Color32 Ore(ResourceType r)
        {
            switch (r)
            {
                case ResourceType.Tree: return new Color32(40, 92, 44, 255);
                case ResourceType.Stone: return new Color32(150, 146, 140, 255);
                case ResourceType.Iron: return new Color32(120, 160, 190, 255);
                case ResourceType.Copper: return new Color32(214, 120, 64, 255);
                case ResourceType.Coal: return new Color32(30, 30, 34, 255);
                case ResourceType.Silicon: return new Color32(200, 200, 225, 255);
                case ResourceType.Titanium: return new Color32(170, 190, 210, 255);
                case ResourceType.Uranium: return new Color32(120, 230, 80, 255);
                case ResourceType.Oil: return new Color32(10, 10, 12, 255);
                default: return new Color32(200, 90, 230, 255);
            }
        }

        /// <summary>Pixels (row 0 = bottom) of the area around `center`, 2 cells per pixel. Pure: safe off the main thread.</summary>
        public static Color32[] Pixels(WorldGenerator gen, Cell center, IEnumerable<(Cell at, int w, int h)> buildings, Cell? player)
        {
            const int scale = 2;
            var px = new Color32[Size * Size];
            int x0 = center.X - Size * scale / 2, y0 = center.Y - Size * scale / 2;
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    var c = new Cell(x0 + x * scale, y0 + y * scale);
                    var d = gen.DepositAt(c);
                    var col = d.IsNone ? Tile(gen.TileAt(c)) : Ore(d.Type);
                    if (((x + y) & 1) == 0) col = new Color32((byte)(col.r * 0.94f), (byte)(col.g * 0.94f), (byte)(col.b * 0.94f), 255);
                    px[y * Size + x] = col;
                }
            var b = new Color32(242, 162, 58, 255);
            if (buildings != null)
                foreach (var (at, w, h) in buildings)
                    for (int dy = 0; dy < Math.Max(1, h); dy += scale)
                        for (int dx = 0; dx < Math.Max(1, w); dx += scale)
                        {
                            int x = (at.X + dx - x0) / scale, y = (at.Y + dy - y0) / scale;
                            if (x >= 0 && y >= 0 && x < Size && y < Size) px[y * Size + x] = b;
                        }
            if (player is Cell p)
                for (int k = -1; k <= 1; k++)
                {
                    int x = (p.X - x0) / scale, y = (p.Y - y0) / scale;
                    foreach (var (ax, ay) in new[] { (x + k, y), (x, y + k) })
                        if (ax >= 0 && ay >= 0 && ax < Size && ay < Size) px[ay * Size + ax] = new Color32(255, 255, 255, 255);
                }
            return px;
        }

        public static Texture2D ToTexture(Color32[] px)
        {
            var t = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            t.SetPixels32(px); t.Apply();
            return t;
        }

        /// <summary>Called when saving: the preview of the live world, written next to the save.</summary>
        public static void Write(GameState g, string pngPath)
        {
            var list = new List<(Cell, int, int)>();
            foreach (var b in g.Grid.Buildings) { var (w, h) = b.Size; list.Add((b.Origin, w, h)); }
            var p = new Cell((int)Math.Floor(g.PlayerX), (int)Math.Floor(g.PlayerY));
            var tex = ToTexture(Pixels(g.World.Generator, p, list, p));
            try { File.WriteAllBytes(pngPath, tex.EncodeToPNG()); }
            finally { UnityEngine.Object.Destroy(tex); }
        }

        /// <summary>For a world without a stored preview: reads the save (seed, planet, player, buildings) off the main thread.</summary>
        public static Task<Color32[]> FromSave(string path) => Task.Run(() =>
        {
            int seed = 0, home = 0; PlanetKind kind = PlanetKind.Home; double px = 0, py = 0; bool havePlayer = false;
            var list = new List<(Cell, int, int)>();
            foreach (var line in File.ReadLines(path))
            {
                var p = line.Split(' ');
                if (p[0] == "seed" && p.Length > 1) int.TryParse(p[1], out seed);
                else if (p[0] == "planet" && p.Length > 3) { int.TryParse(p[2], out int k); kind = (PlanetKind)k; int.TryParse(p[3], out home); }
                else if (p[0] == "player" && p.Length > 2)
                    havePlayer = double.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out px)
                               & double.TryParse(p[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out py);
                else if (p[0] == "b" && p.Length > 4 && int.TryParse(p[2], out int bx) && int.TryParse(p[3], out int by))
                {
                    var def = BuildingDatabase.TryGet(p[1]);
                    int.TryParse(p[4], out int rot);
                    var (w, h) = def != null ? def.Size((Rotation)rot) : (1, 1);
                    list.Add((new Cell(bx, by), w, h));
                }
            }
            var gen = new WorldGenerator(seed, kind);
            var center = havePlayer ? new Cell((int)Math.Floor(px), (int)Math.Floor(py)) : gen.FindSpawn();
            return Pixels(gen, center, list, havePlayer ? center : (Cell?)null);
        });

        /// <summary>Terrain only, for a seed that has no save yet (the new-world screen).</summary>
        public static Task<(Color32[] px, Biome biome, Dictionary<ResourceType, int> near)> FromSeed(int seed) => Task.Run(() =>
        {
            var gen = new WorldGenerator(seed);
            var spawn = gen.FindSpawn();
            var near = new Dictionary<ResourceType, int>();
            for (int y = -40; y <= 40; y += 2)
                for (int x = -40; x <= 40; x += 2)
                {
                    var d = gen.DepositAt(new Cell(spawn.X + x, spawn.Y + y));
                    if (!d.IsNone && d.Type != ResourceType.Tree) near[d.Type] = near.TryGetValue(d.Type, out var n) ? n + 1 : 1;
                }
            return (Pixels(gen, spawn, null, spawn), gen.BiomeAt(spawn), near);
        });
    }
}
