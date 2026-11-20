using System;

namespace Codebound.Core
{
    /// <summary>Integer grid coordinate. 1 cell = 1 world unit.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int X;
        public readonly int Y;

        public Cell(int x, int y) { X = x; Y = y; }

        public static Cell operator +(Cell a, Cell b) => new Cell(a.X + b.X, a.Y + b.Y);
        public static bool operator ==(Cell a, Cell b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Cell a, Cell b) => !(a == b);

        public bool Equals(Cell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Cell c && Equals(c);
        public override int GetHashCode() => unchecked((X * 73856093) ^ (Y * 19349663));
        public override string ToString() => $"({X},{Y})";

        /// <summary>Floor division so negative coordinates map to the right chunk.</summary>
        public static int FloorDiv(int a, int b) => a >= 0 ? a / b : ((a + 1) / b) - 1;
    }

    /// <summary>Clockwise quarter turns.</summary>
    public enum Rotation : byte { R0 = 0, R90 = 1, R180 = 2, R270 = 3 }

    public static class RotationExt
    {
        public static Rotation Next(this Rotation r) => (Rotation)(((int)r + 1) & 3);
        public static bool IsSideways(this Rotation r) => r == Rotation.R90 || r == Rotation.R270;
    }
}
