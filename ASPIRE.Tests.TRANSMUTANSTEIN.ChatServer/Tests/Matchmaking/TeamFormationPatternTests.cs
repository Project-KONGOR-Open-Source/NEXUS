namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down the phased pattern matching in <see cref="MatchmakingAlgorithm.FormTeams"/>.
///     A 5v5 queue is consumed in the order [5], [4+1], [3+2], [3+1+1], [2+2+1], [2+1+1+1], [1+1+1+1+1], with FIFO group selection within each pattern.
/// </summary>
public sealed class TeamFormationPatternTests
{
    /// <summary>
    ///     The TMRs used in the best-opponent test. The pair (Low, NearLow) sits within the default base spread of 50 once the power-mean inflation is applied; (Low, FarHigh) does not.
    /// </summary>
    private const double LowTMR     = 1500.0;
    private const double NearLowTMR = 1505.0;
    private const double FarHighTMR = 1900.0;

    [Test]
    public async Task Two_Five_Stacks_Form_Two_Full_Teams()
    {
        List<MatchmakingGroup> queue =
        [
            BuildBaselineGroup(5),
            BuildBaselineGroup(5)
        ];

        IReadOnlyList<MatchmakingTeam> teams = MatchmakingAlgorithm.FormTeams(queue, playersPerTeam: 5);

        using (Assert.Multiple())
        {
            await Assert.That(teams.Count).IsEqualTo(2);
            await Assert.That(teams.All(team => team.GroupMakeup == 25)).IsTrue();
            await Assert.That(teams.All(team => team.PlayerCount == 5)).IsTrue();
        }
    }

    /// <summary>
    ///     Each row exercises one pattern phase. <paramref name="groupSizes"/> is the queue layout; the algorithm must produce exactly one full team with the expected makeup string.
    /// </summary>
    [Test]
    [Arguments(new[] { 4, 1 },          "4+1")]
    [Arguments(new[] { 3, 2 },          "3+2")]
    [Arguments(new[] { 3, 1, 1 },       "3+1+1")]
    [Arguments(new[] { 2, 2, 1 },       "2+2+1")]
    [Arguments(new[] { 2, 1, 1, 1 },    "2+1+1+1")]
    [Arguments(new[] { 1, 1, 1, 1, 1 }, "1+1+1+1+1")]
    public async Task Pattern_Phase_Forms_Exactly_One_Full_Team(int[] groupSizes, string expectedPattern)
    {
        List<MatchmakingGroup> queue = [.. groupSizes.Select(BuildBaselineGroup)];

        IReadOnlyList<MatchmakingTeam> teams = MatchmakingAlgorithm.FormTeams(queue, playersPerTeam: 5);

        using (Assert.Multiple())
        {
            await Assert.That(teams.Count).IsEqualTo(1);
            await Assert.That(teams[0].GroupMakeupString).IsEqualTo(expectedPattern);
            await Assert.That(teams[0].PlayerCount).IsEqualTo(5);
        }
    }

