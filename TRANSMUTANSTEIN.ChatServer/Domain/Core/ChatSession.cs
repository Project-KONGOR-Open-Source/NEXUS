namespace TRANSMUTANSTEIN.ChatServer.Domain.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    /// <summary>
    ///     Gets the current hosting environment information for the web application.
    /// </summary>
    /// <remarks>
    ///     Use this property to access environment-specific settings, such as the application name, the content root path, or the environment name.
    /// </remarks>
    public IWebHostEnvironment HostEnvironment { get; init; } = serviceProvider.GetRequiredService<IWebHostEnvironment>();

    private static ConcurrentDictionary<ushort, Type> CommandToCommandTypeMap { get; set; } = [];

    private static ConcurrentDictionary<Type, Type> CommandTypeToSessionTypeMap { get; set; } = [];

    /// <summary>
    ///     Gets or sets the polymorphic session instance created after a successful handshake.
    ///     This property is NULL for base sessions before handshake, and references the derived session instance (ClientChatSession, MatchServerChatSession, or MatchServerManagerChatSession) after handshake.
    ///     Commands are dispatched to the polymorphic session instead of the base session, when available.
    /// </summary>
    public ChatSession? Polymorph { get; set; }

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

        if (Polymorph is not null)
        {
            IDatabase distributedCacheStore = serviceProvider.GetRequiredService<IDatabase>();

            switch (Polymorph)
            {
                case ClientChatSession clientSession:
                if (clientSession.Account is not null)
                {
                    Context.ClientChatSessions.TryRemove(clientSession.Account.Name, out _);
                }
                break;

                case MatchServerChatSession matchServerSession:
                if (matchServerSession.Metadata is not null)
                {
                    Context.MatchServerChatSessions.TryRemove(matchServerSession.Metadata.ServerID, out _);

                    _ = distributedCacheStore.RemoveMatchServerByID(matchServerSession.Metadata.ServerID);
                }
                break;

                case MatchServerManagerChatSession matchServerManagerSession:
                if (matchServerManagerSession.Metadata is not null)
                {
                    Context.MatchServerManagerChatSessions.TryRemove(matchServerManagerSession.Metadata.ServerManagerID, out _);

                    _ = distributedCacheStore.RemoveMatchServerManagerByID(matchServerManagerSession.Metadata.ServerManagerID);
                }
                break;
            }
        }
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = RemainingPreviouslyReceivedData.Concat(buffer[(int) offset..(int) size]).ToArray();

        List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

        RemainingPreviouslyReceivedData = remaining;

        foreach (byte[] segment in segments) _ = ProcessDataSegment(segment);
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

    private async Task ProcessDataSegment(byte[] segment)
    {
        try
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
            }

            else
            {
                if (HostEnvironment.IsDevelopment())
                    Log.Debug(@"Processing Command: ""0x{Command}""", command.ToString("X4"));

                object? commandTypeInstance = ActivatorUtilities.CreateInstance(serviceProvider, commandType);

                if (commandTypeInstance is null)
                    Log.Error(@"[BUG] Could Not Create Command Type Instance For Command: ""0x{Command}""", command.ToString("X4"));

                else
                {
                    const string processMethodName = "Process";

                    MethodInfo? processMethod = commandType.GetMethod(processMethodName);

                    if (processMethod is null)
                        Log.Error(@"[BUG] Command Type ""{TypeName}"" Does Not Have A ""{ProcessMethodName}"" Method", commandType.Name, processMethodName);

                    else
                    {
                        Type[] parameterTypes = [.. processMethod.GetParameters().Select(parameter => parameter.ParameterType)];

                        if (parameterTypes.Length is not 2)
                            Log.Error(@"[BUG] The ""{processMethodName}"" Method For Command Type ""{TypeName}"" Does Not Have Exactly Two Parameters (ChatSession, ChatBuffer)",
                                processMethodName, commandType.Name);

                        else
                        {
                            Type sessionParameterType = parameterTypes.First();

                            ChatSession sessionToUse = this;

                            if (CommandTypeToSessionTypeMap.TryGetValue(commandType, out Type? requiredSessionType))
                            {
                                if (requiredSessionType != typeof(ChatSession))
                                {
                                    if (Polymorph is null || Polymorph.GetType() != requiredSessionType)
                                    {
                                        Polymorph = (ChatSession) Activator.CreateInstance(requiredSessionType, Server, serviceProvider)!;

                                        Log.Debug(@"Created Polymorphic Session ""{SessionType}"" For Command ""{CommandType}""", requiredSessionType.Name, commandType.Name);
                                    }

                                    sessionToUse = Polymorph;
                                }
                            }

                            Type actualSessionType = sessionToUse.GetType();

                            if (sessionParameterType.IsAssignableFrom(actualSessionType) is false)
                                Log.Error(@"[BUG] Command Type ""{TypeName}"" Requires Session Type ""{RequiredSessionType}"" But Got ""{ActualSessionType}""",
                                    commandType.Name, sessionParameterType.Name, actualSessionType.Name);

                            else
                            {
                                ChatBuffer buffer = new (segment);

                                object? result = processMethod.Invoke(commandTypeInstance, [sessionToUse, buffer]);

                                if (result is Task task) await task;
                            }
                        }
                    }
                }
            }
        }

        catch (Exception exception)
        {
            Log.Error(exception, @"[BUG] Error Processing Command From Segment");
        }
    }

    private Type? GetCommandType(ushort command)
    {
        if (CommandToCommandTypeMap.TryGetValue(command, out Type? type))
            return type;

        Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

        type = types.SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
            && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is not null)
        {
            if (CommandToCommandTypeMap.TryAdd(command, type) is false && CommandToCommandTypeMap.ContainsKey(command) is false)
                Log.Error(@"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""", command.ToString("X4"), type.Name);

            MapCommandTypeToSessionType(type);
        }

        return type;
    }

    /// <summary>
    ///     Extracts the session type parameter from a command processor's generic interface and populates the command type to session type mapping.
    ///     This method inspects the command processor's implemented interfaces to find IAsynchronousCommandProcessor or ISynchronousCommandProcessor.
    ///     It then extracts the generic type parameter which represents the required session type for that command.
    /// </summary>
    /// <param name="commandType">The command processor type to analyse.</param>
    private static void MapCommandTypeToSessionType(Type commandType)
    {
        if (CommandTypeToSessionTypeMap.ContainsKey(commandType)) return;

        Type? commandProcessorInterface = commandType.GetInterfaces()
            .FirstOrDefault(interfaceType =>
                interfaceType.IsGenericType &&
                    (interfaceType.GetGenericTypeDefinition() == typeof(IAsynchronousCommandProcessor<>) ||
                        interfaceType.GetGenericTypeDefinition() == typeof(ISynchronousCommandProcessor<>)));

        if (commandProcessorInterface is not null)
        {
            Type sessionType = commandProcessorInterface.GetGenericArguments().Single();

            CommandTypeToSessionTypeMap.TryAdd(commandType, sessionType);

            Log.Debug(@"Mapped Command Type ""{CommandType}"" To Session Type ""{SessionType}""", commandType.Name, sessionType.Name);
        }
    }

    /// <summary>
    ///     Send buffer data with non-destructive size prefixing, which does not mutate the original buffer.
    ///     Enforces 16KB (16384 bytes) maximum packet size, as to not exceed the chat protocol's limitations.
    /// </summary>
    public bool Send(ChatBuffer buffer)
    {
        if (buffer.Size > ChatProtocol.MAX_PACKET_SIZE)
        {
            Log.Error(@"Packet Of {PacketSize} Bytes Exceeds Maximum Allowed Size Of {MaximumPacketSize} Bytes", buffer.Size, ChatProtocol.MAX_PACKET_SIZE);

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
