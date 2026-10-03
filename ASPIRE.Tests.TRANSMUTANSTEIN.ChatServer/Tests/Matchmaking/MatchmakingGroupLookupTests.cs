namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Covers the matchmaking group registry lookup, which identifies a group member by either their account ID or their account name.
///     Both identifier kinds are used by the command processors: most look up the requesting account by ID, while invite rejection looks up the inviter by name.
/// </summary>
public sealed class MatchmakingGroupLookupTests
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        MatchmakingService.Groups.Clear();

        return Task.CompletedTask;
    }

    [After(HookType.Test)]
    public Task After_Each_Test()
    {
        MatchmakingService.Groups.Clear();

        return Task.CompletedTask;
    }

    [Test]
    [NotInParallel]
    public async Task Group_Is_Found_By_The_Account_ID_Or_The_Account_Name_Of_Any_Of_Its_Members()
    {
        MatchmakingGroup group = MatchmakingTestBuilder.BuildGroup([MatchmakingTestBuilder.BaselineTMR, MatchmakingTestBuilder.BaselineTMR]);

        MatchmakingService.Groups.TryAdd(group.Leader.Account.ID, group);

        MatchmakingGroupMember follower = group.Members.Single(member => member.IsLeader is false);

        int unknownAccountID = group.Members.Max(member => member.Account.ID) + 1;
        string unknownAccountName = $"{group.Leader.Account.Name}-unknown";

        using (Assert.Multiple())
        {
            await Assert.That(MatchmakingService.GetMatchmakingGroup(group.Leader.Account.ID)).IsEqualTo(group);
            await Assert.That(MatchmakingService.GetMatchmakingGroup(follower.Account.ID)).IsEqualTo(group);
            await Assert.That(MatchmakingService.GetMatchmakingGroup(group.Leader.Account.Name)).IsEqualTo(group);
            await Assert.That(MatchmakingService.GetMatchmakingGroup(follower.Account.Name)).IsEqualTo(group);
            await Assert.That(MatchmakingService.GetMatchmakingGroup(unknownAccountID)).IsNull();
            await Assert.That(MatchmakingService.GetMatchmakingGroup(unknownAccountName)).IsNull();
        }
    }
}
