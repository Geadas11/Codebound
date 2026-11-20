using System.Collections.Generic;
using Codebound.Building;
using Codebound.Core;
using Codebound.Items;
using Codebound.World;
using UnityEngine;

namespace Codebound.Game
{
    /// <summary>
    /// Procedural pixel-art sprites (16 px per world unit, point filtered). Placeholder art that keeps the
    /// project runnable with zero imported assets; each generator is deterministic so art is stable.
    /// Replace with hand-drawn sprite sheets in Etapa 8 by swapping this provider.
    /// </summary>
    public static class PixelArt
    {
        public const int PPU = 16;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        // ---------- palette ----------
        private static readonly Color32 Outline = new Color32(20, 18, 28, 255);
        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);

        // ---------- public sprites ----------

        public static Sprite White => Get("white", () => Build(PPU, PPU, c => Fill(c, PPU, PPU, new Color32(255, 255, 255, 255))));

        /// <summary>1-px border square used for cursor/target highlight.</summary>
        public static Sprite Frame => Get("frame", () => Build(PPU, PPU, c =>
        {
            for (int i = 0; i < PPU; i++)
            {
                bool corner = i < 4 || i >= PPU - 4;
                if (!corner) continue;
                Set(c, PPU, i, 0, Color.white); Set(c, PPU, i, PPU - 1, Color.white);
                Set(c, PPU, 0, i, Color.white); Set(c, PPU, PPU - 1, i, Color.white);
            }
        }));

        public static Sprite Terrain(TerrainType t, int variant) => Get($"terrain.{t}.{variant}", () => Build(PPU, PPU, c =>
        {
            Color32 baseCol, alt;
            switch (t)
            {
                case TerrainType.Grass: baseCol = new Color32(62, 112, 64, 255); alt = new Color32(78, 134, 72, 255); break;
                case TerrainType.Dirt: baseCol = new Color32(110, 82, 58, 255); alt = new Color32(126, 96, 66, 255); break;
                case TerrainType.Sand: baseCol = new Color32(196, 172, 112, 255); alt = new Color32(214, 192, 132, 255); break;
                case TerrainType.Rock: baseCol = new Color32(76, 76, 86, 255); alt = new Color32(96, 96, 108, 255); break;
                case TerrainType.Water: baseCol = new Color32(38, 76, 128, 255); alt = new Color32(52, 98, 156, 255); break;
                default: baseCol = new Color32(58, 50, 48, 255); alt = new Color32(80, 64, 54, 255); break; // crash site
            }
            for (int y = 0; y < PPU; y++)
            for (int x = 0; x < PPU; x++)
            {
                double h = SeededRandom.Hash01(variant * 7919 + (int)t, x, y);
                var col = h < 0.18 ? alt : baseCol;
                if (h > 0.97) col = Shade(baseCol, -18);
                Set(c, PPU, x, y, col);
            }
            if (t == TerrainType.Grass)
                for (int i = 0; i < 3; i++)
                {
                    int x = (int)(SeededRandom.Hash01(variant, i, 1) * 14) + 1, y = (int)(SeededRandom.Hash01(variant, i, 2) * 13) + 1;
                    Set(c, PPU, x, y, new Color32(98, 156, 84, 255)); Set(c, PPU, x, y + 1, new Color32(98, 156, 84, 255));
                }
            if (t == TerrainType.Water)
                for (int i = 0; i < 2; i++)
                {
                    int x = (int)(SeededRandom.Hash01(variant, i, 3) * 11) + 1, y = (int)(SeededRandom.Hash01(variant, i, 4) * 14) + 1;
                    for (int k = 0; k < 4; k++) Set(c, PPU, x + k, y, new Color32(90, 140, 196, 255));
                }
            if (t == TerrainType.Rock)
            {
                for (int x = 0; x < PPU; x++) { Set(c, PPU, x, PPU - 1, Shade(baseCol, 30)); Set(c, PPU, x, 0, Shade(baseCol, -30)); }
            }
        }));

