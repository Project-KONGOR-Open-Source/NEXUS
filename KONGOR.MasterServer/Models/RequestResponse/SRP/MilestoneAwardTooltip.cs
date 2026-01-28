namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class MilestoneAwardTooltip
{
    /// <summary>
    ///     The name of the milestone.
    /// </summary>
    [PHPProperty("aname")]
    public required string AwardName { get; set; }

    /// <summary>
    ///     The value of the milestone in experience.
    /// </summary>
    [PHPProperty("exp")]
    public required string Experience { get; set; }

    /// <summary>
    ///     The value of the milestone in goblin coins.
    /// </summary>
    [PHPProperty("gc")]
    public required string GoblinCoins { get; set; }

    /// <summary>
    ///     The modulus used to determine the frequency of reaching the milestone, e.g. "10" would mean that the milestone is
    ///     reached every 10 ticks.
    /// </summary>
    [PHPProperty("modulo")]
    public required string Modulo { get; set; }
}
