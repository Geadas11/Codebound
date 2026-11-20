using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    public enum CreatureType : byte { Crawler, Drone }

    public sealed class Creature
    {
        public int Id;
        public CreatureType Type;
        public double X, Y;
        public float Health, MaxHealth, Speed;
        public int TargetBuilding;          // instance id, 0 = none
        public bool ChasingPlayer;
        public float HackProgress;          // drones: seconds spent at a computer
        public float StuckTime;
        public double LastX, LastY;
        public bool Alive => Health > 0;
    }

    /// <summary>
    /// Hostile life. Industry makes noise: drills, generators, furnaces, assemblers and labs raise the base's
    /// "attention"; when it is high enough a wave arrives. A quiet base is left alone (not punishing by default).
    /// Crawlers (alien fauna) attack noisy machines and nearby players; rogue drones go for computers and
    /// compromise them (the program stops). Turrets are programmable; the player's multitool also hurts.
    /// </summary>
    public sealed class CreatureSystem
    {
        public const float AttentionPerWave = 60f, AttentionDecay = 0.2f, MaxCreatures = 40;
        public const float CrawlerHealth = 40, CrawlerSpeed = 2.2f, CrawlerDamage = 8f, CrawlerPlayerDamage = 6f;
        public const float DroneHealth = 30, DroneSpeed = 3f, HackSeconds = 3f;
        public const float TurretRange = 10f, TurretCooldown = 0.6f, TurretDamage = 25f, AggroRange = 5f;

        readonly GameState game;
        readonly Random rng;
        readonly List<Creature> creatures = new List<Creature>();
        int nextId = 1;
        public float Attention;
        public int Waves;
        public bool Enabled = true;   // options: peaceful mode

        public event Action<Creature> Died;
        public event Action<PlacedBuilding> BuildingDestroyed;
        public event Action<PlacedBuilding> ComputerCompromised;
        public event Action<int> WaveArrived;

        public CreatureSystem(GameState g) { game = g; rng = new Random(g.Seed ^ 0x5eed); }

        public IReadOnlyList<Creature> All => creatures;
        public Creature ById(int id) { foreach (var c in creatures) if (c.Id == id) return c; return null; }

        // ------------------------------------------------------------ noise → waves
        /// <summary>How much noise the base makes per second right now.</summary>
        public float Noise()
        {
            float n = 0;
            foreach (var b in game.Grid.Buildings)
            {
                bool busy = game.Factory.HasAction(b);
                switch (b.Def.Kind)
                {
                    case BuildingKind.Drill: if (busy) n += 1f; break;
                    case BuildingKind.Assembler: if (busy) n += 1f; break;
                    case BuildingKind.Furnace: case BuildingKind.Lab: if (busy) n += 0.5f; break;
                    case BuildingKind.Generator: n += ((GeneratorState)b.State).LoadKW / Factory.GeneratorKW * 1.5f; break;
                }
            }
            return n;
        }

        public Creature Spawn(CreatureType type, double x, double y)
        {
            var c = new Creature
            {
                Id = nextId++, Type = type, X = x, Y = y, LastX = x, LastY = y,
                MaxHealth = type == CreatureType.Drone ? DroneHealth : CrawlerHealth,
                Speed = type == CreatureType.Drone ? DroneSpeed : CrawlerSpeed,
            };
            c.Health = c.MaxHealth;
            creatures.Add(c);
            return c;
        }

        void SpawnWave()
        {
            Waves++;
            // the wave heads for the noisiest area: pick a random busy machine
            var noisy = new List<PlacedBuilding>();
            bool anyComputer = false;
            foreach (var b in game.Grid.Buildings)
            {
                if (game.Factory.HasAction(b) || (b.State is GeneratorState g && g.LoadKW > 0)) noisy.Add(b);
                if (b.Def.Kind == BuildingKind.Computer) anyComputer = true;
            }
            if (noisy.Count == 0) return;
            var anchor = noisy[rng.Next(noisy.Count)];
            int size = Math.Min(6, 1 + Waves / 3);
            bool drones = anyComputer && rng.NextDouble() < 0.25;
            for (int i = 0; i < size && creatures.Count < MaxCreatures; i++)
            {
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    double a = rng.NextDouble() * Math.PI * 2, d = 26 + rng.NextDouble() * 8;
                    double x = anchor.CenterX + Math.Cos(a) * d, y = anchor.CenterY + Math.Sin(a) * d;
                    if (Solid(x, y)) continue;
                    Spawn(drones ? CreatureType.Drone : CreatureType.Crawler, x, y);
                    break;
                }
            }
            WaveArrived?.Invoke(size);
        }

        // ------------------------------------------------------------ simulation
        bool Solid(double x, double y)
        {
            var c = new Cell((int)Math.Floor(x), (int)Math.Floor(y));
            if (!WorldRules.IsWalkable(game.World.Generator.TileAt(c))) return true;
            var b = game.Grid.At(c);
            return b != null && !b.Def.Walkable;
        }

        PlacedBuilding FindTarget(Creature c)
        {
            PlacedBuilding best = null; double bestD = double.MaxValue;
            foreach (var b in game.Grid.Buildings)
            {
                bool wanted = c.Type == CreatureType.Drone
                    ? b.Def.Kind == BuildingKind.Computer && b.State is ComputerState cs && !cs.Compromised
                    : !b.Def.Walkable;
                if (!wanted) continue;
                double d = Dist(c.X, c.Y, b.CenterX, b.CenterY);
                if (c.Type == CreatureType.Crawler && !game.Factory.HasAction(b) && !(b.State is GeneratorState)) d += 15;  // prefer noisy machines
                if (d < bestD) { bestD = d; best = b; }
            }
            return best;
        }

        static double Dist(double ax, double ay, double bx, double by) { double dx = ax - bx, dy = ay - by; return Math.Sqrt(dx * dx + dy * dy); }

        public void Tick(float dt, double playerX, double playerY)
        {
            if (!Enabled) { creatures.Clear(); Attention = 0; return; }
            Attention = Math.Max(0, Attention + (Noise() - AttentionDecay) * dt);
            if (Attention >= AttentionPerWave) { Attention -= AttentionPerWave; SpawnWave(); }

            for (int i = creatures.Count - 1; i >= 0; i--)
            {
                var c = creatures[i];
                if (!c.Alive) { creatures.RemoveAt(i); Died?.Invoke(c); continue; }
                TickCreature(c, dt, playerX, playerY);
                if (Dist(c.X, c.Y, playerX, playerY) > 120) creatures.RemoveAt(i);   // wandered too far: despawn
            }
        }

        void TickCreature(Creature c, float dt, double px, double py)
        {
            double tx, ty;
            PlacedBuilding target = c.TargetBuilding != 0 ? game.Grid.ById(c.TargetBuilding) : null;
            c.ChasingPlayer = c.Type == CreatureType.Crawler && Dist(c.X, c.Y, px, py) < AggroRange;
            if (c.ChasingPlayer) { tx = px; ty = py; }
            else
            {
                if (target == null) { target = FindTarget(c); c.TargetBuilding = target?.InstanceId ?? 0; }
                if (target == null) return;
                tx = target.CenterX; ty = target.CenterY;
            }

            double d = Dist(c.X, c.Y, tx, ty);
            double reach = c.ChasingPlayer ? 0.9 : Math.Max(target.Size.w, target.Size.h) / 2.0 + 0.7;
            if (d <= reach)
            {
                if (c.ChasingPlayer) { game.Vitals.Health = Math.Max(0, game.Vitals.Health - CrawlerPlayerDamage * dt); return; }
                if (c.Type == CreatureType.Drone)
                {
                    c.HackProgress += dt;
                    if (c.HackProgress >= HackSeconds && target.State is ComputerState cs)
                    {
                        cs.Compromised = true; cs.Running = false;
                        ComputerCompromised?.Invoke(target);
                        c.Health = 0;   // the drone uploads itself and shuts down
                    }
                    return;
                }
                DamageBuilding(target, CrawlerDamage * dt);
                return;
            }

            // move toward the target, sliding along obstacles
            double step = c.Speed * dt, nx = c.X + (tx - c.X) / d * step, ny = c.Y + (ty - c.Y) / d * step;
            if (!Solid(nx, c.Y)) c.X = nx;
            if (!Solid(c.X, ny)) c.Y = ny;
            if (Dist(c.X, c.Y, c.LastX, c.LastY) < step * 0.2) c.StuckTime += dt; else c.StuckTime = 0;
            c.LastX = c.X; c.LastY = c.Y;
            if (c.StuckTime > 2f)
            {   // blocked: crawlers chew through whatever is in the way, drones hop over it
                var block = game.Grid.At(new Cell((int)Math.Floor(nx), (int)Math.Floor(ny)));
                if (c.Type == CreatureType.Crawler && block != null && !block.Def.Walkable) { c.TargetBuilding = block.InstanceId; c.StuckTime = 0; }
                else if (c.Type == CreatureType.Drone) { c.X = nx; c.Y = ny; c.StuckTime = 0; }
            }
        }

        public void DamageBuilding(PlacedBuilding b, float amount)
        {
            b.Health -= amount;
            if (b.Health > 0) return;
            game.Grid.Remove(b);
            BuildingDestroyed?.Invoke(b);
        }

        /// <summary>Damage from the player's multitool or a turret.</summary>
        public void Hurt(Creature c, float amount) { if (c != null && c.Alive) c.Health = Math.Max(0, c.Health - amount); }

        // ------------------------------------------------------------ turrets
        public List<Creature> InRange(PlacedBuilding turret)
        {
            var list = new List<Creature>();
            foreach (var c in creatures) if (c.Alive && Dist(c.X, c.Y, turret.CenterX, turret.CenterY) <= TurretRange) list.Add(c);
            list.Sort((a, b) => Dist(a.X, a.Y, turret.CenterX, turret.CenterY).CompareTo(Dist(b.X, b.Y, turret.CenterX, turret.CenterY)));
            return list;
        }

        /// <summary>One shot at a creature. Returns an error code or null. Needs power and respects the cooldown.</summary>
        public string Fire(PlacedBuilding turret, int creatureId, double now)
        {
            if (!(turret?.State is TurretState ts)) return "not_a_turret";
            if (now < ts.ReadyAt) return "cooldown";
            if (!game.Factory.HasPower(turret)) return "no_power";
            var c = ById(creatureId);
            if (c == null || !c.Alive) return "no_such_target";
            if (Dist(c.X, c.Y, turret.CenterX, turret.CenterY) > TurretRange) return "out_of_range";
            ts.ReadyAt = now + TurretCooldown; ts.Shots++; ts.LastTarget = c.Id; ts.LastShotTime = now;
            Hurt(c, TurretDamage);
            return null;
        }
    }

    public sealed class TurretState : MachineState
    {
        public double ReadyAt, LastShotTime = -10;
        public int Shots, LastTarget;
    }
}
