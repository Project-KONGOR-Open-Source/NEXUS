namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
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

    private static ConcurrentDictionary<ushort, Type> CommandToCommandTypeMap { get; set; } = [];

    private byte[] RemainingPreviouslyReceivedData { get; set; } = [];

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

    protected override void OnConnected()
    {
        ConnectedAt = DateTimeOffset.UtcNow;

        Logger = Log.ForContext(GetType())
            .ForContext("Session.ID", ID)
            .ForContext("Remote.EndPoint", Socket.RemoteEndPoint?.ToString());

        Logger.Information("Chat Session {Session.ID} Connected From {Remote.EndPoint}", ID, Socket.RemoteEndPoint?.ToString());
    }

    protected override void OnError(SocketError error)
    {
        Logger.Information("Chat Session {Session.ID} Caught Socket Error {Socket.ErrorCode} After {Bytes.Received} Bytes Received / {Bytes.Sent} Bytes Sent", ID, error, BytesReceived, BytesSent);
    }

    protected override void OnDisconnected()
    {
        TimeSpan sessionDuration = DateTimeOffset.UtcNow - ConnectedAt;

        string lastProcessedCommand = LastProcessedCommand is { } command ? $"0x{command:X4}" : "(NONE)";

        Logger.Information("Chat Session {Session.ID} Disconnected After {Session.DurationSeconds:F1}s; {Bytes.Received} Bytes Received / {Bytes.Sent} Bytes Sent; Last Command Processed Was {LastProcessedCommand}",
            ID, sessionDuration.TotalSeconds, BytesReceived, BytesSent, lastProcessedCommand);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = RemainingPreviouslyReceivedData.Concat(buffer[(int) offset..(int) size]).ToArray();

        List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

        RemainingPreviouslyReceivedData = remaining;

        foreach (byte[] segment in segments)
            ProcessDataSegment(segment);
    }

    private static List<byte[]> ExtractDataSegments(byte[] buffer, out byte[] remaining)
    {
        remaining = [];

        List<byte[]> segments = [];

        int offset = 0;

        while (offset < buffer.Length)
        {
            ushort size = BitConverter.ToUInt16([buffer[offset + 0], buffer[offset + 1]]);

            if (size + 2 <= buffer.Length - offset)
            {
                byte[] segment = buffer[(offset + 2)..(offset + 2 + size)];

                segments.Add(segment);

                offset += 2 + size;
            }

            else
            {
                remaining = buffer[offset..buffer.Length];

                offset = buffer.Length;
            }
        }

        return segments;
    }

    private void ProcessDataSegment(byte[] segment)
    {
        ushort command = BitConverter.ToUInt16([segment[0], segment[1]]);

        LastProcessedCommand = command;

        // Attach The Session And Command Context To Every Log Event Raised While This Command Is Handled, Including Those Emitted By The Command Processors Themselves
        // These Properties Flow Through The Ambient Log Context, So They Are Also Captured By Asynchronous Command Processors Whose Continuations Run After This Method Returns

        using IDisposable sessionScope = LogContext.PushProperty("Session.ID", ID);
        using IDisposable accountIDScope = LogContext.PushProperty("Account.ID", LoggingAccountID);
        using IDisposable accountNameScope = LogContext.PushProperty("Account.Name", LoggingAccountName);
        using IDisposable commandScope = LogContext.PushProperty("Command", $"0x{command:X4}");
        using IDisposable correlationScope = LogContext.PushProperty("Command.CorrelationID", Guid.CreateVersion7());

        Type? commandType = GetCommandType(command);

        using IDisposable commandNameScope = LogContext.PushProperty("Command.Name", commandType?.Name);

        if (commandType is null)
        {
            Log.Error("Missing Type Mapping For Command {Command}; Payload Was {Payload.Length} Bytes: {Payload.Hex} (UTF-8 Text: {Payload.Text})",
                $"0x{command:X4}", segment.Length, segment.ToHexString(), Encoding.UTF8.GetString(segment));
        }

        else
        {
            if (HostEnvironment.IsDevelopment())
                Log.Debug("Processing Command {Command} ({Command.Name})", $"0x{command:X4}", commandType.Name);

            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
            {
                try
                {
                    ChatBuffer buffer = new (segment);

                    commandTypeInstance.Switch
                    (
                        synchronousInstance =>
                        {
                            Type instanceType = synchronousInstance.GetType();

                            Type? interfaceType = instanceType.GetInterfaces()
                                .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ISynchronousCommandProcessor<>));

                            if (interfaceType is not null)
                            {
                                MethodInfo? processMethod = interfaceType.GetMethod("Process");

                                processMethod?.Invoke(synchronousInstance, [this, buffer]);
                            }
                        },

                        async asynchronousInstance =>
                        {
                            Type instanceType = asynchronousInstance.GetType();

                            Type? interfaceType = instanceType.GetInterfaces()
                                .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IAsynchronousCommandProcessor<>));

                            if (interfaceType is not null)
                            {
                                MethodInfo? processMethod = interfaceType.GetMethod("Process");

                                if (processMethod is not null)
                                {
                                    Task? task = processMethod.Invoke(asynchronousInstance, [this, buffer]) as Task;

                                    if (task is not null) await task;
                                }
                            }
                        }
                    );
                }

                catch (Exception exception)
                {
                    Log.Error(exception, @"[BUG] Unhandled Exception Processing Command {Command} ({Command.Name}); Payload {Payload.Length} Bytes: {Payload.Hex}",
                        $"0x{command:X4}", commandType.Name, segment.Length, segment.ToHexString());
                }
            }

            else Log.Error(@"[BUG] Could Not Create Command Type Instance For Command {Command} ({Command.Name})", $"0x{command:X4}", commandType.Name);
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

    private OneOf<object, object>? GetCommandTypeInstance(Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

        Type? syncInterface = type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ISynchronousCommandProcessor<>));

        if (syncInterface is not null)
            return OneOf<object, object>.FromT0(instance);

        Type? asyncInterface = type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IAsynchronousCommandProcessor<>));

        if (asyncInterface is not null)
            return OneOf<object, object>.FromT1(instance);

        Log.Error(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

        return null;
    }

    /// <summary>
    ///     Send buffer data with non-destructive size prefixing, which does not mutate the original buffer.
    ///     Enforces 16KB (16384 bytes) maximum packet size, as to not exceed the chat protocol's limitations.
    /// </summary>
    public bool Send(ChatBuffer buffer)
    {
        if (buffer.Size > ChatProtocol.MAX_PACKET_SIZE)
        {
            Logger.Error("Outbound Packet Of {Packet.Size} Bytes Exceeds The Maximum Allowed Size Of {Packet.MaximumSize} Bytes And Will Not Be Sent", buffer.Size, ChatProtocol.MAX_PACKET_SIZE);

            return false;
        }

        short messageLength = Convert.ToInt16(buffer.Size);

        byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
        byte[] messageBytes = new byte[messageLengthBytes.Length + messageLength];

        Array.Copy(messageLengthBytes, 0, messageBytes, 0, messageLengthBytes.Length);
        Array.Copy(buffer.Data, 0, messageBytes, messageLengthBytes.Length, messageLength);

        return SendAsync(messageBytes);
    }
}