        /// <summary>Inventory/belt icon for an item: processed materials get their own look, raw ones reuse the deposit art.</summary>
        public static Sprite Icon(string itemId)
        {
            switch (itemId)
            {
                case ItemCatalog.IronPlate: return Plate(itemId, new Color32(176, 184, 196, 255));
                case ItemCatalog.CopperPlate: return Plate(itemId, new Color32(214, 128, 70, 255));
                case ItemCatalog.StoneBrick: return Plate(itemId, new Color32(150, 140, 128, 255));
                default: return Node(itemId);
            }
        }

        private static Sprite Plate(string id, Color32 col) => Get("icon." + id, () => Build(PPU, PPU, c =>
        {
            Rect(c, 2, 4, 12, 8, col);
            Rect(c, 2, 10, 12, 2, Shade(col, 30));
            Rect(c, 2, 4, 12, 1, Shade(col, -30));
            OutlinePass(c, PPU, PPU);
        }));

        public static Sprite Node(string itemId) => Get("node." + itemId, () => Build(PPU, PPU, c =>
        {
            if (itemId == ItemCatalog.Wood) { DrawTree(c); return; }
            if (itemId == ItemCatalog.Scrap) { DrawScrap(c); return; }
            Color32 rock = new Color32(104, 100, 110, 255), speck;
            switch (itemId)
            {
                case ItemCatalog.IronOre: speck = new Color32(196, 120, 90, 255); break;
                case ItemCatalog.CopperOre: speck = new Color32(222, 140, 60, 255); break;
                case ItemCatalog.Coal: speck = new Color32(28, 26, 30, 255); rock = new Color32(72, 70, 76, 255); break;
                case ItemCatalog.Silicon: speck = new Color32(170, 200, 220, 255); break;
                case ItemCatalog.Stone: speck = new Color32(150, 146, 140, 255); break;
                case ItemCatalog.AlienCrystal: speck = new Color32(180, 90, 230, 255); break;
                default: speck = new Color32(230, 230, 120, 255); break;
            }
            DrawBoulder(c, rock, speck, itemId.GetHashCode());
        }));

        /// <summary>Structure sprite: drawn facing North, then rotated clockwise for the given facing.</summary>
        public static Sprite Structure(StructureDefinition def, Direction facing)
        {
            if (!def.Rotatable) facing = Direction.North;
            int w = def.Width * PPU, h = def.Height * PPU;
            return Get($"structure.{def.Id}.{facing}", () =>
            {
                var px = new Color32[w * h];
                DrawStructure(px, def.Id, w, h);
                int cw = w, ch = h;
                for (int i = 0; i < (int)facing; i++) { px = RotateClockwise(px, cw, ch); (cw, ch) = (ch, cw); }
                var result = px;
                return Build(cw, ch, c => System.Array.Copy(result, c, c.Length));
            });
        }

        private static Color32[] RotateClockwise(Color32[] src, int w, int h)
        {
            // Destination is h wide, w tall. Rows go bottom-to-top (y up).
            var dst = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                dst[(w - 1 - x) * h + y] = src[y * w + x];
            return dst;
        }

        /// <summary>Little astronaut, 12x16 inside a 16x16 sprite. Frame 0/1 for walking.</summary>
        public static Sprite Player(Direction facing, int frame) => Get($"player.{facing}.{frame}", () => Build(PPU, PPU, c =>
        {
            var suit = new Color32(226, 226, 220, 255);
            var suitDark = new Color32(170, 172, 176, 255);
            var visor = new Color32(70, 170, 220, 255);
            var accent = new Color32(240, 140, 40, 255);
            // legs
            int lift = frame == 1 ? 1 : 0;
            Rect(c, 5, 1 + lift, 2, 3, suitDark); Rect(c, 9, 1 + (1 - lift), 2, 3, suitDark);
            // body
            Rect(c, 4, 4, 8, 6, suit); Rect(c, 4, 6, 8, 1, accent);
            // backpack hint on side views
            if (facing == Direction.East) Rect(c, 3, 5, 1, 4, suitDark);
            if (facing == Direction.West) Rect(c, 12, 5, 1, 4, suitDark);
            // helmet
            Rect(c, 4, 10, 8, 5, suit);
            if (facing == Direction.South) Rect(c, 5, 11, 6, 3, visor);
            else if (facing == Direction.East) Rect(c, 8, 11, 4, 3, visor);
            else if (facing == Direction.West) Rect(c, 4, 11, 4, 3, visor);
            else Rect(c, 5, 11, 6, 3, suitDark);
            OutlinePass(c, PPU, PPU);
        }));

