namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for sending a locked player back to their match in progress: the notice which explains why another game is refused, the "AUTO_MATCH_CONNECT" which is sent at once, or as soon as another machine which is still connected to the match has left it, and the reminders which repeat it until the player reaches the locked match server, the lock lifts, the session ends, or the return window expires.
///     Every test logs in with a fresh session cookie and disposes its locked match, because every chat server host shares the unprefixed distributed cache while each host's database restarts its identifiers, so a lock or a login marker left behind would bind a later test.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchLockSendBackTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    private const uint SlotTimeRemaining = 120000;

    /// <summary>
    ///     The name of an account of the locked user other than the one which logs in, which holds the user's slot in the locked match.
    /// </summary>
    private const string SlotAccountName = "TEST:SlotAccount";

    /// <summary>
    ///     The IP address of a match server other than the locked one, which no other test registers, so that its address resolves to it alone.
    /// </summary>
    private const string OtherServerIPAddress = "127.0.0.2";

    /// <summary>
    ///     How long a frame which is expected to arrive is awaited.
    /// </summary>
    private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     How long a test listens to be sure that a frame which is not expected does not arrive.
    /// </summary>
    private static readonly TimeSpan QuietPeriod = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     How long a reminder is awaited after the frame which precedes it.
    /// </summary>
    private static readonly TimeSpan ReminderTimeout = MatchLockService.ReturnResendInterval + QuietPeriod;

    /// <summary>
    ///     How far an expiry which the chat server records may lie from the one which a test computes around it, which allows for a slow test host while staying far below the difference between a wait and a send-back.
    /// </summary>
    private static readonly TimeSpan ExpiryMargin = MatchLockService.ReturnResendInterval;

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Another_Account_Is_Named_With_The_Slot_Account_Clan_Tag(bool atLogin)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        // The Slot Is Held By Another Account Of The Same User, Which Was In A Clan When The Match Was Announced
        string slotAccountNameWithClanTag = $"[SLOT]{SlotAccountName}";

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket, accountNameWithClanTag: slotAccountNameWithClanTag);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        if (atLogin)
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        else
            await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.OtherAccountMessage(slotAccountNameWithClanTag));
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        }
    }

    [Test]
    [Arguments(SlotTimeRemaining)]
    [Arguments(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining)]
    public async Task Logging_In_While_Locked_Tells_The_Player_Without_Sending_Them_Back(uint slotTimeRemaining)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(slotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        // The Game Client's Own Reconnect Prompt Handles A Return At Login, So Neither A Free Nor A Still-Connected Slot Starts A Return
        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2 + QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.SlotAccountLoginMessage]);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn).IsNull();
            await Assert.That(gameSocket.Requests.Count).IsEqualTo(1);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
        }
    }

    [Test]
    public async Task Logging_In_On_Another_Account_Of_A_Locked_User_Names_The_Slot_Account_And_Sends_Nothing_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(1);
            await Assert.That(noticeMessages.SingleOrDefault()).IsEqualTo(MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName));
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn).IsNull();
        }
    }

    [Test]
    public async Task Logging_In_Again_With_The_Same_Session_Cookie_Sends_Nothing()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient firstClient = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        await LogInAndReadNotice(firstClient.GetStream());

        // The Game Client Reconnects To The Chat Server With The Session Cookie Of The Same Login, Which Replaces The First Chat Session
        using TcpClient secondClient = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = secondClient.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Logging_In_While_Reporting_A_Game_Sends_Nothing()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        // A Game Client Which Is Already Connected To A Match Server Reports It Before The Login Action, As It Does When Its Chat Connection Is Restored During The Locked Match
        // A Fresh Session Has No Pending Return, So Only The Reported Game Keeps The Login Trigger From Acting
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task An_Unanswered_Return_Is_Repeated_As_A_Reminder()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await RefuseAndReadSendBack(host, accountName, stream);

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);
            await Assert.That(reminder?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
            await Assert.That(reminder?.Address).IsEqualTo(lockedMatch.Lock.ServerAddress);
            await Assert.That((ushort?) reminder?.Port).IsEqualTo(lockedMatch.Lock.ServerPort);
        }
    }

    [Test]
    public async Task An_Acknowledgement_Alone_Keeps_The_Reminders()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await RefuseAndReadSendBack(host, accountName, stream);

        // The Game Client Acknowledges An "AUTO_MATCH_CONNECT" As It Acts On It, While Its Connection To The Match Server Can Still Be Refused
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY));

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);
    }

    [Test]
    public async Task Joining_The_Locked_Server_Stops_The_Reminders()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await RefuseAndReadSendBack(host, accountName, stream);

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, MatchLockService.ReturnResendInterval * 2);

        await Assert.That(reminder).IsNull();
    }

    [Test]
    public async Task Joining_Another_Server_Keeps_The_Reminders()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        (int otherHostAccountID, string otherHostAccountName) = await ChatTestData.SeedAccount(host.Services, AccountType.ServerHost);

        int otherServerID = ChatTestData.NextSeededServerID();
        int otherServerPort = Random.Shared.Next(20_000, 30_000);

        await ChatTestData.SeedMatchServer(host.Services, otherServerID, otherHostAccountID, otherHostAccountName, Guid.CreateVersion7().ToString(), otherServerPort, OtherServerIPAddress);

        try
        {
            using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

            NetworkStream stream = client.GetStream();

            await RefuseAndReadSendBack(host, accountName, stream);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{OtherServerIPAddress}:{otherServerPort}"));

            (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

            await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);
        }

        finally
        {
            // The Other Match Server Has No Chat Session To Remove It, And Every Chat Server Host Shares The Unprefixed Distributed Cache
            await host.Services.GetRequiredService<IDatabase>().RemoveMatchServerByID(otherServerID);
        }
    }

    [Test]
    public async Task Disconnecting_Stops_The_Reminders()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        ClientChatSession session = Context.ClientChatSessions[accountName];

        await RefuseAndReadSendBack(host, accountName, client.GetStream());

        MatchLockReturn? pendingReturn = session.PendingMatchLockReturn;

        client.Close();

        // The Session's Cleanup Ends The Return At Once, Well Before The Reminder Loop Would Notice The Disconnection At Its Next Reminder
        bool cancelled = await ChatTestProtocol.WaitUntil(() => pendingReturn?.Cancellation.IsCancellationRequested is true, MatchLockService.ReturnResendInterval / 2);

        // The Reminder Loop Verifies The Lock Before Every Reminder, So A Loop Which Carried On Would Query The Match Server Again
        int requestCount = gameSocket.Requests.Count;

        await Task.Delay(MatchLockService.ReturnResendInterval * 2);

        using (Assert.Multiple())
        {
            await Assert.That(pendingReturn).IsNotNull();
            await Assert.That(cancelled).IsTrue();
            await Assert.That(session.PendingMatchLockReturn).IsNull();
            await Assert.That(gameSocket.Requests.Count).IsEqualTo(requestCount);
        }
    }

    [Test]
    public async Task A_Lifted_Lock_Stops_The_Reminders()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await RefuseAndReadSendBack(host, accountName, stream);

        // The Player's Slot Is Gone Before The Reminder Loop Verifies The Lock Again
        gameSocket.SlotTimeRemaining = 0;

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(reminder).IsNull();
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
        }
    }

    [Test]
    public async Task Repeated_Enforcements_Within_The_Return_Window_Message_Once()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        MatchLockService matchLockService = host.Services.GetRequiredService<MatchLockService>();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        bool firstRefused = await matchLockService.Enforce(session);
        bool secondRefused = await matchLockService.Enforce(session);

        // Counted Before The Reminder Loop First Verifies The Lock Again, One Resend Interval After The First Send-Back
        int requestCount = gameSocket.Requests.Count;

        (string SenderName, string Message)? firstNotice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (string SenderName, string Message)? secondNotice = await ChatTestProtocol.ReadNotice(stream, MatchLockService.ReturnResendInterval);

        using (Assert.Multiple())
        {
            await Assert.That(firstRefused).IsTrue();
            await Assert.That(secondRefused).IsTrue();
            await Assert.That(requestCount).IsEqualTo(1);
            await Assert.That(firstNotice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
            await Assert.That(secondNotice).IsNull();
        }
    }

    [Test]
    public async Task A_Match_Server_Retry_Of_A_Refused_Attempt_Does_Not_Repeat_The_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        MatchLockService matchLockService = host.Services.GetRequiredService<MatchLockService>();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        bool firstRefused = await matchLockService.Enforce(session);

        // Another Refusal Of The Same Player Soon After Belongs To The Same Attempt To Join Another Game
        await Task.Delay(TimeSpan.FromMilliseconds(MatchServerChatSession.AuthenticationRequestIntervalMilliseconds));

        bool retryRefused = await matchLockService.Enforce(session);

        (List<string> noticeMessages, _) = await ChatTestProtocol.ReadSendBackFrames(client.GetStream(), QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(firstRefused).IsTrue();
            await Assert.That(retryRefused).IsTrue();
            await Assert.That(noticeMessages.Count).IsEqualTo(1);
        }
    }

    [Test]
    [Arguments(SlotTimeRemaining)]
    [Arguments(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining)]
    public async Task A_Player_Still_In_The_Match_Is_Refused_Without_A_Notice_Or_A_Return(uint slotTimeRemaining)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(slotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

        bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => session.ReportedJoiningGame, FrameTimeout);

        MatchLockService matchLockService = host.Services.GetRequiredService<MatchLockService>();

        bool firstRefused = await matchLockService.Enforce(session);
        bool secondRefused = await matchLockService.Enforce(session);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

        using (Assert.Multiple())
        {
            await Assert.That(reportedJoiningGame).IsTrue();
            await Assert.That(firstRefused).IsTrue();
            await Assert.That(secondRefused).IsTrue();
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(session.PendingMatchLockReturn).IsNull();
        }
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task An_Attempt_After_The_Notice_Repeat_Interval_Gets_The_Notice_Again(bool slotAccount)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        int slotAccountID = slotAccount ? accountID : Random.Shared.Next(1_000_000, int.MaxValue);
        string slotAccountName = slotAccount ? accountName : SlotAccountName;

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, slotAccountID, slotAccountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        MatchLockService matchLockService = host.Services.GetRequiredService<MatchLockService>();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        bool firstRefused = await matchLockService.Enforce(session);

        await Task.Delay(MatchLockService.NoticeRepeatInterval + TimeSpan.FromSeconds(1));

        bool secondRefused = await matchLockService.Enforce(session);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(client.GetStream(), QuietPeriod);

        string expectedMessage = slotAccount ? MatchLockService.SlotAccountMessage : MatchLockService.OtherAccountMessage(slotAccountName);

        using (Assert.Multiple())
        {
            await Assert.That(firstRefused).IsTrue();
            await Assert.That(secondRefused).IsTrue();
            await Assert.That(noticeMessages.Count).IsEqualTo(2);
            await Assert.That(noticeMessages.All(message => message == expectedMessage)).IsTrue();

            // The One Reminder Loop Sends Its Initial "AUTO_MATCH_CONNECT" And Then Its Reminders In This Time, While A Second Loop Would Begin With Another Initial One
            await Assert.That(connectRandomIntegers.Count(randomInteger => randomInteger is not -1)).IsEqualTo(slotAccount ? 1 : 0);
            await Assert.That(connectRandomIntegers.Any(randomInteger => randomInteger is -1)).IsEqualTo(slotAccount);
        }
    }

    [Test]
    public async Task Leaving_The_Match_Alone_Triggers_Nothing()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildLeftGame());

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(gameSocket.Requests.Count).IsEqualTo(0);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
        }
    }

    [Test]
    public async Task A_Lock_Whose_Slot_Is_Gone_Lifts_At_Login_And_Sends_Nothing()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(0);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket, GamePhase.GAME_PHASE_ACTIVE);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
        }
    }

    [Test]
    public async Task An_Unanswered_Slot_Query_Lifts_The_Lock_And_Lets_The_Attempt_Proceed()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        gameSocket.Answers = false;

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        bool refused = await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(client.GetStream(), QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(refused).IsFalse();
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
        }
    }

    [Test]
    [Arguments(AccountType.Guest)]
    [Arguments(AccountType.Staff)]
    public async Task Guest_And_Staff_Accounts_Are_Never_Sent_Back(AccountType accountType)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host, accountType);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        bool refused = await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        using (Assert.Multiple())
        {
            await Assert.That(refused).IsFalse();
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(gameSocket.Requests.Count).IsEqualTo(0);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
        }
    }

    [Test]
    public async Task A_Player_Waiting_For_The_Other_Machine_Is_Sent_Back_As_Soon_As_The_Slot_Is_Free()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        // The Game Is Closed On The Other Machine, Which Leaves The Slot Disconnected
        gameSocket.SlotTimeRemaining = SlotTimeRemaining;

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, ReminderTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.ConnectedElsewhereMessage);
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers is [not -1, .. List<int> reminders] && reminders.All(randomInteger => randomInteger is -1)).IsTrue();
            await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);
        }
    }

    [Test]
    public async Task Sending_Back_Never_Tells_The_Player_To_Close_Another_Computer()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await RefuseAndReadSendBack(host, accountName, stream);

        // The Game Client's Own Connection To The Locked Match Server Reads As Connected While It Loads
        gameSocket.SlotTimeRemaining = FakeMatchServerGameSocket.ConnectedSlotTimeRemaining;

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

        gameSocket.SlotTimeRemaining = SlotTimeRemaining;

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);
        }
    }

    [Test]
    public async Task Repeated_Attempts_While_Connected_Elsewhere_Message_Once()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        MatchLockService matchLockService = host.Services.GetRequiredService<MatchLockService>();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        bool firstRefused = await matchLockService.Enforce(session);
        bool secondRefused = await matchLockService.Enforce(session);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(client.GetStream(), MatchLockService.ReturnResendInterval);

        using (Assert.Multiple())
        {
            await Assert.That(firstRefused).IsTrue();
            await Assert.That(secondRefused).IsTrue();
            await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.ConnectedElsewhereMessage]);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task A_Wait_For_The_Other_Machine_Lasts_The_Connected_Elsewhere_Wait_And_A_Send_Back_The_Return_Window()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        DateTimeOffset waitStarted = DateTimeOffset.UtcNow;

        bool refused = await host.Services.GetRequiredService<MatchLockService>().Enforce(session);

        DateTimeOffset waitExpires = session.PendingMatchLockReturn?.Expires ?? DateTimeOffset.MinValue;

        gameSocket.SlotTimeRemaining = SlotTimeRemaining;

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        DateTimeOffset sentBack = DateTimeOffset.UtcNow;

        DateTimeOffset returnExpires = session.PendingMatchLockReturn?.Expires ?? DateTimeOffset.MinValue;

        using (Assert.Multiple())
        {
            await Assert.That(refused).IsTrue();
            await Assert.That(waitExpires).IsEqualTo(waitStarted + MatchLockService.ConnectedElsewhereWait).Within(ExpiryMargin);
            await Assert.That(connect?.RandomInteger).IsNotEqualTo(-1);
            await Assert.That(returnExpires).IsEqualTo(sentBack + MatchLockService.ReturnWindow).Within(ExpiryMargin);
        }
    }

    [Test]
    public async Task A_Server_Lost_During_The_Wait_Lifts_The_Lock_Without_Sending_The_Player_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        // The Match Server Stops Reporting Its Status While The Player Waits, As It Does Once It Has Gone Down, While Its Game Socket Still Reads The Slot As Connected
        Context.MatchServerChatSessions[lockedMatch.Lock.ServerID].Metadata.LastStatusUpdate = DateTimeOffset.UtcNow - StaleHostReaper.MatchServerSessionStaleStateThreshold * 2;

        // The Reminder Loop Verifies The Lock Again One Resend Interval After The Wait Begins
        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, ReminderTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.ConnectedElsewhereMessage);
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNull();
            await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn).IsNull();
        }
    }

    [Test]
    public async Task An_Unanswered_Query_During_The_Wait_Keeps_Waiting()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        // The Match Server's Game Socket Stops Answering While The Player Waits, As It Does When A Few Datagrams Are Lost, While The Match Server Still Reports Its Status
        gameSocket.Answers = false;

        int requestCount = gameSocket.Requests.Count;

        // The Reminder Loop Verifies The Lock Again One Resend Interval After The Wait Begins, And That Verification Gives Up Once Every Attempt Of The Reconnect Query Has Timed Out
        TimeSpan unansweredVerificationTimeout = MatchLockService.ReturnResendInterval + MatchServerReconnectQuery.AttemptTimeout * MatchServerReconnectQuery.RequestAttempts + QuietPeriod;

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, unansweredVerificationTimeout);

        int unansweredRequestCount = gameSocket.Requests.Count - requestCount;

        MatchLock? lockDuringOutage = await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID);

        bool? slotConnectedDuringOutage = Context.ClientChatSessions[accountName].PendingMatchLockReturn?.SlotConnected;

        // The Game Is Closed On The Other Machine, And The Match Server's Game Socket Answers Again
        gameSocket.SlotTimeRemaining = SlotTimeRemaining;
        gameSocket.Answers = true;

        // A Verification Which Was Already Under Way When The Game Socket Answered Again Still Goes Unanswered, So The Send-Back May Follow One Verification Later
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, unansweredVerificationTimeout + ReminderTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.ConnectedElsewhereMessage);
            await Assert.That(unansweredRequestCount).IsGreaterThanOrEqualTo(MatchServerReconnectQuery.RequestAttempts);
            await Assert.That(noticeMessages.Count).IsEqualTo(0);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(lockDuringOutage).IsNotNull();
            await Assert.That(slotConnectedDuringOutage).IsTrue();
            await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
            await Assert.That(connect?.RandomInteger).IsNotEqualTo(-1);
        }
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task A_Return_Which_Runs_Out_Tells_The_Player_Only_When_It_Was_Still_Waiting(bool slotConnected)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(slotConnected ? FakeMatchServerGameSocket.ConnectedSlotTimeRemaining : SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        if (slotConnected)
        {
            await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

            (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.ConnectedElsewhereMessage);
        }

        else
        {
            await RefuseAndReadSendBack(host, accountName, stream);
        }

        MatchLockReturn? pendingReturn = session.PendingMatchLockReturn;

        // The Return Runs Out Before The Reminder Loop Next Verifies The Lock, As A Wait Does Once The Other Machine Has Held The Slot For The Whole Connected Elsewhere Wait
        if (pendingReturn is not null)
            pendingReturn.Expires = DateTimeOffset.UtcNow;

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, ReminderTimeout);

        // A Wait Has Sent Nobody Anywhere, So The Player Is Told That It Ran Out, While A Send-Back Ends Without A Notice, Because The Player Has Already Been Told Why And Sent Back
        List<string> expectedNoticeMessages = slotConnected ? [MatchLockService.ConnectedElsewhereWaitExpiredMessage] : [];

        using (Assert.Multiple())
        {
            await Assert.That(pendingReturn).IsNotNull();
            await Assert.That(noticeMessages).IsEquivalentTo(expectedNoticeMessages);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
            await Assert.That(session.PendingMatchLockReturn).IsNull();
            await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
        }
    }

    [Test]
    public async Task Another_Account_Is_Named_The_Slot_Account_While_The_Slot_Is_Connected_Elsewhere()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName)]);
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
        }
    }

    /// <summary>
    ///     Sends the login action, and reads the notice with which it tells the slot account about the locked match.
    /// </summary>
    private static async Task LogInAndReadNotice(NetworkStream stream)
    {
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

        await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountLoginMessage);
    }

    /// <summary>
    ///     Refuses an attempt to play another game, and reads the notice and the initial "AUTO_MATCH_CONNECT" with which it sends the slot account back to the locked match.
    /// </summary>
    private static async Task RefuseAndReadSendBack(ChatServerHost host, string accountName, NetworkStream stream)
    {
        await host.Services.GetRequiredService<MatchLockService>().Enforce(Context.ClientChatSessions[accountName]);

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
        await Assert.That(connect is { RandomInteger: not -1 }).IsTrue();
    }
}
