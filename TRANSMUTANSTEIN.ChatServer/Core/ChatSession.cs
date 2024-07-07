namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatSession(TCPServer server, IServiceProvider serviceProvider) : TCPSession(server)
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    private ILogger Logger { get; } = serviceProvider.GetRequiredService<ILogger<ChatSession>>();

    private static Dictionary<ushort, Type> CommandToTypeMap { get; set; } = [];

    protected override void OnConnected()
    {
        Logger.LogInformation($"Chat Session ID {ID} Was Created");
    }

    protected override void OnDisconnected()
    {
        Logger.LogInformation($"Chat Session ID {ID} Has Terminated");
    }

    private ushort? _expectedDataSize = null;

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var debug1 = buffer;
        var debug2 = this.BytesPending;
        var debug3 = this.BytesReceived;
        var debug4 = this.BytesSent;


        byte[] frame = buffer[..(int)BytesReceived];

        if (size < 2)
            throw new Exception("Invalid Frame Size (command etc.)");

        else if (size == 2 && _expectedDataSize == null)
        {
            _expectedDataSize = BitConverter.ToUInt16([frame[0], frame[1]]);
        }

        else if (size == _expectedDataSize)
        {
            _expectedDataSize = null;
            ProcessFrame(frame);
        }

        else if (_expectedDataSize == null || _expectedDataSize < size)
        {
            List<byte[]> frames = _expectedDataSize == null ? GetFrames(frame) : GetFrames(BitConverter.GetBytes(_expectedDataSize.Value).Concat(frame).ToArray());

            Parallel.ForEach(frames, ProcessFrame);
        }

        else
        {
            throw new Exception("[BUG]");
        }
    }

    protected override void OnError(SocketError error)
    {
        Logger.LogInformation($"Chat Session ID {ID} Caught A Socket Error With Code {error}");
    }

    private void ProcessFrame(byte[] frame)
    {
        ushort command = BitConverter.ToUInt16([frame[0], frame[1]]);

        Type? commandType = GetCommandType(command);

        if (commandType is null)
        {
            string output = new StringBuilder($@"Missing Command Type For Command: ""{command:X4}""")
                .Append(Environment.NewLine).Append($"Message UTF8 Text: {Encoding.UTF8.GetString(frame)}").ToString();

            Logger.LogError(output);
        }

        else
        {
            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
                commandTypeInstance.Process(this, new ChatBuffer(frame));

            else Logger.LogError($@"[BUG] Could Not Create Command Type Instance For Command: ""{command:X4}""");
        }

        Type? GetCommandType(ushort command)
        {
            if (CommandToTypeMap.TryGetValue(command, out Type? type))
                return type;

            Type[] types = typeof(TRANSMUTANSTEIN).Assembly.GetTypes();

            type = types
                .SingleOrDefault(type => type.GetCustomAttribute<ChatCommandAttribute>() is not null
                                         && (type.GetCustomAttribute<ChatCommandAttribute>()?.Command.Equals(command) ?? false));

            if (type is not null)
                CommandToTypeMap.Add(command, type);

            return type;
        }

        ICommandProcessor? GetCommandTypeInstance(Type type)
        {
            object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

            return instance as ICommandProcessor;
        }
    }

    private List<byte[]> GetFrames(byte[] buffer)
    {
        List<byte[]> frames = [];

        int offset = 0;

        while (offset < buffer.Length)
        {
            ushort frameSize = BitConverter.ToUInt16([buffer[offset], buffer[offset + 1]]);
            byte[] frame = buffer[(offset + 2)..(offset + 2 + frameSize)];

            frames.Add(frame);

            offset += 2 + frameSize;
        }

        return frames;
    }
}
