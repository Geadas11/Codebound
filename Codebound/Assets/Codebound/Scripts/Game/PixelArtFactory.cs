using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// The game's pixel art (16 px per cell), drawn in code: terrain with soft borders, resource variants, buildings with
    /// animation frames, a walking player. Keep sprite sizes/pivots if hand-made sheets ever replace it.
    /// </summary>
    public static class PixelArtFactory
    {
        public const int PPU = 16;
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        // Play Mode without domain reload keeps statics alive while the textures they point to are destroyed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => cache.Clear();

        static Sprite Make(string key, int w, int h, System.Func<int, int, Color32> px, Vector2 pivot)
        {
            if (cache.TryGetValue(key, out var s) && s != null) return s; // `!= null` is false for destroyed objects
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = key };
            var data = new Color32[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) data[y * w + x] = px(x, y);
            tex.SetPixels32(data);
            tex.Apply(false, true);
            s = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, PPU, 0, SpriteMeshType.FullRect);
            s.name = key;
            cache[key] = s;
            return s;
        }

        static float H(int x, int y, int s) => (float)WorldGenerator.Hash01(x, y, s);
        static Color32 Shade(Color32 c, float k) => new Color32((byte)Mathf.Clamp(c.r * k, 0, 255), (byte)Mathf.Clamp(c.g * k, 0, 255), (byte)Mathf.Clamp(c.b * k, 0, 255), c.a);
        static Color32 Hex(uint rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
        static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        static readonly Color32 Outline = new Color32(24, 22, 30, 255);

        // ---------------- terrain ----------------
        public const int TileVariants = 8;

        static Color32 BaseOf(TileType t)
        {
            switch (t)
            {
                case TileType.Grass: return new Color32(66, 128, 66, 255);
                case TileType.Sand: return new Color32(214, 190, 130, 255);
                case TileType.Rock: return new Color32(112, 110, 118, 255);
                case TileType.Water: return new Color32(40, 92, 170, 255);
                case TileType.Ash: return new Color32(74, 66, 64, 255);
                case TileType.Lava: return new Color32(214, 76, 22, 255);
                case TileType.RuinFloor: return new Color32(92, 98, 106, 255);
                case TileType.ToxicSoil: return new Color32(94, 110, 54, 255);
                case TileType.CaveFloor: return new Color32(60, 54, 52, 255);
                case TileType.CaveWall: return new Color32(28, 24, 26, 255);
                default: return new Color32(86, 168, 58, 255); // toxic water
            }
        }

        /// <summary>Smooth noise on a 4-px lattice that wraps every 16 px, so a tile's blotches meet themselves at its edges.</summary>
        static float Blot(int x, int y, int seed)
        {
            float fx = x / 4f, fy = y / 4f;
            int x0 = (int)fx, y0 = (int)fy;
            float tx = fx - x0, ty = fy - y0;
            tx = tx * tx * (3 - 2 * tx); ty = ty * ty * (3 - 2 * ty);
            float a = H(x0 & 3, y0 & 3, seed), b = H((x0 + 1) & 3, y0 & 3, seed), c = H(x0 & 3, (y0 + 1) & 3, seed), d = H((x0 + 1) & 3, (y0 + 1) & 3, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        /// <summary>One ground tile: soft blotches plus details of its kind (grass blades and flowers, sand ripples and pebbles,
        /// rock cracks, water glints...). Eight variants per kind so the ground does not repeat.</summary>
        public static Sprite Tile(TileType t, int variant)
        {
            var baseC = BaseOf(t);
            int seed = (int)t * 97 + variant * 13;
            return Make($"tile_{t}_{variant}", PPU, PPU, (x, y) =>
            {
                float n = H(x, y, seed), b = Blot(x, y, seed + 7);
                var c = Shade(baseC, 0.92f + b * 0.12f + n * 0.05f);
                switch (t)
                {
                    case TileType.Grass:
                    {
                        // short blades: a light pixel over a dark one, at hashed spots
                        if (H(x, y - 1, seed + 3) > 0.9f) c = Shade(baseC, 1.32f);
                        else if (H(x, y, seed + 3) > 0.9f) c = Shade(baseC, 0.74f);
                        float f = H(x, y, seed + 11);
                        if (f > 0.993f) c = variant % 3 == 0 ? new Color32(240, 220, 90, 255) : variant % 3 == 1 ? new Color32(235, 235, 245, 255) : new Color32(150, 170, 255, 255);   // flowers
                        break;
                    }
                    case TileType.Sand:
                        if (variant % 3 == 0 && b > 0.45f && Mathf.Abs(Mathf.Sin((x + variant * 5) * 0.55f) * 2.2f + 6 + (variant & 1) * 4 - y) < 0.6f) c = Shade(baseC, 1.05f);   // ripples, only on some tiles and in patches
                        if (n > 0.975f) c = Shade(baseC, 0.72f);                                                                                     // pebbles
                        break;
                    case TileType.Water: case TileType.ToxicWater:
                        if (Mathf.Abs(Mathf.Sin((x + variant * 3) * 0.7f) * 1.2f + ((y + variant * 5) % 8) - 4) < 0.45f) c = Shade(baseC, 1.25f);   // little waves
                        else if (n > 0.985f) c = Shade(baseC, 1.5f);                                                                                 // glints
                        break;
                    case TileType.Lava:
                        if (b > 0.62f) c = Color32.Lerp(new Color32(255, 150, 40, 255), new Color32(255, 230, 120, 255), (b - 0.62f) * 2.6f);
                        else if (b < 0.3f) c = new Color32(110, 28, 12, 255);
                        break;
                    case TileType.RuinFloor:
                        if (x == 0 || y == 0 || x == 8 || y == 8) c = Shade(baseC, 0.72f);
                        else if (x == 1 || y == 1 || x == 9 || y == 9) c = Shade(baseC, 1.12f);
                        if (n > 0.97f) c = new Color32(60, 200, 210, 255); // dead circuitry glints
                        break;
                    case TileType.CaveWall:
                        if (n > 0.9f) c = Shade(baseC, 1.6f); else if (n < 0.1f) c = new Color32(12, 10, 12, 255);
                        break;
                    case TileType.CaveFloor:
                        if (n > 0.975f) c = new Color32(120, 90, 200, 255);   // faint crystal glints
                        break;
                    case TileType.Rock: case TileType.Ash:
                    {
                        // a crack: a short dark diagonal walk from a hashed start
                        int sx = (int)(H(1, 2, seed) * 12) + 2, sy = (int)(H(3, 4, seed) * 12) + 2;
                        int len = 3 + variant % 4;
                        for (int i = 0; i < len; i++) if (x == sx + i && Mathf.Abs(y - (sy + (int)(i * (H(5, 6, seed) - 0.5f) * 2))) < 1) c = Shade(baseC, 0.62f);
                        if (n > 0.965f) c = Shade(baseC, 1.3f);   // flecks
                        if (t == TileType.Ash && n < 0.006f) c = new Color32(255, 120, 40, 255);   // a last ember
                        break;
                    }
                }
                return c;
            }, Vector2.zero);
        }

        /// <summary>Which ground draws over which at a border (higher spills onto lower).</summary>
        public static int Priority(TileType t)
        {
            switch (t)
            {
                case TileType.Water: return 0;
                case TileType.ToxicWater: return 1;
                case TileType.Lava: return 2;
                case TileType.CaveFloor: return 3;
                case TileType.Sand: return 4;
                case TileType.Ash: return 5;
                case TileType.ToxicSoil: return 6;
                case TileType.Grass: return 7;
                case TileType.RuinFloor: return 8;
                case TileType.Rock: return 9;
                default: return 10;   // cave wall
            }
        }

        /// <summary>A ragged fringe of `over` along the sides in `mask` (1 north, 2 east, 4 south, 8 west), drawn on top of a tile
        /// of another kind; on water it ends in a line of foam.</summary>
        public static Sprite Edge(TileType over, int mask, bool onWater)
        {
            var baseC = BaseOf(over);
            int seed = (int)over * 31 + mask;
            return Make($"edge_{over}_{mask}_{onWater}", PPU, PPU, (x, y) =>
            {
                float best = 99;
                if ((mask & 1) != 0) best = Mathf.Min(best, PPU - 1 - y - Depth(x, seed + 1));
                if ((mask & 2) != 0) best = Mathf.Min(best, PPU - 1 - x - Depth(y, seed + 2));
                if ((mask & 4) != 0) best = Mathf.Min(best, y - Depth(x, seed + 3));
                if ((mask & 8) != 0) best = Mathf.Min(best, x - Depth(y, seed + 4));
                if (best < 0) return Shade(baseC, 0.9f + H(x, y, seed) * 0.12f + (best > -1 ? -0.08f : 0f));   // the fringe, a bit darker at its rim
                if (onWater && best < 1.2f) return new Color32(205, 230, 240, 200);                              // foam
                return Clear;
            }, Vector2.zero);
        }

        static float Depth(int along, int seed) => 2.2f + 1.6f * Mathf.Sin(along * 0.9f + seed) * 0.5f + H(along, seed, 77) * 1.2f;

        // ---------------- resources ----------------
        public const int ResourceVariants = 3;

        public static Sprite Resource(ResourceType r) => Resource(r, 0);

        /// <summary>Ore lumps / crystals / trees; three shapes each, picked per cell so deposits don't look stamped.</summary>
        public static Sprite Resource(ResourceType r, int variant)
        {
            if (r == ResourceType.Tree) return Tree(variant);
            Color32 c;
            switch (r)
            {
                case ResourceType.Stone: c = Hex(0x9a9a9e); break;
                case ResourceType.Iron: c = Hex(0xb07a62); break;
                case ResourceType.Copper: c = Hex(0xd2783a); break;
                case ResourceType.Coal: c = Hex(0x2c2c32); break;
                case ResourceType.Silicon: c = Hex(0xb8c4d8); break;
                case ResourceType.Titanium: c = Hex(0xdfe8f0); break;
                case ResourceType.Uranium: c = Hex(0x58e040); break;
                case ResourceType.Oil: c = Hex(0x141418); break;
                default: c = Hex(0xbe52e6); break; // alien crystal
            }
            int seed = (int)r * 31 + 5 + variant * 101;
            var lumps = new List<Vector3>();
            int count = 3 + variant;
            for (int i = 0; i < count; i++) lumps.Add(new Vector3(3 + H(i, 1, seed) * 10, 3 + H(i, 2, seed) * 9, 1.6f + H(i, 3, seed) * 2.2f));
            bool crystal = r == ResourceType.AlienCrystal || r == ResourceType.Silicon || r == ResourceType.Uranium;
            bool oil = r == ResourceType.Oil;
            return Make($"res_{r}_{variant}", PPU, PPU, (x, y) =>
            {
                if (oil)   // a dark pool with a sheen
                {
                    float dx0 = (x - 7.5f) / (5.5f + variant), dy0 = (y - 7f) / (3.8f + variant * 0.5f);
                    float d0 = dx0 * dx0 + dy0 * dy0;
                    if (d0 < 0.75f) return Mathf.Abs(x - 6 - y * 0.4f) < 0.8f && y > 6 ? new Color32(70, 60, 110, 255) : c;
                    return d0 < 1f ? new Color32(30, 28, 34, 180) : Clear;
                }
                foreach (var l in lumps)
                {
                    float dx = x - l.x, dy = y - l.y;
                    float d = crystal ? Mathf.Abs(dx) * 2f + Mathf.Abs(dy) * 0.75f : Mathf.Sqrt(dx * dx + dy * dy);
                    float rad = crystal ? l.z * 2f : l.z;
                    if (d < rad - 0.8f)
                    {
                        if (crystal) return dx < 0 ? Shade(c, 1.3f) : Shade(c, 0.85f);                           // lit facet / shaded facet
                        if (dx < -0.3f && dy > 0.3f && d < rad * 0.45f) return Shade(c, 1.45f);                     // highlight
                        return Shade(c, dy > 0 ? 1.12f : 0.85f);
                    }
                    if (d < rad + 0.2f) return Outline;
                }
                // small shadow under the lumps
                foreach (var l in lumps) { float dx = x - l.x - 1, dy = y - l.y + 1.2f; if (dx * dx + dy * dy < l.z * l.z) return new Color32(0, 0, 0, 60); }
                return Clear;
            }, Vector2.zero);
        }

        /// <summary>Round broadleaf, pine, or bush — with a soft shadow on the ground.</summary>
        static Sprite Tree(int variant) => Make("res_tree_" + variant, PPU, PPU, (x, y) =>
        {
            var dark = new Color32(30, 90, 40, 255); var mid = new Color32(44, 118, 52, 255); var lite = new Color32(78, 156, 74, 255);
            var trunk = new Color32(110, 72, 40, 255);
            if (variant == 1)   // pine: stacked triangles
            {
                if (x >= 7 && x <= 8 && y >= 1 && y <= 3) return x == 7 ? trunk : Shade(trunk, 0.8f);
                for (int tier = 0; tier < 3; tier++)
                {
                    int top = 15 - tier * 4, bottom = top - 6;
                    if (y > top || y < bottom) continue;
                    float half = (top - y) * 0.75f + 0.5f;
                    float d = Mathf.Abs(x - 7.5f);
                    if (d < half - 0.6f) return x < 8 ? (H(x, y, 9) > 0.7f ? lite : mid) : dark;
                    if (d < half + 0.4f) return Outline;
                }
                if (y == 0 && x >= 4 && x <= 11) return new Color32(0, 0, 0, 70);
                return Clear;
            }
            if (variant == 2)   // bush: wide and low
            {
                float bx = (x - 7.5f) / 6.5f, by = (y - 5f) / 4.2f;
                float bd = bx * bx + by * by;
                if (bd < 0.78f) return by > 0.2f ? (H(x, y, 4) > 0.75f ? lite : mid) : dark;
                if (bd < 1f) return Outline;
                if (y <= 1 && x >= 2 && x <= 13) return new Color32(0, 0, 0, 60);
                return Clear;
            }
            float dx = x - 7.5f, dy = y - 9.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            if (r < 5.2f) return H(x, y, 3) > 0.8f || (dx < -1 && dy > 1 && r < 3.5f) ? lite : (dy > 1 ? mid : dark);
            if (r < 6.2f) return Outline;
            if (x >= 6 && x <= 9 && y >= 1 && y < 5) return x == 6 || x == 9 ? Outline : (x == 7 ? trunk : Shade(trunk, 0.8f));
            if (y == 0 && x >= 4 && x <= 11) return new Color32(0, 0, 0, 70);
            return Clear;
        }, Vector2.zero);

        // ---------------- buildings ----------------
        /// <summary>Animation frames of a working machine (1 = not animated).</summary>
        public static int Frames(BuildingDef def)
        {
            switch (def.Kind)
            {
                case BuildingKind.Belt: case BuildingKind.Conveyor: case BuildingKind.Drill: case BuildingKind.Generator: case BuildingKind.Assembler: return 4;
                case BuildingKind.Furnace: case BuildingKind.Lab: case BuildingKind.Server: case BuildingKind.Scrubber: case BuildingKind.Heater: case BuildingKind.Boiler: return 2;
                case BuildingKind.Pumpjack: return 8;
                case BuildingKind.Lamp: return 2;   // 0 = off, 1 = lit (the view picks, lamps do not cycle)
                case BuildingKind.SteamEngine: case BuildingKind.OffshorePump: return 4;
                default: return 1;
            }
        }

        public static Sprite Building(BuildingDef def) => Building(def, 0);

        public static Sprite Building(BuildingDef def, int frame)
        {
            int w = def.Width * PPU, h = def.Height * PPU;
            var body = Hex(def.Color);
            return Make($"bld_{def.Id}_{frame}", w, h, (x, y) =>
            {
                if (def.Kind == BuildingKind.Belt || def.Kind == BuildingKind.Conveyor)
                {
                    if (x == 0 || x == w - 1) return Outline;
                    if (x == 1 || x == w - 2) return def.Kind == BuildingKind.Conveyor ? new Color32(70, 140, 220, 255) : new Color32(200, 170, 60, 255);
                    if (def.Id == "splitter" && (x == 7 || x == 8) && y > 3) return Shade(body, 0.55f);               // divider
                    if (def.Id == "sorter" && y >= 6 && y <= 9 && x >= 6 && x <= 9) return new Color32(110, 220, 120, 255);   // filter eye
                    float v = (y - frame * 2 + 16) % 8 - Mathf.Abs(x - 7.5f) * 0.5f;   // chevrons, scrolling to the front while it runs
                    if (v > 2 && v <= 4) return Shade(body, 1.6f);
                    if (def.Kind == BuildingKind.Conveyor && x >= 6 && x <= 9 && y <= 1) return new Color32(80, 255, 140, 255);
                    return Shade(body, (x & 1) == 0 ? 0.92f : 0.98f);                                                       // belt rubber
                }
                if (def.Kind == BuildingKind.Inserter)
                {
                    bool longArm = def.Id == "long_inserter";
                    bool plate = x >= 3 && x <= 12 && y >= 1 && y <= 6;
                    bool arm = x >= 7 && x <= 8 && y >= 5 && y <= (longArm ? 14 : 12);
                    int cy0 = longArm ? 13 : 12;
                    bool claw = y >= cy0 && y <= cy0 + 2 && (x == 5 || x == 6 || x == 9 || x == 10 || (y == cy0 && x >= 5 && x <= 10));
                    if (claw) return new Color32(60, 60, 70, 255);
                    if (arm) return x == 7 ? Shade(body, 1.25f) : body;
                    if (plate) return (x == 3 || x == 12 || y == 1 || y == 6) ? Outline : (y == 5 ? new Color32(96, 100, 110, 255) : new Color32(80, 84, 92, 255));
                    return Clear;
                }
                if (def.Id == "pole" || def.Id == "lightning_rod")
                {
                    if (def.Id == "lightning_rod")
                    {
                        if ((x == 7 || x == 8) && y >= 1 && y <= 14) return x == 7 ? new Color32(200, 206, 216, 255) : Shade(body, 0.8f);
                        if (y == 15 && (x == 7 || x == 8)) return new Color32(255, 230, 120, 255);
                        if (y <= 2 && x >= 4 && x <= 11) return y == 2 || x == 4 || x == 11 ? Outline : Shade(body, 0.8f);
                        return Clear;
                    }
                    bool post = x >= 7 && x <= 8 && y >= 1 && y <= 12, arm = y >= 11 && y <= 12 && x >= 3 && x <= 12;
                    bool insulator = y == 13 && (x == 3 || x == 12);
                    if (insulator) return new Color32(120, 200, 230, 255);
                    if (post || arm) return (x == 7 || y == 12) ? Shade(body, 1.2f) : body;
                    if ((x == 6 || x == 9) && y >= 1 && y <= 12) return Outline;
                    if ((y == 10 || y == 13) && x >= 3 && x <= 12) return Outline;
                    return Clear;
                }
                if (def.Kind == BuildingKind.Cable)   // a thin red cross: it joins every neighbouring cable
                {
                    bool v = x >= 7 && x <= 8, hz = y >= 7 && y <= 8;
                    if (!v && !hz) return (x >= 6 && x <= 9 && y >= 6 && y <= 9) ? Outline : Clear;
                    return Shade(body, (x == 7 || y == 8) ? 1.3f : 0.9f);
                }
                if (def.Kind == BuildingKind.Lamp)
                {
                    float lr = Mathf.Sqrt((x - 7.5f) * (x - 7.5f) + (y - 8.5f) * (y - 8.5f));
                    bool lit = frame > 0;
                    if (lr < 5f) return lit ? (lr < 2.5f ? new Color32(255, 250, 210, 255) : new Color32(255, 225, 120, 255)) : new Color32(120, 112, 80, 255);
                    if (lr < 6f) return Outline;
                    if (y <= 3 && x >= 5 && x <= 10) return y == 3 || x == 5 || x == 10 ? Outline : new Color32(90, 94, 104, 255);   // base
                    return lit && lr < 8f ? new Color32(255, 230, 140, 90) : Clear;                                                   // glow
                }
                if (def.Kind == BuildingKind.Pipe || def.Kind == BuildingKind.Pump)
                {
                    // a cross of pipe (it joins every neighbour); the pump adds a motor box and an arrow to the front
                    bool vert = x >= 5 && x <= 10, horiz = y >= 5 && y <= 10;
                    if (def.Kind == BuildingKind.Pump)
                    {
                        if (x >= 3 && x <= 12 && y >= 3 && y <= 12)
                        {
                            if (x == 3 || x == 12 || y == 3 || y == 12) return Outline;
                            if (Mathf.Abs(x - 7.5f) < (11 - y) * 0.6f && y >= 6 && y <= 10) return new Color32(230, 230, 235, 255);   // arrow
                            return Shade(body, 0.9f + (y - 3) * 0.03f);
                        }
                        if (!vert) return Clear;
                    }
                    else if (!vert && !horiz) return Clear;
                    if ((vert && (x == 5 || x == 10) && !horiz) || (horiz && (y == 5 || y == 10) && !vert)) return Outline;
                    if ((vert && (y == 0 || y == 15) && x >= 5 && x <= 10) || (horiz && (x == 0 || x == 15))) return Shade(body, 0.6f);   // flanges
                    return Shade(body, (vert ? x == 6 : y == 9) ? 1.35f : 0.95f);
                }
                if (x == 0 || y == 0 || x == w - 1 || y == h - 1) return Outline;
                if (x == 1 || y == h - 2) return Shade(body, 1.25f);
                if (y == 1 || x == w - 2) return Shade(body, 0.7f);
                float cx = x - w / 2f + 0.5f, cy = y - h / 2f + 0.5f;
                switch (def.Id)
                {
                    case "furnace":
                        if (Mathf.Abs(cx) < 5 && cy < 0 && cy > -10)
                        {
                            float flick = frame == 0 ? 0 : 1.5f;
                            return cy > -3 - flick + H(x, frame, 8) * 2 ? new Color32(255, 190, 60, 255) : new Color32(210, 70, 20, 255);
                        }
                        if (Mathf.Abs(cx) < 6 && cy >= 0 && cy < 2) return Outline;                                               // hood
                        if (cy > 7 && Mathf.Abs(cx - 4) < 2) return Shade(body, 0.6f);                                             // chimney
                        break;
                    case "drill":
                    {
                        float r = Mathf.Sqrt(cx * cx + cy * cy);
                        if (r < 7 && r > 5) return Shade(body, 0.6f);
                        if (r <= 5)   // the bit: a cross that turns while mining
                        {
                            float a = Mathf.Atan2(cy, cx) + frame * Mathf.PI / 8;
                            return Mathf.Abs(Mathf.Sin(a * 2)) < 0.35f || r < 1.5f ? new Color32(60, 60, 70, 255) : new Color32(150, 150, 160, 255);
                        }
                        if ((x / 4 + y / 4) % 2 == 0 && r > 9) return Shade(body, 0.85f);                                          // hazard plating
                        break;
                    }
                    case "computer":
                    case "computer2":
                        if (x > 3 && x < w - 4 && y > h - 14 && y < h - 4) return (x + y) % 7 == 0 ? new Color32(80, 255, 140, 255) : new Color32(14, 30, 26, 255);
                        if (y < 8 && y > 3 && x > 3 && x < w - 4 && x % 2 == 0) return Shade(body, 0.6f);
                        break;
                    case "chest":
                        if (y == h / 2) return Outline;
                        if (x == w / 2 && (y == h / 2 + 1 || y == h / 2 - 1)) return new Color32(230, 200, 80, 255);
                        if (x == 3 || x == w - 4) return Shade(body, 0.75f);                                                       // planks
                        break;
                    case "generator":
                    {
                        float gr = Mathf.Sqrt(cx * cx + (cy - 2) * (cy - 2));
                        if (gr < 6) return ((int)((Mathf.Atan2(cy - 2, cx) + frame * Mathf.PI / 6) * 3 + 30)) % 2 == 0 ? Shade(body, 0.55f) : Shade(body, 0.8f);   // turning fan
                        if (gr < 7) return Outline;
                        if (cy < -8 && Mathf.Abs(cx) < 6) return x % 3 == 0 ? (frame % 2 == 0 ? new Color32(255, 150, 40, 255) : new Color32(255, 200, 80, 255)) : new Color32(60, 40, 30, 255);
                        break;
                    }
                    case "solar":
                        if (x % 8 == 1 || y % 8 == 1) return Shade(body, 1.6f);
                        return Shade(body, 0.9f + 0.25f * (y / (float)h) + (x - y == 4 ? 0.3f : 0f));                               // reflection streak
                    case "battery":
                        if (y >= h - 4 && (x == 5 || x == 6 || x == 9 || x == 10)) return new Color32(200, 200, 210, 255);         // terminals
                        if (x >= 4 && x <= 11 && y >= 3 && y <= 10) return x == 4 || x == 11 || y == 3 || y == 10 ? Outline : (y <= 7 ? new Color32(90, 230, 120, 255) : Shade(body, 0.6f));
                        break;
                    case "turret":
                    {
                        float tr = Mathf.Sqrt(cx * cx + cy * cy);
                        if (x >= 7 && x <= 8 && y >= 8) return new Color32(60, 64, 72, 255);
                        if (tr < 5) return tr < 3 ? new Color32(255, 120, 60, 255) : Shade(body, 0.7f);
                        break;
                    }
                    case "lab":
                        if (Mathf.Abs(cx) < 3 && cy > -6 && cy < 5)
                        {
                            bool bubble = (x + y * 3 + frame * 5) % 9 == 0 && cy < 2;
                            return bubble ? new Color32(230, 255, 240, 255) : cy > 0 ? new Color32(120, 200, 255, 255) : new Color32(70, 230, 160, 255);
                        }
                        if (Mathf.Abs(cx) < 6 && cy <= -6 && cy > -9) return new Color32(70, 230, 160, 255);
                        break;
                    case "greenhouse":
                    case "hydroponics":
                        if (x % 8 == 0 || y % 8 == 0) return Shade(body, 0.75f);
                        if ((x / 8 + y / 8) % 2 == 0 && H(x, y, 21) > 0.55f) return new Color32(70, 170, 80, 255);
                        if (def.Id == "hydroponics" && y % 8 == 2) return new Color32(255, 160, 230, 255);                         // grow lamps
                        return Shade(body, 1.05f + H(x, y, 22) * 0.1f);
                    case "assembler":
                    {
                        float ar = Mathf.Sqrt(cx * cx + cy * cy);
                        if (ar < 8 && ar > 5 && ((int)((Mathf.Atan2(cy, cx) + frame * Mathf.PI / 8) * 4 + 40)) % 2 == 0) return new Color32(200, 205, 215, 255);   // turning gear
                        if (ar <= 3) return Shade(body, 0.5f);
                        break;
                    }
                    case "wall":
                    case "reinforced_wall":
                        if (y % 5 == 0 || (x + (y / 5) * 4) % 8 == 0) return Shade(body, 0.75f);
                        if (def.Id == "reinforced_wall" && (x == 3 || x == 12) && (y == 3 || y == 12)) return new Color32(190, 196, 206, 255);   // rivets
                        break;
                    case "storage_hub":
                        if (x % 10 == 4 || y % 10 == 4) return Shade(body, 0.7f);                                                  // crate grid
                        if (Mathf.Abs(cx) < 3 && Mathf.Abs(cy) < 3) return Mathf.Abs(cx) < 1.5f || Mathf.Abs(cy) < 1.5f ? new Color32(240, 210, 90, 255) : Shade(body, 0.6f);   // hub mark
                        break;
                    case "server":
                        if (x >= 3 && x <= w - 4 && y >= 3 && y <= h - 4)
                        {
                            if (y % 4 == 0) return Outline;                                                                         // rack slots
                            if (x == w - 6 && y % 4 == 2) return (y / 4 + frame) % 2 == 0 ? new Color32(80, 255, 140, 255) : new Color32(40, 90, 60, 255);   // blinking LEDs
                            return Shade(body, 0.75f);
                        }
                        break;
                    case "scrubber":
                    {
                        float sr = Mathf.Sqrt(cx * cx + cy * cy);
                        if (sr < 5.5f) return ((x + y + frame) % 3 == 0) ? Shade(body, 0.55f) : Shade(body, 1.2f);                  // grille
                        if (sr < 6.3f) return Outline;
                        break;
                    }
                    case "heater":
                        if (x >= 3 && x <= 12 && y >= 4 && y <= 11 && y % 3 == 1) return frame == 0 ? new Color32(255, 110, 50, 255) : new Color32(255, 170, 80, 255);   // glowing coils
                        break;
                    case "shock_trap":
                        if (Mathf.Abs(y - 8 - ((x % 6) < 3 ? (x % 3) : 2 - (x % 3)) * 2 + 2) < 0.8f && x > 2 && x < 13) return new Color32(150, 220, 255, 255);   // zig-zag
                        break;
                    case "tank":
                    {
                        float tr = Mathf.Sqrt(cx * cx + cy * cy);
                        if (tr < 13 && tr > 12) return Outline;
                        if (tr < 12) return Shade(body, 1.15f - tr * 0.03f + (Mathf.Abs(cx + cy) < 1 ? 0.25f : 0f));   // round tank with a shine
                        break;
                    }
                    case "offshore_pump":
                        if (y < 6) return (x + frame) % 4 == 0 ? new Color32(140, 200, 255, 255) : new Color32(60, 120, 200, 255);   // intake in the water
                        if (Mathf.Abs(cx) < 3 && y >= 6) return Shade(body, 1.3f);
                        break;
                    case "pumpjack":
                    {
                        float swing = Mathf.Sin(frame * Mathf.PI / 4f) * 3f;   // the walking beam rocks
                        float beamY = 20 + (cx) * swing / 12f;
                        if (Mathf.Abs(cy + 16 - beamY) < 1.5f && Mathf.Abs(cx) < 12) return new Color32(60, 50, 70, 255);
                        if (Mathf.Abs(cx + 12) < 2.5f && Mathf.Abs(cy + 16 - beamY) < 4) return new Color32(220, 190, 60, 255);   // horse head
                        if (Mathf.Abs(cx) < 1.5f && y > 6 && y < 20) return Outline;                                                 // samson post
                        if (y < 6 && Mathf.Abs(cx + 10) < 3) return new Color32(30, 25, 30, 255);                                   // well head
                        break;
                    }
                    case "boiler":
                        if (cy < -6 && Mathf.Abs(cx) < 9) return x % 3 == 0 ? (frame % 2 == 0 ? new Color32(255, 150, 40, 255) : new Color32(255, 210, 90, 255)) : new Color32(50, 30, 25, 255);   // firebox
                        if (cy > 4 && Mathf.Abs(cx) < 10) return Shade(body, 1.2f - (y % 4 == 0 ? 0.3f : 0f));                     // drum
                        break;
                    case "steam_engine":
                    {
                        float er = Mathf.Sqrt((cx + 6) * (cx + 6) + cy * cy);
                        if (er < 9 && er > 7.5f) return Outline;
                        if (er < 7.5f) return ((int)((Mathf.Atan2(cy, cx + 6) + frame * Mathf.PI / 8) * 4 + 40)) % 2 == 0 ? Shade(body, 0.55f) : Shade(body, 0.85f);   // flywheel
                        if (cx > 4 && Mathf.Abs(cy) < 5) return x % 2 == 0 ? Shade(body, 1.3f) : Shade(body, 1.1f);              // cylinder
                        break;
                    }
                    case "filler":
                        if (cy < -2 && ((x / 6) % 2 == 1) && x > 3 && x < w - 4) return y == 6 || y == 7 ? new Color32(220, 60, 50, 255) : new Color32(40, 40, 45, 255);   // canisters
                        if (cy > 4 && Mathf.Abs(cx) < 3) return Shade(body, 0.6f);                                                  // nozzle
                        break;
                    case "roboport":
                    {
                        float rr = Mathf.Sqrt(cx * cx + cy * cy);
                        if (rr < 9 && rr > 7.5f) return new Color32(240, 200, 60, 255);                                       // landing ring
                        if (rr < 3 && (Mathf.Abs(cx) < 1 || Mathf.Abs(cy) < 1)) return new Color32(110, 220, 255, 255);        // beacon
                        if ((x < 5 || x > w - 6) && (y < 5 || y > h - 6)) return Shade(body, 0.6f);                             // corner antennas
                        break;
                    }
                    case "provider_chest":
                    case "requester_chest":
                        if (y == h / 2 || y == h / 2 - 1) return Outline;                                                        // lid
                        if (x >= 6 && x <= 9 && y >= h / 2 - 3 && y <= h / 2 + 2) return new Color32(230, 230, 235, 255);        // latch
                        if (def.Id == "provider_chest" ? y > h / 2 && Mathf.Abs(x - 7.5f) < (h - 2 - y) * 0.7f : y < h / 2 - 1 && Mathf.Abs(x - 7.5f) < (y - 1) * 0.7f)
                            return Shade(body, 1.4f);                                                                            // arrow out / in
                        break;
                    case "constant_combinator":
                    case "arithmetic_combinator":
                    case "decider_combinator":
                    {
                        if (y >= 12 && Mathf.Abs(x - 7.5f) < 15 - y) return new Color32(110, 220, 140, 255);                  // output arrow (front)
                        if (x >= 3 && x <= 12 && y >= 3 && y <= 10)
                        {
                            if (x == 3 || x == 12 || y == 3 || y == 10) return Outline;
                            bool mark = def.Id == "constant_combinator" ? ((x == 6 || x == 9) && y >= 4 && y <= 9) || ((y == 5 || y == 8) && x >= 4 && x <= 11)   // #
                                : def.Id == "arithmetic_combinator" ? (x >= 7 && x <= 8 && y >= 4 && y <= 9) || (y >= 6 && y <= 7 && x >= 5 && x <= 10)          // +
                                : (x >= 7 && x <= 8 && (y == 4 || (y >= 6 && y <= 7))) || (y == 9 && x >= 6 && x <= 9) || (x == 9 && y == 8);                  // ?
                            return mark ? new Color32(255, 200, 80, 255) : new Color32(24, 32, 30, 255);                       // display
                        }
                        break;
                    }
                    case "launchpad":
                    {
                        if (x < 4 || y < 4 || x > w - 5 || y > h - 5) return ((x + y) / 3) % 2 == 0 ? new Color32(240, 200, 40, 255) : new Color32(40, 40, 44, 255);   // hazard border
                        float pr = Mathf.Sqrt(cx * cx + cy * cy);
                        if (pr < 12 && pr > 10.5f) return new Color32(230, 230, 235, 255);
                        if (Mathf.Abs(cx) < 6 && Mathf.Abs(cx) > 3.8f && Mathf.Abs(cy) < 6) return new Color32(230, 230, 235, 255);   // H
                        if (Mathf.Abs(cy) < 1 && Mathf.Abs(cx) < 4) return new Color32(230, 230, 235, 255);
                        return Shade(body, 0.85f + H(x, y, 31) * 0.08f);
                    }
                }
                return Shade(body, 0.95f + H(x, y, 9) * 0.1f);
            }, new Vector2(0.5f, 0.5f));
        }

        /// <summary>A soft dark patch drawn under buildings (offset down-right) so they sit on the ground.</summary>
        public static Sprite Shadow() => Make("shadow", 8, 8, (x, y) =>
        {
            float dx = Mathf.Max(0, Mathf.Abs(x - 3.5f) - 2.5f), dy = Mathf.Max(0, Mathf.Abs(y - 3.5f) - 2.5f);
            return new Color32(0, 0, 0, (byte)(100 * Mathf.Clamp01(1 - (dx + dy))));
        }, new Vector2(0.5f, 0.5f));

        // ---------------- player & UI ----------------
        static readonly string[] PlayerArt =
        {
            "................",
            "................",
            "......####......",
            ".....#WWWW#.....",
            "....#WCCCCW#....",
            "....#WCcCCW#....",
            "....#WCCCCW#....",
            ".....#WWWW#.....",
            "...##WWWWWW##...",
            "..#WW#WOOW#WW#..",
            "..#WW#WWWW#WW#..",
            "..###WWWWWW###..",
            ".....#WW#WW#....",
            ".....#WW#WW#....",
            ".....##..##.....",
            "................",
        };
        // walking: the legs (last three art rows) alternate, the body bobs one pixel
        static readonly string[][] PlayerLegs =
        {
            new[] { ".....#WW#WW#....", ".....#WW#WW#....", ".....##..##....." },
            new[] { ".....#WW#WW#....", "....#WW#..#WW#..", "....##.....##..." },
            new[] { ".....#WW#WW#....", ".....#WW#WW#....", ".....##..##....." },
            new[] { ".....#WW#WW#....", "..#WW#..#WW#....", "..##.....##....." },
        };

        public const int PlayerFrames = 4;

        public static Sprite Player() => Player(-1);

        /// <summary>Frame -1 = standing; 0..3 = walk cycle.</summary>
        public static Sprite Player(int frame) => Make("player_" + frame, PPU, PPU, (x, y) =>
        {
            int row = PPU - 1 - y;
            int bob = frame == 1 || frame == 3 ? 1 : 0;
            string line;
            if (frame >= 0 && row >= 12 && row <= 14) line = PlayerLegs[frame][row - 12];
            else
            {
                int src = row + bob;   // body drawn one pixel lower on the passing frames
                if (frame >= 0 && src >= 12) return Clear;
                line = src < PPU ? PlayerArt[src] : "................";
            }
            switch (line[x])
            {
                case '#': return Outline;
                case 'W': return new Color32(226, 226, 232, 255);
                case 'C': return new Color32(56, 196, 228, 255);
                case 'c': return new Color32(190, 245, 255, 255);
                case 'O': return new Color32(240, 140, 40, 255);
                default: return row == 15 && x >= 5 && x <= 10 ? new Color32(0, 0, 0, 60) : Clear;   // shadow at the feet
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>Small item icon (6x6 px) for items riding on belts.</summary>
        /// <summary>A small logistic robot (drawn flying, with its cargo).</summary>
        public static Sprite Robot() => Make("logistic_robot_flying", 10, 8, (x, y) =>
        {
            bool body = x >= 2 && x <= 7 && y >= 2 && y <= 5;
            if (body) return x == 2 || x == 7 || y == 2 || y == 5 ? Outline : (y == 4 && (x == 4 || x == 5) ? new Color32(110, 220, 255, 255) : new Color32(200, 205, 215, 255));
            if ((y == 6 || y == 7) && (x <= 2 || x >= 7)) return new Color32(90, 96, 108, 255);   // rotors
            return Clear;
        }, new Vector2(0.5f, 0.5f));

        public static Sprite ItemIcon(string itemId)
        {
            uint rgb;
            switch (itemId)
            {
                case ItemDatabase.Stone: rgb = 0x9a9a9e; break;
                case ItemDatabase.IronOre: rgb = 0xb07a62; break;
                case ItemDatabase.CopperOre: rgb = 0xd2783a; break;
                case ItemDatabase.Coal: rgb = 0x2c2c32; break;
                case ItemDatabase.Silicon: rgb = 0xb8c4d8; break;
                case ItemDatabase.IronPlate: rgb = 0xc8ccd4; break;
                case ItemDatabase.CopperPlate: rgb = 0xe89a5a; break;
                case ItemDatabase.StoneBrick: rgb = 0xb5654a; break;
                case ItemDatabase.Wood: rgb = 0x8a5a30; break;
                case ItemDatabase.AlienCrystal: rgb = 0xbe52e6; break;
                case ItemDatabase.UraniumOre: rgb = 0x58e040; break;
                default: rgb = 0xdddddd; break;
            }
            var c = Hex(rgb);
            bool square = itemId.EndsWith("_plate") || itemId == ItemDatabase.StoneBrick;
            return Make("item_" + itemId, 6, 6, (x, y) =>
            {
                bool edge = x == 0 || y == 0 || x == 5 || y == 5;
                bool corner = (x == 0 || x == 5) && (y == 0 || y == 5);
                if (corner && !square) return Clear; // ores are rounded lumps, plates are squares
                if (edge) return Outline;
                return Shade(c, y > 2 ? 1.15f : 0.9f);
            }, new Vector2(0.5f, 0.5f));
        }

        static readonly string[] CrawlerArt =
        {
            "................",
            "................",
            "..#..........#..",
            "...#..####..#...",
            "....#######.....",
            "...##PPPPPP##...",
            "..#PPPpPPpPPP#..",
            ".#PPPPPPPPPPPP#.",
            ".#PPYPPPPPPYPP#.",
            ".#PPPPPPPPPPPP#.",
            "..#PPPPPPPPPP#..",
            "...##########...",
            "..#..#....#..#..",
            ".#..#......#..#.",
            "................",
            "................",
        };
        static readonly string[] DroneArt =
        {
            "................",
            "..##........##..",
            ".#GG#......#GG#.",
            "..##..####..##..",
            "....##GGGG##....",
            "...#GGGGGGGG#...",
            "...#GGGRRGGG#...",
            "...#GGRRRRGG#...",
            "...#GGGRRGGG#...",
            "...#GGGGGGGG#...",
            "....##GGGG##....",
            "..##..####..##..",
            ".#GG#......#GG#.",
            "..##........##..",
            "................",
            "................",
        };

        public static Sprite Creature(CreatureType t) => Creature(t, 0);

        /// <summary>Two frames: the crawler's legs move, the drone's rotors spin (mirror image).</summary>
        public static Sprite Creature(CreatureType t, int frame) => Make("creature_" + t + "_" + frame, PPU, PPU, (x, y) =>
        {
            var art = t == CreatureType.Drone ? DroneArt : CrawlerArt;
            int row = PPU - 1 - y;
            bool legs = t == CreatureType.Crawler && row >= 12, rotor = t == CreatureType.Drone && (row <= 3 || row >= 11);
            int col = frame == 1 && legs ? Mathf.Clamp(x + (x < 8 ? 1 : -1), 0, PPU - 1) : x;   // crawler: legs step in
            char ch = art[row][col];
            switch (ch)
            {
                case '#': return Outline;
                case 'P': return new Color32(150, 70, 200, 255);
                case 'p': return new Color32(200, 120, 240, 255);
                case 'Y': return new Color32(255, 230, 90, 255);
                case 'G': return frame == 1 && rotor ? new Color32(190, 198, 210, 170) : new Color32(120, 128, 140, 255);   // drone: rotors blur
                case 'R': return new Color32(255, 60, 60, 255);
                default: return Clear;
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>Survey outpost props: "terminal" (screen glows) or "cache" (lid open when looted).</summary>
        public static Sprite Facility(string kind, bool open) => Make("facility_" + kind + open, PPU, PPU, (x, y) =>
        {
            bool edge = x == 1 || x == 14 || y == 1 || y == (kind == "terminal" ? 14 : 11);
            if (kind == "relay")
            {
                if (x == 7 || x == 8) return y < 15 ? new Color32(150, 156, 168, 255) : new Color32(255, 80, 60, 255);   // mast with a red light
                if (y == 15 - (x > 8 ? x - 8 : 7 - x) * 2 && y > 6) return new Color32(110, 116, 128, 255);              // guy wires
                return y < 3 && x > 3 && x < 12 ? Shade(new Color32(90, 96, 108, 255), 0.9f) : Clear;
            }
            if (kind == "wreck")
            {
                float w = Mathf.Abs(x - 7.5f) + Mathf.Abs(y - 6f) * 1.4f;
                if (w > 8f || y > 12) return Clear;
                return H(x, y, 17) > 0.85f ? new Color32(255, 140, 40, 255) : Shade(new Color32(130, 136, 146, 255), 0.6f + 0.5f * H(x, y, 9));   // scorched hull
            }
            if (kind == "terminal")
            {
                if (x < 1 || x > 14 || y < 1 || y > 14) return Clear;
                if (edge) return Outline;
                if (x >= 3 && x <= 12 && y >= 6 && y <= 12) return (y % 2 == 0) ? new Color32(60, 230, 110, 255) : new Color32(30, 150, 70, 255);   // screen
                if (y == 3 && x % 2 == 1 && x > 2 && x < 13) return new Color32(90, 96, 110, 255);                                           // keys
                return Shade(new Color32(120, 126, 138, 255), 0.85f + 0.2f * (y / 16f));
            }
            if (x < 1 || x > 14 || y < 1 || y > (open ? 9 : 11)) return Clear;
            if (edge || y == (open ? 9 : 11)) return Outline;
            if (open && y >= 7) return new Color32(30, 26, 22, 255);                                                                    // empty inside
            if (x == 7 || x == 8) return new Color32(255, 200, 60, 255);                                                                // survey stripe
            return Shade(new Color32(150, 110, 60, 255), 0.85f + 0.25f * H(x, y, 5));
        }, Vector2.zero);

        /// <summary>The rebuilt ship standing on its pad (2 cells tall), pivot at the centre of its base.</summary>
        public static Sprite Ship() => Make("ship", PPU, PPU * 2, (x, y) =>
        {
            float half = y > 22 ? (31 - y) * 0.6f : y < 6 ? 6 - y * 0.2f : 4.5f;   // nose cone, body, fins
            float d = Mathf.Abs(x - 7.5f);
            if (d > half + 0.5f) return Clear;
            if (d > half - 0.5f) return Outline;
            if (y >= 15 && y <= 18 && d < 2) return new Color32(90, 200, 255, 255);                     // window
            if (y < 6) return new Color32(200, 70, 60, 255);                                              // fins
            return Shade(new Color32(225, 228, 235, 255), 0.85f + 0.2f * (1 - d / 5f));
        }, new Vector2(0.5f, 0.25f));

        /// <summary>A cave mouth on the surface, or the ladder up underground.</summary>
        public static Sprite CavePortal(bool up) => Make(up ? "cave_ladder" : "cave_mouth", PPU, PPU, (x, y) =>
        {
            float dx = x - 7.5f, dy = y - 7.5f;
            if (up) return (x == 4 || x == 11) && y > 1 && y < 15 ? new Color32(150, 110, 60, 255) : y % 3 == 0 && x > 4 && x < 11 && y > 1 && y < 15 ? new Color32(170, 128, 70, 255) : Clear;
            float d = dx * dx / 49f + dy * dy / 30f;
            return d < 0.55f ? new Color32(8, 6, 8, 255) : d < 1f ? Shade(new Color32(96, 88, 84, 255), 0.7f + 0.4f * H(x, y, 3)) : Clear;
        }, Vector2.zero);

        public static Sprite CellOutline() => Make("cell_outline", PPU, PPU, (x, y) =>
            (x == 0 || y == 0 || x == PPU - 1 || y == PPU - 1) ? new Color32(255, 255, 255, 220) : Clear, Vector2.zero);

        public static Sprite White() => Make("white", 4, 4, (x, y) => new Color32(255, 255, 255, 255), Vector2.zero);
    }
}
