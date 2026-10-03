namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Statistics;

/// <summary>
///     Pure-logic tests for <see cref="MatchSummary"/>, covering the winning-team resolution for completed and aborted matches.
/// </summary>
public sealed class MatchSummaryTests_Unit
{
    /// <summary>
    ///     The constructor for <see cref="MatchSummary"/> resolves the replay file host from the "INFRASTRUCTURE_GATEWAY" environment variable, so it must be present when a summary is built.
    /// </summary>
    /// <remarks>
    ///     The integration factory sets the same value, but this test is a pure-logic unit test and does not use an integration factory.
    /// </remarks>
    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        Environment.SetEnvironmentVariable("INFRASTRUCTURE_GATEWAY", "localhost");

        return Task.CompletedTask;
    }

    [Test]
    public async Task Winning_Team_Is_Zero_When_No_Participant_Has_A_Win()
    {
        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchStatistics matchStatistics = MatchDataHelper.BuildMatchStatistics();

        // An Aborted Or Cancelled Match Has No Winning Participant (Every Participant Has Win = 0)
        List<MatchParticipantStatistics> participants =
        [
            MatchDataHelper.BuildParticipant(accountID: 1, accountName: "PlayerOne", groupNumber: -1),
            MatchDataHelper.BuildParticipant(accountID: 2, accountName: "PlayerTwo", groupNumber: -1)
        ];

        MatchSummary summary = new (matchStatistics, participants, matchInformation);

        await Assert.That(summary.WinningTeam).IsEqualTo("0");
    }

    [Test]
    public async Task Winning_Team_Is_The_Team_Of_The_Winning_Participant()
    {
        MatchInformation matchInformation = MatchDataHelper.BuildMatchInformation(MatchType.AM_PUBLIC);
        MatchStatistics matchStatistics = MatchDataHelper.BuildMatchStatistics();

        // The Winning Participant Is On Team 1 (The Builder Assigns Every Participant To Team 1)
        List<MatchParticipantStatistics> participants =
        [
            MatchDataHelper.BuildParticipant(accountID: 1, accountName: "Winner", groupNumber: -1, win: 1),
            MatchDataHelper.BuildParticipant(accountID: 2, accountName: "Loser", groupNumber: -1, loss: 1)
        ];

        MatchSummary summary = new (matchStatistics, participants, matchInformation);

        await Assert.That(summary.WinningTeam).IsEqualTo("1");
    }
}
