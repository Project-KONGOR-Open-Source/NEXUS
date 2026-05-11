namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Pins the boundary semantics of <see cref="MatchmakingAlgorithm.ResolvePoolSizeParameters"/>.
///     Each tier threshold is exclusive on its upper edge (i.e. a pool of exactly the threshold value sits in the next tier up).
/// </summary>
internal sealed class PoolSizeParameterResolutionTests
{
    /// <summary>
    ///     Macro-tier parameters at default settings: <c>delay 2.5m, rate 25/min, cap 150</c>.
    ///     Expected spread at 5 minutes: <c>50 + (5 - 2.5) * 25 = 112.5</c>.
    /// </summary>
    private const double ExpectedMacroSpreadAtFiveMinutes = 112.5;

    private const double SpreadComparisonTolerance = 0.5;

    [Test]
    [Arguments(0,     PoolSizeTier.Micro)]
    [Arguments(1,     PoolSizeTier.Micro)]
    [Arguments(49,    PoolSizeTier.Micro)]
    [Arguments(50,    PoolSizeTier.Small)]
    [Arguments(499,   PoolSizeTier.Small)]
    [Arguments(500,   PoolSizeTier.Medium)]
    [Arguments(2499,  PoolSizeTier.Medium)]
    [Arguments(2500,  PoolSizeTier.Large)]
    [Arguments(7499,  PoolSizeTier.Large)]
    [Arguments(7500,  PoolSizeTier.Macro)]
    [Arguments(50000, PoolSizeTier.Macro)]
    public async Task Resolve_Pool_Size_Parameters_Produces_Expected_Tier(int queuedPlayerCount, PoolSizeTier expectedTier)
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        PoolSizeParameters resolved = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount, settings);

        await Assert.That(resolved.Tier).IsEqualTo(expectedTier);
    }

    [Test]
    public async Task Micro_Tier_Pulls_Micro_Settings()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        PoolSizeParameters resolved  = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 30, settings);

        using (Assert.Multiple())
        {
            await Assert.That(resolved.ExpansionDelayMinutes).IsEqualTo(settings.MicroPoolExpansionDelayMinutes);
            await Assert.That(resolved.ExpansionRatePerMinute).IsEqualTo(settings.MicroPoolExpansionRatePerMinute);
            await Assert.That(resolved.MaximumTMRSpread).IsEqualTo(settings.MicroPoolMaximumTMRSpread);
            await Assert.That(resolved.GroupMakeupTolerance).IsEqualTo(settings.MicroPoolGroupMakeupTolerance);
            await Assert.That(resolved.EnforcePlusZeroMinusOneCheck).IsEqualTo(settings.MicroPoolEnforcePlusZeroMinusOneCheck);
        }
    }

    [Test]
    public async Task Macro_Tier_Pulls_Macro_Settings()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        PoolSizeParameters resolved  = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 8000, settings);

        using (Assert.Multiple())
        {
            await Assert.That(resolved.ExpansionDelayMinutes).IsEqualTo(settings.MacroPoolExpansionDelayMinutes);
            await Assert.That(resolved.ExpansionRatePerMinute).IsEqualTo(settings.MacroPoolExpansionRatePerMinute);
            await Assert.That(resolved.MaximumTMRSpread).IsEqualTo(settings.MacroPoolMaximumTMRSpread);
            await Assert.That(resolved.GroupMakeupTolerance).IsEqualTo(settings.MacroPoolGroupMakeupTolerance);
            await Assert.That(resolved.EnforcePlusZeroMinusOneCheck).IsEqualTo(settings.MacroPoolEnforcePlusZeroMinusOneCheck);
        }
    }

    [Test]
    public async Task TMR_Spread_At_Baseline_Equals_Configured_Base_Difference()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        PoolSizeParameters macro = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 8000, settings);

        MatchmakingTeam teamA = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, queuedMinutesAgo: 0);
        MatchmakingTeam teamB = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, queuedMinutesAgo: 0);

        double spread = MatchmakingAlgorithm.GetMaxAcceptableTMRSpread(teamA, teamB, settings.MaximumTeamTMRDifference, macro);

        await Assert.That(spread).IsEqualTo(settings.MaximumTeamTMRDifference);
    }

    [Test]
    public async Task TMR_Spread_Past_Delay_Expands_At_Configured_Rate()
    {
        const double FiveMinuteWait = 5.0;

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        PoolSizeParameters macro = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 8000, settings);

        MatchmakingTeam teamA = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, FiveMinuteWait);
        MatchmakingTeam teamB = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, FiveMinuteWait);

        double spread = MatchmakingAlgorithm.GetMaxAcceptableTMRSpread(teamA, teamB, settings.MaximumTeamTMRDifference, macro);

        await Assert.That(Math.Abs(spread - ExpectedMacroSpreadAtFiveMinutes) < SpreadComparisonTolerance).IsTrue();
    }

    [Test]
    public async Task TMR_Spread_Beyond_Cap_Clamps_To_Configured_Maximum()
    {
        const double VeryLongWait = 60.0;

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        PoolSizeParameters macro = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 8000, settings);

        MatchmakingTeam teamA = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, VeryLongWait);
        MatchmakingTeam teamB = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, VeryLongWait);

        double spread = MatchmakingAlgorithm.GetMaxAcceptableTMRSpread(teamA, teamB, settings.MaximumTeamTMRDifference, macro);

        await Assert.That(spread).IsEqualTo(settings.MacroPoolMaximumTMRSpread);
    }

    [Test]
    public async Task TMR_Spread_Uses_Longest_Waiting_Group_Across_Both_Teams()
    {
        const double FreshWait = 0.0;
        const double OldWait   = 6.0;

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();
        PoolSizeParameters large = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 5000, settings);

        // Large Defaults: Delay = 2m, Rate = 50, Cap = 250 → 50 + (6 - 2) * 50 = 250 (Caps)
        MatchmakingTeam teamA = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, FreshWait);
        MatchmakingTeam teamB = SoloTeamAt(MatchmakingTestBuilder.BaselineTMR, OldWait);

        double spread = MatchmakingAlgorithm.GetMaxAcceptableTMRSpread(teamA, teamB, settings.MaximumTeamTMRDifference, large);

        await Assert.That(spread).IsEqualTo(settings.LargePoolMaximumTMRSpread);
    }

    private static MatchmakingTeam SoloTeamAt(double tmr, double queuedMinutesAgo)
    {
        MatchmakingGroup solo = MatchmakingTestBuilder.BuildSoloGroup(tmr, queuedMinutesAgo: queuedMinutesAgo);

        return MatchmakingTeam.FromGroups([solo], teamSize: 1);
    }
}
