namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Locks down the match commitment protocol: the broker re-validates every group in a proposed match at spawn time via <see cref="MatchmakingService.TryClaimMatchGroups"/>, and a user request to leave the queue is refused once the group is committed to a match.
/// </summary>
public sealed class MatchCommitmentTests
{
    [Test]
    public async Task A_Queued_Group_Can_Be_Claimed()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR);

        await Assert.That(group.TryClaimForMatch()).IsTrue();
    }

    [Test]
    public async Task A_Group_That_Left_The_Queue_Cannot_Be_Claimed()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, queuedMinutesAgo: -1);

        await Assert.That(group.TryClaimForMatch()).IsFalse();
    }

    [Test]
    public async Task A_Match_With_Queued_Full_Teams_Can_Be_Claimed()
    {
        MatchmakingMatch match = BuildMatch(out _, out _);

        await Assert.That(MatchmakingService.TryClaimMatchGroups(match)).IsTrue();
    }

    [Test]
    public async Task A_Match_Cannot_Be_Claimed_After_A_Group_Leaves_The_Queue()
    {
        MatchmakingMatch match = BuildMatch(out MatchmakingGroup legionGroup, out _);

        legionGroup.QueueStartTime = null;

        await Assert.That(MatchmakingService.TryClaimMatchGroups(match)).IsFalse();
    }

    [Test]
    public async Task A_Match_Cannot_Be_Claimed_After_A_Team_Loses_A_Member()
    {
        MatchmakingMatch match = BuildMatch(out MatchmakingGroup legionGroup, out _);

        legionGroup.Members.RemoveAt(legionGroup.Members.Count - 1);

        await Assert.That(MatchmakingService.TryClaimMatchGroups(match)).IsFalse();
    }

    [Test]
    public async Task A_Bot_Match_With_A_Partial_Group_Can_Be_Claimed()
    {
        MatchmakingGroup coopGroup = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR], information: MatchmakingTestBuilder.CoopInformation());

        MatchmakingMatch botMatch = MatchmakingMatch.FromBotGroup(coopGroup);

        await Assert.That(MatchmakingService.TryClaimMatchGroups(botMatch)).IsTrue();
    }

    [Test]
    public async Task A_User_Leave_Request_Is_Refused_Once_The_Group_Is_Committed_To_A_Match()
    {
        BuildMatch(out MatchmakingGroup legionGroup, out _);

        bool leftQueue = legionGroup.LeaveQueue();

        using (Assert.Multiple())
        {
            await Assert.That(leftQueue).IsFalse();
            await Assert.That(legionGroup.IsQueued).IsTrue();
        }
    }

    [Test]
    public async Task A_Leave_Request_For_An_Unqueued_Group_Is_Refused()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, queuedMinutesAgo: -1);

        await Assert.That(group.LeaveQueue()).IsFalse();
    }

    /// <summary>
    ///     Builds a committed match between two queued 5-stacks; <see cref="MatchmakingMatch.FromTeams"/> marks both groups as matched up.
    /// </summary>
    private static MatchmakingMatch BuildMatch(out MatchmakingGroup legionGroup, out MatchmakingGroup hellbourneGroup)
    {
        legionGroup = MatchmakingTestBuilder.BuildGroup([.. Enumerable.Repeat(MatchmakingTestBuilder.BaselineTMR, 5)]);
        hellbourneGroup = MatchmakingTestBuilder.BuildGroup([.. Enumerable.Repeat(MatchmakingTestBuilder.BaselineTMR, 5)]);

        MatchmakingTeam legionTeam = MatchmakingTeam.FromGroups([legionGroup], teamSize: 5);
        MatchmakingTeam hellbourneTeam = MatchmakingTeam.FromGroups([hellbourneGroup], teamSize: 5);

        return MatchmakingMatch.FromTeams(legionTeam, hellbourneTeam);
    }
}
