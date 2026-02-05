using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection; // Added for GetRequiredService
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Communication; // Added for ChatChannel
using TRANSMUTANSTEIN.ChatServer.Internals; // Added for IChatContext

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

        int id = Random.Shared.Next(100, 200);
        using TcpClient client =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, id, "ChannelUser");
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
        // Debug timeout to fail fast
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        
        // Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        int p1 = Random.Shared.Next(1000, 2000);
        int p2 = Random.Shared.Next(2001, 3000);

        string aliceName = $"Alice_{Guid.NewGuid().ToString()[..8]}";
        string bobName = $"Bob_{Guid.NewGuid().ToString()[..8]}";

        using TcpClient client1 = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, p1, aliceName);
        
        // Wait for Client 1 to create/join channel
        // Since we are mocking the server context (or strictly, using shared static context), ensure unique channel too?
        // But Channel ID is used.
        // Wait, Client 1 creates "ChatRoom"?
        // Helper `JoinChannelAndGetId` uses "ChatRoom".
        // If "ChatRoom" is static, messages might cross-talk?
        // But Broadcast uses IDs.
        // Let's stick to Names first.

        string channelName = $"Room_{Guid.NewGuid().ToString()[..8]}"; // Randomize
        int channelId = await ChatTestHelpers.JoinChannelAndGetId(client1, channelName);
        Console.WriteLine($"[TEST] Client 1 joined channel ID: {channelId}");

        // Ensure p2 is different from p1
        p2 = Random.Shared.Next(1000, 5000);
        if (p2 == p1) p2++;
        using TcpClient client2 = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, p2, bobName);
        
        Console.WriteLine("[TEST] Clients connected.");

        // Client 2 Joins
        Console.WriteLine("[TEST] Client 2 joining...");
        await ChatTestHelpers.JoinChannelAndGetId(client2, channelName);
        Console.WriteLine("[TEST] Client 2 joined.");

        // Client 1 will also receive 0x0005 (JOINED_CHANNEL) because Client 2 joined
        // We need to consume that from Client 1's stream so it doesn't interfere
        // Or loop until we get it or times out
        Console.WriteLine("[TEST] Client 1 waiting for JOINED_CHANNEL notification...");
        while (!cts.IsCancellationRequested)
        {
            if (client1.Available < 4) 
            {
               await Task.Delay(100, cts.Token);
               continue;
            }

            byte[] h1_notif = new byte[4];
            await client1.GetStream().ReadExactlyAsync(h1_notif, cts.Token);
            ushort l1_notif = BitConverter.ToUInt16(h1_notif, 0);
            ushort c1_notif = BitConverter.ToUInt16(h1_notif, 2);
            
            Console.WriteLine($"[TEST] Client 1 received CMD: 0x{c1_notif:X4} Length: {l1_notif}");

            byte[] py1_notif = new byte[l1_notif - 2];
            int read1_notif = 0;
            while (read1_notif < py1_notif.Length)
            {
                read1_notif += await client1.GetStream()
                    .ReadAsync(py1_notif, read1_notif, py1_notif.Length - read1_notif, cts.Token);
            }

            if (c1_notif == ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL) // 0x0005
            {
                Console.WriteLine("[TEST] Client 1 consumed JOINED_CHANNEL.");
                break;
            }
            else if (c1_notif == ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL) // 0x0006
            {
                // Inspect who left
                ChatBuffer leftBuf = new(py1_notif);
                int leaverId = leftBuf.ReadInt32();
                int leftChanId = leftBuf.ReadInt32();
                Console.WriteLine($"[TEST] Received LEFT_CHANNEL (0x0006). LeaverID: {leaverId}, ChannelID: {leftChanId}");
                
                if (leaverId == p1)
                {
                     Console.WriteLine("[TEST] CRITICAL: Client 1 (Alice) received LEFT_CHANNEL for SELF. Disconnected?");
                }
                
                // If Bob left, we have a problem. If Alice left, we have a problem.
                // If it's a "Left" message, we should probably keep waiting for "Joined"? 
                // But if Bob joined and then left, we missed the Join? Or Join never happened?
                // "Client Joined Channel" log appeared.
                // Maybe Bob joined, then was forced to leave?
                // We'll continue loop.
                continue;
            }
        }

        // Act - Client 1 Sends Message
        Console.WriteLine("[TEST] Client 1 sending message...");
        string messageText = "Hello World";
        ChatBuffer msgBuffer = new();
        msgBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG); // 0x0003
        msgBuffer.WriteString(messageText);
        msgBuffer.WriteInt32(channelId); // Needs ID, not Name

        byte[] msgPacket = msgBuffer.Data.AsSpan(0, (int) msgBuffer.Size).ToArray();
        List<byte> rawMsg = [];
        rawMsg.AddRange(BitConverter.GetBytes((ushort) msgPacket.Length));
        rawMsg.AddRange(msgPacket);
        
        Console.WriteLine($"[TEST] Client 1 sending {rawMsg.Count} bytes: {BitConverter.ToString(rawMsg.ToArray())}");
        
        await client1.GetStream().WriteAsync(rawMsg.ToArray(), cts.Token);
        Console.WriteLine("[TEST] Client 1 sent message.");

        // Wait to ensure Client 2 is ready to receive
        // Wait to ensure Client 2 is ready to receive
        await Task.Delay(1000, cts.Token);

        // Assert - Client 2 Receives Message (0x0003)
        // Loop to find the message command, skipping JOINED_CHANNEL (0x0005) if received (e.g. self-join notif)
        ushort c_msg = 0;
        int maxRetries = 20;
        byte[] py_msg = [];

        Console.WriteLine("[TEST] Client 2 waiting for message...");
        for (int i = 0; i < maxRetries; i++)
        {
            byte[] h_msg = new byte[4];
            await client2.GetStream().ReadExactlyAsync(h_msg, cts.Token);
            ushort l_msg = BitConverter.ToUInt16(h_msg, 0);
            c_msg = BitConverter.ToUInt16(h_msg, 2);
            Console.WriteLine($"[TEST] Client 2 received CMD: 0x{c_msg:X4}");

            // Read Payload
            py_msg = new byte[l_msg - 2];
            int r_msg = 0;
            while (r_msg < py_msg.Length)
            {
                r_msg += await client2.GetStream().ReadAsync(py_msg, r_msg, py_msg.Length - r_msg, cts.Token);
            }

            if (c_msg == ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG)
            {
                Console.WriteLine("[TEST] Client 2 received message.");
                break;
            }

            if (c_msg == ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL) // 0x0006
            {
                ChatBuffer leftBuf = new(py_msg);
                int leaverId = leftBuf.ReadInt32();
                int leftChanId = leftBuf.ReadInt32();
                Console.WriteLine($"[TEST] Client 2 Received LEFT_CHANNEL. LeaverID: {leaverId}, ChannelID: {leftChanId}");
                continue;
            }
            
            // If it's something unrelated (JOINED/CHANGED), ignore and continue
            if (c_msg == ChatProtocol.Command.CHAT_CMD_JOINED_CHANNEL ||
                c_msg == ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL)
            {
                 continue;
            }

            // If it's something else unexpected, logic will break and Assert will fail
            break;
        }

        await Assert.That(c_msg).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_MSG);

        ChatBuffer msgPayload = new(py_msg);
        int senderId = msgPayload.ReadInt32();
        int recvChannelId = msgPayload.ReadInt32();
        string recvMessage = msgPayload.ReadString();

        await Assert.That(senderId).IsEqualTo(p1); // Alice
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

        int id = Random.Shared.Next(301, 399);
        using TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, id, "Leaver");
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

    [Test]
    public async Task JoinChannel_Unjoinable_RejectsNonStaff()
    {
        // Arrange
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(0);

        IChatContext chatContext = app.Services.GetRequiredService<IChatContext>();

        // Create Channel with Unjoinable Flag
        string channelName = "UnjoinableChannel";
        ChatChannel channel = ChatChannel.GetOrCreate(chatContext, null, channelName);
        channel.Flags |= ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_UNJOINABLE;

        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
        joinBuffer.WriteString(channelName);

        // --- Test 1: Regular User ---
        int regularId = Random.Shared.Next(4000, 5000);
        using TcpClient regularClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(
            app, app.ClientPort, regularId, "RegularUser", accountType: AccountType.Normal);
        NetworkStream regularStream = regularClient.GetStream();

        // Act - Join
        await ChatTestHelpers.SendPacketAsync(regularStream, joinBuffer);

        // Assert - Expect Whisper Rejection (CHAT_CMD_WHISPER - 0x0001)
        // Staff mentioned: "Whisper is fine"
        ChatBuffer response = await ChatTestHelpers.ExpectCommandAsync(regularStream, ChatProtocol.Command.CHAT_CMD_WHISPER, 5);
        string sender = response.ReadString();
        string msg = response.ReadString();
        await Assert.That(sender).IsEqualTo("Channel Service");
        await Assert.That(msg).Contains("unjoinable", StringComparison.OrdinalIgnoreCase);

        // --- Test 2: Staff User ---
        int staffId = Random.Shared.Next(5001, 6000);
        using TcpClient staffClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(
            app, app.ClientPort, staffId, "StaffUser", accountType: AccountType.Staff);
        NetworkStream staffStream = staffClient.GetStream();

        // Act - Join
        await ChatTestHelpers.SendPacketAsync(staffStream, joinBuffer);

        // Assert - Expect Success (CHAT_CMD_CHANGED_CHANNEL - 0x0004)
        ChatBuffer staffResponse = await ChatTestHelpers.ExpectCommandAsync(staffStream, ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL, 5);
        string name = staffResponse.ReadString();
        await Assert.That(name).IsEqualTo(channelName);
    }
}