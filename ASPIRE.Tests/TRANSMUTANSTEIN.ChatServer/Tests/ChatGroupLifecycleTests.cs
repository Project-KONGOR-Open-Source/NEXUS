using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TUnit.Assertions;
using TUnit.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public class ChatGroupLifecycleTests
{
    [Test]
    public async Task GroupFlow_MultiUser_JoinAndLeave_ReproducesSyncIssue()
    {
        // 1. Start Application
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(0);

        // 2. Connect User A (Leader) & User B (Joiner)
        using TcpClient clientA = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 2001, "UserA");
        using TcpClient clientB = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 2002, "UserB");

        NetworkStream streamA = clientA.GetStream();
        NetworkStream streamB = clientB.GetStream();

        // 3. User A Creates Group
        // Command: NET_CHAT_CL_TMM_GROUP_CREATE (0x0C0A)
        ChatBuffer createBuffer = new();
        createBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE);
        createBuffer.WriteString("4.10.1"); // ClientVersion
        createBuffer.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN); // GroupType
        createBuffer.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL); // GameType
        createBuffer.WriteString("caldavar"); // MapName
        createBuffer.WriteString("ap"); // GameModes
        createBuffer.WriteString("USE"); // GameRegions
        createBuffer.WriteInt8(0); // Ranked
        createBuffer.WriteInt8(1); // MatchFidelity
        createBuffer.WriteInt8(0); // BotDifficulty
        createBuffer.WriteInt8(0); // RandomizeBots
        // createBuffer.WriteInt8(0); // ArrangedMatch (Removed from some versions, checking CreateRequestData... 
        // Checking ReadOnly MatchmakingGroupCreateRequestData or similar might affect this, but standard is above for now.
        // Assuming strict parity with ChatMatchmakingGroupTests which used Create. 
        // The previous test file didn't show Create packet construction fully in the view (it was truncated).
        // I will assume standard fields. If fails, I'll adjust.
        
        await SendPacketAsync(streamA, createBuffer);
        
        // Assert User A gets Create Confirmation
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // 4. User A Invites User B
        // Command: NET_CHAT_CL_TMM_GROUP_INVITE (0x0C0D)
        ChatBuffer inviteBuffer = new();
        inviteBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);
        inviteBuffer.WriteString("UserB"); // Receiver Name
        
        await SendPacketAsync(streamA, inviteBuffer);
        
        // User B should receive Invitation Broadcast (0x0C0D - same ID as request? No, NET_CHAT_SV_TMM_GROUP_INVITE is likely different or same?
        // ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE = 0x0C0D
        // Response/Broadcast usually depends.
        // Looking at GroupInvite.cs:
        // client.SendResponse(new MatchmakingGroupInviteResponse(...)) -> Packet ?
        // Broadcast(new MatchmakingGroupInviteBroadcast(...)) -> Packet ?
        
        // Simpler approach: Just have User B JOIN. Invitation is usually just UI sugar, unless server enforces "Invited Only"
        // MatchmakingGroup.cs: "if (InvitedAccountIds.Remove(account.AccountId)) ... else ... failureReason = WrongPassword"
        // So User B MUST be invited.
        
        // Verify User B received something? Or just assume it works and try Join?
        // Let's try Join.
        await Task.Delay(200);

        // 5. User B Joins Group
        // Command: NET_CHAT_CL_TMM_GROUP_JOIN (0x0C0B)
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN);
        joinBuffer.WriteString("4.10.1"); // ClientVersion
        joinBuffer.WriteString("UserA"); // InviteIssuerName (Leader)
        
        await SendPacketAsync(streamB, joinBuffer);
        
        // Expect Update on User B (Group Joined - 0x0D03 TMM_PLAYER_JOINED_GROUP = 3)
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);
        
        // Expect Update on User A also
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);

        // 5.1. User A Changes Group Type (Reproducing logs)
        // Command: NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE (0x0D04)
        // Payload: CommandBytes(2) + Type(1)
        ChatBuffer paramBuffer = new();
        paramBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE);
        paramBuffer.WriteInt16((short) ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE); // Manual Command Bytes as GroupChangeType reads it
        paramBuffer.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_PVP); // Change to PVP
        
        await SendPacketAsync(streamA, paramBuffer);
        
        // Expect Update on A and B (Group Type Changed -> TMM_FULL_GROUP_UPDATE = 1)
        // GroupChangeType calls UpdateGroupType which calls MulticastUpdate(TMM_FULL_GROUP_UPDATE)
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
        
        // Change back to Campaign (as per logs)
        paramBuffer = new();
        paramBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE);
        paramBuffer.WriteInt16((short) ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_CHANGE_GROUP_TYPE);
        paramBuffer.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN);
        
        await SendPacketAsync(streamA, paramBuffer);
        
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);

        // 6. User A (Leader) Leaves Group (Forces Leadership Transfer to B)
        // Command: NET_CHAT_CL_TMM_GROUP_LEAVE (0x0C0C)
        ChatBuffer leaveBufferA = new();
        leaveBufferA.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE);
        
        await SendPacketAsync(streamA, leaveBufferA);
        
        // Expect Update on User B (User A Left, Leadership Transferred)
        // We expect TMM_PLAYER_LEFT_GROUP (A removed)
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP);
        // And then FULL_GROUP_UPDATE (B is Leader)
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_FULL_GROUP_UPDATE);

        // 7. User B (New Leader) Leaves Group
        ChatBuffer leaveBufferB = new();
        leaveBufferB.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_LEAVE);
        
        await SendPacketAsync(streamB, leaveBufferB);
        
        // User B should receive Left/Disband update or just succeed without error.
        // Since B is the last one, group disbands. Server might send Left or nothing?
        // Usually sends Left confirm to self.
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP);
    }
    
    private async Task SendPacketAsync(NetworkStream stream, ChatBuffer buffer)
    {
        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawPacket = [];
        rawPacket.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawPacket.AddRange(packet);
        await stream.WriteAsync(rawPacket.ToArray());
    }

    private async Task<(string MapName, string GameModes, byte GameType)> ExpectGroupUpdateAsync(NetworkStream stream, ChatProtocol.TMMUpdateType expectedType)
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
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
            
            if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE) // 0x0D03
            {
                ChatBuffer reader = new(payload);
                byte updateType = reader.ReadInt8();

                if (updateType == (byte)expectedType)
                {
                     // Parse minimum to verify structure
                     reader.ReadInt32(); // Emitter
                     reader.ReadInt8(); // GroupSize
                     // ... skipping validation details for speed, just assume payload exists
                     
                     // Read enough to get to GameType if we need it, but for Leave/Join basic check, type is enough
                     return ("", "", 0);
                }
            }
            // Ignore other packets
        }
        throw new TimeoutException($"Did not receive expected Group Update: {expectedType}");
    }
}
