namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    public ClientInformation ClientInformation { get; set; } = null!; // TODO: If This Is NULL Or Equivalent, Don't Process Data Other Than Client Handshake

    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatSession>>();

    private static ConcurrentDictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

    private byte[] RemainingPreviouslyReceivedData { get; set; } = [];

    protected override void OnConnected()
    {
        Logger.LogInformation("Chat Session ID {SessionID} Was Created", ID);
    }

    protected override void OnError(SocketError error)
    {
        Logger.LogInformation("Chat Session ID {SessionID} Caught A Socket Error With Code {SocketErrorCode}", ID, error);
    }

    protected override void OnDisconnected()
    {
        Logger.LogInformation("Chat Session ID {SessionID} Has Terminated", ID);
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = RemainingPreviouslyReceivedData.Concat(buffer[(int)offset..(int)size]).ToArray();

        List<byte[]> segments = ExtractDataSegments(received, out byte[] remaining);

        RemainingPreviouslyReceivedData = remaining;

        Parallel.ForEach(segments, ProcessDataSegment);
    }

    private List<byte[]> ExtractDataSegments(byte[] buffer, out byte[] remaining)
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

        Type? commandType = GetCommandType(command);

        if (commandType is null)
        {
            string output = new StringBuilder($@"Missing Type Mapping For Command: ""0x{command:X4}""")
                .Append(Environment.NewLine).Append($"Message UTF8 Bytes: {string.Join(':', segment)}")
                .Append(Environment.NewLine).Append($"Message UTF8 Text: {Encoding.UTF8.GetString(segment)}")
                .ToString();

            Logger.LogError(output);
        }

        else
        {
            if (TRANSMUTANSTEIN.RunsInDevelopmentMode)
                Logger.LogDebug(@"Processing Command: ""0x{Command}""", command.ToString("X4"));

            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
            {
                try
                {
                    ChatBuffer buffer = new (segment);

                    commandTypeInstance.Switch
                        (synchronous => synchronous.Process(this, buffer), async asynchronous => await asynchronous.Process(this, buffer));
                }

                catch (Exception exception)
                {
                    Logger.LogError(exception, @"[BUG] Error Processing Command: ""0x{Command}""", command.ToString("X4"));
                }
            }

            else Logger.LogError(@"[BUG] Could Not Create Command Type Instance For Command: ""0x{Command}""", command.ToString("X4"));
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
                Logger.LogError(@"[BUG] Could Not Add Command-To-Type Mapping For Command ""0x{Command}"" And Type ""{TypeName}""", command.ToString("X4"), type.Name);

        return type;
    }

    private OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>? GetCommandTypeInstance(Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

        if (instance is ISynchronousCommandProcessor synchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT0(synchronous);

        if (instance is IAsynchronousCommandProcessor asynchronous)
            return OneOf<ISynchronousCommandProcessor, IAsynchronousCommandProcessor>.FromT1(asynchronous);

        Logger.LogError(@"[BUG] Command Type ""{TypeName}"" Does Not Implement A Supported Processor Interface", type.Name);

        return null;
    }
}
