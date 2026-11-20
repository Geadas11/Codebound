using System;

namespace Codebound.Core
{
    /// <summary>Integer cell coordinate on the world grid. 1 cell = 1 world unit.</summary>
    [Serializable]
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y) { X = x; Y = y; }

        public static readonly GridPos Zero = new GridPos(0, 0);

        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);
        public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(GridPos a, GridPos b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(GridPos a, GridPos b) => !(a == b);

        /// <summary>Cell that contains the given world-space point.</summary>
        public static GridPos FromWorld(float x, float y) => new GridPos((int)Math.Floor(x), (int)Math.Floor(y));

        public Vec2 Center => new Vec2(X + 0.5f, Y + 0.5f);

        public int ManhattanDistance(GridPos other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        public bool Equals(GridPos other) => this == other;
        public override bool Equals(object obj) => obj is GridPos other && this == other;
        public override int GetHashCode() => unchecked((X * 73856093) ^ (Y * 19349663));
        public override string ToString() => $"({X},{Y})";
    }
}
