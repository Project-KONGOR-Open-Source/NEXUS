namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Transport;

/// <summary>
///     Exercises the chat session over a real Kestrel endpoint and a real <see cref="TcpClient"/>, so that the connection-handler wiring and the socket transport are covered end-to-end.
///     A minimal host binds one ephemeral port to <see cref="ClientConnectionHandler"/>; the <c>PING</c> command is used as the probe because it has no database or distributed-cache dependency.
/// </summary>
public sealed class KestrelChatConnectionTests
{
    [Test]
    public async Task Ping_Round_Trips_Over_A_Real_Kestrel_Socket()
    {
        int port = GetFreeTCPPort();

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.ConfigureKestrel(kestrelOptions =>
            kestrelOptions.ListenAnyIP(port, listenOptions => listenOptions.UseConnectionHandler<ClientConnectionHandler>()));

        await using WebApplication application = builder.Build();

        // The Chat Server's Static Logger Facade Throws Until Initialised; A Sink-Less Serilog Logger Satisfies It Without Producing Output
        Log.Initialise(new Serilog.LoggerConfiguration().CreateLogger());

        await application.StartAsync();

        using TcpClient client = new ();

        await client.ConnectAsync(IPAddress.Loopback, port);

        NetworkStream stream = client.GetStream();

        await stream.WriteAsync(Frame(BitConverter.GetBytes((ushort) ChatProtocol.Bidirectional.NET_CHAT_PING)));

        byte[] header = await ReadExactly(stream, 2);
        ushort length = BitConverter.ToUInt16(header, 0);
        byte[] payload = await ReadExactly(stream, length);

        await Assert.That(BitConverter.ToUInt16(payload, 0)).IsEqualTo((ushort) ChatProtocol.Bidirectional.NET_CHAT_PONG);

        await application.StopAsync();
    }

    private static byte[] Frame(byte[] payload)
    {
        byte[] frame = new byte[2 + payload.Length];

        BitConverter.GetBytes((ushort) payload.Length).CopyTo(frame, 0);

        payload.CopyTo(frame, 2);

        return frame;
    }

    private static async Task<byte[]> ReadExactly(NetworkStream stream, int count)
    {
        byte[] buffer = new byte[count];
        int offset = 0;

        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(5));

        while (offset < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset), timeout.Token);

            if (read is 0)
                throw new InvalidOperationException("The Connection Closed Before The Expected Bytes Arrived");

            offset += read;
        }

        return buffer;
    }

    private static int GetFreeTCPPort()
    {
        TcpListener listener = new (IPAddress.Loopback, 0);

        listener.Start();

        int port = ((IPEndPoint) listener.LocalEndpoint).Port;

        listener.Stop();

        return port;
    }
}
