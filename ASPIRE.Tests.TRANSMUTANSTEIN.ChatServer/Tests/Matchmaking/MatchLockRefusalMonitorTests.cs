namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for relaying the master server's refusals of a locked player's connections to other match servers: the chat server sends the player back through their chat session when they have one, and otherwise verifies their lock anyway, so that a player who was removed from their match while their chat is down is served when the match server asks again.
///     Every test logs in with a fresh session cookie and disposes its locked match, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock left behind would bind a later test.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchLockRefusalMonitorTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    private const uint SlotTimeRemaining = 120000;

    /// <summary>
    ///     The name of an account of the locked user other than the one which is refused, which holds the user's slot in the locked match.
    /// </summary>
    private const string SlotAccountName = "TEST:SlotAccount";

    /// <summary>
    ///     How long a frame which is expected to arrive is awaited.
    /// </summary>
    private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     How long a test listens to be sure that a frame which is not expected does not arrive.
    /// </summary>
    private static readonly TimeSpan QuietPeriod = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     How long a lock which is expected to lift is awaited.
    /// </summary>
    private static readonly TimeSpan LockLiftTimeout = TimeSpan.FromSeconds(10);

    [Test]
    public async Task A_Published_Refusal_Sends_The_Locked_Player_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
            await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
            await Assert.That(connect?.Address).IsEqualTo(lockedMatch.Lock.ServerAddress);
            await Assert.That((ushort?) connect?.Port).IsEqualTo(lockedMatch.Lock.ServerPort);
            await Assert.That(connect?.RandomInteger).IsNotEqualTo(-1);
        }
    }

    [Test]
    public async Task A_Published_Refusal_Sends_The_Player_Back_Even_After_The_Client_Reported_Joining_A_Game()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        // A Chat Reconnect While The Refused Join Is Still Connecting Reports Joining The Other Game's Server
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame("127.0.0.2:11236"));

        bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => Context.ClientChatSessions[accountName].ReportedJoiningGame, FrameTimeout);

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(reportedJoiningGame).IsTrue();
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
            await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
        }
    }

    [Test]
    [Arguments(SlotTimeRemaining)]
    [Arguments(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining)]
    public async Task A_Refusal_While_The_Player_Reports_The_Locked_Server_Is_Ignored(uint slotTimeRemaining)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(slotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

        bool reported = await ChatTestProtocol.WaitUntil(() => session.ReportedGameServerAddress is not null, FrameTimeout);

        // The Other Match Server's Last Retry Of An Abandoned Join Arrives After The Player Is Back In Their Own Match, Whose Connection Reads As Connected
        await PublishRefusal(host, userID, accountName);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

        using (Assert.Multiple())
        {
            await Assert.That(reported).IsTrue();
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(session.PendingMatchLockReturn).IsNull();
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
        }
    }

    [Test]
    public async Task Leaving_The_Match_After_An_Ignored_Refusal_Triggers_Nothing()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

        bool reported = await ChatTestProtocol.WaitUntil(() => session.ReportedGameServerAddress is not null, FrameTimeout);

        await PublishRefusal(host, userID, accountName);

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildLeftGame());

        bool left = await ChatTestProtocol.WaitUntil(() => session.ReportedGameServerAddress is null, FrameTimeout);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

        using (Assert.Multiple())
        {
            await Assert.That(reported).IsTrue();
            await Assert.That(left).IsTrue();
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(session.PendingMatchLockReturn).IsNull();
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
        }
    }

    [Test]
    public async Task A_Refusal_After_The_Player_Left_The_Locked_Server_Sends_Them_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

        bool reported = await ChatTestProtocol.WaitUntil(() => session.ReportedGameServerAddress is not null, FrameTimeout);

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildLeftGame());

        bool left = await ChatTestProtocol.WaitUntil(() => session.ReportedGameServerAddress is null, FrameTimeout);

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(reported).IsTrue();
            await Assert.That(left).IsTrue();
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
            await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
        }
    }

    [Test]
    public async Task A_Published_Refusal_While_Connected_Elsewhere_Tells_The_Player_To_Close_The_Other_Game()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.ConnectedElsewhereMessage);
            await Assert.That(connect).IsNull();
        }
    }

    [Test]
    public async Task A_Malformed_Refusal_Does_Not_Stop_The_Monitor()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await host.Services.GetRequiredService<IConnectionMultiplexer>().GetSubscriber().PublishAsync(RedisChannel.Literal(DistributedCacheExtensions.MatchLockRefusalChannel), "NOT-A-REFUSAL");

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
        }
    }

    [Test]
    public async Task A_Published_Refusal_For_Another_Account_Names_The_Slot_Account()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName));
            await Assert.That(connect).IsNull();
        }
    }

    [Test]
    public async Task A_Published_Refusal_Without_A_Chat_Session_Verifies_The_Lock()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        // The Locked Match Server Answers That The Player's Slot Is Gone While Its Match Is In Progress, As It Does Once It Has Removed The Player
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

        (int userID, int accountID, string accountName, _) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

        await PublishRefusal(host, userID, accountName);

        await Assert.That(await ChatTestData.WaitUntilLockIsGone(host, userID, LockLiftTimeout)).IsTrue();
    }

    [Test]
    public async Task A_Refusal_Which_Finds_The_Lock_Lifted_Tells_The_Player_To_Try_Again()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        // The Player Was Removed From The Match, Which Its Match Server Answers With No Time Remaining On The Slot
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await PublishRefusal(host, userID, accountName);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.RefusalLiftedMessage);
            await Assert.That(connect).IsNull();
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
        }
    }

    [Test]
    public async Task A_Removed_Player_Without_Chat_Is_Served_On_Their_Next_Attempt()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        // The Master Server Purges Every Session Cookie When It Starts, So It Starts Before The Player's Session Cookie Is Seeded
        await using MasterServerHost masterServer = MasterServerHost.Start(host);

        // The Locked Match Server Answers That The Player's Slot Is Gone While Its Match Is In Progress, As It Does Once It Has Removed The Player
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

        (int otherHostAccountID, string otherHostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int otherServerID = Random.Shared.Next(100_000, 999_999);
        string otherServerSession = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedMatchServer(host.Services, otherServerID, otherHostAccountID, otherHostAccountName, otherServerSession);

        try
        {
            HttpResponseMessage refused = await masterServer.ConnectClient(otherServerSession, cookie);
            bool lifted = await ChatTestData.WaitUntilLockIsGone(host, userID, LockLiftTimeout);
            HttpResponseMessage retried = await masterServer.ConnectClient(otherServerSession, cookie);

            using (Assert.Multiple())
            {
                await Assert.That(refused.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
                await Assert.That(await refused.Content.ReadAsStringAsync()).DoesNotContain("game_cookie");
                await Assert.That(lifted).IsTrue();
                await Assert.That(retried.StatusCode).IsEqualTo(HttpStatusCode.OK);
                await Assert.That(await retried.Content.ReadAsStringAsync()).Contains("account_id");
            }
        }

        finally
        {
            // The Other Match Server Has No Chat Session To Remove It, And Every Chat Server Host Shares The Unprefixed Distributed Cache
            await host.Services.GetRequiredService<IDatabase>().RemoveMatchServerByID(otherServerID);
        }
    }

    /// <summary>
    ///     Publishes a refusal of the account's connection, as the master server does when a match server other than the locked one asks to authenticate the account's client.
    /// </summary>
    private static async Task PublishRefusal(ChatServerHost host, int userID, string accountName)
        => await host.Services.GetRequiredService<IDatabase>().PublishMatchLockRefusal(new MatchLockRefusal { UserID = userID, AccountName = accountName });
}
