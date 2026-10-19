namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Services;

/// <summary>
///     Tests for the verification which decides whether a match lock still holds before a player is sent back to the locked match, so that nobody is sent to a dead server, a finished match, or an unreclaimable slot.
///     The rules are tested directly, and the verification end to end against a real chat server host and a fake match server game socket.
/// </summary>
public sealed class MatchLockVerificationTests(ServiceContainerContext containerContext)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UnixEpoch.AddDays(1);

    [Test]
    [Arguments(GamePhase.GAME_PHASE_HERO_SELECT, 1u)]
    [Arguments(GamePhase.GAME_PHASE_ACTIVE, 240000u)]
    [Arguments(GamePhase.GAME_PHASE_ACTIVE, uint.MaxValue - 1u)]
    public async Task A_Slot_With_Time_Remaining_Is_Reclaimable(GamePhase gamePhase, uint slotTimeRemaining)
        => await Assert.That(MatchLockService.EvaluateSlot(gamePhase, slotTimeRemaining)).IsEqualTo(MatchLockVerdict.SendBack);

    [Test]
    [Arguments(GamePhase.GAME_PHASE_HERO_LOADING)]
    [Arguments(GamePhase.GAME_PHASE_PRE_MATCH)]
    [Arguments(GamePhase.GAME_PHASE_ACTIVE)]
    [Arguments(GamePhase.GAME_PHASE_POST_MATCH)]
    [Arguments(GamePhase.GAME_PHASE_ENDED)]
    public async Task No_Time_Remaining_Lifts_The_Lock_Once_No_Pick_Phase_Can_Follow(GamePhase gamePhase)
        => await Assert.That(MatchLockService.EvaluateSlot(gamePhase, 0u)).IsEqualTo(MatchLockVerdict.LiftSlotGone);

    [Test]
    [Arguments(GamePhase.GAME_PHASE_IDLE)]
    [Arguments(GamePhase.GAME_PHASE_WAITING_FOR_PLAYERS)]
    [Arguments(GamePhase.GAME_PHASE_HERO_BAN)]
    [Arguments(GamePhase.GAME_PHASE_HERO_SELECT)]
    [Arguments(GamePhase.GAME_PHASE_HERO_BLIND_BAN)]
    [Arguments(GamePhase.GAME_PHASE_HERO_LOCK)]
    [Arguments(GamePhase.GAME_PHASE_HERO_LOCK_PICK)]
    [Arguments(GamePhase.GAME_PHASE_SHUFFLE_PICK)]
    [Arguments(GamePhase.GAME_PHASE_HERO_VOTE_BLIND_BAN)]
    [Arguments(GamePhase.GAME_PHASE_CUSTOM)]
    [Arguments(GamePhase.GAME_PHASE_WAIT)]
    [Arguments(null)]
    public async Task No_Time_Remaining_Never_Lifts_The_Lock_Before_Or_During_A_Pick_Phase(GamePhase? gamePhase)
        => await Assert.That(MatchLockService.EvaluateSlot(gamePhase, 0u)).IsEqualTo(MatchLockVerdict.SendBack);

    [Test]
    public async Task An_Unanswered_Query_Lifts_The_Lock_Without_Confirmation()
        => await Assert.That(MatchLockService.EvaluateSlot(GamePhase.GAME_PHASE_ACTIVE, null)).IsEqualTo(MatchLockVerdict.LiftSlotUnconfirmed);

    [Test]
    public async Task A_Server_Which_Is_Not_Registered_Is_Gone()
    {
        MatchLock matchLock = BuildLock();

        MatchServerChatSessionMetadata metadata = BuildMetadata(matchLock, ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE, matchLock.MatchID);

        await Assert.That(MatchLockService.EvaluateServer(matchLock, null, metadata, Now)).IsEqualTo(MatchLockVerdict.LiftServerGone);
    }

    [Test]
    public async Task A_Registered_Server_Without_A_Chat_Session_Does_Not_Rule_The_Lock_Out()
    {
        MatchLock matchLock = BuildLock();

        await Assert.That(MatchLockService.EvaluateServer(matchLock, BuildServer(matchLock), null, Now)).IsNull();
    }

    [Test]
    public async Task A_Server_Whose_Status_Is_Stale_Is_Gone()
    {
        MatchLock matchLock = BuildLock();

        MatchServerChatSessionMetadata metadata = BuildMetadata(matchLock, ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE, matchLock.MatchID);

        metadata.LastStatusUpdate = Now - StaleHostReaper.MatchServerSessionStaleStateThreshold - TimeSpan.FromSeconds(1);

        await Assert.That(MatchLockService.EvaluateServer(matchLock, BuildServer(matchLock), metadata, Now)).IsEqualTo(MatchLockVerdict.LiftServerGone);
    }

    [Test]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_IDLE)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_SLEEPING)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_CRASHED)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_KILLED)]
    public async Task A_Server_Without_A_Match_Has_Ended_The_Match(ChatProtocol.ServerStatus status)
    {
        MatchLock matchLock = BuildLock();

        MatchServerChatSessionMetadata metadata = BuildMetadata(matchLock, status, matchLock.MatchID);

        await Assert.That(MatchLockService.EvaluateServer(matchLock, BuildServer(matchLock), metadata, Now)).IsEqualTo(MatchLockVerdict.LiftMatchOver);
    }

    [Test]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_LOADING)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_UNKNOWN)]
    public async Task A_Server_Reporting_Another_Match_Has_Ended_The_Match(ChatProtocol.ServerStatus status)
    {
        MatchLock matchLock = BuildLock();

        MatchServerChatSessionMetadata metadata = BuildMetadata(matchLock, status, matchLock.MatchID + 1);

        await Assert.That(MatchLockService.EvaluateServer(matchLock, BuildServer(matchLock), metadata, Now)).IsEqualTo(MatchLockVerdict.LiftMatchOver);
    }

    [Test]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_ACTIVE, true)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_LOADING, false)]
    [Arguments(ChatProtocol.ServerStatus.SERVER_STATUS_UNKNOWN, false)]
    public async Task A_Live_Server_Hosting_The_Match_Does_Not_Rule_The_Lock_Out(ChatProtocol.ServerStatus status, bool reportsTheMatch)
    {
        MatchLock matchLock = BuildLock();

        MatchServerChatSessionMetadata metadata = BuildMetadata(matchLock, status, reportsTheMatch ? matchLock.MatchID : -1);

        await Assert.That(MatchLockService.EvaluateServer(matchLock, BuildServer(matchLock), metadata, Now)).IsNull();
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Verifying_A_Held_Slot_Queries_The_Chat_Connection_Address_Rather_Than_The_Registered_One()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        // The Registered Address Is Reserved For Documentation, So A Query Sent There Would Never Be Answered
        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, registeredAddress: "192.0.2.1");

        MatchLockVerdict verdict = await host.Services.GetRequiredService<MatchLockService>().Verify(lockedMatch.Lock);

        using (Assert.Multiple())
        {
            await Assert.That(verdict).IsEqualTo(MatchLockVerdict.SendBack);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
            await Assert.That(gameSocket.Requests.Contains((lockedMatch.Lock.MatchID, lockedMatch.Lock.AccountID))).IsTrue();
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Verifying_A_Lost_Slot_Lifts_The_Lock()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

        MatchLockVerdict verdict = await host.Services.GetRequiredService<MatchLockService>().Verify(lockedMatch.Lock);

        using (Assert.Multiple())
        {
            await Assert.That(verdict).IsEqualTo(MatchLockVerdict.LiftSlotGone);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Verifying_Against_An_Unanswering_Server_Lifts_The_Lock()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        gameSocket.Answers = false;

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        MatchLockVerdict verdict = await host.Services.GetRequiredService<MatchLockService>().Verify(lockedMatch.Lock);

        using (Assert.Multiple())
        {
            await Assert.That(verdict).IsEqualTo(MatchLockVerdict.LiftSlotUnconfirmed);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
        }
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task An_Unreachable_Server_Is_Logged_As_An_Error_Once()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        gameSocket.Answers = false;

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        RecordingLogger<MatchLockService> logger = new ();

        MatchLockService matchLockService = new (distributedCacheStore, new MatchServerReconnectQuery(), logger);

        await matchLockService.Verify(lockedMatch.Lock);

        await distributedCacheStore.SetMatchLock(lockedMatch.Lock);

        await matchLockService.Verify(lockedMatch.Lock);

        List<string> errors = [.. logger.Entries.Where(entry => entry.Level is LogLevel.Error).Select(entry => entry.Message)];

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors[0]).Contains($"127.0.0.1:{gameSocket.Port}");
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task An_Unreachable_Server_Is_Logged_Again_After_It_Answers_In_Between()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(120000);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        IDatabase distributedCacheStore = host.Services.GetRequiredService<IDatabase>();

        RecordingLogger<MatchLockService> logger = new ();

        MatchLockService matchLockService = new (distributedCacheStore, new MatchServerReconnectQuery(), logger);

        gameSocket.Answers = false;

        await matchLockService.Verify(lockedMatch.Lock);

        await distributedCacheStore.SetMatchLock(lockedMatch.Lock);

        gameSocket.Answers = true;

        MatchLockVerdict answeredVerdict = await matchLockService.Verify(lockedMatch.Lock);

        gameSocket.Answers = false;

        await matchLockService.Verify(lockedMatch.Lock);

        await Assert.That(answeredVerdict).IsEqualTo(MatchLockVerdict.SendBack);
        await Assert.That(logger.Entries.Count(entry => entry.Level is LogLevel.Error)).IsEqualTo(2);
    }

    [Test]
    [NotInParallel(nameof(ChatServerHost))]
    public async Task Verifying_A_Registered_Server_Without_A_Chat_Session_Queries_Its_Registered_Address_And_Keeps_The_Lock()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

        (int userID, int accountID, string accountName) = await SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, registeredAddress: "127.0.0.1", withChatSession: false);

        MatchLockVerdict verdict = await host.Services.GetRequiredService<MatchLockService>().Verify(lockedMatch.Lock);

        using (Assert.Multiple())
        {
            await Assert.That(verdict).IsEqualTo(MatchLockVerdict.SendBack);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
            await Assert.That(gameSocket.Requests.Contains((lockedMatch.Lock.MatchID, lockedMatch.Lock.AccountID))).IsTrue();
        }
    }

    private static async Task<(int UserID, int AccountID, string AccountName)> SeedPlayer(ChatServerHost host)
    {
        (int accountID, string accountName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        return (await ChatTestData.GetUserID(host.Services, accountID), accountID, accountName);
    }

    private static MatchLock BuildLock() => new ()
    {
        UserID = 7,
        AccountID = 17,
        AccountName = "TEST:Player",
        MatchID = 4242,
        CorrelationID = 99,
        ArrangedMatchType = MatchType.AM_MATCHMAKING,
        ServerID = 1,
        ServerAddress = "127.0.0.1",
        ServerPort = 11235,
        TimestampStarted = Now
    };

    private static MatchServer BuildServer(MatchLock matchLock) => new ()
    {
        ID = matchLock.ServerID,
        HostAccountID = 1,
        HostAccountName = "TEST:Host",
        Name = $"server-{matchLock.ServerID}",
        MatchServerManagerID = null,
        Instance = 1,
        IPAddress = matchLock.ServerAddress,
        Port = matchLock.ServerPort,
        Location = "USE",
        Description = "Match Lock Verification Test Server"
    };

    private static MatchServerChatSessionMetadata BuildMetadata(MatchLock matchLock, ChatProtocol.ServerStatus status, int reportedMatchID) => new ()
    {
        ServerID = matchLock.ServerID,
        SessionCookie = Guid.CreateVersion7().ToString(),
        ChatProtocolVersion = (int) ChatProtocol.CHAT_PROTOCOL_EXTERNAL_VERSION,
        Status = status,
        MatchID = reportedMatchID,
        GamePhase = (int) GamePhase.GAME_PHASE_ACTIVE,
        LastStatusUpdate = Now
    };
}
