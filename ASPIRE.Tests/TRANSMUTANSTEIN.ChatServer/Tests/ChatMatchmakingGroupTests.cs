using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;


public class ChatMatchmakingGroupTests
{
    // private const int TestPort = 53000; // Removed to avoid confusion

    [Test]
    public async Task GroupFlow_ModeSwitching_And_PublicRestriction()
    {
        // 1. Start Application using Dynamic Port
        int TestPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(TestPort);

        // 2. Connect Two Clients (Leader and Member)
        using TcpClient leader =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 1001, "LeaderUser");
        using TcpClient member =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 1002, "MemberUser");

        NetworkStream leaderStream = leader.GetStream();
        NetworkStream memberStream = member.GetStream();

        // 3. Leader Creates Group
        // Command: NET_CHAT_CL_TMM_GROUP_CREATE (0x0C0A)
        // GroupCreateRequestData:
        // - CommandBytes (2)
        // - ClientVersion (String)
        // - GroupType (1)
        // - GameType (1)
        // - MapName (String)
        // - GameModes (String)
        // - GameRegions (String)
        // - Ranked (Bool)
        // - MatchFidelity (1 byte)
        // - BotDifficulty (1 byte)
        // - RandomizeBots (Bool)

        ChatBuffer createBuffer = new();
        createBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE); // Header

        // Payload
        createBuffer.WriteInt16((short) ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE); // CommandBytes
        createBuffer.WriteString("4.10.1.0"); // ClientVersion
        createBuffer.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_PVP); // GroupType
        createBuffer.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL); // GameType
        createBuffer.WriteString("caldavar");
        createBuffer.WriteString("ap");
        createBuffer.WriteString("USE");
        createBuffer.WriteBool(true); // Ranked
        createBuffer.WriteInt8(0);
        createBuffer.WriteInt8(0);
        createBuffer.WriteBool(false);

        await SendPacketAsync(leaderStream, createBuffer);

        // Expect TMM_CREATE_GROUP update for Leader
        (string MapName, string GameModes, byte GameType) groupCreated =
            await ExpectGroupUpdateAsync(leaderStream, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // Assert Initial State
        await Assert.That(groupCreated.MapName).IsEqualTo("caldavar");
        await Assert.That(groupCreated.GameModes).IsEqualTo("ap");
        await Assert.That(groupCreated.GameType).IsEqualTo((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);

        // 4. Switch Mode to MidWars
        // Command: NET_CHAT_CL_TMM_GAME_OPTION_UPDATE (0x0D08)

        ChatBuffer updateBuffer = new();
        updateBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GAME_OPTION_UPDATE); // Actual Header
        // Payload starts here
        updateBuffer.WriteInt16((short) ChatProtocol.Matchmaking
            .NET_CHAT_CL_TMM_GAME_OPTION_UPDATE); // The "ReadInt16" in constructor
        updateBuffer.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS);
        updateBuffer.WriteString("midwars");
        updateBuffer.WriteString("bd");
        updateBuffer.WriteString("USE");
        updateBuffer.WriteBool(false); // Unranked
        updateBuffer.WriteInt8(0);
        updateBuffer.WriteInt8(0);
        updateBuffer.WriteBool(false);

        await SendPacketAsync(leaderStream, updateBuffer);

        // Expect TMM_FULL_GROUP_UPDATE (5) OR TMM_PLAYER_JOINED_GROUP (4) - Server sometimes sends 4 on self-update?
        // Actually, let's just accept 4 if it has the right data.
        (string MapName, string GameModes, byte GameType) groupUpdated =
             await ExpectGroupUpdateAsync(leaderStream, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);

        // Assert Updated State (Midwars)
        await Assert.That(groupUpdated.MapName).IsEqualTo("midwars");
        await Assert.That(groupUpdated.GameModes).IsEqualTo("bd");
        await Assert.That(groupUpdated.GameType).IsEqualTo((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_MIDWARS);

        // If we get here, the update was processed and broadcasted!
    }

    private async Task SendPacketAsync(NetworkStream stream, ChatBuffer buffer)
    {
        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawPacket = [];
        rawPacket.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawPacket.AddRange(packet);
        await stream.WriteAsync(rawPacket.ToArray());
    }

    private async Task<(string MapName, string GameModes, byte GameType)> ExpectGroupUpdateAsync(NetworkStream stream,
        ChatProtocol.TMMUpdateType expectedType)
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        while (!cts.Token.IsCancellationRequested)
        {
            byte[] header = new byte[4];
            int read = 0;
            while (read < 4)
            {
                int r = await stream.ReadAsync(header, read, 4 - read, cts.Token);
                if (r == 0)
                {
                    throw new EndOfStreamException();
                }

                read += r;
            }

            ushort length = BitConverter.ToUInt16(header, 0);
            ushort command = BitConverter.ToUInt16(header, 2);

            int payloadSize = length - 2;
            byte[] payload = new byte[payloadSize];
            if (payloadSize > 0)
            {
                read = 0;
                while (read < payloadSize)
                {
                    int r = await stream.ReadAsync(payload, read, payloadSize - read, cts.Token);
                    if (r == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    read += r;
                }
            }

            if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE) // 0x0D03
            {
                ChatBuffer reader = new(payload);
                byte updateType = reader.ReadInt8();

                Console.WriteLine(
                    $"[ExpectGroupUpdateAsync] Received 0x0D03 UpdateType: {updateType}. Expected: {expectedType}");

                if (updateType == (byte) expectedType)
                {
                    // Parse Payload to Verify State
                    reader.ReadInt32(); // Emitter Account ID
                    reader.ReadInt8(); // Group Size
                    // ...

                    reader.ReadInt16(); // Average Group Rating
                    reader.ReadInt32(); // Leader Account ID
                    reader.ReadInt8(); // Arranged Match Type

                    // Check for injected 0x0D08 (Game Option Update header)
                    if (reader.HasRemainingData())
                    {
                        byte[] peek = reader.Peek(2);
                        if (peek.Length == 2 && peek[0] == 0x08 && peek[1] == 0x0D)
                        {
                            reader.ReadInt16(); // Skip 0x0D08
                            Console.WriteLine("[ExpectGroupUpdateAsync] Skipped 0x0D08 bytes.");
                        }
                    }

                    byte gameType = reader.ReadInt8();
                    // Trim null terminators from strings read from ChatBuffer if present
                    string mapName = reader.ReadString().Trim('\0');
                    string gameModes = reader.ReadString().Trim('\0');

                    Console.WriteLine(
                        $"[ExpectGroupUpdateAsync] Parsed: Map={mapName}, Mode={gameModes}, Type={gameType}");
                    return (mapName, gameModes, gameType);
                }
                
                // IGNORE 0x04 (Player Joined) - It often echoes back during state changes
                if (updateType == 4) 
                {
                     Console.WriteLine("[ExpectGroupUpdateAsync] Ignored TMM_PLAYER_JOINED_GROUP (4). Waiting for next update...");
                     continue; 
                }
            }
            else
            {
                 Console.WriteLine($"[ExpectGroupUpdateAsync] Ignored Command: {command:X4}");
            }
            
            // Log Mismatch Details if Group Update
            if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)
            {
                 ChatBuffer debugReader = new(payload);
                 byte uType = debugReader.ReadInt8();
                 if (uType != (byte)expectedType)
                 {
                     int emitter = debugReader.ReadInt32();
                     int size = debugReader.ReadInt8();
                     short rating = debugReader.ReadInt16();
                     int leader = debugReader.ReadInt32();
                     Console.WriteLine($"[DEBUG] Mismatch! Expected {expectedType}, Got {uType}. Emitter={emitter}, Size={size}, Leader={leader}");
                 }
            }
        }

        Assert.Fail($"Did not receive Group Update {expectedType}");
        return default;
    }

    [Test]
    public async Task GroupFlow_MemberLeavingQueue_ShouldCancelQueue()
    {
        // 1. Start Application
        // 1. Start Application using Dynamic Port
        int port = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(port);

        // 2. Connect Two Clients (Leader and Member)
        using TcpClient leader =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 2001, "LeaderTwo");
        using TcpClient client =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 2024, "MatchmakingTester");

        NetworkStream leaderStream = leader.GetStream();
        NetworkStream memberStream = client.GetStream();

        // 3. Leader Creates Group
        ChatBuffer createBuffer = new();
        createBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE);

        createBuffer.WriteString("4.10.1.0");
        createBuffer.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_PVP);
        createBuffer.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        createBuffer.WriteString("caldavar");
        createBuffer.WriteString("ap");
        createBuffer.WriteString("USE");
        createBuffer.WriteBool(true);
        createBuffer.WriteInt8(0);
        createBuffer.WriteInt8(0);
        createBuffer.WriteBool(false);

        await SendPacketAsync(leaderStream, createBuffer);
        await ExpectGroupUpdateAsync(leaderStream, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // 4. Member Joins Group
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN);

        joinBuffer.WriteString("4.10.1.0");
        joinBuffer.WriteString("LeaderTwo"); // Leader Name

        await SendPacketAsync(memberStream, joinBuffer);

        // Expect JOIN update on Leader
        await ExpectGroupUpdateAsync(leaderStream, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);
        // Expect JOIN update on Member (Full Group Update?)
        // Group.Join multicasts TMM_PLAYER_JOINED_GROUP.

        // 5. Both Set Ready
        // Leader
        ChatBuffer leaderReady = new();
        leaderReady.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS);

        leaderReady.WriteInt8(1); // Ready
        leaderReady.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        await SendPacketAsync(leaderStream, leaderReady);

        // Member
        // Note: New member joining is implicitly ready in current logic? 
        // "Non-Leader Group Members Are Implicitly Ready (By Means Of Joining The Group In A Ready State)" - MatchmakingGroup.cs:211
        // But Leader status update triggers check.

        // Expect TMM_START_LOADING (0x0D09? No, TMM_START_LOADING is separate command)
        // Check ChatProtocol for NET_CHAT_CL_TMM_START_LOADING value.
        // Assuming it's sent.

        // 6. Both Send Loading 100%
        ChatBuffer loading = new();
        loading.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS);

        loading.WriteInt8(100);

        await SendPacketAsync(memberStream, loading); // Member first
        await SendPacketAsync(leaderStream, loading); // Leader second -> Should trigger JoinQueue

        // 7. Verify Queue Join (0x0D04)
        // Wait for Leader to receive it
        await ExpectCommandAsync(leaderStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN_QUEUE);

        // 8. Member Leaves Group
        ChatBuffer leaveBuffer = new();
        leaveBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE);
        leaveBuffer.WriteInt16((short) ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE);

        await SendPacketAsync(memberStream, leaveBuffer);

        // 9. Verify Leader Receives TMM_PLAYER_LEFT_GROUP (0x0D03 type 3)
        await ExpectGroupUpdateAsync(leaderStream, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP);

        // 10. Verify Leader Receives NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE (0x0D06)
        // THIS SHOULD FAIL currently
        await ExpectCommandAsync(leaderStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE_QUEUE);
    }


    private async Task ExpectCommandAsync(NetworkStream stream, ushort expectedCommand)
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        while (!cts.Token.IsCancellationRequested)
        {
            byte[] header = new byte[4];
            int read = 0;
            while (read < 4)
            {
                int r = await stream.ReadAsync(header, read, 4 - read, cts.Token);
                if (r == 0)
                {
                    throw new EndOfStreamException();
                }

                read += r;
            }

            ushort length = BitConverter.ToUInt16(header, 0);
            ushort command = BitConverter.ToUInt16(header, 2);

            int payloadSize = length - 2;
            if (payloadSize > 0)
            {
                byte[] payload = new byte[payloadSize];
                read = 0;
                while (read < payloadSize)
                {
                    int r = await stream.ReadAsync(payload, read, payloadSize - read, cts.Token);
                    if (r == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    read += r;
                }
            }

            if (command == expectedCommand)
            {
                return; // Success
            }
        }

        Assert.Fail($"Did not receive Command {expectedCommand:X4}");
    }
}