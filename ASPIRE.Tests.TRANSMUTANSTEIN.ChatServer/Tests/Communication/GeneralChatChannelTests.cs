namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers the load balancing across the general chat channels in <see cref="ChatChannel.GetOrCreateGeneralChannel"/> and their compaction in <see cref="ChatChannel.CompactGeneralChannels"/>.
///     The general channels live in the process-wide channel registry, so these tests run in isolation from every other test and remove the general channels before and after each test.
/// </summary>
[NotInParallel]
public sealed class GeneralChatChannelTests
{
    // Stand-In Members Are Constructed Against The Services Of A Minimal Host, Which Supply The Host Environment That A Chat Session Requires
    private WebApplication StandInHost { get; } = WebApplication.CreateSlimBuilder().Build();

    private const string StandInMemberNamePrefix = "Stand-In Member";

    private int StandInMemberCount { get; set; }

    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        // The Chat Server's Static Logger Facade Throws Until Initialised; A Sink-Less Serilog Logger Satisfies It Without Producing Output
        Log.Initialise(new Serilog.LoggerConfiguration().CreateLogger());

        RemoveGeneralChannels();

        return Task.CompletedTask;
    }

    [After(HookType.Test)]
    public async Task After_Each_Test()
    {
        RemoveGeneralChannels();

        await StandInHost.DisposeAsync();
    }

    [Test]
    public async Task The_First_General_Channel_Is_Unnumbered_Until_It_Overflows_And_Is_Then_Renamed_For_Its_Members()
    {
        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        await Assert.That(firstChannel.Name).IsEqualTo(ChatProtocol.CHAT_CHANNEL_BASE_NAME);

        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("RenamedMember"));

        firstChannel.Members.TryAdd(client.Session.Account.Name, new ChatChannelMember(client.Session, firstChannel));
        client.Session.CurrentChannels.Add(firstChannel.ID);

        Fill(firstChannel);

        ChatChannel overflowChannel = ChatChannel.GetOrCreateGeneralChannel();

        using (Assert.Multiple())
        {
            await Assert.That(firstChannel.Name).IsEqualTo($"{ChatProtocol.CHAT_CHANNEL_BASE_NAME} 1");
            await Assert.That(firstChannel.IsPermanent).IsTrue();
            await Assert.That(Context.ChatChannels.ContainsKey(ChatProtocol.CHAT_CHANNEL_BASE_NAME)).IsFalse();
            await Assert.That(Context.ChatChannels[firstChannel.Name]).IsSameReferenceAs(firstChannel);
            await Assert.That(overflowChannel.Name).IsEqualTo($"{ChatProtocol.CHAT_CHANNEL_BASE_NAME} 2");
            await Assert.That(overflowChannel.IsGeneralChannel).IsTrue();
            await Assert.That(overflowChannel.IsPermanent).IsFalse();
            await Assert.That(client.Session.CurrentChannels).Contains(firstChannel.ID);
            await Assert.That(client.Session.CurrentChannels.Count).IsEqualTo(1);
        }

        // The Game Client Cannot Rename An Open Channel, So The Member Leaves The Channel Under Its Old Name And Joins It Under Its New One
        using (Assert.Multiple())
        {
            await Assert.That(await client.ReadCommand()).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
            await Assert.That(await client.ReadCommand()).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);
        }
    }

    [Test]
    public async Task The_First_General_Channel_With_Capacity_Is_Chosen_When_Several_Have_Capacity()
    {
        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        Fill(firstChannel);

        ChatChannel overflowChannel = ChatChannel.GetOrCreateGeneralChannel();

        // A Member Leaving The First Channel Gives Both General Channels Capacity
        RemoveMembers(firstChannel, 1);

        ChatChannel chosenChannel = ChatChannel.GetOrCreateGeneralChannel();

        using (Assert.Multiple())
        {
            await Assert.That(overflowChannel.Members.IsEmpty).IsTrue();
            await Assert.That(chosenChannel).IsSameReferenceAs(firstChannel);
        }
    }

    [Test]
    public async Task An_Overflow_General_Channel_Takes_The_Lowest_Free_Number()
    {
        Fill(ChatChannel.GetOrCreateGeneralChannel());

        ChatChannel secondChannel = ChatChannel.GetOrCreateGeneralChannel();

        Fill(secondChannel);

        Fill(ChatChannel.GetOrCreateGeneralChannel());

        // An Empty Overflow Channel Is Removed, Leaving A Gap In The Numbering
        Context.ChatChannels.TryRemove(secondChannel.Name, out _);

        ChatChannel overflowChannel = ChatChannel.GetOrCreateGeneralChannel();

        using (Assert.Multiple())
        {
            await Assert.That(overflowChannel.Name).IsEqualTo($"{ChatProtocol.CHAT_CHANNEL_BASE_NAME} 2");
            await Assert.That(overflowChannel.Members.IsEmpty).IsTrue();
        }
    }

    [Test]
    public async Task Compaction_Moves_The_Members_Of_Unneeded_General_Channels_Into_The_Lowest_Numbered_Ones_And_Removes_The_Emptied_Channels()
    {
        int memberCap = (int) ChatProtocol.MAX_USERS_PER_CHANNEL;

        // Every General Channel Has To Fill Up Before The Next One Is Created
        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        AddMembers(firstChannel, memberCap);

        ChatChannel secondChannel = ChatChannel.GetOrCreateGeneralChannel();

        AddMembers(secondChannel, memberCap);

        ChatChannel thirdChannel = ChatChannel.GetOrCreateGeneralChannel();

        AddMembers(thirdChannel, 30);

        // Members Leaving The First Two Channels Leaves Enough Room In Them For Every Member Of The Third One
        RemoveMembers(firstChannel, 20);
        RemoveMembers(secondChannel, memberCap - 10);

        await using RunningClientSession movedClient = RunningClientSession.Start(CreateAccount("CompactedMember"));

        thirdChannel.Members.TryAdd(movedClient.Session.Account.Name, new ChatChannelMember(movedClient.Session, thirdChannel));
        movedClient.Session.CurrentChannels.Add(thirdChannel.ID);

        ChatChannel.CompactGeneralChannels();

        // The First Channel Takes 20 Of The 31 Members Of The Third Channel, And The Second Channel Takes The Remaining 11
        using (Assert.Multiple())
        {
            await Assert.That(firstChannel.Members.Count).IsEqualTo(memberCap);
            await Assert.That(secondChannel.Members.Count).IsEqualTo(10 + 11);
            await Assert.That(Context.ChatChannels.ContainsKey(thirdChannel.Name)).IsFalse();
            await Assert.That(movedClient.Session.CurrentChannels).DoesNotContain(thirdChannel.ID);
            await Assert.That(movedClient.Session.CurrentChannels.Count).IsEqualTo(1);
            await Assert.That(await ReadNoticeMessage(movedClient)).StartsWith($"Moved Here From {thirdChannel.Name}");
        }
    }

    [Test]
    public async Task Compaction_Moves_Nobody_When_It_Would_Not_Remove_A_General_Channel()
    {
        int memberCap = (int) ChatProtocol.MAX_USERS_PER_CHANNEL;

        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        AddMembers(firstChannel, memberCap);

        ChatChannel secondChannel = ChatChannel.GetOrCreateGeneralChannel();

        AddMembers(secondChannel, 20);

        // The Members Still Need Two Channels, Since They Outnumber What A Single Channel Holds
        RemoveMembers(firstChannel, 10);

        ChatChannel.CompactGeneralChannels();

        using (Assert.Multiple())
        {
            await Assert.That(firstChannel.Members.Count).IsEqualTo(memberCap - 10);
            await Assert.That(secondChannel.Members.Count).IsEqualTo(20);
        }
    }

    [Test]
    public async Task Compaction_Renames_The_Only_Remaining_General_Channel_Back_To_The_Unnumbered_Name()
    {
        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        Fill(firstChannel);

        ChatChannel overflowChannel = ChatChannel.GetOrCreateGeneralChannel();

        // An Overflow Channel Is Removed Once Its Last Member Leaves
        Context.ChatChannels.TryRemove(overflowChannel.Name, out _);

        ChatChannel.CompactGeneralChannels();

        using (Assert.Multiple())
        {
            await Assert.That(firstChannel.Name).IsEqualTo(ChatProtocol.CHAT_CHANNEL_BASE_NAME);
            await Assert.That(Context.ChatChannels[ChatProtocol.CHAT_CHANNEL_BASE_NAME]).IsSameReferenceAs(firstChannel);
            await Assert.That(Context.ChatChannels.ContainsKey($"{ChatProtocol.CHAT_CHANNEL_BASE_NAME} 1")).IsFalse();
        }
    }

    private void Fill(ChatChannel channel)
        => AddMembers(channel, (int) ChatProtocol.MAX_USERS_PER_CHANNEL - channel.Members.Count);

    /// <summary>
    ///     Adds stand-in members which can be sent to, since renaming a channel and moving members both announce them to the channel members, but whose sent frames nothing reads.
    /// </summary>
    private void AddMembers(ChatChannel channel, int memberCount)
    {
        for (int memberIndex = 0; memberIndex < memberCount; memberIndex++)
        {
            StandInMemberCount++;

            ClientChatSession session = new (new DefaultConnectionContext(), StandInHost.Services)
            {
                Account = CreateAccount($"{StandInMemberNamePrefix} {StandInMemberCount}"),
                Metadata = (ClientChatSessionMetadata) RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSessionMetadata))
            };

            channel.Members.TryAdd(session.Account.Name, new ChatChannelMember(session, channel));
            session.CurrentChannels.Add(channel.ID);
        }
    }

    /// <summary>
    ///     Removes stand-in members from the channel, as if they had left it, leaving any other members in place.
    /// </summary>
    private static void RemoveMembers(ChatChannel channel, int memberCount)
    {
        List<string> memberNames = [.. channel.Members.Keys.Where(memberName => memberName.StartsWith(StandInMemberNamePrefix)).Take(memberCount)];

        foreach (string memberName in memberNames)
            channel.Members.TryRemove(memberName, out _);
    }

    /// <summary>
    ///     Reads the frames sent to the client until a <see cref="ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL"/> notice arrives, skipping the channel announcements which precede it, and returns the notice's message.
    /// </summary>
    private static async Task<string> ReadNoticeMessage(RunningClientSession client)
    {
        while (true)
        {
            (ushort command, string message) = await client.ReadNotice();

            if (command == (ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL)
                return message;
        }
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

    private static void RemoveGeneralChannels()
    {
        List<ChatChannel> generalChannels = [.. Context.ChatChannels.Values.Where(channel => channel.IsGeneralChannel)];

        foreach (ChatChannel channel in generalChannels)
            Context.ChatChannels.TryRemove(channel.Name, out _);
    }
}
