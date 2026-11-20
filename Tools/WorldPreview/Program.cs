using System.IO;
using Codebound.Core;
using Codebound.Items;
using Codebound.World;

long seed = args.Length > 0 ? long.Parse(args[0]) : 2026;
string outPath = args.Length > 1 ? args[1] : $"world_{seed}.ppm";
const int S = 4; // pixels per cell
var map = TestWorldGenerator.Generate(seed);
int w = map.Width * S, h = map.Height * S;
var px = new byte[w * h * 3];

void Put(int cx, int cy, (byte, byte, byte) c, bool dot)
{
    for (int y = 0; y < S; y++)
    for (int x = 0; x < S; x++)
    {
        if (dot && (x == 0 || y == 0 || x == S - 1 || y == S - 1)) continue;
        int iy = (map.Height - 1 - cy) * S + y, ix = cx * S + x;
        int i = (iy * w + ix) * 3;
        px[i] = c.Item1; px[i + 1] = c.Item2; px[i + 2] = c.Item3;
    }
}

for (int y = 0; y < map.Height; y++)
for (int x = 0; x < map.Width; x++)
{
    var t = map.GetTerrain(new GridPos(x, y));
    (byte, byte, byte) c = t switch
    {
        TerrainType.Grass => ((byte)62, (byte)112, (byte)64),
        TerrainType.Dirt => ((byte)110, (byte)82, (byte)58),
        TerrainType.Sand => ((byte)196, (byte)172, (byte)112),
        TerrainType.Rock => ((byte)76, (byte)76, (byte)86),
        TerrainType.Water => ((byte)38, (byte)76, (byte)128),
        _ => ((byte)40, (byte)34, (byte)32),
    };
    Put(x, y, c, false);
}
foreach (var n in map.Nodes)
{
    (byte, byte, byte) c = n.ItemId switch
    {
        ItemCatalog.Wood => ((byte)20, (byte)60, (byte)24),
        ItemCatalog.Scrap => ((byte)220, (byte)220, (byte)230),
        ItemCatalog.IronOre => ((byte)200, (byte)110, (byte)90),
        ItemCatalog.CopperOre => ((byte)240, (byte)150, (byte)40),
        ItemCatalog.Coal => ((byte)10, (byte)10, (byte)10),
        ItemCatalog.Stone => ((byte)170, (byte)170, (byte)160),
        ItemCatalog.Silicon => ((byte)170, (byte)220, (byte)250),
        _ => ((byte)255, (byte)0, (byte)255),
    };
    Put(n.Cell.X, n.Cell.Y, c, true);
}
Put(map.SpawnPoint.X, map.SpawnPoint.Y, ((byte)255, (byte)40, (byte)40), false);

using var f = File.Create(outPath);
var header = System.Text.Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n");
f.Write(header, 0, header.Length);
f.Write(px, 0, px.Length);
System.Console.WriteLine($"Wrote {outPath} ({map.Nodes.Count} nodes)");
