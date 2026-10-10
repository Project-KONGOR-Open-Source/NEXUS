namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers the channel moderation command processors (<see cref="SetChannelPassword"/>, <see cref="SilenceChannelMember"/>, and <see cref="KickFromChannel"/>) when the requesting client is not a member of the target channel.
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
