namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class AnnihilationAwardTooltip
{
    /// <summary>
    ///     Experience reward.
    /// </summary>
    [PhpProperty("exp")]
    public int Experience { get; set; } = 75;

    /// <summary>
    ///     Goblin coins reward.
    /// </summary>
    [PhpProperty("gc")]
    public int GoblinCoins { get; set; } = 15;

    /// <summary>
    ///     Team experience reward.
    /// </summary>
    [PhpProperty("tm_exp")]
    public int TeamExperience { get; set; } = 25;

    /// <summary>
    ///     Team goblin coins reward.
    /// </summary>
    [PhpProperty("tm_gc")]
    public int TeamGoblinCoins { get; set; } = 5;
}