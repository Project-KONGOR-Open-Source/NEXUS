namespace KONGOR.MasterServer.Models.RequestResponse.Stats;

/// <summary>
///     Response model for the "get_player_award_summ" request.
/// </summary>
public class GetPlayerAwardSummaryResponse
{
    /// <summary>
    ///     The account ID.
    /// </summary>
    [PHPProperty("account_id")]
    public required string AccountID { get; set; }

    /// <summary>
    ///     Most Valuable Player awards.
    /// </summary>
    [PHPProperty("mvp")]
    public required string MVPAwards { get; set; }

    /// <summary>
    ///     Annihilation (5 kills in quick succession) awards.
    /// </summary>
    [PHPProperty("awd_mann")]
    public required string AnnihilationAwards { get; set; }

    /// <summary>
    ///     Quad Kill (4 kills in quick succession) awards.
    /// </summary>
    [PHPProperty("awd_mqk")]
    public required string QuadKillAwards { get; set; }

    /// <summary>
    ///     Longest Kill Streak awards.
    /// </summary>
    [PHPProperty("awd_lgks")]
    public required string LongestKillStreakAwards { get; set; }

    /// <summary>
    ///     Smackdown (killing a player after taunting them) awards.
    /// </summary>
    [PHPProperty("awd_msd")]
    public required string SmackdownAwards { get; set; }

    /// <summary>
    ///     Most Kills awards.
    /// </summary>
    [PHPProperty("awd_mkill")]
    public required string MostKillsAwards { get; set; }

    /// <summary>
    ///     Most Assists awards.
    /// </summary>
    [PHPProperty("awd_masst")]
    public required string MostAssistsAwards { get; set; }

    /// <summary>
    ///     Least Deaths awards.
    /// </summary>
    [PHPProperty("awd_ledth")]
    public required string LeastDeathsAwards { get; set; }

    /// <summary>
    ///     Most Building Damage awards.
    /// </summary>
    [PHPProperty("awd_mbdmg")]
    public required string MostBuildingDamageAwards { get; set; }

    /// <summary>
    ///     Most Wards Destroyed awards.
    /// </summary>
    [PHPProperty("awd_mwk")]
    public required string MostWardsDestroyedAwards { get; set; }

    /// <summary>
    ///     Most Hero Damage Dealt awards.
    /// </summary>
    [PHPProperty("awd_mhdd")]
    public required string MostHeroDamageDealtAwards { get; set; }

    /// <summary>
    ///     Highest Creep Score awards.
    /// </summary>
    [PHPProperty("awd_hcs")]
    public required string HighestCreepScoreAwards { get; set; }

    /// <summary>
    ///     The minimum number of matches a free-to-play (trial) account must complete to become verified.
    ///     A verified account is considered to have full account privileges, and is no longer considered a restricted account.
    /// </summary>
    [PHPProperty("vested_threshold")]
    public int VestedThreshold => 5;

    /// <summary>
    ///     Unknown.
    ///     <br/>
    ///     Seems to be set to "true" on a successful response, or to "false" if an error occurs.
    /// </summary>
    [PHPProperty(0)]
    public bool Zero => true;
}
