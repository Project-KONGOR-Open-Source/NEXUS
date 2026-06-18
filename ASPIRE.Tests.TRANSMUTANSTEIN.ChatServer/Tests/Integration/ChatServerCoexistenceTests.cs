namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Integration;

/// <summary>
///     Verifies that the real host serves its HTTP health surface and the TCP chat protocol concurrently on the same running instance.
///     This exercises the production Kestrel wiring, where configuring the chat protocol endpoints in code also re-applies the HTTP address so that both continue to bind.
/// </summary>
[NotInParallel("ChatServerHost")]
public sealed class ChatServerCoexistenceTests(ServiceContainerContext containerContext)
{
    [Test]
    public async Task Health_Endpoint_And_Chat_Port_Both_Respond_On_The_Same_Host()
    {
        await using ChatServerHost host = await ChatServerHost.StartAsync(containerContext);

        using HttpClient httpClient = host.CreateHTTPClient();

        HttpResponseMessage healthResponse = await httpClient.GetAsync("/health");

        using TcpClient tcpClient = await host.ConnectClientAsync();

        NetworkStream stream = tcpClient.GetStream();

        await stream.WriteAsync(Frame((ushort) ChatProtocol.Bidirectional.NET_CHAT_PING));

        byte[] header = await ReadExactly(stream, 2);
        byte[] payload = await ReadExactly(stream, BitConverter.ToUInt16(header, 0));

        using (Assert.Multiple())
        {
            await Assert.That(healthResponse.IsSuccessStatusCode).IsTrue();
            await Assert.That(BitConverter.ToUInt16(payload, 0)).IsEqualTo((ushort) ChatProtocol.Bidirectional.NET_CHAT_PONG);
        }
    }

    private static byte[] Frame(ushort command)
    {
        byte[] payload = BitConverter.GetBytes(command);
        byte[] frame = new byte[2 + payload.Length];

        BitConverter.GetBytes((ushort) payload.Length).CopyTo(frame, 0);

        payload.CopyTo(frame, 2);

        return frame;
    }

    private static async Task<byte[]> ReadExactly(NetworkStream stream, int count)
    {
        byte[] buffer = new byte[count];
        int offset = 0;

        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(10));

        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), timeout.Token);

            if (read is 0)
                throw new InvalidOperationException("The Connection Closed Before The Expected Bytes Arrived");

            offset += read;
        }

        return buffer;
    }
}
