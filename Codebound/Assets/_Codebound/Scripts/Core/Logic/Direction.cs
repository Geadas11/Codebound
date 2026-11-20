namespace Codebound.Core
{
    /// <summary>Cardinal facing used for structure rotation. Order matters: clockwise.</summary>
    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public static class DirectionExtensions
    {
        public static Direction RotateClockwise(this Direction d) => (Direction)(((int)d + 1) & 3);
        public static Direction RotateCounterClockwise(this Direction d) => (Direction)(((int)d + 3) & 3);

        /// <summary>True when the facing swaps a footprint's width and height.</summary>
        public static bool IsHorizontal(this Direction d) => d == Direction.East || d == Direction.West;

        public static GridPos ToOffset(this Direction d)
        {
            switch (d)
            {
                case Direction.North: return new GridPos(0, 1);
                case Direction.East: return new GridPos(1, 0);
                case Direction.South: return new GridPos(0, -1);
                default: return new GridPos(-1, 0);
            }
        }

        /// <summary>Clockwise rotation in degrees (North = 0).</summary>
        public static float ToDegrees(this Direction d) => (int)d * 90f;
    }
}