        // ---------- drawing helpers ----------

        private static void DrawTree(Color32[] c)
        {
            var trunk = new Color32(96, 64, 40, 255);
            var leaf = new Color32(46, 98, 52, 255);
            var leafLight = new Color32(70, 132, 66, 255);
            Rect(c, 7, 1, 2, 5, trunk);
            Circle(c, 8, 10, 5.5f, leaf);
            Circle(c, 7, 11, 3f, leafLight);
            OutlinePass(c, PPU, PPU);
        }

        private static void DrawScrap(Color32[] c)
        {
            var metal = new Color32(150, 156, 166, 255);
            var dark = new Color32(92, 96, 106, 255);
            var rust = new Color32(160, 92, 56, 255);
            Rect(c, 2, 2, 8, 4, dark); Rect(c, 3, 3, 6, 2, metal);
            Rect(c, 7, 4, 6, 6, metal); Rect(c, 8, 5, 2, 2, rust);
            Rect(c, 4, 7, 3, 5, dark); Rect(c, 11, 2, 3, 2, rust);
            OutlinePass(c, PPU, PPU);
        }

        private static void DrawBoulder(Color32[] c, Color32 rock, Color32 speck, int seed)
        {
            Circle(c, 8, 7, 6.2f, rock);
            Circle(c, 7, 8, 4f, Shade(rock, 16));
            for (int i = 0; i < 7; i++)
            {
                int x = 4 + (int)(SeededRandom.Hash01(seed, i, 0) * 8);
                int y = 3 + (int)(SeededRandom.Hash01(seed, i, 1) * 8);
                Set(c, PPU, x, y, speck); Set(c, PPU, x + 1, y, speck);
            }
            OutlinePass(c, PPU, PPU);
        }

