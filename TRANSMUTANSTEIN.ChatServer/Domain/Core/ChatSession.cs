namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public partial class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    static ChatSession()
    {
        Console.WriteLine("[DEBUG] ChatSession Static Constructor");
    }

    /// <summary>
    ///     Gets the current hosting environment information for the web application.
    /// </summary>
    /// <remarks>
    ///     Use this property to access environment-specific settings, such as the application name, the content root path, or
    ///     the environment name.
    /// </remarks>
    public IWebHostEnvironment HostEnvironment { get; init; } =
        serviceProvider.GetRequiredService<IWebHostEnvironment>();

    // Explicit constructor removed to fix CS0111 (Primary Constructor conflict)

    /// <summary>
    ///     Gets set after a successful handshake (Client or Server).
    ///     Contains the account information of the entity connected to this chat session.
    ///     This property is NULL before authentication, but is guaranteed non-NULL after Accept is called.
    /// </summary>
    public Account Account { get; set; } = null!;

    private static ConcurrentDictionary<ushort, Type> CommandToCommandTypeMap { get; } = [];

    private byte[] RemainingPreviouslyReceivedData { get; set; } = [];

    protected override void OnConnected()
    {
        Log.Information("Chat Session ID {SessionID} Was Created", ID);
    }

    protected override void OnError(SocketError error)
    {
        Log.Information("Chat Session ID {SessionID} Caught A Socket Error With Code {SocketErrorCode}", ID, error);
    }

    protected override void OnDisconnected()
    {
        Log.Information("Chat Session ID {SessionID} Has Terminated", ID);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = RemainingPreviouslyReceivedData.Concat(buffer[(int) offset..(int) size]).ToArray();
        List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

        RemainingPreviouslyReceivedData = remaining;

        foreach (byte[] segment in segments)
        {
            ProcessDataSegment(segment).GetAwaiter().GetResult();
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
            string output = new StringBuilder($@"Missing Type Mapping For Command: ""0x{command:X4}""")
                .Append(Environment.NewLine).Append($"Message UTF8 Bytes: {string.Join(':', segment)}")
                .Append(Environment.NewLine).Append($"Message UTF8 Text: {Encoding.UTF8.GetString(segment)}")
                .ToString();
            Log.Error(output);
            return;
        }

        Log.Information(@"[IN] Processing Command: ""0x{Command}"" From Session {SessionID}", command.ToString("X4"),
            ID);

        // Create a scope for the command execution to allow resolving Scoped services (like DbContext)
        using IServiceScope scope = serviceProvider.CreateScope();

        try
        {
            if (GetCommandTypeInstance(scope.ServiceProvider, commandType) is { } commandTypeInstance)
            {
                ChatBuffer buffer = new(segment);
                Log.Information($"[DEBUG] Dispatching Command Type: {commandTypeInstance.Value.GetType().Name}");

                // Use Match to return a Task from both branches so we can await completion
                Task processingTask = commandTypeInstance.Match(
                    synchronousInstance =>
                    {
                        Type instanceType = synchronousInstance.GetType();
                        Log.Information($"[DEBUG] Synchronous Instance Type: {instanceType.Name}");

                        Type? interfaceType = instanceType.GetInterfaces()
                            .SingleOrDefault(interfaceType =>
                                interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() ==
                                typeof(ISynchronousCommandProcessor<>));

                        if (interfaceType is not null)
                        {
                            MethodInfo? processMethod = interfaceType.GetMethod("Process");
                            Log.Information($"[DEBUG] Invoking SYNC Process Method on {instanceType.Name}");
                            processMethod?.Invoke(synchronousInstance, [this, buffer]);
                        }

                        return Task.CompletedTask;
                    },
                    asynchronousInstance =>
                    {
                        Type instanceType = asynchronousInstance.GetType();
                        Log.Information($"[DEBUG] Async Instance Type: {instanceType.Name}");

                        Type? interfaceType = instanceType.GetInterfaces()
                            .SingleOrDefault(interfaceType =>
                                interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() ==
                                typeof(IAsynchronousCommandProcessor<>));

                        if (interfaceType is not null)
                        {
                            MethodInfo? processMethod = interfaceType.GetMethod("Process");
                            if (processMethod is not null)
                            {
                                Log.Information($"[DEBUG] Invoking ASYNC Process Method on {instanceType.Name}");
                                return (Task?) processMethod.Invoke(asynchronousInstance, [this, buffer]) ??
                                       Task.CompletedTask;
                            }

                            Log.Error($"[CRITICAL] Process Method NOT FOUND on interface {interfaceType.Name}");
                            return Task.CompletedTask;
                        }

                        Log.Error(
                            $"[CRITICAL] IAsynchronousCommandProcessor Interface NOT FOUND on {instanceType.Name}");
                        return Task.CompletedTask;
                    }
                );

                await processingTask;
                Log.Information("[DEBUG] Command Processing Completed");

                // User Requested Full Hex Dump For All Packets
                Log.Information("[DEBUG] Packet {Command} Payload ({Size} bytes): {Payload}",
                    $"0x{command:X4}",
                    buffer.Size,
                    BitConverter.ToString(buffer.Data.ToArray()).Replace("-", " "));
            }
            else
            {
                Log.Error(@"[BUG] Could Not Create Command Type Instance For Command: ""0x{Command}""",
                    command.ToString("X4"));
            }
        }
        catch (Exception exception)
        {
            Log.Error(exception, @"[BUG] Error Processing Command: ""0x{Command}""", command.ToString("X4"));
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
            .SingleOrDefault(type =>
                type.GetCustomAttribute<ChatCommandAttribute>() is not null &&
                (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is not null)
        {
            if (CommandToCommandTypeMap.TryAdd(command, type) is false &&
                CommandToCommandTypeMap.ContainsKey(command) is false)
            {
                Log.Error(
                    @"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""",
                    command.ToString("X4"), type.Name);
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

        Log.Error(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

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
                Log.Information("[DEBUG] Packet 0x{Command} Payload ({Size} bytes): {Payload}", cmd.ToString("X4"),
                    buffer.Size, BitConverter.ToString(buffer.Data.ToArray()).Replace("-", " "));
                Log.Information(@"[OUT] Sending Command: ""0x{Command}"" ({Size} bytes) To Session {SessionID}",
                    cmd.ToString("X4"), buffer.Size, ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Error Logging Outgoing Packet: {ex}");
            }
        }

        if (buffer.Size > ChatProtocol.MAX_PACKET_SIZE)
        {
            Log.Error(@"Packet Of {PacketSize} Bytes Exceeds Maximum Allowed Size Of {MaximumPacketSize} Bytes",
                buffer.Size, ChatProtocol.MAX_PACKET_SIZE);

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