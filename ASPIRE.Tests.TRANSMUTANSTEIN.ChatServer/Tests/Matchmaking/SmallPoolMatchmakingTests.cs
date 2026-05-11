namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Drives the algorithm against pools that resolve to <see cref="PoolSizeTier.Small"/>.
/// </summary>
public sealed class SmallPoolMatchmakingTests
{
    /// <summary>
    ///     Number Of Padding Solos Added To Push The Queue Into The Small Tier (≥ 50). Set Just Above The Tier Threshold So The Tier Is Unambiguous.
    /// </summary>
    private const int SmallTierPaddingCount = 50;

    /// <summary>
    ///     A TMR Far From <see cref="MatchmakingTestBuilder.BaselineTMR"/> Used For Padding Solos So They Cannot Cross-Match With Outlier Or Test Subject Teams.
    /// </summary>
    private const double FarPaddingTMR = 2500.0;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        MatchmakingTestBuilder.ResetAccountIDCounter();

        return Task.CompletedTask;
    }

    [Test]
    public async Task TwoFiveStacks_AreMatchedAgainstEachOther()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR])
        ];

        for (int index = 0; index < SmallTierPaddingCount + 10; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(FarPaddingTMR + index));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        IEnumerable<MatchmakingMatch> stackMatches = matches.Where(match => match.LegionTeam.GroupMakeup == 25 && match.HellbourneTeam!.GroupMakeup == 25);

        await Assert.That(stackMatches.Count()).IsEqualTo(1);
    }

    [Test]
    public async Task MakeupTolerance_5StackVersusSolosIsRejectedAt8()
    {
        // Small Tolerance = 8, And A 5-Stack (25) Vs Five Solos (5) Has A Diff Of 20: Rejected

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR])
        ];

        for (int index = 0; index < 5; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR));

        for (int index = 0; index < SmallTierPaddingCount; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(FarPaddingTMR + index));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        bool fiveStackMatched = matches.Any(match => match.LegionTeam.GroupMakeup == 25 || (match.HellbourneTeam?.GroupMakeup ?? 0) == 25);

        await Assert.That(fiveStackMatched).IsFalse();
    }

    [Test]
    public async Task MakeupTolerance_4Plus1Versus3Plus2IsAccepted()
    {
        // Difference = 17 - 13 = 4, Within Small Tolerance Of 8

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue =
        [
            // 4+1
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),

            // 3+2
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR])
        ];

        for (int index = 0; index < SmallTierPaddingCount; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(FarPaddingTMR + index));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        bool foundMix = matches.Any(match => (match.LegionTeam.GroupMakeup == 17 && match.HellbourneTeam!.GroupMakeup == 13)
            || (match.LegionTeam.GroupMakeup == 13 && match.HellbourneTeam!.GroupMakeup == 17));

        await Assert.That(foundMix).IsTrue();
    }

    [Test]
    public async Task PlusZeroMinusOneCheck_IsStillDisabledInSmallPools()
    {
        // Forces The Small Pool Tier (≥ 50 Queued Players) Manually Rather Than Via The Resolver, Then Verifies That The +0/-1 Check Does Not Reject A Pairing Of Two Outlier Teams
        // Verifies By Checking That The OUTLIER TEAMS SPECIFICALLY Get Paired (Not Just That Some Match Forms, Which Could Pass Even If The Check Was Wrongly Enabled And Padding Solos Matched Among Themselves)

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

        for (int index = 0; index < SmallTierPaddingCount; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(FarPaddingTMR + index));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        bool outlierTeamsPaired = matches.Any(match =>
        {
            double legionHighest     = match.LegionTeam.HighestTMR;
            double hellbourneHighest = match.HellbourneTeam?.HighestTMR ?? 0;

            return legionHighest >= MatchmakingTestBuilder.OutlierHighTMR && hellbourneHighest >= MatchmakingTestBuilder.OutlierHighTMR;
        });

        await Assert.That(outlierTeamsPaired).IsTrue();
    }
}
