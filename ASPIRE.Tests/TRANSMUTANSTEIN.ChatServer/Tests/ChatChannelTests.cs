using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatChannelTests
{
    [Test]
    public async Task JoinChannel_Success()
    {
        // Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        using TcpClient client =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 100, "ChannelUser");
        NetworkStream stream = client.GetStream();

        // Act - Join Channel
        string channelName = "TestChannel";
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL); // 0x001E
        joinBuffer.WriteString(channelName);

        // Send
        byte[] packet = joinBuffer.Data.AsSpan(0, (int) joinBuffer.Size).ToArray();
        ushort packetLength = (ushort) packet.Length;
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes(packetLength));
        rawBytes.AddRange(packet);
        await stream.WriteAsync(rawBytes.ToArray());

        // Assert - Expect CHAT_CMD_CHANGED_CHANNEL (0x0004)
        // Command (2) + String(Name) + Int32(ID) + ...

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, cts.Token);
        ushort length = BitConverter.ToUInt16(header, 0);
        ushort command = BitConverter.ToUInt16(header, 2);

        await Assert.That(command).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL); // 0x0004

        // Read Payload to verify channel name
        int payloadSize = length - 2;
        byte[] payload = new byte[payloadSize];
        int read = 0;
        while (read < payloadSize)
        {
            read += await stream.ReadAsync(payload, read, payloadSize - read, cts.Token);
        }

        // Parse Payload
        ChatBuffer responseBuffer = new(payload);
        string name = responseBuffer.ReadString();
        int channelId = responseBuffer.ReadInt32();

        await Assert.That(name).IsEqualTo(channelName);
        await Assert.That(channelId).IsGreaterThan(0);
    }

    [Test]
    public async Task SendMessage_BroadcastsToChannel()
    {
        // Arrange
        // int testPort = 56015;
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        using TcpClient client1 = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 201, "Alice");
        using TcpClient client2 = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 202, "Bob");

        // Both Join Channel
        string channelName = "ChatRoom";
        int channelId = 0;

        // Client 1 Joins
        {
            ChatBuffer join1 = new();
            join1.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
            join1.WriteString(channelName);
            byte[] p1 = join1.Data.AsSpan(0, (int) join1.Size).ToArray();
            List<byte> r1 = [];
            r1.AddRange(BitConverter.GetBytes((ushort) p1.Length));
            r1.AddRange(p1);
            await client1.GetStream().WriteAsync(r1.ToArray());

            // Read 0x0004 response to get ID
            byte[] h1 = new byte[4];
            await client1.GetStream().ReadExactlyAsync(h1);
            ushort l1 = BitConverter.ToUInt16(h1, 0);
            byte[] py1 = new byte[l1 - 2];
            int read1 = 0;
            while (read1 < py1.Length)
            {
                read1 += await client1.GetStream().ReadAsync(py1, read1, py1.Length - read1);
            }

            ChatBuffer b1 = new(py1);
            b1.ReadString(); // Name
            channelId = b1.ReadInt32(); // ID
        }

        // Client 2 Joins
        {
            ChatBuffer join2 = new();
            join2.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
            join2.WriteString(channelName);
            byte[] p2 = join2.Data.AsSpan(0, (int) join2.Size).ToArray();
            List<byte> r2 = [];
            r2.AddRange(BitConverter.GetBytes((ushort) p2.Length));
            r2.AddRange(p2);
            await client2.GetStream().WriteAsync(r2.ToArray());

            // Consume 0x0004 response
            byte[] h2 = new byte[4];
            await client2.GetStream().ReadExactlyAsync(h2);
            ushort l2 = BitConverter.ToUInt16(h2, 0);
            byte[] py2 = new byte[l2 - 2];
            int read2 = 0;
            while (read2 < py2.Length)
            {
                read2 += await client2.GetStream().ReadAsync(py2, read2, py2.Length - read2);
            }

            // Client 1 will also receive 0x0005 (JOINED_CHANNEL) because Client 2 joined
            // We need to consume that from Client 1's stream so it doesn't interfere
            byte[] h1_notif = new byte[4];
            await client1.GetStream().ReadExactlyAsync(h1_notif);
            ushort l1_notif = BitConverter.ToUInt16(h1_notif, 0);
            byte[] py1_notif = new byte[l1_notif - 2];
            int read1_notif = 0;
            while (read1_notif < py1_notif.Length)
            {
                read1_notif += await client1.GetStream()
                    .ReadAsync(py1_notif, read1_notif, py1_notif.Length - read1_notif);
            }
        }

        // Act - Client 1 Sends Message
        string messageText = "Hello World";
        ChatBuffer msgBuffer = new();
        msgBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG); // 0x0003
        msgBuffer.WriteString(messageText);
        msgBuffer.WriteInt32(channelId); // Needs ID, not Name

        byte[] msgPacket = msgBuffer.Data.AsSpan(0, (int) msgBuffer.Size).ToArray();
        List<byte> rawMsg = [];
        rawMsg.AddRange(BitConverter.GetBytes((ushort) msgPacket.Length));
        rawMsg.AddRange(msgPacket);
        await client1.GetStream().WriteAsync(rawMsg.ToArray());

        // Assert - Client 2 Receives Message (0x0003)
        // Client 1 ALSO receives it (Broadcast includes sender? Check code: yes, unless excluded)
        // Code: channel.BroadcastMessage(broadcast, session.Account.ID); -> excludes Sender?
        // Wait: `channel.BroadcastMessage(broadcast, session.Account.ID);`
        // `BroadcastMessage(..., int? excludeAccountID = null)`
        // `List<ChatChannelMember> recipients = excludeAccountID.HasValue ... Where(member => member.Account.ID != excludeAccountID.Value)`
        // YES. The sender is EXCLUDED in `SendChannelMessage.cs` line 50.
        // So Client 1 does NOT receive it. Client 2 DOES.

        byte[] h_msg = new byte[4];
        await client2.GetStream().ReadExactlyAsync(h_msg);
        ushort l_msg = BitConverter.ToUInt16(h_msg, 0);
        ushort c_msg = BitConverter.ToUInt16(h_msg, 2);

        await Assert.That(c_msg).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);

        byte[] py_msg = new byte[l_msg - 2];
        int r_msg = 0;
        while (r_msg < py_msg.Length)
        {
            r_msg += await client2.GetStream().ReadAsync(py_msg, r_msg, py_msg.Length - r_msg);
        }

        ChatBuffer msgPayload = new(py_msg);
        int senderId = msgPayload.ReadInt32();
        int recvChannelId = msgPayload.ReadInt32();
        string recvMessage = msgPayload.ReadString();

        await Assert.That(senderId).IsEqualTo(201); // Alice
        await Assert.That(recvChannelId).IsEqualTo(channelId);
        await Assert.That(recvMessage).IsEqualTo(messageText);
    }

    [Test]
    public async Task LeaveChannel_Success()
    {
        // Arrange
        // int testPort = 56020;
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        using TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 301, "Leaver");
        NetworkStream stream = client.GetStream();

        // Join First
        string channelName = "LeaveMe";
        int channelId = 0;
        {
            ChatBuffer join = new();
            join.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
            join.WriteString(channelName);
            byte[] p = join.Data.AsSpan(0, (int) join.Size).ToArray();
            List<byte> r = [];
            r.AddRange(BitConverter.GetBytes((ushort) p.Length));
            r.AddRange(p);
            await stream.WriteAsync(r.ToArray());

            // Consume Response
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
            byte[] h = new byte[4];
            await stream.ReadExactlyAsync(h, cts.Token);

            ushort l = BitConverter.ToUInt16(h, 0);
            byte[] py = new byte[l - 2];
            int rd = 0;
            while (rd < py.Length)
            {
                rd += await stream.ReadAsync(py, rd, py.Length - rd, cts.Token);
            }

            ChatBuffer b = new(py);
            b.ReadString();
            channelId = b.ReadInt32();
        }

        // Act - Leave
        ChatBuffer leaveBuffer = new();
        leaveBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_LEAVE_CHANNEL); // 0x0022
        leaveBuffer.WriteString(channelName); // Client sends Name, not ID (checked LeaveChannel.cs)

        byte[] leavePacket = leaveBuffer.Data.AsSpan(0, (int) leaveBuffer.Size).ToArray();
        List<byte> rawLeave = [];
        rawLeave.AddRange(BitConverter.GetBytes((ushort) leavePacket.Length));
        rawLeave.AddRange(leavePacket);
        await stream.WriteAsync(rawLeave.ToArray());

        // Assert - Expect CHAT_CMD_LEFT_CHANNEL (0x0006)
        // Code: ChatChannel.Leave -> sends 0x0006 to [member, .. Members] (so checks self)

        using CancellationTokenSource ctsLeave = new(TimeSpan.FromSeconds(5));
        byte[] h_leave = new byte[4];
        await stream.ReadExactlyAsync(h_leave, ctsLeave.Token);
        ushort c_leave = BitConverter.ToUInt16(h_leave, 2);

        await Assert.That(c_leave).IsEqualTo(ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL);
    }
}