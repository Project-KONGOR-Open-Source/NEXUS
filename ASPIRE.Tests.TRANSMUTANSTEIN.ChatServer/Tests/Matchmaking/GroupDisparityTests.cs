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
        // For A 3-Stack At [1900, 1500, 1500]: Average Of Others = 1500, Disparity = 1900 - 1500 = 400 ≥ 150 (Threshold)

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
        // For A 3-Stack [1640, 1500, 1500]: Average Of Others = 1500, Disparity = 1640 - 1500 = 140 < 150 (Threshold)

        const double JustBelowThresholdHigh = 1640.0;

        MatchmakingGroup borderline = MatchmakingTestBuilder.BuildGroup
        (
            [JustBelowThresholdHigh, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]
        );

        await Assert.That(borderline.HasExcessiveTMRDisparity()).IsFalse();
    }

    [Test]
    public async Task Has_Excessive_TMR_Disparity_Duo_Beyond_Threshold_Returns_True()
    {
        // The Cap Applies To The Actual Group Regardless Of Size: A Duo [1700, 1500] Has Disparity 200 ≥ 150 (Threshold)
        // A Prior Full-Team Extrapolation Diluted This To 125 For A Duo, Letting Such A Pair Queue Despite The 150 Cap

        MatchmakingGroup duo = MatchmakingTestBuilder.BuildGroup
        (
            [1700.0, MatchmakingTestBuilder.BaselineTMR]
        );

        await Assert.That(duo.HasExcessiveTMRDisparity()).IsTrue();
    }
}
