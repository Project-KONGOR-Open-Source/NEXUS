namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Drives the algorithm against pools that resolve to <see cref="PoolSizeTier.Macro"/> (≥ 7,500 queued players).
/// </summary>
public sealed class MacroPoolMatchmakingTests
{
    /// <summary>
    ///     A queued-player count that unambiguously resolves into the Macro tier (≥ 7,500).
    /// </summary>
    private const int MacroTierPlayerCount = 10000;

    /// <summary>
    ///     A TMR delta that exceeds the default base spread of 50 but fits within the Macro 150 cap once expansion has applied.
    /// </summary>
    private const double WithinMacroCapDelta = 100.0;

    /// <summary>
    ///     A TMR delta that exceeds the Macro 150 cap even after maximum expansion.
    /// </summary>
    private const double BeyondMacroCapDelta = 200.0;

    /// <summary>
    ///     A queue wait long enough that <c>50 + (5 - 2.5) * 25 = 112.5</c> ≥ <see cref="WithinMacroCapDelta"/>.
    /// </summary>
    private const double SufficientQueueMinutesForMacroExpansion = 5.0;

    /// <summary>
    ///     A queue wait long enough that the naive expansion would exceed the cap.
    /// </summary>
    private const double VeryLongQueueMinutes = 30.0;

    /// <summary>
    ///     The Macro pool's group makeup tolerance. Pulled from <see cref="MatchmakingSettings.MacroPoolGroupMakeupTolerance"/> default.
    /// </summary>
    private const int MacroTolerance = 2;

    /// <summary>
    ///     The Micro pool's group makeup tolerance. Pulled from <see cref="MatchmakingSettings.MicroPoolGroupMakeupTolerance"/> default.
    /// </summary>
    private const int MicroTolerance = 20;

    [Test]
    public async Task FreshTeams100ApartCannotMatchAtMacroBaseSpread()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        PoolSizeParameters macroTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(MacroTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR - WithinMacroCapDelta / 2, queuedMinutesAgo: 0),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR + WithinMacroCapDelta / 2, queuedMinutesAgo: 0)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, macroTier);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TeamsAreMatchedOnceMacroSpreadHasExpandedPastTheirDifference()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        PoolSizeParameters macroTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(MacroTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR - WithinMacroCapDelta / 2, queuedMinutesAgo: SufficientQueueMinutesForMacroExpansion),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR + WithinMacroCapDelta / 2, queuedMinutesAgo: SufficientQueueMinutesForMacroExpansion)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, macroTier);

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TmrSpreadCapsAtMacroMaximum()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        PoolSizeParameters macroTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(MacroTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR - BeyondMacroCapDelta / 2, queuedMinutesAgo: VeryLongQueueMinutes),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR + BeyondMacroCapDelta / 2, queuedMinutesAgo: VeryLongQueueMinutes)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, macroTier);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task PlusZeroMinusOneCheckIsEnforcedInMacroPools()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        PoolSizeParameters macroTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(MacroTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            // First Outlier Team
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.OutlierHighTMR),

            // Second Outlier Team
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.OutlierHighTMR)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, macroTier);

        // Both Teams Trigger +0/-1 — Macro Pool Rejects
        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task BalancedFiveStacksMatchUpInMacroPool()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        PoolSizeParameters macroTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(MacroTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR])
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, macroTier);

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task C1Fix_TwoMatchingFourPlusOnesPairWithFalseFlag()
    {
        // Two 4+1 Compositions (Both Score 17, Difference = 0) Should Pair And Not Trip MismatchedGroupMakeup In A Macro Pool
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        PoolSizeParameters macroTier = MatchmakingAlgorithm.ResolvePoolSizeParameters(MacroTierPlayerCount, settings);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),

            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings, macroTier);

        using (Assert.Multiple())
        {
            await Assert.That(matches.Count).IsEqualTo(1);
            await Assert.That(matches[0].MismatchedGroupMakeup).IsFalse();
        }
    }

    [Test]
    public async Task C1Fix_FourPlusOneVersusThreePlusTwoFlagsTrueWithMacroTolerance()
    {
        // Bypasses The Broker To Test The Construction-Level Behaviour Of MismatchedGroupMakeup With An Explicit Pool-Aware Tolerance Of 2 (Macro / Large Default)
        // 4+1 (17) Vs 3+2 (13) Has Difference = 4 > <see cref="MacroTolerance"/> So The Flag Must Fire

        MatchmakingTeam fourPlusOne  = BuildFourPlusOneTeam();
        MatchmakingTeam threePlusTwo = BuildThreePlusTwoTeam();

        MatchmakingMatch match = MatchmakingMatch.FromTeams(fourPlusOne, threePlusTwo, logisticPredictionScale: 80.0, groupMakeupTolerance: MacroTolerance);

        await Assert.That(match.MismatchedGroupMakeup).IsTrue();
    }

    [Test]
    public async Task C1Fix_SameDiffWithMicroToleranceFlagsFalse()
    {
        // The Same Pairing With <see cref="MicroTolerance"/> Should NOT Be Flagged; Pins The Tolerance Wiring

        MatchmakingTeam fourPlusOne  = BuildFourPlusOneTeam();
        MatchmakingTeam threePlusTwo = BuildThreePlusTwoTeam();

        MatchmakingMatch match = MatchmakingMatch.FromTeams(fourPlusOne, threePlusTwo, logisticPredictionScale: 80.0, groupMakeupTolerance: MicroTolerance);

        await Assert.That(match.MismatchedGroupMakeup).IsFalse();
    }

    private static MatchmakingTeam BuildFourPlusOneTeam()
    {
        return MatchmakingTeam.FromGroups
        (
            [
                MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
                MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR)
            ],

            teamSize: 5
        );
    }

    private static MatchmakingTeam BuildThreePlusTwoTeam()
    {
        return MatchmakingTeam.FromGroups
        (
            [
                MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
                MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR])
            ],

            teamSize: 5
        );
    }
}
