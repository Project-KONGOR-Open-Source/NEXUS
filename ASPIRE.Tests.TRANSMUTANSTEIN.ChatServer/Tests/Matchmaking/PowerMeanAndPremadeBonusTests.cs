namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down the team-rating maths: power mean (exponent 6.5), the flat premade bonus on a group, the per-team distribution into <see cref="MatchmakingTeam.EffectiveTeamRating"/>, and the group-makeup score table for 5v5 / 3v3 / 1v1.
/// </summary>
public sealed class PowerMeanAndPremadeBonusTests
{
    /// <summary>
    ///     The expected power mean of five players each rated <see cref="MatchmakingTestBuilder.BaselineTMR"/> (1500) under the production formula <c>(5 * 1500^6.5)^(1/6.5)</c>, rounded to the nearest integer.
    ///     Hardcoded so a regression in the production formula causes the test to fail rather than silently inherit the bug.
    /// </summary>
    private const double PowerMeanFiveAtBaseline = 1921.0;

    /// <summary>
    ///     The expected power mean of four players at baseline (1500) plus one outlier at <see cref="OutlierTMR"/> (2000) under <c>(4 * 1500^6.5 + 2000^6.5)^(1/6.5)</c>, rounded to the nearest integer.
    /// </summary>
    private const double PowerMeanFourAtBaselinePlusOneOutlier = 2153.0;

    /// <summary>
    ///     A TMR significantly above <see cref="MatchmakingTestBuilder.BaselineTMR"/> used to construct the outlier scenario for the power-mean test.
    /// </summary>
    private const double OutlierTMR = 2000.0;

    /// <summary>
    ///     The flat premade-coordination bonus for a 5-player group: <c>4 * 2^5 = 128</c>.
    /// </summary>
    private const double FullStackPremadeBonus = 128.0;

    /// <summary>
    ///     The expected effective team rating of a single 5-stack at baseline: <c>PowerMeanFiveAtBaseline + FullStackPremadeBonus / 5</c>.
    /// </summary>
    private const double EffectiveRatingFullStackAtBaseline = PowerMeanFiveAtBaseline + FullStackPremadeBonus / 5.0;

    [Test]
    public async Task PowerMean_OfFiveEqualRatings_HardcodedExpectedValue()
    {
        MatchmakingGroup group = BuildBaselineGroup(5);
        MatchmakingTeam team = MatchmakingTeam.FromGroups([group], teamSize: 5);

        await Assert.That(team.PowerMeanTMR).IsEqualTo(PowerMeanFiveAtBaseline);
    }

    [Test]
    public async Task PowerMean_WithOneHighOutlier_HardcodedExpectedValue()
    {
        MatchmakingGroup uniform = BuildBaselineGroup(5);
        MatchmakingGroup outlier = MatchmakingTestBuilder.BuildGroup
        (
            [MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, OutlierTMR]
        );

        MatchmakingTeam uniformTeam = MatchmakingTeam.FromGroups([uniform], teamSize: 5);
        MatchmakingTeam outlierTeam = MatchmakingTeam.FromGroups([outlier], teamSize: 5);

        const double ArithmeticMeanOfOutlierTeam = (MatchmakingTestBuilder.BaselineTMR * 4 + OutlierTMR) / 5.0;

        using (Assert.Multiple())
        {
            await Assert.That(uniformTeam.PowerMeanTMR).IsEqualTo(PowerMeanFiveAtBaseline);
            await Assert.That(outlierTeam.PowerMeanTMR).IsEqualTo(PowerMeanFourAtBaselinePlusOneOutlier);
            await Assert.That(outlierTeam.PowerMeanTMR).IsGreaterThan(uniformTeam.PowerMeanTMR);
            await Assert.That(outlierTeam.PowerMeanTMR).IsGreaterThan(ArithmeticMeanOfOutlierTeam);
        }
    }

