namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for sending a locked player back to their match in progress: the notice which explains why another game is refused, the "AUTO_MATCH_CONNECT" which is sent at once, and the reminders which repeat it until the player reaches the locked match server, the lock lifts, the session ends, or the return window expires.
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

    [Test]
    public async Task Logging_In_While_Locked_Sends_The_Player_Back_To_Their_Match()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
            await Assert.That(connect?.ArrangedMatchType).IsEqualTo((byte) lockedMatch.Lock.ArrangedMatchType);
            await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
            await Assert.That(connect?.Address).IsEqualTo(lockedMatch.Lock.ServerAddress);
            await Assert.That((ushort?) connect?.Port).IsEqualTo(lockedMatch.Lock.ServerPort);
            await Assert.That(connect?.RandomInteger).IsNotEqualTo(-1);
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

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(stream, QuietPeriod);

        using (Assert.Multiple())
        {
            await Assert.That(noticeMessages.Count).IsEqualTo(1);
            await Assert.That(noticeMessages.SingleOrDefault()).IsEqualTo(MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName));
            await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
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

        await LogInAndReadSendBack(firstClient.GetStream());

        // The Game Client Reconnects To The Chat Server With The Session Cookie Of The Same Login, Which Replaces The First Chat Session
        using TcpClient secondClient = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = secondClient.GetStream();

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(stream, QuietPeriod);

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

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(stream, QuietPeriod);

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

        await LogInAndReadSendBack(stream);

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

        await LogInAndReadSendBack(stream);

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

        await LogInAndReadSendBack(stream);

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

        int otherServerPort = Random.Shared.Next(20_000, 30_000);

        await ChatTestData.SeedMatchServer(host.Services, Random.Shared.Next(1_000_000, int.MaxValue), otherHostAccountID, otherHostAccountName, Guid.CreateVersion7().ToString(), otherServerPort, OtherServerIPAddress);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        await LogInAndReadSendBack(stream);

        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{OtherServerIPAddress}:{otherServerPort}"));

        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, ReminderTimeout);

        await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);
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

        await LogInAndReadSendBack(client.GetStream());

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

        await LogInAndReadSendBack(stream);

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
    [Arguments(true)]
    [Arguments(false)]
    public async Task An_Attempt_After_The_Resend_Interval_Gets_The_Notice_Again(bool slotAccount)
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

        await Task.Delay(MatchLockService.ReturnResendInterval + TimeSpan.FromSeconds(1));

        bool secondRefused = await matchLockService.Enforce(session);

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(client.GetStream(), QuietPeriod);

        string expectedMessage = slotAccount ? MatchLockService.SlotAccountMessage : MatchLockService.OtherAccountMessage(slotAccountName);

        using (Assert.Multiple())
        {
            await Assert.That(firstRefused).IsTrue();
            await Assert.That(secondRefused).IsTrue();
            await Assert.That(noticeMessages.Count).IsEqualTo(2);
            await Assert.That(noticeMessages.All(message => message == expectedMessage)).IsTrue();

            // The One Reminder Loop Sends Its Initial "AUTO_MATCH_CONNECT" And Then One Reminder In This Time, While A Second Loop Would Begin With Another Initial One
            await Assert.That(connectRandomIntegers.Count(randomInteger => randomInteger is not -1)).IsEqualTo(slotAccount ? 1 : 0);
            await Assert.That(connectRandomIntegers.Count(randomInteger => randomInteger is -1)).IsEqualTo(slotAccount ? 1 : 0);
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

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(stream, QuietPeriod);

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

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(stream, QuietPeriod);

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

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(client.GetStream(), QuietPeriod);

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

        (List<string> noticeMessages, List<int> connectRandomIntegers) = await ReadSendBackFrames(stream, QuietPeriod);

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

    /// <summary>
    ///     Sends the login action, and reads the notice and the initial "AUTO_MATCH_CONNECT" with which it sends the slot account back to the locked match.
    /// </summary>
    private static async Task LogInAndReadSendBack(NetworkStream stream)
    {
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(ChatProtocol.ActionCampaign.AC_DAILY_LOGINS));

        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
        await Assert.That(connect is { RandomInteger: not -1 }).IsTrue();
    }

    /// <summary>
    ///     Reads every frame which arrives within the period, and returns the messages of the match lock's notices and the random integers of the "AUTO_MATCH_CONNECT" frames among them, so that frames of either kind are counted however they interleave.
    ///     Notices from other senders are left out, because the chat server also sends notices about chat channels.
    /// </summary>
    private static async Task<(List<string> NoticeMessages, List<int> ConnectRandomIntegers)> ReadSendBackFrames(NetworkStream stream, TimeSpan period)
    {
        List<string> noticeMessages = [];
        List<int> connectRandomIntegers = [];

        using CancellationTokenSource cancellation = new (period);

        try
        {
            while (true)
            {
                ChatBuffer frame = new (await ChatTestProtocol.ReadFramePayload(stream, cancellation.Token));

                ushort command = BitConverter.ToUInt16(frame.ReadCommandBytes(), 0);

                if (command == ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL && ChatTestProtocol.ParseNotice(frame) is { SenderName: MatchLockService.MessageSenderName } notice)
                    noticeMessages.Add(notice.Message);

                else if (command == ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT)
                    connectRandomIntegers.Add(ChatTestProtocol.ParseAutoMatchConnect(frame).RandomInteger);
            }
        }

        catch (OperationCanceledException) { }

        return (noticeMessages, connectRandomIntegers);
    }
}
