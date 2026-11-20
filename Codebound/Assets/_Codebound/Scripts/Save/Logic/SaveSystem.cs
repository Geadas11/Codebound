using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Codebound.Building;
using Codebound.Core;
using Codebound.Inventory;
using Codebound.Items;
using Codebound.Machines;
using Codebound.World;

namespace Codebound.Save
{
    public sealed class SaveFormatException : Exception
    {
        public SaveFormatException(string message) : base(message) { }
    }

    /// <summary>
    /// Versioned binary save format. Terrain is not stored: it is regenerated from the seed. Stored: resource
    /// nodes (current amounts), structures with inventories, machine state, computer programs and links,
    /// player position and inventory, and the simulation tick. Little-endian, UTF-8 strings.
    /// </summary>
    public static class SaveSystem
    {
        private const uint Magic = 0x56534243; // "CBSV"
        public const int CurrentVersion = 1;

        public static void Save(GameState state, Stream stream)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            using var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            w.Write(Magic);
            w.Write(CurrentVersion);
            w.Write(state.World.Seed);
            w.Write(state.World.Width);
            w.Write(state.World.Height);
            w.Write(state.Clock.CurrentTick);

            // Player
            w.Write(state.Motor.Position.X);
            w.Write(state.Motor.Position.Y);
            WriteInventory(w, state.PlayerInventory);

            // Resource nodes
            w.Write(state.World.Nodes.Count);
            foreach (var n in state.World.Nodes)
            {
                WritePos(w, n.Cell);
                w.Write(n.ItemId);
                w.Write(n.Remaining);
                w.Write(n.InitialAmount);
                w.Write(n.YieldPerHarvest);
            }

            // Structures (sorted by id so placement order — and therefore new ids — is stable)
            var structures = new List<PlacedStructure>(state.Grid.Structures);
            structures.Sort((a, b) => a.InstanceId.CompareTo(b.InstanceId));
            w.Write(structures.Count);
            foreach (var s in structures)
            {
                w.Write(s.Definition.Id);
                WritePos(w, s.Origin);
                w.Write((byte)s.Facing);
                w.Write(s.Inventory != null);
                if (s.Inventory != null) WriteInventory(w, s.Inventory);
                WriteMachineState(w, state, s);
            }

            // Computer links (by origin cells, ids are not stable across loads)
            var links = new List<(GridPos computer, GridPos machine)>();
            foreach (var m in state.Machines.Machines)
                if (m is Computer c)
                    foreach (var linked in c.Links) links.Add((c.Structure.Origin, linked.Structure.Origin));
            w.Write(links.Count);
            foreach (var (cp, mp) in links) { WritePos(w, cp); WritePos(w, mp); }
        }

        public static GameState Load(Stream stream, ItemRegistry items = null)
        {
            items ??= ItemCatalog.CreateDefault();
            using var r = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            try
            {
                if (r.ReadUInt32() != Magic) throw new SaveFormatException("Not a Codebound save file");
                int version = r.ReadInt32();
                if (version < 1 || version > CurrentVersion) throw new SaveFormatException($"Unsupported save version {version}");

                long seed = r.ReadInt64();
                int width = r.ReadInt32(), height = r.ReadInt32();
                long tick = r.ReadInt64();
                var state = GameState.CreateForLoad(seed, width, height, items);
                state.Clock.RestoreTick(tick);

                state.Motor.Teleport(new Vec2(r.ReadSingle(), r.ReadSingle()));
                ReadInventory(r, state.PlayerInventory, items);

                state.World.ClearNodes();
                int nodeCount = r.ReadInt32();
                for (int i = 0; i < nodeCount; i++)
                {
                    var cell = ReadPos(r);
                    string itemId = r.ReadString();
                    int remaining = r.ReadInt32(), initial = r.ReadInt32(), yield = r.ReadInt32();
                    if (!items.Contains(itemId)) throw new SaveFormatException($"Unknown item '{itemId}'");
                    state.World.AddNode(new ResourceNode(cell, itemId, remaining, initial, yield));
                }

                int structureCount = r.ReadInt32();
                for (int i = 0; i < structureCount; i++)
                {
                    string defId = r.ReadString();
                    var def = StructureCatalog.Find(defId) ?? throw new SaveFormatException($"Unknown structure '{defId}'");
                    var origin = ReadPos(r);
                    var facing = (Direction)r.ReadByte();
                    var status = state.Grid.TryPlace(def, origin, facing, null, out var placed);
                    if (status != PlacementStatus.Ok) throw new SaveFormatException($"Cannot restore {defId} at {origin}: {status}");
                    if (r.ReadBoolean()) ReadInventory(r, placed.Inventory, items);
                    ReadMachineState(r, state, placed);
                }

                int linkCount = r.ReadInt32();
                for (int i = 0; i < linkCount; i++)
                {
                    var computer = state.Machines.Get<Computer>(state.Grid.GetAt(ReadPos(r)));
                    var machine = state.Machines.Get(state.Grid.GetAt(ReadPos(r)));
                    if (computer == null || machine == null) throw new SaveFormatException("Broken computer link");
                    computer.Link(machine);
                }
                return state;
            }
            catch (EndOfStreamException) { throw new SaveFormatException("Save file is truncated"); }
        }

