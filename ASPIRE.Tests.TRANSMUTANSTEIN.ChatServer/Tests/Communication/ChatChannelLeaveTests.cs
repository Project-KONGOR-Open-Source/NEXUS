namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers the <see cref="LeaveChannel"/> command processor, which the game client invokes without checking whether it is in the channel, such as for every saved channel when it clears its auto-connect list.
/// </summary>
public sealed class ChatChannelLeaveTests
{
    [Test]
    public async Task Leaving_A_Channel_That_Does_Not_Exist_Is_Ignored()
    {
        ClientChatSession session = CreateSession("AbsentLeaver");

        new LeaveChannel().Process(session, BuildLeaveRequest("Nonexistent Leave Channel"));

        await Assert.That(session.CurrentChannels).IsEmpty();
    }

    [Test]
    public async Task Leaving_A_Channel_The_Client_Is_Not_In_Is_Ignored_And_Leaves_The_Channel_Untouched()
    {
        ClientChatSession memberSession = CreateSession("RemainingMember");

        ChatChannel channel = ChatChannel.GetOrCreate(memberSession, "Non-Member Leave Channel");

        channel.Members.TryAdd(memberSession.Account.Name, new ChatChannelMember(memberSession, channel));
        memberSession.CurrentChannels.Add(channel.ID);

        ClientChatSession nonMemberSession = CreateSession("NonMemberLeaver");

        new LeaveChannel().Process(nonMemberSession, BuildLeaveRequest(channel.Name));

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(memberSession.Account.Name)).IsTrue();
            await Assert.That(memberSession.CurrentChannels).Contains(channel.ID);
            await Assert.That(nonMemberSession.CurrentChannels).IsEmpty();
        }
    }

    [Test]
    public async Task Leaving_A_Channel_The_Client_Is_In_Removes_The_Client_From_It_And_Notifies_The_Client_Even_As_The_Last_Member()
    {
        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("MemberLeaver"));

        ChatChannel channel = ChatChannel.GetOrCreate(client.Session, "Member Leave Channel");

        channel.Members.TryAdd(client.Session.Account.Name, new ChatChannelMember(client.Session, channel));
        client.Session.CurrentChannels.Add(channel.ID);

        new LeaveChannel().Process(client.Session, BuildLeaveRequest(channel.Name));

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(client.Session.Account.Name)).IsFalse();
            await Assert.That(client.Session.CurrentChannels).DoesNotContain(channel.ID);
            await Assert.That(await client.ReadCommand()).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
        }
    }

    private static ChatBuffer BuildLeaveRequest(string channelName)
    {
        ChatBuffer request = new ();

        request.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEAVE_CHANNEL);
        request.WriteString(channelName);

        return request;
    }

    /// <summary>
    ///     Constructs a stand-in <see cref="ClientChatSession"/> that bypasses the production constructor, populating only the account and channel-tracking state that leaving a channel reads on the paths that do not send to the client.
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
