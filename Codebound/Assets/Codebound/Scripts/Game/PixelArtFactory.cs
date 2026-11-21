using System.Collections.Generic;
using Codebound.Core;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Procedural placeholder pixel art (16 px per cell). shortcut: generated in code until the
    /// Etapa 8 art pass replaces it with hand-made sprite sheets; keep sprite sizes/pivots identical.
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
        public const int TileVariants = 4;

        public static Sprite Tile(TileType t, int variant)
        {
            Color32 baseC;
            switch (t)
            {
                case TileType.Grass: baseC = new Color32(62, 124, 66, 255); break;
                case TileType.Sand: baseC = new Color32(212, 188, 128, 255); break;
                case TileType.Rock: baseC = new Color32(112, 110, 116, 255); break;
                case TileType.Water: baseC = new Color32(38, 88, 168, 255); break;
                case TileType.Ash: baseC = new Color32(72, 64, 62, 255); break;
                case TileType.Lava: baseC = new Color32(214, 76, 22, 255); break;
                case TileType.RuinFloor: baseC = new Color32(92, 98, 106, 255); break;
                case TileType.ToxicSoil: baseC = new Color32(92, 108, 52, 255); break;
                default: baseC = new Color32(86, 168, 58, 255); break; // toxic water
            }
            int seed = (int)t * 97 + variant * 13;
            return Make($"tile_{t}_{variant}", PPU, PPU, (x, y) =>
            {
                float n = H(x, y, seed);
                var c = Shade(baseC, 0.9f + n * 0.18f);
                switch (t)
                {
                    case TileType.Grass:
                        if (n > 0.93f) c = Shade(baseC, 1.35f);
                        else if (n < 0.06f) c = Shade(baseC, 0.7f);
                        break;
                    case TileType.Water: case TileType.ToxicWater:
                        if ((y + variant * 3) % 6 == 0 && ((x + y) % 5) < 3) c = Shade(baseC, 1.3f);
                        break;
                    case TileType.Lava:
                        if (n > 0.8f) c = new Color32(255, 200, 60, 255); else if (n < 0.2f) c = new Color32(120, 30, 10, 255);
                        break;
                    case TileType.RuinFloor:
                        if (x == 0 || y == 0 || x == 8 || y == 8) c = Shade(baseC, 0.72f);
                        if (n > 0.96f) c = new Color32(60, 200, 210, 255); // dead circuitry glints
                        break;
                    case TileType.Rock: case TileType.Ash:
                        if (n < 0.08f) c = Shade(baseC, 0.6f);
                        break;
                }
                return c;
            }, Vector2.zero);
        }

        // ---------------- resources ----------------
        public static Sprite Resource(ResourceType r)
        {
            if (r == ResourceType.Tree) return Tree();
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
            int seed = (int)r * 31 + 5;
            // a few lumps at hashed positions
            var lumps = new List<Vector3>();
            for (int i = 0; i < 4; i++) lumps.Add(new Vector3(3 + H(i, 1, seed) * 10, 3 + H(i, 2, seed) * 10, 2 + H(i, 3, seed) * 2));
            bool crystal = r == ResourceType.AlienCrystal || r == ResourceType.Silicon || r == ResourceType.Uranium;
            return Make("res_" + r, PPU, PPU, (x, y) =>
            {
                foreach (var l in lumps)
                {
                    float dx = x - l.x, dy = y - l.y;
                    float d = crystal ? Mathf.Abs(dx) * 2f + Mathf.Abs(dy) * 0.8f : Mathf.Sqrt(dx * dx + dy * dy);
                    float rad = crystal ? l.z * 2f : l.z;
                    if (d < rad - 0.8f) return Shade(c, dy > 0 ? 1.2f : 0.95f);
                    if (d < rad + 0.2f) return Outline;
                }
                return Clear;
            }, Vector2.zero);
        }

        static Sprite Tree() => Make("res_tree", PPU, PPU, (x, y) =>
        {
            float dx = x - 7.5f, dy = y - 9.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            if (d < 5.2f) return H(x, y, 3) > 0.8f ? new Color32(70, 150, 70, 255) : (dy > 1 ? new Color32(44, 118, 52, 255) : new Color32(30, 90, 40, 255));
            if (d < 6.2f) return Outline;
            if (x >= 6 && x <= 9 && y >= 1 && y < 5) return x == 6 || x == 9 ? Outline : new Color32(110, 72, 40, 255);
            return Clear;
        }, Vector2.zero);

        // ---------------- buildings ----------------
        public static Sprite Building(BuildingDef def)
        {
            int w = def.Width * PPU, h = def.Height * PPU;
            var body = Hex(def.Color);
            return Make("bld_" + def.Id, w, h, (x, y) =>
            {
                if (def.Kind == BuildingKind.Belt || def.Kind == BuildingKind.Conveyor)
                {
                    if (x == 0 || x == w - 1) return Outline;
                    if (x == 1 || x == w - 2) return def.Kind == BuildingKind.Conveyor ? new Color32(70, 140, 220, 255) : new Color32(200, 170, 60, 255);
                    float v = y - Mathf.Abs(x - 7.5f); // chevrons pointing to the front (up at R0)
                    if ((v > 2 && v <= 4) || (v > 10 && v <= 12)) return Shade(body, 1.6f);
                    if (def.Kind == BuildingKind.Conveyor && x >= 6 && x <= 9 && y <= 1) return new Color32(80, 255, 140, 255);
                    return Shade(body, 0.9f + H(x, y, 4) * 0.1f);
                }
                if (def.Kind == BuildingKind.Inserter)
                {
                    bool plate = x >= 3 && x <= 12 && y >= 1 && y <= 6;
                    bool arm = x >= 7 && x <= 8 && y >= 5 && y <= 12;
                    bool claw = y >= 12 && y <= 14 && (x == 5 || x == 6 || x == 9 || x == 10 || (y == 12 && x >= 5 && x <= 10));
                    if (claw) return new Color32(60, 60, 70, 255);
                    if (arm) return x == 7 ? Shade(body, 1.25f) : body;
                    if (plate) return (x == 3 || x == 12 || y == 1 || y == 6) ? Outline : new Color32(80, 84, 92, 255);
                    return Clear;
                }
                if (def.Id == "pole")
                {
                    bool post = x >= 7 && x <= 8 && y >= 1 && y <= 12, arm = y >= 11 && y <= 12 && x >= 3 && x <= 12;
                    bool insulator = y == 13 && (x == 3 || x == 12);
                    if (insulator) return new Color32(120, 200, 230, 255);
                    if (post || arm) return (x == 7 || y == 12) ? Shade(body, 1.2f) : body;
                    if ((x == 6 || x == 9) && y >= 1 && y <= 12) return Outline;
                    if ((y == 10 || y == 13) && x >= 3 && x <= 12) return Outline;
                    return Clear;
                }
                if (x == 0 || y == 0 || x == w - 1 || y == h - 1) return Outline;
                if (x == 1 || y == h - 2) return Shade(body, 1.25f);
                if (y == 1 || x == w - 2) return Shade(body, 0.7f);
                float cx = x - w / 2f + 0.5f, cy = y - h / 2f + 0.5f;
                switch (def.Id)
                {
                    case "furnace":
                        if (Mathf.Abs(cx) < 5 && cy < 0 && cy > -10) return cy > -3 ? new Color32(255, 160, 40, 255) : new Color32(200, 60, 20, 255);
                        break;
                    case "drill":
                        float r = Mathf.Sqrt(cx * cx + cy * cy);
                        if (r < 7 && r > 5) return Shade(body, 0.6f);
                        if (r < 3) return new Color32(60, 60, 70, 255);
                        if ((x / 4 + y / 4) % 2 == 0 && r > 9) return Shade(body, 0.85f);
                        break;
                    case "computer":
                    case "computer2":
                        if (x > 3 && x < w - 4 && y > h - 14 && y < h - 4) return (x + y) % 7 == 0 ? new Color32(80, 255, 140, 255) : new Color32(14, 30, 26, 255);
                        if (y < 8 && y > 3 && x > 3 && x < w - 4 && x % 2 == 0) return Shade(body, 0.6f);
                        break;
                    case "chest":
                        if (y == h / 2) return Outline;
                        if (x == w / 2 && (y == h / 2 + 1 || y == h / 2 - 1)) return new Color32(230, 200, 80, 255);
                        break;
                    case "generator":
                        float gr = Mathf.Sqrt(cx * cx + (cy - 2) * (cy - 2));
                        if (gr < 6) return (int)(Mathf.Atan2(cy - 2, cx) * 3) % 2 == 0 ? Shade(body, 0.55f) : Shade(body, 0.8f);
                        if (gr < 7) return Outline;
                        if (cy < -8 && Mathf.Abs(cx) < 6) return x % 3 == 0 ? new Color32(255, 150, 40, 255) : new Color32(60, 40, 30, 255);
                        break;
                    case "solar":
                        if (x % 8 == 1 || y % 8 == 1) return Shade(body, 1.6f);                       // cell grid
                        return Shade(body, 0.9f + 0.25f * (y / (float)h));                                // sky reflection
                    case "battery":
                        if (y >= h - 4 && x >= 5 && x <= 10) return new Color32(200, 200, 210, 255);    // terminal
                        if (x >= 4 && x <= 11 && y >= 3 && y <= 10) return y <= 3 + (int)(H(1, 2, 3) * 7) ? new Color32(90, 230, 120, 255) : Shade(body, 0.6f);
                        break;
                    case "turret":
                        float tr = Mathf.Sqrt(cx * cx + cy * cy);
                        if (x >= 7 && x <= 8 && y >= 8) return new Color32(60, 64, 72, 255);             // barrel
                        if (tr < 5) return tr < 3 ? new Color32(255, 120, 60, 255) : Shade(body, 0.7f); // dome + sensor
                        break;
                    case "lab":
                        if (Mathf.Abs(cx) < 3 && cy > -6 && cy < 5) return cy > 0 ? new Color32(120, 200, 255, 255) : new Color32(70, 230, 160, 255); // flask
                        if (Mathf.Abs(cx) < 6 && cy <= -6 && cy > -9) return new Color32(70, 230, 160, 255);
                        break;
                    case "greenhouse":
                        if (x % 8 == 0 || y % 8 == 0) return Shade(body, 0.75f);                       // glass frame
                        if ((x / 8 + y / 8) % 2 == 0 && H(x, y, 21) > 0.55f) return new Color32(70, 170, 80, 255); // plants
                        return Shade(body, 1.05f + H(x, y, 22) * 0.1f);
                    case "assembler":
                        float ar = Mathf.Sqrt(cx * cx + cy * cy);
                        if (ar < 8 && ar > 5 && ((int)(Mathf.Atan2(cy, cx) * 4)) % 2 == 0) return new Color32(200, 205, 215, 255); // gear
                        if (ar <= 3) return Shade(body, 0.5f);
                        break;
                    case "wall":
                        if (y % 5 == 0 || (x + (y / 5) * 4) % 8 == 0) return Shade(body, 0.75f);
                        break;
                }
                return Shade(body, 0.95f + H(x, y, 9) * 0.1f);
            }, new Vector2(0.5f, 0.5f));
        }

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

        public static Sprite Player() => Make("player", PPU, PPU, (x, y) =>
        {
            char ch = PlayerArt[PPU - 1 - y][x];
            switch (ch)
            {
                case '#': return Outline;
                case 'W': return new Color32(226, 226, 232, 255);
                case 'C': return new Color32(56, 196, 228, 255);
                case 'c': return new Color32(190, 245, 255, 255);
                case 'O': return new Color32(240, 140, 40, 255);
                default: return Clear;
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>Small item icon (6x6 px) for items riding on belts.</summary>
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

        public static Sprite Creature(CreatureType t) => Make("creature_" + t, PPU, PPU, (x, y) =>
        {
            char ch = (t == CreatureType.Drone ? DroneArt : CrawlerArt)[PPU - 1 - y][x];
            switch (ch)
            {
                case '#': return Outline;
                case 'P': return new Color32(150, 70, 200, 255);
                case 'p': return new Color32(200, 120, 240, 255);
                case 'Y': return new Color32(255, 230, 90, 255);
                case 'G': return new Color32(120, 128, 140, 255);
                case 'R': return new Color32(255, 60, 60, 255);
                default: return Clear;
            }
        }, new Vector2(0.5f, 0.5f));

        /// <summary>Survey outpost props: "terminal" (screen glows) or "cache" (lid open when looted).</summary>
        public static Sprite Facility(string kind, bool open) => Make("facility_" + kind + open, PPU, PPU, (x, y) =>
        {
            bool edge = x == 1 || x == 14 || y == 1 || y == (kind == "terminal" ? 14 : 11);
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

        public static Sprite CellOutline() => Make("cell_outline", PPU, PPU, (x, y) =>
            (x == 0 || y == 0 || x == PPU - 1 || y == PPU - 1) ? new Color32(255, 255, 255, 220) : Clear, Vector2.zero);

        public static Sprite White() => Make("white", 4, 4, (x, y) => new Color32(255, 255, 255, 255), Vector2.zero);
    }
}
