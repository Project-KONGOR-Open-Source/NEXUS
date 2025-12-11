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

    private static ConcurrentDictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

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

                            if (sessionParameterType.IsAssignableFrom(GetType()) is false)
                                Log.Error(@"[BUG] Command Type ""{TypeName}"" Requires Session Type ""{RequiredSessionType}"" But Got ""{ActualSessionType}""",
                                    commandType.Name, sessionParameterType.Name, GetType().Name);

                            else
                            {
                                ChatBuffer buffer = new (segment);

                                object? result = processMethod.Invoke(commandTypeInstance, [this, buffer]);

                                // If The Result Is A Task, Await It
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
        if (CommandToTypeMap.TryGetValue(command, out Type? type))
            return type;

        Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

        type = types.SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
            && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

        if (type is not null)
            if (CommandToTypeMap.TryAdd(command, type) is false && CommandToTypeMap.ContainsKey(command) is false)
                Log.Error(@"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""", command.ToString("X4"), type.Name);

        return type;
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
