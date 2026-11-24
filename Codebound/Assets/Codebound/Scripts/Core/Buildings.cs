using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    public enum BuildingKind : byte { Structure, Storage, Furnace, Drill, Generator, Pole, Computer, Belt, Conveyor, Inserter, Greenhouse, Assembler, Lab, Turret, Solar, Battery, Scrubber, LaunchPad, Server, StorageHub, Heater, LightningRod, ShockTrap, Pipe, Pump, Tank, OffshorePump, Pumpjack, Boiler, SteamEngine, Filler, Cable, ConstantCombinator, ArithmeticCombinator, DeciderCombinator, Lamp, Roboport, ProviderChest, RequesterChest, Rail, RailSignal, TrainStop, TrainDepot }

    public sealed class BuildingDef
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly BuildingKind Kind;
        public readonly int Width, Height;   // footprint at Rotation.R0
        public readonly ItemStack[] Cost;
        public readonly uint Color;          // placeholder pixel-art base colour 0xRRGGBB

        public BuildingDef(string id, string name, BuildingKind kind, int w, int h, uint color, params ItemStack[] cost)
        {
            Id = id; DisplayName = name; Kind = kind; Width = w; Height = h; Color = color; Cost = cost;
        }

        /// <summary>Production machines only work when a program drives them (Prompt rule). Infrastructure is passive.</summary>
        public bool Programmable => Kind == BuildingKind.Drill || Kind == BuildingKind.Furnace || Kind == BuildingKind.Conveyor
            || Kind == BuildingKind.Greenhouse || Kind == BuildingKind.Assembler || Kind == BuildingKind.Lab || Kind == BuildingKind.Turret
            || Kind == BuildingKind.LaunchPad || Kind == BuildingKind.Pump || Kind == BuildingKind.Pumpjack || Kind == BuildingKind.Filler;

        /// <summary>Belts can be walked over (Factorio-style); everything else blocks the player.</summary>
        public bool Walkable => Kind == BuildingKind.Belt || Kind == BuildingKind.Conveyor || Kind == BuildingKind.ShockTrap || Kind == BuildingKind.Pipe || Kind == BuildingKind.Cable
            || Kind == BuildingKind.Rail || Kind == BuildingKind.RailSignal;

        public (int w, int h) Size(Rotation r) => r.IsSideways() ? (Height, Width) : (Width, Height);

        /// <summary>Hit points against creatures: walls are tough, turrets sturdy.</summary>
        public float MaxHealth => Id == "reinforced_wall" ? 1000f : Kind == BuildingKind.Structure ? 400f : Kind == BuildingKind.Turret ? 200f : 100f;
    }

    public static class BuildingDatabase
    {
        public static readonly List<BuildingDef> All = new List<BuildingDef>
        {
            new BuildingDef("wall", "Stone Wall", BuildingKind.Structure, 1, 1, 0x8a8f99, new ItemStack(ItemDatabase.Stone, 2)),
            new BuildingDef("chest", "Storage Chest", BuildingKind.Storage, 1, 1, 0xa0703c, new ItemStack(ItemDatabase.Wood, 8)),
            new BuildingDef("furnace", "Stone Furnace", BuildingKind.Furnace, 2, 2, 0x6b5a4a, new ItemStack(ItemDatabase.Stone, 10)),
            new BuildingDef("drill", "Electric Drill", BuildingKind.Drill, 2, 2, 0xc9a227,
                new ItemStack(ItemDatabase.Stone, 6), new ItemStack(ItemDatabase.IronOre, 8)),
            new BuildingDef("computer", "Basic Computer", BuildingKind.Computer, 1, 2, 0x3a7bd5,
                new ItemStack(ItemDatabase.CopperOre, 6), new ItemStack(ItemDatabase.IronOre, 4), new ItemStack(ItemDatabase.Silicon, 2)),
            new BuildingDef("generator", "Coal Generator", BuildingKind.Generator, 2, 2, 0x5a6470,
                new ItemStack(ItemDatabase.Stone, 8), new ItemStack(ItemDatabase.IronOre, 6), new ItemStack(ItemDatabase.CopperOre, 4)),
            new BuildingDef("pole", "Power Pole", BuildingKind.Pole, 1, 1, 0x7a5a3a,
                new ItemStack(ItemDatabase.Wood, 2), new ItemStack(ItemDatabase.CopperOre, 1)),
            new BuildingDef("lab", "Research Lab", BuildingKind.Lab, 2, 2, 0xe8e8f0,
                new ItemStack(ItemDatabase.IronOre, 10), new ItemStack(ItemDatabase.CopperOre, 10), new ItemStack(ItemDatabase.Stone, 10), new ItemStack(ItemDatabase.Silicon, 2)),
            new BuildingDef("belt", "Transport Belt", BuildingKind.Belt, 1, 1, 0x4a4f57, new ItemStack(ItemDatabase.IronPlate, 1)),
            new BuildingDef("conveyor", "Programmable Conveyor", BuildingKind.Conveyor, 1, 1, 0x2f5f8f,
                new ItemStack(ItemDatabase.IronPlate, 1), new ItemStack(ItemDatabase.CopperPlate, 1)),
            new BuildingDef("sorter", "Sorter Belt", BuildingKind.Belt, 1, 1, 0x5a6f4a, new ItemStack(ItemDatabase.IronPlate, 2), new ItemStack(ItemDatabase.Circuit, 1)),
            new BuildingDef("splitter", "Splitter", BuildingKind.Belt, 1, 1, 0x6a5a3a, new ItemStack(ItemDatabase.IronPlate, 3), new ItemStack(ItemDatabase.IronGear, 2)),
            new BuildingDef("long_inserter", "Long Inserter", BuildingKind.Inserter, 1, 1, 0xb8821a,
                new ItemStack(ItemDatabase.IronPlate, 3), new ItemStack(ItemDatabase.IronGear, 2), new ItemStack(ItemDatabase.Circuit, 1)),
            new BuildingDef("inserter", "Inserter", BuildingKind.Inserter, 1, 1, 0xd9a520,
                new ItemStack(ItemDatabase.IronPlate, 2), new ItemStack(ItemDatabase.CopperPlate, 1)),
            new BuildingDef("greenhouse", "Greenhouse", BuildingKind.Greenhouse, 2, 2, 0x7fc8d8,
                new ItemStack(ItemDatabase.Wood, 10), new ItemStack(ItemDatabase.Stone, 6), new ItemStack(ItemDatabase.Silicon, 2)),
            new BuildingDef("assembler", "Assembler", BuildingKind.Assembler, 2, 2, 0x6d7f99,
                new ItemStack(ItemDatabase.IronPlate, 8), new ItemStack(ItemDatabase.CopperPlate, 4), new ItemStack(ItemDatabase.StoneBrick, 4)),
            new BuildingDef("turret", "Defense Turret", BuildingKind.Turret, 1, 1, 0x7a8590,
                new ItemStack(ItemDatabase.IronPlate, 10), new ItemStack(ItemDatabase.CopperPlate, 5), new ItemStack(ItemDatabase.StoneBrick, 5)),
            new BuildingDef("solar", "Solar Panel", BuildingKind.Solar, 2, 2, 0x2a4a8a,
                new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.Silicon, 4), new ItemStack(ItemDatabase.CopperPlate, 2)),
            new BuildingDef("battery", "Battery", BuildingKind.Battery, 1, 1, 0x3c5a3c,
                new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.CopperPlate, 6), new ItemStack(ItemDatabase.Coal, 2)),
            new BuildingDef("scrubber", "Air Scrubber", BuildingKind.Scrubber, 1, 1, 0x5fa8a0,
                new ItemStack(ItemDatabase.IronPlate, 6), new ItemStack(ItemDatabase.CopperWire, 6), new ItemStack(ItemDatabase.Silicon, 2)),
            new BuildingDef("launchpad", "Launch Pad", BuildingKind.LaunchPad, 3, 3, 0xc8ccd4,
                new ItemStack(ItemDatabase.IronPlate, 40), new ItemStack(ItemDatabase.StoneBrick, 20), new ItemStack(ItemDatabase.Circuit, 10), new ItemStack(ItemDatabase.CopperWire, 20)),
            new BuildingDef("server", "Data Server", BuildingKind.Server, 1, 2, 0x3a4a6a,
                new ItemStack(ItemDatabase.Circuit, 4), new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.CopperWire, 6)),
            new BuildingDef("storage_hub", "Storage Hub", BuildingKind.StorageHub, 2, 2, 0x6a6a3a,
                new ItemStack(ItemDatabase.IronPlate, 8), new ItemStack(ItemDatabase.Circuit, 2), new ItemStack(ItemDatabase.IronGear, 4)),
            new BuildingDef("hydroponics", "Hydroponic Bay", BuildingKind.Greenhouse, 2, 2, 0x4fa88f,
                new ItemStack(ItemDatabase.IronPlate, 6), new ItemStack(ItemDatabase.Silicon, 2), new ItemStack(ItemDatabase.CopperWire, 6)),
            new BuildingDef("reinforced_wall", "Reinforced Wall", BuildingKind.Structure, 1, 1, 0x5a5f6a,
                new ItemStack(ItemDatabase.StoneBrick, 4), new ItemStack(ItemDatabase.IronPlate, 2)),
            new BuildingDef("shock_trap", "Shock Trap", BuildingKind.ShockTrap, 1, 1, 0x3a6aa0,
                new ItemStack(ItemDatabase.IronPlate, 2), new ItemStack(ItemDatabase.CopperWire, 4)),
            new BuildingDef("heater", "Heater", BuildingKind.Heater, 1, 1, 0xc8603a,
                new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.CopperWire, 6)),
            new BuildingDef("lightning_rod", "Lightning Rod", BuildingKind.LightningRod, 1, 1, 0x9aa4b0,
                new ItemStack(ItemDatabase.IronPlate, 6), new ItemStack(ItemDatabase.CopperWire, 4)),
            new BuildingDef("computer2", "Advanced Computer", BuildingKind.Computer, 1, 2, 0x8a3ad5,
                new ItemStack(ItemDatabase.Circuit, 4), new ItemStack(ItemDatabase.IronGear, 2), new ItemStack(ItemDatabase.CopperWire, 6)),
            // fluids (Etapa 3 of the industrial simulation)
            new BuildingDef("pipe", "Pipe", BuildingKind.Pipe, 1, 1, 0x7d8894, new ItemStack(ItemDatabase.IronPlate, 1)),
            new BuildingDef("pump", "Pump", BuildingKind.Pump, 1, 1, 0x4f7fa8,
                new ItemStack(ItemDatabase.IronPlate, 2), new ItemStack(ItemDatabase.IronGear, 1), new ItemStack(ItemDatabase.Circuit, 1)),
            new BuildingDef("tank", "Storage Tank", BuildingKind.Tank, 2, 2, 0x8a9aa8,
                new ItemStack(ItemDatabase.IronPlate, 20), new ItemStack(ItemDatabase.StoneBrick, 5)),
            new BuildingDef("offshore_pump", "Offshore Pump", BuildingKind.OffshorePump, 1, 1, 0x3f8fc8,
                new ItemStack(ItemDatabase.IronPlate, 2), new ItemStack(ItemDatabase.IronGear, 1), new ItemStack(ItemDatabase.Circuit, 1)),
            new BuildingDef("pumpjack", "Pumpjack", BuildingKind.Pumpjack, 2, 2, 0x6a5a7a,
                new ItemStack(ItemDatabase.IronPlate, 10), new ItemStack(ItemDatabase.IronGear, 6), new ItemStack(ItemDatabase.Circuit, 4)),
            new BuildingDef("boiler", "Boiler", BuildingKind.Boiler, 2, 2, 0x8a5a4a,
                new ItemStack(ItemDatabase.StoneBrick, 6), new ItemStack(ItemDatabase.IronPlate, 4)),
            new BuildingDef("steam_engine", "Steam Engine", BuildingKind.SteamEngine, 2, 2, 0x6f7f6a,
                new ItemStack(ItemDatabase.IronPlate, 10), new ItemStack(ItemDatabase.IronGear, 8), new ItemStack(ItemDatabase.CopperWire, 6)),
            new BuildingDef("filler", "Canister Filler", BuildingKind.Filler, 2, 2, 0x9a7a3a,
                new ItemStack(ItemDatabase.IronPlate, 8), new ItemStack(ItemDatabase.IronGear, 4), new ItemStack(ItemDatabase.Circuit, 2)),
            // circuit signals (Etapa 5 of the industrial simulation)
            new BuildingDef("cable", "Signal Cable", BuildingKind.Cable, 1, 1, 0xc84a3a, new ItemStack(ItemDatabase.CopperWire, 1)),
            new BuildingDef("constant_combinator", "Constant Combinator", BuildingKind.ConstantCombinator, 1, 1, 0x4a5a6a,
                new ItemStack(ItemDatabase.CopperWire, 4), new ItemStack(ItemDatabase.Circuit, 2)),
            new BuildingDef("arithmetic_combinator", "Arithmetic Combinator", BuildingKind.ArithmeticCombinator, 1, 1, 0x3a5a7a,
                new ItemStack(ItemDatabase.CopperWire, 4), new ItemStack(ItemDatabase.Circuit, 3)),
            new BuildingDef("decider_combinator", "Decider Combinator", BuildingKind.DeciderCombinator, 1, 1, 0x5a3a7a,
                new ItemStack(ItemDatabase.CopperWire, 4), new ItemStack(ItemDatabase.Circuit, 3)),
            new BuildingDef("lamp", "Lamp", BuildingKind.Lamp, 1, 1, 0xd8c86a,
                new ItemStack(ItemDatabase.IronPlate, 1), new ItemStack(ItemDatabase.CopperWire, 2), new ItemStack(ItemDatabase.Silicon, 1)),
            // logistic robots (Etapa 6 of the industrial simulation)
            new BuildingDef("roboport", "Roboport", BuildingKind.Roboport, 2, 2, 0x5a6a7a,
                new ItemStack(ItemDatabase.IronPlate, 20), new ItemStack(ItemDatabase.IronGear, 10), new ItemStack(ItemDatabase.Circuit, 10)),
            new BuildingDef("provider_chest", "Provider Chest", BuildingKind.ProviderChest, 1, 1, 0xb84a3a,
                new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.Circuit, 2)),
            new BuildingDef("requester_chest", "Requester Chest", BuildingKind.RequesterChest, 1, 1, 0x3a6ab8,
                new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.Circuit, 2)),
            // trains (Etapa 7 of the industrial simulation)
            new BuildingDef("rail", "Rail", BuildingKind.Rail, 1, 1, 0x6a5a4a, new ItemStack(ItemDatabase.IronPlate, 1), new ItemStack(ItemDatabase.Stone, 1)),
            new BuildingDef("rail_signal", "Rail Signal", BuildingKind.RailSignal, 1, 1, 0x6a5a4a,
                new ItemStack(ItemDatabase.IronPlate, 2), new ItemStack(ItemDatabase.CopperWire, 2), new ItemStack(ItemDatabase.Circuit, 1)),
            new BuildingDef("train_stop", "Train Stop", BuildingKind.TrainStop, 1, 1, 0xc8a03a,
                new ItemStack(ItemDatabase.IronPlate, 4), new ItemStack(ItemDatabase.Circuit, 2)),
            new BuildingDef("train_depot", "Train Depot", BuildingKind.TrainDepot, 2, 2, 0x7a4a3a,
                new ItemStack(ItemDatabase.IronPlate, 20), new ItemStack(ItemDatabase.StoneBrick, 10), new ItemStack(ItemDatabase.IronGear, 6)),
        };

        public static BuildingDef TryGet(string id) { foreach (var b in All) if (b.Id == id) return b; return null; }

        public static BuildingDef Get(string id)
        {
            foreach (var b in All) if (b.Id == id) return b;
            throw new ArgumentException("Unknown building id: " + id);
        }
    }

    public sealed class PlacedBuilding
    {
        public readonly int InstanceId;
        public readonly BuildingDef Def;
        public readonly Cell Origin;      // bottom-left cell of the rotated footprint
        public readonly Rotation Rotation;
        public MachineState State;        // null for plain structures
        public string Owner;              // multiplayer: the guest who built it (null = the host)
        public float Health;

        public PlacedBuilding(int id, BuildingDef def, Cell origin, Rotation rot)
        {
            InstanceId = id; Def = def; Origin = origin; Rotation = rot; Health = def.MaxHealth;
        }

        public (int w, int h) Size => Def.Size(Rotation);
        public double CenterX => Origin.X + Size.w / 2.0;
        public double CenterY => Origin.Y + Size.h / 2.0;

        public double DistanceTo(PlacedBuilding o)
        {
            double dx = CenterX - o.CenterX, dy = CenterY - o.CenterY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public IEnumerable<Cell> Cells()
        {
            var (w, h) = Size;
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    yield return new Cell(Origin.X + dx, Origin.Y + dy);
        }

        /// <summary>Cells just outside the footprint on the side opposite to the front.</summary>
        public IEnumerable<Cell> BackCells()
        {
            var (w, h) = Size;
            switch (Rotation)
            {
                case Rotation.R0: for (int x = 0; x < w; x++) yield return new Cell(Origin.X + x, Origin.Y - 1); break;
                case Rotation.R90: for (int y = 0; y < h; y++) yield return new Cell(Origin.X - 1, Origin.Y + y); break;
                case Rotation.R180: for (int x = 0; x < w; x++) yield return new Cell(Origin.X + x, Origin.Y + h); break;
                default: for (int y = 0; y < h; y++) yield return new Cell(Origin.X + w, Origin.Y + y); break;
            }
        }

        /// <summary>Cells just outside the footprint on the side the building faces (R0 = up, clockwise).</summary>
        public IEnumerable<Cell> FrontCells()
        {
            var (w, h) = Size;
            switch (Rotation)
            {
                case Rotation.R0: for (int x = 0; x < w; x++) yield return new Cell(Origin.X + x, Origin.Y + h); break;
                case Rotation.R90: for (int y = 0; y < h; y++) yield return new Cell(Origin.X + w, Origin.Y + y); break;
                case Rotation.R180: for (int x = 0; x < w; x++) yield return new Cell(Origin.X + x, Origin.Y - 1); break;
                default: for (int y = 0; y < h; y++) yield return new Cell(Origin.X - 1, Origin.Y + y); break;
            }
        }
    }

    /// <summary>Cell occupancy for structures. Terrain rules are injected so Core stays engine-free.</summary>
    public sealed class BuildGrid
    {
        readonly Dictionary<Cell, PlacedBuilding> occupancy = new Dictionary<Cell, PlacedBuilding>();
        readonly Dictionary<int, PlacedBuilding> byId = new Dictionary<int, PlacedBuilding>();
        // the simulation's update order: by id = build order (a Dictionary's order changes after removals)
        readonly List<PlacedBuilding> ordered = new List<PlacedBuilding>();
        readonly Func<BuildingDef, Cell, bool> isBuildableTerrain;
        int nextId = 1;

        public event Action<PlacedBuilding> Placed;
        public event Action<PlacedBuilding> Removed;

        public BuildGrid(Func<BuildingDef, Cell, bool> isBuildableTerrain) { this.isBuildableTerrain = isBuildableTerrain; }

        /// <summary>Every building, oldest first. This order is part of the simulation's determinism.</summary>
        public IReadOnlyList<PlacedBuilding> Buildings => ordered;
        public int Count => byId.Count;
        public bool IsOccupied(Cell c) => occupancy.ContainsKey(c);
        public PlacedBuilding At(Cell c) => occupancy.TryGetValue(c, out var b) ? b : null;
        public PlacedBuilding ById(int id) => byId.TryGetValue(id, out var b) ? b : null;

        public bool CanPlace(BuildingDef def, Cell origin, Rotation rot, Func<Cell, bool> extraBlocker = null)
        {
            var (w, h) = def.Size(rot);
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                {
                    var c = new Cell(origin.X + dx, origin.Y + dy);
                    if (occupancy.ContainsKey(c) || !isBuildableTerrain(def, c)) return false;
                    if (extraBlocker != null && extraBlocker(c)) return false;
                }
            return true;
        }

        public PlacedBuilding Place(BuildingDef def, Cell origin, Rotation rot, Func<Cell, bool> extraBlocker = null)
        {
            if (!CanPlace(def, origin, rot, extraBlocker)) return null;
            var b = new PlacedBuilding(nextId++, def, origin, rot);
            foreach (var c in b.Cells()) occupancy[c] = b;
            byId[b.InstanceId] = b;
            ordered.Add(b);   // ids only grow, so appending keeps the list sorted
            Placed?.Invoke(b);
            return b;
        }

        /// <summary>Walls (structures) crossed by the straight line between two points: sound occlusion, line of sight.</summary>
        public int StructuresBetween(double x0, double y0, double x1, double y1, PlacedBuilding ignore = null)
        {
            double len = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
            int steps = Math.Max(1, (int)(len * 3));
            var seen = new HashSet<int>();
            for (int i = 1; i < steps; i++)
            {
                double t = i / (double)steps;
                var b = At(new Cell((int)Math.Floor(x0 + (x1 - x0) * t), (int)Math.Floor(y0 + (y1 - y0) * t)));
                if (b != null && b != ignore && b.Def.Kind == BuildingKind.Structure) seen.Add(b.InstanceId);
            }
            return seen.Count;
        }

        static readonly IComparer<PlacedBuilding> IdOrder = Comparer<PlacedBuilding>.Create((p, q) => p.InstanceId.CompareTo(q.InstanceId));

        public bool Remove(PlacedBuilding b)
        {
            if (b == null || !byId.Remove(b.InstanceId)) return false;
            ordered.RemoveAt(ordered.BinarySearch(b, IdOrder));
            foreach (var c in b.Cells()) occupancy.Remove(c);
            Removed?.Invoke(b);
            return true;
        }
    }
}
