namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="MatchmakingGroup.HasExcessiveTMRDisparity"/>, the in-group anti-boosting check fired during <see cref="MatchmakingGroup.JoinQueue"/>.
/// </summary>
public sealed class GroupDisparityTests
{
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
