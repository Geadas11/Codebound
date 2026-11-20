namespace Codebound.Game
{
    /// <summary>Top-down depth sorting: things lower on screen draw in front.</summary>
    public static class SortingOrder
    {
        public const int Terrain = -32000;
        public const int Floor = -31000;
        public const int Highlight = 31000;
        public const int Ghost = 31500;

        public static int ForY(float worldY) => 10000 - (int)(worldY * 10f);
    }
}
