using System;
using System.Collections.Generic;
using System.Globalization;

namespace Codebound.Core
{
    public enum PlanetKind : byte { Home, Volcanic, Desert, Ruins, Underground }

    /// <summary>A world of the star system. Each has its own seed and biome bias; all derive from the home seed.</summary>
    public sealed class Planet
    {
        public string Id, Name;
        public PlanetKind Kind;
        public int Seed;
        public float DistanceAU;
    }

    public static class StarSystem
    {
        static readonly string[] Names = { "Cinder", "Dune", "Hollow", "Ember", "Mirage", "Vestige", "Pyre", "Sable", "Relic" };

        public static List<Planet> Of(int homeSeed)
        {
            var l = new List<Planet> { new Planet { Id = "home", Name = "Kepler Landing", Kind = PlanetKind.Home, Seed = homeSeed, DistanceAU = 0 } };
            var kinds = new[] { PlanetKind.Volcanic, PlanetKind.Desert, PlanetKind.Ruins };
            for (int i = 0; i < kinds.Length; i++)
            {
                double h = WorldGenerator.Hash01(i, 77, homeSeed + 5150);
                l.Add(new Planet
                {
                    Id = kinds[i].ToString().ToLowerInvariant(),
                    Name = Names[i + 3 * (int)(h * 3)],   // Cinder/Ember/Pyre volcanic, Dune/Mirage/Sable desert, Hollow/Vestige/Relic ruins
                    Kind = kinds[i],
                    Seed = unchecked(homeSeed * 31 + 1000003 * (i + 1)),
                    DistanceAU = (float)Math.Round(0.4 + i * 0.5 + h * 0.4, 2),
                });
            }
            return l;
        }

        public static Planet Get(int homeSeed, string id) => Of(homeSeed).Find(p => p.Id == id);
    }

    /// <summary>
    /// The ascent to orbit, driven by a program setting the throttle. A gravity turn is flown automatically;
    /// dense air heats the hull with speed³, so flooring it low down burns the heat shield, while being too timid
    /// runs out of fuel. Orbit: 120 km at 7.5 km/s. Failure is safe: the capsule parachutes back to the pad.
    /// </summary>
    public sealed class Ascent
    {
        public const float TargetAltitude = 120f, TargetVelocity = 7.5f, MaxFuel = 250f;
        const float Accel = 0.05f, Burn = 1f, Drag = 0.01f, HeatK = 50f, Cooling = 2f, Radius = 6000f, G0 = 0.0098f;

        public float Altitude, Velocity, Fuel, Heat, Time, Throttle;
        public string Phase = "ascent", Reason;   // ascent | orbit | aborted

        public void Step(float dt)
        {
            if (Phase != "ascent") return;
            float thr = Fuel > 0 ? Math.Max(0f, Math.Min(1f, Throttle)) : 0f;
            double gamma = Math.PI / 2 * Math.Max(0.0, 1.0 - Altitude / (TargetAltitude + 10f));
            double rho = Math.Exp(-Altitude / 8.0);
            double g = G0 * Math.Pow(Radius / (Radius + Altitude), 2);
            double a = thr * Accel - g * Math.Sin(gamma) - Drag * rho * Velocity * Velocity;
            Velocity = (float)Math.Max(0, Velocity + a * dt);
            Altitude = (float)Math.Max(0, Altitude + Velocity * Math.Sin(gamma) * dt);
            Heat = (float)Math.Max(0, Heat + (HeatK * rho * Math.Pow(Velocity, 3) - Cooling) * dt);
            Fuel = Math.Max(0f, Fuel - thr * Burn * dt);
            Time += dt;
            if (Heat >= 100) { Phase = "aborted"; Reason = "heat shield overheated in the dense air — throttle down low in the atmosphere"; }
            else if (Altitude >= TargetAltitude && Velocity >= TargetVelocity) Phase = "orbit";
            else if (Fuel <= 0 && Time > 5 && (Altitude >= TargetAltitude || Velocity * Math.Sin(gamma) <= 0.001)) { Phase = "aborted"; Reason = "out of fuel before orbit — load more fuel or waste less"; }
            else if (Time > 1500) { Phase = "aborted"; Reason = "ascent took too long"; }
        }

