namespace TRANSMUTANSTEIN.ChatServer.Configuration;

/// <summary>
///     Configuration settings for the matchmaking system.
///     These settings control how matches are formed and balanced.
/// </summary>
public class MatchmakingSettings
{
    /// <summary>
    ///     The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Matchmaking";

    /// <summary>
    ///     The number of players per team.
    ///     Set to 1 for 1v1 or testing, 3 for Grimm's Crossing, 5 for standard matches.
    /// </summary>
    public int PlayersPerTeam { get; set; } = 5;

    /// <summary>
    ///     The interval between matchmaking broker cycles.
    /// </summary>
    public TimeSpan MatchmakingCycleInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     The minimum TMR (Team Match Rating) value.
    /// </summary>
    public double MinimumTMR { get; set; } = 1000.0;

    /// <summary>
    ///     The maximum TMR (Team Match Rating) value.
    /// </summary>
    public double MaximumTMR { get; set; } = 2500.0;

    /// <summary>
    ///     The default TMR for new players.
    /// </summary>
    public double DefaultTMR { get; set; } = 1500.0;

    /// <summary>
    ///     The low TMR threshold for outlier detection.
    /// </summary>
    public double LowTMROutlier { get; set; } = 1200.0;

    /// <summary>
    ///     The high TMR threshold for outlier detection.
    /// </summary>
    public double HighTMROutlier { get; set; } = 1750.0;

    /// <summary>
    ///     The maximum TMR difference allowed between teams.
    ///     Set to a high value to allow any match during testing.
    /// </summary>
    public double MaximumTeamTMRDifference { get; set; } = 500.0;

    /// <summary>
    ///     The logistic prediction scale factor for ELO-based win probability calculations.
    /// </summary>
    public double LogisticPredictionScale { get; set; } = 80.0;

    /// <summary>
    ///     The base K-factor for TMR change calculations.
    ///     The K-factor is a constant from the Elo rating system that controls how much a player's rating changes after a single match.
    ///     A higher K-factor results in larger rating swings (more volatile), while a lower K-factor results in smaller rating swings (more stable).
    ///     The base value is modified by the following multipliers:
    ///     <list type="bullet">
    ///         <item>
    ///             <term><see cref="ProvisionalKFactorMultiplier"/></term>
    ///             <description>
    ///                 Applied to provisional players (fewer than <see cref="ProvisionalMatchCount"/> matches and TMR below <see cref="ProvisionalTMRCutoff"/>), so their rating converges faster.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="ReducedKFactorMultiplier"/></term>
    ///             <description>
    ///                 Applied to high-TMR players (above <see cref="ReducedKFactorTMRCutoff"/>), stabilising their rating at the top end.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    public double BaseKFactor { get; set; } = 10.0;

    /// <summary>
    ///     The K-factor multiplier for provisional players.
    ///     Provisional players are those with fewer than <see cref="ProvisionalMatchCount"/> matches and TMR below <see cref="ProvisionalTMRCutoff"/>.
    /// </summary>
    public double ProvisionalKFactorMultiplier { get; set; } = 2.0;

    /// <summary>
    ///     The number of matches required before a player is no longer considered provisional.
    /// </summary>
    public int ProvisionalMatchCount { get; set; } = 10;

    /// <summary>
    ///     The TMR cutoff for provisional player status.
    ///     Players with TMR above this value are not considered provisional.
    /// </summary>
    public double ProvisionalTMRCutoff { get; set; } = 1750.0;

    /// <summary>
    ///     The K-factor reduction multiplier for high TMR players.
    ///     Applied when player TMR exceeds <see cref="ReducedKFactorTMRCutoff"/>.
    /// </summary>
    public double ReducedKFactorMultiplier { get; set; } = 0.20;

    /// <summary>
    ///     The TMR threshold above which K-factor reduction begins.
    /// </summary>
    public double ReducedKFactorTMRCutoff { get; set; } = 1600.0;

    /// <summary>
    ///     Whether matchmaking is currently enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
