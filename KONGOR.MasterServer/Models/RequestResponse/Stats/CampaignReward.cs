namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class CampaignReward
{
    /// <summary>
    ///     The previous level of the player in the Con (Champions of Newerth) campaign.
    /// </summary>
    [PhpProperty("old_lvl")]
    public string OldLevel { get; init; } = "0";

    /// <summary>
    ///     The current level of the player in the Con (Champions of Newerth) campaign.
    /// </summary>
    [PhpProperty("curr_lvl")]
    public string CurrentLevel { get; init; } = "3";

    /// <summary>
    ///     The next level of the player in the Con (Champions of Newerth) campaign.
    /// </summary>
    [PhpProperty("next_lvl")]
    public string NextLevel { get; init; } = "4";

    /// <summary>
    ///     The rank required to level up in the Con (Champions of Newerth) campaign.
    /// </summary>
    [PhpProperty("require_rank")]
    public string RequiredRank { get; init; } = "15";

    /// <summary>
    ///     The number of additional matches the player needs to play to level up in the Con (Champions of Newerth) campaign.
    /// </summary>
    [PhpProperty("need_more_play")]
    public string MatchesNeededToLevelUp { get; init; } = "4";

    /// <summary>
    ///     The percentage progress towards the next level in the Con (Champions of Newerth) campaign.
    /// </summary>
    [PhpProperty("percentage")]
    public string Percentage { get; init; } = "0.00";

    /// <summary>
    ///     The percentage progress towards the next level in the Con (Champions of Newerth) campaign before the current match.
    /// </summary>
    [PhpProperty("percentage_before")]
    public string PercentageBefore { get; init; } = "0.00";
}