        public Dictionary<string, object> Info() => new Dictionary<string, object>
        {
            { "phase", Phase }, { "altitude", Math.Round(Altitude, 2) }, { "velocity", Math.Round(Velocity, 3) }, { "heat", Math.Round(Heat, 1) },
            { "fuel", Math.Round(Fuel, 1) }, { "time", Math.Round(Time, 1) }, { "throttle", Math.Round(Throttle, 3) },
            { "targetAltitude", TargetAltitude }, { "targetVelocity", TargetVelocity }, { "reason", Reason },
        };
    }

    /// <summary>Flying from orbit to another world of the system. The player's things travel; each world keeps its own base.</summary>
    public static class Voyage
    {
        /// <summary>Fuel units for the trip: 20 to leave orbit plus 30 per AU between the two worlds.</summary>
        public static float Cost(GameState from, Planet to) => Math.Abs(to.DistanceAU - (StarSystem.Get(from.HomeSeed, from.PlanetId)?.DistanceAU ?? 0)) * 30 + 20;

        /// <summary>Null when the pad can fly to `to`, otherwise why not.</summary>
        public static string Check(GameState from, PlacedBuilding pad, Planet to)
        {
            if (!(pad?.State is LaunchPadState lp) || lp.Phase != "orbit" || lp.Flight == null) return "the ship must be in orbit first";
            if (to == null) return "unknown planet (see pad.planets())";
            if (to.Id == from.PlanetId) return "already orbiting this planet";
            float cost = Cost(from, to);
            if (lp.Flight.Fuel < cost) return $"needs {cost:0} fuel units, the tanks hold {lp.Flight.Fuel:0}";
            return null;
        }

        /// <summary>What travels with the player between worlds and layers: inventory, research, vitals, skills, HUD, clock.</summary>
        public static void Carry(GameState from, GameState t)
        {
            t.Inventory.Clear();
            for (int i = 0; i < from.Inventory.SlotCount; i++) if (!from.Inventory[i].IsEmpty) t.Inventory.Add(from.Inventory[i].ItemId, from.Inventory[i].Count);
            foreach (var id in from.Research.Done) t.Research.Done.Add(id);
            t.Vitals.Load(from.Vitals.SaveLine().Split(' '));
            t.Skills.CopyFrom(from.Skills);
            foreach (var p in new List<Hud.HudPanel>(t.Hud.Panels)) t.Hud.Remove(p);
            foreach (var line in from.Hud.SaveLines()) t.Hud.LoadLine(line.Split(' '));
            t.Clock.Seconds = Math.Max(t.Clock.Seconds, from.Clock.Seconds);
        }

        /// <summary>Lands on `to` (a world visited before, or a new one when `saved` is null) and returns it.
        /// The ship leaves `from`; inventory, research, vitals, HUD and clock come along; the leftover fuel stays in the tanks.</summary>
        public static GameState Arrive(GameState from, PlacedBuilding pad, Planet to, GameState saved)
        {
            var lp = (LaunchPadState)pad.State;
            float cost = Cost(from, to);
            float left = Math.Max(0, lp.Flight.Fuel - cost);
            from.Grid.Remove(pad);

            var t = saved ?? new GameState(to.Seed, to.Kind) { PlanetId = to.Id, HomeSeed = from.HomeSeed };
            Carry(from, t);

            // touch down near the spawn: first free 3×3 spot
            var s = t.World.Generator.FindSpawn();
            var def = BuildingDatabase.Get("launchpad");
            PlacedBuilding landed = null;
            for (int r = 2; r < 80 && landed == null; r++)
                for (int x = -r; x <= r && landed == null; x++)
                    foreach (int y in new[] { -r, r })
                        if (landed == null && t.CanPlace(def, new Cell(s.X + x, s.Y + y), Rotation.R0))
                            landed = t.Grid.Place(def, new Cell(s.X + x, s.Y + y), Rotation.R0);
            if (landed != null)
            {
                var nl = (LaunchPadState)landed.State;
                nl.Reserve = left; nl.LastResult = "landed on " + to.Name;
                t.PlayerX = landed.Origin.X + 1.5; t.PlayerY = landed.Origin.Y - 1.5;
            }
            else { t.PlayerX = s.X + 0.5; t.PlayerY = s.Y + 0.5; }
            return t;
        }
    }

