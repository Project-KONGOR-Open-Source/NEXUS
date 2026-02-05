using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TUnit.Assertions;
using TUnit.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public class ChatGroupRecreationTests
{
    [Test]
    public async Task CreateGroup_WhileInAnotherGroup_RemovesFromFirstGroup()
    {
        // 1. Start Application
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(0);

        // 2. Connect User A (Leader) & User B (Joiner/Switcher)
        using TcpClient clientA = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 2001, "UserA");
        using TcpClient clientB = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 2002, "UserB");

        NetworkStream streamA = clientA.GetStream();
        NetworkStream streamB = clientB.GetStream();

        // 3. User A Creates Group A
        ChatBuffer createBufferA = new();
        createBufferA.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE);
        createBufferA.WriteString("4.10.1");
        createBufferA.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN);
        createBufferA.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        createBufferA.WriteString("caldavar");
        createBufferA.WriteString("ap");
        createBufferA.WriteString("USE");
        createBufferA.WriteInt8(0);
        createBufferA.WriteInt8(1);
        createBufferA.WriteInt8(0);
        createBufferA.WriteInt8(0);

        await SendPacketAsync(streamA, createBufferA);
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // 4. User B Joins Group A
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_JOIN);
        joinBuffer.WriteString("4.10.1");
        joinBuffer.WriteString("UserA");

        await SendPacketAsync(streamB, joinBuffer);

        // A and B verify Join
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_PLAYER_JOINED_GROUP);

        // 5. User B Creates Group B (While in Group A)
        ChatBuffer createBufferB = new();
        createBufferB.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_CREATE);
        createBufferB.WriteString("4.10.1");
        createBufferB.WriteInt8((byte) ChatProtocol.TMMType.TMM_TYPE_CAMPAIGN);
        createBufferB.WriteInt8((byte) ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL);
        createBufferB.WriteString("midwars");
        createBufferB.WriteString("ap");
        createBufferB.WriteString("USE");
        createBufferB.WriteInt8(0);
        createBufferB.WriteInt8(1);
        createBufferB.WriteInt8(0);
        createBufferB.WriteInt8(0);

        await SendPacketAsync(streamB, createBufferB);

        // 6. Assertions
        // Expect User B to receive TMM_CREATE_GROUP (Success)
        await ExpectGroupUpdateAsync(streamB, ChatProtocol.TMMUpdateType.TMM_CREATE_GROUP);

        // CRITICAL: Expect User A to receive TMM_PLAYER_LEFT_GROUP (User B left Group A)
        // If this times out, it means User B was NOT removed from Group A
        await ExpectGroupUpdateAsync(streamA, ChatProtocol.TMMUpdateType.TMM_PLAYER_LEFT_GROUP);
    }

    private async Task SendPacketAsync(NetworkStream stream, ChatBuffer buffer)
    {
        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawPacket = [];
        rawPacket.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawPacket.AddRange(packet);
        await stream.WriteAsync(rawPacket.ToArray());
    }

    private async Task ExpectGroupUpdateAsync(NetworkStream stream, ChatProtocol.TMMUpdateType expectedType)
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(2)); // Short timeout for failure detection
        try
        {
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

                if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_UPDATE)
                {
                    ChatBuffer reader = new(payload);
                    byte updateType = reader.ReadInt8();

                    if (updateType == (byte)expectedType)
                    {
                        return;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
             throw new TimeoutException($"Did not receive expected Group Update: {expectedType}");
        }
    }
}
