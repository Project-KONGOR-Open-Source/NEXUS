namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class VictoryAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 30;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 6;
}
