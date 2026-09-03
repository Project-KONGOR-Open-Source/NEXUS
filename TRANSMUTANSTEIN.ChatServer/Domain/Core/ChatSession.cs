namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

/// <summary>
///     Reads and writes the chat protocol over a single ASP.NET Core connection.
///     The connection's lifetime is driven by <see cref="RunAsync"/>, which the owning connection handler invokes once per accepted connection.
/// </summary>
/// <remarks>Thread-safe with respect to <see cref="Send"/>, which may be called concurrently from any thread.</remarks>
public class ChatSession(ConnectionContext connection, IServiceProvider serviceProvider)
{
    /// <summary>
    ///     The underlying ASP.NET Core connection that this session reads from and writes to.
    /// </summary>
    protected ConnectionContext Connection { get; } = connection;

    /// <summary>
    ///     Gets the application root service provider associated with the chat server host.
    ///     Used by derived sessions when they need to resolve scoped services from outside a request/command scope (e.g. fire-and-forget work on the disconnect path).
    /// </summary>
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    ///     Gets the current hosting environment information for the web application.
    /// </summary>
    /// <remarks>
    ///     Use this property to access environment-specific settings, such as the application name, the content root path, or the environment name.
    /// </remarks>
    public IWebHostEnvironment HostEnvironment { get; init; } = serviceProvider.GetRequiredService<IWebHostEnvironment>();

    /// <summary>
    ///     The session identifier, generated independently of the connection so that every session-level log event carries a stable identity.
    /// </summary>
    public Guid ID { get; } = Guid.CreateVersion7();

    /// <summary>
    ///     The remote endpoint of the connected peer, or <see langword="null"/> if it is not available.
    /// </summary>
    public EndPoint? RemoteEndPoint => Connection.RemoteEndPoint;

    /// <summary>
    ///     Indicates whether the session is still connected.
    /// </summary>
    public bool IsConnected { get; private set; } = true;

    private static ConcurrentDictionary<ushort, Type> CommandToCommandTypeMap { get; set; } = [];

    /// <summary>
    ///     The session-scoped logger, enriched with the session identifier (and, once authenticated, the account) so that every session-level log event carries that context.
    /// </summary>
    protected ILogger Logger { get; set; } = Log.ForContext<ChatSession>();

    /// <summary>
    ///     The instant at which the session connected, used to report the session's lifetime when it disconnects.
    /// </summary>
    protected DateTimeOffset ConnectedAt { get; private set; }

    /// <summary>
    ///     The most recent command processed for this session, reported when the session disconnects to aid post-mortem diagnosis.
    /// </summary>
    private ushort? LastProcessedCommand { get; set; }

    /// <summary>
    ///     The identifier of the account associated with this session, or <see langword="null"/> if the session is not authenticated.
    ///     It is pushed into the ambient log context for the duration of each command, so that log events raised by the command processors are attributed to the account.
    /// </summary>
    protected virtual int? LoggingAccountID => null;

    /// <summary>
    ///     The name of the account associated with this session, or <see langword="null"/> if the session is not authenticated.
    ///     It is pushed into the ambient log context for the duration of each command, so that log events raised by the command processors are attributed to the account.
    /// </summary>
    protected virtual string? LoggingAccountName => null;

    /// <summary>
    ///     Outbound frames are funnelled through a single channel that one writer pump drains to the connection.
    ///     This preserves strict per-connection send order, while letting <see cref="Send"/> stay a synchronous, thread-safe, fire-and-forget call from any thread.
    /// </summary>
    private readonly Channel<byte[]> OutboundFrames = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    private Task WritePump { get; set; } = Task.CompletedTask;

    // Cancelled To End The Read Loop On A Server-Initiated Teardown Without Aborting The Connection, So A Graceful Close Can Send A FIN Rather Than An Abortive Reset
    private readonly CancellationTokenSource ConnectionTeardown = new ();

    private int TeardownCompleted;

    /// <summary>
    ///     Drives the connection for its entire lifetime.
    ///     ASP.NET Core invokes this once per accepted connection and tears the connection down once the returned task completes.
    /// </summary>
    public async Task RunAsync()
    {
        ApplySocketOptions();

        OnConnected();

        WritePump = WritePumpAsync();

        try
        {
            await ReadLoop();
        }

        catch (Exception exception) when (IsBenignTransportError(exception)) { }

        catch (Exception exception)
        {
            Logger.Error(exception, "Chat Session {SessionID} Encountered An Unexpected Error", ID);
        }

        finally
        {
            // Stop The Writer Pump And Let It Drain Whatever Has Already Been Queued, Then Run The Disconnect Cleanup Exactly Once
            OutboundFrames.Writer.TryComplete();

            try { await WritePump; }
            catch { }

            Teardown();
        }
    }

