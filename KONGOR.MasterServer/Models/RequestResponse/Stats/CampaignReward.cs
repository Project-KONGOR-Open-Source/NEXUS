namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

public class CampaignReward
{
    /// <summary>
    ///     Champions Of Newerth reward level before the match.
    ///     Set to "-2" if no previous match data exists.
    /// </summary>
    [PhpProperty("old_lvl")]
    public int PreviousCampaignLevel { get; init; } = 5;

    /// <summary>
    ///     Current Champions Of Newerth reward level after the match.
    ///     Maximum level is "6".
    /// </summary>
    [PhpProperty("curr_lvl")]
    public int CurrentCampaignLevel { get; init; } = 6;

    /// <summary>
    ///     Next Champions Of Newerth reward level to unlock.
    ///     Set to "0" when maximum level ("6") has been reached.
    /// </summary>
    [PhpProperty("next_lvl")]
    public int NextLevel { get; init; } = 0;

    /// <summary>
    ///     Minimum medal rank required to unlock the next Champions Of Newerth reward level.
    ///     <code>
    ///         Level 1 -> Medal 01 (Bronze V)
    ///         Level 2 -> Medal 06 (Silver V)
    ///         Level 3 -> Medal 11 (Gold V)
    ///         Level 4 -> Medal 15 (Diamond V)
    ///         Level 5 -> Medal 18 (Diamond II)
    ///         Level 6 -> Medal 20 (Immortal)
    ///     </code>
    ///     Set to "0" if the rank requirement is already met or if maximum level has been reached.
    /// </summary>
    [PhpProperty("require_rank")]
    public int RequireRank { get; init; } = 0;

    /// <summary>
    ///     Number of additional matches needed to accumulate enough reward points to reach the next Champions Of Newerth
    ///     level.
    ///     Each level requires "12" reward points, earned from winning matches.
    ///     Set to "0" when maximum level has been reached.
    /// </summary>
    [PhpProperty("need_more_play")]
    public int NeedMorePlay { get; init; } = 0;

    /// <summary>
    ///     Progress percentage towards the next Champions Of Newerth reward level before the match.
    ///     Calculated as "reward_points" divided by 12, formatted as a decimal string (e.g. "0.75" for 75%).
    /// </summary>
    [PhpProperty("percentage_before")]
    public string PercentageBefore { get; init; } = "0.92";

    /// <summary>
    ///     Progress percentage towards the next Champions Of Newerth reward level after the match.
    ///     Calculated as "reward_points" divided by 12, formatted as a decimal string (e.g. "1.00" for 100%).
    /// </summary>
    [PhpProperty("percentage")]
    public string Percentage { get; init; } = "1.00";
}