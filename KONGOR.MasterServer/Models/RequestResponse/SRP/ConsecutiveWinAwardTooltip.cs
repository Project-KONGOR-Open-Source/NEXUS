namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class ConsecutiveWinAwardTooltip
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
    public string GoblinCoins { get; set; } = "2-6";
}