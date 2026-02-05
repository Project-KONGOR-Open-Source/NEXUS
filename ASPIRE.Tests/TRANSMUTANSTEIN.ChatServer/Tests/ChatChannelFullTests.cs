using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using TRANSMUTANSTEIN.ChatServer.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatChannelFullTests
{
    [Test]
    public async Task JoinChannel_RejectsNonAdmin_WhenChannelIsFull()
    {
        // Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        // 1. Connect Setup Client to create channel
        int setupId = 1001;
        using TcpClient setupClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, setupId, "SetupUser");

        string channelName = "FullChannel";
        int channelId = await ChatTestHelpers.JoinChannelAndGetId(setupClient, channelName);

        // 2. Fill Channel Members
        IChatContext chatContext = app.Services.GetRequiredService<IChatContext>();
        ChatChannel channel = chatContext.ChatChannels[channelName];

        // We need a member instance. We can grab the one we just added.
        ChatChannelMember existingMember = channel.Members.Values.First();

        // Fill up to 250 (ChatProtocol.MAX_USERS_PER_CHANNEL)
        // We already have 1 member.
        for (int i = 1; i < ChatProtocol.MAX_USERS_PER_CHANNEL; i++)
        {
            channel.Members.TryAdd($"Dummy_{i}", existingMember);
        }

        await Assert.That(channel.IsFull).IsTrue();

        // 3. Connect Victim Client (Non-Admin)
        int victimId = 1002;
        using TcpClient victimClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, victimId, "VictimUser");
        NetworkStream stream = victimClient.GetStream();

        // Act - Attempt to Join
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
        joinBuffer.WriteString(channelName);
        await ChatTestHelpers.SendPacketAsync(stream, joinBuffer);

        // Assert - Expect CHAT_CMD_WHISPER (0x0008)
        ChatBuffer response = await ChatTestHelpers.ExpectCommandAsync(stream, ChatProtocol.Command.CHAT_CMD_WHISPER);
        string sender = response.ReadString();
        string message = response.ReadString();

        await Assert.That(sender).IsEqualTo("Channel Service");
        await Assert.That(message).IsEqualTo("This channel is full.");

        // Verify not in channel
        await Assert.That(channel.Members.ContainsKey("VictimUser")).IsFalse();
    }

    [Test]
    public async Task JoinChannel_AllowsAdmin_WhenChannelIsFull()
    {
        // Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        // 1. Connect Setup Client to create channel
        int setupId = 2001;
        using TcpClient setupClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, setupId, "SetupUser2");

        string channelName = "FullChannelAdmin";
        int channelId = await ChatTestHelpers.JoinChannelAndGetId(setupClient, channelName);

        // 2. Fill Channel Members
        IChatContext chatContext = app.Services.GetRequiredService<IChatContext>();
        ChatChannel channel = chatContext.ChatChannels[channelName];
        ChatChannelMember existingMember = channel.Members.Values.First();

        for (int i = 1; i < ChatProtocol.MAX_USERS_PER_CHANNEL; i++)
        {
            channel.Members.TryAdd($"Dummy_{i}", existingMember);
        }

        await Assert.That(channel.IsFull).IsTrue();

        // 3. Connect Admin Client (Staff)
        int adminId = 9999;
        // ConnectAndAuthenticateAsync has accountType param.
        using TcpClient adminClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(
            app, app.ClientPort, adminId, "AdminUser", accountType: AccountType.Staff);
        NetworkStream stream = adminClient.GetStream();

        // Act - Attempt to Join
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
        joinBuffer.WriteString(channelName);
        await ChatTestHelpers.SendPacketAsync(stream, joinBuffer);

        // Assert - Expect CHAT_CMD_CHANGED_CHANNEL (0x0004)
        await ChatTestHelpers.ExpectCommandAsync(stream, ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL);

        // Verify is in channel
        await Assert.That(channel.Members.ContainsKey("AdminUser")).IsTrue();
    }
}