    /// <summary>
    ///     Reads from the connection until it closes, isolating complete frames from the inbound byte stream.
    /// </summary>
    private async Task ReadLoop()
    {
        PipeReader input = Connection.Transport.Input;

        while (true)
        {
            ReadResult readResult = await input.ReadAsync(ConnectionTeardown.Token);

            if (readResult.IsCanceled)
                break;

            ReadOnlySequence<byte> buffer = readResult.Buffer;

            List<byte[]> segments = ExtractFrames(buffer, out SequencePosition consumed);

            // Release The Consumed Bytes Before Dispatching, Because The Segments Are Independent Copies; Any Trailing Partial Frame Is Retained By The Reader Until More Data Arrives
            input.AdvanceTo(consumed, buffer.End);

            foreach (byte[] segment in segments)
            {
                // A Command Processor May Tear The Session Down (e.g. A Rejected Handshake Or A Concurrent-Connection Takeover); Once That Happens, The Remaining Frames In This Batch Must Not Be Processed
                if (ConnectionTeardown.IsCancellationRequested)
                    break;

                await ProcessDataSegment(segment);
            }

            // Stop Reading Once The Session Has Been Torn Down Or The Peer Has Closed Its End
            if (ConnectionTeardown.IsCancellationRequested || readResult.IsCompleted)
                break;
        }
    }

    /// <summary>
    ///     Isolates complete length-prefixed frames from the buffered input, returning each one as an independent byte array, and reports the position up to which the input was consumed so that any trailing partial frame is retained until more data arrives.
    /// </summary>
    private static List<byte[]> ExtractFrames(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
    {
        List<byte[]> segments = [];

        SequenceReader<byte> reader = new (buffer);

        while (true)
        {
            SequencePosition frameStart = reader.Position;

            // A Frame Is A Little-Endian 2-Byte Length Prefix Followed By That Many Payload Bytes; If Either The Prefix Or The Whole Payload Has Not Arrived Yet, Stop At The Start Of The Incomplete Frame
            if (reader.Remaining < 2)
            {
                consumed = frameStart;

                return segments;
            }

            reader.TryReadLittleEndian(out short signedLength);

            ushort length = unchecked((ushort) signedLength);

            if (reader.Remaining < length)
            {
                consumed = frameStart;

                return segments;
            }

            byte[] segment = new byte[length];

            buffer.Slice(reader.Position, length).CopyTo(segment);

            reader.Advance(length);

            segments.Add(segment);
        }
    }

    private async Task ProcessDataSegment(byte[] segment)
    {
        ushort command = BitConverter.ToUInt16([segment[0], segment[1]]);

        LastProcessedCommand = command;

        // Attach The Session And Command Context To Every Log Event Raised While This Command Is Handled, Including Those Emitted By The Command Processors Themselves
        // These Properties Flow Through The Ambient Log Context, So They Are Also Captured By Asynchronous Command Processors Whose Continuations Run After Their First Await

        using IDisposable sessionScope = LogContext.PushProperty("SessionID", ID);
        using IDisposable accountIDScope = LogContext.PushProperty("AccountID", LoggingAccountID);
        using IDisposable accountNameScope = LogContext.PushProperty("AccountName", LoggingAccountName);
        using IDisposable commandScope = LogContext.PushProperty("Command", $"0x{command:X4}");
        using IDisposable correlationScope = LogContext.PushProperty("CommandCorrelationID", Guid.CreateVersion7());

        Type? commandType = GetCommandType(command);

        using IDisposable commandNameScope = LogContext.PushProperty("CommandName", commandType?.Name);

        if (commandType is null)
        {
            Log.Error("Missing Type Mapping For Command {Command}; Payload Was {PayloadLength} Bytes: {PayloadHex} (UTF-8 Text: {PayloadText})",
                $"0x{command:X4}", segment.Length, segment.ToHexString(), Encoding.UTF8.GetString(segment));
        }

        else
        {
            if (HostEnvironment.IsDevelopment())
                Log.Debug("Processing Command {Command} ({CommandName})", $"0x{command:X4}", commandType.Name);

            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
            {
                try
                {
                    ChatBuffer buffer = new (segment);

                    switch (commandTypeInstance)
                    {
                        case SynchronousCommandProcessorInstance synchronousProcessor:
                            InvokeSynchronousProcessor(synchronousProcessor.Instance, buffer);
                            break;

                        case AsynchronousCommandProcessorInstance asynchronousProcessor:
                            await InvokeAsynchronousProcessor(asynchronousProcessor.Instance, buffer);
                            break;
                    }
                }

                catch (Exception exception)
                {
                    Log.Error(exception, @"[BUG] Unhandled Exception Processing Command {Command} ({CommandName}); Payload {PayloadLength} Bytes: {PayloadHex}",
                        $"0x{command:X4}", commandType.Name, segment.Length, segment.ToHexString());
                }
            }

            else Log.Error(@"[BUG] Could Not Create Command Type Instance For Command {Command} ({CommandName})", $"0x{command:X4}", commandType.Name);
        }
    }

    private void InvokeSynchronousProcessor(object instance, ChatBuffer buffer)
    {
        Type? interfaceType = instance.GetType().GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ISynchronousCommandProcessor<>));

