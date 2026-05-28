namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down three group-level helpers used during queue-join validation and team formation:
///     <list type="bullet">
///         <item><see cref="MatchmakingGroup.IsExperienced"/> — gates "veterans only" matchmaking buckets.</item>
///         <item><see cref="MatchmakingGroup.GetAdaptiveTMRSpread"/> — widens the per-group TMR spread as queue time grows.</item>
///         <item><see cref="MatchmakingGroup.HasExcessiveTMRDisparity"/> — the in-group anti-boosting check fired during <see cref="MatchmakingGroup.JoinQueue"/>.</item>
///     </list>
/// </summary>
public sealed class GroupExperienceAndDisparityTests
{
    /// <summary>
    ///     A group is experienced once any member has played 50+ matches OR the group's average TMR reaches 1625.
    /// </summary>
    private const int    ExperiencedMatchCountThreshold = 50;
    private const double ExperiencedTMRCutoff           = 1625.0;

    /// <summary>
    ///     The in-group disparity check fires when the highest player exceeds the bottom-(N-1) average by 150 or more.
    /// </summary>
    private const double GroupDisparityThreshold = 150.0;

    /// <summary>
    ///     The base spread (queue time = 0) for <see cref="MatchmakingGroup.GetAdaptiveTMRSpread"/> (equals the in-group TMR range), since the function returns <c>TMRRange + (queueMinutes - 2) * 50</c> only past 2 minutes.
    /// </summary>
    private const double AdaptiveSpreadDelayMinutes = 2.0;
    private const double AdaptiveSpreadRatePerMinute = 50.0;

    [Test]
    [Arguments(0,                                  1500.0, false, "Fresh Account, Mid-Tier TMR")]
    [Arguments(0,                                  1624.0, false, "Fresh Account, One Below TMR Cutoff")]
    [Arguments(0,                                  1625.0, true,  "Fresh Account, At TMR Cutoff")]
    [Arguments(ExperiencedMatchCountThreshold - 1, 1500.0, false, "One Below Match Count, Mid-Tier TMR")]
    [Arguments(ExperiencedMatchCountThreshold,     1500.0, true,  "At Match Count Threshold, Mid-Tier TMR")]
    public async Task Is_Experienced_Gates_On_Either_Match_Count_Or_Average_TMR(int totalMatchCount, double averageTMR, bool expectExperienced, string description)
    {
        // The Group Has A Single Solo Member; AverageTMR Equals That Member's TMR

        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(averageTMR, totalMatchCount: totalMatchCount);

        await Assert.That(group.IsExperienced).IsEqualTo(expectExperienced);
    }

    [Test]
    public async Task Get_Adaptive_TMR_Spread_At_Zero_Queue_Minutes_Equals_In_Group_TMR_Range()
    {
        const double LowTMR  = 1400.0;
        const double HighTMR = 1600.0;

        MatchmakingGroup group = MatchmakingTestBuilder.BuildGroup
        (
            [LowTMR, HighTMR],
            queuedMinutesAgo: 0
        );

        // TMRRange = 1600 - 1400 = 200
        await Assert.That(group.GetAdaptiveTMRSpread()).IsEqualTo(HighTMR - LowTMR);
    }

    [Test]
    public async Task Get_Adaptive_TMR_Spread_Past_Delay_Expands_At_Configured_Rate()
    {
        const double LowTMR              = 1400.0;
        const double HighTMR             = 1600.0;
        const double FiveMinuteWait      = 5.0;
        const double ExpectedExpansion   = (FiveMinuteWait - AdaptiveSpreadDelayMinutes) * AdaptiveSpreadRatePerMinute; // 150
        const double ExpectedTotalSpread = (HighTMR - LowTMR) + ExpectedExpansion; // 350
        const double Tolerance           = 0.5; // The Production Computation Reads QueuedTimeInMinutes Off A Real Clock, Producing Sub-Second Drift

        MatchmakingGroup group = MatchmakingTestBuilder.BuildGroup
        (
            [LowTMR, HighTMR],
            queuedMinutesAgo: FiveMinuteWait
        );

        await Assert.That(Math.Abs(group.GetAdaptiveTMRSpread() - ExpectedTotalSpread) < Tolerance).IsTrue();
    }

    [Test]
    public async Task Get_Adaptive_TMR_Spread_Before_Delay_Does_Not_Expand()
    {
        const double LowTMR  = 1400.0;
        const double HighTMR = 1600.0;
        const double JustUnderDelay = AdaptiveSpreadDelayMinutes - 0.1;

        MatchmakingGroup group = MatchmakingTestBuilder.BuildGroup
        (
            [LowTMR, HighTMR],
            queuedMinutesAgo: JustUnderDelay
        );

        await Assert.That(group.GetAdaptiveTMRSpread()).IsEqualTo(HighTMR - LowTMR);
    }

    [Test]
    public async Task Has_Excessive_TMR_Disparity_Solo_Group_Returns_False()
    {
        // The Early Return For "Members.Count <= 1" Means A Solo Group Never Trips The Check

        MatchmakingGroup solo = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.OutlierHighTMR);

        await Assert.That(solo.HasExcessiveTMRDisparity()).IsFalse();
    }

    [Test]
    public async Task Has_Excessive_TMR_Disparity_Balanced_Three_Stack_Returns_False()
    {
        // Three Players All At Baseline; Synthetic Full Team Has Highest = Average, Disparity = 0

        MatchmakingGroup balanced = MatchmakingTestBuilder.BuildGroup
        (
            [MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]
        );

        await Assert.That(balanced.HasExcessiveTMRDisparity()).IsFalse();
    }

    [Test]
    public async Task Has_Excessive_TMR_Disparity_Three_Stack_With_One_Far_Outlier_Returns_True()
    {
        // For A 3-Stack At [1900, 1500, 1500] Extrapolated To Team Size 5:
        //     Total = 4900, AverageTMR = 1633.33, TeamApproximation = 4900 + 1633.33 * 2 = 8166.66
        //     Bottom-Four-Average = (8166.66 - 1900) / 4 = 1566.67
        //     Disparity = 1900 - 1566.67 ≈ 333.3 ≥ 150 (Threshold)

        const double OutlierWithinGroup = 1900.0;

        MatchmakingGroup boostingShape = MatchmakingTestBuilder.BuildGroup
        (
            [OutlierWithinGroup, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]
        );

        await Assert.That(boostingShape.HasExcessiveTMRDisparity()).IsTrue();
    }

    [Test]
    public async Task Has_Excessive_TMR_Disparity_Three_Stack_Just_Below_Threshold_Returns_False()
    {
        // For A 3-Stack [Highest, Baseline, Baseline] Extrapolated To Team Size 5, Algebra Gives:
        //     Disparity = (10 * Highest - 10 * Baseline) / 12
        // Solving "Disparity &lt; 150" With Baseline = 1500 Yields "Highest &lt; 1680". We Use 1670 (Disparity ≈ 141.7)

        const double JustBelowThresholdHigh = 1670.0;

        MatchmakingGroup borderline = MatchmakingTestBuilder.BuildGroup
        (
            [JustBelowThresholdHigh, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]
        );

        await Assert.That(borderline.HasExcessiveTMRDisparity()).IsFalse();
    }
}
