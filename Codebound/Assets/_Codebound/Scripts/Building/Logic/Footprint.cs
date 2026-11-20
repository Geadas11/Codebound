using System.Collections.Generic;
using Codebound.Core;

namespace Codebound.Building
{
    /// <summary>Footprint math for multi-cell, rotatable structures.</summary>
    public static class Footprint
    {
        /// <summary>Width/height after applying the facing (East/West swap the axes).</summary>
        public static (int w, int h) RotatedSize(int width, int height, Direction facing) =>
            facing.IsHorizontal() ? (height, width) : (width, height);

        /// <summary>All cells covered by a footprint whose bottom-left cell is <paramref name="origin"/>.</summary>
        public static List<GridPos> Cells(GridPos origin, int width, int height, Direction facing)
        {
            var (w, h) = RotatedSize(width, height, facing);
            var cells = new List<GridPos>(w * h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                cells.Add(new GridPos(origin.X + x, origin.Y + y));
            return cells;
        }

        /// <summary>Origin that centres the rotated footprint on <paramref name="cursor"/> (rounding toward bottom-left).</summary>
        public static GridPos OriginCenteredOn(GridPos cursor, int width, int height, Direction facing)
        {
            var (w, h) = RotatedSize(width, height, facing);
            return new GridPos(cursor.X - (w - 1) / 2, cursor.Y - (h - 1) / 2);
        }
    }
}
