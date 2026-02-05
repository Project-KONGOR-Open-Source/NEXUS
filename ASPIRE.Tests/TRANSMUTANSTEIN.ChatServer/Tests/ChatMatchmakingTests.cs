using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public class ChatMatchmakingTests
{
    [Test]
    public async Task Matchmaking_WithIdleServer_ShouldConnectClients()
    {
        // 1. Start Application using Dynamic Port (1v1 config)
        int port = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(port, playersPerTeam: 1);

        // 2. Setup Game Server Mock
        int serverId = 11235;
        string serverCookie = "mock_server_cookie";
        int hostAccountId = 9001;

        await ChatTestHelpers.SeedGameServerHostAsync(app, hostAccountId, serverId, serverCookie);

        // 3. Connect Game Server
        // We need a separate connection for the game server
        using TcpClient gameServerClient = await ChatTestHelpers.ConnectAndAuthenticateGameServerAsync(
            app, app.MatchServerPort, serverId, serverCookie);

        NetworkStream gameServerStream = gameServerClient.GetStream();

        // 4. Connect Clients (Leader and Member)
        using TcpClient leader =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 1001, "LeaderUser", region: "USE");
        using TcpClient member =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 1002, "MemberUser", region: "USE");

        NetworkStream leaderStream = leader.GetStream();
        NetworkStream memberStream = member.GetStream();

        // 5. Leader Creates Group
        ChatBuffer createBuffer = new();
        createBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE);
        createBuffer.WriteString("4.10.1.0");
        createBuffer.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_PVP);
        createBuffer.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        createBuffer.WriteString("caldavar");
        createBuffer.WriteString("ap");
        createBuffer.WriteString("USE");
        createBuffer.WriteBool(true); // Ranked
        createBuffer.WriteInt8(0);
        createBuffer.WriteInt8(0);
        createBuffer.WriteBool(false);
        await ChatTestHelpers.SendPacketAsync(leaderStream, createBuffer);
        
        // Wait for group create
        await ChatTestHelpers.ExpectCommandAsync(leaderStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE);

        // 6. Member Joins Group
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN);
        joinBuffer.WriteString("4.10.1.0");
        joinBuffer.WriteString("LeaderUser");
        await ChatTestHelpers.SendPacketAsync(memberStream, joinBuffer);

        // Wait for joins
        await ChatTestHelpers.ExpectCommandAsync(leaderStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE); // Join
        await ChatTestHelpers.ExpectCommandAsync(memberStream, ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE); // Full Update

        // 7. Set Ready & Loading (Both)
        ChatBuffer ready = new();
        ready.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_READY_STATUS);
        ready.WriteInt8(1);
        ready.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);

        ChatBuffer loading = new();
        loading.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_PLAYER_LOADING_STATUS);
        loading.WriteInt8(100);

        await ChatTestHelpers.SendPacketAsync(leaderStream, ready);
        await ChatTestHelpers.SendPacketAsync(memberStream, ready);
        
        // Note: Ready status updates go to everyone
        // We can just blast through to the loading phase
        
        await ChatTestHelpers.SendPacketAsync(leaderStream, loading);
        await ChatTestHelpers.SendPacketAsync(memberStream, loading);

        // 9. Match Broker Trigger: Wait for GS to receive CREATE_MATCH
        // We verify GS receives it FIRST, because clients won't get anything yet.
        ChatBuffer createMatchPacket = await ChatTestHelpers.ExpectCommandAsync(gameServerStream, ChatProtocol.ChatServerToGameServer.NET_CHAT_GS_CREATE_MATCH);
        
        // Parse CreateMatch to get MatchUpID
        // Offset 0: Command (2 bytes) - Handled by ExpectCommand
        // Offset 0 (in returned buffer): Payload starts
        // Payload: MatchType(1), MatchUpID(4), ...
        byte matchTypeRx = createMatchPacket.ReadInt8();
        int matchUpId = createMatchPacket.ReadInt32();
        
        // 10. Simulate GS sending ANNOUNCE_MATCH (0x0503)
        // Structure: Command(2), MatchUpID(4), Challenge(4), GroupCount(4), GroupIDs(4*N), MatchID(4)
        ChatBuffer announceMatch = new();
        announceMatch.WriteCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_ANNOUNCE_MATCH);
        announceMatch.WriteInt32(matchUpId); // MatchUpID from CreateMatch
        announceMatch.WriteInt32(0); // Challenge
        announceMatch.WriteInt32(0); // GroupCount (Ignored by logic for now)
        // GroupIDs skipped
        announceMatch.WriteInt32(123456); // Real MatchID generated by GS
        
        await ChatTestHelpers.SendPacketAsync(gameServerStream, announceMatch);
        
        // 11. Now verify Clients receive match found
        await VerifyMatchFoundAsync(leaderStream, "127.0.0.1", 11235);
        await VerifyMatchFoundAsync(memberStream, "127.0.0.1", 11235);
    }
 
    private async Task VerifyMatchFoundAsync(NetworkStream stream, string expectedIp, int expectedPort)
    {
         using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10)); // Broker runs every 1s, give it time
         int packetsFound = 0;
         
         while (!cts.Token.IsCancellationRequested)
         {
             byte[] header = new byte[4];
             int read = 0;
             while (read < 4)
             {
                 int r = await stream.ReadAsync(header, read, 4 - read, cts.Token);
                 if (r == 0) throw new EndOfStreamException();
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
                     if (r == 0) throw new EndOfStreamException();
                     read += r;
                 }
             }

             ChatBuffer reader = new(payload);

             if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_MATCH_FOUND_UPDATE) // 0x0D09
             {
                 // Consuming payload so we verify serialization doesn't crash
                 string mapName = reader.ReadString();
                 byte teamSize = reader.ReadInt8();
                 byte gameType = reader.ReadInt8();
                 string modes = reader.ReadString();
                 string regions = reader.ReadString();
                 string unknown = reader.ReadString();
                 
                 Console.WriteLine($"[VerifyMatchFound] Received 0x0D09 MATCH_FOUND_UPDATE: Map={mapName}");
                 packetsFound++;
             }
             else if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_QUEUE_UPDATE) // 0x0D06
             {
                 byte updateType = reader.ReadInt8();
                 // If type 16, no extra data.
                 Console.WriteLine($"[VerifyMatchFound] Received 0x0D06 GROUP_QUEUE_UPDATE: Type={updateType}");
                 packetsFound++;
             }
             else if (command == 0x0062) // NET_CHAT_CL_AUTO_MATCH_CONNECT
             {
                 byte matchType = reader.ReadInt8();
                 int matchId = reader.ReadInt32();
                 string ip = reader.ReadString();
                 short port = reader.ReadInt16();
                 int unknownRandom = reader.ReadInt32();
                 
                 Console.WriteLine($"[VerifyMatchFound] Received 0x0062 AUTO_MATCH_CONNECT: MatchID={matchId}, IP={ip}, Port={port}");
                 
                 await Assert.That(ip).IsEqualTo(expectedIp);
                 await Assert.That(port).IsEqualTo((short)expectedPort);
                 return; // Success!
             }
             else
             {
                 Console.WriteLine($"[VerifyMatchFound] Ignored 0x{command:X4}");
             }
         }
         
         Assert.Fail("Did not receive NET_CHAT_CL_AUTO_MATCH_CONNECT (0x0062)");
    }
}
