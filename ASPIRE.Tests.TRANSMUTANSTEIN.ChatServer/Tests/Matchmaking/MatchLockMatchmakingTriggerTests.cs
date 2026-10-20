namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for refusing matchmaking to a locked player: creating a group of any type, joining a group, queueing, and reaching full loading each send the player back to their match in progress, and a group with a locked member cannot queue.
///     Every test logs in with a fresh session cookie, and disposes its locked match and the groups which it created, because every chat server host shares the unprefixed distributed cache and the matchmaking group registry while each host's database restarts its identifiers, so a lock or a group left behind would bind a later test.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchLockMatchmakingTriggerTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    private const uint SlotTimeRemaining = 120000;

    /// <summary>
    ///     How long a frame which is expected to arrive is awaited.
    /// </summary>
    private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     How long a test listens to be sure that a frame which is not expected does not arrive.
    /// </summary>
    private static readonly TimeSpan QuietPeriod = TimeSpan.FromSeconds(2);

    [Test]
    [Arguments(ChatProtocol.TMMType.TMM_TYPE_PVP)]
    [Arguments(ChatProtocol.TMMType.TMM_TYPE_COOP)]
    public async Task Creating_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back(ChatProtocol.TMMType groupType)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            // A Lock Refuses A Co-Op Group Too, Because Any Group Leads To Another Game
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupCreate(groupType));

            await AssertSentBack(stream, lockedMatch);

            await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Joining_A_Group_While_Locked_Is_Refused_And_Sends_The_Player_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (_, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(leaderClient.GetStream(), leaderAccountID);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoin(leaderAccountName));

            await AssertSentBack(stream, lockedMatch);

            await Assert.That(group.Members.Count).IsEqualTo(1);
        }

        finally
        {
            DisbandGroups(leaderAccountID, accountID);
        }
    }

    [Test]
    public async Task Queueing_While_Locked_Is_Refused_And_Sends_The_Player_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            // A Group Outlives The Match It Is Matched Into, So A Player Can Still Lead A Group Which Was Created Before Their Lock
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            await AssertSentBack(stream, lockedMatch);

            await Assert.That(group.QueueStartTime).IsNull();
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Reaching_Full_Loading_While_Locked_Does_Not_Queue()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            // The Leader Is Ready Before Loading Starts, So That Full Loading Alone Would Queue The Group
            group.Leader.IsReady = true;

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupPlayerLoadingStatus(100));

            await AssertSentBack(stream, lockedMatch);

            await Assert.That(group.QueueStartTime).IsNull();
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Queueing_A_Group_With_A_Locked_Member_Tells_The_Leader_And_Sends_The_Member_Back()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (_, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (int memberUserID, int memberAccountID, string memberAccountName, string memberCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient memberClient = await host.ConnectAndAuthenticateClientAsync(memberAccountID, memberAccountName, memberCookie, RemoteIP);

        NetworkStream leaderStream = leaderClient.GetStream();
        NetworkStream memberStream = memberClient.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(leaderStream, leaderAccountID);

            await JoinGroup(memberStream, leaderAccountName);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, memberUserID, memberAccountID, memberAccountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            (string SenderName, string Message)? leaderNotice = await ChatTestProtocol.ReadNotice(leaderStream, FrameTimeout);

            await AssertSentBack(memberStream, lockedMatch);

            using (Assert.Multiple())
            {
                await Assert.That(leaderNotice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
                await Assert.That(leaderNotice?.Message).IsEqualTo(MatchLockService.GroupMemberMessage(memberAccountName));
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, memberAccountID);
        }
    }

    [Test]
    public async Task Queueing_A_Group_With_A_Member_Still_In_The_Match_Does_Not_Message_The_Member()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (_, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (int memberUserID, int memberAccountID, string memberAccountName, string memberCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient memberClient = await host.ConnectAndAuthenticateClientAsync(memberAccountID, memberAccountName, memberCookie, RemoteIP);

        NetworkStream leaderStream = leaderClient.GetStream();
        NetworkStream memberStream = memberClient.GetStream();

        ClientChatSession memberSession = Context.ClientChatSessions[memberAccountName];

        try
        {
            MatchmakingGroup group = await CreateGroup(leaderStream, leaderAccountID);

            await JoinGroup(memberStream, leaderAccountName);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, memberUserID, memberAccountID, memberAccountName, gameSocket);

            // A Teammate Who Is Still Connected To The Locked Match Reports Joining Its Match Server, And Is Still In The Group Which Its Leader Queues
            await ChatTestProtocol.WriteFrame(memberStream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

            bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => memberSession.ReportedJoiningGame, FrameTimeout);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            (string SenderName, string Message)? leaderNotice = await ChatTestProtocol.ReadNotice(leaderStream, FrameTimeout);

            (List<string> memberNoticeMessages, List<int> memberConnectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(memberStream, MatchLockService.ReturnResendInterval * 2);

            using (Assert.Multiple())
            {
                await Assert.That(reportedJoiningGame).IsTrue();
                await Assert.That(leaderNotice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
                await Assert.That(leaderNotice?.Message).IsEqualTo(MatchLockService.GroupMemberMessage(memberAccountName));
                await Assert.That(group.QueueStartTime).IsNull();
                await Assert.That(memberNoticeMessages.Count).IsEqualTo(0);
                await Assert.That(memberConnectRandomIntegers.Count).IsEqualTo(0);
                await Assert.That(memberSession.PendingMatchLockReturn).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, memberAccountID);
        }
    }

    [Test]
    public async Task Creating_A_Group_While_Still_In_The_Match_Gives_Only_The_In_Match_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        try
        {
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

            bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => session.ReportedJoiningGame, FrameTimeout);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupCreate(ChatProtocol.TMMType.TMM_TYPE_PVP));
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupCreate(ChatProtocol.TMMType.TMM_TYPE_PVP));

            (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

            using (Assert.Multiple())
            {
                await Assert.That(reportedJoiningGame).IsTrue();
                await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.InMatchMessage]);
                await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
                await Assert.That(session.PendingMatchLockReturn).IsNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Queueing_While_Still_In_The_Match_Gives_The_Leader_Only_The_In_Match_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

            bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => session.ReportedJoiningGame, FrameTimeout);

            MakeReadyToQueue(group);

            // The Leader's Own Request To Queue Is Its Own Attempt To Play, So It Is Told Why, Once Per Notice Repeat Interval
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

            using (Assert.Multiple())
            {
                await Assert.That(reportedJoiningGame).IsTrue();
                await Assert.That(noticeMessages).IsEquivalentTo([MatchLockService.InMatchMessage]);
                await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
                await Assert.That(session.PendingMatchLockReturn).IsNull();
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Reaching_Full_Loading_While_Still_In_The_Match_Gives_No_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        ClientChatSession session = Context.ClientChatSessions[accountName];

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

            bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => session.ReportedJoiningGame, FrameTimeout);

            // The Leader Is Ready Before Loading Starts, So That Full Loading Alone Would Queue The Group, Which Is Not The Leader's Own Request To Queue
            group.Leader.IsReady = true;

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupPlayerLoadingStatus(100));

            (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

            using (Assert.Multiple())
            {
                await Assert.That(reportedJoiningGame).IsTrue();
                await Assert.That(noticeMessages.Count).IsEqualTo(0);
                await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
                await Assert.That(session.PendingMatchLockReturn).IsNull();
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Queueing_As_A_Locked_Leader_Sends_The_Leader_Back_Without_A_Group_Message()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int leaderUserID, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (_, int memberAccountID, string memberAccountName, string memberCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient memberClient = await host.ConnectAndAuthenticateClientAsync(memberAccountID, memberAccountName, memberCookie, RemoteIP);

        NetworkStream leaderStream = leaderClient.GetStream();
        NetworkStream memberStream = memberClient.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(leaderStream, leaderAccountID);

            await JoinGroup(memberStream, leaderAccountName);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, leaderUserID, leaderAccountID, leaderAccountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            // The Leader's Own Notice Tells It Why, So A Group Message Naming The Leader Would Only Repeat It
            (List<string> leaderNoticeMessages, List<int> leaderConnectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(leaderStream, QuietPeriod);
            (List<string> memberNoticeMessages, List<int> memberConnectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(memberStream, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(leaderNoticeMessages.Count).IsEqualTo(1);
                await Assert.That(leaderNoticeMessages.SingleOrDefault()).IsEqualTo(MatchLockService.SlotAccountMessage);
                await Assert.That(leaderConnectRandomIntegers.Count(randomInteger => randomInteger is not -1)).IsEqualTo(1);
                await Assert.That(memberNoticeMessages.Count).IsEqualTo(0);
                await Assert.That(memberConnectRandomIntegers.Count).IsEqualTo(0);
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, memberAccountID);
        }
    }

    [Test]
    public async Task Reaching_Full_Loading_Without_A_Lock_Still_Queues()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (_, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            // The Leader Is Ready Before Loading Starts, So That Full Loading Alone Queues The Group
            group.Leader.IsReady = true;

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupPlayerLoadingStatus(100));

            bool queued = await ChatTestProtocol.WaitUntil(() => group.QueueStartTime is not null, FrameTimeout);

            await Assert.That(queued).IsTrue();
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    /// <summary>
    ///     Creates a player-versus-player group led by the account, and returns it once the chat server has sent the new group to its leader.
    /// </summary>
    private static async Task<MatchmakingGroup> CreateGroup(NetworkStream stream, int accountID)
    {
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupCreate(ChatProtocol.TMMType.TMM_TYPE_PVP));

        if (await ChatTestProtocol.ReadUntilCommand(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE, FrameTimeout) is null)
            throw new InvalidOperationException(@$"The Group Of Account ID ""{accountID}"" Was Not Created Within The Timeout");

        return MatchmakingService.GetMatchmakingGroup(accountID)
            ?? throw new InvalidOperationException(@$"No Group Was Found For Account ID ""{accountID}""");
    }

    /// <summary>
    ///     Joins the group which the named account leads, and returns once the chat server has sent the joined group to the joining account.
    /// </summary>
    private static async Task JoinGroup(NetworkStream stream, string leaderAccountName)
    {
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoin(leaderAccountName));

        if (await ChatTestProtocol.ReadUntilCommand(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE, FrameTimeout) is null)
            throw new InvalidOperationException(@$"The Group Of Account ""{leaderAccountName}"" Was Not Joined Within The Timeout");
    }

    /// <summary>
    ///     Makes every member of the group ready and fully loaded, so that only the match lock can refuse its queue.
    /// </summary>
    private static void MakeReadyToQueue(MatchmakingGroup group)
    {
        foreach (MatchmakingGroupMember member in group.Members)
        {
            member.IsReady = true;
            member.LoadingPercent = 100;
        }
    }

    /// <summary>
    ///     Reads the notice and the initial "AUTO_MATCH_CONNECT" with which the account which holds the slot is sent back to the locked match, and asserts that they name that match.
    /// </summary>
    private static async Task AssertSentBack(NetworkStream stream, LockedMatch lockedMatch)
    {
        (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);
        (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? connect = await ChatTestProtocol.ReadAutoMatchConnect(stream, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
            await Assert.That(notice?.Message).IsEqualTo(MatchLockService.SlotAccountMessage);
            await Assert.That(connect?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
            await Assert.That(connect?.RandomInteger).IsNotEqualTo(-1);
        }
    }

    /// <summary>
    ///     Removes each account from its matchmaking group, as the chat server does when the account's session ends, so that the last account to leave a group disbands it.
    /// </summary>
    private static void DisbandGroups(params int[] accountIDs)
    {
        foreach (int accountID in accountIDs)
            MatchmakingService.GetMatchmakingGroup(accountID)?.RemoveMember(accountID);
    }
}
