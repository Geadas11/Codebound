namespace Codebound.World
{
    public enum TerrainType : byte
    {
        Grass,
        Dirt,
        Sand,
        Rock,       // impassable natural rock
        Water,      // impassable for now (swimming/bridges come later)
        CrashSite   // scorched ground around the crashed ship
    }

    public static class TerrainTypeExtensions
    {
        public static bool IsWalkable(this TerrainType t) => t != TerrainType.Rock && t != TerrainType.Water;
        public static bool IsBuildable(this TerrainType t) => t != TerrainType.Rock && t != TerrainType.Water;
    }
}
