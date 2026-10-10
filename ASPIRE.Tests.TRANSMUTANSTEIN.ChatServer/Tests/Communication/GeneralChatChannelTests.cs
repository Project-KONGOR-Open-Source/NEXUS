namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers the load balancing across the general chat channels in <see cref="ChatChannel.GetOrCreateGeneralChannel"/>.
///     The general channels live in the process-wide channel registry, so these tests run in isolation from every other test and remove the general channels before and after each test.
/// </summary>
[NotInParallel]
public sealed class GeneralChatChannelTests
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
    {
        RemoveGeneralChannels();

        return Task.CompletedTask;
    }

    [After(HookType.Test)]
    public Task After_Each_Test()
    {
        RemoveGeneralChannels();

        return Task.CompletedTask;
    }

    [Test]
    public async Task An_Overflow_General_Channel_Is_Created_Once_Every_General_Channel_Is_Full()
    {
        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        Fill(firstChannel);

        ChatChannel overflowChannel = ChatChannel.GetOrCreateGeneralChannel();

        using (Assert.Multiple())
        {
            await Assert.That(firstChannel.Name).IsEqualTo(ChatProtocol.CHAT_CHANNEL_BASE_NAME);
            await Assert.That(firstChannel.IsPermanent).IsTrue();
            await Assert.That(overflowChannel.Name).IsEqualTo($"{ChatProtocol.CHAT_CHANNEL_BASE_NAME} 2");
            await Assert.That(overflowChannel.IsGeneralChannel).IsTrue();
            await Assert.That(overflowChannel.IsPermanent).IsFalse();
        }
    }

    [Test]
    public async Task The_First_General_Channel_With_Capacity_Is_Chosen_When_Several_Have_Capacity()
    {
        ChatChannel firstChannel = ChatChannel.GetOrCreateGeneralChannel();

        Fill(firstChannel);

        ChatChannel overflowChannel = ChatChannel.GetOrCreateGeneralChannel();

        // A Member Leaving The First Channel Gives Both General Channels Capacity
        firstChannel.Members.TryRemove("Filler 0", out _);

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

    private static void Fill(ChatChannel channel)
    {
        // The Load Balancing Only Counts Members, So A Single Stand-In Session Serves Every Filler Member
        ClientChatSession fillerSession = (ClientChatSession) RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSession));

        for (int memberIndex = channel.Members.Count; memberIndex < ChatProtocol.MAX_USERS_PER_HON_CHANNEL; memberIndex++)
            channel.Members.TryAdd($"Filler {memberIndex}", new ChatChannelMember(fillerSession, channel));
    }

    private static void RemoveGeneralChannels()
    {
        List<ChatChannel> generalChannels = [.. Context.ChatChannels.Values.Where(channel => channel.IsGeneralChannel)];

        foreach (ChatChannel channel in generalChannels)
            Context.ChatChannels.TryRemove(channel.Name, out _);
    }
}
