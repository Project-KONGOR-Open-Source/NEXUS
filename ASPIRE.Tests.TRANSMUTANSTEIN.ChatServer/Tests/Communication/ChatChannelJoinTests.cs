namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers the join validation on <see cref="ChatChannel.Join"/>: a channel flagged <see cref="ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_UNJOINABLE"/> and a full channel both reject a manual join and notify the requesting client, while the internal member additions used to populate matchmaking group and match channels are still accepted.
///     It also covers join requests by name, which <see cref="ChatChannel.TryGetOrCreateForJoinRequest"/> resolves so that reserved channels are only joined by the clients entitled to them and the general channel names are routed through the load balancing.
///     The notifying paths run the session over an in-memory connection so the test reads the actual notice frame back off the wire, confirming it is genuinely sent rather than discarded.
/// </summary>
public sealed class ChatChannelJoinTests
{
    [Test]
    public async Task Manual_Join_Of_An_Unjoinable_Channel_Is_Rejected_And_Notifies_The_Client()
    {
        ChatChannel channel = ChatChannel.GetOrCreateGroupChannel(900_001);

        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("UnjoinableJoiner"));

        channel.Join(client.Session);

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(client.Session.Account.Name)).IsFalse();
            await Assert.That(client.Session.CurrentChannels.Count).IsEqualTo(0);
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("This Channel Cannot Be Joined");
        }
    }

    [Test]
    public async Task An_Unjoinable_Channel_Still_Accepts_Internal_Member_Additions()
    {
        ChatChannel channel = ChatChannel.GetOrCreateGroupChannel(900_002);

        ClientChatSession session = CreateSession("InternalJoiner");

        ChatChannelMember member = new (session, channel);

        bool added = channel.Members.TryAdd(session.Account.Name, member);

        await Assert.That(added).IsTrue();
        await Assert.That(channel.Members.ContainsKey(session.Account.Name)).IsTrue();
    }

    [Test]
    public async Task Manual_Join_Of_A_Full_Channel_Is_Rejected_And_Notifies_The_Client()
    {
        ChatChannel channel = ChatChannel.GetOrCreate(CreateSession("FullChannelCreator"), "Full Channel Test");

        // Fill The Channel To Its Member Cap Via The Internal Path, Which Bypasses Join
        for (int memberIndex = 0; memberIndex < ChatProtocol.MAX_USERS_PER_CHANNEL; memberIndex++)
        {
            ClientChatSession existingSession = CreateSession($"FullChannelMember{memberIndex}");

            channel.Members.TryAdd(existingSession.Account.Name, new ChatChannelMember(existingSession, channel));
        }

        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("LateJoiner"));

        channel.Join(client.Session);

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(client.Session.Account.Name)).IsFalse();
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("This Channel Is Full");
        }
    }

    [Test]
    public async Task A_Clan_Channel_Is_Never_Full()
    {
        ClientChatSession creatorSession = CreateSession("UncappedClanCreator");

        creatorSession.Account.Clan = new Clan { Name = "Uncapped Clan", Tag = "UNCP" };

        ChatChannel channel = ChatChannel.GetOrCreate(creatorSession, creatorSession.Account.Clan.GetChatChannelName());

        // Fill The Channel Beyond The Member Cap Via The Internal Path, Which Bypasses Join
        for (int memberIndex = 0; memberIndex <= ChatProtocol.MAX_USERS_PER_CHANNEL; memberIndex++)
        {
            ClientChatSession existingSession = CreateSession($"UncappedClanMember{memberIndex}");

            channel.Members.TryAdd(existingSession.Account.Name, new ChatChannelMember(existingSession, channel));
        }

        using (Assert.Multiple())
        {
            await Assert.That(channel.IsClanChannel).IsTrue();
            await Assert.That(channel.IsFull).IsFalse();
        }
    }

    [Test]
    [Arguments("TERMINAL")]
    [Arguments("VIP")]
    public async Task Joining_A_Role_Channel_Without_The_Role_Is_Rejected_And_Notifies_The_Client(string channelName)
    {
        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("RoleOutsider"));

        new JoinChannel().Process(client.Session, ChatTestProtocol.BuildJoinChannel(channelName));

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("This Channel Cannot Be Joined");
            await Assert.That(client.Session.CurrentChannels).IsEmpty();
        }
    }

    [Test]
    public async Task A_Join_Request_For_A_Role_Channel_With_The_Role_Resolves_To_The_Channel_Under_Its_Canonical_Name()
    {
        ClientChatSession session = CreateSession("RoleInsider");

        session.Account.Type = AccountType.VIP;

        bool resolved = ChatChannel.TryGetOrCreateForJoinRequest(session, "vip", out ChatChannel? channel);

        using (Assert.Multiple())
        {
            await Assert.That(resolved).IsTrue();
            await Assert.That(channel?.Name).IsEqualTo("VIP");
            await Assert.That(Context.ChatChannels.ContainsKey("vip")).IsFalse();
        }
    }

    [Test]
    public async Task A_Join_Request_For_The_Hosts_Channel_Resolves_For_An_Account_Whose_User_Owns_A_Host_Account()
    {
        ClientChatSession session = CreateSession("HostingPlayer");

        session.Account.User.Accounts = [session.Account, new Account { Name = "HostingPlayerHost", User = session.Account.User, IsMain = false, Type = AccountType.ServerHost }];

        bool resolved = ChatChannel.TryGetOrCreateForJoinRequest(session, "HOSTS", out ChatChannel? channel);

        using (Assert.Multiple())
        {
            await Assert.That(resolved).IsTrue();
            await Assert.That(channel?.Name).IsEqualTo("HOSTS");
        }
    }

    [Test]
    public async Task Joining_The_Channel_Of_Another_Clan_Is_Rejected_Without_Creating_It()
    {
        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("ClanOutsider"));

        new JoinChannel().Process(client.Session, ChatTestProtocol.BuildJoinChannel("Clan Someone Else"));

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("Only Clan Members Can Join This Channel");
            await Assert.That(Context.ChatChannels.ContainsKey("Clan Someone Else")).IsFalse();
        }
    }

    [Test]
    [Arguments("Match 424242")]
    [Arguments("TMM Group 424242")]
    public async Task Joining_A_Match_Or_Matchmaking_Group_Channel_By_Name_Is_Rejected_Without_Creating_It(string channelName)
    {
        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("SystemChannelOutsider"));

        new JoinChannel().Process(client.Session, ChatTestProtocol.BuildJoinChannel(channelName));

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("This Channel Cannot Be Joined");
            await Assert.That(Context.ChatChannels.ContainsKey(channelName)).IsFalse();
        }
    }

    [Test]
    public async Task A_Join_Request_For_A_Numbered_General_Channel_Resolves_Through_The_Load_Balancing()
    {
        string channelName = $"{ChatProtocol.CHAT_CHANNEL_BASE_NAME} 7";

        bool resolved = ChatChannel.TryGetOrCreateForJoinRequest(CreateSession("GeneralJoiner"), channelName, out ChatChannel? channel);

        using (Assert.Multiple())
        {
            await Assert.That(resolved).IsTrue();
            await Assert.That(channel?.IsGeneralChannel).IsTrue();
            await Assert.That(Context.ChatChannels.ContainsKey(channelName)).IsFalse();
        }
    }

    /// <summary>
    ///     Constructs a stand-in <see cref="ClientChatSession"/> that bypasses the production constructor, populating only the account and channel-tracking state that <see cref="ChatChannel.Join"/> reads on the paths that do not send to the client.
    /// </summary>
    private static ClientChatSession CreateSession(string accountName)
    {
        ClientChatSession session = (ClientChatSession)RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSession));

        session.Account = CreateAccount(accountName);
        session.CurrentChannels = [];

        return session;
    }

    private static Account CreateAccount(string accountName)
    {
        Role role = new () { Name = "Player" };

        User user = new ()
        {
            EmailAddress    = $"{accountName}@test.local",
            Role            = role,
            SRPPasswordSalt = string.Empty,
            SRPPasswordHash = string.Empty
        };

        return new Account
        {
            ID     = Math.Abs(Guid.NewGuid().GetHashCode()),
            Name   = accountName,
            User   = user,
            IsMain = true,
            Type   = AccountType.Normal
        };
    }
}
