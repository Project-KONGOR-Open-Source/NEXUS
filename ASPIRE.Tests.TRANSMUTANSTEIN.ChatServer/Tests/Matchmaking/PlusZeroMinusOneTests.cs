namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down <see cref="MatchmakingAlgorithm.ProducesPlusZeroMinusOne"/>.
///     A five-player team is rejected when the highest-rated player exceeds the average of the bottom four by more than the production threshold.
/// </summary>
public sealed class PlusZeroMinusOneTests
{
    /// <summary>
    ///     The production threshold above which a team is treated as producing +0/-1 outcomes (see <see cref="MatchmakingAlgorithm.ProducesPlusZeroMinusOne"/>'s <c>&gt; 151.0</c> check).
    /// </summary>
    private const double ThresholdTMR = 151.0;

    /// <summary>
    ///     The smallest disparity that crosses the threshold (<c>&gt; 151</c>): with this delta above the bottom-four average, the team is rejected.
    /// </summary>
    private const double JustAboveThresholdTMR = ThresholdTMR + 1; // 152.0

    [Test]
    public async Task Full_Team_Balanced_Ratings_Does_Not_Produce_Plus_Zero_Minus_One()
    {
        MatchmakingTeam team = SoloAssembledTeam(MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR);

        await Assert.That(MatchmakingAlgorithm.ProducesPlusZeroMinusOne(team)).IsFalse();
    }

    /// <summary>
    ///     Each row pins the boundary behaviour: <paramref name="topPlayerOffset"/> is the gap between the highest player and the bottom-four average.
    /// </summary>
    [Test]
    [Arguments(700.0, true,  "Far Above Threshold")]
    [Arguments(150.0, false, "One Below Threshold (Boundary)")]
    [Arguments(ThresholdTMR, false, "At Threshold (Strict ‘>’ Means Exactly 151 Is Allowed)")]
    [Arguments(JustAboveThresholdTMR, true, "One Above Threshold")]
    public async Task Full_Team_Outlier_At_Boundary_Rejects_Exactly_When_Strictly_Above_Threshold(double topPlayerOffset, bool expectRejection, string description)
    {
        // The Bottom Four At Baseline And One Outlier At "Baseline + Offset" Produces "Highest - AverageOfBottomFour = Offset" Exactly

        double[] ratings =
        [
            MatchmakingTestBuilder.BaselineTMR,
            MatchmakingTestBuilder.BaselineTMR,
            MatchmakingTestBuilder.BaselineTMR,
            MatchmakingTestBuilder.BaselineTMR,
            MatchmakingTestBuilder.BaselineTMR + topPlayerOffset
        ];

        MatchmakingTeam team = SoloAssembledTeam(ratings);

        await Assert.That(MatchmakingAlgorithm.ProducesPlusZeroMinusOne(team)).IsEqualTo(expectRejection);
    }

    [Test]
    public async Task Single_Player_Team_Returns_False_Regardless_Of_Rating()
    {
        // 1v1 Team Size = 1, So The Early-Return Branch Fires

        MatchmakingGroup solo = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.OutlierHighTMR);
        MatchmakingTeam team = MatchmakingTeam.FromGroups([solo], teamSize: 1);

        await Assert.That(MatchmakingAlgorithm.ProducesPlusZeroMinusOne(team)).IsFalse();
    }

    [Test]
    public async Task Partially_Filled_Team_Returns_False()
    {
        // PlayerCount (3) ≠ TeamSize (5) Triggers The Same Early-Return

        MatchmakingGroup trio = MatchmakingTestBuilder.BuildGroup([1000, MatchmakingTestBuilder.BaselineTMR, 2500]);
        MatchmakingTeam team = MatchmakingTeam.FromGroups([trio], teamSize: 5);

        await Assert.That(MatchmakingAlgorithm.ProducesPlusZeroMinusOne(team)).IsFalse();
    }

    private static MatchmakingTeam SoloAssembledTeam(params double[] ratings)
    {
        List<MatchmakingGroup> solos = [.. ratings.Select(rating => MatchmakingTestBuilder.BuildSoloGroup(rating))];

        return MatchmakingTeam.FromGroups(solos, teamSize: 5);
    }
}
