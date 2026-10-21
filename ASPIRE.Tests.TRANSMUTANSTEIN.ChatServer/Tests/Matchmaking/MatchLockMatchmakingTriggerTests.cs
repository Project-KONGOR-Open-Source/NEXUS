namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Matchmaking;

/// <summary>
///     Tests for refusing matchmaking to a locked player: joining a group, queueing, and reaching full loading each send the account which holds the slot back to its match in progress, or show another account, and a slot account which waits for its other game, the game client's "Temporary Ban" popup before the notice, creating a group refuses nothing, and a party with a locked member is unreadied and told which members hold a match lock.
///     Every test logs in with a fresh session cookie, and disposes its locked match and the groups which it created, because every chat server host shares the unprefixed distributed cache and the matchmaking group registry while each host's database restarts its identifiers, so a lock or a group left behind would bind a later test.
/// </summary>
[NotInParallel(nameof(ChatServerHost))]
public sealed class MatchLockMatchmakingTriggerTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    private const uint SlotTimeRemaining = 120000;

    /// <summary>
    ///     The name of an account of the locked user other than the one which logs in, which holds the user's slot in the locked match.
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

    [Test]
    [Arguments(ChatProtocol.TMMType.TMM_TYPE_PVP)]
    [Arguments(ChatProtocol.TMMType.TMM_TYPE_COOP)]
    public async Task Creating_A_Group_While_Locked_Creates_It_And_Sends_Nothing(ChatProtocol.TMMType groupType)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            // The Game Client Creates A Solo Group By Itself After Login, So Creating One Is Not An Attempt To Play
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupCreate(groupType));

            bool groupUpdated = await ChatTestProtocol.ReadUntilCommand(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE, FrameTimeout) is not null;

            (List<string> noticeMessages, List<int> connectRandomIntegers) = await ChatTestProtocol.ReadSendBackFrames(stream, MatchLockService.ReturnResendInterval * 2);

            MatchmakingGroup? group = MatchmakingService.GetMatchmakingGroup(accountID);

            using (Assert.Multiple())
            {
                await Assert.That(groupUpdated).IsTrue();
                await Assert.That(group).IsNotNull();
                await Assert.That(group?.Members.Count).IsEqualTo(1);
                await Assert.That(group?.Leader.Account.ID).IsEqualTo(accountID);
                await Assert.That(noticeMessages.Count).IsEqualTo(0);
                await Assert.That(connectRandomIntegers.Count).IsEqualTo(0);
                await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn).IsNull();
                await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(userID)).IsNotNull();
            }
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
    public async Task Joining_A_Group_On_Another_Account_Shows_The_Temporary_Ban_Before_The_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (_, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        // Another Account Of The Same User Holds The Slot, So This Account Has No Match To Return To
        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(leaderClient.GetStream(), leaderAccountID);

            // The Game Client Creates A Solo Group By Itself, Which It Leaves On The Failure Reply
            await CreateGroup(stream, accountID);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoin(leaderAccountName));

            (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure = await ChatTestProtocol.ReadMatchmakingFailure(stream, FrameTimeout);

            // The Notice Follows The Failure Reply, Because Reading Up To The Failure Reply Would Have Skipped A Notice Which Came First
            // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(MatchLockNotices(later)).IsEquivalentTo([MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName)]);
                await Assert.That(later.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(group.Members.Count).IsEqualTo(1);
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
            }
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
    public async Task Queueing_On_Another_Account_Shows_The_Temporary_Ban_Before_The_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            // Another Account Of The Same User Holds The Slot, So This Account Has No Match To Return To, And Its Matchmaking Screen Covers The Chat
            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure = await ChatTestProtocol.ReadMatchmakingFailure(stream, FrameTimeout);

            // The Notice Follows The Failure Reply, Because Reading Up To The Failure Reply Would Have Skipped A Notice Which Came First
            // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(failure?.Seconds ?? 0).IsLessThanOrEqualTo((uint) MatchLock.Lifetime.TotalSeconds);
                await Assert.That(failure?.Seconds ?? 0).IsGreaterThan((uint) (MatchLock.Lifetime - TimeSpan.FromMinutes(1)).TotalSeconds);
                await Assert.That(MatchLockNotices(later)).IsEquivalentTo([MatchLockService.OtherAccountMessage(lockedMatch.Lock.AccountName)]);
                await Assert.That(later.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(group.QueueStartTime).IsNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Queueing_While_Connected_Elsewhere_Shows_The_Temporary_Ban_Before_The_Notice()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            // The Slot Is Still Connected From Another Game, So The Player Cannot Be Sent Back Yet, And Its Matchmaking Screen Covers The Chat
            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure = await ChatTestProtocol.ReadMatchmakingFailure(stream, FrameTimeout);

            // The Notice Follows The Failure Reply, Because Reading Up To The Failure Reply Would Have Skipped A Notice Which Came First
            // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(MatchLockNotices(later)).IsEquivalentTo([MatchLockService.ConnectedElsewhereMessage]);
                await Assert.That(later.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(group.QueueStartTime).IsNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
            }
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

            // The Loading Update Precedes The Queue, So That No Group Update Follows The Pull
            bool loadingUpdated = await ChatTestProtocol.ReadUntilCommand(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE, FrameTimeout) is not null;

            await AssertSentBack(stream, lockedMatch);

            using (Assert.Multiple())
            {
                await Assert.That(loadingUpdated).IsTrue();
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Queueing_A_Party_With_A_Locked_Member_Unreadies_It_And_Sends_The_Member_Back()
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

            await JoinGroup(memberStream, leaderAccountName, leaderStream);

            // A Premade Party Outlives The Match It Played Together, So It Was Formed Before The Member's Lock
            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, memberUserID, memberAccountID, memberAccountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> memberFrames = await ReadThroughSendBack(memberStream);
            List<(ushort Command, ChatBuffer Frame)> leaderFrames = await ChatTestProtocol.ReadCommandsUntil(leaderStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            string groupMemberLockedMessage = MatchLockService.GroupMemberLockedMessage(memberAccountName);

            using (Assert.Multiple())
            {
                await Assert.That(SendsBack(memberFrames, lockedMatch)).IsTrue();
                await Assert.That(memberFrames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN)).IsFalse();
                await Assert.That(MatchLockNotices(memberFrames)).IsEquivalentTo([MatchLockService.SlotAccountMessage, groupMemberLockedMessage]);
                await Assert.That(MatchmakingService.GetMatchmakingGroup(memberAccountID)).IsEqualTo(group);
                await Assert.That(leaderFrames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsTrue();
                await Assert.That(leaderFrames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN)).IsFalse();
                await Assert.That(MatchLockNotices(leaderFrames)).IsEquivalentTo([groupMemberLockedMessage]);
                await Assert.That(group.Leader.IsReady).IsFalse();
                await Assert.That(group.Members.All(member => member.LoadingPercent is 0)).IsTrue();
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, memberAccountID);
        }
    }

    [Test]
    public async Task Queueing_A_Party_With_A_Member_Still_In_The_Match_Unreadies_It_And_Tells_Every_Member()
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

            await JoinGroup(memberStream, leaderAccountName, leaderStream);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, memberUserID, memberAccountID, memberAccountName, gameSocket);

            // A Teammate Who Is Still Connected To The Locked Match Reports Joining Its Match Server, And Is Still In The Party Which Its Leader Queues
            await ChatTestProtocol.WriteFrame(memberStream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

            bool reportedJoiningGame = await ChatTestProtocol.WaitUntil(() => memberSession.ReportedJoiningGame, FrameTimeout);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> memberFrames = await ChatTestProtocol.ReadCommandsUntil(memberStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, MatchLockService.ReturnResendInterval * 2);
            List<(ushort Command, ChatBuffer Frame)> leaderFrames = await ChatTestProtocol.ReadCommandsUntil(leaderStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            string groupMemberLockedMessage = MatchLockService.GroupMemberLockedMessage(memberAccountName);

            using (Assert.Multiple())
            {
                await Assert.That(reportedJoiningGame).IsTrue();
                await Assert.That(memberFrames.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN)).IsFalse();
                await Assert.That(MatchLockNotices(memberFrames)).IsEquivalentTo([groupMemberLockedMessage]);
                await Assert.That(memberSession.PendingMatchLockReturn).IsNull();
                await Assert.That(leaderFrames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsTrue();
                await Assert.That(MatchLockNotices(leaderFrames)).IsEquivalentTo([groupMemberLockedMessage]);
                await Assert.That(group.Leader.IsReady).IsFalse();
                await Assert.That(group.Members.All(member => member.LoadingPercent is 0)).IsTrue();
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, memberAccountID);
        }
    }

    [Test]
    public async Task Queueing_A_Party_As_Its_Locked_Leader_Unreadies_It_And_Sends_The_Leader_Back()
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

            await JoinGroup(memberStream, leaderAccountName, leaderStream);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, leaderUserID, leaderAccountID, leaderAccountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> leaderFrames = await ReadThroughSendBack(leaderStream);
            List<(ushort Command, ChatBuffer Frame)> memberFrames = await ChatTestProtocol.ReadCommandsUntil(memberStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            string groupMemberLockedMessage = MatchLockService.GroupMemberLockedMessage(leaderAccountName);

            using (Assert.Multiple())
            {
                await Assert.That(SendsBack(leaderFrames, lockedMatch)).IsTrue();
                await Assert.That(leaderFrames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN)).IsFalse();
                await Assert.That(MatchLockNotices(leaderFrames)).IsEquivalentTo([MatchLockService.SlotAccountMessage, groupMemberLockedMessage]);
                await Assert.That(memberFrames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsTrue();
                await Assert.That(MatchLockNotices(memberFrames)).IsEquivalentTo([groupMemberLockedMessage]);
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, memberAccountID);
        }
    }

    [Test]
    public async Task Queueing_A_Party_With_Two_Locked_Members_Tells_Every_Member_About_Both()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket firstGameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);
        await using FakeMatchServerGameSocket secondGameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (_, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (int firstMemberUserID, int firstMemberAccountID, string firstMemberAccountName, string firstMemberCookie) = await ChatTestData.SeedPlayer(host);
        (int secondMemberUserID, int secondMemberAccountID, string secondMemberAccountName, string secondMemberCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient firstMemberClient = await host.ConnectAndAuthenticateClientAsync(firstMemberAccountID, firstMemberAccountName, firstMemberCookie, RemoteIP);
        using TcpClient secondMemberClient = await host.ConnectAndAuthenticateClientAsync(secondMemberAccountID, secondMemberAccountName, secondMemberCookie, RemoteIP);

        NetworkStream leaderStream = leaderClient.GetStream();
        NetworkStream firstMemberStream = firstMemberClient.GetStream();
        NetworkStream secondMemberStream = secondMemberClient.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(leaderStream, leaderAccountID);

            await JoinGroup(firstMemberStream, leaderAccountName, leaderStream);
            await JoinGroup(secondMemberStream, leaderAccountName, leaderStream, firstMemberStream);

            await using LockedMatch firstLockedMatch = await ChatTestData.SeedLockedMatch(host, firstMemberUserID, firstMemberAccountID, firstMemberAccountName, firstGameSocket);
            await using LockedMatch secondLockedMatch = await ChatTestData.SeedLockedMatch(host, secondMemberUserID, secondMemberAccountID, secondMemberAccountName, secondGameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> firstMemberFrames = await ReadThroughSendBack(firstMemberStream);
            List<(ushort Command, ChatBuffer Frame)> secondMemberFrames = await ReadThroughSendBack(secondMemberStream);
            List<(ushort Command, ChatBuffer Frame)> leaderFrames = await ChatTestProtocol.ReadCommandsUntil(leaderStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            List<string> groupMemberLockedMessages = [MatchLockService.GroupMemberLockedMessage(firstMemberAccountName), MatchLockService.GroupMemberLockedMessage(secondMemberAccountName)];

            using (Assert.Multiple())
            {
                await Assert.That(MatchLockNotices(leaderFrames)).IsEquivalentTo(groupMemberLockedMessages);
                await Assert.That(MatchLockNotices(firstMemberFrames)).IsEquivalentTo([MatchLockService.SlotAccountMessage, .. groupMemberLockedMessages]);
                await Assert.That(MatchLockNotices(secondMemberFrames)).IsEquivalentTo([MatchLockService.SlotAccountMessage, .. groupMemberLockedMessages]);
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, firstMemberAccountID, secondMemberAccountID);
        }
    }

    [Test]
    public async Task Queueing_While_Still_In_The_Match_Gives_No_Notice_And_No_Return()
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

            // The Game Client Never Queues From A Game, So A Queue While The Session Reports The Locked Match Is Refused Without Disturbing The Player
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, MatchLockService.ReturnResendInterval * 2);

            using (Assert.Multiple())
            {
                await Assert.That(reportedJoiningGame).IsTrue();
                await Assert.That(MatchLockNotices(frames).Count).IsEqualTo(0);
                await Assert.That(frames.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN)).IsFalse();
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

            // The Leader Is Ready Before Loading Starts, So That Full Loading Alone Would Queue The Group
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
    ///     Joins the group which the named account leads, and returns once the chat server has sent the joined group to the joining account and to each of the given members already in the group, so that no test mistakes that update for a later one.
    /// </summary>
    private static async Task JoinGroup(NetworkStream stream, string leaderAccountName, params NetworkStream[] memberStreams)
    {
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoin(leaderAccountName));

        List<NetworkStream> joinedStreams = [stream, .. memberStreams];

        foreach (NetworkStream joinedStream in joinedStreams)
        {
            if (await ChatTestProtocol.ReadUntilCommand(joinedStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE, FrameTimeout) is null)
                throw new InvalidOperationException(@$"The Group Of Account ""{leaderAccountName}"" Was Not Joined Within The Timeout");
        }
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
    ///     Reads every frame up to and including the first "AUTO_MATCH_CONNECT", and every frame which follows it within the quiet period, which ends before a reminder could follow.
    /// </summary>
    private static async Task<List<(ushort Command, ChatBuffer Frame)>> ReadThroughSendBack(NetworkStream stream)
    {
        List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, FrameTimeout);

        frames.AddRange(await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod));

        return frames;
    }

    /// <summary>
    ///     Returns the messages of the match lock's notices among the frames, in the order in which they arrived.
    ///     Reading a notice moves its frame past it, so it is called once for each list of frames.
    /// </summary>
    private static List<string> MatchLockNotices(List<(ushort Command, ChatBuffer Frame)> frames)
        => [.. frames.Where(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL).Select(frame => ChatTestProtocol.ParseNotice(frame.Frame))
            .Where(notice => notice.SenderName is MatchLockService.MessageSenderName).Select(notice => notice.Message)];

    /// <summary>
    ///     Decides whether the frames hold an initial "AUTO_MATCH_CONNECT", rather than a reminder, which sends the player back to the locked match.
    /// </summary>
    private static bool SendsBack(List<(ushort Command, ChatBuffer Frame)> frames, LockedMatch lockedMatch)
        => frames.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT
            && ChatTestProtocol.ParseAutoMatchConnect(frame.Frame) is { RandomInteger: not -1 } connect && connect.MatchID == lockedMatch.Lock.MatchID);

    /// <summary>
    ///     Reads the frames with which the account which holds the slot is sent back to the locked match, and those which follow within the quiet period, and asserts that its only notice and its initial "AUTO_MATCH_CONNECT" send it back to that match.
    ///     Neither a failure reply nor a group update may arrive, because the game client leaves a solo group by itself when it is sent back, and a group update would put it back into that group.
    /// </summary>
    private static async Task AssertSentBack(NetworkStream stream, LockedMatch lockedMatch)
    {
        List<(ushort Command, ChatBuffer Frame)> frames = await ReadThroughSendBack(stream);

        using (Assert.Multiple())
        {
            await Assert.That(MatchLockNotices(frames)).IsEquivalentTo([MatchLockService.SlotAccountMessage]);
            await Assert.That(SendsBack(frames, lockedMatch)).IsTrue();
            await Assert.That(frames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
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
