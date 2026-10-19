namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for lifting match locks as soon as their match or its match server ends: when the match server reports that the match ended, was abandoned, or was aborted, and when the match server's session is terminated.
///     Every test disposes its locked match, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock left behind would bind a user of a later test.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchLockReleaseTests(ServiceContainerContext containerContext)
{
    /// <summary>
    ///     How long a test watches a lock which must not be lifted, which is ample for the chat server to process the frame which the test has just sent.
    /// </summary>
    private static readonly TimeSpan KeptLockObservationPeriod = TimeSpan.FromSeconds(2);

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

        await ChatTestProtocol.WriteFrame(lockedMatch.MatchServerConnection!.GetStream(), ChatTestProtocol.BuildMatchEnded(matchupID: lockedMatch.Lock.MatchID));

        bool lifted = await ChatTestData.WaitUntilLockIsGone(host, lockedMatch.Lock.UserID, KeptLockObservationPeriod);

        await Assert.That(lifted).IsFalse();
    }

    [Test]
    public async Task Match_Ended_For_Another_Matchup_Keeps_The_Locks()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        await ChatTestProtocol.WriteFrame(lockedMatch.MatchServerConnection!.GetStream(), ChatTestProtocol.BuildMatchEnded(matchupID: lockedMatch.Lock.CorrelationID + 1));

        bool lifted = await ChatTestData.WaitUntilLockIsGone(host, lockedMatch.Lock.UserID, KeptLockObservationPeriod);

        await Assert.That(lifted).IsFalse();
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
}
