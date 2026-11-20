namespace Codebound.Core
{
    /// <summary>Answers whether a moving body may occupy a cell (terrain + structures).</summary>
    public interface IWalkabilityQuery
    {
        bool IsWalkable(GridPos cell);
    }

    /// <summary>Answers whether terrain at a cell allows building (ignores existing structures).</summary>
    public interface ITerrainBuildQuery
    {
        /// <summary>Buildable terrain with no resource deposit on it.</summary>
        bool IsBuildable(GridPos cell);
        /// <summary>Buildable terrain, whether or not a resource deposit is on it (used by drills).</summary>
        bool IsBuildableTerrain(GridPos cell);
        bool HasResource(GridPos cell);
    }
}
