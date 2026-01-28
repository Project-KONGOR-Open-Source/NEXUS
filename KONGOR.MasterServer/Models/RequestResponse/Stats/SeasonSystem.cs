namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class SeasonSystem
{
    /// <summary>
    ///     Number of diamonds earned/dropped from the match.
    ///     Calculated based on drop probability.
    /// </summary>
    [PHPProperty("drop_diamonds")]
    public int DropDiamonds { get; init; } = 0;

    /// <summary>
    ///     Current total diamonds the account has accumulated this season.
    /// </summary>
    [PHPProperty("cur_diamonds")]
    public int TotalDiamonds { get; init; } = 0;

    /// <summary>
    ///     Seasonal shop loot box prices and information.
    /// </summary>
    [PHPProperty("box_price")]
    public Dictionary<int, int> BoxPrice { get; init; } = [];
}
