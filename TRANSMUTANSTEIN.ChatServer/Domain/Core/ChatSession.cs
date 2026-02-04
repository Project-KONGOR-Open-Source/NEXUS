namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

using global::TRANSMUTANSTEIN.ChatServer.Internals;


public partial class ChatSession : TCPSession
{
    // Primary Constructor logic moved to explicit constructor to support base class call
    public ChatSession(TCPServer server, IServiceProvider serviceProvider) : base(server, serviceProvider.GetRequiredService<ILogger<ChatSession>>())
    {
        HostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        _serviceProvider = serviceProvider;
        _chatContext = serviceProvider.GetRequiredService<IChatContext>();
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly IChatContext _chatContext;

    /// <summary>
    ///     Gets the current hosting environment information for the web application.
    /// </summary>
    /// <remarks>
    ///     Use this property to access environment-specific settings, such as the application name, the content root path, or
    ///     the environment name.
    /// </remarks>
    public IWebHostEnvironment HostEnvironment { get; init; }

    /// <summary>
    ///     Gets set after a successful handshake (Client or Server).
    ///     Contains the account information of the entity connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after Accept is called.
    /// </summary>
    public Account Account { get; set; } = null!;

    private static ConcurrentDictionary<ushort, Type> CommandToCommandTypeMap { get; } = [];

    private byte[] RemainingPreviouslyReceivedData { get; set; } = [];

    [LoggerMessage(Level = LogLevel.Information, Message = "Chat Session ID {SessionID} Was Created")]
    private partial void LogSessionCreated(Guid sessionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Chat Session ID {SessionID} Caught A Socket Error With Code {SocketErrorCode}")]
    private partial void LogSocketError(Guid sessionId, SocketError socketErrorCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "Chat Session ID {SessionID} Has Terminated")]
    private partial void LogSessionTerminated(Guid sessionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[RAW] Received {Length} bytes: {BytesHex}")]
    private partial void LogRawReceived(int length, string bytesHex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error in OnReceived")]
    private partial void LogReceiveError(Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Missing Type Mapping For Command: \"0x{Command}\"\nMessage UTF8 Bytes: {Bytes}\nMessage UTF8 Text: {Text}")]
    private partial void LogMissingCommandType(string command, string bytes, string text);

    [LoggerMessage(Level = LogLevel.Information, Message = "[IN] Processing Command: \"0x{Command}\" From Session {SessionID}")]
    private partial void LogProcessingCommand(string command, Guid sessionId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Dispatching Command Type: {TypeName}")]
    private partial void LogDispatchingCommand(string typeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Synchronous Instance Type: {TypeName}")]
    private partial void LogSyncInstanceType(string typeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Invoking SYNC Process Method on {TypeName}")]
    private partial void LogInvokeSync(string typeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Async Instance Type: {TypeName}")]
    private partial void LogAsyncInstanceType(string typeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Invoking ASYNC Process Method on {TypeName}")]
    private partial void LogInvokeAsync(string typeName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CRITICAL] Process Method NOT FOUND on interface {InterfaceName}")]
    private partial void LogProcessMethodNotFound(string interfaceName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[CRITICAL] IAsynchronousCommandProcessor Interface NOT FOUND on {TypeName}")]
    private partial void LogAsyncInterfaceNotFound(string typeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Command Processing Completed")]
    private partial void LogProcessingCompleted();

    [LoggerMessage(Level = LogLevel.Information, Message = "[DEBUG] Packet 0x{Command} Payload ({Size} bytes): {Payload}")]
    private partial void LogPacketPayload(string command, int size, string payload);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Could Not Create Command Type Instance For Command: \"0x{Command}\"")]
    private partial void LogCreateInstanceError(string command);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Error Processing Command: \"0x{Command}\"")]
    private partial void LogProcessingError(Exception ex, string command);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Could Not Add Command-To-Type Mapping For Command \"0x{Command}\" And Type \"{TypeName}\"")]
    private partial void LogMappingError(string command, string typeName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Command Type \"{TypeName}\" Does Not Implement A Supported Processor Interface")]
    private partial void LogInterfaceError(string typeName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[OUT] Sending Command: \"0x{Command}\" ({Size} bytes) To Session {SessionID}")]
    private partial void LogSendingCommand(string command, int size, Guid sessionId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Packet Of {PacketSize} Bytes Exceeds Maximum Allowed Size Of {MaximumPacketSize} Bytes")]
    private partial void LogPacketSizeError(int packetSize, int maximumPacketSize);

    protected override void OnConnected()
    {
        LogSessionCreated(ID);
    }

    protected override void OnError(SocketError error)
    {
        LogSocketError(ID, error);
    }

    protected override void OnDisconnected()
    {
        Cleanup();
        LogSessionTerminated(ID);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        try
        {
            byte[] raw = buffer[(int) offset..(int) size];
            LogRawReceived(raw.Length, BitConverter.ToString(raw).Replace("-", " "));

            byte[] received = RemainingPreviouslyReceivedData.Concat(raw).ToArray();
            List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

            RemainingPreviouslyReceivedData = remaining;

            foreach (byte[] segment in segments)
            {
                ProcessDataSegment(segment).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            LogReceiveError(ex);
        }
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
                segments.Add(buffer[(offset + 2)..(offset + 2 + size)]);
                offset += 2 + size;
            }
            else
            {
                remaining = buffer[offset..];
                offset = buffer.Length;
            }
        }

        return segments;
    }

    private async Task ProcessDataSegment(byte[] segment)
    {
        ushort command = BitConverter.ToUInt16([segment[0], segment[1]]);
        Type? commandType = GetCommandType(command);

        if (commandType is null)
        {
            LogMissingCommandType(command.ToString("X4"), string.Join(':', segment), Encoding.UTF8.GetString(segment));
            return;
        }

        LogProcessingCommand(command.ToString("X4"), ID);

        // Create a scope for the command execution to allow resolving Scoped services (like DbContext)
        using IServiceScope scope = _serviceProvider.CreateScope();

        try
        {
            if (GetCommandTypeInstance(scope.ServiceProvider, commandType) is { } commandTypeInstance)
            {
                ChatBuffer buffer = new(segment);
                LogDispatchingCommand(commandTypeInstance.Value.GetType().Name);

                // Use Match to return a Task from both branches so we can await completion
                Task processingTask = commandTypeInstance.Match(
                    synchronousInstance =>
                    {
                        Type instanceType = synchronousInstance.GetType();
                        LogSyncInstanceType(instanceType.Name);

                        Type? interfaceType = instanceType.GetInterfaces()
                            .SingleOrDefault(interfaceType =>
                                interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() ==
                                typeof(ISynchronousCommandProcessor<>));

                        if (interfaceType is not null)
                        {
                            MethodInfo? processMethod = interfaceType.GetMethod("Process");
                            LogInvokeSync(instanceType.Name);
                            processMethod?.Invoke(synchronousInstance, [this, buffer]);
                        }

                        return Task.CompletedTask;
                    },
                    asynchronousInstance =>
                    {
                        Type instanceType = asynchronousInstance.GetType();
                        LogAsyncInstanceType(instanceType.Name);

                        Type? interfaceType = instanceType.GetInterfaces()
                            .SingleOrDefault(interfaceType =>
                                interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() ==
                                typeof(IAsynchronousCommandProcessor<>));

                        if (interfaceType is not null)
                        {
                            MethodInfo? processMethod = interfaceType.GetMethod("Process");
                            if (processMethod is not null)
                            {
                                LogInvokeAsync(instanceType.Name);
                                return (Task?) processMethod.Invoke(asynchronousInstance, [this, buffer]) ??
                                       Task.CompletedTask;
                            }

                            LogProcessMethodNotFound(interfaceType.Name);
                            return Task.CompletedTask;
                        }

                        LogAsyncInterfaceNotFound(instanceType.Name);
                        return Task.CompletedTask;
                    }
                );

                await processingTask;
                LogProcessingCompleted();

                // User Requested Full Hex Dump For All Packets
                LogPacketPayload(
                    $"0x{command:X4}",
                    (int) buffer.Size,
                    BitConverter.ToString(buffer.Data.ToArray()).Replace("-", " "));
            }
            else
            {
                LogCreateInstanceError(command.ToString("X4"));
            }
        }
        catch (Exception exception)
        {
            LogProcessingError(exception, command.ToString("X4"));
        }
    }

    private Type? GetCommandType(ushort command)
    {
        if (CommandToCommandTypeMap.TryGetValue(command, out Type? type))
        {
            return type;
        }

        Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

        type = types
            .SingleOrDefault(t =>
                t.GetCustomAttribute<ChatCommandAttribute>() is not null &&
                (t.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is null)
        {
            // Log all available commands to debug why it was not found
            List<string> availableCommands = types
                .Where(t => t.GetCustomAttribute<ChatCommandAttribute>() is not null)
                .Select(t =>
                {
                    ChatCommandAttribute? attr = t.GetCustomAttribute<ChatCommandAttribute>();
                    return $"{t.Name} (0x{attr?.Command:X4})";
                })
                .OrderBy(x => x)
                .ToList();

            Log.Warning("Command Lookup Failed for 0x{Command:X4}. Available Commands:\n{AvailableCommands}", 
                command, string.Join("\n", availableCommands));
        }

        if (type is not null)
        {
            if (CommandToCommandTypeMap.TryAdd(command, type) is false &&
                CommandToCommandTypeMap.ContainsKey(command) is false)
            {
                LogMappingError(command.ToString("X4"), type.Name);
            }
        }

        return type;
    }

    private OneOf<object, object>? GetCommandTypeInstance(IServiceProvider scopeProvider, Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(scopeProvider, type);

        Type? syncInterface = type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType &&
                                              interfaceType.GetGenericTypeDefinition() ==
                                              typeof(ISynchronousCommandProcessor<>));

        if (syncInterface is not null)
        {
            return OneOf<object, object>.FromT0(instance);
        }

        Type? asyncInterface = type.GetInterfaces()
            .SingleOrDefault(interfaceType => interfaceType.IsGenericType &&
                                              interfaceType.GetGenericTypeDefinition() ==
                                              typeof(IAsynchronousCommandProcessor<>));

        if (asyncInterface is not null)
        {
            return OneOf<object, object>.FromT1(instance);
        }

        LogInterfaceError(type.Name);

        return null;
    }

    /// <summary>
    ///     Send buffer data with non-destructive size prefixing, which does not mutate the original buffer.
    ///     Enforces 16KB (16384 bytes) maximum packet size, as to not exceed the chat protocol's limitations.
    /// </summary>
    public bool Send(ChatBuffer buffer)
    {
        // Debug Logging for Outgoing Packets
        if (buffer.Data.Length >= 2)
        {
            ushort cmd = BitConverter.ToUInt16([buffer.Data[0], buffer.Data[1]]);

            try
            {
                LogPacketPayload(cmd.ToString("X4"), (int) buffer.Size, BitConverter.ToString(buffer.Data.ToArray()).Replace("-", " "));
                LogSendingCommand(cmd.ToString("X4"), (int) buffer.Size, ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Error Logging Outgoing Packet: {ex}");
            }
        }

        if (buffer.Size > ChatProtocol.MAX_PACKET_SIZE)
        {
            LogPacketSizeError((int) buffer.Size, (int) ChatProtocol.MAX_PACKET_SIZE);

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