using System;
using Codebound.Core;

namespace Codebound.Player
{
    /// <summary>
    /// Top-down character movement with axis-separated sliding collision against the cell grid.
    /// The body is an axis-aligned square of side 2*<see cref="HalfSize"/> centred on <see cref="Position"/>.
    /// Engine independent so it can be simulated identically on a multiplayer host.
    /// </summary>
    public sealed class PlayerMotor
    {
        public const float DefaultSpeed = 5f;
        public const float DefaultHalfSize = 0.3f;
        private const float Skin = 0.001f;
        private const float MaxStep = 0.25f; // never move more than this per sub-step (prevents tunnelling)

        public PlayerMotor(Vec2 position, float speed = DefaultSpeed, float halfSize = DefaultHalfSize)
        {
            if (halfSize <= 0 || halfSize >= 0.5f) throw new ArgumentOutOfRangeException(nameof(halfSize));
            Position = position;
            Speed = speed;
            HalfSize = halfSize;
        }

        public Vec2 Position { get; private set; }
        public float Speed { get; set; }
        public float HalfSize { get; }
        public Direction Facing { get; private set; } = Direction.South;
        public Vec2 LastVelocity { get; private set; }

        public void Teleport(Vec2 p) => Position = p;

        /// <summary>Moves according to <paramref name="input"/> (any length; clamped to 1) for <paramref name="dt"/> seconds.</summary>
        public void Move(Vec2 input, float dt, IWalkabilityQuery world)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (dt <= 0) return;
            var dir = input.ClampLength(1f);
            var start = Position;
            if (dir.Length > 1e-4f) Facing = FacingFrom(dir);

            var delta = dir * (Speed * dt);
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) / MaxStep));
            float sx = delta.X / steps, sy = delta.Y / steps;
            for (int i = 0; i < steps; i++)
            {
                if (sx != 0) TryAxis(sx, 0, world);
                if (sy != 0) TryAxis(0, sy, world);
            }
            LastVelocity = (Position - start) * (1f / dt);
        }

        /// <summary>True if the body would overlap no blocked cell at <paramref name="p"/>.</summary>
        public bool Fits(Vec2 p, IWalkabilityQuery world)
        {
            int minX = (int)Math.Floor(p.X - HalfSize + Skin), maxX = (int)Math.Floor(p.X + HalfSize - Skin);
            int minY = (int)Math.Floor(p.Y - HalfSize + Skin), maxY = (int)Math.Floor(p.Y + HalfSize - Skin);
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (!world.IsWalkable(new GridPos(x, y))) return false;
            return true;
        }

        private void TryAxis(float dx, float dy, IWalkabilityQuery world)
        {
            var target = new Vec2(Position.X + dx, Position.Y + dy);
            if (Fits(target, world)) { Position = target; return; }

            // Blocked: slide flush against the obstacle edge on this axis.
            if (dx != 0)
            {
                float edge = dx > 0
                    ? (float)Math.Floor(Position.X + HalfSize + dx) - HalfSize - Skin
                    : (float)Math.Floor(Position.X - HalfSize + dx) + 1 + HalfSize + Skin;
                var flush = new Vec2(edge, Position.Y);
                if ((dx > 0 ? edge > Position.X : edge < Position.X) && Fits(flush, world)) Position = flush;
            }
            else
            {
                float edge = dy > 0
                    ? (float)Math.Floor(Position.Y + HalfSize + dy) - HalfSize - Skin
                    : (float)Math.Floor(Position.Y - HalfSize + dy) + 1 + HalfSize + Skin;
                var flush = new Vec2(Position.X, edge);
                if ((dy > 0 ? edge > Position.Y : edge < Position.Y) && Fits(flush, world)) Position = flush;
            }
        }

        private static Direction FacingFrom(Vec2 d) =>
            Math.Abs(d.X) > Math.Abs(d.Y) ? (d.X > 0 ? Direction.East : Direction.West)
                                          : (d.Y > 0 ? Direction.North : Direction.South);
    }
}
