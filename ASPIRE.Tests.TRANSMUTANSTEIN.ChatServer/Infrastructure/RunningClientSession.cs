namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

/// <summary>
///     Drives a real <see cref="ClientChatSession"/> over a pair of in-memory pipes, standing in for the duplex pipe that Kestrel would otherwise supply, so that a test can invoke channel operations directly and read the frames the session sends back to the client.
/// </summary>
internal sealed class RunningClientSession : IAsyncDisposable
{
    public ClientChatSession Session { get; }

    private WebApplication Host { get; }

    private Pipe Inbound { get; }

    private Pipe Outbound { get; }

    private Task SessionTask { get; }

    private RunningClientSession(ClientChatSession session, WebApplication host, Pipe inbound, Pipe outbound, Task sessionTask)
    {
        Session = session;
        Host = host;
        Inbound = inbound;
        Outbound = outbound;
        SessionTask = sessionTask;
    }

    public static RunningClientSession Start(Account account)
    {
        WebApplication host = WebApplication.CreateSlimBuilder().Build();

        // The Chat Server's Static Logger Facade Throws Until Initialised; A Sink-Less Serilog Logger Satisfies It Without Producing Output
        Log.Initialise(new Serilog.LoggerConfiguration().CreateLogger());

        Pipe inbound = new ();
        Pipe outbound = new ();

        DefaultConnectionContext connection = new ()
        {
            Transport = new InMemoryDuplexPipe(inbound.Reader, outbound.Writer)
        };

        // The Metadata Stands In For What A Completed Client Handshake Would Record, Which Channel Operations Read When Describing The Session To Channel Members
        ClientChatSession session = new (connection, host.Services)
        {
            Account = account,
            Metadata = (ClientChatSessionMetadata) RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSessionMetadata))
        };

        Task sessionTask = session.RunAsync();

        return new RunningClientSession(session, host, inbound, outbound, sessionTask);
    }

    /// <summary>
    ///     Reads the next frame the session sends and returns its command code and the message string a <see cref="ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL"/> notice carries.
    /// </summary>
    public async Task<(ushort Command, string Message)> ReadNotice()
    {
        byte[] payload = await ReadFrame();

        ushort command = BitConverter.ToUInt16(payload, 0);

        // A Notice Frame Is The Command Followed By Two Null-Terminated UTF-8 Strings: The Sender Name And The Message
        List<string> strings = ParseNullTerminatedStrings(payload, offset: 2);

        string message = strings.Count > 1 ? strings[1] : string.Empty;

        return (command, message);
    }

    /// <summary>
    ///     Reads the next frame the session sends and returns its command code.
    /// </summary>
    public async Task<ushort> ReadCommand()
        => BitConverter.ToUInt16(await ReadFrame(), 0);

    private async Task<byte[]> ReadFrame()
    {
        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(5));

        while (true)
        {
            ReadResult result = await Outbound.Reader.ReadAsync(timeout.Token);
            ReadOnlySequence<byte> buffer = result.Buffer;

            if (buffer.Length >= 2)
            {
                ushort length = BitConverter.ToUInt16(buffer.Slice(0, 2).ToArray(), 0);

                if (buffer.Length >= 2 + length)
                {
                    byte[] payload = buffer.Slice(2, length).ToArray();

                    Outbound.Reader.AdvanceTo(buffer.GetPosition(2 + length));

                    return payload;
                }
            }

            Outbound.Reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                throw new InvalidOperationException("The Connection Closed Before A Complete Frame Was Received");
        }
    }

    private static List<string> ParseNullTerminatedStrings(byte[] payload, int offset)
    {
        List<string> strings = [];

        int start = offset;

        for (int index = offset; index < payload.Length; index++)
        {
            if (payload[index] is not 0)
                continue;

            strings.Add(System.Text.Encoding.UTF8.GetString(payload, start, index - start));

            start = index + 1;
        }

        return strings;
    }

    public async ValueTask DisposeAsync()
    {
        await Inbound.Writer.CompleteAsync();

        try { await SessionTask.WaitAsync(TimeSpan.FromSeconds(5)); }

        catch { }

        await Host.DisposeAsync();
    }
}

internal sealed class InMemoryDuplexPipe(PipeReader input, PipeWriter output) : IDuplexPipe
{
    public PipeReader Input { get; } = input;

    public PipeWriter Output { get; } = output;
}
