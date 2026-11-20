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
        bool IsBuildable(GridPos cell);
    }
}
