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

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        ushort command = BitConverter.ToUInt16([buffer[0], buffer[1]]);

        string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

        Type? commandType = GetCommandType(command);

        if (commandType is null)
            Logger.LogError($@"Missing Command Type For Command: ""{command:X4}""");

        else
        {
            if (GetCommandTypeInstance(commandType) is { } commandTypeInstance)
                commandTypeInstance.Process(this, new ChatBuffer(buffer));

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

            else Logger.LogError($@"Unsupported Command: ""{command:X4}""");

            return type;
        }

        ICommandProcessor? GetCommandTypeInstance(Type type)
        {
            object instance = ActivatorUtilities.CreateInstance(ServiceProvider, type);

            return instance as ICommandProcessor;
        }
    }

    protected override void OnError(SocketError error)
    {
        Logger.LogInformation($"Chat Session ID {ID} Caught A Socket Error With Code {error}");
    }
}
