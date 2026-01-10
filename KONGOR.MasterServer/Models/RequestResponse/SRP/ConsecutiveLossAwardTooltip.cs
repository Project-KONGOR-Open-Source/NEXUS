namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class ConsecutiveLossAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PhpProperty("exp")]
    public int Experience { get; set; } = 0;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PhpProperty("gc")]
    public int GoblinCoins { get; set; } = 1;
}