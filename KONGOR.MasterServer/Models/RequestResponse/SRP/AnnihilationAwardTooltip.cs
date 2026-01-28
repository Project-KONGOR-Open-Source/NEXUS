namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class AnnihilationAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PHPProperty("exp")]
    public int Experience { get; set; } = 75;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PHPProperty("gc")]
    public int GoblinCoins { get; set; } = 15;

    /// <summary>
    ///     Team experience reward.
    /// </summary>
    [PHPProperty("tm_exp")]
    public int TeamExperience { get; set; } = 25;

    /// <summary>
    ///     Team goblin coins reward.
    /// </summary>
    [PHPProperty("tm_gc")]
    public int TeamGoblinCoins { get; set; } = 5;
}
