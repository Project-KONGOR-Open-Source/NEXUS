namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatSession>>();

    private static ConcurrentDictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

    private byte[]? ExpectedDataSize { get; set; }

    protected override void OnConnected()
    {
        Logger.LogInformation($"Chat Session ID {ID} Was Created");
    }

    protected override void OnError(SocketError error)
    {
        Logger.LogInformation($"Chat Session ID {ID} Caught A Socket Error With Code {error}");
    }

    protected override void OnDisconnected()
    {
        Logger.LogInformation($"Chat Session ID {ID} Has Terminated");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        byte[] received = buffer[(int)offset..(int)size];

        // If The Data Size Is Less Than 2 Bytes, Then It Is Not A Valid Data Segment
        if (size < 2) throw new InvalidDataException("Received Data Size Is Less Than 2 Bytes");

        // If The Data Size Is 2 Bytes, Then It Is The Expected Data Size For The Next Data Segment To Be Sent
        else if (size == 2 && ExpectedDataSize == null)
        {
            if (received.Length != 2)
                throw new InvalidDataException("Received Data Size Is Not 2 Bytes");

            ExpectedDataSize = received;
        }

        // If The Received Data Size Matches The Expected Data Size, Then We Can Process The Data Segment
        else if (ExpectedDataSize != null && size == Convert.ToInt64(BitConverter.ToUInt16([ExpectedDataSize.First(), ExpectedDataSize.Last()])))
        {
            ExpectedDataSize = null;
            ProcessDataSegment(received);
        }

        // The Received Data May Contain Multiple Data Segments, So We Need To Split Them And Process Each One Individually
        else if (ExpectedDataSize == null || size > Convert.ToInt64(BitConverter.ToUInt16([ExpectedDataSize.First(), ExpectedDataSize.Last()])))
        {
            List<byte[]> segments = ExpectedDataSize == null ? GetDataSegments(received) : GetDataSegments(ExpectedDataSize.Concat(received).ToArray());

            ExpectedDataSize = null;

            Parallel.ForEach(segments, ProcessDataSegment);
        }

        // If The Received Data Does Not Match Any Of The Previous Conditions, Then It Is Not Valid Data
        else throw new InvalidDataException($"[BUG] Unable To Process Bytes {string.Join(':', received)}");
    }

    private void ProcessDataSegment(byte[] segment)
    {
        ushort command = BitConverter.ToUInt16([segment[0], segment[1]]);

        Type? commandType = GetCommandType(command);

        if (commandType is null)
        {
            string output = new StringBuilder($@"Missing Command Type For Command: ""{command:X4}""")
                .Append(Environment.NewLine).Append($"Message UTF8 Text: {Encoding.UTF8.GetString(segment)}").ToString();

            Logger.LogError(output);
        }

        else
        {
            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
                commandTypeInstance.Process(this, new ChatBuffer(segment));

            else Logger.LogError($@"[BUG] Could Not Create Command Type Instance For Command: ""{command:X4}""");
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
            if (CommandToTypeMap.TryAdd(command, type) is false)
                Logger.LogError($@"[BUG] Could Not Add Command To Type Mapping For Command ""{command:X4}"" And Type ""{type.Name}""");

        return type;
    }

    private ICommandProcessor? GetCommandTypeInstance(Type type)
    {
        object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

        return instance as ICommandProcessor;
    }

    private List<byte[]> GetDataSegments(byte[] buffer)
    {
        List<byte[]> segments = [];

        int offset = 0;

        while (offset < buffer.Length)
        {
            try
            {
                ushort segmentSize = BitConverter.ToUInt16([buffer[offset + 0], buffer[offset + 1]]);
                byte[] segment = buffer[(offset + 2)..(offset + 2 + segmentSize)];

                segments.Add(segment);

                offset += 2 + segmentSize;
            }

            catch (Exception exception)
            {
                Logger.LogError(exception, $"Failed To Get Segment From Buffer With Remaining Bytes {string.Join(':', buffer[offset..buffer.Length])}");
                break;
            }
        }

        return segments;
    }
}