    [Test]
    [Arguments(1, 0.0)]
    [Arguments(2, 16.0)]
    [Arguments(3, 32.0)]
    [Arguments(4, 64.0)]
    [Arguments(5, FullStackPremadeBonus)]
    public async Task PremadeBonus_FollowsFourTimesTwoPowGroupSize(int groupSize, double expectedBonus)
    {
        MatchmakingGroup group = BuildBaselineGroup(groupSize);

        await Assert.That(group.GetPremadeBonus()).IsEqualTo(expectedBonus);
    }

    [Test]
    public async Task EffectiveTeamRating_DistributesPremadeBonusAcrossPlayers()
    {
        MatchmakingGroup fullStack = BuildBaselineGroup(5);
        MatchmakingTeam team = MatchmakingTeam.FromGroups([fullStack], teamSize: 5);

        await Assert.That(team.EffectiveTeamRating).IsEqualTo(EffectiveRatingFullStackAtBaseline);
    }

    [Test]
    public async Task EffectiveTeamRating_FiveSolosHasNoPremadeBonus()
    {
        List<MatchmakingGroup> solos =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR)
        ];

        MatchmakingTeam team = MatchmakingTeam.FromGroups(solos, teamSize: 5);

        // No Group Of Size > 1, So Premade Bonus Sums To Zero; Effective Rating Equals The Power Mean
        await Assert.That(team.EffectiveTeamRating).IsEqualTo(PowerMeanFiveAtBaseline);
    }

    [Test]
    [Arguments(new[] { 5 },              "5",         25)]
    [Arguments(new[] { 4, 1 },           "4+1",       17)]
    [Arguments(new[] { 3, 2 },           "3+2",       13)]
    [Arguments(new[] { 3, 1, 1 },        "3+1+1",     11)]
    [Arguments(new[] { 2, 2, 1 },        "2+2+1",      9)]
    [Arguments(new[] { 2, 1, 1, 1 },     "2+1+1+1",    7)]
    [Arguments(new[] { 1, 1, 1, 1, 1 },  "1+1+1+1+1",  5)]
    public async Task GroupMakeup_5v5_PatternHasExpectedScore(int[] groupSizes, string expectedPattern, int expectedScore)
    {
        List<MatchmakingGroup> groups = [.. groupSizes.Select(BuildBaselineGroup)];

        MatchmakingTeam team = MatchmakingTeam.FromGroups(groups, teamSize: 5);

        using (Assert.Multiple())
        {
            await Assert.That(team.GroupMakeupString).IsEqualTo(expectedPattern);
            await Assert.That(team.GroupMakeup).IsEqualTo(expectedScore);
        }
    }

    [Test]
    [Arguments(new[] { 3 },        "3",     9)]
    [Arguments(new[] { 2, 1 },     "2+1",   5)]
    [Arguments(new[] { 1, 1, 1 },  "1+1+1", 3)]
    public async Task GroupMakeup_3v3_PatternHasExpectedScore(int[] groupSizes, string expectedPattern, int expectedScore)
    {
        List<MatchmakingGroup> groups = [.. groupSizes.Select(BuildBaselineGroup)];

        MatchmakingTeam team = MatchmakingTeam.FromGroups(groups, teamSize: 3);

        using (Assert.Multiple())
        {
            await Assert.That(team.GroupMakeupString).IsEqualTo(expectedPattern);
            await Assert.That(team.GroupMakeup).IsEqualTo(expectedScore);
        }
    }

    [Test]
    public async Task GroupMakeup_1v1_AlwaysScoresOne()
    {
        MatchmakingGroup solo = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR);
        MatchmakingTeam team = MatchmakingTeam.FromGroups([solo], teamSize: 1);

        await Assert.That(team.GroupMakeup).IsEqualTo(1);
    }

    private static MatchmakingGroup BuildBaselineGroup(int memberCount)
        => MatchmakingTestBuilder.BuildGroup([.. Enumerable.Repeat(MatchmakingTestBuilder.BaselineTMR, memberCount)]);
}
