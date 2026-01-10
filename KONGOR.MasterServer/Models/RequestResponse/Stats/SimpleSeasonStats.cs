namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class SimpleSeasonStats
{
    /// <summary>
    ///     The number of ranked matches won.
    /// </summary>
    [PhpProperty("wins")]
    public required int RankedMatchesWon { get; set; }

    /// <summary>
    ///     The number of ranked matches lost.
    /// </summary>
    [PhpProperty("losses")]
    public required int RankedMatchesLost { get; set; }

    /// <summary>
    ///     The current number of consecutive ranked matches won.
    /// </summary>
    [PhpProperty("win_streak")]
    public required int WinStreak { get; set; }

    /// <summary>
    ///     Whether the account needs to play placement matches or not.
    ///     A value of "1" means TRUE, and a value of "0" means FALSE.
    /// </summary>
    [PhpProperty("is_placement")]
    public required int InPlacementPhase { get; set; }

    /// <summary>
    ///     Unknown.
    ///     Potentially, the number of account levels gained during the season.
    /// </summary>
    [PhpProperty("current_level")]
    public required int LevelsGainedThisSeason { get; set; }
}