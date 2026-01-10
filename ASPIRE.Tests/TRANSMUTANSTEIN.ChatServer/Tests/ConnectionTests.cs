using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ConnectionTests
{
    private static int _nextPort = 51000;

    [Test]
    public async Task ConnectsSuccessfullyAndEchoesPing()
    {
        // Arrange
        // Use a unique port for this test run to prevent socket reuse/conflicts
        int port = Interlocked.Increment(ref _nextPort);
        await using TRANSMUTANSTEINServiceProvider app = await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(port);

        using TcpClient client = new ();

        // Act
        // Connect to the configured Client Port (50300)
        // Add retry logic because the server might take a moment to bind
        bool connected = false;
        // Retry loop to handle server startup race conditions
        for(int i=0; i<5; i++)
        {
             try
             {
                await client.ConnectAsync("127.0.0.1", port);
                connected = true;
                break;
             }
             catch
             {
                await Task.Delay(500);
             }
        }

        if(!connected) Assert.Fail("Could not connect to server after retries");

        // Give server a moment to accept and log
        await Task.Delay(100);

        // Arrange Ping Packet
        // Frame: [Length] [Command] [Payload]
        // Length = 2 (Command) + 8 (Timestamp) = 10
        
        ushort command = ChatProtocol.Bidirectional.NET_CHAT_PING;
        long timestamp = Environment.TickCount64;
        
        byte[] commandBytes = BitConverter.GetBytes(command); 
        byte[] timestampBytes = BitConverter.GetBytes(timestamp);
        ushort totalSize = (ushort)(commandBytes.Length + timestampBytes.Length); 
        byte[] lengthBytes = BitConverter.GetBytes(totalSize);

        byte[] packet = [.. lengthBytes, .. commandBytes, .. timestampBytes];

        // Act - Send Ping
        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(packet);

        // Assert - Receive Pong
        // Expected: [Length: 10 bytes] [Command: PONG] [Timestamp]
        // Total bytes to read: 2 (Length) + 2 (Command) + 8 (Timestamp) = 12
        
        byte[] buffer = new byte[12];
        try 
        {
            // Read exactly 12 bytes
            int totalRead = 0;
            while(totalRead < 12)
            {
               int read = await stream.ReadAsync(buffer, totalRead, 12 - totalRead);
               if (read == 0) break;
               totalRead += read;
            }
            await Assert.That(totalRead).IsEqualTo(12);
        }
        catch (IOException ex)
        {
             Assert.Fail($"Connection closed prematurely: {ex.Message}");
        }

        ushort responseLength = BitConverter.ToUInt16(buffer, 0);
        ushort responseCommand = BitConverter.ToUInt16(buffer, 2);
        long responseTimestamp = BitConverter.ToInt64(buffer, 4);

        await Assert.That(responseLength).IsEqualTo(totalSize);
        await Assert.That(responseCommand).IsEqualTo(ChatProtocol.Bidirectional.NET_CHAT_PONG);
        await Assert.That(responseTimestamp).IsEqualTo(timestamp);
    }
}