    /// <summary>The rebuilt ship on its launch pad. Fuel: coal 4 units, crude oil 10 units, up to 250.</summary>
    public sealed class LaunchPadState : MachineState
    {
        public readonly Inventory FuelBay = new Inventory(8);
        public float Reserve;          // fuel units already in the tanks (left over from the last trip)
        public Ascent Flight;          // null on the ground
        public string Phase = "ready"; // ready | ascent | orbit
        public string LastResult;
        public bool Underground;   // set by the factory: a pad in a cave cannot launch

        public LaunchPadState() { Status = "ready"; }

        public static int UnitsOf(string item) => item == ItemDatabase.Coal ? 4 : item == ItemDatabase.Oil ? 10 : 0;
        public float FuelUnits { get { float u = Reserve; for (int i = 0; i < FuelBay.SlotCount; i++) if (!FuelBay[i].IsEmpty) u += UnitsOf(FuelBay[i].ItemId) * FuelBay[i].Count; return Math.Min(Ascent.MaxFuel, u); } }

        public override IEnumerable<(string role, Inventory inv)> Inventories { get { yield return ("fuel", FuelBay); } }
        public override bool TryInsert(string itemId, int count) => UnitsOf(itemId) > 0 && FuelBay.TryAddAll(new[] { new ItemStack(itemId, count) });

        /// <summary>Starts the ascent. Returns null or why not.</summary>
        public string Launch(Research r)
        {
            if (Phase != "ready") return "already flying";
            if (r.MainLineDone < r.MainLineTotal) return "the ship is not rebuilt yet (research the main line)";
            if (Underground) return "cannot launch from underground";
            float units = FuelUnits;
            if (units < 50) return "needs at least 50 fuel units (coal = 4, crude oil = 10)";
            FuelBay.Clear(); Reserve = 0;
            Flight = new Ascent { Fuel = units };
            Phase = "ascent"; LastResult = null;
            return null;
        }

        public override void SaveExtra(IDictionary<string, string> d)
        {
            var inv = CultureInfo.InvariantCulture;
            d["phase"] = Phase;
            d["reserve"] = Reserve.ToString("R", inv);
            if (Flight != null)
                d["flight"] = string.Join(";", Array.ConvertAll(new[] { Flight.Altitude, Flight.Velocity, Flight.Fuel, Flight.Heat, Flight.Time, Flight.Throttle }, x => x.ToString("R", inv)));
        }

        public override void LoadExtra(string k, string v)
        {
            var inv = CultureInfo.InvariantCulture;
            if (k == "phase") { Phase = v; Status = v; }
            else if (k == "reserve") Reserve = float.Parse(v, inv);
            else if (k == "flight")
            {
                var p = v.Split(';');
                Flight = new Ascent { Altitude = float.Parse(p[0], inv), Velocity = float.Parse(p[1], inv), Fuel = float.Parse(p[2], inv),
                                      Heat = float.Parse(p[3], inv), Time = float.Parse(p[4], inv), Throttle = float.Parse(p[5], inv) };
                if (Phase == "orbit") Flight.Phase = "orbit";
            }
        }
    }
}