    [Test]
    public async Task FIFO_Ordering_Longest_Waiting_Four_Stack_Takes_The_Solo()
    {
        // Three Four-Stacks Queued At Different Times, Plus One Solo
        // The Earliest Four-Stack Should Pair With The Solo (Forming A 4+1) Before The Others

        const double OldestQueueMinutes   = 5.0;
        const double MiddleQueueMinutes   = 3.0;
        const double YoungestQueueMinutes = 1.0;

        MatchmakingGroup oldestFourStack   = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: OldestQueueMinutes);
        MatchmakingGroup middleFourStack   = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: MiddleQueueMinutes);
        MatchmakingGroup youngestFourStack = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR], queuedMinutesAgo: YoungestQueueMinutes);
        MatchmakingGroup solo              = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, queuedMinutesAgo: 0);

        List<MatchmakingGroup> queue = [middleFourStack, youngestFourStack, oldestFourStack, solo];

        IReadOnlyList<MatchmakingTeam> teams = MatchmakingAlgorithm.FormTeams(queue, playersPerTeam: 5);

        IEnumerable<MatchmakingTeam> fourPlusOneTeams = teams.Where(team => team.GroupMakeupString == "4+1");

        await Assert.That(fourPlusOneTeams.Count()).IsEqualTo(1);

        MatchmakingTeam fourPlusOne = fourPlusOneTeams.Single();

        bool oldestStackUsed = fourPlusOne.Groups.Any(group => group.GUID == oldestFourStack.GUID);

        await Assert.That(oldestStackUsed).IsTrue();
    }

    [Test]
    public async Task Phase_Ordering_Full_Stacks_Form_Before_Solos_Are_Combined()
    {
        // One Full Stack And Six Solos; Phase 1 Forms The Five-Stack Team; Phase 5 Stitches Five Of The Six Solos Into A Solo Team; The Sixth Solo Is Left Over

        MatchmakingGroup fiveStack = BuildBaselineGroup(5);

        List<MatchmakingGroup> queue = [fiveStack];

        for (int index = 0; index < 6; index++)
            queue.Add(MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR));

        IReadOnlyList<MatchmakingTeam> teams = MatchmakingAlgorithm.FormTeams(queue, playersPerTeam: 5);

        using (Assert.Multiple())
        {
            await Assert.That(teams.Count).IsEqualTo(2);
            await Assert.That(teams.Any(team => team.GroupMakeupString == "5")).IsTrue();
            await Assert.That(teams.Any(team => team.GroupMakeupString == "1+1+1+1+1")).IsTrue();
        }
    }

    [Test]
    public async Task Incomplete_Pattern_Leaves_Groups_Unused()
    {
        // One Trio Plus One Solo; No Way To Form A 5-Player Team With This Composition

        List<MatchmakingGroup> queue =
        [
            BuildBaselineGroup(3),
            MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR)
        ];

        IReadOnlyList<MatchmakingTeam> teams = MatchmakingAlgorithm.FormTeams(queue, playersPerTeam: 5);

        await Assert.That(teams.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Best_Opponent_Selection_Chooses_Closest_TMR_Match_Among_Multiple_Candidates()
    {
        // Three Five-Stack Teams With Distinct Effective Ratings; The Broker Should Pair The Two Closest Teams (A And B), Leaving C Unmatched
        // Without The "Best Opponent" Logic, If The Algorithm Just Pair The First Compatible Opponent, The Test Would Fail Because A Could Be Paired With C First

        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup teamA = MatchmakingTestBuilder.BuildGroup([LowTMR, LowTMR, LowTMR, LowTMR, LowTMR]);
        MatchmakingGroup teamB = MatchmakingTestBuilder.BuildGroup([NearLowTMR, NearLowTMR, NearLowTMR, NearLowTMR, NearLowTMR]);
        MatchmakingGroup teamC = MatchmakingTestBuilder.BuildGroup([FarHighTMR, FarHighTMR, FarHighTMR, FarHighTMR, FarHighTMR]);

        PoolSizeParameters tier = MatchmakingAlgorithm.ResolvePoolSizeParameters(queuedPlayerCount: 10000, settings);

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([teamA, teamB, teamC], settings, tier);

        await Assert.That(matches.Count).IsEqualTo(1);

        MatchmakingMatch match = matches[0];

        bool matchIsAvsB = match.GetAllGroups().Any(group => group.GUID == teamA.GUID)
            && match.GetAllGroups().Any(group => group.GUID == teamB.GUID);

        bool teamCExcluded = match.GetAllGroups().All(group => group.GUID != teamC.GUID);

        using (Assert.Multiple())
        {
            await Assert.That(matchIsAvsB).IsTrue();
            await Assert.That(teamCExcluded).IsTrue();
        }
    }

    private static MatchmakingGroup BuildBaselineGroup(int memberCount)
        => MatchmakingTestBuilder.BuildGroup([.. Enumerable.Repeat(MatchmakingTestBuilder.BaselineTMR, memberCount)]);
}
