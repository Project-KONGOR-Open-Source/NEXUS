namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Covers the readiness and loading reset that the chat server applies to a group when it is matched into a game.
///     The reset mirrors the original chat server's <c>UnloadAndUnreadyPlayers</c>, which the client relies upon to dismiss its matchmaking loading overlay once a match is found (a party group is kept alive on the client, so the chat server must drive the reset).
/// </summary>
public sealed class MatchmakingGroupReadinessTests
{
    [Test]
    public async Task Unloading_And_Unreadying_A_Party_Group_Leaves_Only_The_Leader_Not_Ready_And_Resets_All_Loading_Progress()
    {
        // A Freshly Matched Party Group Begins With Every Member Ready And Fully Loaded
        MatchmakingGroup group = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);

        group.UnloadAndUnreadyMembers();

        using (Assert.Multiple())
        {
            await Assert.That(group.Leader.IsReady).IsFalse();
            await Assert.That(group.Members.Where(member => member.IsLeader is false).All(member => member.IsReady)).IsTrue();
            await Assert.That(group.Members.All(member => member.LoadingPercent is 0)).IsTrue();
        }
    }

    [Test]
    public async Task Unloading_And_Unreadying_A_Solo_Group_Resets_Its_Leaders_Readiness_And_Loading_Progress()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR);

        group.UnloadAndUnreadyMembers();

        using (Assert.Multiple())
        {
            await Assert.That(group.Leader.IsReady).IsFalse();
            await Assert.That(group.Leader.LoadingPercent).IsEqualTo((byte)0);
        }
    }
}
