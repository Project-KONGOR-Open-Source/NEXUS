namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class ConsecutiveWinAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 0;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public string GoblinCoins { get; set; } = "2-6";
}
