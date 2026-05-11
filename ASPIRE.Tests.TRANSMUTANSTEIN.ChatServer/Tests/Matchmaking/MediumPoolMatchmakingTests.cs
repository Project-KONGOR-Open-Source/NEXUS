namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Drives the algorithm against pools that resolve to <see cref="PoolSizeTier.Medium"/> (500 to 2,499 queued players).
/// </summary>
public sealed class MediumPoolMatchmakingTests
{
    /// <summary>
    ///     Number of padding solos to add to push the queue into the medium tier (≥ 500). Set just above the tier threshold for determinism.
    /// </summary>
    private const int MediumTierPaddingCount = 510;

    /// <summary>
    ///     A TMR far from <see cref="MatchmakingTestBuilder.BaselineTMR"/>, used for padding solos so they cannot cross-match with subjects.
    /// </summary>
    private const double FarPaddingTMR = 800.0;

    /// <summary>
    ///     A TMR delta that is just above the default base spread of 50, but within the medium cap of 500.
    /// </summary>
    private const double WithinMediumCapDelta = 100.0;

    /// <summary>
    ///     A queue-wait long enough to push the medium-tier expansion past <see cref="WithinMediumCapDelta"/>: <c>50 + (3 - 1.5) * 75 = 162.5</c> &gt; 100.
    /// </summary>
    private const double SufficientQueueMinutesForMediumExpansion = 3.0;

    [Test]
    public async Task TmrSpread_AfterMediumDelay_ExpandsAtConfiguredRate()
    {
        // Medium: Delay = 1.5m, Rate = 75 / Min, Cap = 500
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        PoolSizeParameters mediumTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 1500, settings);

        double belowBaseline = MatchmakingTestBuilder.BaselineTMR - WithinMediumCapDelta / 2;
        double aboveBaseline = MatchmakingTestBuilder.BaselineTMR + WithinMediumCapDelta / 2;

        List<MatchmakingGroup> freshQueue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(belowBaseline, queuedMinutesAgo: 0),
            MatchmakingTestBuilder.BuildSoloGroup(aboveBaseline, queuedMinutesAgo: 0)
        ];

        List<MatchmakingGroup> matureQueue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(belowBaseline, queuedMinutesAgo: SufficientQueueMinutesForMediumExpansion),
            MatchmakingTestBuilder.BuildSoloGroup(aboveBaseline, queuedMinutesAgo: SufficientQueueMinutesForMediumExpansion)
        ];

        IReadOnlyList<MatchmakingMatch> freshMatches  = MatchmakingAlgorithm.RunMatchBrokerCycle(freshQueue,  settings, mediumTier);
        IReadOnlyList<MatchmakingMatch> matureMatches = MatchmakingAlgorithm.RunMatchBrokerCycle(matureQueue, settings, mediumTier);

        using (Assert.Multiple())
        {
            await Assert.That(freshMatches.Count).IsEqualTo(0);
            await Assert.That(matureMatches.Count).IsEqualTo(1);
        }
    }

    [Test]
    public async Task TmrSpread_BeyondMediumCap_StillRejectsLongQueues()
    {
        // 600 TMR Apart, Queued 10m → Naive Spread = 50 + 8.5 * 75 = 687.5, Cap = 500 → 600 Stays Above Cap → No Match
        const double BeyondMediumCapDelta = 600.0;
        const double LongQueueMinutes     = 10.0;

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        settings.PlayersPerTeam = 1;

        PoolSizeParameters mediumTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 1500, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR - BeyondMediumCapDelta / 2, queuedMinutesAgo: LongQueueMinutes),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR + BeyondMediumCapDelta / 2, queuedMinutesAgo: LongQueueMinutes)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, mediumTier);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task PlusZeroMinusOneCheck_IsEnforcedInMediumPools()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.OutlierHighTMR),

            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.OutlierHighTMR)
        ];

        // Padding (TMRs Sufficiently Far Away So They Cannot Match The Outlier Teams Within The 500 TMR Cap)
        for (int index = 0; index < MediumTierPaddingCount; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(FarPaddingTMR + (index % 5) * 5));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        // The Two Outlier Teams Should NOT Be Matched Together — Each Has A Highest Player Far Above Average
        bool outlierMatched = matches.Any(match =>
        {
            double legionMaxTmr     = match.LegionTeam.HighestTMR;
            double hellbourneMaxTmr = match.HellbourneTeam?.HighestTMR ?? 0;

            return legionMaxTmr >= MatchmakingTestBuilder.OutlierHighTMR && hellbourneMaxTmr >= MatchmakingTestBuilder.OutlierHighTMR;
        });

        await Assert.That(outlierMatched).IsFalse();
    }

    [Test]
    public async Task MakeupTolerance_3PlusTwoVersusFiveSolosIsRejectedAt4()
    {
        // 3+2 (13) Vs 1+1+1+1+1 (5) → Difference = 8, Above Medium Tolerance Of 4
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR])
        ];

        for (int index = 0; index < 5; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR));

        for (int index = 0; index < MediumTierPaddingCount + 90; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(FarPaddingTMR + (index % 7)));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        bool mismatchPaired = matches.Any(match =>
            (match.LegionTeam.GroupMakeup == 13 && match.HellbourneTeam!.GroupMakeup == 5) ||
            (match.LegionTeam.GroupMakeup == 5  && match.HellbourneTeam!.GroupMakeup == 13));

        await Assert.That(mismatchPaired).IsFalse();
    }
}
