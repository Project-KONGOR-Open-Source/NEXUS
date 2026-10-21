namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for creating match locks: a match's roster is snapshotted when the match is committed to its match server, and every committed player of a competitive matchmade match, other than a guest or staff account, is locked to the match once its match server announces it.
///     Every test which writes a lock removes it, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock left behind would bind a user of a later test.
/// </summary>
public sealed class MatchLockCreationTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    private const string ServerAddress = "127.0.0.1";

    private const ushort ServerPort = 11235;

    [Test]
    public async Task Committing_The_Roster_Keeps_Players_Who_Later_Leave_Their_Group()
    {
        MatchmakingMatch match = BuildCompetitiveMatch(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL);
        match.CommitRoster();

        match.LegionTeam.Groups.Single().Members.Clear();

        await Assert.That(match.CommittedRoster.Count).IsEqualTo(2);
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    [Arguments(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL, MatchType.AM_MATCHMAKING_CAMPAIGN)]
    [Arguments(ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS, MatchType.AM_MATCHMAKING_MIDWARS)]
    [Arguments(ChatProtocol.TMMGameType.TMM_GAME_TYPE_RIFTWARS, MatchType.AM_MATCHMAKING_RIFTWARS)]
    public async Task Locking_A_Competitive_Match_Locks_Every_Committed_Player(ChatProtocol.TMMGameType gameType, MatchType arrangedMatchType)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        MatchmakingMatch match = BuildCompetitiveMatch(gameType);

        match.CommitRoster();

        int matchID = Random.Shared.Next(1_000_000, int.MaxValue);
        int serverID = Random.Shared.Next(1_000_000, int.MaxValue);

        try
        {
            DateTimeOffset calledAt = DateTimeOffset.UtcNow;

            await host.Services.GetRequiredService<MatchLockService>().LockMatchRoster(match, matchID, serverID, ServerAddress, ServerPort);

            foreach (MatchmakingGroupMember member in match.CommittedRoster)
            {
                MatchLock? matchLock = await distributedCacheStore.GetMatchLock(member.Account.User.ID);

                await Assert.That(matchLock).IsNotNull();

                using (Assert.Multiple())
                {
                    await Assert.That(matchLock!.MatchID).IsEqualTo(matchID);
                    await Assert.That(matchLock.CorrelationID).IsEqualTo(match.CorrelationID);
                    await Assert.That(matchLock.ServerID).IsEqualTo(serverID);
                    await Assert.That(matchLock.ServerAddress).IsEqualTo(ServerAddress);
                    await Assert.That(matchLock.ServerPort).IsEqualTo(ServerPort);
                    await Assert.That(matchLock.AccountID).IsEqualTo(member.Account.ID);
                    await Assert.That(matchLock.AccountName).IsEqualTo(member.Account.Name);
                    await Assert.That(matchLock.ArrangedMatchType).IsEqualTo(arrangedMatchType);
                    await Assert.That(matchLock.TimestampStarted).IsEqualTo(calledAt).Within(TimeSpan.FromSeconds(5));
                }
            }
        }

        finally
        {
            await RemoveLocks(distributedCacheStore, match.CommittedRoster.Select(member => member.Account.User.ID), matchID);
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Locking_A_Competitive_Match_Records_The_Clan_Tagged_Name_Of_Each_Player()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        MatchmakingMatch match = BuildCompetitiveMatch(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL);

        // The First Player Is A Real Account Which Is In A Clan, And The Second Has No Clan
        (int clanMemberAccountID, _) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string clanMemberNameWithClanTag = await ChatTestData.SeedClanMember(host.Services, clanMemberAccountID);

        await using (AsyncServiceScope scope = host.Services.CreateAsyncScope())
        {
            MerrickContext databaseContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            match.GetAllPlayers().First().Account = await databaseContext.Accounts
                .Include(account => account.User)
                .Include(account => account.Clan)
                .SingleAsync(account => account.ID == clanMemberAccountID);
        }

        match.CommitRoster();

        int clanMemberUserID = match.CommittedRoster.First().Account.User.ID;

        int matchID = Random.Shared.Next(1_000_000, int.MaxValue);

        try
        {
            await host.Services.GetRequiredService<MatchLockService>().LockMatchRoster(match, matchID, Random.Shared.Next(1_000_000, int.MaxValue), ServerAddress, ServerPort);

            foreach (MatchmakingGroupMember member in match.CommittedRoster)
            {
                MatchLock? matchLock = await distributedCacheStore.GetMatchLock(member.Account.User.ID);

                await Assert.That(matchLock).IsNotNull();

                using (Assert.Multiple())
                {
                    await Assert.That(matchLock!.AccountName).IsEqualTo(member.Account.Name);
                    await Assert.That(matchLock.AccountNameWithClanTag).IsEqualTo(member.Account.NameWithClanTag);
                }
            }

            MatchLock clanMemberLock = (await distributedCacheStore.GetMatchLock(clanMemberUserID))!;

            await Assert.That(clanMemberLock.AccountNameWithClanTag).IsEqualTo(clanMemberNameWithClanTag);
        }

        finally
        {
            await RemoveLocks(distributedCacheStore, match.CommittedRoster.Select(member => member.Account.User.ID), matchID);
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Locking_A_Co_Op_Match_Locks_Nobody()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        MatchmakingMatch match = MatchmakingMatch.FromBotGroup(MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, information: MatchmakingTestBuilder.CoopInformation()));

        AssignUniqueUserIDs(match);

        match.CommitRoster();

        int matchID = Random.Shared.Next(1_000_000, int.MaxValue);

        try
        {
            await host.Services.GetRequiredService<MatchLockService>().LockMatchRoster(match, matchID, Random.Shared.Next(1_000_000, int.MaxValue), ServerAddress, ServerPort);

            await Assert.That(await distributedCacheStore.GetMatchLock(match.CommittedRoster.Single().Account.User.ID)).IsNull();
        }

        finally
        {
            await RemoveLocks(distributedCacheStore, match.CommittedRoster.Select(member => member.Account.User.ID), matchID);
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    [Arguments(AccountType.Guest)]
    [Arguments(AccountType.Staff)]
    public async Task Guest_And_Staff_Players_Are_Never_Locked(AccountType accountType)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        MatchmakingMatch match = BuildCompetitiveMatch(ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL);

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Account.Type = accountType;

        match.CommitRoster();

        int matchID = Random.Shared.Next(1_000_000, int.MaxValue);

        try
        {
            await host.Services.GetRequiredService<MatchLockService>().LockMatchRoster(match, matchID, Random.Shared.Next(1_000_000, int.MaxValue), ServerAddress, ServerPort);

            foreach (MatchmakingGroupMember member in match.CommittedRoster)
                await Assert.That(await distributedCacheStore.GetMatchLock(member.Account.User.ID)).IsNull();
        }

        finally
        {
            await RemoveLocks(distributedCacheStore, match.CommittedRoster.Select(member => member.Account.User.ID), matchID);
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Announcing_A_Match_Locks_Its_Committed_Roster_Including_A_Player_Who_Left()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        (int legionAccountID, string legionAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);
        (int hellbourneAccountID, string hellbourneAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string legionCookie = Guid.CreateVersion7().ToString();
        string hellbourneCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, legionCookie, legionAccountName);
        await ChatTestData.SeedClientSessionCookie(host.Services, hellbourneCookie, hellbourneAccountName);

        using TcpClient legionClient = await host.ConnectAndAuthenticateClientAsync(legionAccountID, legionAccountName, legionCookie, RemoteIP);
        using TcpClient hellbourneClient = await host.ConnectAndAuthenticateClientAsync(hellbourneAccountID, hellbourneAccountName, hellbourneCookie, RemoteIP);

        (int hostAccountID, string hostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int serverID = Random.Shared.Next(1_000_000, int.MaxValue);
        string serverCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedMatchServer(host.Services, serverID, hostAccountID, hostAccountName, serverCookie, ServerPort, ServerAddress);

        using TcpClient matchServer = await host.ConnectAndAuthenticateMatchServerAsync(serverID, serverCookie);

        MatchmakingGroupInformation information = MatchmakingTestBuilder.Information(gameType: ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL);

        MatchmakingGroup legionGroup = MatchmakingTestBuilder.BuildGroup([Context.ClientChatSessions[legionAccountName]], information);
        MatchmakingGroup hellbourneGroup = MatchmakingTestBuilder.BuildGroup([Context.ClientChatSessions[hellbourneAccountName]], information);

        MatchmakingMatch match = MatchmakingMatch.FromTeams(MatchmakingTeam.FromGroups([legionGroup], teamSize: 1), MatchmakingTeam.FromGroups([hellbourneGroup], teamSize: 1));

        match.GameType = ChatProtocol.TMMGameType.TMM_GAME_TYPE_CAMPAIGN_NORMAL;
        match.AssignedServerID = serverID;
        match.ServerAddress = ServerAddress;
        match.ServerPort = ServerPort;

        match.CommitRoster();

        int[] userIDs = [await ChatTestData.GetUserID(host.Services, legionAccountID), await ChatTestData.GetUserID(host.Services, hellbourneAccountID)];

        int matchID = Random.Shared.Next(1_000_000, int.MaxValue);

        try
        {
            MatchmakingService.ActiveMatches.TryAdd(match.GUID, match);

            // The Legion Player Leaves After The Roster Was Committed To The Match Server, So That Only The Committed Roster Still Names Them
            legionGroup.Members.Clear();

            await ChatTestProtocol.WriteFrame(matchServer.GetStream(), ChatTestProtocol.BuildAnnounceMatch(match.CorrelationID, matchID));

            bool locked = await ChatTestProtocol.WaitUntil(async () =>
            {
                MatchLock?[] matchLocks = await Task.WhenAll(userIDs.Select(userID => distributedCacheStore.GetMatchLock(userID)));

                return matchLocks.All(matchLock => matchLock?.MatchID == matchID);
            }, TimeSpan.FromSeconds(10));

            await Assert.That(locked).IsTrue();

            foreach (int userID in userIDs)
            {
                MatchLock matchLock = (await distributedCacheStore.GetMatchLock(userID))!;

                using (Assert.Multiple())
                {
                    await Assert.That(matchLock.CorrelationID).IsEqualTo(match.CorrelationID);
                    await Assert.That(matchLock.ServerID).IsEqualTo(serverID);
                    await Assert.That(matchLock.ServerAddress).IsEqualTo(ServerAddress);
                    await Assert.That(matchLock.ServerPort).IsEqualTo(ServerPort);
                }
            }
        }

        finally
        {
            MatchmakingService.ActiveMatches.TryRemove(match.GUID, out _);

            await distributedCacheStore.RemoveMatchInformation(matchID);

            await RemoveLocks(distributedCacheStore, userIDs, matchID);
        }
    }

    /// <summary>
    ///     Builds a one-versus-one match of two solo players in the given game type, whose users have unique random IDs.
    /// </summary>
    private static MatchmakingMatch BuildCompetitiveMatch(ChatProtocol.TMMGameType gameType)
    {
        MatchmakingGroupInformation information = MatchmakingTestBuilder.Information(gameType: gameType);

        MatchmakingTeam legionTeam = MatchmakingTeam.FromGroups([MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, information: information)], teamSize: 1);
        MatchmakingTeam hellbourneTeam = MatchmakingTeam.FromGroups([MatchmakingTestBuilder.BuildSoloGroup(MatchmakingTestBuilder.BaselineTMR, information: information)], teamSize: 1);

        MatchmakingMatch match = MatchmakingMatch.FromTeams(legionTeam, hellbourneTeam);

        match.GameType = gameType;

        AssignUniqueUserIDs(match);

        return match;
    }

    /// <summary>
    ///     Gives each player's user a unique random ID, because the builder leaves every user at the same default ID, which would make the players share one lock.
    ///     The IDs are far above those of the users which a host seeds, so that a lock written here never binds a seeded user.
    /// </summary>
    private static void AssignUniqueUserIDs(MatchmakingMatch match)
    {
        int nextUserID = Random.Shared.Next(1_000_000, 2_000_000_000);

        foreach (MatchmakingGroupMember member in match.GetAllPlayers())
            member.Account.User.ID = nextUserID++;
    }

    private static async Task RemoveLocks(IDatabase distributedCacheStore, IEnumerable<int> userIDs, int matchID)
    {
        foreach (int userID in userIDs)
            await distributedCacheStore.RemoveMatchLock(userID, matchID);
    }
}
