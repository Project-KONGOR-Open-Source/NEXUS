namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class BloodlustAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 10;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 2;
}
