namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.ServerManagement;

/// <summary>
///     Tests for how match locks are stored in the distributed cache: one lock per user, expiring one lifetime after it began, with an index of the locks on each match server.
///     Also tests that removing a match server lifts the locks on it.
/// </summary>
public sealed class MatchLockStoreTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    private IDatabase DistributedCache => webApplicationFactory.Services.GetRequiredService<IDatabase>();

    [Before(Test)]
    public async Task InitialiseFactory()
    {
        await webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();
    }

    [Test]
    public async Task Removing_The_Lock_Of_The_Current_Match_Removes_It()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));

        bool removed = await DistributedCache.RemoveMatchLock(7, 100);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsTrue();
            await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
        }
    }

    [Test]
    public async Task Removing_The_Locks_Of_A_Server_Keeps_A_Lock_Moved_To_Another_Server()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 200, serverID: 2, timestampStarted: DateTimeOffset.UtcNow));

        int removed = await DistributedCache.RemoveMatchLocksForServer(1);

        bool stillIndexed = await DistributedCache.SetContainsAsync(DistributedCacheExtensions.ConstructMatchLockServerKey(1), 7);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsEqualTo(0);
            await Assert.That((await DistributedCache.GetMatchLock(7))?.ServerID).IsEqualTo(2);
            await Assert.That(stillIndexed).IsFalse();
        }
    }

    [Test]
    public async Task An_Unreadable_Lock_Reads_As_Absent_And_Is_Dropped_With_The_Locks_Of_Its_Server()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));

        // The Stored Lock No Longer Deserialises, As It Would After A Change To The Shape Of The Lock
        await DistributedCache.StringSetAsync(DistributedCacheExtensions.ConstructMatchLockKey(7), "NOT-A-MATCH-LOCK", MatchLock.Lifetime);

        MatchLock? stored = await DistributedCache.GetMatchLock(7);

        bool removedForMatch = await DistributedCache.RemoveMatchLock(7, 100);

        int removedForServer = await DistributedCache.RemoveMatchLocksForServer(1);

        bool stillIndexed = await DistributedCache.SetContainsAsync(DistributedCacheExtensions.ConstructMatchLockServerKey(1), 7);

        using (Assert.Multiple())
        {
            await Assert.That(stored).IsNull();
            await Assert.That(removedForMatch).IsFalse();
            await Assert.That(removedForServer).IsEqualTo(0);
            await Assert.That(stillIndexed).IsFalse();
        }
    }

    [Test]
    public async Task A_Match_Lock_Is_Stored_Per_User_Until_One_Lifetime_After_It_Began()
    {
        MatchLock matchLock = BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow);
        await DistributedCache.SetMatchLock(matchLock);

        MatchLock? stored = await DistributedCache.GetMatchLock(7);
        TimeSpan? timeToLive = await DistributedCache.KeyTimeToLiveAsync(DistributedCacheExtensions.ConstructMatchLockKey(7));

        await Assert.That(stored?.MatchID).IsEqualTo(100);
        await Assert.That(stored?.CorrelationID).IsEqualTo(matchLock.CorrelationID);
        await Assert.That(timeToLive > MatchLock.Lifetime - TimeSpan.FromMinutes(1) && timeToLive <= MatchLock.Lifetime).IsTrue();
    }

    [Test]
    public async Task A_Lock_Stored_Without_A_Clan_Tagged_Name_Names_The_Bare_Account()
    {
        // A Lock Written Before The Clan-Tagged Name Was Recorded Has No Such Property, And Reconnecting To The Match Depends On It Still Reading
        JsonObject storedLock = JsonSerializer.SerializeToNode(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow))!.AsObject();

        storedLock.Remove(nameof(MatchLock.AccountNameWithClanTag));

        await DistributedCache.StringSetAsync(DistributedCacheExtensions.ConstructMatchLockKey(7), storedLock.ToJsonString(), MatchLock.Lifetime);

        MatchLock? stored = await DistributedCache.GetMatchLock(7);

        using (Assert.Multiple())
        {
            await Assert.That(stored).IsNotNull();
            await Assert.That(stored!.AccountNameWithClanTag).IsNull();
            await Assert.That(stored.TaggedAccountName).IsEqualTo(stored.AccountName);
        }
    }

    [Test]
    public async Task A_Match_Lock_Lifts_At_The_Safety_Limit()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow - MatchLock.Lifetime + TimeSpan.FromSeconds(2)));

        await Assert.That(await DistributedCache.GetMatchLock(7)).IsNotNull();

        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(5);

        while (DateTimeOffset.UtcNow < deadline && await DistributedCache.GetMatchLock(7) is not null)
            await Task.Delay(100);

        await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
    }

    [Test]
    public async Task A_Lock_Already_Past_The_Safety_Limit_Is_Not_Stored()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow - MatchLock.Lifetime - TimeSpan.FromMinutes(1)));

        await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
    }

    [Test]
    public async Task A_Second_Lock_For_The_Same_User_Replaces_The_First()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 101, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));

        MatchLock? stored = await DistributedCache.GetMatchLock(7);

        await Assert.That(stored?.MatchID).IsEqualTo(101);
    }

    [Test]
    public async Task Removing_A_Lock_For_Another_Match_Keeps_The_Current_Lock()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 101, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));

        bool removed = await DistributedCache.RemoveMatchLock(7, 100);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsFalse();
            await Assert.That(await DistributedCache.GetMatchLock(7)).IsNotNull();
        }
    }

    [Test]
    public async Task Removing_The_Locks_Of_A_Server_Removes_Only_The_Locks_On_That_Server()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));
        await DistributedCache.SetMatchLock(BuildLock(userID: 8, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));
        await DistributedCache.SetMatchLock(BuildLock(userID: 9, matchID: 200, serverID: 2, timestampStarted: DateTimeOffset.UtcNow));

        int removed = await DistributedCache.RemoveMatchLocksForServer(1);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsEqualTo(2);
            await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
            await Assert.That(await DistributedCache.GetMatchLock(8)).IsNull();
            await Assert.That(await DistributedCache.GetMatchLock(9)).IsNotNull();
        }
    }

    [Test]
    public async Task Removing_The_Locks_Of_A_Match_Keeps_A_Newer_Match_On_The_Same_Server()
    {
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));
        await DistributedCache.SetMatchLock(BuildLock(userID: 8, matchID: 101, serverID: 1, timestampStarted: DateTimeOffset.UtcNow));

        int removed = await DistributedCache.RemoveMatchLocksForServer(1, matchLock => matchLock.MatchID == 100);

        using (Assert.Multiple())
        {
            await Assert.That(removed).IsEqualTo(1);
            await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
            await Assert.That((await DistributedCache.GetMatchLock(8))?.MatchID).IsEqualTo(101);
        }
    }

    [Test]
    public async Task Removing_A_Match_Server_Lifts_The_Locks_On_It()
    {
        MatchServer matchServer = new ()
        {
            HostAccountID = 1,
            HostAccountName = "LOCKED-HOST",
            ID = 1,
            Name = "LOCKED-HOST:1",
            MatchServerManagerID = null,
            Instance = 1,
            IPAddress = "127.0.0.1",
            Port = 11235,
            Location = "USE",
            Description = "Match Lock Test Server"
        };

        await DistributedCache.SetMatchServer(matchServer.HostAccountName, matchServer);
        await DistributedCache.SetMatchLock(BuildLock(userID: 7, matchID: 100, serverID: matchServer.ID, timestampStarted: DateTimeOffset.UtcNow));

        await DistributedCache.RemoveMatchServerByID(matchServer.ID);

        await Assert.That(await DistributedCache.GetMatchLock(7)).IsNull();
    }

    private static MatchLock BuildLock(int userID, int matchID, int serverID, DateTimeOffset timestampStarted) => new()
    {
        UserID = userID,
        AccountID = userID,
        AccountName = $"LOCKED-{userID}",
        MatchID = matchID,
        CorrelationID = matchID * 1000 + userID,
        ArrangedMatchType = MatchType.AM_MATCHMAKING,
        ServerID = serverID,
        ServerAddress = "127.0.0.1",
        ServerPort = 11235,
        TimestampStarted = timestampStarted
    };
}
