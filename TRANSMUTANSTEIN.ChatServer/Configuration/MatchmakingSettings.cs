namespace TRANSMUTANSTEIN.ChatServer.Configuration;

/// <summary>
///     Configuration settings for the matchmaking system.
///     These settings control how matches are formed and balanced.
///     Per-map settings (available modes/regions) are defined in "MatchmakingConfiguration.json".
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
    ///     The baseline TMR difference allowed between teams before any queue-time expansion is applied.
    ///     This is the tightest acceptable spread; the pool-size-aware expansion settings widen it over time.
    /// </summary>
    public double MaximumTeamTMRDifference { get; set; } = 50.0;

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

    # region Dynamic Matchmaking Parameters Based On Player Pool Size
    /*
        The player pool is classified into five tiers based on the number of queuing players.
        Each tier has its own matchmaking parameters that control how aggressively the algorithm relaxes constraints.
        The pool tier boundaries are defined by four ascending thresholds.

            ┌────────────┬─────────────────────┬─────────────────────┬──────────────────────┬────────────┐
            │ Micro Pool │     Small Pool      │     Medium Pool     │      Large Pool      │ Macro Pool │
            └────────────┼─────────────────────┼─────────────────────┼──────────────────────┼────────────┘
                         │                     │                     │                      │
                SmallPoolThreshold    MediumPoolThreshold    LargePoolThreshold    MacroPoolThreshold

        TMR spread expansion begins after a certain queue time delay, and increases at a certain rate per minute, up to a maximum cap.

            TMR Spread By Pool Tier And Queue Time (Base = 50)
            ┌────────┬────────┬────────┬──────┬──────┬────────┬────────────┬─────────────┬─────────────┬────────────────┐
            │ Tier   │ Pool   │ Delay  │ Rate │ Cap  │ Makeup │ @ 1 Minute │ @ 3 Minutes │ @ 5 Minutes │ @ 10 Minutes   │
            ├────────┼────────┼────────┼──────┼──────┼────────┼────────────┼─────────────┼─────────────┼────────────────┤
            │ Micro  │ < 50   │ 0.5m   │  250 │ 1500 │ 20     │    175     │     675 [!] │    1175 [!] │    1500 [C][!] │
            │ Small  │ < 500  │ 1.0m   │  100 │ 1000 │  8     │     50     │     250     │     450 [!] │     950 [!]    │
            │ Medium │ < 2.5K │ 1.5m   │   75 │  500 │  4     │     50     │     162     │     312 [!] │     500 [C][!] │
            │ Large  │ < 7.5K │ 2.0m   │   50 │  250 │  2     │     50     │     100     │     200     │     250 [C]    │
            │ Macro  │ ≥ 7.5K │ 2.5m   │   25 │  150 │  2     │     50     │      62     │     112     │     150 [C]    │
            └────────┴────────┴────────┴──────┴──────┴────────┴────────────┴─────────────┴─────────────┴────────────────┘
            [!] = Exceeds 250 TMR (Empirically-Observed Distribution Tail Where Match Quality Degrades)
            [C] = Capped At Maximum TMR Spread For This Tier

        The group makeup tolerance controls how different the team compositions can be in terms of premade groups.
        A lower tolerance means teams must have more similar compositions (e.g. both teams must be 5-stacks, or both teams must be mostly solos), while a higher tolerance allows for more asymmetric matchups (e.g. a 5-stack vs a team of solos).

            Group Makeup Score Differences (5v5)
            ┌───────────────┬──────────┬──────────┬──────────┬────────────┬───────────┬─────────────┬───────────────┐
            │               │   5 (25) │ 4+1 (17) │ 3+2 (13) │ 3+1+1 (11) │ 2+2+1 (9) │ 2+1+1+1 (7) │ 1+1+1+1+1 (5) │
            ├───────────────┼──────────┼──────────┼──────────┼────────────┼───────────┼─────────────┼───────────────┤
            │ 5        (25) │     0    │     8    │    12    │     14     │    16     │      18     │       20      │
            │ 4+1      (17) │     8    │     0    │     4    │      6     │     8     │      10     │       12      │
            │ 3+2      (13) │    12    │     4    │     0    │      2     │     4     │       6     │        8      │
            │ 3+1+1    (11) │    14    │     6    │     2    │      0     │     2     │       4     │        6      │
            │ 2+2+1     (9) │    16    │     8    │     4    │      2     │     0     │       2     │        4      │
            │ 2+1+1+1   (7) │    18    │    10    │     6    │      4     │     2     │       0     │        2      │
            │ 1+1+1+1+1 (5) │    20    │    12    │     8    │      6     │     4     │       2     │        0      │
            └───────────────┴──────────┴──────────┴──────────┴────────────┴───────────┴─────────────┴───────────────┘
    */
    # endregion

    # region Player Pool Size Tier Thresholds
    /// <summary>
    ///     The upper boundary of the "micro" pool tier. The lower boundary of the "small" pool tier.
    ///     Pools with fewer than this many queued players use the most aggressive TMR expansion and the most relaxed fairness constraints.
    /// </summary>
    public int SmallPoolThreshold { get; set; } = 50;

    /// <summary>
    ///     The upper boundary of the "small" pool tier. The lower boundary of the "medium" pool tier.
    /// </summary>
    public int MediumPoolThreshold { get; set; } = 500;

    /// <summary>
    ///     The upper boundary of the "medium" pool tier. The lower boundary of the "large" pool tier.
    /// </summary>
    public int LargePoolThreshold { get; set; } = 2500;

    /// <summary>
    ///     The upper boundary of the "large" pool tier. The lower boundary of the "macro" pool tier.
    ///     Pools with this many or more queued players use the least aggressive TMR expansion and the strictest fairness constraints.
    /// </summary>
    public int MacroPoolThreshold { get; set; } = 7500;
    # endregion

    # region Micro Player Pool Configuration
    /// <summary>
    ///     The number of minutes a group must be queued before TMR spread expansion begins, in a micro pool.
    /// </summary>
    public double MicroPoolExpansionDelayMinutes { get; set; } = 0.5;

    /// <summary>
    ///     The TMR expansion rate per minute of queue time after the expansion delay, in a micro pool.
    /// </summary>
    public double MicroPoolExpansionRatePerMinute { get; set; } = 250.0;

    /// <summary>
    ///     The absolute maximum TMR spread allowed between matched teams, in a micro pool.
    /// </summary>
    public double MicroPoolMaximumTMRSpread { get; set; } = 1500.0;

    /// <summary>
    ///     The maximum allowed group makeup difference between teams, in a micro pool.
    ///     Higher values permit more asymmetric compositions (e.g. 5-stack vs solos).
    /// </summary>
    public int MicroPoolGroupMakeupTolerance { get; set; } = 20;

    /// <summary>
    ///     Whether to enforce the +0/-1 rating outcome check, in a micro pool.
    ///     Disabling this allows matches where the highest-rated player gains nothing for a win but loses rating for a loss.
    /// </summary>
    public bool MicroPoolEnforcePlusZeroMinusOneCheck { get; set; } = false;
    # endregion

    # region Small Player Pool Configuration
    /// <summary>
    ///     The number of minutes a group must be queued before TMR spread expansion begins, in a small pool.
    /// </summary>
    public double SmallPoolExpansionDelayMinutes { get; set; } = 1.0;

    /// <summary>
    ///     The TMR expansion rate per minute of queue time after the expansion delay, in a small pool.
    /// </summary>
    public double SmallPoolExpansionRatePerMinute { get; set; } = 100.0;

    /// <summary>
    ///     The absolute maximum TMR spread allowed between matched teams, in a small pool.
    /// </summary>
    public double SmallPoolMaximumTMRSpread { get; set; } = 1000.0;

    /// <summary>
    ///     The maximum allowed group makeup difference between teams, in a small pool.
    ///     Higher values permit more asymmetric compositions (e.g. 5-stack vs solos).
    /// </summary>
    public int SmallPoolGroupMakeupTolerance { get; set; } = 8;

    /// <summary>
    ///     Whether to enforce the +0/-1 rating outcome check, in a small pool.
    ///     Disabling this allows matches where the highest-rated player gains nothing for a win but loses rating for a loss.
    /// </summary>
    public bool SmallPoolEnforcePlusZeroMinusOneCheck { get; set; } = false;
    # endregion

    # region Medium Player Pool Configuration
    /// <summary>
    ///     The number of minutes a group must be queued before TMR spread expansion begins, in a medium pool.
    /// </summary>
    public double MediumPoolExpansionDelayMinutes { get; set; } = 1.5;

    /// <summary>
    ///     The TMR expansion rate per minute of queue time after the expansion delay, in a medium pool.
    /// </summary>
    public double MediumPoolExpansionRatePerMinute { get; set; } = 75.0;

    /// <summary>
    ///     The absolute maximum TMR spread allowed between matched teams, in a medium pool.
    /// </summary>
    public double MediumPoolMaximumTMRSpread { get; set; } = 500.0;

    /// <summary>
    ///     The maximum allowed group makeup difference between teams, in a medium pool.
    /// </summary>
    public int MediumPoolGroupMakeupTolerance { get; set; } = 4;

    /// <summary>
    ///     Whether to enforce the +0/-1 rating outcome check, in a medium pool.
    /// </summary>
    public bool MediumPoolEnforcePlusZeroMinusOneCheck { get; set; } = true;
    # endregion

    # region Large Player Pool Configuration
    /// <summary>
    ///     The number of minutes a group must be queued before TMR spread expansion begins, in a large pool.
    /// </summary>
    public double LargePoolExpansionDelayMinutes { get; set; } = 2.0;

    /// <summary>
    ///     The TMR expansion rate per minute of queue time after the expansion delay, in a large pool.
    /// </summary>
    public double LargePoolExpansionRatePerMinute { get; set; } = 50.0;

    /// <summary>
    ///     The absolute maximum TMR spread allowed between matched teams, in a large pool.
    /// </summary>
    public double LargePoolMaximumTMRSpread { get; set; } = 250.0;

    /// <summary>
    ///     The maximum allowed group makeup difference between teams, in a large pool.
    /// </summary>
    public int LargePoolGroupMakeupTolerance { get; set; } = 2;

    /// <summary>
    ///     Whether to enforce the +0/-1 rating outcome check, in a large pool.
    /// </summary>
    public bool LargePoolEnforcePlusZeroMinusOneCheck { get; set; } = true;
    # endregion

    # region Macro Player Pool Configuration
    /// <summary>
    ///     The number of minutes a group must be queued before TMR spread expansion begins, in a macro pool.
    /// </summary>
    public double MacroPoolExpansionDelayMinutes { get; set; } = 2.5;

    /// <summary>
    ///     The TMR expansion rate per minute of queue time after the expansion delay, in a macro pool.
    /// </summary>
    public double MacroPoolExpansionRatePerMinute { get; set; } = 25.0;

    /// <summary>
    ///     The absolute maximum TMR spread allowed between matched teams, in a macro pool.
    /// </summary>
    public double MacroPoolMaximumTMRSpread { get; set; } = 150.0;

    /// <summary>
    ///     The maximum allowed group makeup difference between teams, in a macro pool.
    /// </summary>
    public int MacroPoolGroupMakeupTolerance { get; set; } = 2;

    /// <summary>
    ///     Whether to enforce the +0/-1 rating outcome check, in a macro pool.
    /// </summary>
    public bool MacroPoolEnforcePlusZeroMinusOneCheck { get; set; } = true;
    # endregion
}
