namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for lifting match locks as soon as their match or its match server ends: when the match server reports that the match ended, was abandoned, or was aborted, and when the match server's session is terminated.
///     Every test disposes its locked match, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock left behind would bind a user of a later test.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchLockReleaseTests(ServiceContainerContext containerContext)
{
    [Test]
    public async Task Match_Ended_Lifts_The_Locks_Of_That_Matchup()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        await Assert.That(lockedMatch.Lock.CorrelationID).IsNotEqualTo(lockedMatch.Lock.MatchID);

        await ChatTestProtocol.WriteFrame(lockedMatch.MatchServerConnection!.GetStream(), ChatTestProtocol.BuildMatchEnded(matchupID: lockedMatch.Lock.CorrelationID));

        bool lifted = await ChatTestData.WaitUntilLockIsGone(host, lockedMatch.Lock.UserID, TimeSpan.FromSeconds(10));

        await Assert.That(lifted).IsTrue();
    }

    [Test]
    public async Task Match_Ended_For_The_Match_ID_Instead_Of_The_Matchup_Keeps_The_Locks()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        await Assert.That(lockedMatch.Lock.CorrelationID).IsNotEqualTo(lockedMatch.Lock.MatchID);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        MatchLock sentinelLock = await SeedSentinelLock(distributedCacheStore, lockedMatch.Lock, sentMatchupID: lockedMatch.Lock.MatchID);

        try
        {
            NetworkStream stream = lockedMatch.MatchServerConnection!.GetStream();

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildMatchEnded(matchupID: lockedMatch.Lock.MatchID));
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildMatchEnded(matchupID: sentinelLock.CorrelationID));

            bool sentinelLifted = await ChatTestData.WaitUntilLockIsGone(host, sentinelLock.UserID, TimeSpan.FromSeconds(10));

            await Assert.That(sentinelLifted).IsTrue();
            await Assert.That(await distributedCacheStore.GetMatchLock(lockedMatch.Lock.UserID)).IsNotNull();
        }

        finally
        {
            await distributedCacheStore.RemoveMatchLock(sentinelLock.UserID, sentinelLock.MatchID);
        }
    }

    [Test]
    public async Task Match_Ended_For_Another_Matchup_Keeps_The_Locks()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        int otherMatchupID = lockedMatch.Lock.CorrelationID + 1;

        MatchLock sentinelLock = await SeedSentinelLock(distributedCacheStore, lockedMatch.Lock, sentMatchupID: otherMatchupID);

        try
        {
            NetworkStream stream = lockedMatch.MatchServerConnection!.GetStream();

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildMatchEnded(matchupID: otherMatchupID));
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildMatchEnded(matchupID: sentinelLock.CorrelationID));

            bool sentinelLifted = await ChatTestData.WaitUntilLockIsGone(host, sentinelLock.UserID, TimeSpan.FromSeconds(10));

            await Assert.That(sentinelLifted).IsTrue();
            await Assert.That(await distributedCacheStore.GetMatchLock(lockedMatch.Lock.UserID)).IsNotNull();
        }

        finally
        {
            await distributedCacheStore.RemoveMatchLock(sentinelLock.UserID, sentinelLock.MatchID);
        }
    }

    [Test]
    public async Task Match_Abandoned_Lifts_Every_Lock_On_The_Server()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        await ChatTestProtocol.WriteFrame(lockedMatch.MatchServerConnection!.GetStream(), ChatTestProtocol.BuildAbandonMatch(failed: false));

        bool lifted = await ChatTestData.WaitUntilLockIsGone(host, lockedMatch.Lock.UserID, TimeSpan.FromSeconds(10));

        await Assert.That(lifted).IsTrue();
    }

    [Test]
    public async Task Match_Aborted_Lifts_Every_Lock_On_The_Server()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        await ChatTestProtocol.WriteFrame(lockedMatch.MatchServerConnection!.GetStream(), ChatTestProtocol.BuildMatchAborted(matchupID: lockedMatch.Lock.CorrelationID));

        bool lifted = await ChatTestData.WaitUntilLockIsGone(host, lockedMatch.Lock.UserID, TimeSpan.FromSeconds(10));

        await Assert.That(lifted).IsTrue();
    }

    [Test]
    public async Task Terminating_A_Match_Server_Lifts_The_Locks_On_It()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        // The Stale Host Reaper Terminates The Session Of A Hung Match Server In The Same Way
        await Context.MatchServerChatSessions[lockedMatch.Lock.ServerID].Terminate(distributedCacheStore);

        await Assert.That(await distributedCacheStore.GetMatchLock(lockedMatch.Lock.UserID)).IsNull();
    }

    private static async Task<(int UserID, int AccountID, string AccountName)> SeedPlayer(ChatServerHost host)
    {
        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        return (await ChatTestData.GetUserID(host.Services, accountID), accountID, accountName);
    }

    /// <summary>
    ///     Locks a further user to another matchup on the match server of the given lock, with a correlation ID which differs from the given lock's and from the matchup ID of the frame under test.
    ///     The chat server processes the frames of a match server in order, so the sentinel lock being lifted by a frame sent after the frame under test proves that the frame under test was processed.
    ///     The user ID is far above those of the users which a host seeds, so that the sentinel lock never binds a seeded user.
    /// </summary>
    private static async Task<MatchLock> SeedSentinelLock(IDatabase distributedCacheStore, MatchLock lockedMatchLock, int sentMatchupID)
    {
        // Both Candidates Differ From The Locked Match's Correlation ID, And At Most One Of Them Equals The Matchup ID Of The Frame Under Test
        int[] candidateCorrelationIDs = [lockedMatchLock.CorrelationID + 1, lockedMatchLock.CorrelationID + 2];

        int userID = Random.Shared.Next(1_000_000, int.MaxValue);

        MatchLock sentinelLock = new ()
        {
            UserID = userID,
            AccountID = userID,
            AccountName = $"SENTINEL-{userID}",
            MatchID = lockedMatchLock.MatchID + 1,
            CorrelationID = candidateCorrelationIDs.First(candidate => candidate != sentMatchupID),
            ArrangedMatchType = lockedMatchLock.ArrangedMatchType,
            ServerID = lockedMatchLock.ServerID,
            ServerAddress = lockedMatchLock.ServerAddress,
            ServerPort = lockedMatchLock.ServerPort,
            TimestampStarted = DateTimeOffset.UtcNow
        };

        await distributedCacheStore.SetMatchLock(sentinelLock);

        return sentinelLock;
    }
}
