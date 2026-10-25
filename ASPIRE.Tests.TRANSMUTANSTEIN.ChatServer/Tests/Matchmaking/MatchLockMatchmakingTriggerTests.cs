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

            (List<(ushort Command, ChatBuffer Frame)> earlier, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure) = await ReadThroughMatchmakingFailure(stream);

            // The Frames On Both Sides Of The Failure Reply Are Read, Because The Notice Must Come After The Reply, While A Group Update About The Player's Removal Would Come Before It
            // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(MatchLockNotices(later)).IsEquivalentTo([MatchLockService.OtherAccountMessage(lockedMatch.Lock.TaggedAccountName)]);
                await Assert.That(earlier.Concat(later).Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
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
    [Arguments(false)]
    [Arguments(true)]
    public async Task Inviting_A_Locked_Player_Is_Not_Delivered_And_Tells_The_Inviter(bool inviteeStillInTheMatch)
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(inviteeStillInTheMatch ? FakeMatchServerGameSocket.ConnectedSlotTimeRemaining : SlotTimeRemaining);

        (_, int inviterAccountID, string inviterAccountName, string inviterCookie) = await ChatTestData.SeedPlayer(host);
        (int inviteeUserID, int inviteeAccountID, string inviteeAccountName, string inviteeCookie) = await ChatTestData.SeedPlayer(host);

        // The Notice Names The Invitee With Its Clan Tag, Which The Chat Session Loads At The Handshake, While The Invite Names The Invitee Without It
        string inviteeAccountNameWithClanTag = await ChatTestData.SeedClanMember(host.Services, inviteeAccountID);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, inviteeUserID, inviteeAccountID, inviteeAccountName, gameSocket);

        using TcpClient inviterClient = await host.ConnectAndAuthenticateClientAsync(inviterAccountID, inviterAccountName, inviterCookie, RemoteIP);
        using TcpClient inviteeClient = await host.ConnectAndAuthenticateClientAsync(inviteeAccountID, inviteeAccountName, inviteeCookie, RemoteIP);

        NetworkStream inviterStream = inviterClient.GetStream();
        NetworkStream inviteeStream = inviteeClient.GetStream();

        ClientChatSession inviteeSession = Context.ClientChatSessions[inviteeAccountName];

        try
        {
            MatchmakingGroup group = await CreateGroup(inviterStream, inviterAccountID);

            // A Player Still In Their Match Reports Joining Its Match Server, And Is Locked All The Same
            if (inviteeStillInTheMatch)
            {
                await ChatTestProtocol.WriteFrame(inviteeStream, ChatTestProtocol.BuildJoiningGame($"{lockedMatch.Lock.ServerAddress}:{lockedMatch.Lock.ServerPort}"));

                await ChatTestProtocol.WaitUntil(() => inviteeSession.ReportedJoiningGame, FrameTimeout);
            }

            await ChatTestProtocol.WriteFrame(inviterStream, ChatTestProtocol.BuildGroupInvite(inviteeAccountName));

            (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(inviterStream, FrameTimeout);

            ChatBuffer? broadcast = await ChatTestProtocol.ReadUntilCommand(inviterStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE_BROADCAST, QuietPeriod);
            ChatBuffer? invite = await ChatTestProtocol.ReadUntilCommand(inviteeStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(notice?.SenderName).IsEqualTo(MatchLockService.MessageSenderName);
                await Assert.That(notice?.Message).IsEqualTo(MatchLockService.InviteeLockedMessage(inviteeAccountNameWithClanTag));
                await Assert.That(broadcast).IsNull();
                await Assert.That(invite).IsNull();
                await Assert.That(group.Members.Count).IsEqualTo(1);
                await Assert.That(inviteeSession.PendingMatchLockReturn).IsNull();
            }
        }

        finally
        {
            DisbandGroups(inviterAccountID);
        }
    }

    [Test]
    public async Task Inviting_A_Locked_Player_Whose_Match_Server_Does_Not_Answer_Delivers_The_Invite_And_Keeps_The_Lock()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        // The Match Server's Game Socket Does Not Answer The Reconnect Query, As When A Few Datagrams Are Lost, While The Match Server Still Reports Its Status
        gameSocket.Answers = false;

        (_, int inviterAccountID, string inviterAccountName, string inviterCookie) = await ChatTestData.SeedPlayer(host);
        (int inviteeUserID, int inviteeAccountID, string inviteeAccountName, string inviteeCookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, inviteeUserID, inviteeAccountID, inviteeAccountName, gameSocket);

        using TcpClient inviterClient = await host.ConnectAndAuthenticateClientAsync(inviterAccountID, inviterAccountName, inviterCookie, RemoteIP);
        using TcpClient inviteeClient = await host.ConnectAndAuthenticateClientAsync(inviteeAccountID, inviteeAccountName, inviteeCookie, RemoteIP);

        NetworkStream inviterStream = inviterClient.GetStream();

        try
        {
            await CreateGroup(inviterStream, inviterAccountID);

            // An Invite Is Not The Invitee's Own Attempt To Play, So An Unconfirmed Slot Neither Holds The Invite Back Nor Lifts The Invitee's Lock, And The Invitee's Own Group Join Decides
            await ChatTestProtocol.WriteFrame(inviterStream, ChatTestProtocol.BuildGroupInvite(inviteeAccountName));

            ChatBuffer? invite = await ChatTestProtocol.ReadUntilCommand(inviteeClient.GetStream(), ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE, FrameTimeout);

            (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(inviterStream, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(invite).IsNotNull();
                await Assert.That(notice).IsNull();
                await Assert.That(await host.Services.GetRequiredService<IDatabase>().GetMatchLock(inviteeUserID)).IsNotNull();
                await Assert.That(Context.ClientChatSessions[inviteeAccountName].PendingMatchLockReturn).IsNull();
            }
        }

        finally
        {
            DisbandGroups(inviterAccountID, inviteeAccountID);
        }
    }

    [Test]
    public async Task A_Locked_Player_Inviting_Someone_Is_Sent_Back_And_The_Invite_Is_Not_Delivered()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int inviterUserID, int inviterAccountID, string inviterAccountName, string inviterCookie) = await ChatTestData.SeedPlayer(host);
        (_, int receiverAccountID, string receiverAccountName, string receiverCookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, inviterUserID, inviterAccountID, inviterAccountName, gameSocket);

        using TcpClient inviterClient = await host.ConnectAndAuthenticateClientAsync(inviterAccountID, inviterAccountName, inviterCookie, RemoteIP);
        using TcpClient receiverClient = await host.ConnectAndAuthenticateClientAsync(receiverAccountID, receiverAccountName, receiverCookie, RemoteIP);

        NetworkStream inviterStream = inviterClient.GetStream();

        try
        {
            await CreateGroup(inviterStream, inviterAccountID);

            await ChatTestProtocol.WriteFrame(inviterStream, ChatTestProtocol.BuildGroupInvite(receiverAccountName));

            await AssertSentBack(inviterStream, lockedMatch);

            await Assert.That(await ChatTestProtocol.ReadUntilCommand(receiverClient.GetStream(), ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE, QuietPeriod)).IsNull();
        }

        finally
        {
            DisbandGroups(inviterAccountID, receiverAccountID);
        }
    }

    [Test]
    public async Task Another_Account_Of_A_Locked_User_Inviting_Someone_Is_Told_Why()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int inviterUserID, int inviterAccountID, string inviterAccountName, string inviterCookie) = await ChatTestData.SeedPlayer(host);
        (_, int receiverAccountID, string receiverAccountName, string receiverCookie) = await ChatTestData.SeedPlayer(host);

        // Another Account Of The Same User Holds The Slot, So This Account Has No Match To Return To
        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, inviterUserID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

        using TcpClient inviterClient = await host.ConnectAndAuthenticateClientAsync(inviterAccountID, inviterAccountName, inviterCookie, RemoteIP);
        using TcpClient receiverClient = await host.ConnectAndAuthenticateClientAsync(receiverAccountID, receiverAccountName, receiverCookie, RemoteIP);

        NetworkStream inviterStream = inviterClient.GetStream();

        try
        {
            await CreateGroup(inviterStream, inviterAccountID);

            await ChatTestProtocol.WriteFrame(inviterStream, ChatTestProtocol.BuildGroupInvite(receiverAccountName));

            List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(inviterStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            ChatBuffer? invite = await ChatTestProtocol.ReadUntilCommand(receiverClient.GetStream(), ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(MatchLockNotices(frames)).IsEquivalentTo([MatchLockService.OtherAccountMessage(lockedMatch.Lock.TaggedAccountName)]);
                await Assert.That(frames.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN)).IsFalse();
                await Assert.That(invite).IsNull();
            }
        }

        finally
        {
            DisbandGroups(inviterAccountID, receiverAccountID);
        }
    }

    [Test]
    public async Task Inviting_A_Player_Who_Is_Not_Locked_Still_Delivers_The_Invite()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (_, int inviterAccountID, string inviterAccountName, string inviterCookie) = await ChatTestData.SeedPlayer(host);
        (_, int receiverAccountID, string receiverAccountName, string receiverCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient inviterClient = await host.ConnectAndAuthenticateClientAsync(inviterAccountID, inviterAccountName, inviterCookie, RemoteIP);
        using TcpClient receiverClient = await host.ConnectAndAuthenticateClientAsync(receiverAccountID, receiverAccountName, receiverCookie, RemoteIP);

        NetworkStream inviterStream = inviterClient.GetStream();

        try
        {
            await CreateGroup(inviterStream, inviterAccountID);

            await ChatTestProtocol.WriteFrame(inviterStream, ChatTestProtocol.BuildGroupInvite(receiverAccountName));

            ChatBuffer? invite = await ChatTestProtocol.ReadUntilCommand(receiverClient.GetStream(), ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE, FrameTimeout);

            (string SenderName, string Message)? notice = await ChatTestProtocol.ReadNotice(inviterStream, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(invite).IsNotNull();
                await Assert.That(notice).IsNull();
            }
        }

        finally
        {
            DisbandGroups(inviterAccountID, receiverAccountID);
        }
    }

    [Test]
    public async Task Queueing_From_A_Solo_Group_While_Sent_Back_At_Once_Leaves_No_Group_And_The_Next_Group_Is_Created_Without_A_Left_Update()
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

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            await AssertSentBack(stream, lockedMatch);

            await AcknowledgeSendBack(stream);

            await AssertNoGroupLeftBehind(stream, accountID);
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Joining_A_Group_From_A_Solo_Group_While_Sent_Back_At_Once_Leaves_No_Group_And_The_Joined_Group_Unchanged()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (_, int leaderAccountID, string leaderAccountName, string leaderCookie) = await ChatTestData.SeedPlayer(host);
        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient leaderClient = await host.ConnectAndAuthenticateClientAsync(leaderAccountID, leaderAccountName, leaderCookie, RemoteIP);
        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup leaderGroup = await CreateGroup(leaderClient.GetStream(), leaderAccountID);

            await CreateGroup(stream, accountID);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoin(leaderAccountName));

            await AssertSentBack(stream, lockedMatch);

            await AcknowledgeSendBack(stream);

            bool groupLeft = await ChatTestProtocol.WaitUntil(() => MatchmakingService.GetMatchmakingGroup(accountID) is null, FrameTimeout);

            using (Assert.Multiple())
            {
                await Assert.That(groupLeft).IsTrue();
                await Assert.That(leaderGroup.Members.Count).IsEqualTo(1);
            }
        }

        finally
        {
            DisbandGroups(leaderAccountID, accountID);
        }
    }

    [Test]
    public async Task Inviting_From_A_Solo_Group_While_Sent_Back_At_Once_Leaves_No_Group_And_The_Next_Group_Is_Created_Without_A_Left_Update()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);
        (_, int receiverAccountID, string receiverAccountName, string receiverCookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);
        using TcpClient receiverClient = await host.ConnectAndAuthenticateClientAsync(receiverAccountID, receiverAccountName, receiverCookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            // The Game Client Creates A Solo Group Before It Sends An Invite
            await CreateGroup(stream, accountID);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupInvite(receiverAccountName));

            await AssertSentBack(stream, lockedMatch);

            await AcknowledgeSendBack(stream);

            await AssertNoGroupLeftBehind(stream, accountID);
        }

        finally
        {
            DisbandGroups(accountID, receiverAccountID);
        }
    }

    [Test]
    public async Task An_Acknowledged_Reminder_After_The_Game_Client_Created_A_New_Solo_Group_Leaves_No_Group_And_The_Next_Group_Is_Created_Without_A_Left_Update()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);
        (_, int receiverAccountID, string receiverAccountName, string receiverCookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);
        using TcpClient receiverClient = await host.ConnectAndAuthenticateClientAsync(receiverAccountID, receiverAccountName, receiverCookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            await CreateGroup(stream, accountID);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupInvite(receiverAccountName));

            await AssertSentBack(stream, lockedMatch);

            await AcknowledgeSendBack(stream);

            // The Game Client's Connection To The Match Server Fails, So It Returns To The Menu And Creates A Solo Group Again Before The Reminder
            await CreateGroup(stream, accountID);

            (byte ArrangedMatchType, int MatchID, string Address, short Port, int RandomInteger)? reminder = await ChatTestProtocol.ReadAutoMatchConnect(stream, MatchLockService.ReturnResendInterval + QuietPeriod);

            await Assert.That(reminder?.RandomInteger).IsEqualTo(-1);

            await AcknowledgeSendBack(stream, reminder: true);

            await AssertNoGroupLeftBehind(stream, accountID);
        }

        finally
        {
            DisbandGroups(accountID, receiverAccountID);
        }
    }

    [Test]
    public async Task A_Relayed_Refusal_With_A_Solo_Group_Leaves_No_Group_And_The_Next_Group_Is_Created_Without_A_Left_Update()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            await CreateGroup(stream, accountID);

            // The Master Server Refuses The Player's Server List, Or Its Client Connection On Another Match Server, And Publishes The Refusal
            await host.Services.GetRequiredService<IDatabase>().PublishMatchLockRefusal(new MatchLockRefusal { UserID = userID, AccountName = accountName });

            await AssertSentBack(stream, lockedMatch);

            await AcknowledgeSendBack(stream);

            await AssertNoGroupLeftBehind(stream, accountID);
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task A_Send_Back_Which_The_Game_Client_Does_Not_Acknowledge_Keeps_Its_Solo_Group()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            // The Client Connection Is Refused On Another Match Server, While The Game Client Is Still Connecting There
            await host.Services.GetRequiredService<IDatabase>().PublishMatchLockRefusal(new MatchLockRefusal { UserID = userID, AccountName = accountName });

            await AssertSentBack(stream, lockedMatch);

            // The Game Client Ignores The Send-Back While It Is Still Connecting, So It Neither Acknowledges It Nor Deletes Its Group
            using (Assert.Multiple())
            {
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsEqualTo(group);
                await Assert.That(group.Members.Select(member => member.Account.ID)).IsEquivalentTo([accountID]);
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task A_Send_Back_After_Waiting_For_The_Other_Machine_Leaves_No_Group_And_The_Next_Group_Is_Created_Without_A_Left_Update()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(stream, accountID);

            // The Slot Is Still Connected From Another Game, So The Queue Is Refused With The Failure Reply, And The Return Waits For That Connection To Leave
            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, accountID, accountName, gameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (_, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure) = await ReadThroughMatchmakingFailure(stream);

            // The Game Client Leaves Its Group On The Failure Reply, And Creates A Solo Group Again Once It Has Left Its Last One
            await CreateGroup(stream, accountID);

            // The Game Is Closed On The Other Machine, Which Leaves The Slot Disconnected
            gameSocket.SlotTimeRemaining = SlotTimeRemaining;

            List<(ushort Command, ChatBuffer Frame)> frames = await ReadThroughSendBack(stream);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(SendsBack(frames, lockedMatch)).IsTrue();
                await Assert.That(frames.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
            }

            await AcknowledgeSendBack(stream);

            await AssertNoGroupLeftBehind(stream, accountID);
        }

        finally
        {
            DisbandGroups(accountID);
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

            (List<(ushort Command, ChatBuffer Frame)> earlier, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure) = await ReadThroughMatchmakingFailure(stream);

            // The Frames On Both Sides Of The Failure Reply Are Read, Because The Notice Must Come After The Reply, While A Group Update About The Player's Removal Would Come Before It
            // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(failure?.Seconds ?? 0).IsLessThanOrEqualTo((uint) MatchLock.Lifetime.TotalSeconds);
                await Assert.That(failure?.Seconds ?? 0).IsGreaterThan((uint) (MatchLock.Lifetime - TimeSpan.FromMinutes(1)).TotalSeconds);
                await Assert.That(MatchLockNotices(later)).IsEquivalentTo([MatchLockService.OtherAccountMessage(lockedMatch.Lock.TaggedAccountName)]);
                await Assert.That(earlier.Concat(later).Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
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
    public async Task Queueing_Again_On_Another_Account_Within_The_Notice_Repeat_Interval_Still_Shows_The_Temporary_Ban()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket gameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int userID, int accountID, string accountName, string cookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient client = await host.ConnectAndAuthenticateClientAsync(accountID, accountName, cookie, RemoteIP);

        NetworkStream stream = client.GetStream();

        try
        {
            MatchmakingGroup firstGroup = await CreateGroup(stream, accountID);

            await using LockedMatch lockedMatch = await ChatTestData.SeedLockedMatch(host, userID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, gameSocket);

            MakeReadyToQueue(firstGroup);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? firstFailure = await ChatTestProtocol.ReadMatchmakingFailure(stream, FrameTimeout);
            (string SenderName, string Message)? firstNotice = await ChatTestProtocol.ReadNotice(stream, FrameTimeout);

            // The Game Client Creates A Solo Group Again Once It Has Left Its Last One, From Which Its Player Queues Again Well Within The Notice Repeat Interval
            MatchmakingGroup secondGroup = await CreateGroup(stream, accountID);

            MakeReadyToQueue(secondGroup);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (List<(ushort Command, ChatBuffer Frame)> earlier, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? secondFailure) = await ReadThroughMatchmakingFailure(stream);

            // The Notice Repeat Interval Holds Back Only The Notice, Because Without The Failure Reply The Matchmaking Screen Would Stay Locked
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(firstFailure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(firstNotice?.Message).IsEqualTo(MatchLockService.OtherAccountMessage(lockedMatch.Lock.TaggedAccountName));
                await Assert.That(secondFailure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(MatchLockNotices(later).Count).IsEqualTo(0);
                await Assert.That(earlier.Concat(later).Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(secondGroup.QueueStartTime).IsNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
            }
        }

        finally
        {
            DisbandGroups(accountID);
        }
    }

    [Test]
    public async Task Queueing_Again_While_The_Return_Continues_Shows_The_Temporary_Ban_And_Leaves_The_Group()
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

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            await AssertSentBack(stream, lockedMatch);

            await AcknowledgeSendBack(stream);

            // The Game Client Deleted Its Solo Group When It Was Sent Back, So The Player Creates A New One Before Queueing Again
            group = await CreateGroup(stream, accountID);

            MakeReadyToQueue(group);

            // The Player Queues Again Before Reporting Joining The Locked Match Server, So The Return Continues Without Pulling The Player Back At Once, And Only The Failure Reply Releases The Matchmaking Screen
            await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupJoinQueue());

            (List<(ushort Command, ChatBuffer Frame)> earlier, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure) = await ReadThroughMatchmakingFailure(stream);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(earlier.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(group.QueueStartTime).IsNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNull();
                await Assert.That(Context.ClientChatSessions[accountName].PendingMatchLockReturn?.MatchID).IsEqualTo(lockedMatch.Lock.MatchID);
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

            (List<(ushort Command, ChatBuffer Frame)> earlier, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? failure) = await ReadThroughMatchmakingFailure(stream);

            // The Frames On Both Sides Of The Failure Reply Are Read, Because The Notice Must Come After The Reply, While A Group Update About The Player's Removal Would Come Before It
            // The Game Client Has Left Its Group On The Failure Reply, So A Group Update About Its Removal Would Make It Leave Again
            List<(ushort Command, ChatBuffer Frame)> later = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(failure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(MatchLockNotices(later)).IsEquivalentTo([MatchLockService.ConnectedElsewhereMessage]);
                await Assert.That(earlier.Concat(later).Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
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

        // The Notice Names The Locked Member With Its Clan Tag, Which The Chat Session Loads At The Handshake
        string memberAccountNameWithClanTag = await ChatTestData.SeedClanMember(host.Services, memberAccountID);

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

            string groupMemberLockedMessage = MatchLockService.GroupMemberLockedMessage(memberAccountNameWithClanTag);

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
                await Assert.That(group.Members.Count).IsEqualTo(2);
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
    public async Task Queueing_A_Party_With_A_Slot_Account_And_A_Refused_Member_Keeps_The_Slot_Account_In_The_Party_And_Unreadies_It()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket slotGameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);
        await using FakeMatchServerGameSocket otherGameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int slotUserID, int slotAccountID, string slotAccountName, string slotCookie) = await ChatTestData.SeedPlayer(host);
        (int otherUserID, int otherAccountID, string otherAccountName, string otherCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient slotClient = await host.ConnectAndAuthenticateClientAsync(slotAccountID, slotAccountName, slotCookie, RemoteIP);
        using TcpClient otherClient = await host.ConnectAndAuthenticateClientAsync(otherAccountID, otherAccountName, otherCookie, RemoteIP);

        NetworkStream slotStream = slotClient.GetStream();
        NetworkStream otherStream = otherClient.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(slotStream, slotAccountID);

            await JoinGroup(otherStream, slotAccountName, slotStream);

            // The Second Member Is Another Account Of A Locked User, So It Is Refused And Removed While The First Is Sent Back At Once
            await using LockedMatch slotLockedMatch = await ChatTestData.SeedLockedMatch(host, slotUserID, slotAccountID, slotAccountName, slotGameSocket);
            await using LockedMatch otherLockedMatch = await ChatTestData.SeedLockedMatch(host, otherUserID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, otherGameSocket);

            // The Refused Member's Match Server Answers Only A Retry, So That The Refused Member Is Still In The Party When The Slot Account Is Sent Back
            otherGameSocket.IgnoredRequestsRemaining = 1;

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(slotStream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> slotFrames = await ReadThroughSendBack(slotStream);

            // The Game Client Acts On The Send-Back While It Still Sees The Refused Member, So It Keeps Its Party, Which Has Only The Slot Account Left By The Time The Acknowledgement Arrives
            await AcknowledgeSendBack(slotStream);

            // Keeping The Party Changes Nothing Which The Test Could Wait For, So The Acknowledgement Is Given Time To Be Handled
            await Task.Delay(QuietPeriod);

            using (Assert.Multiple())
            {
                await Assert.That(SendsBack(slotFrames, slotLockedMatch)).IsTrue();
                await Assert.That(slotFrames.TakeWhile(frame => frame.Command is not ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT).Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(slotAccountID)).IsNotNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(otherAccountID)).IsNull();
                await Assert.That(group.Members.Count).IsEqualTo(1);
                await Assert.That(group.Members.Single().Account.ID).IsEqualTo(slotAccountID);
                await Assert.That(group.Leader.IsReady).IsFalse();
                await Assert.That(group.QueueStartTime).IsNull();
            }
        }

        finally
        {
            DisbandGroups(slotAccountID, otherAccountID);
        }
    }

    [Test]
    public async Task Queueing_A_Party_Whose_Refused_Member_Leaves_Before_The_Slot_Account_Is_Sent_Back_Leaves_The_Slot_Account_No_Group()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket slotGameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);
        await using FakeMatchServerGameSocket otherGameSocket = FakeMatchServerGameSocket.Start(SlotTimeRemaining);

        (int slotUserID, int slotAccountID, string slotAccountName, string slotCookie) = await ChatTestData.SeedPlayer(host);
        (int otherUserID, int otherAccountID, string otherAccountName, string otherCookie) = await ChatTestData.SeedPlayer(host);

        using TcpClient slotClient = await host.ConnectAndAuthenticateClientAsync(slotAccountID, slotAccountName, slotCookie, RemoteIP);
        using TcpClient otherClient = await host.ConnectAndAuthenticateClientAsync(otherAccountID, otherAccountName, otherCookie, RemoteIP);

        NetworkStream slotStream = slotClient.GetStream();
        NetworkStream otherStream = otherClient.GetStream();

        try
        {
            MatchmakingGroup group = await CreateGroup(slotStream, slotAccountID);

            await JoinGroup(otherStream, slotAccountName, slotStream);

            await using LockedMatch slotLockedMatch = await ChatTestData.SeedLockedMatch(host, slotUserID, slotAccountID, slotAccountName, slotGameSocket);
            await using LockedMatch otherLockedMatch = await ChatTestData.SeedLockedMatch(host, otherUserID, Random.Shared.Next(1_000_000, int.MaxValue), SlotAccountName, otherGameSocket);

            // The Slot Account's Match Server Answers Only A Retry, So That The Refused Member Has Left The Party, And The Slot Account Has Been Told So, When The Slot Account Is Sent Back
            slotGameSocket.IgnoredRequestsRemaining = 1;

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(slotStream, ChatTestProtocol.BuildGroupJoinQueue());

            List<(ushort Command, ChatBuffer Frame)> slotFrames = await ReadThroughSendBack(slotStream);

            List<(ushort Command, ChatBuffer Frame)> slotFramesBeforeSendBack = [.. slotFrames.TakeWhile(frame => frame.Command is not ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT)];

            // The Game Client Deletes Its Solo Group When It Acts On The Send-Back, And Any Group Update After It Would Put The Game Client Back Into That Group
            List<(ushort Command, ChatBuffer Frame)> slotFramesFromSendBack = [.. slotFrames.SkipWhile(frame => frame.Command is not ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT)];

            using (Assert.Multiple())
            {
                await Assert.That(SendsBack(slotFrames, slotLockedMatch)).IsTrue();
                await Assert.That(GroupUpdateAccountIDs(slotFramesBeforeSendBack, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP)).IsEquivalentTo([otherAccountID]);
                await Assert.That(slotFramesFromSendBack.Any(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(otherAccountID)).IsNull();
                await Assert.That(group.QueueStartTime).IsNull();
            }

            await AcknowledgeSendBack(slotStream);

            await AssertNoGroupLeftBehind(slotStream, slotAccountID);
        }

        finally
        {
            DisbandGroups(slotAccountID, otherAccountID);
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
    public async Task Queueing_A_Party_With_Two_Members_Connected_Elsewhere_Removes_Both_And_Sends_Neither_A_Group_Update_About_Its_Own_Removal()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);
        await using FakeMatchServerGameSocket firstGameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);
        await using FakeMatchServerGameSocket secondGameSocket = FakeMatchServerGameSocket.Start(FakeMatchServerGameSocket.ConnectedSlotTimeRemaining);

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

            // Neither Slot Can Be Reclaimed While It Is Still Connected From Another Game, So Neither Member Is Sent Back At Once, And Both Leave The Party Concurrently
            await using LockedMatch firstLockedMatch = await ChatTestData.SeedLockedMatch(host, firstMemberUserID, firstMemberAccountID, firstMemberAccountName, firstGameSocket);
            await using LockedMatch secondLockedMatch = await ChatTestData.SeedLockedMatch(host, secondMemberUserID, secondMemberAccountID, secondMemberAccountName, secondGameSocket);

            MakeReadyToQueue(group);

            await ChatTestProtocol.WriteFrame(leaderStream, ChatTestProtocol.BuildGroupJoinQueue());

            // A Member May Read The Other Member's Removal Before Its Own Failure Reply, While It Is Still In The Party, But Never Its Own Removal, And Nothing About The Party After The Reply
            (List<(ushort Command, ChatBuffer Frame)> firstMemberEarlierFrames, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? firstFailure) = await ReadThroughMatchmakingFailure(firstMemberStream);
            List<(ushort Command, ChatBuffer Frame)> firstMemberFrames = await ChatTestProtocol.ReadCommandsUntil(firstMemberStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            (List<(ushort Command, ChatBuffer Frame)> secondMemberEarlierFrames, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? secondFailure) = await ReadThroughMatchmakingFailure(secondMemberStream);
            List<(ushort Command, ChatBuffer Frame)> secondMemberFrames = await ChatTestProtocol.ReadCommandsUntil(secondMemberStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            List<(ushort Command, ChatBuffer Frame)> leaderFrames = await ChatTestProtocol.ReadCommandsUntil(leaderStream, ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT, QuietPeriod);

            List<string> groupMemberLockedMessages = [MatchLockService.GroupMemberLockedMessage(firstMemberAccountName), MatchLockService.GroupMemberLockedMessage(secondMemberAccountName)];

            using (Assert.Multiple())
            {
                await Assert.That(firstFailure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(GroupUpdateAccountIDs(firstMemberEarlierFrames, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP).Contains(firstMemberAccountID)).IsFalse();
                await Assert.That(firstMemberFrames.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(MatchLockNotices(firstMemberFrames)).IsEquivalentTo([MatchLockService.ConnectedElsewhereMessage, .. groupMemberLockedMessages]);
                await Assert.That(secondFailure?.Reason).IsEqualTo(ChatProtocol.TMMFailedToJoinReason.TMMFTJR_BANNED);
                await Assert.That(GroupUpdateAccountIDs(secondMemberEarlierFrames, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP).Contains(secondMemberAccountID)).IsFalse();
                await Assert.That(secondMemberFrames.Any(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_AUTO_MATCH_CONNECT or ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)).IsFalse();
                await Assert.That(MatchLockNotices(secondMemberFrames)).IsEquivalentTo([MatchLockService.ConnectedElsewhereMessage, .. groupMemberLockedMessages]);
                await Assert.That(GroupUpdateAccountIDs(leaderFrames, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP)).IsEquivalentTo([firstMemberAccountID, secondMemberAccountID]);
                await Assert.That(MatchLockNotices(leaderFrames)).IsEquivalentTo(groupMemberLockedMessages);
                await Assert.That(group.Members.Select(member => member.Account.ID)).IsEquivalentTo([leaderAccountID]);
                await Assert.That(MatchmakingService.GetMatchmakingGroup(firstMemberAccountID)).IsNull();
                await Assert.That(MatchmakingService.GetMatchmakingGroup(secondMemberAccountID)).IsNull();
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
    ///     Reads every frame up to and including the first matchmaking failure reply, and returns them with the reply's reason and seconds, or <see langword="null"/> when no reply arrived in time.
    ///     The frames before the reply are kept, because the player leaves its group before the reply is sent, so that a group update about its removal would arrive before the reply.
    /// </summary>
    private static async Task<(List<(ushort Command, ChatBuffer Frame)> Frames, (ChatProtocol.TMMFailedToJoinReason Reason, uint Seconds)? Failure)> ReadThroughMatchmakingFailure(NetworkStream stream)
    {
        List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN, FrameTimeout);

        return (frames, frames is [.., { Command: ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_FAILED_TO_JOIN } failure] ? ChatTestProtocol.ParseMatchmakingFailure(failure.Frame) : null);
    }

    /// <summary>
    ///     Returns the messages of the match lock's notices among the frames, in the order in which they arrived.
    ///     Reading a notice moves its frame past it, so it is called once for each list of frames.
    /// </summary>
    private static List<string> MatchLockNotices(List<(ushort Command, ChatBuffer Frame)> frames)
        => [.. frames.Where(frame => frame.Command is ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL).Select(frame => ChatTestProtocol.ParseNotice(frame.Frame))
            .Where(notice => notice.SenderName is MatchLockService.MessageSenderName).Select(notice => notice.Message)];

    /// <summary>
    ///     Returns the account IDs which the group updates of the given type among the frames are about, in the order in which they arrived.
    ///     Reading a group update moves its frame past its type and account ID, so it is called once for each list of frames.
    /// </summary>
    private static List<int> GroupUpdateAccountIDs(List<(ushort Command, ChatBuffer Frame)> frames, ChatProtocol.TMMUpdateType updateType)
        => [.. frames.Where(frame => frame.Command is ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)
            .Select(frame => (UpdateType: (ChatProtocol.TMMUpdateType) frame.Frame.ReadInt8(), AccountID: frame.Frame.ReadInt32()))
            .Where(update => update.UpdateType == updateType).Select(update => update.AccountID)];

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
    ///     Acknowledges an "AUTO_MATCH_CONNECT", as the game client does when it acts on one, immediately before it connects to the match and deletes a solo group by itself.
    /// </summary>
    private static async Task AcknowledgeSendBack(NetworkStream stream, bool reminder = false)
        => await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildTrackPlayerAction(reminder ? ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY_REMINDER : ChatProtocol.ActionCampaign.AC_MATCHMAKING_MATCH_READY));

    /// <summary>
    ///     Asserts that the account which acknowledged its send-back leaves its matchmaking group, as its game client does by deleting its solo group, and that the group which its game client creates next is created without a group update about the account leaving another group, which the game client would show as "Group Disbanded".
    /// </summary>
    private static async Task AssertNoGroupLeftBehind(NetworkStream stream, int accountID)
    {
        bool groupLeft = await ChatTestProtocol.WaitUntil(() => MatchmakingService.GetMatchmakingGroup(accountID) is null, FrameTimeout);

        await Assert.That(groupLeft).IsTrue();

        // The Game Client Creates A New Group When It Returns To The Menu
        await ChatTestProtocol.WriteFrame(stream, ChatTestProtocol.BuildGroupCreate(ChatProtocol.TMMType.TMM_TYPE_PVP));

        List<(ushort Command, ChatBuffer Frame)> frames = await ChatTestProtocol.ReadCommandsUntil(stream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE, FrameTimeout);

        using (Assert.Multiple())
        {
            await Assert.That(GroupUpdateAccountIDs(frames, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP)).IsEmpty();
            await Assert.That(MatchmakingService.GetMatchmakingGroup(accountID)).IsNotNull();
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
