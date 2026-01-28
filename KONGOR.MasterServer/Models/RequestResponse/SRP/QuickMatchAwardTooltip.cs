namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class QuickMatchAwardTooltip
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
    public int GoblinCoins { get; set; } = 2;
}