        private static void DrawStructure(Color32[] c, string id, int w, int h)
        {
            switch (id)
            {
                case StructureCatalog.Wall:
                {
                    var brick = new Color32(132, 128, 124, 255);
                    var mortar = new Color32(84, 82, 80, 255);
                    Fill(c, w, h, mortar);
                    for (int row = 0; row < h / 4; row++)
                    for (int col = -1; col < w / 8 + 1; col++)
                    {
                        int ox = col * 8 + (row % 2 == 0 ? 0 : 4);
                        for (int y = row * 4 + 1; y < row * 4 + 4; y++)
                        for (int x = ox + 1; x < ox + 8; x++)
                            if (x >= 0 && x < w) Set(c, w, x, y, Shade(brick, (int)(SeededRandom.Hash01(row, col, 9) * 20) - 10));
                    }
                    break;
                }
                case StructureCatalog.Floor:
                {
                    var tile = new Color32(118, 118, 128, 180);
                    Fill(c, w, h, tile);
                    for (int i = 0; i < w; i++) { Set(c, w, i, 0, new Color32(90, 90, 100, 200)); Set(c, w, 0, i, new Color32(90, 90, 100, 200)); }
                    return; // no outline for floors
                }
                case StructureCatalog.Crate:
                {
                    var wood = new Color32(150, 104, 60, 255);
                    var band = new Color32(110, 74, 44, 255);
                    RectW(c, w, 1, 1, w - 2, h - 2, wood);
                    RectW(c, w, 1, 1, w - 2, 2, band); RectW(c, w, 1, h - 3, w - 2, 2, band);
                    for (int i = 2; i < w - 2; i++) Set(c, w, i, i, band);
                    break;
                }
                case StructureCatalog.ShelterModule:
                {
                    var hull = new Color32(176, 182, 190, 255);
                    var panel = new Color32(140, 148, 160, 255);
                    var stripe = new Color32(240, 140, 40, 255);
                    var window = new Color32(90, 190, 230, 255);
                    RectW(c, w, 1, 1, w - 2, h - 2, hull);
                    for (int x = 8; x < w - 1; x += 16) for (int y = 1; y < h - 1; y++) Set(c, w, x, y, panel);
                    for (int y = 8; y < h - 1; y += 16) for (int x = 1; x < w - 1; x++) Set(c, w, x, y, panel);
                    RectW(c, w, 1, 2, w - 2, 2, stripe);
                    RectW(c, w, w / 2 - 3, h / 2, 6, 4, window);
                    break;
                }
                case StructureCatalog.Conveyor:
                {
                    var belt = new Color32(64, 64, 72, 255);
                    var rail = new Color32(200, 170, 60, 255);
                    var arrow = new Color32(120, 120, 132, 255);
                    RectW(c, w, 0, 0, w, h, belt);
                    RectW(c, w, 0, 0, 2, h, rail); RectW(c, w, w - 2, 0, 2, h, rail);
                    // chevrons pointing North (rotated with the structure)
                    for (int k = 0; k < 2; k++)
                    for (int i = 0; i < 4; i++)
                    {
                        int y0 = 4 + k * 6;
                        Set(c, w, 7 - i, y0 + i, arrow); Set(c, w, 8 + i, y0 + i, arrow);
                    }
                    return; // no outline: belts tile seamlessly
                }
                case StructureCatalog.Furnace:
                {
                    var brick = new Color32(150, 92, 70, 255);
                    var dark = new Color32(60, 40, 36, 255);
                    var glow = new Color32(255, 170, 60, 255);
                    RectW(c, w, 1, 1, w - 2, h - 2, brick);
                    for (int y = 4; y < h - 1; y += 5) RectW(c, w, 1, y, w - 2, 1, Shade(brick, -25));
                    RectW(c, w, 9, 6, 14, 10, dark);
                    RectW(c, w, 11, 7, 10, 5, glow);
                    RectW(c, w, w / 2 - 2, h - 5, 4, 4, dark); // output side marker
                    break;
                }
                case StructureCatalog.PowerPole:
                {
                    var wood = new Color32(120, 84, 52, 255);
                    var metal = new Color32(170, 176, 186, 255);
                    var insul = new Color32(90, 190, 120, 255);
                    RectW(c, w, 7, 1, 2, 12, wood);
                    RectW(c, w, 3, 11, 10, 2, metal);
                    Set(c, w, 3, 13, insul); Set(c, w, 12, 13, insul); Set(c, w, 7, 13, insul);
                    break;
                }
                case StructureCatalog.Computer:
                {
                    var casing = new Color32(70, 74, 86, 255);
                    var screen = new Color32(24, 40, 36, 255);
                    var glow = new Color32(90, 230, 140, 255);
                    RectW(c, w, 1, 1, w - 2, h - 2, casing);
                    RectW(c, w, 3, 6, w - 6, 7, screen);
                    for (int i = 0; i < 3; i++) RectW(c, w, 4, 8 + i * 2, 3 + (i * 3) % 6, 1, glow);
                    RectW(c, w, 3, 2, 4, 2, new Color32(120, 126, 140, 255));
                    Set(c, w, 11, 3, new Color32(240, 80, 60, 255));
                    break;
                }
                case StructureCatalog.Drill:
                {
                    var frame = new Color32(196, 150, 40, 255);
                    var dark = new Color32(84, 70, 40, 255);
                    var bit = new Color32(160, 168, 180, 255);
                    RectW(c, w, 1, 1, w - 2, h - 2, frame);
                    for (int i = 1; i < w - 1; i += 4) RectW(c, w, i, 1, 2, 2, dark);
                    Circle2(c, w, w / 2f, h / 2f, 7f, dark);
                    Circle2(c, w, w / 2f, h / 2f, 4f, bit);
                    RectW(c, w, w / 2 - 1, h - 6, 2, 5, dark); // output chute marker (top = facing side when north)
                    break;
                }
                case StructureCatalog.CoalGenerator:
                {
                    var body = new Color32(110, 70, 60, 255);
                    var steel = new Color32(150, 150, 160, 255);
                    var fire = new Color32(250, 150, 40, 255);
                    RectW(c, w, 1, 1, w - 2, h - 6, body);
                    RectW(c, w, w - 9, h - 8, 6, 7, steel); // chimney
                    RectW(c, w, 5, 5, 10, 6, new Color32(40, 30, 30, 255));
                    RectW(c, w, 7, 6, 6, 3, fire);
                    break;
                }
                case StructureCatalog.EmergencyBattery:
                {
                    var shell = new Color32(60, 90, 140, 255);
                    var cell = new Color32(120, 220, 160, 255);
                    var burn = new Color32(40, 34, 32, 255);
                    RectW(c, w, 1, 2, w - 2, h - 4, shell);
                    for (int i = 0; i < 4; i++) RectW(c, w, 4 + i * 6, 5, 4, 6, cell);
                    RectW(c, w, w - 7, 3, 5, 3, burn); // scorch from the crash
                    break;
                }
                default:
                    Fill(c, w, h, new Color32(255, 0, 255, 255));
                    break;
            }
            OutlinePass(c, w, h);
        }

