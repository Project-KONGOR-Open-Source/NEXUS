namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Drives the algorithm against pools that resolve to <see cref="PoolSizeTier.Micro"/> (under 50 queued players).
/// </summary>
public sealed class MicroPoolMatchmakingTests
{
    /// <summary>
    ///     The TMR span used to build solos that fall just outside the default base spread (<see cref="MatchmakingSettings.MaximumTeamTMRDifference"/> = 50). Two solos this far apart cannot match without queue-time expansion.
    /// </summary>
    private const double WiderThanBaseSpreadDelta = 200.0;

    /// <summary>
    ///     A queue-wait long enough to push the Micro-tier expansion past the 200 TMR delta: <c>50 + (2 - 0.5) * 250 = 425</c>.
    /// </summary>
    private const double SufficientQueueMinutesForMicroExpansion = 2.0;

    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        MatchmakingTestBuilder.ResetAccountIDCounter();

        return Task.CompletedTask;
    }

    [Test]
    public async Task TenSolosAllAtSameRating_FormOneFiveVersusFiveMatch()
    {
        // Ten Solos At Identical TMR; Phase 5 Of FormTeams Will Stitch Them Into Two 1+1+1+1+1 Teams Of Equal Effective Rating

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue = [];

        for (int index = 0; index < 10; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task FiveStackVersusFiveSolos_IsAllowedDespiteHugeMakeupGap()
    {
        // Micro Tolerance Is 20, And A 5-Stack Vs 1+1+1+1+1 Has Makeup Difference = 25 - 5 = 20 (At The Tolerance, So Allowed)

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup fiveStack = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);

        List<MatchmakingGroup> queue = [fiveStack];

        for (int index = 0; index < 5; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        await Assert.That(matches.Count).IsEqualTo(1);

        MatchmakingMatch match = matches[0];

        using (Assert.Multiple())
        {
            await Assert.That(match.LegionTeam.PlayerCount).IsEqualTo(5);
            await Assert.That(match.HellbourneTeam!.PlayerCount).IsEqualTo(5);
            await Assert.That(match.MismatchedGroupMakeup).IsFalse();
        }
    }

    [Test]
    public async Task PlusZeroMinusOneCheck_IsDisabledInMicroPools()
    {
        // Build A Team Whose Highest Player Is 700 Above The Average; Would Be Rejected In Medium Or Above, But Allowed Here

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

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        // Micro Pool Should Form A Match Despite Both Teams Producing +0/-1 Outcomes
        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TmrExpansion_QueueTimeWidensSpread()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        double belowBaseline = MatchmakingTestBuilder.BaselineTMR - WiderThanBaseSpreadDelta / 2;
        double aboveBaseline = MatchmakingTestBuilder.BaselineTMR + WiderThanBaseSpreadDelta / 2;

        // Fresh Queue: Difference Of 200 > Default Base Spread Of 50 → No Match
        List<MatchmakingGroup> freshQueue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(belowBaseline, queuedMinutesAgo: 0),
            MatchmakingTestBuilder.BuildSoloGroup(aboveBaseline, queuedMinutesAgo: 0)
        ];

        // Mature Queue: After 2 Minutes, Spread Expands To 50 + (2 - 0.5) * 250 = 425 → Now Allows The 200 Difference
        List<MatchmakingGroup> matureQueue =
        [
            MatchmakingTestBuilder.BuildSoloGroup(belowBaseline, queuedMinutesAgo: SufficientQueueMinutesForMicroExpansion),
            MatchmakingTestBuilder.BuildSoloGroup(aboveBaseline, queuedMinutesAgo: SufficientQueueMinutesForMicroExpansion)
        ];

        IReadOnlyList<MatchmakingMatch> freshMatches  = MatchmakingAlgorithm.RunMatchBrokerCycle(freshQueue,  settings);
        IReadOnlyList<MatchmakingMatch> matureMatches = MatchmakingAlgorithm.RunMatchBrokerCycle(matureQueue, settings);

        using (Assert.Multiple())
        {
            await Assert.That(freshMatches.Count).IsEqualTo(0);
            await Assert.That(matureMatches.Count).IsEqualTo(1);
        }
    }

    [Test]
    public async Task GameTypeIsolation_NormalGroupsNotMatchedAgainstMidWarsGroups()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        MatchmakingGroupInformation normal  = MatchmakingTestBuilder.Information(gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        MatchmakingGroupInformation midwars = MatchmakingTestBuilder.Information(gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: normal),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: midwars)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RegionFiltering_AppliesAtPatternLevelOnlyNotTeamPairingLevel()
    {
        // Documents A Subtle Behaviour: Region Compatibility Is Only Enforced When Multiple Groups Are Stitched Onto One Team Inside "FormTeamsWithPattern"
        // For 1v1 Or Same-Sized-Solo Pairings, Each Group Becomes Its Own Team, And Team-Level "IsCompatibleWith" Checks Only Player Count And The (Always-Zero) Region Flags
        // The Result: Two 1v1 Solos In Disjoint Regions Without The NEWERTH Wildcard Will Still Match; If The Algorithm Is Ever Tightened To Filter Regions At Pairing Level, This Test Will Need Updating

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        MatchmakingGroupInformation useOnly = MatchmakingTestBuilder.Information(gameRegions: ["USE"]);
        MatchmakingGroupInformation euOnly  = MatchmakingTestBuilder.Information(gameRegions: ["EU"]);

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: useOnly),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: euOnly)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task RankedAndUnrankedGroups_DoNotMatchAgainstEachOther()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        MatchmakingGroupInformation ranked   = MatchmakingTestBuilder.Information(ranked: true);
        MatchmakingGroupInformation unranked = MatchmakingTestBuilder.Information(ranked: false);

        // Two Distinct Solos In Each Bucket So The 1+1 Pattern Can Form Within Each Group, And Then Paired Across
        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: ranked),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: ranked),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: unranked),
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: unranked)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        // Each Pair (Ranked, Unranked) Forms Independently; Ranked And Unranked Never Cross
        using (Assert.Multiple())
        {
            await Assert.That(matches.Count).IsEqualTo(2);
            await Assert.That(matches.All(match => match.LegionTeam.Groups[0].Information.Ranked == match.HellbourneTeam!.Groups[0].Information.Ranked)).IsTrue();
        }
    }
}
