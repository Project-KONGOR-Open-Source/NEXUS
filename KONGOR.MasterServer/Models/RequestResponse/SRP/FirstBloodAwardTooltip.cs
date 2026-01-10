namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class FirstBloodAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PhpProperty("exp")]
    public int Experience { get; set; } = 20;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PhpProperty("gc")]
    public int GoblinCoins { get; set; } = 4;
}