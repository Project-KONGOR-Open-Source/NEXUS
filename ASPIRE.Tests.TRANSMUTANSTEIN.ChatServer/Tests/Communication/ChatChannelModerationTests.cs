namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers channel moderation through the moderation command processors (<see cref="SetChannelPassword"/>, <see cref="SilenceChannelMember"/>, and <see cref="KickFromChannel"/>) and the channel operations behind them.
/// </summary>
public sealed class ChatChannelModerationTests
{
    [Test]
    public async Task Setting_The_Password_Of_A_Channel_The_Client_Is_Not_In_Is_Ignored()
    {
        (ChatChannel channel, ClientChatSession _) = CreateChannelWithMember("Non-Member Password Channel", "PasswordMember");

        ClientChatSession nonMemberSession = CreateSession("PasswordNonMember");

        ChatBuffer request = new ();

        request.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SET_PASSWORD);
        request.WriteInt32(channel.ID);
        request.WriteString("Password");

        new SetChannelPassword().Process(nonMemberSession, request);

        await Assert.That(channel.Password).IsNull();
    }

    [Test]
    public async Task Silencing_A_Member_Of_A_Channel_The_Client_Is_Not_In_Is_Ignored()
    {
        (ChatChannel channel, ClientChatSession memberSession) = CreateChannelWithMember("Non-Member Silence Channel", "SilenceMember");

        ClientChatSession nonMemberSession = CreateSession("SilenceNonMember");

        ChatBuffer request = new ();

        request.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER);
        request.WriteInt32(channel.ID);
        request.WriteString(memberSession.Account.Name);
        request.WriteInt32(60_000);

        new SilenceChannelMember().Process(nonMemberSession, request);

        await Assert.That(channel.IsSilenced(memberSession)).IsFalse();
    }

    [Test]
    public async Task Kicking_A_Member_From_A_Channel_The_Client_Is_Not_In_Is_Ignored()
    {
        (ChatChannel channel, ClientChatSession memberSession) = CreateChannelWithMember("Non-Member Kick Channel", "KickMember");

        ClientChatSession nonMemberSession = CreateSession("KickNonMember");

        ChatBuffer request = new ();

        request.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);
        request.WriteInt32(channel.ID);
        request.WriteInt32(memberSession.Account.ID);

        new KickFromChannel().Process(nonMemberSession, request);

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(memberSession.Account.Name)).IsTrue();
            await Assert.That(memberSession.CurrentChannels).Contains(channel.ID);
        }
    }

    [Test]
    public async Task A_Silence_Survives_The_Silenced_Member_Leaving_And_Rejoining_The_Channel()
    {
        Account moderatorAccount = CreateAccount("SilencingModerator");

        moderatorAccount.Type = AccountType.Staff;

        await using RunningClientSession moderator = RunningClientSession.Start(moderatorAccount);
        await using RunningClientSession target = RunningClientSession.Start(CreateAccount("SilenceEvader"));

        ChatChannel channel = ChatChannel.GetOrCreate(moderator.Session, "Silence Evasion Channel");

        channel.Join(moderator.Session);
        channel.Join(target.Session);

        channel.Silence(moderator.Session, target.Session, 60_000);

        channel.Leave(target.Session);
        channel.Join(target.Session);

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(target.Session.Account.Name)).IsTrue();
            await Assert.That(channel.IsSilenced(target.Session)).IsTrue();
        }
    }

    [Test]
    public async Task Silencing_A_Player_Who_Is_Not_In_The_Channel_Silences_Them_Once_They_Join_And_Notifies_The_Requester()
    {
        Account moderatorAccount = CreateAccount("AbsentSilenceModerator");

        moderatorAccount.Type = AccountType.Staff;

        await using RunningClientSession moderator = RunningClientSession.Start(moderatorAccount);
        await using RunningClientSession target = RunningClientSession.Start(CreateAccount("AbsentSilenceTarget"));

        ChatChannel channel = ChatChannel.GetOrCreate(moderator.Session, "Absent Silence Channel");

        channel.Join(moderator.Session);

        channel.Silence(moderator.Session, target.Session, 60_000);

        channel.Join(target.Session);

        using (Assert.Multiple())
        {
            await Assert.That(channel.IsSilenced(target.Session)).IsTrue();
            await Assert.That(await moderator.ReadCommand()).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);
            await Assert.That((await moderator.ReadNotice()).Message).IsEqualTo("That Player Will Be Silenced Upon Joining This Channel");
        }
    }

    [Test]
    public async Task Silencing_A_Player_Who_Is_Not_Online_Notifies_The_Requester()
    {
        Account moderatorAccount = CreateAccount("OfflineSilenceModerator");

        moderatorAccount.Type = AccountType.Staff;

        await using RunningClientSession moderator = RunningClientSession.Start(moderatorAccount);

        ChatChannel channel = ChatChannel.GetOrCreate(moderator.Session, "Offline Silence Channel");

        channel.Join(moderator.Session);

        ChatBuffer request = new ();

        request.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER);
        request.WriteInt32(channel.ID);
        request.WriteString("NobodyOnline");
        request.WriteInt32(60_000);

        new SilenceChannelMember().Process(moderator.Session, request);

        using (Assert.Multiple())
        {
            await Assert.That(await moderator.ReadCommand()).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);
            await Assert.That((await moderator.ReadNotice()).Message).IsEqualTo("No Player Named NobodyOnline Is Online");
        }
    }

    private static (ChatChannel Channel, ClientChatSession MemberSession) CreateChannelWithMember(string channelName, string memberAccountName)
    {
        ClientChatSession memberSession = CreateSession(memberAccountName);

        ChatChannel channel = ChatChannel.GetOrCreate(memberSession, channelName);

        channel.Members.TryAdd(memberSession.Account.Name, new ChatChannelMember(memberSession, channel));
        memberSession.CurrentChannels.Add(channel.ID);

        return (channel, memberSession);
    }

    /// <summary>
    ///     Constructs a stand-in <see cref="ClientChatSession"/> that bypasses the production constructor, populating only the account and channel-tracking state that the moderation requests read on the paths that do not send to the client.
    /// </summary>
    private static ClientChatSession CreateSession(string accountName)
    {
        ClientChatSession session = (ClientChatSession) RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSession));

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
