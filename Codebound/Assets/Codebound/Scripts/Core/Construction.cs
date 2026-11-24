using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Codebound.Core
{
    /// <summary>A planned building (placed with Shift in build mode): construction robots bring its cost and build it.</summary>
    public sealed class Ghost
    {
        public BuildingDef Def; public Cell Origin; public Rotation Rot;
        public bool Covers(Cell c)
        {
            var (w, h) = Def.Size(Rot);
            return c.X >= Origin.X && c.X < Origin.X + w && c.Y >= Origin.Y && c.Y < Origin.Y + h;
        }
        public double CenterX => Origin.X + Def.Size(Rot).Item1 / 2.0;
        public double CenterY => Origin.Y + Def.Size(Rot).Item2 / 2.0;
    }

    public enum BuildTask : byte { Pickup, ToSite, ToStorage, Return }

    /// <summary>A construction robot in flight: collecting a ghost's materials, building it, or carrying a demolished building's refund to storage.</summary>
    public sealed class ConstructionRobot
    {
        public double X, Y;
        public BuildTask Task;
        public Cell Home, Site;
        public bool Demolish;                     // true: take the building at Site down; false: build the ghost at Site
        public readonly List<(Cell chest, string item, int count)> Pickups = new List<(Cell, string, int)>();
        public readonly List<ItemStack> Cargo = new List<ItemStack>();
        public int CargoCount(string item) { int n = 0; foreach (var s in Cargo) if (s.ItemId == item) n += s.Count; return n; }
        public void Load(string item, int n)
        {
            if (n <= 0) return;
            for (int i = 0; i < Cargo.Count; i++) if (Cargo[i].ItemId == item) { Cargo[i] = new ItemStack(item, Cargo[i].Count + n); return; }
            Cargo.Add(new ItemStack(item, n));
        }
    }

    public sealed partial class Factory
    {
        /// <summary>Planned buildings, oldest first.</summary>
        public readonly List<Ghost> Ghosts = new List<Ghost>();
        /// <summary>Buildings marked for demolition (ids, in the order they were marked).</summary>
        public readonly List<int> Demolish = new List<int>();
        public readonly List<ConstructionRobot> Builders = new List<ConstructionRobot>();
        /// <summary>Set by GameState: may this building go here (terrain, ore, fluids...)?</summary>
        public Func<BuildingDef, Cell, Rotation, bool> CanBuild;

        public Ghost GhostAt(Cell c) { foreach (var g in Ghosts) if (g.Covers(c)) return g; return null; }
        public bool MarkedForDemolition(PlacedBuilding b) => b != null && Demolish.Contains(b.InstanceId);

        /// <summary>Plans a building. Null or why not.</summary>
        public string PlaceGhost(BuildingDef def, Cell origin, Rotation rot)
        {
            if (def == null) return "unknown building";
            if (CanBuild != null && !CanBuild(def, origin, rot)) return "blocked";
            var probe = new Ghost { Def = def, Origin = origin, Rot = rot };
            var (w, h) = def.Size(rot);
            for (int dx = 0; dx < w; dx++) for (int dy = 0; dy < h; dy++) if (GhostAt(new Cell(origin.X + dx, origin.Y + dy)) != null) return "already planned";
            if (Ghosts.Count >= 2000) return "too many plans";
            Ghosts.Add(probe);
            return null;
        }

        public bool CancelGhost(Cell c)
        {
            var g = GhostAt(c);
            if (g == null) return false;
            Ghosts.Remove(g);
            return true;
        }

        /// <summary>Marks a building for demolition, or unmarks it. Returns whether it is now marked.</summary>
        public bool ToggleDemolish(PlacedBuilding b)
        {
            if (b == null) return false;
            if (Demolish.Remove(b.InstanceId)) return false;
            Demolish.Add(b.InstanceId);
            return true;
        }

        LogisticNet NetCovering(double x, double y)
        {
            foreach (var port in roboports) if (Covers(port, x, y)) return logisticOf.TryGetValue(port.InstanceId, out var n) ? n : null;
            return null;
        }

        static PlacedBuilding ReadyBuilderPort(LogisticNet net, double x, double y)
        {
            PlacedBuilding best = null; double bd = double.MaxValue;
            foreach (var p in net.Roboports)
            {
                var rs = (RoboportState)p.State;
                if (rs.PowerSatisfaction <= 0 || rs.Robots.Count(ItemDatabase.ConstructionRobot) == 0) continue;
                double d = (p.CenterX - x) * (p.CenterX - x) + (p.CenterY - y) * (p.CenterY - y);
                if (d < bd) { bd = d; best = p; }
            }
            return best;
        }

        /// <summary>Construction, each tick (after the logistic robots, which already counted reservations): move, then dispatch.</summary>
        void TickBuilders(float dt)
        {
            Demolish.RemoveAll(id => grid.ById(id) == null);
            for (int i = 0; i < Builders.Count; i++)
                if (MoveBuilder(Builders[i], dt)) { Builders.RemoveAt(i); i--; }
            if (logisticNets.Count == 0 || (Ghosts.Count == 0 && Demolish.Count == 0)) return;
            var busy = new HashSet<Cell>();
            foreach (var r in Builders) if (r.Task == BuildTask.Pickup || r.Task == BuildTask.ToSite) busy.Add(r.Site);
            int sent = 0;
            foreach (var id in Demolish)
            {
                if (sent >= MaxDispatchPerTick) break;
                var b = grid.ById(id);
                if (b == null || busy.Contains(b.Origin)) continue;
                var net = NetCovering(b.CenterX, b.CenterY);
                var port = net == null ? null : ReadyBuilderPort(net, b.CenterX, b.CenterY);
                if (port == null) continue;
                ((RoboportState)port.State).Robots.TryRemove(ItemDatabase.ConstructionRobot, 1);
                Builders.Add(new ConstructionRobot { X = port.CenterX, Y = port.CenterY, Task = BuildTask.ToSite, Home = port.Origin, Site = b.Origin, Demolish = true });
                busy.Add(b.Origin); sent++;
            }
            foreach (var g in Ghosts)
            {
                if (sent >= MaxDispatchPerTick) break;
                if (busy.Contains(g.Origin)) continue;
                if (CanBuild != null && !CanBuild(g.Def, g.Origin, g.Rot)) continue;   // blocked for now (someone standing there, a building in the way)
                var net = NetCovering(g.CenterX, g.CenterY);
                if (net == null) continue;
                var port = ReadyBuilderPort(net, g.CenterX, g.CenterY);
                if (port == null) continue;
                // every item of the cost, from the nearest chests that have it unpromised (it may take several stops)
                var plan = new List<(Cell, string, int)>();
                bool enough = true;
                foreach (var cost in g.Def.Cost)
                {
                    int need = cost.Count;
                    while (need > 0)
                    {
                        PlacedBuilding best = null; int avail = 0; double bd = double.MaxValue;
                        bool hub = HubPowered(net);
                        foreach (var list in new[] { net.Providers, net.Storage })
                        {
                            if (list == net.Storage && !hub) continue;
                            foreach (var p in list)
                            {
                                int a = ((StorageState)p.State).Items.Count(cost.ItemId) - (reserved.TryGetValue((p.Origin, cost.ItemId), out var res) ? res : 0);
                                foreach (var (pc, pi, pn) in plan) if (pc.Equals(p.Origin) && pi == cost.ItemId) a -= pn;
                                if (a <= 0) continue;
                                double d = (p.CenterX - g.CenterX) * (p.CenterX - g.CenterX) + (p.CenterY - g.CenterY) * (p.CenterY - g.CenterY);
                                if (d < bd) { bd = d; best = p; avail = a; }
                            }
                        }
                        if (best == null) { enough = false; break; }
                        int take = Math.Min(need, avail);
                        plan.Add((best.Origin, cost.ItemId, take));
                        need -= take;
                    }
                    if (!enough) break;
                }
                if (!enough) continue;   // waits until the materials are in the network
                ((RoboportState)port.State).Robots.TryRemove(ItemDatabase.ConstructionRobot, 1);
                var r = new ConstructionRobot { X = port.CenterX, Y = port.CenterY, Task = BuildTask.Pickup, Home = port.Origin, Site = g.Origin };
                r.Pickups.AddRange(plan);
                foreach (var (c, item, n) in plan) Count(reserved, c, item, n);
                Builders.Add(r);
                busy.Add(g.Origin); sent++;
            }
        }

        bool Fly(ConstructionRobot r, double tx, double ty, float dt)
        {
            var home = grid.At(r.Home);
            float power = home?.State is RoboportState hs ? hs.PowerSatisfaction : 0f;
            double step = RobotSpeed * dt * Math.Max(0.25, power);
            double dx = tx - r.X, dy = ty - r.Y, dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > step) { r.X += dx / dist * step; r.Y += dy / dist * step; return false; }
            r.X = tx; r.Y = ty;
            return true;
        }

        /// <summary>One tick of a construction robot; true when it has landed (and leaves the list).</summary>
        bool MoveBuilder(ConstructionRobot r, float dt)
        {
            switch (r.Task)
            {
                case BuildTask.Pickup:
                {
                    if (r.Pickups.Count == 0) { r.Task = BuildTask.ToSite; return false; }
                    var (cell, item, n) = r.Pickups[0];
                    var chest = grid.At(cell);
                    if (!(chest?.State is StorageState cs)) { r.Pickups.RemoveAt(0); return false; }   // gone: the site check will notice what is missing
                    if (!Fly(r, chest.CenterX, chest.CenterY, dt)) return false;
                    int take = Math.Min(n, cs.Items.Count(item));
                    if (take > 0) { cs.Items.TryRemove(item, take); r.Load(item, take); }
                    r.Pickups.RemoveAt(0);
                    return false;
                }
                case BuildTask.ToSite:
                {
                    if (r.Demolish)
                    {
                        var b = grid.At(r.Site);
                        if (b == null || !Demolish.Contains(b.InstanceId)) { r.Task = BuildTask.Return; return false; }   // unmarked or gone
                        if (!Fly(r, b.CenterX, b.CenterY, dt)) return false;
                        foreach (var c in b.Def.Cost) r.Load(c.ItemId, c.Count);
                        if (b.State != null) foreach (var (_, inv) in b.State.AllInventories) for (int i = 0; i < inv.SlotCount; i++) if (!inv[i].IsEmpty) r.Load(inv[i].ItemId, inv[i].Count);
                        Demolish.Remove(b.InstanceId);
                        grid.Remove(b);
                        r.Task = BuildTask.ToStorage;
                        return false;
                    }
                    var g = GhostAt(r.Site);
                    if (g == null || !g.Origin.Equals(r.Site)) { r.Task = r.Cargo.Count > 0 ? BuildTask.ToStorage : BuildTask.Return; return false; }   // cancelled
                    if (!Fly(r, g.CenterX, g.CenterY, dt)) return false;
                    bool all = true;
                    foreach (var c in g.Def.Cost) if (r.CargoCount(c.ItemId) < c.Count) all = false;
                    if (all && (CanBuild == null || CanBuild(g.Def, g.Origin, g.Rot)) && grid.Place(g.Def, g.Origin, g.Rot) != null)
                    {
                        Ghosts.Remove(g);
                        foreach (var c in g.Def.Cost)   // the cost is used up; anything extra goes to storage
                            for (int i = 0; i < r.Cargo.Count; i++)
                                if (r.Cargo[i].ItemId == c.ItemId) { int left = r.Cargo[i].Count - c.Count; if (left > 0) r.Cargo[i] = new ItemStack(c.ItemId, left); else r.Cargo.RemoveAt(i); break; }
                    }
                    r.Task = r.Cargo.Count > 0 ? BuildTask.ToStorage : BuildTask.Return;   // not built (blocked or short): the ghost waits, the materials go back
                    return false;
                }
                case BuildTask.ToStorage:
                {
                    var net = NetCovering(r.X, r.Y);
                    PlacedBuilding best = null; double bd = double.MaxValue;
                    if (net != null)
                        foreach (var list in new[] { net.Storage, net.Providers })
                        {
                            if (best != null) break;
                            foreach (var p in list)
                            {
                                bool room = false;
                                var inv = ((StorageState)p.State).Items;
                                foreach (var s in r.Cargo) if (inv.Count(s.ItemId) > 0 || HasFreeSlot(inv)) room = true;
                                if (!room) continue;
                                double d = (p.CenterX - r.X) * (p.CenterX - r.X) + (p.CenterY - r.Y) * (p.CenterY - r.Y);
                                if (d < bd) { bd = d; best = p; }
                            }
                        }
                    if (best == null) { r.Task = BuildTask.Return; return false; }   // no room anywhere: the roboport's cargo bay takes it
                    if (!Fly(r, best.CenterX, best.CenterY, dt)) return false;
                    var into = ((StorageState)best.State).Items;
                    for (int i = r.Cargo.Count - 1; i >= 0; i--)
                    {
                        int left = into.Add(r.Cargo[i].ItemId, r.Cargo[i].Count);
                        if (left == 0) r.Cargo.RemoveAt(i); else r.Cargo[i] = new ItemStack(r.Cargo[i].ItemId, left);
                    }
                    if (r.Cargo.Count == 0) r.Task = BuildTask.Return;
                    return false;   // still carrying something: looks for another chest next tick
                }
                default:
                {
                    var port = grid.At(r.Home);
                    if (!(port?.State is RoboportState))
                    {
                        port = NearestRoboport(r.X, r.Y, true) ?? NearestRoboport(r.X, r.Y, false);
                        if (port == null) return false;   // hovers until a roboport exists
                        r.Home = port.Origin;
                    }
                    if (!Fly(r, port.CenterX, port.CenterY, dt)) return false;
                    var rs = (RoboportState)port.State;
                    for (int i = r.Cargo.Count - 1; i >= 0; i--)
                    {
                        int left = rs.Cargo.Add(r.Cargo[i].ItemId, r.Cargo[i].Count);
                        if (left == 0) r.Cargo.RemoveAt(i); else r.Cargo[i] = new ItemStack(r.Cargo[i].ItemId, left);
                    }
                    if (r.Cargo.Count > 0) return false;   // cargo bay full: waits
                    rs.Robots.Add(ItemDatabase.ConstructionRobot, 1);
                    return true;
                }
            }
        }

        static bool HasFreeSlot(Inventory inv) { for (int i = 0; i < inv.SlotCount; i++) if (inv[i].IsEmpty) return true; return false; }

        // ------------------------------------------------------------ save
        public IEnumerable<string> ConstructionSaveLines()
        {
            var inv = CultureInfo.InvariantCulture;
            foreach (var g in Ghosts) yield return $"ghost {g.Def.Id} {g.Origin.X} {g.Origin.Y} {(int)g.Rot}";
            foreach (var id in Demolish) yield return "demolish " + id.ToString(inv);
            foreach (var r in Builders)
            {
                var pk = new StringBuilder(); foreach (var (c, item, n) in r.Pickups) { if (pk.Length > 0) pk.Append(';'); pk.Append(c.X).Append(',').Append(c.Y).Append(',').Append(item).Append(',').Append(n); }
                var cg = new StringBuilder(); foreach (var s in r.Cargo) { if (cg.Length > 0) cg.Append(';'); cg.Append(s.ItemId).Append(',').Append(s.Count); }
                yield return $"crobot {(int)r.Task} {r.X.ToString("R", inv)} {r.Y.ToString("R", inv)} {r.Home.X} {r.Home.Y} {r.Site.X} {r.Site.Y} {(r.Demolish ? 1 : 0)} {(pk.Length > 0 ? pk.ToString() : "-")} {(cg.Length > 0 ? cg.ToString() : "-")}";
            }
        }

        public void LoadConstruction(string[] p)
        {
            var inv = CultureInfo.InvariantCulture;
            switch (p[0])
            {
                case "ghost":
                    var def = BuildingDatabase.TryGet(p[1]);
                    if (def != null) Ghosts.Add(new Ghost { Def = def, Origin = new Cell(int.Parse(p[2], inv), int.Parse(p[3], inv)), Rot = (Rotation)(int.Parse(p[4], inv) & 3) });
                    break;
                case "demolish": Demolish.Add(int.Parse(p[1], inv)); break;
                case "crobot":
                {
                    if (p.Length < 11) return;
                    var r = new ConstructionRobot { Task = (BuildTask)(int.Parse(p[1], inv) & 3), X = double.Parse(p[2], inv), Y = double.Parse(p[3], inv),
                        Home = new Cell(int.Parse(p[4], inv), int.Parse(p[5], inv)), Site = new Cell(int.Parse(p[6], inv), int.Parse(p[7], inv)), Demolish = p[8] == "1" };
                    if (p[9] != "-") foreach (var e in p[9].Split(';')) { var f = e.Split(','); if (f.Length == 4 && ItemDatabase.Exists(f[2])) r.Pickups.Add((new Cell(int.Parse(f[0], inv), int.Parse(f[1], inv)), f[2], int.Parse(f[3], inv))); }
                    if (p[10] != "-") foreach (var e in p[10].Split(';')) { var f = e.Split(','); if (f.Length == 2 && ItemDatabase.Exists(f[0])) r.Cargo.Add(new ItemStack(f[0], Math.Max(0, int.Parse(f[1], inv)))); }
                    Builders.Add(r);
                    break;
                }
            }
        }
    }
}
