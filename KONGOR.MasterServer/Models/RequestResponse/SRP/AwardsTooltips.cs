namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class AwardsTooltips
{
    /// <summary>
    ///     Milestones award.
    /// </summary>
    [PHPProperty("milestones")]
    public MilestonesAwardTooltip Milestones { get; set; } = new();

    /// <summary>
    ///     Leveling award.
    /// </summary>
    [PHPProperty("leveling")]
    public LevelingAwardTooltip Leveling { get; set; } = new();

    /// <summary>
    ///     Bloodlust award.
    /// </summary>
    [PHPProperty("bloodlust")]
    public BloodlustAwardTooltip Bloodlust { get; set; } = new();

    /// <summary>
    ///     Annihilation award.
    /// </summary>
    [PHPProperty("annihilation")]
    public AnnihilationAwardTooltip Annihilation { get; set; } = new();

    /// <summary>
    ///     Immortal award.
    /// </summary>
    [PHPProperty("immortal")]
    public ImmortalAwardTooltip Immortal { get; set; } = new();

    /// <summary>
    ///     Victory award.
    /// </summary>
    [PHPProperty("victory")]
    public VictoryAwardTooltip Victory { get; set; } = new();

    /// <summary>
    ///     Loss award.
    /// </summary>
    [PHPProperty("loss")]
    public LossAwardTooltip Loss { get; set; } = new();

    /// <summary>
    ///     Disconnect award.
    /// </summary>
    [PHPProperty("disco")]
    public DisconnectAwardTooltip Disconnect { get; set; } = new();

    /// <summary>
    ///     Quick match award.
    /// </summary>
    [PHPProperty("quick")]
    public QuickMatchAwardTooltip QuickMatch { get; set; } = new();

    /// <summary>
    ///     First blood award.
    /// </summary>
    [PHPProperty("first")]
    public FirstBloodAwardTooltip FirstBlood { get; set; } = new();

    /// <summary>
    ///     Consecutive wins award.
    /// </summary>
    [PHPProperty("consec_win")]
    public ConsecutiveWinAwardTooltip ConsecutiveWins { get; set; } = new();

    /// <summary>
    ///     Consecutive losses award.
    /// </summary>
    [PHPProperty("consec_loss")]
    public ConsecutiveLossAwardTooltip ConsecutiveLosses { get; set; } = new();
}
