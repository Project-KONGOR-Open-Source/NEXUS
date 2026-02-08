namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Individual hero statistics for a specific game mode.
/// </summary>
public class HeroStatistics
{
    /// <summary>
    ///     The identifier of the hero, in the format Hero_{Snake_Case_Name} (e.g. "Hero_Armadon").
    /// </summary>
    [PHPProperty("cli_name")]
    public required string HeroIdentifier { get; set; }

    /// <summary>
    ///     The number of times the hero has been used.
    /// </summary>
    [PHPProperty("used")]
    public required string TimesUsed { get; set; }

    /// <summary>
    ///     The number of wins with this hero.
    /// </summary>
    [PHPProperty("wins")]
    public required string Wins { get; set; }

    /// <summary>
    ///     The number of losses with this hero.
    /// </summary>
    [PHPProperty("losses")]
    public required string Losses { get; set; }

    /// <summary>
    ///     The total number of hero kills.
    /// </summary>
    [PHPProperty("herokills")]
    public required string HeroKills { get; set; }

    /// <summary>
    ///     The total number of deaths.
    /// </summary>
    [PHPProperty("deaths")]
    public required string Deaths { get; set; }

    /// <summary>
    ///     The total number of hero assists.
    /// </summary>
    [PHPProperty("heroassists")]
    public required string HeroAssists { get; set; }

    /// <summary>
    ///     The total number of team creep kills.
    /// </summary>
    [PHPProperty("teamcreepkills")]
    public required string TeamCreepKills { get; set; }

    /// <summary>
    ///     The total number of denies.
    /// </summary>
    [PHPProperty("denies")]
    public required string Denies { get; set; }

    /// <summary>
    ///     The total experience earned.
    /// </summary>
    [PHPProperty("exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The total gold earned.
    /// </summary>
    [PHPProperty("gold")]
    public required string Gold { get; set; }

    /// <summary>
    ///     The total number of actions performed.
    /// </summary>
    [PHPProperty("actions")]
    public required string Actions { get; set; }

    /// <summary>
    ///     The total time spent earning experience.
    /// </summary>
    [PHPProperty("time_earning_exp")]
    public required string TimeEarningExperience { get; set; }
}
