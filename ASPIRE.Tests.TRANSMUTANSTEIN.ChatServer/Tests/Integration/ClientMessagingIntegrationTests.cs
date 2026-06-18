namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Drives client-to-client messaging against the real host over real sockets, verifying that the chat server delivers whispers and channel messages to their recipients through each connection's outbound write pump, in order, and reports failures back to the sender.
/// </summary>
[NotInParallel("ChatServerHost")]
public sealed class ClientMessagingIntegrationTests(ServiceContainerContext containerContext)
{
    private const string RemoteIP = "127.0.0.1";

    [Test]
    public async Task Whisper_Is_Delivered_To_The_Recipient()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int senderID, string senderName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);
        (int recipientID, string recipientName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string senderCookie = Guid.CreateVersion7().ToString();
        string recipientCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, senderCookie, senderName);
        await ChatTestData.SeedClientSessionCookie(host.Services, recipientCookie, recipientName);

        using TcpClient sender = await host.ConnectAndAuthenticateClientAsync(senderID, senderName, senderCookie, RemoteIP);
        using TcpClient recipient = await host.ConnectAndAuthenticateClientAsync(recipientID, recipientName, recipientCookie, RemoteIP);

        const string message = "Hello Over The Wire";

        await ChatTestProtocol.WriteFrame(sender.GetStream(), ChatTestProtocol.BuildWhisper(recipientName, message));

        ChatBuffer? whisper = await ChatTestProtocol.ReadUntilCommand(recipient.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_WHISPER, TimeSpan.FromSeconds(10));

        await Assert.That(whisper).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(whisper!.ReadString()).IsEqualTo(senderName); // Sender Name
            await Assert.That(whisper.ReadString()).IsEqualTo(message);     // Message Content
        }
    }

    [Test]
    public async Task Whisper_To_An_Offline_Account_Reports_Failure_To_The_Sender()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int senderID, string senderName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string senderCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, senderCookie, senderName);

        using TcpClient sender = await host.ConnectAndAuthenticateClientAsync(senderID, senderName, senderCookie, RemoteIP);

        const string offlineRecipientName = "NobodyHome";
        const string message = "Anyone There";

        await ChatTestProtocol.WriteFrame(sender.GetStream(), ChatTestProtocol.BuildWhisper(offlineRecipientName, message));

        ChatBuffer? failure = await ChatTestProtocol.ReadUntilCommand(sender.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_WHISPER_FAILED, TimeSpan.FromSeconds(10));

        await Assert.That(failure).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(failure!.ReadString()).IsEqualTo(offlineRecipientName); // Recipient's Account Name
            await Assert.That(failure.ReadString()).IsEqualTo(message);               // Message Content
        }
    }

    [Test]
    public async Task Multiple_Whispers_Are_Delivered_To_The_Recipient_In_Order()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        // The Sender Is A Staff Account So That It Is Exempt From Flood Prevention; This Isolates The Test To The Write Pump's Send Ordering Rather Than The Flood Gate, Which Is Covered Separately By The Flood Prevention Service Tests
        (int senderID, string senderName) = await ChatTestData.SeedAccount(host.Services, AccountType.Staff);
        (int recipientID, string recipientName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string senderCookie = Guid.CreateVersion7().ToString();
        string recipientCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, senderCookie, senderName);
        await ChatTestData.SeedClientSessionCookie(host.Services, recipientCookie, recipientName);

        using TcpClient sender = await host.ConnectAndAuthenticateClientAsync(senderID, senderName, senderCookie, RemoteIP);
        using TcpClient recipient = await host.ConnectAndAuthenticateClientAsync(recipientID, recipientName, recipientCookie, RemoteIP);

        const int whisperCount = 50;

        for (int index = 0; index < whisperCount; index++)
            await ChatTestProtocol.WriteFrame(sender.GetStream(), ChatTestProtocol.BuildWhisper(recipientName, $"Message {index}"));

        List<string> receivedMessages = [];

        for (int index = 0; index < whisperCount; index++)
        {
            ChatBuffer? whisper = await ChatTestProtocol.ReadUntilCommand(recipient.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_WHISPER, TimeSpan.FromSeconds(10));

            await Assert.That(whisper).IsNotNull();

            whisper!.ReadString();                       // Sender Name
            receivedMessages.Add(whisper.ReadString());  // Message Content
        }

        List<string> expectedMessages = [.. Enumerable.Range(0, whisperCount).Select(index => $"Message {index}")];

        // The Single Outbound Channel Drained By One Write Pump Must Preserve Strict First-In-First-Out Send Order
        await Assert.That(receivedMessages.SequenceEqual(expectedMessages)).IsTrue();
    }

    [Test]
    public async Task Channel_Message_Is_Broadcast_To_Other_Channel_Members()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        (int firstID, string firstName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);
        (int secondID, string secondName) = await ChatTestData.SeedAccount(host.Services, AccountType.Normal);

        string firstCookie = Guid.CreateVersion7().ToString();
        string secondCookie = Guid.CreateVersion7().ToString();

        await ChatTestData.SeedClientSessionCookie(host.Services, firstCookie, firstName);
        await ChatTestData.SeedClientSessionCookie(host.Services, secondCookie, secondName);

        using TcpClient first = await host.ConnectAndAuthenticateClientAsync(firstID, firstName, firstCookie, RemoteIP);
        using TcpClient second = await host.ConnectAndAuthenticateClientAsync(secondID, secondName, secondCookie, RemoteIP);

        // A Channel Name Unique To This Test, Because Channels Live In The Process-Wide Registry For The Lifetime Of The Host
        string channelName = $"TestChannel_{Guid.CreateVersion7():N}"[..20];

        // The First Client Joins And Learns The Assigned Channel Identifier From Its Channel-Changed Response
        await ChatTestProtocol.WriteFrame(first.GetStream(), ChatTestProtocol.BuildJoinChannel(channelName));

        ChatBuffer? firstChanged = await ChatTestProtocol.ReadUntilCommand(first.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL, TimeSpan.FromSeconds(10));

        await Assert.That(firstChanged).IsNotNull();

        firstChanged!.ReadString();              // Channel Name
        int channelID = firstChanged.ReadInt32(); // Channel ID

        // The Second Client Joins The Same Channel; Reading Its Channel-Changed Response Confirms It Is A Member Before The Message Is Sent
        await ChatTestProtocol.WriteFrame(second.GetStream(), ChatTestProtocol.BuildJoinChannel(channelName));

        await Assert.That(await ChatTestProtocol.ReadUntilCommand(second.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL, TimeSpan.FromSeconds(10))).IsNotNull();

        const string message = "Hello Channel";

        await ChatTestProtocol.WriteFrame(first.GetStream(), ChatTestProtocol.BuildChannelMessage(message, channelID));

        ChatBuffer? received = await ChatTestProtocol.ReadUntilCommand(second.GetStream(), (ushort) ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG, TimeSpan.FromSeconds(10));

        await Assert.That(received).IsNotNull();

        using (Assert.Multiple())
        {
            await Assert.That(received!.ReadInt32()).IsEqualTo(firstID);  // Sender Account ID
            await Assert.That(received.ReadInt32()).IsEqualTo(channelID); // Channel ID
            await Assert.That(received.ReadString()).IsEqualTo(message);  // Message Content
        }
    }
}
