using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Codebound.Core
{
    /// <summary>Everything that defines one world for one player (multiplayer splits player data out in Etapa 6).</summary>
    public sealed class GameState
    {
        public readonly WorldState World;
        public readonly BuildGrid Grid;
        public readonly Factory Factory;
        public readonly Inventory Inventory = new Inventory(40);
        public readonly Hud.HudLayout Hud = new Hud.HudLayout();
        public readonly Vitals Vitals = new Vitals();
        public readonly WorldClock Clock;
        public readonly CreatureSystem Creatures;
        public Research Research => Factory.Research;
        public double PlayerX, PlayerY;

        public GameState(int seed)
        {
            World = new WorldState(new WorldGenerator(seed));
            Grid = new BuildGrid(IsBuildable);
            Factory = new Factory(World, Grid);
            Clock = new WorldClock(seed);
            Creatures = new CreatureSystem(this);
        }

        public int Seed => World.Generator.Seed;

        bool IsBuildable(BuildingDef def, Cell c)
        {
            if (!WorldRules.IsWalkable(World.Generator.TileAt(c))) return false;
            var d = World.DepositAt(c);
            return def.Kind == BuildingKind.Drill ? d.Type != ResourceType.Tree : d.IsNone;
        }

        /// <summary>Grid rules plus: a drill must cover at least one minable deposit.</summary>
        public bool CanPlace(BuildingDef def, Cell origin, Rotation rot, Func<Cell, bool> blocker = null)
        {
            if (!Grid.CanPlace(def, origin, rot, blocker)) return false;
            if (def.Kind != BuildingKind.Drill) return true;
            var (w, h) = def.Size(rot);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (WorldRules.DrillCanMine(World.DepositAt(new Cell(origin.X + x, origin.Y + y)))) return true;
            return false;
        }

        /// <summary>Pays the cost and places. Returns null (and pays nothing) when not possible.</summary>
        public PlacedBuilding TryBuild(BuildingDef def, Cell origin, Rotation rot, Func<Cell, bool> blocker = null)
        {
            if (!Research.IsUnlocked(def) || !CanPlace(def, origin, rot, blocker) || !Inventory.TryRemoveAll(def.Cost)) return null;
            return Grid.Place(def, origin, rot, blocker);
        }

        /// <summary>Removes a building, returning its cost and contents to the player. Refuses if they don't fit.</summary>
        public bool TryDeconstruct(PlacedBuilding b)
        {
            var refund = new List<ItemStack>(b.Def.Cost);
            if (b.State != null)
                foreach (var (_, inv) in b.State.Inventories)
                    for (int i = 0; i < inv.SlotCount; i++) if (!inv[i].IsEmpty) refund.Add(inv[i]);
            if (!Inventory.TryAddAll(refund)) return false;
            return Grid.Remove(b);
        }

        /// <summary>Salvage from the crashed ship: enough electronics for the first computer.</summary>
        public void GiveStartingKit() => Inventory.Add(ItemDatabase.Silicon, 6);
    }

    /// <summary>
    /// Versioned, line-based save format (human-readable, diff-friendly, no engine dependency).
    /// Only player changes are stored; the terrain is regenerated from the seed.
    /// </summary>
    public static class SaveSystem
    {
        public const int Version = 1;
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static void Write(GameState g, TextWriter w)
        {
            w.WriteLine($"CODEBOUND {Version}");
            w.WriteLine($"seed {g.Seed}");
            w.WriteLine($"player {g.PlayerX.ToString("R", Inv)} {g.PlayerY.ToString("R", Inv)}");
            WriteInv(w, "inv", g.Inventory);
            foreach (var kv in g.World.MinedDeltas) w.WriteLine($"mined {kv.Key.X} {kv.Key.Y} {kv.Value}");
            var extra = new SortedDictionary<string, string>();
            foreach (var b in g.Grid.Buildings)
            {
                w.WriteLine($"b {b.Def.Id} {b.Origin.X} {b.Origin.Y} {(int)b.Rotation}");
                if (b.Health < b.Def.MaxHealth) w.WriteLine("bh " + b.Health.ToString("R", Inv));
                if (b.State == null) continue;
                foreach (var (role, inv) in b.State.Inventories) WriteInv(w, "bi " + role, inv);
                extra.Clear();
                b.State.SaveExtra(extra);
                foreach (var kv in extra) w.WriteLine($"bx {kv.Key} {kv.Value}");
            }
            w.WriteLine(g.Vitals.SaveLine());
            w.WriteLine(g.Clock.SaveLine());
            if (g.Research.SaveLine() is string rl) w.WriteLine(rl);
            foreach (var line in g.Hud.SaveLines()) w.WriteLine(line);
            w.WriteLine("end");
        }

        static void WriteInv(TextWriter w, string prefix, Inventory inv)
        {
            for (int i = 0; i < inv.SlotCount; i++)
                if (!inv[i].IsEmpty) w.WriteLine($"{prefix} {inv[i].ItemId} {inv[i].Count}");
        }

        /// <summary>Parses a save. Throws InvalidDataException with the line number on malformed input.</summary>
        public static GameState Read(TextReader r)
        {
            string header = r.ReadLine();
            if (header == null || !header.StartsWith("CODEBOUND ")) throw new InvalidDataException("Not a Codebound save");
            int ver = int.Parse(header.Substring(10), Inv);
            if (ver > Version) throw new InvalidDataException($"Save version {ver} is newer than this game ({Version})");

            GameState g = null;
            PlacedBuilding last = null;
            var pending = new List<(string[] parts, int line)>();
            string line; int n = 1; bool ended = false;
            while ((line = r.ReadLine()) != null)
            {
                n++;
                if (line.Length == 0) continue;
                var p = line.Split(' ');
                try
                {
                    switch (p[0])
                    {
                        case "seed": g = new GameState(int.Parse(p[1], Inv)); break;
                        case "player": Need(g).PlayerX = double.Parse(p[1], Inv); g.PlayerY = double.Parse(p[2], Inv); break;
                        case "inv": Need(g).Inventory.Add(p[1], int.Parse(p[2], Inv)); break;
                        case "mined": Need(g).World.SetMined(new Cell(int.Parse(p[1], Inv), int.Parse(p[2], Inv)), int.Parse(p[3], Inv)); break;
                        case "b":
                            var def = BuildingDatabase.Get(p[1]);
                            last = Need(g).Grid.Place(def, new Cell(int.Parse(p[2], Inv), int.Parse(p[3], Inv)), (Rotation)(int.Parse(p[4], Inv) & 3));
                            if (last == null) throw new InvalidDataException($"Building {p[1]} cannot be placed at its saved position");
                            break;
                        case "bi":
                            foreach (var (role, inv) in Need(last).State.Inventories)
                                if (role == p[1]) inv.Add(p[2], int.Parse(p[3], Inv));
                            break;
                        case "bx": Need(last).State.LoadExtra(p[1], p.Length > 2 ? p[2] : ""); break;
                        case "bh": Need(last).Health = float.Parse(p[1], Inv); break;
                        case "hud": Need(g).Hud.LoadLine(p); break;
                        case "vitals": Need(g).Vitals.Load(p); break;
                        case "clock": Need(g).Clock.Load(p); break;
                        case "research": Need(g).Research.Load(p); break;
                        case "end": ended = true; break;
                        default: throw new InvalidDataException("Unknown record '" + p[0] + "'");
                    }
                }
                catch (Exception e) when (!(e is InvalidDataException))
                {
                    throw new InvalidDataException($"Save line {n}: {e.Message}", e);
                }
                catch (InvalidDataException e)
                {
                    throw new InvalidDataException($"Save line {n}: {e.Message}", e);
                }
            }
            if (g == null || !ended) throw new InvalidDataException("Save file is truncated");
            return g;
        }

        static T Need<T>(T v) where T : class => v ?? throw new InvalidDataException("Record out of order");
    }
}
