using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatCommunicationTests
{
    [Test]
    public async Task Whisper_Delivered()
    {
        // Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        using TcpClient sender =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 601, "WhisperSender_601");
        using TcpClient recipient =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 602, "WhisperRecipient_602");

        // Act - Send Whisper
        string message = "Psst, this is a secret.";
        ChatBuffer whisperBuffer = new();
        whisperBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_WHISPER); // 0x0008
        whisperBuffer.WriteString("WhisperRecipient_602"); // Target Name
        whisperBuffer.WriteString(message); // Message

        byte[] packet = whisperBuffer.Data.AsSpan(0, (int) whisperBuffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await sender.GetStream().WriteAsync(rawBytes.ToArray());

        // Assert - Recipient receives 0x0008
        NetworkStream recvStream = recipient.GetStream();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        byte[] header = new byte[4];
        await recvStream.ReadExactlyAsync(header, cts.Token);
        ushort length = BitConverter.ToUInt16(header, 0);
        ushort command = BitConverter.ToUInt16(header, 2);

        await Assert.That(command).IsEqualTo(ChatProtocol.Command.CHAT_CMD_WHISPER); // 0x0008

        int payloadSize = length - 2;
        byte[] payload = new byte[payloadSize];
        int read = 0;
        while (read < payloadSize)
        {
            read += await recvStream.ReadAsync(payload, read, payloadSize - read, cts.Token);
        }

        ChatBuffer b = new(payload);
        string senderName = b.ReadString();
        string receivedMsg = b.ReadString();

        await Assert.That(senderName).IsEqualTo("WhisperSender_601");
        await Assert.That(receivedMsg).IsEqualTo(message);
    }
}