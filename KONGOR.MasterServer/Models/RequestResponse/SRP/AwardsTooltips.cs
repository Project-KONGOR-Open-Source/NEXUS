namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class AwardsTooltips
{
    /// <summary>
    ///     Milestones award.
    /// </summary>
    [PhpProperty("milestones")]
    public MilestonesAwardTooltip Milestones { get; set; } = new();

    /// <summary>
    ///     Leveling award.
    /// </summary>
    [PhpProperty("leveling")]
    public LevelingAwardTooltip Leveling { get; set; } = new();

    /// <summary>
    ///     Bloodlust award.
    /// </summary>
    [PhpProperty("bloodlust")]
    public BloodlustAwardTooltip Bloodlust { get; set; } = new();

    /// <summary>
    ///     Annihilation award.
    /// </summary>
    [PhpProperty("annihilation")]
    public AnnihilationAwardTooltip Annihilation { get; set; } = new();

    /// <summary>
    ///     Immortal award.
    /// </summary>
    [PhpProperty("immortal")]
    public ImmortalAwardTooltip Immortal { get; set; } = new();

    /// <summary>
    ///     Victory award.
    /// </summary>
    [PhpProperty("victory")]
    public VictoryAwardTooltip Victory { get; set; } = new();

    /// <summary>
    ///     Loss award.
    /// </summary>
    [PhpProperty("loss")]
    public LossAwardTooltip Loss { get; set; } = new();

    /// <summary>
    ///     Disconnect award.
    /// </summary>
    [PhpProperty("disco")]
    public DisconnectAwardTooltip Disconnect { get; set; } = new();

    /// <summary>
    ///     Quick match award.
    /// </summary>
    [PhpProperty("quick")]
    public QuickMatchAwardTooltip QuickMatch { get; set; } = new();

    /// <summary>
    ///     First blood award.
    /// </summary>
    [PhpProperty("first")]
    public FirstBloodAwardTooltip FirstBlood { get; set; } = new();

    /// <summary>
    ///     Consecutive wins award.
    /// </summary>
    [PhpProperty("consec_win")]
    public ConsecutiveWinAwardTooltip ConsecutiveWins { get; set; } = new();

    /// <summary>
    ///     Consecutive losses award.
    /// </summary>
    [PhpProperty("consec_loss")]
    public ConsecutiveLossAwardTooltip ConsecutiveLosses { get; set; } = new();
}