        // ---------- machine state ----------
        // Each structure writes a tag byte then its fields, so new machine types can be added in later versions.
        private const byte TagNone = 0, TagDrill = 1, TagGenerator = 2, TagComputer = 3, TagBattery = 4;

        private static void WriteMachineState(BinaryWriter w, GameState state, PlacedStructure s)
        {
            var battery = state.Machines.GetBattery(s);
            switch (state.Machines.Get(s))
            {
                case Drill d:
                    w.Write(TagDrill); w.Write(d.CyclePending); w.Write(d.Progress); w.Write(d.TotalMined); return;
                case CoalGenerator g:
                    w.Write(TagGenerator); w.Write(g.Enabled); w.Write(g.BufferedJoules); return;
                case Computer c:
                    w.Write(TagComputer);
                    w.Write(c.Language.HasValue ? (int)c.Language.Value : -1);
                    w.Write(c.Source);
                    w.Write((int)c.ProgramStatus);
                    w.Write(c.LastError ?? string.Empty);
                    return;
            }
            if (battery != null) { w.Write(TagBattery); w.Write(battery.StoredJoules); return; }
            w.Write(TagNone);
        }

        private static void ReadMachineState(BinaryReader r, GameState state, PlacedStructure s)
        {
            byte tag = r.ReadByte();
            switch (tag)
            {
                case TagNone: return;
                case TagDrill:
                {
                    bool pending = r.ReadBoolean(); double progress = r.ReadDouble(); int mined = r.ReadInt32();
                    state.Machines.Get<Drill>(s)?.RestoreState(pending, progress, mined);
                    return;
                }
                case TagGenerator:
                {
                    bool enabled = r.ReadBoolean(); double buffered = r.ReadDouble();
                    state.Machines.Get<CoalGenerator>(s)?.RestoreState(enabled, buffered);
                    return;
                }
                case TagComputer:
                {
                    int lang = r.ReadInt32(); string source = r.ReadString(); int status = r.ReadInt32(); string err = r.ReadString();
                    state.Machines.Get<Computer>(s)?.RestoreProgram(lang < 0 ? (ProgrammingLanguage?)null : (ProgrammingLanguage)lang,
                        source, (ProgramStatus)status, err.Length == 0 ? null : err);
                    return;
                }
                case TagBattery:
                {
                    double joules = r.ReadDouble();
                    state.Machines.GetBattery(s)?.RestoreCharge(joules);
                    return;
                }
                default: throw new SaveFormatException($"Unknown machine tag {tag}");
            }
        }

        // ---------- helpers ----------

        private static void WritePos(BinaryWriter w, GridPos p) { w.Write(p.X); w.Write(p.Y); }
        private static GridPos ReadPos(BinaryReader r) => new GridPos(r.ReadInt32(), r.ReadInt32());

        private static void WriteInventory(BinaryWriter w, ItemInventory inv)
        {
            w.Write(inv.SlotCount);
            for (int i = 0; i < inv.SlotCount; i++)
            {
                var slot = inv.GetSlot(i);
                w.Write(slot.IsEmpty ? string.Empty : slot.ItemId);
                w.Write(slot.IsEmpty ? 0 : slot.Count);
            }
        }

        private static void ReadInventory(BinaryReader r, ItemInventory inv, ItemRegistry items)
        {
            int slots = r.ReadInt32();
            inv.Clear();
            for (int i = 0; i < slots; i++)
            {
                string id = r.ReadString();
                int count = r.ReadInt32();
                if (count <= 0 || id.Length == 0) continue;
                if (!items.Contains(id)) throw new SaveFormatException($"Unknown item '{id}'");
                if (inv.Add(id, count) > 0) throw new SaveFormatException("Inventory contents do not fit");
            }
        }
    }
}
