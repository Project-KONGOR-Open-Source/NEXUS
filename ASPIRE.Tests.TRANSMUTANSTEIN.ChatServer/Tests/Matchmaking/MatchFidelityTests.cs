namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down the match fidelity preference: a ranked team containing a group that prefers fairer matches only ever matches within the base TMR spread, forgoing the queue-time expansion.
///     Mirrors the client's host-only, ranked-only fidelity slider ("Shorter Queues" versus "Fairer Matches").
/// </summary>
public sealed class MatchFidelityTests
{
    /// <summary>
    ///     The TMRs of the two 5-stacks used across these tests. Their effective rating difference (≈ 256 after power-mean inflation) exceeds the base spread of 50 but sits well within the micro pool's expanded spread at 5 minutes (1175).
    /// </summary>
    private const double LowTMR  = 1500.0;
    private const double HighTMR = 1700.0;

    private const double QueuedMinutes = 5.0;

    [Test]
    public async Task Teams_Without_A_Fidelity_Preference_Match_Within_The_Expanded_Spread()
    {
        IReadOnlyList<MatchmakingMatch> matches = RunCycle(MatchmakingTestBuilder.Information(matchFidelity: 0), MatchmakingTestBuilder.Information(matchFidelity: 0));

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task A_Fidelity_Preference_Limits_Matching_To_The_Base_Spread()
    {
        IReadOnlyList<MatchmakingMatch> matches = RunCycle(MatchmakingTestBuilder.Information(matchFidelity: 1), MatchmakingTestBuilder.Information(matchFidelity: 0));

        await Assert.That(matches.Count).IsEqualTo(0);
    }

    [Test]
    public async Task A_Fidelity_Preference_Is_Ignored_For_Unranked_Teams()
    {
        IReadOnlyList<MatchmakingMatch> matches = RunCycle(MatchmakingTestBuilder.Information(ranked: false, matchFidelity: 1), MatchmakingTestBuilder.Information(ranked: false, matchFidelity: 0));

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    [Test]
    public async Task A_Fidelity_Team_Still_Matches_Within_The_Base_Spread()
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup fidelityStack = BuildFullStack(LowTMR, MatchmakingTestBuilder.Information(matchFidelity: 1));
        MatchmakingGroup regularStack  = BuildFullStack(LowTMR, MatchmakingTestBuilder.Information(matchFidelity: 0));

        IReadOnlyList<MatchmakingMatch> matches = MatchmakingAlgorithm.RunMatchBrokerCycle([fidelityStack, regularStack], settings);

        await Assert.That(matches.Count).IsEqualTo(1);
    }

    private static IReadOnlyList<MatchmakingMatch> RunCycle(MatchmakingGroupInformation lowStackInformation, MatchmakingGroupInformation highStackInformation)
    {
        MatchmakingSettings settings = MatchmakingTestBuilder.DefaultSettings();

        MatchmakingGroup lowStack  = BuildFullStack(LowTMR, lowStackInformation);
        MatchmakingGroup highStack = BuildFullStack(HighTMR, highStackInformation);

        return MatchmakingAlgorithm.RunMatchBrokerCycle([lowStack, highStack], settings);
    }

    private static MatchmakingGroup BuildFullStack(double tmr, MatchmakingGroupInformation information)
        => MatchmakingTestBuilder.BuildGroup([.. Enumerable.Repeat(tmr, 5)], queuedMinutesAgo: QueuedMinutes, information: information);
}
