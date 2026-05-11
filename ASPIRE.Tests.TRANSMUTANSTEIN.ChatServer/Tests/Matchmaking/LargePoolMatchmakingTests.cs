namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Drives the algorithm against pools that resolve to <see cref="PoolSizeTier.Large"/> (2,500 to 7,499 queued players).
/// </summary>
public sealed class LargePoolMatchmakingTests
{
    /// <summary>
    ///     A queued-player count that unambiguously resolves into the Large tier (2,500-7,499).
    /// </summary>
    private const int LargeTierPlayerCount = 5000;

    /// <summary>
    ///     A TMR delta that exceeds the Large tier's 250 TMR cap.
    /// </summary>
    private const double BeyondLargeCapDelta = 400.0;

    /// <summary>
    ///     A TMR delta that fits within the Large tier's 250 TMR cap once expansion has applied.
    /// </summary>
    private const double WithinLargeCapDelta = 200.0;

    /// <summary>
    ///     Long enough to exceed the Large-tier expansion delay (2.0 minutes) and produce <c>50 + (5 - 2) * 50 = 200</c> ≥ <see cref="WithinLargeCapDelta"/>.
    /// </summary>
    private const double SufficientQueueMinutesForLargeExpansion = 5.0;

    /// <summary>
    ///     Long enough that the naive expansion would exceed the cap (used to confirm the cap clamps).
    /// </summary>
    private const double VeryLongQueueMinutes = 30.0;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        MatchmakingTestBuilder.ResetAccountIDCounter();

        return Task.CompletedTask;
    }

    [Test]
    public async Task TmrSpread_BeyondLargeCap_PreventsMatchEvenWithLongQueue()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        PoolSizeParameters largeTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(LargeTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR - BeyondLargeCapDelta / 2, queuedMinutesAgo: VeryLongQueueMinutes),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR + BeyondLargeCapDelta / 2, queuedMinutesAgo: VeryLongQueueMinutes)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, largeTier);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TmrSpread_WithinLargeCap_AllowsMatchAfterExpansionDelay()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        PoolSizeParameters largeTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(LargeTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR - WithinLargeCapDelta / 2, queuedMinutesAgo: SufficientQueueMinutesForLargeExpansion),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR + WithinLargeCapDelta / 2, queuedMinutesAgo: SufficientQueueMinutesForLargeExpansion)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, largeTier);

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task MakeupTolerance_2InLargePool_RejectsMostAsymmetries()
    {
        // Large Tolerance = 2; A 3+1+1 (11) Vs 1+1+1+1+1 (5) → Difference = 6; Rejected

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        PoolSizeParameters largeTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(LargeTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),

            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, largeTier);

        bool mismatchPaired = matches.Any(match =>
            (match.LegionTeam.GroupMakeup == 11 && match.HellbourneTeam!.GroupMakeup == 5) ||
            (match.LegionTeam.GroupMakeup == 5  && match.HellbourneTeam!.GroupMakeup == 11));

        await Assert.That(mismatchPaired).IsFalse();
    }
}
