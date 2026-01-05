using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ConnectionTests
{
    [Test]
    public async Task ConnectsSuccessfullyAndEchoesPing()
    {
        // Arrange
        await using TRANSMUTANSTEINServiceProvider app = await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync();

        using TcpClient client = new TcpClient();

        // Act
        // Connect to the configured Client Port (50001)
        await client.ConnectAsync("localhost", 50001);

        // Assert Connection
        await Assert.That(client.Connected).IsTrue();

        // Arrange Ping Packet
        // Frame: [Length: 2 bytes][Command: 2 bytes][Total: 4 bytes]
        // Length = 2 (Size of Command)
        // Command = NET_CHAT_PING (0x2A00)

        ushort command = ChatProtocol.Bidirectional.NET_CHAT_PING;
        ushort length = 2;

        byte[] lengthBytes = BitConverter.GetBytes(length); // Little Endian: 0x02 0x00
        byte[] commandBytes = BitConverter.GetBytes(command); // Little Endian: 0x00 0x2A

        byte[] packet = [.. lengthBytes, .. commandBytes];

        // Act - Send Ping
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(packet);

        // Assert - Receive Pong
        // Expected: [Length: 2 bytes][Command: NET_CHAT_PONG (0x2A01)]
        byte[] buffer = new byte[4];
        int bytesRead = await stream.ReadAsync(buffer);

        await Assert.That(bytesRead).IsEqualTo(4);

        ushort responseLength = BitConverter.ToUInt16(buffer, 0);
        ushort responseCommand = BitConverter.ToUInt16(buffer, 2);

        await Assert.That(responseLength).IsEqualTo((ushort) 2);
        await Assert.That(responseCommand).IsEqualTo(ChatProtocol.Bidirectional.NET_CHAT_PONG);
    }
}