        // ---------- low level ----------

        private delegate void Painter(Color32[] pixels);

        private static Sprite Get(string key, System.Func<Sprite> create)
        {
            if (!Cache.TryGetValue(key, out var s) || s == null)
            {
                s = create();
                s.name = key;
                Cache[key] = s;
            }
            return s;
        }

        private static Sprite Build(int w, int h, Painter paint)
        {
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = Clear;
            paint(px);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0f), PPU, 0, SpriteMeshType.FullRect);
        }

        private static void Set(Color32[] c, int w, int x, int y, Color32 col)
        {
            int h = c.Length / w;
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            c[y * w + x] = col;
        }

        private static void Fill(Color32[] c, int w, int h, Color32 col) { for (int i = 0; i < c.Length; i++) c[i] = col; }

        private static void Rect(Color32[] c, int x0, int y0, int rw, int rh, Color32 col) => RectW(c, PPU, x0, y0, rw, rh, col);

        private static void RectW(Color32[] c, int w, int x0, int y0, int rw, int rh, Color32 col)
        {
            for (int y = y0; y < y0 + rh; y++) for (int x = x0; x < x0 + rw; x++) Set(c, w, x, y, col);
        }

        private static void Circle(Color32[] c, float cx, float cy, float r, Color32 col)
        {
            for (int y = 0; y < PPU; y++)
            for (int x = 0; x < PPU; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                if (dx * dx + dy * dy <= r * r) Set(c, PPU, x, y, col);
            }
        }

        private static void Circle2(Color32[] c, int w, float cx, float cy, float r, Color32 col)
        {
            int h = c.Length / w;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                if (dx * dx + dy * dy <= r * r) Set(c, w, x, y, col);
            }
        }

        /// <summary>Draws a dark 1-px outline around every opaque region (classic pixel-art readability).</summary>
        private static void OutlinePass(Color32[] c, int w, int h)
        {
            var src = (Color32[])c.Clone();
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (src[y * w + x].a != 0) continue;
                bool edge = (x > 0 && src[y * w + x - 1].a != 0) || (x < w - 1 && src[y * w + x + 1].a != 0) ||
                            (y > 0 && src[(y - 1) * w + x].a != 0) || (y < h - 1 && src[(y + 1) * w + x].a != 0);
                if (edge) c[y * w + x] = Outline;
            }
            // Opaque pixels on the sprite border also get outlined so tiles read as separate objects.
            for (int x = 0; x < w; x++) { DarkenEdge(c, w, x, 0); DarkenEdge(c, w, x, h - 1); }
            for (int y = 0; y < h; y++) { DarkenEdge(c, w, 0, y); DarkenEdge(c, w, w - 1, y); }
        }

        private static void DarkenEdge(Color32[] c, int w, int x, int y)
        {
            if (c[y * w + x].a != 0) c[y * w + x] = Outline;
        }

        private static Color32 Shade(Color32 col, int d) => new Color32(
            (byte)Mathf.Clamp(col.r + d, 0, 255), (byte)Mathf.Clamp(col.g + d, 0, 255), (byte)Mathf.Clamp(col.b + d, 0, 255), col.a);
    }
}
