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

    [Test]
    public async Task Ten_Solos_All_At_Same_Rating_Form_One_Five_Versus_Five_Match()
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
    public async Task Five_Stack_Versus_Five_Solos_Is_Allowed_Despite_Huge_Makeup_Gap()
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
    public async Task Plus_Zero_Minus_One_Check_Is_Disabled_In_Micro_Pools()
    {
        // Build Teams Whose Highest Player Is 700 Above The Average; Would Be Rejected In Medium Or Above, But Allowed Here
        // The Outlier Sits Inside A Premade 4-Stack So That The Rating-Proximity Slot Filling Cannot Separate The Outliers Into Better-Balanced Teams

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        List<MatchmakingGroup> queue =
        [
            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.OutlierHighTMR]),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR),

            MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.OutlierHighTMR]),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR)
        ];

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle(queue, settings);

        // Micro Pool Should Form A Match Despite Both Teams Producing +0/-1 Outcomes
        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task TMR_Expansion_Queue_Time_Widens_Spread()
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
    public async Task Game_Type_Isolation_Normal_Groups_Not_Matched_Against_MidWars_Groups()
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
    public async Task Region_Filtering_Applies_At_Team_Pairing_Level()
    {
        // Region Compatibility Is Enforced Both When Stitching Groups Onto A Team And When Pairing Teams
        // Two 1v1 Solos In Disjoint Regions Without The NEWERTH Wildcard Each Become Their Own Team And Must Not Be Paired

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

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Ranked_And_Unranked_Groups_Do_Not_Match_Against_Each_Other()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        settings.PlayersPerTeam = 1;

        MatchmakingGroupInformation ranked   = MatchmakingTestBuilder.Information(ranked: true);
        MatchmakingGroupInformation unranked = MatchmakingTestBuilder.Information(ranked: false);

        // Two Distinct Solos In Each Partition So The 1+1 Pattern Can Form Within Each Group, And Then Paired Across
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