        if (interfaceType is not null)
        {
            MethodInfo? processMethod = interfaceType.GetMethod("Process");

            processMethod?.Invoke(instance, [this, buffer]);
        }
    }

    private async Task InvokeAsynchronousProcessor(object instance, ChatBuffer buffer)
    {
        Type? interfaceType = instance.GetType().GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IAsynchronousCommandProcessor<>));

        if (interfaceType is not null)
        {
            MethodInfo? processMethod = interfaceType.GetMethod("Process");

            if (processMethod is not null && processMethod.Invoke(instance, [this, buffer]) is Task task)
                await task;
        }
    }

    private Type? GetCommandType(ushort command)
    {
        if (CommandToCommandTypeMap.TryGetValue(command, out Type? type)) return type;

        Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

        type = types
            .SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is not null)
            if (CommandToCommandTypeMap.TryAdd(command, type) is false && CommandToCommandTypeMap.ContainsKey(command) is false)
                Log.Error(@"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""", command.ToString("X4"), type.Name);

        return type;
    }

    private CommandProcessorInstance? GetCommandTypeInstance(Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

        Type? syncInterface = type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ISynchronousCommandProcessor<>));

        if (syncInterface is not null)
            return new SynchronousCommandProcessorInstance(instance);

        Type? asyncInterface = type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IAsynchronousCommandProcessor<>));

        if (asyncInterface is not null)
            return new AsynchronousCommandProcessorInstance(instance);

        Log.Error(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

        return null;
    }

    /// <summary>
    ///     Send buffer data with non-destructive size prefixing, which does not mutate the original buffer.
    ///     Enforces 16KB (16384 bytes) maximum packet size, as to not exceed the chat protocol's limitations.
    ///     The frame is enqueued for in-order transmission and the method returns immediately. The returned value indicates whether the frame was enqueued, not whether it has reached the wire.
    /// </summary>
    public bool Send(ChatBuffer buffer)
    {
        if (buffer.Size + 2 /* The Buffer Size Prefix */ > ChatProtocol.MAX_PACKET_SIZE)
        {
            Logger.Error("Outbound Packet Of {PacketSize} Bytes Exceeds The Maximum Allowed Size Of {PacketMaximumSize} Bytes And Will Not Be Sent", buffer.Size, ChatProtocol.MAX_PACKET_SIZE);

            return false;
        }

        short messageLength = Convert.ToInt16(buffer.Size);

        byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
        byte[] messageBytes = new byte[messageLengthBytes.Length + messageLength];

        Array.Copy(messageLengthBytes, 0, messageBytes, 0, messageLengthBytes.Length);
        Array.Copy(buffer.Data, 0, messageBytes, messageLengthBytes.Length, messageLength);

        return OutboundFrames.Writer.TryWrite(messageBytes);
    }

    private async Task WritePumpAsync()
    {
        PipeWriter output = Connection.Transport.Output;

        try
        {
            await foreach (byte[] frame in OutboundFrames.Reader.ReadAllAsync(Connection.ConnectionClosed))
            {
                output.Write(frame);

                // Coalesce Any Frames That Are Already Queued Into A Single Flush
                while (OutboundFrames.Reader.TryRead(out byte[]? next))
                    output.Write(next);

                FlushResult flushResult = await output.FlushAsync(Connection.ConnectionClosed);

                if (flushResult.IsCompleted || flushResult.IsCanceled)
                    break;
            }
        }

        catch (Exception exception) when (IsBenignTransportError(exception)) { }
    }

    /// <summary>
    ///     Tears the connection down, unblocking the read loop and running the disconnect cleanup exactly once.
    ///     Outbound frames that have not yet been flushed are dropped; callers that need the final frame delivered should use <see cref="CloseGracefully"/> instead.
    /// </summary>
    public void Disconnect()
    {
        IsConnected = false;

        OutboundFrames.Writer.TryComplete();

        Connection.Abort();

        ConnectionTeardown.Cancel();
    }

    /// <summary>
    ///     Tears the connection down after draining and flushing any queued outbound frames, so that the final frame (e.g. a "quit" remote command) reaches the peer before the socket is closed.
    /// </summary>
    protected async Task CloseGracefully()
    {
        IsConnected = false;

        OutboundFrames.Writer.TryComplete();

        // Allow The Writer Pump To Flush The Queued Frames, But Do Not Wait Indefinitely On A Peer That Has Stopped Reading
        try { await WritePump.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }

        // Complete The Output To Flush The Queued Frames And Send A FIN, So The Peer Receives Them In Full; An Abortive Reset Could Otherwise Discard Data It Has Not Yet Read
        try { await Connection.Transport.Output.CompleteAsync(); } catch { }

        // End The Read Loop Without Aborting, So The Handler Returns And Kestrel Performs An Orderly Close After The Flushed Frames Rather Than An Abortive Reset
        ConnectionTeardown.Cancel();
    }

    private void Teardown()
    {
        // Guarantee The Disconnect Cleanup Runs At Most Once, Whether It Was Triggered By The Read Loop Ending, A Send Failing, Or Business Logic Requesting The Disconnect
        if (Interlocked.Exchange(ref TeardownCompleted, 1) is 1)
            return;

        IsConnected = false;

        OnDisconnected();
    }

    private void ApplySocketOptions()
    {
        if (Connection.Features.Get<IConnectionSocketFeature>()?.Socket is Socket socket)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
        }
    }

    /// <summary>
    ///     Determines whether an exception raised by the connection's read or write path is a benign consequence of the connection ending, rather than a fault in the server.
    ///     A peer disconnecting, a keep-alive timing out, or the session being deliberately torn down (see <see cref="Disconnect"/> and <see cref="CloseGracefully"/>) all surface as one of these exception types while a read or write is in flight.
    ///     These are an expected part of the connection lifecycle, so the caller swallows them and proceeds to the normal teardown instead of logging them as errors.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item><see cref="OperationCanceledException"/>: the connection's <see cref="ConnectionContext.ConnectionClosed"/> token fired because the connection was aborted.</item>
    ///         <item><see cref="ConnectionResetException"/>: the peer reset the connection, for example because the client process exited.</item>
    ///         <item><see cref="ConnectionAbortedException"/>: the connection was aborted locally, for example during host shutdown.</item>
    ///         <item><see cref="IOException"/> and <see cref="SocketException"/>: the underlying socket failed because the connection dropped.</item>
    ///         <item><see cref="ObjectDisposedException"/>: the transport was disposed underneath an in-flight read or write while the connection was being torn down.</item>
    ///     </list>
    /// </remarks>
    private static bool IsBenignTransportError(Exception exception)
        => exception is OperationCanceledException or ConnectionResetException or ConnectionAbortedException or IOException or SocketException or ObjectDisposedException;

    /// <summary>
    ///     Handles the session being connected.
    /// </summary>
    protected virtual void OnConnected()
    {
        ConnectedAt = DateTimeOffset.UtcNow;

        Logger = Log.ForContext(GetType())
            .ForContext("SessionID", ID)
            .ForContext("RemoteEndPoint", RemoteEndPoint?.ToString());

        Logger.Information("Chat Session {SessionID} Connected From {RemoteEndPoint}", ID, RemoteEndPoint?.ToString());
    }

    /// <summary>
    ///     Handles the session being disconnected.
    /// </summary>
    protected virtual void OnDisconnected()
    {
        TimeSpan sessionDuration = DateTimeOffset.UtcNow - ConnectedAt;

        string lastProcessedCommand = LastProcessedCommand is { } command ? $"0x{command:X4}" : "(NONE)";

        Logger.Information("Chat Session {SessionID} Disconnected After {SessionDurationSeconds:F3}s; Last Command Processed Was {LastProcessedCommand}",
            ID, sessionDuration.TotalSeconds, lastProcessedCommand);
    }
}

/// <summary>
///     A command processor instance which implements <see cref="ISynchronousCommandProcessor{TSession}"/>.
/// </summary>
public sealed record SynchronousCommandProcessorInstance(object Instance);

/// <summary>
///     A command processor instance which implements <see cref="IAsynchronousCommandProcessor{TSession}"/>.
/// </summary>
public sealed record AsynchronousCommandProcessorInstance(object Instance);

/// <summary>
///     A command processor instance, which is either synchronous or asynchronous.
/// </summary>
public union CommandProcessorInstance(SynchronousCommandProcessorInstance, AsynchronousCommandProcessorInstance);
